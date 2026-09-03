using UnityEngine;
using UnityEngine.AI;

public class BasicEnemyControl : MonoBehaviour
{
    public int eneHealth = 100;

    public bool hasGun;
    public bool hasStaff;
    public int grenadeCount;

    public Transform targetMove;
    public Transform targetSearch;
    public Transform player;

    public Vector3? lastSeePlayer;

    // 0 Dead, 1 Search, 2 Combat, 3 Hide, 4 Grabbed, 5 Staff
    public int eneState = 1;

    private NavMeshAgent agent;

    public float stoppingDistance = 1f;

    // Vision
    public float sightRange = 25f;
    public float sightAngle = 120f;
    public LayerMask sightBlockMask;
    public float eyeHeight = 1.5f;
    public float targetEyeHeight = 1f;

    // Wandering
    public float wanderRadius = 10f;
    public float wanderWaitTime = 5f;
    private float wanderTimer;

    // Last player search
    public float lastSeeSearchTime = 10f;
    private float lastSeeTimer;

    public float searchTurnSpeed = 45f;

    // Hiding
    public float hidingTime = 30f;
    private float hidingTimer;
    private GameObject[] hidingSpots;
    private GameObject currentHidingSpot;
    private float hidingRotation;

    // Crouching
    public float crouchDistance = 25f;
    private GameObject[] crouchSpots;
    private GameObject currentCrouchSpot;
    private bool isInCrouchSpot;

    // Shooting
    public float shootingDistance = 20f;
    public float shootCooldown = 1f;
    private float shootTimer;

    // Staff
    public float staffAttackDistance = 2.5f;
    public float staffAttackCooldown = 1.2f;
    private float staffAttackTimer;

    // Grabbed
    public float grabbedTime = 3f;
    private float grabbedTimer;

    // Grenade
    public float grenadeDistance = 15f;
    public float grenadeCooldown = 30f;
    private float grenadeTimer;

    public GameObject grenadePrefab;
    public Transform grenadeSpawnPoint;


    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (agent == null)
        {
            Debug.LogError(gameObject.name + " needs a NavMeshAgent.");
            enabled = false;
            return;
        }

        agent.stoppingDistance = stoppingDistance;

        hidingSpots =
            GameObject.FindGameObjectsWithTag("EnemyHidingSpot");

        crouchSpots =
            GameObject.FindGameObjectsWithTag("EnemyCrouchSpot");

        wanderTimer =
            Random.Range(0f, wanderWaitTime);

        grenadeTimer = 0f;
    }


    private void Update()
    {
        if (eneHealth <= 0)
        {
            eneHealth = 0;
            eneState = 0;
        }

        if (grenadeTimer > 0f)
            grenadeTimer -= Time.deltaTime;

        switch (eneState)
        {
            case 0:
                Dead();
                break;

            case 1:
                Searching();
                break;

            case 2:
                Combat();
                break;

            case 3:
                Hiding();
                break;

            case 4:
                Grabbed();
                break;

            case 5:
                Swinging();
                break;
        }
    }


    // Basic NavMesh movement.
    private bool TravelToBasic()
    {
        if (targetMove == null)
            return false;

        agent.isStopped = false;
        agent.SetDestination(targetMove.position);

        if (agent.pathPending)
            return false;

        return agent.remainingDistance <= agent.stoppingDistance;
    }


    // Case 1.
    private void Searching()
    {
        agent.isStopped = false;

        if (player != null &&
            targetSearch == player)
        {
            if (CanSeeTarget(player))
            {
                SeePlayer();
                return;
            }

            if (lastSeePlayer.HasValue)
            {
                SearchLastPlayerPosition();
                return;
            }
        }

        if (targetSearch != null)
        {
            if (CanSeeTarget(targetSearch))
            {
                targetMove = targetSearch;
                OnTargetFound();
                return;
            }
        }

        Wander();
    }


    private void SearchLastPlayerPosition()
    {
        agent.isStopped = false;

        agent.SetDestination(lastSeePlayer.Value);

        if (agent.pathPending)
            return;

        if (agent.remainingDistance >
            agent.stoppingDistance)
        {
            return;
        }

        agent.isStopped = true;

        LookAtPosition(lastSeePlayer.Value);

        if (CanSeeTarget(player))
        {
            SeePlayer();
            return;
        }

        lastSeeTimer -= Time.deltaTime;

        transform.Rotate(
            Vector3.up,
            searchTurnSpeed * Time.deltaTime
        );

        if (lastSeeTimer <= 0f)
        {
            lastSeePlayer = null;
            targetMove = null;
            agent.isStopped = false;
            wanderTimer = 0f;
        }
    }


    private void SeePlayer()
    {
        if (player == null)
            return;

        lastSeePlayer = player.position;
        lastSeeTimer = lastSeeSearchTime;

        targetMove = player;

        // Staff enemies hide when they first see the player.
        if (hasStaff)
        {
            currentHidingSpot = FindBestHidingSpot();

            if (currentHidingSpot != null)
            {
                eneState = 3;
                return;
            }
        }

        // Enemies with a gun or grenade enter combat.
        if (hasGun || grenadeCount > 0)
        {
            PrepareCombat();
            return;
        }

        // No weapon. Chase the player.
        TravelToBasic();
    }


    private void OnTargetFound()
    {
        if (targetMove == null)
            return;

        if (targetMove == player)
        {
            SeePlayer();
            return;
        }

        TravelToBasic();
    }


    private void Wander()
    {
        wanderTimer -= Time.deltaTime;

        if (wanderTimer > 0f)
            return;

        PickRandomDestination();

        wanderTimer =
            wanderWaitTime +
            Random.Range(0f, 3f);
    }


    // Case 2.
    private void Combat()
    {
        if (player == null ||
            (!hasGun && grenadeCount <= 0))
        {
            ReturnToSearching();
            return;
        }

        // Player is visible.
        if (CanSeeTarget(player))
        {
            lastSeePlayer = player.position;
            lastSeeTimer = lastSeeSearchTime;
        }
        else
        {
            SearchInCombat();
            return;
        }

        float distance =
            Vector3.Distance(
                transform.position,
                player.position
            );

        // Staff gets priority when very close.
        if (hasStaff &&
            distance <= staffAttackDistance)
        {
            eneState = 5;
            return;
        }

        // Grenade can be used even if the enemy has a gun.
        if (grenadeCount > 0 &&
            grenadeTimer <= 0f &&
            distance <= grenadeDistance)
        {
            if (currentCrouchSpot == null)
            {
                PrepareCrouchSpot();
            }

            if (currentCrouchSpot != null)
            {
                if (!isInCrouchSpot)
                {
                    targetMove =
                        currentCrouchSpot.transform;

                    if (!TravelToBasic())
                        return;

                    isInCrouchSpot = true;
                }

                agent.isStopped = true;
                LookAtTarget(player);

                ThrowGrenade();

                grenadeCount--;
                grenadeTimer = grenadeCooldown;

                ReturnToSearching();
                return;
            }
        }

        // Gun attack.
        if (hasGun &&
            distance <= shootingDistance)
        {
            if (currentCrouchSpot == null)
            {
                PrepareCrouchSpot();
            }

            if (currentCrouchSpot != null &&
                !isInCrouchSpot)
            {
                targetMove =
                    currentCrouchSpot.transform;

                if (!TravelToBasic())
                    return;

                isInCrouchSpot = true;
            }

            agent.isStopped = true;

            LookAtTarget(player);

            shootTimer -= Time.deltaTime;

            if (shootTimer <= 0f)
            {
                ShootAtPlayer();
                shootTimer = shootCooldown;
            }

            return;
        }

        // Armed enemy, but target is too far away.
        targetMove = player;
        TravelToBasic();
    }


    private void SearchInCombat()
    {
        lastSeeTimer -= Time.deltaTime;

        if (lastSeeTimer <= 0f)
        {
            lastSeePlayer = null;
            ReturnToSearching();
            return;
        }

        // Try to reach a crouch spot first.
        if (currentCrouchSpot == null)
        {
            PrepareCombat();
        }

        if (currentCrouchSpot != null &&
            !isInCrouchSpot)
        {
            targetMove =
                currentCrouchSpot.transform;

            if (!TravelToBasic())
                return;

            isInCrouchSpot = true;
        }

        agent.isStopped = true;

        // Search around instead of wandering.
        transform.Rotate(
            Vector3.up,
            searchTurnSpeed * Time.deltaTime
        );

        if (CanSeeTarget(player))
        {
            lastSeePlayer = player.position;
            lastSeeTimer = lastSeeSearchTime;
        }
    }


    private void PrepareCombat()
    {
        if (currentCrouchSpot == null)
        {
            PrepareCrouchSpot();
        }

        eneState = 2;
    }


    private bool PrepareCrouchSpot()
    {
        currentCrouchSpot =
            FindBestCrouchSpot();

        if (currentCrouchSpot == null)
            return false;

        targetMove =
            currentCrouchSpot.transform;

        isInCrouchSpot = false;

        return true;
    }


    // Case 3.
    private void Hiding()
    {
        if (!hasStaff)
        {
            ReturnToSearching();
            return;
        }

        if (currentHidingSpot == null)
        {
            ReturnToSearching();
            return;
        }

        targetMove = currentHidingSpot.transform;

        // Travel to the hiding spot.
        if (!TravelToBasic())
            return;

        // Just arrived at the hiding spot.
        if (hidingTimer <= 0f)
        {
            agent.isStopped = true;

            transform.rotation =
                Quaternion.Euler(
                    0f,
                    hidingRotation,
                    0f
                );

            hidingTimer = hidingTime;
        }

        // Hiding sensor: if they see the player,
        // immediately leave hiding and swing.
        if (player != null &&
            CanSeeTarget(player))
        {
            lastSeePlayer = player.position;
            lastSeeTimer = lastSeeSearchTime;

            ReleaseHidingSpot();

            eneState = 5;
            return;
        }

        hidingTimer -= Time.deltaTime;

        // Finished hiding.
        if (hidingTimer <= 0f)
        {
            ReleaseHidingSpot();
            ReturnToSearching();
        }
    }


    private void StartHiding()
    {
        if (currentHidingSpot == null)
            return;

        hidingTimer = 0f;

        // Turn exactly 180 degrees from the
        // direction they were facing when entering.
        hidingRotation =
            transform.eulerAngles.y + 180f;

        eneState = 3;
    }

    // Case 4.
    private void Grabbed()
    {
        agent.isStopped = true;

        grabbedTimer -= Time.deltaTime;

        if (grabbedTimer <= 0f)
            ReturnToSearching();
    }


    // Case 5.
    private void Swinging()
    {
        if (player == null || !hasStaff)
        {
            ReturnToSearching();
            return;
        }

        if (CanSeeTarget(player))
        {
            lastSeePlayer = player.position;
            lastSeeTimer = lastSeeSearchTime;
        }
        else
        {
            // Use the shared last-player-position search.
            if (lastSeePlayer.HasValue)
            {
                lastSeeTimer -= Time.deltaTime;

                if (lastSeeTimer <= 0f)
                {
                    lastSeePlayer = null;
                    ReturnToSearching();
                    return;
                }

                targetMove = null;
                agent.isStopped = false;

                agent.SetDestination(
                    lastSeePlayer.Value
                );

                if (!agent.pathPending &&
                    agent.remainingDistance <=
                    agent.stoppingDistance)
                {
                    agent.isStopped = true;

                    transform.Rotate(
                        Vector3.up,
                        searchTurnSpeed *
                        Time.deltaTime
                    );
                }

                if (CanSeeTarget(player))
                {
                    lastSeePlayer =
                        player.position;
                }
            }

            return;
        }

        float distance =
            Vector3.Distance(
                transform.position,
                player.position
            );

        if (distance >
            staffAttackDistance + 1f)
        {
            targetMove = player;
            TravelToBasic();
            return;
        }

        agent.isStopped = true;

        LookAtTarget(player);

        staffAttackTimer -= Time.deltaTime;

        if (staffAttackTimer <= 0f)
        {
            StaffAttack();
            staffAttackTimer =
                staffAttackCooldown;
        }
    }


    private bool CanSeeTarget(Transform target)
    {
        if (target == null)
            return false;

        Vector3 eye =
            transform.position +
            Vector3.up * eyeHeight;

        Vector3 targetPosition =
            target.position +
            Vector3.up * targetEyeHeight;

        Vector3 direction =
            targetPosition - eye;

        float distance =
            direction.magnitude;

        if (distance > sightRange)
            return false;

        direction.Normalize();

        if (Vector3.Angle(
            transform.forward,
            direction) >
            sightAngle * 0.5f)
        {
            return false;
        }

        if (Physics.Raycast(
            eye,
            direction,
            out RaycastHit hit,
            distance,
            sightBlockMask))
        {
            return hit.transform == target ||
                   hit.transform.IsChildOf(target);
        }

        return true;
    }


    private void PickRandomDestination()
    {
        Vector3 randomPosition =
            transform.position +
            Random.insideUnitSphere *
            wanderRadius;

        if (NavMesh.SamplePosition(
            randomPosition,
            out NavMeshHit hit,
            wanderRadius,
            NavMesh.AllAreas))
        {
            targetMove = null;
            agent.isStopped = false;

            agent.SetDestination(
                hit.position
            );
        }
    }


    private GameObject FindBestHidingSpot()
    {
        return FindBestSpot(
            hidingSpots,
            Mathf.Infinity,
            false
        );
    }


    private GameObject FindBestCrouchSpot()
    {
        return FindBestSpot(
            crouchSpots,
            crouchDistance,
            true
        );
    }


    private GameObject FindBestSpot(
        GameObject[] spots,
        float maxDistance,
        bool needsPlayerSight)
    {
        GameObject bestSpot = null;
        float bestDistance = Mathf.Infinity;

        if (spots == null)
            return null;

        foreach (GameObject spot in spots)
        {
            if (spot == null)
                continue;

            NavMeshPath path =
                new NavMeshPath();

            if (!agent.CalculatePath(
                spot.transform.position,
                path))
            {
                continue;
            }

            if (path.status !=
                NavMeshPathStatus.PathComplete)
            {
                continue;
            }

            float distance =
                GetPathLength(path);

            if (distance > maxDistance)
                continue;

            if (needsPlayerSight &&
                player != null &&
                !CanSeeTargetFromPosition(
                    spot.transform.position,
                    player))
            {
                continue;
            }

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestSpot = spot;
            }
        }

        return bestSpot;
    }


    private bool CanSeeTargetFromPosition(
        Vector3 position,
        Transform target)
    {
        if (target == null)
            return false;

        Vector3 eye =
            position +
            Vector3.up * eyeHeight;

        Vector3 targetPosition =
            target.position +
            Vector3.up * targetEyeHeight;

        Vector3 direction =
            targetPosition - eye;

        float distance =
            direction.magnitude;

        if (distance > sightRange)
            return false;

        direction.Normalize();

        if (Physics.Raycast(
            eye,
            direction,
            out RaycastHit hit,
            distance,
            sightBlockMask))
        {
            return hit.transform == target ||
                   hit.transform.IsChildOf(target);
        }

        return true;
    }


    private float GetPathLength(
        NavMeshPath path)
    {
        if (path.corners.Length < 2)
            return Mathf.Infinity;

        float length = 0f;

        for (int i = 1;
             i < path.corners.Length;
             i++)
        {
            length += Vector3.Distance(
                path.corners[i - 1],
                path.corners[i]
            );
        }

        return length;
    }


    private void LookAtTarget(
        Transform target)
    {
        if (target == null)
            return;

        LookAtPosition(target.position);
    }


    private void LookAtPosition(
        Vector3 position)
    {
        Vector3 direction =
            position -
            transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion rotation =
            Quaternion.LookRotation(direction);

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                rotation,
                Time.deltaTime * 8f
            );
    }


    private void Dead()
    {
        agent.isStopped = true;
    }


    public void TakeDamage(int damage)
    {
        eneHealth -= damage;

        if (eneHealth <= 0)
        {
            eneHealth = 0;
            eneState = 0;
            return;
        }

        if (player != null)
        {
            targetSearch = player;
            lastSeePlayer = player.position;
            lastSeeTimer = lastSeeSearchTime;

            // Being attacked immediately puts armed enemies
            // into combat.
            if (hasGun || grenadeCount > 0)
            {
                PrepareCombat();
            }
            else if (hasStaff)
            {
                currentHidingSpot =
                    FindBestHidingSpot();

                if (currentHidingSpot != null)
                    StartHiding();
                else
                    eneState = 5;
            }
            else
            {
                targetMove = player;
                agent.isStopped = false;
            }
        }
    }


    public void SetGrabbed()
    {
        eneState = 4;
        grabbedTimer = grabbedTime;
        agent.isStopped = true;
    }


    private void ReturnToSearching()
    {
        eneState = 1;

        targetMove = null;
        currentCrouchSpot = null;
        currentHidingSpot = null;

        isInCrouchSpot = false;

        agent.isStopped = false;

        if (targetSearch == null &&
            player != null)
        {
            targetSearch = player;
        }
    }


    private void ReleaseHidingSpot()
    {
        currentHidingSpot = null;
        hidingTimer = 0f;
    }


    private void ShootAtPlayer()
    {
        Debug.Log(
            gameObject.name +
            " shoots at " +
            player.name
        );
    }


    private void StaffAttack()
    {
        Debug.Log(
            gameObject.name +
            " swings at " +
            player.name
        );
    }


    private void ThrowGrenade()
    {
        if (grenadePrefab == null ||
            grenadeSpawnPoint == null)
        {
            return;
        }

        GameObject grenade =
            Instantiate(
                grenadePrefab,
                grenadeSpawnPoint.position,
                grenadeSpawnPoint.rotation
            );

        Rigidbody rb =
            grenade.GetComponent<Rigidbody>();

        if (rb != null)
        {
            Vector3 direction =
                player.position -
                grenadeSpawnPoint.position;

            direction.y += 0.35f;

            rb.AddForce(
                direction.normalized * 10f,
                ForceMode.VelocityChange
            );
        }
    }
}

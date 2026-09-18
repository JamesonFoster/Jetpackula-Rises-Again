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

    public Transform gunMuzzle;
    public float gunRayDistance = 50f;
    public LayerMask gunRaycastMask;
    public float gunRayVisibleTime = 0.08f;
    public Color gunRayColor = Color.red;

    public float shootAccuracy = 0.8f;
    public float shootSpread = 3f;

    private LineRenderer gunLine;
    private float gunRayTimer;

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

    public int Blood = 5;

    // Death fade
    public float deathFadeTime = 2f;

    private Animator animator;
    private Renderer[] enemyRenderers;
    private bool isDying;


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

        animator = GetComponent<Animator>();

        enemyRenderers =
            GetComponentsInChildren<Renderer>();

        gunLine = gameObject.AddComponent<LineRenderer>();

        gunLine.positionCount = 2;
        gunLine.startWidth = 0.03f;
        gunLine.endWidth = 0.01f;
        gunLine.material =
            new Material(Shader.Find("Sprites/Default"));

        gunLine.startColor = gunRayColor;
        gunLine.endColor = gunRayColor;
        gunLine.enabled = false;
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

        if (gunRayTimer > 0f)
        {
            gunRayTimer -= Time.deltaTime;

            if (gunRayTimer <= 0f &&
                gunLine != null)
            {
                gunLine.enabled = false;
            }
        }

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


    private bool TravelToBasic()
    {
        if (targetMove == null)
            return false;

        agent.isStopped = false;
        agent.SetDestination(targetMove.position);

        if (agent.pathPending)
            return false;

        return agent.remainingDistance <=
               agent.stoppingDistance;
    }


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

        if (hasStaff)
        {
            currentHidingSpot =
                FindBestHidingSpot();

            if (currentHidingSpot != null)
            {
                eneState = 3;
                return;
            }
        }

        if (hasGun || grenadeCount > 0)
        {
            PrepareCombat();
            return;
        }

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


    private void Combat()
    {
        if (player == null ||
            (!hasGun && grenadeCount <= 0))
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
            SearchInCombat();
            return;
        }

        float distance =
            Vector3.Distance(
                transform.position,
                player.position
            );

        if (hasStaff &&
            distance <= staffAttackDistance)
        {
            eneState = 5;
            return;
        }

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

        targetMove =
            currentHidingSpot.transform;

        if (!TravelToBasic())
            return;

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

        hidingRotation =
            transform.eulerAngles.y + 180f;

        eneState = 3;
    }


    private void Grabbed()
    {
        agent.isStopped = true;

        grabbedTimer -= Time.deltaTime;

        if (grabbedTimer <= 0f)
            ReturnToSearching();
    }


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


    private void LookAtTarget(Transform target)
    {
        if (target == null)
            return;

        LookAtPosition(target.position);
    }


    private void LookAtPosition(Vector3 position)
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
        if (isDying)
            return;

        isDying = true;

        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        if (animator != null)
            animator.enabled = false;

        StartCoroutine(FadeAndDestroy());
    }


    private System.Collections.IEnumerator FadeAndDestroy()
    {
        float timer = 0f;

        Material[] materials =
            new Material[enemyRenderers.Length];

        for (int i = 0;
             i < enemyRenderers.Length;
             i++)
        {
            if (enemyRenderers[i] != null)
                materials[i] =
                    enemyRenderers[i].material;
        }

        while (timer < deathFadeTime)
        {
            timer += Time.deltaTime;

            float alpha =
                1f -
                Mathf.Clamp01(
                    timer / deathFadeTime
                );

            for (int i = 0;
                 i < enemyRenderers.Length;
                 i++)
            {
                if (enemyRenderers[i] == null ||
                    materials[i] == null)
                    continue;

                Color color =
                    materials[i].color;

                color.a = alpha;

                materials[i].color = color;
            }

            yield return null;
        }

        Destroy(gameObject);
    }


    public void TakeDamage(int damage)
    {
        eneHealth -= damage;

        if (Blood != 0)
        {
            Blood -= 1;
            GlobalPlayerVars.BloodCount += 5;
        }

        if (eneHealth <= 0)
        {
            eneHealth = 0;
            eneState = 0;

            Dead();
            return;
        }

        if (player != null)
        {
            targetSearch = player;
            lastSeePlayer = player.position;
            lastSeeTimer = lastSeeSearchTime;

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
        if (player == null)
            return;

        Vector3 origin;

        if (gunMuzzle != null)
        {
            origin = gunMuzzle.position;
        }
        else
        {
            origin =
                transform.position +
                Vector3.up * eyeHeight;
        }

        Vector3 targetPosition =
            player.position +
            Vector3.up * targetEyeHeight;

        Vector3 direction =
            (targetPosition - origin).normalized;

        direction = Quaternion.Euler(
            Random.Range(-shootSpread, shootSpread),
            Random.Range(-shootSpread, shootSpread),
            0f
        ) * direction;

        if (Random.value > shootAccuracy)
        {
            direction = Quaternion.Euler(
                Random.Range(-15f, 15f),
                Random.Range(-15f, 15f),
                0f
            ) * direction;
        }

        Ray ray =
            new Ray(origin, direction);

        RaycastHit hit;

        Vector3 rayEnd =
            origin +
            direction * gunRayDistance;

        if (Physics.Raycast(
            ray,
            out hit,
            gunRayDistance,
            gunRaycastMask,
            QueryTriggerInteraction.Ignore))
        {
            rayEnd = hit.point;

            if (hit.transform == player ||
                hit.transform.IsChildOf(player))
            {
                if (GlobalPlayerVars.ArmState != 'B')
                    GlobalPlayerVars.PlayerHealth -= 2;
                else
                    GlobalPlayerVars.PlayerHealth -= 1;
            }
        }

        if (gunLine != null)
        {
            gunLine.SetPosition(0, origin);
            gunLine.SetPosition(1, rayEnd);

            gunLine.startColor = gunRayColor;
            gunLine.endColor = gunRayColor;

            gunLine.enabled = true;
            gunRayTimer = gunRayVisibleTime;
        }

        Debug.DrawRay(
            origin,
            direction * gunRayDistance,
            gunRayColor,
            gunRayVisibleTime
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
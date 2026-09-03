using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class BasicEnemyControl : MonoBehaviour
{
    // =========================================================
    // ENEMY SETTINGS
    // =========================================================

    public bool hasGun = false;
    public bool hasStaff = false;
    public int grenadeCount = 0;

    public int eneHealth = 100;

    // The current destination for the enemy.
    public Transform targetMove;

    // The player.
    public Transform player;

    // 0 = Dead
    // 1 = Searching
    // 2 = Shooting
    // 3 = Hiding
    // 4 = Grabbed
    // 5 = Swinging
    // 6 = Grenade
    public int eneState = 1;


    // =========================================================
    // NAVIGATION
    // =========================================================

    private NavMeshAgent agent;

    public float stoppingDistance = 1.0f;


    // =========================================================
    // PLAYER DETECTION
    // =========================================================

    public float sightRange = 25f;

    // Field of view in degrees.
    public float sightAngle = 120f;

    // Objects on these layers block enemy vision.
    public LayerMask sightBlockMask;


    // =========================================================
    // SEARCHING / WANDERING
    // =========================================================

    public float wanderRadius = 10f;
    public float wanderWaitTime = 5f;

    private float wanderTimer;


    // =========================================================
    // HIDING
    // =========================================================

    public float hidingDistance = 15f;

    private GameObject[] hidingSpots;

    private GameObject currentHidingSpot;


    // =========================================================
    // CROUCHING
    // =========================================================

    public float crouchDistance = 25f;

    private GameObject[] crouchSpots;

    private GameObject currentCrouchSpot;

    private bool isInCrouchSpot = false;


    // =========================================================
    // SHOOTING
    // =========================================================

    public float shootingDistance = 20f;

    public float shootCooldown = 1f;

    private float shootTimer;


    // =========================================================
    // STAFF
    // =========================================================

    public float staffAttackDistance = 2.5f;

    public float staffAttackCooldown = 1.2f;

    private float staffAttackTimer;


    // =========================================================
    // GRABBED
    // =========================================================

    public float grabbedTime = 3f;

    private float grabbedTimer;


    // =========================================================
    // GRENADE
    // =========================================================

    public float grenadeDistance = 15f;

    // The enemy gets a grenade opportunity every 30 seconds.
    public float grenadeCheckTime = 30f;

    private float grenadeTimer;

    public GameObject grenadePrefab;

    public Transform grenadeSpawnPoint;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (agent == null)
        {
            Debug.LogError(
                gameObject.name +
                " needs a NavMeshAgent component."
            );
        }

        // -----------------------------------------------------
        // Find every hiding spot in the scene by tag.
        // -----------------------------------------------------

        hidingSpots =
            GameObject.FindGameObjectsWithTag("EnemyHidingSpot");

        // -----------------------------------------------------
        // Find every crouching spot in the scene by tag.
        // -----------------------------------------------------

        crouchSpots =
            GameObject.FindGameObjectsWithTag("EnemyCrouchSpot");

        // -----------------------------------------------------
        // Randomize timers so every enemy doesn't act at
        // exactly the same time.
        // -----------------------------------------------------

        wanderTimer =
            Random.Range(0f, wanderWaitTime);

        grenadeTimer =
            Random.Range(0f, grenadeCheckTime);

        shootTimer = 0f;

        staffAttackTimer = 0f;

        grabbedTimer = 0f;

        if (agent != null)
        {
            agent.stoppingDistance = stoppingDistance;
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        // -----------------------------------------------------
        // Death check.
        // -----------------------------------------------------

        if (eneHealth <= 0)
        {
            eneHealth = 0;

            if (eneState != 0)
            {
                eneState = 0;
            }
        }

        // -----------------------------------------------------
        // State machine.
        // -----------------------------------------------------

        switch (eneState)
        {
            case 0:
                Dead();
                break;

            case 1:
                Searching();
                break;

            case 2:
                Shooting();
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

            case 6:
                Grenade();
                break;
        }
    }


    // =========================================================
    // 0 - DEAD
    // =========================================================

    private void Dead()
    {
        if (agent != null)
        {
            agent.isStopped = true;
        }

        // Put your death animation here.

        // Example:
        // animator.SetTrigger("Dead");
    }


    // =========================================================
    // 1 - SEARCHING
    // =========================================================

    private void Searching()
    {
        if (agent == null)
            return;

        agent.isStopped = false;

        // -----------------------------------------------------
        // Check whether player can be seen.
        // -----------------------------------------------------

        if (CanSeePlayer())
        {
            OnPlayerSpotted();
            return;
        }

        // -----------------------------------------------------
        // Wander around the area.
        // -----------------------------------------------------

        wanderTimer -= Time.deltaTime;

        if (wanderTimer <= 0f)
        {
            PickRandomDestination();

            wanderTimer =
                wanderWaitTime +
                Random.Range(0f, 3f);
        }
    }


    // =========================================================
    // PLAYER SPOTTED
    // =========================================================

    private void OnPlayerSpotted()
    {
        if (player == null)
            return;

        float distance =
            Vector3.Distance(
                transform.position,
                player.position
            );

        // -----------------------------------------------------
        // Very close + staff = melee attack.
        // -----------------------------------------------------

        if (hasStaff &&
            distance <= staffAttackDistance)
        {
            eneState = 5;
            return;
        }

        // -----------------------------------------------------
        // Grenade.
        // -----------------------------------------------------

        if (grenadeCount > 0 &&
            distance <= grenadeDistance)
        {
            grenadeTimer -= Time.deltaTime;

            if (grenadeTimer <= 0f)
            {
                eneState = 6;
                return;
            }
        }

        // -----------------------------------------------------
        // Gun.
        // -----------------------------------------------------

        if (hasGun &&
            distance <= shootingDistance)
        {
            GameObject bestCrouchSpot =
                FindBestCrouchSpot();

            if (bestCrouchSpot != null)
            {
                currentCrouchSpot = bestCrouchSpot;

                targetMove =
                    currentCrouchSpot.transform;

                isInCrouchSpot = false;

                eneState = 2;

                agent.isStopped = false;

                agent.SetDestination(
                    targetMove.position
                );

                return;
            }

            // If there is no crouch spot,
            // the enemy can still attack.
            eneState = 2;
            return;
        }

        // -----------------------------------------------------
        // If they can't attack yet, chase the player.
        // -----------------------------------------------------

        targetMove = player;

        agent.isStopped = false;

        agent.SetDestination(
            player.position
        );
    }


    // =========================================================
    // 2 - SHOOTING
    // =========================================================

    private void Shooting()
    {
        if (player == null)
        {
            ReturnToSearching();
            return;
        }

        if (!hasGun)
        {
            ReturnToSearching();
            return;
        }

        // -----------------------------------------------------
        // Player no longer visible.
        // -----------------------------------------------------

        if (!CanSeePlayer())
        {
            ReturnToSearching();
            return;
        }

        float distance =
            Vector3.Distance(
                transform.position,
                player.position
            );

        // -----------------------------------------------------
        // Staff if player gets too close.
        // -----------------------------------------------------

        if (hasStaff &&
            distance <= staffAttackDistance)
        {
            eneState = 5;
            return;
        }

        // -----------------------------------------------------
        // Player is too far away.
        // -----------------------------------------------------

        if (distance > shootingDistance)
        {
            isInCrouchSpot = false;

            currentCrouchSpot = null;

            ReturnToSearching();

            return;
        }

        // -----------------------------------------------------
        // Move to crouching position.
        // -----------------------------------------------------

        if (currentCrouchSpot != null &&
            !isInCrouchSpot)
        {
            float distanceToSpot =
                Vector3.Distance(
                    transform.position,
                    currentCrouchSpot.transform.position
                );

            // Still travelling.
            if (distanceToSpot > 1.5f)
            {
                agent.isStopped = false;

                agent.SetDestination(
                    currentCrouchSpot.transform.position
                );

                return;
            }

            // Arrived.
            isInCrouchSpot = true;

            agent.isStopped = true;
        }

        // -----------------------------------------------------
        // If there is no crouch spot, stop and shoot.
        // -----------------------------------------------------

        if (currentCrouchSpot == null)
        {
            agent.isStopped = true;
        }

        // -----------------------------------------------------
        // Face the player.
        // -----------------------------------------------------

        LookAtPlayer();

        // -----------------------------------------------------
        // Shoot.
        // -----------------------------------------------------

        shootTimer -= Time.deltaTime;

        if (shootTimer <= 0f)
        {
            ShootAtPlayer();

            shootTimer = shootCooldown;
        }
    }


    // =========================================================
    // 3 - HIDING
    // =========================================================

    private void Hiding()
    {
        if (agent == null)
            return;

        // -----------------------------------------------------
        // If we don't currently have a hiding spot,
        // find the best one.
        // -----------------------------------------------------

        if (currentHidingSpot == null)
        {
            currentHidingSpot =
                FindBestHidingSpot();

            if (currentHidingSpot == null)
            {
                ReturnToSearching();
                return;
            }

            targetMove =
                currentHidingSpot.transform;
        }

        // -----------------------------------------------------
        // Go to hiding spot.
        // -----------------------------------------------------

        float distanceToSpot =
            Vector3.Distance(
                transform.position,
                currentHidingSpot.transform.position
            );

        if (distanceToSpot > 1.5f)
        {
            agent.isStopped = false;

            agent.SetDestination(
                currentHidingSpot.transform.position
            );

            return;
        }

        // -----------------------------------------------------
        // Enemy is now hiding.
        // -----------------------------------------------------

        agent.isStopped = true;

        // -----------------------------------------------------
        // Check for player.
        // -----------------------------------------------------

        if (CanSeePlayer())
        {
            float playerDistance =
                Vector3.Distance(
                    transform.position,
                    player.position
                );

            // Player got close enough.
            if (playerDistance <= hidingDistance)
            {
                // Use staff if close.
                if (hasStaff &&
                    playerDistance <= staffAttackDistance)
                {
                    eneState = 5;

                    ReleaseHidingSpot();

                    return;
                }

                // Use gun.
                if (hasGun)
                {
                    currentCrouchSpot =
                        FindBestCrouchSpot();

                    if (currentCrouchSpot != null)
                    {
                        targetMove =
                            currentCrouchSpot.transform;

                        isInCrouchSpot = false;

                        eneState = 2;

                        ReleaseHidingSpot();

                        return;
                    }

                    eneState = 2;

                    ReleaseHidingSpot();

                    return;
                }

                // No weapon.
                // Run toward the player.
                targetMove = player;

                agent.isStopped = false;

                agent.SetDestination(
                    player.position
                );

                ReleaseHidingSpot();
            }
        }
    }


    // =========================================================
    // 4 - GRABBED
    // =========================================================

    private void Grabbed()
    {
        if (agent != null)
        {
            agent.isStopped = true;
        }

        grabbedTimer -= Time.deltaTime;

        if (grabbedTimer <= 0f)
        {
            grabbedTimer = grabbedTime;

            eneState = 1;

            if (agent != null)
            {
                agent.isStopped = false;
            }
        }
    }


    // =========================================================
    // 5 - SWINGING
    // =========================================================

    private void Swinging()
    {
        if (player == null)
        {
            ReturnToSearching();
            return;
        }

        if (!hasStaff)
        {
            ReturnToSearching();
            return;
        }

        float distance =
            Vector3.Distance(
                transform.position,
                player.position
            );

        // -----------------------------------------------------
        // Player is too far away.
        // -----------------------------------------------------

        if (distance > staffAttackDistance + 1f)
        {
            targetMove = player;

            agent.isStopped = false;

            agent.SetDestination(
                player.position
            );

            return;
        }

        // -----------------------------------------------------
        // Stop and attack.
        // -----------------------------------------------------

        agent.isStopped = true;

        LookAtPlayer();

        staffAttackTimer -= Time.deltaTime;

        if (staffAttackTimer <= 0f)
        {
            StaffAttack();

            staffAttackTimer =
                staffAttackCooldown;
        }
    }


    // =========================================================
    // 6 - GRENADE
    // =========================================================

    private void Grenade()
    {
        if (player == null)
        {
            ReturnToSearching();
            return;
        }

        if (grenadeCount <= 0)
        {
            ReturnToSearching();
            return;
        }

        if (!CanSeePlayer())
        {
            ReturnToSearching();
            return;
        }

        float distance =
            Vector3.Distance(
                transform.position,
                player.position
            );

        if (distance > grenadeDistance)
        {
            ReturnToSearching();
            return;
        }

        // -----------------------------------------------------
        // Stop.
        // -----------------------------------------------------

        agent.isStopped = true;

        // -----------------------------------------------------
        // Face player.
        // -----------------------------------------------------

        LookAtPlayer();

        // -----------------------------------------------------
        // Throw.
        // -----------------------------------------------------

        ThrowGrenade();

        grenadeCount--;

        grenadeTimer =
            grenadeCheckTime;

        ReturnToSearching();
    }


    // =========================================================
    // FIND BEST HIDING SPOT
    // =========================================================

    private GameObject FindBestHidingSpot()
    {
        GameObject bestSpot = null;

        float bestPathDistance =
            Mathf.Infinity;

        if (hidingSpots == null)
            return null;

        foreach (GameObject spot in hidingSpots)
        {
            if (spot == null)
                continue;

            // -------------------------------------------------
            // Calculate actual NavMesh path.
            // -------------------------------------------------

            NavMeshPath path =
                new NavMeshPath();

            bool pathFound =
                agent.CalculatePath(
                    spot.transform.position,
                    path
                );

            if (!pathFound)
                continue;

            if (path.status != NavMeshPathStatus.PathComplete)
                continue;

            // -------------------------------------------------
            // Calculate total path length.
            // -------------------------------------------------

            float pathDistance =
                GetPathLength(path);

            if (pathDistance < bestPathDistance)
            {
                bestPathDistance = pathDistance;

                bestSpot = spot;
            }
        }

        return bestSpot;
    }


    // =========================================================
    // FIND BEST CROUCH SPOT
    // =========================================================

    private GameObject FindBestCrouchSpot()
    {
        GameObject bestSpot = null;

        float bestPathDistance =
            Mathf.Infinity;

        if (crouchSpots == null)
            return null;

        foreach (GameObject spot in crouchSpots)
        {
            if (spot == null)
                continue;

            // -------------------------------------------------
            // Check that the spot can actually be reached.
            // -------------------------------------------------

            NavMeshPath path =
                new NavMeshPath();

            bool pathFound =
                agent.CalculatePath(
                    spot.transform.position,
                    path
                );

            if (!pathFound)
                continue;

            if (path.status != NavMeshPathStatus.PathComplete)
                continue;

            // -------------------------------------------------
            // Check distance.
            // -------------------------------------------------

            float pathDistance =
                GetPathLength(path);

            if (pathDistance > crouchDistance)
                continue;

            // -------------------------------------------------
            // Check if player can be seen from this spot.
            // -------------------------------------------------

            if (player != null)
            {
                if (!CanSeePlayerFromPosition(
                    spot.transform.position))
                {
                    continue;
                }
            }

            // -------------------------------------------------
            // Pick shortest reachable path.
            // -------------------------------------------------

            if (pathDistance < bestPathDistance)
            {
                bestPathDistance = pathDistance;

                bestSpot = spot;
            }
        }

        return bestSpot;
    }


    // =========================================================
    // GET NAVMESH PATH LENGTH
    // =========================================================

    private float GetPathLength(NavMeshPath path)
    {
        float length = 0f;

        if (path.corners.Length < 2)
            return Mathf.Infinity;

        for (int i = 1; i < path.corners.Length; i++)
        {
            length += Vector3.Distance(
                path.corners[i - 1],
                path.corners[i]
            );
        }

        return length;
    }


    // =========================================================
    // CHECK LINE OF SIGHT FROM AN ARBITRARY POSITION
    // =========================================================

    private bool CanSeePlayerFromPosition(
        Vector3 position)
    {
        if (player == null)
            return false;

        Vector3 eyePosition =
            position + Vector3.up * 1.5f;

        Vector3 playerPosition =
            player.position + Vector3.up * 1.0f;

        Vector3 direction =
            playerPosition - eyePosition;

        float distance =
            direction.magnitude;

        if (distance > sightRange)
            return false;

        direction.Normalize();

        RaycastHit hit;

        if (Physics.Raycast(
            eyePosition,
            direction,
            out hit,
            distance,
            sightBlockMask
        ))
        {
            if (hit.transform == player ||
                hit.transform.IsChildOf(player))
            {
                return true;
            }

            return false;
        }

        return true;
    }


    // =========================================================
    // PLAYER LINE OF SIGHT
    // =========================================================

    private bool CanSeePlayer()
    {
        if (player == null)
            return false;

        Vector3 eyePosition =
            transform.position +
            Vector3.up * 1.5f;

        Vector3 playerPosition =
            player.position +
            Vector3.up * 1.0f;

        Vector3 direction =
            playerPosition - eyePosition;

        float distance =
            direction.magnitude;

        // -----------------------------------------------------
        // Too far away.
        // -----------------------------------------------------

        if (distance > sightRange)
            return false;

        Vector3 normalizedDirection =
            direction.normalized;

        // -----------------------------------------------------
        // Field of view.
        // -----------------------------------------------------

        float angle =
            Vector3.Angle(
                transform.forward,
                normalizedDirection
            );

        if (angle > sightAngle / 2f)
            return false;

        // -----------------------------------------------------
        // Raycast.
        // -----------------------------------------------------

        RaycastHit hit;

        if (Physics.Raycast(
            eyePosition,
            normalizedDirection,
            out hit,
            distance,
            sightBlockMask
        ))
        {
            if (hit.transform == player ||
                hit.transform.IsChildOf(player))
            {
                return true;
            }

            return false;
        }

        return true;
    }


    // =========================================================
    // RANDOM WANDERING
    // =========================================================

    private void PickRandomDestination()
    {
        if (agent == null)
            return;

        Vector3 randomDirection =
            Random.insideUnitSphere *
            wanderRadius;

        randomDirection += transform.position;

        NavMeshHit hit;

        if (NavMesh.SamplePosition(
            randomDirection,
            out hit,
            wanderRadius,
            NavMesh.AllAreas
        ))
        {
            targetMove = null;

            agent.isStopped = false;

            agent.SetDestination(
                hit.position
            );
        }
    }


    // =========================================================
    // LOOK AT PLAYER
    // =========================================================

    private void LookAtPlayer()
    {
        if (player == null)
            return;

        Vector3 direction =
            player.position -
            transform.position;

        direction.y = 0f;

        if (direction == Vector3.zero)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * 8f
            );
    }


    // =========================================================
    // SHOOT
    // =========================================================

    private void ShootAtPlayer()
    {
        // Replace this with your actual gun system.

        Debug.Log(
            gameObject.name +
            " shoots at " +
            player.name
        );
    }


    // =========================================================
    // STAFF ATTACK
    // =========================================================

    private void StaffAttack()
    {
        // Replace this with your actual staff damage system.

        Debug.Log(
            gameObject.name +
            " swings at " +
            player.name
        );
    }


    // =========================================================
    // GRENADE
    // =========================================================

    private void ThrowGrenade()
    {
        if (grenadePrefab == null)
        {
            Debug.LogWarning(
                gameObject.name +
                " tried to throw a grenade but has no grenadePrefab."
            );

            return;
        }

        if (grenadeSpawnPoint == null)
        {
            Debug.LogWarning(
                gameObject.name +
                " tried to throw a grenade but has no grenadeSpawnPoint."
            );

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

            direction.Normalize();

            direction.y += 0.35f;

            rb.AddForce(
                direction.normalized * 10f,
                ForceMode.VelocityChange
            );
        }
    }


    // =========================================================
    // DAMAGE
    // =========================================================

    public void TakeDamage(int damage)
    {
        eneHealth -= damage;

        if (eneHealth <= 0)
        {
            eneHealth = 0;

            eneState = 0;

            return;
        }

        // Being attacked wakes the enemy.
        if (player != null)
        {
            if (eneState == 1)
            {
                targetMove = player;

                agent.isStopped = false;

                agent.SetDestination(
                    player.position
                );
            }
        }
    }


    // =========================================================
    // GRABBED
    // =========================================================

    public void SetGrabbed()
    {
        eneState = 4;

        grabbedTimer = grabbedTime;

        if (agent != null)
        {
            agent.isStopped = true;
        }
    }


    // =========================================================
    // RETURN TO SEARCHING
    // =========================================================

    private void ReturnToSearching()
    {
        eneState = 1;

        targetMove = null;

        isInCrouchSpot = false;

        currentCrouchSpot = null;

        currentHidingSpot = null;

        if (agent != null)
        {
            agent.isStopped = false;
        }
    }


    // =========================================================
    // RELEASE HIDING SPOT
    // =========================================================

    private void ReleaseHidingSpot()
    {
        currentHidingSpot = null;
    }
}

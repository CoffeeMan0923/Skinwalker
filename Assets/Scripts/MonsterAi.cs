using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class SimpleMonsterAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform eyePoint;

    [Header("Patrol")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float waypointReachDistance = 1.5f;
    [SerializeField] private float patrolSpeed = 2.5f;

    [Header("Vision")]
    [SerializeField] private float viewDistance = 30f;
    [SerializeField, Range(0f, 360f)] private float viewAngle = 120f;
    [SerializeField] private LayerMask visionMask = ~0;

    [Header("Chase")]
    [SerializeField] private float chaseSpeed = 5f;

    [Header("Day / Night")]
    [SerializeField] private Transform monsterSpawnPoint;
    [SerializeField] private Transform monsterEscapePoint;
    [SerializeField] private float escapeSpeed = 7f;
    [SerializeField] private float escapeDistance = 2f;

    private bool leavingForDay;
    private bool monsterActive;

    [Header("Chase Door")]
    [SerializeField] private float chaseDoorCheckDistance = 2f;
    [SerializeField] private float chaseDoorGiveUpTime = 1f;
    [SerializeField] private float chaseDoorDamage = 1f;

    [Header("Entrance Selection")]
    [SerializeField] private float minEntranceTime = 20f;
    [SerializeField] private float maxEntranceTime = 60f;

    [Header("Entrance Attack")]
    [SerializeField] private float attackDistance = 1.5f;
    [SerializeField] private float doorDamage = 1f;

    [Header("Hard Seek")]
    [SerializeField] private float hardSeekDoorDamage = 999f;

    [Header("Secondary Doors")]
    [SerializeField] private float secondaryDoorCheckDistance = 2f;
    [SerializeField] private float secondaryDoorStopTime = 1f;
    [SerializeField] private float secondaryDoorHitDamage = 999f;

    [Header("Monster Attack")]
    [SerializeField] private float damageAmount = 50f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private Animator animator;

    private float attackTimer;
    public bool IsIdle { get; private set; }

    private int currentWaypoint;

    private MonsterEntrance[] entrances;
    private MonsterEntrance currentEntrance;

    private float entranceTimer;

    private bool attackingEntrance;

    private bool hardSeeking;
    private bool isInside;

    private MonsterEntrance secondaryDoorTarget;
    private float secondaryDoorTimer;
    private bool attackingSecondaryDoor;

    private MonsterEntrance chaseDoorTarget;
    private float chaseDoorTimer;
    private bool dealingWithChaseDoor;

    private void Start()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        entrances = FindObjectsByType<MonsterEntrance>(
            FindObjectsSortMode.None
        );

        ResetEntranceTimer();

        // IMPORTANT:
        // Do NOT start patrol here.
        //
        // DaySystem controls when the monster actually spawns.
    }

    private void Update()
    {
        if (!monsterActive)
        {
            IsIdle = false;
            return;
        }

        if (CheckPlayerAttack())
        {
            return;
        }

        IsIdle = false;

        if (leavingForDay)
        {
            HandleLeavingForDay();
            return;
        }

        if (player == null)
            return;

        if (hardSeeking)
        {
            CheckBrokenWindows();

            if (attackingEntrance)
            {
                HandleEntranceAttack();
                return;
            }

            if (attackingSecondaryDoor)
            {
                HandleSecondaryDoor();
                return;
            }

            if (CheckForSecondaryDoor())
            {
                return;
            }

            HardSeekPlayer();
            return;
        }

        if (dealingWithChaseDoor)
        {
            HandleChaseDoor();
            return;
        }

        if (attackingEntrance)
        {
            HandleEntranceAttack();
            return;
        }

        CheckBrokenWindows();

        UpdateEntranceTimer();

        if (CanSeePlayer())
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    // =========================================================
    // RAYCAST MASK
    // =========================================================

    private int GetRaycastMask()
    {
        int mask = ~0;

        // Ignore Layer 8.
        mask &= ~(1 << 8);

        return mask;
    }

    // =========================================================
    // PATROL
    // =========================================================

    private void Patrol()
    {
        if (waypoints == null || waypoints.Length == 0)
            return;

        agent.isStopped = false;
        agent.speed = patrolSpeed;

        if (agent.pathPending)
            return;

        if (agent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            GoToNextWaypoint();
            return;
        }

        if (agent.remainingDistance <= waypointReachDistance)
        {
            currentWaypoint++;

            if (currentWaypoint >= waypoints.Length)
                currentWaypoint = 0;

            GoToNextWaypoint();
        }
    }

    private void GoToNextWaypoint()
    {
        if (waypoints == null || waypoints.Length == 0)
            return;

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[currentWaypoint] == null)
            {
                currentWaypoint++;

                if (currentWaypoint >= waypoints.Length)
                    currentWaypoint = 0;

                continue;
            }

            agent.isStopped = false;
            agent.speed = patrolSpeed;

            if (agent.SetDestination(
                waypoints[currentWaypoint].position))
            {
                return;
            }

            currentWaypoint++;

            if (currentWaypoint >= waypoints.Length)
                currentWaypoint = 0;
        }
    }

    // =========================================================
    // NORMAL CHASE
    // =========================================================

    private void ChasePlayer()
    {
        if (CheckForChaseDoor())
            return;

        agent.isStopped = false;
        agent.speed = chaseSpeed;

        agent.SetDestination(player.position);
    }

    // =========================================================
    // CHASE DOOR
    // =========================================================

    private bool CheckForChaseDoor()
    {
        Vector3 origin = eyePoint != null
            ? eyePoint.position
            : transform.position + Vector3.up;

        Vector3 direction = transform.forward;

        RaycastHit hit;

        if (!Physics.Raycast(
            origin,
            direction,
            out hit,
            chaseDoorCheckDistance,
            GetRaycastMask()))
        {
            return false;
        }

        MonsterEntrance entrance =
            hit.collider.GetComponentInParent<MonsterEntrance>();

        if (entrance == null)
            return false;

        if (entrance.IsWindow)
            return false;

        if (entrance.IsSecondaryDoor)
            return false;

        Door door = entrance.GetComponent<Door>();

        if (door == null)
            return false;

        if (!door.IsClosed)
            return false;

        if (CanSeePlayer())
            return false;

        if (entrance.AttackPoint == null)
            return false;

        chaseDoorTarget = entrance;
        dealingWithChaseDoor = true;

        chaseDoorTimer = chaseDoorGiveUpTime;

        agent.isStopped = false;
        agent.speed = chaseSpeed;

        agent.SetDestination(
            entrance.AttackPoint.position
        );

        return true;
    }

    private void HandleChaseDoor()
    {
        if (chaseDoorTarget == null)
        {
            FinishChaseDoor();
            return;
        }

        if (chaseDoorTarget.AttackPoint == null)
        {
            FinishChaseDoor();
            return;
        }

        if (CanSeePlayer())
        {
            FinishChaseDoor();
            return;
        }

        float distance = Vector3.Distance(
            transform.position,
            chaseDoorTarget.AttackPoint.position
        );

        if (distance > attackDistance)
        {
            agent.isStopped = false;

            agent.SetDestination(
                chaseDoorTarget.AttackPoint.position
            );

            return;
        }

        agent.isStopped = true;

        chaseDoorTimer -= Time.deltaTime;

        if (chaseDoorTimer > 0f)
            return;

        chaseDoorTarget.Damage(chaseDoorDamage);

        FinishChaseDoor();
    }

    private void FinishChaseDoor()
    {
        dealingWithChaseDoor = false;
        chaseDoorTarget = null;
        chaseDoorTimer = 0f;

        agent.isStopped = false;

        GoToNextWaypoint();
    }

    // =========================================================
    // HARD SEEK
    // =========================================================

    private void HardSeekPlayer()
    {
        agent.isStopped = false;
        agent.speed = chaseSpeed;

        agent.SetDestination(player.position);
    }

    // =========================================================
    // BROKEN WINDOWS
    // =========================================================

    private void CheckBrokenWindows()
    {
        if (entrances == null)
            return;

        foreach (MonsterEntrance entrance in entrances)
        {
            if (entrance == null)
                continue;

            if (!entrance.IsWindow)
                continue;

            if (!entrance.IsBroken)
                continue;

            if (entrance.IsBarricaded)
                continue;

            if (!entrance.IsReadyForEntry)
                continue;

            if (entrance.WindowTeleportPoint == null)
                continue;

            Vector3 targetPosition =
                entrance.WindowTeleportPoint.position;

            NavMeshHit hit;

            if (NavMesh.SamplePosition(
                targetPosition,
                out hit,
                2f,
                NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
            }
            else
            {
                agent.Warp(targetPosition);
            }

            isInside = true;
            hardSeeking = true;

            attackingEntrance = false;
            currentEntrance = null;

            agent.isStopped = false;

            HardSeekPlayer();

            return;
        }
    }

    // =========================================================
    // RANDOM ENTRANCE
    // =========================================================

    private void UpdateEntranceTimer()
    {
        entranceTimer -= Time.deltaTime;

        if (entranceTimer <= 0f)
        {
            PickRandomEntrance();
            ResetEntranceTimer();
        }
    }

    private void ResetEntranceTimer()
    {
        entranceTimer = Random.Range(
            minEntranceTime,
            maxEntranceTime
        );
    }

    private void PickRandomEntrance()
    {
        if (entrances == null || entrances.Length == 0)
            return;

        System.Collections.Generic.List<MonsterEntrance> validEntrances =
            new System.Collections.Generic.List<MonsterEntrance>();

        foreach (MonsterEntrance entrance in entrances)
        {
            if (entrance == null)
                continue;

            if (entrance.IsSecondaryDoor)
                continue;

            if (entrance.IsBroken)
                continue;

            if (entrance.AttackPoint == null)
                continue;

            validEntrances.Add(entrance);
        }

        if (validEntrances.Count == 0)
            return;

        MonsterEntrance chosen =
            validEntrances[
                Random.Range(0, validEntrances.Count)
            ];

        currentEntrance = chosen;
        attackingEntrance = true;

        agent.isStopped = false;
        agent.speed = patrolSpeed;

        agent.SetDestination(
            chosen.AttackPoint.position
        );
    }

    // =========================================================
    // ENTRANCE ATTACK
    // =========================================================

    private void HandleEntranceAttack()
    {
        if (currentEntrance == null)
        {
            FinishEntranceAttack();
            return;
        }

        if (currentEntrance.AttackPoint == null)
        {
            FinishEntranceAttack();
            return;
        }

        float distance = Vector3.Distance(
            transform.position,
            currentEntrance.AttackPoint.position
        );

        if (distance > attackDistance)
        {
            agent.isStopped = false;

            agent.SetDestination(
                currentEntrance.AttackPoint.position
            );

            return;
        }

        agent.isStopped = true;

        if (hardSeeking && !currentEntrance.IsWindow)
        {
            currentEntrance.Damage(
                hardSeekDoorDamage
            );

            FinishEntranceAttack();

            return;
        }

        if (currentEntrance.IsWindow)
        {
            currentEntrance.Damage(999f);

            FinishEntranceAttack();

            return;
        }

        currentEntrance.Damage(doorDamage);

        if (currentEntrance.IsBroken)
        {
            isInside = true;
            hardSeeking = true;

            attackingEntrance = false;
            currentEntrance = null;

            agent.isStopped = false;

            return;
        }

        FinishEntranceAttack();
    }

    private void FinishEntranceAttack()
    {
        attackingEntrance = false;
        currentEntrance = null;

        agent.isStopped = false;

        if (hardSeeking)
        {
            HardSeekPlayer();
            return;
        }

        GoToNextWaypoint();
    }

    // =========================================================
    // SECONDARY DOORS
    // =========================================================

    private bool CheckForSecondaryDoor()
    {
        Vector3 origin =
            transform.position + Vector3.up * 0.8f;

        Vector3 direction = transform.forward;

        RaycastHit hit;

        if (!Physics.Raycast(
            origin,
            direction,
            out hit,
            secondaryDoorCheckDistance,
            GetRaycastMask()))
        {
            return false;
        }

        MonsterEntrance entrance =
            hit.collider.GetComponentInParent<MonsterEntrance>();

        if (entrance == null)
            return false;

        if (!entrance.IsSecondaryDoor)
            return false;

        Door door =
            entrance.GetComponent<Door>();

        if (door == null)
            return false;

        if (!door.IsClosed)
            return false;

        secondaryDoorTarget = entrance;
        attackingSecondaryDoor = true;

        secondaryDoorTimer =
            secondaryDoorStopTime;

        agent.isStopped = true;

        return true;
    }

    private void HandleSecondaryDoor()
    {
        if (secondaryDoorTarget == null)
        {
            FinishSecondaryDoor();
            return;
        }

        agent.isStopped = true;

        secondaryDoorTimer -= Time.deltaTime;

        if (secondaryDoorTimer > 0f)
            return;

        secondaryDoorTarget.Damage(
            secondaryDoorHitDamage
        );

        FinishSecondaryDoor();
    }

    private void FinishSecondaryDoor()
    {
        attackingSecondaryDoor = false;
        secondaryDoorTarget = null;
        secondaryDoorTimer = 0f;

        agent.isStopped = false;

        HardSeekPlayer();
    }

    // =========================================================
    // VISION
    // =========================================================

    private bool CanSeePlayer()
    {
        Vector3 origin = eyePoint != null
            ? eyePoint.position
            : transform.position + Vector3.up;

        Vector3 target =
            player.position + Vector3.up;

        Vector3 direction = target - origin;
        float distance = direction.magnitude;

        if (distance > viewDistance)
            return false;

        float angle = Vector3.Angle(
            transform.forward,
            direction
        );

        if (angle > viewAngle * 0.5f)
            return false;

        RaycastHit[] hits = Physics.RaycastAll(
            origin,
            direction.normalized,
            distance,
            GetRaycastMask()
        );

        System.Array.Sort(
            hits,
            (a, b) => a.distance.CompareTo(b.distance)
        );

        foreach (RaycastHit hit in hits)
        {
            if (hit.transform.IsChildOf(transform))
                continue;

            PlayerHealth health =
                hit.collider.GetComponentInParent<PlayerHealth>();

            if (health != null)
                return true;

            return false;
        }

        return false;
    }

    // =========================================================
    // SPAWN
    // =========================================================

    public void SpawnMonster()
    {
        leavingForDay = false;
        monsterActive = true;

        gameObject.SetActive(true);

        // Reset AI states.
        attackingEntrance = false;
        attackingSecondaryDoor = false;
        dealingWithChaseDoor = false;

        currentEntrance = null;
        secondaryDoorTarget = null;
        chaseDoorTarget = null;

        // If the monster has not already entered the house,
        // start fresh patrol.
        if (!hardSeeking)
        {
            if (monsterSpawnPoint != null)
            {
                NavMeshHit hit;

                if (NavMesh.SamplePosition(
                    monsterSpawnPoint.position,
                    out hit,
                    3f,
                    NavMesh.AllAreas))
                {
                    agent.Warp(hit.position);
                }
                else
                {
                    agent.Warp(monsterSpawnPoint.position);
                }
            }

            // Start at waypoint 0 every time the monster spawns.
            currentWaypoint = 0;

            GoToNextWaypoint();
        }
    }

    // =========================================================
    // DAY
    // =========================================================

    public void StartLeavingForDay()
    {
        if (hardSeeking)
            return;

        if (!monsterActive)
            return;

        leavingForDay = true;

        attackingEntrance = false;
        attackingSecondaryDoor = false;
        dealingWithChaseDoor = false;

        currentEntrance = null;
        secondaryDoorTarget = null;
        chaseDoorTarget = null;

        agent.isStopped = false;
        agent.speed = escapeSpeed;

        if (monsterEscapePoint != null)
        {
            agent.SetDestination(
                monsterEscapePoint.position
            );
        }
    }

    public void HideMonster()
    {
        monsterActive = false;
        leavingForDay = false;

        if (agent != null)

        gameObject.SetActive(false);
    }

    private void HandleLeavingForDay()
    {
        if (monsterEscapePoint == null)
        {
            HideMonster();
            return;
        }

        agent.isStopped = false;
        agent.speed = escapeSpeed;

        if (agent.pathPending)
            return;

        if (Vector3.Distance(
            transform.position,
            monsterEscapePoint.position
        ) <= escapeDistance)
        {
            HideMonster();
        }
    }

    // =========================================================
    // GIZMOS
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        Transform eye = eyePoint != null
            ? eyePoint
            : transform;

        Gizmos.DrawWireSphere(
            eye.position,
            viewDistance
        );

        Vector3 left =
            Quaternion.Euler(
                0,
                -viewAngle * 0.5f,
                0
            ) * transform.forward;

        Vector3 right =
            Quaternion.Euler(
                0,
                viewAngle * 0.5f,
                0
            ) * transform.forward;

        Gizmos.DrawRay(
            eye.position,
            left * viewDistance
        );

        Gizmos.DrawRay(
            eye.position,
            right * viewDistance
        );
    }
    public void CancelWindowTarget(MonsterEntrance entrance)
    {
        if (entrance == null)
            return;

        // Cancel normal entrance attack
        if (currentEntrance == entrance)
        {
            attackingEntrance = false;
            currentEntrance = null;

            agent.isStopped = false;

            GoToNextWaypoint();
        }

        // Cancel hard seeking
        if (hardSeeking && entrance.IsWindow)
        {
            hardSeeking = false;
            isInside = false;

            attackingEntrance = false;
            currentEntrance = null;

            agent.isStopped = false;
            agent.speed = patrolSpeed;

            GoToNextWaypoint();
        }
    }
    private bool CheckPlayerAttack()
    {
        if (player == null)
        {
            IsIdle = false;
            agent.isStopped = false;
            return false;
        }

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        // Player is outside attack range
        if (distance > attackDistance)
        {
            IsIdle = false;
            agent.isStopped = false;
            return false;
        }

        // Player is in attack range
        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        IsIdle = true;

        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0f)
        {
            PlayerHealth health =
                player.GetComponent<PlayerHealth>();

            if (health != null)
            {
                health.Damage(damageAmount);
            }

            if (animator != null)
            {
                StartCoroutine(AttackAnimation());
            }

            attackTimer = attackCooldown;
        }

        return true;
    }
    private IEnumerator AttackAnimation()
    {
        animator.SetBool("Attack", true);

        yield return new WaitForSeconds(0.1f);

        animator.SetBool("Attack", false);

        yield return null;
    }
}
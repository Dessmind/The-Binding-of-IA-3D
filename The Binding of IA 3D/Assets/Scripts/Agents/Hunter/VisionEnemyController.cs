using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class VisionEnemyController : EnemyVisionBase
{
    [Header("Vision Settings")]
    public float visionAngle = 45f;
    public float visionDistance = 10f;
    public LayerMask targetMask;
    public LayerMask obstructionMask;

    [Header("Patrol Settings")]
    public List<Transform> patrolPoints;
    private int currentPatrolIndex = 0;
    public float patrolWaitTime = 0.5f;
    public float patrolRadius = 10f;
    private bool isPatrolling = true;

    private NavMeshAgent navMeshAgent;
    private Transform target;
    private Vector3 initialPosition;
    private bool isChasing = false;
    private float lostSightTimer = 0f;
    public float lostSightDuration = 3f;

    [Header("Shooting Settings")]
    public GameObject enemyProjectilePrefab;
    public Transform targetShoot;
    public float projectileSpeed = 10f;
    public float shootRate = 1f;
    private float lastShootTime;

    // Método para inicializar con jugador, cámara y puntos de patrullaje
    public void Initialize(Transform playerTransform, Camera mainCamera, List<Transform> patrolPoints)
    {
        target = playerTransform;
        this.mainCamera = mainCamera;
        this.patrolPoints = patrolPoints;

        // Asegúrate de que navMeshAgent está inicializado
        if (navMeshAgent == null)
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
        }

        if (navMeshAgent != null && patrolPoints != null && patrolPoints.Count > 0)
        {
            currentPatrolIndex = 0;
            navMeshAgent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }
        else
        {
            Debug.LogWarning("NavMeshAgent or patrolPoints not properly set.");
        }
    }

    protected override void Start()
    {
        base.Start();
        navMeshAgent = GetComponent<NavMeshAgent>();
        initialPosition = transform.position;

        if (patrolPoints != null && patrolPoints.Count > 0)
        {
            navMeshAgent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }
        else
        {
            SetRandomPatrol();
        }

        HideQuestionMark();
    }

    private void Update()
    {
        if (isChasing)
        {
            ChaseTarget();
            ShootAtTarget();
        }
        else if (isPatrolling)
        {
            Patrol();
        }

        CheckVision();
    }

    public void SetRandomPatrol()
    {
        StartCoroutine(RandomPatrolRoutine());
    }

    private IEnumerator RandomPatrolRoutine()
    {
        while (true)
        {
            Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
            randomDirection += transform.position;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, NavMesh.AllAreas))
            {
                navMeshAgent.SetDestination(hit.position);
            }
            yield return new WaitUntil(() => !navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance);
            yield return new WaitForSeconds(patrolWaitTime);
        }
    }

    private void Patrol()
    {
        if (!navMeshAgent.isOnNavMesh) return;

        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            StartCoroutine(WaitAtPatrolPoint());
        }
    }

    private IEnumerator WaitAtPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Count == 0)
        {
            Debug.LogWarning("No patrol points assigned for patrol.");
            yield break;
        }

        float waitTime = patrolWaitTime > 0 ? patrolWaitTime : 0.5f;

        if (navMeshAgent.isOnNavMesh)
        {
            isPatrolling = false;
            yield return new WaitForSeconds(waitTime);
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Count;

            navMeshAgent.SetDestination(patrolPoints[currentPatrolIndex].position);
            isPatrolling = true;
        }
        else
        {
            Debug.LogError("NavMeshAgent is not on NavMesh.");
        }
    }

    private void CheckVision()
    {
        Collider[] targetsInView = Physics.OverlapSphere(transform.position, visionDistance, targetMask);

        Transform closestTarget = null;
        float closestDistance = visionDistance;

        foreach (Collider col in targetsInView)
        {
            if (col.CompareTag("Enemy") || col.CompareTag("Player"))
            {
                Transform potentialTarget = col.transform;
                Vector3 directionToTarget = (potentialTarget.position - transform.position).normalized;

                if (Vector3.Angle(transform.forward, directionToTarget) < visionAngle / 2)
                {
                    float distanceToTarget = Vector3.Distance(transform.position, potentialTarget.position);

                    if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstructionMask))
                    {
                        if (distanceToTarget < closestDistance)
                        {
                            closestTarget = potentialTarget;
                            closestDistance = distanceToTarget;
                        }
                    }
                }
            }
        }

        if (closestTarget != null)
        {
            StartChase(closestTarget);
            lostSightTimer = 0f;
            HideQuestionMark();
        }
        else if (isChasing)
        {
            lostSightTimer += Time.deltaTime;
            if (lostSightTimer >= lostSightDuration)
            {
                StopChase();
            }
        }
    }

    private void StartChase(Transform newTarget)
    {
        if (isChasing) return;

        isChasing = true;
        isPatrolling = false;
        target = newTarget;
        StopCoroutine("ReturnToPatrol");
    }

    private void ChaseTarget()
    {
        if (target != null && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.SetDestination(target.position);
            Vector3 directionToTarget = (target.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToTarget.x, 0, directionToTarget.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }

    private void StopChase()
    {
        isChasing = false;
        target = null;
        lostSightTimer = 0f;

        ShowQuestionMark();
        StartCoroutine(ReturnToPatrol());
    }

    private IEnumerator ReturnToPatrol()
    {
        yield return new WaitForSeconds(1f);
        isPatrolling = true;
        if (patrolPoints != null && patrolPoints.Count > 0 && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }
        else
        {
            SetRandomPatrol();
        }
    }

    private void ShootAtTarget()
    {
        if (target == null || enemyProjectilePrefab == null || Time.time < lastShootTime + shootRate) return;

        lastShootTime = Time.time;

        Vector3 shootDirection = (target.position - targetShoot.position).normalized;
        GameObject projectileInstance = Instantiate(enemyProjectilePrefab, targetShoot.position, Quaternion.LookRotation(shootDirection));
        Rigidbody rb = projectileInstance.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.velocity = shootDirection * projectileSpeed;
        }

        Collider enemyCollider = GetComponent<Collider>();
        Collider projectileCollider = projectileInstance.GetComponent<Collider>();
        if (enemyCollider != null && projectileCollider != null)
        {
            Physics.IgnoreCollision(enemyCollider, projectileCollider);
        }
    }
}

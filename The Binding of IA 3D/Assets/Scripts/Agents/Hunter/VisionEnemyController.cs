using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class VisionEnemyController : EnemyVisionBase
{
    [Header("Vision Settings")]
    public float visionAngle = 45f; // Ángulo de visión del enemigo
    public float visionDistance = 10f; // Distancia máxima a la que puede ver
    public LayerMask targetMask; // Máscara para los objetivos (ej. jugador)
    public LayerMask obstructionMask; // Máscara para detectar obstrucciones en la visión

    [Header("Patrol Settings")]
    public List<Transform> patrolPoints; // Lista de puntos de patrullaje
    private int currentPatrolIndex = 0; // Índice del punto de patrullaje actual
    public float patrolWaitTime = 0.5f; // Tiempo que espera en cada punto
    public float patrolRadius = 10f; // Radio para patrullaje aleatorio
    private bool isPatrolling = true; // Indica si está en modo patrullaje

    private NavMeshAgent navMeshAgent; // Agente de navegación
    private Transform target; // Referencia al objetivo a seguir
    private Vector3 initialPosition; // Posición inicial del enemigo
    private bool isChasing = false; // Indica si está persiguiendo al objetivo
    private float lostSightTimer = 0f; // Tiempo desde que perdió de vista al objetivo
    public float lostSightDuration = 3f; // Tiempo máximo que seguirá persiguiendo después de perder de vista

    [Header("Shooting Settings")]
    public GameObject enemyProjectilePrefab; // Prefab del proyectil
    public Transform targetShoot; // Punto de disparo
    public float projectileSpeed = 10f; // Velocidad del proyectil
    public float shootRate = 1f; // Cadencia de disparo
    private float lastShootTime; // Controla el último disparo

    // Inicializa el enemigo con jugador, cámara y puntos de patrullaje
    public void Initialize(Transform playerTransform, Camera mainCamera, List<Transform> patrolPoints)
    {
        target = playerTransform;
        this.mainCamera = mainCamera;
        this.patrolPoints = patrolPoints;

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

    // Configura una patrulla aleatoria
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

    // Patrulla entre los puntos asignados
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

    // Verifica si el objetivo está dentro de la visión
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

    // Persigue al objetivo
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

    // Visualización de los Gizmos para los radios de visión
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionDistance); // Visualiza el radio de visión

        Vector3 visionLeftBoundary = Quaternion.Euler(0, -visionAngle / 2, 0) * transform.forward * visionDistance;
        Vector3 visionRightBoundary = Quaternion.Euler(0, visionAngle / 2, 0) * transform.forward * visionDistance;

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, visionLeftBoundary); // Marca el ángulo izquierdo de visión
        Gizmos.DrawRay(transform.position, visionRightBoundary); // Marca el ángulo derecho de visión
    }
}

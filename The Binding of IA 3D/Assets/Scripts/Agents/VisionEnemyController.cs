using UnityEngine;
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
    public float patrolWaitTime = 2f;
    private bool isPatrolling = true;

    private NavMeshAgent navMeshAgent;
    private Transform target;
    private Vector3 initialPosition;
    private bool isChasing = false;
    private float lostSightTimer = 0f;
    public float lostSightDuration = 3f;

    [Header("Shooting Settings")]
    public GameObject enemyProjectilePrefab; // Prefab del proyectil enemigo
    public Transform targetShoot; // Punto desde el cual se dispararán los proyectiles
    public float projectileSpeed = 10f; // Velocidad del proyectil
    public float shootRate = 1f; // Frecuencia de disparo
    private float lastShootTime;

    protected override void Start()
    {
        base.Start();
        navMeshAgent = GetComponent<NavMeshAgent>();
        initialPosition = transform.position;

        if (patrolPoints != null && patrolPoints.Count > 0)
        {
            navMeshAgent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }

        HideQuestionMark();
    }

    void Update()
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

    private void Patrol()
    {
        if (!navMeshAgent.isOnNavMesh) return;

        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            StartCoroutine(WaitAtPatrolPoint());
        }
    }

    IEnumerator WaitAtPatrolPoint()
    {
        isPatrolling = false;
        yield return new WaitForSeconds(patrolWaitTime);
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Count;
        if (navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }
        isPatrolling = true;
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

    IEnumerator ReturnToPatrol()
    {
        yield return new WaitForSeconds(1f);
        isPatrolling = true;
        if (navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }
    }

    private void ShootAtTarget()
    {
        if (target == null || enemyProjectilePrefab == null || Time.time < lastShootTime + shootRate) return;

        lastShootTime = Time.time;

        // Calcula la dirección hacia el objetivo (target) desde el punto de disparo (targetShoot)
        Vector3 shootDirection = (target.position - targetShoot.position).normalized;

        // Instancia el proyectil en el punto de disparo y establece su rotación para que apunte hacia el objetivo
        GameObject projectileInstance = Instantiate(enemyProjectilePrefab, targetShoot.position, Quaternion.LookRotation(shootDirection));
        Rigidbody rb = projectileInstance.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.velocity = shootDirection * projectileSpeed; // Aplica la velocidad en la dirección calculada
        }

        // Ignora colisiones entre el proyectil y el propio enemigo que dispara
        Collider enemyCollider = GetComponent<Collider>();
        Collider projectileCollider = projectileInstance.GetComponent<Collider>();
        if (enemyCollider != null && projectileCollider != null)
        {
            Physics.IgnoreCollision(enemyCollider, projectileCollider);
        }
    }


}

using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class EnemyControllerNavMesh : EnemyBase
{
    [Header("Patrol Settings")]
    public List<Transform> patrolPoints;
    private int currentPatrolIndex = 0;
    public float patrolWaitTime = 0f;
    public float patrolRadius = 10f;
    private bool isPatrolling = true;

    private NavMeshAgent navMeshAgent;
    private bool isJumping = false;
    private Transform targetPoint;

    protected override void Start()
    {
        base.Start();
        navMeshAgent = GetComponent<NavMeshAgent>();

        if (navMeshAgent == null)
        {
            Debug.LogError("NavMeshAgent no está asignado en " + gameObject.name);
            return;
        }

        if (patrolPoints != null && patrolPoints.Count > 0 && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }
        else
        {
            SetRandomPatrol();
        }
    }

    private void Update()
    {
        if (isPatrolling)
        {
            Patrol();
        }
        else if (targetPoint != null && navMeshAgent.enabled && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.SetDestination(targetPoint.position);
        }

        if (navMeshAgent.isOnOffMeshLink && !isJumping)
        {
            StartCoroutine(HandleJump());
        }
    }

    public void Initialize(Transform target, Camera camera, List<Transform> patrolPoints = null)
    {
        navMeshAgent = GetComponent<NavMeshAgent>();

        if (navMeshAgent == null)
        {
            Debug.LogError("NavMeshAgent no está asignado en " + gameObject.name);
            return;
        }

        targetPoint = target;
        mainCamera = camera;
        this.patrolPoints = patrolPoints; // Asignar directamente los puntos de patrullaje
        currentPatrolIndex = 0; // Reinicia el índice de patrullaje

        if (this.patrolPoints != null && this.patrolPoints.Count > 0 && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.SetDestination(this.patrolPoints[currentPatrolIndex].position);
            isPatrolling = true; // Empieza a patrullar si hay puntos
        }
        else if (targetPoint != null)
        {
            navMeshAgent.SetDestination(targetPoint.position);
            isPatrolling = false; // Si no hay puntos de patrullaje, va hacia el targetPoint
        }
        else
        {
            SetRandomPatrol(); // Alternativa aleatoria si no hay puntos ni target
        }
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
            if (NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, NavMesh.AllAreas) && navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.SetDestination(hit.position);
            }

            yield return new WaitUntil(() => navMeshAgent.isOnNavMesh && !navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance);
            yield return new WaitForSeconds(patrolWaitTime);
        }
    }

    private void Patrol()
    {
        if (navMeshAgent == null || patrolPoints == null || patrolPoints.Count == 0 || !navMeshAgent.isOnNavMesh) return;

        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            StartCoroutine(WaitAtPatrolPoint());
        }
    }

    private IEnumerator WaitAtPatrolPoint()
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

    private IEnumerator HandleJump()
    {
        isJumping = true;
        OffMeshLinkData linkData = navMeshAgent.currentOffMeshLinkData;
        Vector3 startPos = navMeshAgent.transform.position;
        Vector3 endPos = linkData.endPos + Vector3.up * navMeshAgent.baseOffset;

        float jumpHeight = 2.0f;
        float jumpDuration = 1f;
        float timeElapsed = 0;

        navMeshAgent.isStopped = true;

        while (timeElapsed < jumpDuration)
        {
            float t = timeElapsed / jumpDuration;
            float height = Mathf.Sin(Mathf.PI * t) * jumpHeight;
            navMeshAgent.transform.position = Vector3.Lerp(startPos, endPos, t) + Vector3.up * height;
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        navMeshAgent.transform.position = endPos;
        navMeshAgent.CompleteOffMeshLink();
        navMeshAgent.isStopped = false;
        isJumping = false;
    }

    protected override void Die(bool causedByPlayer = false)
    {
        if (navMeshAgent != null && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.enabled = false;
        }

        base.Die(causedByPlayer);
    }

    // Sobreescribimos el método OnTriggerEnter para que no haga daño al jugador
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Projectile"))
        {
            Projectile projectile = other.GetComponent<Projectile>();
            if (projectile != null)
            {
                TakeDamage(projectile.damage, causedByPlayer: true);
                Destroy(other.gameObject);
            }
        }
        else if (other.CompareTag("EnemyVision"))
        {
            TakeDamage(contactDamage, causedByPlayer: false);
        }
    }
}

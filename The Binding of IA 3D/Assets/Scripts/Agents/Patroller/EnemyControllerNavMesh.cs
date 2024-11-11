using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

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
        this.patrolPoints = patrolPoints ?? this.patrolPoints;

        if (targetPoint != null && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.SetDestination(targetPoint.position);
            isPatrolling = false;
        }
        else if (this.patrolPoints != null && this.patrolPoints.Count > 0)
        {
            navMeshAgent.SetDestination(this.patrolPoints[currentPatrolIndex].position);
            isPatrolling = true;
        }
        else
        {
            SetRandomPatrol();
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

    // Sobreescribimos el método Die para desactivar el agente cuando el enemigo muere
    // Sobreescribimos el método Die para desactivar el agente cuando el enemigo muere
    protected override void Die(bool causedByPlayer = false)
    {
        if (navMeshAgent != null && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.enabled = false;
        }

        base.Die(causedByPlayer); // Llama al método Die de la clase base para manejar la muerte
    }

}



using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class EnemyControllerNavMesh : EnemyBase
{
    public List<Transform> patrolPoints; // Puntos de patrullaje
    private int currentPatrolIndex = 0;
    public float patrolWaitTime = 2f;
    private bool isPatrolling = true;

    private NavMeshAgent navMeshAgent;
    private bool isJumping = false;
    private Transform targetPoint;

    protected override void Start()
    {
        base.Start();
        navMeshAgent = GetComponent<NavMeshAgent>();

        // Evita continuar si el NavMeshAgent no está configurado
        if (navMeshAgent == null)
        {
            Debug.LogError("NavMeshAgent no está asignado en " + gameObject.name);
            return;
        }

        // Comienza la patrulla después de un breve retraso para asegurar que los puntos de patrullaje estén asignados
        StartCoroutine(StartPatrolAfterAssignment());
    }

    private IEnumerator StartPatrolAfterAssignment()
    {
        // Espera un fotograma para asegurar que patrolPoints se haya asignado
        yield return null;

        if (patrolPoints != null && patrolPoints.Count > 0 && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }
        else
        {
            Debug.LogWarning("Patrol points no asignados o vacíos en " + gameObject.name);
        }
    }

    void Update()
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
        targetPoint = target;
        mainCamera = camera;
        this.patrolPoints = patrolPoints;

        // Asegúrate de que `navMeshAgent` y `patrolPoints` no sean nulos
        if (navMeshAgent == null || patrolPoints == null || patrolPoints.Count == 0)
        {
            Debug.LogError("NavMeshAgent o patrolPoints no están asignados correctamente en " + gameObject.name);
            return;
        }

        if (targetPoint != null && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.SetDestination(targetPoint.position);
            isPatrolling = false;
        }
        else
        {
            navMeshAgent.SetDestination(patrolPoints[currentPatrolIndex].position);
            isPatrolling = true;
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

    IEnumerator HandleJump()
    {
        isJumping = true;
        OffMeshLinkData linkData = navMeshAgent.currentOffMeshLinkData;
        Vector3 startPos = navMeshAgent.transform.position;
        Vector3 endPos = linkData.endPos + Vector3.up * navMeshAgent.baseOffset;

        float jumpHeight = 2.0f;
        float jumpDuration = 0.5f;
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
}

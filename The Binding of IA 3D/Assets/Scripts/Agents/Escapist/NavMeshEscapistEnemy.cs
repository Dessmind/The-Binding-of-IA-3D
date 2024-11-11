using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class NavMeshEscapistEnemy : EscapistEnemyBase
{
    [Header("Escapist Enemy Settings")]
    public float detectionRadius = 10f;
    public float fleeRadius = 5f;
    public float fleeDuration = 5f;
    public float tiredDuration = 5f;
    public float normalSpeed = 3.5f;
    public float fleeSpeed = 6f;
    public float shootingCooldown = 1.5f;
    public float fleeCheckInterval = 0.5f; // Intervalo para revisar y ajustar la dirección de huida

    [Header("Shooting Settings")]
    public GameObject enemyProjectilePrefab;
    public Transform targetShoot;
    public float normalProjectileForce = 10f;
    public float tiredProjectileForce = 5f;

    [Header("References")]
    public Transform enemyBody;

    private NavMeshAgent agent;
    private Transform player;
    private bool isEscapistTired = false;
    private bool isFleeing = false;
    private float nextShootTime;
    private float fleeEndTime;
    private float tiredEndTime;
    private float nextFleeCheckTime;

    protected override void Start()
    {
        base.Start();
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = normalSpeed;
        }

        // Intenta encontrar al jugador
        StartCoroutine(WaitForPlayer());
    }

    private IEnumerator WaitForPlayer()
    {
        while (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            yield return null;
        }

        // Inicia en el estado activo una vez encontrado el jugador
        EnterActiveState();
    }

    public void Initialize(Transform playerTransform, Camera mainCam)
    {
        player = playerTransform;
        mainCamera = mainCam;
        agent = GetComponent<NavMeshAgent>();

        agent.speed = normalSpeed;
        EnterActiveState();

        if (agent != null)
        {
            agent.speed = normalSpeed;
            EnterActiveState(); // Inicia en estado activo
        }
    }


    void Update()
    {
        if (player == null || agent == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (isFleeing)
        {
            HandleFleeState();
        }
        else if (isEscapistTired)
        {
            HandleTiredState();
        }
        else if (distanceToPlayer <= fleeRadius)
        {
            EnterFleeState();
        }
        else if (distanceToPlayer <= detectionRadius)
        {
            HandleDetectionRadius();
        }
        else
        {
            UpdateDestinationToPlayer();
        }
    }

    private void EnterFleeState()
    {
        isFleeing = true;
        isEscapistTired = false;
        agent.speed = fleeSpeed;
        fleeEndTime = Time.time + fleeDuration;
        nextFleeCheckTime = Time.time + fleeCheckInterval;
        UpdateDestinationToFlee();
    }

    private void HandleFleeState()
    {
        if (Time.time >= fleeEndTime)
        {
            EnterTiredState();
            return;
        }

        if (Vector3.Distance(transform.position, player.position) <= fleeRadius && Time.time >= nextFleeCheckTime)
        {
            UpdateDestinationToFlee();
            nextFleeCheckTime = Time.time + fleeCheckInterval;
        }

        if (Time.time >= nextShootTime)
        {
            ShootAtPlayer(normalProjectileForce);
            nextShootTime = Time.time + shootingCooldown;
        }
    }

    private void UpdateDestinationToFlee()
    {
        Vector3 fleeDirection = (transform.position - player.position).normalized;

        // Introduce algo de aleatoriedad en la dirección de huida
        fleeDirection += new Vector3(Random.Range(-0.2f, 0.2f), 0, Random.Range(-0.2f, 0.2f)).normalized;
        fleeDirection.Normalize();

        Vector3 fleePosition = transform.position + fleeDirection * fleeRadius * 2; // Aumentamos la distancia para una posición de huida más lejana

        if (NavMesh.SamplePosition(fleePosition, out NavMeshHit hit, fleeRadius * 2, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            // Si no encuentra una posición válida, intenta un radio menor
            fleePosition = transform.position + fleeDirection * fleeRadius;
            if (NavMesh.SamplePosition(fleePosition, out hit, fleeRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }
    }

    private void EnterTiredState()
    {
        isEscapistTired = true;
        isFleeing = false;
        agent.ResetPath();
        agent.speed = 0;
        ShowTiredMark();
        tiredEndTime = Time.time + tiredDuration;
    }

    private void HandleTiredState()
    {
        if (Time.time >= tiredEndTime)
        {
            HideTiredMark();
            EnterActiveState();
        }
        else
        {
            // Dispara al jugador a menor potencia mientras está cansado
            if (Time.time >= nextShootTime)
            {
                ShootAtPlayer(tiredProjectileForce);
                nextShootTime = Time.time + shootingCooldown * 2; // Dispara más lento en estado cansado
            }
        }
    }

    private void EnterActiveState()
    {
        isEscapistTired = false;
        isFleeing = false;
        agent.speed = normalSpeed;
        UpdateDestinationToPlayer();
    }

    private void HandleDetectionRadius()
    {
        if (Time.time >= nextShootTime)
        {
            ShootAtPlayer(normalProjectileForce);
            nextShootTime = Time.time + shootingCooldown;
        }

        agent.ResetPath(); // Detiene el movimiento al estar en el detection radius
    }

    private void UpdateDestinationToPlayer()
    {
        if (agent != null && player != null)
        {
            agent.SetDestination(player.position);
        }
    }

    private void ShootAtPlayer(float projectileForce)
    {
        if (enemyProjectilePrefab == null || targetShoot == null) return;

        Vector3 shootDirection = (player.position - targetShoot.position).normalized;
        GameObject projectileInstance = Instantiate(enemyProjectilePrefab, targetShoot.position, Quaternion.LookRotation(shootDirection));
        Rigidbody rb = projectileInstance.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.velocity = shootDirection * projectileForce;
        }

        Collider enemyCollider = GetComponent<Collider>();
        Collider projectileCollider = projectileInstance.GetComponent<Collider>();
        if (enemyCollider != null && projectileCollider != null)
        {
            Physics.IgnoreCollision(enemyCollider, projectileCollider);
        }
    }

    // Añadir los Gizmos para visualización en el editor
    private void OnDrawGizmosSelected()
    {
        // Color para el detectionRadius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Color para el fleeRadius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, fleeRadius);
    }
}

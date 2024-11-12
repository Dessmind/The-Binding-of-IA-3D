using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class NavMeshEscapistEnemy : EscapistEnemyBase
{
    [Header("Escapist Enemy Settings")]
    public float detectionRadius = 10f; // Distancia en la que detecta al jugador
    public float fleeRadius = 5f; // Distancia mínima antes de que empiece a huir
    public float fleeDuration = 5f; // Cuánto tiempo estará huyendo
    public float tiredDuration = 5f; // Cuánto tiempo estará cansado después de huir
    public float normalSpeed = 3.5f; // Velocidad normal de movimiento
    public float fleeSpeed = 6f; // Velocidad al huir
    public float shootingCooldown = 1.5f; // Tiempo entre disparos
    public float fleeCheckInterval = 0.5f; // Cada cuánto revisa y ajusta la dirección de huida

    [Header("Shooting Settings")]
    public GameObject enemyProjectilePrefab; // Prefab del proyectil
    public Transform targetShoot; // Lugar desde donde dispara
    public float normalProjectileForce = 10f; // Fuerza del disparo en estado normal
    public float tiredProjectileForce = 5f; // Fuerza del disparo cuando está cansado

    [Header("References")]
    public Transform enemyBody; // Transform del cuerpo del enemigo para rotar o animar si es necesario

    private NavMeshAgent agent; // Agente de navegación para mover al enemigo en el mapa
    private Transform player; // Referencia al jugador
    private bool isEscapistTired = false; // Si el enemigo está cansado o no
    private bool isFleeing = false; // Si está huyendo o no
    private float nextShootTime; // Controla cuándo puede disparar de nuevo
    private float fleeEndTime; // Controla cuándo termina la huida
    private float tiredEndTime; // Controla cuándo termina el cansancio
    private float nextFleeCheckTime; // Controla el intervalo entre revisiones de la dirección de huida

    protected override void Start()
    {
        base.Start();
        agent = GetComponent<NavMeshAgent>(); // Inicializamos el agente de navegación
        if (agent != null)
        {
            agent.speed = normalSpeed; // Comienza a velocidad normal
        }

        // Empieza a buscar al jugador en la escena
        StartCoroutine(WaitForPlayer());
    }

    private IEnumerator WaitForPlayer()
    {
        while (player == null)
        {
            // Busca un objeto con la etiqueta "Player" (nuestro jugador)
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            yield return null; // Espera un frame y vuelve a intentar
        }

        // Cuando encuentra al jugador, pasa al estado activo
        EnterActiveState();
    }

    public void Initialize(Transform playerTransform, Camera mainCam)
    {
        player = playerTransform;
        mainCamera = mainCam;
        agent = GetComponent<NavMeshAgent>();

        agent.speed = normalSpeed;
        EnterActiveState();
    }

    void Update()
    {
        // Si el jugador o el agente no están, no hacemos nada
        if (player == null || agent == null) return;

        // Calcula la distancia entre el enemigo y el jugador
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Controla en qué estado está el enemigo en función de la distancia al jugador
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
            EnterFleeState(); // Entra en estado de huida
        }
        else if (distanceToPlayer <= detectionRadius)
        {
            HandleDetectionRadius(); // En rango para detectar al jugador y disparar
        }
        else
        {
            UpdateDestinationToPlayer(); // Mueve al enemigo hacia el jugador
        }
    }

    private void EnterFleeState()
    {
        isFleeing = true;
        isEscapistTired = false;
        agent.speed = fleeSpeed; // Cambia a velocidad de huida
        fleeEndTime = Time.time + fleeDuration; // Marca cuándo termina la huida
        nextFleeCheckTime = Time.time + fleeCheckInterval; // Programa la siguiente revisión de huida
        UpdateDestinationToFlee();
    }

    private void HandleFleeState()
    {
        // Si ya es tiempo de detener la huida, pasa al estado de cansancio
        if (Time.time >= fleeEndTime)
        {
            EnterTiredState();
            return;
        }

        // Cada cierto intervalo, recalcula la dirección de huida
        if (Vector3.Distance(transform.position, player.position) <= fleeRadius && Time.time >= nextFleeCheckTime)
        {
            UpdateDestinationToFlee();
            nextFleeCheckTime = Time.time + fleeCheckInterval;
        }

        // Dispara si tiene una línea de visión hacia el jugador
        if (Time.time >= nextShootTime && HasDirectLineOfSightToPlayer())
        {
            ShootAtPlayer(normalProjectileForce);
            nextShootTime = Time.time + shootingCooldown;
        }
    }

    private void UpdateDestinationToFlee()
    {
        // Calcula una dirección opuesta al jugador y le añade algo de aleatoriedad
        Vector3 fleeDirection = (transform.position - player.position).normalized;
        fleeDirection += new Vector3(Random.Range(-0.2f, 0.2f), 0, Random.Range(-0.2f, 0.2f)).normalized;
        fleeDirection.Normalize();

        // Encuentra una posición de huida más alejada
        Vector3 fleePosition = transform.position + fleeDirection * fleeRadius * 2;

        // Establece el destino de huida usando el NavMesh
        if (NavMesh.SamplePosition(fleePosition, out NavMeshHit hit, fleeRadius * 2, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            // Si no encuentra una posición, usa una distancia menor
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
        agent.ResetPath(); // Detiene al enemigo
        agent.speed = 0; // Sin movimiento
        ShowTiredMark(); // Muestra indicador de cansancio
        tiredEndTime = Time.time + tiredDuration; // Marca fin del cansancio
    }

    private void HandleTiredState()
    {
        // Si termina el tiempo de cansancio, vuelve al estado activo
        if (Time.time >= tiredEndTime)
        {
            HideTiredMark();
            EnterActiveState();
        }
        else if (Time.time >= nextShootTime && HasDirectLineOfSightToPlayer())
        {
            // Dispara con menos fuerza y a menor frecuencia cuando está cansado
            ShootAtPlayer(tiredProjectileForce);
            nextShootTime = Time.time + shootingCooldown * 2;
        }
    }

    private void EnterActiveState()
    {
        // Regresa a su estado normal
        isEscapistTired = false;
        isFleeing = false;
        agent.speed = normalSpeed;
        UpdateDestinationToPlayer();
    }

    private void HandleDetectionRadius()
    {
        // Dispara al jugador si está en rango y tiene línea de visión directa
        if (Time.time >= nextShootTime && HasDirectLineOfSightToPlayer())
        {
            ShootAtPlayer(normalProjectileForce);
            nextShootTime = Time.time + shootingCooldown;
        }

        agent.ResetPath(); // Detiene el movimiento cuando está en rango de detección
    }

    private void UpdateDestinationToPlayer()
    {
        // Si el agente y el jugador están listos, sigue al jugador
        if (agent != null && player != null)
        {
            agent.SetDestination(player.position);
        }
    }

    private bool HasDirectLineOfSightToPlayer()
    {
        // Verifica si tiene una línea de visión sin obstáculos hacia el jugador usando Raycast
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        RaycastHit hit;

        if (Physics.Raycast(transform.position, directionToPlayer, out hit, detectionRadius))
        {
            // Si golpea al jugador, hay línea de visión
            return hit.transform == player;
        }
        return false;
    }

    private void ShootAtPlayer(float projectileForce)
    {
        // Si no hay proyectil o punto de disparo, no hace nada
        if (enemyProjectilePrefab == null || targetShoot == null) return;

        // Crea y lanza un proyectil hacia el jugador
        Vector3 shootDirection = (player.position - targetShoot.position).normalized;
        GameObject projectileInstance = Instantiate(enemyProjectilePrefab, targetShoot.position, Quaternion.LookRotation(shootDirection));
        Rigidbody rb = projectileInstance.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.velocity = shootDirection * projectileForce;
        }

        // Ignora colisiones entre el proyectil y el enemigo para evitar autogolpearse
        Collider enemyCollider = GetComponent<Collider>();
        Collider projectileCollider = projectileInstance.GetComponent<Collider>();
        if (enemyCollider != null && projectileCollider != null)
        {
            Physics.IgnoreCollision(enemyCollider, projectileCollider);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Muestra círculos/gizmos de alcance en el editor para los radios de detección y huida
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, fleeRadius);
    }
}

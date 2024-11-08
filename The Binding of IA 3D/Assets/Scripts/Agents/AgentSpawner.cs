using UnityEngine;
using System.Collections.Generic;

public class AgentSpawner : MonoBehaviour
{
    public GameObject agentPrefab; // Prefab del agente
    public Transform spawnPoint; // Punto de spawn del agente
    public List<Transform> patrolPoints; // Puntos de patrullaje
    public Camera mainCamera; // C�mara principal

    private GameObject spawnedAgent; // Referencia al agente instanciado actual
    private bool isInitialized = false;

    private void Update()
    {
        if (!isInitialized && spawnedAgent != null)
        {
            InitializeAgent();
        }
    }

    public void SpawnAgentIfNeeded()
    {
        if (spawnedAgent != null)
        {
            Destroy(spawnedAgent);
        }

        SpawnAgent();
        isInitialized = false; // Resetea la bandera para reintentar la inicializaci�n
    }

    private void SpawnAgent()
    {
        spawnedAgent = Instantiate(agentPrefab, spawnPoint.position, spawnPoint.rotation);
    }

    private void InitializeAgent()
    {
        var enemyController = spawnedAgent.GetComponent<EnemyControllerNavMesh>();
        if (enemyController != null && patrolPoints != null && patrolPoints.Count > 0)
        {
            enemyController.Initialize(null, mainCamera, patrolPoints);
            isInitialized = true;
        }
        else
        {
            Debug.LogWarning("Patrol points no est�n asignados correctamente o faltan en el prefab.");
        }
    }
}

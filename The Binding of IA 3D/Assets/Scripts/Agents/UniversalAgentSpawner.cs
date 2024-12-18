using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UniversalAgentSpawner : MonoBehaviour
{
    public enum EnemyType
    {
        VisionEnemy,
        NavMeshEnemy,
        EscapistEnemy
    }

    [Header("General Settings")]
    public EnemyType enemyType;
    public Transform spawnPoint;
    public Transform playerTransform;

    [Header("Prefabs")]
    public GameObject visionEnemyPrefab;
    public GameObject navMeshEnemyPrefab;
    public GameObject escapistEnemyPrefab;

    [Header("Patrol Points")]
    public List<Transform> patrolPoints;

    private GameObject spawnedAgent;
    private Camera mainCamera;

    private void Start()
    {
        // Inicia la corrutina para esperar y asignar la c�mara, pero no instancia enemigos al inicio
        StartCoroutine(WaitForCamera());
    }

    private IEnumerator WaitForCamera()
    {
        // Espera hasta encontrar la c�mara principal
        while (mainCamera == null)
        {
            mainCamera = Camera.main;
            yield return new WaitForSeconds(0.1f); // Ajusta el tiempo seg�n sea necesario
        }
    }

    public void SpawnAgent()
    {
        if (spawnedAgent != null)
        {
            Destroy(spawnedAgent);
        }

        switch (enemyType)
        {
            case EnemyType.VisionEnemy:
                spawnedAgent = Instantiate(visionEnemyPrefab, spawnPoint.position, spawnPoint.rotation);
                SetupVisionEnemy();
                break;
            case EnemyType.NavMeshEnemy:
                spawnedAgent = Instantiate(navMeshEnemyPrefab, spawnPoint.position, spawnPoint.rotation);
                SetupNavMeshEnemy();
                break;
            case EnemyType.EscapistEnemy:
                spawnedAgent = Instantiate(escapistEnemyPrefab, spawnPoint.position, spawnPoint.rotation);
                SetupEscapistEnemy();
                break;
        }
    }

    private void SetupVisionEnemy()
    {
        VisionEnemyController visionEnemy = spawnedAgent.GetComponent<VisionEnemyController>();
        if (visionEnemy != null)
        {
            visionEnemy.Initialize(playerTransform, mainCamera, patrolPoints);
        }
        else
        {
            Debug.LogError("VisionEnemyController not found on VisionEnemy prefab.");
        }
    }

    private void SetupNavMeshEnemy()
    {
        EnemyControllerNavMesh navMeshEnemy = spawnedAgent.GetComponent<EnemyControllerNavMesh>();
        if (navMeshEnemy != null)
        {
            navMeshEnemy.Initialize(playerTransform, mainCamera, patrolPoints);
        }
        else
        {
            Debug.LogError("EnemyControllerNavMesh not found on NavMeshEnemy prefab.");
        }
    }

    private void SetupEscapistEnemy()
    {
        NavMeshEscapistEnemy escapistEnemy = spawnedAgent.GetComponent<NavMeshEscapistEnemy>();
        if (escapistEnemy != null)
        {
            escapistEnemy.Initialize(playerTransform, mainCamera);
        }
        else
        {
            Debug.LogError("NavMeshEscapistEnemy not found on EscapistEnemy prefab.");
        }
    }
}

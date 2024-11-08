using UnityEngine;
using System.Collections.Generic;

public class Agent2Spawner : MonoBehaviour
{
    public GameObject agent2Prefab; // Prefab del agente con visión
    public Transform spawnPoint; // Punto de spawn del agente
    public List<Transform> patrolPoints; // Lista de puntos de patrullaje para el agente
    public Camera mainCamera; // La cámara principal para asignar a los enemigos

    private GameObject spawnedAgent; // Referencia al agente instanciado actual

    public void SpawnAgent2IfNeeded()
    {
        // Si ya hay un agente en la escena, elimínalo
        if (spawnedAgent != null)
        {
            Destroy(spawnedAgent);
        }

        // Spawnea un nuevo agente
        SpawnAgent2();
    }

    private void SpawnAgent2()
    {
        spawnedAgent = Instantiate(agent2Prefab, spawnPoint.position, spawnPoint.rotation);

        // Asegúrate de que el prefab tenga el script VisionEnemyController
        VisionEnemyController visionEnemyController = spawnedAgent.GetComponent<VisionEnemyController>();
        if (visionEnemyController != null)
        {
            // Asigna los puntos de patrullaje y la cámara
            visionEnemyController.patrolPoints = patrolPoints;
            visionEnemyController.Initialize(null, mainCamera); // null para indicar que no necesita un playerTransform
        }
    }
}
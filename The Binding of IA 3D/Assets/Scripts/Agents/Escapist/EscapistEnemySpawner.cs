using UnityEngine;

public class EscapistEnemySpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject escapistEnemyPrefab; // Prefab del Escapist Enemy
    public Transform spawnPoint;           // Punto de spawn
    public Transform playerTransform;      // Referencia al transform del jugador
    public Camera mainCamera;              // Cámara principal

    private GameObject spawnedEscapistEnemy; // Referencia al enemigo instanciado

    // Método para instanciar y configurar el enemigo
    public void SpawnEscapistEnemy()
    {
        if (spawnedEscapistEnemy != null)
        {
            Destroy(spawnedEscapistEnemy);
        }

        // Instanciar el enemigo en el punto de spawn
        spawnedEscapistEnemy = Instantiate(escapistEnemyPrefab, spawnPoint.position, spawnPoint.rotation);

        // Configurar referencias del jugador y la cámara
        NavMeshEscapistEnemy escapistEnemy = spawnedEscapistEnemy.GetComponent<NavMeshEscapistEnemy>();
        if (escapistEnemy != null)
        {
            escapistEnemy.Initialize(playerTransform, mainCamera);
            Debug.Log("Escapist Enemy spawned and initialized with player and camera references.");
        }
        else
        {
            Debug.LogError("No se encontró el script NavMeshEscapistEnemy en el prefab.");
        }
    }
}

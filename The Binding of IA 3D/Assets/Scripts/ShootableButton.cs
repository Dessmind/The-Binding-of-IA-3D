using UnityEngine;
using System.Collections;

public class ShootableButton : MonoBehaviour
{
    public enum SpawnerType
    {
        AgentSpawner,
        Agent2Spawner
    }

    [Header("Settings")]
    public Color hitColor = Color.red; // Color al recibir disparo, configurable en el inspector
    public AudioClip hitSound; // Sonido al recibir disparo, configurable en el inspector
    public float colorChangeDuration = 0.5f; // Duración del cambio de color en segundos
    public SpawnerType spawnerType; // Selecciona el tipo de spawner en el inspector
    public AgentSpawner agentSpawner; // Referencia al script de AgentSpawner
    public Agent2Spawner agent2Spawner; // Referencia al script de Agent2Spawner
    public float spawnCooldown = 0.5f; // Tiempo de espera entre spawns

    private Renderer buttonRenderer;
    private Color originalColor;
    private AudioSource audioSource;
    private bool isCooldownActive = false; // Controla si está en cooldown

    private void Start()
    {
        buttonRenderer = GetComponent<Renderer>();
        originalColor = buttonRenderer.material.color;

        // Agrega un AudioSource al objeto si no lo tiene
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Projectile") && !isCooldownActive) // Detecta si fue golpeado por un proyectil y si no está en cooldown
        {
            ChangeColor();
            PlaySound();

            // Activa el cooldown
            StartCoroutine(StartSpawnCooldown());

            // Notifica al spawner seleccionado para que intente spawnear un agente
            SpawnAgent();

            // Destruye el proyectil para evitar múltiples colisiones
            Destroy(other.gameObject);
        }
    }

    private void ChangeColor()
    {
        buttonRenderer.material.color = hitColor; // Cambia al color seleccionado
        Invoke("ResetColor", colorChangeDuration); // Vuelve al color original después de un tiempo
    }

    private void ResetColor()
    {
        buttonRenderer.material.color = originalColor; // Restaura el color original
    }

    private void PlaySound()
    {
        if (hitSound != null)
        {
            audioSource.PlayOneShot(hitSound); // Reproduce el sonido al recibir disparo
        }
    }

    private IEnumerator StartSpawnCooldown()
    {
        isCooldownActive = true; // Activa el cooldown
        yield return new WaitForSeconds(spawnCooldown); // Espera el tiempo definido
        isCooldownActive = false; // Desactiva el cooldown
    }

    private void SpawnAgent()
    {
        // Usa el spawner seleccionado en el inspector
        switch (spawnerType)
        {
            case SpawnerType.AgentSpawner:
                if (agentSpawner != null)
                {
                    agentSpawner.SpawnAgentIfNeeded();
                }
                break;

            case SpawnerType.Agent2Spawner:
                if (agent2Spawner != null)
                {
                    agent2Spawner.SpawnAgent2IfNeeded();
                }
                break;
        }
    }
}

using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class ShootableButton : MonoBehaviour
{
    [Header("Settings")]
    public Color hitColor = Color.red;
    public AudioClip hitSound;
    public float colorChangeDuration = 0.5f;
    public UniversalAgentSpawner spawner; // Referencia al nuevo UniversalAgentSpawner
    public UniversalAgentSpawner.EnemyType enemyType; // Tipo de enemigo a spawnear
    public float spawnCooldown = 0.5f;

    private Renderer buttonRenderer;
    private Color originalColor;
    private AudioSource audioSource;
    private bool isCooldownActive = false;

    private void Start()
    {
        buttonRenderer = GetComponent<Renderer>();
        originalColor = buttonRenderer.material.color;

        // Verificamos y asignamos el AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Projectile") && !isCooldownActive)
        {
            ChangeColor();
            PlaySound();
            StartCoroutine(StartSpawnCooldown());

            // Establecemos el tipo de enemigo y lo spawneamos
            spawner.enemyType = enemyType;
            spawner.SpawnAgent();

            Destroy(other.gameObject);
        }
    }

    private void ChangeColor()
    {
        buttonRenderer.material.color = hitColor;
        Invoke("ResetColor", colorChangeDuration);
    }

    private void ResetColor()
    {
        buttonRenderer.material.color = originalColor;
    }

    private void PlaySound()
    {
        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }
        else if (hitSound == null)
        {
            Debug.LogWarning("El AudioClip hitSound no está asignado en " + gameObject.name);
        }
        else if (audioSource == null)
        {
            Debug.LogError("AudioSource no está asignado en " + gameObject.name);
        }
    }

    private IEnumerator StartSpawnCooldown()
    {
        isCooldownActive = true;
        yield return new WaitForSeconds(spawnCooldown);
        isCooldownActive = false;
    }
}

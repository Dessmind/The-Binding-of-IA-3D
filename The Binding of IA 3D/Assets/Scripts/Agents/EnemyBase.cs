using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

public class EnemyBase : MonoBehaviour
{
    [Header("Enemy Settings")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Health Bar")]
    public Image healthBarFill;
    public Transform healthBarCanvas;
    public Camera mainCamera;

    [Header("Damage Flash")]
    public Color damageColor = Color.red;
    public float flashDuration = 0.1f;

    [Header("Damage to Player")]
    public float contactDamage = 10f;

    [Header("Sounds")]
    public AudioClip deathSound;
    public AudioClip damageSound;

    private List<Renderer> enemyRenderers = new List<Renderer>();
    private Dictionary<Renderer, Color> originalColors = new Dictionary<Renderer, Color>();
    private AudioSource audioSource;
    private bool isInvulnerable = false;
    private float invulnerableDuration = 0.1f;

    protected virtual void Start()
    {
        currentHealth = maxHealth;

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (renderer.gameObject != healthBarCanvas.gameObject)
            {
                enemyRenderers.Add(renderer);
                originalColors[renderer] = renderer.material.color;
            }
        }

        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
    }

    void LateUpdate()
    {
        if (healthBarCanvas != null)
        {
            healthBarCanvas.position = transform.position + Vector3.up * 2.5f;
            healthBarCanvas.LookAt(healthBarCanvas.position + mainCamera.transform.rotation * Vector3.forward,
                                   mainCamera.transform.rotation * Vector3.up);
        }
    }

    public void TakeDamage(float damage)
    {
        if (isInvulnerable) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = currentHealth / maxHealth;
        }

        if (damageSound != null)
        {
            audioSource.PlayOneShot(damageSound);
        }

        StartCoroutine(DamageFlash());
        StartCoroutine(TemporarilyInvulnerable());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    IEnumerator DamageFlash()
    {
        foreach (Renderer renderer in enemyRenderers)
        {
            renderer.material.color = damageColor;
        }

        yield return new WaitForSeconds(flashDuration);

        foreach (Renderer renderer in enemyRenderers)
        {
            renderer.material.color = originalColors[renderer];
        }
    }

    void Die()
    {
        if (deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        if (healthBarCanvas != null)
        {
            healthBarCanvas.gameObject.SetActive(false);
        }

        var navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent != null)
        {
            navMeshAgent.enabled = false;
        }

        Destroy(gameObject, deathSound.length);
    }

    private IEnumerator TemporarilyInvulnerable()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(invulnerableDuration);
        isInvulnerable = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerController>()?.TakeDamage(contactDamage);
        }
        else if (other.CompareTag("Projectile"))
        {
            Projectile projectile = other.GetComponent<Projectile>();
            if (projectile != null)
            {
                TakeDamage(projectile.damage);
                Destroy(other.gameObject);
            }
        }
        else if (other.CompareTag("EnemyVision"))
        {
            Debug.Log("Collided with EnemyVision - Taking damage");
            TakeDamage(contactDamage);
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.AI;

public class EscapistEnemyBase : MonoBehaviour
{
    [Header("Enemy Settings")]
    public float maxHealth = 60f;
    private float currentHealth;

    [Header("Health Bar")]
    public Image healthBarFill;
    public Transform healthBarCanvas;
    public Camera mainCamera;

    [Header("Damage Flash")]
    public Color damageColor = Color.red;
    public float flashDuration = 0.15f;

    [Header("Sounds")]
    public AudioClip deathSound;
    public AudioClip damageSound;

    [Header("Cansancio Indicator")]
    public GameObject tiredCanvas; // Interrogación para mostrar cansancio
    public float tiredDisplayTime = 2f;
    private Coroutine tiredCoroutine;

    private NavMeshAgent navMeshAgent;
    private AudioSource audioSource;
    private bool isInvulnerable = false;
    private float invulnerableDuration = 0.1f;

    private Renderer[] renderers;
    private Color[] originalColors;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        navMeshAgent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (tiredCanvas != null)
        {
            tiredCanvas.SetActive(false);
        }

        renderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            originalColors[i] = renderers[i].material.color;
        }
    }

    public void ShowTiredMark()
    {
        if (tiredCanvas != null)
        {
            tiredCanvas.SetActive(true);
            if (tiredCoroutine != null)
            {
                StopCoroutine(tiredCoroutine);
            }
            tiredCoroutine = StartCoroutine(HideTiredMarkAfterDelay());
        }
    }

    private IEnumerator HideTiredMarkAfterDelay()
    {
        yield return new WaitForSeconds(tiredDisplayTime);
        HideTiredMark();
    }

    public void HideTiredMark()
    {
        if (tiredCanvas != null)
        {
            tiredCanvas.SetActive(false);
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
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material.color = damageColor;
        }

        yield return new WaitForSeconds(flashDuration);

        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material.color = originalColors[i];
        }
    }

    void Die()
    {
        if (deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        var navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent != null)
        {
            navMeshAgent.enabled = false;
        }

        gameObject.SetActive(false);
        Destroy(gameObject, deathSound.length);
    }

    private IEnumerator TemporarilyInvulnerable()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(invulnerableDuration);
        isInvulnerable = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Projectile"))
        {
            Projectile projectile = other.GetComponent<Projectile>();
            if (projectile != null)
            {
                TakeDamage(projectile.damage);
                Destroy(other.gameObject);
            }
        }
    }
}

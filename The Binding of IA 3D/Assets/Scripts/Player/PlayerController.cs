using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public float runSpeedMultiplier = 1.5f;
    public float jumpForce = 5f;
    public float gravity = 9.8f;

    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;
    public float shootRate = 1f;

    public float coyoteTime = 0.2f;
    private float coyoteTimeCounter;

    private CharacterController characterController;
    private Vector3 moveDirection = Vector3.zero;
    private bool isGrounded;

    private float originalSpeed;
    private float lastShootTime;

    [Header("Audio Settings")]
    public AudioClip footstepSound;
    public AudioClip runFootstepSound;
    public AudioClip jumpSound;
    public AudioClip shootSound;
    public AudioClip deathSound; // Sonido de muerte
    public AudioClip damageSound; // Sonido de daño

    [Range(0f, 1f)] public float footstepVolume = 0.5f;
    [Range(0f, 1f)] public float runFootstepVolume = 0.5f;
    [Range(0f, 1f)] public float jumpVolume = 0.5f;
    [Range(0f, 1f)] public float shootVolume = 0.5f;
    [Range(0f, 1f)] public float deathVolume = 1f;
    [Range(0f, 1f)] public float damageVolume = 0.5f; // Control de volumen para el sonido de daño

    private AudioSource audioSource;

    private float footstepInterval = 0.5f;
    private float footstepTimer;

    [Header("Player Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;
    private bool isInvulnerable = false;

    [Header("Invulnerability Settings")]
    public float invulnerabilityDuration = 1.5f;
    public float blinkInterval = 0.1f;

    [Header("Death Settings")]
    public ParticleSystem deathParticles; // Sistema de partículas al morir
    public float destroyDelay = 3f;

    [Header("Energy Settings")]
    public float maxEnergy = 100f;
    public float sprintEnergyCost = 10f;
    public float rechargeDelay = 1.5f;
    private float currentEnergy;
    private bool isRecharging = false;
    private float lastSprintEndTime;

    [Header("UI Elements")]
    public Image healthBarFill;
    public Image energyBarFill;

    private Renderer[] renderers;
    private bool isDead = false;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
        originalSpeed = speed;
        currentHealth = maxHealth;
        currentEnergy = maxEnergy;

        UpdateHealthBar();
        UpdateEnergyBar();

        // Obtiene todos los renderers del jugador y sus hijos
        renderers = GetComponentsInChildren<Renderer>();
    }

    void Update()
    {
        if (isDead) return;

        Vector3 movementInput = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) movementInput.z = 1;
        else if (Input.GetKey(KeyCode.S)) movementInput.z = -1;

        if (Input.GetKey(KeyCode.D)) movementInput.x = 1;
        else if (Input.GetKey(KeyCode.A)) movementInput.x = -1;

        if (movementInput.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(Camera.main.transform.forward);
            targetRotation.x = 0;
            targetRotation.z = 0;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        // Sprint si tiene suficiente energía y no está recargando
        if (Input.GetKey(KeyCode.LeftShift) && currentEnergy > 0)
        {
            Sprint();
            lastSprintEndTime = Time.time;
        }
        else
        {
            speed = originalSpeed;
            footstepInterval = 0.5f;

            // Si el jugador no está sprintando, verifica si es momento de comenzar a recargar
            if (Time.time >= lastSprintEndTime + rechargeDelay && currentEnergy < maxEnergy && !isRecharging)
            {
                StartCoroutine(RechargeEnergy());
            }
        }

        Move(movementInput);

        if (isGrounded) coyoteTimeCounter = coyoteTime;
        else coyoteTimeCounter -= Time.deltaTime;

        if (Input.GetKey(KeyCode.Space) && coyoteTimeCounter > 0f) Jump();

        Disparar();
        AplicarGravedad();
        PlayFootstepSound(movementInput);
    }

    void Sprint()
    {
        speed = originalSpeed * runSpeedMultiplier;
        footstepInterval = 0.3f;

        currentEnergy -= sprintEnergyCost * Time.deltaTime;
        currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);

        UpdateEnergyBar();

        if (currentEnergy <= 0)
        {
            StopCoroutine(RechargeEnergy());
            isRecharging = false;
        }
    }

    IEnumerator RechargeEnergy()
    {
        isRecharging = true;

        while (currentEnergy < maxEnergy)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                isRecharging = false;
                yield break;
            }

            currentEnergy += maxEnergy * Time.deltaTime;
            UpdateEnergyBar();
            yield return null;
        }

        currentEnergy = maxEnergy;
        UpdateEnergyBar();
        isRecharging = false;
    }

    void Move(Vector3 direction)
    {
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDirectionXZ = (camForward * direction.z + camRight * direction.x).normalized * speed;
        moveDirection.x = moveDirectionXZ.x;
        moveDirection.z = moveDirectionXZ.z;

        characterController.Move(moveDirection * Time.deltaTime);
        isGrounded = characterController.isGrounded;
    }

    void Jump()
    {
        if (isGrounded || coyoteTimeCounter > 0f)
        {
            moveDirection.y = jumpForce;
            coyoteTimeCounter = 0f;
            audioSource.PlayOneShot(jumpSound, jumpVolume);
        }
    }

    void AplicarGravedad()
    {
        if (!isGrounded) moveDirection.y -= gravity * Time.deltaTime;
    }

    void Disparar()
    {
        if (Input.GetMouseButtonDown(0) && Time.time >= lastShootTime + 1f / shootRate)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 targetPoint = Physics.Raycast(ray, out RaycastHit hit) ? hit.point : ray.GetPoint(100);
            Vector3 shootDirection = (targetPoint - transform.position).normalized;

            GameObject projectile = Instantiate(projectilePrefab, transform.position + shootDirection, Quaternion.LookRotation(shootDirection));
            Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();
            projectileRb.velocity = shootDirection * projectileSpeed;

            audioSource.PlayOneShot(shootSound, shootVolume);
            lastShootTime = Time.time;
            Destroy(projectile, 1.5f);
        }
    }

    public void ApplyJumpForce(float additionalJumpForce)
    {
        moveDirection.y = additionalJumpForce; // Aplica la fuerza de salto adicional en el eje Y
    }


    void PlayFootstepSound(Vector3 movementInput)
    {
        if (isGrounded && movementInput.magnitude > 0.1f)
        {
            footstepTimer -= Time.deltaTime;

            if (footstepTimer <= 0f)
            {
                AudioClip footstepClip = (speed > originalSpeed) ? runFootstepSound : footstepSound;
                float volume = (speed > originalSpeed) ? runFootstepVolume : footstepVolume;
                audioSource.PlayOneShot(footstepClip, volume);
                footstepTimer = footstepInterval;
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (isInvulnerable || isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Reproduce el sonido de daño
        if (damageSound != null)
        {
            audioSource.PlayOneShot(damageSound, damageVolume);
        }

        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvulnerabilityCoroutine());
        }
    }

    IEnumerator InvulnerabilityCoroutine()
    {
        isInvulnerable = true;
        float elapsed = 0f;

        while (elapsed < invulnerabilityDuration)
        {
            foreach (Renderer renderer in renderers)
            {
                renderer.enabled = !renderer.enabled; // Alterna visibilidad para efecto de parpadeo
            }
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = true; // Asegúrate de que todos los renderers estén visibles al final
        }

        isInvulnerable = false;
    }

    void UpdateHealthBar()
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = currentHealth / maxHealth;
        }
    }

    void UpdateEnergyBar()
    {
        if (energyBarFill != null)
        {
            energyBarFill.fillAmount = currentEnergy / maxEnergy;
        }
    }

    void Die()
    {
        Debug.Log("Player has died");

        isDead = true;

        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = false;
        }

        if (deathSound != null)
        {
            audioSource.PlayOneShot(deathSound, deathVolume);
        }

        float particleHeightOffset = 0.5f;
        Vector3 particlePosition = transform.position + new Vector3(0, particleHeightOffset, 0);

        if (deathParticles != null)
        {
            ParticleSystem particles = Instantiate(deathParticles, particlePosition, Quaternion.identity);
            particles.Play();
        }

        Destroy(gameObject, destroyDelay);
    }
}

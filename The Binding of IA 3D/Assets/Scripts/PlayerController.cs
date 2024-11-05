// PlayerController.cs
using UnityEngine;

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
    private float lastShootTime; // Temporizador para la tasa de disparo

    [Header("Audio Settings")]
    public AudioClip footstepSound;
    public AudioClip runFootstepSound;
    public AudioClip jumpSound;
    public AudioClip shootSound;

    [Range(0f, 1f)] public float footstepVolume = 0.5f;
    [Range(0f, 1f)] public float runFootstepVolume = 0.5f;
    [Range(0f, 1f)] public float jumpVolume = 0.5f;
    [Range(0f, 1f)] public float shootVolume = 0.5f;

    private AudioSource audioSource;

    private float footstepInterval = 0.5f; // Intervalo de tiempo entre pasos
    private float footstepTimer;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>(); // Asegúrate de tener un AudioSource en el Player
        originalSpeed = speed;
    }

    void Update()
    {
        Vector3 movementInput = Vector3.zero;

        // Movimiento con WASD
        if (Input.GetKey(KeyCode.W))
        {
            movementInput.z = 1;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            movementInput.z = -1;
        }

        if (Input.GetKey(KeyCode.D))
        {
            movementInput.x = 1;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            movementInput.x = -1;
        }

        // Correr al mantener presionado Shift
        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed = originalSpeed * runSpeedMultiplier;
            footstepInterval = 0.3f; // Intervalo más rápido al correr
        }
        else
        {
            speed = originalSpeed;
            footstepInterval = 0.5f; // Intervalo normal al caminar
        }

        Move(movementInput);

        // Coyote time para salto
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.Space) && coyoteTimeCounter > 0f)
        {
            Jump();
        }

        Disparar();
        AplicarGravedad();
        PlayFootstepSound(movementInput);
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

            // Reproducir sonido de salto
            audioSource.PlayOneShot(jumpSound, jumpVolume);
        }
    }

    void AplicarGravedad()
    {
        if (!isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }
    }

    void Disparar()
    {
        // Limitar el disparo de acuerdo con el tiempo establecido en shootRate
        if (Input.GetMouseButtonDown(0) && Time.time >= lastShootTime + 1f / shootRate)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            Vector3 targetPoint;
            if (Physics.Raycast(ray, out hit))
            {
                targetPoint = hit.point;
            }
            else
            {
                targetPoint = ray.GetPoint(100);
            }

            Vector3 shootDirection = (targetPoint - transform.position).normalized;

            GameObject projectile = Instantiate(projectilePrefab, transform.position + shootDirection, Quaternion.LookRotation(shootDirection));
            Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();
            projectileRb.velocity = shootDirection * projectileSpeed;

            // Reproducir sonido de disparo
            audioSource.PlayOneShot(shootSound, shootVolume);

            // Actualizar el tiempo del último disparo
            lastShootTime = Time.time;

            // Destruir el proyectil después de 3 segundos
            Destroy(projectile, 1.5f);
        }
    }


    void PlayFootstepSound(Vector3 movementInput)
    {
        if (isGrounded && movementInput.magnitude > 0.1f)
        {
            footstepTimer -= Time.deltaTime;

            if (footstepTimer <= 0f)
            {
                // Determinar el sonido de pasos según la velocidad
                AudioClip footstepClip = (speed > originalSpeed) ? runFootstepSound : footstepSound;
                float volume = (speed > originalSpeed) ? runFootstepVolume : footstepVolume;
                audioSource.PlayOneShot(footstepClip, volume);

                footstepTimer = footstepInterval; // Reiniciar el temporizador
            }
        }
    }
}

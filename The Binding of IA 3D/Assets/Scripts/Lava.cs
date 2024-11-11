using UnityEngine;

public class Lava : MonoBehaviour
{
    [Header("Lava Settings")]
    public float damageAmount = 100f; // Cantidad de daño que causará la lava al jugador

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damageAmount); // Llama al método de daño en el jugador
            }
        }
    }
}

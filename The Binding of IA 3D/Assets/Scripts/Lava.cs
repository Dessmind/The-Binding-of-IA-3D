using UnityEngine;

public class Lava : MonoBehaviour
{
    [Header("Lava Settings")]
    public float damageAmount = 100f; // Cantidad de da�o que causar� la lava al jugador

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damageAmount); // Llama al m�todo de da�o en el jugador
            }
        }
    }
}

using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float damage = 20f;
    public float lifetime = 3f;

    void Start()
    {
        Destroy(gameObject, lifetime); // Destruir el proyectil despu�s de cierto tiempo
    }

    private bool hasDealtDamage = false;

    void OnTriggerEnter(Collider other)
    {
        if (hasDealtDamage) return; // Si ya ha causado da�o, no lo hace de nuevo

        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
                hasDealtDamage = true;
                Destroy(gameObject); // Destruye el proyectil despu�s de aplicar el da�o
            }
        }
        else if (other.CompareTag("Enemy"))
        {
            EnemyBase enemy = other.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                hasDealtDamage = true;
                Destroy(gameObject); // Destruye el proyectil despu�s de aplicar el da�o
            }
        }
    }

}

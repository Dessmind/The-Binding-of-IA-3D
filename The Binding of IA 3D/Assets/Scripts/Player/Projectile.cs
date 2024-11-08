using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float damage = 20f;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") || other.CompareTag("EnemyVision"))
        {
            EnemyBase enemy = other.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log("Projectile hit enemy, dealing damage: " + damage);
                Destroy(gameObject);
            }
        }
    }
}

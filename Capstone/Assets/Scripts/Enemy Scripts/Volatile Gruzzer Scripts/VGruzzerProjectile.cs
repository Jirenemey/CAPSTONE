using UnityEngine;

public class VGruzzerProjectile : MonoBehaviour
{
    [SerializeField] float damage = 20f;
    [SerializeField] float lifetime = 3f; // destroy if it never hits a wall

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Hit wall — destroy projectile
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject, 1);
            return;
        }

        // Hit enemy — deal damage but keep going
        if (other.TryGetComponent<IDamageable>(out var target) && other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            target.TakeDamage(damage);
        }
    }
}

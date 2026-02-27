using UnityEngine;

public class VGruzzerProjectile : MonoBehaviour
{
    [SerializeField] float speed = 6f;
    [SerializeField] float damage = 20f;
    [SerializeField] float lifetime = 3f; // destroy if it never hits a wall

    void Update()
    {
        transform.Translate(Vector2.down * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Hit wall — destroy projectile
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
            return;
        }

        // Hit enemy — deal damage but keep going
        if (other.TryGetComponent<IDamageable>(out var target))
        {
            target.TakeDamage(damage);
        }
    }
}

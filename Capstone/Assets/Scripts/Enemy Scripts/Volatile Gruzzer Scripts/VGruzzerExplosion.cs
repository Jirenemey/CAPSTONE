using UnityEngine;

public class VGruzzerExplosion : MonoBehaviour
{
    [SerializeField] private float radius = 2f;
    [SerializeField] private float damage = 25f;
    [SerializeField] private LayerMask hitLayers;
    [SerializeField] private float lifeTime = 1f;

    private void Start()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, hitLayers);

        foreach (Collider2D hit in hits)
        {
            IDamageable damageable = hit.GetComponent<IDamageable>();

            if (damageable != null && hit.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                damageable.TakeDamage(damage);
            }
        }

        Destroy(gameObject, lifeTime);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}

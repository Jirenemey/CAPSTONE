using Unity.VisualScripting;
using UnityEngine;

public class SturdyFoolProjectile : MonoBehaviour, IDamageable
{
    Rigidbody2D rb;
    Collider2D col;
    Animator anim;
    SpriteRenderer spriteRenderer;

    [SerializeField] float damage = 20f;
    [SerializeField] float lifetime = 5f; // destroy if it never hits a wall

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!col) col = GetComponent<Collider2D>();
        if (!anim) anim = GetComponent<Animator>();
        if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual void TakeDamage(float damage)
    {
        
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
        if (other.TryGetComponent<IDamageable>(out var target) && other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            target.TakeDamage(damage);
        }
    }

}

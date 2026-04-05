using UnityEngine;

public class VGruzzerProjectile : MonoBehaviour
{
    [SerializeField] float damage = 20f;
    [SerializeField] float lifetime = 3f; // destroy if it never hits a wall

    Rigidbody2D rb;
    Collider2D col;
    Animator anim;
    SpriteRenderer spriteRenderer;

    //public AudioManager audioManager;

    private void Start()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!col) col = GetComponent<Collider2D>();
        if (!anim) anim = GetComponent<Animator>();
        if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();
        //if (!audioManager) audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Hit wall — destroy projectile
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            anim.SetTrigger("Hit");
            AudioManager.instance.PlaySFX("VG Projectile Break");
            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero;
            col.enabled = false;
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

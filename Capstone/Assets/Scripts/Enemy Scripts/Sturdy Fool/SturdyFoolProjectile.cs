using Unity.VisualScripting;
using UnityEngine;

public class SturdyFoolProjectile : MonoBehaviour
{
    Rigidbody2D rb;
    Collider2D col;
    Animator anim;
    SpriteRenderer spriteRenderer;
    private AudioSource loopSource;

    [Header("Settings")]
    [SerializeField] private float arcHeight = 2f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float lifetime = 5f; // destroy if it never hits a wall
    [SerializeField] private LayerMask targetLayers;
    [SerializeField] private LayerMask Ground;

    private bool launched = false;
    private bool isDead = false;

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!col) col = GetComponent<Collider2D>();
        if (!anim) anim = GetComponent<Animator>();
        if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        Destroy(gameObject, lifetime);
        
    }

    void Update()
    {
        
    }

    public void LaunchArc(Vector2 target)
    {
        launched = true;

        Vector2 start = transform.position;

        float gravity = Mathf.Abs(Physics2D.gravity.y * rb.gravityScale);

        float height = arcHeight;

        // Time to reach peak
        float timeUp = Mathf.Sqrt(2 * height / gravity);

        // Time to fall from peak to target
        float timeDown = Mathf.Sqrt(2 * Mathf.Max(0.01f, (height - (target.y - start.y))) / gravity);

        float totalTime = timeUp + timeDown;

        float vx = (target.x - start.x) / totalTime;
        float vy = gravity * timeUp;

        // Apply velocity (note: Unity gravity is negative)
        rb.linearVelocity = new Vector2(vx, vy);

        if (loopSource == null)
        {
            loopSource = AudioManager.instance.PlayLoopSFXAtObject("SF Projectile", transform);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Hit player
        if ((targetLayers.value & (1 << other.gameObject.layer)) != 0)
        {
            //player doesn't have IDamageable at the moment
            if (other.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(damage);
                Die();
                return;
            }
        }

        // Hit ground
        if ((Ground.value & (1 << other.gameObject.layer)) != 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        col.enabled = false;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        anim.SetTrigger("Break");
        if (loopSource != null)
        {
            Destroy(loopSource);
            loopSource = null;
        }
        AudioManager.instance.PlaySFX("SF Projectile Break");

        Destroy(gameObject, 0.5f);
    }
}

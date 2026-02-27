using UnityEngine;

public class VolatileGruzzerAI : MonoBehaviour
{
    Rigidbody2D rb;
    Collider2D col;
    Animator anim;
    SpriteRenderer spriteRenderer;

    [SerializeField] GameObject projectilePrefab;

    public float health = 100;

    public float moveSpeed = 3.0f;
    public float projectileSpawnRate = 3;
    public float explosionTimer = 10;

    private Vector2 direction;
    private float projectileTimer = 0;


    void Start()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!col) col = GetComponent<Collider2D>();
        if (!anim) anim = GetComponent<Animator>();
        if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();

        float x = Random.value > 0.5f ? 1 : -1;
        float y = Random.value > 0.5f ? 1 : -1;
        if (x > 0) spriteRenderer.flipX = true;
        // start at perfect 45 degrees
        direction = new Vector2(x, y).normalized;

        rb.linearVelocity = direction * moveSpeed;
    }

    private void FixedUpdate()
    {
        if (health <= 0)
        {
            Die();
        }

        if (projectileTimer >= projectileSpawnRate)
        {
            if (projectilePrefab == null) return;

            GameObject proj = Instantiate(
            projectilePrefab,
            transform.position,
            Quaternion.identity
            );

            projectileTimer = 0;
        }

        projectileTimer += Time.deltaTime;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        direction = Vector2.Reflect(direction, collision.GetContact(0).normal);

        if (direction.x < 0)
        {
            spriteRenderer.flipX = false;
        }
        else
        {
            spriteRenderer.flipX = true;
        }

        rb.linearVelocity = direction * moveSpeed;
    }

    private void Die()
    {
         
    }

}

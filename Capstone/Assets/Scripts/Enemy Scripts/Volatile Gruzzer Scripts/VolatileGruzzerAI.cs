using UnityEngine;

public class VolatileGruzzerAI : MonoBehaviour, IDamageable
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

    public float groundCheckRadius = 1f;
    private Transform groundCheck;
    private LayerMask groundLayer;

    private bool isDead = false;

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

        if (!groundCheck) groundCheck = transform.Find("GroundCheck");
        groundLayer = LayerMask.GetMask("Ground");
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.O))
        //{
            if (IsGrounded())
            {
                Debug.Log("is touching the ground");
            }
            else
            {
                Debug.Log("is OFF the ground");
            }

        //}
    }
    private void FixedUpdate()
    {
        //stop anything after this line if dead
        if (isDead) return;

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

        if (Input.GetKeyDown(KeyCode.P))
        {
            Die();
        }

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

        //if (collision.gameObject.layer == LayerMask.NameToLayer("Ground")) { }
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        health -= amount;
        Debug.Log($"{gameObject.name} took {amount} damage. HP: {health}");

        if (health <= 0) Die();
    }

    private void Die()
    {
        isDead = true;

        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 1;


        Destroy(gameObject, 10);
    }

}

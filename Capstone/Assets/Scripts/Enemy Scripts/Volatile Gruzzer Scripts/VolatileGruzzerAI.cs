using UnityEngine;
using System.Collections;

public class VolatileGruzzerAI : MonoBehaviour, IDamageable
{
	public event System.Action OnDeath;
    Rigidbody2D rb;
	Collider2D col;
    Animator anim;
    SpriteRenderer spriteRenderer;

    AudioManager audio;

    [SerializeField] GameObject projectilePrefab;
    [SerializeField] GameObject explosionPrefab;

    [SerializeField] private float health = 100;
    [SerializeField] private float moveSpeed = 3.0f;
    [SerializeField] private float projectileSpawnRate = 3f;
    [SerializeField] private float explosionTimer = 5f;
    [SerializeField] private float groundCheckRadius = 0.2f;

    private Vector2 direction;
    private float projectileTimer = 0;

    private Transform groundCheck;
    private LayerMask groundLayer;

    private bool isDead = false;
    private bool explosionCountdownStarted = false;

    [SerializeField] private Transform player;
    [SerializeField] private float knockbackForce = 5f;
    private float knockbackTime = 0.2f;
    private bool isKnockedBack = false;

    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float flashDuration = 0.1f;

    void Start()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!col) col = GetComponent<Collider2D>();
        if (!anim) anim = GetComponent<Animator>();
        if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning("Player not found! Make sure Player has the Player tag.");

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
        if (Input.GetKeyDown(KeyCode.P))
        {
            TakeDamage(25f);
        }

        if (!isDead) return;

        if (!explosionCountdownStarted && IsGrounded())
        {
            anim.SetBool("isGrounded", true);
            explosionCountdownStarted = true;
            StartCoroutine(ExplosionCountdown());
            //audio.PlaySFX("");
        }
    }
    private void FixedUpdate()
    {
        //stop anything after this line if dead
        if (isDead) return;

        if (projectileTimer >= projectileSpawnRate)
        {
            if (projectilePrefab != null)
            {
                GameObject proj = Instantiate(
                projectilePrefab,
                transform.position,
                Quaternion.identity
                );
            }

            projectileTimer = 0;
        }

        projectileTimer += Time.deltaTime;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead || isKnockedBack) return;

        Vector2 normal = collision.GetContact(0).normal;

        // Flip based on surface hit
        if (Mathf.Abs(normal.x) > 0.5f)
        {
            direction.x *= -1;
        }

        if (Mathf.Abs(normal.y) > 0.5f)
        {
            direction.y *= -1;
        }

        // Re-normalize to keep perfect 45°
        direction = direction.normalized;

        // Flip sprite
        spriteRenderer.flipX = direction.x > 0;

        rb.linearVelocity = direction * moveSpeed;
    }

    private bool IsGrounded()
    {
        if (groundCheck == null) return false;

        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        health -= amount;
        Debug.Log($"{gameObject.name} took {amount} damage. HP: {health}");

        StartCoroutine(HitFlash());
        ApplyKnockback();

        if (health <= 0) Die();
    }

    private void ApplyKnockback()
    {
        if (player == null) return;

        Vector2 dir = (transform.position - player.position).normalized;

        Vector2 knockbackDir;

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            knockbackDir = dir.x > 0 ? Vector2.right : Vector2.left;
        }
        else
        {
            knockbackDir = dir.y > 0 ? Vector2.up : Vector2.down;
        }

        StartCoroutine(KnockbackRoutine(knockbackDir));
    }

    private IEnumerator KnockbackRoutine(Vector2 dir)
    {
        isKnockedBack = true;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackTime);

        isKnockedBack = false;
        //rb.linearVelocity = Vector2.zero;

        rb.linearVelocity = direction * moveSpeed;
    }

    private IEnumerator HitFlash()
    {
        Color originalColor = Color.white;
        spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;

        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 1;

        anim.SetTrigger("Died");

		OnDeath?.Invoke();

		//Destroy(gameObject, 10);
	}

    private IEnumerator ExplosionCountdown()
    {
        yield return new WaitForSeconds(explosionTimer);

        Explode();
    }

    private void Explode()
    {
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Transform gc = transform.Find("GroundCheck");
        if (gc == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(gc.position, groundCheckRadius);
    }
}

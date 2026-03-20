using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]

public class EnemyBase : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    public float maxHealth = 100f;
    public float sightRadius = 10;

    protected float currentHealth;
    protected bool isDead = false;

    [Header("Damage")]
    public float collisionDMG = 1f;

    protected Transform player;

    protected Rigidbody2D rb;
    protected Collider2D col;
    protected Animator anim;
    protected SpriteRenderer spriteRenderer;

    public DetectionComponent detection {  get; private set; }
    public MovementComponent movement {  get; private set; }

    protected EnemyStateMachine fsm;

    public EnemyState DefaultState {  get; protected set; }

    protected virtual void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!col) col = GetComponent<Collider2D>();
        if (!anim) anim = GetComponent<Animator>();
        if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();

        if (!detection) detection = GetComponent<DetectionComponent>();
        if (!movement) movement = GetComponent<MovementComponent>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning("Player not found! Make sure Player has the Player tag.");

        currentHealth = maxHealth;

        fsm = new EnemyStateMachine();
    }

    protected virtual void Update()
    {
        if (isDead) return;
        
        if (currentHealth <= 0)
        {
            Die();
        }

        fsm.UpdateState();
    }

    protected virtual void FixedUpdate()
    {
        if (isDead) return;
    }

    public virtual void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (currentHealth < 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        isDead = true;
        Destroy(gameObject, 2);
    }
}
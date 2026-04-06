using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.EventSystems.EventTrigger;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]

public abstract class EnemyBase : NetworkBehaviour, IDamageable
{
	public event System.Action OnDeath;
	[Header("Stats")]
    public float maxHealth = 100f;

    protected float currentHealth;
    protected bool isDead = false;

    [Header("Damage")]
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float collisionDMG = 1f;

    private float knockbackTime = 0.2f;
    private bool isKnockedBack = false;

    [Header("Others")]
    [SerializeField] private int spriteFacing = -1;
    // 1 = sprite faces RIGHT by default
    // -1 = sprite faces LEFT by default

    private Color originalColor;
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float flashDuration = 0.1f;

    //[SerializeField] private Material flashMaterial;

    //private Material originalMaterial;
    //private Coroutine flashRoutine;

    protected Rigidbody2D rb;
    protected Collider2D col;
    protected Animator anim;
    protected SpriteRenderer spriteRenderer;

    protected AudioSource loopSource;

    public Rigidbody2D RB => rb;
    public Collider2D Col => col;
    public Animator Anim => anim;
    public SpriteRenderer SpriteRenderer => spriteRenderer;
    public AudioSource LoopSource => loopSource;


    public int FacingDirection { get; private set; } = 1;

    public static readonly int DiedHash = Animator.StringToHash("Died");

    public DetectionComponent detection {  get; private set; }
    public MovementComponent movement {  get; private set; }

    //protected EnemyStateMachine fsm;
    public EnemyStateMachine fsm { get; private set; }
    private Dictionary<Type, EnemyState> states = new();

    //Default functions
    protected virtual void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!col) col = GetComponent<Collider2D>();
        if (!anim) anim = GetComponent<Animator>();
        if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();
        //if (!audioManager) audioManager = GameObject.

        if (!detection) detection = GetComponent<DetectionComponent>();
        if (!movement) movement = GetComponent<MovementComponent>();

        currentHealth = maxHealth;

        fsm = new EnemyStateMachine();
    }

    protected virtual void Start()
    {
        RegisterStates();
        originalColor = spriteRenderer.color;
    }

    protected virtual void Update()
    {
        if (isDead) return;
        if (isKnockedBack) return;
        fsm.UpdateState();

        if (Input.GetKeyDown(KeyCode.P))
        {
            TakeDamage(25);
        }
    }

    protected virtual void FixedUpdate()
    {
        if (isDead) return;
        if (isKnockedBack) return;

        fsm.FixedUpdateState();
    }

    //State related functions

    // Each enemy defines its own states
    protected abstract void RegisterStates();

    // Register a state
    protected void AddState(EnemyState state)
    {
        states[state.GetType()] = state;
    }

    // Get a state safely
    public T GetState<T>() where T : EnemyState
    {
        if (states.TryGetValue(typeof(T), out var state))
            return (T)state;

        return null;
    }

    //Sprite related Functions
    public virtual void FaceDirection(float xDirection)
    {
        const float flipDeadZone = 0.5f;

        if (Mathf.Abs(xDirection) < flipDeadZone) return;

        //spriteRenderer.flipX = xDirection > 0f;

        int newDir = xDirection > 0 ? 1 : -1;

        if (newDir != FacingDirection)
        {
            if(NetworkManager.Singleton){
                if (IsServer)
                {
                    FlipClientRpc();
                }
            } else {
                Flip();
            }
        }
    }

    [ClientRpc]
    public virtual void FlipClientRpc()
    {
        FacingDirection *= -1;
        
        Vector2 colOffSet = col.offset;
        colOffSet.x *= -1;
        col.offset = colOffSet;
        spriteRenderer.flipX = (FacingDirection * spriteFacing) == -1;
    }
    [ServerRpc]
    public virtual void FlipServerRpc()
    {
        FacingDirection *= -1;
        
        Vector2 colOffSet = col.offset;
        colOffSet.x *= -1;
        col.offset = colOffSet;
        spriteRenderer.flipX = (FacingDirection * spriteFacing) == -1;
    }

    public virtual void FacePlayer()
    {
        if (detection == null || detection.player == null) return;


        FaceDirection(detection.DirectionToPlayer().x);


    }

    public virtual void Flip()
    {
        FacingDirection *= -1;
        
        Vector2 colOffSet = col.offset;
        colOffSet.x *= -1;
        col.offset = colOffSet;
        spriteRenderer.flipX = (FacingDirection * spriteFacing) == -1;
    }

    //Damage related functions
    public virtual void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        StartCoroutine(HitFlash());
        ApplyKnockback();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void ApplyKnockback()
    {
        if (detection.player == null) return;

        Vector2 dir = (transform.position - detection.player.position).normalized;

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
        rb.linearVelocity = Vector2.zero;
    }

    private IEnumerator HitFlash()
    {
        //Color originalColor = Color.white;
        spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }

    //Audio functions
    public void StartLoopSFX(string name)
    {
        if (loopSource == null)
        {
            loopSource = AudioManager.instance.PlayLoopSFXAtObject(name, transform);
        }
    }

    public void StopLoopSFX()
    {
        if (loopSource != null)
        {
            Destroy(loopSource);
            loopSource = null;
        }
    }

    private void OnCollideEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<Spikes>(out var spikes))
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        if(!NetworkManager.Singleton){
            isDead = true;
            anim.SetTrigger(EnemyBase.DiedHash);
            rb.gravityScale = 1.0f;
            Vector2 currentVel = rb.linearVelocity;
            rb.linearVelocity = new Vector2(currentVel.x, 5f);
            Destroy(gameObject, 2);
            OnDeath?.Invoke();
        } else
        {
            HandleDeathServerRpc();
        }
	}

    [ServerRpc]
    void HandleDeathServerRpc()
    {
        isDead = true;
        anim.SetTrigger(EnemyBase.DiedHash);
        rb.gravityScale = 1.0f;
        Vector2 currentVel = rb.linearVelocity;
        rb.linearVelocity = new Vector2(currentVel.x, 5f);
        Destroy(gameObject, 2);
        OnDeath?.Invoke();
    }
}
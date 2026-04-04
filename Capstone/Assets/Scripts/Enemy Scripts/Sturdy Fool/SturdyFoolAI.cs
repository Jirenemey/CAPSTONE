using UnityEngine;

public class SturdyFoolAI : EnemyBase
{
    [Header("Enemy Specific")]
    [SerializeField] private AttackHitbox slashHitbox;
    [SerializeField] private GameObject projectilePrefab;
    private Vector2 cachedThrowTarget;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private Transform ledgeCheck;
    [SerializeField] private float ledgeCheckDistance = 0.5f;
    [SerializeField] private float ledgeCheckXOffset = 1.3f;
    [SerializeField] private float ledgeCheckYOffset = -1f;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private float wallCheckHeight = 0.5f;
    [SerializeField] private float wallCheckXOffset = 0.5f;
    [SerializeField] private float wallCheckYOffset = 0.5f;
    [SerializeField] private LayerMask groundLayer;

    [SerializeField] private float minAttackCooldown = 0.5f;
    [SerializeField] private float maxAttackCooldown = 1.5f;
    private float nextAttackTime;
    [SerializeField] private float meleeRange = 2f;
    [SerializeField] private float meleeHeight = 2f;
    [SerializeField] private float rangedRange = 2f;
    [SerializeField] private float evadeSpeed = 4f;

    private bool applyforce;
    private bool animationFinished;

    public GameObject ProjectilePrefab => projectilePrefab;
    public float NextAttackTime => nextAttackTime;
    public float MeleeRange => meleeRange;
    public float MeleeHeight => meleeHeight;
    public float RangedRange => rangedRange;
    public float EvadeSpeed => evadeSpeed;
    public bool Applyforce => applyforce;
    public bool AnimationFinished => animationFinished;

    //Animation Hashes
    //trigger
    public static readonly int DiedHash = Animator.StringToHash("Died");
    public static readonly int SlashHash = Animator.StringToHash("Slash");
    public static readonly int ProjectileHash = Animator.StringToHash("Attack");
    public static readonly int EvadeHash = Animator.StringToHash("Evade");
    //bool
    public static readonly int IsMovingHash = Animator.StringToHash("isMoving");
    public static readonly int IsGroundedHash = Animator.StringToHash("isGrounded");

    protected override void Awake()
    {
        base.Awake();

        if (!groundCheck) groundCheck = transform.Find("GroundCheck");
        if (!ledgeCheck) ledgeCheck = transform.Find("LedgeCheck");
        if (!wallCheck) wallCheck = transform.Find("WallCheck");
        groundLayer = LayerMask.GetMask("Ground");
    }

    protected override void Start()
    {
        base.Start();

        UpdateSensorPosition();
        SetAttackCooldown();
    }

    protected override void RegisterStates()
    {
        AddState(new SturdyFIdleState(this, fsm));
        AddState(new SturdyFPatrolState(this, fsm));
        AddState(new SturdyFSlashState(this, fsm));
        AddState(new SturdyFThrowState(this, fsm));
        AddState(new SturdyFEvadeState(this, fsm));
        

        fsm.ChangeState(GetState<SturdyFPatrolState>());
    }

    protected override void Update()
    {
        if (isDead && IsGrounded())
        {
            anim.SetBool(IsGroundedHash, true);
        }

        base.Update();

        IsLedgeAhead();
    }

    public bool IsGrounded()
    {
        if (groundCheck == null) return false;

        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    public bool IsLedgeAhead()
    {
        if (ledgeCheck == null) return false;

        RaycastHit2D hit = Physics2D.Raycast(
            ledgeCheck.position,
            Vector2.down,
            ledgeCheckDistance,
            groundLayer
        );

        Debug.DrawRay(ledgeCheck.position, Vector2.down * ledgeCheckDistance, Color.red);

        return hit.collider != null;
    }

    public bool IsWallAhead()
    {
        if (wallCheck == null) return false;

        RaycastHit2D hit = Physics2D.Raycast(
            wallCheck.position,
            Vector2.down,
            wallCheckHeight,
            groundLayer
        );

        Debug.DrawRay(wallCheck.position, Vector2.down * wallCheckHeight, Color.red);

        return hit.collider != null;
    }

    public void SetAttackCooldown()
    {
        float cooldown = Random.Range(minAttackCooldown, maxAttackCooldown);
        nextAttackTime = Time.time + cooldown;
    }

    public override void Flip()
    {
        base.Flip();

        UpdateSensorPosition();
    }

    private void UpdateSensorPosition()
    {
        // flip the ledge and wall detections
        if (ledgeCheck != null)
        {
            ledgeCheck.localPosition = new Vector2(
                ledgeCheckXOffset * FacingDirection,
                ledgeCheckYOffset
            );
        }

        if (wallCheck != null)
        {
            wallCheck.localPosition = new Vector2(
                wallCheckXOffset * FacingDirection,
                wallCheckYOffset
            );
        }

        // flip the attack box 
        slashHitbox.transform.localPosition = new Vector2(
            Mathf.Abs(slashHitbox.transform.localPosition.x) * FacingDirection,
            slashHitbox.transform.localPosition.y
        );
    }
    public void SetThrowTarget(Vector2 targetPosition)
    {
        cachedThrowTarget = targetPosition;
    }

    public void SpawnProjectile()
    {
        Debug.LogWarning("projectile should be spawned");
        GameObject proj = Instantiate(
            projectilePrefab,
            transform.position,
            Quaternion.identity
        );

        SturdyFoolProjectile projectile = proj.GetComponent<SturdyFoolProjectile>();
        projectile.LaunchArc(cachedThrowTarget);
    }

    // animation events
    public void ApplyForce()
    {
        applyforce = true;
    }

    public void StopForce()
    {
        applyforce = false;
    }

    public void ActivateSlashHitbox()
    {
        slashHitbox.Activate();
    }

    public void DeactivateSlashHitbox()
    {
        slashHitbox.Deactivate();
    }

    public void ResetAnimationFinished()
    {
        animationFinished = false;
    }

    public void OnAnimationFinished()
    {
        animationFinished = true;
    }

    // visuals for editor
    private void OnDrawGizmosSelected()
    {
        Transform gc = transform.Find("GroundCheck");
        if (gc == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(gc.position, groundCheckRadius);

        Gizmos.color = new Color(0.5f, 0.1f, 0.2f, 1.0f);
        Gizmos.DrawWireSphere(transform.position, meleeRange);

        Gizmos.color = new Color(0.7f, 0.15f, 0.25f, 1.0f);
        Gizmos.DrawWireSphere(transform.position, rangedRange);
    }
}

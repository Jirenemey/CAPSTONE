using UnityEngine;

public class SturdyFoolAI : EnemyBase
{
    [Header("Enemy Specific")]
    [SerializeField] GameObject projectilePrefab;

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

    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float lastAttackTime = 2f;
    [SerializeField] private float meleeRange = 2f;
    [SerializeField] private float meleeHeight = 2f;
    [SerializeField] private float rangedRange = 2f;

    [SerializeField] private float evadeSpeed = 4;

    private bool applyforce;
    private bool animationFinished;

    public float AttackCooldown => attackCooldown;
    public float LastAttackTime => lastAttackTime;
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
        base.Update();

        IsGroundAhead();
    }

    public bool IsGrounded()
    {
        if (groundCheck == null) return false;

        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    public override void Flip()
    {
        base.Flip();

        UpdateSensorPosition();
    }

    public bool IsGroundAhead()
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

    private void UpdateSensorPosition()
    {
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
    }

    public void ApplyForce()
    {
        applyforce = true;
    }

    public void StopForce()
    {
        applyforce = false;
    }

    public void ResetAnimationFinished()
    {
        animationFinished = false;
    }

    public void OnAnimationFinished()
    {
        animationFinished = true;
    }

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

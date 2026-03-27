using UnityEngine;

public class SturdyFoolAI : EnemyBase
{
    [Header("Enemy Specific")]
    [SerializeField] GameObject projectilePrefab;

    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float groundCheckRadius = 0.2f;

    private bool animationFinished;

    private Transform groundCheck;
    private LayerMask groundLayer;

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
        groundLayer = LayerMask.GetMask("Ground");
    }

    protected override void RegisterStates()
    {
        AddState(new SturdyFIdleState(this, fsm));
        AddState(new SturdyFPatrolState(this, fsm));
        

        fsm.ChangeState(GetState<SturdyFPatrolState>());
    }

    public void ResetAnimationFinished()
    {
        animationFinished = false;
    }

    public void OnAnimationFinished()
    {
        animationFinished = true;
    }

    private bool IsGrounded()
    {
        if (groundCheck == null) return false;

        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void OnDrawGizmosSelected()
    {
        Transform gc = transform.Find("GroundCheck");
        if (gc == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(gc.position, groundCheckRadius);
    }
}

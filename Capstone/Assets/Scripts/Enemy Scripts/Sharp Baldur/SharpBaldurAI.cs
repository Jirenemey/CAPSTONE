using UnityEngine;

public class SharpBaldurAI : EnemyBase
{
    [Header("Enemy Specific")]
    [SerializeField] private float rollSpeed = 5f;
    [SerializeField] private float rollDuration = 4f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float bounceForce = 2f;

    private bool hitWall;
    private bool animationFinished; 

    public float RollSpeed => rollSpeed;
    public float RollDuration => rollDuration;
    public float AttackCooldown => attackCooldown;
    public float BounceForce => bounceForce;
    public bool HitWall => hitWall;
    public bool AnimationFinished => animationFinished;

    //Animation Hashes
    public static readonly int DiedHash = Animator.StringToHash("Died");
    public static readonly int StartRollHash = Animator.StringToHash("StartRoll");
    public static readonly int RollHash = Animator.StringToHash("Roll");

    protected override void RegisterStates()
    {
        AddState(new SBIdleState(this, fsm));
        AddState(new SBRollState(this, fsm));
        AddState(new SBWindUpState(this, fsm));
        AddState(new SBRecoveryState(this, fsm));

        fsm.ChangeState(GetState<SBIdleState>());
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer != LayerMask.NameToLayer("Ground"))
            return;

        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector2 normal = collision.GetContact(i).normal;

            // Mostly horizontal normal = wall
            if (Mathf.Abs(normal.x) > 0.7f)
            {
                hitWall = true;
                break;
            }
        }
    }

    public void ResetWallHit()
    {
        hitWall = false;
    }

    public void ResetAnimationFinished()
    {
        animationFinished = false;
    }

    public void OnAnimationFinished()
    {
        animationFinished = true;
    }
}

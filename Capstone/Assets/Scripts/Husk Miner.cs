using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class HuskMiner : MonoBehaviour
{
    public float health = 100f;
    public float moveSpeed = 2f;
    public float chaseRange = 8f;
    public float attackRange = 2f;
    public GameObject player;

    private Animator anim;
    private Rigidbody2D rb;
    private SpriteRenderer sprite;

    private bool isAttacking = false;
    private float AttackCooldown = 0;

    void Start()
    {
        player = GameObject.Find("Player");
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (health <= 0f)
        {
            rb.linearVelocity = Vector2.zero;
            anim.SetBool("IsDead", true);
            return;
        }

        if (AttackCooldown > 0)
        {
            AttackCooldown -= Time.deltaTime;
            rb.linearVelocity = Vector2.zero;
            rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            return;
        }

        float distance = Vector2.Distance(transform.position, player.transform.position);

        if (distance <= attackRange)
        {
            rb.linearVelocity = Vector2.zero;
            rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;

            if (!isAttacking)
            {
                isAttacking = true;
                anim.SetBool("IsAttack", true);
                AttackCooldown = 0.5f;
            }
            return;
        }
        else
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            isAttacking = false;
            anim.SetBool("IsAttack", false);
        }

        if (AttackCooldown <= 0 && distance <= chaseRange)
        {


            float direction = Mathf.Sign(player.transform.position.x - transform.position.x);
            rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

            anim.SetBool("IsMoving", true);

            if (direction > 0)
                sprite.flipX = false;
            else
                sprite.flipX = true;
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            anim.SetBool("IsMoving", false);
        }
    }

}

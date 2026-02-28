using Unity.VisualScripting;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public Transform player;
    public float moveSpeed = 3.0f;
    public float sightRadius = 10;

    public float attackRadius = 2;
    public float windupTime = 0.5f;
    public float recoveryTime = 0.5f;
    public float attackCooldown = 2.0f;

    private float lastAttackTime;

    public EnemyStateMachine StateMachine {  get; private set; }

    private void Awake()
    {
       StateMachine = new EnemyStateMachine();
    }
    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning("Player not found! Make sure Player has the Player tag.");

        StateMachine.ChangeState(new IdleState(this, StateMachine));
    }

    void Update()
    {
        StateMachine.UpdateState();
    }

    public bool CanSeePlayer()
    {
        return Vector2.Distance(transform.position, player.position) <= sightRadius;
    }

    public void MoveTowardsPlayer()
    {
        Vector2 dir = (player.position - transform.position).normalized;
        transform.position += (Vector3)dir * moveSpeed * Time.deltaTime;
    }

    public bool InAttackRange()
    {
        return Vector2.Distance(transform.position, player.position) <= attackRadius;
    }

    public bool CanAttackPlayer()
    {
        if (attackCooldown <= lastAttackTime)
        {
            lastAttackTime = 0;
            return true;
        }
        lastAttackTime += Time.deltaTime;
        return false;
    }
}

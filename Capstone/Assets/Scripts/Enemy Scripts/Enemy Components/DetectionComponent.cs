using UnityEngine;

public class DetectionComponent : MonoBehaviour
{
    public Transform player;
    public float sightRadius = 5f;
    public float exitSightRadius = 6.5f;
    public float attackRadius = 2f;

    private void Awake()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning("Player not found! Make sure Player has the Player tag.");
    }

    public void SetPlayer(Transform player)
    {
        this.player = player;
    }

    public bool PlayerEnteredSight()
    {
        return Vector2.Distance(transform.position, player.position) <= sightRadius;
    }

    public bool PlayerExitedSight()
    {
        return Vector2.Distance(transform.position, player.position) >= exitSightRadius;
    }

    public bool InAttackRange()
    {
        return Vector2.Distance(transform.position, player.position) <= attackRadius;
    }

    public Vector2 DirectionToPlayer()
    {
        return (player.position - transform.position).normalized;
    }

    public float DistanceToPlayer()
    {
        return Vector2.Distance(transform.position, player.position);
    }

    public float PlayerLeftOrRight()
    {
        if (DirectionToPlayer().x >= 0)
            return 1f;
        else return -1f;
    }

    public float PlayerRelativeHeight()
    {
        return (player.position.y - transform.position.y);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, sightRadius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, exitSightRadius);
    }
}

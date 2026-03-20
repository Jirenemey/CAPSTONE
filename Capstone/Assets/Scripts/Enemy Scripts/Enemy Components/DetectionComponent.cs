using UnityEngine;

public class DetectionComponent : MonoBehaviour
{
    public Transform player;
    public float sightRadius = 10;
    public float exitSightRadius = 10;
    public float attackRadius = 2;

    private void Awake()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning("Player not found! Make sure Player has the Player tag.");
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
}

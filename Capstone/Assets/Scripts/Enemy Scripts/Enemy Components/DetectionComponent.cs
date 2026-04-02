using Unity.Netcode;
using UnityEngine;

public class DetectionComponent : MonoBehaviour
{
    public GameObject[] playerlist;
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
        if (NetworkManager.Singleton)
            playerlist = GameObject.FindGameObjectsWithTag("Player");
    }

    public Transform FindClosestPlayer()
    {
        if (!NetworkManager.Singleton)
        {
            return player.transform;
        }

        float lowestDis = float.MaxValue;
        int lowestI = 0;
        for (int i = 0; i < playerlist.Length; i++)
        {
            float currentCheck = Vector2.Distance(playerlist[i].transform.position, transform.position);

            if (currentCheck < lowestDis)
            {
                lowestDis = currentCheck;
                lowestI = i;
            }
        }
        return playerlist[lowestI].transform;
    }

    public void SetPlayer(Transform player)
    {
        this.player = player;
    }

    public bool PlayerEnteredSight()
    {
        
        return Vector2.Distance(transform.position, FindClosestPlayer().position) <= sightRadius;
    }

    public bool PlayerExitedSight()
    {
        return Vector2.Distance(transform.position, FindClosestPlayer().position) >= exitSightRadius;
    }

    public bool InAttackRange()
    {
        return Vector2.Distance(transform.position, FindClosestPlayer().position) <= attackRadius;
    }

    public Vector2 DirectionToPlayer()
    {
        return (FindClosestPlayer().position - transform.position).normalized;
    }

    public float DistanceToPlayer()
    {
        return Vector2.Distance(transform.position, FindClosestPlayer().position);
    }

    public float PlayerLeftOrRight()
    {
        if (DirectionToPlayer().x >= 0)
            return 1f;
        else return -1f;
    }

    public float PlayerRelativeHeight()
    {
        return (FindClosestPlayer().position.y - transform.position.y);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, sightRadius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, exitSightRadius);
    }
}

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Enemy : MonoBehaviour
{
    public enum Behaviour
    {
        CHASE,
        EVADE
    }
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Collider2D col;
    [SerializeField] GameObject target;

    public float health = 100f;
    public float speed = 5f;
    public float range = 10f;
    public Behaviour behaviour;
    public bool chase = false;
    public bool evade = false;
    public Vector2 waypoint1;
    public Vector2 waypoint2;
    bool walkRight = true;
    void Start() {
        if(!target) target = GameObject.Find("Player");
        if(!col) col = GetComponent<Collider2D>();
        if(!rb) rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update() {
        Vector3 pos = transform.position;
        Vector2 dir;

        if (walkRight) {
            dir = waypoint1;
            if(pos.x > waypoint1.x) walkRight = false;
        } 
        else {
            dir = waypoint2;
            if(pos.x < waypoint2.x) walkRight = true;
        }

        Vector3 targetPos = target.transform.position;

        if(Vector3.Distance(pos, targetPos) <= range){
            if(behaviour == Behaviour.CHASE) chase = true;
            if(behaviour == Behaviour.EVADE) evade = true;
            walkRight = !walkRight;
        }
        else {
            chase = false;
            evade = false;
        }

        if (chase) dir = targetPos - pos;
        if (evade) dir = -(targetPos - pos);

        dir.Normalize();
        dir.y = rb.linearVelocity.y;
        rb.linearVelocity = speed * dir;
    }
}

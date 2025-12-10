using System.Collections.Generic;
using UnityEngine;

public class BT_Enemy : MonoBehaviour
{
    [SerializeField] List<Transform> waypoints;
    public float speed = 5.0f;

    public int currentWaypointIndex = 0;
    Rigidbody2D rb;
    void Start(){
        rb = GetComponent<Rigidbody2D>();
    }

    void Update() {
		if (Vector2.Distance(rb.position, waypoints[currentWaypointIndex].position) <= 0.1f) {
            print("triger");
			currentWaypointIndex++;
			if (currentWaypointIndex >= waypoints.Count) currentWaypointIndex = 0;
		}
	}
    void FixedUpdate(){
        Vector2 dir = waypoints[currentWaypointIndex].position - transform.position;
        dir.Normalize();
        rb.linearVelocity = new Vector2(dir.x * speed, rb.linearVelocity.y);
    }
}

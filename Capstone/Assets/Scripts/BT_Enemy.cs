using System;
using System.Collections.Generic;
using UnityEngine;

public class BT_Enemy : MonoBehaviour {
	[Header("Patrol Settings")]
	public float patrolSpeed = 2f;
	public float patrolRange = 5f;
	public LayerMask groundLayer; // Assign your ground layer
	public LayerMask wallLayer;   // Assign your wall layer
	public Transform patrolStartPoint;

	[Header("Detection Settings")]
	public float detectionRange = 8f;
	public LayerMask playerLayer = 1 << 8;  // Assign your player layer
	public LayerMask obstacleLayer = ~(1 << 8); // Everything except player

	[Header("Chase & Attack Settings")]
	public float chaseSpeed = 4f;
	public float attackRange = 2f;

	[Header("Debug")]
	public bool showDebugRays = true;

	private Rigidbody2D rb;
	private Animator animator; // Optional: for attack animation
	public Transform player;
	private Vector2 patrolStartPos;
	private Vector2 velocity;
	private int direction = -1; // 1 = right, -1 = left
	private BTNode rootNode;

	private float reversePatrolCooldown = 0.0f;
	public Transform rayOrigin;
	private CircleCollider2D attackTrigger;

	void Start() {
		rb = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();
		attackTrigger = GetComponent<CircleCollider2D>();
		//player = GameObject.FindGameObjectWithTag("Player")?.transform;

		if (patrolStartPoint == null) {
			patrolStartPos = transform.position;
		} else {
			patrolStartPos = patrolStartPoint.position;
		}

		// Build Behavior Tree
		rootNode = new Selector(new List<BTNode> {
			new Sequence(new List<BTNode> {
				new Condition(HasPlayerInAttackRange),
				new Action(PerformAttack)
			}),
			new Sequence(new List<BTNode> {
				new Condition(HasPlayerInSight),
				new Action(ChasePlayer)
			}),
			new Action(Patrol)
		});
	}

	void Update() {
		if (rootNode != null) {
			rootNode.Run();
		}
		DrawDebugRays();
		if(reversePatrolCooldown > 0.0f) {
			reversePatrolCooldown -= Time.deltaTime;
		}
	}

	#region Behavior Tree Tasks

	/// <summary>
	/// Patrols back and forth, reverses on wall/ledge collision
	/// </summary>
	NodeState Patrol() {
		// Check for boundaries/walls/ledges and reverse direction if needed
		if (CheckPatrolBoundary()) {
			ReverseDirection();
		}

		// Move in current direction
		MoveHorizontal(patrolSpeed * direction);

		return NodeState.Success;
	}

	/// <summary>
	/// Chases player if still in sight
	/// </summary>
	NodeState ChasePlayer() {
		if (!HasPlayerInSightImmediate()) {
			return NodeState.Failure; // Lost player
		}

		Vector2 directionToPlayer = (player.position - transform.position).normalized;
		MoveHorizontal(chaseSpeed * Mathf.Sign(directionToPlayer.x));
		FacePlayer();

		return NodeState.Running;
	}

	/// <summary>
	/// Performs attack when in range
	/// </summary>
	NodeState PerformAttack() {
		// Trigger attack animation
		if (animator != null) {
			animator.SetTrigger("Attack");
		}
		print("Attack");

		// Stop movement during attack
		rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
		FacePlayer();

		// Check if attack animation completed (you can use animator state or timer)
		// For simplicity, return Success immediately - replace with actual animation check
		return NodeState.Success;
	}

	#endregion

	#region Conditions

	/// <summary>
	/// Checks if player is within attack range AND visible via raycast
	/// </summary>
	NodeState HasPlayerInAttackRange() {
		return HasPlayerInSightImmediate() && DistanceToPlayer() <= attackRange
			? NodeState.Success : NodeState.Failure;
	}

	/// <summary>
	/// Checks if player is within detection range AND visible via raycast (front/back)
	/// </summary>
	NodeState HasPlayerInSight() {
		return HasPlayerInSightImmediate() ? NodeState.Success : NodeState.Failure;
	}

	private bool HasPlayerInSightImmediate() {
		if (player == null) return false;


		// Front detection ray
		bool frontVisible = RaycastToPlayer(detectionRange);

		// Back detection ray (shorter range)
		bool backVisible = RaycastToPlayerBehind(detectionRange * 0.7f);

		return frontVisible || backVisible;
	}

	#endregion

	#region Movement

	void MoveHorizontal(float speed) {
		velocity = new Vector2(speed, rb.linearVelocity.y);
		rb.linearVelocity = velocity;
	}

	void FacePlayer() {
		if (player != null) {
			Vector2 dir = player.position - transform.position;
			transform.localScale = new Vector3(Mathf.Sign(dir.x), 1f, 1f);
		}
	}

	void ReverseDirection() {
		direction *= -1;
		transform.localScale = new Vector3(direction, 1f, 1f);
	}

	#endregion

	//Patrol Boundary Detection
	bool CheckPatrolBoundary() {
		// add a delay check to prevent multiple triggers at once
		if(reversePatrolCooldown > 0.0f) {
			return false;
		}
		// Check patrol range
		float distanceFromStart = Mathf.Abs(transform.position.x - patrolStartPos.x);
		if (distanceFromStart > patrolRange) {
			reversePatrolCooldown = 0.15f;
			return true;
		}

		// Check wall ahead
		RaycastHit2D wallHit = Physics2D.Raycast(transform.position, Vector2.right * direction, 0.5f, wallLayer);
		if (wallHit.collider != null) {
			reversePatrolCooldown = 0.15f;
			return true;
		}

		// Check ledge ahead (raycast down from ahead position)
		//Vector2 ledgeCheckPos = (Vector2)transform.position + Vector2.right * direction * 0.5f;
		//RaycastHit2D groundHit = Physics2D.Raycast(ledgeCheckPos, Vector2.down, 0.3f, groundLayer);
		//if (groundHit.collider == null) {
		//	return true;
		//}

		return false;
	}


	#region Raycast Vision

	bool RaycastToPlayer(float maxDistance) {
		if (player == null) return false;

		Vector2 rayStartPos = rayOrigin != null ? (Vector2)rayOrigin.position : (Vector2)transform.position;
		Vector2 directionToPlayer = ((Vector2)player.position - rayStartPos).normalized;
		RaycastHit2D hit = Physics2D.Raycast(rayStartPos, directionToPlayer, maxDistance, obstacleLayer);

		return hit.collider != null && hit.collider.gameObject == player.gameObject;
	}

	bool RaycastToPlayerBehind(float maxDistance) {
		if (player == null) return false;

		Vector2 rayStartPos = rayOrigin != null ? (Vector2)rayOrigin.position : (Vector2)transform.position;
		// Check behind regardless of facing direction
		Vector2 directionToPlayer = ((Vector2)player.position - rayStartPos).normalized;
		if (Mathf.Abs(directionToPlayer.x + transform.localScale.x) < 0.5f) { // Player roughly behind
			RaycastHit2D hit = Physics2D.Raycast(rayStartPos, directionToPlayer, maxDistance, obstacleLayer);
			return hit.collider != null && hit.collider.gameObject == player.gameObject;
		}
		return false;
	}

	float DistanceToPlayer() {
		return player != null ? Vector2.Distance(transform.position, player.position) : float.MaxValue;
	}

	#endregion

	//Debug Visualization
	void DrawDebugRays() {
		if (!showDebugRays || !Application.isPlaying) return;

		Vector2 rayStartPos = rayOrigin != null ? (Vector2)rayOrigin.position : (Vector2)transform.position;

		if (player != null) {
			// Front detection ray
			Vector2 frontDir = Vector2.right * transform.localScale.x;
			Debug.DrawRay(rayStartPos, frontDir * detectionRange, Color.yellow);

			// Player direction ray
			Vector2 playerDir = ((Vector2)player.position - rayStartPos).normalized;
			Debug.DrawRay(rayStartPos, playerDir * detectionRange,
				RaycastToPlayer(detectionRange) ? Color.green : Color.red);

			// Attack range circle
			Debug.DrawRay(rayStartPos + (Vector2)Vector3.up * 0.5f, Vector3.right * attackRange, Color.magenta);
			Debug.DrawRay(rayStartPos + (Vector2)Vector3.down * 0.5f, Vector3.right * attackRange, Color.magenta);

			// Patrol boundary
			Debug.DrawLine(patrolStartPos + Vector2.left * patrolRange, patrolStartPos + Vector2.right * patrolRange, Color.blue);
		}

		// Wall/ledge detection rays
		Debug.DrawRay(transform.position, Vector2.right * direction * 0.5f, Color.cyan);
		Vector2 ledgePos = (Vector2)transform.position + Vector2.right * direction * 0.5f;
		Debug.DrawRay(ledgePos, Vector2.down * 0.3f, Color.cyan);
	}
}

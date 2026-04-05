using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;
using Unity.Netcode;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Player : NetworkBehaviour, IDamageable {
	Rigidbody2D rb;
	Collider2D col;
	Animator anim;
	SpriteRenderer spriteRenderer;
	PlayerInputHandler inputHandler;
	public PlayerStats playerStats;
	public event System.Action OnDeath;


	[Header("Movement Settings")]
	[SerializeField] private float walkSpeed = 5.0f;
	float originalWalkSpeed;
	private float horizontalAxis, verticalAxis;
	float playerDirection = 1.0f; // -1 -> left, 1 -> right

	[Header("Sprint Settings")]
	[SerializeField] float sprintMultiplier = 1.3f;
	[SerializeField] float durationBeforeSprint = 2.0f;//sprint after 2 seconds of moving in the same direction
	float oldDirection = 0f;
	float walkHeldDuration = 0.0f;//this tracks how long the player has held the walk button down
	bool isSprinting = false;

	[Header("Dash Settings")]
	public float dashSpeed = 20f;
	public float dashDuration = 0.2f;
	public float dashCooldown = 0.1f;
	//[SerializeField] private TrailRenderer trailRenderer;
	private bool canDash = true;
	private bool isDashing;

	[Header("Jump Settings")]
	public float jumpForce = 2.0f;
	private int jumpCount = 2; // double jump
	private int jumps = 0;
	[SerializeField] private float defaultGravity = 1.0f;
	[SerializeField] private float fallingGravityMultiplier = 2.0f;

	[Header("Ground Check settings")]
	public float groundCheckRadius = 0.2f;
	private Transform groundCheck;
	[SerializeField] private LayerMask groundLayer;

	//[Header("Attck settings")]
	public enum AttackDirection { Left, Right, Up, Down }
	Dictionary<AttackDirection, Vector2> attackPointLocations;
	GameObject attackPoint; // this is the owner of the attack graphics and colliders
	[SerializeField] Vector2 attackOffset = new Vector2(0.844f, 0.826f);
	AttackDirection currentAttackDir;
	bool isAttacking = false;
	bool canAttack = true;
	[SerializeField] float attackCooldown = 0.5f;
	float lastAttackTime = -Mathf.Infinity;
	[SerializeField] float meleeDuration = 0.3f;
	[SerializeField] float pushbackScale = 5f;
	private bool isBouncing = false;
	[SerializeField] private float bounceDuration = 0.2f;

	[Header("Vengful spirite Ability Settings")]
	[SerializeField] GameObject VengefulSpiritProjectilePrefab;
	[SerializeField] Transform projectileSpawnPoint;

	[Header("Howling Wraiths Ability Settings")]
	[SerializeField] GameObject HowlingWraithsPrefab;
	[SerializeField] Transform HowlingWraithsSpawnPoint;

	[Header("Damage Settings")]
	[SerializeField] float invincibilityDuration = 1.0f;
	[SerializeField] AudioClip damageSound;
	private AudioSource audioSource;
	private bool isInvincible = false;

	void Start() {
		if (!rb) rb = GetComponent<Rigidbody2D>();
		if (!col) col = GetComponent<Collider2D>();
		if (!anim) anim = GetComponent<Animator>();
		if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();

		inputHandler = GetComponent<PlayerInputHandler>();
		if (!inputHandler) Assert.Fail("Player does not have an input handler, plz fix.");

		playerStats = GetComponent<PlayerStats>();
		if (!playerStats) Assert.Fail("Player does not have PlayerStats object, plz fix.");

		if (!attackPoint) attackPoint = gameObject.transform.Find("AttackPoint").gameObject;
		attackPoint.SetActive(false);

		rb.gravityScale = defaultGravity;
		if (!groundCheck) groundCheck = transform.Find("GroundCheck");
		groundLayer = LayerMask.GetMask("Ground");
		oldDirection = playerDirection;
		originalWalkSpeed = walkSpeed;

		audioSource = GetComponent<AudioSource>();
		if (!audioSource) {
			audioSource = gameObject.AddComponent<AudioSource>();
		}

		playerStats.OnPlayerDeath += () => OnDeath?.Invoke();

		attackPointLocations = new Dictionary<AttackDirection, Vector2>(){
			{
				AttackDirection.Left,
				new Vector3(-2.60479999f,-0.180000007f)
			},
			{
				AttackDirection.Right,
				new Vector3(1.16999996f,-0.180000007f)
			},
			{
				AttackDirection.Up,
				new Vector3(0.0900000036f,1.01999998f)
			},
			{
				AttackDirection.Down,
				new Vector3(0.0900000036f,-2.76999998f)
			}

		};

		//playerStats.
	}

	void Update() {
		if (NetworkManager.Singleton && !IsOwner) return;
		if (!GetComponent<PlayerInput>().enabled){
			horizontalAxis = 0f;
			walkSpeed = 0.0f;
			Move();
			return;
		};

		float rawHorizontal = inputHandler.MovementInput.x;
		horizontalAxis = Mathf.Abs(rawHorizontal) > 0.01f ? rawHorizontal : 0f;
		SetPlayerDirection();

		if (inputHandler.JumpTriggered) HandleJumpPress();
		if (inputHandler.JumpReleased) HandleJumpRelease();
		if (inputHandler.DashTriggered) HandleDashPress();
		if (inputHandler.AttackTriggered) HandleAttack();
		if (inputHandler.QuickCastTriggered) HandleQuickCast();

		// walk held
		if (Mathf.Abs(horizontalAxis) > 0.01f) {
			walkHeldDuration += Time.deltaTime;
			anim.SetBool("isMoving", true);
		} else {
			// Reset timer if we stop moving
			anim.SetBool("isMoving", false);
			ResetSprint();
		}

		if (walkHeldDuration >= durationBeforeSprint && !isSprinting) {
			isSprinting = true;
			walkSpeed = originalWalkSpeed * sprintMultiplier;
		}

		inputHandler.ConsumeTriggers();

	}

	void FixedUpdate() {

		Move();
		// if player is falling, add gravity
		if (rb.linearVelocity.y < -0.4f) {
			rb.gravityScale = defaultGravity * fallingGravityMultiplier;
		} else {
			rb.gravityScale = defaultGravity;
		}
	}


	private void Move() {
		if (isDashing || isBouncing) return;
		if (Mathf.Abs(horizontalAxis) < 0.01f) {
			horizontalAxis = 0f;
		}
		Vector2 movement = new Vector2(horizontalAxis * walkSpeed, rb.linearVelocity.y);

		rb.linearVelocity = movement;

		//if(Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer)){
		if (IsGrounded()) {
			jumps = 0; // reset double jump
			anim.SetBool("isGrounded", true);
		} else {
			anim.SetBool("isGrounded", false);
		}
	}

	void SetPlayerDirection() {
		if (horizontalAxis < 0) playerDirection = -1f;
		else if (horizontalAxis > 0) playerDirection = 1f;

		if (oldDirection != playerDirection) {
			anim.SetTrigger("Turn");
			ResetSprint();
			oldDirection = playerDirection;

			spriteRenderer.flipX = playerDirection > 0f;
			
			if(NetworkManager.Singleton){
				if(NetworkManager.Singleton.IsClient)
					SetFacingServerRpc(playerDirection < 0f);
				if(NetworkManager.Singleton.IsHost)
					SetFacingClientRpc(playerDirection > 0f);
			}
		}
	}
	private void HandleJumpPress() {
		if (jumps < jumpCount - 1) { // -1 to account for the very next frame where it resets the double jump
			anim.SetTrigger("Jump");
			rb.gravityScale = defaultGravity;
			//Debug.Log("JUMPED");
			//rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
			rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
			if (jumps == 1) {
				anim.SetTrigger("Jump");
			} else if (jumps == 2) {
				anim.SetTrigger("Double Jump");
			}
			jumps++;
		}
	}
	private void HandleJumpRelease() {
		if (rb.linearVelocity.y > 0) {
			rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.1f);
			rb.gravityScale = defaultGravity * fallingGravityMultiplier;
		}
	}

	private void HandleDashPress() {
		if (isDashing) return; // Prevent double dash
		anim.SetTrigger("Dash");
		StartCoroutine(DashRoutine());
	}
	private System.Collections.IEnumerator DashRoutine() {
		if (!canDash) yield break;

		canDash = false;
		isDashing = true;

		float originalGravity = rb.gravityScale;
		rb.gravityScale = 0f; // No gravity during dash (Hollow Knight style)

		rb.linearVelocity = new Vector2(playerDirection * dashSpeed, 0f); // Zero Y velocity

		// visuals
		//if (trailRenderer) trailRenderer.emitting = true;

		yield return new WaitForSeconds(dashDuration);

		// reset
		//if (trailRenderer) trailRenderer.emitting = false;
		rb.gravityScale = originalGravity;
		isDashing = false;

		// Stop momentum (optional, prevents sliding after dash)
		rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

		//cooldown
		yield return new WaitForSeconds(dashCooldown);
		canDash = true;
	}

	private void ResetSprint() {
		walkHeldDuration = 0.0f;
		isSprinting = false;
		walkSpeed = originalWalkSpeed;
	}
	private void HandleAttack() {
		if (Time.time < lastAttackTime + attackCooldown) return;
		lastAttackTime = Time.time;
		if (isAttacking) return; // prevent coroutine stacking
		StartCoroutine(EnableAttack());
	}

	private bool IsGrounded() {
		return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
	}

	private IEnumerator EnableAttack() {
		isAttacking = true;
		attackPoint.SetActive(true);

		Vector2 attackInput = inputHandler.MovementInput;
		var localPos = attackPoint.transform.localPosition;
		Quaternion localRot = Quaternion.identity;

		bool grounded = IsGrounded();
		float attackVerticalThreshold = 0.6f;

		SpriteRenderer sprite = attackPoint.transform.Find("Sprite").GetComponent<SpriteRenderer>();
		if (attackInput.y > attackVerticalThreshold) {
			// UP attack
			currentAttackDir = AttackDirection.Up;
			sprite.flipX = true;
			//localPos = attackPointLocations[AttackDirection.Up];
			localRot = Quaternion.Euler(0, 0, 90f);
		} else if (!grounded && attackInput.y < -attackVerticalThreshold) {
														  // DOWN attack
			currentAttackDir = AttackDirection.Down;
			//localPos = attackPointLocations[AttackDirection.Down];
			sprite.flipX = false;
			localRot = Quaternion.Euler(0, 0, 90f);
		} else {
			// LEFT / RIGHT attack (always available)
			currentAttackDir = playerDirection > 0 ? AttackDirection.Right : AttackDirection.Left;
			switch (currentAttackDir) {
			case AttackDirection.Left:
				sprite.flipX = false;	break;
			case AttackDirection.Right:
				sprite.flipX = true; break;
			}
			//localPos = new Vector2(attackOffset.x * playerDirection, 0f);
			localRot = Quaternion.identity; // hitbox already faces right by default
		}
		localPos = attackPointLocations[currentAttackDir];

		attackPoint.transform.localPosition = localPos;
		attackPoint.transform.localRotation = localRot;

		// Fire the correct animation trigger
		switch (currentAttackDir) {
		case AttackDirection.Up: anim.SetTrigger("AttackUp"); break;
		case AttackDirection.Down: anim.SetTrigger("AttackDown"); break;
		default: anim.SetTrigger("Attack"); break;
		}

		yield return new WaitForSeconds(meleeDuration);
		attackPoint.SetActive(false);
		isAttacking = false;
	}

	private void HandleQuickCast() {
		Vector2 moveInput = inputHandler.MovementInput;

		if (moveInput.y > 0.5f) {
			// if Up is held
			CastHowlingWraiths();
		} else {
			CastVengefulSpirit();
		}
	}

	private void CastVengefulSpirit() {
		if (VengefulSpiritProjectilePrefab == null || projectileSpawnPoint == null) {
			Debug.LogError("Vengeful Spirit: Projectile prefab or projectileSpawnPoint is not set");
			return;
		}
		if(!NetworkManager.Singleton){
			GameObject proj = Instantiate(
				VengefulSpiritProjectilePrefab,
				projectileSpawnPoint.position,
				Quaternion.identity
			);

			// Flip sprite to match travel direction
			proj.GetComponent<SpriteRenderer>().flipX = playerDirection > 0f;

			proj.GetComponent<Projectile>().Init(playerDirection);
		} else {
			SpawnVengefulSpiritServerRpc(projectileSpawnPoint.position, Quaternion.identity);
		}
		
	}
	private void CastHowlingWraiths(){
		if (HowlingWraithsPrefab == null || HowlingWraithsSpawnPoint == null) {
			Debug.LogError("Howling Wraiths: prefab or SpawnPoint is not set");
			return;
		}
		if(!NetworkManager.Singleton){
		GameObject proj = Instantiate(
			HowlingWraithsPrefab,
			HowlingWraithsSpawnPoint.position,
			Quaternion.identity
		);
		} else {
			SyncHowlingWraithsServerRpc(HowlingWraithsSpawnPoint.position, Quaternion.identity);
		}
	}

	// NETWORKING SYNCHRONIZATION METHODS
	// SERVER SIDED METHODS FOR REPLICATION (what server sees)
	[ServerRpc]
	void SetFacingServerRpc(bool facingRight)
	{
		spriteRenderer.flipX = !facingRight;
		playerDirection *= -1;
	}

	[ServerRpc]
    void SpawnVengefulSpiritServerRpc(Vector3 pos, Quaternion rot)
    {
        GameObject obj = Instantiate(VengefulSpiritProjectilePrefab, pos, rot);
        obj.GetComponent<NetworkObject>().Spawn();

		obj.GetComponent<Projectile>().Init(playerDirection);
    }

	[ServerRpc]
    void SyncHowlingWraithsServerRpc(Vector3 pos, Quaternion rot)
    {
        GameObject obj = Instantiate(HowlingWraithsPrefab, pos, rot);
        obj.GetComponent<NetworkObject>().Spawn();
		
    }
	// CLIENT SIDE METHODS FOR REPLICATION (what client sees)
	[ClientRpc]
	void SetFacingClientRpc(bool facingRight)
	{
		spriteRenderer.flipX = !facingRight;
		playerDirection *= -1;
	}

	public void Respawn() {
		SpawnPoint[] spawnPoints = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);

		float minDist = float.MaxValue;
		SpawnPoint closestSpawnPoint = spawnPoints[0];
		foreach (SpawnPoint spawnPoint in spawnPoints) {
			float dist = Vector2.Distance(transform.position, spawnPoint.gameObject.transform.position);
			dist = dist < minDist ? dist : minDist;
			closestSpawnPoint = spawnPoint;
		}
		// move the player to the closes spawn point to them
		Debug.LogWarning("Player respawned");
		transform.position = closestSpawnPoint.transform.position;
	}

	public void BounceBack() {
		StartCoroutine(BounceRoutine(GetBounceDirectionFromAttack()));
	}

	public void BounceBack(Vector2 sourcePosition) {
		StartCoroutine(BounceRoutine(GetBounceDirectionFromSource(sourcePosition)));
	}

	private Vector2 GetBounceDirectionFromAttack() {
		switch (currentAttackDir) {
		case AttackDirection.Up: return Vector2.down;
		case AttackDirection.Down: return Vector2.up;
		case AttackDirection.Right: return Vector2.left;
		case AttackDirection.Left: return Vector2.right;
		default: return Vector2.left;
		}
	}

	private Vector2 GetBounceDirectionFromSource(Vector2 sourcePosition) {
		Vector2 difference = (Vector2)transform.position - sourcePosition;

		// Vertical bounce when the spike is mainly below/above the player
		if (Mathf.Abs(difference.y) > Mathf.Abs(difference.x)) {
			return difference.y >= 0 ? Vector2.up : Vector2.down;
		}

		// Horizontal bounce away from the spike
		return difference.x >= 0 ? Vector2.right : Vector2.left;
	}

	private System.Collections.IEnumerator BounceRoutine(Vector2 bounceDirection) {
		if (isBouncing) yield break;

		isBouncing = true;

		// Reset velocity so the knockback is consistent and doesn't stack with falling momentum
		rb.linearVelocity = Vector2.zero;

		// Apply the knockback force
		rb.AddForce(bounceDirection * pushbackScale, ForceMode2D.Impulse);

		// Wait for the knockback to finish before giving movement back to the player
		yield return new WaitForSeconds(bounceDuration);

		isBouncing = false;
	}

	public void TakeDamage(float amount) {
		print("Player got hit");
		if (isInvincible) return;

		playerStats.TakeDamage(1);

		// Play sound
		if (damageSound && audioSource) {
			audioSource.PlayOneShot(damageSound);
		}

		// Start invincibility
		StartCoroutine(InvincibilityRoutine());
	}

	private System.Collections.IEnumerator InvincibilityRoutine() {
		isInvincible = true;

		// Optional: Make player flash or something for visual feedback
		// For now, just disable collider to prevent further damage
		col.enabled = false;//bug?splayer could fall

		yield return new WaitForSeconds(invincibilityDuration);

		col.enabled = true;
		isInvincible = false;
	}

}

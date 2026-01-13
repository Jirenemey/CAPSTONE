using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Player : MonoBehaviour
{
    Rigidbody2D rb;
    Collider2D col;

	[Header("Input system refrences")]
	[SerializeField] InputActionReference moveAction;
	[SerializeField] InputActionReference jumpAction;
	[SerializeField] InputActionReference dashAction;

	[Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5.0f;
	float originalWalkSpeed;
	private float horizontalAxis;
	float playerDirection = 1.0f;

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

	void Start() {
        if(!rb) rb = GetComponent<Rigidbody2D>();
        if(!col) col = GetComponent<Collider2D>();
        rb.gravityScale = defaultGravity;
        if(!groundCheck) groundCheck = transform.Find("GroundCheck");
        groundLayer = LayerMask.GetMask("Ground");
		oldDirection = playerDirection;
		originalWalkSpeed = walkSpeed;
	}
	private void OnEnable() {
		// Input system event binding
		jumpAction.action.started += OnJumpPress; // ButtonDown equivalent
		jumpAction.action.canceled += OnJumpRelease; // ButtonUp equivalent
		jumpAction.action.Enable();

		dashAction.action.started += OnDashPress;
		dashAction.action.Enable();
	}
	void OnDisable() {
		jumpAction.action.Disable();
		jumpAction.action.started -= OnJumpPress;
		jumpAction.action.canceled -= OnJumpRelease;

		dashAction.action.Disable();
		dashAction.action.started -= OnDashPress;
	}

	void Update() {
        GetInputs();

		if (Mathf.Abs(horizontalAxis) > 0.01f) {
			walkHeldDuration += Time.deltaTime;
		} else {
			// Reset timer if we stop moving
			ResetSprint();
		}

		if (walkHeldDuration >= durationBeforeSprint && !isSprinting) {
			isSprinting = true;
			walkSpeed = originalWalkSpeed * sprintMultiplier;
		}
	}

	void FixedUpdate() {
        Move();
        // if player is falling, add gravity
        if(rb.linearVelocity.y < -0.4f) {
            rb.gravityScale = defaultGravity * fallingGravityMultiplier;
		} else {
            rb.gravityScale = defaultGravity;
        }
    }


	private void Move() {
		if (isDashing) return;
        Vector2 movement = new Vector2(horizontalAxis * walkSpeed, rb.linearVelocity.y);

        rb.linearVelocity = movement;

        if(Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer)){
            jumps = 0; // reset double jump
        }
    }

    void GetInputs(){
		//horizontalAxis = Input.GetAxis("Horizontal");
		horizontalAxis = moveAction.action.ReadValue<Vector2>().x;
		if		(horizontalAxis < 0) playerDirection = -1f;
		else if (horizontalAxis > 0) playerDirection = 1f;

		if (oldDirection != playerDirection) {
			ResetSprint();
			oldDirection = playerDirection;
		}
	}

	private void OnJumpPress(InputAction.CallbackContext context) {
		if (jumps < jumpCount - 1) { // -1 to account for the very next frame where it resets the double jump
			rb.gravityScale = defaultGravity;
			Debug.Log("JUMPED");
			//rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
			rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
			jumps++;
		}
	}
	private void OnJumpRelease(InputAction.CallbackContext context) {
		if (rb.linearVelocity.y > 0) {
			rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.x * 0.1f);
			rb.gravityScale = defaultGravity * fallingGravityMultiplier;
		}
	}

	private void OnDashPress(InputAction.CallbackContext context) {
		if (isDashing) return; // Prevent double dash
		Debug.Log("Dash");
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
}

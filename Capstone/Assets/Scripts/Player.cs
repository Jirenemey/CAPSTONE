using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Player : MonoBehaviour
{
    Rigidbody2D rb;
    Collider2D col;
	Animator anim;
	SpriteRenderer spriteRenderer;

	[Header("Input system refrences")]
	[SerializeField] InputActionReference moveAction;
	[SerializeField] InputActionReference lookAction;
	[SerializeField] InputActionReference jumpAction;
	[SerializeField] InputActionReference dashAction;
	[SerializeField] InputActionReference attackAction;

	[Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5.0f;
	float originalWalkSpeed;
	private float horizontalAxis;
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

	//[Header("Look Settings")]
	////[SerializeField] Vector2 lookAxis;
	//[SerializeField] float lookDelay = 1.0f;
	//float currentLookTimer = 0.0f;// counts up while the look buttom is down
	//[SerializeField] Vector2 cameraLookOffset;
	//bool cameraMoved = false;
	//Vector3 oldCameraPos;

	void Start() {
        if(!rb) rb = GetComponent<Rigidbody2D>();
        if(!col) col = GetComponent<Collider2D>();
		if(!anim) anim = GetComponent<Animator>();
		if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();

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

		attackAction.action.started += OnAttackMeele;
		attackAction.action.Enable();

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
		//Look(lookAxis);

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
			anim.SetBool("isGrounded", true);
		} else {
			anim.SetBool("isGrounded", false);
		}
	}

    void GetInputs(){
		//horizontalAxis = Input.GetAxis("Horizontal");
		horizontalAxis = moveAction.action.ReadValue<Vector2>().x;

		//lookAxis = lookAction.action.ReadValue<Vector2>();

		SetPlayerDirection();
	}

	void SetPlayerDirection() {
		if (horizontalAxis < 0)		 playerDirection = -1f;
		else if (horizontalAxis > 0) playerDirection = 1f;

		if (oldDirection != playerDirection) {
			anim.SetTrigger("Turn");
			ResetSprint();
			oldDirection = playerDirection;

			spriteRenderer.flipX = playerDirection > 0f ? true : false; 

		}
	}
	// Not working
	//private void Look(Vector2 lookAxis_) {
	//	// make sure the look button has been held for long enouph
	//	if (lookAxis_.magnitude > 0f) {
	//		currentLookTimer += Time.deltaTime;
	//		if (currentLookTimer >= lookDelay && !cameraMoved) {
	//			// only look in one direction at a time
	//			// find the dominant direction
	//			lookAxis_ = lookAxis_.normalized; // Normalize to unit vector
	//			Vector2 camerOffsetDirection;
	//			if (Mathf.Abs(lookAxis_.x) > Mathf.Abs(lookAxis_.y))
	//				camerOffsetDirection = new Vector2(Mathf.Sign(lookAxis_.x), 0);
	//			else
	//				camerOffsetDirection = new Vector2(0, Mathf.Sign(lookAxis_.y));
	//			// Offset camera in the right direction
	//			camerOffsetDirection.x *= cameraLookOffset.x;
	//			camerOffsetDirection.y *= cameraLookOffset.y;
	//			oldCameraPos = Camera.allCameras[0].transform.position;
	//			Camera.allCameras[0].transform.position += new Vector3(camerOffsetDirection.x, camerOffsetDirection.y, 0.0f);
	//			cameraMoved = true;
	//		}
	//	} else {
	//		currentLookTimer = 0f;
	//		// reset camera
	//		ResetCamera();
	//	}
	//}
	//void ResetCamera() {
	//	Camera.allCameras[0].transform.position = oldCameraPos;
	//	cameraMoved = false;
	//}

	private void OnJumpPress(InputAction.CallbackContext context) {
		if (jumps < jumpCount - 1) { // -1 to account for the very next frame where it resets the double jump
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
	private void OnJumpRelease(InputAction.CallbackContext context) {
		if (rb.linearVelocity.y > 0) {
			rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.x * 0.1f);
			rb.gravityScale = defaultGravity * fallingGravityMultiplier;
		}
	}

	private void OnDashPress(InputAction.CallbackContext context) {
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

	private void OnAttackMeele(InputAction.CallbackContext context) {
		Debug.Log("Attack");
		anim.SetTrigger("Attack");
	}
}

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Player : MonoBehaviour
{
    Rigidbody2D rb;
    Collider2D col;

    [Header("Horizontal Movement Settings")]
    public float walkSpeed = 5.0f;
	private float horizontalAxis;

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
    }

    // Update is called once per frame
    void Update() {
        GetInputs();
        Jump();
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
        Vector2 movement = new Vector2(horizontalAxis * walkSpeed, rb.linearVelocity.y);

        rb.linearVelocity = movement;

        if(Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer)){
            jumps = 0; // reset double jump
        }
    }
    private void Jump() {
        // cancle jump when player lets got of button
        if(Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0) {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.x*0.1f);
            rb.gravityScale = defaultGravity * fallingGravityMultiplier;
        }
		if (Input.GetButtonDown("Jump") && jumps < jumpCount - 1) { // -1 to account for the very next frame where it resets the double jump
            rb.gravityScale = defaultGravity;
			Debug.Log("JUMPED");
            //rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
			jumps++;
		}
	}

    void GetInputs(){
        horizontalAxis = Input.GetAxis("Horizontal");
    }

}

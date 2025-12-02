using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Player : MonoBehaviour
{
    Rigidbody2D rb;
    Collider2D col;

    // Movement
    public float speed = 5.0f;
    public float jumpForce = 2.0f;
    private float horizontalAxis;

    // Jumping
    private int jumpCount = 2; // double jump
    private int jumps = 0;
    public float groundCheckRadius = 0.2f;
    Transform groundCheck;
    LayerMask groundLayer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(!rb) rb = GetComponent<Rigidbody2D>();
        if(!col) col = GetComponent<Collider2D>();
        if(!groundCheck) groundCheck = transform.Find("GroundCheck");
        groundLayer = LayerMask.GetMask("Ground");
    }

    // Update is called once per frame
    void Update()
    {
        GetInputs();
    }

    void FixedUpdate() {
        PlayerPhysics();
    }

    void PlayerPhysics() {
        Vector2 movement = new Vector3(horizontalAxis * speed, rb.linearVelocity.y);

        rb.linearVelocity = movement;

        if(Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer)){
            jumps = 0; // reset double jump
        }
    }

    void GetInputs(){
        horizontalAxis = Input.GetAxis("Horizontal");

        if(Input.GetButtonDown("Jump") && jumps < jumpCount - 1){ // -1 to account for the very next frame where it resets the double jump
            Debug.Log("JUMPED");
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumps++;
        }
    }
}

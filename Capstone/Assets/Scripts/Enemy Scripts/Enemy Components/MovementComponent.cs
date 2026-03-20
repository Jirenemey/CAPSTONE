using UnityEngine;

public class MovementComponent : MonoBehaviour
{
    [SerializeField] protected float moveSpeed = 3.0f;

    protected Rigidbody2D rb;

    protected void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
    }
    public virtual void Move(Vector2 direction)
    {
        rb.linearVelocity = direction * moveSpeed;
    }

    public virtual void Stop()
    {
        rb.linearVelocity = Vector2.zero;
    }
}

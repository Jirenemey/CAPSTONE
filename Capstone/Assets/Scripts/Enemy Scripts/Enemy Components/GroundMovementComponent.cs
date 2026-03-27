using UnityEngine;

public class GroundMovementComponent : MovementComponent
{
    public override void Move(Vector2 direction)
    {
        //rb.linearVelocity = new Vector2(direction.x * moveSpeed, 0f);
        Vector2 velocity = rb.linearVelocity;
        velocity.x = direction.x * moveSpeed;
        rb.linearVelocity = velocity;
    }

    public override void Stop()
    {
        Vector2 velocity = rb.linearVelocity;
        velocity.x = 0f;
        rb.linearVelocity = velocity;
    }
}

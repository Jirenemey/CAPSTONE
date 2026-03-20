using UnityEngine;

public class GroundMovementComponent : MovementComponent
{
    public override void Move(Vector2 direction)
    {
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, direction.y);
    }
}

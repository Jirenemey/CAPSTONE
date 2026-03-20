using UnityEngine;

public class FlyMovementComponent : MovementComponent
{
    public override void Move(Vector2 direction)
    {
        rb.linearVelocity = direction * moveSpeed;
    }
}

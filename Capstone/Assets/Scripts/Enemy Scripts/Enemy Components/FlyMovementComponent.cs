using UnityEngine;

public class FlyMovementComponent : MovementComponent
{
    public override void Move(Vector2 direction)
    {
        rb.linearVelocity = direction.normalized * moveSpeed;
        //rb.linearVelocity = new Vector2(direction.x * moveSpeed, direction.y * moveSpeed);
    }
}

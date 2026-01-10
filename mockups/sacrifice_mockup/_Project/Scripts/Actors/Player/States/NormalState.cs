using Godot;

namespace Apotemno.Actors.Player.States;

public class NormalState : IPlayerState
{
    public void Enter(PlayerController player) { }

    public void Update(PlayerController player, double delta)
    {
        // Debug
        // if (Input.IsKeyPressed(Key.C)) player.TransitionTo(player.StateCrawl);
    }

    public void PhysicsUpdate(PlayerController player, double delta)
    {
        if (player.InputManager == null) return;

        Vector2 inputDir = player.InputManager.GetMovementVector();
        // Transform 2D input to 3D direction relative to player
        Vector3 direction = (player.Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

        float speed = player.WalkSpeed;
        if (Input.IsActionPressed("sprint") && player.CurrentStamina > 0)
        {
             speed = player.SprintSpeed;
             // We set a flag on player to drain stamina
             // player.IsSprinting = true; // Need to expose setter or method
             // For now simple check in PlayerController handles logic if we just set velocity high?
             // Actually better to let PlayerController handle Sprint state or expose public property.
             // We'll trust PlayerController reads input, but State defines speed.
        }

        Vector3 velocity = player.Velocity;

        if (direction != Vector3.Zero)
        {
            velocity.X = direction.X * speed;
            velocity.Z = direction.Z * speed;
        }
        else
        {
            velocity.X = Mathf.MoveToward(player.Velocity.X, 0, speed);
            velocity.Z = Mathf.MoveToward(player.Velocity.Z, 0, speed);
        }

        player.Velocity = velocity;
    }

    public void Exit(PlayerController player) { }
}

using Godot;

namespace Apotemno.Actors.Player.States;

public class CrawlingState : IPlayerState
{
    public void Enter(PlayerController player) 
    {
        // Reduce height
        if (player.PlayerVisuals != null) player.PlayerVisuals.Scale = new Vector3(1, 0.4f, 1);
        if (player.PlayerCollider != null) player.PlayerCollider.Scale = new Vector3(1, 0.4f, 1);
    }

    public void Update(PlayerController player, double delta) { }

    public void PhysicsUpdate(PlayerController player, double delta)
    {
         if (player.InputManager == null) return;

        Vector2 inputDir = player.InputManager.GetMovementVector();
        Vector3 direction = (player.Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

        float speed = player.WalkSpeed * player.CrawlSpeedPenalty;
        
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

    public void Exit(PlayerController player) 
    {
         // Restore height
        if (player.PlayerVisuals != null) player.PlayerVisuals.Scale = Vector3.One;
        if (player.PlayerCollider != null) player.PlayerCollider.Scale = Vector3.One;
    }
}

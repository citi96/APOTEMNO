using Godot;

namespace Apotemno.Actors.Player.States;

public class CrawlingState : IPlayerState
{
    public void Enter(PlayerController player)
    {
        // Apply penalties
        GD.Print("[VESSEL] Legs gone. Crawling initiated.");
        
        // Visual/Collider Logic - reducing height
        // Since we don't have the specific setup, we'll try to scale the collider or just log it.
        // If PlayerCollider is assigned:
        if (player.PlayerCollider != null)
        {
             // Temporary hack: Scale Y down? Or switch shape?
             // Safest for generic:
             player.PlayerCollider.Scale = new Vector2(1, 0.5f);
        }
        
        // Camera logic
        if (player.PlayerCamera != null)
        {
            // Lower camera ? Zoom in?
            player.PlayerCamera.Zoom = new Vector2(1.2f, 1.2f); // Feel closer to ground
        }
    }

    public void Update(PlayerController player, double delta)
    {
        // Crawling is forced, maybe can't go back unless healed?
        // Logic for "Restoring" legs not requested yet.
        
        // Debug: Press N to return (if allowed)
        if (Input.IsKeyPressed(Key.N))
        {
            player.TransitionTo(player.StateNormal);
        }
    }

    public void PhysicsUpdate(PlayerController player, double delta)
    {
        if (player.InputManager == null) return;

        Vector2 inputDir = player.InputManager.GetMovementVector();
        
        // Reduced Speed
        float speed = player.MovementSpeed * player.CrawlSpeedPenalty;
        
        Vector2 velocity = player.Velocity;
        
        if (inputDir != Vector2.Zero)
        {
            velocity = inputDir * speed;
        }
        else
        {
            velocity = velocity.MoveToward(Vector2.Zero, speed);
        }

        player.Velocity = velocity;
    }

    public void Exit(PlayerController player)
    {
        // Restore
        if (player.PlayerCollider != null)
        {
             player.PlayerCollider.Scale = new Vector2(1, 1);
        }
        
        if (player.PlayerCamera != null)
        {
            player.PlayerCamera.Zoom = new Vector2(1, 1);
        }
    }
}

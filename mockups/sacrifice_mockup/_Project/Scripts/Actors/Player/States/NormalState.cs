using Godot;

namespace Apotemno.Actors.Player.States;

public class NormalState : IPlayerState
{
    public void Enter(PlayerController player)
    {
        // Reset camera/collider to normal
        // Assuming default values or storing "NormalHeight" in player could be useful
        // For now, we trust the editor setup is "Normal"
        if (player.PlayerCamera != null)
        {
            // Reset offset logic if implemented
        }
        
    }

    public void Update(PlayerController player, double delta)
    {
        // Handle State Transitions (e.g. if health is low, or trigger)
        // For PROD-001 we just need the state to exist.
        // User asked: "Questo stato sarà forzato quando avverrà il sacrificio delle gambe."
        // So we might need a method to ForceCrawl.
        
        // Debug testing: Press C to crawl
        if (Input.IsKeyPressed(Key.C)) // Temporary debug
        {
            player.TransitionTo(player.StateCrawl);
        }
    }

    public void PhysicsUpdate(PlayerController player, double delta)
    {
        if (player.InputManager == null) return;

        Vector2 inputDir = player.InputManager.GetMovementVector();
        
        Vector2 velocity = player.Velocity;
        
        if (inputDir != Vector2.Zero)
        {
            velocity = inputDir * player.MovementSpeed;
        }
        else
        {
            velocity = velocity.MoveToward(Vector2.Zero, player.MovementSpeed);
        }

        player.Velocity = velocity;
    }

    public void Exit(PlayerController player)
    {
        // Cleanup if needed
    }
}

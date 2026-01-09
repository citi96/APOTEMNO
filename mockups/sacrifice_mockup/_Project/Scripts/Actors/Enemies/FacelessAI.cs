using Godot;
using Apotemno.Actors.Player;

namespace Apotemno.Actors.Enemies;

public partial class FacelessAI : CharacterBody2D
{
    [Export] public float SpeedMultiplier = 1.8f;
    [Export] public float FovDotThreshold = 0.5f; // ~60 degrees cone
    
    [Export] public NavigationAgent2D NavAgent;
    [Export] public VisibleOnScreenNotifier2D VisNotifier;
    [Export] public AudioStreamPlayer2D StaticSound;
    
    // Aesthetic: Jitter the sprite when observed
    [Export] public Node2D Visuals;

    private PlayerController _targetPlayer;
    private bool _isObserved = false;

    public override void _Ready()
    {
        // Simple search for player if not assigned (or use singleton if available)
        // For now, find first node in group or specific type
        var players = GetTree().GetNodesInGroup("Player");
        if (players.Count > 0)
        {
            _targetPlayer = players[0] as PlayerController;
        }
        else
        {
            GD.PrintErr("[FACELESS] No Player found in group 'Player'!");
        }

        if (NavAgent != null)
        {
            NavAgent.PathDesiredDistance = 10.0f;
            NavAgent.TargetDesiredDistance = 10.0f;
        }

        // Essential: Wait for NavigationServer sync
        Callable.From(ActorSetup).CallDeferred();
    }

    private async void ActorSetup()
    {
        // Wait for the first physics frame so the NavigationServer can sync.
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_targetPlayer == null) return;

        bool currentlyObserved = CheckIfObserved();

        if (currentlyObserved)
        {
            // STATE A: OBSOLETE / STATUE
            Velocity = Vector2.Zero;
            
            // Effect: Jitter / Glitch
            if (Visuals != null)
            {
                Visuals.Position = new Vector2(
                    (float)GD.RandRange(-1, 1), 
                    (float)GD.RandRange(-1, 1)
                );
            }
            
            if (StaticSound != null && !StaticSound.Playing)
            {
                 StaticSound.Play();
            }
        }
        else
        {
            // STATE B: PREDATOR
            if (Visuals != null) Visuals.Position = Vector2.Zero;
            if (StaticSound != null && StaticSound.Playing) StaticSound.Stop();

            MoveToTarget((float)delta);
        }

        // Apply movement
        MoveAndSlide();
        _isObserved = currentlyObserved;
    }

    private void MoveToTarget(float delta)
    {
        if (NavAgent == null) return;
        
        NavAgent.TargetPosition = _targetPlayer.GlobalPosition;
        
        if (NavAgent.IsNavigationFinished()) return;

        Vector2 currentPos = GlobalPosition;
        Vector2 nextPathPos = NavAgent.GetNextPathPosition();
        
        // Calculate velocity
        float moveSpeed = _targetPlayer.MovementSpeed * SpeedMultiplier;
        Vector2 newVelocity = currentPos.DirectionTo(nextPathPos) * moveSpeed;
        
        if (NavAgent.AvoidanceEnabled)
        {
            NavAgent.Velocity = newVelocity;
        }
        else
        {
            Velocity = newVelocity;
        }
    }

    private bool CheckIfObserved()
    {
        // 1. Screen Check
        if (VisNotifier != null && !VisNotifier.IsOnScreen())
        {
            return false; // Not on screen -> Not observed
        }

        // 2. Angle Check (Is player looking at me?)
        Vector2 playerLookDir = Vector2.Right.Rotated(_targetPlayer.Rotation); // Assuming Right is forward (0 deg)
        // Adjust if player sprite orientation differs. PlayerController.cs uses Rotation = velocity.Angle()
        
        Vector2 toEnemy = (GlobalPosition - _targetPlayer.GlobalPosition).Normalized();
        float dot = playerLookDir.Dot(toEnemy);
        
        if (dot < FovDotThreshold)
        {
            return false; // Player looking away
        }

        // 3. Wall Check (RayCast)
        // DirectSpaceState is expensive, but necessary
        var spaceState = GetWorld2D().DirectSpaceState;
        var query = PhysicsRayQueryParameters2D.Create(
            _targetPlayer.GlobalPosition, 
            GlobalPosition
        );
        // Exclude player and self?
        // query.Exclude = new Godot.Collections.Array<Rid> { _targetPlayer.GetRid(), GetRid() }; 
        // Better: Collision Mask. Walls are typically Layer 1. Player is Layer 1 or 2.
        query.CollisionMask = 1; // Walls Only? Needs setup.

        var result = spaceState.IntersectRay(query);
        
        if (result.Count > 0)
        {
            // Hit *something*. Was it me? 
            // If we hit a wall before me, result.Collider will be wall.
            // However, IntersectRay typically reports the FIRST hit. 
            // If I am further than the wall, ray hits wall. Return false (blocked).
            // If I am closer, ray hits me? Or we exclude self and check for wall.
            
            // If we exclude self from query, and we hit nothing, it means clear LO S?
            // Actually, we want to know if there is a WALL.
            // So we trace to Enemy.Position. If hit exists and is NOT Enemy, it's an obstruction.
            
            Node collider = (Node)result["collider"];
            if (collider != this) // Obstruction
            {
                 // Check if it's the player (improbable since ray starts at player)
                 if (collider == _targetPlayer) return true; // LoS Clear
                 return false; // Wall blocked
            }
        }

        return true;
    }
}

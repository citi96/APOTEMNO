using Godot;
using Apotemno.Actors.Player;

namespace Apotemno.Actors.Enemies;

public partial class FacelessAI : CharacterBody3D
{
    [Export] public float SpeedMultiplier = 1.8f;
    [Export] public float FovDotThreshold = 0.5f; // ~60 degrees cone
    
    [Export] public NavigationAgent3D NavAgent;
    [Export] public VisibleOnScreenNotifier3D VisNotifier;
    [Export] public AudioStreamPlayer3D StaticSound;
    
    // Aesthetic: Jitter the sprite when observed
    [Export] public Node3D Visuals; // Was Node2D

    private PlayerController _targetPlayer;
    private bool _isObserved = false;

    public override void _Ready()
    {
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
            NavAgent.PathDesiredDistance = 2.0f; // 3D distance
            NavAgent.TargetDesiredDistance = 2.0f;
        }

        Callable.From(ActorSetup).CallDeferred();
    }

    private async void ActorSetup()
    {
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_targetPlayer == null) return;

        bool currentlyObserved = CheckIfObserved();

        if (currentlyObserved)
        {
            // STATE A: OBSOLETE / STATUE
            Velocity = Vector3.Zero;
            
            // Effect: Jitter / Glitch
            if (Visuals != null)
            {
                Visuals.Position = new Vector3(
                    (float)GD.RandRange(-0.1f, 0.1f), 
                    0,
                    (float)GD.RandRange(-0.1f, 0.1f)
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
            if (Visuals != null) Visuals.Position = Vector3.Zero;
            if (StaticSound != null && StaticSound.Playing) StaticSound.Stop();

            MoveToTarget((float)delta);
        }

        MoveAndSlide();
        _isObserved = currentlyObserved;
    }

    private void MoveToTarget(float delta)
    {
        if (NavAgent == null) return;
        
        NavAgent.TargetPosition = _targetPlayer.GlobalPosition;
        
        if (NavAgent.IsNavigationFinished()) return;

        Vector3 currentPos = GlobalPosition;
        Vector3 nextPathPos = NavAgent.GetNextPathPosition();
        
        float moveSpeed = _targetPlayer.WalkSpeed * SpeedMultiplier; // Use WalkSpeed
        Vector3 newVelocity = (nextPathPos - currentPos).Normalized() * moveSpeed;
        
        // NavigationAgent3D avoidance
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
            return false;
        }

        // 2. Angle Check (Is player looking at me?)
        // In 3D: Player Forward is -Z (usually) or calculated from Rotation.
        // PlayerController uses RotateY. Forward is Basis.Z * -1 ?
        Vector3 playerLookDir = -_targetPlayer.GlobalTransform.Basis.Z;

        Vector3 toEnemy = (GlobalPosition - _targetPlayer.GlobalPosition).Normalized();
        float dot = playerLookDir.Dot(toEnemy);
        
        if (dot < FovDotThreshold)
        {
            return false; // Player looking away
        }

        // 3. Wall Check (RayCast)
        var spaceState = GetWorld3D().DirectSpaceState;
        var query = PhysicsRayQueryParameters3D.Create(
            _targetPlayer.GlobalPosition + Vector3.Up * 1.5f, // Eye level
            GlobalPosition + Vector3.Up * 1.0f  // Center mass
        );
        // query.CollisionMask = 1; // Walls

        var result = spaceState.IntersectRay(query);
        
        if (result.Count > 0)
        {
            Node collider = (Node)result["collider"];
            if (collider != this)
            {
                 if (collider == _targetPlayer) return true;
                 return false; // Blocked
            }
        }

        return true;
    }
}

using Godot;
using Apotemno.Core;
using Apotemno.Actors.Player.States;

namespace Apotemno.Actors.Player;

[GlobalClass]
public partial class PlayerController : CharacterBody2D
{
    [Export] public float MovementSpeed { get; set; } = 300.0f;
    [Export] public float CrawlSpeedPenalty { get; set; } = 0.4f; // 40% of speed (60% reduction)

    [Export] public Camera2D PlayerCamera { get; set; }
    [Export] public CollisionShape2D PlayerCollider { get; set; }

    private IPlayerState _currentState;
    public InputManagerGlobal InputManager { get; private set; }

    // States
    public NormalState StateNormal { get; private set; }
    public CrawlingState StateCrawl { get; private set; }

    public override void _Ready()
    {
        // Cache InputManager
        InputManager = InputManagerGlobal.Instance;
        if (InputManager == null)
        {
            GD.PushWarning("InputManagerGlobal instance not found! Falling back to direct input if possible, or failing.");
           // Potentially try to find it if it's not setup yet, but it should be Autoload.
        }

        // Initialize States
        StateNormal = new NormalState();
        StateCrawl = new CrawlingState();

        // Default State
        TransitionTo(StateNormal);
    }

    public override void _Process(double delta)
    {
        _currentState?.Update(this, delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        _currentState?.PhysicsUpdate(this, delta);
        MoveAndSlide();
    }

    public void TransitionTo(IPlayerState newState)
    {
        _currentState?.Exit(this);
        _currentState = newState;
        _currentState?.Enter(this);
        
        GD.Print($"[VESSEL] Transitioned to {_currentState.GetType().Name}");
    }
}

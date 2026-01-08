using Godot;
using Apotemno.Core;
using Apotemno.Actors.Player.States;
using Apotemno.Systems.Interaction;

namespace Apotemno.Actors.Player;

[GlobalClass]
public partial class PlayerController : CharacterBody2D
{
    [Export] public float MovementSpeed { get; set; } = 300.0f;
    [Export] public float CrawlSpeedPenalty { get; set; } = 0.4f; // 40% of speed (60% reduction)

    [Export] public Camera2D PlayerCamera { get; set; }
    [Export] public CollisionShape2D PlayerCollider { get; set; }
    [Export] public RayCast2D InteractionRay { get; set; }

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
        
        // PERSISTENCE CHECK: Do we have legs?
        if (SacrificeManagerGlobal.Instance != null)
        {
            if (SacrificeManagerGlobal.Instance.HasSacrificed(SacrificeType.Legs))
            {
                TransitionTo(StateCrawl);
            }
            else
            {
                TransitionTo(StateNormal);
            }
            
            // Listen for future sins
            SacrificeManagerGlobal.Instance.SacrificePerformed += OnMutilationOccurred;
        }
        else
        {
             // Default State
            TransitionTo(StateNormal);
        }
    }

    private void OnMutilationOccurred(int typeInt)
    {
        var type = (SacrificeType)typeInt;
        if (type == SacrificeType.Legs)
        {
            GD.Print("[VESSEL] Legs sacrificed. Collapsing.");
            TransitionTo(StateCrawl);
        }
    }

    public override void _ExitTree()
    {
         if (SacrificeManagerGlobal.Instance != null)
         {
             SacrificeManagerGlobal.Instance.SacrificePerformed -= OnMutilationOccurred;
         }
    }

    public override void _Process(double delta)
    {
        _currentState?.Update(this, delta);
        HandleInteraction();
    }

    private void HandleInteraction()
    {
        // Debug Reload for Persistence Testing
        if (Input.IsKeyPressed(Key.R))
        {
            GD.Print("[DEBUG] Reloading Scene to test Persistence...");
            GetTree().ReloadCurrentScene();
        }

        if (Input.IsActionJustPressed("ui_accept"))
        {
            if (InteractionRay != null)
            {
                if (InteractionRay.IsColliding())
                {
                    var collider = InteractionRay.GetCollider();
                    GD.Print($"[INTERACTION] Ray hit: {((Node)collider).Name}");
                    
                    if (collider is IInteractable interactable && interactable.IsInteractable)
                    {
                        interactable.Interact(this);
                    }
                    else
                    {
                         GD.Print("[INTERACTION] Hit object is not IInteractable or not ready.");
                    }
                }
                else
                {
                    GD.Print("[INTERACTION] RayCast did not hit anything.");
                }
            }
            else
            {
                GD.PrintErr("[INTERACTION] InteractionRay is null!");
            }
        }
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

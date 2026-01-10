using Godot;
using Apotemno.Core;
using Apotemno.Actors.Player.States;
using Apotemno.Systems.Interaction;

namespace Apotemno.Actors.Player;

[GlobalClass]
public partial class PlayerController : CharacterBody3D
{
    [ExportCategory("Movement")]
    [Export] public float WalkSpeed { get; set; } = 5.0f;
    [Export] public float SprintSpeed { get; set; } = 8.0f;
    [Export] public float CrawlSpeedPenalty { get; set; } = 0.4f;
    [Export] public float JumpVelocity { get; set; } = 4.5f;

    [ExportCategory("Camera")]
    [Export] public Camera3D PlayerCamera { get; set; }
    [Export] public float MouseSensitivity { get; set; } = 0.003f;

    [ExportCategory("Visuals")]
    [Export] public Node3D PlayerVisuals { get; set; }
    [Export] public CollisionShape3D PlayerCollider { get; set; }
    [Export] public RayCast3D InteractionRay { get; set; }

    // Stamina
    public float MaxStamina { get; set; } = 100f;
    public float CurrentStamina { get; set; } = 100f;
    public float StaminaDrainRate { get; set; } = 20f;
    public float StaminaRegenRate { get; set; } = 10f;
    public bool IsSprinting { get; private set; }

    // Internal
    private float _gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
    private IPlayerState _currentState;
    public InputBroker InputManager { get; private set; }

    // States
    public NormalState StateNormal { get; private set; }
    public CrawlingState StateCrawl { get; private set; }

    public override void _Ready()
    {
        InputManager = InputBroker.Instance;
        Input.MouseMode = Input.MouseModeEnum.Captured;

        StateNormal = new NormalState();
        StateCrawl = new CrawlingState();

        // Default State
        TransitionTo(StateNormal);
        
        // Setup signals logic if needed (e.g. Sacrifice) - Keeping it minimal for now to ensure 3D works
        if (SacrificeManagerGlobal.Instance != null)
        {
             SacrificeManagerGlobal.Instance.SacrificePerformed += OnMutilationOccurred;
        }
    }

    public override void _ExitTree()
    {
        if (SacrificeManagerGlobal.Instance != null)
        {
             SacrificeManagerGlobal.Instance.SacrificePerformed -= OnMutilationOccurred;
        }
    }

    private void OnMutilationOccurred(int typeInt)
    {
         var type = (SacrificeType)typeInt;
         if (type == SacrificeType.Legs) TransitionTo(StateCrawl);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseMotion)
        {
            // Yaw (Body)
            RotateY(-mouseMotion.Relative.X * MouseSensitivity);
            
            // Pitch (Camera)
            if (PlayerCamera != null)
            {
                PlayerCamera.RotateX(-mouseMotion.Relative.Y * MouseSensitivity);
                Vector3 rot = PlayerCamera.Rotation;
                rot.X = Mathf.Clamp(rot.X, Mathf.DegToRad(-89), Mathf.DegToRad(89));
                PlayerCamera.Rotation = rot;
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        // Apply Gravity
        Vector3 velocity = Velocity;
        if (!IsOnFloor())
        {
            velocity.Y -= _gravity * (float)delta;
        }

        Velocity = velocity; // Update for State

        _currentState?.PhysicsUpdate(this, delta);
        
        // Stamina Logic
        HandleStamina(delta);

        MoveAndSlide();

        HandleInteraction();
    }

    private void HandleStamina(double delta)
    {
        if (IsSprinting && Velocity.Length() > 0.1f)
        {
            CurrentStamina = Mathf.Max(0, CurrentStamina - StaminaDrainRate * (float)delta);
            if (CurrentStamina <= 0) IsSprinting = false;
        }
        else
        {
            CurrentStamina = Mathf.Min(MaxStamina, CurrentStamina + StaminaRegenRate * (float)delta);
        }
    }

    public void TransitionTo(IPlayerState newState)
    {
        _currentState?.Exit(this);
        _currentState = newState;
        _currentState?.Enter(this);
        GD.Print($"[VESSEL] Transitioned to {_currentState.GetType().Name}");
    }

    private void HandleInteraction()
    {
        if (Input.IsActionJustPressed("interact"))
        {
            if (InteractionRay != null && InteractionRay.IsColliding())
            {
                var collider = InteractionRay.GetCollider();
                // 3D Interaction logic would go here. 
                // Need to ensure Interactables are 3D or specific agnostic interfaces.
                GD.Print($"[INTERACTION] Ray hit: {((Node)collider).Name}");
            }
        }
    }
}

using Godot;
using Apotemno.Core;
using Apotemno.Actors.Player.States;
using Apotemno.Systems;
using Apotemno.Systems.Interaction;
using Apotemno.UI;
using Apotemno.Items;

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

    [ExportCategory("Systems")]
    [Export] public HealthManager HealthSystem { get; private set; }
    [Export] public HUDController HUD { get; set; }
    [Export] public Gun EquippedGun { get; set; } // New Gun Reference

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

        // 1. Resolve Gun Reference FIRST (Manual Lookup if needed)
        if (EquippedGun == null)
        {
            GD.PrintErr("[PLAYER] EquippedGun is NULL in _Ready! Attempting manual lookup...");
            EquippedGun = GetNodeOrNull<Gun>("Head/Camera3D/Gun");
            if (EquippedGun != null) GD.Print("[PLAYER] Manual lookup SUCCESS!");
            else GD.PrintErr("[PLAYER] Manual lookup FAILED!");
        }
        else
        {
            GD.Print($"[PLAYER] EquippedGun Linked: {EquippedGun.Name}");
        }

        StateNormal = new NormalState();
        StateCrawl = new CrawlingState();
        
        TransitionTo(StateNormal);
        
        if (HealthSystem != null)
        {
            HealthSystem.Died += OnPlayerDied;
        }

        if (HUD != null)
        {
            HUD.ConnectToPlayer(this);
        }

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

    private void OnPlayerDied()
    {
        GD.Print("[PLAYER] DIED! (Logic TBD - Respawn/Game Over)");
    }
    
    public bool HasRightEye { get; private set; } = true;

    public void ApplyEyeSacrifice()
    {
        if (!HasRightEye) return; // Already lost

        HasRightEye = false;
        GD.Print("[VESSEL] Right Eye Sacrificed. Adjusting Perception...");

        // 1. Reduce Accuracy
        if (EquippedGun != null)
        {
            float penalty = 15.0f; // Significant spread
            EquippedGun.CurrentSpread += penalty;
            GD.Print($"[VESSEL] Gun Spread increased to {EquippedGun.CurrentSpread}");
        }

        // 2. Visual Overlay
        if (HUD != null)
        {
            HUD.EnableRightEyeBlindness();
        }
        else
        {
            GD.PrintErr("[VESSEL] Cannot apply Blindness Overlay: HUD is null!");
        }
    }

    public bool HasSkin { get; private set; } = true;

    // Apply Skin Sacrifice
    public void ApplySkinSacrifice()
    {
        if (!HasSkin) return;
        HasSkin = false;
        GD.Print("[VESSEL] Skin Sacrificed. Vulnerability Doubled. Pain is Constant.");

        // 1. Visual HUD Pain
        HUD?.EnableChronicPainEffect();

        // 2. Haptics (Started here, maintained in Process)
        Input.StartJoyVibration(0, 0.1f, 0.1f, 0.0f); // Kickstart
    }

    // Intercept TakeDamage
    public new void TakeDamage(float amount)
    {
        if (!HasSkin)
        {
            amount *= 2.0f; // "Danno Raddoppiato"
            GD.Print($"[VESSEL] Skinless Agony! Damage doubled to {amount}");
        }
        HealthSystem?.TakeDamage(amount);
    }
    
    // Add Haptics to PhysicsProcess or Process
    public override void _Process(double delta)
    {
         if (!HasSkin)
         {
             // "Feedback Aptico Continuo... vibrazione costante a bassa intensità"
             // Godot's vibration usually needs refreshing or it times out?
             // StartJoyVibration: "If the duration is 0, it vibrates indefinitely." -> Actually duration 0 stops it usually?
             // Documentation says duration in seconds. So we refresh it.
             Input.StartJoyVibration(0, 0.05f, 0.1f, 0.1f); // Weak Rumble
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
            // Report to Nervous System
            InputManager?.RegisterCameraInput(mouseMotion.Relative.Length() * 0.01f); // Scale down roughly to radians/intensity

            RotateY(-mouseMotion.Relative.X * MouseSensitivity);
            
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
        // Weapon Inputs (Polling)
        if (Input.IsActionJustPressed("attack_primary"))
        {
            GD.Print($"[CONTROLLER] Attack Primary Pressed. Gun: {EquippedGun?.Name ?? "NULL"}");
            EquippedGun?.Shoot();
        }

        if (Input.IsActionJustPressed("reload"))
        {
             EquippedGun?.Reload();
        }
        else if (Input.IsKeyPressed(Key.R))
        {
             EquippedGun?.Reload();
        }

        Vector3 velocity = Velocity;
        if (!IsOnFloor())
        {
            velocity.Y -= _gravity * (float)delta;
        }

        Velocity = velocity; // Update for State

        _currentState?.PhysicsUpdate(this, delta);
        
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
                GD.Print($"[INTERACTION] Ray hit: {((Node)collider).Name}");
            }
        }
    }
}

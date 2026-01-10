using Godot;
using System;

namespace Apotemno.Systems;

[GlobalClass]
public partial class HealthManager : Node
{
    // Signals for UI and Player integration
    [Signal] public delegate void HealthChangedEventHandler(float real, float fake);
    [Signal] public delegate void DiedEventHandler();

    public float RealHP { get; private set; } = 100.0f;
    public float FakeHP { get; private set; } = 100.0f;

    // Configuration
    private const float GASLIGHT_DELAY = 1.0f; // Seconds before UI reacts to damage
    private const float RECOVERY_SPEED = 20.0f; // Speed of panic recovery
    private const float DROP_SPEED = 150.0f; // Speed of panic drop

    // Internal State
    private float _delayTimer = 0.0f;
    private bool _isPanicAttack = false;
    private bool _panicDropping = false; // Phase 1 of panic: dropping to 0
    private Random _random = new Random();

    public override void _Ready()
    {
        EmitHealthUpdate();
    }

    public override void _Process(double delta)
    {
        HandleDebugInput();
        UpdateParanoiaLogic((float)delta);
        UpdateCardiacDecay((float)delta);
        // We emit update every frame during paranoia/updates to animate the bar smoothly
        EmitHealthUpdate(); 
    }

    private void HandleDebugInput()
    {
        if (Input.IsKeyPressed(Key.H))
        {
            if (!_hPressed) TakeDamage(10);
            _hPressed = true;
        }
        else _hPressed = false;

        if (Input.IsKeyPressed(Key.J))
        {
            if (!_jPressed) TriggerPanicAttack();
            _jPressed = true;
        }
        else _jPressed = false;
    }

    private bool _hPressed = false;
    private bool _jPressed = false;

    public void TakeDamage(float amount)
    {
        RealHP = Mathf.Clamp(RealHP - amount, 0, 100);
        GD.Print($"[HEALTH] Real Damage Taken. RealHP: {RealHP}");

        if (RealHP <= 0)
        {
            EmitSignal(SignalName.Died);
        }

        // Gaslighting: Don't update target immediately? 
        // 30% chance to slightly INCREASE FakeHP first (Gaslighting)
        if (_random.NextDouble() < 0.3)
        {
            FakeHP = Mathf.Min(FakeHP + 5, 100); 
            GD.Print("[HEALTH] UI Gaslight: FakeHP increased!");
        }

        // Set the delay before FakeHP starts chasing RealHP
        _delayTimer = GASLIGHT_DELAY;
    }

    public void Heal(float amount)
    {
        RealHP = Mathf.Clamp(RealHP + amount, 0, 100);
        // Healing might also be delayed or instant? Let's make it instant for relief.
        FakeHP = RealHP; 
    }

    public void TriggerPanicAttack()
    {
        _isPanicAttack = true;
        _panicDropping = true;
        GD.Print("[HEALTH] PANIC ATTACK INITIATED");
    }

    private void UpdateParanoiaLogic(float delta)
    {
        if (_isPanicAttack)
        {
            // Panic Logic
            if (_panicDropping)
            {
                // Drop to 0 fast
                FakeHP = Mathf.MoveToward(FakeHP, 0, DROP_SPEED * delta);
                if (FakeHP <= 0)
                {
                    _panicDropping = false; // Start recovery
                }
            }
            else
            {
                // Recover back to RealHP (or old Fake Target) slow
                FakeHP = Mathf.MoveToward(FakeHP, RealHP, (RECOVERY_SPEED / 2.0f) * delta);
                if (Mathf.Abs(FakeHP - RealHP) < 1.0f)
                {
                    _isPanicAttack = false; // End panic
                }
            }
        }
        else
        {
            // Normal (but delayed) Logic
            if (_delayTimer > 0)
            {
                _delayTimer -= delta;
                return; // FREEZE the UI
            }

            // After delay, slowly interpolate towards RealHP
            if (FakeHP != RealHP)
            {
                float catchUpSpeed = RECOVERY_SPEED;
                
                // If we need to go down, go down slowly.
                if (FakeHP > RealHP) catchUpSpeed = RECOVERY_SPEED * 0.5f; // Slow drop

                FakeHP = Mathf.MoveToward(FakeHP, RealHP, catchUpSpeed * delta);
            }
        }
    }

    public float MaxHP { get; private set; } = 100.0f;

    // Blood Sacrifice: Reduce Max HP
    public void ApplyBloodSacrifice()
    {
        MaxHP *= 0.9f; // -10%
        if (RealHP > MaxHP) RealHP = MaxHP;
        GD.Print($"[HEALTH] Blood Sacrificed. MaxHP reduced to {MaxHP}");
        EmitHealthUpdate();
    }

    // Heart Sacrifice: Decay and No Regen
    private bool _cardiacArrest = false;
    public void EnableCardiacArrest()
    {
        _cardiacArrest = true;
        // "La salute scende a zero... non può più rigenerarsi"
        // We set to 1 to allow a final moment, or let decay kill them.
        GD.Print("[HEALTH] CARDIAC ARREST. Heart Sacrificed. Vitality fading...");
    }

    private void EmitHealthUpdate()
    {
        EmitSignal(SignalName.HealthChanged, RealHP, FakeHP);
    }

    // Update Logic for Decay
    private void UpdateCardiacDecay(float delta)
    {
        if (_cardiacArrest && RealHP > 0)
        {
            // Slow decay
            RealHP -= delta * 0.5f; // 1 HP every 2 seconds
            if (RealHP <= 0)
            {
                RealHP = 0;
                EmitSignal(SignalName.Died);
            }
        }
    }
}

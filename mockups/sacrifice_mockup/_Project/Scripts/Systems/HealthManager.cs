using Godot;
using System;

namespace Apotemno.Systems;

[GlobalClass]
public partial class HealthManager : Node
{
    [Export]
    public ProgressBar DisplayedBar; // The lying bar

    [Export]
    public ProgressBar TrueBar; // The truth (debug)

    public float RealHP { get; private set; } = 100.0f;
    public float FakeHP { get; private set; } = 100.0f;

    // Configuration
    private const float GASLIGHT_DELAY = 1.0f; // Seconds before UI reacts to damage
    private const float RECOVERY_SPEED = 20.0f; // Speed of panic recovery
    private const float DROP_SPEED = 150.0f; // Speed of panic drop

    // Internal State
    private float _delayTimer = 0.0f;
    private float _targetFakeHP = 100.0f;
    private bool _isPanicAttack = false;
    private bool _panicDropping = false; // Phase 1 of panic: dropping to 0
    private Random _random = new Random();

    public override void _Ready()
    {
        UpdateBars();
    }

    public override void _Process(double delta)
    {
        HandleDebugInput();
        UpdateParanoiaLogic((float)delta);
        UpdateBars();
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

        // Gaslighting: Don't update target immediately? 
        // Logic: specific request "When RealHP drops, FakeHP doesn't drop immediately... or goes up slightly"
        
        // 30% chance to slightly INCREASE FakeHP first (Gaslighting)
        if (_random.NextDouble() < 0.3)
        {
            FakeHP = Mathf.Min(FakeHP + 5, 100); 
            GD.Print("[HEALTH] UI Gaslight: FakeHP increased!");
        }

        // Set the delay before FakeHP starts chasing RealHP
        _delayTimer = GASLIGHT_DELAY;
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
                return; // FREEZE the UI (or let it stay at current gaslit value)
            }

            // After delay, slowly interpolate towards RealHP
            // "Scende troppo lentamente"
            if (FakeHP != RealHP)
            {
                float catchUpSpeed = RECOVERY_SPEED;
                
                // If we need to go down, go down slowly. If we need to go up (healing?), go normal.
                if (FakeHP > RealHP) catchUpSpeed = RECOVERY_SPEED * 0.5f; // Slow drop

                FakeHP = Mathf.MoveToward(FakeHP, RealHP, catchUpSpeed * delta);
            }
        }
    }

    private void UpdateBars()
    {
        if (DisplayedBar != null) DisplayedBar.Value = FakeHP;
        if (TrueBar != null) TrueBar.Value = RealHP;
    }
}

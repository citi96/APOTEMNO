using Godot;
using System;

namespace Apotemno.Core;

[GlobalClass]
public partial class InputBroker : Node
{
    public static InputBroker Instance { get; private set; }

    // State Flags
    public bool Inverted { get; set; } = false;
    public bool Unreliable { get; set; } = false;
    public bool Spasmodic { get; set; } = false;

    private Random _random = new Random();
    
    // Spasm internal state
    private Vector2 _currentSpasm = Vector2.Zero;
    private double _spasmTimer = 0;
    private const double SPASM_DURATION = 0.2;
    private const double SPASM_INTERVAL_MIN = 0.5;
    private const double SPASM_INTERVAL_MAX = 2.0;
    private double _nextSpasmTime = 1.0;

    // Unreliable internal state
    private bool _dropInputPacket = false;
    private double _unreliableTimer = 0;
    private const double UNRELIABLE_INTERVAL = 0.1; // Check every 100ms

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _Process(double delta)
    {
        HandleDebugToggles();
        UpdateChaosLogic(delta);
    }

    private void HandleDebugToggles()
    {
        if (Input.IsKeyPressed(Key.F1)) 
        {
            if (!_f1Pressed) { Inverted = !Inverted; GD.Print($"[NERVOUS SYSTEM] Inverted: {Inverted}"); }
            _f1Pressed = true;
        }
        else _f1Pressed = false;

        if (Input.IsKeyPressed(Key.F2))
        {
            if (!_f2Pressed) { Unreliable = !Unreliable; GD.Print($"[NERVOUS SYSTEM] Unreliable: {Unreliable}"); }
            _f2Pressed = true;
        }
        else _f2Pressed = false;

        if (Input.IsKeyPressed(Key.F3))
        {
            if (!_f3Pressed) { Spasmodic = !Spasmodic; GD.Print($"[NERVOUS SYSTEM] Spasmodic: {Spasmodic}"); }
            _f3Pressed = true;
        }
        else _f3Pressed = false;
    }

    private bool _f1Pressed = false;
    private bool _f2Pressed = false;
    private bool _f3Pressed = false;

    private void UpdateChaosLogic(double delta)
    {
        // Spasmodic Logic
        if (Spasmodic)
        {
            _spasmTimer += delta;
            if (_spasmTimer >= _nextSpasmTime)
            {
                // Trigger a spasm
                float angle = (float)(_random.NextDouble() * Math.PI * 2);
                _currentSpasm = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                
                // Reset timer
                _spasmTimer = 0;
                _nextSpasmTime = SPASM_DURATION + _random.NextDouble() * (SPASM_INTERVAL_MAX - SPASM_INTERVAL_MIN) + SPASM_INTERVAL_MIN;
            }
            else if (_spasmTimer > SPASM_DURATION)
            {
                // End of spasm duration
                _currentSpasm = Vector2.Zero;
            }
        }
        else
        {
            _currentSpasm = Vector2.Zero;
        }

        // Unreliable Logic
        if (Unreliable)
        {
            _unreliableTimer += delta;
            if (_unreliableTimer > UNRELIABLE_INTERVAL)
            {
                _unreliableTimer = 0;
                // 30% chance to drop input
                _dropInputPacket = _random.NextDouble() < 0.3;
            }
        }
    }

    public Vector2 GetMoveVector()
    {
        // 1. Base Input
        Vector2 input = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");

        // 2. Unreliable (Input Lag/Loss)
        if (Unreliable && _dropInputPacket)
        {
            // Return zero or keep previous input? 
            // "Si perde per strada" -> Zero.
            input = Vector2.Zero; 
        }

        // 3. Inverted
        if (Inverted)
        {
            input *= -1;
        }

        // 4. Spasmodic (Additive or Override?)
        // "Il personaggio si muove da solo". 
        // We add the spasm vector. If it's strong enough it calls movement.
        if (Spasmodic && _currentSpasm != Vector2.Zero)
        {
            input += _currentSpasm;
            // Clamping to avoid super speed
            if (input.Length() > 1) input = input.Normalized();
        }

        return input;
    }
}

using Godot;
using System;

namespace Apotemno.Core;

[GlobalClass]
public partial class InputManagerGlobal : Node
{
    public static InputManagerGlobal Instance { get; private set; }

    // Events for UI and Systems to react to corruption changes
    [Signal] public delegate void InputStateChangedEventHandler(bool inverted, bool unreliable, bool spasmodic);

    // State Flags with backing fields to trigger events
    private bool _inverted;
    public bool Inverted
    {
        get => _inverted;
        set
        {
            if (_inverted != value)
            {
                _inverted = value;
                EmitSignal(SignalName.InputStateChanged, _inverted, _unreliable, _spasmodic);
            }
        }
    }

    private bool _unreliable;
    public bool Unreliable
    {
        get => _unreliable;
        set
        {
            if (_unreliable != value)
            {
                _unreliable = value;
                EmitSignal(SignalName.InputStateChanged, _inverted, _unreliable, _spasmodic);
            }
        }
    }

    private bool _spasmodic;
    public bool Spasmodic
    {
        get => _spasmodic;
        set
        {
            if (_spasmodic != value)
            {
                _spasmodic = value;
                EmitSignal(SignalName.InputStateChanged, _inverted, _unreliable, _spasmodic);
            }
        }
    }

    private Random _random = new Random();
    private readonly object _lock = new object();

    // Internal logic states
    private Vector2 _currentSpasm = Vector2.Zero;
    private double _spasmTimer = 0;
    private double _nextSpasmTime = 1.0;
    private const double SPASM_DURATION_BASE = 0.2;
    private const double SPASM_INTERVAL_MIN = 0.5;
    private const double SPASM_INTERVAL_MAX = 2.0;

    private bool _dropInputPacket = false;
    private double _unreliableTimer = 0;
    private const double UNRELIABLE_CHECK_INTERVAL = 0.1;

    public override void _EnterTree()
    {
        lock (_lock)
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                QueueFree();
            }
        }
    }

    public override void _Process(double delta)
    {
        UpdateChaosLogic(delta);
        HandleDebugToggles();
    }

    private void HandleDebugToggles()
    {
        if (Input.IsKeyPressed(Key.F1) && !IsF1Pressed)
        {
            Inverted = !Inverted; 
            GD.Print($"[NERVOUS SYSTEM] Inverted: {Inverted}");
        }
        IsF1Pressed = Input.IsKeyPressed(Key.F1);

        if (Input.IsKeyPressed(Key.F2) && !IsF2Pressed)
        {
            Unreliable = !Unreliable;
            GD.Print($"[NERVOUS SYSTEM] Unreliable: {Unreliable}");
        }
        IsF2Pressed = Input.IsKeyPressed(Key.F2);

        if (Input.IsKeyPressed(Key.F3) && !IsF3Pressed)
        {
            Spasmodic = !Spasmodic;
            GD.Print($"[NERVOUS SYSTEM] Spasmodic: {Spasmodic}");
        }
        IsF3Pressed = Input.IsKeyPressed(Key.F3);
    }
    private bool IsF1Pressed;
    private bool IsF2Pressed;
    private bool IsF3Pressed;

    private void UpdateChaosLogic(double delta)
    {
        // Spasmodic Logic
        if (Spasmodic)
        {
            _spasmTimer += delta;
            if (_spasmTimer >= _nextSpasmTime)
            {
                float angle = (float)(_random.NextDouble() * Math.PI * 2);
                _currentSpasm = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                _spasmTimer = 0;
                _nextSpasmTime = SPASM_DURATION_BASE + _random.NextDouble() * (SPASM_INTERVAL_MAX - SPASM_INTERVAL_MIN) + SPASM_INTERVAL_MIN;
            }
            else if (_spasmTimer > SPASM_DURATION_BASE)
            {
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
            if (_unreliableTimer > UNRELIABLE_CHECK_INTERVAL)
            {
                _unreliableTimer = 0;
                _dropInputPacket = _random.NextDouble() < 0.3; // 30% packet loss
            }
        }
        else
        {
            _dropInputPacket = false;
        }
    }

    public Vector2 GetMovementVector()
    {
        Vector2 input = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");

        if (Unreliable && _dropInputPacket)
        {
            return Vector2.Zero;
        }

        if (Inverted)
        {
            input *= -1;
        }

        if (Spasmodic && _currentSpasm != Vector2.Zero)
        {
            input += _currentSpasm;
            if (input.Length() > 1) input = input.Normalized();
        }

        return input;
    }
}

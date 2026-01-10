using Godot;
using System;

namespace Apotemno.Core;

[GlobalClass]
/// <summary>
/// Central Input Broker for managing player input and injecting "Nervous System" effects (Inverted, Unreliable, Spasmodic).
/// Functions as a thread-safe Singleton (though Godot Input API is Main Thread only).
/// </summary>
public partial class InputBroker : Node
{
    private static InputBroker _instance;
    private static readonly object _instanceLock = new object();
    
    public static InputBroker Instance 
    { 
        get 
        {
            lock (_instanceLock)
            {
                return _instance;
            }
        }
        private set
        {
            lock (_instanceLock)
            {
                _instance = value;
            }
        }
    }

    // Events for UI and Systems to react to corruption changes
    [Signal] public delegate void InputStateChangedEventHandler(bool inverted, bool unreliable, bool spasmodic);

    // Locks for thread-safe property access
    private readonly object _stateLock = new object();

    // State Flags with backing fields to trigger events
    private bool _inverted;
    public bool Inverted
    {
        get { lock (_stateLock) return _inverted; }
        set
        {
            bool changed = false;
            lock (_stateLock)
            {
                if (_inverted != value)
                {
                    _inverted = value;
                    changed = true;
                }
            }
            if (changed) EmitSignal(SignalName.InputStateChanged, Inverted, Unreliable, Spasmodic);
        }
    }

    private bool _unreliable;
    public bool Unreliable
    {
        get { lock (_stateLock) return _unreliable; }
        set
        {
            bool changed = false;
            lock (_stateLock)
            {
                if (_unreliable != value)
                {
                    _unreliable = value;
                    changed = true;
                }
            }
            if (changed) EmitSignal(SignalName.InputStateChanged, Inverted, Unreliable, Spasmodic);
        }
    }

    private bool _spasmodic;
    public bool Spasmodic
    {
        get { lock (_stateLock) return _spasmodic; }
        set
        {
            bool changed = false;
            lock (_stateLock)
            {
                if (_spasmodic != value)
                {
                    _spasmodic = value;
                    changed = true;
                }
            }
            if (changed) EmitSignal(SignalName.InputStateChanged, Inverted, Unreliable, Spasmodic);
        }
    }

    private Random _random = new Random();
    
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
        lock (_instanceLock)
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else
            {
                QueueFree();
            }
        }
    }

    public override void _Process(double delta)
    {
        // Internal logic states update - mostly safe to do in _Process (Main Thread)
        // If we wanted full thread safety for logic, we'd lock specific logic updates too,
        // but _Process is guaranteed main thread.
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
        bool isSpasmodic = Spasmodic; // Local copy for consistent logic loop
        bool isUnreliable = Unreliable;

        // Spasmodic Logic
        if (isSpasmodic)
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
        if (isUnreliable)
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
        // Use new Action names
        Vector2 input = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");

        if (Unreliable && _dropInputPacket)
        {
            return Vector2.Zero;
        }

        if (Inverted)
        {
            input *= -1;
        }

        // Check spasms
        Vector2 spasm = _currentSpasm; // Copy
        if (Spasmodic && spasm != Vector2.Zero)
        {
            input += spasm;
            if (input.Length() > 1) input = input.Normalized();
        }

        return input;
    }

    /// <summary>
    /// Triggers gamepad rumble/haptic feedback.
    /// </summary>
    /// <param name="weakMagnitude">Low-frequency motor (0.0 to 1.0)</param>
    /// <param name="strongMagnitude">High-frequency motor (0.0 to 1.0)</param>
    /// <param name="duration">Duration in seconds (0 for indefinite)</param>
    /// <param name="device">Joypad device ID (default 0 for player 1)</param>
    public void TriggerRumble(float weakMagnitude, float strongMagnitude, float duration = 0.5f, int device = 0)
    {
        Input.StartJoyVibration(device, weakMagnitude, strongMagnitude, duration);
    }

    /// <summary>
    /// Stops all rumble on the specified device.
    /// </summary>
    public void StopRumble(int device = 0)
    {
        Input.StopJoyVibration(device);
    }
}

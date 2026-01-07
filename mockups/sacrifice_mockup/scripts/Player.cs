using Godot;
using System;

namespace SacrificeMockup;

[GlobalClass]
public partial class Player : CharacterBody2D
{
    public const float Speed = 300.0f;


    public override void _PhysicsProcess(double delta)
    {
        Vector2 velocity = Velocity;

        // Get the input direction from the Rotten Nervous System
        Vector2 direction = Vector2.Zero;
        
        if (InputBroker.Instance != null)
        {
            direction = InputBroker.Instance.GetMoveVector();
        }
        else
        {
            // Fallback if broker is missing
            direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
        }

        if (direction != Vector2.Zero)
        {
            velocity = direction * Speed;
        }
        else
        {
            velocity = velocity.MoveToward(Vector2.Zero, Speed);
        }

        Velocity = velocity;
        MoveAndSlide();
    }
}

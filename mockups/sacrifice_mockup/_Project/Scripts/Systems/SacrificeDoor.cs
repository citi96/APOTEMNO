using Godot;
using Apotemno.Core;

namespace Apotemno.Systems;

[GlobalClass]
public partial class SacrificeDoor : StaticBody2D
{
    [Export]
    public SacrificeType RequiredSacrifice = SacrificeType.None;

    [Export]
    public Node2D Visuals;

    [Export]
    public CollisionShape2D Collision;

    public override void _Ready()
    {
        var manager = SacrificeManagerGlobal.Instance;
        if (manager != null)
        {
            // Initial Check (Persistence)
            if (manager.HasSacrificed(RequiredSacrifice))
            {
                OpenDoor();
            }
            else
            {
                // Listen for runtime sacrifice
                manager.SacrificePerformed += OnSacrificePerformed;
            }
        }
        else
        {
             // Fallback if no manager (open/closed default?)
        }
    }

    private void OnSacrificePerformed(int typeInt)
    {
        GD.Print($"[DOOR] Event Received: {typeInt}. Required: {(int)RequiredSacrifice}");
        if ((SacrificeType)typeInt == RequiredSacrifice)
        {
            OpenDoor();
        }
    }

    private void OpenDoor()
    {
        // Simple "Open" = Disable + Hide
        if (Visuals != null) Visuals.Visible = false;
        if (Collision != null) Collision.SetDeferred("disabled", true);
        
        // Optional: Play sound or animation?
        GD.Print($"[DOOR] Sacrifice {RequiredSacrifice} accepted. Door opened.");
        
        // Clean up listener
        if (SacrificeManagerGlobal.Instance != null)
        {
             SacrificeManagerGlobal.Instance.SacrificePerformed -= OnSacrificePerformed;
        }
    }
    
    public override void _ExitTree()
    {
         if (SacrificeManagerGlobal.Instance != null)
         {
             SacrificeManagerGlobal.Instance.SacrificePerformed -= OnSacrificePerformed;
         }
    }
}

using Godot;
using System;

namespace Apotemno.Systems;

[GlobalClass]
public partial class Portal : Node2D
{
    [Export]
    public Node2D Destination; 

    [Export]
    public Camera2D PortalCamera;

    [Export]
    public Node2D PlayerCamera; 

    [Export]
    public Area2D TeleportArea;

    public override void _Ready()
    {
        // 1. Teleport Setup
        if (TeleportArea != null)
        {
            TeleportArea.BodyEntered += OnBodyEntered;
        }

        // 2. Viewport & Sprite Linking
        var subViewport = GetNodeOrNull<SubViewport>("SubViewport");
        var portalSprite = GetNodeOrNull<Sprite2D>("PortalSprite");

        if (subViewport != null && PortalCamera != null && portalSprite != null)
        {
            // A. Link Camera
            PortalCamera.CustomViewport = subViewport;
            PortalCamera.Enabled = true; 
            PortalCamera.Zoom = new Vector2(0.5f, 0.5f); // Zoom out to ensure we see the floor
            
            // HYBRID FIX: Share World2D even with CustomViewport (Godot quirk?)
            subViewport.World2D = GetViewport().World2D;

            // B. Link Sprite Texture
            portalSprite.Texture = subViewport.GetTexture();
            
            // C. Visual Fixes
            portalSprite.ZIndex = 10; // Ensure it draws on top of floor
            portalSprite.Modulate = Colors.White; // Reset any debug colors
            
            GD.Print($"[PORTAL] {Name}: Ready. Camera Linked. Texture Assigned.");
        }
    }

    private static ulong _lastTeleportTime = 0;
    private const ulong TELEPORT_COOLDOWN_MS = 200;

    public override void _Process(double delta)
    {
        if (PortalCamera == null) return;
        
        // FAILSAFE DEBUGGING
        if (Destination == null)
        {
            if (Engine.GetFramesDrawn() % 60 == 0) GD.PrintErr($"[PORTAL] {Name}: DISTINATION IS NULL! Linker failed.");
            return;
        }

        if (PlayerCamera == null)
        {
             // Try to find player camera dynamically if missing
             var player = GetTree().GetFirstNodeInGroup("Player"); // Assuming player is in group, or just find by name
             if (player != null) PlayerCamera = player.GetNodeOrNull<Camera2D>("Camera2D");
             return;
        }

        // Visual Sync Logic
        Vector2 relativePos = PlayerCamera.GlobalPosition - this.GlobalPosition;
        PortalCamera.GlobalPosition = Destination.GlobalPosition + relativePos;
        
        // FORCE UPDATE (Fix for Black Texture in Godot 4.x)
        var sv = GetNodeOrNull<SubViewport>("SubViewport");
        if (sv != null)
        {
            sv.RenderTargetUpdateMode = SubViewport.UpdateMode.Always;
        }

        // HARD DATA
        if (Engine.GetFramesDrawn() % 60 == 0)
        {
             GD.Print($"[PORTAL] {Name} CamPos: {PortalCamera.GlobalPosition} | Dest: {Destination.Name} | S/V Tex Valid: {sv?.GetTexture() != null} | Sprite Tex Valid: {GetNode<Sprite2D>("PortalSprite").Texture != null}"); 
        }
    }

    private void OnBodyEntered(Node body)
    {
        if (body is Player player && Destination != null)
        {
            ulong now = Time.GetTicksMsec();
            if (now - _lastTeleportTime < TELEPORT_COOLDOWN_MS) return;

            GD.Print($"[PORTAL] Teleporting {body.Name} to {Destination.Name}");
            Vector2 entryOffset = player.GlobalPosition - this.GlobalPosition;
            player.GlobalPosition = Destination.GlobalPosition + entryOffset;
            _lastTeleportTime = now;
        }
    }
}

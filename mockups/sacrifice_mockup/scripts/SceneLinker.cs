using Godot;
using System;

namespace SacrificeMockup;

[GlobalClass]
public partial class SceneLinker : Node
{
    [Export] public Portal PortalA;
    [Export] public Portal PortalB;
    [Export] public Camera2D PlayerCamera;

    public override void _Ready()
    {
        // Allow a frame for things to settle if needed, but usually _Ready is fine.
        // We defer call just to be safe vs Order of Operations.
        CallDeferred(nameof(ConnectPortals));
    }

    private void ConnectPortals()
    {
        if (PortalA != null && PortalB != null)
        {
            // Link A -> B
            PortalA.Destination = PortalB;
            PortalA.PlayerCamera = PlayerCamera;
            
            // Link B -> A
            PortalB.Destination = PortalA;
            PortalB.PlayerCamera = PlayerCamera;

            // FORCE PLAYER CAMERA DOMINANCE
            if (PlayerCamera != null)
            {
                PlayerCamera.Enabled = true;
                PlayerCamera.MakeCurrent();
                GD.Print("[SCENE LINKER] Player Camera set to CURRENT.");
            }

            GD.Print("[SCENE LINKER] Portals Connected Successfully (C#).");
        }
        else
        {
            GD.PrintErr($"[SCENE LINKER] Missing References! A: {PortalA}, B: {PortalB}");
        }
    }
}

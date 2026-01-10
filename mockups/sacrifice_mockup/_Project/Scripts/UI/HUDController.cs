using Godot;
using Apotemno.Actors.Player;
using Apotemno.Systems;

namespace Apotemno.UI;

public partial class HUDController : Control
{
    [Export] public ProgressBar RealHealthBar;
    [Export] public ProgressBar FakeHealthBar;
    [Export] public Label AmmoLabel;

    public void ConnectToPlayer(PlayerController player)
    {
        if (player.HealthSystem != null)
        {
            player.HealthSystem.HealthChanged += OnHealthChanged;
            OnHealthChanged(player.HealthSystem.RealHP, player.HealthSystem.FakeHP);
        }

        if (player.EquippedGun != null)
        {
            player.EquippedGun.AmmoChanged += OnAmmoChanged;
            // Init
            OnAmmoChanged(player.EquippedGun.CurrentAmmo, player.EquippedGun.MaxAmmo);
        }
    }

    private void OnHealthChanged(float real, float fake)
    {
        if (RealHealthBar != null) RealHealthBar.Value = real;
        if (FakeHealthBar != null) FakeHealthBar.Value = fake;
    }

    private void OnAmmoChanged(int current, int max)
    {
        if (AmmoLabel != null)
        {
            AmmoLabel.Text = $"{current} / {max}";
        }
    }

    public void EnableRightEyeBlindness()
    {
        // specific request: "campo visivo dimezzato: oscura o sfoca permanentemente metà dello schermo (il lato destro)"
        // Create a ColorRect dynamically
        ColorRect blindOverlay = new ColorRect();
        blindOverlay.Name = "RightEyeBlindness";
        blindOverlay.Color = new Color(0, 0, 0, 0.95f); // Almost pitch black
        
        // Position on Right Half
        blindOverlay.LayoutMode = 1; // Anchors
        blindOverlay.AnchorsPreset = (int)LayoutPreset.RightWide; // or manual
        
        // Manual anchor for Right Half (0.5 to 1.0)
        blindOverlay.AnchorLeft = 0.5f;
        blindOverlay.AnchorRight = 1.0f;
        blindOverlay.AnchorTop = 0.0f;
        blindOverlay.AnchorBottom = 1.0f;
        
        // Ensure it's on top of health bars but below menus (HUD usually implies low Z)
        // If HUD is a CanvasLayer, this works.
        // We'll add it as a child of this HUD Control.
        AddChild(blindOverlay); 
        
        GD.Print("[HUD] Right Eye Blindness Applied.");
    }
}

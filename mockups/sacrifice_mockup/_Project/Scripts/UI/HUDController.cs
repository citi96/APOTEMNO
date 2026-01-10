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
    private ColorRect _chronicPainOverlay;
    private float _painPulseTime;

    public void EnableChronicPainEffect()
    {
        // "Effetto Schermo Rosso: attiva un overlay visivo di dolore costante"
        _chronicPainOverlay = new ColorRect();
        _chronicPainOverlay.Name = "ChronicPainOverlay";
        _chronicPainOverlay.Color = new Color(0.8f, 0, 0, 0.0f); // Red, starts invisible
        _chronicPainOverlay.LayoutMode = 1;
        _chronicPainOverlay.AnchorsPreset = (int)LayoutPreset.FullRect;
        _chronicPainOverlay.MouseFilter = MouseFilterEnum.Ignore; // Crucial

        AddChild(_chronicPainOverlay);
        GD.Print("[HUD] Chronic Pain Effect Applied.");
    }

    public override void _Process(double delta)
    {
        if (_chronicPainOverlay != null)
        {
            _painPulseTime += (float)delta * 2.0f; // Speed of pulse
            // Sin wave from 0.05 to 0.2 alpha (subtle but visible)
            float alpha = 0.05f + (Mathf.Sin(_painPulseTime) + 1.0f) * 0.075f; 
            _chronicPainOverlay.Color = new Color(0.8f, 0, 0, alpha);
        }
    }
}

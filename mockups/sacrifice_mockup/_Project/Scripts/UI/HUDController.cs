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
}

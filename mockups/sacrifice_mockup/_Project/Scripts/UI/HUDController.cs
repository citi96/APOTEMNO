using Godot;
using Apotemno.Actors.Player;
using Apotemno.Systems;

namespace Apotemno.UI;

public partial class HUDController : Control
{
    [Export] public ProgressBar RealHealthBar;
    [Export] public ProgressBar FakeHealthBar;

    public void ConnectToPlayer(PlayerController player)
    {
        if (player.HealthSystem != null)
        {
            player.HealthSystem.HealthChanged += OnHealthChanged;
            // Initialize
            OnHealthChanged(player.HealthSystem.RealHP, player.HealthSystem.FakeHP);
        }
    }

    private void OnHealthChanged(float real, float fake)
    {
        if (RealHealthBar != null) RealHealthBar.Value = real;
        if (FakeHealthBar != null) FakeHealthBar.Value = fake;
    }
}

using Godot;
using Apotemno.Core;
using Apotemno.Actors.Player;

namespace Apotemno.Systems.Interaction;

[GlobalClass]
public partial class SacrificeAltar : Node2D, IInteractable
{
    [Export]
    public SacrificeType SacrificeToPerform = SacrificeType.None;

    [Export]
    public Color AvailableColor = Colors.Red;

    [Export]
    public Color DepletedColor = Colors.Gray;

    [Export]
    public Sprite2D AltarSprite; // Assign in editor or find child

    public bool IsInteractable 
    {
        get 
        {
            // Can only interact if we haven't sacrificed this part yet
            if (SacrificeManagerGlobal.Instance == null) return false;
            return !SacrificeManagerGlobal.Instance.HasSacrificed(SacrificeToPerform);
        }
    }

    public override void _Ready()
    {
        // Initial visual update (give usage a moment to settle)
        GetTree().CreateTimer(0.1f).Timeout += UpdateVisuals;
    }

    public void Interact(PlayerController player)
    {
        if (!IsInteractable) return;

        GD.Print($"[ALTAR] Player interacts. Demanding: {SacrificeToPerform}");
        
        if (SacrificeManagerGlobal.Instance != null)
        {
            SacrificeManagerGlobal.Instance.PerformSacrifice(SacrificeToPerform);
            UpdateVisuals();
        }
    }

    public string GetInteractionPrompt()
    {
        return IsInteractable ? $"Sacrifice {SacrificeToPerform}" : "Empty Altar";
    }

    private void UpdateVisuals()
    {
        if (AltarSprite == null) return;

        if (IsInteractable)
        {
            AltarSprite.Modulate = AvailableColor;
        }
        else
        {
            AltarSprite.Modulate = DepletedColor;
        }
    }
}

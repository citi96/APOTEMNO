using Apotemno.Actors.Player;

namespace Apotemno.Systems.Interaction;

public interface IInteractable
{
    // Can this object currently be interacted with?
    bool IsInteractable { get; }

    // Execute the interaction
    void Interact(PlayerController player);
    
    // Optional: Prompt text to show on UI?
    string GetInteractionPrompt();
}

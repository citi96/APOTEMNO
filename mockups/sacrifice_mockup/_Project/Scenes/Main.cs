using Godot;
using Apotemno.Core;

public partial class Main : Node
{
    public override void _Ready()
    {
        // Defer the game start to ensure all Autoloads are fully initialized
        // and the tree is stable.
        CallDeferred(nameof(BootStrap));
    }

    private void BootStrap()
    {
        GD.Print("[Main] Bootstrapping...");
        
        // Direct to GameManager to start the game loop.
        // In a full game, this might check for a saved game or show a Main Menu first.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartNewGame();
        }
        else
        {
            GD.PrintErr("[Main] GameManager Instance is null!");
        }
    }
}

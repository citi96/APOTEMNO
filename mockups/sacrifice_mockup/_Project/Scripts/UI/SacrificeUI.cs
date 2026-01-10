using Godot;
using Apotemno.Core;
using System.Collections.Generic;

namespace Apotemno.UI;

public partial class SacrificeUI : CanvasLayer
{
    [Export] public Control MenuContainer { get; set; }
    [Export] public Control OptionsListContainer { get; set; } // VBoxContainer
    [Export] public PackedScene ButtonTemplate { get; set; } 
    
    private bool _isOpen = false;

    public override void _Ready()
    {
        if (MenuContainer != null) MenuContainer.Visible = false;
        ProcessMode = ProcessModeEnum.Always; 
    }

    public override void _Input(InputEvent @event)
    {
        // Use Action "sacrifice_menu" (Mapped to K)
        if (@event.IsActionPressed("sacrifice_menu") && !@event.IsEcho()) 
        {
            GD.Print("[UI] Input Received: sacrifice_menu (K)");
            ToggleMenu();
        }
    }

    private void ToggleMenu()
    {
        if (_isOpen)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    private void Open()
    {
        GD.Print("[UI] Open() Method Called."); 

        // Fallback: If Export failed, try dynamic lookup
        if (MenuContainer == null)
        {
            MenuContainer = GetNodeOrNull<Control>("Control");
        }

        if (MenuContainer == null) 
        {
            GD.PrintErr("[UI] CRITICAL: MenuContainer is Null. Cannot open menu."); 
            return; 
        }
        
        try
        {
            _isOpen = true; // Sync State
            GetTree().Paused = true;
            MenuContainer.Visible = true;
            Input.MouseMode = Input.MouseModeEnum.Visible;
            PopulateOptions();
        }
        catch (System.Exception e)
        {
            GD.PrintErr($"[UI] Error: {e.Message}");
            _isOpen = false; // Revert on failure
        }
    }

    private void Close()
    {
        GD.Print("[UI] Closing Sacrifice Menu...");
        if (MenuContainer == null) return;
        
        _isOpen = false; // Sync State
        MenuContainer.Visible = false;
        GetTree().Paused = false;
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    private void PopulateOptions()
    {
        // Fallback for broken Export
        if (OptionsListContainer == null)
        {
             GD.PrintErr("[UI] OptionsListContainer (Export) was null. Attempting dynamic lookup...");
             // Path based on Scene Structure: Control -> CenterContainer -> VBoxContainer -> ScrollContainer -> OptionList
             OptionsListContainer = MenuContainer.GetNodeOrNull<Control>("CenterContainer/VBoxContainer/ScrollContainer/OptionList");
        }

        if (OptionsListContainer == null) { GD.PrintErr("[UI] OptionsListContainer is STILL Null. Cannot populate."); return; }
        
        GD.Print("[UI] Populating Options...");

        foreach(Node child in OptionsListContainer.GetChildren()) child.QueueFree();

        if (SacrificeManagerGlobal.Instance == null) { GD.PrintErr("[UI] SacrificeManagerGlobal Null"); return; }

        var available = SacrificeManagerGlobal.Instance.GetAvailableSacrifices();
        GD.Print($"[UI] Found {available.Count} available sacrifices.");

        if (available.Count == 0)
        {
            Label empty = new Label();
            empty.Text = "No more flesh to give.";
            OptionsListContainer.AddChild(empty);
            return;
        }

        foreach(var opt in available)
        {
            Button btn = new Button();
            btn.Text = $"{opt.DisplayName} [IRREVERSIBLE]";
            btn.TooltipText = opt.Description;
            btn.Alignment = HorizontalAlignment.Left;
            btn.CustomMinimumSize = new Vector2(0, 40);
            
            var type = opt.Type; 
            btn.Pressed += () => OnSacrificeSelected(type);
            
            OptionsListContainer.AddChild(btn);
        }
    }

    private void OnSacrificeSelected(SacrificeType type)
    {
        GD.Print($"[UI] Selected Sacrifice: {type}");
        SacrificeManagerGlobal.Instance.PerformSacrifice(type);
        Close();
    }
}

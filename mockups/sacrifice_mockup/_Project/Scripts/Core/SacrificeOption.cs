using Godot;
using System;

namespace Apotemno.Core;

[GlobalClass]
public partial class SacrificeOption : Resource
{
    [Export] public string Id { get; set; } = "new_sacrifice"; // Unique string ID if needed, or rely on Type
    [Export] public SacrificeType Type { get; set; }
    [Export] public string DisplayName { get; set; } = "Body Part";
    [Export(PropertyHint.MultilineText)] public string Description { get; set; } = "Sacrificing this will...";
    [Export] public Texture2D Icon { get; set; }
    
    // Future: We could add specific modifiers here (e.g. float MovementSpeedMult = 0.5f)
}

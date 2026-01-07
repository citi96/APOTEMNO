using Godot;
using System;

namespace SacrificeMockup;

[GlobalClass]
public partial class SanityManager : Node
{
    [Export]
    public ColorRect PostProcessRect;

    public float Sanity { get; private set; } = 1.0f;

    public override void _Process(double delta)
    {
        HandleDebugInput((float)delta);
        UpdateShader();
    }

    private void HandleDebugInput(float delta)
    {
        // K: Decrease Sanity (Despair)
        if (Input.IsKeyPressed(Key.K))
        {
            Sanity = Mathf.Clamp(Sanity - 0.5f * delta, 0.0f, 1.0f);
        }

        // L: Increase Sanity (Hope)
        if (Input.IsKeyPressed(Key.L))
        {
            Sanity = Mathf.Clamp(Sanity + 0.5f * delta, 0.0f, 1.0f);
        }
    }

    private void UpdateShader()
    {
        if (PostProcessRect != null && PostProcessRect.Material is ShaderMaterial mat)
        {
            mat.SetShaderParameter("sanity", Sanity);
        }
    }
}

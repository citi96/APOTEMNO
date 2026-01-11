using Godot;
using Apotemno.Core;
using Apotemno.Core.Narrative;

namespace Apotemno.Levels;

public partial class LevelStartLogic : Node3D
{
    [Export] public Node3D DoorVisuals;
    [Export] public CollisionShape3D DoorCollider;
    [Export] public Node3D PitCover; // Visual floor that disappears
    [Export] public Light3D PitGlow;
    [Export] public float DoorDropDistance = 2.5f;
    [Export] public float PitCoverDropDistance = 2.0f;
    [Export] public float OpenDuration = 1.4f;

    private Vector3 _doorStartPosition;
    private Vector3 _pitCoverStartPosition;
    private float _pitGlowStartEnergy;
    private bool _pathOpened;

    public override void _Ready()
    {
        if (DoorVisuals != null) _doorStartPosition = DoorVisuals.Position;
        if (PitCover != null) _pitCoverStartPosition = PitCover.Position;
        if (PitGlow != null) _pitGlowStartEnergy = PitGlow.LightEnergy;

        if (SacrificeManagerGlobal.Instance != null)
        {
            SacrificeManagerGlobal.Instance.SacrificePerformed += OnSacrificeperformed;
        }

        CallDeferred(nameof(PlayIntroSequence));
    }

    public override void _ExitTree()
    {
        if (SacrificeManagerGlobal.Instance != null)
        {
            SacrificeManagerGlobal.Instance.SacrificePerformed -= OnSacrificeperformed;
        }
    }

    private void OnSacrificeperformed(int typeInt)
    {
        var type = (SacrificeType)typeInt;
        if (type == SacrificeType.Blood)
        {
             OpenThePath();
        }
    }

    private void OpenThePath()
    {
        if (_pathOpened)
        {
            return;
        }

        _pathOpened = true;
        GD.Print("[LEVEL] Blood spilled. The path opens.");

        NarrativeManagerGlobal.Instance?.PlayLine("[center]Il Cenotafio si apre. La carne ricorda.[/center]", 3.0f, true);

        if (DoorCollider != null) DoorCollider.Disabled = true;

        var tween = CreateTween().SetParallel();

        if (DoorVisuals != null)
        {
            tween.TweenProperty(DoorVisuals, "position:y", _doorStartPosition.Y - DoorDropDistance, OpenDuration)
                .SetTrans(Tween.TransitionType.Cubic)
                .SetEase(Tween.EaseType.In);
        }

        if (PitCover != null)
        {
            tween.TweenProperty(PitCover, "position:y", _pitCoverStartPosition.Y - PitCoverDropDistance, OpenDuration)
                .SetTrans(Tween.TransitionType.Quad)
                .SetEase(Tween.EaseType.In);
        }

        if (PitGlow != null)
        {
            tween.TweenProperty(PitGlow, "light_energy", _pitGlowStartEnergy + 15.0f, OpenDuration)
                .SetTrans(Tween.TransitionType.Sine)
                .SetEase(Tween.EaseType.Out);
        }

        tween.TweenCallback(Callable.From(DisablePitCoverCollision));
    }

    private async void PlayIntroSequence()
    {
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        if (NarrativeManagerGlobal.Instance == null)
        {
            return;
        }

        NarrativeManagerGlobal.Instance.PlayLine("[center]Respira. Il cemento ti ha già misurato.[/center]", 3.2f, true);
        NarrativeManagerGlobal.Instance.PlayLine("[center]Segui il battito. Ti chiederà qualcosa.[/center]", 3.0f);
    }

    private void DisablePitCoverCollision()
    {
        if (PitCover is CSGShape3D coverShape)
        {
            coverShape.UseCollision = false;
        }
    }
}

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

        RegisterSacrificeListener();

        CallDeferred(nameof(PlayIntroSequence));
    }

    public override void _ExitTree()
    {
        UnregisterSacrificeListener();
    }

    private void OnSacrificePerformed(int typeInt)
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

        AnimateNodeDrop(tween, DoorVisuals, _doorStartPosition, DoorDropDistance, Tween.TransitionType.Cubic);
        AnimateNodeDrop(tween, PitCover, _pitCoverStartPosition, PitCoverDropDistance, Tween.TransitionType.Quad);
        AnimatePitGlow(tween);

        tween.TweenCallback(Callable.From(DisablePitCoverCollision));
    }

    private async void PlayIntroSequence()
    {
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        if (NarrativeManagerGlobal.Instance == null)
        {
            return;
        }

        var introLines = new[]
        {
            new DialogueFragment("[center]Respira. Il cemento ti ha già misurato.[/center]", 3.2f, null, true),
            new DialogueFragment("[center]Segui il battito. Ti chiederà qualcosa.[/center]", 3.0f)
        };

        foreach (var line in introLines)
        {
            NarrativeManagerGlobal.Instance.PlayLine(line);
        }
    }

    private void DisablePitCoverCollision()
    {
        if (PitCover is CsgShape3D coverShape)
        {
            coverShape.UseCollision = false;
        }
    }

    private void RegisterSacrificeListener()
    {
        if (SacrificeManagerGlobal.Instance == null)
        {
            return;
        }

        SacrificeManagerGlobal.Instance.SacrificePerformed += OnSacrificePerformed;
    }

    private void UnregisterSacrificeListener()
    {
        if (SacrificeManagerGlobal.Instance == null)
        {
            return;
        }

        SacrificeManagerGlobal.Instance.SacrificePerformed -= OnSacrificePerformed;
    }

    private void AnimateNodeDrop(Tween tween, Node3D target, Vector3 startPosition, float distance, Tween.TransitionType transition)
    {
        if (target == null)
        {
            return;
        }

        tween.TweenProperty(target, "position:y", startPosition.Y - distance, OpenDuration)
            .SetTrans(transition)
            .SetEase(Tween.EaseType.In);
    }

    private void AnimatePitGlow(Tween tween)
    {
        if (PitGlow == null)
        {
            return;
        }

        tween.TweenProperty(PitGlow, "light_energy", _pitGlowStartEnergy + 15.0f, OpenDuration)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.Out);
    }
}

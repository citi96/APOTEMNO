using Godot;

namespace Apotemno.Actors.Player.States;

public interface IPlayerState
{
    void Enter(PlayerController player);
    void Update(PlayerController player, double delta);
    void PhysicsUpdate(PlayerController player, double delta);
    void Exit(PlayerController player);
}

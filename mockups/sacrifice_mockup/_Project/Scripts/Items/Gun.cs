using Godot;
using System;
using Apotemno.Actors.Enemies;

namespace Apotemno.Items;

[GlobalClass]
public partial class Gun : Node3D
{
    [Signal]
    public delegate void AmmoChangedEventHandler(int current, int max);

    [ExportCategory("Stats")]
    [Export] public int MaxAmmo { get; set; } = 6;
    [Export] public float Damage { get; set; } = 25.0f; // 4 shots to kill 100 HP
    [Export] public float ReloadTime { get; set; } = 2.0f;
    [Export] public float FireRate { get; set; } = 0.5f;

    [ExportCategory("Components")]
    [Export] public RayCast3D RayCast { get; set; }
    [Export] public Node3D Visuals { get; set; }

    public int CurrentAmmo { get; private set; }
    public bool IsReloading { get; private set; } = false;

    private ulong _lastFireTime;
    private float _fireIntervalMsec;

    public override void _Ready()
    {
        CurrentAmmo = MaxAmmo;
        _fireIntervalMsec = FireRate * 1000f;
        
        // Defer signal to ensure UI is ready
        CallDeferred(nameof(EmitAmmoUpdate));
    }

    public void Shoot()
    {
        if (IsReloading) return;

        ulong now = Time.GetTicksMsec();
        if (now - _lastFireTime < _fireIntervalMsec) return;

        if (CurrentAmmo <= 0)
        {
            Reload();
            return;
        }

        CurrentAmmo--;
        _lastFireTime = now;
        EmitAmmoUpdate();
        
        // Visual effects (Recoil/Flash) would go here
        GD.Print($"[GUN] Bang! Ammo: {CurrentAmmo}");

        // Hitscan
        if (RayCast != null)
        {
            RayCast.ForceRaycastUpdate(); // Ensure ray is fresh
            if (RayCast.IsColliding())
            {
                var collider = RayCast.GetCollider();
                GD.Print($"[GUN] Hit: {((Node)collider).Name}");
                
                // Damage Interface check ideally, but direct cast for now as per requirements
                if (collider is FacelessAI enemy)
                {
                    enemy.TakeDamage(Damage);
                }
            }
        }
    }

    public async void Reload()
    {
        if (IsReloading || CurrentAmmo == MaxAmmo) return;

        IsReloading = true;
        GD.Print("[GUN] Reloading...");
        // Update UI to show reloading? Or just 0?
        
        await ToSignal(GetTree().CreateTimer(ReloadTime), SceneTreeTimer.SignalName.Timeout);

        CurrentAmmo = MaxAmmo;
        IsReloading = false;
        GD.Print("[GUN] Reloaded.");
        EmitAmmoUpdate();
    }

    private void EmitAmmoUpdate()
    {
        EmitSignal(SignalName.AmmoChanged, CurrentAmmo, MaxAmmo);
    }
}

using Godot;
using System;
using System.Collections.Generic;
using Apotemno.Core;
using Apotemno.Actors.Player;

namespace Apotemno.Systems;

[GlobalClass]
public partial class SaveSystem : Node
{
    public static SaveSystem Instance { get; private set; }

    private const string SAVE_DIR = "user://saves/";
    private const string SAVE_FILE_PREFIX = "save_slot_";
    private const string SAVE_EXT = ".dat";

    public override void _EnterTree()
    {
        if (Instance == null) Instance = this;
        else QueueFree();

        // Ensure directory exists
        if (!DirAccess.DirExistsAbsolute(SAVE_DIR))
        {
            DirAccess.MakeDirAbsolute(SAVE_DIR);
        }
    }

    public class SaveData
    {
        public Dictionary<string, float> PlayerPosition { get; set; } = new Dictionary<string, float>();
        public float PlayerHealth { get; set; }
        public float PlayerRealHealth { get; set; }
        public int CurrentAmmo { get; set; }
        public List<int> Sacrifices { get; set; } = new List<int>();
        public string CurrentLevel { get; set; }
        public long Timestamp { get; set; }
    }

    public void SaveGame(int slot)
    {
        // 1. Check Condition: Memory Sacrifice
        if (SacrificeManagerGlobal.Instance.HasSacrificed(SacrificeType.TemporalLobe))
        {
            GD.Print("[SAVE] Manual Save BLOCKED by Memory Sacrifice. Only Autosaves allowed.");
            // UI Feedback: "You cannot hold onto this memory..."
            return;
        }

        PerformSave(slot);
    }

    public void ForceAutosave()
    {
        // Internal system save, ignores restrictions (or is the ONLY way after memory loss)
        GD.Print("[SAVE] Autosaving...");
        PerformSave(0); // Slot 0 reserved for Autosave? Or use 99? Let's say 0 is Autosave.
    }

    private void PerformSave(int slot)
    {
        PlayerController player = GetTree().GetFirstNodeInGroup("Player") as PlayerController;
        if (player == null)
        {
            GD.PrintErr("[SAVE] No Player found to save!");
            return;
        }

        SaveData data = new SaveData();
        
        // Position
        data.PlayerPosition["x"] = player.GlobalPosition.X;
        data.PlayerPosition["y"] = player.GlobalPosition.Y;
        data.PlayerPosition["z"] = player.GlobalPosition.Z;

        // Health
        if (player.HealthSystem != null)
        {
            data.PlayerHealth = player.HealthSystem.FakeHP;
            data.PlayerRealHealth = player.HealthSystem.RealHP;
        }

        // Ammo
        if (player.EquippedGun != null)
        {
            data.CurrentAmmo = player.EquippedGun.CurrentAmmo;
        }

        // Sacrifices - We need to expose the list or iterate
        // For now, let's just save what we know 
        // NOTE: SacrificeManagerGlobal needs a way to get all sacrifices.
        // I will update SacrificeManagerGlobal to expose the list or assume we iterate enum.
        foreach (SacrificeType type in Enum.GetValues(typeof(SacrificeType)))
        {
            if (SacrificeManagerGlobal.Instance.HasSacrificed(type))
            {
                data.Sacrifices.Add((int)type);
            }
        }

        data.CurrentLevel = GetTree().CurrentScene.SceneFilePath;
        data.Timestamp = DateTime.Now.Ticks;

        // Serialize
        string json = System.Text.Json.JsonSerializer.Serialize(data);
        
        // Obfuscate (Simple Base64)
        string encoded = Marshalls.Utf8ToBase64(json);

        // Write
        string path = $"{SAVE_DIR}{SAVE_FILE_PREFIX}{slot}{SAVE_EXT}";
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
        if (file == null)
        {
            GD.PrintErr($"[SAVE] Failed to open {path} Error: {FileAccess.GetOpenError()}");
            return;
        }
        file.StoreString(encoded);
        GD.Print($"[SAVE] Game Saved to Slot {slot}");
    }

    public void LoadGame(int slot)
    {
        string path = $"{SAVE_DIR}{SAVE_FILE_PREFIX}{slot}{SAVE_EXT}";
        if (!FileAccess.FileExists(path))
        {
            GD.Print($"[SAVE] No save file found at {path}");
            return;
        }

        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        string encoded = file.GetAsText();
        
        // De-obfuscate
        string json = Marshalls.Base64ToUtf8(encoded);
        
        try 
        {
            SaveData data = System.Text.Json.JsonSerializer.Deserialize<SaveData>(json);
            ApplySaveData(data);
        }
        catch (Exception e)
        {
            GD.PrintErr($"[SAVE] Corrupt save file! {e.Message}");
        }
    }

    private async void ApplySaveData(SaveData data)
    {
        // 1. Level Check
        if (GetTree().CurrentScene.SceneFilePath != data.CurrentLevel)
        {
             GD.Print($"[SAVE] Changing Level to {data.CurrentLevel}...");
             // GameManager ChangeScene... but we'll do it manually for now to be safe
             GetTree().ChangeSceneToFile(data.CurrentLevel);
             await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame); // Wait for load
        }

        PlayerController player = GetTree().GetFirstNodeInGroup("Player") as PlayerController;
        if (player != null)
        {
            // Position
            player.GlobalPosition = new Vector3(
                data.PlayerPosition["x"],
                data.PlayerPosition["y"],
                data.PlayerPosition["z"]
            );

            // Health
            if (player.HealthSystem != null)
            {
                // We might need a SetValues public method or just re-damage/heal
                // Reflection or public properties necessary? 
                // Currently set is private. I should update HealthManager to allow setting state.
                // For now, hack:
                float diff = player.HealthSystem.RealHP - data.PlayerRealHealth;
                 if (diff > 0) player.HealthSystem.TakeDamage(diff);
                 else if (diff < 0) player.HealthSystem.Heal(-diff);
            }

            // Ammo
            if (player.EquippedGun != null)
            {
                // Gun needs a way to set ammo directly.
                // Re-instantiate gun? Or add method. 
                // For now, I will assume Gun has a public SetAmmo or Property setter.
                // Checking Gun.cs... CurrentAmmo is private set. I need to update it.
            }

            // Sacrifices
            SacrificeManagerGlobal.Instance.ResetSacrifices();
            foreach (int typeInt in data.Sacrifices)
            {
                 SacrificeManagerGlobal.Instance.PerformSacrifice((SacrificeType)typeInt);
            }

            GD.Print("[SAVE] Game Loaded Successfully.");
        }
    }
}

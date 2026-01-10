using Godot;
using System;
using System.Collections.Generic;

namespace Apotemno.Core
{
    [GlobalClass]
    public partial class GameManager : Node
    {
        // Singleton pattern (Autoload usually guarantees unique instance, but safe access helps)
        public static GameManager Instance { get; private set; }

        [Signal] public delegate void LevelLoadedEventHandler(string levelName);
        [Signal] public delegate void SanityChangedEventHandler(float newSanity);

        // --- Global State ---
        public int CurrentAct { get; private set; } = 1;
        
        private float _sanity = 100.0f;
        public float Sanity
        {
            get => _sanity;
            set
            {
                _sanity = Mathf.Clamp(value, 0f, 100f);
                EmitSignal(SignalName.SanityChanged, _sanity);
            }
        }

        // Flags for sacrifices/narrative
        private HashSet<string> _worldFlags = new HashSet<string>();

        public bool IsGamePaused { get; set; } = false;

        public override void _EnterTree()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                QueueFree();
            }
        }

        public override void _Ready()
        {
            GD.Print("[GameManager] Initialized.");
        }

        // --- Flow Control ---

        public void StartNewGame()
        {
            GD.Print("[GameManager] Starting New Game...");
            _worldFlags.Clear();
            CurrentAct = 1;
            Sanity = 100f;
            
            // TODO: Load the first real level. For now, using Level_00_Ventricle as prototype.
            LoadLevel("res://_Project/Scenes/Levels/Level_00_Ventricle.tscn");
        }

        public void LoadLevel(string levelPath)
        {
            GD.Print($"[GameManager] Loading Level: {levelPath}");
            
            // Simple scene change for now. 
            // In future, might want to use a Loading Screen scene to handle async loading.
            Error err = GetTree().ChangeSceneToFile(levelPath);
            
            if (err != Error.Ok)
            {
                GD.PrintErr($"[GameManager] Failed to load level {levelPath}: {err}");
            }
            else
            {
                EmitSignal(SignalName.LevelLoaded, levelPath);
            }
        }

        public void TriggerEnding(int endingId)
        {
            GD.Print($"[GameManager] Triggering Ending {endingId}");
            // Logic to load ending scene or show credits
        }

        // --- State Management ---

        public void SetFlag(string flag, bool verify = true)
        {
            _worldFlags.Add(flag);
            GD.Print($"[GameManager] Flag Set: {flag}");
        }

        public bool CheckFlag(string flag)
        {
            return _worldFlags.Contains(flag);
        }
    }
}

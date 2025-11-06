using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace HKAccessibility;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    internal static Plugin Instance { get; private set; }

    private Harmony harmony;
    private InputManager inputManager;
    private MenuNavigationMonitor menuNavigationMonitor;
    private InventoryReader inventoryReader;

    private void Awake()
    {
        Instance = this;
        Logger = base.Logger;
        Logger.LogInfo("=== Hollow Knight Accessibility Mod ===");
        Logger.LogInfo($"Version {MyPluginInfo.PLUGIN_VERSION} loaded successfully!");

        // Initialize Tolk screen reader
        try
        {
            if (TolkScreenReader.Instance.Initialize())
            {
                Logger.LogInfo("Tolk initialized successfully!");

                string detectedReader = TolkScreenReader.Instance.DetectScreenReader();
                if (!string.IsNullOrEmpty(detectedReader))
                {
                    Logger.LogInfo($"Detected screen reader: {detectedReader}");
                }
                else
                {
                    Logger.LogInfo("No screen reader detected, using SAPI fallback");
                }

                if (TolkScreenReader.Instance.HasSpeech())
                {
                    Logger.LogInfo("Speech output available");
                    TolkScreenReader.Instance.Speak("Hollow Knight Accessibility Mod loaded successfully", false);
                }

                if (TolkScreenReader.Instance.HasBraille())
                {
                    Logger.LogInfo("Braille output available");
                }
            }
            else
            {
                Logger.LogWarning("Failed to initialize Tolk - falling back to console logging");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to initialize screen reader: {ex}");
        }

        // Apply Harmony patches
        ApplyHarmonyPatches();

        // Initialize menu navigation monitor (handles menu navigation and value monitoring)
        InitializeMenuNavigationMonitor();

        // Inventory monitor: reads inventory open/close and dynamic texts (names/descriptions)
        inventoryReader = gameObject.AddComponent<InventoryReader>();
        Logger.LogInfo("Inventory reader initialized.");

        // Initialize input manager
        inputManager = new InputManager();

        Logger.LogInfo("All accessibility systems initialized successfully");
    }

    private void InitializeMenuNavigationMonitor()
    {
        try
        {
            menuNavigationMonitor = gameObject.AddComponent<MenuNavigationMonitor>();
            Logger.LogInfo("Menu navigation monitor initialized.");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to initialize menu navigation monitor: {ex}");
        }
    }

    private void Update()
    {
        try
        {
            // Handle accessibility input keys
            inputManager?.HandleInput();
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error in Plugin.Update: {ex}");
        }
    }

    private void ApplyHarmonyPatches()
    {
        try
        {
            harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
            harmony.PatchAll();
            Logger.LogInfo("Harmony patches applied successfully.");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to apply Harmony patches: {ex}");
        }
    }

    private void OnDestroy()
    {
        try
        {
            harmony?.UnpatchSelf();
            Logger.LogInfo("Harmony patches removed.");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error removing Harmony patches: {ex}");
        }

        try
        {
            TolkScreenReader.Instance.Speak("Hollow Knight Accessibility Mod unloaded", false);
            TolkScreenReader.Instance.Cleanup();
            Logger.LogInfo("Screen reader support unloaded.");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error during cleanup: {ex}");
        }

        Logger.LogInfo("Accessibility Mod unloaded.");
    }
}

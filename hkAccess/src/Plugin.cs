using System;
using BepInEx;
using BepInEx.Logging;
using System.Runtime.InteropServices;

namespace HKAccessibility;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    private MenuAccessibility menuAccessibility;

    private void Awake()
    {
        Logger = base.Logger;
        Logger.LogInfo("=== Hollow Knight Accessibility Mod ===");
        Logger.LogInfo($"Version {MyPluginInfo.PLUGIN_VERSION} loaded successfully!");

        try
        {
            TolkBridge.Load();
            string screenReaderName = TolkBridge.DetectScreenReader();

            if (!string.IsNullOrEmpty(screenReaderName))
            {
                Logger.LogInfo($"Screen reader detected: {screenReaderName}");
                TolkBridge.Output("Hollow Knight Accessibility Mod loaded successfully!");
                Logger.LogInfo("Screen reader announcement sent.");
            }
            else
            {
                Logger.LogWarning("No screen reader detected.");
            }
        }
        catch (System.Exception ex)
        {
            Logger.LogError($"Failed to initialize screen reader: {ex.Message}");
        }

        InitializeMenuAccessibility();
        Logger.LogInfo("Audio navigation and screen reader support initialized.");
    }

    private void InitializeMenuAccessibility()
    {
        try
        {
            menuAccessibility = gameObject.AddComponent<MenuAccessibility>();
            Logger.LogInfo("Menu accessibility system initialized.");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to initialize menu accessibility: {ex.Message}");
        }
    }

    private void OnDestroy()
    {
        try
        {
            TolkBridge.Output("Hollow Knight Accessibility Mod unloaded.");
            TolkBridge.Unload();
            Logger.LogInfo("Screen reader support unloaded.");
        }
        catch (System.Exception ex)
        {
            Logger.LogError($"Error during cleanup: {ex.Message}");
        }

        Logger.LogInfo("Accessibility Mod unloaded.");
    }
}

public static class TolkBridge
{
    [DllImport("Tolk.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void Tolk_Load();

    [DllImport("Tolk.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void Tolk_Unload();

    [DllImport("Tolk.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    public static extern IntPtr Tolk_DetectScreenReader();

    [DllImport("Tolk.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    public static extern bool Tolk_Output([MarshalAs(UnmanagedType.LPWStr)] string text, [MarshalAs(UnmanagedType.Bool)] bool interrupt = false);

    public static void Load() => Tolk_Load();
    public static void Unload() => Tolk_Unload();

    public static string DetectScreenReader()
    {
        IntPtr ptr = Tolk_DetectScreenReader();
        return ptr != IntPtr.Zero ? Marshal.PtrToStringUni(ptr) : string.Empty;
    }

    public static void Output(string text, bool interrupt = false) => Tolk_Output(text, interrupt);
}

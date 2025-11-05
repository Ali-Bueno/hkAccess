using HarmonyLib;
using System;

namespace HKAccessibility.Patches
{
    /// <summary>
    /// Patches for MenuAudioSlider to announce volume changes in real-time
    /// </summary>
    [HarmonyPatch(typeof(MenuAudioSlider))]
    public static class MenuAudioSliderPatches
    {
        private static float lastAnnouncedValue = -1;
        private static float lastAnnounceTime = 0;

        /// <summary>
        /// Patch UpdateTextValue to announce slider changes when user presses arrow keys
        /// </summary>
        [HarmonyPatch("UpdateTextValue")]
        [HarmonyPostfix]
        public static void OnUpdateTextValue(float value)
        {
            try
            {
                // Debounce: Don't announce the same value twice within 0.15 seconds
                if (value == lastAnnouncedValue && UnityEngine.Time.realtimeSinceStartup - lastAnnounceTime < 0.15f)
                {
                    return;
                }

                lastAnnouncedValue = value;
                lastAnnounceTime = UnityEngine.Time.realtimeSinceStartup;

                int roundedValue = UnityEngine.Mathf.RoundToInt(value);
                Plugin.Logger.LogInfo($"[MenuAudioSlider] Value changed to: {roundedValue}");
                TolkScreenReader.Instance.Speak(roundedValue.ToString(), true);
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Error in MenuAudioSlider.UpdateTextValue patch: {ex}");
            }
        }
    }
}

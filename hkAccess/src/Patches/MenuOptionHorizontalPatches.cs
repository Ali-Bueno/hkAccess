using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace HKAccessibility.Patches
{
    /// <summary>
    /// Patches for MenuOptionHorizontal to announce option changes in real-time
    /// Covers: Resolution, Display Mode, Frame Cap, VSync, Fullscreen, Backer Credits,
    /// Native Achievements, Controller Rumble, and more
    /// </summary>
    [HarmonyPatch(typeof(MenuOptionHorizontal))]
    public static class MenuOptionHorizontalPatches
    {
        private static string lastAnnouncedText = "";
        private static float lastAnnounceTime = 0;

        /// <summary>
        /// Patch UpdateText to announce option changes when user presses arrow keys
        /// This is called by both IncrementOption and DecrementOption
        /// </summary>
        [HarmonyPatch("UpdateText")]
        [HarmonyPostfix]
        public static void OnUpdateText(MenuOptionHorizontal __instance)
        {
            try
            {
                AnnounceOptionChange(__instance);
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Error in MenuOptionHorizontal.UpdateText patch: {ex}");
            }
        }

        private static void AnnounceOptionChange(MenuOptionHorizontal instance)
        {
            // Only announce if this option is currently selected by the user
            if (EventSystem.current == null || EventSystem.current.currentSelectedGameObject != instance.gameObject)
            {
                return;
            }

            // Get the optionText field using reflection
            FieldInfo optionTextField = typeof(MenuOptionHorizontal).GetField("optionText", BindingFlags.Public | BindingFlags.Instance);
            if (optionTextField == null)
            {
                return;
            }

            Text optionText = optionTextField.GetValue(instance) as Text;
            if (optionText == null || string.IsNullOrEmpty(optionText.text))
            {
                return;
            }

            string currentText = optionText.text;

            // Debounce: Don't announce the same text twice within 0.15 seconds
            if (currentText == lastAnnouncedText && UnityEngine.Time.realtimeSinceStartup - lastAnnounceTime < 0.15f)
            {
                return;
            }

            lastAnnouncedText = currentText;
            lastAnnounceTime = UnityEngine.Time.realtimeSinceStartup;

            Plugin.Logger.LogInfo($"[MenuOptionHorizontal] Value changed to: {currentText}");
            TolkScreenReader.Instance.Speak(currentText, true);
        }
    }

    /// <summary>
    /// Patches for MenuLanguageSetting which overrides UpdateText()
    /// </summary>
    [HarmonyPatch(typeof(MenuLanguageSetting))]
    public static class MenuLanguageSettingPatches
    {
        private static string lastAnnouncedLanguage = "";
        private static float lastAnnounceTime = 0;

        /// <summary>
        /// Patch UpdateText to announce language changes
        /// MenuLanguageSetting overrides UpdateText, so we need a separate patch
        /// </summary>
        [HarmonyPatch("UpdateText")]
        [HarmonyPostfix]
        public static void OnUpdateText(MenuLanguageSetting __instance)
        {
            try
            {
                // Only announce if this option is currently selected by the user
                if (EventSystem.current == null || EventSystem.current.currentSelectedGameObject != __instance.gameObject)
                {
                    return;
                }

                // Get the optionText field using reflection
                FieldInfo optionTextField = typeof(MenuOptionHorizontal).GetField("optionText", BindingFlags.Public | BindingFlags.Instance);
                if (optionTextField == null)
                {
                    return;
                }

                Text optionText = optionTextField.GetValue(__instance) as Text;
                if (optionText == null || string.IsNullOrEmpty(optionText.text))
                {
                    return;
                }

                string currentText = optionText.text;

                // Debounce: Don't announce the same language twice within 0.15 seconds
                if (currentText == lastAnnouncedLanguage && UnityEngine.Time.realtimeSinceStartup - lastAnnounceTime < 0.15f)
                {
                    return;
                }

                lastAnnouncedLanguage = currentText;
                lastAnnounceTime = UnityEngine.Time.realtimeSinceStartup;

                Plugin.Logger.LogInfo($"[MenuLanguageSetting] Language changed to: {currentText}");
                TolkScreenReader.Instance.Speak(currentText, true);
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Error in MenuLanguageSetting.UpdateText patch: {ex}");
            }
        }
    }
}

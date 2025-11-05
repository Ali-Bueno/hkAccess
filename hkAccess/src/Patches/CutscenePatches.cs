using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HKAccessibility.Patches
{
    /// <summary>
    /// Patches for cutscene text announcements
    /// Monitors and announces all text that appears during cutscenes
    /// </summary>
    [HarmonyPatch]
    public static class CutscenePatches
    {
        private static GlobalEnums.UIState lastUIState = GlobalEnums.UIState.INACTIVE;
        private static HashSet<string> announcedCutsceneTexts = new HashSet<string>();
        private static bool isMonitoringCutscene = false;

        /// <summary>
        /// Patch to detect when entering/exiting cutscene mode
        /// </summary>
        [HarmonyPatch(typeof(UIManager), "SetState")]
        [HarmonyPostfix]
        public static void OnUIStateChanged(UIManager __instance, GlobalEnums.UIState newState)
        {
            try
            {
                // Detect when entering cutscene
                if (newState == GlobalEnums.UIState.CUTSCENE && lastUIState != GlobalEnums.UIState.CUTSCENE)
                {
                    Plugin.Logger.LogInfo("=== ENTERED UI CUTSCENE STATE ===");
                    announcedCutsceneTexts.Clear();
                    isMonitoringCutscene = true;
                    __instance.StartCoroutine(MonitorCutsceneTexts());
                }
                // Detect when exiting cutscene
                else if (newState != GlobalEnums.UIState.CUTSCENE && lastUIState == GlobalEnums.UIState.CUTSCENE)
                {
                    Plugin.Logger.LogInfo("=== EXITED UI CUTSCENE STATE ===");
                    isMonitoringCutscene = false;
                    announcedCutsceneTexts.Clear();
                }

                lastUIState = newState;
            }
            catch (System.Exception ex)
            {
                Plugin.Logger.LogError($"Error in OnUIStateChanged: {ex}");
            }
        }

        private static IEnumerator MonitorCutsceneTexts()
        {
            Plugin.Logger.LogInfo("Starting cutscene text monitoring...");

            // Monitor while in cutscene
            while (isMonitoringCutscene)
            {
                // Find all TextMeshPro components in scene
                var allTMPTexts = UnityEngine.Object.FindObjectsOfType<TMPro.TextMeshPro>();

                foreach (var text in allTMPTexts)
                {
                    if (text == null || !text.gameObject.activeInHierarchy)
                        continue;

                    // Check if text is visible (alpha > 0 and has content)
                    if (text.enabled && text.alpha > 0.1f && !string.IsNullOrWhiteSpace(text.text) && text.text.Length >= 5)
                    {
                        // Create unique ID for this text
                        string textId = text.gameObject.name + "|" + text.text;

                        // If not announced yet, announce it
                        if (!announcedCutsceneTexts.Contains(textId))
                        {
                            announcedCutsceneTexts.Add(textId);
                            Plugin.Logger.LogInfo($"Announcing cutscene text: {text.text}");
                            TolkScreenReader.Instance.Speak(text.text, true);
                        }
                    }
                }

                // Check every 0.1 seconds
                yield return new WaitForSeconds(0.1f);
            }

            Plugin.Logger.LogInfo("Cutscene text monitoring stopped");
        }
    }
}

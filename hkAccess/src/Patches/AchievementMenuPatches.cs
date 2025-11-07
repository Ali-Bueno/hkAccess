using HarmonyLib;
using UnityEngine;

namespace HKAccessibility.Patches
{
    /// <summary>
    /// Patches the UIManager to attach accessibility information to achievement UI elements.
    /// </summary>
    [HarmonyPatch(typeof(UIManager), "UpdateMenuAchievementStatus")]
    public static class AchievementMenuPatches
    {
        /// <summary>
        /// After the game populates an achievement's UI, we build and attach our full announcement string.
        /// </summary>
        private static void Postfix(UIManager __instance, Achievement ach, MenuAchievement menuAch)
        {
            try
            {
                if (ach == null || menuAch == null) return;

                // The game has already populated the title and text fields with localized content.
                string title = menuAch.title?.text ?? "";
                string desc = menuAch.text?.text ?? "";

                // Determine the status based on the game's own logic.
                bool isUnlocked = GameManager.instance.IsAchievementAwarded(ach.key);
                string status = isUnlocked ? ModLocalization.Get("ACHIEVEMENT_UNLOCKED") : ModLocalization.Get("ACHIEVEMENT_LOCKED");

                // For hidden achievements that are not yet unlocked, the game shows "???".
                // We should announce it as a hidden achievement.
                if (!isUnlocked && ach.type == GlobalEnums.AchievementType.Hidden)
                {
                    title = ModLocalization.Get("HIDDEN_ACHIEVEMENT_TITLE");
                    desc = ModLocalization.Get("HIDDEN_ACHIEVEMENT");
                }

                string fullAnnouncement = $"{title}. {desc}. {status}";

                // Add or get our custom component to store the full string.
                AccessibilityInfo info = menuAch.gameObject.GetComponent<AccessibilityInfo>();
                if (info == null)
                {
                    info = menuAch.gameObject.AddComponent<AccessibilityInfo>();
                }
                info.fullDescription = fullAnnouncement;

                Plugin.Logger.LogInfo($"[AchievementMenuPatches] Attached info to '{ach.key}': \"{fullAnnouncement}\"");
            }
            catch (System.Exception ex)
            {
                Plugin.Logger.LogError($"Error in AchievementMenuPatches: {ex}");
            }
        }
    }
}

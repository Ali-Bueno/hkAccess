using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HKAccessibility.Patches
{
    /// <summary>
    /// Patches for the Extras menu to announce content pack details.
    /// </summary>
    [HarmonyPatch(typeof(ContentPackDetailsUI), "OnEnable")]
    public static class ExtrasMenuPatches
    {
        private static void Postfix(ContentPackDetailsUI __instance)
        {
            try
            {
                // OnEnable starts the coroutine that populates the text.
                // We start our own coroutine to wait for it to finish, then read the text.
                __instance.StartCoroutine(AnnounceContentAfterDelay(__instance));
            }
            catch (System.Exception ex)
            {
                Plugin.Logger.LogError($"Error in ExtrasMenuPatches: {ex}");
            }
        }

        private static IEnumerator AnnounceContentAfterDelay(ContentPackDetailsUI ui)
        {
            // Wait for the UI's own coroutine to finish populating the text.
            yield return new WaitForSeconds(0.4f);

            if (ui == null) yield break;

            List<string> allText = new List<string>();

            // The UI uses standard Text components for this screen.
            var title = ui.titleText;
            if (title != null && title.gameObject.activeInHierarchy && !string.IsNullOrEmpty(title.text))
            {
                allText.Add(title.text.Trim());
            }

            var description = ui.descriptionText;
            if (description != null && description.gameObject.activeInHierarchy && !string.IsNullOrEmpty(description.text))
            {
                allText.Add(description.text.Trim());
            }

            if (allText.Count > 0)
            {
                // Join all found text, replacing newlines, and announce.
                string announcement = string.Join(". ", allText).Replace("\n", " ").Replace("..", ".");
                Plugin.Logger.LogInfo($"[ExtrasContent] Speaking: {announcement}");
                TolkScreenReader.Instance.Speak(announcement, false); // Use interrupt = false as requested
            }
            else
            {
                Plugin.Logger.LogWarning("[ExtrasContent] Could not find any text to announce.");
            }
        }
    }
}

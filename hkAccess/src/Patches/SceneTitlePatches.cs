using HarmonyLib;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace HKAccessibility.Patches
{
    /// <summary>
    /// Patches for announcing scene/menu titles when entering new scenes
    /// </summary>
    [HarmonyPatch(typeof(GameManager), nameof(GameManager.BeginScene))]
    public static class SceneTitlePatches
    {
        private static string lastAnnouncedScene = "";

        /// <summary>
        /// Patch GameManager.BeginScene to announce menu titles when scenes start
        /// </summary>
        [HarmonyPostfix]
        public static void OnBeginScene()
        {
            try
            {
                string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

                if (sceneName == lastAnnouncedScene)
                    return;

                lastAnnouncedScene = sceneName;

                // Use GameManager to start coroutine (it persists across scenes)
                if (GameManager.instance != null)
                {
                    GameManager.instance.StartCoroutine(AnnounceSceneTitleDelayed());
                }
            }
            catch (System.Exception ex)
            {
                Plugin.Logger.LogError($"Error in OnBeginScene: {ex}");
            }
        }

        private static IEnumerator AnnounceSceneTitleDelayed()
        {
            // Wait for scene to load and UI to initialize
            yield return new WaitForSeconds(0.8f);

            try
            {
                Text[] allTexts = UnityEngine.Object.FindObjectsOfType<Text>();
                Text titleText = null;
                float largestFontSize = 0;

                foreach (var text in allTexts)
                {
                    if (text.gameObject.activeInHierarchy &&
                        !string.IsNullOrEmpty(text.text) &&
                        text.fontSize > largestFontSize &&
                        text.fontSize >= 30 &&
                        text.GetComponentInParent<Button>() == null &&
                        !text.text.Contains("User Display") &&
                        !text.text.Contains("1.5.") &&
                        !text.text.Contains("\n") &&
                        !text.text.Contains("Conecta") &&
                        !text.text.ToLower().Contains("connect"))
                    {
                        string cleanText = text.text.Trim();
                        if (cleanText.Length > 0 && cleanText.Length < 50)
                        {
                            largestFontSize = text.fontSize;
                            titleText = text;
                        }
                    }
                }

                if (titleText != null)
                {
                    string announcement = titleText.text.Trim();

                    // Double-check for controller messages
                    if (announcement.Contains("Conecta") || announcement.ToLower().Contains("connect"))
                    {
                        Plugin.Logger.LogInfo($"Filtered controller message: {announcement}");
                        yield break;
                    }

                    Plugin.Logger.LogInfo($"Menu title: {announcement}");
                    TolkScreenReader.Instance.Speak(announcement, true);
                }
            }
            catch (System.Exception ex)
            {
                Plugin.Logger.LogError($"Error in AnnounceSceneTitleDelayed: {ex}");
            }
        }
    }
}

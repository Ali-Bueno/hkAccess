using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace HKAccessibility.Patches
{
    /// <summary>
    /// Patches for game state changes
    /// Handles: Save game, prompts, menu navigation, save slots
    /// </summary>
    [HarmonyPatch]
    public static class GameStatePatches
    {
        // Patch for quit game prompt
        [HarmonyPatch(typeof(UIManager), nameof(UIManager.UIShowQuitGamePrompt))]
        [HarmonyPostfix]
        public static void OnShowQuitGamePrompt(UIManager __instance)
        {
            Plugin.Logger.LogInfo("UIShowQuitGamePrompt called - announcing quit game prompt");
            __instance.StartCoroutine(AnnouncePromptDelayed(__instance.quitGamePrompt));
        }

        // Patch for return to menu prompt
        [HarmonyPatch(typeof(UIManager), nameof(UIManager.UIShowReturnMenuPrompt))]
        [HarmonyPostfix]
        public static void OnShowReturnMenuPrompt(UIManager __instance)
        {
            Plugin.Logger.LogInfo("UIShowReturnMenuPrompt called - announcing return menu prompt");
            __instance.StartCoroutine(AnnouncePromptDelayed(__instance.returnMainMenuPrompt));
        }

        // Patch for resolution change prompt
        [HarmonyPatch(typeof(UIManager), nameof(UIManager.UIShowResolutionPrompt))]
        [HarmonyPostfix]
        public static void OnShowResolutionPrompt(UIManager __instance)
        {
            Plugin.Logger.LogInfo("UIShowResolutionPrompt called - announcing resolution prompt");
            __instance.StartCoroutine(AnnouncePromptDelayed(__instance.resolutionPrompt));
        }

        private static IEnumerator AnnouncePromptDelayed(MenuScreen prompt)
        {
            yield return new WaitForSecondsRealtime(0.5f);

            if (prompt == null || prompt.gameObject == null)
            {
                Plugin.Logger.LogWarning("Prompt is null or destroyed");
                yield break;
            }

            Plugin.Logger.LogInfo($"Announcing prompt: {prompt.gameObject.name}");

            Text[] texts = prompt.GetComponentsInChildren<Text>(true);
            List<string> promptMessages = new List<string>();

            foreach (var text in texts)
            {
                if (text == null || !text.gameObject.activeInHierarchy || string.IsNullOrEmpty(text.text))
                    continue;

                string cleanText = text.text.Trim();

                // Filter unwanted texts
                if (cleanText.Contains("User Display"))
                    continue;

                // Filter technical names
                bool looksLikeTechnicalName = !cleanText.Contains(" ") &&
                                              (cleanText.Contains("Prompt") ||
                                               cleanText.Contains("Menu") ||
                                               cleanText.Contains("Screen") ||
                                               cleanText.Contains("Panel") ||
                                               cleanText.Contains("Canvas") ||
                                               cleanText.Contains("QuitGame") ||
                                               cleanText.Contains("ReturnMenu") ||
                                               cleanText.Contains("ExitToMenu"));

                if (looksLikeTechnicalName)
                {
                    Plugin.Logger.LogInfo($"  Filtering technical name: '{cleanText}'");
                    continue;
                }

                bool isButton = text.GetComponentInParent<Button>() != null;
                bool isSelectable = text.GetComponentInParent<UnityEngine.UI.Selectable>() != null;

                Plugin.Logger.LogInfo($"  Found text: '{cleanText}' (isButton: {isButton}, isSelectable: {isSelectable})");

                // Only add non-button, non-selectable texts
                if (!isButton && !isSelectable && !promptMessages.Contains(cleanText))
                {
                    promptMessages.Add(cleanText);
                }
            }

            if (promptMessages.Count > 0)
            {
                string announcement = string.Join(". ", promptMessages);
                Plugin.Logger.LogInfo($">>> Announcing: {announcement}");
                TolkScreenReader.Instance.Speak(announcement, true);
            }
            else
            {
                Plugin.Logger.LogWarning("No text found in prompt");
            }
        }

        // Patch for save slot selection
        [HarmonyPatch(typeof(UnityEngine.UI.SaveSlotButton), "OnSelect")]
        [HarmonyPostfix]
        public static void OnSaveSlotSelect(UnityEngine.UI.SaveSlotButton __instance, BaseEventData eventData)
        {
            try
            {
                Plugin.Logger.LogInfo($"SaveSlotButton.OnSelect called - Slot: {__instance.saveSlot}, State: {__instance.saveFileState}");
                GameManager.instance?.StartCoroutine(AnnounceSaveSlotAfterSelect(__instance));
            }
            catch (System.Exception ex)
            {
                Plugin.Logger.LogError($"Error in OnSaveSlotSelect: {ex}");
            }
        }

        private static IEnumerator AnnounceSaveSlotAfterSelect(UnityEngine.UI.SaveSlotButton saveSlotButton)
        {
            yield return new WaitForSecondsRealtime(0.3f);

            var slotNumber = saveSlotButton.saveSlot switch
            {
                UnityEngine.UI.SaveSlotButton.SaveSlot.SLOT_1 => 1,
                UnityEngine.UI.SaveSlotButton.SaveSlot.SLOT_2 => 2,
                UnityEngine.UI.SaveSlotButton.SaveSlot.SLOT_3 => 3,
                UnityEngine.UI.SaveSlotButton.SaveSlot.SLOT_4 => 4,
                _ => 0
            };

            var saveFileState = saveSlotButton.saveFileState;

            Plugin.Logger.LogInfo($"Announcing slot {slotNumber}, state: {saveFileState}");

            switch (saveFileState)
            {
                case UnityEngine.UI.SaveSlotButton.SaveFileStates.Empty:
                    TolkScreenReader.Instance.Speak($"Slot {slotNumber}. Nuevo juego", true);
                    break;

                case UnityEngine.UI.SaveSlotButton.SaveFileStates.Corrupted:
                    TolkScreenReader.Instance.Speak($"Slot {slotNumber}. Archivo corrupto", true);
                    break;

                case UnityEngine.UI.SaveSlotButton.SaveFileStates.LoadedStats:
                    var saveStatsField = typeof(UnityEngine.UI.SaveSlotButton).GetField("saveStats",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                    if (saveStatsField != null)
                    {
                        var saveStats = saveStatsField.GetValue(saveSlotButton) as SaveStats;
                        if (saveStats != null)
                        {
                            AnnounceSaveSlotWithStats(saveSlotButton, saveStats, slotNumber);
                        }
                        else
                        {
                            Plugin.Logger.LogWarning("SaveStats is null");
                            TolkScreenReader.Instance.Speak($"Slot {slotNumber}", true);
                        }
                    }
                    else
                    {
                        Plugin.Logger.LogWarning("saveStats field not found");
                        TolkScreenReader.Instance.Speak($"Slot {slotNumber}", true);
                    }
                    break;

                case UnityEngine.UI.SaveSlotButton.SaveFileStates.OperationInProgress:
                    TolkScreenReader.Instance.Speak($"Slot {slotNumber}. Cargando", true);
                    break;

                default:
                    TolkScreenReader.Instance.Speak($"Slot {slotNumber}", true);
                    break;
            }
        }

        private static void AnnounceSaveSlotWithStats(UnityEngine.UI.SaveSlotButton saveSlotButton, SaveStats saveStats, int slotNumber)
        {
            string announcement = $"Slot {slotNumber}. Partida guardada";

            if (saveSlotButton.locationText != null && !string.IsNullOrEmpty(saveSlotButton.locationText.text))
            {
                string location = saveSlotButton.locationText.text.Replace("\n", " ").Replace("\r", " ").Trim();
                announcement += $". {location}";
            }

            if (saveSlotButton.completionText != null && !string.IsNullOrEmpty(saveSlotButton.completionText.text))
            {
                announcement += $". {saveSlotButton.completionText.text} completado";
            }

            if (saveSlotButton.playTimeText != null && !string.IsNullOrEmpty(saveSlotButton.playTimeText.text))
            {
                announcement += $". {saveSlotButton.playTimeText.text}";
            }

            if (saveSlotButton.geoText != null && !string.IsNullOrEmpty(saveSlotButton.geoText.text) && saveSlotButton.geoIcon.enabled)
            {
                announcement += $". {saveSlotButton.geoText.text} Geo";
            }

            if (saveStats.permadeathMode == 1)
            {
                announcement += ". Modo Steel Soul";
            }
            else if (saveStats.permadeathMode == 2)
            {
                announcement = $"Slot {slotNumber}. Steel Soul derrotado";
            }

            Plugin.Logger.LogInfo($">>> Announcing: {announcement}");
            TolkScreenReader.Instance.Speak(announcement, true);
        }

        // Patch to announce when entering a menu
        [HarmonyPatch(typeof(UIManager), "ShowMenu")]
        [HarmonyPrefix]
        public static void OnShowMenu(MenuScreen menu)
        {
            try
            {
                if (menu == null || menu.gameObject == null)
                    return;

                string menuName = menu.gameObject.name;

                // Skip prompts - handled by specific patches
                if (menuName.Contains("Prompt"))
                {
                    Plugin.Logger.LogInfo($"Skipping prompt announcement (handled by specific patch): {menuName}");
                    return;
                }

                // Convert technical names to friendly names
                string friendlyName = menuName switch
                {
                    "OptionsMenuScreen" => "Opciones",
                    "AudioMenuScreen" => "Audio",
                    "VideoMenuScreen" => "Vídeo",
                    "GamepadMenuScreen" => "Mando",
                    "KeyboardMenuScreen" => "Teclado",
                    "GameOptionsMenuScreen" => "Opciones de juego",
                    "PauseMenuScreen" => "Pausa",
                    "AchievementsMenuScreen" => "Logros",
                    "ExtrasMenuScreen" => "Extras",
                    "OverscanMenuScreen" => "Ajuste de pantalla",
                    "BrightnessMenuScreen" => "Brillo",
                    "RemapGamepadMenuScreen" => "Reasignar controles de mando",
                    _ => menuName.Replace("MenuScreen", "").Replace("Screen", "")
                };

                Plugin.Logger.LogInfo($">>> Announcing menu: {friendlyName}");
                TolkScreenReader.Instance.Speak(friendlyName, true);
            }
            catch (System.Exception ex)
            {
                Plugin.Logger.LogError($"Error announcing menu: {ex}");
            }
        }

        // Patch to announce when game is being saved
        [HarmonyPatch(typeof(GameManager), "SaveGame", new System.Type[] { typeof(System.Action<bool>) })]
        [HarmonyPrefix]
        public static void OnSaveGame()
        {
            try
            {
                Plugin.Logger.LogInfo(">>> Game save triggered");
                TolkScreenReader.Instance.Speak("Guardando juego", false);
            }
            catch (System.Exception ex)
            {
                Plugin.Logger.LogError($"Error announcing game save: {ex}");
            }
        }

        // Patch to announce when save completes
        [HarmonyPatch(typeof(GameManager), "SaveGame", new System.Type[] { typeof(System.Action<bool>) })]
        [HarmonyPostfix]
        public static void OnSaveGameComplete()
        {
            try
            {
                Plugin.Logger.LogInfo(">>> Game save completed");
                GameManager.instance.StartCoroutine(AnnounceSaveComplete());
            }
            catch (System.Exception ex)
            {
                Plugin.Logger.LogError($"Error announcing save completion: {ex}");
            }
        }

        private static IEnumerator AnnounceSaveComplete()
        {
            yield return new WaitForSecondsRealtime(0.3f);
            TolkScreenReader.Instance.Speak("Juego guardado", false);
        }
    }
}

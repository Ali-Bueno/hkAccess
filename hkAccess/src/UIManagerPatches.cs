using System.Collections;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace HKAccessibility;

[HarmonyPatch]
public static class UIManagerPatches
{
    private static ManualLogSource Logger => Plugin.Logger;

    // Patch para detectar cuando se muestra el popup de "salir del juego"
    [HarmonyPatch(typeof(UIManager), nameof(UIManager.UIShowQuitGamePrompt))]
    [HarmonyPostfix]
    public static void OnShowQuitGamePrompt(UIManager __instance)
    {
        Logger.LogInfo("UIShowQuitGamePrompt called - announcing quit game prompt");
        __instance.StartCoroutine(AnnouncePromptDelayed(__instance.quitGamePrompt));
    }

    // Patch para detectar cuando se muestra el popup de "volver al menú principal"
    [HarmonyPatch(typeof(UIManager), nameof(UIManager.UIShowReturnMenuPrompt))]
    [HarmonyPostfix]
    public static void OnShowReturnMenuPrompt(UIManager __instance)
    {
        Logger.LogInfo("UIShowReturnMenuPrompt called - announcing return menu prompt");
        __instance.StartCoroutine(AnnouncePromptDelayed(__instance.returnMainMenuPrompt));
    }

    // Patch para detectar cuando se muestra el popup de "confirmar resolución"
    [HarmonyPatch(typeof(UIManager), nameof(UIManager.UIShowResolutionPrompt))]
    [HarmonyPostfix]
    public static void OnShowResolutionPrompt(UIManager __instance)
    {
        Logger.LogInfo("UIShowResolutionPrompt called - announcing resolution prompt");
        __instance.StartCoroutine(AnnouncePromptDelayed(__instance.resolutionPrompt));
    }

    private static IEnumerator AnnouncePromptDelayed(MenuScreen prompt)
    {
        // Esperar a que el popup se muestre completamente
        yield return new WaitForSecondsRealtime(0.5f);

        if (prompt == null || prompt.gameObject == null)
        {
            Logger.LogWarning("Prompt is null or destroyed");
            yield break;
        }

        Logger.LogInfo($"Announcing prompt: {prompt.gameObject.name}");

        // Recoger todo el texto del prompt
        Text[] texts = prompt.GetComponentsInChildren<Text>(true);
        System.Collections.Generic.List<string> promptMessages = new System.Collections.Generic.List<string>();
        System.Collections.Generic.List<string> buttonTexts = new System.Collections.Generic.List<string>();

        foreach (var text in texts)
        {
            if (text == null || !text.gameObject.activeInHierarchy || string.IsNullOrEmpty(text.text))
                continue;

            string cleanText = text.text.Trim();

            // Filtrar textos que no queremos
            if (cleanText.Contains("User Display"))
                continue;

            // Filtrar nombres técnicos de prompts
            if (cleanText.Contains("Prompt") || cleanText.Contains("QuitGame") || cleanText.Contains("ReturnMenu"))
                continue;

            bool isButton = text.GetComponentInParent<Button>() != null;

            // También verificar si es un Selectable (incluye botones y otros controles)
            bool isSelectable = text.GetComponentInParent<UnityEngine.UI.Selectable>() != null;

            Logger.LogInfo($"  Found text: '{cleanText}' (isButton: {isButton}, isSelectable: {isSelectable})");

            // SOLO agregar textos que NO sean botones ni otros elementos seleccionables
            if (!isButton && !isSelectable)
            {
                // Solo agregar si no está duplicado
                if (!promptMessages.Contains(cleanText))
                {
                    promptMessages.Add(cleanText);
                }
            }
        }

        // Construir el anuncio - solo el mensaje del popup, no los botones
        if (promptMessages.Count > 0)
        {
            string announcement = string.Join(". ", promptMessages);
            Logger.LogInfo($">>> Announcing: {announcement}");
            TolkBridge.Output(announcement, true);
        }
        else
        {
            Logger.LogWarning("No text found in prompt");
        }
    }

    // Patch para anunciar información de slots de guardado cuando se seleccionan
    [HarmonyPatch(typeof(UnityEngine.UI.SaveSlotButton), "OnSelect")]
    [HarmonyPostfix]
    public static void OnSaveSlotSelected(UnityEngine.UI.SaveSlotButton __instance, BaseEventData eventData)
    {
        try
        {
            Logger.LogInfo($"SaveSlotButton selected: {__instance.saveSlot}, state: {__instance.saveFileState}");

            // Iniciar coroutine para anunciar con delay (para evitar conflicto con MenuAccessibility)
            GameManager.instance?.StartCoroutine(AnnounceSaveSlotDelayed(__instance));
        }
        catch (System.Exception ex)
        {
            Logger.LogError($"Error announcing save slot: {ex.Message}");
        }
    }

    private static IEnumerator AnnounceSaveSlotDelayed(UnityEngine.UI.SaveSlotButton saveSlotButton)
    {
        // Esperar un poco para que MenuAccessibility no interfiera
        yield return new WaitForSecondsRealtime(0.15f);

        // Obtener el estado del slot
        var saveFileState = saveSlotButton.saveFileState;
        var slotNumber = saveSlotButton.saveSlot switch
        {
            UnityEngine.UI.SaveSlotButton.SaveSlot.SLOT_1 => 1,
            UnityEngine.UI.SaveSlotButton.SaveSlot.SLOT_2 => 2,
            UnityEngine.UI.SaveSlotButton.SaveSlot.SLOT_3 => 3,
            UnityEngine.UI.SaveSlotButton.SaveSlot.SLOT_4 => 4,
            _ => 0
        };

        string announcement = $"Slot {slotNumber}";

        Logger.LogInfo($"Processing save slot announcement, state: {saveFileState}");

        // Según el estado del archivo de guardado
        switch (saveFileState)
        {
            case UnityEngine.UI.SaveSlotButton.SaveFileStates.Empty:
                announcement += ". Nuevo juego";
                break;

            case UnityEngine.UI.SaveSlotButton.SaveFileStates.Corrupted:
                announcement += ". Archivo corrupto";
                break;

            case UnityEngine.UI.SaveSlotButton.SaveFileStates.LoadedStats:
                // Obtener la información del guardado usando reflexión
                var saveStatsField = typeof(UnityEngine.UI.SaveSlotButton).GetField("saveStats",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (saveStatsField != null)
                {
                    var saveStats = saveStatsField.GetValue(saveSlotButton) as SaveStats;
                    Logger.LogInfo($"SaveStats retrieved: {(saveStats != null ? "found" : "null")}");

                    if (saveStats != null)
                    {
                        announcement += ". Partida guardada";

                        // Obtener la ubicación
                        if (saveSlotButton.locationText != null && !string.IsNullOrEmpty(saveSlotButton.locationText.text))
                        {
                            string location = saveSlotButton.locationText.text.Replace("\n", " ").Replace("\r", " ").Trim();
                            announcement += $". {location}";
                            Logger.LogInfo($"Location: {location}");
                        }

                        // Obtener porcentaje de completado
                        if (saveSlotButton.completionText != null && !string.IsNullOrEmpty(saveSlotButton.completionText.text))
                        {
                            announcement += $". {saveSlotButton.completionText.text} completado";
                            Logger.LogInfo($"Completion: {saveSlotButton.completionText.text}");
                        }

                        // Obtener tiempo de juego
                        if (saveSlotButton.playTimeText != null && !string.IsNullOrEmpty(saveSlotButton.playTimeText.text))
                        {
                            announcement += $". {saveSlotButton.playTimeText.text}";
                            Logger.LogInfo($"Playtime: {saveSlotButton.playTimeText.text}");
                        }

                        // Obtener geo
                        if (saveSlotButton.geoText != null && !string.IsNullOrEmpty(saveSlotButton.geoText.text) && saveSlotButton.geoIcon.enabled)
                        {
                            announcement += $". {saveSlotButton.geoText.text} Geo";
                            Logger.LogInfo($"Geo: {saveSlotButton.geoText.text}");
                        }

                        // Detectar modo Steel Soul
                        if (saveStats.permadeathMode == 1)
                        {
                            announcement += ". Modo Steel Soul";
                        }
                        else if (saveStats.permadeathMode == 2)
                        {
                            announcement = $"Slot {slotNumber}. Steel Soul derrotado";
                        }
                    }
                    else
                    {
                        Logger.LogWarning("SaveStats was null");
                    }
                }
                else
                {
                    Logger.LogWarning("saveStats field not found via reflection");
                }
                break;

            case UnityEngine.UI.SaveSlotButton.SaveFileStates.OperationInProgress:
                announcement += ". Cargando";
                break;

            case UnityEngine.UI.SaveSlotButton.SaveFileStates.NotStarted:
                // No anunciar nada si aún no ha empezado a cargar
                Logger.LogInfo("Slot not started yet, skipping announcement");
                yield break;
        }

        Logger.LogInfo($">>> Announcing save slot: {announcement}");
        TolkBridge.Output(announcement, true);
    }

    // Patch para anunciar cuando entras a un menú
    [HarmonyPatch(typeof(UIManager), "ShowMenu")]
    [HarmonyPrefix]
    public static void OnShowMenu(MenuScreen menu)
    {
        try
        {
            if (menu == null || menu.gameObject == null)
                return;

            string menuName = menu.gameObject.name;

            // Convertir nombres técnicos a nombres amigables
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

            Logger.LogInfo($">>> Announcing menu: {friendlyName}");
            TolkBridge.Output(friendlyName, true);
        }
        catch (System.Exception ex)
        {
            Logger.LogError($"Error announcing menu: {ex.Message}");
        }
    }

    // Patch para anunciar cuando se guarda el juego
    [HarmonyPatch(typeof(GameManager), "SaveGame", new System.Type[] { typeof(System.Action<bool>) })]
    [HarmonyPrefix]
    public static void OnSaveGame()
    {
        try
        {
            Logger.LogInfo(">>> Game save triggered");
            TolkBridge.Output("Guardando juego", false);
        }
        catch (System.Exception ex)
        {
            Logger.LogError($"Error announcing game save: {ex.Message}");
        }
    }

    // Patch para anunciar cuando termina de guardarse
    [HarmonyPatch(typeof(GameManager), "SaveGame", new System.Type[] { typeof(System.Action<bool>) })]
    [HarmonyPostfix]
    public static void OnSaveGameComplete()
    {
        try
        {
            Logger.LogInfo(">>> Game save completed");
            // Pequeño delay para que el anuncio no interrumpa el anterior
            GameManager.instance.StartCoroutine(AnnounceSaveComplete());
        }
        catch (System.Exception ex)
        {
            Logger.LogError($"Error announcing save completion: {ex.Message}");
        }
    }

    private static IEnumerator AnnounceSaveComplete()
    {
        yield return new WaitForSecondsRealtime(0.3f);
        TolkBridge.Output("Juego guardado", false);
    }
}

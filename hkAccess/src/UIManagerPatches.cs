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

            // Filtrar si el texto contiene SOLO palabras técnicas (sin espacios o con muy pocos caracteres)
            // Esto atrapa "QuitGamePrompt", "ExitToMenuPrompt", etc.
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
                Logger.LogInfo($"  Filtering technical name (no spaces): '{cleanText}'");
                continue;
            }

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

    // Patch para anunciar información cuando se selecciona un save slot
    [HarmonyPatch(typeof(UnityEngine.UI.SaveSlotButton), "OnSelect")]
    [HarmonyPostfix]
    public static void OnSaveSlotSelect(UnityEngine.UI.SaveSlotButton __instance, BaseEventData eventData)
    {
        try
        {
            Logger.LogInfo($"SaveSlotButton.OnSelect called - Slot: {__instance.saveSlot}, State: {__instance.saveFileState}");

            // Iniciar coroutine para anunciar después de un pequeño delay
            GameManager.instance?.StartCoroutine(AnnounceSaveSlotAfterSelect(__instance));
        }
        catch (System.Exception ex)
        {
            Logger.LogError($"Error in OnSaveSlotSelect: {ex.Message}");
        }
    }

    private static IEnumerator AnnounceSaveSlotAfterSelect(UnityEngine.UI.SaveSlotButton saveSlotButton)
    {
        // Esperar para que MenuAccessibility anuncie primero y luego lo sobrescribimos
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

        Logger.LogInfo($"Announcing slot {slotNumber}, state: {saveFileState}");

        // Según el estado del archivo de guardado
        switch (saveFileState)
        {
            case UnityEngine.UI.SaveSlotButton.SaveFileStates.Empty:
                TolkBridge.Output($"Slot {slotNumber}. Nuevo juego", true);
                break;

            case UnityEngine.UI.SaveSlotButton.SaveFileStates.Corrupted:
                TolkBridge.Output($"Slot {slotNumber}. Archivo corrupto", true);
                break;

            case UnityEngine.UI.SaveSlotButton.SaveFileStates.LoadedStats:
                // Obtener saveStats usando reflexión
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
                        Logger.LogWarning("SaveStats is null");
                        TolkBridge.Output($"Slot {slotNumber}", true);
                    }
                }
                else
                {
                    Logger.LogWarning("saveStats field not found");
                    TolkBridge.Output($"Slot {slotNumber}", true);
                }
                break;

            case UnityEngine.UI.SaveSlotButton.SaveFileStates.OperationInProgress:
                TolkBridge.Output($"Slot {slotNumber}. Cargando", true);
                break;

            default:
                // NotStarted o cualquier otro estado
                TolkBridge.Output($"Slot {slotNumber}", true);
                break;
        }
    }

    private static void AnnounceSaveSlotWithStats(UnityEngine.UI.SaveSlotButton saveSlotButton, SaveStats saveStats, int slotNumber)
    {
        string announcement = $"Slot {slotNumber}. Partida guardada";

        // Obtener la ubicación
        if (saveSlotButton.locationText != null && !string.IsNullOrEmpty(saveSlotButton.locationText.text))
        {
            string location = saveSlotButton.locationText.text.Replace("\n", " ").Replace("\r", " ").Trim();
            announcement += $". {location}";
        }

        // Obtener porcentaje de completado
        if (saveSlotButton.completionText != null && !string.IsNullOrEmpty(saveSlotButton.completionText.text))
        {
            announcement += $". {saveSlotButton.completionText.text} completado";
        }

        // Obtener tiempo de juego
        if (saveSlotButton.playTimeText != null && !string.IsNullOrEmpty(saveSlotButton.playTimeText.text))
        {
            announcement += $". {saveSlotButton.playTimeText.text}";
        }

        // Obtener geo
        if (saveSlotButton.geoText != null && !string.IsNullOrEmpty(saveSlotButton.geoText.text) && saveSlotButton.geoIcon.enabled)
        {
            announcement += $". {saveSlotButton.geoText.text} Geo";
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

        Logger.LogInfo($">>> Announcing: {announcement}");
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

            // Filtrar prompts - estos se manejan con sus propios patches específicos
            if (menuName.Contains("Prompt"))
            {
                Logger.LogInfo($"Skipping prompt announcement (handled by specific patch): {menuName}");
                return;
            }

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

    // Patch para anunciar diálogos in-game
    [HarmonyPatch(typeof(DialogueBox), "ShowPage")]
    [HarmonyPostfix]
    public static void OnDialogueShowPage(DialogueBox __instance, int pageNum)
    {
        try
        {
            // Esperar a que el texto se muestre
            GameManager.instance?.StartCoroutine(AnnounceDialogue(__instance));
        }
        catch (System.Exception ex)
        {
            Logger.LogError($"Error announcing dialogue: {ex.Message}");
        }
    }

    private static IEnumerator AnnounceDialogue(DialogueBox dialogueBox)
    {
        // Esperar a que el diálogo termine de animarse
        yield return new WaitForSecondsRealtime(0.5f);

        try
        {
            // Obtener el TextMeshPro usando reflexión
            var textMeshField = typeof(DialogueBox).GetField("textMesh",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (textMeshField != null)
            {
                var textMesh = textMeshField.GetValue(dialogueBox) as TMPro.TextMeshPro;
                if (textMesh != null)
                {
                    // Obtener la página actual
                    var currentPageField = typeof(DialogueBox).GetField("currentPage",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                    if (currentPageField != null)
                    {
                        int currentPage = (int)currentPageField.GetValue(dialogueBox);

                        // Obtener el texto de la página actual
                        string pageText = GetDialoguePageText(textMesh, currentPage);

                        if (!string.IsNullOrEmpty(pageText))
                        {
                            Logger.LogInfo($">>> Announcing dialogue: {pageText}");
                            TolkBridge.Output(pageText, true);
                        }
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            Logger.LogError($"Error in AnnounceDialogue: {ex.Message}");
        }
    }

    private static string GetDialoguePageText(TMPro.TextMeshPro textMesh, int pageNum)
    {
        if (textMesh == null || textMesh.textInfo == null)
            return "";

        try
        {
            int pageIndex = pageNum - 1;
            if (pageIndex < 0 || pageIndex >= textMesh.textInfo.pageCount)
                return "";

            var pageInfo = textMesh.textInfo.pageInfo[pageIndex];
            int firstCharIndex = pageInfo.firstCharacterIndex;
            int lastCharIndex = pageInfo.lastCharacterIndex;

            string fullText = textMesh.text;
            if (firstCharIndex >= 0 && lastCharIndex < fullText.Length && lastCharIndex >= firstCharIndex)
            {
                string pageText = fullText.Substring(firstCharIndex, lastCharIndex - firstCharIndex + 1);
                pageText = pageText.Replace("<br>", " ").Trim();
                return pageText;
            }
        }
        catch (System.Exception ex)
        {
            Logger.LogError($"Error extracting page text: {ex.Message}");
        }

        return "";
    }

    // Monitorear cambios de UIState para detectar cutscenes
    private static GlobalEnums.UIState lastUIState = GlobalEnums.UIState.INACTIVE;
    private static System.Collections.Generic.HashSet<string> announcedCutsceneTexts = new System.Collections.Generic.HashSet<string>();
    private static bool isMonitoringCutscene = false;

    // Patch para detectar cuando el UI entra en modo CUTSCENE (escenas de intro/opening)
    [HarmonyPatch(typeof(UIManager), "SetState")]
    [HarmonyPostfix]
    public static void OnUIStateChanged(UIManager __instance, GlobalEnums.UIState newState)
    {
        try
        {
            // Detectar cuando entramos en cutscene
            if (newState == GlobalEnums.UIState.CUTSCENE && lastUIState != GlobalEnums.UIState.CUTSCENE)
            {
                Logger.LogInfo("=== ENTERED UI CUTSCENE STATE ===");
                // Limpiar el registro de textos anunciados
                announcedCutsceneTexts.Clear();
                isMonitoringCutscene = true;
                // Iniciar monitoreo continuo de textos de cutscene
                __instance.StartCoroutine(MonitorCutsceneTexts());
            }
            // Detectar cuando salimos de cutscene
            else if (newState != GlobalEnums.UIState.CUTSCENE && lastUIState == GlobalEnums.UIState.CUTSCENE)
            {
                Logger.LogInfo("=== EXITED UI CUTSCENE STATE ===");
                isMonitoringCutscene = false;
                announcedCutsceneTexts.Clear();
            }

            lastUIState = newState;
        }
        catch (System.Exception ex)
        {
            Logger.LogError($"Error in OnUIStateChanged: {ex.Message}");
        }
    }

    private static IEnumerator MonitorCutsceneTexts()
    {
        Logger.LogInfo("Starting cutscene text monitoring...");

        // Monitorear mientras estemos en cutscene
        while (isMonitoringCutscene)
        {
            // Buscar todos los TextMeshPro en la escena
            var allTMPTexts = UnityEngine.Object.FindObjectsOfType<TMPro.TextMeshPro>();

            foreach (var text in allTMPTexts)
            {
                if (text == null || !text.gameObject.activeInHierarchy)
                    continue;

                // Verificar si el texto es visible (alpha > 0 y tiene contenido)
                if (text.enabled && text.alpha > 0.1f && !string.IsNullOrWhiteSpace(text.text) && text.text.Length >= 5)
                {
                    // Crear un ID único para este texto
                    string textId = text.gameObject.name + "|" + text.text;

                    // Si no lo hemos anunciado aún, anunciarlo
                    if (!announcedCutsceneTexts.Contains(textId))
                    {
                        announcedCutsceneTexts.Add(textId);
                        Logger.LogInfo($"Announcing cutscene text: {text.text}");
                        TolkBridge.Output(text.text, true);
                    }
                }
            }

            // Chequear cada 0.1 segundos
            yield return new WaitForSeconds(0.1f);
        }

        Logger.LogInfo("Cutscene text monitoring stopped");
    }

    private static string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform current = obj.transform.parent;
        while (current != null)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }
        return path;
    }
}

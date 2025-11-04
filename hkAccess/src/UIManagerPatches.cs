using System.Collections;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

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

            bool isButton = text.GetComponentInParent<Button>() != null;

            Logger.LogInfo($"  Found text: '{cleanText}' (isButton: {isButton})");

            if (isButton)
            {
                buttonTexts.Add(cleanText);
            }
            else
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
}

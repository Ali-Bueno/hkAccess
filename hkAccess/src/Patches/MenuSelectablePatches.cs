using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

namespace HKAccessibility.Patches
{
    /// <summary>
    /// Patches MenuSelectable to announce UI elements when selected
    /// Handles: CharmItem, InvItemDisplay, MapMarkerButton, ShopItemStats, JournalEntryStats, and generic UI elements
    /// Uses Postfix to not interfere with MenuAccessibility's processing
    /// </summary>
    [HarmonyPatch(typeof(MenuSelectable), "OnSelect")]
    public static class MenuSelectable_OnSelect_Patch
    {
        private static void Postfix(BaseEventData eventData, MenuSelectable __instance)
        {
            // High-priority check for pre-formatted accessibility info
            try
            {
                var accessibilityInfo = __instance.GetComponentInParent<AccessibilityInfo>();
                if (accessibilityInfo != null && !string.IsNullOrEmpty(accessibilityInfo.fullDescription))
                {
                    if (!SpokenTextHistory.HasBeenSpoken(accessibilityInfo.fullDescription))
                    {
                        Plugin.Logger.LogInfo($"[MenuSelectable (AccessibilityInfo)] Speaking: {accessibilityInfo.fullDescription}");
                        TolkScreenReader.Instance.Speak(accessibilityInfo.fullDescription, true);
                    }
                    return;
                }
            }
            catch (System.Exception ex)
            {
                Plugin.Logger.LogError($"Error in MenuSelectable_OnSelect (AccessibilityInfo): {ex}");
            }

            // Handle Charms via InvCharmBackboard in hierarchy (reliable in inventory)
            try
            {
                var backboard = __instance.GetComponentInParent<InvCharmBackboard>() ?? __instance.GetComponentInChildren<InvCharmBackboard>();
                if (backboard != null)
                {
                    HKAccessibility.Patches.InventoryPatches.SpeakCharm(backboard.charmNum, backboard);
                    return;
                }
            }
            catch (System.Exception ex)
            {
                Plugin.Logger.LogError($"Error in MenuSelectable_OnSelect (InvCharmBackboard): {ex}");
            }

            // Handle other Inventory Items
            try
            {
                InvItemDisplay invItem = __instance.GetComponentInParent<InvItemDisplay>() ?? __instance.GetComponentInChildren<InvItemDisplay>();
                if (invItem != null)
                {
                    // Prefer reading visible text near the selection (inventory shows name/desc labels)
                    string spoken = null;
                    var tmpu = __instance.GetComponentInChildren<TextMeshProUGUI>();
                    var tmp = __instance.GetComponentInChildren<TextMeshPro>();
                    if (tmpu != null && !string.IsNullOrEmpty(tmpu.text)) spoken = tmpu.text;
                    if (string.IsNullOrEmpty(spoken) && tmp != null && !string.IsNullOrEmpty(tmp.text)) spoken = tmp.text;

                    if (!string.IsNullOrEmpty(spoken) && !SpokenTextHistory.HasBeenSpoken(spoken))
                    {
                        Plugin.Logger.LogInfo($"[MenuSelectable (InvItemDisplay)] Speaking: {spoken}");
                        TolkScreenReader.Instance.Speak(spoken, false);
                    }
                    else
                    {
                        // As a fallback, read PlayerData bool/count if possible
                        string pdField = Traverse.Create(invItem).Field<string>("playerDataBool").Value;
                        if (!string.IsNullOrEmpty(pdField))
                        {
                            var pd = PlayerData.instance;
                            // Try bool first
                            bool has = pd.GetBool(pdField);
                            int count = pd.GetInt(pdField);
                            string statement = has ? "obtenido" : (count > 0 ? count.ToString() : null);
                            if (!string.IsNullOrEmpty(statement))
                            {
                                var nameGuess = __instance.gameObject.name;
                                string msg = string.IsNullOrEmpty(nameGuess) ? statement : ($"{nameGuess}: {statement}");
                                if (!SpokenTextHistory.HasBeenSpoken(msg))
                                {
                                    Plugin.Logger.LogInfo($"[MenuSelectable (InvItemDisplay fallback)] Speaking: {msg}");
                                    TolkScreenReader.Instance.Speak(msg, false);
                                }
                            }
                        }
                    }
                    return;
                }
            }
            catch (System.Exception ex)
            {
                Plugin.Logger.LogError($"Error in MenuSelectable_OnSelect (InvItemDisplay): {ex}");
            }

            // Handle Map Markers
            try
            {
                MapMarkerButton mapMarkerButton = __instance.GetComponent<MapMarkerButton>();
                if (mapMarkerButton != null)
                {
                    var textMesh = __instance.GetComponentInChildren<TextMeshProUGUI>();
                    if (textMesh != null && !string.IsNullOrEmpty(textMesh.text))
                    {
                        if (!SpokenTextHistory.HasBeenSpoken(textMesh.text))
                        {
                            Plugin.Logger.LogInfo($"[MenuSelectable (MapMarkerButton)] Speaking: {textMesh.text}");
                            TolkScreenReader.Instance.Speak(textMesh.text, false);
                        }
                    }
                    return;
                }
            }
            catch (System.Exception ex)
            {
                Plugin.Logger.LogError($"Error in MenuSelectable_OnSelect (MapMarkerButton): {ex}");
            }

            // Handle Shop Items
            try
            {
                ShopItemStats shopItemStats = __instance.GetComponent<ShopItemStats>();
                if (shopItemStats != null)
                {
                    string name = Language.Language.Get(shopItemStats.GetNameConvo(), "Shop");
                    string desc = Language.Language.Get(shopItemStats.GetDescConvo(), "Shop");
                    int cost = shopItemStats.GetCost();

                    string announcement = $"{name}, cuesta {cost}. {desc}.";
                    if (!SpokenTextHistory.HasBeenSpoken(announcement))
                    {
                        Plugin.Logger.LogInfo($"[MenuSelectable (ShopItemStats)] Speaking: {announcement}");
                        TolkScreenReader.Instance.Speak(announcement, false);
                    }
                    return;
                }
            }
            catch (System.Exception ex)
            {
                Plugin.Logger.LogError($"Error in MenuSelectable_OnSelect (ShopItemStats): {ex}");
            }

            // Handle Journal Entries
            try
            {
                var journalEntry = __instance.GetComponent<JournalEntryStats>();
                if (journalEntry != null)
                {
                    var textMesh = __instance.GetComponentInChildren<TextMeshProUGUI>();
                    if (textMesh != null && !string.IsNullOrEmpty(textMesh.text))
                    {
                        if (!SpokenTextHistory.HasBeenSpoken(textMesh.text))
                        {
                            Plugin.Logger.LogInfo($"[MenuSelectable (JournalEntry)] Speaking: {textMesh.text}");
                            TolkScreenReader.Instance.Speak(textMesh.text, false);
                        }
                    }
                    return;
                }
            }
            catch (System.Exception ex)
            {
                Plugin.Logger.LogError($"Error in MenuSelectable_OnSelect (JournalEntry): {ex}");
            }

            // Generic fallback - for standard UI elements (buttons, sliders, toggles, etc.)
            // This is only executed if none of the above specific handlers matched
            string textToSpeak = null;

            // Strategy 1: Look in self, children, then parents
            textToSpeak = GetTextFromComponent<TextMeshProUGUI>(__instance.gameObject);
            if (string.IsNullOrEmpty(textToSpeak)) textToSpeak = GetTextFromComponent<TextMeshPro>(__instance.gameObject);
            if (string.IsNullOrEmpty(textToSpeak)) textToSpeak = GetTextFromComponent<Text>(__instance.gameObject);

            if (string.IsNullOrEmpty(textToSpeak))
            {
                textToSpeak = GetTextFromComponentInChildren<TextMeshProUGUI>(__instance.gameObject);
                if (string.IsNullOrEmpty(textToSpeak)) textToSpeak = GetTextFromComponentInChildren<TextMeshPro>(__instance.gameObject);
                if (string.IsNullOrEmpty(textToSpeak)) textToSpeak = GetTextFromComponentInChildren<Text>(__instance.gameObject);
            }

            // Strategy 2: Look for labels in siblings of the parent (for sliders and complex UI)
            if (string.IsNullOrEmpty(textToSpeak) && __instance.transform.parent != null)
            {
                Transform parent = __instance.transform.parent;
                // Go up one more level if the parent is something like a "Controls" container
                if (parent.parent != null)
                {
                    var labels = parent.parent.GetComponentsInChildren<Text>();
                    if (labels != null)
                    {
                        foreach (var label in labels)
                        {
                            if (!string.IsNullOrEmpty(label.text) && !int.TryParse(label.text, out _))
                            {
                                textToSpeak = label.text;
                                break;
                            }
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(textToSpeak))
            {
                textToSpeak = GetTextFromComponentInParent<TextMeshProUGUI>(__instance.gameObject);
                if (string.IsNullOrEmpty(textToSpeak)) textToSpeak = GetTextFromComponentInParent<TextMeshPro>(__instance.gameObject);
                if (string.IsNullOrEmpty(textToSpeak)) textToSpeak = GetTextFromComponentInParent<Text>(__instance.gameObject);
            }

            // Filter unwanted texts (controller messages, etc.)
            if (!string.IsNullOrEmpty(textToSpeak))
            {
                if (textToSpeak.Contains("Conecta") || textToSpeak.ToLower().Contains("connect"))
                {
                    Plugin.Logger.LogInfo($"[MenuSelectable (Generic)] Filtered controller message: {textToSpeak}");
                    return;
                }

                if (!SpokenTextHistory.HasBeenSpoken(textToSpeak))
                {
                    Plugin.Logger.LogInfo($"[MenuSelectable (Generic)] Speaking: {textToSpeak}");
                    TolkScreenReader.Instance.Speak(textToSpeak, false);
                }
            }
        }

        private static string GetTextFromComponent<T>(GameObject obj) where T : Component
        {
            T component = obj.GetComponent<T>();
            if (component != null)
            {
                return GetTextFromComponent(component);
            }
            return null;
        }

        private static string GetTextFromComponentInChildren<T>(GameObject obj) where T : Component
        {
            T component = obj.GetComponentInChildren<T>();
            if (component != null)
            {
                return GetTextFromComponent(component);
            }
            return null;
        }

        private static string GetTextFromComponentInParent<T>(GameObject obj) where T : Component
        {
            T component = obj.GetComponentInParent<T>();
            if (component != null)
            {
                return GetTextFromComponent(component);
            }
            return null;
        }

        private static string GetTextFromComponent(Component component)
        {
            if (component is TextMeshProUGUI tmpu)
            {
                return tmpu.text;
            }
            if (component is TextMeshPro tmp)
            {
                return tmp.text;
            }
            if (component is Text uiText)
            {
                return uiText.text;
            }
            return null;
        }
    }
}

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
            // Handle Charms
            try
            {
                CharmItem charmItem = __instance.GetComponent<CharmItem>();
                if (charmItem != null)
                {
                    var charmID = Traverse.Create(charmItem).Field<int>("charmID").Value;

                    string charmName = Language.Language.Get($"CHARM_NAME_{charmID}", "UI");
                    string charmDesc = Language.Language.Get($"CHARM_DESC_{charmID}", "UI");
                    int charmCost = PlayerData.instance.GetInt("charmCost_" + charmID);
                    bool isEquipped = PlayerData.instance.GetBool($"equippedCharm_{charmID}");
                    bool isNew = PlayerData.instance.GetBool($"newCharm_{charmID}");

                    string announcement = "";
                    if (isNew)
                    {
                        announcement += "Nuevo. ";
                    }
                    announcement += $"{charmName}. Costo: {charmCost} muescas. {charmDesc}";
                    if (isEquipped)
                    {
                        announcement += " Actualmente equipado.";
                    }

                    if (!SpokenTextHistory.HasBeenSpoken(announcement))
                    {
                        Plugin.Logger.LogInfo($"[MenuSelectable (CharmItem)] Speaking: {announcement}");
                        TolkScreenReader.Instance.Speak(announcement, false);
                    }
                    return;
                }
            }
            catch (System.Exception ex)
            {
                Plugin.Logger.LogError($"Error in MenuSelectable_OnSelect (CharmItem): {ex}");
            }

            // Handle other Inventory Items
            try
            {
                InvItemDisplay invItem = __instance.GetComponent<InvItemDisplay>();
                if (invItem != null)
                {
                    string playerdataName = Traverse.Create(invItem).Field<string>("playerdataName").Value;

                    string baseName = playerdataName.ToUpper();
                    string itemNameKey = $"INV_NAME_{baseName}";
                    string itemDescKey = $"INV_DESC_{baseName}";

                    string itemName = Language.Language.Get(itemNameKey, "UI");
                    string itemDesc = Language.Language.Get(itemDescKey, "UI");

                    if (itemName != itemNameKey)
                    {
                        int amount = PlayerData.instance.GetInt(playerdataName);
                        string announcement = $"{itemName} ({amount}). {itemDesc}";
                        if (!SpokenTextHistory.HasBeenSpoken(announcement))
                        {
                            Plugin.Logger.LogInfo($"[MenuSelectable (InvItemDisplay)] Speaking: {announcement}");
                            TolkScreenReader.Instance.Speak(announcement, false);
                        }
                    }
                    else
                    {
                        var textMesh = __instance.GetComponentInChildren<TextMeshProUGUI>();
                        if (textMesh != null && !string.IsNullOrEmpty(textMesh.text))
                        {
                            if (!SpokenTextHistory.HasBeenSpoken(textMesh.text))
                            {
                                Plugin.Logger.LogInfo($"[MenuSelectable (InvItemDisplay - fallback)] Speaking: {textMesh.text}");
                                TolkScreenReader.Instance.Speak(textMesh.text, false);
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

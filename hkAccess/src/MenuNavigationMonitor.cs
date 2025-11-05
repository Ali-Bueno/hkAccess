using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace HKAccessibility
{
    /// <summary>
    /// MonoBehaviour that monitors menu navigation (EventSystem selection changes)
    /// and announces UI elements when they are selected
    /// Value changes are handled by Harmony patches (MenuAudioSliderPatches, MenuOptionHorizontalPatches)
    /// </summary>
    public class MenuNavigationMonitor : MonoBehaviour
    {
        private GameObject lastSelectedObject;
        private float announceDelay = 0.1f;
        private float lastAnnounceTime;

        private void Update()
        {
            try
            {
                CheckMenuNavigation();
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Error in MenuNavigationMonitor.Update: {ex}");
            }
        }

        private void CheckMenuNavigation()
        {
            GameObject currentSelected = EventSystem.current?.currentSelectedGameObject;

            if (currentSelected != null && currentSelected != lastSelectedObject)
            {
                if (Time.realtimeSinceStartup - lastAnnounceTime > announceDelay)
                {
                    AnnounceUIElement(currentSelected);
                    lastSelectedObject = currentSelected;
                    lastAnnounceTime = Time.realtimeSinceStartup;
                }
            }
        }

        private void AnnounceUIElement(GameObject uiObject)
        {
            if (uiObject == null) return;

            string announcement = GetFullUIDescription(uiObject);

            if (!string.IsNullOrEmpty(announcement))
            {
                Plugin.Logger.LogInfo($"Announcing: {announcement}");
                TolkScreenReader.Instance.Speak(announcement, true);
            }
        }

        private string GetFullUIDescription(GameObject uiObject)
        {
            // Handle charm inventory navigation
            Component invCharmBackboard = uiObject.GetComponent("InvCharmBackboard");
            if (invCharmBackboard != null)
            {
                return BuildCharmInventoryDescription(invCharmBackboard);
            }

            // Handle item inventory navigation
            Component invItemDisplay = uiObject.GetComponent("InvItemDisplay");
            if (invItemDisplay != null)
            {
                return BuildItemInventoryDescription(invItemDisplay, uiObject);
            }

            // Handle MenuOptionHorizontal (resolution, display mode, language, etc.)
            Component menuOptionHorizontal = null;
            Component[] allComponents = uiObject.GetComponents<Component>();

            foreach (var comp in allComponents)
            {
                if (comp != null)
                {
                    Type type = comp.GetType();
                    if (type.Name == "MenuOptionHorizontal" ||
                        type.BaseType?.Name == "MenuOptionHorizontal" ||
                        (type.BaseType?.BaseType != null && type.BaseType.BaseType.Name == "MenuOptionHorizontal"))
                    {
                        menuOptionHorizontal = comp;
                        break;
                    }
                }
            }

            if (menuOptionHorizontal != null)
            {
                return BuildMenuOptionHorizontalDescription(menuOptionHorizontal, uiObject);
            }

            // Handle MenuAudioSlider (volume sliders)
            Component menuAudioSlider = uiObject.GetComponent("MenuAudioSlider");
            if (menuAudioSlider != null)
            {
                return BuildMenuAudioSliderDescription(menuAudioSlider, uiObject);
            }

            // Check if this is a Slider whose parent has MenuAudioSlider
            var slider = uiObject.GetComponent<Slider>();
            if (slider != null && uiObject.transform.parent != null)
            {
                Component parentMenuAudioSlider = uiObject.transform.parent.GetComponent("MenuAudioSlider");
                if (parentMenuAudioSlider != null)
                {
                    return BuildMenuAudioSliderDescription(parentMenuAudioSlider, uiObject.transform.parent.gameObject);
                }
            }

            // Handle Toggle
            var toggle = uiObject.GetComponent<Toggle>();
            if (toggle != null)
            {
                return BuildToggleDescription(toggle, uiObject);
            }

            // Handle standard Slider (not part of MenuAudioSlider)
            if (slider != null)
            {
                return BuildSliderDescription(slider, uiObject);
            }

            // Generic fallback for buttons and other elements
            string text = GetUIElementText(uiObject);

            if (string.IsNullOrEmpty(text))
            {
                Plugin.Logger.LogInfo($"Empty text after filtering for object: {uiObject.name}");
                return "";
            }

            var button = uiObject.GetComponent<Button>();
            if (button != null)
            {
                return $"{text}, botón";
            }

            return text;
        }

        private string BuildMenuOptionHorizontalDescription(Component menuOption, GameObject uiObject)
        {
            Type type = menuOption.GetType();
            FieldInfo optionTextField = type.GetField("optionText", BindingFlags.Public | BindingFlags.Instance);
            Text optionText = optionTextField?.GetValue(menuOption) as Text;

            // Get label
            Text[] allTexts = uiObject.GetComponentsInChildren<Text>();
            string labelText = "";
            List<string> additionalTexts = new List<string>();

            foreach (var textComp in allTexts)
            {
                if (!string.IsNullOrEmpty(textComp.text))
                {
                    if (textComp.text.Contains(":"))
                    {
                        labelText = textComp.text.Replace(":", "").Trim();
                    }
                    else if (textComp.gameObject != uiObject &&
                             textComp != optionText &&
                             textComp.GetComponentInParent<Button>() == null &&
                             textComp.text.Length > 1)
                    {
                        string cleanText = textComp.text.Replace("\n", " ").Replace("\r", " ").Trim();
                        if (!cleanText.Contains("Conecta") && !cleanText.ToLower().Contains("connect"))
                        {
                            while (cleanText.Contains("  "))
                                cleanText = cleanText.Replace("  ", " ");
                            additionalTexts.Add(cleanText);
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(labelText))
                labelText = uiObject.name;

            string valueText = optionText != null ? optionText.text.Replace("\n", " ").Trim() : "";

            // Value changes are now handled by MenuOptionHorizontalPatches

            string announcement = labelText;
            if (!string.IsNullOrEmpty(valueText))
                announcement = $"{labelText}: {valueText}";

            if (additionalTexts.Count > 0)
                announcement += ". " + string.Join(". ", additionalTexts);

            return announcement;
        }

        private string BuildMenuAudioSliderDescription(Component menuAudioSlider, GameObject uiObject)
        {
            Type type = menuAudioSlider.GetType();
            FieldInfo sliderField = type.GetField("slider", BindingFlags.Public | BindingFlags.Instance);

            string labelText = "";
            Text[] allTexts = uiObject.GetComponentsInChildren<Text>();
            foreach (var textComp in allTexts)
            {
                if (!string.IsNullOrEmpty(textComp.text) && textComp.text.Contains(":"))
                {
                    labelText = textComp.text.Replace(":", "").Trim();
                    break;
                }
            }

            if (sliderField != null)
            {
                Slider audioSlider = sliderField.GetValue(menuAudioSlider) as Slider;
                if (audioSlider != null)
                {
                    int sliderValue = Mathf.RoundToInt(audioSlider.value);

                    // Value changes are now handled by MenuAudioSliderPatches

                    return $"{labelText}: {sliderValue}";
                }
            }

            return labelText;
        }

        private string BuildToggleDescription(Toggle toggle, GameObject uiObject)
        {
            Text[] childTexts = uiObject.GetComponentsInChildren<Text>();
            string labelText = "";
            string stateText = null;

            foreach (var textComp in childTexts)
            {
                if (!string.IsNullOrEmpty(textComp.text))
                {
                    if (textComp.text.Contains(":"))
                        labelText = textComp.text.Replace(":", "").Trim();
                    else if (textComp.gameObject != uiObject)
                        stateText = textComp.text.Trim();
                }
            }

            if (string.IsNullOrEmpty(stateText))
                stateText = toggle.isOn ? "activado" : "desactivado";

            if (!string.IsNullOrEmpty(labelText))
                return $"{labelText}: {stateText}";

            // Don't announce toggles without labels (avoid generic "activado" announcements)
            if (string.IsNullOrEmpty(labelText))
                return "";

            return stateText;
        }

        private string BuildSliderDescription(Slider slider, GameObject uiObject)
        {
            string labelText = GetUIElementText(uiObject);
            int sliderValue = Mathf.RoundToInt(slider.value);

            return $"{labelText}: {sliderValue}";
        }

        private string GetUIElementText(GameObject uiObject)
        {
            var text = uiObject.GetComponent<Text>();
            if (text != null && !string.IsNullOrEmpty(text.text))
            {
                if (text.text.Contains("Conecta") || text.text.ToLower().Contains("connect"))
                    return GetCleanObjectName(uiObject.name);
                return text.text;
            }

            text = uiObject.GetComponentInChildren<Text>();
            if (text != null && !string.IsNullOrEmpty(text.text))
            {
                if (text.text.Contains("Conecta") || text.text.ToLower().Contains("connect"))
                    return GetCleanObjectName(uiObject.name);
                return text.text;
            }

            if (uiObject.transform.parent != null)
            {
                text = uiObject.transform.parent.GetComponentInChildren<Text>();
                if (text != null && !string.IsNullOrEmpty(text.text))
                {
                    if (text.text.Contains("Conecta") || text.text.ToLower().Contains("connect"))
                        return GetCleanObjectName(uiObject.name);
                    return text.text;
                }
            }

            return GetCleanObjectName(uiObject.name);
        }

        private string GetCleanObjectName(string objectName)
        {
            if (string.IsNullOrEmpty(objectName))
                return "";

            string[] technicalKeywords = new string[]
            {
                "Prompt", "QuitGame", "ReturnMenu", "ExitToMenu",
                "Resolution", "Canvas", "Panel", "Container", "Holder"
            };

            foreach (string keyword in technicalKeywords)
            {
                if (objectName.Contains(keyword))
                {
                    Plugin.Logger.LogInfo($"Filtered technical GameObject name: {objectName}");
                    return "";
                }
            }

            if (objectName.EndsWith("Menu") || objectName.EndsWith("Screen"))
            {
                Plugin.Logger.LogInfo($"Filtered technical GameObject name (ends with Menu/Screen): {objectName}");
                return "";
            }

            return objectName;
        }

        private string BuildCharmInventoryDescription(Component invCharmBackboard)
        {
            try
            {
                Type type = invCharmBackboard.GetType();
                FieldInfo charmNumField = type.GetField("charmNum", BindingFlags.Public | BindingFlags.Instance);

                if (charmNumField == null)
                    return "";

                int charmNum = (int)charmNumField.GetValue(invCharmBackboard);
                PlayerData pd = PlayerData.instance;

                // Check if player has this charm
                bool hasCharm = pd.GetBool($"gotCharm_{charmNum}");

                if (!hasCharm)
                {
                    return "Encanto no obtenido";
                }

                // Get charm details
                string charmName = Language.Language.Get($"CHARM_NAME_{charmNum}", "UI");
                string charmDescription = Language.Language.Get($"CHARM_DESC_{charmNum}", "UI");
                int notchesCost = pd.GetInt($"charmCost_{charmNum}");
                bool isEquipped = pd.GetBool($"equippedCharm_{charmNum}");
                bool isNew = pd.GetBool($"newCharm_{charmNum}");

                // Get current notch status
                int maxNotches = pd.GetInt("maxCharmNotches");
                int notchesUsed = maxNotches - pd.GetInt("charmNotches");

                // Build announcement
                string statusText = isEquipped ? "Equipado" : "No equipado";
                string newText = isNew ? " (Nuevo)" : "";

                return $"{charmName}{newText}. {statusText}. Costo: {notchesCost} muesca{(notchesCost != 1 ? "s" : "")}. Muescas disponibles: {pd.GetInt("charmNotches")} de {maxNotches}. {charmDescription}";
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Error in BuildCharmInventoryDescription: {ex}");
                return "";
            }
        }

        private string BuildItemInventoryDescription(Component invItemDisplay, GameObject uiObject)
        {
            try
            {
                Type type = invItemDisplay.GetType();
                FieldInfo playerDataBoolField = type.GetField("playerDataBool", BindingFlags.Public | BindingFlags.Instance);

                if (playerDataBoolField == null)
                    return "";

                string playerDataBool = (string)playerDataBoolField.GetValue(invItemDisplay);
                bool hasItem = PlayerData.instance.GetBool(playerDataBool);

                // Try to get a readable name from the GameObject hierarchy
                string itemName = uiObject.name;
                if (itemName.Contains("_"))
                    itemName = itemName.Replace("_", " ");

                // Try to get localized text if available
                Text[] texts = uiObject.GetComponentsInChildren<Text>();
                if (texts.Length > 0)
                {
                    foreach (var text in texts)
                    {
                        if (!string.IsNullOrEmpty(text.text))
                        {
                            itemName = text.text;
                            break;
                        }
                    }
                }

                string status = hasItem ? "Obtenido" : "No obtenido";
                return $"{itemName}. {status}";
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Error in BuildItemInventoryDescription: {ex}");
                return "";
            }
        }
    }
}

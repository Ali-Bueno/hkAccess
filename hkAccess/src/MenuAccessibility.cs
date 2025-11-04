using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace HKAccessibility;

public class MenuAccessibility : MonoBehaviour
{
    private static ManualLogSource Logger => Plugin.Logger;
    private GameObject lastSelectedObject;
    private string lastSceneName;
    private List<Canvas> announcedCanvases = new List<Canvas>();
    private float announceDelay = 0.1f;
    private float lastAnnounceTime;
    private float lastValueChangeTime;
    private float valueChangeDelay = 0.3f;
    private Dictionary<Slider, Coroutine> activeSliderMonitors = new Dictionary<Slider, Coroutine>();
    private Dictionary<Dropdown, int> lastDropdownValues = new Dictionary<Dropdown, int>();

    private void Start()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneChanged;
    }

    private void OnDestroy()
    {
        Cleanup();
    }

    public void Cleanup()
    {
        Logger.LogInfo("MenuAccessibility: Starting cleanup...");

        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= OnSceneChanged;

        StopAllCoroutines();

        foreach (var kvp in activeSliderMonitors)
        {
            if (kvp.Value != null)
            {
                StopCoroutine(kvp.Value);
            }
        }
        activeSliderMonitors.Clear();

        lastDropdownValues.Clear();
        announcedCanvases.Clear();

        lastSelectedObject = null;
        lastSceneName = null;

        Logger.LogInfo("MenuAccessibility: Cleanup completed.");
    }

    private void OnSceneChanged(Scene oldScene, Scene newScene)
    {
        lastSceneName = newScene.name;
        announcedCanvases.Clear();
        lastDropdownValues.Clear();

        foreach (var kvp in activeSliderMonitors)
        {
            if (kvp.Value != null)
            {
                StopCoroutine(kvp.Value);
            }
        }
        activeSliderMonitors.Clear();

        StartCoroutine(AnnounceSceneTitleDelayed());
    }

    private IEnumerator AnnounceSceneTitleDelayed()
    {
        yield return new WaitForSeconds(0.8f);
        AnnounceSceneTitle();
    }

    private void Update()
    {
        try
        {
            CheckMenuNavigation();
            CheckForPopups();
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error in MenuAccessibility.Update: {ex.Message}");
        }
    }

    private void CheckForPopups()
    {
        // DESACTIVADO: Los popups ahora se manejan mediante Harmony patches en UIManagerPatches
        // Esta función causaba duplicación y anunciaba nombres técnicos de GameObjects
        // Ver: UIManagerPatches.OnShowQuitGamePrompt, OnShowReturnMenuPrompt, OnShowResolutionPrompt
        return;
    }


    private void AnnounceSceneTitle()
    {
        Text[] allTexts = FindObjectsOfType<Text>();
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

            // Doble verificación para filtrar mensajes de mando
            if (announcement.Contains("Conecta") || announcement.ToLower().Contains("connect"))
            {
                Logger.LogInfo($"Filtered controller message: {announcement}");
                return;
            }

            Logger.LogInfo($"Menu title: {announcement}");
            TolkBridge.Output(announcement, true);
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
            Logger.LogInfo($"Announcing: {announcement}");
            TolkBridge.Output(announcement, true);
        }
    }

    private string GetFullUIDescription(GameObject uiObject)
    {
        Component[] allComponents = uiObject.GetComponents<Component>();
        Component menuOptionHorizontal = null;

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
            Type type = menuOptionHorizontal.GetType();
            Logger.LogInfo($"Found MenuOptionHorizontal type: {type.Name}");

            // Get the optionText field directly using reflection
            FieldInfo optionTextField = type.GetField("optionText", BindingFlags.Public | BindingFlags.Instance);
            Text optionText = optionTextField?.GetValue(menuOptionHorizontal) as Text;

            // Get label from texts that contain ":"
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
                        // Collect additional descriptive texts (not buttons, not the option value itself)
                        // Clean up newlines and extra whitespace
                        string cleanText = textComp.text.Replace("\n", " ").Replace("\r", " ").Trim();

                        // Filtrar mensajes de mando
                        if (cleanText.Contains("Conecta") || cleanText.ToLower().Contains("connect"))
                            continue;

                        // Remove multiple spaces
                        while (cleanText.Contains("  "))
                        {
                            cleanText = cleanText.Replace("  ", " ");
                        }
                        additionalTexts.Add(cleanText);
                    }
                }
            }

            if (string.IsNullOrEmpty(labelText))
            {
                labelText = uiObject.name;
            }

            // Get the actual value from optionText field
            string valueText = optionText != null ? optionText.text.Replace("\n", " ").Trim() : "";

            Logger.LogInfo($"Label: {labelText}, Value: {valueText}, Additional texts: {string.Join(" | ", additionalTexts)}");

            StartCoroutine(MonitorMenuOptionHorizontal(menuOptionHorizontal, labelText));

            // Build full announcement
            string announcement = labelText;
            if (!string.IsNullOrEmpty(valueText))
            {
                announcement = $"{labelText}: {valueText}";
            }

            // Add additional descriptive text if available
            if (additionalTexts.Count > 0)
            {
                announcement += ". " + string.Join(". ", additionalTexts);
            }

            return announcement;
        }

        Component menuAudioSlider = uiObject.GetComponent("MenuAudioSlider");
        if (menuAudioSlider != null)
        {
            Type type = menuAudioSlider.GetType();
            FieldInfo sliderField = type.GetField("slider", BindingFlags.Public | BindingFlags.Instance);

            string labelText = "";
            Text[] allTexts = uiObject.GetComponentsInChildren<Text>();
            foreach (var textComp2 in allTexts)
            {
                if (!string.IsNullOrEmpty(textComp2.text) && textComp2.text.Contains(":"))
                {
                    labelText = textComp2.text.Replace(":", "").Trim();
                    break;
                }
            }

            if (sliderField != null)
            {
                Slider audioSlider = sliderField.GetValue(menuAudioSlider) as Slider;
                if (audioSlider != null)
                {
                    int sliderValue = Mathf.RoundToInt(audioSlider.value);

                    if (activeSliderMonitors.ContainsKey(audioSlider))
                    {
                        StopCoroutine(activeSliderMonitors[audioSlider]);
                        activeSliderMonitors.Remove(audioSlider);
                    }

                    Coroutine monitorCoroutine = StartCoroutine(MonitorSliderChanges(audioSlider, labelText));
                    activeSliderMonitors[audioSlider] = monitorCoroutine;

                    return $"{labelText}: {sliderValue}";
                }
            }

            return labelText;
        }

        var toggle = uiObject.GetComponent<Toggle>();
        if (toggle != null)
        {
            Text[] childTexts = uiObject.GetComponentsInChildren<Text>();
            string labelText = "";
            string stateText = null;

            foreach (var textComp3 in childTexts)
            {
                if (!string.IsNullOrEmpty(textComp3.text))
                {
                    if (textComp3.text.Contains(":"))
                    {
                        labelText = textComp3.text.Replace(":", "").Trim();
                    }
                    else if (textComp3.gameObject != uiObject)
                    {
                        stateText = textComp3.text.Trim();
                    }
                }
            }

            if (string.IsNullOrEmpty(stateText))
            {
                stateText = toggle.isOn ? "activado" : "desactivado";
            }

            StartCoroutine(MonitorToggleChanges(toggle, labelText));

            if (!string.IsNullOrEmpty(labelText))
            {
                return $"{labelText}: {stateText}";
            }
            return stateText;
        }

        var slider = uiObject.GetComponent<Slider>();
        if (slider != null)
        {
            string labelText = GetUIElementText(uiObject);
            int sliderValue = Mathf.RoundToInt(slider.value);

            if (activeSliderMonitors.ContainsKey(slider))
            {
                StopCoroutine(activeSliderMonitors[slider]);
                activeSliderMonitors.Remove(slider);
            }

            Coroutine monitorCoroutine = StartCoroutine(MonitorSliderChanges(slider, labelText));
            activeSliderMonitors[slider] = monitorCoroutine;

            return $"{labelText}: {sliderValue}";
        }

        string text = GetUIElementText(uiObject);

        // Si el texto está vacío después de filtrar, no anunciar nada
        if (string.IsNullOrEmpty(text))
        {
            Logger.LogInfo($"Empty text after filtering for object: {uiObject.name}");
            return "";
        }

        var button = uiObject.GetComponent<Button>();
        if (button != null)
        {
            return $"{text}, botón";
        }

        return text;
    }

    private IEnumerator MonitorSliderChanges(Slider slider, string label)
    {
        if (slider == null) yield break;

        int lastValue = Mathf.RoundToInt(slider.value);
        GameObject sliderParent = slider.transform.parent?.gameObject;

        while (slider != null)
        {
            GameObject currentSelected = EventSystem.current?.currentSelectedGameObject;
            if (currentSelected == null || (currentSelected != slider.gameObject && currentSelected != sliderParent))
            {
                break;
            }

            int currentValue = Mathf.RoundToInt(slider.value);
            if (currentValue != lastValue &&
                Time.realtimeSinceStartup - lastValueChangeTime > 0.2f)
            {
                lastValue = currentValue;
                lastValueChangeTime = Time.realtimeSinceStartup;

                TolkBridge.Output(currentValue.ToString(), true);
            }

            yield return new WaitForSeconds(0.1f);
        }

        if (activeSliderMonitors.ContainsKey(slider))
        {
            activeSliderMonitors.Remove(slider);
        }
    }

    private IEnumerator MonitorMenuOptionHorizontal(Component menuOption, string label)
    {
        if (menuOption == null)
        {
            Logger.LogWarning("MonitorMenuOptionHorizontal: menuOption is null");
            yield break;
        }

        Type type = menuOption.GetType();
        FieldInfo optionTextField = type.GetField("optionText", BindingFlags.Public | BindingFlags.Instance);

        if (optionTextField == null)
        {
            Logger.LogWarning("MonitorMenuOptionHorizontal: optionText field not found");
            yield break;
        }

        Text optionText = optionTextField.GetValue(menuOption) as Text;
        if (optionText == null)
        {
            Logger.LogWarning("MonitorMenuOptionHorizontal: optionText is null");
            yield break;
        }

        string lastText = optionText.text;
        Logger.LogInfo($"Starting monitor for {label}, initial value: {lastText}");

        while (menuOption != null)
        {
            GameObject currentSelected = EventSystem.current?.currentSelectedGameObject;
            if (currentSelected != menuOption.gameObject)
            {
                Logger.LogInfo($"Monitor stopped for {label} - no longer selected");
                break;
            }

            string currentText = optionText.text;
            if (currentText != lastText && !string.IsNullOrEmpty(currentText))
            {
                if (Time.realtimeSinceStartup - lastValueChangeTime > 0.1f)
                {
                    lastText = currentText;
                    lastValueChangeTime = Time.realtimeSinceStartup;

                    Logger.LogInfo($"MenuOption changed to: {currentText}");
                    TolkBridge.Output(currentText, true);
                }
            }

            yield return new WaitForSeconds(0.05f);
        }
    }

    private IEnumerator MonitorToggleChanges(Toggle toggle, string label)
    {
        if (toggle == null) yield break;

        bool lastState = toggle.isOn;

        while (toggle != null)
        {
            GameObject currentSelected = EventSystem.current?.currentSelectedGameObject;
            if (currentSelected != toggle.gameObject)
            {
                break;
            }

            if (toggle.isOn != lastState &&
                Time.realtimeSinceStartup - lastValueChangeTime > 0.15f)
            {
                lastState = toggle.isOn;
                lastValueChangeTime = Time.realtimeSinceStartup;

                Text[] childTexts = toggle.GetComponentsInChildren<Text>();
                string stateText = null;

                foreach (var text in childTexts)
                {
                    if (!string.IsNullOrEmpty(text.text) &&
                        text.gameObject != toggle.gameObject &&
                        text.text != label)
                    {
                        stateText = text.text;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(stateText))
                {
                    stateText = toggle.isOn ? "activado" : "desactivado";
                }

                Logger.LogInfo($"Toggle changed to: {stateText}");
                TolkBridge.Output(stateText, true);
            }

            yield return new WaitForSeconds(0.05f);
        }
    }

    private string GetUIElementText(GameObject uiObject)
    {
        var text = uiObject.GetComponent<Text>();
        if (text != null && !string.IsNullOrEmpty(text.text))
        {
            // Filtrar textos no deseados
            if (text.text.Contains("Conecta") || text.text.ToLower().Contains("connect"))
                return GetCleanObjectName(uiObject.name);

            return text.text;
        }

        text = uiObject.GetComponentInChildren<Text>();
        if (text != null && !string.IsNullOrEmpty(text.text))
        {
            // Filtrar textos no deseados
            if (text.text.Contains("Conecta") || text.text.ToLower().Contains("connect"))
                return GetCleanObjectName(uiObject.name);

            return text.text;
        }

        if (uiObject.transform.parent != null)
        {
            text = uiObject.transform.parent.GetComponentInChildren<Text>();
            if (text != null && !string.IsNullOrEmpty(text.text))
            {
                // Filtrar textos no deseados
                if (text.text.Contains("Conecta") || text.text.ToLower().Contains("connect"))
                    return GetCleanObjectName(uiObject.name);

                return text.text;
            }
        }

        return GetCleanObjectName(uiObject.name);
    }

    private string GetCleanObjectName(string objectName)
    {
        // Filtrar nombres técnicos de GameObjects que no deberían anunciarse
        if (string.IsNullOrEmpty(objectName))
            return "";

        // Lista de palabras que indican un nombre técnico que no queremos anunciar
        string[] technicalKeywords = new string[]
        {
            "Prompt",
            "QuitGame",
            "ReturnMenu",
            "ExitToMenu",
            "Resolution",
            "Canvas",
            "Panel",
            "Container",
            "Holder"
        };

        foreach (string keyword in technicalKeywords)
        {
            if (objectName.Contains(keyword))
            {
                Logger.LogInfo($"Filtered technical GameObject name: {objectName}");
                return ""; // Devolver vacío en lugar del nombre técnico
            }
        }

        // Si el nombre termina en "Menu" o "Screen", también filtrarlo
        if (objectName.EndsWith("Menu") || objectName.EndsWith("Screen"))
        {
            Logger.LogInfo($"Filtered technical GameObject name (ends with Menu/Screen): {objectName}");
            return "";
        }

        return objectName;
    }
}

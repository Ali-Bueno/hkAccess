using System.Collections;
using HutongGames.PlayMaker;
using UnityEngine;

namespace HKAccessibility
{
    /// <summary>
    /// Simple inventory reader that announces selected items
    /// </summary>
    public class InventoryReader : MonoBehaviour
    {
        private PlayMakerFSM charmsFSM;
        private int lastSelectedCharm = -1;
        private bool inventoryOpen = false;
        private bool didInitialInventorySnapshot = false;
        private readonly System.Collections.Generic.HashSet<string> announcedInventoryTexts = new System.Collections.Generic.HashSet<string>();
        private string lastSpokenSelectionSummary;
        private readonly System.Collections.Generic.Dictionary<int, string> lastVisibleTextById = new System.Collections.Generic.Dictionary<int, string>();
        private bool aggregateScheduled = false;
        private float lastCharmsFindAttempt;
        private bool suppressCharmsLookupThisOpen;

        void Start()
        {
            Plugin.Logger.LogInfo("[InventoryReader] Started");
            StartCoroutine(MonitorInventory());
        }

        private IEnumerator MonitorInventory()
        {
            while (true)
            {
                // Check if we're in a gameplay scene
                string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                bool isGameplay = !sceneName.Contains("Menu") && !sceneName.Contains("menu");

                if (!isGameplay)
                {
                    yield return new WaitForSeconds(1f);
                    continue;
                }

                // Check if inventory is open
                bool wasOpen = inventoryOpen;
                inventoryOpen = IsInventoryOpen();

                if (inventoryOpen && !wasOpen)
                {
                    Plugin.Logger.LogInfo("[InventoryReader] Inventory opened");
                    TolkScreenReader.Instance.Speak("Inventario abierto", true);
                    didInitialInventorySnapshot = false;
                    announcedInventoryTexts.Clear();
                    lastVisibleTextById.Clear();
                    aggregateScheduled = false;
                    suppressCharmsLookupThisOpen = false;
                    lastCharmsFindAttempt = 0f;
                }
                else if (!inventoryOpen && wasOpen)
                {
                    Plugin.Logger.LogInfo("[InventoryReader] Inventory closed");
                    lastSelectedCharm = -1;
                    didInitialInventorySnapshot = false;
                    announcedInventoryTexts.Clear();
                    lastVisibleTextById.Clear();
                    aggregateScheduled = false;
                    suppressCharmsLookupThisOpen = false;
                }

                // If inventory is open, monitor charm selection
                if (inventoryOpen)
                {
                    CheckCharmSelection();
                    MonitorAndAggregateInventoryTexts();
                }

                yield return new WaitForSeconds(0.1f);
            }
        }

        private bool IsInventoryOpen()
        {
            // Check if the inventory FSM says it's open
            if (GameManager.instance?.inventoryFSM != null)
            {
                FsmBool openVar = GameManager.instance.inventoryFSM.FsmVariables.FindFsmBool("Open");
                if (openVar != null)
                {
                    return openVar.Value;
                }
            }

            // Alternative: check if Charms Pane exists
            GameObject charmsPane = GameObject.FindWithTag("Charms Pane");
            return charmsPane != null && charmsPane.activeInHierarchy;
        }

        private void CheckCharmSelection()
        {
            // Try to find the Charms FSM if we don't have it
            if (charmsFSM == null)
            {
                if (!suppressCharmsLookupThisOpen)
                {
                    if (Time.unscaledTime - lastCharmsFindAttempt >= 2f)
                    {
                        lastCharmsFindAttempt = Time.unscaledTime;
                        var charmsPane = GameObject.FindWithTag("Charms Pane");
                        if (charmsPane != null)
                        {
                            Plugin.Logger.LogInfo("[InventoryReader] Found Charms Pane GameObject");
                            charmsFSM = PlayMakerFSM.FindFsmOnGameObject(charmsPane, "UI Charms");
                            if (charmsFSM != null)
                            {
                                Plugin.Logger.LogInfo("[InventoryReader] Successfully found UI Charms FSM!");
                                LogFSMVariables();
                            }
                            else
                            {
                                Plugin.Logger.LogWarning("[InventoryReader] Could not find UI Charms FSM");
                                suppressCharmsLookupThisOpen = true;
                            }
                        }
                        else
                        {
                            Plugin.Logger.LogInfo("[InventoryReader] Charms Pane not found - skipping lookup this open");
                            suppressCharmsLookupThisOpen = true;
                        }
                    }
                }
            }

            if (charmsFSM == null)
                return;

            // Check for selected charm in various FSM variables
            int currentCharm = GetSelectedCharmNumber();

            if (currentCharm > 0 && currentCharm != lastSelectedCharm)
            {
                lastSelectedCharm = currentCharm;
                AnnounceCharm(currentCharm);
            }
            else if (currentCharm == -1)
            {
                // Log current state to debug
                Plugin.Logger.LogInfo($"[InventoryReader] No charm selected. FSM State: {charmsFSM.ActiveStateName}");
            }
        }

        private void MonitorAndAggregateInventoryTexts()
        {
            var invFsm = GameManager.instance?.inventoryFSM;
            if (invFsm == null) return;

            var invRoot = invFsm.gameObject; // Inventory root on HudCamera
            if (invRoot == null || !invRoot.activeInHierarchy) return;

            // On first open, take a snapshot so we don't speak all initial static labels
            if (!didInitialInventorySnapshot)
            {
                SnapshotVisibleTexts(invRoot);
                didInitialInventorySnapshot = true;
                return;
            }

            bool anyChange = false;

            var tmpu = invRoot.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
            foreach (var t in tmpu)
            {
                if (t == null || !t.isActiveAndEnabled || t.alpha <= 0.1f) continue;
                var s = CleanText(t.text);
                if (string.IsNullOrEmpty(s)) continue;
                int id = t.GetInstanceID();
                if (!lastVisibleTextById.TryGetValue(id, out var prev) || prev != s)
                {
                    lastVisibleTextById[id] = s;
                    anyChange = true;
                }
            }

            var tmp = invRoot.GetComponentsInChildren<TMPro.TextMeshPro>(true);
            foreach (var t in tmp)
            {
                if (t == null || !t.isActiveAndEnabled || t.alpha <= 0.1f) continue;
                var s = CleanText(t.text);
                if (string.IsNullOrEmpty(s)) continue;
                int id = t.GetInstanceID();
                if (!lastVisibleTextById.TryGetValue(id, out var prev) || prev != s)
                {
                    lastVisibleTextById[id] = s;
                    anyChange = true;
                }
            }

            // tk2dTextMesh (2D Toolkit) used widely in inventory UI
            var tk2d = invRoot.GetComponentsInChildren<tk2dTextMesh>(true);
            foreach (var t in tk2d)
            {
                if (t == null || !t.gameObject.activeInHierarchy) continue;
                var rend = t.GetComponent<Renderer>();
                if (rend != null && !rend.enabled) continue;
                var s = CleanText(t.text);
                if (string.IsNullOrEmpty(s)) continue;
                int id = t.GetInstanceID();
                if (!lastVisibleTextById.TryGetValue(id, out var prev) || prev != s)
                {
                    lastVisibleTextById[id] = s;
                    anyChange = true;
                }
            }

            if (anyChange && !aggregateScheduled)
            {
                aggregateScheduled = true;
                StartCoroutine(AnnounceSelectionDetailsNextFrame(invRoot));
            }
        }

        private void SnapshotVisibleTexts(GameObject root)
        {
            var tmpuList = root.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
            foreach (var t in tmpuList)
            {
                CacheVisibleText(t, t.text, t.alpha, t.isActiveAndEnabled);
            }

            var tmpList = root.GetComponentsInChildren<TMPro.TextMeshPro>(true);
            foreach (var t in tmpList)
            {
                CacheVisibleText(t, t.text, t.alpha, t.isActiveAndEnabled);
            }

            // Legacy TextMesh snapshot omitted to avoid extra module references

            // tk2dTextMesh snapshot
            var tk2d = root.GetComponentsInChildren<tk2dTextMesh>(true);
            foreach (var t in tk2d)
            {
                bool vis = t != null && t.gameObject.activeInHierarchy && (t.GetComponent<Renderer>()?.enabled ?? true);
                if (!vis) continue;
                CacheVisibleText(t, t.text, 1f, true);
            }
        }

        private System.Collections.IEnumerator AnnounceSelectionDetailsNextFrame(GameObject invRoot)
        {
            // small delay to let UI update name/desc texts
            yield return new WaitForSeconds(0.12f);

            try
            {
                aggregateScheduled = false;
                var texts = new System.Collections.Generic.List<(Component c, float y, string s)>();
                var tmpu = invRoot.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
                foreach (var t in tmpu)
                {
                    if (t == null || !t.isActiveAndEnabled || t.alpha <= 0.1f) continue;
                    var s = CleanText(t.text);
                    if (string.IsNullOrEmpty(s)) continue;
                    texts.Add((t, t.transform.position.y, s));
                }
                var tmp = invRoot.GetComponentsInChildren<TMPro.TextMeshPro>(true);
                foreach (var t in tmp)
                {
                    if (t == null || !t.isActiveAndEnabled || t.alpha <= 0.1f) continue;
                    var s = CleanText(t.text);
                    if (string.IsNullOrEmpty(s)) continue;
                    texts.Add((t, t.transform.position.y, s));
                }

                // tk2dTextMesh
                var tk2d = invRoot.GetComponentsInChildren<tk2dTextMesh>(true);
                foreach (var t in tk2d)
                {
                    if (t == null || !t.gameObject.activeInHierarchy) continue;
                    var rend = t.GetComponent<Renderer>();
                    if (rend != null && !rend.enabled) continue;
                    var s = CleanText(t.text);
                    if (string.IsNullOrEmpty(s)) continue;
                    texts.Add((t, t.transform.position.y, s));
                }

                if (texts.Count == 0) yield break;

                // Identify description as the longest meaningful paragraph
                string name = null;
                string desc = null;
                float descY = 0f;

                bool IsHeader(string s)
                {
                    if (string.IsNullOrEmpty(s)) return true;
                    string low = s.Trim().ToLowerInvariant();
                    switch (low)
                    {
                        case "inventario":
                        case "inventar":
                        case "inventaire":
                        case "inventário":
                        case "inventario ":
                        case "inventory":
                        case "charms":
                        case "amuletos":
                        case "amuleti":
                        case "amulette":
                        case "objetos":
                        case "objets":
                        case "items":
                        case "reliquias":
                        case "reliquie":
                        case "relics":
                        case "map":
                        case "journal":
                            return true;
                    }
                    return false;
                }

                // Prefer the single longest string as description candidate
                int longestLen = 0;
                foreach (var entry in texts)
                {
                    var l = entry.s;
                    if (string.IsNullOrEmpty(l)) continue;
                    var low = l.ToLowerInvariant();
                    if (low.Contains("conecta") || low.Contains("connect")) continue;
                    if (l.Length < 8) continue;
                    if (IsHeader(l)) continue;
                    if (int.TryParse(l, out _)) continue;
                    if (l.Length > longestLen)
                    {
                        longestLen = l.Length;
                        desc = l.Length > 220 ? l.Substring(0, 220) : l;
                        descY = entry.y;
                    }
                }

                // Choose a short-ish name near the description vertically (usually above it)
                int bestNameLen = int.MaxValue;
                float bestNameDy = float.MaxValue;
                foreach (var entry in texts)
                {
                    var l = entry.s;
                    if (string.IsNullOrEmpty(l)) continue;
                    var low = l.ToLowerInvariant();
                    if (low.Contains("conecta") || low.Contains("connect")) continue;
                    if (IsHeader(l)) continue;
                    if (int.TryParse(l, out _)) continue;
                    if (desc != null && ReferenceEquals(l, desc)) continue;

                    int len = l.Length;
                    if (len > 2 && len <= 64)
                    {
                        float dy = (desc != null) ? Mathf.Abs(entry.y - descY) : 0f;
                        // Prefer names close to desc vertically and with smaller length
                        if (dy <= bestNameDy + 0.05f && len < bestNameLen)
                        {
                            bestNameLen = len;
                            bestNameDy = dy;
                            name = l;
                        }
                    }
                }

                string combined = null;

                // Look for quantity displayed near the item name
                // The game uses DisplayItemAmount components that show quantities as separate text elements
                string quantity = null;
                if (!string.IsNullOrEmpty(name))
                {
                    // Search for a numeric text near the name (usually to the right or below)
                    foreach (var entry in texts)
                    {
                        if (int.TryParse(entry.s, out int qty))
                        {
                            // Check if this number is close to the name (within reasonable distance)
                            float distance = Mathf.Abs(entry.y - (texts.Find(t => t.s == name).y));
                            if (distance < 1.5f) // Close enough vertically
                            {
                                quantity = qty.ToString();
                                break;
                            }
                        }
                    }
                }

                string nameWithQty = name;
                if (!string.IsNullOrEmpty(quantity)) nameWithQty = $"{name} ({quantity})";

                if (!string.IsNullOrEmpty(nameWithQty) && !string.IsNullOrEmpty(desc)) combined = nameWithQty + ". " + desc;
                else if (!string.IsNullOrEmpty(desc) && string.IsNullOrEmpty(nameWithQty)) combined = desc;
                else combined = nameWithQty;

                if (!string.IsNullOrEmpty(combined) && combined != lastSpokenSelectionSummary)
                {
                    lastSpokenSelectionSummary = combined;
                    Plugin.Logger.LogInfo($"[InventoryReader] Speaking selection summary: {combined}");
                    // Interrupt to cancel any previous scanning speech
                    TolkScreenReader.Instance.Speak(combined, true);
                }
            }
            catch (System.Exception ex)
            {
                Plugin.Logger.LogError($"[InventoryReader] Error aggregating selection details: {ex}");
            }
        }

        private void CacheVisibleText(Component comp, string text, float alpha, bool enabled)
        {
            if (comp == null) return;
            if (!enabled) return;
            if (alpha <= 0.1f) return;
            text = CleanText(text);
            if (string.IsNullOrEmpty(text)) return;
            string key = comp.GetInstanceID() + "|" + text;
            announcedInventoryTexts.Add(key);
        }

        private void AnnounceIfNewVisibleText(Component comp, string text, float alpha, bool enabled)
        {
            if (comp == null) return;
            if (!enabled) return;
            if (alpha <= 0.1f) return;
            text = CleanText(text);
            if (string.IsNullOrEmpty(text)) return;
            string key = comp.GetInstanceID() + "|" + text;
            if (!announcedInventoryTexts.Contains(key))
            {
                announcedInventoryTexts.Add(key);
                if (!SpokenTextHistory.HasBeenSpoken(text))
                {
                    Plugin.Logger.LogInfo($"[InventoryReader] Speaking inventory text: {text}");
                    TolkScreenReader.Instance.Speak(text, true);
                }
            }
        }

        private string CleanText(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return string.Empty;
            string t = raw.Replace("\n", " ").Replace("\r", " ").Trim();
            while (t.Contains("  ")) t = t.Replace("  ", " ");
            if (t.Length < 2) return string.Empty;

            // Filter controller/connect prompts and button instructions
            string low = t.ToLowerInvariant();

            // Connection prompts
            if (low.Contains("conecta") || low.Contains("connect") || low.Contains("controlador") || low.Contains("controller"))
                return string.Empty;

            // Button press instructions (all languages)
            // English
            if (low.StartsWith("press") || low.StartsWith("hold") || low.StartsWith("tap"))
                return string.Empty;
            // Spanish
            if (low.StartsWith("presiona") || low.StartsWith("pulsa") || low.StartsWith("mantén") || low.StartsWith("mantenga") || low.StartsWith("manten"))
                return string.Empty;
            // Portuguese
            if (low.StartsWith("pressione") || low.StartsWith("segure") || low.StartsWith("mantenha"))
                return string.Empty;
            // Italian
            if (low.StartsWith("premi") || low.StartsWith("tieni") || low.StartsWith("mantieni"))
                return string.Empty;
            // French
            if (low.StartsWith("appuyer") || low.StartsWith("appuyez") || low.StartsWith("maintenir") || low.StartsWith("maintenez"))
                return string.Empty;
            // German
            if (low.StartsWith("drücken") || low.StartsWith("drucken") || low.StartsWith("halten"))
                return string.Empty;

            // Single word button prompts that might appear
            if (low == "press" || low == "hold" || low == "presiona" || low == "pressione" || low == "premi")
                return string.Empty;

            return t;
        }

        private void LogFSMVariables()
        {
            if (charmsFSM == null) return;

            Plugin.Logger.LogInfo("[InventoryReader] UI Charms FSM Variables:");

            Plugin.Logger.LogInfo("  Int Variables:");
            foreach (var intVar in charmsFSM.FsmVariables.IntVariables)
            {
                Plugin.Logger.LogInfo($"    - {intVar.Name} = {intVar.Value}");
            }

            Plugin.Logger.LogInfo("  String Variables:");
            foreach (var strVar in charmsFSM.FsmVariables.StringVariables)
            {
                Plugin.Logger.LogInfo($"    - {strVar.Name} = {strVar.Value}");
            }

            Plugin.Logger.LogInfo("  Bool Variables:");
            foreach (var boolVar in charmsFSM.FsmVariables.BoolVariables)
            {
                Plugin.Logger.LogInfo($"    - {boolVar.Name} = {boolVar.Value}");
            }
        }

        private int GetSelectedCharmNumber()
        {
            if (charmsFSM == null)
                return -1;

            // Try different variable names that might contain the selected charm
            string[] possibleVarNames = {
                "Current Item Number",
                "Charm Num Equiped",
                "Equipped Charm",
                "Charm To Inspect",
                "Selected Charm",
                "Current Charm"
            };

            foreach (string varName in possibleVarNames)
            {
                FsmInt charmVar = charmsFSM.FsmVariables.FindFsmInt(varName);
                if (charmVar != null && charmVar.Value > 0)
                {
                    return charmVar.Value;
                }
            }

            // Alternative: Check the active state name, sometimes it contains the charm number
            string stateName = charmsFSM.ActiveStateName;
            if (stateName.Contains("Charm"))
            {
                // Try to extract number from state name
                System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(stateName, @"\d+");
                if (match.Success && int.TryParse(match.Value, out int charmNum))
                {
                    return charmNum;
                }
            }

            return -1;
        }

        private void AnnounceCharm(int charmNum)
        {
            try
            {
                // Get charm name and description from localization
                string nameKey = $"CHARM_NAME_{charmNum}";
                string descKey = $"CHARM_DESC_{charmNum}";

                string charmName = "";
                string charmDesc = "";

                // Try to get from Language system
                if (Language.Language.Has(nameKey, "UI"))
                {
                    charmName = Language.Language.Get(nameKey, "UI");
                }
                else if (Language.Language.Has(nameKey))
                {
                    charmName = Language.Language.Get(nameKey);
                }

                if (Language.Language.Has(descKey, "UI"))
                {
                    charmDesc = Language.Language.Get(descKey, "UI");
                }
                else if (Language.Language.Has(descKey))
                {
                    charmDesc = Language.Language.Get(descKey);
                }

                // If we didn't find the name, try alternative formats
                if (string.IsNullOrEmpty(charmName) || charmName.StartsWith("#!#"))
                {
                    nameKey = $"CHARM_{charmNum}_NAME";
                    if (Language.Language.Has(nameKey))
                    {
                        charmName = Language.Language.Get(nameKey);
                    }
                }

                // Build announcement
                string announcement = "";

                if (!string.IsNullOrEmpty(charmName) && !charmName.StartsWith("#!#"))
                {
                    announcement = charmName;
                }
                else
                {
                    announcement = $"Amuleto {charmNum}";
                }

                if (!string.IsNullOrEmpty(charmDesc) && !charmDesc.StartsWith("#!#"))
                {
                    announcement += ". " + charmDesc;
                }

                // Check notch cost
                string notchCostVar = $"charmCost_{charmNum}";
                if (PlayerData.instance != null)
                {
                    int notchCost = PlayerData.instance.GetInt(notchCostVar);
                    if (notchCost > 0)
                    {
                        announcement += $". Costo: {notchCost} muesca{(notchCost > 1 ? "s" : "")}";
                    }

                    // Check if equipped
                    bool isEquipped = PlayerData.instance.GetBool($"equippedCharm_{charmNum}");
                    if (isEquipped)
                    {
                        announcement += ". Equipado";
                    }
                }

                Plugin.Logger.LogInfo($"[InventoryReader] Announcing charm {charmNum}: {announcement}");
                TolkScreenReader.Instance.Speak(announcement, true);
            }
            catch (System.Exception ex)
            {
                Plugin.Logger.LogError($"[InventoryReader] Error announcing charm: {ex}");
                TolkScreenReader.Instance.Speak($"Amuleto {charmNum}", true);
            }
        }
    }
}

using HarmonyLib;
using System;
using UnityEngine;

namespace HKAccessibility.Patches
{
    /// <summary>
    /// Inventory-related patches: announce charms on selection and provide helpers.
    /// </summary>
    [HarmonyPatch]
    public static class InventoryPatches
    {
        [HarmonyPatch(typeof(InvCharmBackboard), nameof(InvCharmBackboard.SelectCharm))]
        [HarmonyPostfix]
        private static void InvCharmBackboard_SelectCharm_Postfix(InvCharmBackboard __instance)
        {
            try
            {
                if (__instance == null) return;

                int charmNum = __instance.charmNum;
                if (charmNum <= 0) charmNum = __instance.GetCharmNum();
                if (charmNum <= 0) return;

                SpeakCharm(charmNum, __instance);
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"[InventoryPatches] Error announcing charm: {ex}");
            }
        }

        internal static void SpeakCharm(int charmNum, InvCharmBackboard source = null)
        {
            try
            {
                var pd = PlayerData.instance;

                // Determine state from PlayerData if available
                bool isEquipped = pd != null && pd.GetBool($"equippedCharm_{charmNum}");
                bool hasCharm = pd != null && pd.GetBool($"gotCharm_{charmNum}");
                int cost = pd != null ? pd.GetInt($"charmCost_{charmNum}") : 0;
                bool isNew = false;
                if (pd != null)
                {
                    // Some charms use newCharm_N
                    var newField = $"newCharm_{charmNum}";
                    isNew = pd.GetBool(newField);
                }

                // Resolve localized name/description
                string nameKey1 = $"CHARM_NAME_{charmNum}";
                string descKey1 = $"CHARM_DESC_{charmNum}";
                string name = Language.Language.Has(nameKey1, "UI") ? Language.Language.Get(nameKey1, "UI") : Language.Language.Get(nameKey1);
                string desc = Language.Language.Has(descKey1, "UI") ? Language.Language.Get(descKey1, "UI") : Language.Language.Get(descKey1);

                if (string.IsNullOrEmpty(name) || name.StartsWith("#!#"))
                {
                    string altKey = $"CHARM_{charmNum}_NAME";
                    name = Language.Language.Has(altKey, "UI") ? Language.Language.Get(altKey, "UI") : Language.Language.Get(altKey);
                }

                string announcement = string.Empty;
                if (isNew) announcement += "Nuevo. ";
                if (!string.IsNullOrEmpty(name)) announcement += name;
                if (cost > 0) announcement += $". Costo: {cost} muescas";
                if (!string.IsNullOrEmpty(desc)) announcement += $". {desc}";
                if (hasCharm)
                {
                    announcement += isEquipped ? ". Actualmente equipado." : ". No equipado.";
                }

                if (!SpokenTextHistory.HasBeenSpoken(announcement))
                {
                    Plugin.Logger.LogInfo($"[Inventory] Charm {charmNum}: {announcement}");
                    TolkScreenReader.Instance.Speak(announcement, false);
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"[InventoryPatches] Error in SpeakCharm: {ex}");
            }
        }
    }
}


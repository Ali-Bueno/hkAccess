using HarmonyLib;

namespace HKAccessibility.Patches
{
    /// <summary>
    /// Patches for inventory-related actions
    /// Announces when charms are equipped/unequipped
    /// </summary>
    [HarmonyPatch(typeof(PlayerData), "SetBool")]
    public static class PlayerData_SetBool_Patch_Inventory
    {
        private static void Postfix(string boolName, bool value)
        {
            // Only handle charm equipment changes
            if (!boolName.StartsWith("equippedCharm_"))
                return;

            try
            {
                string charmIdStr = boolName.Substring("equippedCharm_".Length);
                int charmID = int.Parse(charmIdStr);

                string charmName = Language.Language.Get($"CHARM_NAME_{charmID}", "UI");
                int notchesCost = PlayerData.instance.GetInt("charmCost_" + charmID);
                int maxNotches = PlayerData.instance.GetInt("maxCharmNotches");
                int notchesUsed = maxNotches - PlayerData.instance.GetInt("charmNotches");

                string textToSpeak;
                if (value)
                {
                    textToSpeak = $"Equipado {charmName}. Muescas usadas: {notchesUsed} de {maxNotches}.";
                }
                else
                {
                    textToSpeak = $"Desequipado {charmName}. Muescas usadas: {notchesUsed} de {maxNotches}.";
                }

                Plugin.Logger.LogInfo($"[InventoryPatches] Speaking: {textToSpeak}");
                TolkScreenReader.Instance.Speak(textToSpeak, false);
            }
            catch (System.Exception ex)
            {
                Plugin.Logger.LogError($"Error in PlayerData_SetBool_Patch_Inventory: {ex}");
            }
        }
    }
}

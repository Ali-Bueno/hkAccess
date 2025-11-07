using HarmonyLib;
using System.Globalization;

namespace HKAccessibility.Patches
{
    /// <summary>
    /// Patches for map and area announcements
    /// Announces when player enters new areas or opens the map
    /// </summary>
    public static class MapPatches
    {
        /// <summary>
        /// Patch to announce area names when entering new zones
        /// </summary>
                        [HarmonyPatch(typeof(AreaTitleController), "Play")]
                        public static class AreaTitleController_Play_Patch
                        {
                            private static void Postfix(AreaTitleController __instance)
                            {
                                try
                                {
                                    var areaEvent = Traverse.Create(__instance).Field<string>("areaEvent").Value;
                                    if (string.IsNullOrEmpty(areaEvent))
                                    {
                                        return;
                                    }
                
                                    // Specific fix for King's Pass key which is missing an underscore
                                    if (areaEvent == "KINGSPASS")
                                    {
                                        areaEvent = "KING_PASS";
                                    }
                
                                    string areaName = Language.Language.Get(areaEvent, "Map Zones");
                
                                    // If localization fails, format the key nicely as a fallback
                                    if (areaName.StartsWith("#!#"))
                                    {
                                        Plugin.Logger.LogWarning($"[AreaTitleController] Localization failed for key '{areaEvent}'. Formatting fallback.");
                                        areaName = areaEvent.Replace("_", " ").ToLower();
                                        areaName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(areaName);
                                    }
                
                                    Plugin.Logger.LogInfo($"[AreaTitleController] Speaking: {areaName}");
                                    TolkScreenReader.Instance.Speak(areaName, false);
                                }
                                catch (System.Exception ex)
                                {
                                    Plugin.Logger.LogError($"Error in AreaTitleController_Play_Patch: {ex}");
                                }
                            }
                        }
        /// <summary>
        /// Patch to announce when the world map is opened
        /// </summary>
        [HarmonyPatch(typeof(GameMap), "WorldMap")]
        public static class GameMap_WorldMap_Patch
        {
            private static void Postfix()
            {
                try
                {
                    string currentMapZone = PlayerData.instance.GetString("mapZone");
                    string areaName = Language.Language.Get($"MAP_ZONE_{currentMapZone.ToUpper()}", "UI");

                    string textToSpeak;
                    if (!string.IsNullOrEmpty(areaName) && areaName != $"MAP_ZONE_{currentMapZone.ToUpper()}")
                    {
                        textToSpeak = $"Mapa de {areaName}";
                    }
                    else
                    {
                        textToSpeak = "Mapa";
                    }

                    Plugin.Logger.LogInfo($"[GameMap_WorldMap] Speaking: {textToSpeak}");
                    TolkScreenReader.Instance.Speak(textToSpeak, false);
                }
                catch (System.Exception ex)
                {
                    Plugin.Logger.LogError($"Error in GameMap_WorldMap_Patch: {ex}");
                }
            }
        }
    }
}

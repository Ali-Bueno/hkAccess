using System;
using UnityEngine;

namespace HKAccessibility
{
    public static class PlayerLocation
    {
        public static void AnnounceLocation()
        {
            try
            {
                if (GameManager.instance != null && GameManager.instance.gameState == GlobalEnums.GameState.PLAYING)
                {
                    var pos = HeroController.instance.transform.position;
                    var mapZone = GameManager.instance.playerData.mapZone;

                    // From MapPatches, we know the key is the area event string, and the sheet is "Map Zones"
                    string mapKey = mapZone.ToString();
                    string localizedMapZone = Language.Language.Get(mapKey, "Map Zones");

                    // If there's no localization, format it nicely like the AreaTitleController patch does
                    if (localizedMapZone == mapKey)
                    {
                        localizedMapZone = mapKey.Replace("_", " ").ToLower();
                        localizedMapZone = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(localizedMapZone);
                    }

                    string format = ModLocalization.Get("PLAYER_LOCATION");
                    string announcement = string.Format(format, localizedMapZone, pos.x, pos.y);

                    TolkScreenReader.Instance.Speak(announcement, true);
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Error announcing player location: {ex}");
                TolkScreenReader.Instance.Speak("Error getting location", true);
            }
        }
    }
}

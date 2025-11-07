using System;
using UnityEngine;

namespace HKAccessibility
{
    /// <summary>
    /// Provides player status announcements (health, soul, geo, etc.)
    /// </summary>
    public static class PlayerStatus
    {
        public static void AnnouncePlayerStatus()
        {
            try
            {
                if (GameManager.instance != null)
                {
                    PlayerData playerData = GameManager.instance.playerData;
                    if (playerData != null)
                    {
                        string status = $"{ModLocalization.Get("HEALTH")}: {playerData.health}/{playerData.maxHealth}, {ModLocalization.Get("SOUL")}: {playerData.MPCharge}/{playerData.maxMP}, {ModLocalization.Get("GEO")}: {playerData.geo}";
                        TolkScreenReader.Instance.Speak(status, false);
                    }
                    else
                    {
                        TolkScreenReader.Instance.Speak(ModLocalization.Get("PLAYER_DATA_NOT_AVAILABLE"), false);
                    }
                }
                else
                {
                    TolkScreenReader.Instance.Speak(ModLocalization.Get("GAME_MANAGER_NOT_FOUND"), false);
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Error getting player status: {ex}");
                TolkScreenReader.Instance.Speak(ModLocalization.Get("ERROR_GETTING_PLAYER_STATUS"), false);
            }
        }
    }
}

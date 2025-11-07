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
                        string status = $"Health: {playerData.health}/{playerData.maxHealth}, Soul: {playerData.MPCharge}/{playerData.maxMP}, Geo: {playerData.geo}";
                        TolkScreenReader.Instance.Speak(status, false);
                    }
                    else
                    {
                        TolkScreenReader.Instance.Speak("Player data not available", false);
                    }
                }
                else
                {
                    TolkScreenReader.Instance.Speak("Game manager not found", false);
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Error getting player status: {ex}");
                TolkScreenReader.Instance.Speak("Error getting player status", false);
            }
        }
    }
}

using UnityEngine;

namespace HKAccessibility
{
    /// <summary>
    /// Centralized system to prevent duplicate text announcements using debouncing
    /// </summary>
    public static class SpokenTextHistory
    {
        private static string lastSpokenText;
        private static float lastSpokenTime;
        private const float debounceTime = 0.1f; // 100ms

        /// <summary>
        /// Check if text has already been spoken recently (within debounce time)
        /// </summary>
        /// <param name="text">Text to check</param>
        /// <returns>True if already spoken recently, false otherwise</returns>
        public static bool HasBeenSpoken(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return true;
            }

            if (text == lastSpokenText && Time.unscaledTime - lastSpokenTime < debounceTime)
            {
                return true;
            }

            lastSpokenText = text;
            lastSpokenTime = Time.unscaledTime;
            return false;
        }

        /// <summary>
        /// Reset the history (useful when changing scenes or menus)
        /// </summary>
        public static void Reset()
        {
            lastSpokenText = null;
            lastSpokenTime = 0f;
        }
    }
}

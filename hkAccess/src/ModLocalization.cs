using System;
using System.Collections.Generic;

namespace HKAccessibility
{
    /// <summary>
    /// Provides simple English messages for mod-specific features.
    /// For game terms (UI elements, items, stats), the game's own localized text is used directly.
    /// </summary>
    public static class ModLocalization
    {
        /// <summary>
        /// Initialize - kept for compatibility, but no longer needed
        /// </summary>
        public static void Initialize()
        {
            // No longer needed - we use simple English for mod messages
            // and read game terms directly from UI text
        }

        /// <summary>
        /// Get a simple English message for mod-specific features
        /// </summary>
        public static string Get(string key)
        {
            // Simple, universal English messages for mod-only features
            return key switch
            {
                "MOD_LOADED" => "Hollow Knight Accessibility Mod loaded",
                "MOD_UNLOADED" => "Hollow Knight Accessibility Mod unloaded",
                "INVENTORY_OPENED" => "Inventory opened",
                _ => key // If not found, return the key itself
            };
        }
    }
}

using UnityEngine;

namespace HKAccessibility
{
    /// <summary>
    /// Manages accessibility-related keyboard inputs
    /// </summary>
    public class InputManager
    {
        /// <summary>
        /// Check and handle accessibility input keys
        /// Should be called from Update() in a MonoBehaviour
        /// </summary>
        public void HandleInput()
        {
            // F9 - Announce player status
            if (Input.GetKeyDown(KeyCode.F9))
            {
                PlayerStatus.AnnouncePlayerStatus();
            }

            // F10 - Re-announce current UI element (future feature)
            if (Input.GetKeyDown(KeyCode.F10))
            {
                // TODO: Implement re-announce current selection
            }

            // F11 - Announce current location/area (future feature)
            if (Input.GetKeyDown(KeyCode.F11))
            {
                // TODO: Implement location announcement
            }
        }
    }
}

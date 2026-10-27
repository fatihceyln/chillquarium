using UnityEngine;
using UnityEngine.UIElements;

namespace Chillquarium.Gameplay
{
    /// <summary>
    /// Finds the UI element under a mouse position read from the Input System.
    /// </summary>
    public static class PanelPicking
    {
        public static VisualElement Pick(IPanel panel, Vector2 screenPosition)
        {
            // Input System y counts up from the bottom; panel y counts down from the top.
            var panelPosition = RuntimePanelUtils.ScreenToPanel(panel, new Vector2(screenPosition.x, Screen.height - screenPosition.y));
            return panel.Pick(panelPosition);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Chillquarium.Gameplay
{
    /// <summary>
    /// Drops a food pellet where the player clicks in the water and keeps track of the pellets fish can eat.
    /// </summary>
    public sealed class Feeder : MonoBehaviour
    {
        [SerializeField] private GameplayConfig gameplayConfig;
        [SerializeField] private Camera worldCamera;
        [SerializeField] private PanelRenderer hudPanelRenderer;
        [SerializeField] private SwimArea swimArea;
        [SerializeField] private FoodPellet pelletPrefab;
        [SerializeField] private AudioSource soundSource;
        [SerializeField] private AudioClip dropSound;
        [SerializeField] private AudioClip eatSound;

        private readonly List<FoodPellet> pellets = new();
        private VisualElement hudRoot;

        // Awake rather than OnEnable: windows switch the feeder off and on, and the HUD root must survive that.
        private void Awake()
        {
            hudPanelRenderer.RegisterUIReloadCallback(HandleHUDReloaded);
        }

        private void Update()
        {
            if (Mouse.current.leftButton.wasPressedThisFrame && !IsOverHUD(Mouse.current.position.ReadValue()))
            {
                // Vector2 drops the camera's z, so the pellet sits at z = 0 with the fish instead of on the camera plane.
                Vector2 clickPoint = worldCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                DropPellet(clickPoint);
            }
        }

        private void OnDestroy()
        {
            hudPanelRenderer.UnregisterUIReloadCallback(HandleHUDReloaded);
        }

        public bool TryGetNearestPellet(Vector3 position, out FoodPellet nearest)
        {
            nearest = null;
            var nearestDistance = float.MaxValue;

            foreach (var pellet in pellets)
            {
                var distance = Vector3.Distance(position, pellet.transform.position);

                if (distance < nearestDistance)
                {
                    nearest = pellet;
                    nearestDistance = distance;
                }
            }

            return pellets.Count > 0;
        }

        public void Eat(FoodPellet pellet, Fish fish)
        {
            pellets.Remove(pellet);
            Destroy(pellet.gameObject);
            fish.Feed(gameplayConfig.FoodXp);
            soundSource.PlayOneShot(eatSound);
        }

        private void HandleHUDReloaded(PanelRenderer renderer, VisualElement rootElement, int version)
        {
            hudRoot = rootElement;
        }

        // Clicks on HUD elements over the water, like the fish cards, must not drop food.
        private bool IsOverHUD(Vector2 screenPosition)
        {
            // Input System y counts up from the bottom; panel y counts down from the top.
            var panelPosition = RuntimePanelUtils.ScreenToPanel(hudRoot.panel, new Vector2(screenPosition.x, Screen.height - screenPosition.y));
            var picked = hudRoot.panel.Pick(panelPosition);

            // Full-screen layers ignore picking, so only a real element under the pointer counts.
            return picked != null && picked != hudRoot && hudRoot.Contains(picked);
        }

        private void DropPellet(Vector2 point)
        {
            var area = swimArea.Area;

            // Clicks outside the swim area, like on the sand or the surface, do not feed.
            if (!area.Contains(point))
            {
                return;
            }

            var pellet = Instantiate(pelletPrefab, point, Quaternion.identity, transform);
            pellet.Initialize(area.yMin);
            pellet.Expired += HandlePelletExpired;
            pellets.Add(pellet);
            soundSource.PlayOneShot(dropSound);
        }

        private void HandlePelletExpired(FoodPellet pellet)
        {
            pellets.Remove(pellet);
        }
    }
}

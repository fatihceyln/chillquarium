using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Chillquarium.Gameplay
{
    /// <summary>
    /// Drops a food pellet where the player clicks in the water and keeps track of the pellets fish can eat.
    /// </summary>
    public sealed class Feeder : MonoBehaviour
    {
        [SerializeField] private GameplayConfig gameplayConfig;
        [SerializeField] private Camera worldCamera;
        [SerializeField] private SwimArea swimArea;
        [SerializeField] private FoodPellet pelletPrefab;
        [SerializeField] private AudioSource soundSource;
        [SerializeField] private AudioClip dropSound;
        [SerializeField] private AudioClip eatSound;

        private readonly List<FoodPellet> pellets = new();

        private void Update()
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                // Vector2 drops the camera's z, so the pellet sits at z = 0 with the fish instead of on the camera plane.
                Vector2 clickPoint = worldCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                DropPellet(clickPoint);
            }
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

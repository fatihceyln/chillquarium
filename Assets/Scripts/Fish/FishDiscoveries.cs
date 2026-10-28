using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chillquarium.Gameplay
{
    /// <summary>
    /// The species whose adult the player has seen. The first adult of a species raises Discovered,
    /// so DiscoveryView can show it; the save keeps the list.
    /// </summary>
    public sealed class FishDiscoveries : MonoBehaviour
    {
        [SerializeField] private FishSpawner spawner;

        private readonly HashSet<FishSpecies> discovered = new();

        public event Action<FishSpecies> Discovered;

        public IReadOnlyCollection<FishSpecies> DiscoveredSpecies => discovered;

        private void OnEnable()
        {
            spawner.FishGrewUp += HandleFishGrewUp;
        }

        private void OnDisable()
        {
            spawner.FishGrewUp -= HandleFishGrewUp;
        }

        // Marks a species as seen without announcing it, for the save.
        public void Restore(FishSpecies species)
        {
            discovered.Add(species);
        }

        private void HandleFishGrewUp(Fish fish)
        {
            if (discovered.Add(fish.Species))
            {
                Discovered?.Invoke(fish.Species);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Chillquarium.Gameplay
{
    /// <summary>
    /// Creates fish from the fish prefab, connects each one to the scene (swim area, feeder)
    /// and keeps the list of every fish in the tank.
    /// </summary>
    public sealed class FishSpawner : MonoBehaviour
    {
        [SerializeField] private Fish fishPrefab;
        [SerializeField] private SwimArea swimArea;
        [SerializeField] private Feeder feeder;

        private readonly List<Fish> fishes = new();

        // Any fish in the tank growing into an adult, so listeners need not follow each fish.
        public event Action<Fish> FishGrewUp;

        public IReadOnlyList<Fish> Fishes => fishes;

        public void Spawn(FishSpecies species, float xp = 0f)
        {
            var area = swimArea.Area;
            Spawn(species, new Vector3(Random.Range(area.xMin, area.xMax), Random.Range(area.yMin, area.yMax), 0f), xp);
        }

        // The position is kept inside the swim area, so a fish never starts out of the water.
        public void Spawn(FishSpecies species, Vector3 position, float xp = 0f)
        {
            var area = swimArea.Area;
            position = new Vector3(Mathf.Clamp(position.x, area.xMin, area.xMax), Mathf.Clamp(position.y, area.yMin, area.yMax), 0f);
            var fish = Instantiate(fishPrefab, position, Quaternion.identity, transform);

            // The list length doubles as a unique draw order for the new fish.
            fish.Initialize(species, xp, swimArea, feeder, fishes.Count);
            fishes.Add(fish);
            fish.GrewUp += HandleFishGrewUp;
        }

        private void HandleFishGrewUp(Fish fish)
        {
            FishGrewUp?.Invoke(fish);
        }
    }
}

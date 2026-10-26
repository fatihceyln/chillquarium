using System.Collections.Generic;
using UnityEngine;

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

        public IReadOnlyList<Fish> Fishes => fishes;

        public void Spawn(FishSpecies species, float ageSeconds = 0f)
        {
            var area = swimArea.Area;
            var position = new Vector3(Random.Range(area.xMin, area.xMax), Random.Range(area.yMin, area.yMax), 0f);
            var fish = Instantiate(fishPrefab, position, Quaternion.identity, transform);

            // The list length doubles as a unique draw order for the new fish.
            fish.Initialize(species, ageSeconds, swimArea, feeder, fishes.Count);
            fishes.Add(fish);
        }
    }
}

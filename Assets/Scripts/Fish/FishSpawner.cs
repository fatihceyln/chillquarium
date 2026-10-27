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

        public void Spawn(FishSpecies species, float xp = 0f)
        {
            var area = swimArea.Area;
            Spawn(species, xp, new Vector3(Random.Range(area.xMin, area.xMax), Random.Range(area.yMin, area.yMax), 0f));
        }

        // The position is kept inside the swim area, so a fish never starts out of the water.
        public void Spawn(FishSpecies species, float xp, Vector3 position)
        {
            var area = swimArea.Area;
            position = new Vector3(Mathf.Clamp(position.x, area.xMin, area.xMax), Mathf.Clamp(position.y, area.yMin, area.yMax), 0f);
            var fish = Instantiate(fishPrefab, position, Quaternion.identity, transform);

            // The list length doubles as a unique draw order for the new fish.
            fish.Initialize(species, xp, swimArea, feeder, fishes.Count);
            fishes.Add(fish);
        }
    }
}

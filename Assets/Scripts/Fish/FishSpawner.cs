using UnityEngine;

namespace Chillquarium.Gameplay
{
    /// <summary>
    /// Creates fish from the fish prefab and connects each one to the scene: swim area, feeder and income.
    /// </summary>
    public sealed class FishSpawner : MonoBehaviour
    {
        [SerializeField] private GameplayConfig gameplayConfig;
        [SerializeField] private Fish fishPrefab;
        [SerializeField] private SwimArea swimArea;
        [SerializeField] private Feeder feeder;
        [SerializeField] private FishIncome income;

        private int spawnedCount;

        private void Start()
        {
            Spawn(gameplayConfig.StarterSpecies);
        }

        public void Spawn(FishSpecies species)
        {
            var area = swimArea.Area;
            var position = new Vector3(Random.Range(area.xMin, area.xMax), Random.Range(area.yMin, area.yMax), 0f);
            var fish = Instantiate(fishPrefab, position, Quaternion.identity, transform);
            fish.Initialize(species, swimArea, feeder, spawnedCount);
            spawnedCount++;
            income.Register(fish);
        }
    }
}

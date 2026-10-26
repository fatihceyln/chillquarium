using System.IO;
using UnityEngine;

namespace Chillquarium.Gameplay
{
    /// <summary>
    /// Saves the money and the fish to a JSON file and restores them when the game starts.
    /// Without a save file the game is new and starts with the starter fish.
    /// </summary>
    public sealed class GameSave : MonoBehaviour
    {
        [SerializeField] private GameplayConfig gameplayConfig;
        [SerializeField] private MoneyWallet wallet;
        [SerializeField] private FishSpawner spawner;
        [SerializeField, Min(1f)] private float autosaveIntervalSeconds = 5f;

        private float timeSinceLastSave;

        // On macOS: ~/Library/Application Support/<company>/<product>/save.json
        public static string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

        private void Start()
        {
            if (File.Exists(SavePath))
            {
                Restore(JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath)));
            }
            else
            {
                spawner.Spawn(gameplayConfig.StarterSpecies);
            }
        }

        private void Update()
        {
            timeSinceLastSave += Time.deltaTime;

            if (timeSinceLastSave >= autosaveIntervalSeconds)
            {
                timeSinceLastSave = 0f;
                Save();
            }
        }

        // Also runs when Play Mode stops in the Editor.
        private void OnApplicationQuit()
        {
            Save();
        }

        private void Save()
        {
            File.WriteAllText(SavePath, JsonUtility.ToJson(Capture(), true));
        }

        private SaveData Capture()
        {
            var data = new SaveData { money = wallet.Balance };

            foreach (var fish in spawner.Fishes)
            {
                data.fishes.Add(new FishSaveData { species = fish.Species.name, ageSeconds = fish.AgeSeconds });
            }

            return data;
        }

        private void Restore(SaveData data)
        {
            wallet.RestoreBalance(data.money);

            foreach (var savedFish in data.fishes)
            {
                if (TryFindSpecies(savedFish.species, out var species))
                {
                    spawner.Spawn(species, savedFish.ageSeconds);
                }
                else
                {
                    Debug.LogWarning($"The save has a fish of unknown species \"{savedFish.species}\". It was skipped.");
                }
            }
        }

        private bool TryFindSpecies(string speciesName, out FishSpecies found)
        {
            foreach (var species in gameplayConfig.CardSpecies)
            {
                if (species.name == speciesName)
                {
                    found = species;
                    return true;
                }
            }

            // The starter species does not have to be one the card gives.
            found = gameplayConfig.StarterSpecies;
            return found.name == speciesName;
        }
    }
}

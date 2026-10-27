using System.IO;
using UnityEngine;

namespace Chillquarium.Gameplay
{
    /// <summary>
    /// Saves the money, the fish and the unopened cards to a JSON file and restores them when the game starts.
    /// Without a save file the game is new and starts with the starting money and the starter fish.
    /// </summary>
    public sealed class GameSave : MonoBehaviour
    {
        [SerializeField] private GameplayConfig gameplayConfig;
        [SerializeField] private MoneyWallet wallet;
        [SerializeField] private FishSpawner spawner;
        [SerializeField] private CardPacks cardPacks;
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
                wallet.SetBalance(gameplayConfig.StartingMoney);
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
            var data = new SaveData { money = wallet.Balance, packs = cardPacks.PackCount };

            foreach (var fish in spawner.Fishes)
            {
                data.fishes.Add(new FishSaveData { species = fish.Species.name, xp = fish.Xp });
            }

            // Cards laid out but not clicked are saved as new babies, so quitting mid-card loses no fish.
            foreach (var species in cardPacks.UnclaimedSpecies)
            {
                data.fishes.Add(new FishSaveData { species = species.name, xp = 0f });
            }

            return data;
        }

        private void Restore(SaveData data)
        {
            wallet.SetBalance(data.money);
            cardPacks.Restore(data.packs);

            foreach (var savedFish in data.fishes)
            {
                if (TryFindSpecies(savedFish.species, out var species))
                {
                    spawner.Spawn(species, savedFish.xp);
                }
                else
                {
                    Debug.LogWarning($"The save has a fish of unknown species \"{savedFish.species}\". It was skipped.");
                }
            }
        }

        private bool TryFindSpecies(string speciesName, out FishSpecies found)
        {
            foreach (var fish in gameplayConfig.Fishes)
            {
                if (fish.Species.name == speciesName)
                {
                    found = fish.Species;
                    return true;
                }
            }

            found = null;
            return false;
        }
    }
}

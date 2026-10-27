using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chillquarium.Gameplay
{
    /// <summary>
    /// The bought fish cards and the fish cards laid out from them. Holds the state that the shop, the save
    /// and CardPackView share; the view only shows it and forwards the player's input.
    /// </summary>
    public sealed class CardPacks : MonoBehaviour
    {
        [SerializeField] private GameplayConfig gameplayConfig;
        [SerializeField] private FishSpawner spawner;
        [SerializeField] private CardPackType packType;

        private readonly List<FishSpecies> unclaimedSpecies = new();
        private int packCount;

        public event Action PackCountChanged;

        public CardPackType PackType => packType;
        public int PackCount => packCount;
        // Fish whose cards are laid out but not collected yet. The save keeps them as tank fish so they are not lost.
        public IReadOnlyList<FishSpecies> UnclaimedSpecies => unclaimedSpecies;

        public void AddPacks(int count)
        {
            packCount += count;
            PackCountChanged?.Invoke();
        }

        public void Restore(int count)
        {
            packCount = count;
            PackCountChanged?.Invoke();
        }

        // Opens one card and returns the fish it rolled; empty when no card is left.
        public IReadOnlyList<FishSpecies> Open()
        {
            var opened = new List<FishSpecies>();

            if (packCount == 0)
            {
                return opened;
            }

            packCount--;

            for (var index = 0; index < gameplayConfig.FishPerCard; index++)
            {
                opened.Add(CardRoll.PickSpecies(gameplayConfig));
            }

            unclaimedSpecies.AddRange(opened);
            PackCountChanged?.Invoke();
            return opened;
        }

        public void Collect(FishSpecies species, Vector3 position)
        {
            unclaimedSpecies.Remove(species);
            spawner.Spawn(species, position);
        }
    }
}

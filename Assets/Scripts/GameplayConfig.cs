using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chillquarium.Gameplay
{
    /// <summary>
    /// Every balance value of the game in one place: prices, incomes, growth times and rarity odds.
    /// </summary>
    [CreateAssetMenu(fileName = "GameplayConfig", menuName = "Chillquarium/Gameplay Config")]
    public sealed class GameplayConfig : ScriptableObject
    {
        [Header("Money")]
        // Money values have no [Min]: its Inspector drawer may edit them as floats and lose the precision a double keeps.
        [SerializeField] private double startingMoney = 10d;
        [SerializeField] private double cardPrice = 5d;
        [SerializeField, Min(0.1f)] private float incomeIntervalSeconds = 1f;

        [Header("Food")]
        [Tooltip("Seconds of growth a baby gains from eating one food pellet.")]
        [SerializeField, Min(0f)] private float foodGrowthSeconds = 5f;

        [Header("Rarities")]
        [SerializeField] private RarityTier common = new();
        [SerializeField] private RarityTier rare = new();
        [SerializeField] private RarityTier epic = new();
        [SerializeField] private RarityTier legendary = new();

        [Header("Fish")]
        [Tooltip("The fish the tank starts with. It must be in the list below.")]
        [SerializeField] private FishSpecies starterSpecies;
        [Tooltip("Every species in the game with its balance values. The fish card gives these species.")]
        [SerializeField] private List<FishBalance> fishes = new();

        public double StartingMoney => startingMoney;
        public double CardPrice => cardPrice;
        public float IncomeIntervalSeconds => incomeIntervalSeconds;
        public float FoodGrowthSeconds => foodGrowthSeconds;
        public FishSpecies StarterSpecies => starterSpecies;
        public IReadOnlyList<FishBalance> Fishes => fishes;

        public RarityTier GetTier(FishRarity rarity)
        {
            return rarity switch
            {
                FishRarity.Common => common,
                FishRarity.Rare => rare,
                FishRarity.Epic => epic,
                FishRarity.Legendary => legendary,
                _ => throw new ArgumentOutOfRangeException(nameof(rarity), rarity, null),
            };
        }

        public FishBalance GetBalance(FishSpecies species)
        {
            foreach (var balance in fishes)
            {
                if (balance.Species == species)
                {
                    return balance;
                }
            }

            throw new KeyNotFoundException($"{species.name} has no row in the GameplayConfig fishes list.");
        }
    }
}

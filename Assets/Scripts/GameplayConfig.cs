using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chillquarium.Gameplay
{
    /// <summary>
    /// The game's shared balance and display values in one place.
    /// Values that belong to a single species live on its FishSpecies asset.
    /// </summary>
    [CreateAssetMenu(fileName = "GameplayConfig", menuName = "Chillquarium/Gameplay Config")]
    public sealed class GameplayConfig : ScriptableObject
    {
        [SerializeField] private RarityTier common = new();
        [SerializeField] private RarityTier rare = new();
        [SerializeField] private RarityTier epic = new();
        [SerializeField] private RarityTier legendary = new();
        [Tooltip("The fish the tank starts with.")]
        [SerializeField] private FishSpecies starterSpecies;
        [Tooltip("The species the fish card can give.")]
        [SerializeField] private List<FishSpecies> cardSpecies = new();
        // Money values have no [Min]: its Inspector drawer may edit them as floats and lose the precision a double keeps.
        [SerializeField] private double startingMoney = 10d;
        [SerializeField] private double cardPrice = 5d;
        [SerializeField, Min(0.1f)] private float incomeIntervalSeconds = 1f;
        [Tooltip("Seconds of growth a baby gains from eating one food pellet.")]
        [SerializeField, Min(0f)] private float foodGrowthSeconds = 5f;

        public FishSpecies StarterSpecies => starterSpecies;
        public IReadOnlyList<FishSpecies> CardSpecies => cardSpecies;
        public double StartingMoney => startingMoney;
        public double CardPrice => cardPrice;
        public float IncomeIntervalSeconds => incomeIntervalSeconds;
        public float FoodGrowthSeconds => foodGrowthSeconds;

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
    }
}

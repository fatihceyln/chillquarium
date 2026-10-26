using System;
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
        // No [Min] here: its Inspector drawer may edit the value as a float and lose the precision a double keeps.
        [SerializeField] private double startingMoney = 10d;

        public double StartingMoney => startingMoney;

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

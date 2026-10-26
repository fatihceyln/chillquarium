using UnityEngine;

namespace Chillquarium.Gameplay
{
    /// <summary>
    /// One kind of fish: its name, rarity, look and how much money it earns.
    /// </summary>
    [CreateAssetMenu(fileName = "NewFishSpecies", menuName = "Chillquarium/Fish Species")]
    public sealed class FishSpecies : ScriptableObject
    {
        [SerializeField] private string displayName;
        [SerializeField] private FishRarity rarity;
        [Tooltip("Body color of the placeholder fish until the real fish art arrives.")]
        [SerializeField] private Color placeholderColor = Color.white;
        [SerializeField, Min(0f)] private float babyIncomePerSecond = 1f;
        [SerializeField, Min(0f)] private float adultIncomePerSecond = 4f;
        [Tooltip("Seconds a baby needs to become an adult.")]
        [SerializeField, Min(1f)] private float growthSeconds = 60f;

        public string DisplayName => displayName;
        public FishRarity Rarity => rarity;
        public Color PlaceholderColor => placeholderColor;
        public float BabyIncomePerSecond => babyIncomePerSecond;
        public float AdultIncomePerSecond => adultIncomePerSecond;
        public float GrowthSeconds => growthSeconds;
    }
}

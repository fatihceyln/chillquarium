using UnityEngine;

namespace Chillquarium.Gameplay
{
    /// <summary>
    /// One kind of fish: its name, rarity and look. Its balance values (income, growth time) are in GameplayConfig.
    /// </summary>
    [CreateAssetMenu(fileName = "NewFishSpecies", menuName = "Chillquarium/Fish Species")]
    public sealed class FishSpecies : ScriptableObject
    {
        [SerializeField] private string displayName;
        [SerializeField] private FishRarity rarity;
        [Tooltip("Body color of the placeholder fish until the real fish art arrives.")]
        [SerializeField] private Color placeholderColor = Color.white;

        public string DisplayName => displayName;
        public FishRarity Rarity => rarity;
        public Color PlaceholderColor => placeholderColor;
    }
}

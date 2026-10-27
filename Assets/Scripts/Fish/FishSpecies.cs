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
        [SerializeField] private Sprite babySprite;
        [SerializeField] private Sprite adultSprite;

        public string DisplayName => displayName;
        public FishRarity Rarity => rarity;
        public Sprite BabySprite => babySprite;
        public Sprite AdultSprite => adultSprite;
    }
}

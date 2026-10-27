using UnityEngine;

namespace Chillquarium.Gameplay
{
    /// <summary>
    /// A kind of fish card the shop sells, like "Freshwater Friends". Holds only its identity and look;
    /// its price and the fish it gives are balance values in GameplayConfig.
    /// </summary>
    [CreateAssetMenu(fileName = "CardPackType", menuName = "Chillquarium/Card Pack Type")]
    public sealed class CardPackType : ScriptableObject
    {
        [SerializeField] private string displayName = "Fish Card";
        [SerializeField] private Sprite art;

        public string DisplayName => displayName;
        public Sprite Art => art;
    }
}

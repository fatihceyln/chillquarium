using System;
using UnityEngine;

namespace Chillquarium.Gameplay
{
    /// <summary>
    /// One rarity's settings: its UI color (e.g. the "Legendary" label) and how often a card gives it.
    /// </summary>
    [Serializable]
    public sealed class RarityTier
    {
        [SerializeField] private Color displayColor = Color.white;
        [Tooltip("Relative chance of this rarity when a card is opened. Weights are compared to each other, so 70 vs 1 means 70 times more often.")]
        [SerializeField, Min(0f)] private float dropWeight = 1f;

        public Color DisplayColor => displayColor;
        public float DropWeight => dropWeight;
    }
}

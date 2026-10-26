using System;
using UnityEngine;

namespace Chillquarium.Gameplay
{
    /// <summary>
    /// How one rarity looks in the UI, e.g. the color of the "Legendary" label.
    /// </summary>
    [Serializable]
    public sealed class RarityTier
    {
        [SerializeField] private Color displayColor = Color.white;

        public Color DisplayColor => displayColor;
    }
}

using System;
using UnityEngine;

namespace Chillquarium.Gameplay
{
    /// <summary>
    /// One species' balance values: how much it earns and how much XP it needs to grow up.
    /// </summary>
    [Serializable]
    public sealed class FishBalance
    {
        [SerializeField] private FishSpecies species;
        [SerializeField, Min(0f)] private float babyIncomePerSecond = 1f;
        [SerializeField, Min(0f)] private float adultIncomePerSecond = 4f;
        [Tooltip("XP a baby needs to become an adult.")]
        [SerializeField, Min(1f)] private float adultXp = 60f;

        public FishSpecies Species => species;
        public float BabyIncomePerSecond => babyIncomePerSecond;
        public float AdultIncomePerSecond => adultIncomePerSecond;
        public float AdultXp => adultXp;
    }
}

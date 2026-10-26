using UnityEngine;

namespace Chillquarium.Gameplay
{
    /// <summary>
    /// A fish in the tank. Grows from a small, pale baby into a full-size adult over its species' growth time.
    /// </summary>
    public sealed class Fish : MonoBehaviour
    {
        [SerializeField] private FishSpecies species;
        [SerializeField] private Transform visual;
        [SerializeField] private SpriteRenderer body;
        [SerializeField] private SpriteRenderer tail;
        [SerializeField, Range(0f, 1f)] private float tailShade = 0.15f;
        [SerializeField, Min(0.01f)] private float babySize = 0.5f;
        [Tooltip("How much lighter a newborn's placeholder color is than an adult's.")]
        [SerializeField, Range(0f, 1f)] private float babyPaleness = 0.4f;
        [Tooltip("Seconds this fish has grown. Set it to the species' growth time or more to start as an adult.")]
        [SerializeField, Min(0f)] private float ageSeconds;

        public FishSpecies Species => species;
        public bool IsAdult => ageSeconds >= species.GrowthSeconds;
        public double IncomePerSecond => IsAdult ? species.AdultIncomePerSecond : species.BabyIncomePerSecond;

        private void Start()
        {
            ApplyGrowth();
        }

        private void Update()
        {
            if (IsAdult)
            {
                return;
            }

            ageSeconds += Time.deltaTime;
            ApplyGrowth();
        }

        public void Feed(float growthSeconds)
        {
            ageSeconds += growthSeconds;
            ApplyGrowth();
        }

        private void ApplyGrowth()
        {
            // 0 for a newborn, 1 for an adult.
            var growth = Mathf.Clamp01(ageSeconds / species.GrowthSeconds);
            var size = Mathf.Lerp(babySize, 1f, growth);

            // Keep the sign of x: FishSwimmer flips it to face the swim direction.
            var scale = visual.localScale;
            visual.localScale = new Vector3(Mathf.Sign(scale.x) * size, size, scale.z);

            var color = Color.Lerp(species.PlaceholderColor, Color.white, babyPaleness * (1f - growth));
            body.color = color;
            tail.color = Color.Lerp(color, Color.black, tailShade);
        }
    }
}

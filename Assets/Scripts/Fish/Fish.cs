using UnityEngine;
using UnityEngine.Rendering;

namespace Chillquarium.Gameplay
{
    /// <summary>
    /// A fish in the tank. Grows from a small, pale baby into a full-size adult over its species' growth time.
    /// Fish are spawned from a prefab, so FishSpawner hands each one its species and the scene objects it needs.
    /// </summary>
    public sealed class Fish : MonoBehaviour
    {
        [SerializeField] private GameplayConfig gameplayConfig;
        [SerializeField] private FishSwimmer swimmer;
        [SerializeField] private SortingGroup sortingGroup;
        [SerializeField] private Transform visual;
        [SerializeField] private SpriteRenderer body;
        [SerializeField] private SpriteRenderer tail;
        [SerializeField, Range(0f, 1f)] private float tailShade = 0.15f;
        [SerializeField, Min(0.01f)] private float babySize = 0.5f;
        [Tooltip("How much lighter a newborn's placeholder color is than an adult's.")]
        [SerializeField, Range(0f, 1f)] private float babyPaleness = 0.4f;
        [Tooltip("Seconds this fish has grown. Raise it during Play to fast-forward growth.")]
        [SerializeField, Min(0f)] private float ageSeconds;

        private FishSpecies species;

        public FishSpecies Species => species;
        public float AgeSeconds => ageSeconds;
        public bool IsAdult => ageSeconds >= Balance.GrowthSeconds;
        public double IncomePerSecond => IsAdult ? Balance.AdultIncomePerSecond : Balance.BabyIncomePerSecond;

        // Looked up on every use, so balance edits in GameplayConfig apply to fish already in the tank.
        private FishBalance Balance => gameplayConfig.GetBalance(species);

        private void Update()
        {
            if (IsAdult)
            {
                return;
            }

            ageSeconds += Time.deltaTime;
            ApplyGrowth();
        }

        public void Initialize(FishSpecies species, float ageSeconds, SwimArea swimArea, Feeder feeder, int drawOrder)
        {
            this.species = species;
            this.ageSeconds = ageSeconds;

            // Each fish draws on its own layer, so overlapping fish do not mix their parts or flicker.
            sortingGroup.sortingOrder = drawOrder;
            swimmer.Initialize(swimArea, feeder);
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
            var growth = Mathf.Clamp01(ageSeconds / Balance.GrowthSeconds);
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

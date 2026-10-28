using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Chillquarium.Gameplay
{
    /// <summary>
    /// A fish in the tank. Earns XP over time and from food; once its XP reaches its species' adult XP it turns
    /// from a small baby into a full-size adult, switching from the species' baby sprite to its adult sprite.
    /// Fish are spawned from a prefab, so FishSpawner hands each one its species and the scene objects it needs.
    /// </summary>
    public sealed class Fish : MonoBehaviour
    {
        [SerializeField] private GameplayConfig gameplayConfig;
        [SerializeField] private FishSwimmer swimmer;
        [SerializeField] private SortingGroup sortingGroup;
        [SerializeField] private Transform visual;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField, Min(0.01f)] private float babySize = 0.5f;
        [Tooltip("XP this fish has earned. Raise it during Play to fast-forward growth.")]
        [SerializeField, Min(0f)] private float xp;

        private FishSpecies species;
        private bool wasAdult;

        // Raised once, when this fish grows into an adult; a fish that starts as an adult never raises it.
        public event Action<Fish> GrewUp;

        public FishSpecies Species => species;
        public float XP => xp;
        public bool IsAdult => xp >= Balance.AdultXP;
        public double IncomePerSecond => IsAdult ? Balance.AdultIncomePerSecond : Balance.BabyIncomePerSecond;

        // Looked up on every use, so balance edits in GameplayConfig apply to fish already in the tank.
        private FishBalance Balance => gameplayConfig.GetBalance(species);

        private void Update()
        {
            if (IsAdult)
            {
                return;
            }

            xp += gameplayConfig.XpPerSecond * Time.deltaTime;
            ApplyGrowth();
        }

        public void Initialize(FishSpecies species, float xp, SwimArea swimArea, Feeder feeder, int drawOrder)
        {
            this.species = species;
            this.xp = xp;

            // Each fish draws on its own layer, so overlapping fish do not mix their parts or flicker.
            sortingGroup.sortingOrder = drawOrder;
            swimmer.Initialize(swimArea, feeder);
            wasAdult = IsAdult;
            ApplyGrowth();
        }

        public void Feed()
        {
            xp += gameplayConfig.FoodXP;
            ApplyGrowth();
        }

        private void ApplyGrowth()
        {
            var size = IsAdult ? 1f : babySize;

            // Keep the sign of x: FishSwimmer flips it to face the swim direction.
            var scale = visual.localScale;
            visual.localScale = new Vector3(Mathf.Sign(scale.x) * size, size, scale.z);

            spriteRenderer.sprite = IsAdult ? species.AdultSprite : species.BabySprite;

            if (IsAdult && !wasAdult)
            {
                wasAdult = true;
                GrewUp?.Invoke(this);
            }
        }
    }
}

using UnityEngine;

namespace Chillquarium.Gameplay
{
    /// <summary>
    /// A fish in the tank. Knows its species and paints the placeholder body in the species color.
    /// </summary>
    public sealed class Fish : MonoBehaviour
    {
        [SerializeField] private FishSpecies species;
        [SerializeField] private SpriteRenderer body;
        [SerializeField] private SpriteRenderer tail;
        [SerializeField, Range(0f, 1f)] private float tailShade = 0.15f;

        public FishSpecies Species => species;

        private void Start()
        {
            body.color = species.PlaceholderColor;
            tail.color = Color.Lerp(species.PlaceholderColor, Color.black, tailShade);
        }
    }
}

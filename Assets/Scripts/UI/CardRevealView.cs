using UnityEngine;
using UnityEngine.UIElements;

namespace Chillquarium.Gameplay
{
    /// <summary>
    /// The card opening window: shows the fish a card gave, then adds it to the tank as a baby.
    /// </summary>
    public sealed class CardRevealView : MonoBehaviour
    {
        [SerializeField] private PanelRenderer panelRenderer;
        [SerializeField] private GameplayConfig gameplayConfig;
        [SerializeField] private FishSpawner spawner;
        [SerializeField] private Feeder feeder;
        [SerializeField] private string revealPanelName = "reveal-panel";
        [SerializeField] private string fishNameLabelName = "reveal-fish-name-label";
        [SerializeField] private string rarityLabelName = "reveal-rarity-label";
        [SerializeField] private string fishArtName = "reveal-fish-art";
        [SerializeField] private string confirmButtonName = "reveal-confirm-button";
        [SerializeField] private string hiddenClassName = "hidden";

        private VisualElement revealPanel;
        private Label fishNameLabel;
        private Label rarityLabel;
        private VisualElement fishArt;
        private FishSpecies revealedSpecies;

        private void OnEnable()
        {
            panelRenderer.RegisterUIReloadCallback(HandleUIReloaded);
        }

        private void OnDisable()
        {
            panelRenderer.UnregisterUIReloadCallback(HandleUIReloaded);
        }

        public void Show(FishSpecies species)
        {
            revealedSpecies = species;
            fishNameLabel.text = species.DisplayName;
            rarityLabel.text = species.Rarity.ToString();
            rarityLabel.style.color = gameplayConfig.GetTier(species.Rarity).DisplayColor;
            fishArt.style.backgroundColor = species.PlaceholderColor;
            revealPanel.RemoveFromClassList(hiddenClassName);

            // The window covers the water, so clicks on it must not drop food.
            feeder.enabled = false;
        }

        // Runs once the panel has built its UXML (after the first frame) and again on every live reload.
        private void HandleUIReloaded(PanelRenderer renderer, VisualElement rootElement, int version)
        {
            revealPanel = rootElement.Q<VisualElement>(revealPanelName);
            fishNameLabel = rootElement.Q<Label>(fishNameLabelName);
            rarityLabel = rootElement.Q<Label>(rarityLabelName);
            fishArt = rootElement.Q<VisualElement>(fishArtName);
            rootElement.Q<Button>(confirmButtonName).clicked += Confirm;
        }

        private void Confirm()
        {
            revealPanel.AddToClassList(hiddenClassName);
            feeder.enabled = true;
            spawner.Spawn(revealedSpecies);
        }
    }
}

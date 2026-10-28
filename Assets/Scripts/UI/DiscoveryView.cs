using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Chillquarium.Gameplay
{
    /// <summary>
    /// Shows a window with a species' adult the first time one grows up. Discoveries that come together
    /// wait and show one after another; the state lives in FishDiscoveries.
    /// </summary>
    public sealed class DiscoveryView : MonoBehaviour
    {
        [SerializeField] private PanelRenderer panelRenderer;
        [SerializeField] private GameplayConfig gameplayConfig;
        [SerializeField] private FishDiscoveries discoveries;
        [SerializeField] private AudioSource soundSource;
        [SerializeField] private AudioClip revealSound;
        [SerializeField] private string discoveryPanelName = "discovery-panel";
        [SerializeField] private string discoveryWindowName = "discovery-window";
        [SerializeField] private string discoveryFishName = "discovery-fish";
        [SerializeField] private string discoveryNameLabelName = "discovery-name-label";
        [SerializeField] private string discoveryRarityLabelName = "discovery-rarity-label";
        [SerializeField] private string discoveryOkButtonName = "discovery-ok-button";
        [SerializeField] private string hiddenClassName = "hidden";
        [SerializeField] private string discoveryWindowOpenClassName = "discovery-window--open";

        private readonly Queue<FishSpecies> waiting = new();
        private FishSpecies shown;
        private VisualElement discoveryPanel;
        private VisualElement discoveryWindow;
        private VisualElement discoveryFish;
        private Label discoveryNameLabel;
        private Label discoveryRarityLabel;
        private Button discoveryOkButton;

        private void OnEnable()
        {
            panelRenderer.RegisterUIReloadCallback(HandleUIReloaded);
            discoveries.Discovered += HandleDiscovered;
        }

        private void OnDisable()
        {
            panelRenderer.UnregisterUIReloadCallback(HandleUIReloaded);
            discoveries.Discovered -= HandleDiscovered;
        }

        // Runs once the panel has built its UXML (after the first frame) and again on every live reload.
        private void HandleUIReloaded(PanelRenderer renderer, VisualElement rootElement, int version)
        {
            discoveryPanel = rootElement.Q<VisualElement>(discoveryPanelName);
            discoveryWindow = rootElement.Q<VisualElement>(discoveryWindowName);
            discoveryFish = rootElement.Q<VisualElement>(discoveryFishName);
            discoveryNameLabel = rootElement.Q<Label>(discoveryNameLabelName);
            discoveryRarityLabel = rootElement.Q<Label>(discoveryRarityLabelName);
            discoveryOkButton = rootElement.Q<Button>(discoveryOkButtonName);

            // Removing first keeps a reload of the same tree from listening twice.
            discoveryOkButton.clicked -= ShowNext;
            discoveryOkButton.clicked += ShowNext;

            if (shown != null)
            {
                Show(shown);
            }
            else
            {
                discoveryPanel.AddToClassList(hiddenClassName);
            }
        }

        private void HandleDiscovered(FishSpecies species)
        {
            waiting.Enqueue(species);

            if (shown == null)
            {
                ShowNext();
            }
        }

        // Also the OK button: closes the window, or swaps in the next discovery that is waiting.
        private void ShowNext()
        {
            if (waiting.Count == 0)
            {
                shown = null;
                discoveryPanel.AddToClassList(hiddenClassName);
                return;
            }

            Show(waiting.Dequeue());
            soundSource.PlayOneShot(revealSound);
        }

        // The open overlay covers the whole screen and takes the pointer, like the shop, so the water does not react.
        private void Show(FishSpecies species)
        {
            shown = species;
            discoveryFish.style.backgroundImage = new StyleBackground(species.AdultSprite);
            discoveryNameLabel.text = species.DisplayName;
            discoveryRarityLabel.text = species.Rarity.ToString();
            discoveryRarityLabel.style.color = gameplayConfig.GetTier(species.Rarity).DisplayColor;
            discoveryPanel.RemoveFromClassList(hiddenClassName);

            // Start small and clear, then let the USS transition grow the window in; the wait of one frame
            // gives the transition a starting style.
            discoveryWindow.RemoveFromClassList(discoveryWindowOpenClassName);
            discoveryWindow.schedule.Execute(() => discoveryWindow.AddToClassList(discoveryWindowOpenClassName)).StartingIn(16);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Chillquarium.Gameplay
{
    /// <summary>
    /// The bought fish cards waiting at the bottom of the tank. Each click on the pack opens one card and adds its fish to the row;
    /// pressing a fish card, or sliding onto it with the button held, sends that fish into the tank as a baby from where the card was.
    /// </summary>
    public sealed class CardPackView : MonoBehaviour
    {
        [SerializeField] private PanelRenderer panelRenderer;
        [SerializeField] private GameplayConfig gameplayConfig;
        [SerializeField] private FishSpawner spawner;
        [SerializeField] private Camera worldCamera;
        [SerializeField] private SwimArea swimArea;
        [SerializeField] private AudioSource soundSource;
        [SerializeField] private AudioClip revealSound;
        [SerializeField] private AudioClip collectSound;
        [SerializeField] private string packLayerName = "pack-layer";
        [SerializeField] private string packButtonName = "pack-button";
        [SerializeField] private string packCountLabelName = "pack-count-label";
        [SerializeField] private string cardRowName = "card-row";
        [SerializeField] private string hiddenClassName = "hidden";
        [SerializeField] private string cardClassName = "fish-card";
        [SerializeField] private string cardPopClassName = "fish-card--pop";
        [SerializeField] private string cardCollectClassName = "fish-card--collect";
        [SerializeField] private string cardSqueezeClassName = "fish-card--squeeze";
        [SerializeField] private string cardNameClassName = "fish-card__name";
        [SerializeField] private string cardRarityClassName = "fish-card__rarity";
        [SerializeField] private string cardFrameClassName = "fish-card__frame";
        [SerializeField] private string cardFishClassName = "fish-card__fish";
        [Tooltip("Delay between the fish cards of one opened card, so they come in one after another.")]
        [SerializeField, Min(0)] private int cardPopStaggerMilliseconds = 70;
        [Tooltip("How long the cards already out stay squeezed when another card opens, before they spring back.")]
        [SerializeField, Min(1)] private int cardSqueezeMilliseconds = 120;
        [Tooltip("How long a collected card spins away before it is removed. Match the USS transition.")]
        [SerializeField, Min(1)] private int collectMilliseconds = 350;
        [Tooltip("Panel pixels between the points checked along a swipe, so a fast swipe does not skip cards.")]
        [SerializeField, Min(1f)] private float swipeSampleSpacing = 40f;

        private readonly List<FishSpecies> unclaimedSpecies = new();
        private VisualElement hudRoot;
        private VisualElement packLayer;
        private Button packButton;
        private Label packCountLabel;
        private VisualElement cardRow;
        private int packCount;
        private Vector2 lastPointerPanelPosition;
        private bool wasPointerHeld;

        public int PackCount => packCount;
        // Fish whose cards are laid out but not clicked yet. The save keeps them as tank fish so they are not lost.
        public IReadOnlyList<FishSpecies> UnclaimedSpecies => unclaimedSpecies;

        private void OnEnable()
        {
            panelRenderer.RegisterUIReloadCallback(HandleUIReloaded);
        }

        // Collects by polling the mouse rather than by UI pointer events: a press, and every card the pointer
        // passes over while the button is held, is collected, so one swipe can take a whole row.
        private void Update()
        {
            var mouse = Mouse.current;

            if (unclaimedSpecies.Count == 0 || !mouse.leftButton.isPressed)
            {
                wasPointerHeld = false;
                return;
            }

            var screenPosition = mouse.position.ReadValue();

            // Input System y counts up from the bottom; panel y counts down from the top.
            var panelPosition = RuntimePanelUtils.ScreenToPanel(hudRoot.panel, new Vector2(screenPosition.x, Screen.height - screenPosition.y));
            var from = wasPointerHeld ? lastPointerPanelPosition : panelPosition;
            var steps = Mathf.Max(1, Mathf.CeilToInt(Vector2.Distance(from, panelPosition) / swipeSampleSpacing));

            for (var step = 1; step <= steps; step++)
            {
                CollectCardAt(Vector2.Lerp(from, panelPosition, (float)step / steps));
            }

            lastPointerPanelPosition = panelPosition;
            wasPointerHeld = true;
        }

        private void OnDisable()
        {
            panelRenderer.UnregisterUIReloadCallback(HandleUIReloaded);
        }

        public void AddPack()
        {
            packCount++;
            ShowPack();
        }

        // GameSave restores before the panel has built its UXML; HandleUIReloaded shows the count once it has.
        public void RestorePacks(int count)
        {
            packCount = count;
        }

        // Runs once the panel has built its UXML (after the first frame) and again on every live reload.
        private void HandleUIReloaded(PanelRenderer renderer, VisualElement rootElement, int version)
        {
            hudRoot = rootElement;
            packLayer = rootElement.Q<VisualElement>(packLayerName);
            packButton = rootElement.Q<Button>(packButtonName);
            packCountLabel = rootElement.Q<Label>(packCountLabelName);
            cardRow = rootElement.Q<VisualElement>(cardRowName);
            packButton.clicked += OpenPack;

            // A reload rebuilds the UXML, so lay out the cards that were showing again.
            foreach (var species in unclaimedSpecies)
            {
                AddCard(species, 0);
            }

            ShowPack();
            UpdateLayerPicking();
        }

        private void OpenPack()
        {
            if (packCount == 0)
            {
                return;
            }

            packCount--;
            ShowPack();
            SqueezeCards();

            for (var index = 0; index < gameplayConfig.FishPerCard; index++)
            {
                var species = CardRoll.PickSpecies(gameplayConfig);
                unclaimedSpecies.Add(species);
                AddCard(species, index * cardPopStaggerMilliseconds);
            }

            soundSource.PlayOneShot(revealSound);
            UpdateLayerPicking();
        }

        private void ShowPack()
        {
            packCountLabel.text = "x" + packCount;
            packButton.EnableInClassList(hiddenClassName, packCount == 0);
        }

        // While fish cards are out, the full-screen layer takes the pointer: presses on the water do not drop food,
        // and a swipe can start anywhere. It sits under the top bar, so the Shop button still works.
        private void UpdateLayerPicking()
        {
            packLayer.pickingMode = unclaimedSpecies.Count > 0 ? PickingMode.Position : PickingMode.Ignore;
        }

        // The cards already out flinch sideways, as if the new ones push in, then spring back.
        private void SqueezeCards()
        {
            foreach (var card in cardRow.Children())
            {
                if (card.ClassListContains(cardCollectClassName))
                {
                    continue;
                }

                card.AddToClassList(cardSqueezeClassName);
                card.schedule.Execute(() => card.RemoveFromClassList(cardSqueezeClassName)).StartingIn(cardSqueezeMilliseconds);
            }
        }

        private void AddCard(FishSpecies species, int popDelayMilliseconds)
        {
            var card = new VisualElement();
            card.AddToClassList(cardClassName);

            var nameLabel = new Label(species.DisplayName);
            nameLabel.AddToClassList(cardNameClassName);
            card.Add(nameLabel);

            var rarityLabel = new Label(species.Rarity.ToString());
            rarityLabel.AddToClassList(cardRarityClassName);
            rarityLabel.style.color = gameplayConfig.GetTier(species.Rarity).DisplayColor;
            card.Add(rarityLabel);

            var frame = new VisualElement();
            frame.AddToClassList(cardFrameClassName);
            card.Add(frame);

            var fish = new VisualElement();
            fish.AddToClassList(cardFishClassName);
            fish.style.backgroundImage = new StyleBackground(species.AdultSprite);
            frame.Add(fish);

            // Only the card itself is picked, so anywhere on it finds the card; it keeps its species for the collect.
            nameLabel.pickingMode = PickingMode.Ignore;
            rarityLabel.pickingMode = PickingMode.Ignore;
            frame.pickingMode = PickingMode.Ignore;
            fish.pickingMode = PickingMode.Ignore;
            card.userData = species;
            cardRow.Add(card);

            // Start with no width, small and low, then let the USS transition grow it in; the wait of at least
            // one frame gives the transition a starting style, and the growing width slides the other cards aside.
            card.AddToClassList(cardPopClassName);
            card.schedule.Execute(() => card.RemoveFromClassList(cardPopClassName)).StartingIn(popDelayMilliseconds + 16);
        }

        private void CollectCardAt(Vector2 panelPosition)
        {
            // A collected card ignores picking, so it is never found twice.
            var picked = hudRoot.panel.Pick(panelPosition);

            if (picked != null && picked.parent == cardRow)
            {
                CollectCard(picked, (FishSpecies)picked.userData);
            }
        }

        private void CollectCard(VisualElement card, FishSpecies species)
        {
            card.pickingMode = PickingMode.Ignore;
            card.AddToClassList(cardCollectClassName);
            unclaimedSpecies.Remove(species);

            // The fish swims out where the card was clicked, kept inside the water.
            var area = swimArea.Area;
            Vector2 clickPoint = worldCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            var position = new Vector3(Mathf.Clamp(clickPoint.x, area.xMin, area.xMax), Mathf.Clamp(clickPoint.y, area.yMin, area.yMax), 0f);
            spawner.Spawn(species, 0f, position);
            soundSource.PlayOneShot(collectSound);

            // Free the water only after the card has gone; the press that took the last card must not also drop food.
            packLayer.schedule.Execute(UpdateLayerPicking).StartingIn(collectMilliseconds);

            // The collect class also shrinks the card's width to 0, so the rest slide together and stay centered.
            card.schedule.Execute(() => card.RemoveFromHierarchy()).StartingIn(collectMilliseconds);
        }
    }
}

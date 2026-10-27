using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Chillquarium.Gameplay
{
    /// <summary>
    /// Shows the bought fish cards waiting at the bottom of the tank and the fish cards laid out from them.
    /// Each click on the pack opens one card; pressing a fish card, or passing over it with the button held,
    /// sends that fish into the tank as a baby. The state lives in CardPacks.
    /// </summary>
    public sealed class CardPackView : MonoBehaviour
    {
        [SerializeField] private PanelRenderer panelRenderer;
        [SerializeField] private GameplayConfig gameplayConfig;
        [SerializeField] private CardPacks cardPacks;
        [SerializeField] private Camera worldCamera;
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
        [Tooltip("Screen pixels between the points checked along a swipe, so a fast swipe does not skip cards.")]
        [SerializeField, Min(1f)] private float swipeSampleSpacing = 40f;

        // Only the collect state changes rotate, so its end marks the moment a collected card has spun away.
        private static readonly StylePropertyName CollectEndProperty = new("rotate");

        private VisualElement hudRoot;
        private VisualElement packLayer;
        private Button packButton;
        private Label packCountLabel;
        private VisualElement cardRow;
        private Vector2 lastPointerScreenPosition;
        private bool wasPointerHeld;

        private void OnEnable()
        {
            panelRenderer.RegisterUIReloadCallback(HandleUIReloaded);
        }

        // Collects by polling the mouse rather than by UI pointer events, which do not arrive while the button is held:
        // a press, and every card the pointer passes over while the button is held, is collected.
        private void Update()
        {
            var mouse = Mouse.current;

            if (cardPacks.UnclaimedSpecies.Count == 0 || !mouse.leftButton.isPressed)
            {
                wasPointerHeld = false;
                return;
            }

            var screenPosition = mouse.position.ReadValue();
            var from = wasPointerHeld ? lastPointerScreenPosition : screenPosition;
            var steps = Mathf.Max(1, Mathf.CeilToInt(Vector2.Distance(from, screenPosition) / swipeSampleSpacing));

            for (var step = 1; step <= steps; step++)
            {
                CollectCardAt(Vector2.Lerp(from, screenPosition, (float)step / steps));
            }

            lastPointerScreenPosition = screenPosition;
            wasPointerHeld = true;
        }

        private void OnDisable()
        {
            panelRenderer.UnregisterUIReloadCallback(HandleUIReloaded);
            cardPacks.PackCountChanged -= ShowPackCount;
        }

        // Runs once the panel has built its UXML (after the first frame) and again on every live reload.
        private void HandleUIReloaded(PanelRenderer renderer, VisualElement rootElement, int version)
        {
            hudRoot = rootElement;
            packLayer = rootElement.Q<VisualElement>(packLayerName);
            packButton = rootElement.Q<Button>(packButtonName);
            packCountLabel = rootElement.Q<Label>(packCountLabelName);
            cardRow = rootElement.Q<VisualElement>(cardRowName);
            packButton.style.backgroundImage = new StyleBackground(cardPacks.PackType.Art);

            // Removing first keeps a reload of the same tree from listening twice.
            packButton.clicked -= OpenPack;
            packButton.clicked += OpenPack;
            cardPacks.PackCountChanged -= ShowPackCount;
            cardPacks.PackCountChanged += ShowPackCount;

            // Lay out the fish cards from the state, so a reload shows each one exactly once.
            cardRow.Clear();

            foreach (var species in cardPacks.UnclaimedSpecies)
            {
                AddCard(species, 0);
            }

            ShowPackCount();
            UpdateLayerPicking();
        }

        private void OpenPack()
        {
            var opened = cardPacks.Open();

            if (opened.Count == 0)
            {
                return;
            }

            SqueezeCards();

            for (var index = 0; index < opened.Count; index++)
            {
                AddCard(opened[index], index * cardPopStaggerMilliseconds);
            }

            soundSource.PlayOneShot(revealSound);
            UpdateLayerPicking();
        }

        private void ShowPackCount()
        {
            packCountLabel.text = "x" + cardPacks.PackCount;
            packButton.EnableInClassList(hiddenClassName, cardPacks.PackCount == 0);
        }

        // While fish cards are on screen, the full-screen layer takes the pointer: presses on the water do not drop food,
        // and a swipe can start anywhere. It sits under the top bar, so the Shop button still works.
        private void UpdateLayerPicking()
        {
            packLayer.pickingMode = cardRow.childCount > 0 ? PickingMode.Position : PickingMode.Ignore;
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

            // Removed once its spin-away ends, so the USS transition alone sets how long that takes.
            card.RegisterCallback<TransitionEndEvent>(transitionEvent =>
            {
                if (transitionEvent.target == card && transitionEvent.stylePropertyNames.Contains(CollectEndProperty))
                {
                    card.RemoveFromHierarchy();

                    // Free the water only once the card has gone, so the press that took the last card does not also drop food.
                    UpdateLayerPicking();
                }
            });
            cardRow.Add(card);

            // Start with no width, small and low, then let the USS transition grow it in; the wait of at least
            // one frame gives the transition a starting style, and the growing width slides the other cards aside.
            card.AddToClassList(cardPopClassName);
            card.schedule.Execute(() => card.RemoveFromClassList(cardPopClassName)).StartingIn(popDelayMilliseconds + 16);
        }

        private void CollectCardAt(Vector2 screenPosition)
        {
            // A collected card ignores picking, so it is never found twice.
            var picked = PanelPicking.Pick(hudRoot.panel, screenPosition);

            if (picked != null && picked.parent == cardRow)
            {
                CollectCard(picked, (FishSpecies)picked.userData, screenPosition);
            }
        }

        private void CollectCard(VisualElement card, FishSpecies species, Vector2 screenPosition)
        {
            card.pickingMode = PickingMode.Ignore;

            // The collect class spins the card away and shrinks its width to 0, so the rest slide together and stay centered.
            card.AddToClassList(cardCollectClassName);

            // The fish swims out where the pointer passed over its card.
            Vector2 worldPoint = worldCamera.ScreenToWorldPoint(screenPosition);
            cardPacks.Collect(species, worldPoint);
            soundSource.PlayOneShot(collectSound);
        }
    }
}

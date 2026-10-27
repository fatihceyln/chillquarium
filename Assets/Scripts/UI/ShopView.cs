using UnityEngine;
using UnityEngine.UIElements;

namespace Chillquarium.Gameplay
{
    /// <summary>
    /// The shop window: opens from the Shop button and sells the fish card, one at a time or several at once;
    /// bought cards wait as a pack in the tank.
    /// </summary>
    public sealed class ShopView : MonoBehaviour
    {
        [SerializeField] private PanelRenderer panelRenderer;
        [SerializeField] private MoneyWallet wallet;
        [SerializeField] private GameplayConfig gameplayConfig;
        [SerializeField] private CardPacks cardPacks;
        [SerializeField] private string shopButtonName = "shop-button";
        [SerializeField] private string shopPanelName = "shop-panel";
        [SerializeField] private string shopCloseButtonName = "shop-close-button";
        [SerializeField] private string cardNameLabelName = "card-name-label";
        [SerializeField] private string cardArtName = "card-art";
        [SerializeField] private string cardPriceLabelName = "card-price-label";
        [SerializeField] private string cardBuyButtonName = "card-buy-button";
        [SerializeField] private string cardBulkPriceLabelName = "card-bulk-price-label";
        [SerializeField] private string cardBulkBuyButtonName = "card-bulk-buy-button";
        [Tooltip("How many cards the second Buy button buys at once, each at the normal card price.")]
        [SerializeField, Min(2)] private int bulkBuyCount = 5;
        [SerializeField] private string hiddenClassName = "hidden";

        private VisualElement shopPanel;
        private Button shopButton;
        private Button shopCloseButton;
        private Button cardBuyButton;
        private Button cardBulkBuyButton;

        private double BulkPrice => gameplayConfig.CardPrice * bulkBuyCount;

        private void OnEnable()
        {
            panelRenderer.RegisterUIReloadCallback(HandleUIReloaded);
        }

        private void OnDisable()
        {
            panelRenderer.UnregisterUIReloadCallback(HandleUIReloaded);
            wallet.BalanceChanged -= UpdateBuyButton;
        }

        // Runs once the panel has built its UXML (after the first frame) and again on every live reload.
        private void HandleUIReloaded(PanelRenderer renderer, VisualElement rootElement, int version)
        {
            shopPanel = rootElement.Q<VisualElement>(shopPanelName);
            shopButton = rootElement.Q<Button>(shopButtonName);
            shopCloseButton = rootElement.Q<Button>(shopCloseButtonName);
            cardBuyButton = rootElement.Q<Button>(cardBuyButtonName);
            cardBulkBuyButton = rootElement.Q<Button>(cardBulkBuyButtonName);
            cardBulkBuyButton.text = "Buy x" + bulkBuyCount;
            rootElement.Q<Label>(cardNameLabelName).text = cardPacks.PackType.DisplayName;
            rootElement.Q<VisualElement>(cardArtName).style.backgroundImage = new StyleBackground(cardPacks.PackType.Art);
            rootElement.Q<Label>(cardPriceLabelName).text = MoneyFormat.Format(gameplayConfig.CardPrice);
            rootElement.Q<Label>(cardBulkPriceLabelName).text = MoneyFormat.Format(BulkPrice);

            // Removing first keeps a reload of the same tree from listening twice.
            shopButton.clicked -= Open;
            shopButton.clicked += Open;
            shopCloseButton.clicked -= Close;
            shopCloseButton.clicked += Close;
            cardBuyButton.clicked -= BuyCard;
            cardBuyButton.clicked += BuyCard;
            cardBulkBuyButton.clicked -= BuyBulk;
            cardBulkBuyButton.clicked += BuyBulk;
            wallet.BalanceChanged -= UpdateBuyButton;
            wallet.BalanceChanged += UpdateBuyButton;
            UpdateBuyButton(wallet.Balance);
            Close();
        }

        // The open overlay covers the whole screen and takes the pointer, so nothing behind it, like the water,
        // the pack or the fish cards, reacts until the shop closes.
        private void Open()
        {
            shopPanel.RemoveFromClassList(hiddenClassName);
        }

        private void Close()
        {
            shopPanel.AddToClassList(hiddenClassName);
        }

        private void BuyCard()
        {
            Buy(1);
        }

        private void BuyBulk()
        {
            Buy(bulkBuyCount);
        }

        private void Buy(int count)
        {
            if (!wallet.TrySpend(gameplayConfig.CardPrice * count))
            {
                return;
            }

            Close();
            cardPacks.AddPacks(count);
        }

        private void UpdateBuyButton(double balance)
        {
            cardBuyButton.SetEnabled(balance >= gameplayConfig.CardPrice);
            cardBulkBuyButton.SetEnabled(balance >= BulkPrice);
        }
    }
}

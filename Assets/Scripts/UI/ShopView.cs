using UnityEngine;
using UnityEngine.UIElements;

namespace Chillquarium.Gameplay
{
    /// <summary>
    /// The shop window: opens from the Shop button and sells the fish card, which then waits as a pack in the tank.
    /// </summary>
    public sealed class ShopView : MonoBehaviour
    {
        [SerializeField] private PanelRenderer panelRenderer;
        [SerializeField] private MoneyWallet wallet;
        [SerializeField] private GameplayConfig gameplayConfig;
        [SerializeField] private Feeder feeder;
        [SerializeField] private CardPackView cardPackView;
        [SerializeField] private string shopButtonName = "shop-button";
        [SerializeField] private string shopPanelName = "shop-panel";
        [SerializeField] private string shopCloseButtonName = "shop-close-button";
        [SerializeField] private string cardPriceLabelName = "card-price-label";
        [SerializeField] private string cardBuyButtonName = "card-buy-button";
        [SerializeField] private string hiddenClassName = "hidden";

        private VisualElement shopPanel;
        private Button cardBuyButton;

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
            cardBuyButton = rootElement.Q<Button>(cardBuyButtonName);
            rootElement.Q<Label>(cardPriceLabelName).text = MoneyFormat.Format(gameplayConfig.CardPrice);

            rootElement.Q<Button>(shopButtonName).clicked += Open;
            rootElement.Q<Button>(shopCloseButtonName).clicked += Close;
            cardBuyButton.clicked += BuyCard;

            // Removing first keeps a live reload from listening twice.
            wallet.BalanceChanged -= UpdateBuyButton;
            wallet.BalanceChanged += UpdateBuyButton;
            UpdateBuyButton(wallet.Balance);
            Close();
        }

        private void Open()
        {
            shopPanel.RemoveFromClassList(hiddenClassName);

            // The window covers the water, so clicks on it must not drop food.
            feeder.enabled = false;
        }

        private void Close()
        {
            shopPanel.AddToClassList(hiddenClassName);
            feeder.enabled = true;
        }

        private void BuyCard()
        {
            if (!wallet.TrySpend(gameplayConfig.CardPrice))
            {
                return;
            }

            Close();
            cardPackView.AddPack();
        }

        private void UpdateBuyButton(double balance)
        {
            cardBuyButton.SetEnabled(balance >= gameplayConfig.CardPrice);
        }
    }
}

using UnityEngine;
using UnityEngine.UIElements;

namespace Chillquarium.Gameplay
{
    /// <summary>
    /// Shows the wallet balance in the HUD.
    /// </summary>
    public sealed class MoneyView : MonoBehaviour
    {
        [SerializeField] private PanelRenderer panelRenderer;
        [SerializeField] private MoneyWallet wallet;
        [SerializeField] private string moneyLabelName = "money-label";

        private Label moneyLabel;

        private void OnEnable()
        {
            panelRenderer.RegisterUIReloadCallback(HandleUIReloaded);
        }

        private void OnDisable()
        {
            panelRenderer.UnregisterUIReloadCallback(HandleUIReloaded);
            wallet.BalanceChanged -= ShowBalance;
        }

        // Runs once the panel has built its UXML (after the first frame) and again on every live reload.
        private void HandleUIReloaded(PanelRenderer renderer, VisualElement rootElement, int version)
        {
            moneyLabel = rootElement.Q<Label>(moneyLabelName);

            // Listen only once the label exists. Removing first keeps a live reload from listening twice.
            wallet.BalanceChanged -= ShowBalance;
            wallet.BalanceChanged += ShowBalance;
            ShowBalance(wallet.Balance);
        }

        private void ShowBalance(double balance)
        {
            moneyLabel.text = MoneyFormat.Format(balance);
        }
    }
}

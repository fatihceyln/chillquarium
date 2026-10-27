using UnityEngine;
using UnityEngine.UIElements;

namespace Chillquarium.Gameplay
{
    /// <summary>
    /// Shows the wallet balance in the HUD. When money comes in, the panel bumps and a "+$…" label rises and fades.
    /// </summary>
    public sealed class MoneyView : MonoBehaviour
    {
        [SerializeField] private PanelRenderer panelRenderer;
        [SerializeField] private MoneyWallet wallet;
        [SerializeField] private string moneyLabelName = "money-label";
        [SerializeField] private string moneyName = "money";
        [SerializeField] private string bumpClassName = "money--bump";
        [SerializeField] private string gainClassName = "money-gain";
        [SerializeField] private string gainRiseClassName = "money-gain--rise";

        // The bump ends when the money panel has scaled up, and a gain label is gone when it has faded out.
        // Waiting for these transitions keeps the timing in the USS alone.
        private static readonly StylePropertyName BumpEndProperty = new("scale");
        private static readonly StylePropertyName GainEndProperty = new("opacity");

        private Label moneyLabel;
        private VisualElement money;
        private double shownBalance;

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
            money = rootElement.Q<VisualElement>(moneyName);
            money.UnregisterCallback<TransitionEndEvent>(HandleMoneyTransitionEnd);
            money.RegisterCallback<TransitionEndEvent>(HandleMoneyTransitionEnd);

            // Listen only once the label exists. Removing first keeps a live reload from listening twice.
            wallet.BalanceChanged -= ShowBalance;
            wallet.BalanceChanged += ShowBalance;

            // The balance on screen at load is not a gain.
            shownBalance = wallet.Balance;
            ShowBalance(wallet.Balance);
        }

        private void ShowBalance(double balance)
        {
            if (balance > shownBalance)
            {
                ShowGain(balance - shownBalance);
            }

            shownBalance = balance;
            moneyLabel.text = MoneyFormat.Format(balance);
        }

        private void ShowGain(double amount)
        {
            money.AddToClassList(bumpClassName);

            var gainLabel = new Label("+" + MoneyFormat.Format(amount));
            gainLabel.AddToClassList(gainClassName);
            gainLabel.RegisterCallback<TransitionEndEvent>(transitionEvent =>
            {
                if (transitionEvent.stylePropertyNames.Contains(GainEndProperty))
                {
                    gainLabel.RemoveFromHierarchy();
                }
            });
            money.Add(gainLabel);

            // Add the rise class a frame later, so the USS transition has a starting point to animate from.
            gainLabel.schedule.Execute(() => gainLabel.AddToClassList(gainRiseClassName));
        }

        private void HandleMoneyTransitionEnd(TransitionEndEvent transitionEvent)
        {
            // Gain labels inside the panel send their own transition events up; only the panel's scale ends the bump.
            if (transitionEvent.target == money && transitionEvent.stylePropertyNames.Contains(BumpEndProperty))
            {
                money.RemoveFromClassList(bumpClassName);
            }
        }
    }
}

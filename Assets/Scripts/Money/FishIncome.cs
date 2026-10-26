using UnityEngine;

namespace Chillquarium.Gameplay
{
    /// <summary>
    /// Pays the income of every fish in the tank into the wallet once per income interval.
    /// </summary>
    public sealed class FishIncome : MonoBehaviour
    {
        [SerializeField] private GameplayConfig gameplayConfig;
        [SerializeField] private MoneyWallet wallet;
        [SerializeField] private FishSpawner spawner;

        private float timeSinceLastPayment;

        private void Update()
        {
            timeSinceLastPayment += Time.deltaTime;
            var interval = gameplayConfig.IncomeIntervalSeconds;

            if (timeSinceLastPayment < interval)
            {
                return;
            }

            // Subtracting instead of resetting to zero keeps the leftover time, so payments do not drift late.
            timeSinceLastPayment -= interval;
            var total = 0d;

            foreach (var fish in spawner.Fishes)
            {
                total += fish.IncomePerSecond * interval;
            }

            wallet.Add(total);
        }
    }
}

using System;
using UnityEngine;

namespace Chillquarium.Gameplay
{
    /// <summary>
    /// The player's money. A double, because balances grow into the trillions where a float loses cents.
    /// </summary>
    public sealed class MoneyWallet : MonoBehaviour
    {
        private double balance;

        public event Action<double> BalanceChanged;

        public double Balance => balance;

        // GameSave sets the starting money of a new game or the saved balance.
        public void SetBalance(double amount)
        {
            balance = amount;
            BalanceChanged?.Invoke(balance);
        }

        public void Add(double amount)
        {
            balance += amount;
            BalanceChanged?.Invoke(balance);
        }

        public bool TrySpend(double amount)
        {
            if (amount > balance)
            {
                return false;
            }

            balance -= amount;
            BalanceChanged?.Invoke(balance);
            return true;
        }
    }
}

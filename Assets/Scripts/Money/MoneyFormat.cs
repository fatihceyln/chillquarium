using System.Globalization;

namespace Chillquarium.Gameplay
{
    public static class MoneyFormat
    {
        private static readonly string[] Suffixes = { "M", "B", "T", "Q", "Qi", "Sx", "Sp", "Oc", "No", "Dc" };

        // Like the reference game: the full amount below a million ($12,500.00), then three decimals and a suffix ($72.556T).
        public static string Format(double amount)
        {
            if (amount < 1_000_000d)
            {
                return "$" + amount.ToString("#,0.00", CultureInfo.InvariantCulture);
            }

            var scaled = amount / 1_000_000d;
            var suffixIndex = 0;

            while (scaled >= 1000d && suffixIndex < Suffixes.Length - 1)
            {
                scaled /= 1000d;
                suffixIndex++;
            }

            return "$" + scaled.ToString("0.000", CultureInfo.InvariantCulture) + Suffixes[suffixIndex];
        }
    }
}

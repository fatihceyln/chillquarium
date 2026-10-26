using UnityEngine;

namespace Chillquarium.Gameplay
{
    /// <summary>
    /// Picks the species a fish card gives, using the rarity drop weights.
    /// </summary>
    public static class CardRoll
    {
        public static FishSpecies PickSpecies(GameplayConfig gameplayConfig)
        {
            var cardSpecies = gameplayConfig.CardSpecies;
            var totalWeight = 0f;

            foreach (var species in cardSpecies)
            {
                totalWeight += Weight(species, gameplayConfig);
            }

            // Walk the species until the random point falls inside one species' share of the total.
            var roll = Random.Range(0f, totalWeight);

            foreach (var species in cardSpecies)
            {
                roll -= Weight(species, gameplayConfig);

                if (roll < 0f)
                {
                    return species;
                }
            }

            // A roll of exactly the total weight passes every share.
            return cardSpecies[cardSpecies.Count - 1];
        }

        // Species of the same rarity split that rarity's weight, so adding a second Rare does not make Rares more common.
        private static float Weight(FishSpecies species, GameplayConfig gameplayConfig)
        {
            var sameRarityCount = 0;

            foreach (var other in gameplayConfig.CardSpecies)
            {
                if (other.Rarity == species.Rarity)
                {
                    sameRarityCount++;
                }
            }

            return gameplayConfig.GetTier(species.Rarity).DropWeight / sameRarityCount;
        }
    }
}

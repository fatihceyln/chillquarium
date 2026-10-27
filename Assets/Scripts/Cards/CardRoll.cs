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
            var fishes = gameplayConfig.Fishes;
            var totalWeight = 0f;

            foreach (var fish in fishes)
            {
                totalWeight += Weight(fish.Species, gameplayConfig);
            }

            // Walk the species until the random point falls inside one species' share of the total.
            var roll = Random.Range(0f, totalWeight);

            foreach (var fish in fishes)
            {
                roll -= Weight(fish.Species, gameplayConfig);

                if (roll < 0f)
                {
                    return fish.Species;
                }
            }

            // A roll of exactly the total weight passes every share.
            return fishes[fishes.Count - 1].Species;
        }

        // Species of the same rarity split that rarity's weight, so adding a second Rare does not make Rares more common.
        private static float Weight(FishSpecies species, GameplayConfig gameplayConfig)
        {
            var sameRarityCount = 0;

            foreach (var other in gameplayConfig.Fishes)
            {
                if (other.Species.Rarity == species.Rarity)
                {
                    sameRarityCount++;
                }
            }

            return gameplayConfig.GetTier(species.Rarity).DropWeight / sameRarityCount;
        }
    }
}

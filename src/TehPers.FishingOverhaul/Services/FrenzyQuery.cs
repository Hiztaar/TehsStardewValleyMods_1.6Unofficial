using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;
using StardewValley.Delegates;

namespace TehPers.FishingOverhaul.Services
{
    /// <summary>
    /// Custom query to check if a player is fishing in a specific frenzy.
    /// </summary>
    public static class FrenzyQuery
    {
        private static bool isRegistered = false;

        /// <summary>
        /// Registers the custom query with Stardew Valley's engine.
        /// Syntax: CATCHING_FRENZY_FISH [QualifiedItemId]
        /// Example: CATCHING_FRENZY_FISH (O)136
        /// </summary>
        public static void Register()
        {
            if (isRegistered)
            {
                return;
            }

            GameStateQuery.Register("CATCHING_FRENZY_FISH", CheckFrenzy);
            isRegistered = true;
        }

        private static bool CheckFrenzy(string[] query, GameStateQueryContext context)
        {
            // 1. Validation du contexte
            if (context.Location is not { } location)
            {
                return false;
            }

            if (context.Player is not { } player)
            {
                return false;
            }

            // 2. Validation des arguments
            if (query.Length < 2)
            {
                return false;
            }

            var targetFishId = query[1];

            // 3. Vérifier si la Frénésie active concerne ce poisson
            if (location.fishFrenzyFish.Value != targetFishId)
            {
                return false;
            }

            // 4. Vérifier si le joueur pêche DANS les bulles (Calcul manuel)
            if (player.CurrentTool is FishingRod rod)
            {
                // La position des bulles sur la carte (en coordonnées Tuiles/Tiles)
                var splashPoint = location.fishSplashPoint.Value;

                // Si le point est à (0,0), il n'y a pas de bulles actives
                if (splashPoint == Point.Zero)
                {
                    return false;
                }

                // La position du bouchon est en pixels, on divise par 64 pour avoir la tuile
                // On ajoute un petit offset pour s'assurer qu'on prend le centre du bouchon
                var bobberTileX = (int)(rod.bobber.X / 64f);
                var bobberTileY = (int)(rod.bobber.Y / 64f);

                // On compare : Le bouchon est-il sur la même tuile que les bulles ?
                return bobberTileX == splashPoint.X && bobberTileY == splashPoint.Y;
            }

            return false;
        }
    }
}

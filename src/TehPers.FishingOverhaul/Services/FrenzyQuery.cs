using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;
using StardewValley.Delegates;
using TehPers.FishingOverhaul.Config;

namespace TehPers.FishingOverhaul.Services
{
    /// <summary>
    /// Registers custom GameStateQueries for Fishing Overhaul.
    /// </summary>
    public static class FrenzyQuery
    {
        private static bool isRegistered = false;

        /// <summary>
        /// The active configuration for the fishing overhaul mod.
        /// Used to determine settings such as legendary fish recatchability.
        /// </summary>
        public static FishConfig? Configuration { get; set; }

        /// <summary>
        /// Registers the custom queries with Stardew Valley's engine.
        /// </summary>
        public static void Register()
        {
            if (isRegistered)
            {
                return;
            }

            GameStateQuery.Register("CATCHING_FRENZY_FISH", CheckFrenzy);
            GameStateQuery.Register("PLAYER_HAS_SPECIAL_ORDER_RULE", CheckSpecialOrderRule);
            GameStateQuery.Register("BOBBER_IN_RECT", CheckBobberInRect);
            GameStateQuery.Register("WATER_DEPTH", CheckWaterDepth);
            GameStateQuery.Register("PLAYER_TILE_X", CheckPlayerTileX);
            GameStateQuery.Register("PLAYER_TILE_Y", CheckPlayerTileY);

            // NEW: Recatch check
            GameStateQuery.Register("LEGENDARY_IS_RECHARGEABLE", CheckLegendaryRechargeable);

            isRegistered = true;
        }

        private static bool CheckLegendaryRechargeable(string[] query, GameStateQueryContext context)
        {
            // Syntax: LEGENDARY_IS_RECHARGEABLE <Target> <ItemID>
            if (query.Length < 3)
            {
                return false;
            }

            var targetPlayer = context.Player;
            if (targetPlayer == null)
            {
                return false;
            }

            var fishId = query[2]; // e.g., "(O)163"

            // 1. If the player NEVER caught it, allow it.
            if (!targetPlayer.fishCaught.ContainsKey(fishId))
            {
                return true;
            }

            // 2. If "Recatchable" option is disabled in config, deny it.
            if (Configuration == null || !Configuration.RecatchableLegendaries)
            {
                return false;
            }

            // 3. Frequency check
            if (Configuration.RecatchFrequency == RecatchFrequency.Always)
            {
                return true;
            }

            // Get the date of the last catch
            var key = $"Hiztaar.FishingOverhaulRevived/LastCaught_{fishId}";
            if (targetPlayer.modData.TryGetValue(key, out var lastCaughtStr) && uint.TryParse(lastCaughtStr, out var lastCaughtDay))
            {
                var daysWait = Configuration.RecatchFrequency switch
                {
                    RecatchFrequency.Daily => 1u,
                    RecatchFrequency.EveryTwoDays => 2u,
                    RecatchFrequency.Weekly => 7u,
                    RecatchFrequency.BiWeekly => 14u,
                    RecatchFrequency.Season => 28u,
                    RecatchFrequency.Year => 112u,
                    _ => 28u
                };

                // If enough days have passed since the last catch
                return Game1.stats.DaysPlayed >= lastCaughtDay + daysWait;
            }

            // If caught before having the mod (no date saved), allow immediate recatch
            return true;
        }

        private static bool CheckWaterDepth(string[] query, GameStateQueryContext context)
        {
            if (query.Length < 2 || !int.TryParse(query[1], out var minDepth))
            {
                return false;
            }
            return context.Player.CurrentTool is FishingRod rod && rod.clearWaterDistance >= minDepth;
        }

        private static bool CheckSpecialOrderRule(string[] query, GameStateQueryContext context)
        {
            return query.Length >= 3 && Game1.player.team.SpecialOrderRuleActive(query[2]);
        }

        private static bool CheckPlayerTileX(string[] query, GameStateQueryContext context)
        {
            return query.Length >= 4 && context.Player != null && int.TryParse(query[2], out var min) && int.TryParse(query[3], out var max) && context.Player.TilePoint.X >= min && context.Player.TilePoint.X < max;
        }

        private static bool CheckPlayerTileY(string[] query, GameStateQueryContext context)
        {
            return query.Length >= 4 && context.Player != null && int.TryParse(query[2], out var min) && int.TryParse(query[3], out var max) && context.Player.TilePoint.Y >= min && context.Player.TilePoint.Y < max;
        }

        private static bool CheckBobberInRect(string[] query, GameStateQueryContext context)
        {
            if (query.Length < 5)
            {
                return false;
            }
            if (!int.TryParse(query[1], out var x) || !int.TryParse(query[2], out var y) || !int.TryParse(query[3], out var w) || !int.TryParse(query[4], out var h))
            {
                return false;
            }
            if (context.Player.CurrentTool is FishingRod rod)
            {
                var bx = (int)(rod.bobber.X / 64f);
                var by = (int)(rod.bobber.Y / 64f);
                return bx >= x && bx < x + w && by >= y && by < y + h;
            }
            return false;
        }

        private static bool CheckFrenzy(string[] query, GameStateQueryContext context)
        {
            if (context.Location == null || context.Player == null || query.Length < 2)
            {
                return false;
            }
            if (context.Location.fishFrenzyFish.Value != query[1])
            {
                return false;
            }
            if (context.Player.CurrentTool is FishingRod rod)
            {
                var pt = context.Location.fishSplashPoint.Value;
                if (pt == Point.Zero)
                {
                    return false;
                }
                return (int)(rod.bobber.X / 64f) == pt.X && (int)(rod.bobber.Y / 64f) == pt.Y;
            }
            return false;
        }
    }
}

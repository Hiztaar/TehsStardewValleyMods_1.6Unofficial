using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using TehPers.Core.Api.Items;
using TehPers.FishingOverhaul.Api;
using TehPers.FishingOverhaul.Api.Content;

namespace TehPers.FishingOverhaul.Services
{
    internal sealed partial class DefaultFishingSource : IFishingContentSource
    {
        private readonly IMonitor monitor;
        private readonly IManifest manifest;

        public DefaultFishingSource(IMonitor monitor, IManifest manifest)
        {
            this.monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
            this.manifest = manifest ?? throw new ArgumentNullException(nameof(manifest));
        }

        public IEnumerable<FishingContent> Reload(IMonitor _)
        {
            var fishContent = this.GetDefaultFishData();
            var trashContent = this.GetDefaultTrashData();
            var treasureContent = this.GetDefaultTreasureData();
            var effectContent = this.GetDefaultEffectData();

            // --- 1.6 JELLIES LOGIC ---
            var jellies = new List<TrashEntry>
            {
                // River Jelly
                new(
                    NamespacedKey.SdvObject("RiverJelly"),
                    new AvailabilityInfo(0.05d)
                    {
                        WaterTypes = WaterTypes.River | WaterTypes.PondOrOcean,
                        IncludeLocations = ImmutableArray.Create("Town", "Mountain", "Forest", "Desert", "Woods")
                    }
                ),

                // Sea Jelly
                new(
                    NamespacedKey.SdvObject("SeaJelly"),
                    new AvailabilityInfo(0.05d)
                    {
                        WaterTypes = WaterTypes.PondOrOcean,
                        IncludeLocations = ImmutableArray.Create("Beach", "BeachNightMarket", "IslandWest", "IslandSouth", "IslandSouthEast")
                    }
                ),

                // Cave Jelly
                new(
                    NamespacedKey.SdvObject("CaveJelly"),
                    new AvailabilityInfo(0.05d)
                    {
                        WaterTypes = WaterTypes.All,
                        IncludeLocations = ImmutableArray.Create("UndergroundMine")
                    }
                )
            };

            // Combine default trash with new jellies
            var combinedTrash = trashContent.AddTrash.Concat(jellies).ToImmutableArray();

            yield return new FishingContent(this.manifest)
            {
                AddFish = fishContent.AddFish,
                SetFishTraits = fishContent.SetFishTraits,
                AddTrash = combinedTrash,
                AddTreasure = treasureContent.AddTreasure,
                AddEffects = effectContent.AddEffects
            };
        }
    }
}

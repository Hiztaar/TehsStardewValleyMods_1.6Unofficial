using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;
using StardewValley.SpecialOrders;
using TehPers.Core.Api.Setup;
using TehPers.FishingOverhaul.Services.Tokens;
using System.Reflection;
using TehPers.Core.Api.Items;
using StardewValley.Monsters;

namespace TehPers.FishingOverhaul.Services.Setup
{
    internal sealed class ContentPatcherSetup : ISetup
    {
        private readonly IManifest manifest;
        private readonly IContentPatcherAPI contentPatcherApi;
        private readonly MissingSecretNotesToken missingSecretNotesToken;
        private readonly MissingJournalScrapsToken missingJournalScrapsToken;

        public ContentPatcherSetup(
            IManifest manifest,
            IContentPatcherAPI contentPatcherApi,
            MissingSecretNotesToken missingSecretNotesToken,
            MissingJournalScrapsToken missingJournalScrapsToken
        )
        {
            this.manifest = manifest ?? throw new ArgumentNullException(nameof(manifest));
            this.contentPatcherApi = contentPatcherApi
                ?? throw new ArgumentNullException(nameof(contentPatcherApi));
            this.missingSecretNotesToken = missingSecretNotesToken
                ?? throw new ArgumentNullException(nameof(missingSecretNotesToken));
            this.missingJournalScrapsToken = missingJournalScrapsToken
                ?? throw new ArgumentNullException(nameof(missingJournalScrapsToken));
        }

        public void Setup()
        {
            // 1. Enregistrement principal (Pour votre mod)
            this.RegisterAllTokens(this.manifest);

            // 2. Enregistrement Legacy (Pour la compatibilité)
            IManifest legacyManifest = new LegacyIdManifest(this.manifest, "TehPers.FishingOverhaul");
            this.RegisterAllTokens(legacyManifest);
        }

        private void RegisterAllTokens(IManifest owner)
        {
            this.contentPatcherApi.RegisterToken(owner, "BooksFound", new BooksFoundToken());
            this.contentPatcherApi.RegisterToken(owner, "HasItem", new HasItemToken());
            this.contentPatcherApi.RegisterToken(
                owner,
                "SpecialOrderRuleActive",
                new MaybeReadyToken(ContentPatcherSetup.GetSpecialOrderRuleActive)
            );
            this.contentPatcherApi.RegisterToken(
                owner,
                "MissingSecretNotes",
                this.missingSecretNotesToken
            );
            this.contentPatcherApi.RegisterToken(
                owner,
                "MissingJournalScraps",
                this.missingJournalScrapsToken
            );
            this.contentPatcherApi.RegisterToken(
                owner,
                "RandomGoldenWalnuts",
                new MaybeReadyToken(ContentPatcherSetup.GetRandomGoldenWalnuts)
            );
            this.contentPatcherApi.RegisterToken(
                owner,
                "TidePoolGoldenWalnut",
                new MaybeReadyToken(ContentPatcherSetup.GetTidePoolGoldenWalnut)
            );
            this.contentPatcherApi.RegisterToken(
                owner,
                "ActiveBait",
                new MaybeReadyToken(ContentPatcherSetup.GetActiveBait)
            );
            this.contentPatcherApi.RegisterToken(
                owner,
                "ActiveTackle",
                new MaybeReadyToken(ContentPatcherSetup.GetActiveTackle)
            );
        }

        private static IEnumerable<string>? GetSpecialOrderRuleActive()
        {
            if (Game1.player is not { } player)
            {
                return null;
            }

            if (player is not { team.specialOrders: { } specialOrders })
            {
                return Enumerable.Empty<string>();
            }

            return specialOrders.SelectMany(
                    specialOrder =>
                    {
                        if (specialOrder.questState.Value is not SpecialOrderStatus.InProgress)
                        {
                            return Enumerable.Empty<string>();
                        }

                        if (specialOrder.specialRule.Value is not { } specialRule)
                        {
                            return Enumerable.Empty<string>();
                        }

                        return specialRule.Split(
                            ',',
                            StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries
                        );
                    }
                )
                .OrderBy(val => val, StringComparer.OrdinalIgnoreCase);
        }

        private static IEnumerable<string>? GetRandomGoldenWalnuts()
        {
            if (Game1.player is not { } player)
            {
                return null;
            }

            if (player is not { team: { limitedNutDrops: { } limitedNutDrops } })
            {
                return Enumerable.Empty<string>();
            }

            return limitedNutDrops.TryGetValue("IslandFishing", out var fishingNuts)
                ? new[] { fishingNuts.ToString("G") }
                : new[] { "0" };
        }

        private static IEnumerable<string>? GetTidePoolGoldenWalnut()
        {
            if (Game1.player is not { } player)
            {
                return null;
            }

            if (player is not { team: { } team })
            {
                return Enumerable.Empty<string>();
            }

            return team.collectedNutTracker.Contains("StardropPool")
                ? new[] { "true" }
                : new[] { "false" };
        }

        private static IEnumerable<string>? GetActiveBait()
        {
            if (Game1.player is not { CurrentItem: FishingRod rod })
            {
                return null;
            }

            var bait = rod.GetBait();
            return bait is null ? Enumerable.Empty<string>() : new[] { NamespacedKey.SdvObject(bait.ItemId).ToString() };
        }

        private static IEnumerable<string>? GetActiveTackle()
        {
            if (Game1.player is not { CurrentItem: FishingRod rod })
            {
                return null;
            }

            var tackleList = rod.GetTackle();
            if (tackleList == null)
            {
                return Enumerable.Empty<string>();
            }

            return tackleList
                .Where(tackle => tackle != null)
                .Select(tackle => NamespacedKey.SdvObject(tackle.ItemId).ToString());
        }

        /// <summary>
        /// A wrapper around IManifest to spoof the UniqueID.
        /// </summary>
        private class LegacyIdManifest : IManifest
        {
            private readonly IManifest original;
            public string UniqueID { get; }

            public LegacyIdManifest(IManifest original, string legacyId)
            {
                this.original = original;
                this.UniqueID = legacyId;
            }

            public string Name => this.original.Name;
            public string Description => this.original.Description;
            public string Author => this.original.Author;
            public ISemanticVersion Version => this.original.Version;
            public ISemanticVersion? MinimumApiVersion => this.original.MinimumApiVersion;
            public string? EntryDll => this.original.EntryDll;
            public IManifestContentPackFor? ContentPackFor => this.original.ContentPackFor;
            public IManifestDependency[] Dependencies => this.original.Dependencies;
            public string[] UpdateKeys => this.original.UpdateKeys;
            public IDictionary<string, object> ExtraFields => this.original.ExtraFields;
            public ISemanticVersion? MinimumGameVersion => this.original.MinimumGameVersion;
        }
    }
}

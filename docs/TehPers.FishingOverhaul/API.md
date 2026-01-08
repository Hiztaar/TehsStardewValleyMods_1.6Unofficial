# Teh's Fishing Overhaul Revived - API

**Teh's Fishing Overhaul Revived** provides a public API that other mods can use to interact with the fishing system, modify chances, query data, or add custom content.

## Accessing the API

The API is exposed through SMAPI's standard mod registry.

To access it, you first need to reference the **API assembly** in your project.

### 1. Add References

Add the following to your project's `.csproj` file. This assumes you have the mod installed in your game's `Mods` folder for development.

```xml
<ItemGroup>
    <Reference Include="TehPers.FishingOverhaul.Api">
        <HintPath>$(GameModsPath)\TehPers.FishingOverhaul\TehPers.FishingOverhaul.Api.dll</HintPath>
        <Private>false</Private>
    </Reference>
    <Reference Include="TehPers.Core.Api">
        <HintPath>$(GameModsPath)\TehPers.Core\TehPers.Core.Api.dll</HintPath>
        <Private>false</Private>
    </Reference>
</ItemGroup>

```

> **Note:** Set `<Private>false</Private>` (or `CopyLocal` to `false`) to ensure these DLLs are not copied into your mod's release folder.

### 2. Mod Dependencies

In your `manifest.json`, add a dependency to `Hiztaar.FishingOverhaulRevived` to ensure it loads before your mod.

```json
"Dependencies": [
   {
      "UniqueID": "Hiztaar.FishingOverhaulRevived",
      "IsRequired": true
   }
]

```

### 3. Request the API

In your `ModEntry.cs`, request the API using the unique ID `Hiztaar.FishingOverhaulRevived`:

```csharp
using TehPers.FishingOverhaul.Api;

public override void Entry(IModHelper helper)
{
    var fishingApi = helper.ModRegistry.GetApi<IFishingApi>("Hiztaar.FishingOverhaulRevived");

    if (fishingApi != null)
    {
        this.Monitor.Log("Fishing Overhaul API loaded!", LogLevel.Info);
    }
}

```

---

## API Features (`IFishingApi`)

The `IFishingApi` interface is the primary entry point. It allows you to calculate chances, preview loot, and manage player streaks.

### Fishing Info & Calculations

* **`CreateDefaultFishingInfo(Farmer farmer)`**: Creates a snapshot of the fishing context (location, bait, bobber, etc.) for a player. This object is required for most calculation methods.
* **`GetChanceForFish(FishingInfo info)`**: Returns the probability (0.0 to 1.0) that a cast will result in a fish bite rather than trash.
* **`GetChanceForTreasure(FishingInfo info)`**: Returns the probability (0.0 to 1.0) of a treasure chest appearing.

### Loot Tables

These methods return weighted lists of potential catches for a specific context.

* **`GetFishChances(FishingInfo info)`**: Returns all available fish and their weights.
* **`GetTrashChances(FishingInfo info)`**: Returns all available trash items.
* **`GetTreasureChances(FishingInfo info)`**: Returns all available treasure loot.
* **`GetPossibleCatch(FishingInfo info)`**: Simulates a single catch (returns either a `Fish` or `Trash` result).
* **`GetPossibleTreasure(CatchInfo.FishCatch catchInfo)`**: Simulates opening a treasure chest.

### Fish Data

* **`TryGetFishTraits(NamespacedKey fishKey, out FishTraits traits)`**: Retrieves data about a fish (difficulty, behavior, legendary status).
* **`IsLegendary(NamespacedKey fishKey)`**: Helper to check if a fish is legendary.

### Player State

* **`GetStreak(Farmer farmer)`**: Gets the player's current "Perfect" fishing streak.
* **`SetStreak(Farmer farmer, int streak)`**: Modifies the player's streak.

---

## Adding Custom Content (`IFishingContentSource`)

To add **new** fishing content (like custom fish rules, new treasure, or trash entries) via code instead of JSON content packs, you can implement `IFishingContentSource`.

> **Note:** This requires using the Dependency Injection system provided by `TehPers.Core`.

1. Create a class that implements `IFishingContentSource`.
2. Inject it into the kernel in your Mod Entry:

```csharp
// Request the mod kernel from TehPers.Core
var kernel = ModServices.Factory.GetKernel(this);

// Bind your content source so Fishing Overhaul can find it
kernel.GlobalProxyRoot
    .Bind<IFishingContentSource>()
    .To<YourContentSource>()
    .InSingletonScope();

```

Whenever your content changes (e.g. config update), call `IFishingApi.RequestReload()` to force the mod to rebuild its cache.

---

## Namespaced Keys

The API uses `NamespacedKey` to identify items.

* **Format:** `"<namespace>:<key>"`
* **Example:** `"StardewValley:Object/138"` (Rainbow Trout)

You can create these using helper methods:

```csharp
var key = NamespacedKey.SdvObject(138);
```


[simplified interface]: /TehPers.FishingOverhaul.Api/ISimplifiedFishingApi.cs
[content pack docs]: /docs/TehPers.FishingOverhaul/Content%20Packs.md
[ninject docs]: https://github.com/ninject/ninject/wiki

## Domain model and code index

This document binds **gathered items**, **equipment material enums**, **craft modifiers**, and the **canonical C# files** that define them.

### Primary types

| Concept | Purpose | Canonical location |
|---|---|---|
| `CraftResource` | Persisted discriminator on crafted/gather-derived items (`Item.Resource`); ordered in bands (`Iron`=1 … `TurtleSpec` end). | `World/Source/System/Item.cs` (enum starts ~L7175) |
| `CraftResourceType` | High-level taxonomy for tooling / imbuing tiers (`Metal`, `Leather`, `Scales`, `Wood`, `Block`, `Skin`, `Special`, `Skeletal`, `Fabric`). | `World/Source/Scripts/System/Misc/ResourceInfo.cs` L7–18 |
| `CraftResourceInfo` | One row per `CraftResource` material: hues, dmg/arm multipliers-ish fields, skill gate, misc tool stats, three Cliloc ids, optional `CraftAttributeInfo`, and `params Type[]` registration. Constructor calls `CraftResources.RegisterType` per type. | Same file ~L291–355; tables `m_MetalInfo` … `m_SkeletalInfo` L362+ |
| `CraftAttributeInfo` | Numeric packages applied to armour + weapons (`CraftAttributeInfo.*` ints). Factory: `CraftAttInfo(...)`. | Same file ~L21–289 |
| `CraftResources` | Static registry (`RegisterType`), getters (`GetInfo`, `GetFromType`, tier helpers toward file end ~L2790+). | Same file ~L357+ |
| `ResourceMods` / `ResourceModInfo` / `GemModInfo` | Apply crafted modifiers, salvage-to-gold for base resource item classes, imbue-ish attribute dumps. | `World/Source/Scripts/System/Misc/ResourceMods.cs` |

### `CraftResource` numeric bands (summary)

Exact ordering must match `Item.cs`. Notable jumps:

| First member | Assigned / implied value | Semantic band |
|---|---|---|
| `None` | 0 | Sentinel |
| `Iron` … `Xonolite` | 1–70 | Metals & alloys (ingot/ore/granite lines) |
| `AmethystBlock` | **71** (explicit) … `CaddelliteBlock` | Polished gemstone **blocks** (tinker masonry family) |
| `RegularLeather` | **101** … `Thermoweave` | Leathers then sci‑fi cloth-like leathers |
| `DemonSkin` | **151** … `DeadSkin` | Tanning hides / skins |
| `RedScales` | **201** … `KraytScales` | Dragon / exotic scales (`Gorn`… enums exist but see design note below) |
| `Fabric` | **251** … `FiendishFabric` | Tailoring cloth tiers |
| `RegularWood` | **301** … `VeshokTree` | Log / board tiers (Star Wars–named exotic woods occupy high end) |
| `BrittleSkeletal` | **401** … `ZabrakSkeletal` | Bone crafting skeleton types |
| `SpectralSpec` | **501** … `TurtleSpec` | “Special spectral” slabs + elemental concentrates + Exodus/Turtle shells |

Always diff `Item.cs` when adding enums — sentinel gaps are intentional bookkeeping; do **not** renumber without migration.

### Implementation quirk — high-tier reptile scales

In `m_ScaleInfo`, rows **Gorn**, **Trandoshan**, **Silurian**, **Krayt** use **`CraftResource.CadalyteScales`** as the persisted enum while **`typeof(GornScales)`** etc. still register distinctly.

Consequences:

1. **`GetFromType(item.GetType())`** remains correct — each concrete item class maps cleanly.
2. **`GetInfo(CraftResource.CadalyteScales)`** returns **whatever array index wins** inside `CraftResources` lookups — verify any code assuming one row per enum for those four display names during enhancement / imbuing UI.
3. Player-facing taxonomy should describe **four named scale items** separate from Cadalyte even though enum storage may coincide.

Coordinate changes with testers if you unify enums.

### Material registration

Every `CraftResourceInfo` ctor registers **`resourceTypes[] → CraftResource`**:

```352:354:World/Source/Scripts/System/Misc/ResourceInfo.cs
			for ( int i = 0; i < resourceTypes.Length; ++i )
				CraftResources.RegisterType( resourceTypes[i], resource );
```

Missing registration manifests as **`CraftResource.None`** after smelting or broken craft menus — when adding ore/log classes, mirror all three tiers (ore/ingot/board etc.) consistently.

### `ResourceMods.ResourceToGold` eligible bases

Eligible for direct “transmute stack to gold ingots mass” shortcut:

`BaseBlocks`, `BaseIngot`, `BaseOre`, `BaseScales`, `BaseWoodBoard`, `BaseSkeletal`, `BaseLog`, `BaseGranite`, `BaseSpecial`, `BaseHides`, `BaseFabric`, `BaseLeather`, `BaseSkins` — see `ResourceMods.ResourceToGold` ~L113–137.

### Cross-links

- Harvest loop details: [`02-harvest-systems.md`](02-harvest-systems.md)
- Full attribute rows: [`03-reference-tables-metal-scales-special.md`](03-reference-tables-metal-scales-special.md), [`04-reference-tables-leather-wood-fabric-block-skin-skeletal.md`](04-reference-tables-leather-wood-fabric-block-skin-skeletal.md)
- Player strings: [`07-localization-and-player-copy.md`](07-localization-and-player-copy.md)

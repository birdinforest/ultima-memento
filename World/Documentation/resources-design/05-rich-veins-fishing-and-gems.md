## Rich harvest extensions, fishing mutation, mined gems & jewelry sockets

---

### Rare ore (`RichVeinMining`)

**File:** `World/Source/Scripts/Engines and Systems/Trades/Harvest/RichVeins/RichVeinMining.cs`

Adds **second HarvestDefinition**:

| Property | Typical value vs normal mining |
|---|---|
| Tiles | Specialized **item/object id** list (**`m_RareNodeItemIds`**) mined via **`RichVeinMineable`** |
| Bank footprint | **1×1**, deep pocket (**20–34** pulls) |
| Respawn | **50–70** minutes |
| Consumption flags | **`int.MaxValue`** (acts as choke — bank empties logically per design) |

Extra harvest resource appended after Valorite:

```68:69:World/Source/Scripts/Engines and Systems/Trades/Harvest/RichVeins/RichVeinMining.cs
                    new HarvestResource( 100.1, 69.0, 140.0, "", typeof( DwarvenOre ),      typeof( DwarvenGranite ),       typeof( EarthElemental ) )
```

Vein probability layout shifts mass to iron fallback vs surface nodes (**`HarvestVein(0,...)` baseline** iron + weight list **46% DC** descending — see **`L72–85`**).

**`CheckHarvest` extension:** forbids **`Mining.Base < 65`** on **`RichVeinMineable`** targets (**`CheckHarvest(...)` overrides** truncated in doc — enforced in continuation of file).

**Possible message typo:** ctor sets **`DoubleHarvestMessage = 50304`** (**`RichVeinMining` L49**) while classic mining uses **`503042`** — validate Cliloc existence (may be dormant if branch never fires).

---

### Volatile lumber (`RichLumberjacking`)

**File:** `…/Harvest/RichLumberjacking/RichLumberjacking.cs`.

Clones baseline definition into **`VolatileHarvestDefinition`** layered on **`SECRET_TILE_ID = -2`** sentinel to overlay volatile banks on vanilla tree coordinates.

Volatile stats:

| Field | Override |
|---|---|
| Bank totals | **8–18** logs |
| Resources | Duplicate ordinary ladder **plus** **`ElvenLog` row (**`req 100.1 / min 95 / max 140`)** (`L74–87`) |
| Consumption | **`int.MaxValue`** pattern |
| Veins | Separate weight table ending **Elven vein 2.5%** fallback weight (`L103–104`) |

`GetHarvestDetails` replaces visible tile id with sentinel when **`m_VolatileDefinition`** owns the bank (**`HasBank`** check). Sparkle interaction path supports direct targeting sparkles (**`RichLumberjackingSparkle`** hook).

Finish hook seeds new sparkle candidates (**25%**, skill **≥55**, bank empty…) — propagate carefully when patching anti-exploit cooldowns.

---

### Fishing mutation table (`Fishing.MutateType`)

**Ordering matters** — first qualifying row wins stochastic draw.

| Req base skill | Min sk val | Max sk val | Deep only? | Possible types |
|---|---:|---:|:---:|---|
|80|80|4080|**yes**|`SpecialFishingNet`,`PearlSkull`,`FabledFishingNet`|
|90|80|4080|**yes**|`FabledFishingNet`,`NeptunesFishingNet`,`TreasureMap`,`MessageInABottle`|
|0|125|-2375|no|Prized/Wondrous/TrulyRare/Peculiar fish|
|0|105|-420|no|`WetClothes`,`RustyJunk`,`SpecialSeaweed`|
|50|125|-1000|no|`FishingNet`,`CorpseSailor`,`SunkenBag`,`NewFish`|
|0|200|-200|no|Sentinel **`null`** entry (may yield silent failure — audit usage)|

Interpretation helpers:

```192:197:World/Source/Scripts/Engines and Systems/Trades/Harvest/Fishing.cs
				if ( skillBase >= entry.m_ReqSkill )
				{
					double chance = (skillValue - entry.m_MinSkill) / (entry.m_MaxSkill - entry.m_MinSkill);

					if ( chance > Utility.RandomDouble() )
						return entry.m_Types[Utility.Random( entry.m_Types.Length )];
				}
```

Negative **`m_MaxSkill`** inverts slope — revisit when retuning to avoid exploding probabilities.

Regional **shipwreck** proximity toggles ancillary logic via **`IsNearHugeShipWreck`** (coordinate blocks per map listed in **`Fishing.cs`**).

---

### ML gem drops from ore (`Mining` bonus rows)

While **`Core.ML`**, each successful qualifying mine may roll **`BonusHarvestResource`** entries chaining independent gem drops with server string messages (**hardcoded English** harvesting lines — localize per **`07`**).

Gem item types dropped: **`Amber, Amethyst, Citrine, Diamond, Emerald, Ruby, Sapphire, StarSapphire, Tourmaline`** (each **~0.1%** contingent on **>=90 skill** rows — revisit exact weight table in **`Mining.cs` L114–121**).

These differ from **`m_BlockInfo`** gemstone masonry — do not confuse *raw mined gem* loot with **`AmethystBlocks`** tinkering mats.

---

### `CraftResources.GetGemMods` jewellery augments

**Function:** **`ResourceInfo.cs` ~L2684+** delegates to **`ResourceMods.ModifyJewelry`** per **`GemType`**.

Documented excerpts:

|Gem|Representative mods (non-exhaustive — read source for completeness)|
|---|---|
| Amber | RegenStam **+1**, BonusDex **+1**, EnhancePotions **+5** |
| Citrine | RegenStam **+1**, BonusDex **+1**, WeaponSpeed **+5** |
| Ruby | BonusStr **+1**, BonusHits **+3**, WeaponDamage **+2**, NightSight |

Other branches continue for Tourmaline, Amethyst… — **engineering must open file** whenever balancing socket economy.

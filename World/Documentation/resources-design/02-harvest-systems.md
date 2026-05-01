## Harvest systems (gather loops)

Harvest flows inherit `HarvestSystem`. Each **`HarvestDefinition`** configures bank geometry (`BankWidth` / `BankHeight`), per-bank totals, respawn window, **`Skill`**, **`Tiles`**, effect packets, **`HarvestResource[]`**, **`HarvestVein[]`**, optional **`BonusHarvestResource[]`**, and consumption multipliers gated by **`MyServerSettings.Resources()`** / Isles Dread.

### Harvest resource constructor semantics

```25:32:World/Source/Scripts/Engines and Systems/Trades/Harvest/HarvestResource.cs
		public HarvestResource( double reqSkill, double minSkill, double maxSkill, object message, params Type[] types )
		{
			m_ReqSkill = reqSkill;
			m_MinSkill = minSkill;
			m_MaxSkill = maxSkill;
			m_Types = types;
			m_SuccessMessage = message;
		}
```

- **`ReqSkill`** — gates whether the vein entry is attempted for the miner’s *base* skill (see core harvest math in `HarvestSystem`).
- **`MinSkill` / `MaxSkill`** — roll band for skill check success curves.
- **`message`** — `int` localized id or plaintext `SendMessage`; empty string skips custom success toast.
- **`types`** — `[0]` usually primary loot (ore/log/fish…); `[1]` alternate (granite/board…); `[2]` sometimes elemental garnish for mining tiers.

Veins additionally specify primary vs fallback distributions — see concrete rows below.

Shared code roots: `World/Source/Scripts/Engines and Systems/Trades/Harvest/`.

---

### `Mining` (+ `Sand`)

**File:** `Mining.cs`; **exported singleton:** `Mining.System` redirects to **`RichVeinMining.System`** — rare surface nodes reuse the richer class.

#### Ore + stone (`m_OreAndStone`)

| Field | Value / note |
|---|---|
| Tiles | Mountains/caves numeric id table in file (`m_MountainAndCaveTiles`) |
| Bank | **8×8** |
| Capacity | Total **5–17** consumed units per respawn slice |
| Respawn | **10–20** minutes |
| Skill | **`SkillName.Mining`** |
| **`ConsumedPerHarvest`** | **`1 × MyServerSettings.Resources()`** (scaled variant for Isles Dread) |

**Harvest resources:** Iron…Valorite tiers with skill gates **65→99**, each row listing ore + colored granite + optional elemental `Type`:

```81:103:World/Source/Scripts/Engines and Systems/Trades/Harvest/Mining.cs
			res = new HarvestResource[]
				{
					new HarvestResource( 00.0, 00.0, 85.0, "", typeof( IronOre ),			typeof( Granite ) ),
					new HarvestResource( 65.0, 25.0, 105.0, "", typeof( DullCopperOre ),	typeof( DullCopperGranite ),	typeof( DullCopperElemental ) ),
					...
					new HarvestResource( 99.0, 59.0, 139.0, "", typeof( ValoriteOre ),		typeof( ValoriteGranite ),		typeof( ValoriteElemental ) ),
				};
```

**Veins:** Iron baseline **45%** weight; rarity metals scale down (**16% DC** … **2% Val**) with **`HarvestVein(weight, swingChance, primary, fallbackToIron)`** pattern (`L93–103`).

**ML extras:**

- **`BonusHarvestResource`** gem drops at skill **≥90** (+message strings for each gem — see **`05`**).
- **`RaceBonus`** / **`RandomizeVeins`** toggled via **`Core.ML`**.

#### Player toggles affecting ore output

```192:203:World/Source/Scripts/Engines and Systems/Trades/Harvest/Mining.cs
		public override Type GetResourceType( Mobile from, Item tool, HarvestDefinition def, Map map, Point3D loc, HarvestResource resource )
		{
			if ( def == m_OreAndStone )
			{
				PlayerMobile pm = from as PlayerMobile;
				if ( pm != null && pm.StoneMining && pm.ToggleMiningStone && from.Skills[SkillName.Mining].Base >= 100.0 && 0.1 > Utility.RandomDouble() )
					return resource.Types[1];
				return resource.Types[0];
			}
			return base.GetResourceType( from, tool, def, map, loc, resource );
		}
```

- **StoneMining + toggle + GM skill** ⇒ **10%** chance coloured **Granite** instead of ore lump.

```257:269:World/Source/Scripts/Engines and Systems/Trades/Harvest/Mining.cs
		public override HarvestVein MutateVein( Mobile from, Item tool, HarvestDefinition def, HarvestBank bank, object toHarvest, HarvestVein vein )
		{
			if ( Pickaxe.IsGargoylePickaxe(tool) && def == m_OreAndStone )
			{
				int veinIndex = Array.IndexOf( def.Veins, vein );

				if ( veinIndex >= 0 && veinIndex < (def.Veins.Length - 1) )
					return def.Veins[veinIndex + 1];
			}
			else if ( (from.HarvestOrdinary || tool is TrainingShovel) && def == m_OreAndStone )
			{
				int veinIndex = Array.IndexOf( def.Veins, vein );
				return def.Veins[0];
			} 

			return base.MutateVein( from, tool, def, bank, toHarvest, vein );
		}
```

#### Sand (`m_Sand`)

Separate definition: consumes **twice** the resource multiplier versus ore; requires **Mining 100 + `SandMining`** toggle for eligible tiles (`m_SandTiles`), message ids `1044629–1044631`.

---

### `Lumberjacking` (+ `RichLumberjacking`)

**File:** `Lumberjacking.cs`; **singleton alias:** **`Lumberjacking.System` ⇒ `RichLumberjacking.System`**.

Baseline tree harvesting:

| Field | Value |
|---|---|
| Bank | **per tree tile (1×1)** |
| Totals | **4–6** logs before respawn |
| Respawn | **20–30** min |
| Skill | **`SkillName.Lumberjacking`** |
| Consumption | **`1 × MyServerSettings.Resources()`** (+ Isles variant) |

**Log tiers (`L78–106`):**

| Tier | Req / Min / Max skill | Produced `typeof` |
|---|---|---|
| Base | `0 / 0 / 85` | `Log` |
| Ash→Walnut | `55→100` escalating bands | Matching `AshLog` … `WalnutLog` |
| Veins | Iron-like weighting (**30%** ordinary … **2%** walnut) | Fallback to ordinary vein |

**Bonus rolls (`L108–113`):**

- **Bark fragments** (`BarkFragment`) — **Skill ≥90**, **10%** after guarding roll.
- **Mushrooms** (`HomePlants_Mushroom`) — **Skill 100**, **1%**.
- Idle chance mass **83.9%** nothing extra.

**`MutateVein`:** **`from.HarvestOrdinary`** ⇒ force **`def.Veins[0]`** (ordinary logs) — parallels mining training mode.

Rich “volatile trees” (**`RichLumberjacking`**) are documented under **`05`**.

---

### `Fishing`

**File:** `Fishing.cs` — **`SkillName.Seafaring`** (skill rename from classic Fishing).

| Field | Value |
|---|---|
| Bank | **8×8**, **5–15** fish-equivalent pulls |
| Range | **4** tiles (**`MaxRange`**), **`RangedTiles = true`** |
| Primary resource | **`typeof(Fish)`** with localized success **`1043297`** |
| Bonus | **≥80** skill oysters (`1072597`), **0.6%** base weight row |

Skill gain damping on land (**`FinishHarvesting`**) warns + disables gains when **`Seafaring ≥ 50`** and not **`Worlds.IsOnBoat`**.

**Type mutation table** replaces default fish according to **`MutateEntry`** predicates — enumerated in **`05`**.

Additionally **`IsNearHugeShipWreck`** influences regional tables (extended coordinate list in **`Fishing.cs`** beyond excerpt).

---

### `Librarian` (Inscribe bookshelf harvest)

**File:** `Librarian.cs` — targets **specific shelf static ids** (`m_LibraryTiles`).

| Field | Value |
|---|---|
| Skill | **`SkillName.Inscribe`** |
| Regions | Allowed only in dungeon-like region classes (`DungeonRegion`, `DeadRegion`, `CaveRegion`, `BardDungeonRegion`, `OutDoorBadRegion`) |
| Yield rows | **`BlankScroll` → `ExplosionScroll`** analog to mining tiers (skill **0 → 140.1** top band) |

**ML bonus relic drops:** lore books / DD relic bundles via **`BonusHarvestResource`**.

Rare shelf jackpot (`OnHarvestFinished`, `L218–259`): RNG vs **`Inscribe.Value`**, gated skill check → spawns deeds, treasure maps, spellbooks, **power scrolls**, aquatic vessel deeds, **`DDRelicTablet`**, etc. Most feedback is **hardcoded English** **`SendMessage`** — localization backlog (see **`07`**).

---

### `GraveRobbing`

**File:** `GraveRobbing.cs` — **`SkillName.Forensics`** on **`m_GraveTiles`**.

Bodies / parts / dust / fertile dirt / weak potions / brimstone (`Brimstone` reagent sack) / heal scroll veins all share zero **ReqSkill** in table but differentiated by weights in **`HarvestVein`** setup.

Completion hook applies **karma penalty** (**`-50`** threshold **`-2459`**) plus witness / criminal flag logic (**`Utility.RandomMinMax`** **~3%** branch**)** — see file `L206+`.

Bonus ML rows mirror mining/lib relic tables (`DDRelic*` category items).

---

### Tooling integrations

**`ResourceMods`:** When imbuing harvesting tools (`BaseHarvestTool`), uses **`CraftResources.GetUses/GetWeight/GetBonus`** to adjust **`UsesRemaining`**, **`SkillBonuses`**, Spyglass-ish bonuses — traced in **`ResourceMods.cs`** (long `ModifyItem` chain).

Cross-reference numeric columns in **`CraftResourceInfo`** (**`03`**–**`04`**) whenever balancing tool durability boosts per metal tier.

---

### Index of classes

| Class | Skill | Definitions | Companion doc |
|---|---|---|---|
| `RichVeinMining` | Mining | Rare node + parent ore defs | **`05`** |
| `Mining` base | Mining | Embedded in ctor of `RichVeinMining` ancestral chain | **`05`**, above |
| `RichLumberjacking` | Lumber | Volatile high-tier tree bank | **`05`** |
| `Lumberjacking` | Lumber | Ordinary forest loop | Above |
| `Fishing` | Seafaring | Sea fish + oyster + mutate | **`05`** |
| `Librarian` | Inscribe | Shelves | Above |
| `GraveRobbing` | Forensics | Graves | Above |

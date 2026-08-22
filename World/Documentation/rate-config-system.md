# RateConfig — Generic JSON-Driven Probability Engine

## Overview

`RateConfigEngine` is a small, domain-agnostic engine for feature probabilities/weights that need to
be **hand-tunable in a JSON file** and **hot-reloadable in-game**, without a recompile or restart. It
was built for the dragon Bright-breed rarity rework (see [`dragon-rarity.json`](#dragon-breed-rarity-datarateconfigdragon-rarityjson)
and [`gemdragon.json`](#gemdragon-scale-rarity-datarateconfiggemdragonjson) below), but the engine itself
knows nothing about dragons — any future feature can adopt it by picking a new dotted key namespace and
dropping a JSON file under `Data/RateConfig/`.

Cross-repo mechanics analysis: `<UO_DEV_DOCS_ROOT>/memento/game-mechanism/DRAGON_EGG_SYSTEM.md` §5.4/§5.6
and `<UO_DEV_DOCS_ROOT>/memento/game-mechanism/DRAGON_RIDING_SCROLL_SYSTEM.md` describe the player-facing
mechanic this engine now gates.

## Architecture

```
World/Source/System/RateConfig/
├── RateConfigEngine.cs   # load/reload, GetDouble(key, default), GetTable(prefix), GetAll()
└── WeightedPick.cs       # WeightedPick.Pick(weights) and WeightedPick.KeepChance(chance)

World/Data/RateConfig/
├── dragon-rarity.json           # dragon.breedWeight.<name> — RidingDragon/Dragons/Wyrms Bright breeds
├── gemdragon.json               # gemdragon.scaleWeight.<name> — GemDragon scale colors
├── inscription-recipe-drop.json # inscription.drop.* / inscription.enemy.* / inscription.avatar.*
├── avatar-fortune.json          # avatar.fortune.* — AscentHuntBonus multipliers
├── dragon-riding-scroll.json    # dragon.ridingScroll.luckCap / maxChancePct
└── relics-drop.json             # relics.drop.luckCap / first|repeat.rankNMaxPct
```

- **`RateConfigEngine`** lives in `System.csproj` (the `World` executable assembly) alongside
  `Server.Localization.StringCatalog`, and reuses the same minimal JSON reader,
  `Server.Localization.SimpleJsonObject.ParseStringProperties` (internal, same assembly — see
  [JSON format constraint](#json-format-constraint) below).
- At startup it merges every `*.json` under `Data/RateConfig/` (recursive) into one flat
  `"dotted.key" -> double` map.
- **Startup invocation — important, non-obvious wiring:** `RateConfigEngine` is compiled directly into
  `World.exe` (via `System.csproj`), *not* into the runtime-compiled "Scripts" assembly
  (`Source/Scripts/**`, produced by `ScriptCompiler.CompileCSScripts`/`GetScripts`, which only scans
  `Info/Scripts` and `Source/Scripts`). `Main.cs`'s `ScriptCompiler.Invoke("Initialize")` reflects
  **only** over that Scripts assembly, so `[CallPriority(-150)]` on `RateConfigEngine.Initialize()` is
  never picked up by it — that attribute is decorative, kept only for stylistic parity with
  `Server.Localization.LocalizationBootstrap` (which has the exact same characteristic: also under
  `System/`, also `[CallPriority]`-decorated, but actually invoked by an explicit direct call in
  `Main.cs`, not the reflection scan). `Main.cs` therefore calls `RateConfigEngine.Load()` **directly by
  name**, right after `LocalizationBootstrap.Initialize()` and before `ScriptCompiler.Invoke("Initialize")`
  — i.e. before any spawner/timer can run. As a second line of defense, `GetDouble`/`GetTable`/`GetAll`
  also lazy-load on first access if nothing has loaded yet, so a future refactor that accidentally drops
  the `Main.cs` call degrades to "loads slightly later" rather than "silently always returns defaults".
- **Consumers** (in `Scripts.csproj`, which references `System.csproj` — the compiler reference list in
  `ScriptCompiler.GetReferenceAssemblies()` includes `Core.ExePath`, so Scripts code can reference any
  public type in the exe assembly) call `GetDouble` / `GetTable` directly; the engine has no
  dragon-specific (or any other feature-specific) code.

### JSON format constraint

The codebase has no `Newtonsoft.Json` / `System.Text.Json` reference — the only JSON reader is
`SimpleJsonObject.ParseStringProperties`, which reads flat `{"key":"value"}` objects (quoted string
keys and values only; no numbers, booleans, nested objects, or arrays). Numeric rate/weight values are
therefore stored as **quoted numeric strings** (e.g. `"0.1"`) and parsed with
`double.TryParse(..., NumberStyles.Float, CultureInfo.InvariantCulture, ...)`. Malformed entries are
skipped with a `Console.WriteLine` warning at load time — they never throw, since this runs at server
startup and on live GM reload.

## API

```csharp
using Server.RateConfig;

// Read a single value, with a caller-supplied default when the key is absent:
double keep = RateConfigEngine.GetDouble( "dragon.breedWeight.xormite", 1.0 );

// Read every entry under a dotted prefix, keyed by the remainder (generic — this is what makes
// the engine reusable for any future "list of named weights" feature):
Dictionary<string, double> scaleWeights = RateConfigEngine.GetTable( "gemdragon.scaleWeight" );
// -> {"red": 10, "yellow": 10, ..., "platinum": 2}

// Weighted-random pick over a name -> weight map (subtractive method, same style as
// LootPack / SpawnGroup's inline pickers, centralized here for reuse):
string picked = WeightedPick.Pick( scaleWeights );

// Reject/keep gate for a single probability (0..1):
bool keepThisRareBreed = WeightedPick.KeepChance( keep );
```

- `GetDouble` / `GetTable` never throw and never require the engine to have successfully loaded —
  they fail safe to the caller-supplied default / an empty table.
- `RateConfigEngine.Reload()` (== `Load()`) re-reads every file under `Data/RateConfig/` from disk.

## GM commands (`RateConfigCommands.cs`)

| Command | Access | Purpose |
|---|---|---|
| `[ratereload` | Administrator | Re-read all `Data/RateConfig/*.json` from disk (no restart). |
| `[ratelist <prefix>` | GameMaster | List every configured key under a dotted prefix, e.g. `[ratelist dragon.breedWeight` lists all 26 Bright breed keep-chances plus `default`. |
| `[rateget <key>` | GameMaster | Print the effective value of one key (debug aid). |

## Consumers

### Dragon breed rarity (`Data/RateConfig/dragon-rarity.json`)

`Server.Mobiles.DragonBreedRarity.AdjustWildBreed(candidateId, terrain)` gates the 26 **Bright**
(glowing) breed IDs shared identically by `RidingDragon`, `Dragons`, and `Wyrms`'s `CreateDragon(terrain)`
switch table. Keys: `dragon.breedWeight.<name>` (e.g. `dragon.breedWeight.xormite`) is the **keep
chance** (0..1) for that Bright breed once rolled; `dragon.breedWeight.default` applies to every other
(non-Bright, common) breed id and is effectively unused today since non-Bright ids return immediately
without a lookup — it exists so a future policy change (e.g. gating common breeds too) doesn't need a
new key scheme.

- **Wild-only:** the gate only runs when `Hue <= 0` at the point `CreateDragon` executes (i.e. the
  creature was not spawned by `DragonEgg` hatching, which locks the breed via `Hue` before calling
  `CreateDragon` — see the `if (Hue > 0) dragon = Hue;` branch in each caller, which bypasses the gate
  entirely). This preserves the promise that a hatched egg always yields the mother dragon's exact
  breed.
- **Reject → reroll:** a rejected Bright id is rerolled via `DragonBreedRarity.PickRawBreed(terrain)`
  (a faithful extraction of the terrain → id-pool selection already inlined in each `CreateDragon`,
  kept here as the single source of truth for the reroll) from the **same terrain pool**, up to 8
  attempts.
- **Radiation edge case:** the `radiation` terrain pool (`{5, 6, 7, 54, 97, 104, 106, 146}`) is **100%
  Bright ids** — every reroll from that pool would still fail the gate. After 2 failed rerolls at
  `radiation`, `AdjustWildBreed` escapes into the `dungeon` pool (which is mostly common breeds) so the
  gate can actually converge on a non-Bright breed instead of exhausting the attempt cap. If it still
  can't land on an accepted breed after 8 total attempts, it gives up and accepts whatever it last
  rolled (never loops forever).
- **GemDragon is not part of this table** — see below.

### GemDragon scale rarity (`Data/RateConfig/gemdragon.json`)

Unlike `RidingDragon`/`Dragons`/`Wyrms` (where `CraftResource`/scale type is skinning-only loot and has
no visible effect on a living mount), `GemDragon.Resource` **directly sets its `Hue`**
(`Hue = CraftResources.GetClr(Resource)`), so its scale color is a visible rarity axis in its own right.
`GemDragon.OnAfterSpawn` reads `gemdragon.scaleWeight.*` via `RateConfigEngine.GetTable` and uses
`WeightedPick.Pick` to choose a scale name, then maps it back to a `CraftResource` via a small static
`name -> CraftResource` dictionary local to `GemDragons.cs`. If the config table is empty/malformed, it
falls back to `CraftResource.MetallicScales` (matching the old hardcoded default).

### Relics chest drop (`Data/RateConfig/relics-drop.json`)

`RelicChestDropHelper.GetRollMaxPct` / `GetRollActualPct` read:

| Key | Default | Meaning |
|-----|---------|---------|
| `relics.drop.luckCap` | 2000 | Luck at which rank max % is reached |
| `relics.drop.first.rank1MaxPct` | 20 | First-kill Rank 1 max % |
| `relics.drop.first.rank2MaxPct` | 10 | First-kill Rank 2 max % |
| `relics.drop.first.rank3MaxPct` | 5 | First-kill Rank 3 max % |
| `relics.drop.repeat.rank1MaxPct` | 5 | Repeat Rank 1 max % |
| `relics.drop.repeat.rank2MaxPct` | 4 | Repeat Rank 2 max % |
| `relics.drop.repeat.rank3MaxPct` | 3 | Repeat Rank 3 max % |

Formula: `P% = min(Luck, luckCap) / luckCap × rankMaxPct × fortuneMult`.
Hot-reload via `[ratereload]`. Debug: `[rateget relics.drop.first.rank1MaxPct]`, `[ratelist relics.drop]`.
Cross-ref: `<UO_DEV_DOCS_ROOT>/memento/game-mechanism/RELICS_DROP_REFORM_TOP3_DAMAGE_SYSTEM.md`.

### Dragon Riding Scroll drop (`Data/RateConfig/dragon-riding-scroll.json`)

`GetPlayerInfo.DragonRidingScrollLuckyDrop` (Dragon King only) reads:

| Key | Default | Meaning |
|-----|---------|---------|
| `dragon.ridingScroll.luckCap` | 2000 | Luck at which the base chance reaches `maxChancePct` |
| `dragon.ridingScroll.maxChancePct` | 5 | Base drop % at `luckCap` (before `AscentHuntBonus`) |

Formula: `P% = min(Luck, luckCap) / luckCap × maxChancePct × fortuneMult`.
Hot-reload via `[ratereload]`. Debug: `[rateget dragon.ridingScroll.maxChancePct]`, `[ratelist dragon.ridingScroll]`.
Cross-ref: `<UO_DEV_DOCS_ROOT>/memento/game-mechanism/DRAGON_RIDING_SCROLL_SYSTEM.md`.

### Inscription advanced recipe drops (`Data/RateConfig/inscription-recipe-drop.json` + `Data/InscriptionRecipeDrop/tier-scrolls.json`)

`InscriptionRecipeDropConfig` (see `World/Source/System/RateConfig/InscriptionRecipeDropConfig.cs`) loads **numeric**
drop tuning from `inscription-recipe-drop.json` (merged into `RateConfigEngine` like any other RateConfig file)
and **scroll class name lists** from `tier-scrolls.json` (string CSV values — not parsed by `RateConfigEngine`).
Consumed by `InscriptionRecipeDropHelper.TryDropRecipe` on `BaseCreature.OnDeath` when
`MySettings.S_UseLegacyInscription` is false. Cross-ref:
`<UO_DEV_DOCS_ROOT>/memento/game-mechanism/INSCRIPTION_ADVANCED_RECIPE_DROP_SYSTEM.md`.

- Hot reload: `[ratereload` reloads both `RateConfigEngine` and `InscriptionRecipeDropConfig`.
- Debug: `[rateget inscription.enemy.boss.rank1MaxPct`, `[ratelist inscription.enemy`.
- **Tier scroll type lists** (`inscription.tier.T1.types` … `T4.types` in `tier-scrolls.json`) are **not**
  exposed through `[rateget]` / `[ratelist]` — those commands only read numeric keys merged into
  `RateConfigEngine`. To inspect which scroll class names loaded, read `Data/InscriptionRecipeDrop/tier-scrolls.json`
  on disk or check the startup / `[ratereload]` console line (`T1=… T2=…` counts only, not names). Unknown
  type names are logged at load time and skipped.

## Adding a new consumer

1. Pick a dotted key namespace that won't collide with existing ones (e.g. `myfeature.someWeight.*`).
2. Add a new `*.json` file under `World/Data/RateConfig/` with quoted numeric string values.
3. Call `RateConfigEngine.GetDouble` / `GetTable` from your feature code; use `WeightedPick` if you need
   a weighted random pick or a reject/keep gate.
4. No engine changes needed. `[ratelist your.prefix` and `[rateget your.key` work immediately, and
   `[ratereload` picks up your new file automatically (recursive directory scan).
5. Document the new consumer in this file's [Consumers](#consumers) section and, if it's a Memento
   game-mechanic (not pure engineering infra), cross-reference it from the relevant
   `<UO_DEV_DOCS_ROOT>/memento/game-mechanism/*.md` analysis doc.

## Save compatibility

N/A — the engine and its JSON config have no serialization footprint. The `CreateDragon` /
`GemDragon.OnAfterSpawn` edits only affect newly rolled/spawned creatures going forward; existing saved
mobiles (including already-hatched dragons) are untouched, since their breed/`Resource`/`Hue` were
already assigned and are not re-rolled on load.

## Resource systems — documentation pack

Purpose: authoritative **design / engineering / localization / eventual player-site** reference for **gatherable inputs** and **`CraftResource` equipment materials** on **Ultima: Memento** (this repo). Source of truth is always **C#**; these Markdown files summarise structure, link paths, known quirks, and maintenance rules.

Audience:

- Designers balancing spawn, yield, tiers, rarity.
- Engineers extending `HarvestSystem`, new `CraftResource` bands, imbuing/modifiers.
- Localizers aligning Cliloc-backed names (`CraftResourceInfo` message ids) with catalog policy.
- Web authors distilling player-facing summaries (omit internal enum names unless in a nerd FAQ).

Research methodology (used to build this pack):

| Phase | Focus | Deliverable doc |
|---|---|---|
| A | Persisted resource identity (`Item.Resource`, `CraftResource` bands) | `01` |
| B | Gathering loops (`HarvestSystem` subclasses + rich variants) | `02`, `05` |
| C | Authoritative numeric tables (`m_*Info` arrays in `ResourceInfo.cs`) | `03`, `04` |
| D | Attribute application (`CraftAttributeInfo.*` fields, `CraftAttInfo` mapping) | `06` |
| E | Messaging / Cliloc / player-facing distill rules | `07` |

Companion files (read in order):

1. [`01-domain-model-code-index.md`](01-domain-model-code-index.md) — `CraftResource` enum bands, `CraftResourceType`, `CraftResources`, `CraftAttributeInfo`, `ResourceMods`.
2. [`02-harvest-systems.md`](02-harvest-systems.md) — `Mining`, `Lumberjacking`, `Fishing`, `Librarian`, `GraveRobbing`, timers, yields, specials.
3. [`03-reference-tables-metal-scales-special.md`](03-reference-tables-metal-scales-special.md) — full `CraftResourceInfo` mapping for metals, scales, spectral/special tiers.
4. [`04-reference-tables-leather-wood-fabric-block-skin-skeletal.md`](04-reference-tables-leather-wood-fabric-block-skin-skeletal.md) — remainder of crafting material families + item `Type`s.
5. [`05-rich-veins-fishing-and-gems.md`](05-rich-veins-fishing-and-gems.md) — Rich ore nodes, volatile rich trees, fishing mutation table, mined gem bonuses & jewelry `GetGemMods`.
6. [`06-craft-attribute-math.md`](06-craft-attribute-math.md) — interpreting `CraftAttInfo(...)`, armour vs elemental weapon damage splits, durability / luck / lower req columns.
7. [`07-localization-and-player-copy.md`](07-localization-and-player-copy.md) — Cliloc triplets (`CraftText` / `MaterialText` / `LowCaseText`), hardcoded harvest strings that may need localization, web summary guidance, **`resource-harvest-extra.json`** + localization regression golden cases.

Maintenance contract:

1. Any new `CraftResource` enum member **must** have a matching `CraftResourceInfo` row, `CraftAttributeInfo` row (unless intentionally blank), `RegisterType`, and authoring plan for localization (Cliloc or string catalog policy).
2. Any new harvest `typeof(...)` outcome must appear in **`02`** and, if it maps to equipment material, **`03–04`**.
3. Prefer updating this folder in the **same PR** as gameplay changes touching `HarvestDefinition`/`CraftResources`. When **`StringCatalog` / zh-Hans** copy for harvesting or **`CraftResources` shorts** shifts in a noticeable way, add or refresh **`string_catalog_only`** cases under **`World/Data/Localization/regression/cases/`** and run **`bash World/Source/Tools/run_localization_regression.sh`** (see `AGENTS.md` §4.4).
4. If this pack drifts from code, **`ResourceInfo.cs` + `Harvest/*.cs`** win until the Markdown is patched.

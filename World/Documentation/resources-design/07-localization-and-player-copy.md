## Localization, Cliloc-backed names, player-site copy guidance

### Cliloc triplets on `CraftResourceInfo`

Three integers per row:

```323:326:World/Source/Scripts/System/Misc/ResourceInfo.cs
		public int CraftText{ get{ return m_CraftText; } }
		public int MaterialText{ get{ return m_MaterialText; } }
		public int LowCaseText{ get{ return m_LowCaseText; } }
```

Author intent (inline ctor comments):

- **`CraftText`** — categorical uppercase label (historically “RESOURCE (count)”).
- **`MaterialText`** — human material phrase (“Valorite Ingots”).
- **`LowCaseText`** — sentence-case variant (“valorite…”).

UO client **Cliloc**.map** resolves these IDs at runtime — they are **not** automatically synced with `zh-Hans` JSON. For Mandarin UI parity:

| Task owner | Responsibility |
|---|---|
| Server engineers | Prefer consolidated modern pattern (`StringCatalog` / logical keys per **`AGENTS.md` §3**) when touching new smith menus or harvest books |
| Localizers | If leaving legacy Cliloc, document ID ↔ Chinese translation in glossary or operational spreadsheet until migrated |
| Web team | Rewrite for players using **literary Mandarin** (**`ultima-memento-web`** LLM workflow) referencing glossary — **never** expose raw numeric Cliloc IDs in prose |

Harvest feedback uses **mixed** modalities:

| Source | Mechanism |
|---|---|
| **`HarvestDefinition.*Message` fields | Numeric localized ids (Mining fail / pack full / etc.) |
| **`HarvestResource`** `message | `SendLocalizedMessage` or `SendMessage` |
| Lumber bonus | plaintext `"You harvest some Bark Fragments."` (**`Lumberjacking.cs`**) |
| ML gem bonuses | English strings baked into **`BonusHarvestResource`** mining entries |
| Fishing land warning | **`SendMessage("You would get better at seafaring if you fished from a boat.")`** |
| Librarian gates | Numerous **`SendMessage("…")`** region failures + shelf jackpot narration |
| Grave Robbing gathers | Embedded English success strings passed as `HarvestResource` message |

**Localization debt marker:** Anything using **`Mobile.SendMessage("literal english")`** should be enumerated when planning a harvesting copy pass (extract literals per **`build_localization_strings.py`** conventions after refactor). Refactored gathers use **`StringCatalog.Resolve`** / **`ResolveFormat`** (**`HarvestSystem`** ore/granite/log templates, librarian book/scroll/blank-scroll lines, grave chest); **`TextDefinition.SendMessageTo`** (bonus gems, bark, mushrooms) resolves string **`BonusHarvestResource`** messages through **`StringCatalog`** for **`zh-Hans`**.

### Hand-maintained `resource-harvest-extra.json`

`StringCatalog` merges every `*.json` under **`Data/Localization/en/`** and **`zh-Hans/`**; duplicate hash keys **keep the first merged value** (lexicographic file path order). **`resource-harvest-extra.json`** sorts before **`scripts-*.json`**, so overrides for harvest polish land here deliberately.

Paired **`en/`** + **`zh-Hans/`** files (**`keep_extra`** in **`build_localization_strings.py`**) carry craft-resource shorts, ML gem / bark / mushroom lines, **`some`**, **`You found {0}!`**, grave-chest wording as needed — see **`World/Data/Localization/README.txt`**.

**Regression workflow:** After a substantive harvest/resource **`StringCatalog`** zh pass, extend **`World/Data/Localization/regression/cases/`** with typical **`pipeline: "string_catalog_only"`** inputs (bonus phrase, `{0}`/`{1}` template, quantity word **`some`**, material short **`Iron`**), then run **`bash World/Source/Tools/run_localization_regression.sh`** (**`World/Documentation/localization-regression-testing.md`**).

---

### External player-facing synopsis guidance

Suggested site article shape ( bilingual ):

1. Explain **skills** (**Mining**, **Lumber**, **Seafaring**, **Inscribe shelves**, **Forensics graves**) without enum names — “deep mountain ore tiers”, “rare shimmering ore spires”, “volatile ancient growth”.
2. Link **economy tiers** narratively (**Agapite** vs **Dwarven**) using glossary-canonical zh where available.
3. Avoid promising exact percentage tables unless ops commits to **`MyServerSettings`** snapshot — note **“default Memento values”** policy from web **`AGENTS.md`** when describing defaults.
4. Rich systems (**Elven logs**, **Dwarven ore nodes**, **fishing treasures**) = marketing-friendly mystery; detailed tables stay in this internal pack.

---

### Cross-repo glossary alignment

When a material **Name** in `CraftResourceInfo` introduces a net-new English headword for Chinese documentation, follow **`glossary-approved-zh.json`** curation (reviewed entry, `sync_localization_glossary.py --check`).

Sci‑fi material names (Star Wars–adjacent woods / metals) may need **transliteration policy** (bracket English per server translation guide) before site publication.

---

### Verification checklist (copy change)

- [ ] Enumerate affected Cliloc IDs + confirm client string meaning in both EN clients used by playerbase.
- [ ] If replacing with `StringCatalog`, run extractor (`--no-translate`) + queue LLM translation per **`AGENTS.md`**.
- [ ] Player MDX: no raw `S_*` settings symbols; describe outcomes.
- [ ] Add or refresh representative **`string_catalog_only`** regressions (`World/Data/Localization/regression/cases/`) and run **`run_localization_regression.sh`** when harvest/resource **`StringCatalog`** zh changes materially.
- [ ] Update this doc folder if new harvest string pattern introduced (maintenance contract in **`README.md`**).

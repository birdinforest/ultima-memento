# Plan: migrate all server game content text to JSON (multi-language)

This document extends [`localization-plan-multiple-languages.md`](./localization-plan-multiple-languages.md). It assumes the **runtime façade** (`ILocalizationProvider`, per-account language, `en.json` + `zh-Hans.json` in repo, config default) is implemented or in progress. Here we plan how to **move essentially all player-visible server text** into JSON and keep the codebase maintainable.

---

## 1. Definition of “all game content text” (this shard)

### In scope (server-side, player-visible)

| Category | Typical APIs / locations | Notes |
|----------|---------------------------|--------|
| **Plain system strings** | `SendMessage("...")`, `Say("...")`, `PublicOverheadMessage`, etc. | Direct migration to keys + `Localization.Get(m, key, args)`. |
| **Gumps** | `AddHtml`, `AddLabel`, button text, dialog bodies | Often HTML-wrapped; keep **layout in code**, move **copy** to JSON (or small HTML fragments if unavoidable). |
| **Books, notes, scrolls** | `BookContent`, dynamic pages, quest blurbs | Long text: prefer **dedicated keys** or **split JSON files** per item/quest to avoid megabyte monoliths. |
| **Commands & help** | Player commands, `HelpGump`, MOTD-style content | High traffic; migrate early. |
| **NPC speech** | Citizen/dialogue strings in scripts | May be huge; phase by region or system. |

### Explicitly out of scope (per locked product decisions)

- **Client Cliloc** and **client-rendered tooltips** that only exist as numeric Cliloc IDs on the client. Those strings are not authored in this repo as free text.

### Gray area: `SendLocalizedMessage(cliloc)`

There are on the order of **thousands** of `SendLocalizedMessage(` call sites under `World/Source` (rough magnitude **~3k** match count across the tree). Numeric Clilocs are **not** JSON-friendly for translators unless you supply text.

**Policy (choose one and apply consistently):**

- **Option A — Hybrid (recommended for “all content” without client mod):**  
  - Keep `SendLocalizedMessage` where you are satisfied with the **client’s** localized Cliloc.  
  - For accounts using **`zh-Hans`**, replace selected call sites with **`SendMessage(Localization.Get(...))`** (or a helper `SendLocalizedText(m, key)`) **only where** Chinese must come from the shard.  
  - Document a **growing denylist** of Cliloc IDs that must be overridden for `zh-Hans`.

- **Option B — Full Cliloc overlay in JSON:**  
  - Introduce `cliloc.501023` style keys in `en.json` / `zh-Hans.json` whose values are the **full English / Chinese** sentences.  
  - Replace `SendLocalizedMessage(501023)` with a helper that **either** sends the JSON string for `zh-Hans` **or** falls back to `SendLocalizedMessage` for `en` (to preserve classic English Cliloc behavior).  
  - Requires **inventorying every Cliloc** used (grep + generated map). High effort; best automated.

The rest of this plan assumes **Option B** for “translate everything server-controlled” and **Option A** for Clilocs you never override (true client-only strings), unless the project commits to full Cliloc data files (see §6).

---

## 2. Inventory and metrics (rough)

Automated counts from the repository are **indicative**, not exact strings:

| Pattern | Approx. occurrences (matches) |
|---------|----------------------------------|
| `SendMessage("` … | ~3 500 |
| `SendLocalizedMessage(` | ~2 900 |
| `Say("` … | ~600 |
| `AddHtml(` with verbatim `@"` (Scripts only sample) | Hundreds of files |

**Implication:** this is a **multi-sprint** program. Treat it like a **data migration** with CI gates, not a single patch.

---

## 3. JSON layout (scalable)

Avoid a single `en.json` with 50k keys.

```text
World/Data/Localization/
  _index.json              # optional: list of fragment files to load
  core/common.en.json
  core/common.zh-Hans.json
  quests/mlquest.en.json
  items/boats.en.json
  ...
```

**Loader behavior:** merge dictionaries at startup (and on reload) in a defined order; later files **must not** unintentionally overwrite keys (detect duplicates in CI).

**Key naming convention (stable IDs):**

```text
{domain}.{subsystem}.{context}.{name}
```

Examples: `items.boats.drydock.must_be_near_dock`, `gumps.help.section.pvp`, `cliloc.501023`.

**Rules:**

- Keys are **English identifiers**, not English prose — prose lives only in JSON values.
- Prefer **full-sentence** values with `{0}` placeholders instead of concatenating fragments across languages.
- **Do not** embed translator-facing strings in C# after migration of a file; new strings go straight into JSON.

---

## 4. API patterns (minimize churn)

Introduce thin helpers so call sites stay readable and merges stay small:

```csharp
// Illustrative — actual signatures should match your Mobile/Account model.
void LMessage(Mobile m, string key, params object[] args);
void LSay(Mobile m, string key, params object[] args);
string LHtml(Mobile m, string key, params object[] args); // returns escaped fragment if needed
```

For **Cliloc overlay (Option B)**:

```csharp
void SendClilocOrText(Mobile m, int cliloc, string keyFallback, params object[] args);
```

Behavior: if account language is `zh-Hans` and `keyFallback` exists, send plain/Unicode message from JSON; else use `SendLocalizedMessage(cliloc)` (or inverse policy if English should always use JSON too — pick one).

---

## 5. Extraction pipeline (automation)

### Phase 0 — Tooling (no player-facing change yet)

1. **Scanner** (Roslyn or scripted grep with manual review):  
   - Find `SendMessage("...")`, `Say("...")`, selected `AddHtml` literals.  
   - Emit `pending-keys.csv` with: suggested key, file, line, original English.

2. **Cliloc harvester:**  
   - Grep all numeric literals passed to `SendLocalizedMessage` / `SendLocalizedMessageAffix` / similar.  
   - Emit `clilocs-used.txt` sorted unique.

3. **Key assignment:**  
   - Script assigns deterministic keys from path + hash short id if collision, **or** human review per subsystem.

4. **JSON writer:**  
   - Writes/updates `*.en.json` fragments.  
   - `zh-Hans` fragments copy English as placeholder **or** machine-translate with mandatory human QA for quest/lore.

### Phase 1 — CI gates

- **Duplicate key detector** across merged JSON.
- **Orphan key detector** (keys in JSON never referenced — allowlist for WIP).
- **Missing zh-Hans** report (warn or error per stage).

---

## 6. Cliloc text sourcing (if Option B)

English strings for `cliloc.*` keys must come from somewhere:

1. **Official Cliloc.enu** (UO client data) — legal/licensing and redistribution constraints are **your** responsibility; do not commit proprietary client files if policy forbids it.  
2. **Manual transcription** from in-game English for critical subsets.  
3. **Hybrid:** English stays on client Cliloc; only `zh-Hans.json` stores overrides and code uses Option A for those IDs.

Document the chosen approach in `World/Data/Localization/README.txt` so translators know the source of truth.

---

## 7. Migration waves (recommended order)

| Wave | Target | Rationale |
|------|--------|-----------|
| **W1** | Login, MOTD, shard greeter, account/language commands, core help gumps | Every player hits these; validates plumbing. |
| **W2** | Global commands (`Scripts/System/Commands/Player`), death/stuck/resurrect gumps | High visibility, limited lore risk. |
| **W3** | Trades: crafting gumps, BOD/bulk, Global Shoppe | Many repeated patterns; good for tooling refinement. |
| **W4** | Quest systems (MLQuest, major quest lines), achievements | Sensitive narrative; slower QA. |
| **W5** | Items & mobiles (bulk of `SendMessage` / `Say`) | Split by folder (`Items/`, `Mobiles/`) to parallelize. |
| **W6** | `SendLocalizedMessage` overlay | After patterns stable; automate as much as possible. |

Within each wave: **(1)** add keys to JSON → **(2)** replace literals in C# → **(3)** run shard smoke tests → **(4)** merge.

---

## 8. Lore, placeholders, and formatting

- **Names** (players, towns) should usually remain **dynamic `{0}`** arguments, not duplicated per language in JSON.
- **Item type names** often come from Cliloc or `Name` properties — decide per string whether the sentence is built in code or fully localized.
- **Gender / plural:** avoid split English fragments; use separate keys if Chinese needs different structure (`quest.accept_male`, `quest.accept_female`) only when unavoidable.

---

## 9. Testing and sign-off

Per wave:

- Switch account `en` ↔ `zh-Hans`; exercise all changed flows.
- Check **Unicode** rendering in journal and gumps for long Chinese strings.
- Regression: **fallback** when `zh-Hans` missing a key (must not crash).
- Spot-check **gump width**: Chinese can be narrower or wider; adjust gump dimensions if text clips.

---

## 10. Reducing merge pain with upstream

- Keep **Memento-specific** JSON under clearly named paths (`Data/Localization/memento/` vs `Data/Localization/core/` if you fork-merge core).
- Prefer **small mechanical commits** per folder (“migrate Items/Boats strings”) over mixing with logic changes.
- Avoid reformatting untouched upstream blocks when touching a file.

---

## 11. Definition of done (program-level)

The program is “complete” when:

1. **No remaining player-facing string literals** in agreed directories (enforce with scanner in CI), **except** documented exceptions (debug-only, admin-only, legal logs).
2. **`en` + `zh-Hans`** JSON cover all keys; CI fails on missing `zh-Hans` for non-WIP keys.
3. **Cliloc policy** documented (A vs B) and implemented consistently.
4. **Operator/translator README** exists under `Data/Localization/` describing key rules and file layout.

---

## 12. Open decision (single)

Confirm **Cliloc strategy** for “all content”: **Option A (hybrid override)** vs **Option B (full numeric map in JSON with helper)** vs **English always from JSON too** (maximum consistency, largest `en.json`).

Once that is chosen, the first automation task is to generate **`clilocs-used.txt`** and **`SendMessage` inventory** from `World/Source` and size the waves accordingly.

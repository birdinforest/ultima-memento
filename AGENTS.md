# Ultima Memento — AI Agent Guide

> **Scope:** Game server repo (`ultima-memento`). For the website see `ultima-memento-web/AGENTS.md`. **Cross-repo practice (site media + glossary-driven wiki index):** [§7](#7-website--player-facing-docs-ultima-memento-web).
> **Update protocol:** When you change a convention or discover something that conflicts with this file, propose an edit at the end of your turn. Do not silently diverge.

---

## 0. Quick-Start Index

| Task | Jump to |
|---|---|
| Add a new game feature (C#) | [§2 Engineering Practices](#2-engineering-practices) |
| Rename/remove persistable C# types (`Item` / `Mobile` / …) | [§2.4 Persistence & saves](#24-persistence--saves) |
| Add translatable strings to C# code | [§3.2 Adding Strings](#32-adding-strings-to-cs) |
| Run the localization extractor | [§3.3 Extraction Tool](#33-extraction-tool) |
| Incremental LLM locale queue (`stats` / `queue` / `apply`) | [§3.4 Translation Workflow](#34-translation-workflow--llm-only) |
| Logical-key JSON (shard greeter, war shouts, …) | [§3.1 Architecture](#31-localization-architecture) |
| Translate new strings (LLM, not Google) | [§3.4 Translation Workflow](#34-translation-workflow--llm-only) |
| Update or add glossary terms | [§3.5 Glossary](#35-glossary-management) |
| Website: images/GIFs, wiki index from glossary | [§7 Website (`ultima-memento-web`)](#7-website--player-facing-docs-ultima-memento-web) |
| Build and test the server | [§4 Build & Test](#4-build--test) |
| Understand what an AI agent may/must not do | [§5 Boundaries & Verification](#5-agent-boundaries--verification) |

---

## 1. Project Structure

```
ultima-memento/
├── World/
│   ├── Source/
│   │   ├── System/          # Core engine (C#): Localization/, Gumps/, etc.
│   │   ├── Scripts/
│   │   │   ├── Engines and Systems/   # Quest, Trade, Dungeons, etc.
│   │   │   ├── Items/                 # Items, Books
│   │   │   ├── Mobiles/               # Creatures, NPCs
│   │   │   ├── System/                # Commands, Skills, Misc
│   │   │   └── Utilities/
│   │   └── Tools/           # Python pipelines (localization, build helpers)
│   ├── Data/
│   │   ├── Localization/
│   │   │   ├── en/          # Split EN JSON (source of truth)
│   │   │   ├── zh-Hans/     # Split ZH JSON (generated + curated)
│   │   │   ├── glossary-approved-zh.json   # Canonical glossary (hand-curated)
│   │   │   └── zh-Hans-glossary-sync-rules.json
│   │   └── System/CFG/      # localization.cfg, other runtime config
│   ├── Documentation/       # Workflow guides (authoritative)
│   └── Saves/               # Runtime world state — never edit manually
└── WorldLinux.exe           # Runtime binary (Linux/macOS)
```

**Key documentation to read before working on each domain:**

- Localization: `World/Data/Localization/README.txt` — authoritative layout and commands.
- Book translation: `World/Documentation/scripts-books-zh-translation-workflow.md`
- Glossary sync: `World/Documentation/zh-localization-glossary-sync-workflow.md`
- Translation editorial rules: `World/Documentation/zh-localization-translation-guide.md`
- Coverage roadmap: `World/Documentation/localization-complete-coverage-roadmap.md`

---

## 2. Engineering Practices

### 2.1 C# Style

- **RunUO / ServUO conventions apply.** When in doubt, match the surrounding file's style.
- **No unsolicited refactoring.** Only modify code outside the stated task scope if a linter error or critical bug requires it. Note any such changes in your response.
- **Avoid magic numbers and bare strings.** Use constants, enums, and `StringCatalog`-managed strings.
- **Gumps:** All user-visible strings in gumps must pass through the localization catalog (see §3). Do not hardcode Chinese or English inline.
- **Access modifiers:** Default to the narrowest access level that works (`private` → `protected` → `public`).
- **Error handling:** Follow existing patterns. Do not silently swallow exceptions.

### 2.2 Adding New Features

1. **Read at least one analogous existing implementation** before starting. Pattern-match to the existing architecture.
2. **Plan before writing:** For features touching more than two files, state your approach in plain language first.
3. **Localization from day one:** Any user-visible text added to C# must go through `StringCatalog` (see §3.2). Never defer localization.
4. **Quests:** Follow `World/Source/Scripts/Engines and Systems/Quests/Core/` patterns. New quest types subclass existing base classes.
5. **Items/Books:** Follow `World/Source/Scripts/Items/Books/` patterns for book text extraction compatibility.

### 2.3 File Naming

- C# files: `PascalCase.cs`, matching the primary public class name.
- Python tools: `snake_case.py`.
- Localization JSON: `<category>.json` (see §3.1 for category names).

### 2.4 Persistence & saves

Persisted world state under `World/Saves/` (see §5.1 — agents do not edit it by hand) contains **serialized type names** and instance data for items, mobiles, and other `ISerializable` / `ISerialized` objects.

**During development:**

- **Removing, renaming, or moving** a C# type that can already exist in live saves may cause **deserialize failures**, startup exceptions, or **orphaned / vanishing** objects on existing shards.
- Treat this as a **compatibility** change: call it out in the PR/response; do not assume only fresh installs.
- **Mitigations** (choose what matches operator expectations): keep a **stub subclass** or alias that reads old data and deletes/replaces the item; add **`Deserialize` version branches** that migrate fields; document a one-time **save wipe** or tooling for operators.

---

## 3. Localization System

The server supports **English (`en`)** and **Simplified Chinese (`zh-Hans`)**. Every player-facing string must be in the catalog. The default language is `en`; `zh-Hans` is the secondary locale.

### 3.1 Localization Architecture

```
Data/Localization/
  en/
    system.json                        ← World/Source/System/**
    scripts-system.json                ← Scripts/System/**
    scripts-items.json                 ← Scripts/Items/** (excl. Books)
    scripts-mobiles.json               ← Scripts/Mobiles/**
    scripts-engines-and-systems.json   ← Scripts/Engines and Systems/** (excl. Quests)
    scripts-utilities.json             ← Scripts/Utilities/**
    scripts-quests.json                ← Scripts/Engines and Systems/Quests/**
    scripts-books.json                 ← Scripts/Items/Books/**
  zh-Hans/                             ← Mirrors en/ structure
```

**Hand-maintained logical-key JSON** (same `en/` and `zh-Hans/` folders; **not** emitted by the C# extractor; keys are stable IDs consumed via `StringCatalog.TryResolveByKey` or equivalent). **Do not remove** — `build_localization_strings.py` whitelists them in `keep_extra`; deleting them drops translations at runtime. When adding a new bundle, edit that `keep_extra` set and extend this list.

| File | Purpose (summary) |
|---|---|
| `race-system.json` | Race / gypsy potion shelf UI and related copy (`racepotions.*`, `baserace.*`, …). |
| `shard-greeter.json` | Shard welcome / tarot gypsy and related copy (`shardgreeter.*`, …). |
| `stats-gump.json` | Stats gump strings. |
| `temptation-gump.json` | Temptation gump strings. |
| `thewar-quest.json` | War recruiter shouts and other curated war-quest lines (`thewar.*`, …). |
| `script-quest-unsent-letter.json` | **The Unsent Letter** quest: stable logical keys (`quest-unsent-letter.*`, …) via `StringCatalog.TryResolveByKey`, same pattern as other logical bundles; see `World/Documentation/quest-unsent-letter-implementation.md`. |

Other non-category locale files (also whitelisted, not scanner-owned): `vendor_npc_speech.json` (see `World/Source/Tools/` vendor speech scripts). Authoritative notes: `World/Data/Localization/README.txt`.

**Key management:**
- Hash keys: `s.` + 16 hex chars (SHA-256 of the exact EN string). These are stable as long as the English text is unchanged.
- Logical keys (e.g. `books.dynamic.*`): preserved manually across re-extraction runs.
- The runtime merges **all `*.json`** under `en/` and `zh-Hans/` at startup.

**Resolution:** `StringCatalog.TryResolve(key, lang)` → falls back to `en` if `zh-Hans` key is missing.

### 3.2 Adding Strings to C#

**Rule: Every user-visible literal must be localized. No exceptions.**

Use the `StringCatalog`-aware APIs that the extractor already handles:

```csharp
// NPC/system messages
mobile.SendMessage("Your message here.");
mobile.Say("Greeting text.");

// Gumps
AddLabel(x, y, hue, "Label text");
AddHtml(x, y, w, h, "Html content", false, false);
AddTooltip("Tooltip text");

// Quest objectives / text blocks
new TextDefinition("Objective description")
```

**Do not** pass variables as the string argument if the content must be localized; the extractor only captures string literals.

After adding strings to C#, **always run the extractor** (§3.3) to register them in the JSON files.

### 3.3 Extraction Tool

Run from repo root (`ultima-memento/`):

```bash
# Re-scan C#; preserve existing ZH translations where EN is unchanged
python3 World/Source/Tools/build_localization_strings.py --no-translate

# Optional: delete unknown *.json under en/ and zh-Hans/ (off by default — safe for hand-maintained logical bundles)
# python3 World/Source/Tools/build_localization_strings.py --no-translate --prune-stale-locale-files

# Optional CI guard: fail if a hash key with reviewed Chinese disappears from category JSON
# python3 World/Source/Tools/build_localization_strings.py --no-translate --fail-on-translated-zh-drop

# After adding new EN strings, verify extraction output before committing:
#   Check that new keys appear in the correct en/<category>.json
#   Check that zh-Hans/<category>.json has no stale English echoes for new entries
#   Inspect tools-output/extractor-key-drop-report.json (see README.txt) for removed keys vs pre-run JSON
```

The extractor does **not** translate. Translation is a separate step (§3.4). Each run writes `World/Data/Localization/tools-output/extractor-key-drop-report.json` (gitignored) listing keys dropped from category files compared to the JSON on disk before the run — use it to audit removals, not as a runtime merge.

#### 3.3.1 Runic spell words and English-only incantations

Some extracted strings are **not natural-language copy**: they are UO / Ultima **runic** circle words
(e.g. *In Vas Mani*) or other **fictional incantation tokens** (e.g. *Xtee Mee Glau*). For these,
`zh-Hans` **must match `en` verbatim** (same English letters and spacing). Do **not** translate
or spell them out in Chinese (e.g. avoid「在瓦斯玛尼」for *In Vas Mani*). When using the LLM
incremental queue, copy the English string unchanged into the Chinese field; `llm_incremental_locale.py`
also whitelists common examples in `_IDENTITY_HASH_EN_VALUES` so `stats` does not count them as
untranslated. Add new fixed incantation strings to that set if they are meant to stay English-only.

**Verification after extraction:**
- New EN keys present in the correct category file. ✓
- No EN strings duplicated into `zh-Hans/` as untranslated placeholders. ✓
- Run `python3 World/Source/Tools/sync_localization_glossary.py --check` — must exit 0. ✓

### 3.4 Translation Workflow — LLM Only

> **Policy:** Use LLM-based translation (e.g. Claude, GPT-4) for all new translations. **Do not use Google Translate or DeepL.** Machine translation from non-LLM sources produces lower-quality results that require more correction effort.

**Standard translation process for new strings:**

0. **Emit only deltas (saves tokens):** from repo root,
   `python3 World/Source/Tools/llm_incremental_locale.py stats` then
   `python3 World/Source/Tools/llm_incremental_locale.py queue -o Data/Localization/tools-output/llm-translation-queue.jsonl`.
   Each JSONL line is one `{ "file", "key", "en" }` — that set is exactly what still needs Chinese (hash keys: missing or zh == en; logical keys: missing unless you pass `--include-named-keys`). Do **not** paste entire `zh-Hans/*.json` files into an LLM unless you intend a full review pass.
1. **Extract** (already done if you ran `build_localization_strings.py --no-translate`): new/changed EN lives in `en/<category>.json`; queue step compares to `zh-Hans/<category>.json`.
2. **Load the glossary** (`glossary-approved-zh.json`). Any EN term present in the glossary **must** use its `canonical` Chinese translation verbatim in the output.
3. **Translate with LLM** (batch on the queue lines or split batches only), providing:
   - The game context: Ultima Online-style fantasy MMORPG, historical Chinese localization sensibility.
   - The full glossary as a constraint.
   - The editorial rules from `World/Documentation/zh-localization-translation-guide.md`.
4. **Apply** LLM output into `zh-Hans/`:
   ```bash
   python3 World/Source/Tools/llm_incremental_locale.py apply -i path/to/llm-translation-response.json
   ```
   Response shape: nested JSON `{ "<category>.json": { "<key>": "<zh>", ... }, ... }`, or a JSON array / JSONL of `{ "file", "key", "zh" }` objects (see tool docstring).

5. **Apply glossary normalization** after translation:
   ```bash
   python3 World/Source/Tools/sync_localization_glossary.py
   ```
6. **Verify:**
   ```bash
   python3 World/Source/Tools/sync_localization_glossary.py --check
   # Must exit 0 (no unapproved glossary terms remain)
   ```

**LLM translation prompt template:**

```
You are translating Ultima Online game server strings into Simplified Chinese (zh-Hans).
Context: Historical fantasy MMORPG. Tone: literary, slightly archaic, consistent with classic UO lore.

Mandatory glossary (use these exact translations, no alternatives):
<paste relevant entries from glossary-approved-zh.json>

For proper nouns NOT in the glossary: transliterate in brackets 【English】 on first use.
Do not paraphrase beyond what a professional game translator would. Keep punctuation natural for Chinese.

Translate these strings (one category per block if multiple files):
<key>: <English value>
...
Return JSON. Prefer nested maps:
``{ "scripts-quests.json": { "<key>": "<zh>", ... }, "scripts-system.json": { ... } }``.
For one category, a flat ``{ "<key>": "<zh>", ... }`` is OK if you merge with
``llm_incremental_locale.py apply -i response.json --base-file scripts-quests.json``.
```

**For book text** (logical key `books.dynamic.*` and `scripts-books`):
- Follow `World/Documentation/scripts-books-zh-translation-workflow.md` for the fragment-based merge process.
- Use the same LLM policy above; do not write fragments directly into `zh-Hans/scripts-books.json`.

### 3.5 Glossary Management

**File:** `World/Data/Localization/glossary-approved-zh.json`

Each entry:
```json
"English Term": {
  "canonical": "中文正式译名",
  "alternatives": ["备用译名1"],
  "category": "creature|item|place|skill|title|book|system",
  "notes": "English rationale for why this translation was chosen.",
  "translation_basis_zh": "中文说明，说明翻译选择的理由。"
}
```

**Rules:**
- **Only add entries that have been reviewed and agreed upon.** Do not add speculative terms.
- Every new entry needs both `notes` (English) and `translation_basis_zh` (Chinese).
- After adding or modifying glossary entries, run:
  ```bash
  python3 World/Source/Tools/sync_localization_glossary.py
  python3 World/Source/Tools/sync_localization_glossary.py --check
  ```
- Run `python3 World/Source/Tools/review_translations_glossary.py` to check consistency across all ZH files.

**Adding a new term:**
1. Confirm the English term is stable (not likely to be renamed).
2. Propose the canonical Chinese with rationale in `translation_basis_zh`.
3. Get human confirmation before committing the glossary entry.
4. Run glossary sync after confirmation.

### 3.6 Localization Checklist for Any PR

Before finalizing any change that touches C# user-visible strings:

- [ ] Ran `build_localization_strings.py --no-translate`
- [ ] New EN keys appear in correct category JSON
- [ ] For new zh-Hans work: used `llm_incremental_locale.py stats` / `queue` (delta only) before LLM, then `apply` — not Google/DeepL
- [ ] New ZH translations follow LLM policy (§3.4) — not Google/DeepL
- [ ] Glossary terms used correctly (`sync_localization_glossary.py --check` exits 0)
- [ ] No hardcoded Chinese or English strings remaining in C# gumps/messages

---

## 4. Build & Test

### 4.1 Building the Server

```bash
# Linux/macOS
cd World/Source
./compile-world-linux.sh        # or compile-world-mac.sh if present
# Windows
.\compile-world-win.bat
```

On compile error: read the error, trace the file and line, fix the issue. Do not guess or apply partial fixes.

### 4.2 Running the Server

```bash
mono WorldLinux.exe             # from ultima-memento/ root
```

The server outputs to stdout/stderr and writes logs under `World/`. Do not commit `World/Saves/` changes — these are runtime state.

**Optional `.env` (secrets):** At startup, `DotEnvLoader` reads `.env` from `Core.BaseDirectory` (the folder containing `WorldLinux.exe`, usually `World/`) and sets process environment variables without overwriting keys already set in the shell. For analytics, set `UO_MEMENTO_ANALYTICS_ACCOUNT_SALT` (see `World/.env.example`). Do not commit `.env` (gitignored).

### 4.3 What to Verify After Changes

| Change type | Verification |
|---|---|
| New C# feature | Compile succeeds; server starts without exception |
| New localization strings | Extraction runs cleanly; ZH file updated; glossary check passes |
| Glossary edit | `sync_localization_glossary.py --check` exits 0 |
| Quest system changes | No null reference exceptions on quest board load |
| Renamed/removed persistable types | See §2.4; load test or document migration for existing `World/Saves/` |

---

## 5. Agent Boundaries & Verification

### 5.1 Hard Boundaries — Never Do These

- **Never edit `World/Saves/`** (accounts, items, mobiles). These are live runtime state.
- **Never translate using Google Translate or DeepL APIs.** LLM-based translation only (§3.4).
- **Never add a glossary entry without human review** unless the user has explicitly approved the term in this session.
- **Never commit binary files** (`*.bin`, `*.idx`, `*.tdb`, `*.exe`, `*.dll`, compiled `*.pyc`).
- **Never modify the extraction regex patterns** in `build_localization_strings.py` without stating the change and its impact first.
- **Never hardcode user-visible strings in C#.** Always use the catalog.

### 5.2 Required Verification Steps

Before declaring a task complete, you must verify:

1. **Code compiles.** If you cannot run the compiler, state this explicitly.
2. **Localization extractor runs without error** (if C# strings were added/changed).
3. **Glossary sync check passes** (if any ZH files were modified).
4. **No unintended files modified.** Run `git diff --name-only` and confirm the list matches your intent.

### 5.3 When to Pause and Ask

Pause and ask the user before proceeding when:

- A task would modify more than 5 files outside of the primary stated scope.
- A glossary term conflict is discovered (existing `canonical` contradicts a new string).
- A C# compile error originates in a file you did not modify (may indicate pre-existing breakage).
- Translation of a culturally sensitive or lore-critical term has no obvious correct answer.
- The extraction tool output looks wrong (key counts change unexpectedly, categories mismatch).
- You **remove or rename** a type that may exist in player saves, unless a migration path or operator acknowledgment is explicit (see §2.4).

### 5.4 Self-Reporting

At the end of every substantial task, report:
- Files modified (list them).
- Verification steps completed (and their outcome).
- Any deferred items or open questions.
- Any discovered conventions that contradict this guide (propose an update).

---

## 6. Scaling & Updating This Guide

### 6.1 When to Update This File

Update `AGENTS.md` when:
- A new localization language is added beyond `en` / `zh-Hans`.
- A new **logical-key JSON** bundle is added under `Data/Localization/en/` (and `zh-Hans/`) — update §3.1 table and `keep_extra` in `build_localization_strings.py`.
- A new Python tool is added to `World/Source/Tools/`.
- A new source directory category is added under `World/Source/Scripts/`.
- A build or test process changes.
- Cross-repo website conventions change (§7: media paths, wiki index pipeline, glossary inputs).
- An AI agent discovers a recurring mistake pattern (add it to §5.1, §5.2, or §2.4 as appropriate).

### 6.2 Language Expansion Protocol

When adding a third language (e.g. `zh-Hant`, `ja`):
1. Create `World/Data/Localization/<new-locale>/` mirroring `en/` categories.
2. Add the locale to `World/Data/System/CFG/localization.cfg`.
3. Create a dedicated translation guide under `World/Documentation/`.
4. Create a glossary file `glossary-approved-<locale>.json` following the same schema.
5. Update `build_localization_strings.py` to emit to the new locale directory.
6. Update this guide (§3.1 table, §3.4 prompt template locale, §3.6 checklist).

### 6.3 Versioning

This file uses a simple date-stamp comment at the top for tracking. When making substantive updates, add a one-line change note at the bottom of this section.

**Change log:**
- 2026-04-18: Initial version created. Covers C# practices, localization pipeline, LLM translation policy, agent boundaries.
- 2026-04-18: Added §7 — cross-repo practice standard for `ultima-memento-web` (media vendoring, glossary-driven wiki index).
- 2026-04-29: §3.1 — documented hand-maintained logical-key JSON files and `keep_extra` contract; §6.1 — update trigger for new bundles.
- 2026-04-29: §3.3 — `build_localization_strings.py` defaults to **not** pruning extra locale JSON; drop-report + `--fail-on-translated-zh-drop`; `SendMessage`/GreeterKey extractor fix documented in `README.txt`.
- 2026-04-29: §3.4 + README — `llm_incremental_locale.py` (`stats` / `queue` / `split-queue` / `apply`) for token-efficient incremental LLM translation.
- 2026-04-29: `build_localization_strings.py` — quest scan adds **`new DummyReward("…")`** and **`DefaultName => "…"`** (alongside existing `ItemReward` / RPG gump patterns) so text-only rewards and co-located quest item names register in `scripts-quests.json`.
- 2026-04-29: §3.3.1 — runic / incantation strings (e.g. *In Vas Mani*, *Xtee Mee Glau*): `zh-Hans` mirrors `en`; `llm_incremental_locale.py` `_IDENTITY_HASH_EN_VALUES` extended; `README.txt` + `zh-localization-translation-guide.md` §2.6.
- 2026-04-29: §2.4 — persistence / deserialization when removing or renaming persistable C# types; §0 index row; §4.3 verification table; §5.3 pause trigger; §6.1 update trigger.

---

## 7. Website & player-facing docs (`ultima-memento-web`)

> **Canonical detail:** `ultima-memento-web/AGENTS.md` (Next.js, routes, MDX).  
> **This section** is the **practice standard** agents should follow when work touches **both** repos: game glossary / showcase assets ↔ public site.

### 7.1 Scope split

| Concern | Owns it |
|---|---|
| Server C#, runtime strings, `glossary-approved-zh.json` schema & curation | **This repo** (`ultima-memento`) |
| Next.js app, MDX under `content/` / `content-en/`, `public/` assets, wiki index JSON | **`ultima-memento-web`** |

When you add or rename a **glossary** headword that should appear on the site’s auto wiki index, update the site after merging glossary changes (§7.4).

### 7.2 Media practice standard (images & GIFs)

**Rule:** Player-facing media on the website must be **vendored in the web repo** under `public/` — no dependence on hotlinked Wikimedia, arbitrary CDNs, or **GitHub `raw.githubusercontent.com`** for default page rendering.

| Asset type | Location (web repo) | Notes |
|---|---|---|
| Encyclopedia / article stills (e.g. MDX figures) | `public/images/…` (e.g. `public/images/encyclopedia/`) | Prefer stable filenames; MDX uses paths like `/images/encyclopedia/foo.jpg`. |
| Feature & home **GIF** previews | `public/showcase/*.gif` | Filenames must match `messages/zh.json` and `messages/en.json` (`media` fields) and any MDX that references the same names. |
| Source of GIFs when refreshing from game tree | `World/Documentation/Showcase/` in **this** repo | Copy or sync binaries into `ultima-memento-web/public/showcase/`; do not rely on raw GitHub URLs in production `showcaseUrl` logic. |

**Agent checklist (web PR):**

- [ ] No new `https://` image URLs in MDX for assets we can legally mirror into `public/`.
- [ ] New GIFs added to `public/showcase/` and wired in `messages` (both locales).
- [ ] `npm run build` in `ultima-memento-web` passes.

### 7.3 Glossary ↔ Ultima Codex / UOGuide index

**Purpose:** MDX pages (guide / mechanics / history) can show a **“wiki index”** block: terms that appear on the page, intersected with the **approved glossary** in this repo, linked only to **Ultima Codex** and **UOGuide** URLs that have been **HEAD-validated**.

**Source of truth for terms:** `World/Data/Localization/glossary-approved-zh.json` (English headwords under `terms`, plus top-level entries with `canonical` / `alternatives`).

**Generated artifact (web repo):** `ultima-memento-web/lib/wiki-glossary-index.json`  
**Generator:** `ultima-memento-web/scripts/build-wiki-glossary-index.mjs`  
**Command:** `npm run build:wiki-index` (run from `ultima-memento-web/`).

**Default glossary path** inside the script: sibling checkout  
`ultima-memento/World/Data/Localization/glossary-approved-zh.json`  
If the layout differs, set **`GLOSSARY_PATH`** to the absolute path of `glossary-approved-zh.json`.

**Matching rules (high level):**

- English headwords: phrase / word-boundary style matching against concatenated MDX from `content/` and `content-en/`.
- Chinese surfaces: `canonical` and `alternatives` from the glossary entry (`matchZh` in the generated JSON) for matching **Chinese** MDX.
- Script maintains a **denylist** and **minimum token length** for headwords that would otherwise create false positives (e.g. generic English words with unrelated wiki articles).
- Some glossary **alternatives** are valid for translation but must **not** drive wiki matching (e.g. ambiguous substring); those are stripped in the script (`MATCH_ZH_STRIP` map) — extend it when adding a problematic alternative.

**Agent checklist (after glossary or site MDX changes that affect lore terms):**

- [ ] If new headwords should appear in the site index: ensure they exist in `glossary-approved-zh.json` with stable English keys and appropriate `canonical` / `alternatives`.
- [ ] Re-run `npm run build:wiki-index` with network access; commit the updated `wiki-glossary-index.json` if entries changed.
- [ ] Spot-check Codex/UOGuide pages for **sense** (UO item vs Ultima lore disambiguation); drop or split a headword in the denylist or strip list if the wrong article keeps winning.

### 7.4 When to touch which repo

| You did this… | Also do this |
|---|---|
| Added GIFs under `World/Documentation/Showcase/` | Copy into `ultima-memento-web/public/showcase/` and verify both `messages/*.json` locales. |
| Added/changed glossary entries used in player-facing lore on the site | Re-run `build:wiki-index`; adjust denylist / `MATCH_ZH_STRIP` in the script if matching becomes noisy. |
| Wrote new MDX that introduces a major Ultima/UO proper noun | Prefer adding the term to **glossary** first, then regenerate the wiki index. |

### 7.5 Single source of truth reminder

- **Glossary curation** stays in **this** repo (§3.5).  
- **Wiki URL validation and MDX matching** are implemented in **`ultima-memento-web`**; do not duplicate the JSON generator inside `World/Source/Tools/` unless we explicitly decide to merge pipelines later.

# Localization plan: current baseline and remaining gaps

This document reflects the **implemented** localization architecture in `ultima-memento`, then defines the remaining plan to reach broader coverage.

---

## 1. Implemented baseline (as of current code)

- **Locales:** `en` and `zh-Hans` are shipped; `zh-Hant` is out of scope.
- **Language state:** per-account via `Account` tag (`AccountLang.TagName = "Language"`).
- **Default/fallback config:** `World/Data/System/CFG/localization.cfg`
  - `DefaultLanguage=...`
  - `FallbackLanguage=...`
- **Catalog loader:** `StringCatalog` loads all `*.json` under:
  - `World/Data/Localization/en/`
  - `World/Data/Localization/zh-Hans/`
- **Fallback behavior:** `zh-Hans` key missing -> `en` key -> original English literal.
- **Legacy compatibility:** still supports deprecated monolith files if split directories are empty.

---

## 2. Implemented data layout

```text
World/Data/Localization/
  en/
    system.json
    scripts-system.json
    scripts-items.json
    scripts-books.json
    scripts-mobiles.json
    scripts-quests.json
    scripts-engines-and-systems.json
    scripts-utilities.json
    ... (extra maintained JSON files, e.g. vendor_npc_speech.json)
  zh-Hans/
    (mirror of en/)
```

Notes:

- Runtime merges all files into a single in-memory dictionary per locale.
- Hash keys (`s.<16hex>`) come from the exact English sentence (`StringKey.ForEnglish`).
- Logical keys (e.g. `books.dynamic.*`) are also supported via `TryResolveByKey`.

---

## 3. Implemented runtime localization paths

The following are language-aware in current runtime:

- `Mobile.SendMessage(string)` / `SendAsciiMessage(string)`
- Overhead string paths:
  - `PublicOverheadMessage(string)`
  - `PrivateOverheadMessage(string)`
  - `LocalOverheadMessage(string)`
  - `NonlocalOverheadMessage(string)`
- `Item.PublicOverheadMessage(string)`
- `World.Broadcast(string)`
- Gump string intern path via `Gump.InternLocalized(...)`
- Books:
  - `BaseBook` title/author/pages
  - `DynamicBook` resolver paths (including split/template handling where added)

Out of v1 scope / not fully owned:

- client cliloc-only numeric resources
- player-authored text
- all dynamic runtime-composed strings (depends on per-case resolver support)

---

## 4. Implemented extraction baseline

`World/Source/Tools/build_localization_strings.py` currently extracts:

- core message literals (`SendMessage`, `SendAsciiMessage`, `Say`, tooltip/label/html/broadcast helpers)
- gump-related literals (`AddHtml`, `AddLabel`, `AddLabelCropped`, `AddTooltip`, `LabelTo`)
- quest/book-specific patterns (`builder.Append`, objective text, quest/gump helper patterns)
- `DynamicBook` targeted constant extraction (`public const string ... = @"..."`) for hash stability

Category mapping is path-based and already includes dedicated:

- `scripts-books`
- `scripts-quests`

---

## 5. Translation policy (project rule)

- **LLM-only** translation workflow for new zh-Hans content.
- Do **not** use Google/DeepL output as final shipping text.
- Glossary enforcement is mandatory via:
  - `sync_localization_glossary.py`
  - `sync_localization_glossary.py --check`

---

## 6. Remaining gaps to close

1. **Dynamic composition tails:** strings assembled at runtime still need targeted resolver handling when exact-hash matching is impossible.
2. **Extractor breadth:** keep expanding edge-pattern extraction where new misses appear.
3. **Doc + tooling alignment:** ensure extractor behavior around extra split JSON files stays aligned with repo-maintained assets.
4. **QA gates:** continuously reduce English leftovers in changed zones before merge.

---

## 7. Acceptance checklist for each localization wave

- New English literals appear in correct `en/*.json` category.
- `zh-Hans` updates preserve placeholders (`{0}`, etc.) and tags (`<BR>`, HTML spans).
- `sync_localization_glossary.py --check` passes.
- Runtime behavior validates in-game for touched UI/messages/books.
- Diff contains only intended localization and supporting code/doc updates.

---

## 8. Related docs

- `localization-implementation-review.md`
- `localization-complete-coverage-roadmap.md`
- `zh-localization-translation-guide.md`
- `zh-localization-glossary-sync-workflow.md`

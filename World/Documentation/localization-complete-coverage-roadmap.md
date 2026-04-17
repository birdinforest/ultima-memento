# Localization complete-coverage roadmap

This document defines how Memento should reach practical full multi-language coverage for server-provided text, with `zh-Hans` as the first fully supported non-English locale.

It extends and consolidates:

- `localization-plan-multiple-languages.md`
- `localization-content-migration-plan.md`
- `localization-implementation-review.md`
- `zh-localization-translation-guide.md`
- `zh-localization-glossary-sync-workflow.md`

## 1. Definition of complete coverage

For this project, "complete" means:

1. Player-visible server text is routed through a language-aware path whenever the shard, rather than the client, owns the sentence.
2. Extractable English source text is emitted into versioned localization JSON.
3. Proper nouns are extracted, reviewed, and normalized through glossary assets before shipping Chinese text.
4. `zh-Hans` JSON stays synchronized with the latest approved canonical terminology.

Not fully owned by this roadmap:

- client cliloc resources that only exist as numeric client-side entries
- player-authored chat text

## 2. Coverage model

### 2.1 Runtime send paths

These should resolve by recipient account language:

- `SendMessage` / `SendAsciiMessage`
- `PublicOverheadMessage(string)`
- `PrivateOverheadMessage(string)`
- `LocalOverheadMessage(string)`
- `NonlocalOverheadMessage(string)`
- item `PublicOverheadMessage(string)`
- `World.Broadcast(string)`
- gump text that compiles through `Gump.InternLocalized`
- books and dynamic book chrome

### 2.2 Extraction model

The extractor must collect the English literals that feed those runtime paths, including:

- `SendMessage` / `SendAsciiMessage` with and without a leading hue parameter
- `Say`
- gump `AddHtml`, `AddLabel`, `AddLabelCropped`
- `AddTooltip(string)`
- `LabelTo(string)`
- string overloads of overhead-message helpers
- `Broadcast(...)`
- quest / book-specific builders and assignments

## 3. Proper noun workflow

This workflow is mandatory before large-scale Chinese refreshes:

1. Rebuild English localization artifacts.
2. Rebuild the heuristic lore glossary.
3. Review recurring proper nouns and shard-specific terms against Ultima / UO worldview.
4. Update curated glossary files.
5. Run glossary sync so shipped `zh-Hans` files converge to the approved canon.
6. Only then perform broader Chinese refresh / QA.

## 4. Canonical assets

### 4.1 Machine-extracted glossary assets

- `World/Data/Localization/lore-glossary.json`
- `World/Documentation/lore-glossary.md`

Produced by:

```bash
python3 World/Source/Tools/build_lore_glossary.py
```

### 4.2 Curated terminology assets

- `World/Data/Localization/glossary-approved-zh.json`
- `World/Data/Localization/zh-Hans-glossary-reference.md`
- `World/Data/Localization/zh-Hans-glossary-sync-rules.json`

Responsibilities:

- `lore-glossary.json`: heuristic discovery and frequency/co-occurrence evidence
- `glossary-approved-zh.json`: approved canonical zh-Hans translations plus alternatives and rationale
- `zh-Hans-glossary-reference.md`: human reference sheet and research notes
- `zh-Hans-glossary-sync-rules.json`: locale-specific follow-up normalization rules when glossary-only replacement is not enough

## 5. Recommended end-to-end workflow

Run from repository root:

```bash
python3 World/Source/Tools/build_localization_strings.py --no-translate
python3 World/Source/Tools/build_lore_glossary.py
python3 World/Source/Tools/review_translations_glossary.py
python3 World/Source/Tools/translate_zh_from_en.py
python3 World/Source/Tools/build_scripts_books_zh.py
python3 World/Source/Tools/sync_localization_glossary.py
python3 World/Source/Tools/sync_localization_glossary.py --check
```

Operator meaning:

- `build_localization_strings.py`: rebuild English and placeholder zh-Hans catalogs from source
- `build_lore_glossary.py`: refresh machine-extracted proper-noun evidence
- `review_translations_glossary.py`: detect English leftovers / inconsistent zh-Hans handling
- `translate_zh_from_en.py`: incremental machine-assisted fill for new keys
- `build_scripts_books_zh.py`: merge hand-maintained book fragments
- `sync_localization_glossary.py`: enforce approved glossary canon and context-specific overrides

## 6. Priority implementation order

### Phase A: runtime architecture gaps

Highest value:

1. `Mobile.LocalOverheadMessage(string)`
2. `Mobile.NonlocalOverheadMessage(string)`
3. `Item.PublicOverheadMessage(string)`
4. `World.Broadcast(string)`

These affect visible gameplay messages even if the JSON files are already translated.

### Phase B: extraction coverage

Expand `build_localization_strings.py` so that regular gumps and helper wrappers are actually added to localization JSON.

### Phase C: glossary refresh

After extraction coverage grows, rerun glossary assets and curate any new high-frequency lore names before mass-refreshing Chinese text.

### Phase D: locale refresh

Regenerate and normalize `zh-Hans/*.json`, then review hotspots with long narrative or UI-heavy copy.

## 7. Ultima worldview rules for term approval

When approving proper nouns:

- prefer established Ultima / UO community usage when it does not conflict with clear shard lore
- distinguish place vs character collisions
- prefer phonetic transliteration for unique entities where literal translation obscures proper-name status
- keep alternatives in the glossary when player usage is mixed
- record rationale in `translation_basis_zh`

Examples of common risk:

- city name vs title or character name collisions
- realm name vs city name collisions
- generic metaphysical noun vs specific lore location

## 8. Acceptance criteria

We consider a wave complete when:

1. The runtime send path is language-aware where it should be.
2. The extractor emits the English source text into `Data/Localization/en/*.json`.
3. The relevant proper nouns appear in `lore-glossary.json` or are explicitly documented as exceptions.
4. `glossary-approved-zh.json` contains approved zh-Hans canon for recurring lore terms touched by the wave.
5. `sync_localization_glossary.py --check` passes.
6. Spot scans do not show obvious English leftovers in the changed area.

## 9. Current program-level risks

- cliloc-driven UI still follows client resources rather than shard JSON
- tooltip argument strings in `GumpTooltip` are not yet routed through `StringCatalog`
- highly dynamic formatted strings still depend on extractor coverage and exact post-format matching
- long-tail scripts across `Mobiles/`, `Gumps/`, and commands remain large-volume migration work even after central runtime fixes

## 10. Deliverables for each wave

- code changes for runtime coverage
- regenerated `en/*.json` and `zh-Hans/*.json`
- refreshed `lore-glossary.json`
- curated `glossary-approved-zh.json` updates where needed
- updated QA / workflow docs if the process changes

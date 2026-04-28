# Localization developer guide (implementation + add-new-text workflow)

This guide helps developers understand how localization currently works in `ultima-memento`, and how to add new player-facing text correctly.

---

## 1) Quick model

Localization is hash-key based at runtime:

1. Code emits an English literal (or a known template/key).
2. Runtime computes a stable key from that English text (`StringKey.ForEnglish`).
3. `StringCatalog` resolves by account language (`en` or `zh-Hans`).
4. If zh-Hans is missing, fallback goes to English.

Important consequence:

- Any text change to the English literal creates a new hash key.
- Dynamic concatenation/interpolation can break direct hash matching unless you localize by segments or a dedicated resolver.

---

## 2) Core runtime components

- `World/Source/System/Localization/StringCatalog.cs`
  - Loads merged locale dictionaries from `Data/Localization/en/**/*.json` and `Data/Localization/zh-Hans/**/*.json`.
  - Main APIs:
    - `TryResolve(languageCode, englishLiteral)` (hash-based)
    - `TryResolveByKey(languageCode, logicalKey)` (stable logical key path)
    - `Resolve(account, english)` / `ResolveFormat(account, englishFormat, args...)`
- `World/Source/System/Localization/AccountLang.cs`
  - Stores account language in account tag `Language`.
  - Chooses default/fallback using `LangConfig`.
- `World/Source/System/Localization/LangConfig.cs`
  - Reads `World/Data/System/CFG/localization.cfg`.

Language config:

- `DefaultLanguage=zh-Hans` (or `en`)
- `FallbackLanguage=en`

---

## 3) Where localized text data lives

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
    ...
  zh-Hans/
    (same split structure)
```

Notes:

- Runtime merges all `*.json` under each locale folder.
- Hash keys look like `s.4ead48b3a9c22bd8`.
- Some entries can use logical keys (for stable fragments/templates).

---

## 4) Runtime-localized paths already in place

Current implementation already localizes major server-owned paths, including:

- `Mobile.SendMessage` / `SendAsciiMessage`
- `PublicOverheadMessage` / `PrivateOverheadMessage` / `LocalOverheadMessage` / `NonlocalOverheadMessage` (string overloads)
- `Item.PublicOverheadMessage` (string overload)
- `World.Broadcast` (string overload)
- gump text through `Gump.InternLocalized`
- books (`BaseBook`, `DynamicBook` resolver paths)

Still requires care:

- deeply dynamic composed strings
- format paths where the final emitted sentence does not exactly match extractor/runtime assumptions

---

## 5) Extraction pipeline

Primary extractor:

```bash
python3 World/Source/Tools/build_localization_strings.py --no-translate
```

What it does:

- scans source literals
- writes/updates split `en/*.json`
- writes `zh-Hans/*.json` placeholders while preserving existing zh translations

After extraction, always verify:

1. New keys landed in the expected category file.
2. zh-Hans entries for new keys are translated (not English placeholders).
3. No unintended large churn in unrelated localization files.

---

## 6) Glossary workflow (mandatory for zh-Hans consistency)

Canonical glossary file:

- `World/Data/Localization/glossary-approved-zh.json`

Sync/check:

```bash
python3 World/Source/Tools/sync_localization_glossary.py
python3 World/Source/Tools/sync_localization_glossary.py --check
```

Use this to enforce canonical terms (places, titles, system terms, etc.) before merging.

---

## 7) How to add new text (recommended workflow)

### Step A: Write code using localizable literals

For regular messages/gumps, keep user-visible strings as explicit literals so extractor can see them.

Avoid:

- hidden string construction that extractor cannot detect
- mixing multiple variable fragments into one runtime-only sentence

Prefer:

- full literal sentence, or
- dedicated resolver method that localizes stable segments separately.

### Step B: Run extractor

```bash
python3 World/Source/Tools/build_localization_strings.py --no-translate
```

### Step C: Translate new zh-Hans entries

- Use LLM translation with project terminology rules.
- Keep placeholders and markup intact (`{0}`, `<BR>`, etc.).

### Step D: Run glossary normalization/check

```bash
python3 World/Source/Tools/sync_localization_glossary.py
python3 World/Source/Tools/sync_localization_glossary.py --check
```

### Step E: Verify behavior in game

- language switch `en` <-> `zh-Hans` on account
- check touched gumps/messages/books
- ensure no English leakage in changed flow

---

## 8) Dynamic text patterns: what to do

If one sentence is built from dynamic parts, do not rely on whole-string hash lookup.

Use one of these patterns:

1. **Segment localization**: resolve fixed segments independently, then concatenate.
2. **Format template localization**: localize a template with placeholders, then `string.Format`.
3. **Logical key path**: for fragile fragments, resolve via `TryResolveByKey`.

Existing examples:

- Work Shoppes body/suffix split resolver
- Guide-to-Adventure format resolver (`{0}` server name / bribery amount)
- Rune Journal fixed segments + dynamic spell list

---

## 9) Common mistakes to avoid

- Editing English literals casually after translation is complete (creates new keys).
- Translating placeholders incorrectly (e.g. dropping `{0}`).
- Breaking HTML/line-break tags in book/gump text.
- Assuming all cliloc numeric text is covered by server JSON localization.
- Shipping with untranslated English placeholders in `zh-Hans`.

---

## 10) Minimal PR checklist (localization-related changes)

- [ ] New user-facing literals extracted to `en/*.json`
- [ ] New zh-Hans values translated and reviewed
- [ ] `sync_localization_glossary.py --check` passes
- [ ] Runtime flow verified in-game for touched path
- [ ] Diff scope matches intended localization changes

---

## 11) Related docs

- `localization-plan-multiple-languages.md`
- `localization-implementation-review.md`
- `localization-complete-coverage-roadmap.md`
- `zh-localization-translation-guide.md`
- `zh-localization-glossary-sync-workflow.md`

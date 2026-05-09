# zh-Hans glossary sync workflow

This document records the repeatable post-processing step that normalizes shipped zh-Hans JSON files to the current approved glossary.

It complements, rather than replaces:

- `World/Source/Tools/build_localization_strings.py` for extracting English / placeholder JSON
- `World/Source/Tools/translate_zh_from_en.py` for machine-assisted incremental fill
- `World/Source/Tools/build_scripts_books_zh.py` for the hand-maintained books fragment merge

## 1. Purpose

Machine translation, older manual edits, and incremental lore updates can leave the locale files in a mixed state:

- approved canonical term in one file
- older Chinese variant in another file
- raw English proper noun still inside `【】`
- context-sensitive string that needs a fixed phrasing, not just a term swap

`World/Source/Tools/sync_localization_glossary.py` is the repeatable cleanup step for this.

## 2. Inputs

### 2.1 Curated glossary

- `World/Data/Localization/glossary-approved-zh.json`

The script reads:

- English glossary key
- `canonical`
- `alternatives`

From those it builds a variant map such as:

- `Sosaria` -> `索沙尼亚`
- `索萨里亚` -> `索沙尼亚`
- `索萨利亚` -> `索沙尼亚`（与站内外文、approved glossary 主译名对齐）
- `Skara Brae` -> `斯卡拉·布雷`
- `斯卡拉布雷` -> `斯卡拉·布雷`

### 2.2 Locale-specific follow-up rules

- `World/Data/Localization/zh-Hans-glossary-sync-rules.json`

Use this rules file for cases that glossary-only normalization cannot safely infer:

- literal replacements scoped to one or more files
- exact value overrides for known lines whose final phrasing is contextual

This keeps the script generic while still making the zh-Hans behavior reproducible.

## 3. What the script does

Run from repository root:

```bash
python3 World/Source/Tools/sync_localization_glossary.py
```

The script applies four passes to each target locale JSON file:

1. Bracket normalization
   - Example: `【Sosaria】` -> `【索沙尼亚】`
   - Example: `【索萨里亚】` -> `【索沙尼亚】`
2. Whole-value normalization
   - Example: a standalone item / place name like `蒙托尔` -> `蒙托城`
3. Literal replacements from the locale rules file
   - Example: `datacron` -> `数据晶石`
4. Exact value overrides from the locale rules file
   - Example: a specific quest line can be forced to `终极智慧法典` wording

## 4. Recommended workflow

### 4.1 Normal zh-Hans maintenance

1. Rebuild extracted localization files

```bash
python3 World/Source/Tools/build_localization_strings.py --no-translate
```

2. Fill new zh-Hans placeholders if needed

```bash
python3 World/Source/Tools/translate_zh_from_en.py
```

3. Rebuild hand-maintained books if that workflow is involved

```bash
python3 World/Source/Tools/build_scripts_books_zh.py
```

4. Update glossary decisions when canon changes

- edit `World/Data/Localization/glossary-approved-zh.json`
- if needed, edit `World/Data/Localization/zh-Hans-glossary-sync-rules.json`

5. Normalize the shipped locale files

```bash
python3 World/Source/Tools/sync_localization_glossary.py
```

6. Verify no unexpected drift remains

```bash
python3 World/Source/Tools/sync_localization_glossary.py --check
```

If `--check` exits non-zero, some file still needs normalization or a new rule.

### 4.2 Target only a few files

```bash
python3 World/Source/Tools/sync_localization_glossary.py scripts-quests.json scripts-items.json
```

## 5. Maintaining the rules file

### 5.1 Add a `literal_replacements` rule when

- the term is a stable proper noun or approved Chinese variant
- the replacement is safe within the listed file scope
- you want future incremental updates to normalize automatically

Example:

```json
{
  "from": "斯卡拉布雷",
  "to": "斯卡拉·布雷",
  "files": ["scripts-items.json", "scripts-quests.json"]
}
```

### 5.2 Add an `exact_value_overrides` rule when

- the final shipped Chinese depends on sentence context
- a simple glossary swap would produce awkward Chinese
- the string must keep a very specific phrasing

Example:

```json
{
  "file": "scripts-system.json",
  "key": "s.47ba6ba213e96c66",
  "value": "这在终极智慧法典之厅内不起作用。"
}
```

## 6. Other locales

The script is locale-agnostic. For another locale pack, provide:

- a locale directory under `World/Data/Localization/<locale>/`
- a locale glossary via `--glossary`
- an optional locale rules file via `--rules`

Example:

```bash
python3 World/Source/Tools/sync_localization_glossary.py \
  --locale ja \
  --glossary World/Data/Localization/glossary-approved-ja.json \
  --rules World/Data/Localization/ja-glossary-sync-rules.json
```

## 7. Files involved

| Path | Role |
|------|------|
| `World/Source/Tools/sync_localization_glossary.py` | Generic glossary sync script |
| `World/Data/Localization/glossary-approved-zh.json` | Curated zh-Hans glossary decisions |
| `World/Data/Localization/zh-Hans-glossary-sync-rules.json` | zh-Hans-specific follow-up rules |
| `World/Data/Localization/zh-Hans/*.json` | Shipped Simplified Chinese locale files |
| `World/Data/Localization/README.txt` | Main localization overview |

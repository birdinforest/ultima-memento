# scripts-books.json — zh-Hans translation workflow

This document records how **`World/Data/Localization/zh-Hans/scripts-books.json`** is produced and maintained from **`World/Data/Localization/en/scripts-books.json`**, including scripts, tooling, and update procedures.

Editorial rules for the Chinese text itself live in **`World/Documentation/zh-localization-translation-guide.md`** (proper names, tone, no silent expansion in shipped JSON, etc.).

---

## 1. Source of English strings

- Book and related strings are extracted from C# (e.g. `BookText`, titles, literals) into split JSON by:
  - `World/Source/Tools/build_localization_strings.py`
- After a scan, **`en/scripts-books.json`** holds the canonical English keys (`s.<hash>`, plus `books.dynamic.*` for dynamic book chrome).
- Runtime loads merged keys from all `en/*.json` and `zh-Hans/*.json` under `Data/Localization/` (see `World/Data/Localization/README.txt`).

---

## 2. Why fragments instead of one huge hand-edit?

- The file is large and many entries are long lore or UI tutorials.
- Hand translation (including LLM-assisted drafting) is done in **batches** so reviews stay manageable and diffs stay smaller than a single 100k+ line file.
- Batches are sized by approximate character volume so each fragment stays a comfortable editing unit. The list of keys per batch is kept in:
  - **`World/Source/Tools/scripts_books_zh_fragments/_batches.txt`**  
  Sections are labeled `### batch 1`, `### batch 2`, … with one key per line.

---

## 3. Fragment files and merge script

| Path | Role |
|------|------|
| `World/Source/Tools/scripts_books_zh_fragments/frag_01.json` … `frag_13.json` | One JSON object per file: only the keys for that batch, `key` → Simplified Chinese string. |
| `World/Source/Tools/scripts_books_zh_fragments/_batches.txt` | Human-readable index of which keys belong to which batch (regenerate if you change batching strategy). |
| `World/Source/Tools/build_scripts_books_zh.py` | Merges all `frag_*.json` into **`World/Data/Localization/zh-Hans/scripts-books.json`**. |

**Merge rules (`build_scripts_books_zh.py`):**

1. Load `en/scripts-books.json` (defines the full key set and key order in output).
2. Load every `World/Source/Tools/scripts_books_zh_fragments/frag_*.json` in **sorted filename order** (`frag_01` … `frag_13`; use zero-padded two digits so sort order matches batch order).
3. **No duplicate keys** across fragments; **no extra keys** not present in English; **no missing keys** — otherwise the script exits with an error and prints offending keys.
4. Write `zh-Hans/scripts-books.json` with `ensure_ascii=False`, `indent=2`, keys in the same order as English.
5. After merge, optionally run the glossary normalizer to align proper nouns with the latest approved canonical forms:
   - `python3 World/Source/Tools/sync_localization_glossary.py scripts-books.json`

**Run from repository root:**

```bash
python3 World/Source/Tools/build_scripts_books_zh.py
```

---

## 4. How the first full pass was done (reference process)

1. **Editorial baseline** — Follow `zh-localization-translation-guide.md` (e.g. uncertain proper nouns in **【English】**; preserve HTML, `{0}`, mantras, client command snippets where the English does).
2. **Batches** — Keys were grouped using a small Python one-off (cumulative English length per batch) and recorded in `_batches.txt`.
3. **Translation** — Each batch was turned into a fragment JSON (`frag_NN.json`). Work was done with **human / LLM drafting**, not Google Translate, per project preference.
4. **Truncated English** — Some `en` values end mid-sentence because the game **appends** more text at runtime (e.g. city names). Chinese in the matching keys should **stop at the same truncation**, not invent the rest.
5. **Merge** — Run `build_scripts_books_zh.py`; fix any duplicate/missing/extra key errors until it succeeds.
6. **Glossary sync** — Run `sync_localization_glossary.py scripts-books.json` if glossary-approved spellings changed since the fragments were last reviewed.
7. **Spot checks** — Grep for leftover English phrases in `zh-Hans/scripts-books.json` on long entries that were previously untranslated (e.g. UI tutorials).

---

## 5. Ongoing maintenance (when English changes)

### 5.1 After `build_localization_strings.py` updates `en/scripts-books.json`

1. **Diff** `en/scripts-books.json` (new keys, changed strings, removed keys).
2. **New hash key** — Add the new key to the appropriate batch in `_batches.txt` (or add a new batch and a new `frag_NN.json` if you split differently), translate, append to an existing fragment only if you keep batch boundaries clear.
3. **Changed English** — Update the Chinese value in the fragment that owns that key.
4. **Removed key** — Remove the key from the fragment file(s); ensure no fragment still references it.
5. **Run** `python3 World/Source/Tools/build_scripts_books_zh.py`.
6. **Then run** `python3 World/Source/Tools/sync_localization_glossary.py scripts-books.json` if the glossary or proper-name canon changed.
7. Commit `zh-Hans/scripts-books.json` plus any edited `frag_*.json` / `_batches.txt`.

### 5.2 Optional: machine-assisted first draft for *new* keys only

- **`translate_zh_from_en.py`** can fill missing zh from en (see `README.txt` in `Data/Localization`). It may use external translator backends depending on your environment — **review output** before treating it as final, especially for lore and for keys that share the truncation rules above.
- Curated spellings: **`Data/Localization/glossary-approved-zh.json`** (optional); glossary QA: `build_lore_glossary.py` / `review_translations_glossary.py` (see main localization README).
- Repeatable glossary normalization pass: **`World/Source/Tools/sync_localization_glossary.py`** with locale rules in **`Data/Localization/zh-Hans-glossary-sync-rules.json`**.

### 5.3 Regenerating `_batches.txt` (optional)

If you want to re-split batches by size from current English:

```python
import json
from pathlib import Path

root = Path("World/Data/Localization")
en = json.loads((root / "en" / "scripts-books.json").read_text(encoding="utf-8"))
keys = list(en.keys())
batches, cur, sz = [], [], 0
max_sz = 11000  # tune
for k in keys:
    L = len(en[k])
    if cur and sz + L > max_sz:
        batches.append(cur)
        cur, sz = [], 0
    cur.append(k)
    sz += L
if cur:
    batches.append(cur)

out = Path("World/Source/Tools/scripts_books_zh_fragments/_batches.txt")
lines = []
for i, b in enumerate(batches, 1):
    lines.append(f"### batch {i} ({len(b)} keys)")
    lines.extend(b)
    lines.append("")
out.write_text("\n".join(lines), encoding="utf-8")
print("batches:", len(batches))
```

Then realign `frag_*.json` files to match the new batch boundaries (or rename/re-split fragments accordingly).

---

## 6. Files checklist

| Item | Location |
|------|----------|
| English source | `World/Data/Localization/en/scripts-books.json` |
| Shipped Chinese | `World/Data/Localization/zh-Hans/scripts-books.json` |
| Fragments | `World/Source/Tools/scripts_books_zh_fragments/frag_*.json` |
| Batch index | `World/Source/Tools/scripts_books_zh_fragments/_batches.txt` |
| Merge tool | `World/Source/Tools/build_scripts_books_zh.py` |
| Glossary sync tool | `World/Source/Tools/sync_localization_glossary.py` |
| zh-Hans sync rules | `World/Data/Localization/zh-Hans-glossary-sync-rules.json` |
| Editorial guide | `World/Documentation/zh-localization-translation-guide.md` |
| Glossary sync guide | `World/Documentation/zh-localization-glossary-sync-workflow.md` |
| Localization overview | `World/Data/Localization/README.txt` |

---

## 7. Pitfalls

- **Glob order** — Fragment names must sort correctly (`frag_01` … `frag_09`, `frag_10`, …); single-digit-only names would break sort order after nine.
- **Duplicate keys** — The merge script rejects duplicates across files.
- **Dynamic book keys** — `books.dynamic.between_title_author` / `books.dynamic.by_author_line` are short UI chrome; keep width/punctuation consistent with gump layout expectations (e.g. fullwidth space before 著).

# Review: implementation vs. content migration plan

This document answers two audit questions against [`localization-content-migration-plan.md`](./localization-content-migration-plan.md) and [`localization-plan-multiple-languages.md`](./localization-plan-multiple-languages.md).

---

## 1. Has all server-provided text been processed?

**No.** The current pipeline does **not** cover all player-visible server text. It matches the migration plan’s warning that this is a **multi-phase** effort.

### What is covered today

| Mechanism | Coverage |
|-----------|-----------|
| **`SendMessage("literal")`** | Yes — resolved at runtime via hash key → JSON. |
| **`SendAsciiMessage("literal")`** | Yes — same; non-ASCII result uses Unicode packet. |
| **`Say("literal")`** | Yes — **per receiving client** for `PublicOverheadMessage` Unicode path. |
| **Gump strings** | **`AddHtml` / `AddLabel` / text entry initial text** — `Gump.InternLocalized` at compile time (viewer language). |
| **Books (`BaseBook`)** | **Title, author, page lines** — resolved when `BookHeader` / `BookPageDetails` are built. |
| **Quest tree literals** | Extra extraction in `Scripts/.../Quests/` for `builder.Append("...")`, `Title=` / `Description=`, `DummyObjective`, `CollectObjective` names, `TextDefinition("...")`, `AddHtml` verbatim strings, etc. — stored in **`scripts-quests.json`**. |
| **Books tree literals** | Same patterns under **`Scripts/Items/Books`** → **`scripts-books.json`**. |
| **Catalog size** | On the order of **~3.3k+** unique English keys (grows as scanner patterns expand). |

### What is explicitly **not** covered (still English / Cliloc-only)

| Mechanism | Why |
|-----------|-----|
| **`SendMessage(format, args)`** | Format string is not the same as stored English sentence key; not in scanner. |
| **`SendLocalizedMessage(int)`** | Client Cliloc; out of scope for v1 per product decision (plan §1 / gray area). |
| **`Emote` / `Whisper` / `Yell` with string literals** | Not wired in `Mobile.cs` (only `Say` path uses overhead localization). |
| **Gumps outside Quests/Books** | Strings are **localized at send** if present in JSON; extraction currently adds **extra** patterns only under Quests and Books paths (other folders still rely on `SendMessage`/`Say` scan or future scanner expansion). |
| **Books, dynamic quest text, concatenated strings** | Not systematically scanned. |
| **Console / staff-only / debug strings** | Intentionally excluded from “game content” in practice. |

**Conclusion:** The implementation is a **solid first wave** for the three literal patterns above, not completion of “all game content text” from the migration plan.

---

## 2. Are strings split by category (not one monolithic file)?

**Yes, after this revision.**

### Layout

```text
World/Data/Localization/
  en/
    system.json                      # World/Source/System/*.cs
    scripts-system.json              # World/Source/Scripts/System/...
    scripts-items.json               # World/Source/Scripts/Items/...
    scripts-mobiles.json             # World/Source/Scripts/Mobiles/...
    scripts-engines-and-systems.json # World/Source/Scripts/Engines and Systems/...
    scripts-utilities.json           # World/Source/Scripts/Utilities/...
  zh-Hans/
    (same file names as en/)
```

Categories are derived from the **first folder under `Scripts/`** (with spaces → hyphens), plus **`system`** for `Source/System`. This mirrors **repository layout**, not every semantic domain in the plan (e.g. “quests” vs “items” are both under `scripts-engines-and-systems` today). Finer splits (e.g. `quests/`, `items/`) would require **naming rules or path depth** changes in `build_localization_strings.py`.

### Runtime

`StringCatalog` loads **all** `*.json` under `Data/Localization/en` and `Data/Localization/zh-Hans`, merges into one dictionary, and logs duplicate keys if the same hash appears with different text.

Legacy monoliths `strings.en.json` / `strings.zh-Hans.json` are **removed**; if `en/` is empty, the loader still falls back to those paths for backward compatibility.

---

## Lore glossary and translation QA

Automated (heuristic) extraction of recurring **proper nouns / multi-word phrases** and **co-occurrence** from English localization plus quest `typeof()` links:

- `World/Source/Tools/build_lore_glossary.py` → `Data/Localization/lore-glossary.json` and `Documentation/lore-glossary.md`
- `World/Source/Tools/review_translations_glossary.py` → `Documentation/translation-glossary-review.md` (Latin leftovers in zh-Hans, EN phrases still embedded in ZH)
- Optional curator file: `Data/Localization/glossary-approved-zh.json`

This does **not** replace human lore judgment; it speeds **consistent** naming across machine translations.

---

## Recommendations (next steps)

1. **Document “definition of done”** per migration plan §11: CI scanner for remaining `SendMessage("` without matching catalog key (allowlist for formatted calls).
2. **Extend `Mobile`** for `Emote`/`Whisper`/`Yell` string paths if NPC flavor text matters for zh-Hans.
3. **Optional:** second-level categories (e.g. split `scripts-engines-and-systems` by `Quests/`, `Magic/`) in the Python script.
4. **Cliloc policy:** keep Option A (hybrid) or invest in Option B per plan §1 gray area.

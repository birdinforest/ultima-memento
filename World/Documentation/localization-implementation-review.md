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
| **Catalog source** | **~2784** unique string literals from those patterns under `World/Source`. |

### What is explicitly **not** covered (still English / Cliloc-only)

| Mechanism | Why |
|-----------|-----|
| **`SendMessage(format, args)`** | Format string is not the same as stored English sentence key; not in scanner. |
| **`SendLocalizedMessage(int)`** | Client Cliloc; out of scope for v1 per product decision (plan §1 / gray area). |
| **`Emote` / `Whisper` / `Yell` with string literals** | Not wired in `Mobile.cs` (only `Say` path uses overhead localization). |
| **Gumps: `AddHtml`, `AddLabel`, verbatim UI copy** | Plan calls these out; scanner does not extract them. |
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

## Recommendations (next steps)

1. **Document “definition of done”** per migration plan §11: CI scanner for remaining `SendMessage("` without matching catalog key (allowlist for formatted calls).
2. **Extend `Mobile`** for `Emote`/`Whisper`/`Yell` string paths if NPC flavor text matters for zh-Hans.
3. **Optional:** second-level categories (e.g. split `scripts-engines-and-systems` by `Quests/`, `Magic/`) in the Python script.
4. **Cliloc policy:** keep Option A (hybrid) or invest in Option B per plan §1 gray area.

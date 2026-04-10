# Localization plan: English + Chinese (file-backed, hot-reloadable)

This document describes how to add **Chinese (Simplified)** as a second language for **Memento** (RunUO/ServUO-style C# shard under `World/Source`) while keeping **string data in external files**, supporting **in-game language switching without restart**, and **minimizing coupling** so future upstream merges stay tractable.

---

## 1. Goals and non-goals

### Goals

- **Bilingual UX**: English remains the default; Chinese is available as an explicit player choice.
- **Data outside code**: Translatable strings live in **versioned text files** (recommended: **JSON** per language; alternatives below).
- **Hot reload**: Editing translation files or switching language updates what the player sees **without restarting the shard**, within defined limits (see §5).
- **Low merge friction**: Core engine scripts stay mostly untouched; new logic lives in a **small, isolated module** plus data files.

### Non-goals (initial phase)

- Replacing **client Cliloc** strings (UO client UI) — server-side `Say`, gumps, system messages, and custom shard text are in scope first.
- Full coverage of every legacy literal in thousands of scripts in one step — use a **phased** approach (§7).

---

## 2. Recommended architecture (decoupled)

Introduce a single façade used by gameplay code:

| Layer | Responsibility | Upstream coupling |
|--------|----------------|-------------------|
| **`ILocalizationProvider`** (interface) | `Get(string key, params object[] args)` (or keyed overloads) | None — lives in your tree |
| **`JsonLocalizationProvider`** | Loads `Data/Localization/{lang}.json`, caches dictionary, thread-safe reads | None |
| **`Localization`** (static entry) | Delegates to current provider; optional `Reload()` / `SetLanguage` | One thin static used by scripts |
| **Data files** | Keys → strings (and optional metadata) | Zero C# coupling |

**Rule for script authors:** new user-visible English strings use **keys** (`Localization.Get("quest.blacksmith.intro")`) instead of embedding Chinese or English in random scripts. Existing literals migrate gradually.

**Why JSON:** built-in `System.Text.Json` on modern .NET, diff-friendly, easy hot reload, no extra dependencies. **XML** is viable if you prefer tooling your translators already use; **RESX** is compile-time oriented and is weaker for hot reload unless you write a custom loader.

Example file layout:

```text
World/Data/Localization/
  en.json      # default / fallback
  zh-Hans.json # Simplified Chinese
  README.txt   # conventions for contributors (optional)
```

Example `en.json` shape (illustrative):

```json
{
  "commands.language.current": "Your language is set to {0}.",
  "commands.language.changed": "Language changed to {0}."
}
```

Same keys in `zh-Hans.json` with Chinese values. Missing keys fall back to `en.json` and optionally log once per key for translators.

---

## 3. Player language state (in-game switch)

- **Persist per character** (typical): add a field on `PlayerMobile` (or a small `PlayerSettings` component) such as `string LanguageCode` defaulting to `en`.
- **Command or gump**: e.g. `[lang zh]` / `[lang en]` or an options menu entry that sets the code, saves the mobile, and notifies the provider for that session.
- **Session binding**: `Localization` resolves strings using **the speaking mobile’s** language when `Mobile` context exists; for log-only or system-wide messages, define explicit policy (server locale vs. mobile override).

Hot reload of **files** is independent: `Reload()` re-reads JSON from disk for all languages or for one file; active players keep their `LanguageCode` but see updated strings on next `Get`.

---

## 4. Hot reload semantics

| Trigger | Behavior |
|---------|----------|
| Player switches language | In-memory preference changes immediately; no file read required beyond initial load. |
| Operator edits JSON on disk | Expose **`[reloadlang]`** (staff-only) or a **watch timer** (e.g. poll `LastWriteTime` every N seconds in DEBUG only) to call `Reload()`. |
| Malformed JSON on reload | Keep previous cache; log error; optionally notify staff. |

**Limits:** code that captured a translated string at startup will not auto-update until that code path runs again — acceptable if UI is built on demand. Long-lived gumps may need **close/reopen** or explicit refresh hooks; document this for designers.

---

## 5. Minimizing coupling with upstream (Ruins and Riches / ServUO-style cores)

1. **New folder** under something like `Scripts/Localization/` (or `Engines/Localization/`) containing only **new** types — avoid editing `PlayerMobile.cs` until necessary; if you must, keep the diff to **one field + serialization** and no refactors.
2. **Do not** mass-reformat upstream files when touching them.
3. **Prefer wrapper** over forking: e.g. `LocalizedSay(Mobile m, string key)` extension instead of changing every `Say()` in core.
4. **String extraction** for legacy scripts: optional tooling (Roslyn or regex-based) can live in `World/Source/Tools/` — not loaded at runtime — so it does not bloat `World.exe`.
5. **Merge strategy**: keep `Data/Localization/*.json` on a branch or submodule if you want maximum isolation; submodule adds workflow cost — usually a dedicated directory in-repo is enough.

---

## 6. Chinese-specific notes

- Use **`zh-Hans`** as the culture/language tag (BCP 47) for clarity vs. Traditional (`zh-Hant`).
- Ensure **UTF-8** encoding for JSON files (no BOM preferred for cross-platform tooling).
- **Pluralization and gender**: English and Chinese differ; prefer keys that encode full sentences (`"shop.buy.confirm": "Buy {0} for {1} gold?"`) rather than fragile concatenation.
- **Font/client**: confirm the **game client** displays Chinese in journal/system messages; if not, scope may be limited to **Unicode-capable** paths only.

---

## 7. Phased rollout

| Phase | Scope | Risk |
|-------|--------|------|
| **0** | Plan + sample keys + `en`/`zh-Hans` files + provider + staff reload command | Low |
| **1** | Player command + persistence + all **new** features use keys | Low |
| **2** | High-traffic areas (login, help, achievements, main gumps) | Medium |
| **3** | Broader script migration (opportunistic with file edits) | Ongoing |

---

## 8. Testing checklist

- Switch `en` ↔ `zh-Hans` mid-session; journal and targeted gumps show correct language.
- Missing key in `zh-Hans` falls back to `en` without crash.
- Reload with bad JSON does not take down the shard.
- Serialize/deserialize `PlayerMobile` with new field (if added) across restart.

---

## 9. Clarifications needed from maintainers

Please confirm or correct the following so implementation matches product intent:

1. **Scope of “Chinese”**: Simplified only (`zh-Hans`), or also Traditional (`zh-Hant`) later?
2. **Client vs. server**: Must **all** visible text (including Cliloc-dependent tooltips) be Chinese, or is **server-emitted text only** acceptable for v1?
3. **Default for new players**: Always English until chosen, or infer from OS/client (usually not available on shard)?
4. **Who may hot-reload**: Players, all accounts, or **staff only**?
5. **Persistence**: Language preference per **character**, per **account**, or global per installation?
6. **Distribution**: Ship `zh-Hans.json` in the repo, or download/update from a separate pack for modders?
7. **Target .NET / JSON API**: Confirm runtime supports `System.Text.Json` (vs. Newtonsoft) for consistency with the rest of the solution.

---

## 10. Summary

Use a **small localization façade**, **JSON per language under `Data/Localization/`**, **per-player language** with optional **reload command**, and **incremental key adoption** in scripts. That satisfies Chinese-only support for the first wave, file-backed content, hot reload for players/operators, and **minimal surface area** on upstream-sensitive files.

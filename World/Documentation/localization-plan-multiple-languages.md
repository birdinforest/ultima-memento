# Localization plan: English + Chinese (file-backed, hot-reloadable)

This document describes how to add **Chinese (Simplified)** as a second language for **Memento** (RunUO/ServUO-style C# shard under `World/Source`) while keeping **string data in external files**, supporting **in-game language switching without restart**, and **minimizing coupling** so future upstream merges stay tractable.

---

## 1. Goals and non-goals

### Goals

- **Bilingual UX**: **`zh-Hans` only** (no Traditional in scope). The **default language for new accounts** comes from **shard configuration** (e.g. `en` or `zh-Hans`), not from hard-coded English.
- **Data outside code**: Translatable strings live in **versioned text files** (recommended: **JSON** per language; alternatives below). **`zh-Hans.json` (and companion files) are committed in the repository.**
- **Hot reload (player-facing)**: Switching language **in game** takes effect **without shard restart** and **without special permissions** — any player with account access can change their preference (see §4).
- **Low merge friction**: Core engine scripts stay mostly untouched; new logic lives in a **small, isolated module** plus data files.

### Non-goals (initial phase)

- **v1 coverage is server-only**: custom shard strings (`Say`, gumps, system messages, etc.). **Client Cliloc / client tooltips are out of scope** for v1.
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

## 3. Account language state (in-game switch)

- **Persist per account**: store `LanguageCode` (BCP 47, e.g. `en`, `zh-Hans`) on **`Account`** (see `Scripts/System/Misc/Accounts.cs`) — e.g. new serializable field or **`AccountTag`** so all characters on that login share the same language.
- **Initial value**: when an account is first created, set `LanguageCode` from **configuration** (`Localization:DefaultLanguage` or equivalent in existing config style). Do not infer from OS/client.
- **Command or gump**: e.g. `[lang zh-Hans]` / `[lang en]` or an options menu entry that updates the **account**, persists, and applies immediately for the current session (and any concurrent characters on that account, if applicable).
- **Session binding**: resolve strings using the **current `Mobile`’s account** language; for messages with no player context, fall back to **configured default language**.

Reloading **string data from JSON on disk** (operator edits files while shard runs) is optional and separate from player language switching: if implemented, prefer **`Reload()` on a timer**, on **next login**, or an **optional staff command** — product decision not locked for v1.

---

## 4. Hot reload semantics (locked for v1)

| Trigger | Behavior | Permissions |
|---------|----------|-------------|
| Player switches language in game | Account `LanguageCode` updates; subsequent `Localization.Get` for that account uses the new language **without shard restart**. | **None** — not staff-gated. |
| Operator edits JSON on disk (optional) | If supported: `Reload()` refreshes in-memory dictionaries; players keep account language tags. | Define separately if needed (not required for v1 player switching). |
| Malformed JSON on reload | Keep previous cache; log error. | N/A |

**Limits:** code that captured a translated string at startup will not auto-update until that code path runs again — acceptable if UI is built on demand. Long-lived gumps may need **close/reopen** or explicit refresh hooks; document this for designers.

---

## 5. Minimizing coupling with upstream (Ruins and Riches / ServUO-style cores)

1. **New folder** under something like `Scripts/Localization/` (or `Engines/Localization/`) containing only **new** types — prefer extending **`Account`** (or tags) for persistence rather than `PlayerMobile` to reduce coupling and match **per-account** storage.
2. **Do not** mass-reformat upstream files when touching them.
3. **Prefer wrapper** over forking: e.g. `LocalizedSay(Mobile m, string key)` extension instead of changing every `Say()` in core.
4. **String extraction** for legacy scripts: optional tooling (Roslyn or regex-based) can live in `World/Source/Tools/` — not loaded at runtime — so it does not bloat `World.exe`.
5. **Merge strategy**: keep `Data/Localization/*.json` on a branch or submodule if you want maximum isolation; submodule adds workflow cost — usually a dedicated directory in-repo is enough.

---

## 6. Chinese-specific notes

- Use **`zh-Hans`** only (BCP 47); **`zh-Hant` is not in scope.**
- Ensure **UTF-8** encoding for JSON files (no BOM preferred for cross-platform tooling).
- **Pluralization and gender**: English and Chinese differ; prefer keys that encode full sentences (`"shop.buy.confirm": "Buy {0} for {1} gold?"`) rather than fragile concatenation.
- **Font/client**: v1 is **server-emitted text only**; still confirm the client displays Unicode in journal/system messages where the shard sends Chinese.

---

## 7. Phased rollout

| Phase | Scope | Risk |
|-------|--------|------|
| **0** | Plan + sample keys + `en`/`zh-Hans` in repo + provider + config default + account persistence + in-game language switch (no permissions) | Low |
| **1** | Optional on-disk JSON reload for operators + all **new** features use keys | Low |
| **2** | High-traffic areas (login, help, achievements, main gumps) | Medium |
| **3** | Broader script migration (opportunistic with file edits) | Ongoing |

---

## 8. Testing checklist

- New account gets **configured default** language; switching `en` ↔ `zh-Hans` mid-session updates **account** and all characters under that login as designed.
- Journal and targeted gumps show correct language for **server-emitted** strings.
- Missing key in `zh-Hans` falls back to configured fallback (typically `en`) without crash.
- If on-disk reload is implemented: bad JSON does not take down the shard.
- Serialize/deserialize **account** language across restart.

---

## 9. Product decisions (locked)

| Topic | Decision |
|-------|----------|
| Chinese variant | **`zh-Hans` only** (no `zh-Hant`) |
| v1 coverage | **Server-emitted text only** (no client Cliloc/tooltips) |
| Default language | **Shard configuration** defines default for new accounts |
| Hot reload / language switch | **In-game switch without restart; no special permissions** |
| Persistence | **Per account** |
| Shipping translations | **`zh-Hans.json` (and related files) committed in repo** |

**Still to confirm with engineering:** whether to standardize on **`System.Text.Json`** vs **Newtonsoft.Json** for the loader (match existing solution packages).

---

## 10. Summary

Use a **small localization façade**, **JSON per language under `Data/Localization/`** (in repo), **per-account `LanguageCode`** with **config-defined default**, **in-game language switching without restart or privileges**, **server-only** v1 scope (**`zh-Hans` only**), and **incremental key adoption** in scripts. Keep optional **on-disk file reload** separate if operators need live translation edits without redeploy.

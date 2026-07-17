# Ultima Memento — AI Agent Guide

> **Scope:** Game server repo (`ultima-memento`). For the website see `ultima-memento-web/AGENTS.md`. **Cross-repo practice (site media + glossary-driven wiki index):** [§7](#7-website--player-facing-docs-ultima-memento-web).
> **Update protocol:** When you change a convention or discover something that conflicts with this file, propose an edit at the end of your turn. Do not silently diverge.

---

## 0. Quick-Start Index

| Task | Jump to |
|---|---|
| Add a new game feature (C#) | [§2 Engineering Practices](#2-engineering-practices) |
| Add translatable strings to C# (gumps, quests, `SendMessage` / `Say`: shotkeys or hash literals; **refine** hash → shotkey when feasible — §3.2) | [§3.2 Adding Strings](#32-adding-strings-to-cs) |
| Run the localization extractor | [§3.3 Extraction Tool](#33-extraction-tool) |
| Incremental LLM locale queue (`stats` / `queue` / `apply`) | [§3.4 Translation Workflow](#34-translation-workflow--llm-only) |
| Logical-key JSON (shard greeter, war shouts, …) | [§3.1 Architecture](#31-localization-architecture) |
| Translate new strings (LLM, not Google) | [§3.4 Translation Workflow](#34-translation-workflow--llm-only) |
| Update or add glossary terms | [§3.5 Glossary](#35-glossary-management) |
| Website: images/GIFs, wiki index from glossary | [§7 Website (`ultima-memento-web`)](#7-website--player-facing-docs-ultima-memento-web) |
| Build and test the server | [§4 Build & Test](#4-build--test) |
| **Existing save compatibility** (final step before declaring done) | [§4.5](#45-existing-save-compatibility-mandatory-final-review) |
| Localization regression (lightweight host, CI) | [§4.4](#44-localization-regression-lightweight-host) |
| Understand what an AI agent may/must not do | [§5 Boundaries & Verification](#5-agent-boundaries--verification) |
| **Global Shoppe system** (player workshops, customer contracts, bulk orders, rewards) | [`World/Documentation/global-shoppe-system.md`](World/Documentation/global-shoppe-system.md) — architecture, order context types, calculators, shoppe catalog |
| **Generic JSON rate/probability config** (hot-reloadable weights, e.g. dragon Bright-breed rarity, GemDragon scale rarity) | [`World/Documentation/rate-config-system.md`](World/Documentation/rate-config-system.md) — `RateConfigEngine`/`WeightedPick`, `Data/RateConfig/*.json`, `[ratereload]`/`[ratelist]`/`[rateget]` |
| **Avoid server crashes / stability pitfalls** (OPL reentrancy, serializers, timers, …) | [`World/Documentation/server-stability-crash-patterns.md`](World/Documentation/server-stability-crash-patterns.md) — **read before** touching `Item`/`Mobile` OPL paths, `Serialize`/`Deserialize`, or tick/timer-heavy logic |
| **Cross-repo design/mechanics documentation index** | [§8 Design & Analysis Documentation](#8-design--analysis-documentation-uo-dev-documentations) — read before codebase search |
| **NPC dialogue intelligence mechanisms** | `<UO_DEV_DOCS_ROOT>/memento/game-mechanism/NPC_INTELLIGENCE_DIALOGUE_MECHANISM.md` (EN) / `NPC对话情报机制分析.md` (ZH) — refresh frequencies, file references |

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

**Cross-repo documentation root:**

- `UO_DEV_DOCS_ROOT` = `/Users/forrrest/projects/uo-dev/uo-dev-documentations`
  Memento shard design docs, game mechanism analysis, dev logs, quest design.
  When a path in this file starts with `UO_DEV_DOCS_ROOT/`, resolve it against that absolute path.
  See [§8](#8-design--analysis-documentation-uo-dev-documentations) for the indexed document list.

**Key documentation to read before working on each domain (inside this repo):**

- Localization: `World/Data/Localization/README.txt` — authoritative layout and commands.
- Book translation: `World/Documentation/scripts-books-zh-translation-workflow.md`
- Glossary sync: `World/Documentation/zh-localization-glossary-sync-workflow.md`
- Translation editorial rules: `World/Documentation/zh-localization-translation-guide.md`
- Coverage roadmap: `World/Documentation/localization-complete-coverage-roadmap.md`
- Localization regression testing plan & test tiers: `World/Documentation/localization-regression-testing.md`
- Craft tiers, harvest definitions, `CraftResource` tables: `World/Documentation/resources-design/README.md`
- Castle of Knowledge (Lodor landmark, Power Scroll vendors): `World/Documentation/castle-of-knowledge.md`
- **Server stability & crash avoidance (AI checklist):** `World/Documentation/server-stability-crash-patterns.md` — OPL / `InvalidateProperties` reentrancy, serializers, null-safety, timers, collection mutation, etc.
- **Generic JSON rate/probability config engine:** `World/Documentation/rate-config-system.md` — `RateConfigEngine`/`WeightedPick` under `World/Source/System/RateConfig/`, `Data/RateConfig/*.json`, GM hot-reload commands; read before adding any new rate/weight/probability table (dragon Bright rarity, GemDragon scale rarity are its first consumers).

**Design & analysis docs (cross-repo, under UO_DEV_DOCS_ROOT):**

- NPC dialogue intelligence systems: `<UO_DEV_DOCS_ROOT>/memento/game-mechanism/NPC_INTELLIGENCE_DIALOGUE_MECHANISM.md` (EN) / `NPC对话情报机制分析.md` (ZH) — refresh frequencies, file references, architecture.
- NPC economy & vendor mechanisms: `<UO_DEV_DOCS_ROOT>/memento/game-mechanism/npc-game-mechanisms.md` — vendor buy/sell, black market, services, shoppe.
- Broader index: [§8](#8-design--analysis-documentation-uo-dev-documentations).

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

1. **Read at least one analogous existing implementation** before starting. Pattern-match to the existing architecture. **Also check [§8](#8-design--analysis-documentation-uo-dev-documentations) for any existing analysis doc under `UO_DEV_DOCS_ROOT` that covers the domain — read it before searching the codebase.**
2. **Plan before writing:** For features touching more than two files, state your approach in plain language first.
3. **Localization from day one:** Any user-visible text added to C# must go through `StringCatalog` (see §3.2). Never defer localization.
4. **Quests:** Follow `World/Source/Scripts/Engines and Systems/Quests/Core/` patterns. New quest types subclass existing base classes.
5. **Items/Books:** Follow `World/Source/Scripts/Items/Books/` patterns for book text extraction compatibility.

### 2.3 File Naming

- C# files: `PascalCase.cs`, matching the primary public class name.
- Python tools: `snake_case.py`.
- Localization JSON: `<category>.json` (see §3.1 for category names).

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
| `resource-harvest-extra.json` | Hash-key harvest / craft-material copy (`CraftResources` shorts, gem/bark/mushroom bonus strings, harvest quantity **some**, `You found {0}!`, grave chest, …). Pair `en/` + `zh-Hans/`; **`build_localization_strings.py` `keep_extra`**. See `World/Documentation/resources-design/07-localization-and-player-copy.md`. |
| `equipment-properties.json` | Equipment / weapon OPL shotkeys（`prop.*`）、**物品 OPL 主名（`item.*`，与 `prop.*` 同文件）**、以及绑在同一批物品上的玩家提示（如 `prop.magical.moonstone.gate.inert`）。Pair `en/` + `zh-Hans/`；**`build_localization_strings.py` `keep_extra`**。主名流水线见 `World/Documentation/waiting-localization.md` §「物品 OPL 主显示名」。 |
| `legend-book-rows.json` | `LegendsBook` / `ManualOfItems` 图鉴 Gump 列表行（`god.legendbook.row.001` …）：**仅 `zh-Hans/`** 维护中文行；英文运行时回退为 C# 表内嵌英文；**`keep_extra`**。 |
| `trade-commodity.json` | 贸易物资双语 OPL：`trade.suffix.*`（矿石/锭/板等词尾）、`trade.compose.material_suffix`、`trade.custom.*`（马铠/望远镜/十尺杆/装订）、`trade.keg.potion.*`（药剂桶）、`placemap.name.format`（与 `placemap-labels.json` 地名哈希配合）。Pair `en/` + `zh-Hans/`；**`keep_extra`**。 |
| `placemap-labels.json` | `Worlds.GetAreaEntrance` / `GetTown` 返回的英文地名 → 哈希键；`PlaceMap` 在 OPL 中按账号语言解析。Pair `en/` + `zh-Hans/`；**`keep_extra`**。 |
| `decoration-sign-labels.json` | `Data/Decoration/*.cfg` 中 `Static … Name=` 门牌/店招英文字符串 → 哈希键；`Static.AddNameProperty` 经 `StringCatalog.TryResolve` 解析。Pair `en/` + `zh-Hans/`；**`keep_extra`**。 |
| `chat3-ui.json` | Knives.Chat3 UI：`chat3.000`…`chat3.294`（与 `DefaultLocal.Load()` 顺序一致）由 `General.LocalFor` / `General.Local`（shard 默认语言）经 `StringCatalog.TryResolveByKey` 解析；`DefaultLocal` 为缺失键时的回退。Pair `en/` + `zh-Hans/`；**`keep_extra`**。 |
| `mob-loot-infotext.json` | Boss 战利品 `InfoText1`–`InfoText5` 的 OPL：已知英文行用哈希键（`s.*`，与 `StringCatalog.TryResolve` 一致）+ `mob.loot.infotext.champion.belonged` 模板（`BaseChampion` `[Belonged to: {0}]`）；运行时写入的其它字符串回退为存档原文。Pair `en/` + `zh-Hans/`；**`keep_extra`**。 |
| `world-player-text.json` | 从「任务 / 系统帮助 / 引擎物品描述 / 书籍默认说明」等迁移出来的 **shotkey** 文案（`quest.*`、`eng.*`、`sys.*`、`mob.*`、`book.*`）；C# 使用 `StringCatalog.ResolveByKey` / `ResolveFormatByKey`。Pair `en/` + `zh-Hans/`；**`keep_extra`**。由 `World/Source/Tools/build_world_player_text_from_queue.py`（自 `llm-queue-*.jsonl` 生成键）与 `patch_cs_resolve_to_shotkeys.py` 批量替换辅助维护。 |
| `trap-system.json` | HiddenTrap subsystem logical keys: 25 trap-type trigger messages, proximity/perception strings, trap item names, avoidance/removal messages, direction/distance descriptors, SpellTrap/SetTrap/TrapKit/TenFootPole/TrapWand copy, CurseItem tooltip, base-trap detection suffixes. Uses `StringCatalog.ResolveByKey` / `ResolveFormatByKey` in C#. |
| `charrestore.json` | Character Item Restore system (`Scripts/Engines and Systems/CharacterRestore/` + `Scripts/Mobiles/Civilized/Special/LostItemsRestorerNPC.cs`): NPC speech (`charrestore.npc.*`), three-stage dialog gump titles/body/buttons (`charrestore.dialog.*`), GM gump labels/buttons/messages (`charrestore.gump.*`). Uses `StringCatalog.TryResolveByKey` via `CitizenLocalization.SayLocalizedByKey` and inline helpers. |
| `avatar-system.json` | Avatar's Ascent subsystem (`Scripts/Engines and Systems/Avatar/`): `avatar-enable` / `avatar-shop` commands, confirmation gumps, `AvatarShopGump` UI, ascension/template/reward copy, rival faction names, `AvatarBook` / `SafetyDepositBox` OPL names (`avatar.book.*`, `avatar.item.*`, `avatar.msg.*`, `avatar.gump.*`, `avatar.reward.*`). C# via `AvatarLocalization.Key` / `KeyFormat` / `Send` + `StringCatalog.TryResolveByKey`. Pair `en/` + `zh-Hans/`; **`keep_extra`**. |

Other non-category locale files (also whitelisted, not scanner-owned): `vendor_npc_speech.json` (see `World/Source/Tools/` vendor speech scripts). Authoritative notes: `World/Data/Localization/README.txt`.

**Key management:**
- Hash keys: `s.` + 16 hex chars (SHA-256 of the exact EN string). These are stable as long as the English text is unchanged.
- Logical keys (e.g. `books.dynamic.*`): preserved manually across re-extraction runs.
- The runtime merges **all `*.json`** under `en/` and `zh-Hans/` at startup.

**Resolution:** `StringCatalog.TryResolve(key, lang)` → falls back to `en` if `zh-Hans` key is missing.

### 3.2 Adding Strings to C#

**Rule: Every user-visible literal must be localized. No exceptions.**

Use the `StringCatalog`-aware APIs that the extractor already handles.

**Tinted `SendMessage` (`SendMessage(int hue, string text)`):** The first argument is only a **client hue** (e.g. **68** for a green “success/info” tint). **The `text` argument must still be localized** — never concatenate raw English with runtime values for zh-Hans accounts. Prefer **`StringCatalog.ResolveFormatByKey`** plus **per‑value shotkeys** so **Chinese sentence order** stays natural (template with `{0}` for the variable fragment).

**OPL tooltip lines (`ObjectPropertyList`):** The **same rule applies** as for tinted `SendMessage`: a **cliloc slot number or `list.Add` overload does not localize text**. Chinese grammar and word order differ — avoid passing zh-Hans accounts raw English composition.

Anti-pattern:

```csharp
list.Add(1049644, m_Count.ToString() + " Songs");    // WRONG — locale-unfriendly word order
list.Add(1049644, "Contains: " + gold + " Gold");    // WRONG — concatenation not locale-safe
list.Add("Drag onto Paperdoll");                     // WRONG — hardcoded literal
```

Prefer bilingual gate + legacy fallback:

```csharp
if (BuildingPropertyListLocale != null)
    AddLocalizedProperty(list, "prop.songbook.count", m_Count); // shotkey in logical JSON
else
    list.Add(1049644, m_Count.ToString() + " Songs");
```

For **`zh-Hans`**, use a template such as **`含 {0} 首歌曲`** (not **`{0} 歌曲`** alone). **`prop.*`** / **`AddLocalizedProperty`** / **`ResolveFormatByKey`** with **`equipment-properties.json`** (or domain **`keep_extra`** bundle). Audit patterns: **`World/Documentation/waiting-localization.md`** §9.

**Player/system messages (`SendMessage`, `Say`, etc.):** the bare form `mobile.SendMessage("...")` is **not** localized. Use either:

1. **Shotkeys (preferred):** stable logical keys in hand-maintained JSON (`equipment-properties.json`, `trap-system.json`, `charrestore.json`, … — see §3.1 `keep_extra` table). **Do not** rely on the C# extractor for these; add the same key to **`en/<file>.json` and `zh-Hans/<file>.json`**.
   ```csharp
   mobile.SendMessage(StringCatalog.ResolveByKey(mobile.Account, "prop.magical.moonstone.gate.inert"));
   // Templates: StringCatalog.ResolveFormatByKey(mobile.Account, "some.key", arg0);
   ```

2. **Hash keys (extractor):** English literal inside `StringCatalog.Resolve` / `ResolveFormat` so `build_localization_strings.py` emits `s.` + hex keys into the correct `scripts-*.json` category.
   ```csharp
   mobile.SendMessage(StringCatalog.Resolve(mobile.Account, "Your one-off message here."));
   ```

**OPL 物品主显示名（`AddNameProperty`）：** 第一行物品名在 **`BuildingPropertyListLocale != null`** 时用 **`AddLocalizedProperty(list, "item....")`** 或 **`ResolvePropertyText`** + `list.Add` / `1050039`（叠堆）。键名建议 **`item.special.*` / `item.magical.*`**，与 **`prop.*`**（属性行、`SendMessage` 键）分前缀，**仍在 `equipment-properties.json`**。类须 **`IsContentLocalized => true`**。完整步骤见 **`World/Documentation/waiting-localization.md`** §「物品 OPL 主显示名」。

**OPL 扩展：第三条彩色属性行（`AddColorText3Property`）：** `Item.AddNameProperties` 在固定顺序内调用 **`AddColorText3Property(ObjectPropertyList list, string colorHue3)`**（对应客户端 **1072173** / 原 **`ColorText3`** 槽位）。默认实现仅在 **`ColorText3 != null`** 时输出。若该行需要随 **`BuildingPropertyListLocale`** 用 **`ResolvePropertyText("prop.*")`**、**`StringCatalog.ResolveFormatByKey`** 或 **`string.Format`** 生成（例如动态金币数），请 **override `AddColorText3Property`**，在同一 OPL 位置写入文案，并避免把**仅单语**或**易过期**的成品字符串长期存进 **`ColorText3`**（世界存档里若已有旧值，可在子类 **`Deserialize` 末尾**将 **`ColorText3 = null`** 清一次）。参考：`World/Source/Scripts/Items/Trades/Tinkering/Clocks.cs`（**`DDRelicClockBase`** + **`prop.trade.relicclock.worth`**）。

```csharp
using Server.Localization;

// … prefer ResolveByKey + logical JSON when the string is stable / domain-owned (§3.1 shotkey bundles)

// Gumps
AddLabel(x, y, hue, "Label text");
AddHtml(x, y, w, h, "Html content", false, false);
AddTooltip("Tooltip text");

// Quest objectives / text blocks
new TextDefinition("Objective description")
```

**Do not** pass variables as the string argument **to `Resolve`** if the content must be localized; the extractor only captures string literals. (`ResolveByKey` always uses a string literal key — fine.)

**AI localization workflow (`SendMessage` / item feedback, shotkey path):** (1) Pick a stable dot-key (e.g. `prop.magical.*` next to existing OPL keys in `equipment-properties.json`, or `trap.*` in `trap-system.json`). (2) Add EN + zh-Hans entries in the paired logical JSON files. (3) Call `SendMessage(StringCatalog.ResolveByKey(from.Account, "your.key"))` and `using Server.Localization;`. (4) Run `sync_localization_glossary.py --check` if copy touches glossary terms. See `World/Documentation/waiting-localization.md` §「SendMessage 与中文字符串」与「任务：硬编码 SendMessage / Say 全库清查」.

**AI workflow (hash path):** `Resolve` literal → `build_localization_strings.py --no-translate` → §3.4 translate new `s.` line → `sync_localization_glossary.py --check`.

For **any** new `Resolve` literals in C#, run the extractor (§3.3) before committing. Logical shotkey files are **not** populated by the extractor.

**Hash → shotkey refinement (AI / human review):** If a string (or its Chinese) already lives under **hash keys** (`s.` + hex in `scripts-*.json` / category JSON from the extractor), **when feasible** prefer **migrating to a stable shotkey** in an appropriate logical bundle (`equipment-properties.json`, `trap-system.json`, … — see §3.1 `keep_extra`). Steps: add the same **EN + zh-Hans** lines under a new dot-key; change C# to `ResolveByKey` / `ResolveFormatByKey` / `AddLocalizedProperty` / `ResolvePropertyText` as for other shotkeys; remove or stop using the obsolete `s.*` rows once verified (re-run extractor/`--fail-on-translated-zh-drop` if needed to confirm no regressions). **Do not migrate** when there is a **material risk**: e.g. the line is intentionally **extractor-owned** high-churn one-off copy; **shared hash** dedupes identical EN literals and migration would **fork** maintenance; resolution path requires **`TryResolve(english)`** with **non-literal** or runtime-composed text; **regression golden cases**, **external tools**, or **docs** still assume the hash id; or the correct **`keep_extra`** target or **PropertyColorMap** / OPL wiring is unclear. In those cases **keep the hash-based entries** and **explain in your reply** why migration was skipped or deferred.

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

For proper nouns NOT in the glossary: use inline `中文（English）` on first use — do not wrap in `【】`.
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

### 3.5 Proper Noun Annotation Convention

**Rule:** In zh-Hans localization files, every proper noun (place, person, creature, item, faction, deity, dungeon, race) must be annotated with its English original using inline `中文（English）` only. Do not wrap proper nouns in `【】`; when editing existing lines, strip outer `【】` around names and keep the parenthetical English.

This applies to all zh-Hans translation files regardless of context type (scripts-books, engines, quests, items, mobiles, commontalk-fragment-zh, vendor_npc_speech, etc.):

| Context | Format | Example |
|---|---|---|
| All player-facing zh-Hans strings | `中文（English）` inline | `蒙丹（Mondain）打开了通往异界的门。` / `莫瑞尼亚矿坑（Mines of Morinia）出产上佳矿石。` |

**When to annotate:**
- **All proper noun categories:** place, character, creature, item, deity, faction, dungeon, race.
- **Skip:** concept, system, skill, title, book — these are functional descriptors, not named entities.
- **Skip:** segments that are already fully English in the source string — keep English as-is; no forced Chinese wrapper.
- **Skip:** text already annotated as `中文（English）` — no double-annotation.

**Tool support:** `World/Source/Tools/annotate_proper_nouns.py`
- `python3 World/Source/Tools/annotate_proper_nouns.py --dry-run` — preview new annotations
- `python3 World/Source/Tools/annotate_proper_nouns.py` — apply annotations

The tool scans all `zh-Hans/*.json` strings: removes outer `【】` around proper names, converts unannotated `【中文】` to `中文（English）` via the glossary, converts legacy `【English】` (Latin-only) to `（English）`, and leaves `【{0}】`-style placeholders as `{0}`. **`commontalk-fragment-zh.json`** still uses the curated `COMMONTALK_ANNOTATIONS` map (word-boundary match on English keys).

`sync_localization_glossary.py` **normalize_brackets** applies the same idea to any remaining `【…】` spans: English inside brackets becomes `中文（English）`; Chinese variant swaps lose the brackets.

**Translation workflow for new strings:**
When writing new zh-Hans translations:
1. Identify all proper nouns in the English source text.
2. Look up each term in `glossary-approved-zh.json` (`terms` section).
3. Annotate at first occurrence with inline `中文（English）` everywhere — e.g. `索沙尼亚（Sosaria）` (no `【】`).
4. If the English term is not in the glossary, propose an entry via the glossary management process (§3.6).
5. Run `sync_localization_glossary.py` and `--check` to verify consistency.
6. If adding new EN-glossary proper nouns that appear in commontalk English keys, extend `COMMONTALK_ANNOTATIONS` in the annotation script.

**Limitation:** Inline annotation without English-key validation (e.g. `vendor_npc_speech.json`) is not automated because short Chinese glossary terms (2–3 chars like `恶魔`, `宝箱`) cause false positives. Such files should be annotated manually or by extending the curated mapping.

---

### 3.6 Glossary Management

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

### 3.7 Localization Checklist for Any PR

Before finalizing any change that touches C# user-visible strings:

- [ ] Ran `build_localization_strings.py --no-translate`
- [ ] New EN keys appear in correct category JSON
- [ ] For new zh-Hans work: used `llm_incremental_locale.py stats` / `queue` (delta only) before LLM, then `apply` — not Google/DeepL
- [ ] New ZH translations follow LLM policy (§3.4) — not Google/DeepL
- [ ] Glossary terms used correctly (`sync_localization_glossary.py --check` exits 0); if you touched `resource-harvest-extra.json`, apply bracket wholesale sync first (`sync_localization_glossary.py` without `--check`) until `--check` exits 0
- [ ] Harvest/resource **`StringCatalog`** work: extend **golden `string_catalog_only`** cases under `World/Data/Localization/regression/cases/` for representative English lines (bonus gem phrases, `You dig/chop/find…` templates, quantity **some**, a material short such as **Iron**)
- [ ] Dynamic zh pipelines (`CommonTalkDynamicZh`, `QuestCompositeResolver`, overhead) **and harvest–resource catalog regressions**: `bash World/Source/Tools/run_localization_regression.sh` exits 0 (after compile)

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
| Localization dynamic pipelines (tavern/composite/overhead) | After the lightweight host is implemented: regression suite passes in CI (see §4.4). |
| Any C# change touching `Item`/`Mobile`, serialization, OPL, gumps, or persisted player state | [§4.5](#45-existing-save-compatibility-mandatory-final-review) save-compatibility review (mandatory final step) |

### 4.4 Localization regression (lightweight host)

**Implementation:** After `LocalizationBootstrap.Initialize()`, run:

```bash
bash World/Source/Tools/run_localization_regression.sh
```

(from repo root; requires `World/WorldLinux.exe`). Equivalent: `cd World && mono WorldLinux.exe -localization-regression` (alias `-locreg`). Exit **1** on mismatch; failures also listed in `World/Data/Localization/tools-output/localization-regression-report.json` (gitignored).

**Trade-off:** The hook runs after **`World.Load()`** in `Main` today — CI is correct but startup is **slow**; Phase 2 may skip world load (see [`World/Documentation/localization-regression-testing.md`](World/Documentation/localization-regression-testing.md)).

**Chosen model (target):** Golden-case checks for dynamic zh (minimal long-lived server; early `Environment.Exit`). **Not** “compiler-only” tests.

**Authoritative detail** (pipelines, `Data/Localization/regression/cases/`, T0–T3 test-tier framework): [`World/Documentation/localization-regression-testing.md`](World/Documentation/localization-regression-testing.md).

**Implemented** — run the command above after changing `CommonTalkDynamicZh`, `QuestCompositeResolver`, `NpcSpeechTokenZh`, **`resource-harvest-extra.json`**, harvest-related **`StringCatalog`** English literals, or related data.

### 4.5 Existing save compatibility (mandatory final review)

> **When:** After every implementation or recommendation that touches server C# (`Item`/`Mobile`/`CraftSystem`/gumps), **`Serialize`/`Deserialize`**, OPL, timers, or anything that could affect **`World/Saves/`** at load time. **Skip** for docs-only, localization JSON-only, or `ultima-memento-web` work with no server code impact — state **N/A** explicitly in self-report (§5.4).
>
> **Role:** This is the **last verification step** before declaring a task complete or handing off a plan. Agents must run this review themselves and **summarize the outcome** in the final reply (compatible / risks noted / could not verify).

**Do not edit `World/Saves/`** to test. Review is read-only against code paths and, when possible, a normal server startup against the developer’s existing save tree.

#### 4.5.1 Serialization contract

- [ ] **`Serialize`/`Deserialize` version** unchanged, **or** version incremented with a branch that reads **all** bytes written by older saves (no stream misalignment).
- [ ] **No new persisted fields** without a version bump and read path for old saves; **or** new data is provably runtime-only (not written in `Serialize`).
- [ ] **Field order** in `Deserialize` still matches what older saves wrote.

#### 4.5.2 Display-only vs on-disk state

- [ ] **`DisplayNameLocalizationKey`**, shotkeys, `StringCatalog`, gump label keys, and similar are **display-layer only** unless you also changed what is stored in `Name`, `InfoText*`, custom `m_*` fields, or addon/component references.
- [ ] **In-memory-only state** (e.g. `CraftContext`, craft menu strings resolved at draw time) is not assumed to exist on disk from before the change.
- [ ] If code **reads English literals from saved `Name`/`InfoText`** (e.g. `StartsWith("painting of ")`), old saves still match; new logic does not require reserializing existing items.

#### 4.5.3 Load-time behavior & stability

- [ ] **`Deserialize` does not assume** the world is fully loaded unless guarded with `World.Loading` (or the same pattern as neighboring types).
- [ ] **OPL paths** (`GetProperties`, `AddNameProperties`, `AddColorText*Property`) do not assign **`ColorText*` setters** during property-list build (see [`server-stability-crash-patterns.md`](World/Documentation/server-stability-crash-patterns.md) §1).
- [ ] **Null-safe paths** for `Mobile`/`Item`/`Map`/`Account`/`NetState` on loaded or opened UI (gumps, `LabelTo`, localization resolve).
- [ ] **Enum/int fields from saves**: invalid or legacy values handled (default branch, clamp, or explicit guard) — no unhandled cast assumptions.
- [ ] **Localization fallbacks**: missing keys or null `Account` fall back to English/key text without throwing (see `StringCatalog.ResolveByKey`, `CraftDisplayLocale`-style pass-through for non-keys).

#### 4.5.4 Runtime verification (when feasible)

- [ ] **Compile** succeeds.
- [ ] **Start server** against existing saves (or confirm `World/server-start.log` / stdout shows `Game: Loading...` completes with **no** deserialize/exception spam for the touched types).
- [ ] If startup cannot be run, state **“save compatibility: not runtime-verified”** and list what was checked statically.

#### 4.5.5 Report template (include in §5.4 self-report)

Use a short block such as:

```text
Save compatibility: [Compatible | Risk noted | N/A | Not runtime-verified]
- Serialization: …
- On-disk fields: …
- Load/OPL/null paths: …
- Startup test: …
```

---

## 5. Agent Boundaries & Verification

### 5.1 Hard Boundaries — Never Do These

- **Stability:** Before changing **OPL construction** (`GetProperties`, `AddNameProperties`, `AddColorText*Property`), **serialization**, or **timer/tick-heavy** code paths, read [`World/Documentation/server-stability-crash-patterns.md`](World/Documentation/server-stability-crash-patterns.md) and avoid the documented anti-patterns (especially **`ColorText*` setters during property list build**). **Also check §8 for any domain-appropriate analysis doc under `UO_DEV_DOCS_ROOT` before searching the codebase.**
- **Never edit `World/Saves/`** (accounts, items, mobiles). These are live runtime state.
- **Never translate using Google Translate or DeepL APIs.** LLM-based translation only (§3.4).
- **Never add a glossary entry without human review** unless the user has explicitly approved the term in this session.
- **Never commit binary files** (`*.bin`, `*.idx`, `*.tdb`, `*.exe`, `*.dll`, compiled `*.pyc`).
- **Never modify the extraction regex patterns** in `build_localization_strings.py` without stating the change and its impact first.
- **Never hardcode user-visible strings in C#.** Always use the catalog.
- **Never modify a localization regression test case (`expectedZh`) solely to make a test pass.** The correct fix is to ensure the localization chain produces the correct Chinese output. If a test case modification is genuinely warranted (e.g. the expected value was incorrectly authored from the start), state the justification explicitly in your reply.

### 5.2 Required Verification Steps

Before declaring a task complete, you must verify:

1. **Code compiles.** If you cannot run the compiler, state this explicitly.
2. **Localization extractor runs without error** (if C# strings were added/changed).
3. **Glossary sync check passes** (if any ZH files were modified).
4. **No unintended files modified.** Run `git diff --name-only` and confirm the list matches your intent.
5. **Existing save compatibility** — complete [§4.5](#45-existing-save-compatibility-mandatory-final-review) as the **final** step (after compile/localization checks). Include the §4.5.5 summary in self-report (§5.4). Pure docs / locale-JSON / web-only changes: mark **N/A** with one-line reason.

### 5.3 When to Pause and Ask

Pause and ask the user before proceeding when:

- A task would modify more than 5 files outside of the primary stated scope.
- A glossary term conflict is discovered (existing `canonical` contradicts a new string).
- A C# compile error originates in a file you did not modify (may indicate pre-existing breakage).
- Translation of a culturally sensitive or lore-critical term has no obvious correct answer.
- The extraction tool output looks wrong (key counts change unexpectedly, categories mismatch).

### 5.4 Self-Reporting

At the end of every substantial task, report:
- Files modified (list them).
- Verification steps completed (and their outcome).
- **Save compatibility** (§4.5.5 block) when server C# or serialization was in scope; otherwise **N/A**.
- Any deferred items or open questions.
- Any discovered conventions that contradict this guide (propose an update).

---

## 6. Scaling & Updating This Guide

### 6.1 When to Update This File

Update `AGENTS.md` when:
- A new localization language is added beyond `en` / `zh-Hans`.
- A new **logical-key JSON** bundle is added under `Data/Localization/en/` (and `zh-Hans/`) — update §3.1 table and `keep_extra` in `build_localization_strings.py`.
- A new Python tool is added to `World/Source/Tools/`.
- A new shell helper under `World/Source/Tools/` is added (e.g. `run_localization_regression.sh`).
- A new source directory category is added under `World/Source/Scripts/`.
- A build or test process changes (including localization regression host invocation or test tiers).
- Cross-repo website conventions change (§7: media paths, wiki index pipeline, glossary inputs).
- An AI agent discovers a recurring mistake pattern (add it to §5.1 or §5.2).
- A new **authoritative design pack** is added under `World/Documentation/` that agents should routinely consult (index it in §1 bullet list and here).
- **Stability / crash-pattern** guidance changes (`server-stability-crash-patterns.md`) — keep §0 index, §1 bullets, and §5.1 in sync.
- **Save compatibility review** process changes — keep §0, §4.3, §4.5, §5.2, and §5.4 in sync.

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
- 2026-04-30: §1 / §6.1 — design pack index for **`CraftResource`** / harvest definitions: `World/Documentation/resources-design/README.md` (+ linked split docs).
- 2026-04-18: Initial version created. Covers C# practices, localization pipeline, LLM translation policy, agent boundaries.
- 2026-04-18: Added §7 — cross-repo practice standard for `ultima-memento-web` (media vendoring, glossary-driven wiki index).
- 2026-04-29: §3.1 — documented hand-maintained logical-key JSON files and `keep_extra` contract; §6.1 — update trigger for new bundles.
- 2026-04-29: §3.3 — `build_localization_strings.py` defaults to **not** pruning extra locale JSON; drop-report + `--fail-on-translated-zh-drop`; `SendMessage`/GreeterKey extractor fix documented in `README.txt`.
- 2026-04-29: §3.4 + README — `llm_incremental_locale.py` (`stats` / `queue` / `split-queue` / `apply`) for token-efficient incremental LLM translation.
- 2026-05-03: §3.5 — new **Proper Noun Annotation Convention** for zh-Hans: all proper nouns must show `中文（English）` format in 【】 brackets or inline; `annotate_proper_nouns.py` tool for automated annotation.
- 2026-05-17: §3.2 — **Hash → shotkey refinement**: when feasible, migrate existing `s.*` localized strings to stable shotkeys in `keep_extra` bundles; skip and explain when risky.
- 2026-05-17: §3.2 / §0 / §3.1 `equipment-properties` — **`SendMessage` / `Say` shotkeys** (`StringCatalog.ResolveByKey` + logical JSON) preferred over hash `Resolve` literals when a stable `keep_extra` bundle fits; `waiting-localization.md` — shotkey-first pipeline + **任务：硬编码 SendMessage / Say 全库清查**；`MoonStone` 使用 `prop.magical.moonstone.gate.inert`.
- 2026-05-17: §3.5 — **no `【】` wrapping** for proper nouns; inline `中文（English）` only; strip legacy `【】` when editing; LLM template and tool notes updated accordingly. `annotate_proper_nouns.py` migrates all `zh-Hans/*.json`; `sync_localization_glossary.normalize_brackets` emits inline forms; locale data migrated.
- 2026-05-15: §3.1 — added `charrestore.json` logical-key bundle for the Character Item Restore system (NPC dialog + GM gump); `CitizenLocalization.SayLocalizedByKey` added for shortkey-based NPC speech broadcast.
- 2026-05-16: §1 — indexed `World/Documentation/castle-of-knowledge.md` (Lodor Castle of Knowledge + Power Scroll merchants).
- 2026-05-17: §3.1 — `legend-book-rows.json`（`god.legendbook.row.*`，zh-Hans-only）+ `keep_extra`；§3.2 — **`SendMessage(int hue, string)`** 仍须目录化，hue 仅客户端着色。
- 2026-06-30: §3.1 — `decoration-sign-labels.json`（`keep_extra`）：装饰物 `Static` 门牌 `Name=` 哈希双语 OPL。
- 2026-05-17: §3.1 — `trade-commodity.json`、`placemap-labels.json`（`keep_extra`）：动态材料全名、`PotionKeg` 桶名、`PlaceMap` 地名 OPL。
- 2026-05-17: §3.2 — **`Item.AddColorText3Property`**：OPL 第三条彩色行（1072173）的可覆盖扩展点；用于 `ResolvePropertyText` / 格式化双语估价等，替代依赖 **`ColorText3`** 存英文。
- 2026-05-18: §3.2 — **`ObjectPropertyList` / `list.Add`**：cliloc 槽或未保护的英文字符串、以及与变量的英文拼接（如 **`count + " Songs"`**）须 **`BuildingPropertyListLocale`** 分支下 **`AddLocalizedProperty` / `ResolveFormatByKey`**（与 tinted **`SendMessage(int hue, string)`** 同约束）；§3.2 增补反例/正例代码块与 **`含 {0} 首歌曲`** 模板说明；稽核模式见 **`World/Documentation/waiting-localization.md`** §9。
- 2026-05-18: §0 / §1 / §5.1 / §6.1 — [`server-stability-crash-patterns.md`](World/Documentation/server-stability-crash-patterns.md)：常见崩溃模式与 Agent 检查清单（OPL 重入、序列化、定时器等）。
- 2026-05-20: §3.1 — `mob-loot-infotext.json`（`keep_extra`）：Boss 战利品与冠军掉落 `InfoText` OPL 双语；`Item` 内 `ResolveInfoTextForPropertyList` 使用哈希 `TryResolve` + `mob.loot.infotext.champion.belonged` 模板。
- 2026-07-09: §8.1 — indexed `EQUIPMENT_ENHANCEMENT_SYSTEM.md`（装备强化：Enhance Item / 公会 / 磨刀石 / 转化药水）。
- 2026-07-09: §8.1 — indexed `EQUIPMENT_BREAKDOWN_SYSTEM.md`（装备分解：Break Down / Scissors、`BaseItemBreakDown`、容器批量分解）。
- 2026-07-06: §8.1 — indexed `TREASURE_HOARD_SYSTEM.md`（宝藏堆：HoardTile 锚点、Fame≥15000 击杀触发、HoardPiles 战利品表、Hoard Minion）.
- 2026-06-28: §8.1 — indexed `PVP_COMBAT_SYSTEM.md` (guild-gated PvP, attack pipeline, notoriety/murder, region matrix).
- 2026-07-05: §8.1 — indexed `BLOOD_TEMPLE_SYSTEM.md`（惧怖群岛鲜血神殿：双地图、刷怪、魔法监狱 #44、Search area 50）.
- 2026-07-03: §1 — indexed `RESEARCH_BAG_SYSTEM.md`（ResearchBag 初始化、字段、NPC 发放、GM/Avatar 死亡测试基线）；§8.1 交叉索引。
- 2026-07-03: §8.1 — indexed `RELICS_DROP_REFORM_TOP3_DAMAGE_SYSTEM.md`（Feature Request：Gate A/B/C Top-3 伤害贡献制掉落改革，首次/Repeat 双概率，OSI Champ 基准评估与修改意见）。
- 2026-07-03: §8.1 — indexed `DRAGON_KING_SYSTEM.md`、`DRAGON_EGG_SYSTEM.md`；`DRAGON_RIDING_SCROLL_SYSTEM.md` 范围收窄为骑卷轴 + 门禁。
- 2026-07-03: §8.1 — indexed `DRAGON_RIDING_SCROLL_SYSTEM.md`（DragonRidingScroll / Dragon King 掉落 / 龙类骑乘门禁）。
- 2026-07-03: §3.1 — `avatar-system.json`（`keep_extra`）：Avatar's Ascent 子系统（`AvatarLocalization` + `avatar.*` shotkeys；命令、商店 Gump、飞升/模板/奖励文案、世仇派系名、物品 OPL）。
- 2026-07-03: §0 / §4.3 / §4.5 / §5.2 / §5.4 / §6.1 — mandatory **existing save compatibility** final review (§4.5 checklist + §5.2 step 5 + self-report template); cross-ref in `server-stability-crash-patterns.md` Agent checklist.
- 2026-05-23: §1 — defined `UO_DEV_DOCS_ROOT` variable (_cross-repo documentation root_); §0 / §1 / §5 / §8 — added cross-repo doc index table, document-first exploration guidance, and `UO_DEV_DOCS_ROOT` resolution rule.
- 2026-07-16: §0 / §1 — indexed `World/Documentation/rate-config-system.md`: new generic `RateConfigEngine`/`WeightedPick` infra (`World/Source/System/RateConfig/`, `Data/RateConfig/*.json`, `[ratereload]`/`[ratelist]`/`[rateget]`); first consumers are dragon Bright-breed rarity (`DragonBreedRarity`, `dragon-rarity.json`) and `GemDragon` scale rarity (`gemdragon.json`), replacing the uniform `Utility.RandomMinMax`/switch pickers in `RidingDragon`/`Dragons`/`Wyrms`/`GemDragons.cs`.

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

---

## 8. Design & Analysis Documentation (uo-dev-documentations)

> **Root path:** `UO_DEV_DOCS_ROOT` = `/Users/forrrest/projects/uo-dev/uo-dev-documentations`  
> **Resolution rule:** Every path in this section is relative to `UO_DEV_DOCS_ROOT`.  
> When the game-server agent guide (this file) references `UO_DEV_DOCS_ROOT/<path>`, it means the absolute path `<UO_DEV_DOCS_ROOT>/<path>`.  
>
> **Protocol:** Before analyzing or modifying a game mechanism listed below, read the analysis doc **first**. Search the codebase only if the doc is insufficient. Then update the doc with new findings (see [uo-dev-documentations AGENTS.md §4.3 Document-First Exploration Principle](<UO_DEV_DOCS_ROOT>/AGENTS.md#43-document-first-exploration-principle)).

### 8.1 Memento Mechanics Analysis

| Topic | Path (under `UO_DEV_DOCS_ROOT`) | When to read |
|---|---|---|
| NPC dialogue intelligence systems | `memento/game-mechanism/NPC_INTELLIGENCE_DIALOGUE_MECHANISM.md` (EN) / `NPC对话情报机制分析.md` (ZH) | Before analyzing or modifying any NPC dialogue/intel flow |
| NPC economy & vendor mechanisms | `memento/game-mechanism/npc-game-mechanisms.md` | Before modifying vendor, buy/sell, black market, shoppe, or service NPC code |
| Champion spawn system | `memento/game-mechanism/CHAMPION_SPAWN_SYSTEM.md` | Before touching champion/raid spawn logic |
| Gypsy tarot / starting fates | `memento/game-mechanism/GYPSY_TAROT_STARTING_FATES.md` | Before modifying starting-area NPCs or race selection |
| Trap system (code analysis) | `memento/game-mechanism/trap-system/trap_system_code_analysis.md` | Before modifying trap items or trap-related NPC speech |
| Magic system docs | `memento/game-mechanism/spell-and-magic/` (8 files) | Before modifying magic, spell, or rune systems |
| Ancient spell research system | `memento/game-mechanism/ANCIENT_SPELL_RESEARCH_SYSTEM.md` | Before modifying ResearchBag, AncientSpellbook, prepared-spell counters, or any ancient magic casting flow |
| Research Bag (ResearchBag item / GM test) | `memento/game-mechanism/RESEARCH_BAG_SYSTEM.md` | Before modifying ResearchBag initialization, BagOwner, GM issuance, or Avatar death ResearchBag debug |
| Avatar core item death / Memory Echo Resonance | `memento/game-mechanism/AVATAR_CORE_ITEM_RESEARCH_RESONANCE_SYSTEM.md` | Before modifying `AvatarCoreItemMigration`, Dormant/Resonance gumps, `SearchBase` dormant branch, or research snapshot merge on rebirth |
| Death / resurrection / bank tribute | `memento/game-mechanism/death-resurrection-bank-tribute.md` | Before modifying death or tribute handling |
| Pet taming & Jako system | `memento/game-mechanism/PET_TAMING_AND_JAKO_SYSTEM.md` | Before modifying pet/taming mechanics |
| Dragon riding scroll / Dragon King / draconic mounts | `memento/game-mechanism/DRAGON_RIDING_SCROLL_SYSTEM.md` | Before modifying `DragonRidingScroll`, `DragonRiding` keys, or `RidingDragon`/`Dragoon`/`GemDragon` mount gate |
| Dragon King boss | `memento/game-mechanism/DRAGON_KING_SYSTEM.md` | Before modifying `DragonKing`, lucky kills, ManualOfItems relics book, or dragon `DropSpecial` on Great Dragons |
| ManualOfItems / Relics chest | `memento/game-mechanism/MANUAL_OF_ITEMS_RELICS_SYSTEM.md` | Before modifying `ManualOfItems`, `RelicBoxGump`, `GiveItemBonus`, `Gift*` enchant flow, or boss relic drop tables |
| Legendary / Relics / Standard / Sage 高级装备横向分析 | `memento/game-design-idea/ADVANCED_EQUIPMENT_ARTIFACT_SYSTEMS_COMPARATIVE_ANALYSIS.md` | Before balancing or documenting Legendary vs Relics vs Standard/Sage artefact acquisition and power ceiling |
| Relics 掉落改革 Feature Request（Top-3 伤害制） | `memento/game-design-idea/RELICS_DROP_REFORM_TOP3_DAMAGE_SYSTEM.md` | Before implementing the Top-3 damage contributor drop reform (Gate A/B/C, Dragon King, Shadowlord, RelicChestDropHelper) |
| Epic Tribute（`EpicCharacter`）获取难度提升设计 | `memento/game-design-idea/EPIC_TRIBUTE_ACQUISITION_DIFFICULTY_REDESIGN.md` | Before modifying `EpicCharacter.cs`, `QuestTome.cs`/`QuestTake.cs` (Epic NPC branch), or `SummonCarriers.cs` key-mob difficulty |
| Dragon egg hatch | `memento/game-mechanism/DRAGON_EGG_SYSTEM.md` | Before modifying `DragonEgg`, Search potions, or hatch-at-vet flow |
| Race temptation & potion shelf | `memento/game-mechanism/race-temptation-and-potion-shelf.md` | Before modifying race temptation or potion shelf |
| Player hazards & threats | `memento/game-mechanism/PLAYER_HAZARDS_AND_THREATS.md` | Before modifying hazard/threat systems |
| Black Knight NPC / black key / Vault of the Black Knight / Vordo boss / Bottle World of Kuldar | `memento/game-mechanism/BLACK_KNIGHT_VAULT_BOTTLE_WORLD.md` | Before modifying BlackKnight, BlackKnightBox, Vordo, VordoScroll, GateMoon in Kuldar, or the Kuldar Bottle World region logic |
| Golem Porter (搬运魔像) system | `memento/game-mechanism/GOLEM_PORTER_SYSTEM.md` | Before modifying GolemPorter, GolemPorterItem, GolemManual, or any porter/pack creature |
| Treasure Hoard (宝藏堆) | `memento/game-mechanism/TREASURE_HOARD_SYSTEM.md` | Before modifying HoardPiles, HoardTile, HoardPile.MakeHoard, or HoardMinionFamiliar |
| Equipment breakdown (Break Down / Scissors) | `memento/game-mechanism/EQUIPMENT_BREAKDOWN_SYSTEM.md` | Before modifying `BreakDown.cs`, `Scissors.CutUp`, `BaseItemBreakDown`, craft `BreakDown` flags, or `ColorlessFabricBreakdown` |
| Equipment enhancement (Enhance / Guild / sharpening stones) | `memento/game-mechanism/EQUIPMENT_ENHANCEMENT_SYSTEM.md` | Before modifying `Enhance.cs`, `GuildCraftingProcess`, `AttributeHandler`, sharpening items, or `ResourceCanChange` |
| PvP / harmful actions / notoriety / murder | `memento/game-mechanism/PVP_COMBAT_SYSTEM.md` | Before modifying player combat, `Mobile_AllowHarmful`, criminal/murder reporting, or region combat rules |
| Slayer weapon system | `memento/game-mechanism/SLAYER_WEAPON_SYSTEM.md` | Before modifying slayer mechanics |
| Endgame boss data & BeefUp | `memento/game-mechanism/ENDGAME_BOSS_ANALYSIS.md` | Before balancing named bosses, champ/world boss stats, or difficulty scaling |
| Endgame content survey | `memento/game-mechanism/survey_endgame_content.md` | Broad endgame inventory (champs, Avatar, quests, items) |
| Ultima-Adventures team PvE comparison | `memento/game-mechanism/ULTIMA_ADVENTURES_ENDGAME_TEAM_DESIGN_COMPARISON.md` | Cross-shard endgame/team-challenge design reference (external Adventures repo) |

### 8.2 Castle of Knowledge

| Topic | Path (under `UO_DEV_DOCS_ROOT`) | When to read |
|---|---|---|
| Castle of Knowledge (Power Scroll vendors) | `memento/castle-of-knowledge.md` | Before modifying Lodor castle, power scroll vendors, or related NPCs |

### 8.3 Quest / Lore Design

| Topic | Path (under `UO_DEV_DOCS_ROOT`) | When to read |
|---|---|---|
| Character item restore system | `memento/character-item-restore-system.md` | Before modifying item restore NPC or gump |
| Bard's Tale / Skara Brae questline | `memento/quest/bottle-world-skara-brae/` — implementation / redesign / texts / **short-term immersive UX** (`BARDS_TALE_SKARA_BRAE_SHORT_TERM_UX.md`) | Before modifying Skara Brae quest NPCs, guidance, temporary exit, or texts |
| Unsent letter quest | `memento/quest/quest-unsent-letter-design.md` | Before modifying unsent letter quest NPCs |
| Magical prison | `memento/game-mechanism/MAGICAL_PRISON.md` | Before modifying prison system |
| Blood Temple (Isles of Dread dungeon / prison key #44) | `memento/game-mechanism/BLOOD_TEMPLE_SYSTEM.md` | Before modifying the Blood Temple region, spawns, SummonCarriers #44, or SearchBase area 50 |

### 8.4 Starting a New Analysis

If you need to analyze a game mechanism not yet documented under `UO_DEV_DOCS_ROOT/memento/`:

1. Search the `uo-dev-documentations` repo for related docs first (e.g. `tech-notes/`, `game-balance-design/`).
2. Search the `ultima-memento` codebase for the relevant C# source files.
3. Create a new analysis doc under `<UO_DEV_DOCS_ROOT>/memento/game-mechanism/` following the conventions in `UO_DEV_DOCS_ROOT/AGENTS.md`.
4. Update this §8 table and `uo-dev-documentations/AGENTS.md` to index the new doc.

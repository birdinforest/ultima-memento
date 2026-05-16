# Character Item Restore System — Technical Reference

Authoritative paths for engineers and shard operators:

| Component | Location |
|-----------|----------|
| GM command + gump + backup analyzer + spawn logic | [`World/Source/Scripts/Engines and Systems/CharacterRestore/CharacterRestoreGump.cs`](../Source/Scripts/Engines%20and%20Systems/CharacterRestore/CharacterRestoreGump.cs) |
| Session disk logger | [`World/Source/Scripts/Engines and Systems/CharacterRestore/CharRestoreLogger.cs`](../Source/Scripts/Engines%20and%20Systems/CharacterRestore/CharRestoreLogger.cs) |
| Delivery NPC + player dialog | [`World/Source/Scripts/Mobiles/Civilized/Special/LostItemsRestorerNPC.cs`](../Source/Scripts/Mobiles/Civilized/Special/LostItemsRestorerNPC.cs) |
| Logical-key locales | [`World/Data/Localization/en/charrestore.json`](../Data/Localization/en/charrestore.json), [`World/Data/Localization/zh-Hans/charrestore.json`](../Data/Localization/zh-Hans/charrestore.json) |
| Keyed NPC speech helper | [`World/Source/Scripts/Mobiles/Civilized/CitizenLocalization.cs`](../Source/Scripts/Mobiles/Civilized/CitizenLocalization.cs) |
| Offline manifest tool | [`World/Source/Tools/analyze_character_backup.py`](../Source/Tools/analyze_character_backup.py) |
| Extractor whitelist | `keep_extra` in [`World/Source/Tools/build_localization_strings.py`](../Source/Tools/build_localization_strings.py) (includes `charrestore.json`) |

---

## 1. Purpose

Give **Game Masters** a safe way to **inspect a filesystem backup** of shard `Saves`, list a character’s **equipped gear** plus **recursive backpack contents**, selectively recreate those items on the **live world**, and hand them to the correct player via a **temporary NPC** with localized dialogue—without relying on brittle hash-keyed catalog entries for restore-specific copy.

Design goals enforced in code:

- **Bounded behavior**: capped batch size, guarded binary reads, clamped numeric fields.
- **Audit trail**: append-only UTF-8 logs per restore session under `Logs/CharacterRestore/` (below).
- **No silent stat inflation**: only **base-class** `Item` fields read from serialization are reapplied with **explicit caps**; subclass-only stats are **not** read from backup slices used here.

---

## 2. Operator Flow (GM)

1. Grant: **GM** runs `[CharRestore` (square bracket convention for in-game commands in this codebase).
2. **Tab 1 — Setup**: Backup directory path (e.g. a copy of `Saves`), **account name**, **character name** (required fields for Analyze). Click **Analyze Backup**.
3. **Tab 2 — Items**: Paginated checklist of discovered items (equipped flagged by layer label). Adjust inclusions.
4. **Tab 3 — NPC & Spawn**: Optionally set target player **by name**, or click **target player ingame** to populate from `Mobile.Name`. Optional **personal note** (hue message after delivery). **Spawn NPC** at GM location.

Spawn creates:

- One internal **`Bag`** named via logical key `charrestore.npc.bundle_name` (fallback `"Restoration Bundle"`), populated with reconstructed items.
- One **`LostItemsRestorerNPC`** at the GM’s location; **`RestorationBag`** reference + **`LogPath`** set on the NPC.

The target player interacts with the NPC; after a short three-stage **`LostItemsDialogGump`**, `DeliverItems` creates a localized delivery bag (`charrestore.npc.bag_name`) and moves restored items into the player’s backpack.

---

## 3. Command Registration

`CharacterRestoreCommand.Initialize()` registers **`CharRestore`** at `AccessLevel.GameMaster` via `CommandSystem.Register`. Server startup invokes all script `Initialize` methods (`ScriptCompiler.Invoke("Initialize")` in core), so no separate bootstrap call is needed once the scripts assembly loads.

---

## 4. Backup Analysis — Files and Prerequisites

### 4.1 Required paths (relative to the backup directory)

| Relative path | Role |
|---------------|------|
| `Accounts/accounts.xml` | Locate account + character serials |
| `Items/Items.tdb` | Type-name table indexed by TypeID |
| `Items/Items.idx` | Per-item TypeID, Serial, blob offset, blob length |
| `Items/Items.bin` | Raw `GenericWriter`/binary serialization blobs |

All four must exist; otherwise analysis throws **`FileNotFoundException`** surfaced to the GM as `Analysis failed: …`.

### 4.2 Serial resolution (in-game analyzer)

**Important limitation:** `BackupSaveAnalyzer.Analyze(..., accountName, characterName, …)` currently passes `characterName` through the UI for validation and logging context, but **`FindCharacterSerial` only resolves the account and returns the first `<char>` serial** under that account’s `chars` element. It does **not** walk `Mobiles` data to pick a character by name.

**Contrast:** The Python tool **`analyze_character_backup.py`** resolves the correct mobile by scanning **`Mobiles/Mobiles.*`** when multiple character serials exist, using `Mobile.Serialize`-compatible name extraction (see that script).

**Recommendation:** Accounts with multiple characters should use **either** offline Python analysis to confirm serial **or** ensure the desired character’s serial is listed first until in-game matching is enhanced.

---

## 5. Items.bin Parsing

### 5.1 Index map build

Every `Items.idx` entry contributes:

```
serial → (TypeFull string from Items.tdb, byte offset in Items.bin, length)
```

Malformed or out-of-range slices are skipped (counted toward parse failures in status text).

### 5.2 Per-record parsing

Each blob is interpreted as **`Item.Serialize` version read as first `Int32`**:

- Accepted versions **6–14**.
- Versions **outside** that range throw **`InvalidDataException`** for that serial (caught per item; increments parse failure tally).

Parsing mirrors the layering of `Item.Serialize` in ServUO-derived trees:

- **≥ 14**: v14-exclusive block (`Purchased` read-and-discarded, enchant/color/world/catalog/resource/artifact/limit fields, **`BuiltBy` ref discarded**).
- **≥ 11**: `EnchantedSpell` + charge fields.
- **≥ 10**: `GraphicID`, `GraphicHue`, **`LastMobile` ref/name discarded**.
- **Case 6**: `SaveFlags Int32`, optional location/parent/name/hue/layer subtree per flag bits (**SaveFlag** constants mirrored in-code from `Item`).

Children list: If `SF_Items`, read `count` × `childSerial`; these populate both **per-record `ChildrenSerials`** and **`childrenMap`** aggregation.

Parent: If `SF_Parent`, reads parent serial → **`parentMap[child]=parent`** and bidirectional adjacency equivalent.

### 5.3 Defensive I/O guards

| Guard | Behavior |
|-------|----------|
| **`SafeReadString`** | Reimplements 7-bit length prefix validation; rejects negative or **`> MaxStringBytes` (4096)** before allocating UTF-8 payload. Prevents malformed saves from inducing huge allocations. |
| **`ReadEncodedInt`** | At most **five** continuation bytes (`for (i … < 5)`) mirroring `.NET`-style 32-bit encoded ints; avoids infinite loops on truncated streams. |

### 5.4 Item aggregation for restoration

Given character serial **`charSerial`** from accounts.xml:

1. **`directItems`**: all children serials linked to **`charSerial`**.
2. **Backpack detection**: Prefer item with **`Layer == 0x15`** (`Backpack`); else type name heuristic containing `"Backpack"`.
3. **Equipped**: every direct item **except** the backpack serial is listed as equipped-ish (layer label from map).
4. **Backpack recurse**: **`CollectContainerContents(container, depth)`**—depth capped at **`5`**; recurse into subtype names matching **Bag | Backpack | Pouch | Container | Chest | Box | Sack** substrings (`IsContainerType`).

If backpack missing: warning **`Backpack not found — only equipped items listed.`**.

---

## 6. Structured backup descriptor: `BackupItemInfo`

Used by the GM gump and spawn pipeline:

- **`TypeFull` / `TypeShort`**: Full script type vs short name suffix (from TDB row).
- **`IsEquipped`, `Selected`**, display **`Hue`, `Amount`, `Name`, `Layer`**.
- **`FullProps`** (`ParsedItemProps?`): full base-class decode for apply step; **`null`** if parsing failed for that serial (type still visible from IDX/TDB).

`DisplayLabel` augments tooltip-style text with **CraftResource**, **MagicSpell**, **ArtifactLevel** when present in **`FullProps`**.

---

## 7. Parsed base-class snapshot: `ParsedItemProps`

Structured mirror of **`Item.Serialize` v14-visible base fields**:

- Enchantment/display: `EnchantMod`, `ColorHue*`, `ColorText*`, `WorldItemID`.
- Booleans: `Technology`, `VirtualContainer`, `NotIdentified`, `NotModAble`, `NeedsBothHands`, `LimitsDelete`, `Built`.
- Identification/catalog: `NotIDAttempts`, `NotIDSource`, `NotIDSkill`, `Catalog`.
- Economy/material: `CoinPrice`, `Resource`, `SubResource`, `SubName`, `ArtifactLevelVal`.
- Narrative/UI: `InfoData`, `InfoText1`…`5`.
- Limits: `Limits`, `LimitsMax`, `LimitsName`.
- Enchant charges: `EnchantedSpell`, `EnchantUses`, `EnchantUsesMax` (≥ v11 block).
- Custom graphic: `GraphicID`, `GraphicHue` (≥ v10 block).
- Core flags section: **`Hue`, `Amount`, `Layer`, `Name`, `Parent`, `ChildrenSerials`**, **`ParsedVersion`**.

**Explicitly excluded** from restore (read and dropped or intentionally not stored): **`Purchased`**, **`BuiltBy`**, **`LastMobile` / name** references—prevent cross-world pointer resurrection.

---

## 8. Spawn Pipeline and Caps

### 8.1 Batch limits

- **`MaxRestoreItems = 500`**: rejecting larger selections with GM message prevents pathological allocations.

### 8.2 `TryCreateItem`

1. **Type resolution**: `ScriptCompiler.FindTypeByFullName` then `FindTypeByName`.
2. Validations: must be concrete **`Item`** subclass with **parameterless ctor**.
3. `Activator.CreateInstance(t)`.
4. If **`FullProps`**, call **`ApplyBackupProperties`**; else **`ApplyBasicFields`** (hue/amount/name only with same caps).

Failures log **`CREATE FAIL`** lines with failure reason via **`CharRestoreLogger.LogItemFail`**.

### 8.3 `ApplyBackupProperties` — numeric caps (`BackupSaveAnalyzer` constants)

| Constant | Value | Applied to |
|----------|-------|-------------|
| `Cap_Hue` | 3000 | `Hue`, `GraphicHue` |
| `Cap_Amount` | 60000 | stackable **`Amount`** only |
| `Cap_ItemID` | 0xFFFF | validates `WorldItemID` applicability |
| `Cap_CoinPrice` | 10_000_000 | `CoinPrice` |
| `Cap_EnchantMod` | 100 | `EnchantMod` |
| `Cap_NotIDAttempts` | 100 | `NotIDAttempts` |
| `Cap_EnchantUses` | 500 | enchant charge fields |
| `Cap_Limits` | 50_000 | `Limits` / `LimitsMax` cascade |
| `Cap_GraphicID` | 0xFFFF | `GraphicID` |
| String caps | 80 / 80 / 80 / **500** / **20** | Name, SubName, LimitsName; Info\*; Color\* tags |

Hue **0** is treated as **unset** (no assignment from backup). Charges: **`EnchantUsesMax` then `EnchantUses`** with clamp so current never exceeds max. Limits: **`LimitsMax` then `Limits`**.

### 8.4 Enum hygiene

Assignments guarded with **`Enum.IsDefined`**:

- **`Identity`** (`NotIDSource`), **`IDSkill`**, **`Catalogs`**, **`CraftResource`** (`Resource`, `SubResource`), **`ArtifactLevel`**, **`MagicSpell`** (`Enchanted` assignment).

Absent or future enum ints are ignored (preserve constructed defaults).

### 8.5 String hygiene

**`SanitizeName`**: trims control characters `< 0x20`, strips `DEL`, enforces maximum length via truncation—aligns clamped payloads with extractor/localization ergonomics rather than verbatim binary garbage.

Some out-of-band values log **`CREATE FAIL` as “clamp / not applied”** but continue when safe (hue clamp message uses same logger path intentionally for auditing).

---

## 9. Known Limitations — Subclass Persistence

Weapons, armor pieces, imbued artifacts, pets, `Container` holdings beyond depth cap, scripted quest state, etc., may serialize **derived-class fields** after `Item.Deserialize` merges base state. **`ParsedItemProps` deliberately stops at shared `Item` layout**.

Implication: reconstructed objects get **fresh subclass defaults** (e.g., weapon damage tiers, elemental resist overlays, augmentation tables) matching **whatever the ctor sets today**, not arbitrary historical outliers from backup blobs.

Extending fidelity would require **per-type parsers** co-located with each type’s **`Serialize`/GenericWriter ordering** contracts.

---

## 10. Delivery NPC — Behavior and Safety

### 10.1 `LostItemsRestorerNPC`

- Derives **`BasePerson`**. **Invulnerable**, **cannot die**, **`IsEnemy` false**.
- Randomized citizen outfit; title **"the sea salvager"**.
- Holds **`Bag RestorationBag`** and optional **`TargetName`** string lock.
- **24 h** lifespan via **`DeleteTimer`**; resets to ~**8 seconds** via **`ScheduleDeparture()`** post-delivery countdown.

Double-click guards:

| Condition | Feedback |
|-----------|----------|
| Wrong **`Mobile.Name`** (ordinal case-insensitive) vs **`TargetName`** | `CitizenLocalization.SayLocalizedByKey` → **`charrestore.npc.deflect`** |
| Restoration bag missing / deleted | **`charrestore.npc.lost_parcel`** |

### 10.2 `DeliverItems`

Preconditions validated before movement:

- Alive player, **`Backpack` non-null**, restoration bag nonempty.
- Copies **`m_RestorationBag.Items`** to `List<Item>` snapshot (no mutation races).
- For each element: **`LogDeliveredItem`**, **`deliveryBag.DropItem(item)`**. Failures logged; originals remain unless moved.
- **`AddToBackpack(deliveryBag)`**; fallback **`MoveToWorld`** on catastrophic failure attempt.
- **`LogDeliveryEnd`**, keyed farewell **`charrestore.npc.farewell`**, optional **`PersonalNote`** system message (**hue `0x59`** branch), then **`ScheduleDeparture()`**.

### 10.3 Persistence

Serialization **version `1`** appends **`m_LogPath`**. **`m_CreatedTime`** clamped after load if obviously invalid (> now or older than ~30 days) so delete timers behave predictably across edge corruption.

---

## 11. Player Dialog Gump — `LostItemsDialogGump`

Three conversational stages keyed under **`charrestore.dialog.*`**:

| Stage | Purpose | Typical keys |
|-------|---------|---------------|
| 0 | Identity check | `.title/body/buttons.*` greeting |
| 1 | Narrative preamble | `.body.story` |
| 2 | Hand-off prompt | `.body.handoff` + thanks button triggers delivery |

Titles pulled from **`charrestore.dialog.title.*`**.

Helper **`K(viewer,key,fallback)`** mirrors GM gump’s pattern: resolves via **`StringCatalog.TryResolveByKey(AccountLang.GetLanguageCode, key)`**.

NPC branching speech:

- Confirmation → **`charrestore.npc.confirmed`**.
- Wrong identity → **`charrestore.npc.wrong_person`**.
- Acceptance button final → **`DeliverItems`**.

---

## 12. Localization Architecture

Logical JSON bundle **`charrestore.json`** (EN + zh-Hans) grouped into:

| Prefix | Audience |
|--------|----------|
| `charrestore.gump.*` | GM wizard labels, pagination, diagnostics |
| `charrestore.dialog.*` | Three-stage conversational gump |
| `charrestore.npc.*` | NPC keyed speech + localized bag naming |

Mechanisms:

| Surface | Resolver |
|---------|----------|
| GM gump | `CharacterRestoreGump.L(key,fallback)` |
| NPC broadcast speech | **`CitizenLocalization.SayLocalizedByKey`** (`TryResolveByKey` per observer) |

The extractor tool **must** retain `charrestore.json` via **`keep_extra`** (`build_localization_strings.py`) — it is not scanner-owned JSON.

Operational note: **`LostItemsRestorerNPC.DeliverItems`** still contains a few **`SendMessage(0x20, "…English…")`** hardcoded failure strings for exceptional states; those bypass the extractor. Consider migrating if policy requires universal catalog coverage.

---

## 13. Disk Logging (`CharRestoreLogger`)

**Directory:**

```
Path.Combine(Core.BaseDirectory, "Logs", "CharacterRestore")
```

Typically adjacent to **`WorldLinux.exe`** / shard base directory (**not** nested under Saves).

**Filename pattern:**  
`restore-YYYYMMDD-HHmmss-{SanitizedGMName}.log`  
(`Sanitize`: alnum + `_` `-`).

**Concurrency:** **`lock(s_Lock)`** around **`StreamWriter(path, append: true, UTF-8)`** for thread-safe concurrent append (**Mono-compatible constructor** historically significant).

Recorded events:

1. **`BeginSession`**: GM, backup path, account/char/target labels, selection count.
2. **`CREATE OK` / `CREATE FAIL`** per reconstructed item (`CREATE FAIL` also used opportunistically for clamp notices).
3. **`LogSessionSummary`**: totals, bundled bag serial, NPC placement, TTL notice.
4. **Delivery subsection**: opener, line per **`ITEM`**, summary + NPC removal note.
5. **`LogError`** for exceptional paths (empty **`logPath`** falls back console).

NPC stores **`LogPath`** so **post-reboot** auditing can theoretically continue—but delivery likely already finished; field exists for forensic continuity.

Standard command pipeline also emits **`CommandLogging.WriteLine`** for spawn actions.

---

## 14. Offline Analyzer — Python

**Purpose:** Produce an auditable **`character-backup-manifest.json`** referencing the same **`Items.idx` / `Items.bin` / accounts layout**, but with **explicit character-name disambiguation** via mobile scanning.

CLI (required args):

```
python3 World/Source/Tools/analyze_character_backup.py \
  --backup-path /path/to/Saves \
  --account AccountName \
  --character CharacterName \
  [--output custom.json]
```

Key behaviors:

- **Character serial lookup** distinguishes multi-character accounts.
- Mirrors container recursion (**depth ≤ 5**, same heuristic type tokens).
- Per-item JSON merges **thin display fields** + non-default **`FullProps`-style snake_case keys**, each optionally **clamp-logged** to the same **`CAPS` map** enumerated near `add_item(...)`.

Integration note: this manifest is presently **standalone** documentation; **`CharacterRestoreGump` does not import JSON**. Use it when legal/ops policy demands file-based review before spawning.

---

## 15. Security and Abuse Considerations

- **Access control**: Spawn path requires **GM**.
- **Target lock**: `TargetName` optional open → any alive player matching flow can claim if unset; tighten by targeting known player (`TargetPlayerTarget`).
- **Economic deltas**: **`CoinPrice`**, artifact metadata, stacks—clamped—not a dupe exploit channel by itself but still review high-value restores.
- **Disk path**: Analyzer reads filesystem—GM machine must isolate backup copies to prevent accidental production overwrite (tool never writes into backup dirs).

---

## 16. Troubleshooting Cheat Sheet

| Symptom | Likely Cause |
|---------|---------------|
| `Analysis failed` file missing path | Incorrect backup root (`Accounts/` or `Items/` subtree absent). |
| Zero items parsed | IDX/BIN mismatched or corrupt; check parse ratio in GM status message. |
| Items created but stripped stats | Expected—subclass-only fields not serialized in base snapshot. |
| Type not created | Scripted type removed/renamed, missing parameterless ctor, or abstract stub. |
| NPC silent for player | **`TargetName` mismatch** (`Mobile.Name` vs spelled name accents). |

---

## 17. Regression / Verification Checklist

When modifying restore logic:

- [ ] `./compile-world-linux.sh` succeeds (Linux).
- [ ] Spot-check bilingual JSON keys stay synchronized (`en/charrestore.json`, `zh-Hans/charrestore.json`).
- [ ] `python3 World/Source/Tools/sync_localization_glossary.py --check` if zh copy edited for glossary interplay.
- [ ] Run Python analyzer vs a known-good backup comparing serial coverage with in-game Analyze for accounts with single character (**parity test** manual until automated harness exists).

---

## 18. Related Reading

- High-level localization policy: [`AGENTS.md`](../../AGENTS.md) (**charrestore.json** bundle table).
- General localization extractor contract: [`World/Data/Localization/README.txt`](../Data/Localization/README.txt).

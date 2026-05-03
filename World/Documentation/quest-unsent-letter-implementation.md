# The Unsent Letter — implementation and testing

This document describes how **The Unsent Letter** is implemented on top of the **ML quest system**, where NPCs are placed, how objectives advance, and how to test the quest end-to-end.

For narrative design, beats, and localization notes, see the sibling documentation repository: `uo-dev-documentations/memento/quest/quest-unsent-letter-design.md`.

## Source of truth in this repo

| Area | Location |
|------|----------|
| Quest class, objectives, NPCs, items, spawners | `World/Source/Scripts/Engines and Systems/Quests/Core/Definitions/UnsentLetter.cs` |
| Quest pack DTO + loader + runtime helpers | `World/Source/Scripts/Engines and Systems/Quests/Core/UnsentLetterQuestPack.cs` |
| **Data-driven pack (JSON)** | `World/Data/Quests/unsent-letter-pack.json` (resolved under **`Core.BaseDirectory`**, same folder as the runtime executable — e.g. `World/`) |
| RPG dialogue (UI + branching; opened from NPC context menu **Talk**) | `World/Source/Scripts/Engines and Systems/Quests/RpgDialogue/UnsentLetterRpgDialogue.cs`, `DynamicRpgDialogueGump.cs` |
| Quest strings (EN + zh-Hans), **stable logical keys** | `World/Data/Localization/en/script-quest-unsent-letter.json`, `zh-Hans/script-quest-unsent-letter.json` (`quest-unsent-letter-*`; resolved via `StringCatalog.TryResolveLogicalOrHash`) |
| Legacy quest hash keys (other quests) | `scripts-quests.json` / `zh-Hans/scripts-quests.json` (extractor) |

### Localization (logical keys)

Player-facing Unsent Letter copy lives in **`script-quest-unsent-letter.json`**. Code passes keys such as `quest-unsent-letter-meta-title-001`; **`StringCatalog.TryResolveLogicalOrHash`** tries **`TryResolveByKey`** when the string starts with **`quest-`**, then hash **`TryResolve`** (migration / edge cases). **`UnsentLetterI18n`** wraps resolution plus **`QuestCompositeResolver`** for zh-Hans. ML quest gumps use the same helper via **`BaseQuestGump.ResolveQuestCatalogString`** / **`ResolveQuestTextDefinition`**. **`DynamicRpgDialogueGump`** resolves `quest-` prefixes for body and option labels.

- **Whitelist:** `script-quest-unsent-letter.json` is in **`keep_extra`** in `build_localization_strings.py`.
- **Docs:** `AGENTS.md` §3.1, `World/Data/Localization/README.txt`.

---

## How it plugs into the ML quest system

### Registration and givers

- **`UnsentLetterQuest`** subclasses **`MLQuest`** (`MLQuest.cs`). The engine discovers all non-abstract `MLQuest` subclasses at startup (when `Data/MLQuests.cfg` is absent) and registers each quest and its givers.
- **`GetQuestGivers()`** returns **`UnsentLetterMara`** only. **Mara** is the sole quest giver. **`UnsentLetterQuest.SendOffer`** is overridden to open **`DynamicRpgDialogueGump`** (pitch + catalog **`Description`**, Accept / Not now) instead of **`QuestOfferGump`**. The ML Quest **context-menu “Quest”** entry still calls **`MLQuestSystem.OnDoubleClick`**, which uses this same RPG offer. Story beats on Mara, Lina, Thomas, Henrick, and Garron advance through **context-menu Talk → RPG gump** (and **combat** where scripted), not player chat keywords. **`OnDoubleClick`** on these NPCs is **not** used for quest dialogue (default client behavior, e.g. paperdoll).
- **`ObjectiveType.All`**: every listed objective must complete before the quest is marked complete and the reward can be claimed (return to Mara).
- **`HasRestartDelay`** is true; **`GetRestartDelay()`** is **22 hours** between completions for the same character (subject to `MLQuestContext` completion records).

### Instances and context

- When the player accepts the quest, **`MLQuestInstance`** is created with one **`BaseObjectiveInstance`** per **`BaseObjective`**, in list order.
- **`MLQuestSystem.GetOrCreateContext(pm)`** holds **`QuestInstances`** for that player.

### Player-visible status (`UnsentLetterQuestPhase`)

The implementation exposes a derived **phase** for routing and docs (not a duplicate save format): **`UnsentLetterQuestHelper.GetCurrentPhase(pm)`** returns **`UnsentLetterQuestPhase`**, computed from the **first incomplete** objective in the fixed ML list. **`FindInstance(pm)`** skips **`ClaimReward`** instances; **`FindAnyUnsentLetterInstance(pm)`** includes them (Mara reward **Talk**).

| Phase | Objective (first incomplete) | Serialized state (summary) |
|-------|------------------------------|---------------------------|
| **`SpeakFamily`** | **`UnsentSpeakFamilyObjectiveInstance`** | **`HeardMara`**, **`HeardLina`**, **`HeardThomas`** |
| **`Puzzle`** | **`UnsentPuzzleObjectiveInstance`** | **`Started`**, **`Step`**, etc. |
| **`Ambush`** | **`UnsentAmbushObjectiveInstance`** | **`Phase`**, **`Remaining`** |
| **`Evidence`** | **`UnsentEvidenceObjectiveInstance`** | **`LinaTold`** + backpack checks |
| **`Clerk`** | **`UnsentClerkObjectiveInstance`** | Fight progression |
| **`Ending`** | **`UnsentFamilyEndingObjectiveInstance`** | **`FamilyTalks`**, **`EndingChoice`** |
| **`ClaimReward`** | ML **`ClaimReward`** set | Reward gump pending |

Persistence is **server-side** via **`MLQuestContext`** / **`MLQuestPersistence`** on world save.

### RPG dialogue UI (context menu **Talk**)

Each quest NPC adds **`UnsentLetterTalkEntry`** (cliloc **6146**) in **`AddCustomContextEntries`**. **`UnsentLetterRpgDialogue.TryOpenFromTalk`** opens the appropriate **`DynamicRpgDialogueGump`** and calls **`MLQuestSystem.TurnToFace`** on success.

- **Mara, no instance**: **`TryOpenMaraQuestOffer`** (same offer as ML **Quest** menu) or a short **not-ready** fallback gump.
- **Mara, reward pending**: **`SendQuestRewardOffer`**.
- **In-progress**: **`GetCurrentPhase`** gates which NPC shows the main branch; otherwise **`quest-unsent-letter-rpg-*-hint-*`** / **`fallback-*`** strings explain what to do next.

**Ranges**: 5 (Mara, Lina, Thomas), 8 (miner), 6 (clerk). Out of range: **`quest-unsent-letter-msg-toofar-001`**.

**`OnRefuse`**: still uses **`DynamicRpgDialogueGump`**. **`OnDoubleClick`** on these NPCs does **not** open quest dialogue (paperdoll / default). **Chat keywords are not** used to advance this quest.

### Quest actions (`UnsentLetterQuestHandlers`)

Gump options invoke **`Apply*`** / **`Begin*`** methods only (e.g. **`ApplyMaraPuzzleAnswer`**, **`BeginMinerEscort`**).

| Helper | Behavior |
|--------|----------|
| **`FindInstance(pm)`** | Active instance; skips **`ClaimReward`**. |
| **`FindAnyUnsentLetterInstance(pm)`** | Any non-removed Unsent Letter instance. |
| **`FindObjective<T>(pm)`** | First incomplete **`T`** on **`FindInstance`**. |
| **`FindIncompleteObjective<T>(inst)`** | First incomplete **`T`** on a given instance (phase). |
| **`FindObjectiveAny<T>(pm)`** | First **`T`** regardless of completion. |

### Combat wiring

- **`UnsentGreyCloakBrigand`**: **`QuestPlayer`** is set when spawned during the Montor-road ambush. On death, **`OnDeath`** resolves **`UnsentAmbushObjectiveInstance`** and calls **`OnBrigandKilled`**.
- **`UnsentHireling`**: On death, **`OnHirelingDeath`** on **`UnsentClerkObjectiveInstance`** runs so the clerk fight can count down and award the full letter.

### Serialization

Objective instances that implement **`IDeserializable`** pair **`Serialize`/`Deserialize`** with the ML quest persistence pipeline so progress survives server restarts.

### Dev-only logging

**`UnsentLetterDevLog`** (same file, namespace **`Server.Engines.MLQuests`**) is **off by default** (`Enabled == false`). When enabled, it writes **`[UnsentLetter]`** lines to the console and appends to **`{Core.BaseDirectory}/Logs/unsent-letter-quest-dev.log`**. Use it for step-by-step investigation without enabling global **`MLQuestSystem.Debug`**.

---

## Objective chain (order matters)

The quest adds objectives in this order; **`FindObjective<T>`** always targets the **first incomplete** objective of that type, which matches the intended story order.

| # | Objective | Completion rule (summary) |
|---|-----------|---------------------------|
| 1 | **`UnsentSpeakFamilyObjective`** | **`HeardMara`**, **`HeardLina`**, **`HeardThomas`** all true (three **Talk** conversations). |
| 2 | **`UnsentPuzzleObjective`** | Puzzle **started** and **`Step > 3`** (three answers processed). |
| 3 | **`UnsentAmbushObjective`** | **`Phase >= 3`** (two waves cleared, evidence dropped). |
| 4 | **`UnsentEvidenceObjective`** | Backpack contains **`UnsentAdrianBadge`** and **`UnsentTornLetterPage`**, and **`LinaTold`**. |
| 5 | **`UnsentClerkObjective`** | Fight started and all hirelings defeated (default **four**; see pack JSON); **`UnsentFullLetter`** added to backpack. |
| 6 | **`UnsentFamilyEndingObjective`** | **`UnsentFullLetter`** in backpack, **`FamilyTalks >= 2`**, **`EndingChoice != 0`** (Mara ending options in **Talk**). |

---

## NPCs and world spawns

**`UnsentLetterQuest.Generate()`** reads **`UnsentLetterQuestPackLoader`** and places **ML-QS spawners** (named `MLQS-UnsentLetterQuest`) from the pack’s **`spawners`** array. Each entry uses **`Map.Parse(map)`** (invalid map names are skipped with a console line). If the JSON file is missing, corrupt, or **`schemaVersion < 1`**, the loader falls back to **`CreateBuiltinDefaults()`** (same coordinates as the table below). First boot or regeneration creates spawners; existing spawners at the same spot are replaced per **`PutSpawner`** rules.

| Mobile type | Role | Default location (X, Y, Z) | Map |
|-------------|------|---------------------------|-----|
| **`UnsentLetterMara`** | Quest giver, puzzle host, ending choice | (2999, 1064, 0) | Sosaria |
| **`UnsentLetterLina`** | Family + evidence (“evidence”) | (2997, 1061, 0) | Sosaria |
| **`UnsentLetterThomas`** | Family + ending (“letter”) | (1458, 3788, 0) | Sosaria |
| **`UnsentLetterMiner`** (“Henrick”) | Starts ambush via **Talk** escort option | (3185, 2585, 0) | Sosaria |
| **`UnsentLetterClerk`** (“Garron”) | Clerk fight via **Talk** | (1455, 3792, 0) | Sosaria |

In character, Mara and Lina are **Britain**; Thomas and the clerk are **Renika**; the miner is on the **Montor road** quarry-side area as implemented by coordinates above.

### Data-driven pack: `Data/Quests/unsent-letter-pack.json`

Edit this file and **restart the world** for changes to apply. **`System.Runtime.Serialization.Json.DataContractJsonSerializer`** deserializes into **`UnsentLetterPackRoot`** (`UnsentLetterQuestPack.cs`). Property names are **camelCase** in JSON.

| Section | Purpose |
|---------|---------|
| **`schemaVersion`** | Must be **`>= 1`**. Older/missing → built-in defaults. |
| **`questId`** | Informational (`unsent-letter`). |
| **`spawners[]`** | **`typeName`**: NPC class name (must resolve via **`ScriptCompiler.FindTypeByName`**). **`map`**: string for **`Map.Parse`**. **`x`/`y`/`z`**. **`amount`**, **`minDelayMinutes`**, **`maxDelayMinutes`**, **`team`**, **`homeRange`** → **`Spawner`** ctor. **`walkingRange`**: if **`>= 0`**, sets **`Spawner.WalkingRange`**; **`-1`** leaves spawner default. Unknown **`typeName`** → entry skipped (logged). |
| **`ambush`** | **`waves[]`**: each **`count`** for wave 1 and 2. **`spawnOffsetMin`/`spawnOffsetMax`**: random tile offset from player when spawning brigands. **`buffSecondWaveHits`**: if true, second wave gets extra hits (legacy behavior). **`brigandTypeName`**: default **`UnsentGreyCloakBrigand`** — **keep this type** unless you duplicate **`OnDeath`** quest hooks in another mobile type. **`brigandBody`/`brigandHue`**: **`-1`** = do not override. **`equipment[]`**: **`typeName`** (item class), optional **`hue`**, optional **`layer`** (0 = omit). Items are **`EquipItem`** first; fallback **`AddItem`** / backpack. |
| **`clerkFight`** | **`hirelingCount`**, **`hirelingTypeName`** (default **`UnsentHireling`** — same hook warning as brigands). **`hirelingExtraHits`**: applied only when the spawned mobile is **not** **`UnsentHireling`** (the hireling ctor already adds HP for the scripted type). **`hirelingBody`/`hirelingHue`**, **`equipment[]`** as for ambush. **`offsets[]`**: **`x`/`y`** pairs relative to the clerk for each hireling index. |
| **`questItems`** | **`adrianBadge`**, **`tornPage`**, **`fullLetter`**: each may set **`itemId`** (decimal in JSON, e.g. **5357** for **0x14ED**), **`hue`**, **`weight`**. Applied after **`new UnsentAdrianBadge`** / **`UnsentTornLetterPage`** / **`UnsentFullLetter`** via **`ApplyQuestItem`** (affects **new** instances only; not retroactive on saved items). |

Bad JSON or I/O errors: console message and full fallback to built-in defaults (world still starts).

---

## Talk flow and puzzles (no chat keywords)

Advance the quest only through **Talk → RPG gump** (plus combat). **`UnsentLetterQuestPhase`** determines which NPC shows the main dialogue; others show hint fallbacks.

### Family (objective 1)

| NPC | Talk gump |
|-----|-----------|
| Mara | Truth / Adrian options → **`ApplyMaraFamilyTruth`** |
| Lina | Father line → **`ApplyLinaFather`** |
| Thomas | Family line → **`ApplyThomasFamily`** |

Visit in any order until all three flags are set.

### Puzzle (objective 2) — Mara only

**Talk** on Mara auto-**`ApplyMaraPuzzleStart`** and shows question **1–3** in sequence. Answers use gump options (same logical strings as before: Montor road, winter, silence / pride, etc.); **`ApplyMaraPuzzleAnswer`** updates **`Step`**.

### Ambush (objective 3) — Henrick

**Talk** when **`Phase == 0`**: escort option → **`BeginMinerEscort`**. Mid-fight **Talk**: combat hint gump. After **`Phase >= 3`**: “return to Britain” hint.

### Evidence (objective 4) — Lina

Both quest items in pack; **Talk** → evidence option → **`ApplyLinaEvidence`**.

### Clerk (objective 5) — Garron

**Talk** → records option → **`BeginClerkFight`**.

### Ending (objective 6)

**Talk** on Mara / Lina / Thomas for **letter** lines until **`FamilyTalks >= 2`**, then **Talk** on Mara for public / private / quiet choices (**`ApplyMaraEndingChoice`** 1–3).

---

## Quest items

| Type | Role |
|------|------|
| **`UnsentAdrianBadge`** | Dropped after ambush |
| **`UnsentTornLetterPage`** | Dropped after ambush |
| **`UnsentFullLetter`** | Awarded after clerk hirelings defeated |

---

## How to test (checklist)

1. **Confirm spawners** as before.
2. **Accept**: **Mara** → context menu **Quest** or **Talk** (when no instance, both can reach the RPG offer). Choose accept or decline.
3. **Objective 1**: **Talk** on **Lina**, **Thomas**, **Mara** (family branches). Journal completes when all three heard.
4. **Objective 2**: **Talk** on **Mara** — riddles start immediately; answer via gump options through Q3.
5. **Objective 3**: **Talk** on **Henrick** → escort; clear waves; loot badge + page.
6. **Objective 4**: **Talk** on **Lina** with items in pack → evidence option.
7. **Objective 5**: **Talk** on **Garron** → fight option; kill hirelings; **`UnsentFullLetter`** in pack.
8. **Objective 6**: **Talk** for family letter lines (≥ 2 total talks) then **Talk** on **Mara** for ending trio when unlocked.
9. **Turn in**: **Talk** on **Mara** with **`ClaimReward`** → RPG reward gump → claim.

**Regression**: Restart server mid-quest; confirm phase and objectives restore from save. No typing should be required to advance (only **Talk** + gumps + combat).

**Edge cases** unchanged: death, cancel quest, **`UnsentLetterDevLog`**.

---

## Related

- Pointer file: `World/Documentation/quest-unsent-letter-pointer.md`
- ML core: `World/Source/Scripts/Engines and Systems/Quests/Core/MLQuest.cs`, `MLQuestSystem.cs`, `MLQuestEntry.cs`
- **传言系统集成设计（跨仓库，任务融入传闻网络）：** `uo-dev-documentations/memento/game-design-idea/UNSENT_LETTER_RUMOR_INTEGRATION.md`

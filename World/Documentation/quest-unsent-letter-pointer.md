# Quest: The Unsent Letter

**Authoritative narrative and design** (premise, emotional beats, localization key scheme, hooks) live in the documentation repository:

- `uo-dev-documentations/memento/quest/quest-unsent-letter-design.md` (sibling checkout path)

**Implementation** in this repo:

- `World/Source/Scripts/Engines and Systems/Quests/Core/Definitions/UnsentLetter.cs` — `UnsentLetterQuest`, objectives, NPCs, items, `Generate()` spawners
- `World/Source/Scripts/Engines and Systems/Quests/RpgDialogue/UnsentLetterRpgDialogue.cs` — RPG branching dialogue (`DynamicRpgDialogueGump`); opened from NPC **Talk**; gump options use **`UnsentLetterQuestHandlers`** (`Apply*` / `Begin*`).
- `World/Source/Scripts/Engines and Systems/Quests/RpgDialogue/DynamicRpgDialogueGump.cs` — runtime dialogue gump (body + options) for quests
- `World/Data/Localization/en/scripts-quests.json` and `zh-Hans/scripts-quests.json` — extractor-managed strings
- `World/Data/Localization/en/script-quest-unsent-letter.json` and `zh-Hans/script-quest-unsent-letter.json` — planned logical-key bundle (`TryResolveByKey`; whitelisted in `build_localization_strings.py` `keep_extra`)
- `World/Documentation/quest-unsent-letter-implementation.md` — ML quest integration, spawn coordinates, phase (`UnsentLetterQuestPhase`), Talk flow, testing checklist

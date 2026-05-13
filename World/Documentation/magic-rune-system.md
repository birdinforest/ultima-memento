# Magic Rune System — Technical Reference

> **Scope:** Server-side C# implementation of recall runes, runebooks, and the custom rune-stone caster bag (`MagicRuneBag` + `RuneStone`).  
> **Authoritative sources:**
> - `World/Source/Scripts/Items/Trades/Magical/RecallRune.cs`
> - `World/Source/Scripts/Engines and Systems/Magic/Base/Runebook.cs`
> - `World/Source/Scripts/System/Gumps/RunebookGump.cs`
> - `World/Source/Scripts/Engines and Systems/Magic/Misc/MagicRuneBag.cs`
> - `World/Source/Scripts/Engines and Systems/Magic/Base/SpellItemInfo.cs`

---

## 1. Overview

The term "Rune" in Ultima: Memento covers **three distinct game systems**:

| System | Primary Class(es) | Core Purpose | UO Classic? |
|---|---|---|---|
| **Recall Rune** | `RecallRune` | Mark a location, then teleport via Recall/Gate | Yes (extended) |
| **Runebook** | `Runebook`, `RunebookEntry`, `RunebookGump` | Organize up to 16 recall runes | Yes (extended) |
| **Magic Rune Bag** | `MagicRuneBag`, `RuneStone` (26 subclasses) | Reagent-free spellcasting via rune-stone combination | **No** — Memento custom |

This document covers all three, with emphasis on the custom **Magic Rune Bag** system.

---

## 2. Recall Rune (`RecallRune`)

### 2.1 Class Hierarchy

```
RecallRune : Item, IAosItem
```

Source: `World/Source/Scripts/Items/Trades/Magical/RecallRune.cs` (~307 lines)

### 2.2 Key Properties

| Property | Type | Description |
|---|---|---|
| `Description` | `string` | Player-given name of the marked location (or auto-detected region name) |
| `Marked` | `bool` | Whether the rune has been marked with the Mark spell |
| `Target` | `Point3D` | The marked location coordinates |
| `TargetMap` | `Map` | The map the rune points to |
| `House` | `BaseHouse` | If marked inside a house, links to the house (affects hue) |

### 2.3 Hue Encoding (CalculateHue)

The rune changes color based on map and house status:

| Condition | Hue |
|---|---|
| Not marked (`Marked == false`) | `0` (default) |
| Marked → Sosaria (no house) | `0x967` |
| Marked → Lodor (no house) | `0x490` |
| Marked → Underworld (no house) | `0x48D` |
| Marked → SerpentIsland (no house) | `0x48E` |
| Marked → IslesDread (no house) | `0x489` |
| Marked → SavagedEmpire (no house) | `0x48F` |
| Linked to a house (any map) | `0x47E` |

### 2.4 Mark() Method Flow

```
Mark(Mobile caster)
├── Set Marked = true
├── If AOS mode:
│   ├── Find BaseHouse at caster's location
│   ├── If inside a house:
│   │   ├── Set Description = house sign name
│   │   ├── Set Target = house ban location (+2 Y offset)
│   │   └── Set TargetMap = house.Map
│   └── If NOT inside a house:
│       ├── Set Target = caster.Location
│       └── Set TargetMap = caster.Map
├── If NOT AOS mode:
│   ├── Set Target = caster.Location, TargetMap = caster.Map
│   └── House = null
├── If Description not set by house:
│   └── Set Description = BaseRegion.GetRuneNameFor(target region)
├── CalculateHue()
└── InvalidateProperties()
```

### 2.5 Double-Click Behavior

| State | Action |
|---|---|
| Not in backpack | Message: "That must be in your pack for you to use it." |
| Marked + linked to house | Message: "You cannot edit the description for this rune." |
| Marked + no house | Prompt: "Please enter a description for this marked object." |
| Not marked | Message: "That rune is not yet marked." |

### 2.6 Object Properties Display

```
Marked → "a recall rune for {description}"
Not marked → "an unmarked recall rune"
```

### 2.7 Craft Resource

Default: `CraftResource.RegularLeather`. Weight: 1.0.

---

## 3. Runebook (`Runebook`)

### 3.1 Class Hierarchy

```
Runebook : Item, ISecurable, ICraftable
```

Source: `World/Source/Scripts/Engines and Systems/Magic/Base/Runebook.cs`

### 3.2 Key Constants & Properties

| Property | Value / Type | Description |
|---|---|---|
| `MAX_RECALL_RUNES` | `16` | Maximum recall runes per book |
| `CurCharges` | `int` | Current charge count for recall/gate from book |
| `MaxCharges` | `int` | Maximum charges (depends on spell circle used to charge) |
| `Default` | `RunebookEntry` | Currently selected default entry |

### 3.3 Internal Class: `RunebookEntry`

Records a single rune's destination:

```
RunebookEntry
├── Location (Point3D)
├── Map (Map)
├── Description (string)
└── House (BaseHouse)
```

### 3.4 Drop Recall Rune Logic

When a `RecallRune` is dropped on a `Runebook`:

```
DropRecallRune(Mobile from, RecallRune rune)
├── If rune is NOT marked → reject ("That rune has not been marked.")
├── If rune belongs to another → reject ("That rune does not belong to you.")
├── If book already has entry for this location → reject
├── If book is full (≥16) → reject
└── On success:
    ├── Add new RunebookEntry
    ├── Delete the physical RecallRune item
    └── Play sound
```

### 3.5 Gump (`RunebookGump`)

- Dual-page layout (`page_width = 1050`)
- Each rune entry shows: description, map icon, one-click Recall/Gate/Sacred Journey/Astral Travel buttons
- Per-player default spell type stored in `PlayerPreferenceContext.DefaultRunebookSpellType`
- Supports rename prompt for individual entries

### 3.6 Related Items

| Item | Purpose |
|---|---|
| `RunebookDyeTub` | Dye tub for runebooks/rune stones |
| `AllDyeTubsBookRune` | Universal book/rune dye tub (consumes gold + charges) |

---

## 4. Magic Rune Bag (`MagicRuneBag` + `RuneStone`)

**This is a Memento-custom system**, not present in standard RunUO/ServUO.

### 4.1 Class Hierarchy

```
MagicRuneBag : Item
├── MagicRuneGump : Gump (nested, ~400 lines)
└── SpellCheck() — static method

RuneStone : Item (abstract)
├── An
├── Bet
├── Corp
├── Des
├── Ex
├── Flam
├── Grav
├── Hur
├── In
├── Jux
├── Kal
├── Lor
├── Mani
├── Nox
├── Ort
├── Por
├── Quas
├── Rel
├── Sanct
├── Tym
├── Uus
├── Vas
├── Wis
├── Xen
├── Ylem
└── Zu
```

Source: `World/Source/Scripts/Engines and Systems/Magic/Misc/MagicRuneBag.cs` (~1681 lines)

### 4.2 MagicRuneBag Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `EnchantUsesMax` | `int` | `200` | Maximum charge capacity |
| `EnchantUses` | `int` | (set by `ChangeMagicSpell`) | Current charges |
| `Layer` | `Layer` | `Trinket` | Equip slot |
| `Resource` | `CraftResource` | Random leather | Visual material |
| `Rune_Xxx` (26x) | `bool` | `false` | Whether rune Xxx is in the bag |
| `Selected_Xxx` (26x) | `bool` | `false` | Whether rune Xxx is currently selected for casting |

### 4.3 RuneStone Display

Each `RuneStone` subclass has:

- **ItemID**: Unique per stone (0x2379–0x239E)
- **ColorText1**: `"Magic Rune"`
- **ColorText2**: The rune name (e.g., `"An"`, `"Bet"`, ...)
- **Name**: `"{Name} Stone"` (e.g., `"An Stone"`)
- **Weight**: `0.01`

### 4.4 The 26 Rune Stones & Their Meanings

| Rune | ItemID | Meaning (per in-game book) |
|---|---|---|
| An | 0x2379 | Negate / Dispel |
| Bet | 0x237A | Small |
| Corp | 0x237B | Death |
| Des | 0x237C | Lower / Down |
| Ex | 0x237D | Freedom |
| Flam | 0x2387 | Flame |
| Grav | 0x2389 | Energy / Field |
| Hur | 0x238A | Wind |
| In | 0x2393 | Make / Create / Cause |
| Jux | 0x2394 | Danger / Trap / Harm |
| Kal | 0x2395 | Summon / Invoke |
| Lor | 0x2396 | Light |
| Mani | 0x237E | Life / Healing |
| Nox | 0x238B | Poison |
| Ort | 0x2398 | Magic |
| Por | 0x237F | Move / Movement |
| Quas | 0x2380 | Illusion |
| Rel | 0x2381 | Change |
| Sanct | 0x2382 | Protection |
| Tym | 0x2383 | Time |
| Uus | 0x2384 | Raise / Up |
| Vas | 0x2385 | Great |
| Wis | 0x2399 | Knowledge |
| Xen | 0x239C | Creature |
| Ylem | 0x239D | Matter |
| Zu | 0x239E | Sleep |

### 4.5 User Interaction Flow

```
Player has MagicRuneBag in pack
├── Double-click the bag
│   ├── → Opens MagicRuneGump
│   │   ├── Shows all 26 rune slots (only those present in bag are visible)
│   │   ├── Each slot: [remove button] [select toggle] [rune name]
│   │   ├── Instruction text (localized via StringCatalog)
│   │   ├── "Read Book" button (opens RuneJournal)
│   │   └── "Close" button
│   │
│   ├── Player selects rune combination
│   │   └── SetSpell() → calls SpellCheck() for each known spell
│   │   │   └── On match: bag.Enchanted = MagicSpell.Xxx
│   │   │   └── On no match: bag.Enchanted = MagicSpell.None
│   │
│   └── Player drags a RuneStone onto the bag
│       └── PutInBag() → sets Rune_Xxx = true, deletes physical stone
│
├── Equip the bag (Layer.Trinket)
│   └── OnEquip: checks bag must be enchanted (has a spell loaded)
│       └── If not enchanted: "There is no magic on the bag!"
│
├── Double-click equipped bag
│   ├── → CastEnchantment()
│   │   ├── Check bag has enough charges
│   │   ├── Check bag is equipped
│   │   └── → SpellRegistry.NewSpell() → cast the loaded spell
│
└── Drop gold on the bag (near a NPC mage)
    └── → Recharge: refill EnchantUses up to EnchantUsesMax
        └── Mage receives gold payment
```

### 4.6 Charge Economy

| Aspect | Value |
|---|---|
| Max charges | 200 (set in constructor) |
| Charge cost per cast | Depends on spell circle: `GetCircleNumber()` → 3/6/9/12/15/18/21/24 |
| Recharge cost | 1 gold per charge (dropped onto bag while near a mage NPC) |

### 4.7 Spell Combination Mapping

The rune-to-spell mapping is defined in two places that **must be kept in sync**:

1. `MagicRuneBag.MagicRuneGump.SetSpell()` — static `if/else` chain (~83 spells)
2. `SpellItemInfo.m_MagicInfo` — data array in `SpellItemInfo.cs` (source of truth for `GetRunes()`)

Each `SpellItemInfo` entry has a `Runes` string like `"Uus Jux"`. The order of rune names in this string is arbitrary (the combination is unordered — `SpellCheck()` compares each selected boolean).

**Example combinations (from SpellItemInfo):**

| Spell | Circle | Rune Combination |
|---|---|---|
| Clumsy | First | Uus Jux |
| Heal | First | In Mani |
| Magic Arrow | First | In Por Ylem |
| Recall | Fourth | Kal Ort Por |
| Mark | Sixth | Kal Por Ylem |
| Gate Travel | Seventh | Vas Rel Por |
| Resurrection | Eighth | An Corp |
| Summon Daemon | Eighth | Kal Vas Xen Corp |

### 4.8 SpellCheck() Algorithm

```csharp
public static bool SpellCheck(
    MagicRuneBag bag,
    bool x_an, bool x_bet, ..., bool x_zu  // 26 parameters
)
{
    // Returns true only if ALL 26 selected-state booleans
    // exactly match the bag's current selection.
    return bag.Selected_An == x_an
        && bag.Selected_Bet == x_bet
        && ...
        && bag.Selected_Zu == x_zu;
}
```

Complexity note: Each `SpellCheck()` call passes **all 26 booleans** — both `true` and `false` — to ensure the entire selection state is matched. This means a spell definition must list every rune as either required or explicitly NOT required.

### 4.9 PutInBag() Duplicate Protection

```csharp
if (this is An && !bag.Rune_An)   { bag.Rune_An = true; success = true; }
else if (this is Bet && !bag.Rune_Bet) { bag.Rune_Bet = true; success = true; }
// ... 24 more ...
if (success) {
    from.SendMessage("You place the rune in the bag.");
    // Refresh gump if open
} else {
    from.SendMessage("That rune is already in the bag.");
}
```

Each rune type can only be in the bag once. Dropping a duplicate returns the "already in the bag" message.

---

## 5. Backing Lore (in-game book: "Rune Magic" by Garamon the Wizard)

The `RuneJournal` book (`DynamicBook` subclass, `ItemID = 0x5687`, `Hue = 0xAFE`) contains:

### 5.1 Fixed Text Sections

| Section | Content |
|---|---|
| **Intro** | Reagents being rare in the Abyss, Garamon's research into ancient rune stones used by wizards of old |
| **Follow Research** | Transition to listing findings |
| **Rune Bags** | How to use: place bag in pack, open it, select runes, equip, double-click |
| **Rune Meanings** | Table of all 26 runes and their meanings |
| **Spell List** | Dynamically generated from `SpellItems.GetRunes()` — all known spells with their rune combinations |

### 5.2 Dynamic Spell List Generation

```csharp
AppendRuneJournalSpellEntries(StringBuilder sb, TextInfo cultInfo)
├── Iterate MagicSpell enum from 1 to VampiricEmbrace
├── For each spell: if GetRunes() != "":
│   ├── Append spell name (TitleCase)
│   ├── Append rune combination string
│   ├── Append spell description from GetData()
│   └── Append separator
```

---

## 6. Related Spell School Rune Systems

Several non-Magery spell schools have their own rune-marking mechanics:

| School | Rune Spell | Mark Equivalent |
|---|---|---|
| Druidism | `DruidicRuneSpell` (4th circle) | Marks a location (druidic marking oil) |
| Elementalism | `Elemental_Rune_Spell` (6th circle) | "Marca" — marks an elemental rune |
| Witchcraft | `HellsBrand` | Marks an evil rune (hellish branding ooze) |

These all produce location markers that can be used with their respective travel spells (`MushroomGatewaySpell`, `Elemental_Void`, `Elemental_Gate`, `HellsGate`, `GraveyardGateway`).

---

## 7. Localization Status

### 7.1 C# Hardcoded Strings (Pending StringCatalog Pipeline)

Per `World/Documentation/magic-localization-worklist.md`:

| File | Priority | Type |
|---|---|---|
| `MagicRuneBag.cs` | **A** | User-facing `SendMessage` + gump HTML text |

The gump text in `MagicRuneBag.cs` lines 119–120 is already wrapped in `StringCatalog.Resolve()` calls, but the `SendMessage` strings on lines 55–56, 94, 1035, and 1046 are also piped through `StringCatalog`. As of this writing, these have been resolved.

### 7.2 Dictionary Entries

- `en/scripts-engines-and-systems.json` — contains all mark/recall spell descriptions, "Rune", "Elemental Rune", bag interaction strings
- `en/scripts-items.json` — recall rune display strings, rune of enhancing entries
- `en/scripts-system.json` — "Rename Rune", "Rename Runebook"
- `en/scripts-books.json` — Rune Magic book text (~10 entries)
- `en/trap-system.json` — `"a glowing runic trap"` (trap.category.runic)

---

## 8. Future Considerations

| Area | Notes |
|---|---|
| **RuneBag localization** | All UI strings in `MagicRuneBag.cs` use `StringCatalog.Resolve()`; dictionary extraction is tracked under magic-localization-worklist.md |
| **SpellCheck() refactor** | Current 26-boolean-parameter approach is fragile; a bitmask or `HashSet<MagicRuneType>` would be more maintainable |
| **Spell list DRY** | `SetSpell()` (MagicRuneBag.cs) and `m_MagicInfo` (SpellItemInfo.cs) use separate definitions for the same rune→spell mapping. Consider generating `SetSpell()` from the data array. |
| **RuneJournal text** | Fixed English text blocks in `RuneJournalBookText` are partially localized through `StringCatalog` with `TryResolve()` fallback; ZH translation exists in `scripts-books.json` |

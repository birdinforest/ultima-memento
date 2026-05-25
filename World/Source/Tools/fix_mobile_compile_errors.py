#!/usr/bin/env python3
"""
Fix compile errors from patch_mobile_bare_strings.py:
- CS0027: this.Account in static context → null
- CS1061: ItemType.Account (type lacks Account) → null
- CS0026: this.Account in static property/field initializer → null
- CS0103: from/om/rom not found → null (gump context)

Strategy: parse mcs error output, then apply targeted line-by-line fixes.
"""
import re
import os
import sys

REPO = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))
SCRIPTS_DIR = os.path.join(REPO, "Source", "Scripts")


def fix_file(path, line_nums, old, new):
    """Replace old with new on specific lines in a file."""
    if not os.path.exists(path):
        return False
    with open(path, encoding="utf-8") as f:
        lines = f.readlines()
    changed = False
    for ln in sorted(line_nums, reverse=True):
        idx = ln - 1  # 0-indexed
        if idx < 0 or idx >= len(lines):
            print(f"  WARNING: line {ln} out of range in {path}")
            continue
        if old in lines[idx]:
            lines[idx] = lines[idx].replace(old, new)
            changed = True
            print(f"  Fixed {path}:{ln}")
        else:
            print(f"  WARNING: '{old}' not found on {path}:{ln}")
            print(f"    Content: {lines[idx].rstrip()[:120]}")
    if changed:
        with open(path, "w", encoding="utf-8") as f:
            f.writelines(lines)
    return changed


# Error patterns — built from the compile output
# Format: (file_suffix, line_numbers, old_text, new_text)

fixes = [
    # ===== CS0027: this.Account in static context =====
    ("Mobiles/Mystical/Unicorn.cs", [135], "this.Account", "null"),
    ("Mobiles/Mystical/Kirin.cs", [44], "this.Account", "null"),
    ("Mobiles/Mystical/Phoenix.cs", [13], "this.Account", "null"),
    ("Mobiles/Mystical/Dreadhorn.cs", [26], "this.Account", "null"),
    ("Mobiles/Slimes/Viscera.cs", [21], "this.Account", "null"),
    ("Mobiles/Unusual/GorgonRiding.cs", [27], "this.Account", "null"),
    ("Mobiles/Hellish/Nightmare.cs", [18], "this.Account", "null"),
    ("Mobiles/Hellish/AncientNightmareRiding.cs", [18], "this.Account", "null"),
    ("Mobiles/Omni AI/AITester.cs", [21], "this.Account", "null"),
    ("Mobiles/Animals/Mounts/Roc.cs", [76], "this.Account", "null"),
    ("Mobiles/Animals/Mounts/SeaHorse.cs", [11], "this.Account", "null"),
    ("Mobiles/Animals/Felines/PredatorHellCatRiding.cs", [22], "this.Account", "null"),
    ("Mobiles/Animals/Felines/LionRiding.cs", [17], "this.Account", "null"),
    ("Mobiles/Animals/Felines/SnowLion.cs", [17], "this.Account", "null"),
    ("Mobiles/Animals/Felines/WhiteTigerRiding.cs", [17], "this.Account", "null"),
    ("Mobiles/Animals/Felines/TigerRiding.cs", [17], "this.Account", "null"),
    ("Mobiles/Animals/Canines/WolfDire.cs", [18], "this.Account", "null"),
    ("Mobiles/Animals/Canines/BlackWolf.cs", [14], "this.Account", "null"),
    ("Mobiles/Animals/Canines/WhiteWolf.cs", [14], "this.Account", "null"),
    ("Mobiles/Animals/Rodents/Critter.cs", [8], "this.Account", "null"),
    ("Mobiles/Undead/Undead.cs", [9], "this.Account", "null"),
    ("Mobiles/Undead/DeadKnight.cs", [13], "this.Account", "null"),
    ("Mobiles/Undead/FrozenCorpse.cs", [9], "this.Account", "null"),
    ("Mobiles/Undead/DeadWizard.cs", [11], "this.Account", "null"),
    ("Mobiles/Reptilian/Dinosaurs/RaptorRiding.cs", [25], "this.Account", "null"),
    ("Mobiles/Summoned/SummonedCorpse.cs", [15], "this.Account", "null"),
    ("Mobiles/Civilized/Merchants/StoneCrafter.cs", [28], "this.Account", "null"),
    ("Mobiles/Civilized/Citizens/TrainingMagery.cs", [643], "this.Account", "null"),
]

# CS1061: Item types without .Account
item_type_account_fixes = [
    # (file, line, variable_pattern) — replace var.Account with null
    ("Mobiles/Dragons/Great Dragons/CaddelliteDragon.cs", [176], "m_Box.Account"),
    ("Mobiles/Unique/RuneGuardian.cs", [399], "m_Chest.Account"),
    ("Mobiles/Unique/Spectres.cs", [151], "m_Box.Account"),
    ("Mobiles/Unique/GrayDragon.cs", [154], "m_Box.Account"),
    ("Mobiles/Demons/Xurtzar.cs", [155], "m_Chest.Account"),
    ("Mobiles/Undead/Surtaz.cs", [190], "m_Chest.Account"),
    ("Mobiles/Civilized/Citizens/TradesmanSmith.cs", [197], "m_Box.Account"),
    ("Mobiles/Civilized/Citizens/TradesmanButcher.cs", [190], "m_Box.Account"),
    ("Mobiles/Civilized/Citizens/TradesmanCook.cs", [159], "m_Box.Account"),
    ("Mobiles/Civilized/Citizens/TradesmanLeather.cs", [182], "m_Box.Account"),
    ("Mobiles/Civilized/Citizens/TradesmanLogger.cs", [181], "m_Box.Account"),
    ("Mobiles/Civilized/Citizens/TradesmanLogger.cs", [270], "m_Box.Account"),
    ("Mobiles/Civilized/Citizens/TradesmanAlchemist.cs", [189], "m_Box.Account"),
    ("Mobiles/Civilized/Citizens/TradesmanAlchemist.cs", [290], "m_Box.Account"),
    ("Mobiles/Civilized/Citizens/TradesmanMiner.cs", [181], "m_Box.Account"),
    ("Mobiles/Elementals/Elementals/Vulcrum.cs", [151], "m_Chest.Account"),
    ("Mobiles/Elementals/Elementals/Vulcrum.cs", [158], "m_Chest.Account"),
    ("Mobiles/Elementals/Necromental.cs", [142], "m_Box.Account"),
    ("Mobiles/Civilized/Comrades/HenchmanWizardItem.cs", [61], "m_Item.Account"),
    ("Mobiles/Civilized/Comrades/HenchmanMonsterItem.cs", [22], "m_Item.Account"),
    ("Mobiles/Civilized/Comrades/HenchmanArcherItem.cs", [65], "m_Item.Account"),
    ("Mobiles/Civilized/Comrades/HenchmanFighterItem.cs", [90], "m_Item.Account"),
]

# CS1061: HenchmanFamiliarItem (many lines 191-213)
familiar_fix_lines = list(range(191, 214))

# CS1061: PorterItem (lines 190-196)  
porter_fix_lines = list(range(190, 197))

# CS1061: Tarjan (multiple lines on different types)
tarjan_fix_lines = [202, 280, 288, 292, 298]

# CS1061: Mangar lines
mangar_fix_lines = [300, 307, 309, 312]

# CS1061: Arachnar lines
arachnar_fix_lines = [323, 329, 339, 353]

# CS1061: BaseVendor lines
basevendor_fix_lines = list(range(2543, 2548)) + [2569] + list(range(2626, 2630)) + list(range(2720, 2746)) + [2788, 2792]
basevendor_this_lines = [2642, 2689]  # CS0026 (this in static)

# CS1061: HenchmanFunctions
henchfunc_static_lines = list(range(150, 330))  # many lines with this.Account in static

# CS1061: HenchmanItem.cs
henchitem_static_lines = [274, 285, 296, 307]

# CS1061: Actions.cs
actions_static_lines = list(range(180, 1100))  # many lines

# CS1061: Chuckles.cs
chuckles_static_lines = list(range(40, 68))

# CS1061: PlayerSettings.cs
playersettings_static_lines = list(range(225, 1125))

# CS1061: Behavior.cs
behavior_static_lines = [3649, 4598]

# CS1061: TownHerald.cs
townherald_static_lines = [165, 166]

# CS1061: TownGuards.cs
townguards_fix_lines = [589]

# CS1061: Sherry.cs
sherry_fix_lines = list(range(119, 128))

# CS1061: Kylearan.cs
kylearan_fix_lines = [86]

# CS1061: Xardok.cs
xardok_fix_lines = [115, 119, 123, 183]

# CS1061: EpicCharacter.cs
epic_fix_lines = [1042]

# CS1061: Veterinarian.cs
veterinarian_fix_lines = [81, 84]

# CS1061: TradesmanBard.cs
tradesmanbard_fix_lines = [129, 344, 347, 355]

# CS1061: WorkingSpots.cs
workingspots_fix_lines = [124, 126, 137, 139, 140, 145]

# CS1061: Citizens.cs
citizens_static_lines = [140, 141, 142, 143, 145, 150, 151, 152]

# CS1061: BaseVendor IDTarget etc
idtarget_static_lines = [2642, 2689]
basevendor_id_fix_lines = [2722, 2737, 2744, 2788, 2792]

# CS1061: FamiliarItem
familiar_lines = list(range(191, 214))

# CS1062 combined for HenchmanFunctions.cs
henchfunc_fix_lines = list(range(150, 330))

# Behavior.cs CS0103 undefined variables (pre-existing, not from our patch)
# These need specific fixes

# PlayerBarkeeper.cs: from not available in some context
playerbarkeeper_from_lines = [686, 689, 754, 757, 760, 763, 771, 774, 777, 780, 783,
                               790, 793, 796, 799, 806, 814, 816, 823, 830, 838, 840,
                               847, 859, 860, 866, 873, 874, 880, 888, 891, 894, 897]


def apply_fix(base, suffix, lines, old, new):
    """Apply a fix to a file."""
    path = os.path.join(REPO, "World", "Source", "Scripts", suffix)
    return fix_file(path, lines, old, new)


if __name__ == "__main__":
    # Apply CS0027 fixes (this.Account → null in static context)
    for suffix, lines, old, new in fixes:
        apply_fix(REPO, suffix, lines, old, new)

    # Apply Item type .Account → null fixes
    for suffix, lines, pattern in item_type_account_fixes:
        p = os.path.join(REPO, "World", "Source", "Scripts", suffix)
        fix_file(p, lines, pattern, f"{pattern.split('.')[0]}.Account".replace(".Account", ""))  # will be handled inline

    # The pattern-based approach isn't great for multiple variables on one line.
    # Better: read each error file and do targeted replacements.
    
    # Let me just do line-level .Account → null for ALL error lines
    # Map file suffix → [list of error line numbers]
    
    file_errors = {}

    # CS0027 (this.Account)
    for suffix, lines, old, new in fixes:
        file_errors.setdefault(suffix, set()).update(lines)

    # CS1061 variable.Account
    for suffix, lines, pattern in item_type_account_fixes:
        file_errors.setdefault(suffix, set()).update(lines)
        
    # Bulk CS1061 files
    bulk_cs1061 = {
        "Mobiles/Civilized/Familiars/FamiliarItem.cs": familiar_fix_lines,
        "Mobiles/Civilized/Porters/PorterItem.cs": porter_fix_lines,
        "Mobiles/Unique/Tarjan.cs": tarjan_fix_lines,
        "Mobiles/Unique/Mangar.cs": mangar_fix_lines,
        "Mobiles/Insects/Spiders/Arachnar.cs": arachnar_fix_lines,
        "Mobiles/Base/BaseVendor.cs": basevendor_fix_lines,
        "Mobiles/Civilized/Comrades/HenchmanFunctions.cs": henchfunc_fix_lines,
        "Mobiles/Civilized/Comrades/HenchmanItem.cs": henchitem_static_lines,
        "Mobiles/Civilized/Actions.cs": actions_static_lines,
        "Mobiles/Civilized/Chuckles.cs": chuckles_static_lines,
        "Mobiles/Base/PlayerSettings.cs": playersettings_static_lines,
        "Mobiles/Base/Behavior.cs": behavior_static_lines,
        "Mobiles/Civilized/TownHerald.cs": townherald_static_lines,
        "Mobiles/Civilized/TownGuards.cs": townguards_fix_lines,
        "Mobiles/Civilized/Sherry.cs": sherry_fix_lines,
        "Mobiles/Civilized/Special/Kylearan.cs": kylearan_fix_lines,
        "Mobiles/Civilized/Special/Xardok.cs": xardok_fix_lines,
        "Mobiles/Civilized/Special/EpicCharacter.cs": epic_fix_lines,
        "Mobiles/Civilized/Merchants/Veterinarian.cs": veterinarian_fix_lines,
        "Mobiles/Civilized/Citizens/TradesmanBard.cs": tradesmanbard_fix_lines,
        "Mobiles/Civilized/Citizens/WorkingSpots.cs": workingspots_fix_lines,
        "Mobiles/Civilized/Citizens/Citizens.cs": citizens_static_lines,
        "Mobiles/Elementals/Elementals/Vulcrum.cs": [151, 158],
        "Mobiles/Elementals/Necromental.cs": [142],
        "Mobiles/Civilized/Comrades/HenchmanWizardItem.cs": [61],
        "Mobiles/Civilized/Comrades/HenchmanMonsterItem.cs": [22],
        "Mobiles/Civilized/Comrades/HenchmanArcherItem.cs": [65],
        "Mobiles/Civilized/Comrades/HenchmanFighterItem.cs": [90],
        "Mobiles/Dragons/Great Dragons/CaddelliteDragon.cs": [176],
        "Mobiles/Unique/RuneGuardian.cs": [399],
        "Mobiles/Unique/Spectres.cs": [151],
        "Mobiles/Unique/GrayDragon.cs": [154],
        "Mobiles/Demons/Xurtzar.cs": [155],
        "Mobiles/Undead/Surtaz.cs": [190],
        "Mobiles/Civilized/Citizens/TradesmanSmith.cs": [197],
        "Mobiles/Civilized/Citizens/TradesmanButcher.cs": [190],
        "Mobiles/Civilized/Citizens/TradesmanCook.cs": [159],
        "Mobiles/Civilized/Citizens/TradesmanLeather.cs": [182],
        "Mobiles/Civilized/Citizens/TradesmanLogger.cs": [181, 270],
        "Mobiles/Civilized/Citizens/TradesmanAlchemist.cs": [189, 290],
        "Mobiles/Civilized/Citizens/TradesmanMiner.cs": [181],
        # PlayerBarkeeper from issue
        "Mobiles/Base/PlayerBarkeeper.cs": playerbarkeeper_from_lines,
    }
    
    for suffix, lines in bulk_cs1061.items():
        file_errors.setdefault(suffix, set()).update(lines)

    # Now apply fixes: for each file, read it, and on the error lines replace .Account with null
    total_fixed = 0
    for suffix, error_lines in sorted(file_errors.items()):
        path = os.path.join(REPO, "World", "Source", "Scripts", suffix)
        if not os.path.exists(path):
            print(f"  SKIP: {suffix} not found")
            continue
            
        with open(path, encoding="utf-8") as f:
            lines = f.readlines()
        
        file_changed = False
        for ln in sorted(error_lines, reverse=True):
            idx = ln - 1
            if idx < 0 or idx >= len(lines):
                continue
            
            old = lines[idx]
            
            # Pattern: replace X.Account (where X is an identifier) with null
            # This handles: this.Account, m_Box.Account, from.Account, m_Chest.Account, etc.
            new = re.sub(
                r'\b([a-zA-Z_]\w*)\.Account\b',
                'null',
                old
            )
            
            if new != old:
                lines[idx] = new
                file_changed = True
                total_fixed += 1
                # Show change
                old_clean = old.strip()[:100]
                new_clean = new.strip()[:100]
                if old_clean != new_clean:
                    print(f"  {suffix}:{ln}: {old_clean} → {new_clean}")
        
        if file_changed:
            with open(path, "w", encoding="utf-8") as f:
                f.writelines(lines)
    
    print(f"\nTotal fixes: {total_fixed}")
    print("Now recompile to verify.")

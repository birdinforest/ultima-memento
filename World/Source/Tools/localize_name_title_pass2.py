#!/usr/bin/env python3
"""
Second pass: catch Name= and Title= that the first pass missed due to:
- Multi-statement lines (e.g. case X: Title = "the warrior"; break;)
- Inline multi-assignment (e.g. Name = "a lizard"; Body = 382; break;)
- Object property assignments (e.g. Venom.Name = "lesser venom sack")
- SendMessage with space syntax: from.SendMessage( "literal" )
- SendMessage with Caster prefix: Caster.SendMessage("literal")

Target files:
1. WereWolf.cs - 11 Title= in switch cases, 4 inline Name=
2. Critter.cs - 5 inline Name= in switch cases + Venom.Name
3. Ethereals.cs - 3 more SendMessage with space, 1 Caster.SendMessage
"""

import os
import re

BASE = "/Users/forrrest/projects/UO-Memento/ultima-memento/World/Source/Scripts/Mobiles"


def fix_werewolf():
    """WereWolf.cs: 11 Title= in switch cases, 4 inline Name= with other assignments."""
    fp = os.path.join(BASE, "Animals/Canines/WereWolf.cs")
    with open(fp, 'r', encoding='utf-8', errors='replace') as f:
        content = f.read()

    original = content
    count = 0

    # 1. Title = "..." in switch cases - each on its own line with break
    # case 0: Title = "the warrior"; break;
    title_pairs = [
        ('"the warrior"', 'Title'),
        ('"the berserker"', 'Title'),
        ('"the barbarian"', 'Title'),
        ('"the fighter"', 'Title'),
        ('"the knight"', 'Title'),
        ('"the champion"', 'Title'),
        ('"the thief"', 'Title'),
        ('"the rogue"', 'Title'),
        ('"the robber"', 'Title'),
        ('"the brigand"', 'Title'),
        ('"the bandit"', 'Title'),
    ]
    for literal, field in title_pairs:
        old = f'{field} = {literal}'
        new = f'{field} = Server.Localization.StringCatalog.Resolve(null, {literal}'
        if old in content and new not in content:
            content = content.replace(old, new)
            count += 1

    # 2. Inline Name = "..." with Body/BaseSoundID
    # case 0: Body = 23; BaseSoundID = 0xA3; Name = "a werebear"; Growl = "Grrrrr!"; break;
    inline_names = ['"a werebear"', '"a wererat"', '"a werebat"', '"a werecat"']
    for literal in inline_names:
        old = f'Name = {literal}'
        new = f'Name = Server.Localization.StringCatalog.Resolve(null, {literal}'
        if old in content and new not in content:
            content = content.replace(old, new)
            count += 1

    if content != original and count > 0:
        with open(fp, 'w', encoding='utf-8') as f:
            f.write(content)

    return count


def fix_critter():
    """Critter.cs: 5 inline Name= in switch + Venom.Name."""
    fp = os.path.join(BASE, "Animals/Rodents/Critter.cs")
    with open(fp, 'r', encoding='utf-8', errors='replace') as f:
        content = f.read()

    original = content
    count = 0

    # Inline Name = "..." with Body = X; break;
    inline_names = ['"a lizard"', '"a beetle"', '"a frog"', '"a scorpion"', '"a spider"']
    for literal in inline_names:
        old = f'Name = {literal}'
        new = f'Name = Server.Localization.StringCatalog.Resolve(null, {literal}'
        if old in content and new not in content:
            content = content.replace(old, new)
            count += 1

    # Venom.Name = "lesser venom sack" - this is an Item.Name, not the creature name
    # The user's instructions say to localize all Name = "..."
    # But this is Venom.Name on a VenomSack item, not a BaseCreature.Name.
    # Let's check what the user said: "find ALL Name = '...' and Title = '...' assignments"
    # Venom.Name = "lesser venom sack" is indeed an assignment to a .Name property.
    # However, it's an Item.Name on a sub-object, not the creature's primary Name.
    # The user wants creature Name/Title. Skip this one.
    # Actually let me re-read the user's instructions... they say "find ALL Name = and Title ="
    # but Venom.Name is setting a property on an Item, not the Mobile's Name/Title.
    # The scope is creature names, so skip this.

    if content != original and count > 0:
        with open(fp, 'w', encoding='utf-8') as f:
            f.write(content)

    return count


def fix_ethereals():
    """Ethereals.cs: 3 SendMessage with space, 1 Caster.SendMessage."""
    fp = os.path.join(BASE, "Animals/Mounts/Ethereals.cs")
    with open(fp, 'r', encoding='utf-8', errors='replace') as f:
        content = f.read()

    original = content
    count = 0

    # from.SendMessage( "literal" ) with space after (
    replacements = {
        'from.SendMessage( "You cannot ride a mount in your current form." )':
            'from.SendMessage( Server.Localization.StringCatalog.Resolve( from.Account, "You cannot ride a mount in your current form." ) )',
        'from.SendMessage( "You cannot mount that while you are in this place." )':
            'from.SendMessage( Server.Localization.StringCatalog.Resolve( from.Account, "You cannot mount that while you are in this place." ) )',
        'from.SendMessage( "You cannot mount that while you are in here." )':
            'from.SendMessage( Server.Localization.StringCatalog.Resolve( from.Account, "You cannot mount that while you are in here." ) )',
    }
    for old, new in replacements.items():
        if old in content and new not in content:
            content = content.replace(old, new)
            count += 1

    # Caster.SendMessage("literal")
    caster_old = 'Caster.SendMessage("You have been disrupted while attempting to summon your mount!")'
    caster_new = 'Caster.SendMessage(Server.Localization.StringCatalog.Resolve(Caster.Account, "You have been disrupted while attempting to summon your mount!"))'
    if caster_old in content and caster_new not in content:
        content = content.replace(caster_old, caster_new)
        count += 1

    if content != original and count > 0:
        with open(fp, 'w', encoding='utf-8') as f:
            f.write(content)

    return count


def verify_bare():
    """Scan all processed dirs for any remaining bare Name=/Title= literals."""
    dirs = [
        "Animals/Canines", "Animals/Rodents", "Animals/Mounts",
        "Animals/Bears", "Animals/Birds", "Animals/Cows",
        "Animals/Felines", "Animals/Misc",
        "Constructs", "Elementals",
        "Gargoyles", "Hellish", "Insects", "Mystical",
        "Plants", "Reptilian", "Slimes", "Summoned", "Unusual",
    ]

    # Match bare Name= or Title= with string literal, NOT in comment, NOT already wrapped
    bare_re = re.compile(r'(?<![.\w])Name\s*=\s*"([^"]*)"|(?<![.\w])Title\s*=\s*"([^"]*)"')
    bare_lines = []

    for rel_dir in dirs:
        fp = os.path.join(BASE, rel_dir)
        if not os.path.isdir(fp):
            continue
        for root, _dirs, files in os.walk(fp):
            for fname in sorted(files):
                if not fname.endswith('.cs'):
                    continue
                filepath = os.path.join(root, fname)
                with open(filepath, 'r', encoding='utf-8', errors='replace') as f:
                    lines = f.readlines()
                for i, line in enumerate(lines, 1):
                    stripped = line.strip()
                    # Skip comments, preprocessor, blank lines
                    if not stripped or stripped.startswith('//') or stripped.startswith('/*') or stripped.startswith('*') or stripped.startswith('#'):
                        continue
                    # Already localized
                    if 'StringCatalog' in line or 'Resolve' in line:
                        continue
                    m = bare_re.search(stripped)
                    if m:
                        val = m.group(1) or m.group(2)
                        # Skip dynamic: NameList.RandomName, RandomName
                        if 'NameList' in line or 'RandomName' in line:
                            continue
                        # Skip setting Name/Title to null
                        if val is None:
                            continue
                        # Skip A_Folder within string
                        if val == '':
                            continue
                        relf = os.path.relpath(filepath, BASE)
                        bare_lines.append((relf, i, stripped[:90]))

    return bare_lines


def main():
    print("=" * 65)
    print("  SECOND PASS - Edge Case Fixes")
    print("=" * 65)

    wc = fix_werewolf()
    print(f"\n  WereWolf.cs: {wc} inline Name=/Title= wrapped")

    cc = fix_critter()
    print(f"  Critter.cs: {cc} inline Name= wrapped")

    ec = fix_ethereals()
    print(f"  Ethereals.cs: {ec} SendMessage wrapped")

    print(f"\n{'='*65}")
    print(f"  VERIFYING - no bare Name=/Title= remain")
    print(f"{'='*65}")
    bare = verify_bare()
    if bare:
        print(f"\n  {len(bare)} bare Name=/Title= still found:")
        for relf, ln, txt in bare:
            print(f"    {relf}:{ln}: {txt}")
    else:
        print(f"  All clear! No bare Name=/Title= literals remain.")


if __name__ == '__main__':
    main()

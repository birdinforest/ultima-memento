#!/usr/bin/env python3
"""
Localize hardcoded English strings in mobile C# files - pass 2 (fix remaining).

Processes all remaining bare string patterns using group(0) matching to
preserve whitespace.

Usage: python3 World/Source/Tools/localize_more_mobiles_pass2.py
"""

import os
import re

BASE = "/Users/forrrest/projects/UO-Memento/ultima-memento/World/Source/Scripts/Mobiles"

TARGET_DIRS = [
    "Constructs/Golems/IronCobra.cs",
    "Mystical/AncientSphinx.cs",
    "Mystical/Unicorn.cs",
    "Reptilian/BasiliskRiding.cs",
    "Reptilian/Sea/Leviathan.cs",
    "Slimes/OilSlick.cs",
    "Unusual/GorgonRiding.cs",
    "Unusual/UmberHulk.cs",
    "Unusual/Xorn.cs",
    "Insects/Beetles/RuneBeetle.cs",
]

ALREADY_RE = re.compile(r'Server\.Localization\.StringCatalog\.')
NAME_TITLE_RE = re.compile(r'^(\s*)(Name|Title)\s*=\s*"([^"]*)"\s*;')
SENDMSG_SIMPLE_RE = re.compile(r'(\w+)\.SendMessage\(\s*"([^"]+)"\s*\)')
SENDMSG_HUE_RE = re.compile(r'(\w+)\.SendMessage\(\s*(\w+|0x[0-9a-fA-F]+)\s*,\s*"([^"]+)"\s*\)')
OVERHEAD_4ARG_RE = re.compile(r'(\w+)\.(Local|Public)OverheadMessage\(([^,]+),\s*([^,]+),\s*([^,]+),\s*"([^"]+)"\s*\)')
SAY_RE = re.compile(r'(\w+)\.Say\(\s*"([^"]+)"\s*\)')


def has_localization_import(content):
    return 'using Server.Localization;' in content


def fix_line(line):
    """Try to localize any hardcoded strings in a single line."""
    modified = line
    
    # Skip if already localized
    if ALREADY_RE.search(line):
        return modified
    
    # Skip if comment
    if line.strip().startswith('//'):
        return modified
    
    # OverheadMessage(4 args + string)
    m = OVERHEAD_4ARG_RE.search(modified)
    if m and any(c.isalpha() for c in m.group(6)):
        mobile_var = m.group(1)
        text = m.group(6)
        matched = m.group(0)
        replacement = f'{mobile_var}.{m.group(2)}OverheadMessage({m.group(3)}, {m.group(4)}, {m.group(5)}, Server.Localization.StringCatalog.Resolve({mobile_var}.Account, "{text}"))'
        modified = modified.replace(matched, replacement, 1)
    
    # SendMessage(hue, "text")
    m = SENDMSG_HUE_RE.search(modified)
    if m and not ALREADY_RE.search(modified) and any(c.isalpha() for c in m.group(3)):
        mobile_var = m.group(1)
        first_arg = m.group(2)
        text = m.group(3)
        matched = m.group(0)
        replacement = f'{mobile_var}.SendMessage({first_arg}, Server.Localization.StringCatalog.Resolve({mobile_var}.Account, "{text}"))'
        modified = modified.replace(matched, replacement, 1)
    
    # SendMessage("text")
    m = SENDMSG_SIMPLE_RE.search(modified)
    if m and not ALREADY_RE.search(modified) and any(c.isalpha() for c in m.group(2)):
        mobile_var = m.group(1)
        text = m.group(2)
        matched = m.group(0)
        replacement = f'{mobile_var}.SendMessage(Server.Localization.StringCatalog.Resolve({mobile_var}.Account, "{text}"))'
        modified = modified.replace(matched, replacement, 1)
    
    # Say("text")
    if 'SayHued' not in modified and 'SayTo' not in modified:
        m = SAY_RE.search(modified)
        if m and not ALREADY_RE.search(modified) and any(c.isalpha() for c in m.group(2)):
            mobile_var = m.group(1)
            text = m.group(2)
            matched = m.group(0)
            replacement = f'{mobile_var}.Say(Server.Localization.StringCatalog.Resolve({mobile_var}.Account, "{text}"))'
            modified = modified.replace(matched, replacement, 1)
    
    # Name/Title = "text"
    m = NAME_TITLE_RE.match(modified)
    if m and ALREADY_RE.search(modified) is None:
        var = m.group(2)
        text = m.group(3)
        indent = m.group(1)
        modified = f'{indent}{var} = Server.Localization.StringCatalog.Resolve( null, "{text}" );'
    
    return modified


def process_file(filepath):
    with open(filepath, 'r', encoding='utf-8', errors='replace') as f:
        content = f.read()
    
    lines = content.split('\n')
    new_lines = []
    changes = []
    
    for i, line in enumerate(lines):
        fixed = fix_line(line)
        if fixed != line:
            changes.append(f"  L{i+1}")
        new_lines.append(fixed)
    
    content = '\n'.join(new_lines)
    
    if changes:
        if not has_localization_import(content):
            parts = content.split('\n')
            last_using = -1
            for j, l in enumerate(parts):
                if re.match(r'^\s*using\s+[^;]+;\s*$', l):
                    last_using = j
            if last_using >= 0:
                parts.insert(last_using + 1, 'using Server.Localization;')
                content = '\n'.join(parts)
                changes.append("  Added: using Server.Localization;")
        
        with open(filepath, 'w', encoding='utf-8') as f:
            f.write(content)
        return True, changes
    return False, []


def main():
    all_modified = []
    
    for rel_path in TARGET_DIRS:
        filepath = os.path.join(BASE, rel_path)
        if not os.path.isfile(filepath):
            print(f"File not found: {filepath}")
            continue
        
        modified, changes = process_file(filepath)
        if modified:
            all_modified.append((rel_path, changes))
            print(f"MODIFIED: {rel_path}")
            for c in changes:
                print(f"  {c}")
    
    print(f"\n{'='*60}")
    print(f"Total: {len(all_modified)} files modified (pass 2)")
    print(f"{'='*60}")


if __name__ == '__main__':
    main()

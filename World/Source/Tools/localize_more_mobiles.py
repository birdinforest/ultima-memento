#!/usr/bin/env python3
"""
Localize hardcoded English strings in mobile C# files.

Carefully preserves original line endings. Only writes back files
that have actual content changes.

Usage: python3 World/Source/Tools/localize_more_mobiles.py
"""

import os
import re

BASE = "/Users/forrrest/projects/UO-Memento/ultima-memento/World/Source/Scripts/Mobiles"

TARGET_DIRS = [
    "Constructs/",
    "Mystical/",
    "Plants/",
    "Reptilian/",
    "Slimes/",
    "Summoned/",
    "Unusual/",
    "Hellish/",
    "Insects/",
    "Gargoyles/",
    "Omni AI/",
]

ALREADY_RE = re.compile(r'Server\.Localization\.StringCatalog\.')

def is_dynamic(s):
    """True if Name/Title assignment uses a dynamic expression, not a literal."""
    return any(kw in s for kw in ['RandomName', 'RandomThings', 'Random(', 'NameList', '.Loot'])

def has_localization_import(content):
    return 'using Server.Localization;' in content

def process_file(filepath):
    # Read with newline='' to preserve original line endings
    with open(filepath, 'r', encoding='utf-8', errors='replace', newline='') as f:
        content = f.read()
    
    # Detect line ending
    if '\r\n' in content:
        newline = '\r\n'
    else:
        newline = '\n'
    
    lines = content.split(newline)
    new_lines = []
    changes = []
    needs_import = False
    
    for i, line in enumerate(lines):
        modified = line
        stripped = line.strip()
        
        if ALREADY_RE.search(line):
            new_lines.append(line)
            continue
        
        # Skip comments
        if stripped.startswith('//'):
            new_lines.append(line)
            continue
        
        # Name = "text";
        m = re.match(r'^(\s*)(Name)\s*=\s*"([^"]*)"\s*;?\s*$', line)
        if m and not is_dynamic(line):
            indent, var, text = m.group(1), m.group(2), m.group(3)
            modified = f'{indent}{var} = Server.Localization.StringCatalog.Resolve( null, "{text}" );'
            changes.append(f"  L{i+1}: {var} = \"{text}\"")
            needs_import = True
            new_lines.append(modified)
            continue
        
        # Title = "text";
        m = re.match(r'^(\s*)(Title)\s*=\s*"([^"]*)"\s*;?\s*$', line)
        if m and not is_dynamic(line):
            indent, var, text = m.group(1), m.group(2), m.group(3)
            modified = f'{indent}{var} = Server.Localization.StringCatalog.Resolve( null, "{text}" );'
            changes.append(f"  L{i+1}: {var} = \"{text}\"")
            needs_import = True
            new_lines.append(modified)
            continue
        
        # .SendMessage("text") or .SendMessage(hue, "text") or m.SendMessage( "text" )
        # Use m.group(0) to preserve original whitespace
        for pattern_fn, label in [
            (lambda ln: fix_sendmessage(ln), "SendMessage"),
            (lambda ln: fix_overhead(ln), "OverheadMessage"),
            (lambda ln: fix_say(ln), "Say"),
        ]:
            if not ALREADY_RE.search(modified):
                result = pattern_fn(modified)
                if result != modified:
                    modified = result
                    needs_import = True
                    # Only add one change per line
                    if not changes or changes[-1] != f"  L{i+1}: {label}":
                        changes.append(f"  L{i+1}: {label}")
        
        new_lines.append(modified)
    
    if changes and needs_import and not has_localization_import(content):
        # Insert using after last using statement
        last_using = -1
        for j, l in enumerate(new_lines):
            if re.match(r'^\s*using\s+[^;]+;\s*$', l):
                last_using = j
        if last_using >= 0:
            new_lines.insert(last_using + 1, 'using Server.Localization;')
            changes.append("  Added: using Server.Localization;")
    
    new_content = newline.join(new_lines)
    
    if new_content != content:
        with open(filepath, 'w', encoding='utf-8', newline=newline) as f:
            f.write(new_content)
        return True, changes
    
    return False, []


def fix_sendmessage(line):
    """Fix SendMessage patterns in a line, preserving whitespace."""
    # .SendMessage( "text" )
    m = re.search(r'(\w+)\.SendMessage\(\s*"([^"]+)"\s*\)', line)
    if m and any(c.isalpha() for c in m.group(2)):
        var, text = m.group(1), m.group(2)
        # Get exactly what was matched
        matched = m.group(0)
        replacement = f'{var}.SendMessage(Server.Localization.StringCatalog.Resolve({var}.Account, "{text}"))'
        if matched in line:
            return line.replace(matched, replacement, 1)
    
    # .SendMessage(hue, "text")
    m = re.search(r'(\w+)\.SendMessage\(\s*(\w+\s*,\s*)"([^"]+)"\s*\)', line)
    if m and not ALREADY_RE.search(line) and any(c.isalpha() for c in m.group(3)):
        var, first_part, text = m.group(1), m.group(2), m.group(3)
        matched = m.group(0)
        replacement = f'{var}.SendMessage({first_part}Server.Localization.StringCatalog.Resolve({var}.Account, "{text}"))'
        if matched in line:
            return line.replace(matched, replacement, 1)
    
    # .SendMessage( hue , "text" )  (space around hue)
    m = re.search(r'(\w+)\.SendMessage\(\s*(0x[0-9a-fA-F]+)\s*,\s*"([^"]+)"\s*\)', line)
    if m and not ALREADY_RE.search(line) and any(c.isalpha() for c in m.group(3)):
        var, first_arg, text = m.group(1), m.group(2), m.group(3)
        matched = m.group(0)
        replacement = f'{var}.SendMessage({first_arg}, Server.Localization.StringCatalog.Resolve({var}.Account, "{text}"))'
        if matched in line:
            return line.replace(matched, replacement, 1)
    
    return line


def fix_overhead(line):
    """Fix LocalOverheadMessage / PublicOverheadMessage patterns."""
    # .method(MessageType.X, hue, bool, "text")
    m = re.search(r'(\w+)\.(Local|Public)OverheadMessage\(([^,]+),\s*([^,]+),\s*([^,]+),\s*"([^"]+)"\s*\)', line)
    if m and not ALREADY_RE.search(line) and any(c.isalpha() for c in m.group(6)):
        var = m.group(1)
        method = m.group(2)
        arg1 = m.group(3).rstrip()
        arg2 = m.group(4).rstrip()
        arg3 = m.group(5).rstrip()
        text = m.group(6)
        matched = m.group(0)
        replacement = f'{var}.{method}OverheadMessage({arg1}, {arg2}, {arg3}, Server.Localization.StringCatalog.Resolve({var}.Account, "{text}"))'
        if matched in line:
            return line.replace(matched, replacement, 1)
    return line


def fix_say(line):
    """Fix Say patterns, excluding SayHued and SayTo."""
    if 'SayHued' in line or 'SayTo' in line:
        return line
    # .Say("text")
    m = re.search(r'(\w+)\.Say\(\s*"([^"]+)"\s*\)', line)
    if m and not ALREADY_RE.search(line) and any(c.isalpha() for c in m.group(2)):
        var, text = m.group(1), m.group(2)
        matched = m.group(0)
        replacement = f'{var}.Say(Server.Localization.StringCatalog.Resolve({var}.Account, "{text}"))'
        if matched in line:
            return line.replace(matched, replacement, 1)
    return line


def main():
    all_modified = []
    
    for rel_dir in TARGET_DIRS:
        full_dir = os.path.join(BASE, rel_dir)
        if not os.path.isdir(full_dir):
            print(f"Directory not found: {full_dir}")
            continue
        
        for root, dirs, files in os.walk(full_dir):
            for fname in sorted(files):
                if not fname.endswith('.cs'):
                    continue
                filepath = os.path.join(root, fname)
                rel_path = os.path.relpath(filepath, BASE)
                
                modified, changes = process_file(filepath)
                if modified:
                    all_modified.append((rel_path, changes))
                    print(f"MODIFIED: {rel_path}")
                    for c in changes:
                        print(f"  {c}")
    
    print(f"\n{'='*60}")
    print(f"Total: {len(all_modified)} files modified")
    print(f"{'='*60}")
    for rel_path, changes in all_modified:
        print(f"\n{rel_path}:")
        for c in changes:
            print(f"  {c}")


if __name__ == '__main__':
    main()

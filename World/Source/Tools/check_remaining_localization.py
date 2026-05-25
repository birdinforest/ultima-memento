#!/usr/bin/env python3
"""
Find remaining hardcoded English strings in mobile C# files after first pass.
Checks for:
1. Name = "text" still remaining (not wrapped with StringCatalog)
2. Title = "text" still remaining
3. SendMessage / Say / OverheadMessage with bare string literals

Also checks for base constructor name arguments (these are harder to fix).

Usage: python3 World/Source/Tools/check_remaining_localization.py
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

SKIP_DYNAMIC = re.compile(r'(RandomName|RandomThings|StringCatalog|NameList|\.Random)')

def find_remaining(filepath):
    """Find remaining hardcoded strings that should be localized."""
    with open(filepath, 'r', encoding='utf-8', errors='replace') as f:
        content = f.read()

    lines = content.split('\n')
    remaining = []
    
    for i, line in enumerate(lines):
        stripped = line.strip()
        lineno = i + 1
        
        # Skip comments
        if stripped.startswith('//'):
            continue
        
        # Skip CorpseName and other attribute patterns
        if '[CorpseName(' in stripped:
            continue
        
        # Skip using statements
        if stripped.startswith('using '):
            continue
        
        # Already localized
        if 'StringCatalog' in stripped:
            continue
        
        # Name = "text" - string literal, not dynamic
        m = re.match(r'\s*Name\s*=\s*"([^"]*)"\s*;', line)
        if m and SKIP_DYNAMIC.search(stripped) is None:
            remaining.append((lineno, 'Name', m.group(1)))
            continue
        
        # Title = "text"
        m = re.match(r'\s*Title\s*=\s*"([^"]*)"\s*;', line)
        if m and SKIP_DYNAMIC.search(stripped) is None:
            remaining.append((lineno, 'Title', m.group(1)))
            continue
        
        # SendMessage("text")
        m = re.search(r'\.SendMessage\(\s*"([^"]+)"\s*\)', line)
        if m and any(c.isalpha() for c in m.group(1)):
            remaining.append((lineno, 'SendMessage', m.group(1)))
            continue
        
        # SendMessage(hue, "text")
        m = re.search(r'\.SendMessage\(\s*\w+\s*,\s*"([^"]+)"\s*\)', line)
        if m and any(c.isalpha() for c in m.group(1)) and not m.group(1).startswith('Server.'):
            remaining.append((lineno, 'SendMessage(hue)', m.group(1)))
            continue
        
        # Say("text") - but not SayHued/SayTo
        if 'SayHued' not in stripped and 'SayTo' not in stripped:
            m = re.search(r'\.Say\(\s*"([^"]+)"\s*\)', line)
            if m and any(c.isalpha() for c in m.group(1)):
                remaining.append((lineno, 'Say', m.group(1)))
                continue
        
        # OverheadMessage(..., "text")
        m = re.search(r'\.(Local|Public)OverheadMessage\([^,]+,\s*[^,]+,\s*[^,]+,\s*"([^"]+)"\s*\)', line)
        if m and any(c.isalpha() for c in m.group(2)):
            remaining.append((lineno, 'OverheadMessage', m.group(2)))
            continue
        
        # PublicOverheadMessage without the extra bool argument (2-arg form)
        m = re.search(r'\.PublicOverheadMessage\(\s*[^,]+,\s*"([^"]+)"\s*\)', line)
        if m and any(c.isalpha() for c in m.group(1)):
            remaining.append((lineno, 'PublicOverheadMessage', m.group(1)))
            continue
    
    return remaining

def main():
    all_remaining = {}
    
    for rel_dir in TARGET_DIRS:
        full_dir = os.path.join(BASE, rel_dir)
        if not os.path.isdir(full_dir):
            continue
        
        for root, dirs, files in os.walk(full_dir):
            for fname in sorted(files):
                if not fname.endswith('.cs'):
                    continue
                filepath = os.path.join(root, fname)
                rel_path = os.path.relpath(filepath, BASE)
                remaining = find_remaining(filepath)
                if remaining:
                    all_remaining[rel_path] = remaining
                    print(f"\n{rel_path}:")
                    for lineno, kind, text in remaining:
                        print(f"  L{lineno}: {kind} = \"{text}\"")
    
    if not all_remaining:
        print("\n✓ No remaining hardcoded strings found!")
    else:
        total = sum(len(v) for v in all_remaining.values())
        print(f"\n{'='*60}")
        print(f"Total: {len(all_remaining)} files with {total} remaining hardcoded strings")
        print(f"{'='*60}")

if __name__ == '__main__':
    main()

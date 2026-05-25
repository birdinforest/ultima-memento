#!/usr/bin/env python3
"""Verify no bare Name= / Title= with string literals remain in processed files."""
import os
import re

BASE = "/Users/forrrest/projects/UO-Memento/ultima-memento/World/Source/Scripts/Mobiles"

dirs = [
    "Animals", "Constructs", "Elementals",
    "Gargoyles", "Hellish", "Insects", "Mystical",
    "Plants", "Reptilian", "Slimes", "Summoned", "Unusual",
]

# Match base-level Name = or Title = with string literal
# This matches at the start of a statement (not preceded by word char or dot)
bare_re = re.compile(r'(?<![.\w])(Name|Title)\s*=\s*"([^"]*)"')

total = 0
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
                # Skip comments, blank, etc.
                if not stripped or stripped.startswith('//') or stripped.startswith('/*') or stripped.startswith('*') or stripped.startswith('#') or stripped.startswith('/*'):
                    continue
                if 'StringCatalog' in line or 'Resolve' in line or 'Localized' in line:
                    continue
                m = bare_re.search(stripped)
                if m:
                    val = m.group(2)
                    if val == '' or val is None:
                        continue
                    # Skip NameList.RandomName
                    if 'NameList' in line or 'RandomName' in line:
                        continue
                    relf = os.path.relpath(filepath, BASE)
                    print(f"  BARE: {relf}:{i}: {stripped[:100]}")
                    total += 1

print(f"\n  Total bare Name=/Title= with literals: {total}")

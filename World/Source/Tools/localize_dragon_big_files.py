#!/usr/bin/env python3
"""
Localize hardcoded strings in the big dragon switch-case files.
Handles:
  - rName = "..."  →  rName = StringCatalog.Resolve(null, "...")
  - broke.Name = "..."  →  StringCatalog.Resolve
  - LocalOverheadMessage with English strings
  - egg.Name = "egg of " + this.Title/Name  →  ResolveFormat
  - LocalOverheadMessage rust patterns
"""

import re
import os

DRAGONS_DIR = "/Users/forrrest/projects/UO-Memento/ultima-memento/World/Source/Scripts/Mobiles/Dragons"

def localize_file(filepath, has_egg_title=True, has_egg_name=False):
    """Localize strings in a dragon file."""
    with open(filepath, 'r') as f:
        content = f.read()
    
    changes = []
    
    # 1. LocalOverheadMessage with string literals (rust messages)
    # Pattern: m.LocalOverheadMessage(MessageType.Emote, 1150, true, "text")
    def replace_local_overhead(m):
        prefix = m.group(1)
        hue = m.group(2)
        text = m.group(3)
        # Get the mobile variable name for Account resolution
        # For m.LocalOverheadMessage -> m.Account, defender -> defender.Account, etc.
        # We use the object before .LocalOverheadMessage
        obj_expr = m.group(1).rstrip('.')
        # Check if it has a closing paren from a complex expression
        replacement = f'{prefix}{hue}, true, Server.Localization.StringCatalog.Resolve({obj_expr}.Account, "{text}")'
        changes.append(f"LocalOverheadMessage: \"{text}\"")
        return replacement
    
    # Match: something.LocalOverheadMessage(MessageType.Emote, 0xNNNN, true, "text")
    content = re.sub(
        r'((?:[\w.]+)\.LocalOverheadMessage\(MessageType\.Emote,\s*(0x[0-9A-Fa-f]+),\s*true,\s*)"([^"]+)"\)',
        replace_local_overhead,
        content
    )
    
    # 2. rName = "..." assignments
    def replace_rname(m):
        text = m.group(1)
        changes.append(f"rName: \"{text}\"")
        return f'rName = Server.Localization.StringCatalog.Resolve(null, "{text}")'
    
    content = re.sub(
        r'rName\s*=\s*"([^"]+)"',
        replace_rname,
        content
    )
    
    # 3. broke.Name = "..." (rusty junk)
    def replace_broke_name(m):
        text = m.group(1)
        changes.append(f"broke.Name: \"{text}\"")
        return f'broke.Name = Server.Localization.StringCatalog.Resolve(null, "{text}")'
    
    content = re.sub(
        r'broke\.Name\s*=\s*"([^"]+)"',
        replace_broke_name,
        content
    )
    
    # 4. egg.Name = "egg of " + this.Title/Name → ResolveFormat
    if has_egg_title:
        content = re.sub(
            r'egg\.Name\s*=\s*"egg of "\s*\+\s*this\.Title',
            'egg.Name = Server.Localization.StringCatalog.ResolveFormat(null, "egg of {0}", this.Title)',
            content
        )
        changes.append("egg.Name: \"egg of {0}\" (from this.Title)")
    
    if has_egg_name:
        content = re.sub(
            r'egg\.Name\s*=\s*"egg of "\s*\+\s*this\.Name',
            'egg.Name = Server.Localization.StringCatalog.ResolveFormat(null, "egg of {0}", this.Name)',
            content
        )
        changes.append("egg.Name: \"egg of {0}\" (from this.Name)")
    
    # Also handle Name = "..." and Title = "..." assignments
    # (but NOT Name = NameList.RandomName(...) which is dynamic)
    def replace_name_title(m):
        kind = m.group(1)  # Name or Title
        text = m.group(2)
        changes.append(f"{kind}: \"{text}\"")
        return f'{kind} = Server.Localization.StringCatalog.Resolve(null, "{text}")'
    
    content = re.sub(
        r'^\s*(Name|Title)\s*=\s*"([^"]+)"\s*$',
        replace_name_title,
        content,
        flags=re.MULTILINE
    )
    
    with open(filepath, 'w') as f:
        f.write(content)
    
    return changes

def main():
    # Files to process
    files_config = [
        # (relative_path, has_egg_title, has_egg_name)
        ("Wyrms/Wyrms.cs", True, False),
        ("Dragons/Dragons.cs", False, True),  # uses this.Name instead of this.Title
        ("Dragons/RidingDragon.cs", False, True),
        ("Young/YoungDragon.cs", False, False),
    ]
    
    for rel_path, has_egg_title, has_egg_name in files_config:
        full_path = os.path.join(DRAGONS_DIR, rel_path)
        if not os.path.exists(full_path):
            print(f"WARNING: File not found: {full_path}")
            continue
        
        print(f"\n=== Processing: {rel_path} ===")
        
        # Read file and get original content
        with open(full_path, 'r') as f:
            original = f.read()
        
        # Check if using Server.Localization already exists
        has_using = "using Server.Localization;" in original
        
        changes = localize_file(full_path, has_egg_title, has_egg_name)
        
        if not has_using:
            # Add using statement after the last using
            with open(full_path, 'r') as f:
                content = f.read()
            
            # Find the last using line and add after it
            lines = content.split('\n')
            last_using_idx = -1
            for i, line in enumerate(lines):
                if line.strip().startswith('using ') and line.strip().endswith(';'):
                    last_using_idx = i
            
            if last_using_idx >= 0:
                lines.insert(last_using_idx + 1, 'using Server.Localization;')
                with open(full_path, 'w') as f:
                    f.write('\n'.join(lines))
        
        print(f"  Changes made: {len(changes)}")
        for c in changes:
            print(f"    - {c}")

if __name__ == "__main__":
    main()

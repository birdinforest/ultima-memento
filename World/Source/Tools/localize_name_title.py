#!/usr/bin/env python3
"""
Localize Name = and Title = hardcoded strings in mobile C# files.
Also handle SendMessage in Ethereals.cs.

For each .cs file in given directories:
- Wraps Name = "..." and Title = "..." with StringCatalog.Resolve(null, "...")
- For Ethereals.cs: also wraps from.SendMessage("...") with StringCatalog.Resolve(from.Account, "...")
- Adds using Server.Localization; if not already present
"""

import os
import re

# Pattern: Name = "literal" or Title = "literal"
NAME_TITLE_RE = re.compile(
    r'^(?P<indent>\s*)(?P<field>Name|Title)\s*=\s*"(?P<value>[^"]*)"\s*;\s*$'
)

# Pattern for SendMessage: from.SendMessage("literal")
SENDMSG_RE = re.compile(r'(from\.SendMessage\()"([^"]*)"(\s*\))')

# Detection patterns
USING_RE = re.compile(r'^\s*using\s+Server\.Localization\s*;\s*')
NAMESPACE_RE = re.compile(r'^\s*namespace\s+')

MOBILES_DIR = "/Users/forrrest/projects/UO-Memento/ultima-memento/World/Source/Scripts/Mobiles"


def process_file(filepath, treat_sendmsg=False):
    """Process a single .cs file. Returns (changed, name_count, title_count, msg_count)."""
    with open(filepath, 'r', encoding='utf-8', errors='replace') as f:
        lines = f.readlines()

    new_lines = []
    name_count = 0
    title_count = 0
    msg_count = 0
    has_using = False
    last_using_idx = -1

    for i, line in enumerate(lines):
        # Check for existing using Server.Localization;
        if USING_RE.match(line):
            has_using = True
            last_using_idx = i

        # If line already has StringCatalog or isn't a plain Name/Title literal, pass through
        if 'StringCatalog' in line:
            new_lines.append(line)
            continue

        # Check for Name = / Title = pattern
        m = NAME_TITLE_RE.match(line)
        if m:
            indent = m.group('indent')
            field = m.group('field')
            value = m.group('value')
            new_line = f'{indent}{field} = Server.Localization.StringCatalog.Resolve(null, "{value}");\n'
            new_lines.append(new_line)
            if field == 'Name':
                name_count += 1
            else:
                title_count += 1
            continue

        # Handle SendMessage for Ethereals.cs
        if treat_sendmsg:
            new_line = SENDMSG_RE.sub(
                r'\1Server.Localization.StringCatalog.Resolve(from.Account, "\2"\3', line
            )
            if new_line != line:
                msg_count += len(SENDMSG_RE.findall(line))
                new_lines.append(new_line)
                continue

        new_lines.append(line)

    total_changes = name_count + title_count + msg_count

    # Add using directive if not present and changes were made
    if total_changes > 0 and not has_using:
        # Find the right insertion point
        insert_at = 0
        # If we have a using line, insert after it (and any blank lines)
        if last_using_idx >= 0:
            insert_at = last_using_idx + 1
            while insert_at < len(new_lines) and new_lines[insert_at].strip() == '':
                insert_at += 1
        else:
            # Find the namespace declaration, or the first meaningful line
            for i, line in enumerate(new_lines):
                if NAMESPACE_RE.match(line):
                    insert_at = i
                    break
                stripped = line.strip()
                if stripped and not stripped.startswith('//') and not stripped.startswith('/*') \
                        and not stripped.startswith('*') and not stripped.startswith('#'):
                    # Walk back past blank lines
                    insert_at = i
                    while insert_at > 0 and new_lines[insert_at - 1].strip() == '':
                        insert_at -= 1
                    break

        new_lines.insert(insert_at, "using Server.Localization;\n")

    if total_changes > 0:
        with open(filepath, 'w', encoding='utf-8') as f:
            f.writelines(new_lines)

    return (total_changes > 0, name_count, title_count, msg_count)


def main():
    # Directories to process (relative to Mobiles/ dir)
    # Key: relative path, Value: treat Ethereals.cs specially
    dirs = {
        # Animals subdirectories
        "Animals/Bears": False,
        "Animals/Birds": False,
        "Animals/Canines": False,
        "Animals/Cows": False,
        "Animals/Felines": False,
        "Animals/Misc": False,
        "Animals/Mounts": True,   # Ethereals.cs has SendMessage
        "Animals/Rodents": False,
        # Constructs (process root + all subdirs recursively)
        "Constructs": False,
        # Elementals (process root + all subdirs recursively)
        "Elementals": False,
        # Other directories (process recursively)
        "Gargoyles": False,
        "Hellish": False,
        "Insects": False,
        "Mystical": False,
        "Plants": False,
        "Reptilian": False,
        "Slimes": False,
        "Summoned": False,
        "Unusual": False,
    }

    grand_total_changed = 0
    grand_total_name = 0
    grand_total_title = 0
    grand_total_msg = 0

    for rel_dir, is_mounts in dirs.items():
        full_path = os.path.join(MOBILES_DIR, rel_dir)
        if not os.path.isdir(full_path):
            print(f"[SKIP] {rel_dir} — not found")
            continue

        print(f"\n{'='*65}")
        print(f"  Processing: {rel_dir}/")
        print(f"{'='*65}")

        dir_changed = 0
        dir_name_count = 0
        dir_title_count = 0
        dir_msg_count = 0

        # Walk through all .cs files recursively
        for root, _dirs, files in os.walk(full_path):
            for fname in sorted(files):
                if not fname.endswith('.cs'):
                    continue
                filepath = os.path.join(root, fname)

                # Determine relative display path
                rel_file = os.path.relpath(filepath, MOBILES_DIR)

                # Only Ethereals.cs in Animals/Mounts gets SendMessage treatment
                treat_sendmsg = (is_mounts and fname == "Ethereals.cs")

                changed, nc, tc, mc = process_file(filepath, treat_sendmsg)
                if changed:
                    dir_changed += 1
                    dir_name_count += nc
                    dir_title_count += tc
                    dir_msg_count += mc
                    details = []
                    if nc: details.append(f"{nc} Name")
                    if tc: details.append(f"{tc} Title")
                    if mc: details.append(f"{mc} SendMsg")
                    print(f"  {rel_file}: {', '.join(details)}")

        if dir_changed == 0:
            print(f"  (no changes needed)")

        grand_total_changed += dir_changed
        grand_total_name += dir_name_count
        grand_total_title += dir_title_count
        grand_total_msg += dir_msg_count

    print(f"\n{'='*65}")
    print("  FINAL SUMMARY")
    print(f"{'='*65}")
    print(f"  Files modified:  {grand_total_changed}")
    print(f"  Name = wraps:    {grand_total_name}")
    print(f"  Title = wraps:   {grand_total_title}")
    print(f"  SendMessage:     {grand_total_msg}")
    print(f"  Total changes:   {grand_total_name + grand_total_title + grand_total_msg}")
    print(f"{'='*65}")


if __name__ == '__main__':
    main()

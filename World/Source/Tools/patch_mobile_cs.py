#!/usr/bin/env python3
"""
Patch C# files: replace StringCatalog.Resolve(text) → StringCatalog.ResolveByKey(key).
Uses mobile-shotkey-mapping.json + en/world-player-text.json for reverse lookup.
"""
import json
import re
import os
import sys

REPO = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
SCRIPTS_DIR = os.path.join(REPO, "Source", "Scripts", "Mobiles")
MAPPING_PATH = os.path.join(REPO, "Data", "Localization", "tools-output", "mobile-shotkey-mapping.json")
WPT_EN_PATH = os.path.join(REPO, "Data", "Localization", "en", "world-player-text.json")


def load_json(p):
    if os.path.exists(p):
        with open(p, encoding="utf-8") as f:
            return json.load(f)
    return {}


def build_reverse_lookup():
    """Build English text → shotkey from mapping + WPT EN."""
    mapping = load_json(MAPPING_PATH)
    wpt_en = load_json(WPT_EN_PATH)

    text_to_key = {}
    for hk, sk in mapping.items():
        en_text = wpt_en.get(sk, "")
        if en_text:
            text_to_key[en_text] = sk

    print(f"Reverse lookup from WPT EN: {len(text_to_key)} texts → shotkeys")

    # Verify with sample lookups
    samples = [
        "All should fear ",
        "Your life ends here!",
        "the dracolich",
        "Speak to me when that strange effect has worn off.",
        "I don't think we could let someone like you join.",
        "You have resigned from the local guild.",
        "Here is your replacement ring.",
        "Your friend is not dead.",
        "You do not have enough gold.",
        "You may not ride this creature.",
    ]
    found = 0
    for s in samples:
        if s in text_to_key:
            print(f"  ✓ '{s[:45]}' → {text_to_key[s]}")
            found += 1
        else:
            print(f"  ✗ '{s[:45]}' NOT FOUND")
    print(f"  {found}/{len(samples)} samples matched")
    return text_to_key


def patch_cs_files(text_to_key):
    """Replace all StringCatalog.Resolve(text) with ResolveByKey(key)."""
    total_files = 0
    total_changed = 0
    total_replaced = 0
    skipped_texts = set()

    # Pattern: StringCatalog.Resolve(prefix, "literal") or StringCatalog.ResolveFormat(prefix, "literal", args...)
    # Group 1: method name (Resolve or ResolveFormat)
    # Group 2: prefix (null, m.Account, from.Account, player.Account, etc.)
    # Group 3: string literal
    # Group 4: remaining args for ResolveFormat (e.g. ", arg0, arg1")
    pattern = re.compile(
        r'StringCatalog\.(Resolve(?:Format)?)\s*\(\s*([^,]+?)\s*,\s*"((?:[^"\\]|\\.)*)"\s*((?:,\s*[^)]+)*)\s*\)'
    )

    for root, dirs, files in os.walk(SCRIPTS_DIR):
        for fn in files:
            if not fn.endswith(".cs"):
                continue
            path = os.path.join(root, fn)
            total_files += 1

            with open(path, encoding="utf-8") as f:
                content = f.read()

            original = content
            this_changed = 0

            def replace_resolve(m):
                nonlocal this_changed
                meth = m.group(1)
                prefix = m.group(2).strip()
                inner = m.group(3)
                extra = m.group(4)

                if inner not in text_to_key:
                    skipped_texts.add(inner)
                    return m.group(0)

                sk = text_to_key[inner]
                this_changed += 1

                if meth == "ResolveFormat":
                    return f'StringCatalog.ResolveFormatByKey({prefix}, "{sk}"{extra})'
                else:
                    return f'StringCatalog.ResolveByKey({prefix}, "{sk}")'

            new_content = re.sub(pattern, replace_resolve, content)

            if new_content != original:
                with open(path, "w", encoding="utf-8") as f:
                    f.write(new_content)
                print(f"  [{this_changed:3d}] patched: {os.path.relpath(path, REPO)}")
                total_changed += 1
                total_replaced += this_changed

    print(f"\nScanned: {total_files} files")
    print(f"Changed: {total_changed} files, {total_replaced} replacements")

    if skipped_texts:
        print(f"\nUnmatched texts ({len(skipped_texts)}):")
        for t in sorted(skipped_texts)[:15]:
            print(f"  '{t[:60]}'")
        if len(skipped_texts) > 15:
            print(f"  ... ({len(skipped_texts) - 15} more)")

    # Check for remaining StringCatalog.Resolve calls (that aren't already ByKey)
    remaining = 0
    remaining_examples = []
    for root, dirs, files in os.walk(SCRIPTS_DIR):
        for fn in files:
            if not fn.endswith(".cs"):
                continue
            path = os.path.join(root, fn)
            with open(path, encoding="utf-8") as f:
                c = f.read()
            for m in pattern.finditer(c):
                inner = m.group(3)
                # Skip if already a named key
                if inner.startswith(("s.", "mob.", "prop.", "item.", "quest.", "book.",
                                      "eng.", "sys.", "charrestore.", "trap.", "race.",
                                      "chat3.", "god.")):
                    continue
                remaining += 1
                if len(remaining_examples) < 10:
                    remaining_examples.append((inner[:60], os.path.relpath(path, REPO)))
    if remaining:
        print(f"\nRemaining non-keyed StringCatalog.Resolve calls: {remaining}")
        for txt, p in remaining_examples:
            print(f"  '{txt}' in {p}")


if __name__ == "__main__":
    text_to_key = build_reverse_lookup()
    if len(text_to_key) < 100:
        print("ERROR: Too few lookups (<100), aborting.")
        sys.exit(1)
    patch_cs_files(text_to_key)

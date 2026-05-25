#!/usr/bin/env python3
"""
Patch C# files in Mobiles/: replace bare English literals in Say/SendMessage/
OverheadMessage/LabelTo/Broadcast with StringCatalog.ResolveByKey calls.

Strategy: For each text in the shotkey map, find `"text"` occurrences and replace
with `StringCatalog.ResolveByKey(account, "mob.key")`, detecting the calling
method context to determine the correct Account expression.
"""

import json
import re
import os
import sys
import shutil

REPO = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
SCRIPTS_DIR = os.path.join(REPO, "Source", "Scripts", "Mobiles")
WPT_EN_PATH = os.path.join(REPO, "Data", "Localization", "en", "world-player-text.json")
MAPPING_PATH = os.path.join(REPO, "Data", "Localization", "tools-output", "mobile-shotkey-mapping.json")
BACKUP_DIR = os.path.join(REPO, "Data", "Localization", "tools-output", "backup-mobile-bare-patch")

stats = {"scanned": 0, "changed": 0, "replaced": 0}
using_added = 0


def load_json(p):
    try:
        with open(p, encoding="utf-8") as f:
            return json.load(f)
    except:
        return {}


def build_text_to_key():
    """English text → mob.* shotkey."""
    wpt_en = load_json(WPT_EN_PATH)
    mapping = load_json(MAPPING_PATH)
    t2k = {}
    for hk, sk in mapping.items():
        en = wpt_en.get(sk, "")
        if en:
            t2k[en] = sk
    for k, v in wpt_en.items():
        if k.startswith("mob.") and v and v not in t2k:
            t2k[v] = k
    return t2k


def backup_file(path):
    rel = os.path.relpath(path, REPO)
    bp = os.path.join(BACKUP_DIR, rel)
    os.makedirs(os.path.dirname(bp), exist_ok=True)
    shutil.copy2(path, bp)


def add_using(content):
    global using_added
    if 'using Server.Localization;' in content:
        return content
    lines = content.split('\n')
    last = -1
    for i, line in enumerate(lines):
        s = line.strip()
        if s.startswith('using ') and s.endswith(';') and '=' not in s and '(' not in s:
            last = i
    if last >= 0:
        lines.insert(last + 1, 'using Server.Localization;')
        using_added += 1
        return '\n'.join(lines)
    return content


def detect_account(context, literal_pos, content):
    """
    Look backward from the literal to determine the account expression.
    """
    ctx = context  # already the ~200 chars before literal

    # Order by specificity
    # LabelTo(mob, "text")
    idx = ctx.rfind('LabelTo(')
    if idx >= 0 and 'LabelTo(' in ctx[-80:]:
        mob_m = re.search(r'LabelTo\s*\(\s*([a-zA-Z_]\w*)\s*,', ctx[idx:])
        if mob_m:
            return f"{mob_m.group(1)}.Account"

    # AddHtml / AddLabel (Gump context)
    for meth in ('AddHtml(', 'AddLabel('):
        if meth in ctx[-80:]:
            return "from.Account"

    # Broadcast
    if 'Broadcast(' in ctx[-60:]:
        return "null"

    # PublicOverheadMessage / LocalOverheadMessage
    for meth in ('PublicOverheadMessage(', 'LocalOverheadMessage('):
        if meth in ctx[-80:]:
            m = re.search(r'([a-zA-Z_]\w*)\.' + re.escape(meth), ctx[-150:])
            if m:
                p = m.group(1)
                return f"{p}.Account" if p not in ('this', 'base') else "this.Account"
            return "this.Account"

    # SendMessage / SendAsciiMessage
    for meth in ('SendMessage(', 'SendAsciiMessage('):
        if meth in ctx[-80:]:
            m = re.search(r'([a-zA-Z_]\w*)\.' + re.escape(meth), ctx[-150:])
            if m:
                p = m.group(1)
                return f"{p}.Account" if p not in ('this', 'base') else "this.Account"
            return "this.Account"

    # Say
    if 'Say(' in ctx[-80:]:
        m = re.search(r'([a-zA-Z_]\w*)\.Say\s*\(', ctx[-150:])
        if m:
            p = m.group(1)
            return f"{p}.Account" if p not in ('this', 'base') else "this.Account"
        return "this.Account"

    return "this.Account"


def patch_file(path, t2k):
    """Apply all replacements to a .cs file."""
    global stats
    stats["scanned"] += 1
    with open(path, encoding="utf-8") as f:
        content = f.read()
    original = content

    texts_sorted = sorted(t2k.keys(), key=len, reverse=True)

    for text in texts_sorted:
        sk = t2k[text]
        escaped = re.escape(text)
        pattern = re.compile(f'"{escaped}"')

        pos = 0
        while True:
            m = pattern.search(content, pos)
            if not m:
                break

            context = content[max(0, m.start() - 200):m.start()]
            account = detect_account(context, m.start(), content)

            replacement = f'StringCatalog.ResolveByKey({account}, "{sk}")'
            content = content[:m.start()] + replacement + content[m.end():]
            stats["replaced"] += 1
            pos = m.start() + len(replacement)

    if content != original:
        content = add_using(content)
        backup_file(path)
        with open(path, "w", encoding="utf-8") as f:
            f.write(content)
        stats["changed"] += 1
        return True
    return False


def scan(t2k):
    changed = []
    for root, dirs, files in os.walk(SCRIPTS_DIR):
        for fn in sorted(files):
            if not fn.endswith(".cs"):
                continue
            path = os.path.join(root, fn)
            try:
                if patch_file(path, t2k):
                    changed.append(os.path.relpath(path, REPO))
            except Exception as e:
                import traceback
                print(f"  ERROR {os.path.relpath(path, REPO)}: {e}")
                traceback.print_exc()

    print(f"\nFiles scanned:   {stats['scanned']}")
    print(f"Files changed:   {stats['changed']}")
    print(f"Replacements:    {stats['replaced']}")
    print(f"using added:     {using_added}")

    if changed:
        print(f"\nChanged files ({len(changed)}):")
        for f in changed:
            print(f"  {f}")


if __name__ == "__main__":
    t2k = build_text_to_key()
    print(f"Text→key map: {len(t2k)}")
    sm_en = load_json(os.path.join(REPO, "Data", "Localization", "en", "scripts-mobiles.json"))
    covered = sum(1 for v in sm_en.values() if v in t2k)
    print(f"scripts-mobiles coverage: {covered}/{len(sm_en)}")

    if len(t2k) < 100:
        print("ERROR: too few lookups")
        sys.exit(1)

    scan(t2k)

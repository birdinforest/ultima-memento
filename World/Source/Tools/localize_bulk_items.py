#!/usr/bin/env python3
"""
Transform C# files to use StringCatalog.Resolve for hardcoded English strings.

Covers batches B11-B15 for Items/. Skips files already using StringCatalog.

Rules:
- Add 'using Server.Localization;' if not present
- Wrap SendMessage("literal") → SendMessage(StringCatalog.Resolve(mobile.Account, "literal"))
- Wrap Name = "literal" → Name = StringCatalog.Resolve(null, "literal") (constructors only)
- Wrap Say("literal") → Say(StringCatalog.Resolve(mobile.Account, "literal"))
- Wrap return "literal" (in simple getters/properties) → return StringCatalog.Resolve(null, "literal")
- Wrap AddLabel(..., "literal") → AddLabel(..., StringCatalog.Resolve(from.Account, "literal"))
- Wrap PublicOverheadMessage/PrivateOverheadMessage with simple string literals

Skips any line containing ' + ' (variable concatenation) to avoid breaking runtime logic.
"""

import re
import os
import glob as glob_mod

REPO_ROOT = "/Users/forrrest/projects/UO-Memento/ultima-memento"
SCRIPTS_BASE = os.path.join(REPO_ROOT, "World", "Source", "Scripts")

# Files that already use StringCatalog - skip entirely
SKIP_FILES = {
    "Items/Misc/OilCloth.cs",
    "Items/Misc/Translocation/MessageHelper.cs",
    "Items/Houses/Monopoly/Gumps/Gumps Plus Light/InfoGump.cs",
    "Items/Containers/WeightReductionContainer.cs",
    "Items/Technology/Canteen.cs",
    "Items/Technology/RomulanAle.cs",
}

# Regex: find literal string in SendMessage/Say, supporting dotted access chains like pmi.Mobile
# The object accessor captures: alphanumeric words separated by dots, ending before .SendMessage/.Say
SENDMSG = re.compile(
    r'((?:\w+\.)*\w+)\.(SendMessage|Say)\s*\(\s*"((?:[^\\"]|\\.)*)"\s*\)'
)

# Regex: context for hue overloads: mobile.SendMessage(hue, "text")
SENDMSG_HUE = re.compile(
    r'((?:\w+\.)*\w+)\.(SendMessage|Say)\s*\(\s*(\w+)\s*,\s*"((?:[^\\"]|\\.)*)"\s*\)'
)

# Regex: Name = "literal"
NAME_ASSIGN = re.compile(
    r'^\s*Name\s*=\s*"((?:[^\\"]|\\.)*)"\s*;',
    re.MULTILINE
)

# Regex: return "literal"
RETURN_LIT = re.compile(
    r'^\s*return\s+"((?:[^\\"]|\\.)*)"\s*;',
    re.MULTILINE
)

# Regex: get { return "literal"; }
PROPERTY_RETURN = re.compile(
    r'get\s*\{\s*return\s+"((?:[^\\"]|\\.)*)"\s*;\s*\}'
)

# Regex: AddLabel(x, y, hue, "text")
GUMP_LABEL = re.compile(
    r'(AddLabel\s*\([^,]+,\s*[^,]+,\s*[^,]+,\s*)"((?:[^\\"]|\\.)*)"(\s*\))'
)

# Regex: PublicOverheadMessage / PrivateOverheadMessage with string literal
OVERHEAD = re.compile(
    r'(\w+)\.(PublicOverheadMessage|PrivateOverheadMessage)\s*\(\s*(MessageType\.\w+)\s*,\s*(\d+)\s*,\s*false\s*,\s*"((?:[^\\"]|\\.)*)"\s*,\s*\w+\.NetState\s*\)'
)

# Regex for simple "text" in AddHtml (too varied - we'll handle via manual patterns)
# Actually skip AddHtml automation since strings are mixed with HTML tags

def has_var_concat(text):
    """Check if text has variable concatenation (skip those lines)."""
    # Check if the text contains ' + ' with non-literal parts
    # Simple heuristic: if it has + and not all parts are string literals
    if ' + ' not in text:
        return False
    return True

def should_modify_file(filepath):
    """Check if file should be modified."""
    rel = os.path.relpath(filepath, SCRIPTS_BASE)
    if rel in SKIP_FILES:
        return False
    if not os.path.exists(filepath):
        return False
    with open(filepath, "r", encoding="utf-8", errors="replace") as f:
        content = f.read()
    if "using Server.Localization;" in content or "StringCatalog" in content:
        return False
    return True

def add_localization_using(content):
    """Add 'using Server.Localization;' after the last 'using' directive."""
    lines = content.split('\n')
    last_using = -1
    for i, line in enumerate(lines):
        s = line.strip()
        if s.startswith("using ") and s.endswith(";"):
            last_using = i
    if last_using >= 0:
        indent = re.match(r"^(\s*)", lines[last_using]).group(1)
        lines.insert(last_using + 1, f"{indent}using Server.Localization;\n")
    return '\n'.join(lines)

def transform_file(filepath):
    """Apply transformations to a single file. Returns (changed, content)."""
    with open(filepath, "r", encoding="utf-8", errors="replace") as f:
        content = f.read()
    original = content

    # 1. Add using directive
    content = add_localization_using(content)

    # 2. Transform SendMessage/Say with simple literals
    def _sendmsg(m):
        obj, method, text = m.group(1), m.group(2), m.group(3)
        # Check the full line for variable concat
        line_start = max(0, content.rfind('\n', 0, m.start()) + 1)
        line = content[line_start:content.find('\n', m.start())]
        if has_var_concat(line):
            return m.group(0)
        return f'{obj}.{method}(StringCatalog.Resolve({obj}.Account, "{text}"))'
    content = SENDMSG.sub(_sendmsg, content)

    # 3. Transform SendMessage/Say with hue overloads
    def _sendmsg_hue(m):
        obj, method, arg1, text = m.group(1), m.group(2), m.group(3), m.group(4)
        line_start = max(0, content.rfind('\n', 0, m.start()) + 1)
        line = content[line_start:content.find('\n', m.start())]
        if has_var_concat(line):
            return m.group(0)
        return f'{obj}.{method}({arg1}, StringCatalog.Resolve({obj}.Account, "{text}"))'
    content = SENDMSG_HUE.sub(_sendmsg_hue, content)

    # 4. Transform Name = "literal"
    def _name(m):
        text = m.group(1)
        line_start = content.rfind('\n', 0, m.start()) + 1
        line_end = content.find('\n', m.start())
        if line_end == -1:
            line_end = len(content)
        line = content[line_start:line_end].strip()
        if has_var_concat(line):
            return m.group(0)
        return f'Name = StringCatalog.Resolve(null, "{text}");'
    content = NAME_ASSIGN.sub(_name, content)

    # 5. Transform return "literal"
    def _return(m):
        text = m.group(1)
        line_start = content.rfind('\n', 0, m.start()) + 1
        line_end = content.find('\n', m.start())
        if line_end == -1:
            line_end = len(content)
        line = content[line_start:line_end].strip()
        if has_var_concat(line):
            return m.group(0)
        return f'return StringCatalog.Resolve(null, "{text}");'
    content = RETURN_LIT.sub(_return, content)

    # 6. Transform get { return "literal"; }
    def _prop_return(m):
        text = m.group(1)
        # Check for vars in the property text
        if has_var_concat(m.group(0)):
            return m.group(0)
        return f'get {{ return StringCatalog.Resolve(null, "{text}"); }}'
    content = PROPERTY_RETURN.sub(_prop_return, content)

    # 7. Transform AddLabel Gump strings
    def _gump_label(m):
        prefix, text, suffix = m.group(1), m.group(2), m.group(3)
        return f'{prefix}StringCatalog.Resolve(from.Account, "{text}"){suffix}'
    content = GUMP_LABEL.sub(_gump_label, content)

    # 8. Transform Overhead messages
    def _overhead(m):
        obj, method, msgtype, hue, text = m.group(1), m.group(2), m.group(3), m.group(4), m.group(5)
        line_start = max(0, content.rfind('\n', 0, m.start()) + 1)
        line = content[line_start:content.find('\n', m.start())]
        if has_var_concat(line):
            return m.group(0)
        return f'{obj}.{method}({msgtype}, {hue}, false, StringCatalog.Resolve({obj}.Account, "{text}"), {obj}.NetState)'
    content = OVERHEAD.sub(_overhead, content)

    if content == original:
        return False, content

    with open(filepath, "w", encoding="utf-8") as f:
        f.write(content)
    return True, content


def main():
    # Define target files
    target_rel = [
        # B11: Misc/
        "Items/Misc/MagicForges.cs",
        "Items/Misc/MusicBox.cs",
        "Items/Misc/Dyes/DyeTub.cs",
        "Items/Misc/Dyes/CustomHuePicker.cs",
        "Items/Misc/Dyes/UnusualDyes.cs",
        "Items/Misc/Dyes/MagicPigment.cs",
        "Items/Misc/Dyes/MagicalDyes.cs",
        "Items/Misc/Bodies/LivingDead/BookofDead.cs",
        "Items/Misc/Bodies/Corpses/Corpse.cs",
        # B12: Houses/
        "Items/Houses/HouseSign.cs",
        "Items/Houses/Monopoly/Gumps/TownHouse Gumps/TownHouseSetupGump.cs",
        # B13: Containers/
        "Items/Containers/Container.cs",
        "Items/Containers/ContainerFunctions.cs",
        # B14: Technology/
        "Items/Technology/SciFiJunk.cs",
        "Items/Technology/PlasmaTorch.cs",
        "Items/Technology/AlienEgg.cs",
        "Items/Technology/MaterialLiquifier.cs",
        # B15: Games/
        "Items/Games/BlackJack/BlackJack.cs",
        "Items/Games/LiarsDice/LiarsDice.cs",
        "Items/Games/TarotPoker/TarotPoker.cs",
    ]

    results = []
    for rel in target_rel:
        fp = os.path.join(SCRIPTS_BASE, rel)
        if not os.path.exists(fp):
            # Try glob for case-variant
            candidates = glob_mod.glob(os.path.join(os.path.dirname(fp), "*"))
            match = [c for c in candidates if os.path.basename(c).lower().replace(" ", "").replace("_", "") == os.path.basename(rel).lower().replace(" ", "").replace("_", "")]
            if match:
                fp = match[0]
            else:
                print(f"  NOT FOUND: {rel}")
                continue

        if not should_modify_file(fp):
            print(f"  SKIP: {rel}")
            continue

        changed, _ = transform_file(fp)
        if changed:
            print(f"  MODIFIED: {rel}")
            results.append(rel)
        else:
            print(f"  UNCHANGED: {rel}")

    print(f"\n{'='*60}")
    print(f"Modified {len(results)} files:")
    for r in sorted(results):
        print(f"  - {r}")


if __name__ == "__main__":
    main()

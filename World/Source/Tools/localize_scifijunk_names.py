"""
Fix Name assignments in SciFiJunk.cs switch statements.
Usage: python3 World/Source/Tools/localize_scifijunk_names.py
"""

import re
import os

REPO_ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
PATH = os.path.join(REPO_ROOT, "Source", "Scripts", "Items", "Technology", "SciFiJunk.cs")

with open(PATH, "r", encoding="utf-8", errors="replace") as f:
    content = f.read()

original = content

# Pattern: Name = "literal"; (NOT already wrapped, NOT on a line with + concat)
NAME_LIT = re.compile(r'Name\s*=\s*"((?:[^\\"]|\\.)*)"\s*;')

def _replace(m):
    text = m.group(1)
    line_start = content.rfind('\n', 0, m.start()) + 1
    line_end = content.find('\n', m.start())
    if line_end == -1:
        line_end = len(content)
    line = content[line_start:line_end]
    # Skip if already has StringCatalog
    if "StringCatalog" in line:
        return m.group(0)
    # Skip if variable concat
    if " + " in line:
        return m.group(0)
    return f'Name = StringCatalog.Resolve(null, "{text}");'

content = NAME_LIT.sub(_replace, content)

if content != original:
    with open(PATH, "w", encoding="utf-8") as f:
        f.write(content)
    print(f"MODIFIED: {PATH}")
else:
    print(f"UNCHANGED: {PATH}")

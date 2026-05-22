"""
Fix localized Name assignments in ContainerFunctions.cs that were missed by the
bulk script (patterns like 'box.Name = "text"' instead of 'Name = "text"').

Usage: python3 World/Source/Tools/localize_container_functions.py
"""

import re
import os
import sys

REPO_ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
PATH = os.path.join(REPO_ROOT, "Source", "Scripts", "Items", "Containers", "ContainerFunctions.cs")

with open(PATH, "r", encoding="utf-8", errors="replace") as f:
    content = f.read()

original = content

# Only transform simple box.Name = "simple literal"; that doesn't involve concatenation with '+',
# and isn't already wrapped with StringCatalog.
# Pattern: \w+\.Name\s*=\s*"literal";
NAME_DOT = re.compile(r'(\w+)\.Name\s*=\s*"(?P<text>(?:[^\\"]|\\.)*)"\s*;')

def _replace(m):
    obj = m.group(1)
    text = m.group("text")
    full_line_start = content.rfind('\n', 0, m.start()) + 1
    full_line_end = content.find('\n', m.start())
    if full_line_end == -1:
        full_line_end = len(content)
    line = content[full_line_start:full_line_end]
    # Skip if already wrapped
    if "StringCatalog" in line:
        return m.group(0)
    # Skip if variable concatenation
    if " + " in line:
        return m.group(0)
    return f'{obj}.Name = StringCatalog.Resolve(null, "{text}");'

content = NAME_DOT.sub(_replace, content)

if content != original:
    with open(PATH, "w", encoding="utf-8") as f:
        f.write(content)
    print(f"MODIFIED: {PATH}")
else:
    print(f"UNCHANGED: {PATH}")

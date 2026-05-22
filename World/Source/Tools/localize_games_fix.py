#!/usr/bin/env python3
"""Fix BlackJack.cs and TarotPoker.cs which weren't found by the glob."""
import re
import os

REPO_ROOT = "/Users/forrrest/projects/UO-Memento/ultima-memento"
SCRIPTS_BASE = os.path.join(REPO_ROOT, "World", "Source", "Scripts")

files = [
    "Items/Games/BlackJack.cs",
    "Items/Games/TarotPoker.cs",
]

SENDMSG = re.compile(
    r'(\w+)\.(SendMessage|Say)\s*\(\s*"((?:[^\\"]|\\.)*)"\s*\)'
)
NAME_ASSIGN = re.compile(
    r'^\s*Name\s*=\s*"((?:[^\\"]|\\.)*)"\s*;',
    re.MULTILINE
)
RETURN_LIT = re.compile(
    r'^\s*return\s+"((?:[^\\"]|\\.)*)"\s*;',
    re.MULTILINE
)
GUMP_LABEL = re.compile(
    r'(AddLabel\s*\([^,]+,\s*[^,]+,\s*[^,]+,\s*)"((?:[^\\"]|\\.)*)"(\s*\))'
)
OVERHEAD = re.compile(
    r'(\w+)\.(PublicOverheadMessage|PrivateOverheadMessage)\s*\(\s*(MessageType\.\w+|0)\s*,\s*(\d+)\s*,\s*false\s*,\s*"((?:[^\\"]|\\.)*)"\s*,\s*\w+\.NetState\s*\)'
)
SENDMSG_HUE = re.compile(
    r'(\w+)\.(SendMessage|Say)\s*\(\s*(\w+)\s*,\s*"((?:[^\\"]|\\.)*)"\s*\)'
)

def has_var_concat(text):
    if ' + ' not in text:
        return False
    return True

def add_localization_using(content):
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

def process(filepath):
    with open(filepath, "r", encoding="utf-8", errors="replace") as f:
        content = f.read()
    original = content
    
    if "using Server.Localization;" in content or "StringCatalog" in content:
        print(f"  SKIP (already localized): {filepath}")
        return
    
    content = add_localization_using(content)
    
    def _sendmsg(m):
        obj, method, text = m.group(1), m.group(2), m.group(3)
        line_start = max(0, content.rfind('\n', 0, m.start()) + 1)
        line = content[line_start:content.find('\n', m.start())]
        if has_var_concat(line):
            return m.group(0)
        return f'{obj}.{method}(StringCatalog.Resolve({obj}.Account, "{text}"))'
    content = SENDMSG.sub(_sendmsg, content)
    
    def _sendmsg_hue(m):
        obj, method, arg1, text = m.group(1), m.group(2), m.group(3), m.group(4)
        line_start = max(0, content.rfind('\n', 0, m.start()) + 1)
        line = content[line_start:content.find('\n', m.start())]
        if has_var_concat(line):
            return m.group(0)
        return f'{obj}.{method}({arg1}, StringCatalog.Resolve({obj}.Account, "{text}"))'
    content = SENDMSG_HUE.sub(_sendmsg_hue, content)
    
    def _name(m):
        text = m.group(1)
        line_start = content.rfind('\n', 0, m.start()) + 1
        line_end = content.find('\n', m.start())
        line = content[line_start:line_end].strip() if line_end > line_start else m.group(0)
        if has_var_concat(line):
            return m.group(0)
        return f'Name = StringCatalog.Resolve(null, "{text}");'
    content = NAME_ASSIGN.sub(_name, content)
    
    def _return(m):
        text = m.group(1)
        line_start = content.rfind('\n', 0, m.start()) + 1
        line_end = content.find('\n', m.start())
        line = content[line_start:line_end].strip() if line_end > line_start else m.group(0)
        if has_var_concat(line):
            return m.group(0)
        return f'return StringCatalog.Resolve(null, "{text}");'
    content = RETURN_LIT.sub(_return, content)
    
    def _gump_label(m):
        prefix, text, suffix = m.group(1), m.group(2), m.group(3)
        return f'{prefix}StringCatalog.Resolve(from.Account, "{text}"){suffix}'
    content = GUMP_LABEL.sub(_gump_label, content)
    
    def _overhead(m):
        # Handle both MessageType.Enum and raw 0 format
        if m.group(3).startswith("MessageType"):
            obj, method, msgtype, hue, text = m.group(1), m.group(2), m.group(3), m.group(4), m.group(5)
        else:
            return m.group(0)  # skip if not MessageType pattern
        line_start = max(0, content.rfind('\n', 0, m.start()) + 1)
        line = content[line_start:content.find('\n', m.start())]
        if has_var_concat(line):
            return m.group(0)
        return f'{obj}.{method}({msgtype}, {hue}, false, StringCatalog.Resolve({obj}.Account, "{text}"), {obj}.NetState)'
    content = OVERHEAD.sub(_overhead, content)
    
    if content == original:
        print(f"  UNCHANGED: {filepath}")
        return
    
    with open(filepath, "w", encoding="utf-8") as f:
        f.write(content)
    print(f"  MODIFIED: {filepath}")

for rel in files:
    fp = os.path.join(SCRIPTS_BASE, rel)
    if os.path.exists(fp):
        process(fp)
    else:
        print(f"  NOT FOUND: {fp}")

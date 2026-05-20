#!/usr/bin/env python3
"""
Wrap simple one-line DefaultDescription English literals with StringCatalog.Resolve(null, "...")
so build_localization_strings.py extracts hash keys and InfoDataGump can TryResolve zh-Hans.

Skips lines containing '+' (use ResolveFormat manually) and lines not matching the one-liner pattern.
Idempotent: leaves files that already use StringCatalog.Resolve in the getter.
"""
from __future__ import annotations

import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]  # .../World
SCRIPTS = ROOT / "Source" / "Scripts"

# One line: public override string DefaultDescription{ get{ return "...."; } }
ONELINER = re.compile(
    r'(public\s+override\s+string\s+DefaultDescription\s*\{\s*get\s*\{\s*return\s+)'
    r'("(?:[^"\\]|\\.)*")\s*;\s*\}\s*\}',
    re.MULTILINE,
)


def ensure_using_localization(text: str) -> str:
    if "using Server.Localization;" in text:
        return text
    # Insert after last using at top
    lines = text.splitlines(keepends=True)
    i = 0
    while i < len(lines) and lines[i].startswith("using "):
        i += 1
    lines.insert(i, "using Server.Localization;\n")
    return "".join(lines)


def process_file(path: Path) -> bool:
    raw = path.read_text(encoding="utf-8", errors="replace")
    if "public override string DefaultDescription" not in raw:
        return False
    m = ONELINER.search(raw)
    if not m:
        return False
    span_line = raw.count("\n", 0, m.start()) + 1
    line_text = raw[raw.rfind("\n", 0, m.start()) + 1 : raw.find("\n", m.end())]
    if "+" in line_text:
        return False

    def repl(mo: re.Match) -> str:
        prefix = mo.group(1)
        lit = mo.group(2)
        return f"{prefix}StringCatalog.Resolve( null, {lit} ); }} }}"

    new_raw = ONELINER.sub(repl, raw)
    if new_raw == raw:
        return False
    new_raw = ensure_using_localization(new_raw)
    path.write_text(new_raw, encoding="utf-8")
    return True


def main() -> int:
    changed = 0
    for path in sorted(SCRIPTS.rglob("*.cs")):
        try:
            if process_file(path):
                print("wrapped:", path.relative_to(ROOT.parent))
                changed += 1
        except Exception as ex:
            print("error", path, ex, file=sys.stderr)
            return 1
    print("files changed:", changed)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

#!/usr/bin/env python3
"""
Replace StringCatalog.Resolve( acc, "literal" ) -> ResolveByKey( acc, "shotkey" )
and ResolveFormat( acc, "...", -> ResolveFormatByKey( acc, "shotkey",
using world-player-shotkey-map.json (en -> shotkey).
"""
from __future__ import annotations

import json
import os
import re
import sys

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
MAP_PATH = os.path.join(
    ROOT, "Data", "Localization", "tools-output", "world-player-shotkey-map.json"
)
SOURCE_DIRS = [
    os.path.join(ROOT, "Source", "Scripts"),
    os.path.join(ROOT, "Source", "System"),
]


def iter_cs_files():
    for base in SOURCE_DIRS:
        if not os.path.isdir(base):
            continue
        for dirpath, _, filenames in os.walk(base):
            for fn in filenames:
                if fn.endswith(".cs"):
                    yield os.path.join(dirpath, fn)


def patch_file(path: str, pairs: list[tuple[str, str]], stats: dict) -> bool:
    with open(path, encoding="utf-8", errors="replace") as f:
        text = f.read()
    orig = text
    # Longest English first to avoid partial matches
    for en, shotkey in pairs:
        if en not in text:
            continue

        pat_resolve = (
            r"StringCatalog\.Resolve\s*\(\s*([^,]+?)\s*,\s*"
            + re.escape('"' + en + '"')
            + r"\s*\)"
        )
        repl_r = f"StringCatalog.ResolveByKey(\\1, \"{shotkey}\")"
        n1 = len(re.findall(pat_resolve, text))
        if n1:
            text = re.sub(pat_resolve, repl_r, text)
            stats["resolve"] = stats.get("resolve", 0) + n1

        pat_fmt = (
            r"StringCatalog\.ResolveFormat\s*\(\s*([^,]+?)\s*,\s*"
            + re.escape('"' + en + '"')
            + r"\s*,"
        )
        repl_f = f"StringCatalog.ResolveFormatByKey(\\1, \"{shotkey}\","
        n2 = len(re.findall(pat_fmt, text))
        if n2:
            text = re.sub(pat_fmt, repl_f, text)
            stats["format"] = stats.get("format", 0) + n2

    if text != orig:
        with open(path, "w", encoding="utf-8", newline="\n") as f:
            f.write(text)
        return True
    return False


def main() -> int:
    if not os.path.isfile(MAP_PATH):
        print(f"missing {MAP_PATH}", file=sys.stderr)
        return 1
    with open(MAP_PATH, encoding="utf-8") as f:
        mapping = json.load(f)
    pairs = [(m["en"], m["shotkey"]) for m in mapping]
    pairs.sort(key=lambda x: len(x[0]), reverse=True)

    totals = {"files": 0, "resolve": 0, "format": 0}
    touched = []
    for path in iter_cs_files():
        st: dict = {}
        if patch_file(path, pairs, st):
            touched.append(path)
            totals["files"] += 1
            totals["resolve"] += st.get("resolve", 0)
            totals["format"] += st.get("format", 0)

    print(f"touched {totals['files']} files, Resolve->ByKey: {totals['resolve']}, Format->FormatByKey: {totals['format']}")
    missing_en: list[str] = []
    for en, sk in pairs:
        hit = False
        for p in iter_cs_files():
            with open(p, encoding="utf-8", errors="replace") as f:
                body = f.read()
            if en in body and (
                f'StringCatalog.Resolve(' in body or "StringCatalog.ResolveFormat(" in body
            ):
                # could be in comment; cheap check
                if (
                    f'StringCatalog.Resolve(' in body
                    and '"' + en + '"' in body
                    and "ResolveByKey" not in body.split('"' + en + '"')[0][-200:]
                ):
                    hit = True
                    break
        if not hit:
            # Heuristic: already migrated or SendMessage-only
            missing_en.append(sk)
    if missing_en:
        print(f"note: {len(missing_en)} keys may still need manual wiring (no literal match):", file=sys.stderr)
        for x in missing_en[:25]:
            print(f"  {x}", file=sys.stderr)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

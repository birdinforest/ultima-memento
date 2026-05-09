#!/usr/bin/env python3
"""Merge English scripts-books.json with hand-authored zh-Hans fragments into zh-Hans/scripts-books.json."""
from __future__ import annotations

import json
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
EN_PATH = ROOT / "Data" / "Localization" / "en" / "scripts-books.json"
OUT_PATH = ROOT / "Data" / "Localization" / "zh-Hans" / "scripts-books.json"
FRAG_DIR = Path(__file__).resolve().parent / "scripts_books_zh_fragments"


def main() -> int:
    en = json.loads(EN_PATH.read_text(encoding="utf-8"))
    zh: dict[str, str] = {}
    if not FRAG_DIR.is_dir():
        print("Missing fragment dir:", FRAG_DIR, file=sys.stderr)
        return 1
    for path in sorted(FRAG_DIR.glob("frag_*.json")):
        part = json.loads(path.read_text(encoding="utf-8"))
        overlap = set(zh) & set(part)
        if overlap:
            print("Duplicate keys in", path.name, ":", sorted(overlap)[:10], file=sys.stderr)
            return 1
        zh.update(part)
    missing = [k for k in en if k not in zh]
    extra = [k for k in zh if k not in en]
    if extra:
        print("Unknown keys in fragments:", extra[:20], file=sys.stderr)
        return 1
    if missing:
        print("Missing translations:", len(missing), file=sys.stderr)
        for k in missing[:30]:
            print(" ", k, file=sys.stderr)
        return 1
    out_obj = {k: zh[k] for k in en}
    OUT_PATH.write_text(
        json.dumps(out_obj, ensure_ascii=False, indent=2) + "\n",
        encoding="utf-8",
    )
    print("Wrote", OUT_PATH, "keys=", len(out_obj))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

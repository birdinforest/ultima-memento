#!/usr/bin/env python3
"""
From llm-queue-179.jsonl (or any queue with file, key, en), emit:
  - Data/Localization/en/world-player-text.json (shotkeys, no hash)
  - Data/Localization/tools-output/world-player-shotkey-map.json (en -> shotkey, for C# migration)

Skips equipment-properties.json (logical keys live in equipment-properties.json already).
"""
from __future__ import annotations

import json
import os
import re
import sys

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
QUEUE_DEFAULT = os.path.join(
    ROOT, "Data", "Localization", "tools-output", "llm-queue-179.jsonl"
)
OUT_EN = os.path.join(ROOT, "Data", "Localization", "en", "world-player-text.json")
OUT_MAP = os.path.join(
    ROOT, "Data", "Localization", "tools-output", "world-player-shotkey-map.json"
)

CAT_PREFIX = {
    "scripts-quests.json": "quest",
    "scripts-engines-and-systems.json": "eng",
    "scripts-mobiles.json": "mob",
    "scripts-system.json": "sys",
    "scripts-books.json": "book",
}


def slug_key(en: str, cat: str) -> str:
    pfx = CAT_PREFIX[cat]
    s = en.lower()
    for i in range(10):
        s = s.replace("{%d}" % i, "n%d" % i)
    s = s.replace("!", "_ex").replace(".", "_dot").replace("?", "_q").replace(",", "_c")
    s = re.sub(r"[^a-z0-9]+", "_", s)
    s = re.sub(r"_+", "_", s).strip("_")[:100]
    if not s:
        s = "line"
    return f"{pfx}.{s}"


def main() -> int:
    queue_path = sys.argv[1] if len(sys.argv) > 1 else QUEUE_DEFAULT
    if not os.path.isfile(queue_path):
        print(f"missing queue: {queue_path}", file=sys.stderr)
        return 1

    entries: list[dict] = []
    with open(queue_path, encoding="utf-8") as f:
        for line in f:
            line = line.strip()
            if not line:
                continue
            o = json.loads(line)
            if o["file"] == "equipment-properties.json":
                continue
            if o["file"] not in CAT_PREFIX:
                print(f"skip unknown file {o['file']}", file=sys.stderr)
                continue
            entries.append(o)

    en_out: dict[str, str] = {}
    mapping: list[dict[str, str]] = []

    for o in entries:
        cat = o["file"]
        en = o["en"]
        sk = slug_key(en, cat)
        if sk in en_out and en_out[sk] != en:
            raise SystemExit(f"shotkey collision after skip: {sk!r}")
        en_out[sk] = en
        mapping.append({"shotkey": sk, "en": en, "source": cat, "old_hash": o.get("key", "")})

    os.makedirs(os.path.dirname(OUT_EN), exist_ok=True)
    with open(OUT_EN, "w", encoding="utf-8") as f:
        json.dump(dict(sorted(en_out.items())), f, ensure_ascii=False, indent=2)
        f.write("\n")

    os.makedirs(os.path.dirname(OUT_MAP), exist_ok=True)
    with open(OUT_MAP, "w", encoding="utf-8") as f:
        json.dump(mapping, f, ensure_ascii=False, indent=2)
        f.write("\n")

    print(f"wrote {OUT_EN} ({len(en_out)} keys)")
    print(f"wrote {OUT_MAP}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

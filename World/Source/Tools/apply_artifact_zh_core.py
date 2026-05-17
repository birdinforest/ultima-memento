#!/usr/bin/env python3
"""Merge artifact_zh_core.json into zh-Hans/equipment-properties.json (dup-fallback rows only)."""
from __future__ import annotations

import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
CORE_PATH = Path(__file__).resolve().parent / "artifact_zh_core.json"
EQ_ZH = ROOT / "Data/Localization/zh-Hans/equipment-properties.json"
EQ_EN = ROOT / "Data/Localization/en/equipment-properties.json"

DUP_RE = re.compile(r"^(.+?)（\1）$")


def main() -> None:
    core = json.loads(CORE_PATH.read_text(encoding="utf-8"))
    zh = json.loads(EQ_ZH.read_text(encoding="utf-8"))
    ed = json.loads(EQ_EN.read_text(encoding="utf-8"))
    n = 0
    for k, v in zh.items():
        if not k.startswith("item.magical.artifact."):
            continue
        if not DUP_RE.match(v.strip()):
            continue
        en = ed.get(k, "").strip()
        if not en or en not in core:
            raise SystemExit(f"missing core for {k} en={en!r}")
        newv = f"{core[en]}（{en}）"
        if zh[k] != newv:
            zh[k] = newv
            n += 1
    EQ_ZH.write_text(
        json.dumps(zh, ensure_ascii=False, indent=2) + "\n",
        encoding="utf-8",
    )
    print("updated rows:", n, "file:", EQ_ZH)


if __name__ == "__main__":
    main()

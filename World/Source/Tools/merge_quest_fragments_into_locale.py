#!/usr/bin/env python3
"""Merge quest-fragment-zh-table.json into en/scripts-quests.json and zh-Hans/scripts-quests.json (hash keys)."""
from __future__ import annotations

import hashlib
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
TABLE = ROOT / "Data" / "Localization" / "quest-fragment-zh-table.json"
EN = ROOT / "Data" / "Localization" / "en" / "scripts-quests.json"
ZH = ROOT / "Data" / "Localization" / "zh-Hans" / "scripts-quests.json"


def key_for_english(text: str) -> str:
    h = hashlib.sha256(text.encode("utf-8")).digest()
    return "s." + "".join(f"{b:02x}" for b in h[:8])


EXTRA_QUEST_UI = {
    "Ahh...they had {0}!": "啊……他们带着{0}！",
    "Ahh...I found {0}!": "啊……我找到了{0}！",
    "Here is {0} gold for you.": "这是给你的{0}金币。",
}


def main() -> int:
    table = json.loads(TABLE.read_text(encoding="utf-8"))
    table.update(EXTRA_QUEST_UI)
    en_data = json.loads(EN.read_text(encoding="utf-8"))
    zh_data = json.loads(ZH.read_text(encoding="utf-8"))
    added = 0
    for en_lit, zh_lit in table.items():
        k = key_for_english(en_lit)
        if k not in en_data:
            en_data[k] = en_lit
            added += 1
        if zh_data.get(k) != zh_lit:
            zh_data[k] = zh_lit
    EN.write_text(json.dumps(en_data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    ZH.write_text(json.dumps(zh_data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(f"merged {len(table)} fragments; new en keys: {added}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

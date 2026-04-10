#!/usr/bin/env python3
"""Fill strings.zh-Hans.json from strings.en.json using Google Translate (machine)."""
from __future__ import annotations

import json
import os
import sys
import time

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
EN = os.path.join(ROOT, "Data", "Localization", "strings.en.json")
ZH = os.path.join(ROOT, "Data", "Localization", "strings.zh-Hans.json")


def main() -> int:
    from deep_translator import GoogleTranslator

    with open(EN, encoding="utf-8") as f:
        en: dict[str, str] = json.load(f)

    items = list(en.items())
    t = GoogleTranslator(source="en", target="zh-CN")
    zh: dict[str, str] = {}
    batch = 80

    for i in range(0, len(items), batch):
        keys = [k for k, _ in items[i : i + batch]]
        vals = [v for _, v in items[i : i + batch]]
        try:
            translated = t.translate_batch(vals)
        except Exception:
            translated = None
        if translated is None or len(translated) != len(vals):
            translated = []
            for v in vals:
                try:
                    translated.append(t.translate(v))
                except Exception:
                    translated.append(v)
                time.sleep(0.05)
        for k, z in zip(keys, translated):
            zh[k] = z or en[k]
        print(f"translated {min(i + batch, len(items))}/{len(items)}")
        time.sleep(0.35)

    os.makedirs(os.path.dirname(ZH), exist_ok=True)
    with open(ZH, "w", encoding="utf-8") as f:
        json.dump(zh, f, ensure_ascii=False, indent=2, sort_keys=True)
        f.write("\n")

    print(f"wrote {ZH}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

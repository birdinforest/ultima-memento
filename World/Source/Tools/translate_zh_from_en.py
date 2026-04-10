#!/usr/bin/env python3
"""Fill Data/Localization/zh-Hans/*.json from matching en/*.json (machine translate)."""
from __future__ import annotations

import json
import os
import sys
import time

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
EN_DIR = os.path.join(ROOT, "Data", "Localization", "en")
ZH_DIR = os.path.join(ROOT, "Data", "Localization", "zh-Hans")


def translate_batch(vals: list[str]) -> list[str]:
    from deep_translator import GoogleTranslator

    t = GoogleTranslator(source="en", target="zh-CN")
    batch = 80
    out: list[str] = []
    for i in range(0, len(vals), batch):
        chunk = vals[i : i + batch]
        try:
            zh_list = t.translate_batch(chunk)
        except Exception:
            zh_list = None
        if zh_list is None or len(zh_list) != len(chunk):
            zh_list = []
            for v in chunk:
                try:
                    zh_list.append(t.translate(v))
                except Exception:
                    zh_list.append(v)
                time.sleep(0.05)
        out.extend(zh_list)
        print(f"translated {min(i + batch, len(vals))}/{len(vals)}")
        time.sleep(0.35)
    return out


def main() -> int:
    if not os.path.isdir(EN_DIR):
        print(f"missing {EN_DIR}", file=sys.stderr)
        return 1

    os.makedirs(ZH_DIR, exist_ok=True)

    for fn in sorted(os.listdir(EN_DIR)):
        if not fn.endswith(".json"):
            continue
        en_path = os.path.join(EN_DIR, fn)
        zh_path = os.path.join(ZH_DIR, fn)
        with open(en_path, encoding="utf-8") as f:
            en: dict[str, str] = json.load(f)
        items = list(en.items())
        if not items:
            with open(zh_path, "w", encoding="utf-8") as f:
                json.dump({}, f)
                f.write("\n")
            continue
        keys = [k for k, _ in items]
        vals = [v for _, v in items]
        translated = translate_batch(vals)
        zh = {k: (z or en[k]) for k, z in zip(keys, translated)}
        with open(zh_path, "w", encoding="utf-8") as f:
            json.dump(dict(sorted(zh.items())), f, ensure_ascii=False, indent=2, sort_keys=True)
            f.write("\n")
        print(f"wrote {zh_path}")

    return 0


if __name__ == "__main__":
    raise SystemExit(main())

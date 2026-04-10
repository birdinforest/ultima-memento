#!/usr/bin/env python3
"""
Scan World/Source for SendMessage/SendAsciiMessage/Say string literals and emit
Data/Localization/strings.en.json and strings.zh-Hans.json with SHA256-based keys
matching Server.Localization.StringKey.ForEnglish (UTF-8, first 8 bytes hex).

Requires: pip install deep-translator (for --translate)

Usage:
  python3 build_localization_strings.py [--no-translate]
"""
from __future__ import annotations

import argparse
import hashlib
import json
import os
import re
import sys
import time
from typing import Dict, Iterable, List, Set, Tuple

# This file lives at World/Source/Tools/ — repo root is two levels up (World/).
ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
SOURCE_ROOT = os.path.join(ROOT, "Source")
OUT_EN = os.path.join(ROOT, "Data", "Localization", "strings.en.json")
OUT_ZH = os.path.join(ROOT, "Data", "Localization", "strings.zh-Hans.json")

# C# verbatim and regular string extraction is incomplete; we focus on common patterns.
RE_SEND = re.compile(
    r"\b(?:SendMessage|SendAsciiMessage)\s*\(\s*\"((?:\\.|[^\"\\])*)\"",
    re.MULTILINE,
)
RE_SAY = re.compile(r"\bSay\s*\(\s*\"((?:\\.|[^\"\\])*)\"", re.MULTILINE)


def csharp_unescape(s: str) -> str:
    """Decode minimal C# escapes for string literals."""
    out: List[str] = []
    i = 0
    while i < len(s):
        c = s[i]
        if c != "\\":
            out.append(c)
            i += 1
            continue
        i += 1
        if i >= len(s):
            break
        e = s[i]
        i += 1
        if e == "n":
            out.append("\n")
        elif e == "r":
            out.append("\r")
        elif e == "t":
            out.append("\t")
        elif e == "\\":
            out.append("\\")
        elif e == '"':
            out.append('"')
        elif e == "0":
            out.append("\0")
        elif e == "u" and i + 4 <= len(s):
            try:
                cp = int(s[i : i + 4], 16)
                out.append(chr(cp))
            except ValueError:
                out.append("?")
            i += 4
        else:
            out.append(e)
    return "".join(out)


def key_for_english(text: str) -> str:
    h = hashlib.sha256(text.encode("utf-8")).digest()
    return "s." + "".join(f"{b:02x}" for b in h[:8])


def iter_cs_files(base: str) -> Iterable[str]:
    for dirpath, _, files in os.walk(base):
        for fn in files:
            if fn.endswith(".cs"):
                yield os.path.join(dirpath, fn)


def collect_strings() -> Set[str]:
    found: Set[str] = set()
    for path in iter_cs_files(SOURCE_ROOT):
        try:
            data = open(path, encoding="utf-8", errors="ignore").read()
        except OSError:
            continue
        for rx in (RE_SEND, RE_SAY):
            for m in rx.finditer(data):
                raw = m.group(1)
                text = csharp_unescape(raw)
                if not text.strip():
                    continue
                found.add(text)
    return found


def translate_batch(texts: List[str], target: str = "zh-CN") -> Dict[str, str]:
    from deep_translator import GoogleTranslator

    t = GoogleTranslator(source="en", target=target)
    out: Dict[str, str] = {}
    batch = 50
    for i in range(0, len(texts), batch):
        chunk = texts[i : i + batch]
        try:
            zh_list = t.translate_batch(chunk)
            if zh_list is None or len(zh_list) != len(chunk):
                raise ValueError("batch length mismatch")
            for s, z in zip(chunk, zh_list):
                out[s] = z or s
        except Exception as ex:  # noqa: BLE001
            print(f"batch translate fail ({ex!s}), falling back per string", file=sys.stderr)
            for s in chunk:
                try:
                    out[s] = t.translate(s) or s
                except Exception as ex2:  # noqa: BLE001
                    out[s] = s
                    print(f"translate fail: {ex2!s}", file=sys.stderr)
        time.sleep(0.4)
    return out


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument(
        "--no-translate",
        action="store_true",
        help="Only write English JSON (Chinese copies English).",
    )
    args = ap.parse_args()

    texts = sorted(collect_strings())
    print(f"unique literals: {len(texts)}")

    en_map: Dict[str, str] = {}
    for s in texts:
        en_map[key_for_english(s)] = s

    os.makedirs(os.path.dirname(OUT_EN), exist_ok=True)
    with open(OUT_EN, "w", encoding="utf-8") as f:
        json.dump(en_map, f, ensure_ascii=False, indent=2, sort_keys=True)
        f.write("\n")

    zh_map: Dict[str, str] = {}
    if args.no_translate:
        prev_zh: Dict[str, str] = {}
        if os.path.isfile(OUT_ZH):
            try:
                with open(OUT_ZH, encoding="utf-8") as f:
                    prev_zh = json.load(f)
            except Exception:
                prev_zh = {}
        for k, en_val in en_map.items():
            if k in prev_zh and prev_zh[k] and prev_zh[k] != en_val:
                zh_map[k] = prev_zh[k]
            else:
                zh_map[k] = en_val
    else:
        tr = translate_batch(texts)
        for s in texts:
            zh_map[key_for_english(s)] = tr.get(s, s)

    with open(OUT_ZH, "w", encoding="utf-8") as f:
        json.dump(zh_map, f, ensure_ascii=False, indent=2, sort_keys=True)
        f.write("\n")

    print(f"wrote {OUT_EN}")
    print(f"wrote {OUT_ZH}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

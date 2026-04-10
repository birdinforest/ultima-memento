#!/usr/bin/env python3
"""
Scan World/Source for SendMessage/SendAsciiMessage/Say string literals and emit
split JSON files under Data/Localization/en/<category>.json and
Data/Localization/zh-Hans/<category>.json.

Category is derived from the source path (Scripts/* top folder, or System).

Keys match Server.Localization.StringKey.ForEnglish (SHA-256 UTF-8, first 8 bytes hex).

Usage:
  python3 build_localization_strings.py [--no-translate]

  --no-translate: merge existing zh-Hans/*.json for unchanged English values.
"""
from __future__ import annotations

import argparse
import hashlib
import json
import os
import re
import sys
from typing import Dict, Iterable, List, Set

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
SOURCE_ROOT = os.path.join(ROOT, "Source")
OUT_EN_DIR = os.path.join(ROOT, "Data", "Localization", "en")
OUT_ZH_DIR = os.path.join(ROOT, "Data", "Localization", "zh-Hans")

RE_SEND = re.compile(
    r"\b(?:SendMessage|SendAsciiMessage)\s*\(\s*\"((?:\\.|[^\"\\])*)\"",
    re.MULTILINE,
)
RE_SAY = re.compile(r"\bSay\s*\(\s*\"((?:\\.|[^\"\\])*)\"", re.MULTILINE)


def csharp_unescape(s: str) -> str:
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


def category_for_path(abs_path: str) -> str:
    rel = os.path.relpath(abs_path, SOURCE_ROOT).replace("\\", "/")
    parts = rel.split("/")
    if not parts:
        return "misc"
    if parts[0] == "Scripts" and len(parts) > 1:
        seg = parts[1].lower().replace(" ", "-")
        return "scripts-" + seg
    if parts[0] == "System":
        return "system"
    return "misc"


def iter_cs_files(base: str) -> Iterable[str]:
    for dirpath, _, files in os.walk(base):
        for fn in files:
            if fn.endswith(".cs"):
                yield os.path.join(dirpath, fn)


def collect_by_category() -> Dict[str, Dict[str, str]]:
    """
    category -> { hash_key: english_text }
    First occurrence of a string wins its category (stable when scanning sorted paths).
    """
    buckets: Dict[str, Dict[str, str]] = {}
    seen_key: Set[str] = set()

    for path in sorted(iter_cs_files(SOURCE_ROOT)):
        try:
            data = open(path, encoding="utf-8", errors="ignore").read()
        except OSError:
            continue
        cat = category_for_path(path)
        if cat not in buckets:
            buckets[cat] = {}
        for rx in (RE_SEND, RE_SAY):
            for m in rx.finditer(data):
                raw = m.group(1)
                text = csharp_unescape(raw)
                if not text.strip():
                    continue
                k = key_for_english(text)
                if k in seen_key:
                    continue
                seen_key.add(k)
                buckets[cat][k] = text
    return buckets


def load_all_zh_flat(zh_dir: str) -> Dict[str, str]:
    merged: Dict[str, str] = {}
    if not os.path.isdir(zh_dir):
        return merged
    for root, _, files in os.walk(zh_dir):
        for fn in files:
            if not fn.endswith(".json"):
                continue
            p = os.path.join(root, fn)
            try:
                with open(p, encoding="utf-8") as f:
                    chunk = json.load(f)
                for kk, vv in chunk.items():
                    merged[kk] = vv
            except Exception:
                pass
    return merged


def load_legacy_monolith(path: str) -> Dict[str, str]:
    if not os.path.isfile(path):
        return {}
    try:
        with open(path, encoding="utf-8") as f:
            return json.load(f)
    except Exception:
        return {}


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


def write_json(path: str, obj: dict) -> None:
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "w", encoding="utf-8") as f:
        json.dump(obj, f, ensure_ascii=False, indent=2, sort_keys=True)
        f.write("\n")


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--no-translate", action="store_true")
    args = ap.parse_args()

    buckets = collect_by_category()
    total_keys = sum(len(v) for v in buckets.values())
    print(f"categories: {len(buckets)}, unique string keys: {total_keys}")

    prev_zh_flat = load_all_zh_flat(OUT_ZH_DIR)
    if not prev_zh_flat:
        legacy_zh = os.path.join(ROOT, "Data", "Localization", "strings.zh-Hans.json")
        prev_zh_flat = load_legacy_monolith(legacy_zh)

    os.makedirs(OUT_EN_DIR, exist_ok=True)
    os.makedirs(OUT_ZH_DIR, exist_ok=True)

    all_en_flat: Dict[str, str] = {}
    for cat, en_map in sorted(buckets.items()):
        for k, v in en_map.items():
            all_en_flat[k] = v
        write_json(os.path.join(OUT_EN_DIR, f"{cat}.json"), dict(sorted(en_map.items())))

    flat_texts = sorted({v for m in buckets.values() for v in m.values()})

    if args.no_translate:
        for cat, en_map in sorted(buckets.items()):
            zh_map: Dict[str, str] = {}
            for k, en_val in en_map.items():
                prev = prev_zh_flat.get(k)
                if prev and prev != en_val:
                    zh_map[k] = prev
                else:
                    zh_map[k] = en_val
            write_json(os.path.join(OUT_ZH_DIR, f"{cat}.json"), dict(sorted(zh_map.items())))
    else:
        tr = translate_batch(flat_texts)
        for cat, en_map in sorted(buckets.items()):
            zh_map = {k: tr.get(v, v) for k, v in en_map.items()}
            write_json(os.path.join(OUT_ZH_DIR, f"{cat}.json"), dict(sorted(zh_map.items())))

    # Drop stale category files (optional cleanup)
    en_names = {f"{c}.json" for c in buckets}
    zh_names = en_names.copy()
    for d, keep in ((OUT_EN_DIR, en_names), (OUT_ZH_DIR, zh_names)):
        if not os.path.isdir(d):
            continue
        for fn in os.listdir(d):
            if fn.endswith(".json") and fn not in keep and fn != "_readme.json":
                try:
                    os.remove(os.path.join(d, fn))
                    print(f"removed stale {os.path.join(d, fn)}")
                except OSError:
                    pass

    legacy_en = os.path.join(ROOT, "Data", "Localization", "strings.en.json")
    legacy_zh = os.path.join(ROOT, "Data", "Localization", "strings.zh-Hans.json")
    for p in (legacy_en, legacy_zh):
        if os.path.isfile(p):
            try:
                os.remove(p)
                print(f"removed legacy monolith {p}")
            except OSError as e:
                print(f"could not remove {p}: {e}", file=sys.stderr)

    print(f"wrote {OUT_EN_DIR}/*.json and {OUT_ZH_DIR}/*.json")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

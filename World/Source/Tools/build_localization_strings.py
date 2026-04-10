#!/usr/bin/env python3
"""
Scan World/Source for translatable string literals and emit split JSON under
Data/Localization/en/<category>.json and Data/Localization/zh-Hans/<category>.json.

Categories: scripts-quests, scripts-books (subpaths), else scripts-* / system.

Patterns:
  - SendMessage / SendAsciiMessage / Say (all Scripts)
  - Quests & Books trees: builder.Append("..."), Title=/Description=/etc., DummyObjective,
    CollectObjective name string, TextDefinition("..."), AddHtml (incl. verbatim @"),
    AddLabel, ItemReward("...", MLQuestSystem.Tell(..., "..."), etc.

Keys: SHA-256 UTF-8 of exact English (matches Server.Localization.StringKey.ForEnglish).

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

# Quest / book–specific (also applied when path matches Quests/ or Books/)
RE_APPEND = re.compile(
    r"(?:\bbuilder|\b(?:sb|str|desc|msg|s|text))\s*\.Append\s*\(\s*\"((?:\\.|[^\"\\])*)\"",
    re.MULTILINE,
)
RE_QUEST_ASSIGN = re.compile(
    r"\b(?:Title|Description|RefusalMessage|InProgressMessage|CompletionMessage|CompletionNotice|Name|ScrollMessage)\s*=\s*\"((?:\\.|[^\"\\])*)\"",
    re.MULTILINE,
)
RE_DUMMY_OBJ = re.compile(r"new\s+DummyObjective\s*\(\s*\"((?:\\.|[^\"\\])*)\"\s*\)")
RE_COLLECT_NAME = re.compile(
    r"new\s+CollectObjective\s*\([^,]+,\s*[^,]+,\s*\"((?:\\.|[^\"\\])*)\"\s*\)"
)
RE_TEXT_DEF = re.compile(r"new\s+TextDefinition\s*\(\s*\"((?:\\.|[^\"\\])*)\"\s*\)")
RE_ITEM_REWARD = re.compile(r"new\s+ItemReward\s*\(\s*\"((?:\\.|[^\"\\])*)\"")
RE_TELL = re.compile(
    r"\bMLQuestSystem\.Tell\s*\([^,]+,\s*[^,]+,\s*\"((?:\\.|[^\"\\])*)\""
)
RE_ADD_LABEL = re.compile(
    r"\bAddLabel\s*\(\s*[^,]+,\s*[^,]+,\s*[^,]+,\s*\"((?:\\.|[^\"\\])*)\""
)
RE_ADD_HTML_TEXT = re.compile(
    r"TextDefinition\.AddHtmlText\s*\(\s*[^,]+,\s*[^,]+,\s*[^,]+,\s*[^,]+,\s*\"((?:\\.|[^\"\\])*)\""
)


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
        if len(parts) > 2 and parts[1] == "Engines and Systems" and parts[2] == "Quests":
            return "scripts-quests"
        if parts[1] == "Items" and len(parts) > 2 and parts[2] == "Books":
            return "scripts-books"
        seg = parts[1].lower().replace(" ", "-")
        return "scripts-" + seg
    if parts[0] == "System":
        return "system"
    return "misc"


def is_quest_or_book_path(abs_path: str) -> bool:
    rel = os.path.relpath(abs_path, SOURCE_ROOT).replace("\\", "/")
    return "/Engines and Systems/Quests/" in "/" + rel + "/" or "/Items/Books/" in "/" + rel + "/"


def extract_addhtml_fifth_arg(data: str) -> List[str]:
    """Fifth argument to AddHtml( x, y, w, h, TEXT, ... ) — regular or verbatim @\"\"\"."""
    found: List[str] = []
    i = 0
    while True:
        idx = data.find("AddHtml(", i)
        if idx < 0:
            break
        p = idx + len("AddHtml(")
        depth = 1
        commas = 0
        start_fifth = None
        while p < len(data) and depth > 0:
            c = data[p]
            if c == "(":
                depth += 1
            elif c == ")":
                depth -= 1
            elif c == "," and depth == 1:
                commas += 1
                if commas == 4:
                    start_fifth = p + 1
                    break
            p += 1
        if start_fifth is None:
            i = idx + 1
            continue
        p = start_fifth
        while p < len(data) and data[p] in " \t\n\r":
            p += 1
        if p + 1 < len(data) and data[p : p + 2] == '@"':
            p += 2
            sb: List[str] = []
            while p < len(data):
                if data[p] == '"' and p + 1 < len(data) and data[p + 1] == '"':
                    sb.append('"')
                    p += 2
                    continue
                if data[p] == '"':
                    found.append("".join(sb))
                    break
                sb.append(data[p])
                p += 1
        elif p < len(data) and data[p] == '"':
            p += 1
            m = re.match(r"((?:\\.|[^\"\\])*)\"", data[p:])
            if m:
                found.append(csharp_unescape(m.group(1)))
        i = idx + 1
    return found


def collect_strings_from_file(path: str, data: str) -> List[str]:
    texts: List[str] = []
    for rx in (RE_SEND, RE_SAY):
        for m in rx.finditer(data):
            texts.append(csharp_unescape(m.group(1)))

    if "Scripts" in path.replace("\\", "/") and is_quest_or_book_path(path):
        for rx in (
            RE_APPEND,
            RE_QUEST_ASSIGN,
            RE_DUMMY_OBJ,
            RE_COLLECT_NAME,
            RE_TEXT_DEF,
            RE_ITEM_REWARD,
            RE_TELL,
            RE_ADD_LABEL,
            RE_ADD_HTML_TEXT,
        ):
            for m in rx.finditer(data):
                texts.append(csharp_unescape(m.group(1)))
        texts.extend(extract_addhtml_fifth_arg(data))

    return [t for t in texts if t and t.strip()]


def iter_cs_files(base: str) -> Iterable[str]:
    for dirpath, _, files in os.walk(base):
        for fn in files:
            if fn.endswith(".cs"):
                yield os.path.join(dirpath, fn)


def collect_by_category() -> Dict[str, Dict[str, str]]:
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

        for text in collect_strings_from_file(path, data):
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
            if not fn.endswith(".json") or fn == "_index.json":
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

    for cat, en_map in sorted(buckets.items()):
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

    en_names = {f"{c}.json" for c in buckets}
    for d, keep in ((OUT_EN_DIR, en_names), (OUT_ZH_DIR, en_names)):
        if not os.path.isdir(d):
            continue
        for fn in os.listdir(d):
            if fn.endswith(".json") and fn not in keep and fn not in ("_index.json",):
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

#!/usr/bin/env python3
"""
Scan World/Source for translatable string literals and emit split JSON under
Data/Localization/en/<category>.json and Data/Localization/zh-Hans/<category>.json.

Categories: scripts-quests, scripts-books (subpaths), else scripts-* / system.

Patterns:
  - SendMessage / SendAsciiMessage / Say (all Source, including hue-first overloads)
  - AddHtml / AddLabel / AddTooltip / LabelTo / *OverheadMessage / Broadcast string literals
  - Quests & Books trees: builder.Append("..."), Title=/Description=/etc., DummyObjective,
    CollectObjective name string, TextDefinition("..."), AddHtml (incl. verbatim @"),
    AddLabel, ItemReward("...", MLQuestSystem.Tell(..., "..."), etc.
  - Items/Books: book.BookText = / +=, BookTitle = / BookAuthor = string literals,
    BasicHelp() string segments, mercrate = (DynamicBook / LoreBook).
  - DynamicBook.cs: public const string … = @\"…\" bodies (WorkShoppesBookText,
    BasicHelpBookText, RuneJournalBookText) for StringCatalog hash alignment.

Keys: SHA-256 UTF-8 of exact English (matches Server.Localization.StringKey.ForEnglish).
  Logical keys (e.g. books.dynamic.*) in existing JSON are preserved across rebuilds.

Usage:
  python3 build_localization_strings.py [--no-translate] [--prune-stale-locale-files] [--fail-on-translated-zh-drop]

  By default, extra ``*.json`` files under ``en/`` and ``zh-Hans/`` are **not** deleted (safe).
  Pass ``--prune-stale-locale-files`` only when you intend to remove unknown JSON (see ``keep_extra``).

  After each run, ``Data/Localization/tools-output/extractor-key-drop-report.json`` lists hash / logical
  keys that disappeared from category JSON (often because the English literal left the C# scan). Use
  ``--fail-on-translated-zh-drop`` to exit non-zero if any dropped **hash** key had zh different from en
  (likely loss of reviewed Chinese).
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
# First argument must not embed GreeterKey/RaceLocalization.Key calls (comma inside nested call
# used to wrongly capture the logical key as the message literal).
RE_SEND_WITH_PREFIX = re.compile(
    r"\b(?:SendMessage|SendAsciiMessage)\s*\(\s*(?:(?!GreeterKey\()(?!RaceLocalization\.Key\()[^,])+,\s*\"((?:\\.|[^\"\\])*)\"",
    re.MULTILINE,
)
RE_SAY = re.compile(r"\bSay\s*\(\s*\"((?:\\.|[^\"\\])*)\"", re.MULTILINE)
RE_SAY_LOCALIZED = re.compile(
    r"\bSayLocalized(?:Format)?\s*\(\s*[^,\n]+,\s*\"((?:\\.|[^\"\\])*)\"",
    re.MULTILINE,
)
RE_TEXT_DEF = re.compile(r"new\s+TextDefinition\s*\(\s*\"((?:\\.|[^\"\\])*)\"\s*\)")
RE_ADD_LABEL = re.compile(
    r"\bAddLabel\s*\(\s*[^,]+,\s*[^,]+,\s*[^,]+,\s*\"((?:\\.|[^\"\\])*)\""
)
RE_ADD_LABEL_CROPPED = re.compile(
    r"\bAddLabelCropped\s*\(\s*[^,]+,\s*[^,]+,\s*[^,]+,\s*[^,]+,\s*[^,]+,\s*\"((?:\\.|[^\"\\])*)\""
)
RE_LABEL_TO = re.compile(
    r"\bLabelTo\s*\(\s*[^,\n]+,\s*\"((?:\\.|[^\"\\])*)\"",
    re.MULTILINE,
)
RE_ADD_TOOLTIP = re.compile(
    r"\bAddTooltip\s*\(\s*\"((?:\\.|[^\"\\])*)\"",
    re.MULTILINE,
)
RE_BROADCAST = re.compile(
    r"\b(?:World\.)?Broadcast\s*\(\s*[^,\n]+,\s*[^,\n]+,\s*\"((?:\\.|[^\"\\])*)\"",
    re.MULTILINE,
)
RE_OVERHEAD_STRING = re.compile(
    r"\b(?:LocalOverheadMessage|NonlocalOverheadMessage|PublicOverheadMessage|PrivateOverheadMessage)\s*\(\s*[^,\n]+,\s*[^,\n]+,\s*(?:true|false|[A-Za-z_][A-Za-z0-9_\.]*)\s*,\s*\"((?:\\.|[^\"\\])*)\"",
    re.MULTILINE,
)

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
RE_ITEM_REWARD = re.compile(r"new\s+ItemReward\s*\(\s*\"((?:\\.|[^\"\\])*)\"")
RE_TELL = re.compile(
    r"\bMLQuestSystem\.Tell\s*\([^,]+,\s*[^,]+,\s*\"((?:\\.|[^\"\\])*)\""
)
RE_ADD_HTML_TEXT = re.compile(
    r"TextDefinition\.AddHtmlText\s*\(\s*[^,]+,\s*[^,]+,\s*[^,]+,\s*[^,]+,\s*\"((?:\\.|[^\"\\])*)\""
)

# DynamicBook / LoreBook — literal assignments (matches runtime BookText / titles for StringCatalog)
RE_BOOKTEXT_ASSIGN = re.compile(
    r"\bbook\.BookText\s*=\s*\"((?:\\.|[^\"\\])*)\"",
    re.MULTILINE,
)
RE_BOOKTEXT_IADD = re.compile(
    r"\bbook\.BookText\s*\+=\s*\"((?:\\.|[^\"\\])*)\"",
    re.MULTILINE,
)
RE_BOOK_TITLE_ASSIGN = re.compile(
    r"\bBookTitle\s*=\s*\"((?:\\.|[^\"\\])*)\"",
    re.MULTILINE,
)
RE_BOOK_AUTHOR_ASSIGN = re.compile(
    r"\bBookAuthor\s*=\s*\"((?:\\.|[^\"\\])*)\"",
    re.MULTILINE,
)
RE_BOOK_MERCRATE = re.compile(
    r"\bmercrate\s*=\s*\"((?:\\.|[^\"\\])*)\"",
    re.MULTILINE,
)
# BasicHelp(): split literals around MySettings.S_ServerName and text += "..."
RE_BASICHELP_TEXT_START = re.compile(
    r"\bstring\s+text\s*=\s*\"((?:\\.|[^\"\\])*)\"\s*\+",
    re.MULTILINE,
)
RE_BASICHELP_AFTER_SERVERNAME = re.compile(
    r"MySettings\.S_ServerName\s*\+\s*\"((?:\\.|[^\"\\])*)\"",
    re.MULTILINE,
)
RE_BASICHELP_TEXT_CONCAT = re.compile(
    r"\btext\s*=\s*text\s*\+\s*\"((?:\\.|[^\"\\])*)\"",
    re.MULTILINE,
)
RE_HELP_ADD_ACTION = re.compile(
    r"\bAddAction\s*\(\s*[^,\n]+,\s*[^,\n]+,\s*[^,\n]+,\s*\"((?:\\.|[^\"\\])*)\"",
    re.MULTILINE,
)
RE_HELP_ADD_SETTING = re.compile(
    r"\bAddSetting\s*\(\s*[^,\n]+,\s*[^,\n]+,\s*[^,\n]+,\s*\"((?:\\.|[^\"\\])*)\"",
    re.MULTILINE,
)
RE_HELP_TOOLBAR_ROW = re.compile(
    r"\bAddMagicToolbarRow\s*\(\s*[^,\n]+,\s*[^,\n]+,\s*[^,\n]+,\s*\"((?:\\.|[^\"\\])*)\"",
    re.MULTILINE,
)
RE_HELP_ASSIGN = re.compile(
    r"\b(?:string\s+)?(?P<name>HelpText|title|info)\s*=\s*(?P<expr>.*?);",
    re.MULTILINE | re.DOTALL,
)
RE_MYLIBRARY_TITLE = re.compile(
    r"\btitle\s*=\s*\"((?:\\.|[^\"\\])*)\"\s*;",
    re.MULTILINE,
)
RE_TRADE_BOOK_TEXT = re.compile(
    r"\bstring\s+(?P<name>rock|sand|leather|bone|tailoring|cloth|wood|carve|craft|scales|smithing|tinkering|mining|crafting|p1|p2)\s*=\s*(?P<expr>.*?);",
    re.MULTILINE | re.DOTALL,
)
RE_INFO_GUMP_LITERAL = re.compile(
    r"new\s+InfoGump\s*\(\s*[^,\n]+,\s*[^,\n]+,\s*[^,\n]+,\s*\"((?:\\.|[^\"\\])*)\"",
    re.MULTILINE,
)

# ResolveText / Resolve / ResolveFormat helpers used in localized gumps and NPC speech
RE_RESOLVE_TEXT = re.compile(
    r"\bResolveText\s*\(\s*[^,\n]+,\s*\"((?:\\.|[^\"\\])*)\"",
    re.MULTILINE,
)
RE_RESOLVE_PLAIN = re.compile(
    r"(?<![A-Za-z])Resolve\s*\(\s*[^,\n]+,\s*\"((?:\\.|[^\"\\])*)\"",
    re.MULTILINE,
)
RE_TRY_RESOLVE = re.compile(
    r"\bTryResolve\s*\(\s*[^,\n]+,\s*\"((?:\\.|[^\"\\])*)\"",
    re.MULTILINE,
)
RE_RESOLVE_FORMAT = re.compile(
    r"\bResolveFormat\s*\(\s*[^,\n]+,\s*\"((?:\\.|[^\"\\])*)\"",
    re.MULTILINE,
)
RE_RESOLVE_QUEST_CATALOG = re.compile(
    r"\bResolveQuestCatalogString\s*\(\s*[^,]+,\s*\"((?:\\.|[^\"\\])*)\"",
    re.MULTILINE,
)
RE_ADD_HTML_TEXT_RESOLVE = re.compile(
    r"TextDefinition\.AddHtmlText\s*\([^,\n]+,\s*[^,\n]+,\s*[^,\n]+,\s*[^,\n]+,\s*ResolveText\s*\([^,\n]+,\s*\"((?:\\.|[^\"\\]*))\"\s*\)",
    re.MULTILINE,
)

# AbilityBook switch-case variable assignments
RE_ABILITY_ASSIGN = re.compile(
    r"\b(?:myAttack|myDescribe1|myDescribe2)\s*=\s*\"((?:\\.|[^\"\\])*)\"",
    re.MULTILINE,
)

RE_HASH_KEY = re.compile(r"^s\.[0-9a-f]{16}$")
# Dot-separated logical ids (shardgreeter.*, racepotions.*) are not English player text; never catalog as hash keys.
RE_LOGICAL_KEYISH_LITERAL = re.compile(r"^[a-z][a-z0-9_]*(?:\.[a-z][a-z0-9_]*)+$")


def is_probable_localization_logical_key_id(s: str) -> bool:
    t = (s or "").strip()
    if len(t) < 5 or "." not in t:
        return False
    return bool(RE_LOGICAL_KEYISH_LITERAL.match(t))


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


def extract_dynamic_book_public_const_verbatim(data: str) -> List[str]:
    """Bodies of `public const string Name = @\"...\"` in DynamicBook catalog helpers."""
    found: List[str] = []
    marker = "public const string "
    i = 0
    while True:
        j = data.find(marker, i)
        if j < 0:
            break
        eq = data.find('= @"', j)
        if eq < 0 or eq > j + 200:
            i = j + 1
            continue
        p = eq + len('= @"')
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
        i = j + 1
    return found


def extract_string_catalog_resolve_verbatim(data: str) -> List[str]:
    """Second argument to StringCatalog.Resolve / ResolveFormat when it is a verbatim C# string (@\"...\")."""
    found: List[str] = []
    markers = ("StringCatalog.Resolve(", "StringCatalog.ResolveFormat(")
    i = 0
    while True:
        best = -1
        which_len = 0
        for m in markers:
            j = data.find(m, i)
            if j >= 0 and (best < 0 or j < best):
                best = j
                which_len = len(m)
        if best < 0:
            break
        p = best + which_len
        depth = 1
        first_comma: int | None = None
        while p < len(data) and depth > 0:
            c = data[p]
            if c == "(":
                depth += 1
            elif c == ")":
                depth -= 1
            elif c == "," and depth == 1:
                first_comma = p
                break
            p += 1
        if first_comma is None:
            i = best + 1
            continue
        p = first_comma + 1
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
        i = best + 1
    return found


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


def join_csharp_string_literals(expr: str) -> str:
    expr = re.sub(r"(?m)^\s*//.*$", "", expr)
    parts = [csharp_unescape(m.group(1)) for m in re.finditer(r"\"((?:\\.|[^\"\\])*)\"", expr)]
    return "".join(parts)


def collect_targeted_ui_strings(path: str, data: str) -> List[str]:
    rel = path.replace("\\", "/")
    texts: List[str] = []

    if rel.endswith("/Scripts/System/Help/HelpGump.cs"):
        for m in RE_HELP_ADD_ACTION.finditer(data):
            texts.append(csharp_unescape(m.group(1)))
        for m in RE_HELP_ADD_SETTING.finditer(data):
            texts.append(csharp_unescape(m.group(1)))
        for m in RE_HELP_TOOLBAR_ROW.finditer(data):
            texts.append(csharp_unescape(m.group(1)))

        for m in RE_HELP_ASSIGN.finditer(data):
            name = m.group("name")
            expr = m.group("expr")
            if name == "HelpText" and "?" in expr:
                continue
            joined = join_csharp_string_literals(expr)
            if joined:
                texts.append(joined)

    if rel.endswith("/Scripts/System/Commands/Player/MyLibrary.cs"):
        for m in RE_MYLIBRARY_TITLE.finditer(data):
            texts.append(csharp_unescape(m.group(1)))

    if "/Scripts/Items/Books/Trades/" in rel:
        for m in RE_TRADE_BOOK_TEXT.finditer(data):
            joined = join_csharp_string_literals(m.group("expr"))
            if joined:
                texts.append(joined)

    if rel.endswith("/InfoGump.cs"):
        for m in RE_INFO_GUMP_LITERAL.finditer(data):
            texts.append(csharp_unescape(m.group(1)))

    if rel.endswith("/AbilityBook.cs"):
        for m in RE_ABILITY_ASSIGN.finditer(data):
            texts.append(csharp_unescape(m.group(1)))

    return texts


def collect_strings_from_file(path: str, data: str) -> List[str]:
    texts: List[str] = []
    for rx in (
        RE_SEND,
        RE_SEND_WITH_PREFIX,
        RE_SAY,
        RE_SAY_LOCALIZED,
        RE_TEXT_DEF,
        RE_ADD_LABEL,
        RE_ADD_LABEL_CROPPED,
        RE_LABEL_TO,
        RE_ADD_TOOLTIP,
        RE_BROADCAST,
        RE_OVERHEAD_STRING,
    ):
        for m in rx.finditer(data):
            texts.append(csharp_unescape(m.group(1)))

    if "/Scripts/" in path.replace("\\", "/") or "/System/" in path.replace("\\", "/"):
        texts.extend(extract_addhtml_fifth_arg(data))
        texts.extend(extract_string_catalog_resolve_verbatim(data))
        for rx in (
            RE_RESOLVE_TEXT,
            RE_RESOLVE_PLAIN,
            RE_TRY_RESOLVE,
            RE_RESOLVE_FORMAT,
            RE_RESOLVE_QUEST_CATALOG,
            RE_ADD_HTML_TEXT_RESOLVE,
        ):
            for m in rx.finditer(data):
                texts.append(csharp_unescape(m.group(1)))

    if "Scripts" in path.replace("\\", "/") and is_quest_or_book_path(path):
        for rx in (
            RE_APPEND,
            RE_QUEST_ASSIGN,
            RE_DUMMY_OBJ,
            RE_COLLECT_NAME,
            RE_ITEM_REWARD,
            RE_TELL,
            RE_ADD_HTML_TEXT,
        ):
            for m in rx.finditer(data):
                texts.append(csharp_unescape(m.group(1)))

    if "Scripts" in path.replace("\\", "/") and "/Items/Books/" in path.replace("\\", "/"):
        for rx in (
            RE_BOOKTEXT_ASSIGN,
            RE_BOOKTEXT_IADD,
            RE_BOOK_TITLE_ASSIGN,
            RE_BOOK_AUTHOR_ASSIGN,
            RE_BOOK_MERCRATE,
            RE_BASICHELP_TEXT_START,
            RE_BASICHELP_AFTER_SERVERNAME,
            RE_BASICHELP_TEXT_CONCAT,
        ):
            for m in rx.finditer(data):
                texts.append(csharp_unescape(m.group(1)))

    rel_path = os.path.relpath(path, SOURCE_ROOT).replace("\\", "/")
    if rel_path == "Scripts/Items/Books/DynamicBook.cs":
        texts.extend(extract_dynamic_book_public_const_verbatim(data))

    texts.extend(collect_targeted_ui_strings(path, data))

    return [t for t in texts if t and t.strip() and not is_probable_localization_logical_key_id(t)]


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


def load_category_json(path: str) -> Dict[str, str]:
    if not os.path.isfile(path):
        return {}
    try:
        with open(path, encoding="utf-8") as f:
            return json.load(f)
    except Exception:
        return {}


def merge_preserved_logical_keys(en_json_path: str, en_map: Dict[str, str]) -> None:
    """Re-insert keys that are not scanner-generated hash keys (e.g. books.dynamic.*)."""
    if not os.path.isfile(en_json_path):
        return
    try:
        with open(en_json_path, encoding="utf-8") as f:
            prev = json.load(f)
    except Exception:
        return
    for k, v in prev.items():
        if RE_HASH_KEY.match(k):
            continue
        if k not in en_map:
            en_map[k] = v


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--no-translate", action="store_true")
    ap.add_argument(
        "--prune-stale-locale-files",
        action="store_true",
        help="Remove *.json under en/ and zh-Hans/ that are not scanner categories or keep_extra (dangerous).",
    )
    ap.add_argument(
        "--fail-on-translated-zh-drop",
        action="store_true",
        help="Exit with code 2 if any dropped hash key had zh != en in the previous zh-Hans category file.",
    )
    args = ap.parse_args()

    buckets = collect_by_category()
    total_keys = sum(len(v) for v in buckets.values())
    print(f"categories: {len(buckets)}, unique string keys: {total_keys}")

    prev_en_cat: Dict[str, Dict[str, str]] = {}
    prev_zh_cat: Dict[str, Dict[str, str]] = {}
    for cat in sorted(buckets.keys()):
        prev_en_cat[cat] = load_category_json(os.path.join(OUT_EN_DIR, f"{cat}.json"))
        prev_zh_cat[cat] = load_category_json(os.path.join(OUT_ZH_DIR, f"{cat}.json"))

    prev_zh_flat = load_all_zh_flat(OUT_ZH_DIR)
    if not prev_zh_flat:
        legacy_zh = os.path.join(ROOT, "Data", "Localization", "strings.zh-Hans.json")
        prev_zh_flat = load_legacy_monolith(legacy_zh)

    os.makedirs(OUT_EN_DIR, exist_ok=True)
    os.makedirs(OUT_ZH_DIR, exist_ok=True)

    final_en_by_cat: Dict[str, Dict[str, str]] = {}

    for cat, en_map in sorted(buckets.items()):
        en_out = os.path.join(OUT_EN_DIR, f"{cat}.json")
        merge_preserved_logical_keys(en_out, en_map)
        final_en_by_cat[cat] = dict(en_map)
        write_json(en_out, dict(sorted(final_en_by_cat[cat].items())))

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
        # Only translate strings that are not yet in zh (or identical to English).
        # This preserves any existing Chinese translations even when Google Translate fails.
        to_translate = sorted({
            v for m in buckets.values() for k, v in m.items()
            if not (prev_zh_flat.get(k) and prev_zh_flat.get(k) != v)
        })
        tr = translate_batch(to_translate)
        for cat, en_map in sorted(buckets.items()):
            zh_map: Dict[str, str] = {}
            for k, en_val in en_map.items():
                prev = prev_zh_flat.get(k)
                if prev and prev != en_val:
                    zh_map[k] = prev          # keep existing Chinese
                else:
                    zh_map[k] = tr.get(en_val, en_val)   # use translation or English fallback
            write_json(os.path.join(OUT_ZH_DIR, f"{cat}.json"), dict(sorted(zh_map.items())))

    drop_report: List[Dict[str, str]] = []
    for cat in sorted(buckets.keys()):
        old_en = prev_en_cat.get(cat, {})
        new_en = final_en_by_cat.get(cat, {})
        dropped = sorted(set(old_en.keys()) - set(new_en.keys()))
        old_zh = prev_zh_cat.get(cat, {})
        for k in dropped:
            drop_report.append(
                {
                    "category": f"{cat}.json",
                    "key": k,
                    "previousEn": old_en.get(k),
                    "previousZh": old_zh.get(k),
                }
            )

    tools_out = os.path.join(ROOT, "Data", "Localization", "tools-output")
    os.makedirs(tools_out, exist_ok=True)
    report_path = os.path.join(tools_out, "extractor-key-drop-report.json")
    with open(report_path, "w", encoding="utf-8") as rf:
        json.dump({"droppedKeyCount": len(drop_report), "droppedKeys": drop_report}, rf, ensure_ascii=False, indent=2)
        rf.write("\n")
    print(f"wrote {report_path} ({len(drop_report)} dropped keys vs pre-run category JSON)")

    if args.fail_on_translated_zh_drop:
        bad = [
            r
            for r in drop_report
            if RE_HASH_KEY.match(r.get("key", ""))
            and r.get("previousZh")
            and r.get("previousEn")
            and r["previousZh"] != r["previousEn"]
        ]
        if bad:
            print(
                f"error: --fail-on-translated-zh-drop: {len(bad)} hash key(s) lost Chinese (see report). First: {bad[0]['key']}",
                file=sys.stderr,
            )
            return 2

    en_names = {f"{c}.json" for c in buckets}
    # Hash-keyed templates maintained by gen_vendor_npc_speech_en.py; zh file synced by apply_vendor_npc_speech_zh.py (JSON is source of truth)
    # Logical-key JSON not produced by this scanner (TryResolveByKey / curated copy). Never delete — removal drops zh/en at runtime.
    keep_extra = frozenset(
        {
            "_index.json",
            "vendor_npc_speech.json",
            "race-system.json",
            "shard-greeter.json",
            "stats-gump.json",
            "temptation-gump.json",
            "thewar-quest.json",
        }
    )
    if args.prune_stale_locale_files:
        for d, keep in ((OUT_EN_DIR, en_names), (OUT_ZH_DIR, en_names)):
            if not os.path.isdir(d):
                continue
            for fn in os.listdir(d):
                if fn.endswith(".json") and fn not in keep and fn not in keep_extra:
                    try:
                        os.remove(os.path.join(d, fn))
                        print(f"removed stale {os.path.join(d, fn)}")
                    except OSError:
                        pass
    elif any(os.path.isdir(d) for d in (OUT_EN_DIR, OUT_ZH_DIR)):
        extra = []
        for d, keep in ((OUT_EN_DIR, en_names), (OUT_ZH_DIR, en_names)):
            if not os.path.isdir(d):
                continue
            for fn in os.listdir(d):
                if fn.endswith(".json") and fn not in keep_extra and fn not in en_names:
                    extra.append(os.path.join(d, fn))
        if extra:
            print(
                "note: extra locale JSON present (not removed; use --prune-stale-locale-files to delete): "
                + ", ".join(sorted(os.path.basename(x) for x in extra))
            )

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

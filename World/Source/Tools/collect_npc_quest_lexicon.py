#!/usr/bin/env python3
"""
Collect English strings for:
  1) NPC dialogue under Scripts/Mobiles (SendMessage / Say / overhead literals)
  2) Quest-related lexicon: lands (Map.cs), quest places & item vocab (QuestStories.cs)

Writes review files under Data/Localization/_extracted/ (gitignored or committed per project policy).
"""
from __future__ import annotations

import os
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "Source"
MOBILES = SOURCE / "Scripts" / "Mobiles"
MAP_CS = SOURCE / "System" / "Map.cs"
QUEST_STORIES = SOURCE / "Scripts" / "Engines and Systems" / "Quests" / "QuestStories.cs"
WORLD_CS = SOURCE / "Scripts" / "System" / "Regions" / "Core" / "World.cs"
OUT_DIR = ROOT / "Data" / "Localization" / "_extracted"

RE_SEND = re.compile(
    r"\b(?:SendMessage|SendAsciiMessage)\s*\(\s*\"((?:\\.|[^\"\\])*)\"",
    re.MULTILINE,
)
RE_SEND_PREFIX = re.compile(
    r"\b(?:SendMessage|SendAsciiMessage)\s*\(\s*[^,\n]+,\s*\"((?:\\.|[^\"\\])*)\"",
    re.MULTILINE,
)
RE_SAY = re.compile(r"\bSay\s*\(\s*\"((?:\\.|[^\"\\])*)\"", re.MULTILINE)
RE_OVERHEAD = re.compile(
    r"\b(?:LocalOverheadMessage|NonlocalOverheadMessage|PublicOverheadMessage|PrivateOverheadMessage)\s*\(\s*[^,\n]+,\s*[^,\n]+,\s*(?:true|false|[A-Za-z_][A-Za-z0-9_\.]*)\s*,\s*\"((?:\\.|[^\"\\])*)\"",
    re.MULTILINE,
)


def csharp_unescape(s: str) -> str:
    out = []
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
        elif e in "\\\"":
            out.append(e)
        elif e == "0":
            out.append("\0")
        else:
            out.append(e)
    return "".join(out)


def collect_from_mobiles() -> set[str]:
    found: set[str] = set()
    if not MOBILES.is_dir():
        return found
    for path in sorted(MOBILES.rglob("*.cs")):
        try:
            data = path.read_text(encoding="utf-8", errors="ignore")
        except OSError:
            continue
        for rx in (RE_SEND, RE_SEND_PREFIX, RE_SAY, RE_OVERHEAD):
            for m in rx.finditer(data):
                t = csharp_unescape(m.group(1)).strip()
                if len(t) < 2 or t.startswith("<"):
                    continue
                found.add(t)
    return found


def collect_lands_from_map_cs() -> set[str]:
    if not MAP_CS.is_file():
        return set()
    data = MAP_CS.read_text(encoding="utf-8", errors="ignore")
    out: set[str] = set()
    for m in re.finditer(r'return\s+"((?:\\.|[^"\\])*)"\s*;', data):
        s = csharp_unescape(m.group(1))
        if s and ("Land" in data[max(0, m.start() - 80) : m.start()] or "land" in s.lower()):
            out.add(s)
    # LandName / LandNameShort explicit branches
    for m in re.finditer(r'else if \( land == Land\.\w+\)\s*\n\s*return "([^"]+)"', data):
        out.add(m.group(1))
    for m in re.finditer(r'case Land\.\w+:\s*return "([^"]+)"', data):
        out.add(m.group(1))
    return {x for x in out if x}


def collect_world_fallback_regions() -> set[str]:
    if not WORLD_CS.is_file():
        return set()
    data = WORLD_CS.read_text(encoding="utf-8", errors="ignore")
    return {csharp_unescape(m.group(1)) for m in re.finditer(r'regionName = "((?:\\.|[^"\\])*)"', data)}


def collect_quest_stories_literals() -> set[str]:
    if not QUEST_STORIES.is_file():
        return set()
    data = QUEST_STORIES.read_text(encoding="utf-8", errors="ignore")
    out: set[str] = set()
    for m in re.finditer(r'sPlace = "([^"]+)"\s*;?\s*break', data):
        out.add(m.group(1))
    # xItem array (single line)
    m = re.search(r'string\[\]\s+xItem\s*=\s*new\s+string\[\]\s*\{([^}]+)\}', data)
    if m:
        for q in re.findall(r'"([^"]*)"', m.group(1)):
            out.add(q)
    m = re.search(r'string\[\]\s+xAdj\s*=\s*new\s+string\[\]\s*\{([^}]+)\}', data)
    if m:
        for q in re.findall(r'"([^"]*)"', m.group(1)):
            out.add(q)
    for m in re.finditer(r'eAdjective = "([^"]+)"\s*;', data):
        out.add(m.group(1))
    return out


def main() -> int:
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    npc = sorted(collect_from_mobiles())
    quest_lex = collect_lands_from_map_cs() | collect_world_fallback_regions() | collect_quest_stories_literals()
    # de-dupe short noise
    quest_lex = {x.strip() for x in quest_lex if x and len(x.strip()) >= 2}

    (OUT_DIR / "npc-mobiles-dialogue.txt").write_text(
        "# NPC literal strings from Scripts/Mobiles (SendMessage/Say/overhead)\n"
        + "\n".join(npc),
        encoding="utf-8",
    )
    ordered = sorted(quest_lex, key=lambda s: (-len(s), s.lower()))
    (OUT_DIR / "quest-lexicon-english-ordered.txt").write_text(
        "# Quest display fragments (lands, regions, places, item words) longest-first for composite resolve\n"
        + "\n".join(ordered),
        encoding="utf-8",
    )
    print(f"Wrote {len(npc)} NPC strings -> {OUT_DIR / 'npc-mobiles-dialogue.txt'}")
    print(f"Wrote {len(ordered)} quest lexicon lines -> {OUT_DIR / 'quest-lexicon-english-ordered.txt'}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

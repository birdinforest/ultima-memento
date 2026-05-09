#!/usr/bin/env python3
"""
Build a lore / proper-noun glossary from:
  - Data/Localization/en/*.json (all shard strings)
  - World/Source/Scripts/Engines and Systems/Quests/**/*.cs (typeof links + string literals)

Outputs:
  - World/Data/Localization/lore-glossary.json  (machine-readable)
  - World/Documentation/lore-glossary.md        (human-readable)

Relationships:
  - co_occurs_with: other terms appearing in the same English string
  - item_types_from_quest_cs: C# typeof(Foo) -> quest source file
"""
from __future__ import annotations

import json
import os
import re
from collections import Counter, defaultdict
from typing import Dict, List, Set, Tuple

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
LOC_EN = os.path.join(ROOT, "Data", "Localization", "en")
QUEST_CS = os.path.join(ROOT, "Source", "Scripts", "Engines and Systems", "Quests")
OUT_JSON = os.path.join(ROOT, "Data", "Localization", "lore-glossary.json")
OUT_MD = os.path.join(ROOT, "Documentation", "lore-glossary.md")

# Common English words to drop from single-token "proper noun" candidates
STOP_SINGLE = {
    "the", "and", "for", "you", "your", "this", "that", "with", "from", "have", "has",
    "are", "was", "were", "been", "being", "will", "would", "could", "should", "must",
    "not", "but", "what", "when", "where", "which", "who", "how", "why", "all", "any",
    "can", "may", "one", "two", "new", "old", "get", "got", "use", "using", "used",
    "head", "back", "into", "over", "after", "before", "then", "than", "them", "they",
    "here", "there", "some", "such", "only", "also", "just", "like", "make", "made",
    "need", "want", "come", "goes", "going", "gone", "give", "take", "each", "every",
    "other", "more", "most", "many", "much", "very", "even", "still", "well", "back",
    "return", "click", "quest", "item", "items", "gold", "time", "day", "days", "way",
    "smith", "blacksmith", "player", "character", "ingots", "ingot", "ore", "mine",
    "mines", "forge", "hammer", "armor", "weapon", "weapons", "skill", "skills",
    "tips", "following", "complete", "completed", "reward", "rewards", "objective",
    "true", "false", "null", "void", "base", "public", "private", "static", "class",
    # UI / system verbs (not lore keywords)
    "target", "select", "choose", "please", "enter", "error", "format", "string", "filename",
    "color", "name", "she", "yes", "done", "close", "open", "page", "pages", "bank",
    "banks", "gifts", "local", "guilds", "guild", "tinker", "carpenters", "archers",
    "delivery", "cast", "sound", "add", "echo", "animate",
    "these", "those", "sorry", "try", "feel", "fool", "against", "stranger",
}

# Multi-word phrases starting with these are usually not lore names
STOP_PHRASE_START = {
    "The ", "A ", "An ", "You ", "Your ", "This ", "That ", "If ", "When ", "How ",
    "All ", "Some ", "There ", "These ", "Those ", "What ", "Which ", "Return ",
    "Click ", "Double ", "Mark ", "Craft ", "Collect ", "Deliver ", "Speak ",
}

RE_MULTI = re.compile(r"\b(?:[A-Z][a-z]+(?:\s+[A-Z][a-z]+)+)\b")
RE_SINGLE = re.compile(r"\b[A-Z][a-z]{2,}\b")
RE_TYPEOF = re.compile(r"typeof\s*\(\s*([A-Za-z0-9_]+)\s*\)")
RE_TITLE = re.compile(r"\bTitle\s*=\s*\"((?:\\.|[^\"\\])*)\"")
RE_ITEM_REWARD = re.compile(r"new\s+ItemReward\s*\(\s*\"((?:\\.|[^\"\\])*)\"\s*,")


def load_en_strings() -> List[str]:
    texts: List[str] = []
    if not os.path.isdir(LOC_EN):
        return texts
    for fn in sorted(os.listdir(LOC_EN)):
        if not fn.endswith(".json"):
            continue
        path = os.path.join(LOC_EN, fn)
        try:
            with open(path, encoding="utf-8") as f:
                d = json.load(f)
            texts.extend(str(v) for v in d.values() if isinstance(v, str))
        except Exception:
            pass
    return texts


def csharp_unescape(s: str) -> str:
    return (
        s.replace("\\n", "\n")
        .replace("\\r", "\r")
        .replace("\\t", "\t")
        .replace('\\"', '"')
        .replace("\\\\", "\\")
    )


def scan_quest_cs() -> Tuple[Dict[str, List[str]], List[str]]:
    """typeof(Type) -> file paths; extra literal strings from titles/rewards."""
    type_to_files: Dict[str, List[str]] = defaultdict(list)
    extra_strings: List[str] = []

    if not os.path.isdir(QUEST_CS):
        return dict(type_to_files), extra_strings

    for dirpath, _, files in os.walk(QUEST_CS):
        for fn in files:
            if not fn.endswith(".cs"):
                continue
            path = os.path.join(dirpath, fn)
            rel = os.path.relpath(path, ROOT).replace("\\", "/")
            try:
                raw = open(path, encoding="utf-8", errors="ignore").read()
            except OSError:
                continue
            for m in RE_TYPEOF.finditer(raw):
                type_to_files[m.group(1)].append(rel)
            for m in RE_TITLE.finditer(raw):
                extra_strings.append(csharp_unescape(m.group(1)))
            for m in RE_ITEM_REWARD.finditer(raw):
                extra_strings.append(csharp_unescape(m.group(1)))

    return dict(type_to_files), extra_strings


def extract_terms(text: str) -> Set[str]:
    found: Set[str] = set()
    for m in RE_MULTI.finditer(text):
        phrase = m.group(0)
        if any(phrase.startswith(p) for p in STOP_PHRASE_START):
            continue
        if len(phrase) < 6:
            continue
        found.add(phrase)
    for m in RE_SINGLE.finditer(text):
        w = m.group(0)
        if w.lower() in STOP_SINGLE:
            continue
        found.add(w)
    return found


def main() -> int:
    en_texts = load_en_strings()
    type_files, quest_literals = scan_quest_cs()
    all_texts = en_texts + quest_literals

    term_freq: Counter = Counter()
    cooccur: Dict[str, Counter] = defaultdict(Counter)
    term_contexts: Dict[str, List[str]] = defaultdict(list)

    for t in all_texts:
        if not t or len(t) < 3:
            continue
        terms = extract_terms(t)
        for a in terms:
            term_freq[a] += 1
            if len(term_contexts[a]) < 5:
                snippet = t.replace("\n", " ").strip()
                if len(snippet) > 160:
                    snippet = snippet[:157] + "..."
                term_contexts[a].append(snippet)
        for a in terms:
            for b in terms:
                if a < b:
                    cooccur[a][b] += 1
                    cooccur[b][a] += 1

    # Item types as entities linked to quest files
    item_entities = [
        {
            "kind": "item_type",
            "name": tn,
            "source_files": sorted(set(files))[:40],
            "file_count": len(set(files)),
        }
        for tn, files in sorted(type_files.items(), key=lambda x: -len(x[1]))
    ]

    glossary_terms = []
    for term, freq in term_freq.most_common():
        is_multi = " " in term
        # Single-token lore candidates need stronger signal unless tied to a quest item type
        if not is_multi:
            if term not in type_files and freq < 3:
                continue
        else:
            if freq < 2:
                continue
        top_co = cooccur[term].most_common(12)
        glossary_terms.append(
            {
                "canonical_en": term,
                "occurrence_count": freq,
                "linked_item_type": term if term in type_files else None,
                "co_occurs_with": [{"term": x, "shared_strings": c} for x, c in top_co],
                "sample_contexts": term_contexts.get(term, []),
            }
        )

    doc = {
        "version": 1,
        "description": "Heuristic proper-noun and lore-term glossary from localization + quest C#.",
        "stats": {
            "unique_terms": len(glossary_terms),
            "localization_strings": len(en_texts),
            "quest_item_types": len(item_entities),
        },
        "item_types_in_quests": item_entities[:500],
        "terms": glossary_terms[:2000],
    }

    os.makedirs(os.path.dirname(OUT_JSON), exist_ok=True)
    os.makedirs(os.path.dirname(OUT_MD), exist_ok=True)

    with open(OUT_JSON, "w", encoding="utf-8") as f:
        json.dump(doc, f, ensure_ascii=False, indent=2)
        f.write("\n")

    lines = [
        "# Lore glossary (auto-generated)",
        "",
        "This file is produced by `World/Source/Tools/build_lore_glossary.py`.",
        "It lists **heuristic** proper nouns and recurring phrases from English localization",
        "and quest scripts, plus **co-occurrence** (terms that appear in the same string).",
        "",
        "## How to use",
        "",
        "- Pick canonical **Chinese** renderings for high-frequency `canonical_en` values.",
        "- Store approved pairs in `World/Data/Localization/glossary-approved-zh.json` (optional).",
        "- Run `review_translations_glossary.py` to flag mixed English/Chinese or divergent zh.",
        "",
        "## Stats",
        "",
        f"- Unique scored terms: **{doc['stats']['unique_terms']}**",
        f"- Localization English strings scanned: **{doc['stats']['localization_strings']}**",
        f"- Distinct `typeof()` item types under Quests: **{doc['stats']['quest_item_types']}**",
        "",
        "## Top terms (by frequency)",
        "",
    ]

    for entry in glossary_terms[:80]:
        lines.append(f"### `{entry['canonical_en']}`")
        lines.append("")
        lines.append(f"- Occurrences: **{entry['occurrence_count']}**")
        if entry["co_occurs_with"]:
            co = ", ".join(f"`{x['term']}` ({x['shared_strings']})" for x in entry["co_occurs_with"][:8])
            lines.append(f"- Often appears with: {co}")
        if entry["sample_contexts"]:
            lines.append("- Example:")
            lines.append(f"  - {entry['sample_contexts'][0]}")
        lines.append("")

    lines.append("## Item types referenced from quest scripts (sample)")
    lines.append("")
    for ent in item_entities[:40]:
        lines.append(f"- **{ent['name']}** — {ent['file_count']} file(s), e.g. `{ent['source_files'][0]}`")

    with open(OUT_MD, "w", encoding="utf-8") as f:
        f.write("\n".join(lines) + "\n")

    print(f"wrote {OUT_JSON}")
    print(f"wrote {OUT_MD}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

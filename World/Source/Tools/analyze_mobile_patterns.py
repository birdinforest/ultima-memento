#!/usr/bin/env python3
"""
Analyze which C# regex patterns in build_localization_strings.py generate each
surviving mobile localization entry (hash keys from scripts-mobiles.json) by
searching C# source under World/Source/Scripts/Mobiles/.

Phase 1: Scan Mobiles/ for extractor pattern matches → builds pattern index.
Phase 2: Any entry not found in pattern index is searched for as literal text
         across the ENTIRE World/Source/ tree, then pattern-matched.
Phase 3: Any entry still not found is reported as "not found in any C# source".

Outputs:
  tools-output/mobile-pattern-analysis.json
  tools-output/mobile-text-to-shotkey.json
"""

import json
import os
import re
from collections import defaultdict

REPO_ROOT = "/Users/forrrest/projects/UO-Memento/ultima-memento"
C_SHARP_MOBILE = os.path.join(REPO_ROOT, "World/Source/Scripts/Mobiles")
C_SHARP_ALL = os.path.join(REPO_ROOT, "World/Source")
EN_MOBILES = os.path.join(REPO_ROOT, "World/Data/Localization/en/scripts-mobiles.json")
EN_WPT = os.path.join(REPO_ROOT, "World/Data/Localization/en/world-player-text.json")
OUTPUT_ANALYSIS = os.path.join(REPO_ROOT, "World/Data/Localization/tools-output/mobile-pattern-analysis.json")
OUTPUT_TEXT_MAP = os.path.join(REPO_ROOT, "World/Data/Localization/tools-output/mobile-text-to-shotkey.json")

# ---- Regex patterns (from build_localization_strings.py) ----
PATTERNS = {
    "RE_SEND": re.compile(
        r'\b(?:SendMessage|SendAsciiMessage)\s*\(\s*"((?:\\.|[^"\\])*)"'
    ),
    "RE_SEND_WITH_PREFIX": re.compile(
        r'\b(?:SendMessage|SendAsciiMessage)\s*\(\s*[^,]+?,\s*"((?:\\.|[^"\\])*)"'
    ),
    "RE_SAY": re.compile(
        r'\bSay\s*\(\s*"((?:\\.|[^"\\])*)"'
    ),
    "RE_ADD_LABEL": re.compile(
        r'AddLabel\s*\(\s*[^,]+?,\s*[^,]+?,\s*[^,]+?,\s*"((?:\\.|[^"\\])*)"\s*\)'
    ),
    "RE_LABEL_TO": re.compile(
        r'\bLabelTo\s*\(\s*[^,]+?,\s*"((?:\\.|[^"\\])*)"'
    ),
    "RE_ADD_TOOLTIP": re.compile(
        r'\bAddTooltip(?:Html)?\s*\(\s*[^,]+?,\s*"((?:\\.|[^"\\])*)"'
    ),
    "RE_BROADCAST": re.compile(
        r'\bBroadcast\s*\(\s*[^,]+?,\s*"((?:\\.|[^"\\])*)"'
    ),
    "RE_OVERHEAD_STRING": re.compile(
        r'\b(?:PublicOverheadMessage|LocalOverheadMessage)\s*\([^)]*?"((?:\\.|[^"\\])*)"'
    ),
    "RE_SCRIPT_NAME_ASSIGN": re.compile(
        r'^\s*Name\s*=\s*"((?:\\.|[^"\\])*)"\s*;'
    ),
    "RE_RESOLVE_PLAIN": re.compile(
        r'(?<![A-Za-z])Resolve\s*\(\s*[^,\n]+,\s*"((?:\\.|[^"\\])*)"'
    ),
    "RE_RESOLVE_FORMAT": re.compile(
        r'\bResolveFormat\s*\(\s*[^,\n]+,\s*"((?:\\.|[^"\\])*)"'
    ),
    "RE_ADD_HTML_TEXT": re.compile(
        r'AddHtml\s*\(\s*[^,]+?,\s*[^,]+?,\s*[^,]+?,\s*[^,]+?,\s*"((?:\\.|[^"\\])*)"'
    ),
}


def load_json(path):
    with open(path, "r", encoding="utf-8") as f:
        return json.load(f)


def build_text_to_shotkey(wpt_data):
    """Build English text → shotkey map from world-player-text.json (only mob.*)."""
    mapping = {}
    for key, value in wpt_data.items():
        if key.startswith("mob."):
            mapping[value] = key
    return mapping


def collect_csharp_files(root):
    """Collect all .cs files recursively under root."""
    files = []
    for dirpath, dirnames, filenames in os.walk(root):
        # Skip hidden dirs
        dirnames[:] = [d for d in dirnames if not d.startswith(".")]
        for fn in filenames:
            if fn.endswith(".cs"):
                files.append(os.path.join(dirpath, fn))
    return sorted(files)


def normalize_path(path, root):
    """Return path relative to root, with forward slashes."""
    return os.path.relpath(path, root).replace("\\", "/")


def scan_files_for_patterns(csharp_root):
    """
    Walk all .cs files once and extract all pattern matches.

    Returns:
      text_to_info: dict mapping extracted text → {
        "patterns": set of pattern names,
        "files": set of relative file paths
      }
    """
    text_to_info = defaultdict(lambda: {"patterns": set(), "files": set()})
    all_files = collect_csharp_files(csharp_root)

    for abs_path in all_files:
        rel_path = normalize_path(abs_path, csharp_root)
        try:
            with open(abs_path, "r", encoding="utf-8", errors="replace") as f:
                lines = f.readlines()
        except Exception:
            continue

        for line in lines:
            line_matches = defaultdict(set)

            for pname, pcre in PATTERNS.items():
                for match in pcre.finditer(line):
                    text = match.group(1)
                    line_matches[text].add(pname)

            # Resolve overlap: if both RE_SEND and RE_SEND_WITH_PREFIX match
            # the same text on the same line, keep only RE_SEND_WITH_PREFIX
            for text, pats in line_matches.items():
                if "RE_SEND_WITH_PREFIX" in pats and "RE_SEND" in pats:
                    pats.discard("RE_SEND")

            for text, pats in line_matches.items():
                text_to_info[text]["patterns"].update(pats)
                text_to_info[text]["files"].add(rel_path)

    return text_to_info


def find_text_globally(text, all_files):
    """
    Search for literal text in a pre-loaded list of (path, lines) pairs.
    Returns list of (rel_path, [pattern_names]) for files containing the text.
    """
    results = []
    # Use the full C_SHARP_ALL files list
    for abs_path, file_lines in all_files:
        rel_path = normalize_path(abs_path, C_SHARP_ALL)
        for line in file_lines:
            if text not in line:
                continue
            patterns_on_line = set()
            for pname, pcre in PATTERNS.items():
                for match in pcre.finditer(line):
                    extracted = match.group(1)
                    if extracted == text:
                        patterns_on_line.add(pname)
            if patterns_on_line:
                # Resolve overlap
                if "RE_SEND_WITH_PREFIX" in patterns_on_line and "RE_SEND" in patterns_on_line:
                    patterns_on_line.discard("RE_SEND")
                results.append((rel_path, sorted(patterns_on_line)))
    return results


def main():
    # ---- Phase 1: Scan Mobiles/ for extractor patterns ----
    print("Phase 1: Scanning Mobiles/ for extractor pattern matches...")
    mobile_pattern_index = scan_files_for_patterns(C_SHARP_MOBILE)
    print(f"  Found {len(mobile_pattern_index)} unique texts extracted by patterns under Mobiles/")

    # ---- Load data ----
    print("Loading scripts-mobiles.json...")
    mobiles_data = load_json(EN_MOBILES)
    total = len(mobiles_data)
    print(f"  {total} hash-key entries")

    print("Loading world-player-text.json...")
    wpt_data = load_json(EN_WPT)
    text_to_shotkey = build_text_to_shotkey(wpt_data)
    print(f"  {len(text_to_shotkey)} mob.* shotkeys")

    # ---- Pre-load all files under World/Source/ for global search ----
    print("Phase 2: Pre-loading all World/Source/ .cs files for global literal search...")
    all_cs_files = collect_csharp_files(C_SHARP_ALL)
    loaded_files = []
    for abs_path in all_cs_files:
        try:
            with open(abs_path, "r", encoding="utf-8", errors="replace") as f:
                lines = f.readlines()
            loaded_files.append((abs_path, lines))
        except Exception:
            pass
    print(f"  Loaded {len(loaded_files)} .cs files")

    # ---- Build result structures ----
    by_pattern = defaultdict(lambda: {"count": 0, "examples": []})
    all_texts = {}
    matched_in_mobiles = 0
    matched_outside_mobiles = 0
    not_found = 0

    os.makedirs(os.path.dirname(OUTPUT_ANALYSIS), exist_ok=True)

    for idx, (hash_key, english_text) in enumerate(mobiles_data.items()):
        if (idx + 1) % 100 == 0:
            print(f"  Processing {idx+1}/{total}...")

        shotkey = text_to_shotkey.get(english_text, None)

        # Look up in mobile pattern index
        info = mobile_pattern_index.get(english_text)

        if info is not None:
            patterns = sorted(info["patterns"])
            files = sorted(info["files"])
            rel_files = [os.path.join("Mobiles", f) for f in files]
            matched_in_mobiles += 1
        else:
            # Phase 2: search globally
            global_results = find_text_globally(english_text, loaded_files)
            if global_results:
                patterns_set = set()
                files_set = set()
                for rel_path, pats in global_results:
                    patterns_set.update(pats)
                    files_set.add(rel_path)
                patterns = sorted(patterns_set)
                rel_files = sorted(files_set)
                matched_outside_mobiles += 1
            else:
                patterns = []
                rel_files = []
                not_found += 1

        if not patterns:
            entry = {
                "text": english_text,
                "shotkey": shotkey,
                "patterns": [],
                "files": rel_files,
            }
            all_texts[hash_key] = entry
            continue

        # Update by_pattern
        for p in patterns:
            by_pattern[p]["count"] += 1
            if len(by_pattern[p]["examples"]) < 5:
                by_pattern[p]["examples"].append(english_text)

        entry = {
            "text": english_text,
            "shotkey": shotkey,
            "patterns": patterns,
            "files": rel_files,
        }
        all_texts[hash_key] = entry

    # Build output
    by_pattern_sorted = dict(sorted(by_pattern.items(), key=lambda x: -x[1]["count"]))
    total_pattern_matches = sum(v["count"] for v in by_pattern.values())

    output = {
        "total_entries": total,
        "matched_in_mobiles_pattern": matched_in_mobiles,
        "matched_outside_mobiles": matched_outside_mobiles,
        "not_found_anywhere": not_found,
        "total_pattern_matches": total_pattern_matches,
        "note": "total_pattern_matches can exceed total_entries because one string can match multiple patterns",
        "by_pattern": by_pattern_sorted,
        "all_texts_with_patterns": all_texts,
    }

    with open(OUTPUT_ANALYSIS, "w", encoding="utf-8") as f:
        json.dump(output, f, indent=2, ensure_ascii=False)
    print(f"\nWrote {OUTPUT_ANALYSIS}")

    # Write text-to-shotkey map
    text_map = {}
    for hash_key, english_text in mobiles_data.items():
        shotkey = text_to_shotkey.get(english_text, None)
        text_map[hash_key] = {
            "text": english_text,
            "shotkey": shotkey,
        }

    with open(OUTPUT_TEXT_MAP, "w", encoding="utf-8") as f:
        json.dump(text_map, f, indent=2, ensure_ascii=False)
    print(f"Wrote {OUTPUT_TEXT_MAP}")

    # Summary
    print(f"\n{'=' * 60}")
    print(f"SUMMARY")
    print(f"{'=' * 60}")
    print(f"Total entries in scripts-mobiles.json: {total}")
    print(f"Matched by pattern in Mobiles/: {matched_in_mobiles}")
    print(f"Matched by pattern outside Mobiles/: {matched_outside_mobiles}")
    print(f"Not found in any C# source: {not_found}")
    print(f"Total pattern matches (may double-count): {total_pattern_matches}")
    print(f"\nPattern breakdown:")
    for pname, pdata in by_pattern_sorted.items():
        print(f"  {pname}: {pdata['count']}")
    print(f"\nTexts with shotkeys in world-player-text.json: {sum(1 for v in all_texts.values() if v['shotkey'])}")


if __name__ == "__main__":
    main()

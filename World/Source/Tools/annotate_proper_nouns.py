#!/usr/bin/env python3
"""
Annotate zh-Hans localization files with English parenthetical for proper nouns.

Converts:
  【中文】 → 【中文（English）】  (in zh-Hans files with hash keys)
  中文     → 中文（English）     (inline in commontalk-fragment-zh.json)

Already-annotated brackets like 【中文（English）】 are skipped silently.
Only proper noun categories (place, character, creature, item, deity, faction,
dungeon, race) are annotated — concept/system/skill/title terms are skipped.

Commontalk uses English keys as validation reference to avoid false positives.

Usage:
    python3 annotate_proper_nouns.py                # apply all changes
    python3 annotate_proper_nouns.py --dry-run      # preview only
    python3 annotate_proper_nouns.py --zh-hans-only # skip commontalk

Files:
    zh-Hans/*.json  — standard 【】 bracket annotation
    commontalk-fragment-zh.json  — inline annotation from English keys
"""

import json, re, os, sys

TOOLS_DIR = os.path.dirname(os.path.abspath(__file__))
LOCALE_DIR = os.path.join(TOOLS_DIR, '../../Data/Localization')
GLOSSARY_PATH = os.path.join(LOCALE_DIR, 'glossary-approved-zh.json')

# Categories that represent proper nouns eligible for annotation
PROPER_CATEGORIES = {'place', 'character', 'creature', 'item', 'deity', 'faction', 'dungeon', 'race'}

# Composite terms (Chinese → English) for cases where glossary entries combine
COMPOSITE_OVERRIDES = {
    '西斯尊主阿萨吉·芬崔斯': 'Asajj Ventress the Syth Lord',
}

# Commontalk: curated EN-phrase → zh-phrase mappings.
# Each entry: English key substring → (Chinese substring to find, English annotation)
# Covers terms NOT in the glossary, and glossary proper nouns with non-obvious
# Chinese rendering that should still appear in commontalk's natural prose.
COMMONTALK_ANNOTATIONS = {
    # Non-glossary place / character names (not in glossary at all)
    # These are proper nouns that should be annotated so players see the English
    'Mines of Morinia': ('莫瑞尼亚矿坑', 'Mines of Morinia'),
    'Dardin': ('达丁', 'Dardin'),
    'Kazibal': ('卡齐巴尔', 'Kazibal'),
    'Azerok': ('阿泽洛克', 'Azerok'),
    'Poseidon': ('波塞冬', 'Poseidon'),
    'Zuluu': ('祖鲁族', 'Zuluu'),
    'Forgotten Lighthouse': ('被遗忘灯塔', 'Forgotten Lighthouse'),
    'Hidden Valley': ('隐秘谷', 'Hidden Valley'),
    'Village of Whisper': ('私语者村落', 'Village of Whisper'),
    'City of Mistas': ('米斯塔斯城', 'City of Mistas'),
    # Place names with alternative Chinese rendering vs glossary canonical
    'Stonegate': ('石门城堡', 'Stonegate'),
    'Isles of Dread': ('恐惧群岛', 'Isles of Dread'),
    'Umber Veil': ('乌伯维尔', 'Umber Veil'),
    'Castle British': ('不列颠城堡', 'Castle British'),
    'Castle Exodus': ('出埃及城堡', 'Castle Exodus'),
    'Skara Brae': ('斯卡拉·布雷', 'Skara Brae'),
    'Serpent Island': ('巨蛇岛', 'Serpent Island'),
    'Town of Moon': ('月镇', 'Moon'),
    'Village of Grey': ('灰镇', 'Grey'),
    # Dungeons (clearly proper nouns)
    'Dungeon Doom': ('末日地城', 'Doom'),
    'Dungeon Deceit': ('欺诈', 'Deceit'),
    'Dungeon Destard': ('德斯塔德地城', 'Destard'),
    'Dungeon Scorn': ('轻蔑地城', 'Scorn'),
    'Dungeon Bane': ('灾厄地城', 'Bane'),
    'Dungeon Hate': ('憎恨地城', 'Hate'),
    'Dungeon Wrath': ('愤怒地城', 'Wrath'),
    'Dungeon Fire': ('火焰地城', 'Fire'),
    'Dungeon Ankh': ('安卡地城', 'Ankh'),
    'Dungeon Clues': ('线索地城', 'Clues'),
    'Cave of Fire': ('火焰洞窟', "Cave of Fire"),
    'Flooded Temple': ('淹没神殿', "Flooded Temple"),
    'Tomb of the Fallen Wizard': ('陨落法师之墓', "Tomb of the Fallen Wizard"),
    'Dungeon Torment': ('折磨地城', 'Torment'),
    # Characters and people
    'Lord British': ('不列颠领主', 'Lord British'),
    'Mondain': ('蒙代恩', 'Mondain'),
    'Mangar': ('曼加尔', 'Mangar'),
    'Stranger': ('异乡人', 'Stranger'),
    # Places (commontalk uses specific Chinese forms that differ from glossary)
    'Lodoria': ('洛多利亚', 'Lodoria'),
    'Savaged Empire': ('蛮荒帝国', 'Savaged Empire'),
}
# NOTE: Lodor / Sosaria are intentionally excluded — their Chinese forms (洛多尔, 索沙尼亚)
# appear in nearly every commontalk entry and annotating them would clutter the text.
# Dungeon Vile / Dungeon Wicked are excluded because both map to the same 污秽地城.


def load_glossary():
    """Load glossary, filtering to proper noun categories only for annotation."""
    with open(GLOSSARY_PATH, 'r', encoding='utf-8') as f:
        raw = json.load(f)
    data = raw.get("terms", raw)

    # Full mapping (all categories) — for bracket annotation in zh-Hans files
    zh_to_en_full = {}
    en_to_zh_full = {}

    # Proper-noun-only mapping — for inline annotation (avoids false positives)
    zh_to_en_proper = {}
    en_to_zh_proper = {}

    for key, val in data.items():
        if key.startswith('_') or not isinstance(val, dict) or 'canonical' not in val:
            continue
        canonical = val['canonical']
        cat = val.get('category', '')

        # Full map
        if canonical not in zh_to_en_full:
            zh_to_en_full[canonical] = key
        en_to_zh_full[key] = canonical
        for alt in val.get('alternatives', []):
            if alt not in zh_to_en_full:
                zh_to_en_full[alt] = key

        # Proper-only map
        if cat in PROPER_CATEGORIES:
            if canonical not in zh_to_en_proper:
                zh_to_en_proper[canonical] = key
            en_to_zh_proper[key] = canonical
            for alt in val.get('alternatives', []):
                if alt not in zh_to_en_proper:
                    zh_to_en_proper[alt] = key

    # Override with composite terms
    zh_to_en_full.update(COMPOSITE_OVERRIDES)
    zh_to_en_proper.update(COMPOSITE_OVERRIDES)

    return zh_to_en_full, en_to_zh_full, zh_to_en_proper, en_to_zh_proper


def has_cjk(s):
    """Check if string contains CJK characters."""
    return bool(re.search(r'[\u4e00-\u9fff\u3400-\u4dbf]', s))


def is_already_annotated(bracket_text):
    """Check if a 【...】 bracket is already annotated with （English）."""
    return '（' in bracket_text


def annotate_bracketed_file(zh_file, zh_to_en, dry_run=True):
    """Annotate 【】 brackets in a zh-Hans file.

    Only annotates brackets that contain CJK and are NOT already in 【中文（English）】 format.
    """
    zh_path = os.path.join(LOCALE_DIR, 'zh-Hans', zh_file)
    if not os.path.exists(zh_path):
        return 0, [], 0

    with open(zh_path, 'r', encoding='utf-8') as f:
        data = json.load(f)

    total_changed = 0
    warnings = []
    already_count = 0
    ascii_skip_count = 0
    new_data = {}

    for key, val in data.items():
        if not isinstance(val, str):
            new_data[key] = val
            continue

        def replacer(m):
            nonlocal total_changed, warnings, already_count, ascii_skip_count
            zh_term = m.group(1)

            if is_already_annotated(zh_term):
                already_count += 1
                return m.group(0)

            if not has_cjk(zh_term):
                ascii_skip_count += 1
                return m.group(0)

            en_term = zh_to_en.get(zh_term)
            if en_term is None:
                warnings.append(zh_term)
                return m.group(0)

            total_changed += 1
            return f'【{zh_term}（{en_term}）】'

        new_val = re.sub(r'【([^】]+)】', replacer, val)
        new_data[key] = new_val

    if not dry_run and total_changed > 0:
        with open(zh_path, 'w', encoding='utf-8') as f:
            json.dump(new_data, f, ensure_ascii=False, indent=1)
            f.write('\n')

    return total_changed, sorted(set(warnings)), already_count


def annotate_commontalk_zh(dry_run=True):
    """Annotate proper nouns inline in commontalk-fragment-zh.json.

    Uses curated COMMONTALK_ANNOTATIONS only (no glossary auto-matching).
    Each entry matches an English key phrase → annotates the Chinese phrase.
    English phrases use word-boundary matching to avoid false positives.
    """
    ct_path = os.path.join(LOCALE_DIR, 'commontalk-fragment-zh.json')
    if not os.path.exists(ct_path):
        return 0, []

    with open(ct_path, 'r', encoding='utf-8') as f:
        zh_data = json.load(f)

    # Sort by English phrase length descending to match longest first
    ordered = sorted(COMMONTALK_ANNOTATIONS.items(), key=lambda x: len(x[0]), reverse=True)

    total_changed = 0
    warnings = []
    new_data = {}

    for en_key, zh_val in zh_data.items():
        if not isinstance(zh_val, str):
            new_data[en_key] = zh_val
            continue

        result = zh_val

        for en_phrase, (zh_phrase, en_label) in ordered:
            # Word-boundary match on the English key
            pattern = r'(?<![a-zA-Z])' + re.escape(en_phrase) + r'(?![a-zA-Z])'
            if re.search(pattern, en_key, re.IGNORECASE):
                if zh_phrase in result:
                    if f'（{en_label}）' in result:
                        continue
                    result = result.replace(zh_phrase, f'{zh_phrase}（{en_label}）', 1)
                    total_changed += 1

        new_data[en_key] = result

    if not dry_run and total_changed > 0:
        with open(ct_path, 'w', encoding='utf-8') as f:
            json.dump(new_data, f, ensure_ascii=False, indent=1)
            f.write('\n')

    return total_changed, sorted(set(warnings))


def main():
    dry_run = '--dry-run' in sys.argv
    zh_hans_only = '--zh-hans-only' in sys.argv

    print(f"Loading glossary...")
    zh_to_en_full, en_to_zh_full, zh_to_en_proper, en_to_zh_proper = load_glossary()
    print(f"  Full: {len(zh_to_en_full)} zh→en, {len(en_to_zh_full)} en→zh")
    print(f"  Proper-only: {len(zh_to_en_proper)} zh→en, {len(en_to_zh_proper)} en→zh")

    total_changes = 0
    total_warnings = 0

    # === Z-HANS FILES (bracket annotation) ===
    paired = [
        'scripts-books.json',
        'scripts-engines-and-systems.json',
        'scripts-quests.json',
        'scripts-items.json',
        'scripts-mobiles.json',
    ]

    for zh_file in paired:
        count, warnings, already = annotate_bracketed_file(zh_file, zh_to_en_full, dry_run)
        status = "CHANGES" if count > 0 else "up to date"
        print(f"\n{zh_file}: {count} annotated, {already} already annotated — {status}")
        total_changes += count
        if warnings:
            total_warnings += len(warnings)
            print(f"  Not in glossary ({len(warnings)}):")
            for w in warnings[:15]:
                print(f"    【{w}】")
            if len(warnings) > 15:
                print(f"    ... and {len(warnings) - 15} more")

    # === EXTRA ZH-HANS FILES ===
    extra = [
        'thewar-quest.json',
        'stats-gump.json',
        'race-system.json',
        'shard-greeter.json',
        'temptation-gump.json',
        'resource-harvest-extra.json',
        'scripts-utilities.json',
        'system.json',
    ]

    for f in extra:
        c, w, already = annotate_bracketed_file(f, zh_to_en_full, dry_run)
        if c or w:
            print(f"\n{f}: {c} annotations, {already} already annotated")
            if w:
                for ww in w[:5]:
                    print(f"  ⚠ {ww}")

    # === COMMONTALK ===
    if not zh_hans_only:
        ct_count, ct_warnings = annotate_commontalk_zh(dry_run)
        print(f"\ncommontalk-fragment-zh.json: {ct_count} inline annotations")
        if ct_warnings:
            for w in ct_warnings[:10]:
                print(f"  ⚠ {w}")
    else:
        print(f"\ncommontalk-fragment-zh.json: skipped (--zh-hans-only)")

    print(f"\n{'=' * 60}")
    print(f"Total new annotations: {total_changes}")
    print(f"Total warnings: {total_warnings}")
    if dry_run:
        print("DRY RUN — no files written. Run without --dry-run to apply.")
    else:
        print("All files updated successfully.")
        print("Next steps:")
        print("  1. python3 World/Source/Tools/sync_localization_glossary.py")
        print("  2. python3 World/Source/Tools/sync_localization_glossary.py --check")


if __name__ == '__main__':
    main()

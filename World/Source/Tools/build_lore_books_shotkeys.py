#!/usr/bin/env python3
"""Build en/zh-Hans lore-books.json shotkeys from LoreBook.cs + existing scripts-books.json.

Reuses existing hash-key Chinese. Completes the two GetRandomCity() templates only
where the extractor stored a prefix fragment (no new full-book translation).
"""
from __future__ import annotations

import hashlib
import json
import os
import re

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
LORE_CS = os.path.join(ROOT, "Source", "Scripts", "Items", "Books", "LoreBook.cs")
EN_BOOKS = os.path.join(ROOT, "Data", "Localization", "en", "scripts-books.json")
ZH_BOOKS = os.path.join(ROOT, "Data", "Localization", "zh-Hans", "scripts-books.json")
GLOSSARY = os.path.join(ROOT, "Data", "Localization", "glossary-approved-zh.json")
OUT_EN = os.path.join(ROOT, "Data", "Localization", "en", "lore-books.json")
OUT_ZH = os.path.join(ROOT, "Data", "Localization", "zh-Hans", "lore-books.json")

TITLE_TO_SLUG = {
    "Akalabeth's Tale": "akalabeths_tale",
    "The Lost Land": "lost_land",
    "The Balance Vol I of II": "balance_vol1",
    "The Balance Vol II of II": "balance_vol2",
    "The Black Gate Demon": "black_gate_demon",
    "The Blue Ore": "blue_ore",
    "Crystal Flasks": "crystal_flasks",
    "The Curse of the Island": "curse_of_the_island",
    "The Dark Age": "dark_age",
    "The Dark Core": "dark_core",
    "Death to Pirates": "death_to_pirates",
    "The Death Knights": "death_knights",
    "The Darkness Within": "darkness_within",
    "The Destruction of Exodus": "destruction_of_exodus",
    "The Knight Who Fell": "knight_who_fell",
    "The Fall of Mondain": "fall_of_mondain",
    "Forging the Fire": "forging_the_fire",
    "Forgotten Dungeons": "forgotten_dungeons",
    "The Cruel Game": "cruel_game",
    "The Ice Queen": "ice_queen",
    "Luck of the Rogue": "luck_of_the_rogue",
    "A Tattered Journal": "tattered_journal",
    "The Curse of Mangar": "curse_of_mangar",
    "The Times of Minax": "times_of_minax",
    "Rangers of Lodoria": "rangers_of_lodoria",
    "Gem of Immortality": "gem_of_immortality",
    "The Gods of Men": "gods_of_men",
    "Castles Above": "castles_above",
    "Staff of Five Parts": "staff_of_five_parts",
    "The Story of Exodus": "story_of_exodus",
    "The Story of Minax": "story_of_minax",
    "The Story of Mondain": "story_of_mondain",
    "The Bard's Tale": "bards_tale",
    "Death Dealing": "death_dealing",
    "The Orb of the Abyss": "orb_of_the_abyss",
    "The Underworld Gate": "underworld_gate",
    "The Elemental Titans": "elemental_titans",
    "The Dragon's Egg": "dragons_egg",
    "Magic in the Moon": "magic_in_the_moon",
    "The Maze of Wonder": "maze_of_wonder",
    "The Pass of the Gods": "pass_of_the_gods",
    "Valley of Corruption": "valley_of_corruption",
    "The Demon Shard": "demon_shard",
    "The Syth Order": "syth_order",
    "The Rule of One": "rule_of_one",
    "Antiquities": "antiquities",
    "The Jedi Order": "jedi_order",
}

CITIES = [
    "Britain",
    "Fawn",
    "Grey",
    "Moon",
    "Yew",
    "Montor",
    "Umbra",
    "Devil Guard",
    "Death Gulch",
    "Renika",
    "Glacial Hills",
    "Springvale",
    "Elidor",
    "Islegem",
    "the Port of Dusk",
    "the Port of Starguide",
    "Portshine",
    "Greensky Village",
    "the City of Lodoria",
    "the Cimmeran Hold",
    "the Village of Barako",
    "the Village of Kurak",
    "Kuldara",
]

# Completions for extractor prefix fragments only (not a full re-translation).
ORB_ZH_SUFFIX = (
    "{0}的贤者所言不虚，则恶魔真名为「Lucifer」，"
    "我须在找到那扇地狱之门时高喊此名。我须先去{1}，盼能在那里寻得盟友，以赴即将到来的战斗。"
)
UNDERWORLD_ZH_SUFFIX = (
    "{0}，但也有传闻说他被带回妻子的故乡斯卡拉·布雷（Skara Brae）。"
    "若裂幕者（Slayer）之死未能使他的尸身复活游荡世间，便只需找到他的墓穴取回头颅。"
    "将头颅献于那宏伟的符文之门，或可破除法术、打开通往冥界深渊（Underworld）的封印。"
    "然而须当心冥界深渊（Underworld）：古籍记载，行于深渊之中时巫术会受阻。"
)


def shotkey_hash(text: str) -> str:
    h = hashlib.sha256(text.encode("utf-8")).digest()
    return "s." + "".join(f"{b:02x}" for b in h[:8])


def csharp_unescape(s: str) -> str:
    return (
        s.replace("\\\\", "\x00")
        .replace("\\n", "\n")
        .replace("\\r", "\r")
        .replace("\\t", "\t")
        .replace('\\"', '"')
        .replace("\\'", "'")
        .replace("\x00", "\\")
    )


def load_json(path: str) -> dict:
    with open(path, encoding="utf-8") as f:
        return json.load(f)


def glossary_zh(glossary: dict, english: str) -> str:
    terms = glossary.get("terms", glossary)
    entry = terms.get(english) if isinstance(terms, dict) else None
    if isinstance(entry, dict) and entry.get("canonical"):
        return f"{entry['canonical']}（{english}）"
    return english


def city_slug(english: str) -> str:
    return (
        english.replace("the ", "")
        .replace(" ", "_")
        .replace("'", "")
        .lower()
    )


def parse_writebook(src: str) -> dict:
    out = {}
    for m in re.finditer(
        r'case\s+\d+:\s*BookTitle\s*=\s*"([^"]+)";\s*BookAuthor\s*=\s*"([^"]+)";',
        src,
    ):
        out[m.group(1)] = m.group(2)
    return out


def parse_bodies(src: str) -> dict:
    bodies = {}
    for m in re.finditer(
        r'book\.BookTitle\s*==\s*"([^"]+)"\s*\)\{\s*book\.BookText\s*=\s*(.+?);\s*\}',
        src,
        flags=re.S,
    ):
        title, expr = m.group(1), m.group(2)
        parts = []
        idx = 0
        city_n = 0
        for piece in re.finditer(
            r'"((?:\\.|[^"\\])*)"|RandomThings\.GetRandomCity\(\)',
            expr,
        ):
            if piece.group(0).startswith("RandomThings"):
                parts.append("{" + str(city_n) + "}")
                city_n += 1
            else:
                parts.append(csharp_unescape(piece.group(1)))
            idx += 1
        bodies[title] = "".join(parts)
    return bodies


def lookup_zh(en_map: dict, zh_map: dict, english: str) -> str | None:
    key = shotkey_hash(english)
    zh = zh_map.get(key)
    if zh and zh != english:
        return zh
    # fallback: scan en values
    for k, v in en_map.items():
        if v == english:
            z = zh_map.get(k)
            if z and z != english:
                return z
    return None


def main() -> None:
    src = open(LORE_CS, encoding="utf-8").read()
    en_map = load_json(EN_BOOKS)
    zh_map = load_json(ZH_BOOKS)
    glossary = load_json(GLOSSARY)
    prev_en = load_json(OUT_EN) if os.path.isfile(OUT_EN) else {}
    prev_zh = load_json(OUT_ZH) if os.path.isfile(OUT_ZH) else {}

    titles_authors = parse_writebook(src)
    bodies = parse_bodies(src)

    missing = []
    en_out = {}
    zh_out = {}

    for title, slug in TITLE_TO_SLUG.items():
        author = titles_authors.get(title) or prev_en.get(f"lore.book.{slug}.author")
        body = bodies.get(title) or prev_en.get(f"lore.book.{slug}.body")
        if not author or body is None:
            missing.append(f"parse fail: {title}")
            continue

        t_key, a_key, b_key = (
            f"lore.book.{slug}.title",
            f"lore.book.{slug}.author",
            f"lore.book.{slug}.body",
        )
        en_out[t_key] = title
        en_out[a_key] = author
        en_out[b_key] = body

        zh_title = prev_zh.get(t_key) or lookup_zh(en_map, zh_map, title)
        zh_author = prev_zh.get(a_key) or lookup_zh(en_map, zh_map, author)
        if not zh_title:
            missing.append(f"zh title: {title}")
            zh_title = title
        if not zh_author:
            missing.append(f"zh author: {author} ({title})")
            zh_author = author
        zh_out[t_key] = zh_title
        zh_out[a_key] = zh_author

        if "{0}" not in body:
            zh_body = prev_zh.get(b_key) or lookup_zh(en_map, zh_map, body)
            if not zh_body:
                missing.append(f"zh body: {title}")
                zh_body = body
            zh_out[b_key] = zh_body
            continue

        if prev_zh.get(b_key) and "{0}" in prev_zh[b_key]:
            zh_out[b_key] = prev_zh[b_key]
            continue

        # Concatenated books: reuse prefix fragment + complete suffix.
        prefix = body.split("{0}", 1)[0]
        zh_prefix = lookup_zh(en_map, zh_map, prefix)
        if title == "The Orb of the Abyss":
            zh_out[b_key] = (zh_prefix or "如果那位") + ORB_ZH_SUFFIX
        elif title == "The Underworld Gate":
            zh_out[b_key] = (zh_prefix or "有人认为他就安葬在") + UNDERWORLD_ZH_SUFFIX
        else:
            missing.append(f"unexpected template: {title}")
            zh_out[b_key] = body

    for city in CITIES:
        key = f"lore.book.city.{city_slug(city)}"
        en_out[key] = city
        zh_out[key] = glossary_zh(glossary, city)

    def dump(path, data):
        with open(path, "w", encoding="utf-8") as f:
            json.dump(data, f, ensure_ascii=False, indent=2)
            f.write("\n")

    dump(OUT_EN, en_out)
    dump(OUT_ZH, zh_out)
    print(f"wrote {len(en_out)} keys -> {OUT_EN}")
    print(f"wrote {len(zh_out)} keys -> {OUT_ZH}")
    if missing:
        print("warnings:")
        for row in missing:
            print(" ", row)


if __name__ == "__main__":
    main()

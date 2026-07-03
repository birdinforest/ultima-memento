#!/usr/bin/env python3
"""Extract Research spell/scroll tables from ResearchFunctions.cs into locale JSON."""
from __future__ import annotations

import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
CS = ROOT / "Source/Scripts/Engines and Systems/Magic/Research/ResearchFunctions.cs"
OUT_EN = ROOT / "Data/Localization/en/research-spell-catalog.json"
OUT_ZH = ROOT / "Data/Localization/zh-Hans/research-spell-catalog.json"
ZH_SOURCE = Path(__file__).resolve().parent / "data" / "research_spell_catalog_zh.json"

REAGENT_FALLBACK = {
    "Bat Wing": ("research.reagent.bat_wing", "Bat Wing"),
    "Blood Moss": ("research.reagent.blood_moss", "Blood Moss"),
    "Bloodmoss": ("research.reagent.bloodmoss", "Bloodmoss"),
    "Daemon Blood": ("research.reagent.daemon_blood", "Daemon Blood"),
    "Garlic": ("research.reagent.garlic", "Garlic"),
    "Ginseng": ("research.reagent.ginseng", "Ginseng"),
    "Grave Dust": ("research.reagent.grave_dust", "Grave Dust"),
    "Mandrake Root": ("research.reagent.mandrake_root", "Mandrake Root"),
    "Nightshade": ("research.reagent.nightshade", "Nightshade"),
    "Nox Crystal": ("research.reagent.nox_crystal", "Nox Crystal"),
    "Pig Iron": ("research.reagent.pig_iron", "Pig Iron"),
    "Spiders Silk": ("research.reagent.spiders_silk", "Spiders Silk"),
    "Sulfurous Ash": ("research.reagent.sulfurous_ash", "Sulfurous Ash"),
    "Silver Serpent Venom": ("research.reagent.silver_serpent_venom", "Silver Serpent Venom"),
    "Dragon Blood": ("research.reagent.dragon_blood", "Dragon Blood"),
    "Enchanted Seaweed": ("research.reagent.enchanted_seaweed", "Enchanted Seaweed"),
    "Dragon Tooth": ("research.reagent.dragon_tooth", "Dragon Tooth"),
    "Golden Serpent Venom": ("research.reagent.golden_serpent_venom", "Golden Serpent Venom"),
    "Lich Dust": ("research.reagent.lich_dust", "Lich Dust"),
    "Demon Claw": ("research.reagent.demon_claw", "Demon Claw"),
    "Pegasus Feather": ("research.reagent.pegasus_feather", "Pegasus Feather"),
    "Phoenix Feather": ("research.reagent.phoenix_feather", "Phoenix Feather"),
    "Unicorn Horn": ("research.reagent.unicorn_horn", "Unicorn Horn"),
    "Demigod Blood": ("research.reagent.demigod_blood", "Demigod Blood"),
    "Ghostly Dust": ("research.reagent.ghostly_dust", "Ghostly Dust"),
}

RUNES = [
    ("An", "an"), ("Bet", "bet"), ("Corp", "corp"), ("Des", "des"), ("Ex", "ex"),
    ("Flam", "flam"), ("Grav", "grav"), ("Hur", "hur"), ("In", "in"), ("Jux", "jux"),
    ("Kal", "kal"), ("Lor", "lor"), ("Mani", "mani"), ("Nox", "nox"), ("Ort", "ort"),
    ("Por", "por"), ("Quas", "quas"), ("Rel", "rel"), ("Sanct", "sanct"), ("Tym", "tym"),
    ("Uus", "uus"), ("Vas", "vas"), ("Wis", "wis"), ("Xen", "xen"), ("Ylem", "ylem"), ("Zu", "zu"),
]


def extract_index_blocks(text: str, start_marker: str, end_marker: str) -> dict[int, str]:
    start = text.index(start_marker)
    end = text.index(end_marker, start)
    block = text[start:end]
    slice_gate = block.find("if ( slice == 1 )")
    if slice_gate > 0:
        block = block[:slice_gate]
    blocks: dict[int, str] = {}
    for m in re.finditer(r"(?:if|else if) \( index == (\d+) \)\{", block):
        idx = int(m.group(1))
        body_start = m.end()
        depth = 1
        i = body_start
        while i < len(block) and depth:
            if block[i] == "{":
                depth += 1
            elif block[i] == "}":
                depth -= 1
            i += 1
        body = block[body_start : i - 1]
        if "name =" in body:
            blocks[idx] = body
    return blocks


def field(body: str, name: str) -> str:
    m = re.search(rf'{name} = "((?:\\.|[^"\\])*)"', body)
    return m.group(1) if m else ""


def main() -> int:
    text = CS.read_text(encoding="utf-8")
    ancient_blocks = extract_index_blocks(
        text, "public static string SpellInformation", "public static string ScrollInformation"
    )
    scroll_blocks = extract_index_blocks(
        text, "public static string ScrollInformation", "public static void GiveScroll"
    )

    en: dict[str, str] = {}
    zh: dict[str, str] = {}
    zh_extra: dict[str, str] = {}
    if ZH_SOURCE.is_file():
        zh_extra = json.loads(ZH_SOURCE.read_text(encoding="utf-8"))

    for i in range(1, 27):
        upper, lower = RUNES[i - 1]
        en[f"research.rune.{i:02d}.upper"] = upper
        en[f"research.rune.{i:02d}.lower"] = lower
        zh[f"research.rune.{i:02d}.upper"] = upper
        zh[f"research.rune.{i:02d}.lower"] = lower

    for _label, (key, en_label) in REAGENT_FALLBACK.items():
        en[key] = en_label
        zh[key] = zh_extra.get(key, en_label)

    for idx in sorted(ancient_blocks):
        body = ancient_blocks[idx]
        name = field(body, "name")
        desc = field(body, "description")
        en[f"research.ancient.{idx:03d}.name"] = name
        en[f"research.ancient.{idx:03d}.description"] = desc
        zh[f"research.ancient.{idx:03d}.name"] = zh_extra.get(
            f"research.ancient.{idx:03d}.name", name
        )
        zh[f"research.ancient.{idx:03d}.description"] = zh_extra.get(
            f"research.ancient.{idx:03d}.description", desc
        )

    for idx in sorted(scroll_blocks):
        body = scroll_blocks[idx]
        name = field(body, "name")
        en[f"research.scroll.{idx:03d}.name"] = name
        zh[f"research.scroll.{idx:03d}.name"] = zh_extra.get(
            f"research.scroll.{idx:03d}.name", name
        )

    OUT_EN.parent.mkdir(parents=True, exist_ok=True)
    OUT_EN.write_text(json.dumps(en, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    OUT_ZH.write_text(json.dumps(zh, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(
        f"ancient={len(ancient_blocks)} scroll={len(scroll_blocks)} "
        f"keys_en={len(en)} zh_overrides={len(zh_extra)} -> {OUT_EN.name}"
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

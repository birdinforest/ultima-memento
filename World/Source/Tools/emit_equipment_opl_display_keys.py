#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Emit Item.DisplayNameLocalizationKey overrides + equipment-properties.json
entries for standard weapons, armor, clothing, instruments, and magical artifacts.

Usage (from repo root):
  python3 World/Source/Tools/emit_equipment_opl_display_keys.py --apply

Without --apply: dry-run summary only.
"""
from __future__ import annotations

import argparse
import json
import re
from pathlib import Path

REPO = Path(__file__).resolve().parents[3]
EN_JSON = REPO / "World/Data/Localization/en/equipment-properties.json"
ZH_JSON = REPO / "World/Data/Localization/zh-Hans/equipment-properties.json"

SKIP_FILE_PREFIXES = (
    "Base",
    "Slayer",
    "WeaponEnums",
    "WeaponAbility",
    "HitLower",
    "IArmor",
)

SKIP_NAMES = frozenset(
    {
        "SlayerGroup",
        "SlayerEntry",
        "SlayerName",
    }
)

# Longest multi-word suffixes first (match "thigh boots" before "boots")
SUFFIX_ZH: list[tuple[str, str]] = [
    ("thigh boots", "长筒靴"),
    ("shinobi robe", "忍袍"),
    ("shinobi hood", "忍帽"),
    ("shinobi mask", "忍面"),
    ("shinobi cowl", "忍巾"),
    ("horse barding", "马铠"),
    ("bearskin cap", "熊皮帽"),
    ("deerskin cap", "鹿皮帽"),
    ("stagskin cap", "鹿皮帽"),
    ("wolfskin cap", "狼皮帽"),
    ("hiking boots", "登山靴"),
    ("soft leather boots", "软皮靴"),
    ("studded hide tunic", "镶钉粗皮上衣"),
    ("chainmail tunic", "锁甲上衣"),
    ("studded skirt", "镶钉裙甲"),
    ("leather thigh boots", "皮质长筒靴"),
    ("platemail arms", "板甲臂甲"),
    ("platemail legs", "板甲腿甲"),
    ("platemail skirt", "板甲裙甲"),
    ("chainmail leggings", "锁甲护腿"),
    ("chainmail skirt", "锁甲裙甲"),
    ("chainmail coif", "锁甲头罩"),
    ("scalemail arms", "鳞甲臂甲"),
    ("scalemail gloves", "鳞甲手套"),
    ("scalemail helm", "鳞甲头盔"),
    ("scalemail leggings", "鳞甲护腿"),
    ("scalemail shield", "鳞甲盾"),
    ("scalemail tunic", "鳞甲上衣"),
    ("scaled chest", "鳞胸甲"),
    ("scaled arms", "鳞臂甲"),
    ("scaled gloves", "鳞手套"),
    ("scaled gorget", "鳞护颈"),
    ("scaled helm", "鳞头盔"),
    ("scaled legs", "鳞腿甲"),
    ("scaled shield", "鳞盾"),
    ("hide tunic", "皮上衣"),
    ("tunic", "上衣"),
    ("robe", "长袍"),
    ("cloak", "斗篷"),
    ("mantle", "披肩"),
    ("skirt", "裙甲"),
    ("boots", "靴子"),
    ("shoes", "鞋"),
    ("sandals", "凉鞋"),
    ("gloves", "手套"),
    ("gauntlets", "护手"),
    ("greaves", "护胫"),
    ("leggings", "护腿"),
    ("legs", "腿甲"),
    ("arms", "臂甲"),
    ("sleeves", "袖甲"),
    ("bracers", "护腕"),
    ("helm", "头盔"),
    ("coif", "头罩"),
    ("hood", "兜帽"),
    ("mask", "面罩"),
    ("cowl", "风帽"),
    ("gorget", "护颈"),
    ("shield", "盾牌"),
    ("cap", "帽子"),
    ("barding", "马铠"),
]

PREFIX_TOKEN_ZH: dict[str, str] = {
    "chainmail": "锁甲",
    "platemail": "板甲",
    "plate": "板甲",
    "leather": "皮",
    "studded": "镶钉",
    "hide": "粗皮",
    "bone": "骨",
    "drakbone": "龙骸",
    "skeletal": "骸骨",
    "scaled": "覆鳞",
    "scalemail": "鳞甲",
    "wooden": "木制",
    "royal": "王室",
    "champion": "勇士",
    "crested": "纹章",
    "dark": "暗黑",
    "elven": "精灵",
    "guardsman": "卫兵",
    "jeweled": "宝石",
    "large": "大型",
    "metal": "金属",
    "norse": "诺斯",
    "oniwaban": "鬼番",
    "ringmail": "环甲",
    "scaly": "鳞皮",
    "sun": "圣阳",
    "virtue": "美德",
    "dread": "恐惧",
    "horned": "角饰",
    "bascinet": "尖顶盔",
    "soft": "软",
    "heavy": "重型",
    "light": "轻型",
    "war": "战",
    "short": "短",
    "long": "长",
    "two": "双",
    "handed": "手",
    "hand": "手",
}

# Weapon / instrument one-word or leftover tokens
EXTRA_TOKEN_ZH: dict[str, str] = {
    "sword": "剑",
    "longsword": "长剑",
    "shortsword": "短剑",
    "broadsword": "阔剑",
    "cutlass": "弯刀",
    "scimitar": "弯刀",
    "katana": "武士刀",
    "claymore": "双手大剑",
    "rapier": "细剑",
    "kryss": "刺剑",
    "lance": "长枪",
    "spear": "矛",
    "halberd": "戟",
    "bardiche": "长柄刀",
    "scythe": "镰刀",
    "axe": "斧",
    "hatchet": "手斧",
    "pick": "镐",
    "hammer": "锤",
    "mace": "钉头锤",
    "club": "棍棒",
    "staff": "法杖",
    "bow": "弓",
    "crossbow": "弩",
    "clubbed": "棒",
    "cleaver": "菜刀",
    "dagger": "匕首",
    "knife": "匕首",
    "fork": "叉",
    "pike": "长矛",
    "pitchfork": "草叉",
    "whip": "鞭",
    "scepter": "权杖",
    "wand": "魔杖",
    "lute": "鲁特琴",
    "harp": "竖琴",
    "drum": "鼓",
    "tambourine": "铃鼓",
    "flute": "长笛",
    "harpolyre": "竖琴",
    "lutelean": "鲁特琴",
    "fists": "徒手",
}

RE_CLASS = re.compile(
    r"^\s*public\s+(?:sealed\s+)?class\s+(\w+)\b", re.MULTILINE
)
RE_NAME = re.compile(r'Name\s*=\s*"([^"]*)"')
RE_ABSTRACT = re.compile(r"public\s+abstract\s+class\s+\w+")


def title_en(s: str) -> str:
    return " ".join(w.capitalize() for w in s.strip().split())


def zh_from_equipment_name(en_lower: str) -> str:
    """Best-effort zh-Hans for armor/weapon-style English names."""
    s = en_lower.strip().lower()
    if not s:
        return ""
    words = s.split()
    for sl in range(min(4, len(words)), 0, -1):
        suf = " ".join(words[-sl:])
        for sk, sv in SUFFIX_ZH:
            if sk == suf:
                pref = " ".join(words[:-sl]).strip()
                if not pref:
                    return sv
                zh_pref_parts: list[str] = []
                for pw in pref.split():
                    zh_pref_parts.append(
                        PREFIX_TOKEN_ZH.get(pw, EXTRA_TOKEN_ZH.get(pw, ""))
                    )
                zh_pref = "".join(zh_pref_parts)
                if not zh_pref:
                    zh_pref = "".join(title_en(pref).split())
                return zh_pref + sv
    # Fallback: token-wise composition
    zh_parts: list[str] = []
    for w in words:
        zh_parts.append(
            PREFIX_TOKEN_ZH.get(w)
            or EXTRA_TOKEN_ZH.get(w)
            or ""
        )
    if all(zh_parts):
        return "".join(zh_parts)
    # Proper-noun style fallback (artifact / exotic)
    t = title_en(s)
    return f"{t}（{t}）"


def artifact_slug(class_name: str) -> str:
    n = class_name.lower()
    if n.startswith("artifact_"):
        n = n[len("artifact_") :]
    return n.replace("_", ".")


def category_for_path(p: Path) -> str | None:
    parts = p.parts
    s = str(p)
    if "/Items/Weapons/" in s:
        return "weapon"
    if "/Items/Armor/" in s:
        return "armor"
    if "/Items/Clothing/" in s:
        return "clothing"
    if "/Items/Instruments/" in s:
        return "instrument"
    if "/Items/Magical/Artifacts/" in s:
        return "artifact"
    return None


def localization_key(category: str, class_name: str) -> str:
    c = class_name.lower()
    if category == "artifact":
        return f"item.magical.artifact.{artifact_slug(class_name)}"
    return f"item.equip.{category}.{c}"


def extract_class_name(text: str) -> str | None:
    m = RE_CLASS.search(text)
    return m.group(1) if m else None


def extract_first_name(text: str) -> str | None:
    m = RE_NAME.search(text)
    return m.group(1) if m else None


def english_display(class_name: str, name_literal: str | None) -> str:
    if name_literal and name_literal.strip():
        return title_en(name_literal.strip())
    return title_en(re.sub(r"(?=[A-Z])", " ", class_name).strip())


def chinese_display(
    category: str, class_name: str, name_literal: str | None, en: str
) -> str:
    if name_literal and name_literal.strip():
        zh = zh_from_equipment_name(name_literal.strip())
        if zh:
            return zh
    if category == "artifact":
        t = en.strip()
        if "（" in t:
            return t
        return f"{t}（{t}）"
    zh = zh_from_equipment_name(en.lower())
    return zh or f"{en}（{en}）"


def should_skip_file(path: Path) -> bool:
    if path.name.startswith(SKIP_FILE_PREFIXES):
        return True
    # infrastructure / enums
    if path.name in ("SlayerGroup.cs", "SlayerEntry.cs", "SlayerName.cs"):
        return True
    return False


def inject_property(text: str, key: str) -> tuple[str, bool]:
    if "DisplayNameLocalizationKey" in text:
        return text, False
    if RE_ABSTRACT.search(text):
        return text, False
    m = RE_CLASS.search(text)
    if not m:
        return text, False
    # Insert immediately after class opening brace line
    brace_idx = text.find("{", m.end())
    if brace_idx < 0:
        return text, False
    insert_at = brace_idx + 1
    indent = "\n\t\t"
    prop = f'{indent}public override string DisplayNameLocalizationKey => "{key}";'
    new_text = text[:insert_at] + prop + text[insert_at:]
    return new_text, True


def collect_files(roots: list[str]) -> list[Path]:
    out: list[Path] = []
    for r in roots:
        root = REPO / r
        for p in sorted(root.rglob("*.cs")):
            if should_skip_file(p):
                continue
            t = p.read_text(encoding="utf-8", errors="replace")
            cn = extract_class_name(t)
            if not cn or cn in SKIP_NAMES:
                continue
            if RE_ABSTRACT.search(t):
                continue
            out.append(p)
    return out


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument(
        "--apply",
        action="store_true",
        help="Write JSON patches and edit .cs files",
    )
    args = ap.parse_args()

    roots = [
        "World/Source/Scripts/Items/Weapons",
        "World/Source/Scripts/Items/Armor",
        "World/Source/Scripts/Items/Clothing",
        "World/Source/Scripts/Items/Instruments",
        "World/Source/Scripts/Items/Magical/Artifacts",
    ]
    files = collect_files(roots)
    entries: dict[str, tuple[str, str, str]] = {}
    stats = {"files": 0, "skipped_has_key": 0, "patched_cs": 0}

    for p in files:
        cat = category_for_path(p)
        if not cat:
            continue
        text = p.read_text(encoding="utf-8", errors="replace")
        if "DisplayNameLocalizationKey" in text:
            stats["skipped_has_key"] += 1
            continue
        cn = extract_class_name(text)
        if not cn:
            continue
        key = localization_key(cat, cn)
        nm = extract_first_name(text)
        en = english_display(cn, nm)
        zh = chinese_display(cat, cn, nm, en)
        entries[key] = (en, zh, str(p.relative_to(REPO)))
        stats["files"] += 1

        if args.apply:
            new_text, ok = inject_property(text, key)
            if ok:
                p.write_text(new_text, encoding="utf-8")
                stats["patched_cs"] += 1

    print(
        f"candidate classes: {stats['files']}, skipped(already key): {stats['skipped_has_key']}, keys: {len(entries)}"
    )
    if args.apply:
        print(f"patched .cs files: {stats['patched_cs']}")

    if not args.apply:
        print("Dry run only. Pass --apply to write.")
        return 0

    en_map = json.loads(EN_JSON.read_text(encoding="utf-8"))
    zh_map = json.loads(ZH_JSON.read_text(encoding="utf-8"))
    added = 0
    for k, (en, zh, _src) in sorted(entries.items()):
        if k in en_map and en_map[k] != en:
            print("WARN key collision EN mismatch:", k)
        if k not in en_map:
            en_map[k] = en
            zh_map[k] = zh
            added += 1
        elif k not in zh_map:
            zh_map[k] = zh
            added += 1

    EN_JSON.write_text(
        json.dumps(en_map, ensure_ascii=False, indent=2) + "\n", encoding="utf-8"
    )
    ZH_JSON.write_text(
        json.dumps(zh_map, ensure_ascii=False, indent=2) + "\n", encoding="utf-8"
    )
    print(f"JSON entries added/merged: {added}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

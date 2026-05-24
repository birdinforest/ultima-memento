#!/usr/bin/env python3
"""
read_character_stats_from_backup.py

从离线 Saves 备份读取指定角色的基础属性（Str / Int / Dex）与全部技能 Base 值（不含装备加成），
并根据**已装备物品**重建 StatMod / SkillMod（与登录后 Deserialize 时一致）。

Note: StatMod / SkillMod 列表本身不写入 Mobile 存档；本工具从 Items.bin 中 parent=角色的
装备层物品解析 AOS 属性 / 技能加成，模拟运行时 mod 名（0x{serial}Str|Dex|Int）。

Usage:
    python3 World/Source/Tools/read_character_stats_from_backup.py \
        --backup-path /path/to/Saves \
        --account AccountName \
        --character CharacterName \
        [--output stats.json] \
        [--all-skills]   # 默认只输出 base > 0 的技能；加此开关输出全部 58 项
        [--skip-mods]    # 不扫描 Items.bin（仅 Raw 属性 + 技能 Base）
"""

from __future__ import annotations

import argparse
import json
import os
import struct
import sys
import xml.etree.ElementTree as ET
from typing import Any, Dict, List, Optional, Tuple

# Load analyze_character_backup from the same Tools directory (shared item parsers).
_TOOLS_DIR = os.path.dirname(os.path.abspath(__file__))
if _TOOLS_DIR not in sys.path:
    sys.path.insert(0, _TOOLS_DIR)
import analyze_character_backup as acb  # noqa: E402

# ---------------------------------------------------------------------------
# SkillInfo.Table names (Skills.cs m_Table, 58 entries)
# ---------------------------------------------------------------------------

SKILL_NAMES: List[str] = [
    "Alchemy", "Anatomy", "Druidism", "Mercantile", "Arms Lore", "Parrying", "Begging",
    "Blacksmithy", "Bowcrafting", "Peacemaking", "Camping", "Carpentry", "Cartography",
    "Cooking", "Searching", "Discordance", "Psychology", "Healing", "Seafaring",
    "Forensics", "Herding", "Hiding", "Provocation", "Inscription", "Lockpicking",
    "Magery", "Magic Resistance", "Tactics", "Snooping", "Musicianship", "Poisoning",
    "Marksmanship", "Spiritualism", "Stealing", "Tailoring", "Taming", "Tasting",
    "Tinkering", "Tracking", "Veterinary", "Swordsmanship", "Bludgeoning", "Fencing",
    "Fist Fighting", "Lumberjacking", "Mining", "Meditation", "Stealth", "Remove Trap",
    "Necromancy", "Focus", "Knightship", "Bushido", "Ninjitsu", "Elementalism",
    "Mysticism", "Imbuing", "Throwing",
]

# ---------------------------------------------------------------------------
# BinaryReader (matches .NET BinaryWriter / BinaryFileReader)
# ---------------------------------------------------------------------------

class BinaryReader:
    def __init__(self, data: bytes, pos: int = 0):
        self._data = data
        self._pos = pos

    @property
    def pos(self) -> int:
        return self._pos

    def read_bytes(self, n: int) -> bytes:
        if self._pos + n > len(self._data):
            raise EOFError(f"need {n} bytes at {self._pos}, have {len(self._data) - self._pos}")
        out = self._data[self._pos : self._pos + n]
        self._pos += n
        return out

    def read_bool(self) -> bool:
        return self.read_bytes(1)[0] != 0

    def read_byte(self) -> int:
        return self.read_bytes(1)[0]

    def read_int32(self) -> int:
        return struct.unpack("<i", self.read_bytes(4))[0]

    def read_int64(self) -> int:
        return struct.unpack("<q", self.read_bytes(8))[0]

    def read_uint16(self) -> int:
        return struct.unpack("<H", self.read_bytes(2))[0]

    def read_encoded_int(self) -> int:
        result = 0
        shift = 0
        while True:
            b = self.read_byte()
            result |= (b & 0x7F) << shift
            if (b & 0x80) == 0:
                break
            shift += 7
        return result

    def read_prefixed_shard_string(self) -> Optional[str]:
        """BinaryFileReader.ReadString(): 0x00 = null, else 7-bit len + UTF-8."""
        if self.read_byte() == 0:
            return None
        length = self.read_encoded_int()
        if length <= 0:
            return ""
        return self.read_bytes(length).decode("utf-8", errors="replace")

    def read_delta_time(self) -> None:
        self.read_int64()

    def read_date_time(self) -> None:
        self.read_int64()

    def read_point3d(self) -> None:
        self.read_int32()
        self.read_int32()
        self.read_int32()

    def read_map(self) -> None:
        self.read_byte()

    def read_item_ref(self) -> None:
        self.read_int32()

    def read_mobile_ref(self) -> None:
        self.read_int32()

    def read_guild_ref(self) -> None:
        self.read_int32()

    def read_race(self) -> None:
        self.read_byte()

    def read_strong_mobile_list(self) -> None:
        count = self.read_int32()
        for _ in range(count):
            self.read_mobile_ref()

    def read_strong_item_list(self) -> None:
        count = self.read_int32()
        for _ in range(count):
            self.read_item_ref()

    def skip_hair_info(self) -> None:
        ver = self.read_int32()
        if ver == 0:
            self.read_int32()
            self.read_int32()

    def skip_virtue_info(self) -> None:
        ver = self.read_byte()
        if ver in (0, 1):
            mask = self.read_byte()
            for i in range(8):
                if mask & (1 << i):
                    self.read_int32()

    def skip_poison(self) -> None:
        tag = self.read_byte()
        if tag == 1:
            self.read_byte()
        elif tag == 2:
            self.read_int32()
            self.read_bytes(8)  # double
            self.read_int32()
            self.read_int64()   # TimeSpan


# ---------------------------------------------------------------------------
# Mobile.Deserialize preamble (mirrors Mobile.cs, target version 37)
# ---------------------------------------------------------------------------

def skip_mobile_preamble(r: BinaryReader, version: int) -> None:
    if version >= 37:
        r.read_bool()
        r.read_int32()
        r.read_int32()
        r.read_int32()
    if version >= 35:
        r.read_bool()
    if version >= 34:
        r.read_bool()
    if version >= 33:
        r.read_bool()
        r.read_int32()
        r.read_bool()
        r.read_int32()
        r.read_int32()
    if version >= 32:
        # RecordHair..RecordSkinColor (5), RaceID, five race sounds (5)
        for _ in range(11):
            r.read_int32()
        if version < 37:
            r.read_int32()  # removed CoinPurse
        for _ in range(4):  # DataStoreInt2..4, StolenBoxTime
            r.read_int32()
        for _ in range(5):  # DataStoreStr1..4, StolenArtifacts
            r.read_prefixed_shard_string()
    if version >= 31:
        r.read_delta_time()
        r.read_delta_time()
        r.read_delta_time()
    if version >= 30:
        hairflag = r.read_byte()
        if hairflag & 0x01:
            r.skip_hair_info()
        if hairflag & 0x02:
            r.skip_hair_info()
    if version >= 29:
        r.read_race()
    if version >= 28 and version <= 30:
        r.read_delta_time()
    if version >= 27:
        r.read_int32()
    if version >= 24:
        r.read_item_ref()  # m_Corpse
    if version >= 23:
        r.read_date_time()  # m_CreationTime
    if version >= 20:
        r.read_strong_mobile_list()
    if version >= 20:
        r.read_bool()
    if version >= 17:
        r.skip_virtue_info()
    if version >= 17:
        r.read_int32()
        r.read_int32()
    if version >= 16:
        r.read_int32()
        if version <= 24:
            r.read_date_time()
            r.read_date_time()
    if version >= 15:
        if version < 22:
            r.read_int32()
        r.read_int32()
    if version >= 14:
        r.read_int32()
    if version >= 13:
        r.read_mobile_ref()
    if version >= 12:
        r.read_guild_ref()
    if version >= 11:
        r.read_bool()
    if version >= 10:
        r.read_bool()
    if version >= 9:
        r.read_bool()
    if version >= 8:
        r.read_item_ref()
    if version >= 7:
        r.read_int32()
    if version >= 6:
        r.read_int32()
    if version >= 5:
        r.read_bool()
        r.read_bool()
    if version >= 4 and version <= 25:
        r.skip_poison()
    if version >= 3:
        r.read_int32()
    if version >= 2:
        r.read_int32()
    if version >= 1:
        r.read_int32()


def read_skill_entry(r: BinaryReader) -> Tuple[int, int]:
    """Returns (base_fixed, cap_fixed). base display = base_fixed / 10.0"""
    tag = r.read_byte()
    if tag == 0xFF:
        return 0, 1000
    if (tag & 0xC0) != 0x00:
        return 0, 1000
    base_fp = r.read_uint16() if (tag & 0x1) else 0
    cap_fp = r.read_uint16() if (tag & 0x2) else 1000
    if tag & 0x4:
        r.read_byte()  # lock
    return base_fp, cap_fp


def parse_skills(r: BinaryReader) -> List[Dict]:
    sk_version = r.read_int32()
    if sk_version not in (1, 2, 3):
        raise ValueError(f"unsupported Skills blob version {sk_version}")
    if sk_version >= 2:
        r.read_int32()  # cap
    if sk_version < 3:
        r.read_int32()  # legacy total
    count = r.read_int32()
    skills: List[Dict] = []
    for i in range(count):
        base_fp, cap_fp = read_skill_entry(r)
        name = SKILL_NAMES[i] if i < len(SKILL_NAMES) else f"Skill_{i}"
        skills.append({
            "id": i,
            "name": name,
            "base": round(base_fp / 10.0, 1),
            "base_fixed": base_fp,
            "cap": round(cap_fp / 10.0, 1),
        })
    return skills


def parse_mobile_stats_and_skills(blob: bytes) -> Dict:
    r = BinaryReader(blob)
    version = r.read_int32()
    if version < 30 or version > 37:
        raise ValueError(
            f"Mobile save version {version} not supported (expected 30–37). "
            "Extend skip_mobile_preamble for older backups."
        )
    skip_mobile_preamble(r, version)

    r.read_point3d()
    r.read_int32()                      # Body
    name = r.read_prefixed_shard_string()
    r.read_prefixed_shard_string()      # GuildTitle
    r.read_bool()
    r.read_int32()
    for _ in range(4):
        r.read_int32()                  # speech hues
    r.read_prefixed_shard_string()      # Language
    r.read_bool()
    r.read_bool()
    r.read_bool()
    r.read_byte()                       # Direction
    r.read_int32()                      # Hue
    raw_str = r.read_int32()
    raw_dex = r.read_int32()
    raw_int = r.read_int32()
    r.read_int32()                      # Hits
    r.read_int32()                      # Stam
    r.read_int32()                      # Mana
    r.read_map()
    r.read_bool()
    r.read_int32()                      # Fame
    r.read_int32()                      # Karma
    r.read_byte()                       # AccessLevel

    skills = parse_skills(r)

    return {
        "mobile_version": version,
        "name": name,
        "stats": {"str": raw_str, "dex": raw_dex, "int": raw_int},
        "skills": skills,
        "bytes_consumed": r.pos,
    }


# ---------------------------------------------------------------------------
# Account / Mobiles.idx helpers (same layout as analyze_character_backup.py)
# ---------------------------------------------------------------------------

def read_idx(path: str) -> List[Tuple[int, int, int, int]]:
    with open(path, "rb") as f:
        data = f.read()
    r = BinaryReader(data)
    count = r.read_int32()
    out = []
    for _ in range(count):
        type_id = r.read_int32()
        serial = r.read_int32()
        pos = r.read_int64()
        length = r.read_int32()
        out.append((type_id, serial, pos, length))
    return out


def find_account_char_serials(accounts_xml: str, account_name: str) -> List[int]:
    root = ET.parse(accounts_xml).getroot()
    for acct in root.iter("account"):
        username = ""
        uel = acct.find("username")
        if uel is not None and uel.text:
            username = uel.text.strip()
        elif acct.get("username"):
            username = acct.get("username", "").strip()
        if username.lower() != account_name.lower():
            continue
        serials = []
        chars = acct.find("chars")
        if chars is not None:
            for cel in chars.findall("char"):
                if cel.text and int(cel.text.strip()) != 0:
                    serials.append(int(cel.text.strip()))
        if not serials:
            raise ValueError(f"account '{account_name}' has no characters")
        return serials
    raise ValueError(f"account '{account_name}' not found")


def _encode_net_string(s: str) -> bytes:
    raw = s.encode("utf-8")
    n = len(raw)
    hdr = bytearray()
    while n >= 0x80:
        hdr.append((n & 0x7F) | 0x80)
        n >>= 7
    hdr.append(n)
    return bytes(hdr) + raw


def resolve_character_serial(
    backup_path: str, char_serials: List[int], character_name: str
) -> Tuple[int, List[str]]:
    warnings: List[str] = []
    if len(char_serials) == 1 or not character_name.strip():
        return char_serials[0], warnings

    idx_path = os.path.join(backup_path, "Mobiles", "Mobiles.idx")
    bin_path = os.path.join(backup_path, "Mobiles", "Mobiles.bin")
    if not os.path.isfile(idx_path) or not os.path.isfile(bin_path):
        warnings.append("Mobiles.idx/bin missing — using first character serial on account.")
        return char_serials[0], warnings

    idx = read_idx(idx_path)
    wanted = set(char_serials)
    entries = {serial: (pos, length) for _, serial, pos, length in idx if serial in wanted}
    with open(bin_path, "rb") as f:
        bin_data = f.read()

    needle = _encode_net_string(character_name)
    for serial in char_serials:
        ent = entries.get(serial)
        if not ent:
            continue
        pos, length = ent
        if needle in bin_data[pos : pos + length]:
            return serial, warnings

    warnings.append(
        f"name '{character_name}' not found in Mobiles.bin — using first serial {char_serials[0]:#x}."
    )
    return char_serials[0], warnings


def load_mobile_blob(backup_path: str, serial: int) -> bytes:
    idx_path = os.path.join(backup_path, "Mobiles", "Mobiles.idx")
    bin_path = os.path.join(backup_path, "Mobiles", "Mobiles.bin")
    for p in (idx_path, bin_path):
        if not os.path.isfile(p):
            raise FileNotFoundError(p)
    for _, s, pos, length in read_idx(idx_path):
        if s == serial:
            with open(bin_path, "rb") as f:
                f.seek(pos)
                return f.read(length)
    raise ValueError(f"serial {serial:#x} not in Mobiles.idx")


# ---------------------------------------------------------------------------
# Equipped-item StatMod / SkillMod reconstruction (from Items.bin)
# ---------------------------------------------------------------------------

def _mod_serial_name(serial: int) -> str:
    """Matches Server.Serial.ToString() → 0xXXXXXXXX."""
    return f"0x{serial & 0xFFFFFFFF:08X}"


def _compute_item_stat_offsets(props: dict) -> Tuple[int, int, int]:
    """Mirror BaseArmor.ComputeStatBonus / weapon AOS attributes / BaseRace attributes."""
    attrs = props.get("attributes") or {}
    str_b = int(props.get("str_bonus") or 0) + int(attrs.get("bonus_str") or 0)
    dex_b = int(props.get("dex_bonus") or 0) + int(attrs.get("bonus_dex") or 0)
    int_b = int(props.get("int_bonus") or 0) + int(attrs.get("bonus_int") or 0)
    return str_b, dex_b, int_b


def _build_stat_mods_for_item(
    serial: int,
    type_short: str,
    layer_name: str,
    props: dict,
) -> List[Dict[str, Any]]:
    str_b, dex_b, int_b = _compute_item_stat_offsets(props)
    mod_base = _mod_serial_name(serial)
    src = {
        "source_serial": mod_base,
        "source_type": type_short,
        "source_layer": layer_name,
    }
    mods: List[Dict[str, Any]] = []
    for stat, offset in (("str", str_b), ("dex", dex_b), ("int", int_b)):
        if offset:
            suffix = stat.capitalize()  # Str / Dex / Int
            mods.append({
                "name": mod_base + suffix,
                "type": stat,
                "offset": offset,
                "duration": "permanent",
                **src,
            })
    return mods


def _build_skill_mods_for_item(
    serial: int,
    type_short: str,
    layer_name: str,
    props: dict,
) -> List[Dict[str, Any]]:
    bonuses = props.get("skill_bonuses") or []
    mod_base = _mod_serial_name(serial)
    mods: List[Dict[str, Any]] = []
    for entry in bonuses:
        skill = entry.get("skill")
        bonus = entry.get("bonus")
        if skill is None or bonus is None or bonus == 0:
            continue
        mods.append({
            "skill": skill,
            "bonus": bonus,
            "relative": True,
            "mod_class": "DefaultSkillMod",
            "source_serial": mod_base,
            "source_type": type_short,
            "source_layer": layer_name,
        })
    return mods


def _sum_stat_mod_offsets(stat_mods: List[Dict[str, Any]]) -> Dict[str, int]:
    totals = {"str": 0, "dex": 0, "int": 0}
    for mod in stat_mods:
        t = mod.get("type")
        if t in totals:
            totals[t] += int(mod.get("offset") or 0)
    return totals


def _sum_skill_mod_bonuses(skill_mods: List[Dict[str, Any]]) -> Dict[str, float]:
    totals: Dict[str, float] = {}
    for mod in skill_mods:
        skill = mod.get("skill")
        if not skill:
            continue
        totals[skill] = round(totals.get(skill, 0.0) + float(mod.get("bonus") or 0), 1)
    return totals


def scan_equipped_mods_from_backup(
    backup_path: str, char_serial: int
) -> Tuple[List[Dict[str, Any]], List[Dict[str, Any]], List[Dict[str, Any]], List[str]]:
    """
    Parse Items.bin and return (stat_mods, skill_mods, equipped_items_summary, warnings).
    Only items directly parented to the character on equipped layers (incl. Layer.Special race).
    """
    warnings: List[str] = []
    items_tdb = os.path.join(backup_path, "Items", "Items.tdb")
    items_idx = os.path.join(backup_path, "Items", "Items.idx")
    items_bin = os.path.join(backup_path, "Items", "Items.bin")
    for p in (items_tdb, items_idx, items_bin):
        if not os.path.isfile(p):
            warnings.append(f"Items save missing ({p}); cannot reconstruct StatMod/SkillMod.")
            return [], [], [], warnings

    item_types = acb.read_tdb(items_tdb)
    item_idx_entries = acb.read_idx(items_idx)
    serial_to_entry: Dict[int, dict] = {}
    for type_id, serial, pos, length in item_idx_entries:
        type_name = item_types[type_id] if 0 <= type_id < len(item_types) else "Unknown"
        serial_to_entry[serial] = {
            "type_full": type_name,
            "type_short": type_name.rsplit(".", 1)[-1] if "." in type_name else type_name,
            "pos": pos,
            "length": length,
        }

    with open(items_bin, "rb") as f:
        bin_data = f.read()

    parent_map: Dict[int, int] = {}
    children_map: Dict[int, List[int]] = {}
    item_props: Dict[int, dict] = {}
    parse_fail = 0

    for serial, entry in serial_to_entry.items():
        pos, length = entry["pos"], entry["length"]
        if pos < 0 or pos + length > len(bin_data):
            continue
        chunk = bin_data[pos : pos + length]
        try:
            parsed, r_after = acb.parse_item_record(chunk)
            props = {k: v for k, v in parsed.items() if k != "children"}
            family = acb._classify_item_family(entry["type_short"], entry["type_full"])
            if family:
                props.update(acb._parse_subclass(r_after, family))
            item_props[serial] = props
            if parsed.get("parent") is not None:
                p = parsed["parent"]
                parent_map[serial] = p
                children_map.setdefault(p, []).append(serial)
        except Exception:
            parse_fail += 1

    if parse_fail:
        warnings.append(
            f"{parse_fail} item record(s) failed to parse while scanning for equipped mods."
        )

    direct_items = children_map.get(char_serial, [])
    stat_mods: List[Dict[str, Any]] = []
    skill_mods: List[Dict[str, Any]] = []
    equipped_summary: List[Dict[str, Any]] = []

    for serial in direct_items:
        props = item_props.get(serial, {})
        layer = props.get("layer", 0)
        if layer not in acb.EQUIPPED_LAYERS and layer != 0x0F:  # Layer.Special
            continue
        entry = serial_to_entry.get(serial, {})
        type_short = entry.get("type_short", "Unknown")
        layer_name = acb.LAYER_NAMES.get(layer, f"Layer_{layer}")

        str_b, dex_b, int_b = _compute_item_stat_offsets(props)
        item_stat_mods = _build_stat_mods_for_item(serial, type_short, layer_name, props)
        item_skill_mods = _build_skill_mods_for_item(serial, type_short, layer_name, props)
        stat_mods.extend(item_stat_mods)
        skill_mods.extend(item_skill_mods)

        equipped_summary.append({
            "serial": _mod_serial_name(serial),
            "type": type_short,
            "layer": layer_name,
            "stat_bonus": {"str": str_b, "dex": dex_b, "int": int_b},
            "skill_bonuses": props.get("skill_bonuses") or [],
            **(
                {"species_level": props["species_level"], "species_index": props.get("species_index")}
                if type_short == "BaseRace"
                else {}
            ),
        })

    if not direct_items:
        warnings.append(
            f"No items parented to character serial {char_serial:#x} in Items.bin."
        )
    elif not equipped_summary:
        warnings.append(
            "Character has direct items but none on equipped layers (only backpack/mount?)."
        )

    return stat_mods, skill_mods, equipped_summary, warnings


# ---------------------------------------------------------------------------
# CLI
# ---------------------------------------------------------------------------

def main() -> int:
    ap = argparse.ArgumentParser(description="Read base stats/skills from a Saves backup.")
    ap.add_argument("--backup-path", required=True, help="Path to Saves backup root")
    ap.add_argument("--account", required=True)
    ap.add_argument("--character", required=True)
    ap.add_argument("--output", "-o", help="Write JSON to this path")
    ap.add_argument(
        "--all-skills",
        action="store_true",
        help="Include skills with base 0 (default: only base > 0)",
    )
    ap.add_argument(
        "--skip-mods",
        action="store_true",
        help="Skip Items.bin scan (no StatMod/SkillMod reconstruction)",
    )
    args = ap.parse_args()

    backup = os.path.abspath(args.backup_path)
    accounts_xml = os.path.join(backup, "Accounts", "accounts.xml")
    if not os.path.isfile(accounts_xml):
        print(f"ERROR: missing {accounts_xml}", file=sys.stderr)
        return 1

    char_serials = find_account_char_serials(accounts_xml, args.account)
    serial, warnings = resolve_character_serial(backup, char_serials, args.character)
    blob = load_mobile_blob(backup, serial)
    parsed = parse_mobile_stats_and_skills(blob)

    skills = parsed["skills"]
    if not args.all_skills:
        skills = [s for s in skills if s["base_fixed"] > 0]

    mod_warnings: List[str] = []
    stat_mods: List[Dict[str, Any]] = []
    skill_mods: List[Dict[str, Any]] = []
    equipped_items: List[Dict[str, Any]] = []
    if not args.skip_mods:
        stat_mods, skill_mods, equipped_items, mod_warnings = scan_equipped_mods_from_backup(
            backup, serial
        )

    raw = parsed["stats"]
    stat_totals = _sum_stat_mod_offsets(stat_mods)
    effective = {
        "str": raw["str"] + stat_totals["str"],
        "dex": raw["dex"] + stat_totals["dex"],
        "int": raw["int"] + stat_totals["int"],
    }
    skill_mod_totals = _sum_skill_mod_bonuses(skill_mods)

    result = {
        "account": args.account,
        "character": args.character,
        "character_serial": f"0x{serial:X}",
        "stored_name": parsed.get("name"),
        "mobile_save_version": parsed["mobile_version"],
        "stats": {
            "raw_str": raw["str"],
            "raw_dex": raw["dex"],
            "raw_int": raw["int"],
        },
        "stat_mods": stat_mods,
        "stat_mod_totals": stat_totals,
        "effective_stats": effective,
        "skill_mods": skill_mods,
        "skill_mod_totals": skill_mod_totals,
        "equipped_mod_sources": equipped_items,
        "skills": skills,
        "notes": [
            "stats.raw_* are RawStr/RawDex/RawInt from Mobile save.",
            "skill.base is Skill.Base (no SkillMod).",
            "stat_mods / skill_mods are reconstructed from equipped items in Items.bin "
            "(same mod names as runtime: 0x{serial}Str|Dex|Int).",
            "Spell buffs, orphan mods, and backpack items are NOT included.",
        ],
        "warnings": warnings + mod_warnings,
    }

    text = json.dumps(result, ensure_ascii=False, indent=2)
    if args.output:
        with open(args.output, "w", encoding="utf-8") as f:
            f.write(text + "\n")
        print(f"Wrote {args.output}", file=sys.stderr)
    else:
        print(text)

    # Human-readable summary to stderr when writing JSON file
    if args.output:
        st = result["stats"]
        eff = result["effective_stats"]
        tot = result["stat_mod_totals"]
        print(
            f"\n{result['stored_name'] or args.character}  "
            f"Raw Str/Int/Dex={st['raw_str']}/{st['raw_int']}/{st['raw_dex']}  "
            f"Mod +{tot['str']}/+{tot['int']}/+{tot['dex']}  "
            f"Effective {eff['str']}/{eff['int']}/{eff['dex']}  "
            f"({len(stat_mods)} stat mods, {len(skill_mods)} skill mods, "
            f"{len(skills)} skills with base > 0)",
            file=sys.stderr,
        )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
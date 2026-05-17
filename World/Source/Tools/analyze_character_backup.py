#!/usr/bin/env python3
"""
analyze_character_backup.py
Analyzes a backup of RunUO/ServUO Saves files to extract a specific character's
equipment and backpack items. Outputs a JSON manifest used by CharacterRestoreGump.

Usage:
    python3 analyze_character_backup.py \
        --backup-path /path/to/Saves \
        --account AccountName \
        --character CharacterName \
        [--output path/to/output.json]

Output JSON schema:
{
  "account": "...",
  "character": "...",
  "character_serial": "0x...",
  "items": [
    {
      "serial": "0x...",
      "type_full": "Server.Items.Katana",
      "type_short": "Katana",
      "hue": 0,
      "amount": 1,
      "name": null,
      "layer": "OneHanded",
      "is_equipped": true
    },
    ...
  ],
  "errors": ["..."]
}
"""

import argparse
import json
import os
import struct
import sys
import xml.etree.ElementTree as ET
from pathlib import Path
from typing import Optional

# ---------------------------------------------------------------------------
# Binary reader helpers matching .NET BinaryWriter / GenericWriter format
# ---------------------------------------------------------------------------

class BinaryReader:
    def __init__(self, data: bytes):
        self._data = data
        self._pos = 0

    @property
    def pos(self):
        return self._pos

    @property
    def remaining(self):
        return len(self._data) - self._pos

    def seek(self, pos: int):
        self._pos = pos

    def read_bytes(self, n: int) -> bytes:
        if self._pos + n > len(self._data):
            raise EOFError(f"Expected {n} bytes at pos {self._pos}, only {self.remaining} left")
        result = self._data[self._pos:self._pos + n]
        self._pos += n
        return result

    def read_bool(self) -> bool:
        return struct.unpack('<B', self.read_bytes(1))[0] != 0

    def read_byte(self) -> int:
        return struct.unpack('<B', self.read_bytes(1))[0]

    def read_sbyte(self) -> int:
        return struct.unpack('<b', self.read_bytes(1))[0]

    def read_int16(self) -> int:
        return struct.unpack('<h', self.read_bytes(2))[0]

    def read_int32(self) -> int:
        return struct.unpack('<i', self.read_bytes(4))[0]

    def read_uint32(self) -> int:
        return struct.unpack('<I', self.read_bytes(4))[0]

    def read_int64(self) -> int:
        return struct.unpack('<q', self.read_bytes(8))[0]

    def read_float(self) -> float:
        return struct.unpack('<f', self.read_bytes(4))[0]

    def read_double(self) -> float:
        return struct.unpack('<d', self.read_bytes(8))[0]

    def read_uint32(self) -> int:
        return struct.unpack('<I', self.read_bytes(4))[0]

    def read_encoded_int(self) -> int:
        """7-bit encoded int (same as .NET BinaryWriter.Write7BitEncodedInt)."""
        result = 0
        shift = 0
        while True:
            b = self.read_byte()
            result |= (b & 0x7F) << shift
            if (b & 0x80) == 0:
                break
            shift += 7
        if result >= 0x80000000:
            result -= 0x100000000
        return result

    def read_string(self) -> Optional[str]:
        """Plain string used by Saves/*.tdb: 7-bit byte length + UTF-8 payload (no 0/null prefix byte)."""
        length = self.read_encoded_int()
        if length < 0:
            return None
        if length == 0:
            return ""
        return self.read_bytes(length).decode('utf-8', errors='replace')

    def read_prefixed_shard_string(self) -> Optional[str]:
        """
        World item/mobile blobs use BinaryFileReader.ReadString():
        prefix byte (0 = null reference), otherwise UTF-8 body with 7-bit encoded byte-length.
        See Serialization.cs BinaryFileReader.ReadString / BinaryFileWriter internal format.
        """
        sentinel = self.read_byte()
        if sentinel == 0:
            return None
        length = self.read_encoded_int()
        if length < 0:
            return None
        if length == 0:
            return ""
        return self.read_bytes(length).decode('utf-8', errors='replace')

    def read_mobile_ref(self) -> int:
        """Mobile reference: int32 serial (-1 = null)."""
        return self.read_int32()


# ---------------------------------------------------------------------------
# SaveFlag enum (from Item.cs)
# ---------------------------------------------------------------------------

class SaveFlag:
    Direction       = 0x00000001
    Bounce          = 0x00000002
    LootType        = 0x00000004
    LocationFull    = 0x00000008
    ItemID          = 0x00000010
    Hue             = 0x00000020
    Amount          = 0x00000040
    Layer           = 0x00000080
    Name            = 0x00000100
    Parent          = 0x00000200
    Items           = 0x00000400
    WeightNot1or0   = 0x00000800
    Map             = 0x00001000
    Visible         = 0x00002000
    Movable         = 0x00004000
    Stackable       = 0x00008000
    WeightIs0       = 0x00010000
    LocationSByteZ  = 0x00020000
    LocationShortXY = 0x00040000
    LocationByteXY  = 0x00080000
    ImplFlags       = 0x00100000
    InsuredFor      = 0x00200000
    BlessedFor      = 0x00400000
    HeldBy          = 0x00800000
    IntWeight       = 0x01000000
    SavedFlags      = 0x02000000
    NullWeight      = 0x04000000


# Layer index → name mapping (from Layer enum in Layer.cs)
LAYER_NAMES = {
    0x00: "Invalid",
    0x01: "OneHanded",
    0x02: "TwoHanded",
    0x03: "Shoes",
    0x04: "Pants",
    0x05: "Shirt",
    0x06: "Helm",
    0x07: "Gloves",
    0x08: "Ring",
    0x09: "Trinket",
    0x0A: "Neck",
    0x0B: "Hair",
    0x0C: "Waist",
    0x0D: "InnerTorso",
    0x0E: "Bracelet",
    0x0F: "Special",
    0x10: "FacialHair",
    0x11: "MiddleTorso",
    0x12: "Earrings",
    0x13: "Arms",
    0x14: "Cloak",
    0x15: "Backpack",
    0x16: "OuterTorso",
    0x17: "OuterLegs",
    0x18: "InnerLegs",
    0x19: "Mount",
    0x1A: "ShopBuy",
    0x1B: "ShopResale",
    0x1C: "ShopSell",
    0x1D: "Bank",
}

EQUIPPED_LAYERS = {
    0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09,
    0x0A, 0x0C, 0x0D, 0x0E, 0x0F, 0x11, 0x12, 0x13, 0x14,
    0x16, 0x17, 0x18,
}


# ---------------------------------------------------------------------------
# AOS attribute / enum tables (for subclass property decoding)
# ---------------------------------------------------------------------------

_AOS_ATTRS = {
    0x00000001: "regen_hits",
    0x00000002: "regen_stam",
    0x00000004: "regen_mana",
    0x00000008: "defend_chance",
    0x00000010: "attack_chance",
    0x00000020: "bonus_str",
    0x00000040: "bonus_dex",
    0x00000080: "bonus_int",
    0x00000100: "bonus_hits",
    0x00000200: "bonus_stam",
    0x00000400: "bonus_mana",
    0x00000800: "weapon_damage",
    0x00001000: "weapon_speed",
    0x00002000: "spell_damage",
    0x00004000: "cast_recovery",
    0x00008000: "cast_speed",
    0x00010000: "lower_mana_cost",
    0x00020000: "lower_reg_cost",
    0x00040000: "reflect_physical",
    0x00080000: "enhance_potions",
    0x00100000: "luck",
    0x00200000: "spell_channeling",
    0x00400000: "night_sight",
}
_AOS_WEAPON_ATTRS = {
    0x00000001: "lower_stat_req",
    0x00000002: "self_repair",
    0x00000004: "hit_leech_hits",
    0x00000008: "hit_leech_stam",
    0x00000010: "hit_leech_mana",
    0x00000020: "hit_lower_attack",
    0x00000040: "hit_lower_defend",
    0x00000080: "hit_magic_arrow",
    0x00000100: "hit_harm",
    0x00000200: "hit_fireball",
    0x00000400: "hit_lightning",
    0x00000800: "hit_dispel",
    0x00001000: "hit_cold_area",
    0x00002000: "hit_fire_area",
    0x00004000: "hit_poison_area",
    0x00008000: "hit_energy_area",
    0x00010000: "hit_physical_area",
    0x00020000: "resist_physical_bonus",
    0x00040000: "resist_fire_bonus",
    0x00080000: "resist_cold_bonus",
    0x00100000: "resist_poison_bonus",
    0x00200000: "resist_energy_bonus",
    0x00400000: "use_best_skill",
    0x00800000: "mage_weapon",
    0x01000000: "durability_bonus",
}
_AOS_ARMOR_ATTRS = {
    0x00000001: "lower_stat_req",
    0x00000002: "self_repair",
    0x00000004: "mage_armor",
    0x00000008: "durability_bonus",
}
_AOS_ELEMENT_ATTRS = {
    0x00000001: "physical",
    0x00000002: "fire",
    0x00000004: "cold",
    0x00000008: "poison",
    0x00000010: "energy",
    0x00000020: "chaos",
    0x00000040: "direct",
}

# SkillName enum index → string (from Skills.cs)
_SKILL_NAMES = [
    "Alchemy", "Anatomy", "Druidism", "Mercantile", "ArmsLore", "Parry",
    "Begging", "Blacksmith", "Bowcraft", "Peacemaking", "Camping", "Carpentry",
    "Cartography", "Cooking", "Searching", "Discordance", "Psychology",
    "Healing", "Seafaring", "Forensics", "Herding", "Hiding", "Provocation",
    "Inscribe", "Lockpicking", "Magery", "MagicResist", "Tactics", "Snooping",
    "Musicianship", "Poisoning", "Marksmanship", "Spiritualism", "Stealing",
    "Tailoring", "Taming", "Tasting", "Tinkering", "Tracking", "Veterinary",
    "Swords", "Bludgeoning", "Fencing", "FistFighting", "Lumberjacking",
    "Mining", "Meditation", "Stealth", "RemoveTrap", "Necromancy", "Focus",
    "Knightship", "Bushido", "Ninjitsu", "Elementalism", "Mysticism",
    "Imbuing", "Throwing",
]
_SLAYER_NAMES = [
    "None", "Silver", "OrcSlaying", "TrollSlaughter", "OgreTrashing", "Repond",
    "DragonSlaying", "Terathan", "SnakesBane", "LizardmanSlaughter",
    "ReptilianDeath", "DaemonDismissal", "GargoylesFoe", "BalronDamnation",
    "Exorcism", "Ophidian", "SpidersDeath", "ScorpionsBane", "ArachnidDoom",
    "FlameDousing", "WaterDissipation", "Vacuum", "ElementalHealth",
    "EarthShatter", "BloodDrinking", "SummerWind", "ElementalBan",
    "WizardSlayer", "AvianHunter", "SlimyScourge", "AnimalHunter",
    "GiantKiller", "GolemDestruction", "WeedRuin", "NeptunesBane", "Fey",
]
_WEAPON_DAMAGE_LEVELS = ["Regular", "Ruin", "Might", "Force", "Power", "Vanq"]
_WEAPON_ACCURACY_LEVELS = [
    "Regular", "Accurate", "Surpassingly", "Eminently", "Exceedingly", "Supremely"
]
_WEAPON_DURABILITY_LEVELS = [
    "Regular", "Durable", "Substantial", "Massive", "Fortified", "Indestructible"
]
_WEAPON_QUALITY_NAMES = ["Low", "Regular", "Exceptional"]
_ARMOR_QUALITY_NAMES = ["Low", "Regular", "Exceptional"]
_ARMOR_DURABILITY_NAMES = [
    "Regular", "Durable", "Substantial", "Massive", "Fortified", "Indestructible"
]
_ARMOR_PROTECTION_NAMES = [
    "Regular", "Defense", "Guarding", "Hardening", "Fortification", "Invulnerability"
]
_TRINKET_QUALITY_NAMES = ["Regular", "Exceptional"]

# ---------------------------------------------------------------------------
# Density computation (mirrors ResourceInfo.GetDensity C# logic)
# ---------------------------------------------------------------------------

_DENSITY_NAMES = ["None", "Weak", "Regular", "Great", "Greater", "Superior", "Ultimate"]

def _resource_rtype(resource: int) -> str:
    """Return the CraftResourceType name for a CraftResource enum value."""
    if 1   <= resource <= 30:  return "Metal"
    if 71  <= resource <= 85:  return "Block"
    if 101 <= resource <= 122: return "Leather"
    if 151 <= resource <= 160: return "Skin"
    if 201 <= resource <= 217: return "Scales"
    if 251 <= resource <= 262: return "Fabric"
    if 301 <= resource <= 322: return "Wood"
    if 401 <= resource <= 443: return "Skeletal"
    if resource >= 501:        return "Special"
    return "Unknown"

def _armor_mtype(type_short: str) -> str:
    """Derive ArmorMaterialType from type_short name keywords."""
    ts = type_short
    # Dragon* items are scaled armor in-game (DragonGloves, DragonChest, etc.)
    if any(k in ts for k in ("Scaled", "Scaly", "Dragon")):  return "Scaled"
    if "Plate"    in ts:                                      return "Plate"
    if "Chain"    in ts:                                      return "Chainmail"
    if "Ringmail" in ts:                                      return "Ringmail"
    if "Studded"  in ts:                                      return "Studded"
    if "Bone"     in ts:                                      return "Bone"
    if any(k in ts for k in ("Scholar", "Robe")):             return "Cloth"
    if "Leather"  in ts:                                      return "Leather"
    return "Unknown"

def _compute_density(resource: int, type_short: str) -> str:
    """
    Compute the Density enum string for an armor piece.
    Resource type takes priority over material type in the C# GetDensity logic,
    except for material-type overrides (Cloth, Scaled, Plate, Chainmail, Ringmail)
    when the resource type alone doesn't decide.
    """
    if not resource:
        return "None"
    rtype = _resource_rtype(resource)
    mtype = _armor_mtype(type_short)

    if rtype == "Special":
        return "Superior" if resource == 506 else "Ultimate"   # 506 = GildedSpec
    # Fabric resource or Cloth material → Weak
    if rtype == "Fabric" or mtype == "Cloth":
        return "Weak"
    # Scales resource or Scaled material → Greater  (checked before Leather to handle Dragon items)
    if rtype == "Scales" or mtype == "Scaled":
        return "Greater"
    # Leather/Skin resource or Leather/Studded material → Regular
    if rtype in ("Leather", "Skin") or mtype in ("Leather", "Studded"):
        return "Regular"
    if rtype == "Wood":
        return "Great"
    if rtype == "Skeletal" or mtype == "Bone":
        return "Great"
    if mtype == "Plate":
        if rtype == "Metal":  return "Superior"
        if rtype == "Block":  return "Ultimate"
    if mtype in ("Chainmail", "Ringmail"):
        if rtype == "Metal":  return "Greater"
        if rtype == "Block":  return "Superior"
    if rtype == "Metal":  return "Greater"
    if rtype == "Block":  return "Superior"
    return "None"


def _read_base_attributes(r: BinaryReader, bit_names: dict) -> dict:
    """Deserialize a BaseAttributes-derived blob → {name: value} with non-zero values only."""
    ver = r.read_byte()
    names_mask = r.read_uint32()
    if ver == 1:
        count = r.read_encoded_int()
        values = [r.read_encoded_int() for _ in range(count)]
    else:
        count = r.read_int32()
        values = [r.read_int32() for _ in range(count)]
    result: dict = {}
    val_idx = 0
    for bit in range(32):
        mask = 1 << bit
        if names_mask & mask:
            if val_idx < len(values):
                v = values[val_idx]
                if v != 0:
                    label = bit_names.get(mask, f"attr_0x{mask:08x}")
                    result[label] = v
            val_idx += 1
    return result


def _read_skill_bonuses(r: BinaryReader) -> list:
    """Deserialize AosSkillBonuses → list of {skill, bonus} pairs (non-zero only)."""
    ver = r.read_byte()
    names_mask = r.read_uint32()
    if ver == 1:
        count = r.read_encoded_int()
        values = [r.read_encoded_int() for _ in range(count)]
    else:
        count = r.read_int32()
        values = [r.read_int32() for _ in range(count)]
    result = []
    val_idx = 0
    for bit in range(32):
        if names_mask & (1 << bit):
            if val_idx < len(values):
                packed = values[val_idx]
                # Simulate AosSkillBonuses.GetValues — mirrors the C# loop:
                #   vSkill <<= 1; vSkill |= (v & 1); v >>= 1;
                #   vBonus <<= 1; vBonus |= (v & 1); v >>= 1;
                v_skill = v_bonus = 0
                tmp = packed
                for _ in range(16):
                    v_skill = (v_skill << 1) | (tmp & 1)
                    tmp >>= 1
                    v_bonus = (v_bonus << 1) | (tmp & 1)
                    tmp >>= 1
                bonus = v_bonus / 10.0
                if bonus != 0.0:
                    skill_name = (_SKILL_NAMES[v_skill]
                                  if 0 <= v_skill < len(_SKILL_NAMES)
                                  else f"Skill_{v_skill}")
                    result.append({"skill": skill_name, "bonus": round(bonus, 1)})
            val_idx += 1
    return result


# BaseWeapon SaveFlag constants (from BaseWeapon.cs)
class _WpnFlag:
    DamageLevel      = 0x00000001
    AccuracyLevel    = 0x00000002
    DurabilityLevel  = 0x00000004
    Quality          = 0x00000008
    Hits             = 0x00000010
    MaxHits          = 0x00000020
    Slayer           = 0x00000040
    Poison           = 0x00000080
    PoisonCharges    = 0x00000100
    NoLonger_Used    = 0x00000200  # BuiltBy (mobile ref)
    Identified       = 0x00000400  # no data bytes for version >= 6
    StrReq           = 0x00000800
    DexReq           = 0x00001000
    IntReq           = 0x00002000
    MinDamage        = 0x00004000
    MaxDamage        = 0x00008000
    HitSound         = 0x00010000
    MissSound        = 0x00020000
    Speed            = 0x00040000
    MaxRange         = 0x00080000
    Skill            = 0x00100000
    Type             = 0x00200000
    Animation        = 0x00400000
    Resource         = 0x00800000
    xAttributes      = 0x01000000
    xWeaponAttributes= 0x02000000
    NoLongerUsed     = 0x04000000  # no data bytes in v13
    SkillBonuses     = 0x08000000
    Slayer2          = 0x10000000
    ElementalDamages = 0x20000000
    EngravedText     = 0x40000000
    TrapDamaged      = -0x80000000  # signed int interpretation


# BaseArmor SaveFlag constants (from BaseArmor.cs)
class _ArmFlag:
    Attributes      = 0x00000001
    ArmorAttributes = 0x00000002
    PhysicalBonus   = 0x00000004
    FireBonus       = 0x00000008
    ColdBonus       = 0x00000010
    PoisonBonus     = 0x00000020
    EnergyBonus     = 0x00000040
    Identified      = 0x00000080  # no data bytes for version >= 7
    MaxHitPoints    = 0x00000100
    HitPoints       = 0x00000200
    NoLonger_Used   = 0x00000400  # BuiltBy mobile, read for all versions
    Quality         = 0x00000800
    Durability      = 0x00001000
    Protection      = 0x00002000
    Resource        = 0x00004000
    BaseArmor       = 0x00008000
    StrBonus        = 0x00010000
    DexBonus        = 0x00020000
    IntBonus        = 0x00040000
    StrReq          = 0x00080000
    DexReq          = 0x00100000
    IntReq          = 0x00200000
    MedAllowance    = 0x00400000
    SkillBonuses    = 0x00800000
    NotUsedAnymore  = 0x01000000  # no data bytes
    TrapDamaged     = 0x02000000


def _parse_weapon_subclass(r: BinaryReader) -> dict:
    """
    Parse BaseWeapon.Serialize data (version 13) from the byte stream.
    Called immediately after parse_item_record consumes the Item base-class bytes.
    Returns weapon properties dict; raises on alignment error.
    """
    version = r.read_int32()
    if version < 5 or version > 15:
        raise ValueError(f"Unexpected BaseWeapon version {version}")

    flags = r.read_int32()

    def flag(f: int) -> bool:
        return bool(flags & f)

    out: dict = {}

    if flag(_WpnFlag.DamageLevel):
        v = r.read_int32()
        out["damage_level"] = (_WEAPON_DAMAGE_LEVELS[v]
                               if 0 <= v < len(_WEAPON_DAMAGE_LEVELS) else v)
    if flag(_WpnFlag.AccuracyLevel):
        v = r.read_int32()
        out["accuracy_level"] = (_WEAPON_ACCURACY_LEVELS[v]
                                 if 0 <= v < len(_WEAPON_ACCURACY_LEVELS) else v)
    if flag(_WpnFlag.DurabilityLevel):
        v = r.read_int32()
        out["durability_level"] = (_WEAPON_DURABILITY_LEVELS[v]
                                   if 0 <= v < len(_WEAPON_DURABILITY_LEVELS) else v)
    if flag(_WpnFlag.Quality):
        v = r.read_int32()
        out["quality"] = (_WEAPON_QUALITY_NAMES[v]
                          if 0 <= v < len(_WEAPON_QUALITY_NAMES) else v)
    if flag(_WpnFlag.Hits):
        out["hits"] = r.read_int32()
    if flag(_WpnFlag.MaxHits):
        out["max_hits"] = r.read_int32()
    if flag(_WpnFlag.Slayer):
        v = r.read_int32()
        name = _SLAYER_NAMES[v] if 0 <= v < len(_SLAYER_NAMES) else f"Slayer_{v}"
        out["slayer"] = name
    if flag(_WpnFlag.Poison):
        sentinel = r.read_byte()
        if sentinel == 1:
            lvl = r.read_byte()
            out["poison_level"] = lvl
    if flag(_WpnFlag.PoisonCharges):
        out["poison_charges"] = r.read_int32()
    if flag(_WpnFlag.NoLonger_Used):
        r.read_int32()                        # BuiltBy mobile serial — skip
    # Identified: for version >= 6, no bytes consumed (flag only)
    if flag(_WpnFlag.StrReq):
        out["str_req"] = r.read_int32()
    if flag(_WpnFlag.DexReq):
        out["dex_req"] = r.read_int32()
    if flag(_WpnFlag.IntReq):
        out["int_req"] = r.read_int32()
    if flag(_WpnFlag.MinDamage):
        out["min_damage"] = r.read_int32()
    if flag(_WpnFlag.MaxDamage):
        out["max_damage"] = r.read_int32()
    if flag(_WpnFlag.HitSound):
        r.read_int32()                        # HitSound — cosmetic, skip
    if flag(_WpnFlag.MissSound):
        r.read_int32()                        # MissSound — cosmetic, skip
    if flag(_WpnFlag.Speed):
        out["speed"] = r.read_float()
    if flag(_WpnFlag.MaxRange):
        out["max_range"] = r.read_int32()
    if flag(_WpnFlag.Skill):
        v = r.read_int32()
        out["skill"] = (_SKILL_NAMES[v]
                        if 0 <= v < len(_SKILL_NAMES) else f"Skill_{v}")
    if flag(_WpnFlag.Type):
        r.read_int32()                        # WeaponType — skip (internal)
    if flag(_WpnFlag.Animation):
        r.read_int32()                        # WeaponAnimation — skip
    if flag(_WpnFlag.Resource):
        out["weapon_resource"] = r.read_int32()
    if flag(_WpnFlag.xAttributes):
        attrs = _read_base_attributes(r, _AOS_ATTRS)
        if attrs:
            out["attributes"] = attrs
    if flag(_WpnFlag.xWeaponAttributes):
        wattrs = _read_base_attributes(r, _AOS_WEAPON_ATTRS)
        if wattrs:
            out["weapon_attributes"] = wattrs
    # NoLongerUsed: no bytes consumed
    if flag(_WpnFlag.SkillBonuses):
        bonuses = _read_skill_bonuses(r)
        if bonuses:
            out["skill_bonuses"] = bonuses
    if flag(_WpnFlag.Slayer2):
        v = r.read_int32()
        name = _SLAYER_NAMES[v] if 0 <= v < len(_SLAYER_NAMES) else f"Slayer_{v}"
        if name not in ("None", "Slayer_0"):
            out["slayer2"] = name
    if flag(_WpnFlag.ElementalDamages):
        edamg = _read_base_attributes(r, _AOS_ELEMENT_ATTRS)
        if edamg:
            out["elemental_damage"] = edamg
    if flag(_WpnFlag.EngravedText):
        out["engraved_text"] = r.read_prefixed_shard_string()
    if flag(_WpnFlag.TrapDamaged):
        r.read_bool()                         # TrapDamaged — skip

    return out


def _parse_armor_subclass(r: BinaryReader) -> dict:
    """
    Parse BaseArmor.Serialize data (version 11) from the byte stream.
    Called immediately after parse_item_record consumes the Item base-class bytes.
    """
    version = r.read_int32()
    if version < 5 or version > 15:
        raise ValueError(f"Unexpected BaseArmor version {version}")

    flags = r.read_encoded_int()

    def flag(f: int) -> bool:
        return bool(flags & f)

    out: dict = {}

    if flag(_ArmFlag.Attributes):
        attrs = _read_base_attributes(r, _AOS_ATTRS)
        if attrs:
            out["attributes"] = attrs
    if flag(_ArmFlag.ArmorAttributes):
        aattrs = _read_base_attributes(r, _AOS_ARMOR_ATTRS)
        if aattrs:
            out["armor_attributes"] = aattrs
    if flag(_ArmFlag.PhysicalBonus):
        out["resist_physical"] = r.read_encoded_int()
    if flag(_ArmFlag.FireBonus):
        out["resist_fire"] = r.read_encoded_int()
    if flag(_ArmFlag.ColdBonus):
        out["resist_cold"] = r.read_encoded_int()
    if flag(_ArmFlag.PoisonBonus):
        out["resist_poison"] = r.read_encoded_int()
    if flag(_ArmFlag.EnergyBonus):
        out["resist_energy"] = r.read_encoded_int()
    # Identified: for version >= 7 no bytes consumed
    if flag(_ArmFlag.MaxHitPoints):
        out["max_hit_points"] = r.read_encoded_int()
    if flag(_ArmFlag.HitPoints):
        out["hit_points"] = r.read_encoded_int()
    if flag(_ArmFlag.NoLonger_Used):
        r.read_int32()                        # BuiltBy mobile serial — skip
    if flag(_ArmFlag.Quality):
        v = r.read_encoded_int()
        out["quality"] = (_ARMOR_QUALITY_NAMES[v]
                          if 0 <= v < len(_ARMOR_QUALITY_NAMES) else v)
    if flag(_ArmFlag.Durability):
        v = r.read_encoded_int()
        out["durability"] = (_ARMOR_DURABILITY_NAMES[v]
                             if 0 <= v < len(_ARMOR_DURABILITY_NAMES) else v)
    if flag(_ArmFlag.Protection):
        v = r.read_encoded_int()
        out["protection"] = (_ARMOR_PROTECTION_NAMES[v]
                             if 0 <= v < len(_ARMOR_PROTECTION_NAMES) else v)
    if flag(_ArmFlag.Resource):
        out["armor_resource"] = r.read_encoded_int()
    if flag(_ArmFlag.BaseArmor):
        out["base_armor"] = r.read_encoded_int()
    if flag(_ArmFlag.StrBonus):
        out["str_bonus"] = r.read_encoded_int()
    if flag(_ArmFlag.DexBonus):
        out["dex_bonus"] = r.read_encoded_int()
    if flag(_ArmFlag.IntBonus):
        out["int_bonus"] = r.read_encoded_int()
    if flag(_ArmFlag.StrReq):
        out["str_req"] = r.read_encoded_int()
    if flag(_ArmFlag.DexReq):
        out["dex_req"] = r.read_encoded_int()
    if flag(_ArmFlag.IntReq):
        out["int_req"] = r.read_encoded_int()
    if flag(_ArmFlag.MedAllowance):
        r.read_encoded_int()                  # MedAllowance — skip (internal)
    if flag(_ArmFlag.SkillBonuses):
        bonuses = _read_skill_bonuses(r)
        if bonuses:
            out["skill_bonuses"] = bonuses
    # NotUsedAnymore: no bytes consumed
    if flag(_ArmFlag.TrapDamaged):
        r.read_bool()                         # TrapDamaged — skip

    return out


def _parse_trinket_subclass(r: BinaryReader) -> dict:
    """
    Parse BaseTrinket.Serialize data (version 5) from the byte stream.
    """
    version = r.read_int32()
    if version < 0 or version > 10:
        raise ValueError(f"Unexpected BaseTrinket version {version}")

    out: dict = {}

    if version >= 5:
        # AosArmorAttributes + quality
        aattrs = _read_base_attributes(r, _AOS_ARMOR_ATTRS)
        if aattrs:
            out["armor_attributes"] = aattrs
        v = r.read_encoded_int()
        out["quality"] = (_TRINKET_QUALITY_NAMES[v]
                          if 0 <= v < len(_TRINKET_QUALITY_NAMES) else v)

    if version >= 3:
        out["max_hit_points"] = r.read_encoded_int()
        out["hit_points"] = r.read_encoded_int()

    if version >= 2:
        if version < 4:
            r.read_encoded_int()              # resource — dropped in v4
        v = r.read_encoded_int()
        out["gem_type"] = v                   # GemType enum index

    if version >= 1:
        attrs = _read_base_attributes(r, _AOS_ATTRS)
        if attrs:
            out["attributes"] = attrs
        resistances = _read_base_attributes(r, _AOS_ELEMENT_ATTRS)
        if resistances:
            out["resistances"] = resistances
        bonuses = _read_skill_bonuses(r)
        if bonuses:
            out["skill_bonuses"] = bonuses

    return out


# ---------------------------------------------------------------------------
# Container layer skip (intermediate class between Item and BaseQuiver)
# ---------------------------------------------------------------------------

def _skip_container_layer(r: BinaryReader):
    """
    Consume Container.Serialize bytes.
    Chain: Item -> Container -> BaseQuiver -> MagicQuiver.
    Container.Serialize currently writes version 2.
    """
    version = r.read_int32()
    if version == 2:
        flags = r.read_byte()
        if flags & 0x01:  r.read_encoded_int()  # MaxItems
        if flags & 0x02:  r.read_encoded_int()  # GumpID
        if flags & 0x04:  r.read_encoded_int()  # DropSound
        # LiftOverride is flags & 0x08, no data bytes
    elif version == 1:
        r.read_int32()   # MaxItems (falls through to case 0)
        r.read_int32()   # GumpID
        r.read_int32()   # DropSound
        for _ in range(4): r.read_int32()   # 2x Point2D (bounds, legacy)
    elif version == 0:
        r.read_int32()   # GumpID
        r.read_int32()   # DropSound
        for _ in range(4): r.read_int32()   # 2x Point2D (bounds, legacy)
    else:
        raise ValueError(f"Unexpected Container layer version {version}")


def _parse_quiver_subclass(r: BinaryReader) -> dict:
    """
    Parse Container.Serialize + BaseQuiver.Serialize + MagicQuiver.Serialize.
    Inheritance: Item -> Container -> BaseQuiver -> MagicQuiver.
    """
    _skip_container_layer(r)

    # BaseQuiver.Serialize flags
    _QF_Attributes     = 0x01
    _QF_LowerAmmoCost  = 0x04
    _QF_WeightReduct   = 0x08
    _QF_DamageIncrease = 0x80
    _QF_Crafter        = 0x10
    _QF_Quality        = 0x20
    _QF_Capacity       = 0x40

    version = r.read_int32()
    if version < 0 or version > 5:
        raise ValueError(f"Unexpected BaseQuiver version {version}")
    flags = r.read_encoded_int()

    out: dict = {}

    if flags & _QF_Attributes:
        attrs = _read_base_attributes(r, _AOS_ATTRS)
        if attrs:
            out["attributes"] = attrs
    if flags & _QF_LowerAmmoCost:
        out["lower_ammo_cost"] = r.read_int32()
    if flags & _QF_WeightReduct:
        out["weight_reduction"] = r.read_int32()
    if flags & _QF_DamageIncrease:
        out["damage_increase"] = r.read_int32()
    if flags & _QF_Crafter:
        r.read_mobile_ref()          # Crafter mobile — skip for all versions
    if flags & _QF_Quality:
        out["quality"] = r.read_int32()
    if flags & _QF_Capacity:
        out["capacity"] = r.read_int32()

    # MagicQuiver.Serialize: just a version int (no additional data)
    r.read_int32()

    return out


# ---------------------------------------------------------------------------
# Spellbook subclass parser
# ---------------------------------------------------------------------------

def _parse_spellbook_subclass(r: BinaryReader) -> dict:
    """
    Parse Spellbook.Serialize data (versions 0–5).
    Also consumed for all Spellbook subclasses (BookOfChivalry, SongBook, …).
    """
    version = r.read_int32()
    if version < 0 or version > 10:
        raise ValueError(f"Unexpected Spellbook version {version}")

    out: dict = {}

    # case 5/4/3: if version == 3 (< 4), read BuiltBy mobile ref
    if version == 3:
        r.read_mobile_ref()

    # case 2: slayer × 2
    if version >= 2:
        v = r.read_int32()
        slayer = _SLAYER_NAMES[v] if 0 <= v < len(_SLAYER_NAMES) else f"Slayer_{v}"
        if slayer != "None":
            out["slayer"] = slayer
        v = r.read_int32()
        slayer2 = _SLAYER_NAMES[v] if 0 <= v < len(_SLAYER_NAMES) else f"Slayer_{v}"
        if slayer2 != "None":
            out["slayer2"] = slayer2

    # case 1: AosAttributes, optionally AosElementAttributes (v>4), AosSkillBonuses
    if version >= 1:
        attrs = _read_base_attributes(r, _AOS_ATTRS)
        if attrs:
            out["attributes"] = attrs
        if version > 4:
            resists = _read_base_attributes(r, _AOS_ELEMENT_ATTRS)
            if resists:
                out["resistances"] = resists
        bonuses = _read_skill_bonuses(r)
        if bonuses:
            out["skill_bonuses"] = bonuses

    # case 0: ulong content (spell bitmask) + int spell count
    r.read_bytes(8)                    # content (ulong) — raw spell bits, skip
    out["spell_count"] = r.read_int32()

    return out


# ---------------------------------------------------------------------------
# Runebook subclass parser
# ---------------------------------------------------------------------------

def _parse_runebook_subclass(r: BinaryReader) -> dict:
    """
    Parse Runebook.Serialize data (version 5).
    Runebook inherits directly from Item (no intermediate Container layer).
    """
    version = r.read_int32()
    if version < 0 or version > 10:
        raise ValueError(f"Unexpected Runebook version {version}")

    out: dict = {}

    # case 5/4: if version < 5, read SpellType (int32)
    if version < 5:
        r.read_int32()             # SpellType — skip

    # case 3/2: if version < 3, read BuiltBy mobile ref
    if version < 3:
        r.read_mobile_ref()

    # case 1: SecureLevel
    if version >= 1:
        out["secure_level"] = r.read_int32()

    # case 0: entries + description + charges + defaultIndex
    count = r.read_int32()
    entries = []
    for _ in range(count):
        entry_ver = r.read_byte()
        if entry_ver >= 1:
            r.read_int32()         # house item serial — skip
        x = r.read_int32()
        y = r.read_int32()
        z = r.read_int32()
        map_idx = r.read_byte()
        desc = r.read_prefixed_shard_string()
        entries.append({
            "x": x, "y": y, "z": z,
            "map": map_idx,
            "description": desc,
        })
    if entries:
        out["rune_entries"] = entries

    description = r.read_prefixed_shard_string()
    if description:
        out["description"] = description
    out["cur_charges"]  = r.read_int32()
    out["max_charges"]  = r.read_int32()
    out["default_index"] = r.read_int32()

    return out


# ---------------------------------------------------------------------------
# Type-family detection (determines which subclass parser to call)
# ---------------------------------------------------------------------------

# Keywords in type_short that identify weapon types
# NOTE: "Quiver" removed — BaseQuiver inherits Container, not BaseWeapon
_WEAPON_KW = frozenset({
    "Axe", "Sword", "Katana", "Wakizashi", "Kama", "Lajatang", "NoDachi",
    "Nunchaku", "Sai", "Tessen", "Tetsubo", "Bokuto", "Daisho",
    "Bow", "Crossbow", "Yumi", "Harpoon",
    "Dagger", "Knife", "Lance", "Spear", "Fork", "Bardiche", "Halberd",
    "Scythe", "Pike", "Staff", "Mace", "Club", "Maul", "Hammer", "Wand",
    "Whip", "Scimitar", "Broadsword", "Longsword", "ShortSword", "Kryss",
    "Cutlass", "Blade", "Machete", "Shovel", "Pickaxe", "Crook",
    "Pitchfork", "Leafblade", "Cleaver", "AssassinSpike",
    "Gladius", "Falchion", "ElvenSpellblade", "RuneBlade", "RadiantScimitar",
    "ThinLongsword", "RoyalSword", "BoneHarvester", "CrescentBlade",
    "Claymore", "VikingSword", "Tekagi", "WarFork", "MagicalWand",
    "ThrowingGloves", "ThrowingWeapon", "ThrowingDagger",
    "PugilistGlove", "PugilistMits",
})

# Keywords in type_short that identify armor types
_ARMOR_KW = frozenset({
    "Gorget", "Helm", "Helmet", "Coif", "Cap", "Hood", "Bonnet",
    "Gloves", "Gauntlets", "Gaunts", "Pauldrons",
    "Chest", "Breastplate", "Tunic",
    "Leggings", "Legs", "Pants", "Boots", "Shoes",
    "Arms", "Vambraces", "Sleeves",
    "Shield", "Kite", "Buckler", "Heater",
    "Plate", "Chainmail", "Ringmail", "Banded", "Bone",
    "Studded", "Leather", "Scaled", "Scaly", "Dragon",
    "Cloak", "Robe",
})

# Keywords in type_short that identify trinket types
_TRINKET_KW = frozenset({
    "Ring", "Earrings", "Bracelet", "Necklace", "Amulet",
    "Talisman", "Circlet", "Jewelry",
})

# Spellbook family: Spellbook and all subclasses (BookOfChivalry, SongBook, …)
_SPELLBOOK_KW = frozenset({
    "Spellbook", "BookOf", "SongBook", "AncientSpellbook",
})


def _classify_item_family(type_short: str) -> str:
    """Returns 'weapon', 'armor', 'trinket', 'quiver', 'spellbook', 'runebook', or ''."""
    # Exact-prefix checks first to avoid false keyword matches
    if "Runebook" in type_short:
        return "runebook"
    if "Quiver" in type_short:
        return "quiver"
    for kw in _SPELLBOOK_KW:
        if kw in type_short:
            return "spellbook"
    # Broad keyword checks
    for kw in _TRINKET_KW:
        if kw in type_short:
            return "trinket"
    for kw in _WEAPON_KW:
        if kw in type_short:
            return "weapon"
    for kw in _ARMOR_KW:
        if kw in type_short:
            return "armor"
    return ""


def _parse_subclass(r: BinaryReader, family: str) -> dict:
    """Dispatch to the appropriate subclass parser. Returns {} on any error."""
    try:
        if family == "weapon":
            return _parse_weapon_subclass(r)
        if family == "armor":
            return _parse_armor_subclass(r)
        if family == "trinket":
            return _parse_trinket_subclass(r)
        if family == "quiver":
            return _parse_quiver_subclass(r)
        if family == "spellbook":
            return _parse_spellbook_subclass(r)
        if family == "runebook":
            return _parse_runebook_subclass(r)
    except Exception:
        pass
    return {}


# ---------------------------------------------------------------------------
# BinaryReader float support
# ---------------------------------------------------------------------------

def skip_bounce_info(r: BinaryReader):
    """Skip BounceInfo.Serialize output (written only when Bounce flag is set)."""
    is_present = r.read_bool()
    if is_present:
        r.read_byte()      # Map (1 byte)
        r.read_int32()     # Location.X
        r.read_int32()     # Location.Y
        r.read_int32()     # Location.Z
        r.read_int32()     # WorldLoc.X
        r.read_int32()     # WorldLoc.Y
        r.read_int32()     # WorldLoc.Z
        r.read_int32()     # parent serial


def parse_item_record(data: bytes):
    """
    Parse all fields from an Item.Serialize version 6-14 binary record.

    Captures every base-class property stored by Item.Serialize v14.
    Subclass-specific properties (BaseWeapon stats etc.) are not accessible here.
    Returns a dict; raises on parse failure.
    """
    r = BinaryReader(data)
    version = r.read_int32()
    if version < 6 or version > 14:
        raise ValueError(f"Unsupported item version {version}")

    props: dict = {
        "version": version,
        "hue": 0, "amount": 1, "layer": 0, "name": None,
        "parent": None, "children": [], "flags": 0,
    }

    if version >= 14:
        r.read_bool()                            # Purchased — internal flag, not restored
        props["enchant_mod"]     = r.read_int32()
        props["color_hue1"]      = r.read_prefixed_shard_string()
        props["color_text1"]     = r.read_prefixed_shard_string()
        props["color_hue2"]      = r.read_prefixed_shard_string()
        props["color_text2"]     = r.read_prefixed_shard_string()
        props["color_hue3"]      = r.read_prefixed_shard_string()
        props["color_text3"]     = r.read_prefixed_shard_string()
        props["color_hue4"]      = r.read_prefixed_shard_string()
        props["color_text4"]     = r.read_prefixed_shard_string()
        props["color_hue5"]      = r.read_prefixed_shard_string()
        props["color_text5"]     = r.read_prefixed_shard_string()
        props["world_item_id"]   = r.read_int32()
        props["technology"]      = r.read_bool()
        props["virtual_container"] = r.read_bool()
        props["not_identified"]  = r.read_bool()
        props["not_id_attempts"] = r.read_int32()
        props["not_id_source"]   = r.read_encoded_int()  # Identity enum
        props["not_id_skill"]    = r.read_encoded_int()  # IDSkill enum
        props["catalog"]         = r.read_encoded_int()  # Catalogs enum
        props["coin_price"]      = r.read_int32()
        props["resource"]        = r.read_encoded_int()  # CraftResource enum
        props["sub_resource"]    = r.read_encoded_int()  # CraftResource enum
        props["sub_name"]        = r.read_prefixed_shard_string()
        props["artifact_level"]  = r.read_int32()        # ArtifactLevel enum
        props["not_mod_able"]    = r.read_bool()
        props["needs_both_hands"] = r.read_bool()
        props["info_data"]       = r.read_prefixed_shard_string()
        props["info_text1"]      = r.read_prefixed_shard_string()
        props["info_text2"]      = r.read_prefixed_shard_string()
        props["info_text3"]      = r.read_prefixed_shard_string()
        props["info_text4"]      = r.read_prefixed_shard_string()
        props["info_text5"]      = r.read_prefixed_shard_string()
        props["limits"]          = r.read_int32()
        props["limits_max"]      = r.read_int32()
        props["limits_name"]     = r.read_prefixed_shard_string()
        props["limits_delete"]   = r.read_bool()
        r.read_mobile_ref()                      # BuiltBy — not restored
        props["built"]           = r.read_bool()

    if version >= 11:
        props["enchanted_spell"]  = r.read_encoded_int()  # MagicSpell enum
        props["enchant_uses"]     = r.read_int32()
        props["enchant_uses_max"] = r.read_int32()

    if version >= 10:
        props["graphic_id"]       = r.read_int32()
        props["graphic_hue"]      = r.read_int32()
        r.read_mobile_ref()                       # LastMobile — not restored
        r.read_prefixed_shard_string()                           # LastMobileName — not restored

    # ── Flags-based section (all versions ≥ 6) ──────────────────────────
    flags = r.read_int32()
    props["flags"] = flags

    if version >= 7:
        r.read_encoded_int()   # minutes since last moved
    else:
        r.read_int64()         # DeltaTime (ticks)

    if flags & SaveFlag.Direction:
        r.read_byte()

    if flags & SaveFlag.Bounce:
        skip_bounce_info(r)

    if flags & SaveFlag.LootType:
        r.read_byte()

    if flags & SaveFlag.LocationFull:
        r.read_encoded_int(); r.read_encoded_int(); r.read_encoded_int()
    else:
        if flags & SaveFlag.LocationByteXY:
            r.read_byte(); r.read_byte()
        elif flags & SaveFlag.LocationShortXY:
            r.read_int16(); r.read_int16()
        if flags & SaveFlag.LocationSByteZ:
            r.read_sbyte()

    if flags & SaveFlag.ItemID:
        r.read_encoded_int()   # ItemID — handled by constructor

    if flags & SaveFlag.Hue:
        props["hue"] = r.read_encoded_int()

    if flags & SaveFlag.Amount:
        props["amount"] = r.read_encoded_int()

    if flags & SaveFlag.Layer:
        props["layer"] = r.read_byte()

    if flags & SaveFlag.Name:
        props["name"] = r.read_prefixed_shard_string()

    if flags & SaveFlag.Parent:
        props["parent"] = r.read_int32()

    if flags & SaveFlag.Items:
        count = r.read_int32()
        props["children"] = [r.read_int32() for _ in range(count)]

    # ── Weight ────────────────────────────────────────────────────────────
    if version < 8 or not (flags & SaveFlag.NullWeight):
        if flags & SaveFlag.IntWeight:
            r.read_encoded_int()
        elif flags & SaveFlag.WeightNot1or0:
            r.read_double()
        # WeightIs0 and default (1.0) need no read

    # ── Map (1 byte) ──────────────────────────────────────────────────────
    if flags & SaveFlag.Map:
        r.read_byte()

    # ── Boolean flags (Visible / Movable / Stackable) ─────────────────────
    if flags & SaveFlag.Visible:
        r.read_bool()
    if flags & SaveFlag.Movable:
        r.read_bool()
    if flags & SaveFlag.Stackable:
        r.read_bool()

    # ── Packed impl flags ─────────────────────────────────────────────────
    if flags & SaveFlag.ImplFlags:
        r.read_encoded_int()

    # ── Mobile refs (Insurance / Blessing / HeldBy) ───────────────────────
    if flags & SaveFlag.InsuredFor:
        r.read_int32()
    if flags & SaveFlag.BlessedFor:
        r.read_int32()
    if flags & SaveFlag.HeldBy:
        r.read_int32()

    # ── SavedFlags ────────────────────────────────────────────────────────
    if flags & SaveFlag.SavedFlags:
        r.read_encoded_int()

    return props, r


# ---------------------------------------------------------------------------
# TDB / IDX readers
# ---------------------------------------------------------------------------

def read_tdb(path: str):
    """Returns list of type name strings (index == typeID)."""
    with open(path, 'rb') as f:
        data = f.read()
    r = BinaryReader(data)
    count = r.read_int32()
    return [r.read_string() for _ in range(count)]


def read_idx(path: str):
    """
    Returns list of (typeID, serial, pos, length) tuples.
    IDX entry: int32 typeID, int32 serial, int64 pos, int32 length
    """
    with open(path, 'rb') as f:
        data = f.read()
    r = BinaryReader(data)
    count = r.read_int32()
    entries = []
    for _ in range(count):
        type_id = r.read_int32()
        serial = r.read_int32()
        pos = r.read_int64()
        length = r.read_int32()
        entries.append((type_id, serial, pos, length))
    return entries


# ---------------------------------------------------------------------------
# Account XML parsing
# ---------------------------------------------------------------------------

def find_character_serial(accounts_xml_path: str, account_name: str, character_name: str):
    """
    Parses accounts.xml, finds the account by name, then searches the live world
    for a mobile matching character_name among the account's character serials.
    Returns (character_serial: int, all_char_serials: list[int]).
    """
    tree = ET.parse(accounts_xml_path)
    root = tree.getroot()

    for acct_el in root.iter('account'):
        name_el = acct_el.find('username')
        if name_el is None:
            name_el = acct_el.get('username') and type('_', (), {'text': acct_el.get('username')})()
        username = name_el.text.strip() if name_el is not None and name_el.text else acct_el.get('username', '')

        if username.lower() != account_name.lower():
            continue

        # Found the account — gather character serials
        chars_el = acct_el.find('chars')
        char_serials = []
        if chars_el is not None:
            for char_el in chars_el.findall('char'):
                try:
                    serial_val = int(char_el.text.strip()) if char_el.text else 0
                    char_serials.append(serial_val)
                except (ValueError, AttributeError):
                    pass

        return char_serials

    raise ValueError(f"Account '{account_name}' not found in {accounts_xml_path}")


# ---------------------------------------------------------------------------
# Mobile IDX/BIN scanning to find character name → serial
# ---------------------------------------------------------------------------

def _encode_net_string(s: str) -> bytes:
    """Encode a string as .NET BinaryWriter format: 7-bit length + UTF-8 bytes."""
    encoded = s.encode('utf-8')
    length = len(encoded)
    length_bytes = bytearray()
    while length >= 0x80:
        length_bytes.append((length & 0x7F) | 0x80)
        length >>= 7
    length_bytes.append(length)
    return bytes(length_bytes) + encoded


def _scan_mobile_name(chunk: bytes) -> Optional[str]:
    """
    Heuristic scan for the character name string inside a raw Mobile.Serialize record.
    The name appears around bytes 230-260 as a .NET BinaryWriter-encoded string
    (1-byte length prefix + UTF-8). Returns the first plausible candidate found.
    """
    # Search the region where the name typically lives (bytes 230-280).
    window = chunk[230:280]
    for i in range(len(window) - 1):
        b = window[i]
        if 3 <= b <= 20:  # plausible name length
            candidate = window[i + 1:i + 1 + b]
            if len(candidate) == b and all(32 <= c < 127 for c in candidate):
                name = candidate.decode('ascii', errors='replace')
                # Filter out non-name-like strings (e.g. pure digits, single chars)
                if any(c.isalpha() for c in name):
                    return name
    return None


def find_mobile_by_name(mobiles_idx_path: str, mobiles_bin_path: str, mobiles_tdb_path: str,
                         char_serials: list, character_name: str):
    """
    Scans Mobiles.bin for the character whose stored name best matches character_name.
    Tries exact match first, then case-insensitive, then partial match.
    Falls back to the first serial with a warning listing available names.
    """
    if not char_serials:
        raise ValueError("No character serials found for this account")

    if len(char_serials) == 1:
        return char_serials[0]

    idx = read_idx(mobiles_idx_path)
    serial_to_entry = {serial: (pos, length)
                       for (_, serial, pos, length) in idx
                       if serial in set(char_serials)}

    with open(mobiles_bin_path, 'rb') as f:
        bin_data = f.read()

    # Try .NET BinaryWriter encoded exact match first (most reliable)
    name_pattern = _encode_net_string(character_name)
    for serial in char_serials:
        entry = serial_to_entry.get(serial)
        if entry is None:
            continue
        pos, length = entry
        if pos < 0 or pos + length > len(bin_data):
            continue
        if name_pattern in bin_data[pos:pos + length]:
            print(f"  Matched character '{character_name}' → serial {hex(serial)}", file=sys.stderr)
            return serial

    # Extract stored names for all character serials for fallback reporting
    serial_names: dict = {}
    for serial in char_serials:
        entry = serial_to_entry.get(serial)
        if entry is None:
            continue
        pos, length = entry
        if pos < 0 or pos + length > len(bin_data):
            continue
        chunk = bin_data[pos:pos + length]
        found_name = _scan_mobile_name(chunk)
        if found_name:
            serial_names[serial] = found_name

    # Case-insensitive match
    lower_target = character_name.lower()
    for serial, stored_name in serial_names.items():
        if stored_name.lower() == lower_target:
            print(f"  Matched character '{character_name}' (case-insensitive) → "
                  f"serial {hex(serial)} ('{stored_name}')", file=sys.stderr)
            return serial

    # Partial / contains match
    for serial, stored_name in serial_names.items():
        if lower_target in stored_name.lower() or stored_name.lower() in lower_target:
            print(f"  Partial-matched character '{character_name}' → "
                  f"serial {hex(serial)} (stored name: '{stored_name}')", file=sys.stderr)
            return serial

    name_list = ', '.join(
        f"'{n}' ({hex(s)})" for s, n in serial_names.items()
    ) or "none found"
    print(
        f"  [Warning] Character name '{character_name}' not matched in backup. "
        f"Available names: {name_list}. "
        f"Defaulting to first serial {hex(char_serials[0])}. "
        f"Re-run with --character <name from list> to select the correct character.",
        file=sys.stderr
    )
    return char_serials[0]


# ---------------------------------------------------------------------------
# Main analysis function
# ---------------------------------------------------------------------------

def analyze_backup(backup_path: str, account_name: str, character_name: str):
    """
    Full analysis pipeline. Returns (items list, errors list).
    items: list of dicts with keys: serial, type_full, type_short, hue, amount, name, layer, is_equipped
    """
    errors = []

    # File paths
    accounts_xml = os.path.join(backup_path, "Accounts", "accounts.xml")
    items_tdb    = os.path.join(backup_path, "Items", "Items.tdb")
    items_idx    = os.path.join(backup_path, "Items", "Items.idx")
    items_bin    = os.path.join(backup_path, "Items", "Items.bin")
    mobs_tdb     = os.path.join(backup_path, "Mobiles", "Mobiles.tdb")
    mobs_idx     = os.path.join(backup_path, "Mobiles", "Mobiles.idx")
    mobs_bin     = os.path.join(backup_path, "Mobiles", "Mobiles.bin")

    for p, label in [
        (accounts_xml, "accounts.xml"),
        (items_tdb, "Items.tdb"),
        (items_idx, "Items.idx"),
        (items_bin, "Items.bin"),
    ]:
        if not os.path.exists(p):
            raise FileNotFoundError(f"Required file not found: {p} ({label})")

    # Step 1: Find character serial(s) from accounts.xml
    print(f"  Reading {accounts_xml}...", file=sys.stderr)
    char_serials = find_character_serial(accounts_xml, account_name, character_name)
    if not char_serials:
        raise ValueError(f"No characters found for account '{account_name}'")

    char_serial = char_serials[0]
    if all(os.path.exists(p) for p in [mobs_tdb, mobs_idx, mobs_bin]):
        char_serial = find_mobile_by_name(mobs_idx, mobs_bin, mobs_tdb, char_serials, character_name)

    print(f"  Character serial: {hex(char_serial)}", file=sys.stderr)

    # Step 2: Build item type lookup from Items.tdb + Items.idx
    print(f"  Reading Items.tdb + Items.idx...", file=sys.stderr)
    item_types = read_tdb(items_tdb)
    item_idx_entries = read_idx(items_idx)

    serial_to_entry = {}
    for (type_id, serial, pos, length) in item_idx_entries:
        type_name = item_types[type_id] if 0 <= type_id < len(item_types) else "Unknown"
        serial_to_entry[serial] = {
            "type_full": type_name,
            "type_short": type_name.rsplit('.', 1)[-1] if '.' in type_name else type_name,
            "pos": pos,
            "length": length,
        }

    print(f"  Total items in backup: {len(serial_to_entry)}", file=sys.stderr)

    # Step 3: Scan Items.bin to find parent-child relationships
    print(f"  Scanning Items.bin for parent relationships...", file=sys.stderr)
    with open(items_bin, 'rb') as f:
        bin_data = f.read()

    # parent_map[child_serial] = parent_serial
    parent_map = {}
    # children_map[parent_serial] = list of child serials
    children_map = {}
    # item_props[serial] = {hue, amount, name, layer}
    item_props = {}

    parse_success = 0
    parse_fail = 0

    for serial, entry in serial_to_entry.items():
        pos = entry["pos"]
        length = entry["length"]
        if pos < 0 or pos + length > len(bin_data):
            errors.append(f"Out-of-bounds item 0x{serial:08X}")
            continue

        chunk = bin_data[pos:pos + length]
        try:
            parsed, r_after = parse_item_record(chunk)
            base_props = {k: v for k, v in parsed.items() if k != "children"}
            # Attempt subclass property extraction (weapon/armor/trinket)
            family = _classify_item_family(entry["type_short"])
            if family:
                sub = _parse_subclass(r_after, family)
                base_props.update(sub)
            item_props[serial] = base_props
            if parsed["parent"] is not None:
                p = parsed["parent"]
                parent_map[serial] = p
                if p not in children_map:
                    children_map[p] = []
                children_map[p].append(serial)
            for child_serial in parsed["children"]:
                parent_map[child_serial] = serial
                if serial not in children_map:
                    children_map[serial] = []
                children_map[serial].append(child_serial)
            parse_success += 1
        except Exception as e:
            parse_fail += 1
            # Per-item failures are usually unrelated world items; surface a short sample on stderr only.
            if parse_fail <= 10:
                print(
                    f"  [parse] item 0x{serial:08X} ({entry['type_short']}): {e}",
                    file=sys.stderr,
                )

    print(f"  Parsed {parse_success} items successfully, {parse_fail} failed.", file=sys.stderr)
    if parse_fail > 0:
        errors.append(
            f"{parse_fail} item record(s) in this backup failed to parse (exotic format or truncation); "
            "the manifest lists only items that parsed successfully."
        )

    # Step 4: Find the character's direct items (equipped + backpack)
    direct_items = children_map.get(char_serial, [])

    # Find the backpack serial
    backpack_serial = None
    for s in direct_items:
        props = item_props.get(s, {})
        layer = props.get("layer", 0)
        if layer == 0x15:  # Layer.Backpack
            backpack_serial = s
            break
        # Fallback: check type name
        entry = serial_to_entry.get(s, {})
        if "Backpack" in entry.get("type_short", ""):
            backpack_serial = s
            break

    # Step 5: Build the final item list
    items = []

    # ── Property caps matching BackupSaveAnalyzer constants in CharacterRestoreGump.cs ──
    CAPS = {
        "hue":             3000,
        "amount":          60000,
        "world_item_id":   0xFFFF,
        "coin_price":      10_000_000,
        "enchant_mod":     100,
        "not_id_attempts": 100,
        "enchant_uses":    500,
        "enchant_uses_max":500,
        "limits":          50_000,
        "limits_max":      50_000,
        "graphic_id":      0xFFFF,
        "graphic_hue":     3000,
        "artifact_level":  4,    # max ArtifactLevel enum value
    }

    def cap_value(key: str, value):
        """Return value clamped to the game-defined cap, or None if 0/empty."""
        if value is None or value == 0 or value == "" or value is False:
            return None
        max_val = CAPS.get(key)
        if max_val is not None and isinstance(value, int):
            return min(value, max_val)
        return value

    added_serials = set()

    def add_item(serial, is_equipped):
        if serial in added_serials:
            return
        entry = serial_to_entry.get(serial)
        if entry is None:
            return
        tn = entry.get("type_short", "") or ""
        tf = entry.get("type_full", "") or ""
        if tn == "BankBox" or tf.endswith(".BankBox") or ".BankBox" in tf:
            return
        added_serials.add(serial)
        props = item_props.get(serial, {})
        layer = props.get("layer", 0)

        record = {
            "serial":     hex(serial & 0xFFFFFFFF),
            "type_full":  entry["type_full"],
            "type_short": entry["type_short"],
            "layer":      LAYER_NAMES.get(layer, f"Layer_{layer}"),
            "is_equipped": is_equipped,
            # ── Display-critical fields (always include) ──────────────────
            "hue":    props.get("hue", 0),
            "amount": max(1, props.get("amount", 1)),
            "name":   props.get("name"),
        }

        # ── Full set of base-class properties (include non-default values only) ──
        for key in (
            "enchant_mod", "world_item_id",
            "color_hue1", "color_text1", "color_hue2", "color_text2",
            "color_hue3", "color_text3", "color_hue4", "color_text4",
            "color_hue5", "color_text5",
            "technology", "virtual_container", "not_identified",
            "not_id_attempts", "not_id_source", "not_id_skill", "catalog",
            "coin_price", "resource", "sub_resource", "sub_name",
            "artifact_level", "not_mod_able", "needs_both_hands",
            "info_data", "info_text1", "info_text2", "info_text3", "info_text4", "info_text5",
            "limits", "limits_max", "limits_name", "limits_delete", "built",
            "enchanted_spell", "enchant_uses", "enchant_uses_max",
            "graphic_id", "graphic_hue",
        ):
            raw = props.get(key)
            capped = cap_value(key, raw)
            if capped is not None:
                record[key] = capped
                if isinstance(capped, int) and isinstance(raw, int) and capped != raw:
                    errors.append(
                        f"Item 0x{serial:08X} ({entry['type_short']}): "
                        f"{key}={raw} clamped to {capped}"
                    )

        # Determine item family once for use in all subclass sections below
        item_family = _classify_item_family(entry["type_short"])

        # ── Weapon subclass properties ────────────────────────────────────────
        for key in (
            "quality", "damage_level", "accuracy_level", "durability_level",
            "hits", "max_hits",
            "slayer", "slayer2", "poison_level", "poison_charges",
            "str_req", "dex_req", "int_req",
            "min_damage", "max_damage", "speed", "max_range", "skill",
            "weapon_resource", "engraved_text",
        ):
            v = props.get(key)
            if v is not None and v not in (0, "", "Regular", "None"):
                record[key] = round(v, 2) if isinstance(v, float) else v

        for key in ("attributes", "weapon_attributes", "skill_bonuses", "elemental_damage"):
            v = props.get(key)
            if v:
                record[key] = v

        # ── Armor subclass properties ─────────────────────────────────────────
        for key in (
            "quality", "durability", "protection",
            "hit_points", "max_hit_points",
            "base_armor", "str_bonus", "dex_bonus", "int_bonus",
            "str_req", "dex_req", "int_req",
            "armor_resource",
        ):
            v = props.get(key)
            if v is not None and v not in (0, "Regular"):
                if key not in record:          # don't overwrite weapon quality
                    record[key] = v

        # Always emit all five resist fields for armor items (0 = no bonus)
        if item_family == "armor":
            for key in ("resist_physical", "resist_fire", "resist_cold",
                        "resist_poison", "resist_energy"):
                record[key] = props.get(key, 0)
            # Density — computed from craft resource and armor material type
            res = props.get("armor_resource") or props.get("resource") or 0
            density = _compute_density(res, entry["type_short"])
            if density and density != "None":
                record["density"] = density
        else:
            for key in ("resist_physical", "resist_fire", "resist_cold",
                        "resist_poison", "resist_energy"):
                v = props.get(key)
                if v is not None and v not in (0,):
                    if key not in record:
                        record[key] = v

        for key in ("armor_attributes", "skill_bonuses"):
            v = props.get(key)
            if v and key not in record:
                record[key] = v

        if "attributes" not in record:
            v = props.get("attributes")
            if v:
                record["attributes"] = v

        # ── Trinket subclass properties ───────────────────────────────────────
        for key in ("quality", "hit_points", "max_hit_points", "gem_type"):
            v = props.get(key)
            if v is not None and v not in (0, "Regular"):
                if key not in record:
                    record[key] = v

        for key in ("resistances", "skill_bonuses"):
            v = props.get(key)
            if v and key not in record:
                record[key] = v

        if "attributes" not in record:
            v = props.get("attributes")
            if v:
                record["attributes"] = v

        if "armor_attributes" not in record:
            v = props.get("armor_attributes")
            if v:
                record["armor_attributes"] = v

        # ── Quiver subclass properties ────────────────────────────────────────
        if item_family == "quiver":
            for key in ("lower_ammo_cost", "weight_reduction", "damage_increase", "capacity"):
                v = props.get(key)
                if v is not None and v not in (0,):
                    record[key] = v
            v = props.get("quality")
            if v is not None and v not in (0, "Regular") and "quality" not in record:
                record["quality"] = v
            if "attributes" not in record:
                v = props.get("attributes")
                if v:
                    record["attributes"] = v

        # ── Spellbook subclass properties ─────────────────────────────────────
        if item_family == "spellbook":
            for key in ("slayer", "slayer2", "spell_count"):
                v = props.get(key)
                if v is not None and v not in (0, "None", ""):
                    if key not in record:
                        record[key] = v
            if "attributes" not in record:
                v = props.get("attributes")
                if v:
                    record["attributes"] = v
            for key in ("resistances", "skill_bonuses"):
                v = props.get(key)
                if v and key not in record:
                    record[key] = v

        # ── Runebook subclass properties ──────────────────────────────────────
        if item_family == "runebook":
            for key in ("description", "cur_charges", "max_charges",
                        "default_index", "secure_level"):
                v = props.get(key)
                if v is not None and v not in (0, "", None):
                    record[key] = v
            runes = props.get("rune_entries")
            if runes:
                record["rune_entries"] = runes

        items.append(record)

    # Add equipped items (items directly parented to character, not the backpack)
    for s in direct_items:
        if s == backpack_serial:
            continue
        props = item_props.get(s, {})
        layer = props.get("layer", 0)
        is_equipped = layer in EQUIPPED_LAYERS
        add_item(s, is_equipped)

    # Add backpack contents (recursive one level — nested containers included)
    def add_container_contents(container_serial, depth=0):
        if depth > 5:
            return
        contents = children_map.get(container_serial, [])
        for s in contents:
            add_item(s, False)
            # Recurse into sub-containers (e.g., pouches inside backpack)
            sub_entry = serial_to_entry.get(s, {})
            if any(t in sub_entry.get("type_short", "")
                   for t in ("Bag", "Backpack", "Pouch", "Container", "Box", "Chest", "Sack")):
                add_container_contents(s, depth + 1)

    if backpack_serial is not None:
        add_container_contents(backpack_serial)
    else:
        errors.append("Backpack not found; only equipped items will be listed.")

    if not items and not direct_items:
        errors.append(
            f"No items found directly parented to character serial {hex(char_serial)}. "
            "This may mean the backup was saved while the character was in a different state, "
            "or parent-child parsing failed for critical items."
        )

    return items, char_serial, errors


# ---------------------------------------------------------------------------
# Entry point
# ---------------------------------------------------------------------------

def main():
    parser = argparse.ArgumentParser(
        description="Analyze RunUO/ServUO backup Saves to extract a character's items."
    )
    parser.add_argument("--backup-path", required=True,
                        help="Path to the backup Saves directory (e.g. Saves_backup/)")
    parser.add_argument("--account", required=True,
                        help="Account name (case-insensitive)")
    parser.add_argument("--character", required=True,
                        help="Character name (used for disambiguation when multiple characters exist)")
    parser.add_argument("--output", default=None,
                        help="Output JSON file path. Defaults to tools-output/character-backup-manifest.json")
    args = parser.parse_args()

    if args.output is None:
        output_dir = os.path.join(os.path.dirname(os.path.dirname(os.path.dirname(
            os.path.abspath(__file__)))), "Data", "Localization", "tools-output")
        os.makedirs(output_dir, exist_ok=True)
        args.output = os.path.join(output_dir, "character-backup-manifest.json")

    print(f"Analyzing backup: {args.backup_path}", file=sys.stderr)
    print(f"  Account: {args.account}, Character: {args.character}", file=sys.stderr)

    try:
        items, char_serial, errors = analyze_backup(args.backup_path, args.account, args.character)
    except Exception as e:
        result = {
            "account": args.account,
            "character": args.character,
            "character_serial": "0x0",
            "items": [],
            "errors": [str(e)],
        }
        with open(args.output, 'w', encoding='utf-8') as f:
            json.dump(result, f, indent=2, ensure_ascii=False)
        print(f"Error: {e}", file=sys.stderr)
        sys.exit(1)

    result = {
        "account": args.account,
        "character": args.character,
        "character_serial": hex(char_serial & 0xFFFFFFFF),
        "items": items,
        "errors": errors,
    }

    with open(args.output, 'w', encoding='utf-8') as f:
        json.dump(result, f, indent=2, ensure_ascii=False)

    print(f"\nResult: {len(items)} items written to {args.output}", file=sys.stderr)
    if errors:
        print(f"Warnings/errors ({len(errors)}):", file=sys.stderr)
        for e in errors[:10]:
            print(f"  - {e}", file=sys.stderr)
    print("Done.", file=sys.stderr)


if __name__ == "__main__":
    main()

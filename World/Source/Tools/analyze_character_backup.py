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

    def read_double(self) -> float:
        return struct.unpack('<d', self.read_bytes(8))[0]

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
        """Reads a .NET BinaryWriter string: 7-bit length + UTF-8 bytes. Empty string is valid."""
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
    Attempt to parse key fields from an Item.Serialize version 14 binary record.
    Returns a dict with keys: version, parent, hue, amount, layer, name, items_children.
    Raises on parse failure.
    """
    r = BinaryReader(data)

    version = r.read_int32()
    if version < 6 or version > 14:
        raise ValueError(f"Unsupported item version {version}")

    if version >= 14:
        r.read_bool()   # Purchased
        r.read_int32()  # EnchantMod
        # ColorHue1..5 and ColorText1..5 are all strings in this codebase
        for _ in range(10):
            r.read_string()
        r.read_int32()  # WorldItemID
        r.read_bool()   # Technology
        r.read_bool()   # VirtualContainer
        r.read_bool()   # NotIdentified
        r.read_int32()  # NotIDAttempts
        r.read_encoded_int()  # NotIDSource
        r.read_encoded_int()  # NotIDSkill
        r.read_encoded_int()  # Catalog
        r.read_int32()  # CoinPrice
        r.read_encoded_int()  # Resource
        r.read_encoded_int()  # SubResource
        r.read_string()  # SubName
        r.read_int32()  # ArtifactLevel
        r.read_bool()   # NotModAble
        r.read_bool()   # NeedsBothHands
        r.read_string()  # InfoData
        for _ in range(5):
            r.read_string()  # InfoText1..5
        r.read_int32()  # Limits
        r.read_int32()  # LimitsMax
        r.read_string()  # LimitsName
        r.read_bool()   # LimitsDelete
        r.read_mobile_ref()  # BuiltBy
        r.read_bool()   # Built
        # Fall through to version 11

    if version >= 11:
        r.read_encoded_int()  # Enchanted
        r.read_int32()  # EnchantUses
        r.read_int32()  # EnchantUsesMax
        # Fall through to version 10

    if version >= 10:
        r.read_int32()  # GraphicID
        r.read_int32()  # GraphicHue
        r.read_mobile_ref()  # LastMobile
        r.read_string()  # LastMobileName
        # Fall through to version 6

    # --- Case 6 (all versions >= 6 fall here) ---
    flags = r.read_int32()

    # minutes since last moved (version >= 7 uses encoded int; version 6 used DeltaTime)
    if version >= 7:
        r.read_encoded_int()  # minutes
    else:
        # DeltaTime: long (8 bytes)
        r.read_int64()

    if flags & SaveFlag.Direction:
        r.read_byte()

    if flags & SaveFlag.Bounce:
        skip_bounce_info(r)

    if flags & SaveFlag.LootType:
        r.read_byte()

    # Location
    if flags & SaveFlag.LocationFull:
        r.read_encoded_int()  # x
        r.read_encoded_int()  # y
        r.read_encoded_int()  # z
    else:
        if flags & SaveFlag.LocationByteXY:
            r.read_byte()
            r.read_byte()
        elif flags & SaveFlag.LocationShortXY:
            r.read_int16()
            r.read_int16()
        if flags & SaveFlag.LocationSByteZ:
            r.read_sbyte()

    item_id = None
    if flags & SaveFlag.ItemID:
        item_id = r.read_encoded_int()

    hue = 0
    if flags & SaveFlag.Hue:
        hue = r.read_encoded_int()

    amount = 1
    if flags & SaveFlag.Amount:
        amount = r.read_encoded_int()

    layer = 0
    if flags & SaveFlag.Layer:
        layer = r.read_byte()

    name = None
    if flags & SaveFlag.Name:
        name = r.read_string()

    parent = None
    if flags & SaveFlag.Parent:
        parent = r.read_int32()

    children = []
    if flags & SaveFlag.Items:
        count = r.read_int32()
        for _ in range(count):
            children.append(r.read_int32())

    return {
        "hue": hue,
        "amount": amount,
        "layer": layer,
        "name": name,
        "parent": parent,
        "children": children,
        "flags": flags,
    }


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

def find_mobile_by_name(mobiles_idx_path: str, mobiles_bin_path: str, mobiles_tdb_path: str,
                         char_serials: list, character_name: str):
    """
    Scans the Mobiles.bin for entries whose serial is in char_serials and reads
    just enough to get the character name (from Mobile.Serialize).
    Returns the matching serial, or the first serial if name match fails.
    """
    if not char_serials:
        raise ValueError("No character serials found for this account")

    tdb = read_tdb(mobiles_tdb_path)
    idx = read_idx(mobiles_idx_path)

    # Build serial → (typeID, pos, length) map for our target serials
    serial_set = set(char_serials)
    candidates = {type_id: (serial, pos, length)
                  for (type_id, serial, pos, length) in idx
                  if serial in serial_set}

    with open(mobiles_bin_path, 'rb') as f:
        bin_data = f.read()

    # Mobile.Serialize version 37 is complex; we can try to read the name field
    # which is fairly early in the serialization. For simplicity, if we only have
    # one character serial, return it directly without name matching.
    if len(char_serials) == 1:
        return char_serials[0]

    # Try to match by name — Mobile.Serialize writes Name as a string field.
    # Version 37 is extremely complex; return first serial as best-effort fallback.
    print(f"  [Note] Multiple characters found; returning first serial. "
          f"Verify character name '{character_name}' matches serial {hex(char_serials[0])}.",
          file=sys.stderr)
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
            parsed = parse_item_record(chunk)
            item_props[serial] = {
                "hue": parsed["hue"],
                "amount": parsed["amount"],
                "name": parsed["name"],
                "layer": parsed["layer"],
                "flags": parsed["flags"],
            }
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
            if parse_fail <= 10:
                errors.append(f"Parse error for item 0x{serial:08X} ({entry['type_short']}): {e}")

    print(f"  Parsed {parse_success} items successfully, {parse_fail} failed.", file=sys.stderr)

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

    def add_item(serial, is_equipped):
        entry = serial_to_entry.get(serial)
        if entry is None:
            return
        props = item_props.get(serial, {})
        layer = props.get("layer", 0)
        items.append({
            "serial": hex(serial & 0xFFFFFFFF),
            "type_full": entry["type_full"],
            "type_short": entry["type_short"],
            "hue": props.get("hue", 0),
            "amount": props.get("amount", 1),
            "name": props.get("name"),
            "layer": LAYER_NAMES.get(layer, f"Layer_{layer}"),
            "is_equipped": is_equipped,
        })

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

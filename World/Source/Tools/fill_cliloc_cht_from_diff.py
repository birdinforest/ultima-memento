#!/usr/bin/env python3
"""
Fill empty CliLoc-cht.csv texts using CliLoc-en-cht-diff.csv / cliloc-diff-payload.json.

- Translates EN → zh-Hans (rule + glossaries), then zhconv → zh-Hant for client CHT.
- Preserves CliLoc tokens (~1_...~), HTML tags, and existing CHT Flag column.
- Requires: pip install --target World/Documentation/tools-output/pydeps zhconv
  (PYTHONPATH must include that directory, or install zhconv globally.)

Usage (repo root):
  PYTHONPATH=World/Documentation/tools-output/pydeps \
    python3 World/Source/Tools/fill_cliloc_cht_from_diff.py --dry-run
  PYTHONPATH=... python3 World/Source/Tools/fill_cliloc_cht_from_diff.py --write
"""

from __future__ import annotations

import argparse
import csv
import json
import re
import sys
from pathlib import Path

REPO = Path(__file__).resolve().parents[3]
DOC = REPO / "World" / "Documentation"
DIFF_CSV = DOC / "CliLoc-en-cht-diff.csv"
CHT_CSV = DOC / "CliLoc-cht.csv"
PAYLOAD = DOC / "tools-output" / "cliloc-diff-payload.json"

try:
    import zhconv
except ImportError:
    zhconv = None


def to_traditional(s: str) -> str:
    if zhconv is None:
        print("ERROR: zhconv not found. Set PYTHONPATH to tools-output/pydeps or pip install zhconv.", file=sys.stderr)
        sys.exit(1)
    return zhconv.convert(s, "zh-tw")


def parse_line(line: str) -> tuple[str, str, str] | None:
    line = line.rstrip("\n\r")
    if not line or line.startswith("#") or line.lower().startswith("number;"):
        return None
    first = line.find(";")
    last = line.rfind(";")
    if first < 0 or last <= first:
        return None
    num = line[:first].strip()
    flag = line[last + 1 :].strip()
    text = line[first + 1 : last]
    if not num or not num[0].isdigit():
        return None
    return (num, text, flag)


def spell_inner_en_to_zh() -> dict[str, str]:
    """Magery / Necro spell names for colored BASEFONT lines (inner text only)."""
    return {
        "Clumsy": "笨拙",
        "Create Food": "造食術",
        "Feeble Mind": "弱智術",
        "Heal": "治療術",
        "Magic Arrow": "魔法箭",
        "Night Sight": "夜視術",
        "Reactive Armor": "反應護甲",
        "Weaken": "削弱術",
        "Agility": "敏捷術",
        "Cunning": "精明術",
        "Cure": "解毒術",
        "Harm": "殺傷術",
        "Magic Trap": "魔法陷阱",
        "Remove Trap": "解除陷阱",
        "Protection": "保護術",
        "Strength": "力量術",
        "Bless": "祝福術",
        "Fireball": "火球術",
        "Magic Lock": "魔法鎖",
        "Poison": "毒術",
        "Telekinesis": "心靈遙控",
        "Teleport": "瞬間移動",
        "Unlock": "開鎖術",
        "Wall of Stone": "石墻術",
        "Arch Cure": "強效解毒術",
        "Arch Protection": "強效保護術",
        "Curse": "詛咒術",
        "Fire Field": "火場術",
        "Greater Heal": "強效治療術",
        "Lightning": "落雷術",
        "Mana Drain": "吸魔術",
        "Recall": "記憶傳送",
        "Blade Spirits": "劍靈術",
        "Dispel Field": "驅散力場",
        "Incognito": "隱姓埋名",
        "Magic Reflect": "魔法反射",
        "Mind Blast": "心靈爆震",
        "Paralyze": "麻痺術",
        "Poison Field": "毒場術",
        "Summon Creature": "召喚生物",
        "Dispel": "驅魔術",
        "Energy Bolt": "能量箭",
        "Explosion": "爆炸術",
        "Invisibility": "隱身術",
        "Mark": "標記術",
        "Mass Curse": "集體詛咒",
        "Paralyze Field": "麻痺力場",
        "Reveal": "現形術",
        "Chain Lightning": "連鎖閃電",
        "Energy Field": "能量力場",
        "Flamestrike": "焰擊術",
        "Gate Travel": "裂界之門",
        "Mana Vampire": "魔力吸噬",
        "Mass Dispel": "集體驅魔",
        "Meteor Swarm": "隕石術",
        "Polymorph": "變形術",
        "Earthquake": "地震術",
        "Energy Vortex": "能量漩渦",
        "Resurrection": "復活術",
        "Summon Air Elemental": "召喚風元素",
        "Summon Daemon": "召喚惡魔",
        "Summon Earth Elemental": "召喚土元素",
        "Summon Fire Elemental": "召喚火元素",
        "Summon Water Elemental": "召喚水元素",
        "Summon Snakes": "召喚蛇群",
        "Summon Dragon": "召喚龍",
        "Summon Skeleton": "召喚骷髏",
        "Identify": "鑒定",
        "Curse Weapon": "武器詛咒",
        "Blood Oath": "血之誓約",
        "Corpse Skin": "屍膚術",
        "Evil Omen": "凶兆",
        "Pain Spike": "痛苦尖刺",
        "Wraith Form": "怨靈形態",
        "Mind Rot": "腐心術",
        "Summon Familiar": "召喚使魔",
        "Animate Dead": "操屍術",
        "Horrific Beast": "恐獸形態",
        "Poison Strike": "劇毒打擊",
        "Wither": "凋零術",
        "Strangle": "勒殺術",
        "Lich Form": "巫妖形態",
        "Exorcism": "驅魔儀式",
        "Vengeful Spirit": "復仇亡魂",
        "Vampiric Embrace": "吸血鬼之擁",
    }


def carpet_master_lines() -> dict[str, str]:
    return {
        "You must be on the carpet to open the bag.": "你必須站在飛毯上才能打開袋子。",
        "I can not open the bag while the carpet is moving.": "飛毯移動時我無法打開袋子。",
        "The carpet is already steady, my master.": "主人，飛毯已經停穩了。",
        "The carpet is steadied, my master.": "主人，飛毯已經停穩了。",
        "The carpet has not been steadied, my master.": "主人，飛毯還沒有停穩。",
        "The carpet is ready to glide, my master.": "主人，飛毯已準備滑行。",
        "Yes, my master.": "是的，主人。",
        "What name do you want for this carpet, my master?": "主人，你想將這張飛毯命名為何？",
        "Only my master may name this carpet.": "只有我的主人能為這張飛毯命名。",
        "This carpet now has no name.": "此飛毯現在沒有名字。",
        "This carpet has no name.": "此飛毯沒有名字。",
        "I have no current nav point, my master.": "主人，我目前沒有導航點。",
        "That is no a map, my master.": "主人，那不是地圖啊。",
        "That map has no course on it, my master.": "主人，那張地圖上沒有航線。",
        "You have a map, my master.": "主人，你身上帶著地圖。",
        "I cannot turn, my master": "主人，我無法轉向。",
        "The carpet is not moving, my master.": "主人，飛毯沒有在動。",
        "There is no map, my master.": "主人，沒有地圖。",
        "The map is too far away from me, my master.": "主人，那張地圖離我太遠了。",
        "I do not see the nav point, my master.": "主人，我看不到導航點。",
        "The path is complete, my master.": "主人，路線已經走完了。",
        "We have arrived at nav point ~1_POINT_NUM~ , my master.": "主人，我們已抵達導航點 ~1_POINT_NUM~。",
        "Heading to nav point ~1_POINT_NUM~, my master.": "主人，正在前往導航點 ~1_POINT_NUM~。",
        "Our fate appears to be sealed, my master.": "主人，我們的命運看來已經註定了。",
        "I would advise against that, my master.": "主人，我不建議你這麼做。",
        "This carpet is like new.": "這張飛毯幾乎像新的一樣。",
        "This carpet is slightly worn.": "這張飛毯略有磨損。",
        "This carpet is somewhat worn.": "這張飛毯有些磨損。",
        "This carpet is fairly worn.": "這張飛毯磨損頗重。",
        "This carpet is greatly worn.": "這張飛毯嚴重磨損。",
        "This carpet is in danger of rotting.": "這張飛毯幾乎要腐爛了。",
        "A magic key is now in my safety deposit box.": "魔法鑰匙已放入我的保險箱。",
        "A magic key is now in my backpack.": "魔法鑰匙已放入我的背包。",
        "A magic key is now at my feet.": "魔法鑰匙在我腳邊。",
        "You must have the magic key to roll up the carpet.": "你必須擁有魔法鑰匙才能收起飛毯。",
        "You must steady the carpet to roll it up.": "你必須先讓飛毯停穩才能收起。",
        "You cannot roll up the carpet with beings on board!": "船上還有人或生物時不能收起飛毯！",
        "You cannot roll up the carpet when it is cluttered.": "飛毯上堆滿雜物時無法收起。",
        "Make sure the magic bag is empty, and try again!": "請先清空魔法袋後再試一次！",
        "It appears to have a bitter taste of light poison.": "嘗起來似乎帶有輕微的毒味。",
        "It appears to have a bitter taste of poison.": "嘗起來似乎帶有毒性。",
        "It appears to have a bitter taste of greater poison.": "嘗起來似乎帶有劇烈毒性。",
        "It appears to have a bitter taste of deadly poison.": "嘗起來似乎帶有致命毒性。",
        "It appears to have a bitter taste of lethal poison.": "嘗起來似乎帶有即死剧毒。",
        "the magic lamp of the ~1_SHIP_NAME~": "~1_SHIP_NAME~ 的神燈",
        "This carpet is now called the ~1_NEW_SHIP_NAME~.": "此飛毯現已命名為 ~1_NEW_SHIP_NAME~。",
        "This carpet is the ~1_BOAT_NAME~.": "此飛毯是 ~1_BOAT_NAME~。",
        "My current destination navpoint is nav ~1_NAV_POINT_NUM~.": "我目前的導航目的地為 ~1_NAV_POINT_NUM~。",
        "The carpet has stopped, my master.": "主人，飛毯已停下。",
    }


def work_material_lines() -> dict[str, str]:
    return {
        "You have no idea how to work this material.": "你不知道如何處理這種材料。",
        "You have no idea how to work this metal.": "你不知道如何處理這種金屬。",
        "You have no idea how to work this bone.": "你不知道如何處理這種骨材。",
        "You have no idea how to work this cloth.": "你不知道如何處理這種布料。",
        "You have no idea how to work this stone.": "你不知道如何處理這種石料。",
        "You have no idea how to work this wood.": "你不知道如何處理這種木料。",
        "You have no idea how to work these scales.": "你不知道如何處理這些鱗片。",
        "You have no idea how to work this gemstone.": "你不知道如何處理這種寶石。",
        "!You have no idea how to work this skin.": "",  # placeholder
    }


def label_dictionary() -> dict[str, str]:
    """Short labels: keys are exact English (mixed case)."""
    d: dict[str, str] = {}
    # Skills / UI / misc
    base = {
        "Bald": "禿頭",
        "Leatherworker": "皮匠",
        "Black Market": "黑市",
        "Rent Room": "租屋",
        "Examine": "檢視",
        "Magic": "魔法",
        "Riding": "騎乘",
        "Tribute": "獻金",
        "Setup Shoppe": "設立店鋪",
        "Pets in Stable": "馬廄寵物",
        "Check Map": "查看地圖",
        "Organize": "整理",
        "Enchant": "附魔",
        "Identify": "鑒定",
        "Repair": "修理",
        "Recharge": "充能",
        "Use Mercantile": "使用商賈",
        "Use Arms Lore": "使用兵器學識",
        "Use Tasting": "使用鑑味",
        "Leather": "皮革",
        "Deep Sea Leather": "深海皮革",
        "Lizard Leather": "蜥蜴皮",
        "Serpent Leather": "蛇皮",
        "Necrotic Leather": "死靈皮革",
        "Volcanic Leather": "火山皮革",
        "Frozen Leather": "凍原皮革",
        "Goliath Leather": "巨獸皮革",
        "Draconic Leather": "龍族皮革",
        "Hellish Leather": "地獄皮革",
        "Dinosaur Leather": "恐龍皮革",
        "Alien Leather": "異星皮革",
        "MAKE": "製作",
        "Witch": "女巫",
        "DRUIDIC": "德魯伊",
        "BONECRAFTING": "骨製",
        "Star Sapphire Setting": "星藍寶石座",
        "Emerald Setting": "祖母綠座",
        "Sapphire Setting": "藍寶石座",
        "Ruby Setting": "紅寶石座",
        "Citrine Setting": "黃水晶座",
        "Amethyst Setting": "紫水晶座",
        "Tourmaline Setting": "碧璽座",
        "Amber Setting": "琥珀座",
        "Diamond Setting": "鑽石座",
        "Pearl Setting": "珍珠座",
        "Wagon": "馬車",
        "Death Knight": "死亡騎士",
        "Grim Reaper": "死神使者",
        "Shield of Hate": "憎恨之盾",
        "Orb of Orcus": "奧卡斯之球",
        "Strength of Steel": "鋼鐵之力",
        "Succubus Skin": "魅魔皮膚",
        "Empty Jars": "空瓶罐",
        "ceramic mug": "陶瓷杯",
        "pewter mug": "白鑞杯",
        "skull mug": "顱骨杯",
        "goblet": "高腳杯",
        "Dinosaur Scales": "恐龍鱗",
        "Metallic Scales": "金屬鱗",
        "Brazen Scales": "黃銅色鱗",
        "Umber Scales": "赭色鱗",
        "Violet Scales": "紫色鱗",
        "Platinum Scales": "白金鱗",
        "Cadalyte Scales": "鉆萊特鱗",
        "Spyglass": "望遠鏡",
        "Confidence": "自信",
        "Counter Attack": "反擊",
        "Evasion": "閃避",
        "Honorable Execution": "榮譽斬殺",
        "Suppressed": "壓制",
        "Stat gain is on cooldown": "屬性獲得尚在冷卻",
        "No Dex Gain": "無法提升敏捷",
        "No Int Gain": "無法提升智力",
        "No Str Gain": "無法提升力量",
        "Owner & Co-Owners": "屋主與共同屋主",
    }
    d.update(base)

    # Decanters
    for drink, zh in (
        ("decanter of ale", "麥酒細頸瓶"),
        ("decanter of cider", "蘋果酒細頸瓶"),
        ("decanter of liquor", "烈酒細頸瓶"),
        ("decanter of milk", "牛乳細頸瓶"),
        ("decanter of wine", "葡萄酒細頸瓶"),
        ("decanter of water", "清水細頸瓶"),
        ("decanter", "細頸瓶"),
    ):
        d[drink] = zh

    # House / deed templates
    house = {
        "Castle Tower": "城堡高塔",
        "Pyramid": "金字塔",
        "Large Tent": "大帳篷",
        "Fortress": "要塞",
        "Log Mansion": "原木大宅",
        "deed to a pyramid": "一紙金字塔地契",
        "deed to a large tent": "一紙大帳篷地契",
        "deed to a castle tower": "一紙城堡高塔地契",
        "deed to a fortress": "一紙要塞地契",
        "Two Story Sandstone House": "兩層砂岩屋",
        "Brick House With Steeple": "帶尖塔的磚屋",
        "Two Story Brick House": "兩層磚屋",
        "Plaster House Picture Window": "帶景觀窗的灰泥屋",
        "Two Story Brick Home": "兩層磚造住宅",
        "Two Story Wooden Home With Porch": "兩層帶門廊木屋",
        "Small Stone Shoppe": "小型石造店鋪",
        "Wooden Home Porch": "木屋門廊",
        "Small Tower Of Stone": "小型石塔",
        "Three Story Stone Villa": "三層石造別墅",
        "Two Story Small Stone Home": "兩層小型石宅",
        "Two Story Small Stone House": "兩層小型石屋",
        "Two Story Small Stone Dwelling": "兩層小型石砌居所",
        "Two Story Small Wooden Dwelling": "兩層小型木造居所",
        "Wooden Mansion": "木造大宅",
        "Small Stone Store Front": "小型石造店面",
        "Small Stone Home": "小型石宅",
        "Fancy Stone Wood Home": "精雕石木住宅",
        "Fancy Wooden Stone House": "精雕木石房屋",
        "Small Stone House": "小型石屋",
        "Small Wooden Shack Porch": "帶門廊小木屋",
        "Plain Plaster House": "朴素灰泥屋",
        "Plain Stone House": "朴素石屋",
        "Plaster Home With Dirt Deck": "帶泥地露臺的灰泥屋",
        "Wooden Home Upper Deck": "木屋上層露臺",
        "Two Story Stone Villa": "兩層石造別墅",
        "Two Story Small Plaster Dwelling": "兩層小型灰泥居所",
        "Small Stone Temple": "小型石廟",
        "Small Sandstone Workshop": "小型砂岩工坊",
        "Stone Home With Enclosed Patio": "帶封閉庭院的石宅",
        "Log Home": "木屋",
        "Small Log Cabin With Deck": "帶露臺的小木屋",
        "Raised Brick Home": "高架磚造住宅",
        "Brick Arena": "磚造競技場",
        "Stone Fort": "石造碉堡",
        "Old Stone Home And Shoppe": "古老石造住宅與店鋪",
        "Small Brick Castle": "小型磚造城堡",
        "Small Wizard Tower": "小型巫師塔",
        "Brick Home With Front Deck": "前露臺磚屋",
        "Marble Shoppe": "大理石店鋪",
        "Brick Home With Large Porch": "大門廊磚屋",
        "Log Cabin": "原木小屋",
    }
    d.update(house)
    return d


def translate_basefont(
    en: str, spell_map: dict[str, str], setting_inner: dict[str, str]
) -> str | None:
    m = re.match(r"^<BASEFONT COLOR=#([0-9A-Fa-f]{6})>(.*)</BASEFONT>$", en, re.DOTALL)
    if not m:
        m2 = re.match(
            r"^<BASEFONT COLOR=#([0-9A-Fa-f]{6})>\s*~1_VAL~\s*Type Material\s*</BASEFONT>$",
            en,
            re.IGNORECASE,
        )
        if m2:
            return f'<BASEFONT COLOR=#{m2.group(1)}> ~1_VAL~ 種材料 </BASEFONT>'
        return None
    color, inner = m.group(1), m.group(2).strip()
    if re.match(r"^~1_VAL~\s*Type Material\s*$", inner, re.IGNORECASE):
        return f"<BASEFONT COLOR=#{color}> ~1_VAL~ 種材料 </BASEFONT>"
    if inner in setting_inner:
        return f"<BASEFONT COLOR=#{color}>{setting_inner[inner]}</BASEFONT>"
    # charges lines
    if "Charges Per Use" in inner:
        return f'<BASEFONT COLOR=#{color}>每次消耗 ~1_val~ 次使用次數</BASEFONT>'
    if re.match(r"^Charges: ~1_val~/~2_val~$", inner):
        return f'<BASEFONT COLOR=#{color}>次數：~1_val~/~2_val~</BASEFONT>'
    if re.match(r"^Charges: ~1_val~$", inner):
        return f'<BASEFONT COLOR=#{color}>次數：~1_val~</BASEFONT>'
    if inner in spell_map:
        zh = spell_map[inner]
        return f"<BASEFONT COLOR=#{color}>{zh}</BASEFONT>"
    # vendor appraise
    appraise = re.match(r"^(.*) Can Appraise$", inner)
    if appraise:
        role = appraise.group(1)
        role_zh = {
            "Bowyer": "弓匠",
            "Armorer": "軍械師",
            "Sage": "賢者",
            "Scribe": "書記",
            "Tailor": "裁縫",
            "Banker": "銀行家",
            "Jeweler": "珠寶匠",
            "Leatherworker": "皮匠",
            "Bard": "吟遊詩人",
            "Alchemist": "煉金師",
            "Herbalist": "草藥師",
            "Mage": "法師",
            "Weaponsmith": "武器師",
            "Woodworker": "木工",
            "Merchant": "商人",
        }.get(role, role)
        return f"<BASEFONT COLOR=#{color}>{role_zh}可鑒定</BASEFONT>"
    if inner.startswith("Or Use Your ") and inner.endswith(" Skill"):
        sk = inner[len("Or Use Your ") : -len(" Skill")]
        sk_zh = {"Mercantile": "商賈", "Arms Lore": "兵器學識", "Tasting": "鑑味"}.get(sk, sk)
        return f"<BASEFONT COLOR=#{color}>或使用你的{sk_zh}技能</BASEFONT>"
    # legend / artefact
    if inner == "Artifact":
        return f'<BASEFONT COLOR=#{color}>神兵</BASEFONT>'
    if inner == "Artefact":
        return f'<BASEFONT COLOR=#{color}>古物</BASEFONT>'
    if inner == "Legendary Artefact":
        return f'<BASEFONT COLOR=#{color}>傳奇古物</BASEFONT>'
    return None


def translate_center(en: str) -> str | None:
    m = re.match(r"^<CENTER>(.*)</CENTER>$", en)
    if not m:
        return None
    inner = m.group(1).strip()
    zh = {
        "WITCH BREWING MENU": "女巫釀造選單",
        "DRUIDIC HERBALISM MENU": "德魯伊草藥選單",
        "BONECRAFTING MENU": "骨製工藝選單",
    }.get(inner)
    if zh:
        return f"<CENTER>{zh}</CENTER>"
    return None


def gem_setting_inner() -> dict[str, str]:
    return {
        "Star Sapphire Setting": "星藍寶石座",
        "Emerald Setting": "祖母綠座",
        "Sapphire Setting": "藍寶石座",
        "Ruby Setting": "紅寶石座",
        "Citrine Setting": "黃水晶座",
        "Amethyst Setting": "紫水晶座",
        "Tourmaline Setting": "碧璽座",
        "Amber Setting": "琥珀座",
        "Diamond Setting": "鑽石座",
        "Pearl Setting": "珍珠座",
    }


def translate_en_to_zh_hans(
    en: str,
    num: str,
    spell_map: dict[str, str],
    carpet: dict[str, str],
    work_mat: dict[str, str],
    labels: dict[str, str],
    setting_inner: dict[str, str],
) -> str:
    if not en.strip():
        return en
    direct = labels.get(en)
    if direct:
        return direct
    c = translate_center(en)
    if c:
        return c
    b = translate_basefont(en, spell_map, setting_inner)
    if b:
        return b
    if en in carpet:
        return carpet[en]
    if en in work_mat:
        return work_mat[en]
    if en == "You have no idea how to work this skin.":
        return "你不知道如何處理這種皮革。"
    if en == "You have no idea how to work this fabric.":
        return "你不知道如何處理這種織物。"

    # Menus / misc messages
    misc = {
        "You have use the cauldron too much and the metal corroded!": "大鍋使用過度，金屬已腐蝕！",
        "The cauldron must be in your pack to use.": "大鍋必須在背包中才能使用。",
        "You pour the potion into a jar...": "你將藥水倒入瓶中……",
        "You do not have enough reagents to make that.": "材料不足以製作該物。",
        "You need an empty jar to make a potion.": "製作藥水需要一個空瓶。",
        "There is not enough material here to break this down.": "這裡的材料不足，無法分解。",
        "You can only use this at the Enchanted Spinning Wheel.": "僅能於附魔紡車上使用。",
        "You can only use this at the Golden Alchemist.": "僅能於黃金煉金台前使用。",
        "You can only use this at the Dragon Head Forge.": "僅能於龍首鍛爐旁使用。",
        "You break the item down into ordinary resources.": "你把物品拆解成普通資源。",
        "You have no idea how to break this item down.": "你不知如何拆解該物。",
        "You cannot work these strange and unusual scales.": "你無法處理這些怪異罕見的鱗片。",
        "Thou art a criminal and cannot rent a room.": "你是罪犯，無法租屋。",
        "You seem to be struggling with your bedroll.": "看來你在睡袋上有些力不從心。",
        "You must be a Journeyman or higher Tinker to construct a golem.": "你必須是工匠（Tinker）熟手或以上才能製作魔像。",
        "The treasure chest is very close!": "寶箱已經非常近了！",
        "~1_NOTHING~": "~1_NOTHING~",
        "You are not skilled enough in tailoring to use these tools.": "你的裁縫技能不足以使用這些工具。",
        "You are not skilled enough in blacksmithing to use these tools.": "你的鍛造技能不足以使用這些工具。",
        "You are not skilled enough in alchemy  to use these tools.": "你的煉金技能不足以使用這些工具。",
    }
    if en in misc:
        return misc[en]

    m_amt = re.match(
        r"^~1_AMT~ (strength|intelligence|dexterity|energy resistance|fire resistance|cold resistance|physical resistance|poison resistance)\.$",
        en,
    )
    if m_amt:
        tail = m_amt.group(1)
        tmap = {
            "strength": "力量",
            "intelligence": "智力",
            "dexterity": "敏捷",
            "energy resistance": "能量抗性",
            "fire resistance": "火焰抗性",
            "cold resistance": "寒冷抗性",
            "physical resistance": "物理抗性",
            "poison resistance": "毒素抗性",
        }
        return f"~1_AMT~ {tmap[tail]}。"

    # Poison / carpet wear already in carpet

    # Token-only templates (preserve tokens)
    if en == "Equipment: ~1_val~":
        return "裝備：~1_val~"
    if en.startswith("METALLIC ("):
        return en.replace("METALLIC", "金屬鱗").replace("DINOSAUR", "恐龍").replace("BRAZEN", "黃銅").replace("UMBER", "赭").replace("VIOLET", "紫").replace("PLATINUM", "白金").replace("CADALYTE", "鉆萊特")

    def repl_gem(m):
        return m.group(0)  # handled below

    # Density lines
    density = {
        "Density: Weak": "密度：弱",
        "Density: Regular": "密度：一般",
        "Density: Great": "密度：良好",
        "Density: Greater": "密度：優良",
        "Density: Superior": "密度：卓越",
        "Density: Ultimate": "密度：頂尖",
    }
    if en in density:
        return density[en]

    # Special format with <br>
    if "<br>" in en.lower():
        if en.startswith("20% life drain."):
            return (
                "20% 吸血。<br>+15 精力恢復。<br>+3 魔力恢復。<br>對多數毒素具抗性。<br>無法使用解毒藥水。<br>大蒜會對你造成傷害。<br>-25% 火焰抗性。"
            )
        if en.startswith("+13 Mana Regeneration."):
            return "+13 魔力恢復。<br>-5 生命恢復。<br>-10% 火焰抗性。<br>+10% 毒素抗性。<br>+10% 寒冷抗性。"
        if en.startswith("+20 Hit Point Regeneration"):
            return "+20 生命恢復<br>+25% 近戰傷害<br>無法施放其他法術"

        def repl_stat(m):
            return m.group(0)

        if "strength.<br>" in en.lower() and "sanctify" not in en.lower().replace(" ", ""):
            return en.replace("strength", "力量").replace("dexterity", "敏捷").replace("intelligence", "智力").replace("parry", "格擋").replace("tactics", "戰術").replace("anatomy", "解剖")
        if "Orb of Trap Removal" in en:
            return "陷阱移除法球"
        if "Avoiding Traps" in en:
            return en.replace("Avoiding Traps on Walls & Floors.", "避開牆面與地面陷阱。").replace("Orb of Trap Removal", "陷阱移除法球")

    # Band buffs / ability blurbs (long section) — use pattern translation file
    z = translate_ability_blurb(en)
    if z:
        return z

    z2 = translate_material_tokens(en)
    if z2:
        return z2

    # Fallback: keep English (log in dry-run)
    return f"__UNTRANSLATED__:{en}"


def translate_material_tokens(en: str) -> str | None:
    """Handles GEM (~1~) style and many ALLCAPS resource lines."""
    m = re.match(r"^([A-Z][A-Z0-9' ]*?) \(~1_AMT~\)$", en)
    if m:
        name = m.group(1).strip()
        mapn = {
            "DINOSAUR": "恐龍",
            "METALLIC": "金屬鱗",
            "BRAZEN": "黃銅色",
            "UMBER": "赭色",
            "VIOLET": "紫色",
            "PLATINUM": "白金",
            "CADALYTE": "鉆萊特",
            "AMETHYST": "紫水晶",
            "EMERALD": "祖母綠",
            "GARNET": "石榴石",
            "ICE": "冰",
            "JADE": "翡翠",
            "MARBLE": "大理石",
            "ONYX": "縞瑪瑙",
            "QUARTZ": "石英",
            "RUBY": "紅寶石",
            "SAPPHIRE": "藍寶石",
            "SILVER": "銀",
            "SPINEL": "尖晶石",
            "STAR RUBY": "星形紅寶石",
            "TOPAZ": "黃玉",
            "CADDELLITE": "卡德莱特",
            "DEMON": "惡魔",
            "DRAGON": "龍",
            "NIGHTMARE": "夢魘",
            "SNAKE": "蛇",
            "TROLL": "巨魔",
            "UNICORN": "獨角獸",
            "ICY": "寒冰",
            "LAVA": "熔岩",
            "SEAWEED": "海草",
            "DEAD": "亡灵",
            "FIRE": "火焰",
            "COLD": "寒冰",
            "VENOM": "毒液",
            "ENERGY": "能量",
            "EXODUS": "脫離",
            "CLOTH": "布料",
            "FURRY": "毛皮",
            "WOOLY": "羊毛",
            "SILK": "絲綢",
            "HAUNTED": "幽影",
            "ARCTIC": "極地",
            "PYRE": "烈焰",
            "VENOMOUS": "劇毒",
            "MYSTERIOUS": "神秘",
            "VILE": "邪惡",
            "DIVINE": "聖洁",
            "FIENDISH": "魔性",
            "BRITTLE": "易碎",
            "DRACO": "龍裔",
            "DROW": "黑暗精靈",
            "ORC": "獸人",
            "REPTILE": "爬行",
            "OGRE": "食人魔",
            "TROLL": "巨魔",
            "GARGOYLE": "石像鬼",
            "MINOTAUR": "牛頭怪",
            "LYCAN": "狼人",
            "SHARK": "鯊魚",
            "COLOSSAL": "巨像",
            "MYSTICAL": "秘法",
            "VAMPIRE": "吸血鬼",
            "LICH": "巫妖",
            "SPHINX": "斯芬克斯",
            "DEVIL": "魔鬼",
            "XENO": "異種",
            "ANDORIAN": "安卓里安",
            "CARDASSIAN": "卡達西",
            "MARTIAN": "火星",
            "RODIAN": "羅迪安",
            "TUSKEN": "塔斯肯",
            "TWI'LEK": "緹萊克",
            "XINDI": "辛迪",
            "ZABRAK": "札布拉克",
            "ADESOTE": "阿德索特",
            "BIOMESH": "仿生網",
            "CERLIN": "瑟林",
            "DURAFIBER": "硬纖維",
            "FLEXICRIS": "弗謝克里",
            "HYPERCLOTH": "超纖布",
            "NYLAR": "耐拉",
            "NYLONITE": "尼龍岩",
            "POLYFIBER": "聚合纖維",
            "SYNCLOTH": "同調布",
            "THERMOWEAVE": "熱織",
            "AGRINIUM": "農金",
            "BESKAR": "貝斯卡",
            "CARBONITE": "碳鋼岩",
            "CORTOSIS": "科托西斯",
            "DURASTEEL": "硬鋼",
            "DURITE": "杜里特",
            "FARIUM": "法理姆",
            "LAMINASTEEL": "層壓鋼",
            "NEURANIUM": "神經合金",
            "PHRIK": "弗里克",
            "PROMETHIUM": "钷合金",
            "QUADRANIUM": "四元合金",
            "SONGSTEEL": "歌鋼",
            "TITANIUM": "鈦",
            "TRIMANTIUM": "三合金",
            "XONOLITE": "索諾萊特",
            "BORL": "波爾木",
            "COSIAN": "科西安木",
            "GREEL": "格里爾木",
            "JAPOR": "賈波爾木",
            "KYSHYYYK": "卡希克木",
            "LAROON": "拉倫木",
            "TEEJ": "提吉木",
            "VESHOK": "維肖克木",
            "GORN": "戈恩",
            "TRANDOSHAN": "特蘭多沙",
            "SILURIAN": "志留",
            "KRAYT": "鯊齒龍鱗",
            "TURTLE": "龜甲",
            "SPECTRAL": "幽靈",
            "DREAD": "恐懼",
            "GHOULISH": "屍鬼",
            "WYRM": "古龍",
            "HOLY": "聖",
            "BLOODLESS": "無血",
            "GILDED": "鎏金",
            "DEMILICH": "半巫妖",
            "WINTRY": "寒冬",
            "GOLDEN": "黃金",
        }
        zh = mapn.get(name, name)
        return f"{zh}（~1_AMT~）"

    # Skin / blocks / lower-case materials: handled by label_dictionary expansion — skip here

    return None


def translate_ability_blurb(en: str) -> str | None:
    """Status / buff one-liners from the large 10634xx block."""
    table = label_dictionary()
    if en in table:
        return table[en]
    # Cheetah / run
    m = {
        "Cheetah Paws": "獵豹之足",
        "Run much faster.": "奔跑速度快上許多。",
        "Deception": "欺瞞",
        "You are disguised.": "你正披著偽裝。",
        "Poisoned": "中毒",
        "Health is diminishing.": "生命值正在流失。",
        "Eyes of the Dead": "亡者之眼",
        "See better in darkness.": "在黑暗中看得更清楚。",
        "Spectre Shadow": "怨靈之影",
        "Invisible to others.": "他人無法看見你。",
        "Ghostly Image": "幽靈影像",
        "Illusionary spirit of yourself.": "你自身的幻影分身。",
        "Drain Life": "吸血",
        "Draining 10-15 health from another.": "從他人身上吸取10–15點生命。",
        "Losing 10-15 health to a Syth.": "被西斯（Syth）吸取10–15點生命。",
        "Projection": "投影",
        "Projection of yourself to distract others.": "製造投影來分散敵人注意。",
        "Speed": "疾速",
        "Absorption": "吸收",
        "Reflect some magic back at the caster.": "將部分魔法反射回施法者。",
        "Astral Projection": "星體投射",
        "Soul is immune to harm.": "靈魂免疫傷害。",
        "PsychicWall": "靈能牆",
        "Wind Runner": "御風者",
        "Insult": "侮辱",
        "Feelings are hurt and mana drains.": "心意受創且魔力流失。",
        "Hilarity": "滑稽",
        "Frozen in laughter.": "笑得無法動彈。",
        "Celerity": "迅捷",
        "Deflection": "偏斜",
        "Psychic Aura": "靈能光環",
        "Elemental Armor": "元素護甲",
        "Stasis Field": "靜滯力場",
        "Paralyzed in a stasis field.": "被靜滯力場麻痺。",
        "Mirage": "幻象",
        "Mirage of yourself to distract others.": "製造幻象來分散敵人注意。",
        "Hammer of Faith": "信仰之錘",
        "Hammer summoned by the gods.": "諸神召喚的圣錘。",
        "Sacred Boon": "神圣恩賜",
        "Healing much quicker.": "傷勢恢復更快。",
        "Sanctify": "聖化",
        "Seance": "通靈會",
        "Spirit is immune from harm.": "靈體免受傷害。",
        "Trial by Fire": "烈火試煉",
        "Enchant": "附魔",
        "Weapon imbued with holy powers.": "武器灌注了聖力。",
        "Blend With Forest": "林間匿跡",
        "Blended seamlessly with the forest.": "與森林融為一體。",
        "Grim Reaper": "死神使者",
        "Enemy is marked by the Grim Reaper, where damage is increased to them but you are more vulnerable from others.": "你被死神印記，對該敵人造成更高傷害，但自身也更易受到其他人攻擊。",
        "Grasping Roots": "攫根",
        "Entangled by roots.": "被樹根緊緊纏住。",
        "Woodland Protection": "林地守護",
        "Orb of Orcus": "奧卡斯之球",
        "Shield of Hate": "憎恨之盾",
        "Hatred shields you from physical harm.": "憎恨護佑你免受物理傷害。",
        "Soul Reaper": "奪魂者",
        "Soul is draining as well as your mana.": "靈魂與魔力同時被抽干。",
        "Strength of Steel": "鋼鐵之力",
        "Succubus Skin": "魅魔皮膚",
        "Health regenerates over time.": "生命會隨時間恢復。",
        "Army's Paeon": "軍旅讚歌",
        "Your health is regenerating better.": "生命恢復更佳。",
        "Enchanting Etude": "迷人的練習曲",
        "Energy Carol": "能量頌歌",
        "Energy Threnody": "能量輓歌",
        "Fire Carol": "烈焰頌歌",
        "Fire Threnody": "烈焰輓歌",
        "Ice Carol": "寒冰頌歌",
        "Ice Threnody": "寒冰輓歌",
        "Knight's Minne": "騎士小步舞曲",
        "Mage's Ballad": "法師歌謠",
        "Your mana is regenerating better.": "魔力恢復更佳。",
        "Poison Carol": "劇毒頌歌",
        "Poison Threnody": "劇毒輓歌",
        "Shepherd's Dance": "牧者之舞",
        "Sinewy Etude": "筋骨練習曲",
        "Agility Potion": "敏捷藥水",
        "Greater Agility Potion": "強效敏捷藥水",
        "Strength Potion": "力量藥水",
        "Greater Strength Potion": "強效力量藥水",
        "Nightsight Potion": "夜視藥水",
        "Lesser Invisibility Potion": "弱效隱形藥水",
        "Invisibility Potion": "隱形藥水",
        "Greater Invisibility Potion": "強效隱形藥水",
        "Superior Potion": "極效藥水",
        "You can see in darkness.": "你能於黑暗中視物。",
        "Invulnerability Potion": "無敵藥水",
        "You cannot come to harm.": "你暫時不會受到傷害。",
        "Your hiding and stealth are enhanced.": "你的潛藏與匿蹤能力提升。",
        "Consecrate Weapon": "聖化武器",
        "Your weapon is consecrated.": "你的武器已被聖化。",
        "Wraith Form": "怨靈形態",
        "Vampiric Embrace": "吸血鬼之擁",
        "Lich Form": "巫妖形態",
        "Horrific Beast": "恐獸形態",
        "Cursed Weapon": "詛咒武器",
        "Weapon heals you with 50% of damage dealt.": "武器將造成傷害的50%轉化為治療。",
        "Magic Reflection": "魔法反射",
        "Polymorphed": "變形",
        "Appearance of another creature.": "外觀變成其他生物。",
        "Paralyzed": "麻痺",
        "You cannot move.": "你無法移動。",
        "Orb of Trap Removal": "陷阱移除法球",
        "Mass Curse": "集體詛咒",
        "Resurrection": "復活",
        "You possess something that will resurrect you after death.": "你持有能在死後復活你的物品。",
        "Elemental Hold": "元素束縛",
        "Elemental Protection": "元素防護",
        "Elemental Echo": "元素回音",
        "Air Walk": "馭風而行",
        "Hover over floor traps and harmful liquids.": "飄過地面陷阱與有害液體。",
        "Confusion Blast": "混亂爆震",
        "You are confused and cannot move.": "你陷入混亂無法移動。",
        "Enchanted Weapon": "附魔武器",
        "Your weapon damage is increased.": "你的武器傷害提高。",
        "Endure Cold": "禦寒",
        "Endure Heat": "耐熱",
        "Mask of Death": "死神面具",
        "Supernatural creatures ignore you.": "超自然生物會忽略你。",
        "Mass Might": "集體神力",
        "Sleep": "沉睡",
        "You are asleep.": "你陷入沉睡。",
        "Sleep Field": "沉睡力場",
        "Mass Sleep": "集體沉睡",
        "Rock Flesh": "岩膚",
        "90% physical resistance.": "90% 物理抗性。",
        "Sneak": "潛行",
        "Withstand Death": "抵禦死亡",
        "You have a shard that will fully heal you if you perish.": "你持有碎晶，死亡時將使你完全恢復。",
        "Gem of Immortality": "不朽寶石",
        "You have a gem that will fully restore you if you perish.": "你持有寶石，死亡時將使你完全恢復。",
        "Intervention": "神佑介入",
        "Discordance": "不協和音",
        "Peaced": "安撫",
        "You are at peace and wish not to fight.": "你心平氣和，不欲戰鬥。",
        "Begged": "懇求",
        "You were begged not to fight.": "你被勸止，不再想戰鬥。",
        "Fireflies": "萤火蟲",
        "Surrounded by fireflies is causing you not to fight.": "身周萤火使你無心戰鬥。",
        "Bandage": "包扎",
        "When your bandage is wrapped.": "當包扎完成時。",
        "Strength Fish": "力量魚",
        "+~1_AMT~ Strength.": "+~1_AMT~ 力量。",
        "Agility Fish": "敏捷魚",
        "+~1_AMT~ Dexterity.": "+~1_AMT~ 敏捷。",
        "Intellect Fish": "智力魚",
        "+~1_AMT~ Intelligence.": "+~1_AMT~ 智力。",
        "Strength Zap": "力量電殛",
        "~1_AMT~ Strength.": "~1_AMT~ 力量。",
        "Dexterity Zap": "敏捷電殛",
        "~1_AMT~ Dexterity.": "~1_AMT~ 敏捷。",
        "Intelligence Zap": "智力電殛",
        "~1_AMT~ Intelligence.": "~1_AMT~ 智力。",
        "You are too confused to move.": "你混亂過度無法移動。",
        "Charmed": "魅惑",
        "You do not feel like moving.": "你提不起勁移動。",
        "Fear": "恐懼",
        "You are too frightened to move.": "你嚇得無法移動。",
        "You are in a defensive stance.": "你採取防禦姿態。",
        "You are in an evasive stance.": "你採取閃避姿態。",
        "Ready for a killing blow.": "準備施展致命一擊。",
        "Your skills are reduced.": "你的技能被降低。",
        "Amethyst": "紫水晶",
        "Amethyst Blocks": "紫水晶塊",
        "amethyst": "紫水晶",
        "Blocks": "塊材",
        "Skins": "皮革",
        "+~1_AMT~ Tracking.": "+~1_AMT~ 追蹤。",
    }
    if en in m:
        return m[en]

    # Numeric templates with ~1_AMT~%
    if "Avoiding Traps" in en or "~1_AMT~% Avoiding" in en:
        return "~1_AMT~% 避開牆面與地面陷阱。"
    if en.startswith("~1_AMT~") and "<br>" in en:
        # resistance templates
        def rep(t):
            t = t.replace("strength", "力量").replace("dexterity", "敏捷").replace("intelligence", "智力")
            t = t.replace("energy resistance", "能量抗性").replace("fire resistance", "火焰抗性")
            t = t.replace("cold resistance", "寒冷抗性").replace("poison resistance", "毒素抗性")
            t = t.replace("physical resistance", "物理抗性")
            return t

        return rep(en)
    if "Reflect some magic back at the caster." in en and "<br>" in en:
        return (
            en.replace("Reflect some magic back at the caster.", "將部分魔法反射回施法者。")
            .replace("physical resist", "物理抗性")
            .replace("fire resist", "火焰抗性")
            .replace("cold resist", "寒冷抗性")
            .replace("poison resist", "毒素抗性")
            .replace("energy resist", "能量抗性")
        )

    # Intervention style line
    if "physical resist" in en and "~" in en:
        return translate_material_tokens(en) or None

    return None


def augment_label_dictionary() -> dict[str, str]:
    d = label_dictionary()
    # gemstone / wood / metals single lines — batch from unique list via script logic
    extras = {}
    gem = ["Amethyst", "Emerald", "Garnet", "Ice", "Jade", "Marble", "Onyx", "Quartz", "Ruby", "Sapphire", "Silver", "Spinel", "Star Ruby", "Topaz", "Caddellite"]
    for g in gem:
        extras[g] = d.get(g, {"Amethyst": "紫水晶", "Emerald": "祖母綠", "Garnet": "石榴石", "Ice": "冰", "Jade": "翡翠", "Marble": "大理石", "Onyx": "縞瑪瑙", "Quartz": "石英", "Ruby": "紅寶石", "Sapphire": "藍寶石", "Silver": "銀", "Spinel": "尖晶石", "Star Ruby": "星形紅寶石", "Topaz": "黃玉", "Caddellite": "卡德莱特"}[g])
        extras[f"{g} Blocks"] = extras[g] + "塊"
        extras[g.lower()] = extras[g]
    creature = ["Demon", "Dragon", "Nightmare", "Snake", "Troll", "Unicorn"]
    for c in creature:
        zh = {"Demon": "惡魔", "Dragon": "龍", "Nightmare": "夢魘", "Snake": "蛇", "Troll": "巨魔", "Unicorn": "獨角獸"}[c]
        extras[c] = zh
        extras[f"{c} Skin"] = zh + "皮"
        extras[c.lower()] = zh
    elem = ["Icy", "Lava", "Seaweed", "Dead"]
    for c in elem:
        zh = {"Icy": "寒冰", "Lava": "熔岩", "Seaweed": "海草", "Dead": "亡灵"}[c]
        extras[c] = zh
        extras[f"{c} Skin"] = zh + "皮"
        extras[c.lower()] = zh
    extras["Fire"] = "火焰"
    extras["fire"] = "火焰"
    extras["Cold"] = "寒冷"
    extras["cold"] = "寒冷"
    extras["Venom"] = "毒液"
    extras["venom"] = "毒液"
    extras["Energy"] = "能量"
    extras["energy"] = "能量"
    extras["Cloth"] = "布料"
    extras["cloth"] = "布料"
    extras["crimson"] = "緋紅"
    extras["golden"] = "金黄"
    extras["dark"] = "幽暗"
    extras["viridian"] = "翠綠"
    extras["ivory"] = "象牙"
    extras["azure"] = "蔚藍"
    extras["dinosaur"] = "恐龍"
    extras["Spectral"] = "幽靈"
    extras["Dread"] = "恐懼"
    extras["Ghoulish"] = "屍鬼"
    extras["Wyrm"] = "古龍"
    extras["Holy"] = "聖"
    extras["Bloodless"] = "無血"
    extras["Gilded"] = "鎏金"
    extras["Demilich"] = "半巫妖"
    extras["Wintry"] = "寒冬"
    extras["spectral"] = "幽靈"
    extras["dread"] = "恐懼"
    extras["ghoulish"] = "屍鬼"
    extras["wyrm"] = "古龍"
    extras["holy"] = "聖"
    extras["bloodless"] = "無血"
    extras["gilded"] = "鎏金"
    extras["demilich"] = "半巫妖"
    extras["wintry"] = "寒冬"
    extras["metallic"] = "金屬"
    extras["Metallic"] = "金屬"
    extras["Brazen"] = "黃銅"
    extras["brazen"] = "黃銅"
    extras["Umber"] = "赭"
    extras["umber"] = "赭"
    extras["Violet"] = "紫"
    extras["violet"] = "紫"
    extras["Platinum"] = "白金"
    extras["platinum"] = "白金"
    extras["cadalyte"] = "鉆萊特"
    extras["Cadalyte"] = "鉆萊特"
    # Sci-fi mats keep transliteration
    for w, zh in [
        ("Beskar", "貝斯卡"),
        ("Carbonite", "碳鋼岩"),
        ("Cortosis", "科托西斯"),
        ("Durasteel", "硬鋼"),
        ("Durite", "杜里特"),
        ("Farium", "法理姆"),
        ("Laminasteel", "層壓鋼"),
        ("Neuranium", "神經合金"),
        ("Phrik", "弗里克"),
        ("Promethium", "钷合金"),
        ("Quadranium", "四元合金"),
        ("Songsteel", "歌鋼"),
        ("Titanium", "鈦"),
        ("Trimantium", "三合金"),
        ("Xonolite", "索諾萊特"),
        ("Agrinium", "農金"),
        ("Adesote", "阿德索特"),
        ("Biomesh", "仿生網"),
        ("Cerlin", "瑟林"),
        ("Durafiber", "硬纖維"),
        ("Flexicris", "弗謝克里"),
        ("Hypercloth", "超纖布"),
        ("Nylar", "耐拉"),
        ("Nylonite", "尼龍岩"),
        ("Polyfiber", "聚合纖維"),
        ("Syncloth", "同調布"),
        ("Thermoweave", "熱織"),
    ]:
        extras[w] = zh
        extras[w.lower()] = zh
        extras[f"{w} Metal"] = zh + "金屬"
        extras[f"{w} Material"] = zh + "材料"
    for w, zh in [
        ("Borl", "波爾"), ("Cosian", "科西安"), ("Greel", "格里爾"), ("Japor", "賈波爾"),
        ("Kyshyyyk", "卡希克"), ("Laroon", "拉倫"), ("Teej", "提吉"), ("Veshok", "維肖克"),
    ]:
        extras[w] = zh
        extras[w.lower()] = zh
        extras[f"{w} Timber"] = zh + "木料"
    for w, zh in [
        ("Andorian", "安卓里安"), ("Cardassian", "卡達西"), ("Martian", "火星人"),
        ("Rodian", "羅迪安"), ("Tusken", "塔斯肯"), ("Twi'lek", "緹萊克"),
        ("Xindi", "辛迪"), ("Zabrak", "札布拉克"), ("Gorn", "戈恩"), ("Trandoshan", "特蘭多沙"),
        ("Silurian", "志留"), ("Krayt", "鯊齒"),
    ]:
        extras[w] = zh
        extras[w.lower()] = zh
        extras[f"{w} Bones"] = zh + "骨骸"
        extras[f"{w} Scales"] = zh + "鱗"
    for w in ["Xeno", "Devil", "Sphinx", "Vampire", "Lich"]:
        zh = {"Xeno": "異種", "Devil": "魔鬼", "Sphinx": "斯芬克斯", "Vampire": "吸血鬼", "Lich": "巫妖"}[w]
        extras[w] = zh
        extras[w.lower()] = zh
        extras[f"{w} Bones"] = zh + "骨骸"
    bone_pairs = [
        ("Brittle", "易碎"),
        ("Draco", "龍裔"),
        ("Drow", "黑暗精靈"),
        ("Orc", "獸人"),
        ("Reptile", "爬行生物"),
        ("Ogre", "食人魔"),
        ("Troll", "巨魔"),
        ("Gargoyle", "石像鬼"),
        ("Minotaur", "牛頭怪"),
        ("Lycan", "狼人"),
        ("Shark", "鯊魚"),
        ("Colossal", "巨像"),
        ("Mystical", "秘法"),
    ]
    for eng, zh in bone_pairs:
        extras[eng] = zh
        extras[eng.lower()] = zh
        extras[f"{eng} Bones"] = zh + "骨骸"
    extras["Exodus"] = "脫離"
    extras["exodus"] = "脫離"
    for rn in [
        "Spectral", "Dread", "Ghoulish", "Wyrm", "Holy", "Bloodless", "Gilded", "Demilich", "Wintry",
        "Fire", "Cold", "Venom", "Energy", "Exodus",
    ]:
        zh0 = extras.get(rn, rn)
        extras[f"{rn} Rune"] = zh0 + "符文"
    extras["Turtle Shell Rune"] = "龜殼符文"
    extras["Turtle Shell"] = "龜殼"
    extras["turtle shell"] = "龜殼"
    for cloth_eng, cloth_zh in [
        ("Haunted Cloth", "幽影布料"),
        ("Arctic Cloth", "極地布料"),
        ("Pyre Cloth", "烈焰布料"),
        ("Venomous Cloth", "毒染布料"),
        ("Mysterious Cloth", "神秘布料"),
        ("Vile Cloth", "邪惡布料"),
        ("Divine Cloth", "聖潔布料"),
        ("Fiendish Cloth", "魔性布料"),
        ("Furry Cloth", "毛皮布料"),
        ("Wooly Cloth", "羊毛布料"),
        ("Silk Cloth", "絲綢布料"),
    ]:
        extras[cloth_eng] = cloth_zh
    for adj, azh in [
        ("furry", "毛皮"),
        ("wooly", "羊毛"),
        ("silk", "絲綢"),
        ("haunted", "幽影"),
        ("arctic", "極地"),
        ("pyre", "烈焰"),
        ("venomous", "毒染"),
        ("mysterious", "神秘"),
        ("vile", "邪惡"),
        ("divine", "聖潔"),
        ("fiendish", "魔性"),
    ]:
        extras[adj] = azh
    extras.update(d)
    return extras


def load_payload_numbers() -> dict[str, str]:
    data = json.loads(PAYLOAD.read_text(encoding="utf-8"))
    return {r["n"]: r["en"] for r in data}


def main() -> None:
    ap = argparse.ArgumentParser()
    ap.add_argument("--write", action="store_true", help="Write CliLoc-cht.csv")
    ap.add_argument("--dry-run", action="store_true", help="Print stats and untranslated")
    args = ap.parse_args()

    num_to_en = load_payload_numbers()
    spell_map = spell_inner_en_to_zh()
    carpet = carpet_master_lines()
    work_mat = {k: v for k, v in work_material_lines().items() if v}
    labels = augment_label_dictionary()
    settings = gem_setting_inner()

    translations: dict[str, str] = {}
    untranslated: list[tuple[str, str]] = []

    for num, en in num_to_en.items():
        zh_hans = translate_en_to_zh_hans(
            en, num, spell_map, carpet, work_mat, labels, settings
        )
        if zh_hans.startswith("__UNTRANSLATED__:"):
            untranslated.append((num, en))
            continue
        translations[num] = to_traditional(zh_hans)

    print(f"Translated: {len(translations)}, untranslated: {len(untranslated)}")
    if untranslated:
        for num, en in untranslated[:40]:
            print("  MISSING", num, en[:90])
        if len(untranslated) > 40:
            print("  ...", len(untranslated) - 40, "more")

    if args.dry_run or not args.write:
        return

    # Apply to CliLoc-cht.csv
    lines = CHT_CSV.read_text(encoding="utf-8").splitlines(keepends=True)
    out_lines: list[str] = []
    changed = 0
    for line in lines:
        p = parse_line(line)
        if not p:
            out_lines.append(line)
            continue
        num, old_text, flag = p
        if num in translations:
            new_t = translations[num]
            if old_text.strip() != new_t:
                changed += 1
            out_lines.append(f"{num};{new_t};{flag}\n")
        else:
            out_lines.append(line)

    CHT_CSV.write_text("".join(out_lines), encoding="utf-8")
    print(f"Updated {changed} lines in {CHT_CSV}")


if __name__ == "__main__":
    main()

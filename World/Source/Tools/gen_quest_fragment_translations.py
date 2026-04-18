#!/usr/bin/env python3
"""Generate quest_fragment_zh_table.json from quest-composite-terms-order.txt using layered rules + explicit map."""
from __future__ import annotations

import json
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
ORDER = ROOT / "Data" / "Localization" / "quest-composite-terms-order.txt"
OUT = ROOT / "Data" / "Localization" / "quest-fragment-zh-table.json"
GLOSS = ROOT / "Data" / "Localization" / "glossary-approved-zh.json"

# Canonical land / world phrases (must match stored quest + GetRegionName fallbacks)
LANDS = {
    "the Land of Sosaria": "索萨利亚大陆",
    "the Land of Lodoria": "洛多利亚大陆",
    "the Land of Ambrosia": "安布罗西亚大陆",
    "the Land of Atlantis": "亚特兰蒂斯大陆",
    "the Bottle World of Kuldar": "库尔达瓶中世界",
    "the Island of Umber Veil": "乌伯维尔岛",
    "the Isles of Dread": "恐惧群岛",
    "Isles of Dread": "恐惧群岛",
    "the Serpent Island": "巨蛇岛",
    "Serpent Island": "巨蛇岛",
    "the Savaged Empire": "蛮荒帝国",
    "Savaged Empire": "蛮荒帝国",
    "the Underworld": "冥界深渊",
    "Underworld": "冥界深渊",
    "the Town of Skara Brae": "斯卡拉·布雷镇",
    "Skara Brae": "斯卡拉·布雷",
    "the Moon of Luna": "露娜之月",
    "Sosaria": "索萨利亚",
    "Lodoria": "洛多利亚",
    "Ambrosia": "安布罗西亚",
    "Atlantis": "亚特兰蒂斯",
    "Kuldar": "库尔达",
    "Umber Veil": "乌伯维尔",
    "Luna": "露娜",
}

DUNGEONS = {
    "the Stygian Abyss": "冥狱深渊",
    "the Halls of Undermountain": "幽暗地域厅堂",
    "the Hall of the Mountain King": "山王殿堂",
    "the Tomb of the Fallen Wizard": "陨落法师之墓",
    "the Vault of the Black Knight": "黑骑士宝库",
    "the Dungeon of the Mad Archmage": "疯法师地下城",
    "the Dungeon of the Lich King": "巫妖王地下城",
    "the Dungeon of Time Awaits": "时光候待之穴",
    "the Cave of Banished Mages": "流放法师洞窟",
    "the Depths of Carthax Lake": "卡瑟克斯湖底",
    "the Valley of Dark Druids": "黑暗德鲁伊谷",
    "the Catacombs of Azerok": "阿泽洛克地下墓穴",
    "the Caverns of Poseidon": "波塞冬洞窟",
    "the Ancient Elven Mine": "古代精灵矿坑",
    "the Ice Queen Fortress": "冰霜女王要塞",
    "the Castle of Dracula": "德古拉城堡",
    "the Cave of the Zuluu": "祖鲁洞窟",
    "the Crypts of Dracula": "德古拉墓穴",
    "the Lodoria Catacombs": "洛多利亚地下墓穴",
    "the Ancient Sky Ship": "古代天空船",
    "the City of the Dead": "亡者之城",
    "the Crypts of Kuldar": "库尔达墓穴",
    "the Halls of Ogrimar": "奥格瑞玛厅堂",
    "the Mines of Morinia": "莫瑞尼亚矿坑",
    "the Storm Giant Lair": "风暴巨人巢穴",
    "the Temple of Osirus": "奥西鲁斯神殿",
    "Morgaelin's Inferno": "莫盖林炼狱",
    "the Ancient Pyramid": "古代金字塔",
    "the Forgotten Halls": "被遗忘的大厅",
    "the Gargoyle Crypts": "石像鬼墓穴",
    "the Perinian Depths": "佩里尼亚深渊",
    "the Serpent Sanctum": "巨蛇圣所",
    "the Tomb of Kazibal": "卡齐巴尔之墓",
    "the Undersea Castle": "海底城堡",
    "the Ancient Prison": "古代监狱",
    "the City of Embers": "余烬之城",
    "the Flooded Temple": "淹没神殿",
    "the Ice Fiend Lair": "冰霜魔巢穴",
    "the Kuldara Sewers": "库尔达拉下水道",
    "the Tower of Brass": "黄铜之塔",
    "Argentrock Castle": "银岩城堡",
    "the Cave of Souls": "灵魂洞窟",
    "the Daemon's Crag": "恶魔峭壁",
    "the Fires of Hell": "地狱烈焰",
    "the Undersea Pass": "海底通道",
    "the Volcanic Cave": "火山洞穴",
    "Dungeon Covetous": "贪婪地城",
    "Dungeon Hythloth": "希斯洛地城",
    "Dungeon Despise": "轻蔑地城",
    "Dungeon Destard": "德斯塔德地城",
    "Dungeon Torment": "折磨地城",
    "Dungeon Deceit": "欺诈地城",
    "Dungeon Exodus": "出埃及地城",
    "Dungeon Wicked": "邪恶地城",
    "Dungeon Clues": "线索地城",
    "Dungeon Scorn": "蔑视地城",
    "Dungeon Shame": "羞耻地城",
    "Dungeon Wrath": "愤怒地城",
    "Dungeon Wrong": "谬误地城",
    "Dungeon Ankh": "安卡地城",
    "Dungeon Bane": "灾厄地城",
    "Dungeon Doom": "末日地城",
    "Dungeon Hate": "憎恨地城",
    "Dungeon Rock": "岩石地城",
    "Dungeon Vile": "污秽地城",
    "Vordo's Dungeon": "沃多地城",
    "Vordo's Castle": "沃多城堡",
    "Dardin's Pit": "达丁深坑",
    "Terathan Keep": "特拉森要塞",
    "Stonegate Castle": "石门城堡",
    "the Azure Castle": "蔚蓝城堡",
    "the Blood Temple": "鲜血神殿",
    "the Cave of Fire": "火焰洞窟",
    "the Corrupt Pass": "腐化隘口",
    "the Dragon's Maw": "龙喉",
    "the Frozen Hells": "冰冻地狱",
    "the Glacial Scar": "冰川裂隙",
    "the Ratmen Mines": "鼠人矿坑",
    "the Zealan Tombs": "泽兰陵墓",
    "the Sanctum of Saltmarsh": "盐沼圣所",
    "the Pirate Cave": "海盗洞窟",
    "the Ratmen Lair": "鼠人巢穴",
    "the Scurvy Reef": "坏血病礁",
}

ITEMS = {
    "Amulet": "护身符", "Armor": "护甲", "Axe": "斧", "Bag": "袋子", "Belt": "腰带", "Blade": "刀刃",
    "Bones": "骨头", "Book": "书籍", "Boots": "靴子", "Bottle": "瓶子", "Bow": "弓", "Bracelet": "手镯",
    "Candle": "蜡烛", "Cape": "斗篷", "Chalice": "圣杯", "Cloak": "斗篷", "Club": "棍棒", "Codex": "法典",
    "Crossbow": "弩", "Crown": "王冠", "Crystal Ball": "水晶球", "Cutlass": "弯刀", "Dagger": "匕首",
    "Drum": "鼓", "Dust": "尘埃", "Earrings": "耳环", "Elixir": "灵药", "Flute": "长笛", "Gem": "宝石",
    "Gloves": "手套", "Goblet": "酒杯", "Halberd": "戟", "Hat": "帽子", "Helm": "头盔", "Horn": "号角",
    "Key": "钥匙", "Knife": "小刀", "Kryss": "细剑", "Lantern": "提灯", "Lexicon": "辞典", "Lute": "鲁特琴",
    "Mace": "钉头锤", "Mirror": "镜子", "Necklace": "项链", "Parchment": "羊皮纸", "Portrait": "肖像",
    "Potion": "药水", "Pouch": "小袋", "Ring": "戒指", "Robe": "长袍", "Rod": "法杖", "Rope": "绳索",
    "Scabbard": "剑鞘", "Sceptre": "权杖", "Scimitar": "弯刀", "Scroll": "卷轴", "Shackles": "镣铐",
    "Shield": "盾牌", "Skull": "颅骨", "Spellbook": "法术书", "Staff": "法杖", "Stone": "石头",
    "Sword": "剑", "Tablet": "石板", "Tome": "大部头典籍", "Trident": "三叉戟", "Wand": "魔杖",
    "Warhammer": "战锤",
}

ADJECTIVES = {
    "Exotic": "异域的", "Mysterious": "神秘的", "Enchanted": "附魔的", "Marvelous": "奇妙的", "Amazing": "惊人的",
    "Astonishing": "令人震惊的", "Mystical": "秘术般的", "Astounding": "骇人的", "Magical": "魔法的",
    "Divine": "神圣的", "Excellent": "卓越的", "Magnificent": "壮丽的", "Phenomenal": "非凡的",
    "Fantastic": "奇异的", "Incredible": "难以置信的", "Extraordinary": "非凡的", "Fabulous": "极好的",
    "Wondrous": "奇异的", "Glorious": "辉煌的", "Lost": "失落的", "Fabled": "传说中的", "Legendary": "传奇的",
    "Mythical": "神话的", "Missing": "失踪的", "Ancestral": "祖传的", "Ornate": "华丽的", "Ultimate": "终极的",
    "Rare": "稀有的", "Wonderful": "奇妙的", "Sacred": "神圣的", "Almighty": "全能的", "Supreme": "至上的",
    "Mighty": "强大的", "Unspeakable": "不可名状的", "Unknown": "未知的", "Forgotten": "被遗忘的",
    "Cursed": "诅咒的", "Glowing": "发光的", "Dark": "黑暗的", "Evil": "邪恶的", "Holy": "圣洁的",
    "Vile": "污秽的", "Ethereal": "虚空的", "Demonic": "恶魔的", "Burning": "燃烧的", "Angelic": "天使的",
    "Frozen": "冰冻的", "Icy": "冰冷的", "Blackened": "焦黑的", "Lunar": "月之", "Solar": "日之",
    "Bright": "明亮的", "Electrical": "雷电的", "Deathly": "死寂的", "Hexed": "被咒的", "Unholy": "不洁",
    "Blessed": "祝福的", "Infernal": "炼狱的", "Damned": "被诅咒的", "Doomed": "注定毁灭的",
}

EPITHETS = {
    "the Light": "光明", "the Dark": "黑暗", "the Spirits": "灵体", "the Dead": "亡者", "the Fowl": "飞禽",
    "Hades": "冥府", "Fire": "火焰", "Ice": "寒冰", "the Void": "虚空", "Venom": "剧毒", "the Planes": "位面",
    "the Demon": "恶魔", "the Angel": "天使", "the Devil": "魔鬼", "Death": "死亡", "Life": "生命",
    "Illusions": "幻象", "the Other World": "异界", "Negative Energy": "负能量", "Reality": "现实",
    "the Sky": "天空", "the Moon": "月亮", "the Sun": "太阳", "the Stars": "星辰", "the Earth": "大地",
    "the Dungeon": "地下城", "the Tomb": "墓穴", "the Ghost": "幽灵",
    "Ultimate Evil": "终极之恶", "Pure Evil": "纯粹之恶", "Demonic Power": "恶魔之力", "Holy Light": "圣光",
    "the Cursed": "受诅者", "the Damned": "被谴者", "the Vile": "邪恶之徒", "Darkness": "黑暗", "Purity": "纯净",
    "Might": "力量", "Power": "力量", "Greatness": "伟大", "Magic": "魔法", "Supremacy": "至高",
    "the Almighty": "全能者", "the Sacred": "神圣", "Magnificence": "壮丽", "Excellence": "卓越",
    "Glory": "荣耀", "Mystery": "奥秘", "the Divine": "神圣", "the Forgotten": "被遗忘者", "Legend": "传说",
    "the Lost": "失落者", "the Ancients": "远古者", "Wonder": "奇迹", "the Mighty": "强者", "Marvel": "奇观",
    "Nobility": "高贵", "Mysticism": "秘术", "Enchantment": "附魔",
}

ROLES = {
    "the Templar": "圣殿骑士", "the Thief": "盗贼", "the Illusionist": "幻术师", "the Princess": "公主",
    "the Invoker": "召唤师", "the Priestess": "女祭司", "the Conjurer": "咒术师", "the Bandit": "强盗",
    "the Baroness": "男爵夫人", "the Wizard": "巫师", "the Cleric": "牧师", "the Monk": "僧侣",
    "the Minstrel": "吟游诗人", "the Defender": "捍卫者", "the Cavalier": "骑士", "the Magician": "魔术师",
    "the Witch": "女巫", "the Fighter": "战士", "the Seeker": "探求者", "the Slayer": "猎杀者",
    "the Ranger": "游侠", "the Barbarian": "野蛮人", "the Explorer": "探险家", "the Heretic": "异端",
    "the Gladiator": "角斗士", "the Sage": "贤者", "the Rogue": "无赖", "the Paladin": "圣骑士",
    "the Bard": "诗人", "the Diviner": "占卜师", "the Lady": "女士", "the Outlaw": "亡命徒",
    "the Prophet": "先知", "the Mercenary": "佣兵", "the Adventurer": "冒险者", "the Enchantress": "女巫",
    "the Queen": "女王", "the Scout": "斥候", "the Mystic": "秘术师", "the Mage": "法师",
    "the Traveler": "旅人", "the Summoner": "召唤者", "the Warrior": "战士", "the Sorcereress": "女术士",
    "the Seer": "先知", "the Hunter": "猎人", "the Knight": "骑士", "the Necromancer": "死灵法师",
    "the Shaman": "萨满", "the Prince": "王子", "the Priest": "祭司", "the Baron": "男爵",
    "the Warlock": "术士", "the Lord": "领主", "the Enchanter": "附魔师", "the King": "国王",
    "the Sorcerer": "术士",
}

MISC_PLACES = {
    "the cave": "洞穴", "the castle": "城堡", "the tower": "高塔", "the ruins": "废墟", "the dungeon": "地下城",
    "the graveyard": "墓地", "the cemetery": "墓园", "the crypt": "墓穴", "the labyrinth": "迷宫",
    "the maze": "迷宫", "the forest": "森林", "the jungle": "丛林", "the desert": "沙漠", "the swamp": "沼泽",
    "the tunnels": "隧道", "the Tombs": "陵墓", "the tomb": "墓穴",
}


def load_glossary_canonical() -> dict[str, str]:
    out: dict[str, str] = {}
    if not GLOSS.is_file():
        return out
    data = json.loads(GLOSS.read_text(encoding="utf-8"))
    terms = data.get("terms") or {}
    for en, obj in terms.items():
        if isinstance(obj, dict) and "canonical" in obj:
            out[en] = obj["canonical"]
    return out


def build_table() -> dict[str, str]:
    gloss = load_glossary_canonical()
    main: dict[str, str] = {}
    for d in (LANDS, DUNGEONS, ITEMS, ADJECTIVES, EPITHETS, ROLES, MISC_PLACES):
        for k, v in d.items():
            main[k] = v
    for k, v in gloss.items():
        main.setdefault(k, v)

    lines = [
        ln.strip()
        for ln in ORDER.read_text(encoding="utf-8").splitlines()
        if ln.strip() and not ln.startswith("#")
    ]
    out: dict[str, str] = {}
    for en in lines:
        if en in main:
            out[en] = main[en]
            continue
        low = en.lower()
        # generic "the X" dungeon / place
        if en.startswith("the ") and len(en) > 6:
            body = en[4:]
            if body in main:
                out[en] = "「" + main[body] + "」"
                continue
            if "dungeon" in low:
                out[en] = "「" + body.replace("Dungeon", "地城").replace("dungeon", "地城") + "」"
                continue
            if "cave" in low:
                out[en] = "「" + body + "洞窟」"
                continue
            if "castle" in low:
                out[en] = "「" + body.replace("Castle", "城堡") + "」"
                continue
        # title-case single token roles / adjectives fallback
        if en in ROLES or en in ADJECTIVES or en in ITEMS:
            out[en] = main.get(en, en)
            continue
        out[en] = main.get(en, en)
    return out


def main() -> int:
    if not ORDER.is_file():
        print("missing", ORDER, file=sys.stderr)
        return 1
    table = build_table()
    OUT.write_text(json.dumps(table, ensure_ascii=False, indent=2), encoding="utf-8")
    print(f"wrote {len(table)} entries -> {OUT}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

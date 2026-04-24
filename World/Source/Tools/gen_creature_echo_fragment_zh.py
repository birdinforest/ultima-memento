#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Generate zh-Hans for quest-composite echo lines (creature / epithet fragments).

Output: World/Data/Localization/creature-echo-fragment-zh.json
Merged by gen_quest_fragment_translations.py into quest-fragment-zh-table.json.

Run from repo root:
  python3 World/Source/Tools/gen_creature_echo_fragment_zh.py
"""
from __future__ import annotations

import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
TABLE = ROOT / "Data" / "Localization" / "quest-fragment-zh-table.json"
OUT = ROOT / "Data" / "Localization" / "creature-echo-fragment-zh.json"

# Single-token gloss (lowercase key) — fantasy UO tone, 略正式
WORD: dict[str, str] = {
    "a": "",
    "an": "",
    "the": "",
    "of": "",
    "adder": "蝰蛇",
    "adventurer": "冒险者",
    "air": "气",
    "amethyst": "紫晶",
    "anaconda": "森蚺",
    "ancient": "远古",
    "antaur": "蚁牛人",
    "aquatic": "水生",
    "arcadion": "阿卡迪翁",
    "archer": "弓手",
    "arctic": "极地",
    "ashen": "灰烬",
    "avenger": "复仇者",
    "baby": "幼",
    "balinor": "巴利诺尔",
    "balrog": "炎魔",
    "balron": "魔将",
    "bandit": "强盗",
    "banshee": "女妖",
    "barbarian": "蛮族战士",
    "bard": "吟游诗人",
    "baron": "男爵",
    "baroness": "女男爵",
    "bat": "蝙蝠",
    "bear": "熊",
    "beetle": "甲虫",
    "berserker": "狂战士",
    "black": "黑",
    "blackthorne": "布莱克索恩",
    "blood": "血",
    "bomb": "炸弹",
    "bone": "骨",
    "brass": "黄铜",
    "buccaneer": "海盗",
    "bugbear": "熊地精",
    "bunny": "怪兔",
    "burnt": "焦骸",
    "captain": "队长",
    "cavalier": "骑士",
    "cleric": "神职者",
    "coldwater": "寒水",
    "conjurer": "召唤师",
    "corpse": "尸体",
    "crab": "蟹",
    "creature": "生物",
    "daemon": "魔族",
    "dark": "黑暗",
    "dead": "死者",
    "deadly": "致命",
    "death": "死亡",
    "decaying": "腐朽",
    "deep": "深海",
    "defender": "卫士",
    "demon": "恶魔",
    "demonic": "魔性",
    "devil": "魔鬼",
    "diseased": "染病",
    "diviner": "占卜师",
    "dragon": "龙",
    "dragyn": "龙裔",
    "drake": "飞龙",
    "draxinusom": "德拉齐努索姆",
    "dread": "恐惧",
    "dreadhorn": "恐角兽",
    "drone": "工兵",
    "earth": "大地",
    "eel": "鳗",
    "elder": "长老",
    "elemental": "元素",
    "emerald": "翡翠",
    "enchanter": "附魔师",
    "enchantress": "女附魔师",
    "enslaved": "被奴役的",
    "ettin": "双头巨人",
    "explorer": "探索者",
    "fiend": "魔物",
    "fighter": "战士",
    "fire": "火焰",
    "firebone": "焰骨",
    "fisherman": "渔夫",
    "flamecaster": "塑焰师",
    "flayer": "夺心魔",
    "flesh": "血肉",
    "frail": "枯瘦",
    "frost": "霜",
    "frozen": "冰封",
    "gargoyle": "石像鬼",
    "ghost": "幽灵",
    "ghostly": "幽影",
    "ghoul": "食尸鬼",
    "ghoulish": "尸鬼般",
    "giant": "巨",
    "gladiator": "角斗士",
    "gnoll": "豺狼人",
    "gnome": "侏儒",
    "goblin": "哥布林",
    "golden": "黄金",
    "golem": "魔像",
    "gorgon": "蛇发女妖",
    "gotha": "戈萨",
    "grave": "墓穴",
    "gray": "灰",
    "great": "巨",
    "greater": "高阶",
    "guard": "卫兵",
    "harpy": "鹰身女妖",
    "haunt": "怨灵",
    "headless": "无首",
    "hen": "雌妖",
    "heretic": "异教徒",
    "hobgoblin": "大地精",
    "horse": "马",
    "huge": "巨型",
    "hulk": "巨怪",
    "hunter": "猎人",
    "ice": "寒冰",
    "icy": "冰霜",
    "illithid": "夺心魔",
    "illusionist": "幻术师",
    "invoker": "咒召师",
    "iron": "铁",
    "jade": "翡翠",
    "jailor": "狱卒",
    "jungle": "丛林",
    "kelp": "海藻",
    "king": "国王",
    "knight": "骑士",
    "kobold": "狗头人",
    "kraken": "海怪",
    "lady": "贵妇",
    "large": "大",
    "lava": "熔岩",
    "leech": "水蛭",
    "lesser": "低阶",
    "leviathan": "利维坦",
    "lich": "巫妖",
    "lightning": "闪电",
    "lion": "狮",
    "lord": "领主",
    "mage": "法师",
    "magi": "法师",
    "magician": "魔法师",
    "magma": "岩浆",
    "marble": "大理石",
    "matriarch": "族母",
    "mausoleum": "陵墓寝堂",
    "medusa": "美杜莎",
    "megalodon": "巨齿鲨",
    "mercenary": "佣兵",
    "mind": "心灵",
    "minotaur": "牛头人",
    "minstrel": "吟游歌手",
    "mongbat": "蝠猴",
    "monk": "修士",
    "monstrous": "巨怪",
    "morphius": "莫菲乌斯",
    "mors": "莫斯",
    "mummy": "木乃伊",
    "mutant": "变异",
    "mystic": "神秘者",
    "naga": "那伽",
    "nightmare": "梦魇",
    "nox": "剧毒",
    "nystal": "尼斯塔尔",
    "ogre": "食人魔",
    "one": "者",
    "onyx": "缟玛瑙",
    "ophidian": "蛇人",
    "orc": "兽人",
    "outlaw": "亡命之徒",
    "paladin": "圣骑士",
    "phantom": "幻影",
    "phase": "相位",
    "phoenix": "凤凰",
    "pirate": "海盗",
    "plasma": "等离子",
    "poison": "剧毒",
    "priest": "祭司",
    "priestess": "女祭司",
    "prince": "王子",
    "princess": "公主",
    "prophet": "先知",
    "psychic": "灵能者",
    "pyromancer": "烈焰法师",
    "queen": "女王",
    "ranger": "游侠",
    "reaper": "收割者",
    "restless": "不安",
    "revenant": "还魂尸",
    "roc": "巨鹏",
    "rogue": "盗贼",
    "rotten": "腐烂",
    "rotting": "腐溃",
    "ruby": "红宝石",
    "rune": "符文",
    "runic": "符文",
    "rust": "锈蚀",
    "sage": "智者",
    "sailor": "水手",
    "samhayne": "萨姆海恩",
    "samurai": "武士",
    "sand": "沙",
    "sapphire": "蓝宝石",
    "scorpion": "蝎",
    "scout": "斥候",
    "sea": "海",
    "seeker": "寻觅者",
    "seer": "预言者",
    "seggallion": "塞加利翁",
    "serpent": "巨蛇",
    "shade": "阴魂",
    "shadow": "阴影",
    "shaman": "萨满",
    "shark": "鲨",
    "shimazu": "岛津",
    "silver": "银",
    "skeletal": "骷髅",
    "skeleton": "骷髅",
    "skellot": "骨奴",
    "slasher": "撕裂者",
    "slayer": "猎杀者",
    "snake": "蛇",
    "snow": "雪",
    "soldier": "兵卒",
    "sorcerer": "术士",
    "sorceress": "女术士",
    "soul": "灵魂",
    "specter": "幽影",
    "spectral": "灵体",
    "spectre": "怨灵",
    "spider": "蛛",
    "spinner": "织网者",
    "spirit": "灵体",
    "squid": "乌贼",
    "steam": "蒸汽",
    "steed": "坐骑",
    "stone": "岩石",
    "succubus": "魅魔",
    "sucker": "吸魂怪",
    "summoner": "召唤师",
    "swamp": "沼泽",
    "tarantula": "狼蛛",
    "templar": "圣殿骑士",
    "terathan": "特拉森",
    "thief": "盗贼",
    "titan": "泰坦",
    "titanoboa": "泰坦蟒",
    "tomb": "墓穴",
    "traveler": "旅人",
    "troll": "巨魔",
    "trollbear": "巨魔熊",
    "tyball": "泰鲍尔",
    "umber": "暗褐",
    "undead": "亡灵",
    "unicorn": "独角兽",
    "vampire": "吸血鬼",
    "vampyre": "血族",
    "viper": "毒蛇",
    "vorpal": "斩首",
    "vrock": "弗洛魔",
    "wailing": "哀嚎",
    "walking": "行尸",
    "warlock": "战锁",
    "warrior": "战士",
    "watcher": "守墓者",
    "water": "水",
    "wax": "蜡",
    "white": "白",
    "widow": "寡妇蛛",
    "wight": "尸妖",
    "wisp": "精魂",
    "witch": "女巫",
    "wizard": "巫师",
    "wolf": "狼",
    "wooden": "木制",
    "worker": "工蚁",
    "worshipper": "崇拜者",
    "wraith": "怨灵",
    "wurm": "巨虫",
    "wyrm": "古龙",
    "wyvra": "飞龙",
    "young": "幼年",
    "zombie": "僵尸",
}

# Multi-word compounds (longest first when matching)
BIGRAM = {
    "fire mage": "烈焰法师",
    "ice elemental": "寒冰元素",
    "ice golem": "寒冰魔像",
    "ice serpent": "寒冰巨蛇",
    "deep sea": "深海",
    "deep water": "深水",
    "great white": "大白",
    "blood snake": "血蛇",
    "death adder": "死亡蝰蛇",
    "giant black": "巨型黑",
    "giant spider": "巨型蜘蛛",
    "giant snake": "巨蛇",
    "giant serpent": "巨蟒",
    "giant squid": "大王乌贼",
    "giant crab": "巨蟹",
    "giant leech": "巨蛭",
    "giant adder": "巨蝰",
    "giant eel": "巨鳗",
    "sea dragon": "海龙",
    "sea drake": "海飞龙",
    "sea serpent": "海蟒",
    "sea troll": "海巨魔",
    "sea viper": "海蝰",
    "sand spider": "沙蛛",
    "sand gargoyle": "沙石像鬼",
    "snow elemental": "雪元素",
    "snow harpy": "雪鹰身女妖",
    "stone elemental": "岩石元素",
    "stone gargoyle": "岩石石像鬼",
    "stone golem": "岩石魔像",
    "stone harpy": "岩石鹰身女妖",
    "water elemental": "水元素",
    "water naga": "水那伽",
    "lava elemental": "熔岩元素",
    "lava serpent": "熔岩巨蛇",
    "magma elemental": "岩浆元素",
    "magma snake": "岩浆蛇",
    "blood elemental": "血元素",
    "plasma elemental": "等离子元素",
    "poison elemental": "剧毒元素",
    "nox elemental": "剧毒元素",
    "lightning elemental": "闪电元素",
    "earth elemental": "大地元素",
    "air elemental": "气元素",
    "steam elemental": "蒸汽元素",
    "rust golem": "锈蚀魔像",
    "rune beetle": "符文甲虫",
    "runic golem": "符文魔像",
    "wax golem": "蜡魔像",
    "wooden golem": "木魔像",
    "iron golem": "钢铁魔像",
    "bone golem": "骨魔像",
    "bone demon": "骨魔",
    "bone knight": "骨骑士",
    "bone magi": "骨法师",
    "bone slasher": "骨撕裂者",
    "dark reaper": "黑暗收割者",
    "dead pirate": "死海盗",
    "demon lord": "恶魔领主",
    "demonic ghost": "魔性幽灵",
    "ghost dragyn": "幽灵龙裔",
    "ghostly dragon": "幽影龙",
    "ghostly gargoyle": "幽影石像鬼",
    "ghoulish pirate": "尸鬼海盗",
    "golden serpent": "黄金巨蛇",
    "grave seeker": "墓穴寻觅者",
    "grave wurm": "墓穴巨虫",
    "gray dragon": "灰龙",
    "great serpent": "巨蟒",
    "greater demon": "高阶恶魔",
    "harpy hen": "鹰身女妖雌体",
    "headless one": "无首者",
    "jungle viper": "丛林蝰蛇",
    "kelp fiend": "海藻魔",
    "kobold shaman": "狗头人萨满",
    "large crab": "大蟹",
    "large snake": "大蛇",
    "lesser demon": "低阶恶魔",
    "lesser devil": "低阶魔鬼",
    "lich lord": "巫妖领主",
    "mind flayer": "夺心魔",
    "minotaur lord": "牛头人领主",
    "orc captain": "兽人队长",
    "orc lord": "兽人领主",
    "orc mage": "兽人法师",
    "orc warrior": "兽人战士",
    "umber hulk": "暗褐巨怪",
    "undead bear": "亡灵熊",
    "undead captain": "亡灵船长",
    "undead corpse": "亡灵尸躯",
    "undead giant": "亡灵巨人",
    "undead lion": "亡灵狮",
    "undead sailor": "亡灵水手",
    "undead wolf": "亡灵狼",
    "coldwater serpent": "寒水巨蛇",
    "deep sea serpent": "深海巨蟒",
    "deep sea snake": "深海巨蛇",
    "deep water troll": "深水巨魔",
    "fire naga": "火焰那伽",
    "fire demon": "火焰恶魔",
    "brass serpent": "黄铜巨蛇",
    "jade serpent": "翡翠巨蛇",
    "silver serpent": "银蛇",
    "marble gargoyle": "大理石石像鬼",
    "ruby gargoyle": "红宝石石像鬼",
    "sapphire gargoyle": "蓝宝石石像鬼",
    "emerald gargoyle": "翡翠石像鬼",
    "amethyst gargoyle": "紫晶石像鬼",
    "onyx gargoyle": "缟玛瑙石像鬼",
    "ashen gargoyle": "灰烬石像鬼",
    "ancient gargoyle": "远古石像鬼",
    "ancient nightmare": "远古梦魇",
    "elder gargoyle": "长老石像鬼",
    "elder harpy": "长老鹰身女妖",
    "elder vampire": "长老吸血鬼",
    "spectral gargoyle": "灵体石像鬼",
    "spectral pirate": "灵体海盗",
    "zombie dragon": "僵尸龙",
    "zombie gargoyle": "僵尸石像鬼",
    "zombie mage": "僵尸法师",
    "young vampire": "幼年吸血鬼",
    "vampire bat": "吸血蝠",
    "vorpal bunny": "斩首怪兔",
    "walking corpse": "行尸",
    "walking dead": "行尸走肉",
    "soul reaper": "灵魂收割者",
    "soul sucker": "吸魂怪",
    "shadow demon": "阴影恶魔",
    "shadow fiend": "阴影魔物",
    "shadow wisp": "阴影精魂",
    "terathan avenger": "特拉森复仇者",
    "terathan drone": "特拉森工兵",
    "terathan matriarch": "特拉森族母",
    "terathan warrior": "特拉森战士",
    "antaur lord": "蚁牛人领主",
    "antaur soldier": "蚁牛人战士",
    "antaur worker": "蚁牛人劳工",
    "arctic ettin": "极地双头巨人",
    "arctic ogre": "极地食人魔",
    "arctic ogre lord": "极地食人魔领主",
    "aquatic ghoul": "水生食尸鬼",
    "icy ghoul": "冰霜食尸鬼",
    "ettin shaman": "双头巨人萨满",
    "ogre magi": "食人魔法师",
    "worshipper of the bomb": "炸弹教崇拜者",
    "psychic of the bomb": "炸弹教灵能者",
}

# Full-line overrides (exact English key)
FULL: dict[str, str] = {
    "the Mausoleum": "陵墓寝堂",
    "Lord Blackthorne": "布莱克索恩领主",
    "Lord Draxinusom": "德拉齐努索姆领主",
    "Mors Gotha": "莫斯·戈萨",
    "Seggallion": "塞加利翁",
    "Tyball": "泰鲍尔",
    "Morphius": "莫菲乌斯",
    "Nystal": "尼斯塔尔",
    "Samhayne": "萨姆海恩",
    "Shimazu": "岛津",
    "Balinor": "巴利诺尔",
    "Arcadion": "阿卡迪翁",
}

# Single-token role / title lines in quest-composite-terms-order (echo without "the …")
ROLE_SINGLE: dict[str, str] = {
    "adventurer": "一名冒险者",
    "bandit": "一名强盗",
    "barbarian": "一名蛮族战士",
    "bard": "一名吟游诗人",
    "baron": "一名男爵",
    "baroness": "一名女男爵",
    "cavalier": "一名骑士",
    "cleric": "一名神职者",
    "conjurer": "一名召唤师",
    "defender": "一名卫士",
    "diviner": "一名占卜师",
    "enchanter": "一名附魔师",
    "enchantress": "一名女附魔师",
    "explorer": "一名探索者",
    "fighter": "一名战士",
    "gladiator": "一名角斗士",
    "heretic": "一名异教徒",
    "hunter": "一名猎人",
    "illusionist": "一名幻术师",
    "invoker": "一名咒召师",
    "king": "一位国王",
    "knight": "一名骑士",
    "lady": "一位贵妇",
    "lord": "一位领主",
    "mage": "一名法师",
    "magician": "一名魔法师",
    "mercenary": "一名佣兵",
    "minstrel": "一名吟游歌手",
    "monk": "一名修士",
    "mystic": "一名神秘者",
    "outlaw": "一名亡命之徒",
    "paladin": "一名圣骑士",
    "priest": "一名祭司",
    "priestess": "一名女祭司",
    "prince": "一位王子",
    "princess": "一位公主",
    "prophet": "一名先知",
    "queen": "一位女王",
    "ranger": "一名游侠",
    "rogue": "一名盗贼",
    "sage": "一名智者",
    "scout": "一名斥候",
    "seeker": "一名寻觅者",
    "seer": "一名预言者",
    "shaman": "一名萨满",
    "slayer": "一名猎杀者",
    "sorcerer": "一名术士",
    "sorceress": "一名女术士",
    "summoner": "一名召唤师",
    "templar": "一名圣殿骑士",
    "thief": "一名盗贼",
    "traveler": "一名旅人",
    "warlock": "一名战锁",
    "warrior": "一名战士",
    "witch": "一名女巫",
    "wizard": "一名巫师",
}


def _compose_tail(tail: str) -> str:
    tail = tail.strip()
    if not tail:
        return ""
    if tail in FULL:
        return FULL[tail]
    s = tail.lower()
    if s in BIGRAM:
        return BIGRAM[s]
    words = tail.split()
    out: list[str] = []
    i = 0
    while i < len(words):
        matched = False
        for L in range(min(3, len(words) - i), 1, -1):
            chunk = " ".join(words[i : i + L]).lower()
            if chunk in BIGRAM:
                out.append(BIGRAM[chunk])
                i += L
                matched = True
                break
        if matched:
            continue
        w = words[i].lower()
        out.append(WORD.get(w, words[i]))
        i += 1
    return "".join(out)


def _zh_creature_line(en: str) -> str | None:
    en = en.strip()
    if en in FULL:
        return FULL[en]
    if en.startswith("a skeletal "):
        rest = en[11:]
        return "一名骷髅" + _compose_tail(rest)
    if en.startswith("an undead "):
        rest = en[10:]
        return "一名亡灵" + _compose_tail(rest)
    if en.startswith("a giant "):
        return "一只巨型" + _compose_tail(en[8:])
    if en.startswith("a bone "):
        return "一只骨" + _compose_tail(en[7:])
    if en.startswith("an orc "):
        return "一名兽人" + _compose_tail(en[7:])
    if en.startswith("a sea "):
        return "一只海" + _compose_tail(en[5:])
    if en.startswith("a rotting "):
        return "一只腐溃" + _compose_tail(en[10:])
    if en.startswith("a stone "):
        return "一只岩石" + _compose_tail(en[8:])
    if en.startswith("a terathan "):
        return "一只特拉森" + _compose_tail(en[12:])
    if en.startswith("a zombie "):
        return "一只僵尸" + _compose_tail(en[9:])
    if en.startswith("an antaur "):
        return "一只蚁牛人" + _compose_tail(en[10:])
    if en.startswith("a shadow "):
        return "一只阴影" + _compose_tail(en[9:])
    if en.startswith("a frost "):
        return "一只霜" + _compose_tail(en[8:])
    if en.startswith("an arctic "):
        return "一只极地" + _compose_tail(en[10:])
    if en.startswith("a deep "):
        return "一只深海" + _compose_tail(en[7:])
    if en.startswith("an elder "):
        return "一只长老" + _compose_tail(en[9:])
    if en.startswith("a gnome "):
        return "一名侏儒" + _compose_tail(en[8:])
    if en.startswith("a gargoyle"):
        if en == "a gargoyle":
            return "一只石像鬼"
        return "一只石像鬼" + _compose_tail(en[11:].lstrip())
    if en.startswith("a fire"):
        if en == "a fire demon":
            return "一只火焰恶魔"
        if en == "a fire elemental":
            return "一只火焰元素"
        if en == "a fire naga":
            return "一只火焰那伽"
        if en == "a firebone warrior":
            return "一名焰骨战士"
        rest = en[6:].lstrip()
        return "一只火焰" + _compose_tail(rest) if rest else "火焰生物"
    if en.startswith("an ice "):
        return "一只寒冰" + _compose_tail(en[7:])
    if en.startswith("an ogre "):
        return "一名食人魔" + _compose_tail(en[7:])
    if en.startswith("an ancient "):
        return "一只远古" + _compose_tail(en[11:])
    if en.startswith("a spectral "):
        return "一只灵体" + _compose_tail(en[11:])
    if en.startswith("a great "):
        return "一只巨" + _compose_tail(en[7:])
    if en.startswith("a blood "):
        return "一只血" + _compose_tail(en[7:])
    if en.startswith("a minotaur "):
        return "一只牛头人" + _compose_tail(en[12:])
    if en.startswith("a coldwater "):
        return "一只寒水" + _compose_tail(en[12:])
    if en.startswith("a deep sea "):
        return "一只深海" + _compose_tail(en[11:])
    if en.startswith("a deep water "):
        return "一只深水" + _compose_tail(en[14:])
    if en.startswith("a marble "):
        return "一只大理石" + _compose_tail(en[9:])
    if en.startswith("a ruby "):
        return "一只红宝石" + _compose_tail(en[7:])
    if en.startswith("a sapphire "):
        return "一只蓝宝石" + _compose_tail(en[11:])
    if en.startswith("an emerald "):
        return "一只翡翠" + _compose_tail(en[11:])
    if en.startswith("an amethyst "):
        return "一只紫晶" + _compose_tail(en[12:])
    if en.startswith("an onyx "):
        return "一只缟玛瑙" + _compose_tail(en[8:])
    if en.startswith("an ashen "):
        return "一只灰烬" + _compose_tail(en[8:])
    if en.startswith("a sand "):
        return "一只沙" + _compose_tail(en[7:])
    if en.startswith("a snow "):
        return "一只雪" + _compose_tail(en[7:])
    if en.startswith("a steam "):
        return "一只蒸汽" + _compose_tail(en[8:])
    if en.startswith("a water "):
        return "一只水" + _compose_tail(en[7:])
    if en.startswith("a lava "):
        return "一只熔岩" + _compose_tail(en[7:])
    if en.startswith("a magma "):
        return "一只岩浆" + _compose_tail(en[8:])
    if en.startswith("a plasma "):
        return "一只等离子" + _compose_tail(en[9:])
    if en.startswith("a poison "):
        return "一只剧毒" + _compose_tail(en[9:])
    if en.startswith("a lightning "):
        return "一只闪电" + _compose_tail(en[12:])
    if en.startswith("an earth "):
        return "一只大地" + _compose_tail(en[9:])
    if en.startswith("an air "):
        return "一只气" + _compose_tail(en[6:])
    if en.startswith("a wax "):
        return "一只蜡" + _compose_tail(en[6:])
    if en.startswith("a wooden "):
        return "一只木制" + _compose_tail(en[9:])
    if en.startswith("a rust "):
        return "一只锈蚀" + _compose_tail(en[7:])
    if en.startswith("a runic "):
        return "一只符文" + _compose_tail(en[8:])
    if en.startswith("a rune "):
        return "一只符文" + _compose_tail(en[7:])
    if en.startswith("a young "):
        return "一只幼年" + _compose_tail(en[8:])
    if en.startswith("a wailing "):
        return "一只哀嚎" + _compose_tail(en[10:])
    if en.startswith("a walking "):
        return "一只行尸" + _compose_tail(en[10:])
    if en.startswith("a soul "):
        return "一只灵魂" + _compose_tail(en[7:])
    if en.startswith("a phase "):
        return "一只相位" + _compose_tail(en[8:])
    if en.startswith("an ophidian "):
        return "一只蛇人" + _compose_tail(en[12:])
    if en.startswith("a psychic "):
        return "一名灵能者" + _compose_tail(en[10:]) if not en.startswith("a psychic of the bomb") else None
    if en.startswith("a worshipper "):
        return None  # handled by BIGRAM full
    if en.startswith("an illithid"):
        return "一只夺心魔" + ("" if en == "an illithid" else _compose_tail(en[11:].lstrip()))
    if en.startswith("an iron "):
        return "一只钢铁" + _compose_tail(en[8:])
    if en.startswith("an enslaved "):
        return "一名被奴役的" + _compose_tail(en[12:])
    if en.startswith("an aquatic "):
        return "一只水生" + _compose_tail(en[11:])
    if en.startswith("an ettin "):
        return "一只双头巨人" + _compose_tail(en[8:])
    if en.startswith("an umber "):
        return "一只暗褐" + _compose_tail(en[8:])
    if en.startswith("a corpse"):
        return "一具尸体" if en == "a corpse" else "一具" + _compose_tail(en[8:].lstrip())
    if en.startswith("a daemon"):
        return "一只魔族" if en == "a daemon" else "一只魔族" + _compose_tail(en[8:].lstrip())
    if en.startswith("a dragon"):
        return "一条龙" if en == "a dragon" else "一只龙" + _compose_tail(en[8:].lstrip())
    if en.startswith("a drake"):
        return "一只飞龙" if en == "a drake" else "一只飞龙" + _compose_tail(en[7:].lstrip())
    if en.startswith("a dragyn"):
        return "一只龙裔" if en == "a dragyn" else "一只龙裔" + _compose_tail(en[8:].lstrip())
    if en.startswith("a wyrm"):
        return "一条古龙" if en == "a wyrm" else "一条古龙" + _compose_tail(en[6:].lstrip())
    if en.startswith("a wyvra"):
        return "一只飞龙" if en == "a wyvra" else "一只飞龙" + _compose_tail(en[7:].lstrip())
    if en.startswith("a kraken"):
        return "一头海怪"
    if en.startswith("a leviathan"):
        return "一头利维坦"
    if en.startswith("a titan"):
        return "一名泰坦" if en == "a titan" else "一名泰坦" + _compose_tail(en[7:].lstrip())
    if en.startswith("a roc"):
        return "一头巨鹏"
    if en.startswith("a harpy"):
        return "一只鹰身女妖" + ("" if en == "a harpy" else _compose_tail(en[7:].lstrip()))
    if en.startswith("a medusa"):
        return "一名美杜莎"
    if en.startswith("a succubus"):
        return "一名魅魔"
    if en.startswith("a naga"):
        return "一条那伽"
    if en.startswith("a scorpion"):
        return "一只蝎" + _compose_tail(en[10:])
    if en.startswith("a tarantula"):
        return "一只狼蛛"
    if en.startswith("a spider"):
        return "一只蜘蛛" + _compose_tail(en[8:])
    if en.startswith("a snake"):
        return "一条蛇" + _compose_tail(en[7:])
    if en.startswith("a serpent"):
        return "一条巨蛇" + _compose_tail(en[9:])
    if en.startswith("a shark"):
        return "一条鲨" + _compose_tail(en[7:])
    if en.startswith("a crab"):
        return "一只蟹" + _compose_tail(en[6:])
    if en.startswith("a eel"):
        return "一条鳗" + _compose_tail(en[5:])
    if en.startswith("a troll"):
        return "一只巨魔" + ("" if en == "a troll" else _compose_tail(en[7:].lstrip()))
    if en.startswith("a mummy"):
        return "一具木乃伊"
    if en.startswith("a golem"):
        return "一尊魔像" + ("" if en == "a golem" else _compose_tail(en[7:].lstrip()))
    if en.startswith("a wight"):
        return "一只尸妖"
    if en.startswith("a ghoul"):
        return "一只食尸鬼" + ("" if en == "a ghoul" else _compose_tail(en[7:].lstrip()))
    if en.startswith("a banshee"):
        return "一名女妖"
    if en.startswith("a wraith"):
        return "一只怨灵"
    if en.startswith("a vampire"):
        return "一只吸血鬼" + ("" if en == "a vampire" else _compose_tail(en[9:].lstrip()))
    if en.startswith("a lich"):
        return "一名巫妖" + ("" if en == "a lich" else _compose_tail(en[6:].lstrip()))
    if en.startswith("a demon"):
        return "一只恶魔" + ("" if en == "a demon" else _compose_tail(en[7:].lstrip()))
    if en.startswith("a devil"):
        return "一只魔鬼" + ("" if en == "a devil" else _compose_tail(en[7:].lstrip()))
    if en.startswith("a balrog"):
        return "一只炎魔"
    if en.startswith("a balron"):
        return "一只魔将"
    if en.startswith("a vrock"):
        return "一只弗洛魔"
    if en.startswith("a bugbear"):
        return "一只熊地精"
    if en.startswith("a gnoll"):
        return "一只豺狼人"
    if en.startswith("a kobold"):
        return "一只狗头人" + ("" if en == "a kobold" else _compose_tail(en[8:].lstrip()))
    if en.startswith("a goblin"):
        return "一只哥布林" + ("" if en == "a goblin" else _compose_tail(en[8:].lstrip()))
    if en.startswith("a hobgoblin"):
        return "一只大地精"
    if en.startswith("an ogre"):
        return "一名食人魔" + ("" if en == "an ogre" else _compose_tail(en[7:].lstrip()))
    if en.startswith("an ettin"):
        return "一名双头巨人" + ("" if en == "an ettin" else _compose_tail(en[8:].lstrip()))
    if en.startswith("a minotaur"):
        return "一名牛头人" + ("" if en == "a minotaur" else _compose_tail(en[10:].lstrip()))
    if en.startswith("a mongbat"):
        return "一只蝠猴"
    if en.startswith("a monstrous"):
        return "一只巨怪" + _compose_tail(en[11:])
    if en.startswith("a phantom"):
        return "一只幻影"
    if en.startswith("a reaper"):
        return "一名收割者"
    if en.startswith("a roc"):
        return "一头巨鹏"
    if en.startswith("a revenant"):
        return "一名还魂尸"
    if en.startswith("a rot"):
        return "一只" + _compose_tail(en[2:])  # fallback
    if en.startswith("a rotting"):
        return "一只腐溃" + _compose_tail(en[9:])
    if en.startswith("a rotten"):
        return "一只腐烂" + _compose_tail(en[8:])
    if en.startswith("a decaying"):
        return "一只腐朽" + _compose_tail(en[10:])
    if en.startswith("a frail"):
        return "一只枯瘦" + _compose_tail(en[7:])
    if en.startswith("a lesser"):
        return "一只低阶" + _compose_tail(en[8:])
    if en.startswith("a greater"):
        return "一只高阶" + _compose_tail(en[9:])
    if en.startswith("a dark"):
        return "一只黑暗" + _compose_tail(en[6:])
    if en.startswith("a dread"):
        return "一只恐惧" + _compose_tail(en[7:])
    if en.startswith("a death"):
        return "一只死亡" + _compose_tail(en[7:])
    if en.startswith("a deadly"):
        return "一只致命" + _compose_tail(en[8:])
    if en.startswith("a brass"):
        return "一只黄铜" + _compose_tail(en[7:])
    if en.startswith("a golden"):
        return "一只黄金" + _compose_tail(en[8:])
    if en.startswith("a silver"):
        return "一只银" + _compose_tail(en[8:])
    if en.startswith("a jade"):
        return "一只翡翠" + _compose_tail(en[6:])
    if en.startswith("a gray"):
        return "一只灰" + _compose_tail(en[6:])
    if en.startswith("a white"):
        return "一只白" + _compose_tail(en[7:])
    if en.startswith("a ghost"):
        return "一只幽灵" + ("" if en == "a ghost" else _compose_tail(en[7:].lstrip()))
    if en.startswith("a ghoul"):
        return "一只食尸鬼" + ("" if en == "a ghoul" else _compose_tail(en[7:].lstrip()))
    if en.startswith("a haunt"):
        return "一只怨灵"
    if en.startswith("a shade"):
        return "一只阴魂"
    if en.startswith("a spirit"):
        return "一只灵体"
    if en.startswith("a spectre"):
        return "一只怨灵"
    if en.startswith("a specter"):
        return "一只幽影"
    if en.startswith("a skeleton"):
        return "一具骷髅"
    if en.startswith("a skellot"):
        return "一只骨奴"
    if en.startswith("a spinner"):
        return "一只织网者"
    if en.startswith("a succubus"):
        return "一名魅魔"
    if en.startswith("a tomb"):
        return "一只墓穴" + _compose_tail(en[6:])
    if en.startswith("a unicorn"):
        return "一只独角兽"
    if en.startswith("a vampyre"):
        return "一名血族"
    if en.startswith("a wisp"):
        return "一只精魂"
    if en.startswith("a zombie"):
        return "一只僵尸" + ("" if en == "a zombie" else _compose_tail(en[8:].lstrip()))
    if en.startswith("an air "):
        return "一只气" + _compose_tail(en[6:])
    if en.startswith("an anaconda"):
        return "一条森蚺"
    if en.startswith("an elephant"):
        pass
    # generic "a X Y" / "an X"
    if en.startswith("a ") and len(en) > 2:
        return "一只" + _compose_tail(en[2:])
    if en.startswith("an ") and len(en) > 3:
        return "一只" + _compose_tail(en[3:])
    return None


def build_echo_map() -> dict[str, str]:
    data = json.loads(TABLE.read_text(encoding="utf-8"))
    echo = [k for k, v in data.items() if isinstance(v, str) and v.strip() == k.strip()]
    out: dict[str, str] = {}
    for k in echo:
        if k in ROLE_SINGLE:
            out[k] = ROLE_SINGLE[k]
            continue
        if k in FULL:
            out[k] = FULL[k]
            continue
        # bomb worshipper exact
        if k == "a worshipper of the bomb":
            out[k] = "一名炸弹教崇拜者"
            continue
        if k == "a psychic of the bomb":
            out[k] = "一名炸弹教灵能者"
            continue
        z = _zh_creature_line(k)
        if z:
            out[k] = z
    return out


def main() -> int:
    m = build_echo_map()
    OUT.write_text(json.dumps(m, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print("wrote", len(m), "creature echo entries ->", OUT)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

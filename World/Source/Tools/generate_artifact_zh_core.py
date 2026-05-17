#!/usr/bin/env python3
"""
One-shot generator: builds artifact_zh_core.json (EN display -> Chinese core, no parens).
Run from repo root: python3 World/Source/Tools/generate_artifact_zh_core.py
"""
from __future__ import annotations

import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
OUT = Path(__file__).resolve().parent / "artifact_zh_core.json"
EQ_ZH = ROOT / "Data/Localization/zh-Hans/equipment-properties.json"
EQ_EN = ROOT / "Data/Localization/en/equipment-properties.json"

DUP_RE = re.compile(r"^(.+?)（\1）$")

OF: list[tuple[str, str]] = sorted(
    [
        ("Good Fortune", "鸿运"),
        ("Cold Light", "寒辉"),
        ("Prismatic Magic", "虹彩法术"),
        ("The Lurker", "海德罗斯"),
        ("The Mountain King", "利托斯"),
        ("The Daemon King", "皮罗斯"),
        ("The Mystic Voice", "斯特拉托斯"),
        ("The Fallen King", "陨落君王"),
        ("The Harrower", "冥劫者"),
        ("The False Goddess", "伪神"),
        ("The Juka King", "朱卡之王"),
        ("The Polar Bear", "北极熊灵"),
        ("The Sorceress", "女巫"),
        ("The Pugilist", "拳师"),
        ("The Cu Sidhe", "库希仙灵"),
        ("The Serpent", "巨蛇"),
        ("The Elements", "元素"),
        ("The Minotaur", "弥诺陶洛斯"),
        ("The Phoenix", "凤凰"),
        ("The Beast", "巨兽"),
        ("The Titans", "泰坦"),
        ("The Righteous", "义人"),
        ("The Gods", "诸神"),
        ("The Heavens", "天界"),
        ("The Magician", "法师"),
        ("The Magi", "贤者"),
        ("The Vile", "邪秽"),
        ("The Shadows", "暗影"),
        ("The Dryad", "树妖"),
        ("The Eclipse", "日蚀"),
        ("The Equinox", "昼夜平分"),
        ("The Cimmerian", "西米里人"),
        ("The Rogue", "侠盗"),
        ("Fortune", "幸运"),
        ("Insight", "洞察"),
        ("Nobility", "高贵"),
        ("Aegis", "神盾"),
        ("Grace", "恩典"),
        ("Bane", "灾厄"),
        ("Fire", "烈焰"),
        ("Deceit", "欺诈"),
        ("Enlightenment", "悟道"),
        ("Embers", "余烬"),
        ("Health", "生命"),
        ("Dexterity", "敏捷"),
        ("Regeneration", "再生"),
        ("Corruption", "腐化"),
        ("Swiftness", "迅捷"),
        ("Brilliance", "璀璨"),
        ("Protection", "护佑"),
        ("Venom", "剧毒"),
        ("Shadows", "幽影"),
        ("Trap Burning", "焚陷"),
        ("Infinity", "无极"),
        ("Ice", "寒冰"),
        ("Lightning", "迅雷"),
        ("Rage", "狂怒"),
        ("Blight", "凋朽"),
        ("Toxicity", "剧毒"),
        ("Anger", "暴怒"),
        ("Insanity", "癫狂"),
        ("Treason", "叛逆"),
        ("Knowledge", "真知"),
        ("Souls", "群魂"),
        ("Seeing", "洞视"),
        ("Battle", "战意"),
        ("Death", "冥亡"),
        ("Teleportation", "传送"),
        ("Dragons", "巨龙"),
    ],
    key=lambda x: len(x[0]),
    reverse=True,
)

HEAD: dict[str, str] = {
    "Armor": "护铠",
    "Arms": "臂甲",
    "Axe": "战斧",
    "Bandana": "面巾",
    "Bauble": "小饰物",
    "Beacon": "信标",
    "Blade": "利刃",
    "Book": "典籍",
    "Boots": "长靴",
    "Bottle": "瓶",
    "Bow": "弓",
    "Box": "匣",
    "Bracelet": "手镯",
    "Breastplate": "胸甲",
    "Britches": "衬裤",
    "Candelabra": "烛台",
    "Cape": "斗篷",
    "Cap": "便帽",
    "Chest": "胸甲",
    "Chest Plate": "胸甲",
    "Circlet": "头环",
    "Clasp": "扣环",
    "Club": "棍",
    "Coat": "外套",
    "Coif": "头罩",
    "Collar": "项圈",
    "Countenance": "容颜",
    "Crossbow": "重弩",
    "Cincture": "束腰",
    "Cloak": "斗篷",
    "Crown": "冠冕",
    "Cutlass": "弯刀",
    "Dagger": "匕首",
    "Embrace": "圣拥",
    "Earrings": "耳环",
    "Flame": "烈焰",
    "Fang": "獠牙",
    "Gauntlets": "护手",
    "Gem": "宝石",
    "Gloves": "手套",
    "Gorget": "护颈",
    "Grimoire": "魔典",
    "Hammer": "战锤",
    "Harp": "竖琴",
    "Hat": "帽",
    "Hatchet": "手斧",
    "Headdress": "头饰",
    "Helm": "头盔",
    "Hood": "兜帽",
    "Knife": "刀",
    "Lance": "骑枪",
    "Lantern": "提灯",
    "Lexicon": "法术宝典",
    "Legging": "护腿",
    "Leggings": "护腿",
    "Legs": "腿甲",
    "Loin Cloth": "腰布",
    "Loaf": "面包",
    "Longbow": "长弓",
    "Lute": "鲁特琴",
    "Maul": "重锤",
    "Manual": "秘典",
    "Mask": "面具",
    "Mempo": "面具",
    "Mittens": "连指手套",
    "Needle": "针",
    "Neck": "颈环",
    "Necklace": "项链",
    "Obi": "和服腰带",
    "Ornament": "饰物",
    "Pads": "护垫",
    "Pendant": "护符",
    "Pickaxe": "镐",
    "Pitchfork": "草叉",
    "Plate Helm": "板甲盔",
    "Quiver": "箭袋",
    "Rapier": "细剑",
    "Robe": "长袍",
    "Rod": "节杖",
    "Rum": "朗姆酒",
    "Sash": "腰带",
    "Scepter": "权杖",
    "Scimitar": "弯刀",
    "Scalpel": "手术刀",
    "Scythe": "长柄镰",
    "Shield": "盾牌",
    "Shroud": "裹布",
    "Spear": "长矛",
    "Staff": "法杖",
    "Statuette": "小像",
    "Steed": "木马",
    "Survival Knife": "求生刀",
    "Sword": "剑",
    "Talisman": "护符",
    "Thrasher": "碎击",
    "Tome": "巨著",
    "Torch": "火把",
    "Totem": "图腾",
    "Tunic": "上衣",
    "Visage": "容貌",
    "Whip": "鞭",
    "Belt": "腰带",
    "Mantle": "披风",
    "Candle": "蜡烛",
    "Aura": "灵光",
    "Essence": "精粹",
    "Heart": "圣心",
    "Horn": "号角",
    "Legacy": "遗赠",
    "Bramble": "荆棘",
    "Fur": "皮草",
    "Swatter": "拍子",
    "Nightlight": "夜灯",
    "Ring": "戒指",
    "Blaze": "冥焰",
}

HSORT = sorted(HEAD.keys(), key=len, reverse=True)

PREFIX_MOD: list[tuple[str, str]] = sorted(
    [
        ("Spell Woven", "织咒"),
        ("Royal Archer's", "王室弓手"),
        ("Royal Guard's", "王家卫队"),
        ("Royal Guard", "王家卫队"),
        ("Holy Knight's", "圣骑士"),
        ("Nox Ranger's", "诺克斯游侠"),
        ("Dark Guardian's", "暗黑守护者"),
        ("Dark Lord's", "暗黑魔君"),
        ("Royal ", "王室"),
        ("Holy ", "神圣"),
        ("Dark ", "暗影"),
        ("Blight Gripped", "疫握"),
        ("Luminous", "烁光"),
        ("Vampiric", "嗜血"),
        ("Enchanted", "魔咒"),
        ("Necromancer", "死灵法师"),
    ],
    key=lambda x: len(x[0]),
    reverse=True,
)


def fix_apostrophe(s: str) -> str:
    return s.replace("\u2019", "'").replace("\ufffd", "'").replace("`", "'")


def head_zh(h: str) -> str | None:
    t = h.strip()
    if t in HEAD:
        return HEAD[t]
    for pref_en, pref_zh in PREFIX_MOD:
        if t.startswith(pref_en):
            rest = t[len(pref_en) :].strip()
            if rest in HEAD:
                return pref_zh + HEAD[rest]
            break
    for k in HSORT:
        if t.endswith(" " + k):
            pfx = t[: -len(k) - 1].strip()
            if not pfx:
                continue
            zh_k = HEAD[k]
            for pref_en, pref_zh in PREFIX_MOD:
                if pfx.startswith(pref_en.strip()):
                    rest2 = pfx[len(pref_en) :].strip()
                    if not rest2:
                        return pref_zh + zh_k
            if pfx in ("Giant", "Spell"):
                pm = {"Giant": "巨型", "Spell": "法术"}
                return pm.get(pfx, pfx) + zh_k
    return None


def of_pattern(en: str) -> str | None:
    e = fix_apostrophe(en)
    for phrase, zh_t in OF:
        suf = " Of " + phrase
        if e.endswith(suf):
            head = e[: -len(suf)].strip()
            zh_h = head_zh(head)
            if zh_h:
                return zh_t + zh_h
    return None


POS_OWNER: dict[str, str] = {
    "Achille": "阿喀琉斯",
    "Ailric": "艾尔里克",
    "Alchemist": "炼金师",
    "Burglar": "窃贼",
    "Captain John": "约翰船长",
    "Captain Quacklebush": "夸克尔布什船长",
    "Djinni": "镇尼",
    "Dupre": "杜普雷",
    "Gwenno": "格温诺",
    "Iolo": "伊欧洛",
    "Jackal": "豺狼",
    "Melisande": "梅莉桑德",
    "Ramus": "拉摩斯",
    "Raed": "瑞德",
    "Robin Hood": "罗宾汉",
    "Shamino": "沙米诺",
    "Yashimoto": "吉本",
    "Grim Reaper": "死神",
    "Gladiator": "角斗士",
    "Hunter": "猎人",
    "Inquisitor": "审判官",
    "Mage": "法师",
    "Magician": "魔术师",
    "Madman": "狂人",
    "Miner": "矿工",
    "Nox Ranger": "诺克斯游侠",
    "Ossian": "奥西恩",
    "Phillips": "菲利普斯",
    "Royal Guard": "王家卫队",
    "Serpent": "毒蛇",
    "Silvani": "西尔瓦妮",
    "Stitcher": "针线匠",
    "Warrior": "战士",
}


def possessive(en: str) -> str | None:
    e = fix_apostrophe(en)
    m = re.match(r"^(.+)'s (.+)$", e) or re.match(r"^(.+)' (.+)$", e)
    if not m:
        return None
    owner, thing = m.group(1).strip(), m.group(2).strip()
    zh_o = POS_OWNER.get(owner)
    if not zh_o:
        return None
    zh_t = HEAD.get(thing)
    if zh_t:
        return f"{zh_o}的{zh_t}"
    # multi-word things
    MAP_THING = {
        "Heavy Crossbow": "重弩",
        "Wooden Steed": "木马",
        "Feathered Hat": "羽饰帽",
        "Survival Knife": "求生刀",
        "Corroded Hatchet": "锈蚀手斧",
        "Necromantic Scalpel": "死灵手术刀",
        "Feywood Bow": "灵木弓",
        "Arm Plates": "臂甲片",
        "Breastplate": "胸甲",
        "Legging": "护腿",
        "Chest Plate": "胸甲",
        "Resolution": "决心",
        "Illusion": "幻象",
    }
    zt = MAP_THING.get(thing)
    if zt:
        return f"{zh_o}的{zt}"
    return None


# Explicit cores for names / myth / pop culture / odd grammar
PATCH: dict[str, str] = {
    "Excalibur": "王者之剑",
    "Aegis": "圣盾埃癸斯",
    "Admirals Hearty Rum": "海军上将浓情朗姆",
    "Aegis Of Grace": "恩典神盾",
    "Necromancer Shroud": "死灵法师裹布",
    "Arctic Death Dealer": "极寒死亡使者",
    "Arty": "雅号「阿蒂」",
    "Anger Of The Gods": "诸神之怒",
    "Annihilation": "湮灭之光",
    "Angelic Embrace": "天使圣拥",
    "Aura Of Shadows": "幽影灵光",
    "Blaze Of Death": "冥灭烈焰",
    "Belt Of Hercules": "海格力斯腰带",
    "Blade Dance": "刃舞",
    "Bloodwood Spirit": "血木精魄",
    "Bone Crusher": "碎骨者",
    "Bonesmasher": "碎骨锤",
    "Boomstick": "轰鸣火铳",
    "Boots Of Hermes": "赫尔墨斯长靴",
    "Bramble Coat": "荆棘外套",
    "Brave Knight Of Sosaria": "索萨利亚勇骑斗篷",
    "Breath Of The Dead": "亡者吐息",
    "Cavorting Club": "嬉闹短棍",
    "Circlet Of The Sorceress": "女巫头环",
    "Cloak Of The Rogue": "侠盗斗篷",
    "Cold Blood": "冷血之锋",
    "Cold Forged Blade": "寒锻利刃",
    "Crimson Cincture": "绯红束腰",
    "Crown Of Tal'keesh": "塔尔基什冠冕",
    "Dark Guardian's Chest": "暗黑守护者胸甲",
    "Dark Lord's Pitchfork": "暗黑魔君草叉",
    "Dark Neck": "幽暗颈环",
    "Detective Boots Of The Royal Guard": "王家卫队侦探长靴",
    "Divine Countenance": "神圣容颜",
    "Dread Pirate Hat": "惊心海盗帽",
    "Dryad Bow": "树妖弓",
    "Enchanted Pirate Rapier": "魔咒海盗细剑",
    "Festering Wound": "溃烂创伤",
    "Flesh Ripper": "裂肉者",
    "Fortunate Blades": "幸运双锋",
    "Frostbringer": "霜噬者",
    "Fur Cape Of The Sorceress": "女巫皮草斗篷",
    "Fury": "狂烈",
    "Geishas Obi": "艺伎和服腰带",
    "Ghost Ship Anchor": "幽灵船锚",
    "Giant Blackjack": "巨型短棍",
    "Hammer Of Thor": "索尔之锤",
    "Horn Of King Triton": "特里同王号角",
    "Heart Of The Lion": "雄狮圣心",
    "Holy Lance": "圣骑枪",
    "Holy Sword": "圣洁长剑",
    "Indecency": "无礼之举",
    "Inquisitor's Resolution": "审判官决心",
    "Ironwood Crown": "铁木冠冕",
    "Jade Scimitar": "碧玉弯刀",
    "Jester Hat Of Chuckles": "哗笑小丑帽",
    "Jin-baori Of Good Fortune": "鸿运阵羽织",
    "Kami-naris Indestructable Axe": "雷神不坏双刃斧",
    "Lantern Of Power": "威能提灯",
    "Legacy Of The Dread Lord": "惧魔君王遗赠",
    "Long Shot": "远射弓",
    "Lucky Necklace": "幸运项链",
    "Luminous Rune Blade": "烁光符文刃",
    "Madman's Hatchet": "狂人手斧",
    "Night Reaper": "夜幕收割者",
    "Night's Kiss": "夜吻匕首",
    "Nordic Dragon Blade": "北欧龙刃",
    "Nox Bow": "诺克斯弓",
    "Nox Nightlight": "诺克斯夜灯",
    "Oblivion Needle": "湮灭针芒",
    "Orcish Visage": "兽人容貌",
    "Ornate Crown Of The Harrower": "华丽冥劫者冠冕",
    "Ossian Grimoire": "奥西恩魔典",
    "Overseer Sundered Blade": "监工裂解之刃",
    "Pacify": "平息",
    "Pandora's Box": "潘多拉之匣",
    "Pestilence": "疫祸",
    "Phantom Staff": "魅影法杖",
    "Phillips Wooden Steed": "菲利普斯木马",
    "Pixie Swatter": "仙子拍",
    "Polar Bear Cape": "北极熊斗篷",
    "Quell": "镇压",
    "Raed's Glory": "瑞德荣耀之锋",
    "Resillient Bracer": "坚韧臂箍",
    "Retort": "反诘",
    "Righteous Anger": "义愤",
    "Robe Of Sosaria": "索萨利亚长袍",
    "Royal Guard Sash": "王家卫队腰带",
    "Royal Guard Survival Knife": "王家卫队求生刀",
    "Rune Carving Knife": "符文雕刻刀",
    "Seahorse Statuette": "海马小像",
    "Shamino's Crossbow": "沙米诺重弩",
    "Shard Thrasher": "裂片碎击者",
    "Shield Of Invulnerability": "无敌盾牌",
    "Shimmering Talisman": "闪烁护符",
    "Ship Model Of The H M S Cape": "凯普号舰模",
    "Slayer Of Dragons": "屠龙之刃",
    "Soul Seeker": "觅魂者",
    "Spell Woven Britches": "织咒衬裤",
    "Spirit Of The Polar Bear": "北极熊灵披风",
    "Spirit Of The Totem": "图腾之灵",
    "Staff Of Power": "威能法杖",
    "Staff Of The Serpent": "巨蛇法杖",
    "Stormbringer": "风暴使者",
    "Subdue": "慑服",
    "Swift Strike": "疾袭",
    "Sword Of Shattered Hopes": "碎梦之剑",
    "Sword Of Sinbad": "辛巴达之剑",
    "Talon Bite": "利爪撕咬",
    "Taskmaster": "峻法监吏",
    "Titan's Hammer": "泰坦之锤",
    "Torch Of Trap Burning": "焚陷火把",
    "Totem Of The Void": "虚空图腾",
    "Vampire Killer": "灭吸血鬼长鞭",
    "Vampiric Daisho": "嗜血大小对剑",
    "Violet Courage": "紫罗兰勇气",
    "Voice Of The Fallen King": "陨落君王之声",
    "Warrior's Clasp": "战士扣环",
    "Wildfire Bow": "野火弓",
    "Windsong": "风歌之弓",
    "Winter Beacon": "凛冬信标",
    "Wizard's Pants": "法师长裤",
    "Wrath Of The Dryad": "树妖之怒",
    "Yashimoto's Hatsuburi": "吉本兜帽面甲",
    "Zyronic Claw": "扎龙尼克之爪",
    "Fang Of Ractus": "拉库图斯之牙",
    "Gold Bricks": "金砖",
    "Calm": "平息",
    "Iolo's Lute": "伊欧洛鲁特琴",
    "Merlin's Mystical Hat": "梅林奇术帽",
    "Merlin's Mystical Staff": "梅林奇术杖",
    "Book Of Prismatic Magic": "虹彩法术典籍",
    "Mask Of Death": "冥亡面具",
    "Scepter Of The False Goddess": "伪神权杖",
    "Blade Of The Cimmerian": "西米里人之刃",
    "Helm Of The Cimmerian": "西米里人头盔",
    "Loin Cloth Of The Cimmerian": "西米里人腰布",
    "Dupre's Collar": "杜普雷项圈",
    "Book Of Knowledge": "真知典籍",
    "Candelabra Of Souls": "群魂烛台",
    "Candle Of Cold Light": "寒辉蜡烛",
    "Everlasting Bottle": "不竭之瓶",
    "Everlasting Loaf": "不竭面包",
    "Essence Of Battle": "战意精粹",
    "Eternal Flame": "不灭烈焰",
    "Gem Of Seeing": "洞视宝石",
    "Rod Of Resurrection": "复活节杖",
    "Pendant Of The Magi": "贤者护符",
    "Staff Of The Magi": "贤者法杖",
    "Hat Of The Magi": "贤者帽",
    "Grimoire Of The Daemon King": "皮罗斯魔典",
    "Lexicon Of The Lurker": "海德罗斯法术宝典",
    "Manual Of The Mystic Voice": "斯特拉托斯秘典",
    "Tome Of The Mountain King": "利托斯巨著",
    "Royal Leggings Of Embers": "余烬王室护腿",
    "Blade Of The Shadows": "暗影之刃",
    "Shroud Of Deceit": "欺诈裹布",
    "Shroud Of Shadows": "幽影裹布",
    "Miner's Pickaxe": "矿工镐",
    "Mage's Band": "法师束带",
    "Magician's Illusion": "魔术师幻象",
    "Magician's Mempo": "魔术师面具",
    "Berserker's Maul": "狂战士重锤",
    "Maul Of The Beast": "巨兽重锤",
    "Maul Of The Titans": "泰坦重锤",
    "Holy Knight's Arm Plates": "圣骑士臂甲片",
    "Holy Knight's Breastplate": "圣骑士胸甲",
    "Holy Knight's Legging": "圣骑士护腿",
    "Royal Guard's Chest Plate": "王家卫队胸甲",
    "Royal Archer's Bow": "王室弓手长弓",
    "Nox Ranger's Heavy Crossbow": "诺克斯游侠重弩",
}

def core_line(en: str) -> str:
    e = fix_apostrophe(en)
    if e in PATCH:
        return PATCH[e]
    r = of_pattern(e)
    if r:
        return r
    r = possessive(e)
    if r:
        return r
    # Tal'keesh crown etc.
    if " Of Tal'keesh" in e:
        return "塔尔基什冠冕" if e.startswith("Crown") else e
    zh_h = head_zh(e)
    if zh_h:
        return zh_h
    # last resort: title words
    return e


def main() -> None:
    zd = json.loads(EQ_ZH.read_text(encoding="utf-8"))
    ed = json.loads(EQ_EN.read_text(encoding="utf-8"))
    ens: list[str] = sorted(
        {
            ed[k].strip()
            for k, v in zd.items()
            if k.startswith("item.magical.artifact.") and DUP_RE.match(v.strip())
        }
    )
    out: dict[str, str] = {}
    missing: list[str] = []
    for e in ens:
        c = core_line(e)
        if c == e and e not in PATCH:
            # still English — mark
            missing.append(e)
        out[e] = c
    OUT.write_text(json.dumps(out, ensure_ascii=False, indent=2), encoding="utf-8")
    print("written", OUT, "entries", len(out))
    if missing:
        print("MISSING", len(missing))
        for m in missing:
            print(" ", m)


if __name__ == "__main__":
    main()

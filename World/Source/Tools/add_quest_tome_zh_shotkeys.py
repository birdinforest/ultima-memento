#!/usr/bin/env python3
"""Add quest tome localization shotkeys for epic NPCs, evil titles, relic parts, and reagents."""
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
EN = ROOT / "Data/Localization/en/world-player-text.json"
ZH = ROOT / "Data/Localization/zh-Hans/world-player-text.json"
GLOSS = ROOT / "Data/Localization/glossary-approved-zh.json"
FRAGMENTS = ROOT / "Data/Localization/quest-fragment-zh-table.json"

EPIC_NPCS = {
    "Lord British": ("不列颠王", {"the King of Britain": "不列颠之王"}),
    "Lord Blackthorne": ("黑索恩勋爵", {"the Ruler of Kuldar": "库尔达统治者"}),
    "Lord Draxinusom": ("德拉克西诺索姆", {"the Gargoyle King": "石像鬼之王"}),
    "Mondain": ("蒙丹", {"the Wizard": "巫师"}),
    "Minax": ("米纳克斯", {"the Enchantress": "女巫"}),
    "Morphius": ("墨菲乌斯", {"the Vile Lich": "邪恶巫妖"}),
    "Tyball": ("泰鲍尔", {"the Demonologist": "恶魔学家"}),
    "Arcadion": ("阿卡迪翁", {"the Daemon": "恶魔"}),
    "Samhayne": ("萨姆海恩", {"the Master Sailor": "航海大师"}),
    "Seggallion": ("塞加利翁", {"the Pirate Lord": "海盗领主"}),
    "Nystal": ("尼斯特尔", {"the Royal Wizard": "皇家巫师"}),
    "Geoffrey": ("杰弗里", {"the Knight": "骑士"}),
    "Shimazu": ("岛津", {"the Shogun Samurai": "将军武士"}),
    "Gorn": ("戈恩", {"the King of Cimmeran": "西默兰之王"}),
    "Jaana": ("雅娜", {"the Herb Healer": "草药治疗师"}),
    "Dupre": ("杜普雷", {"the Paladin": "圣骑士"}),
    "Gwenno": ("格温诺", {"the Bard": "吟游诗人"}),
    "Iolo": ("爱罗", {"the Bowman": "弓箭手"}),
    "Shamino": ("沙米诺", {"the Woodsman": "林野人"}),
    "Stefano": ("斯特凡诺", {"the Sneak": "潜行者"}),
    "Katrina": ("卡特里娜", {"the Shepherd": "牧羊人"}),
    "the Guardian": ("守护者", {}),
    "Garamon": ("加拉蒙", {"the Wizard": "巫师"}),
    "Mors Gotha": ("莫斯·戈萨", {"the Death Knight": "死亡骑士"}),
    "Lethe": ("莱瑟", {"the Dreaded Lich": "可怖巫妖"}),
    "the Great Earth Serpent": ("大地巨蛇", {}),
}

EVIL_SUBS = {
    "Ruler": "统治者", "Warlord": "军阀", "Lord": "领主", "Overseer": "监工",
    "Servant": "仆从", "Dweller": "居者", "Slave": "奴仆", "Eye": "之眼", "Hand": "之手",
    "Heart": "之心", "Minion": "爪牙", "Master": "主宰", "Conqueror": "征服者",
    "Leader": "首领", "Herald": "传令者", "Omen": "预兆", "Bearer": "承载者",
    "Sign": "征兆", "Disciple": "门徒",
}

EVIL_ADJ = {
    "Mad": "疯狂", "Hated": "可憎", "Feared": "可惧", "Cursed": "受诅咒", "Scorned": "被轻蔑",
    "Despised": "被唾弃", "Lost": "迷失", "Insane": "癫狂", "Deranged": "错乱", "Demented": "痴呆",
    "Blighted": "腐化", "Corrupt": "堕落", "Angry": "暴怒", "Wicked": "邪恶", "Loathsome": "可憎",
    "Baneful": "祸害", "Cruel": "残忍", "Atrocious": "凶残", "Barbarous": "野蛮", "Brutal": "残暴",
    "Heartless": "无情", "Merciless": "冷酷", "Ruthless": "狠辣", "Sadistic": "虐杀", "Tyrannical": "暴虐",
    "Vicous": "恶毒", "Bloodthirsty": "嗜血", "Ferocious": "凶猛", "Fierce": "凶猛", "Malevolent": "恶意",
    "Loathed": "被憎恶",
}

EVIL_THEME = {
    "Evil": "邪恶", "the Corrupt": "腐化", "Destruction": "毁灭", "the Hated": "憎恨",
    "the Heinous": "可怖", "the Malevolent": "恶意", "the Malicious": "恶毒", "the Nefarious": "奸邪",
    "the Wicked": "邪恶", "the Vicious": "凶残", "the Vile": "卑劣", "Villainy": "恶行",
    "the Foul": "污秽", "Damnation": "诅咒", "Terror": "恐怖", "the Cursed": "受诅咒",
    "Doom": "末日", "Dire": "凶兆", "Death": "死亡", "the Sinister": "邪异",
    "Darkness": "黑暗", "the Mad": "疯狂", "the Insane": "癫狂", "Fire": "火焰", "Ice": "寒冰",
    "the Void": "虚空", "the Demon": "恶魔", "the Angel": "天使", "the Devil": "魔鬼",
    "Life": "生命", "the Light": "光明", "the Dark": "黑暗", "the Spirits": "灵魂",
    "the Dead": "亡者", "Hades": "冥府", "Venom": "剧毒", "the Planes": "位面",
    "Magic": "魔法", "Power": "力量", "Might": "威能", "Legend": "传说", "Wonder": "奇迹",
}

RELIC_NOUNS = {
    "Amulet": "护符", "Armor": "护甲", "Axe": "战斧", "Bag": "袋", "Belt": "腰带", "Blade": "利刃",
    "Bones": "骸骨", "Book": "书", "Boots": "靴", "Bottle": "瓶", "Bow": "弓", "Bracelet": "手镯",
    "Candle": "蜡烛", "Cape": "披风", "Chalice": "圣杯", "Cloak": "斗篷", "Club": "棍棒",
    "Codex": "典籍", "Crossbow": "弩", "Crown": "王冠", "Crystal Ball": "水晶球", "Cutlass": "弯刀",
    "Dagger": "匕首", "Drum": "鼓", "Dust": "尘", "Earrings": "耳环", "Elixir": "灵药", "Flute": "长笛",
    "Gem": "宝石", "Gloves": "手套", "Goblet": "高脚杯", "Halberd": "戟", "Hat": "帽",
    "Helm": "头盔", "Horn": "号角", "Key": "钥匙", "Knife": "刀", "Kryss": "细剑", "Lantern": "灯笼",
    "Lexicon": "词典", "Lute": "鲁特琴", "Mace": "钉锤", "Mirror": "镜", "Necklace": "项链",
    "Parchment": "羊皮纸", "Portrait": "肖像", "Potion": "药剂", "Pouch": "锦囊", "Ring": "戒指",
    "Robe": "长袍", "Rod": "杖", "Rope": "绳", "Scabbard": "剑鞘", "Sceptre": "权杖", "Scimitar": "弯刀",
    "Scroll": "卷轴", "Shackles": "镣铐", "Shield": "盾", "Skull": "颅骨", "Spellbook": "法术书",
    "Staff": "法杖", "Stone": "石", "Sword": "剑", "Tablet": "石板", "Tome": "典籍", "Trident": "三叉戟", "Veil": "面纱",
    "Wand": "魔杖", "Warhammer": "战锤",
}

RELIC_ADJ = {
    "Exotic": "奇异", "Mysterious": "神秘", "Enchanted": "附魔", "Marvelous": "奇妙", "Amazing": "惊人",
    "Astonishing": "骇人", "Mystical": "玄妙", "Astounding": "骇人", "Magical": "魔法",
    "Divine": "神圣", "Excellent": "卓越", "Magnificent": "宏伟", "Phenomenal": "惊异",
    "Fantastic": "奇幻", "Incredible": "难以置信", "Extraordinary": "非凡", "Fabulous": "惊世",
    "Wondrous": "奇妙", "Glorious": "辉煌", "Lost": "失落", "Fabled": "传说", "Legendary": "传奇",
    "Mythical": "神话", "Missing": "失踪", "Ancestral": "先祖", "Ornate": "华美", "Ultimate": "终极",
    "Rare": "稀有", "Wonderful": "奇妙", "Sacred": "神圣", "Almighty": "全能", "Supreme": "至高",
    "Mighty": "威猛", "Unspeakable": "不可名状", "Unknown": "未知", "Forgotten": "被遗忘",
    "Cursed": "受诅咒", "Glowing": "发光", "Dark": "黑暗", "Evil": "邪恶", "Holy": "神圣",
    "Vile": "卑劣", "Ethereal": "灵幽", "Demonic": "恶魔", "Burning": "燃烧", "Angelic": "天使",
    "Frozen": "冰冻", "Icy": "冰寒", "Blackened": "焦黑", "Lunar": "月华", "Solar": "日曜",
    "Bright": "明亮", "Electrical": "闪电", "Deathly": "死寂", "Hexed": "中咒", "Unholy": "不洁",
    "Blessed": "祝福", "Infernal": "地狱", "Damned": "天谴", "Doomed": "注定毁灭",
    "mystical": "玄妙", "magical": "魔法", "enchanted": "附魔", "cursed": "受诅咒",
    "ancient": "远古", "tainted": "污染", "charmed": "魅惑", "ensorcelled": "附咒",
    "powerful": "强大",
}

RELIC_CREATURE = {
    "ant": "蚁", "animal": "兽", "bat": "蝙蝠", "bear": "熊", "beetle": "甲虫", "boar": "野猪",
    "brownie": "棕仙", "bugbear": "熊地精", "basilisk": "石化蜥", "bull": "公牛", "froglok": "蛙人",
    "cat": "猫", "centaur": "半人马", "chimera": "奇美拉", "cow": "牛", "crocodile": "鳄鱼",
    "cyclops": "独眼巨人", "dark elf": "暗精灵", "demon": "恶魔", "devil": "魔鬼", "doppelganger": "变形怪",
    "dragon": "龙", "drake": "幼龙", "dryad": "树精", "dwarf": "矮人", "elf": "精灵", "ettin": "双头巨人",
    "frog": "蛙", "gargoyle": "石像鬼", "ghoul": "食尸鬼", "giant": "巨人", "gnoll": "豺狼人",
    "gnome": "侏儒", "goblin": "地精", "gorilla": "猩猩", "gremlin": "小妖", "griffin": "狮鹫",
    "hag": "巫婆", "hobbit": "霍比特", "harpy": "鹰身女妖", "hippogriff": "骏鹰", "hobgoblin": "大地精",
    "horse": "马", "hydra": "九头蛇", "imp": "小恶魔", "kobold": "狗头人", "kraken": "克拉肯",
    "leprechaun": "小妖精", "lizard": "蜥蜴", "lizard man": "蜥蜴人", "medusa": "美杜莎", "human": "人类",
    "minotaur": "牛头怪", "mouse": "鼠", "naga": "娜迦", "nightmare": "梦魇", "nixie": "水妖",
    "ogre": "食人魔", "orc": "兽人", "pixie": "小仙", "pegasus": "天马", "phoenix": "凤凰",
    "giant lizard": "巨蜥", "rat": "鼠", "giant snake": "巨蛇", "satyr": "萨堤", "scorpion": "蝎",
    "serpent": "巨蛇", "shark": "鲨", "snake": "蛇", "sphinx": "斯芬克斯", "giant spider": "巨蛛",
    "spider": "蜘蛛", "sylvan": "林精", "sprite": "小仙", "succubus": "魅魔", "titan": "泰坦",
    "toad": "蟾", "troglodite": "穴居人", "troll": "巨魔", "unicorn": "独角兽", "vampire": "吸血鬼",
    "weasel": "鼬", "werebear": "熊人", "wererat": "鼠人", "werewolf": "狼人", "werecat": "猫人",
    "wolf": "狼", "worm": "虫", "wyrm": "翼龙", "wyvern": "双足飞龙", "yeti": "雪人", "zombie": "僵尸",
    "ants": "蚁", "bats": "蝙蝠", "worms": "虫", "wasps": "黄蜂", "leeches": "水蛭", "bees": "蜂",
    "centipedes": "蜈蚣", "mosquitoes": "蚊虫",
}

RELIC_SUBSTANCE = {
    "bile": "胆汁", "blood": "血液", "bone dust": "骨粉", "essence": "精华", "extract": "萃取",
    "eyes": "眼", "hair/skin": "毛皮", "herbs": "草药", "juice": "汁液", "oil": "油",
    "powder": "粉末", "salt": "盐", "sauce": "浆", "scent": "气息", "serum": "血清",
    "spice": "香料", "spit": "唾液", "tears": "泪", "teeth": "牙", "urine": "尿液",
    "flesh": "肉", "ash": "灰烬", "dirt": "尘土", "dust": "尘", "flakes": "碎屑",
    "goo": "黏液", "leaves": "叶", "root": "根", "sap": "树液", "scales": "鳞", "wings": "翼",
    "whiskers": "须", "hair": "毛",
}

RELIC_HERB = {
    "eye of newt": "蝾螈眼", "bat whiskers": "蝙蝠须", "black cat hair": "黑猫毛",
    "black salt": "黑盐", "bloodworms": "血虫", "cat whiskers": "猫须", "coffin shavings": "棺木屑",
    "crystal moonbeams": "水晶月光", "cyclops eyelashes": "独眼睫毛", "dragon scales": "龙鳞",
    "efreet dust": "火灵尘", "elemental dust": "元素尘", "fairy dust": "仙尘", "fairy wings": "仙翼",
    "fire giant ash": "火巨人灰", "gelatinous goo": "凝胶黏液", "genie smoke": "精灵烟",
    "ghoul skin flakes": "食尸鬼皮屑", "graveyard dirt": "墓园土", "hell hound ash": "地狱犬灰",
    "lich dirt": "巫妖尘", "love honey": "爱情蜜", "mummy spice": "木乃伊香料", "mystic dust": "玄妙之尘",
    "ochre jelly": "赭色凝胶", "phoenix ash": "凤凰灰", "pixie dust": "小仙之尘", "pixie wings": "小仙之翅",
    "ritual powder": "仪式粉", "sea serpent salt": "海蛇盐", "serpent scales": "蛇鳞", "snake scales": "蛇鳞",
    "sorcerer sand": "术士沙", "sprite wings": "小仙翼", "tree leaves": "树叶", "reaper root": "收割者根",
    "ent sap": "树人树液", "vampire ash": "吸血鬼灰", "viper essence": "毒蛇精华", "wisp dust": "鬼火尘",
    "witch hazel": "金缅梅", "zombie flesh": "僵尸肉", "slime": "黏液",
    "wyrm bile": "翼龙胆汁", "dragon tears": "龙泪", "dragon bile": "龙胆汁",
}

# Epithets + roles from QuestStories eAdjective switch (merged with quest-fragment table when present)
RELIC_OF = {
    "Might": "威能", "Fire": "火焰", "Ice": "寒冰", "the Light": "光明", "the Dark": "黑暗",
    "the Spirits": "灵体", "the Dead": "亡者", "Hades": "冥府", "the Void": "虚空", "Venom": "剧毒",
    "the Planes": "位面", "the Demon": "恶魔", "the Angel": "天使", "the Devil": "魔鬼", "Death": "死亡",
    "Life": "生命", "Evil": "邪恶", "Darkness": "黑暗", "Magic": "魔法", "Power": "力量",
    "Legend": "传说", "Wonder": "奇迹", "the Mighty": "强者", "the Wizard": "巫师",
    "the Knight": "骑士", "the Mage": "法师", "the Paladin": "圣骑士", "the Thief": "盗贼",
    "Ultimate Evil": "终极之恶", "Pure Evil": "纯粹之恶", "Holy Light": "圣光",
    "the Fowl": "飞禽", "Illusions": "幻象", "the Other World": "异界", "Negative Energy": "负能量",
    "Reality": "现实", "the Sky": "天空", "the Moon": "月亮", "the Sun": "太阳", "the Stars": "星辰",
    "the Earth": "大地", "the Dungeon": "地下城", "the Tomb": "墓穴", "the Ghost": "幽灵",
    "Demonic Power": "恶魔之力", "the Cursed": "受诅者", "the Damned": "被谴者", "the Vile": "邪恶之徒",
    "Purity": "纯净", "Greatness": "伟大", "Supremacy": "至高", "the Almighty": "全能者",
    "the Sacred": "神圣", "Magnificence": "壮丽", "Excellence": "卓越", "Glory": "荣耀",
    "Mystery": "奥秘", "the Divine": "神圣", "the Forgotten": "被遗忘者", "the Lost": "失落者",
    "the Ancients": "远古者", "Marvel": "奇观", "Nobility": "高贵", "Mysticism": "秘术",
    "Enchantment": "附魔", "the Templar": "圣殿骑士", "the Illusionist": "幻术师", "the Princess": "公主",
    "the Invoker": "召唤师", "the Priestess": "女祭司", "the Conjurer": "咒术师", "the Bandit": "强盗",
    "the Baroness": "男爵夫人", "the Cleric": "牧师", "the Monk": "僧侣", "the Minstrel": "吟游诗人",
    "the Defender": "捍卫者", "the Cavalier": "骑士", "the Magician": "魔术师", "the Witch": "女巫",
    "the Fighter": "战士", "the Seeker": "探求者", "the Slayer": "猎杀者", "the Ranger": "游侠",
    "the Barbarian": "野蛮人", "the Explorer": "探险家", "the Heretic": "异端", "the Gladiator": "角斗士",
    "the Sage": "贤者", "the Rogue": "无赖", "the Bard": "诗人", "the Diviner": "占卜师",
    "the Lady": "女士", "the Outlaw": "亡命徒", "the Prophet": "先知", "the Mercenary": "佣兵",
    "the Adventurer": "冒险者", "the Enchantress": "女巫", "the Queen": "女王", "the Scout": "斥候",
    "the Mystic": "秘术师", "the Traveler": "旅人", "the Summoner": "召唤者", "the Warrior": "战士",
    "the Sorcereress": "女术士", "the Seer": "先知", "the Hunter": "猎人", "the Necromancer": "死灵法师",
    "the Shaman": "萨满", "the Prince": "王子", "the Priest": "祭司", "the Baron": "男爵",
    "the Warlock": "术士", "the Lord": "领主", "the Enchanter": "附魔师", "the King": "国王",
    "the Sorcerer": "术士", "Burning Foresight": "敏锐洞察", "Foresight": "洞察",
}


def slug(s: str) -> str:
    return s.lower().replace(" ", "_").replace("'", "").replace(".", "").replace("/", "_")


def merge_fragments(target: dict[str, str]) -> None:
    if not FRAGMENTS.exists():
        return
    table = json.loads(FRAGMENTS.read_text(encoding="utf-8"))
    for en_w, zh_val in table.items():
        if not en_w or not zh_val:
            continue
        if en_w.startswith("the ") or en_w[0].isupper():
            target.setdefault(en_w, zh_val)


def emit_pairs(en: dict, zh: dict, prefix: str, mapping: dict[str, str]) -> None:
    for en_w, zh_val in mapping.items():
        k = f"{prefix}{slug(en_w)}"
        en[k] = en_w
        zh[k] = zh_val


def main() -> None:
    merge_fragments(RELIC_OF)
    merge_fragments(RELIC_ADJ)

    en = json.loads(EN.read_text(encoding="utf-8"))
    zh = json.loads(ZH.read_text(encoding="utf-8"))
    gloss = json.loads(GLOSS.read_text(encoding="utf-8"))
    terms = gloss.setdefault("terms", {})

    skip_gloss = {"the Guardian", "the Great Earth Serpent", "Lord British", "Mondain", "Minax"}

    for name, (zh_name, titles) in EPIC_NPCS.items():
        key = f"quest.tome.noun.epic.name.{slug(name)}"
        en[key] = name
        zh[key] = f"{zh_name}（{name}）"
        if name not in terms and name not in skip_gloss:
            terms[name] = {
                "canonical": zh_name,
                "alternatives": [],
                "category": "character",
                "notes": "EpicCharacter quest NPC from Ultima lore.",
                "translation_basis_zh": "Ultima 世界观人物音译/意译，用于任务日记。",
            }
        for en_title, zh_title in titles.items():
            tk = f"quest.tome.noun.epic.title.{slug(en_title)}"
            en[tk] = en_title
            zh[tk] = f"{zh_title}（{en_title}）"

    emit_pairs(en, zh, "quest.tome.noun.evil.subs.", EVIL_SUBS)
    emit_pairs(en, zh, "quest.tome.noun.evil.adj.", EVIL_ADJ)
    emit_pairs(en, zh, "quest.tome.noun.evil.theme.", EVIL_THEME)
    emit_pairs(en, zh, "quest.tome.noun.relic.", RELIC_NOUNS)
    emit_pairs(en, zh, "quest.tome.noun.relic.adj.", RELIC_ADJ)
    emit_pairs(en, zh, "quest.tome.noun.relic.of.", RELIC_OF)
    emit_pairs(en, zh, "quest.tome.noun.relic.creature.", RELIC_CREATURE)
    emit_pairs(en, zh, "quest.tome.noun.relic.substance.", RELIC_SUBSTANCE)
    emit_pairs(en, zh, "quest.tome.noun.relic.herb.", RELIC_HERB)

    EN.write_text(json.dumps(en, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    ZH.write_text(json.dumps(zh, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    GLOSS.write_text(json.dumps(gloss, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(
        "Updated shotkeys:",
        len(RELIC_OF), "of,",
        len(RELIC_CREATURE), "creature,",
        len(RELIC_SUBSTANCE), "substance,",
        len(RELIC_HERB), "herb",
    )


if __name__ == "__main__":
    main()

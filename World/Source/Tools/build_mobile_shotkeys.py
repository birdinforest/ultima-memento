#!/usr/bin/env python3
"""
Build the complete shotkey mapping + world-player-text.json for scripts-mobiles.json.
Phase 1: Generate keys, populate EN+ZH world-player-text (EN = source, ZH = existing or auto).
Phase 2: Apply LLM translations (ZH only).
Phase 3: Prune migrated hash keys from scripts-mobiles.json.
Phase 4: Patch C# files.
"""
import json
import re
import os
import sys

REPO = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
EN_SM_PATH = os.path.join(REPO, "Data", "Localization", "en", "scripts-mobiles.json")
ZH_SM_PATH = os.path.join(REPO, "Data", "Localization", "zh-Hans", "scripts-mobiles.json")
GLOSSARY_PATH = os.path.join(REPO, "Data", "Localization", "glossary-approved-zh.json")
WPT_EN_PATH = os.path.join(REPO, "Data", "Localization", "en", "world-player-text.json")
WPT_ZH_PATH = os.path.join(REPO, "Data", "Localization", "zh-Hans", "world-player-text.json")
MAPPING_PATH = os.path.join(REPO, "Data", "Localization", "tools-output", "mobile-shotkey-mapping.json")
LLM_PATH = os.path.join(REPO, "Data", "Localization", "tools-output", "mobile-llm-translation-v2.json")

def load_json(p):
    if os.path.exists(p):
        with open(p, encoding="utf-8") as f:
            return json.load(f)
    return {}

# ---- Known translation lookup tables ----

CREATURE_KNOWN = {
    "a corpser": "一株魔血藤", "a blood worm": "一条血虫",
    "a quartz grue": "一只石英奎怪", "a giant lizard": "一只巨型蜥蜴",
    "the vampire": "吸血鬼", "the combat droid": "战斗机器人",
    "a decaying zombie": "一具腐烂僵尸", "a snake": "一条蛇",
    "a rotten corpse": "一具腐烂尸体", "a construct": "一个构造体",
    "a creeping fungus": "一株爬行真菌", "a copper grue": "一只铜奎怪",
    "a plague vortex": "一个瘟疫旋涡", "a wisp": "一个幽光",
    "a golden statue": "一座金色雕像", "an azathoth": "一个阿撒托斯",
    "a satyr": "一个萨特", "a giant black widow": "一只巨型黑寡妇",
    "a jellyfish": "一只水母", "a jade statue": "一座翡翠雕像",
    "an icy blade spirit": "一个寒冰刀灵", "a pile of viscera": "一堆内脏",
    "a grue": "一只奎怪", "a kraken": "一只克拉肯", "a ghoul": "一只食尸鬼",
    "an ice colossus": "一尊寒冰巨人", "a giant serpent": "一条巨蛇",
    "a steam elemental": "一个蒸汽元素", "a skeletal horse": "一匹骷髅战马",
    "a fire toad": "一只火焰蟾蜍", "a rust golem": "一个锈蚀魔像",
    "a silver statue": "一座银质雕像", "a sapphire elemental": "一个蓝宝石元素",
    "a water spawn": "一个水裔", "a pack turtle": "一只驮行龟",
    "a pegasus": "一匹飞马", "a decaying spider": "一具腐烂蜘蛛",
    "a hell beast": "一只地狱兽", "a unicorn": "一只独角兽",
    "a styguana": "一只冥蜥蜴", "a shark": "一条鲨鱼", "an ape": "一只猿",
    "a seaweed elemental": "一个海藻元素", "a bull frog": "一只牛蛙",
    "an obsidian grue": "一只黑曜石奎怪", "a mysterious rabbit hole": "一个神秘兔洞",
    "a grum": "一个虎怪", "a dead wood tree": "一株枯木",
    "a hell lion": "一只地狱狮", "an enslaved demon": "一个受奴役恶魔",
    "an ankhheg": "一只安克赫格", "a snow elemental": "一个雪元素",
    "a dreadhorn": "一只惧角兽", "an efreet": "一个伊夫利特",
    "a blade spirit": "一个刀灵", "a frost spider": "一只霜蜘蛛",
    "a xorn": "一个索恩怪", "a serpentaur": "一个蛇人",
    "a necromental": "一个亡灵元素", "the dracolich": "龙巫妖",
    "a shadow wisp": "一个暗影幽光", "a huge spider": "一只巨型蜘蛛",
    "a marble gargoyle": "大理石石像鬼", "a star ruby grue": "一只星红宝石奎怪",
    "an emerald gargoyle": "一只翡翠石像鬼", "a deep crawler": "一只深潜者",
    "a mystical fox": "一只神秘狐", "a gargoyle lord": "石像鬼领主",
    "a carcass worm": "一条腐尸虫", "a guardian wolf": "一头守护狼",
    "a cave lizard": "一只洞穴蜥蜴", "a sea horse": "一只海马",
    "a trilithium elemental": "一个三锂元素", "a large snake": "一条大蛇",
    "a scorching vortex": "一个灼热旋涡", "an undead creature": "一个亡灵生物",
    "a silver elemental": "一个银元素", "a white wolf": "一头白狼",
    "a wax golem": "一个蜡质魔像", "a manure golem": "一个粪石魔像",
    "a shambling mound": "一具蹒跚巨体", "a walking corpse": "一具行走尸体",
    "a gazer": "一个凝视者", "a frog": "一只青蛙",
    "a seeker": "一个探寻者", "a skitter": "一只窜行者",
    "a swarm of insects": "一群昆虫", "a rotting zombie": "一具腐烂僵尸",
    "the lost knight": "失落骑士", "an elemental mineral": "一个元素矿物",
    "an ice toad": "一只冰蟾蜍", "a snow lion": "一只雪狮",
    "a gorilla": "一只大猩猩", "a bake kitsune": "一只妖狐",
    "a caddellite golem": "一个卡德利特魔像", "a calamari": "一只鱿鱼",
    "a dryad": "一个树妖", "a horde minion": "一个部落仆从",
    "a shadow fiend": "一个暗影魔", "a large crab": "一只大蟹",
    "a verite elemental": "一个真理元素", "a wight": "一个尸妖",
    "a blood spawn": "一个血裔", "a black wolf": "一头黑狼",
    "a stone gargoyle": "石质石像鬼", "a caddellite elemental": "一个卡德利特元素",
    "a corpse": "一具尸体", "a swamp tentacle": "一条沼泽触须",
    "a black pudding": "一块黑布丁", "an iron beetle": "一只铁甲虫",
    "an energy vortex": "一个能量旋涡", "a giant leech": "一只巨型水蛭",
    "a reaper": "一个死神", "a roc": "一只巨鹏",
    "a valorite grue": "一只勇气奎怪", "a poison elemental": "一个毒元素",
    "a sand vortex": "一个沙旋涡", "a pack horse": "一匹驮马",
    "a mystical tiger": "一只神秘虎", "an amethyst gargoyle": "一只紫水晶石像鬼",
    "an enchanted item": "一件附魔物品", "a darkrazor": "一把暗影剃刀",
    "the beholder": "眼魔", "a gorgon": "一个蛇发女妖",
    "a skeletal wizard": "一个骷髅巫师", "a vrock": "一只弗洛克魔",
    "a fungal mage": "一个真菌法师", "a whipping vine": "一条鞭藤",
    "a shambler": "一个蹒跚者", "a sea snake": "一条海蛇",
    "a wine elemental": "一个酒元素", "a familiar": "一个使魔",
    "a crag cat": "一只岩山猫", "an ice serpent": "一条冰蛇",
    "a lava lizard": "一只熔岩蜥蜴", "a raptus": "一个拉普图斯",
    "a rune beetle": "一只符文甲虫", "a giant eel": "一条巨型鳗鱼",
    "a scorpion": "一只蝎子", "an anaconda": "一条森蚺", "a raven": "一只乌鸦",
    "a shaclaw": "一只影爪", "a bronze statue": "一座青铜雕像",
    "a stone roper": "一个岩石诱捕者", "a sunlyte": "一个日光灵",
    "a marsh wurm": "一条沼泽巨虫", "a water weird": "一个水妖",
    "a storm cloud": "一朵风暴云", "a security droid": "警卫机器人",
    "a golden grue": "一只金奎怪", "a blood lotus": "一株血莲",
    "a jade serpent": "一条玉蛇", "a cronosaurus": "一只冠龙鲨",
    "a slitheran": "一只滑行者", "a dilithium elemental": "一个双锂元素",
    "a cave fisher": "一只洞穴渔者", "a crocodile": "一只鳄鱼",
    "a zombie mage": "一个僵尸法师", "a serpent": "一条蛇",
    "a sand squid": "一只沙鱿鱼", "a kith": "一个凯斯怪",
    "a mutant gargoyle": "一个变种石像鬼", "an acid puddle": "一滩酸液",
    "a lava puddle": "一滩熔岩", "a dull copper elemental": "一个钝铜元素",
    "an animated statue": "一座活化雕像", "an onyx gargoyle": "一只缟玛瑙石像鬼",
    "a meglasaur": "一只巨齿龙", "a marsh frog": "一只沼泽蛙",
    "an agapite elemental": "一个爱情元素", "a blood snake": "一条血蛇",
    "a driftwood elemental": "一个浮木元素", "a chimera": "一只奇美拉",
    "a placeron": "一个奇兽", "a garnet grue": "一只石榴石奎怪",
    "a giant spider": "一只巨型蜘蛛", "a blood elemental": "一个血元素",
    "a dinosaur": "一只恐龙", "a spinel grue": "一只尖晶石奎怪",
    "a hell hound": "一只地狱犬", "a nightmare": "一只梦魇",
    "a dire wolf": "一只恐狼", "a deep sea elemental": "一个深海元素",
    "a garnet elemental": "一个石榴石元素", "a pack animal": "一只驮兽",
    "an iron golem": "一个铁魔像", "a deathvine": "一条死亡藤",
    "a shadow demon": "一个暗影恶魔", "a poison cloud": "一朵毒云",
    "an elder gazer": "一个长者凝视者", "a monstrous spider": "一只巨蜘蛛",
    "a green slime": "一块绿泥", "an antaur worker": "一个蚁人劳工",
    "a glacial elemental": "一个冰川元素", "a megalodon": "一只巨齿鲨",
    "a skitterling": "一只窜行幼体", "a xormite elemental": "一个索尔米特元素",
    "a seaweeder": "一株海藻怪", "an iron statue": "一座铁质雕像",
    "a quagmire": "一块泥沼", "an ant lion": "一只蚁狮",
    "a shadow recluse": "一只暗影隐士蛛", "a slime devil": "一个泥魔",
    "an ice snake": "一条冰蛇", "a silver serpent": "一条银蛇",
    "a meteor elemental": "一个流星元素", "a titanoboa": "一条泰坦蚺",
    "a firerock elemental": "一个火岩元素", "a typhoon": "一个台风",
    "an arcticonda": "一条极地森蚺", "a wooden golem": "一个木质魔像",
    "a lavapede": "一只熔岩蜈蚣", "a bear": "一只熊",
    "a sentaur": "一个半人马",
    "a slime": "一块史莱姆", "an iron cobra": "一条铁眼镜蛇",
    "a phoenix": "一只凤凰", "a special item": "一件特殊物品",
    "a quartz elemental": "一个石英元素", "a topaz elemental": "一个黄玉元素",
    "a mud elemental": "一个泥元素", "a headless one": "一个无头者",
    "a tortuga": "一只龟怪", "a mummy": "一具木乃伊",
    "a dune beetle": "一只沙丘甲虫", "a giant toad": "一只巨蟾蜍",
    "a dust elemental": "一个尘埃元素", "a stegosaurus": "一只剑龙",
    "a deadly scorpion": "一只致命蝎子", "a magma snake": "一条熔岩蛇",
    "an ice golem": "一个冰魔像", "a phase spider": "一只相位蜘蛛",
    "a runic golem": "一个符文魔像", "a tarantula": "一只狼蛛",
    "a metallic beetle": "一只金属甲虫", "a fungal": "一株真菌",
    "a mantis": "一只螳螂", "a deep sea serpent": "一条深海蛇",
    "a bone golem": "一个骨魔像", "a weed elemental": "一个杂草元素",
    "a raptor": "一只迅猛龙", "a stalker": "一个潜行者",
    "a stone statue": "一座石质雕像", "an alien spider": "一只异形蜘蛛",
    "a ruby gargoyle": "一只红宝石石像鬼", "a cerberus": "一只刻耳柏洛斯",
    "a stone elemental": "一个岩石元素", "a kuthulu": "一个克苏鲁",
    "a deep sea snake": "一条深海蛇", "a cinder elemental": "一个煤渣元素",
    "a hawk": "一只鹰", "a valorite elemental": "一个勇气元素",
    "a sewage elemental": "一个污水元素", "a floating eye": "一只漂浮之眼",
    "a kelp elemental": "一个海带元素", "an oil slick": "一滩油滑",
    "a xatyr": "一个阴影萨特", "a soul sucker": "一个噬魂者",
    "a swamp gator": "一只沼泽鳄鱼", "a mud man": "一个泥人",
    "an undead corpse": "一具亡灵尸体", "a dull copper grue": "一只钝铜奎怪",
    "a dread spider": "一只恐惧蜘蛛", "an ancient sphinx": "一只远古狮身人面兽",
    "a sapphire gargoyle": "一只蓝宝石石像鬼", "a toraxen": "一只托拉克森",
    "a giant adder": "一条巨型蝰蛇", "a dark hound": "一头黑暗猎犬",
    "a xenomorph": "一个异形", "a marble statue": "一座大理石雕像",
    "a jungle viper": "一条丛林蝰蛇", "an acid elemental": "一个酸元素",
    "a xenomutant": "一个异形变种", "a sleech": "一只泥蛭",
    "a royal sphinx": "一只皇家狮身人面兽", "a verite grue": "一只真理奎怪",
    "a vorpal bunny": "一只致命兔", "a woodland devil": "一只林地恶魔",
    "an icy vortex": "一个寒冰旋涡", "a shadow iron grue": "一只暗影铁奎怪",
    "a mutant": "一个变种怪", "a skellot": "一个骷髅虫",
    "a stone grue": "一只石头奎怪", "a watcher": "一个观察者",
    "a sand spider": "一只沙蜘蛛", "a golden elemental": "一个金元素",
    "a pack stegosaurus": "一头驮行剑龙", "a strangle vine": "一条绞杀藤",
    "a shadow iron elemental": "一个暗影铁元素", "a walking dead": "一个行尸",
    "a kirin": "一只麒麟", "a star ruby elemental": "一个星红宝石元素",
    "a frost ooze": "一块霜冻软泥", "a silver grue": "一只银奎怪",
    "a lava elemental": "一个熔岩元素", "a golden serpent": "一条金蛇",
    "an eye of the deep": "一只深海之眼", "an ancient flesh golem": "一个远古血肉魔像",
    "a snapper": "一只龟", "a bronze grue": "一只青铜奎怪",
    "a huge lizard": "一只巨型蜥蜴", "a grave dust elemental": "一个墓尘元素",
    "an ancient nightmare": "一只远古梦魇", "an obsidian elemental": "一个黑曜石元素",
    "a giant snake": "一条巨蛇", "a spectre": "一个幽灵",
    "an umber hulk": "一只土巨怪", "a swamp thing": "一个沼泽怪物",
    "an alien": "一个异形", "a brontosaur": "一只雷龙",
    "an archangel": "一位大天使", "a topaz grue": "一只黄玉奎怪",
    "a spider": "一只蜘蛛", "a huge fish": "一条巨型鱼",
    "a panther": "一只黑豹", "a tortuga": "一只龟怪",
    "the balron": "巴洛魔", "the daemon": "恶魔",
    "the ent": "树精", "the centaur": "半人马",
    "the rotted ent": "朽木树精", "the ancient ent": "远古树精",
    "the lich": "巫妖", "the ghost": "幽灵",
    "the stone crafter": "石匠", "the stonecrafter": "石匠",
    "the priest": "祭司", "the sprite": "精灵",
}

def make_shotkey_category(en_text):
    t = en_text.strip()
    if t.startswith("*") and t.endswith("*"):
        return "mob.emote"
    if "{0}" in t or "{1}" in t:
        return "mob.fmt"
    if t.startswith(("What do ", "Why did ", "How do ", "What did ", "How did ",
                      "Why was ", "What was ", "Why are ", "What is ", "How is ")):
        return "mob.joke"
    return "mob.other"

def _make_slug(text):
    slug = text.strip().lower()
    slug = re.sub(r'[^a-z0-9\s]', ' ', slug)
    slug = re.sub(r'\s+', '_', slug.strip())
    slug = re.sub(r'_+', '_', slug)
    if len(slug) > 55:
        slug = slug[:55].rstrip('_')
    return slug or "entry"

def ensure_unique(key, used):
    if key not in used:
        return key
    i = 2
    while f"{key}_{i}" in used:
        i += 1
    return f"{key}_{i}"


def phase1_build_mapping():
    """Phase 1: Generate shotkey mapping + EN/ZH world-player-text entries."""
    en_sm = load_json(EN_SM_PATH)
    zh_sm = load_json(ZH_SM_PATH)
    wpt_en = load_json(WPT_EN_PATH)
    wpt_zh = load_json(WPT_ZH_PATH)

    mapping = {}
    used_keys = set()
    mob_en = {}
    mob_zh = {}
    auto_translated = 0
    llm_queue = {}

    for hk in sorted(en_sm.keys()):
        en_text = en_sm[hk]
        zh_text = zh_sm.get(hk, en_text)
        is_untranslated = (zh_text == en_text)

        cat_prefix = make_shotkey_category(en_text)
        slug = _make_slug(en_text)
        sk = ensure_unique(f"{cat_prefix}.{slug}", used_keys)
        used_keys.add(sk)

        mapping[hk] = sk
        mob_en[sk] = en_text

        if is_untranslated:
            # Auto-translate from known table or glossary
            if en_text in CREATURE_KNOWN:
                zh_text = CREATURE_KNOWN[en_text]
                auto_translated += 1
            else:
                llm_queue[sk] = en_text
                zh_text = ""  # placeholder for now

        mob_zh[sk] = zh_text

    # Merge into world-player-text (existing keys keep their values)
    for k, v in mob_en.items():
        if k not in wpt_en:
            wpt_en[k] = v
    for k, v in mob_zh.items():
        if k not in wpt_zh:
            wpt_zh[k] = v  # write even empty as placeholder

    os.makedirs(os.path.dirname(MAPPING_PATH), exist_ok=True)

    # Write mapping
    with open(MAPPING_PATH, "w", encoding="utf-8") as f:
        json.dump(mapping, f, ensure_ascii=False, indent=2)
        f.write("\n")

    # Write WPT
    with open(WPT_EN_PATH, "w", encoding="utf-8") as f:
        json.dump(dict(sorted(wpt_en.items())), f, ensure_ascii=False, indent=2)
        f.write("\n")
    with open(WPT_ZH_PATH, "w", encoding="utf-8") as f:
        json.dump(dict(sorted(wpt_zh.items())), f, ensure_ascii=False, indent=2)
        f.write("\n")

    # Write LLM queue
    llm_q_path = os.path.join(os.path.dirname(LLM_PATH), "mobile-translate-queue-v2.json")
    with open(llm_q_path, "w", encoding="utf-8") as f:
        json.dump(llm_queue, f, ensure_ascii=False, indent=2)
        f.write("\n")

    print(f"Mapping: {len(mapping)}")
    print(f"WPT EN: {len(wpt_en)}")
    print(f"WPT ZH: {len(wpt_zh)} (including {len(mob_zh)} mob keys)")
    print(f"Auto-translated from known table: {auto_translated}")
    print(f"Need LLM translation: {len(llm_queue)}")

    # Verify EN texts are correct
    for sk in list(mob_en.keys())[:5]:
        en_text = mob_en[sk]
        assert en_text.strip() != "", f"Empty EN for {sk}"
        assert not re.match(r'[\u4e00-\u9fff]', en_text), f"ZH in EN for {sk}: {en_text[:40]}"

    return llm_queue


def phase2_apply_llm():
    """Phase 2: Apply LLM translations to ZH world-player-text (only)."""
    wpt_zh = load_json(WPT_ZH_PATH)
    llm = load_json(LLM_PATH)

    # Also get EN texts for keys that only exist in LLM output
    wpt_en = load_json(WPT_EN_PATH)

    applied = 0
    added_en = 0
    for sk, zh_text in llm.items():
        if sk in wpt_zh:
            if not wpt_zh[sk]:
                wpt_zh[sk] = zh_text
                applied += 1
        else:
            wpt_zh[sk] = zh_text
            applied += 1
            # Also ensure EN counterpart exists
            if sk not in wpt_en:
                # Try to restore EN from the queue file
                q = load_json(os.path.join(os.path.dirname(LLM_PATH), "mobile-translate-queue-v2.json"))
                if sk in q:
                    wpt_en[sk] = q[sk]
                    added_en += 1

    with open(WPT_ZH_PATH, "w", encoding="utf-8") as f:
        json.dump(dict(sorted(wpt_zh.items())), f, ensure_ascii=False, indent=2)
        f.write("\n")
    if added_en:
        with open(WPT_EN_PATH, "w", encoding="utf-8") as f:
            json.dump(dict(sorted(wpt_en.items())), f, ensure_ascii=False, indent=2)
            f.write("\n")

    print(f"Applied {applied} LLM translations to ZH, added {added_en} EN keys")
    print(f"ZH WPT now has {len(wpt_zh)} keys, EN WPT now has {len(wpt_en)} keys")

    # Verify no ZH in EN
    zh_in_en = 0
    for k, v in wpt_en.items():
        if re.search(r'[\u4e00-\u9fff]', v) and k.startswith("mob."):
            zh_in_en += 1
            if zh_in_en <= 3:
                print(f"  WARNING: ZH in EN: {k}: {v[:40]}")
    if zh_in_en:
        print(f"WARNING: {zh_in_en} ZH texts found in EN WPT mob keys!")
    else:
        print("OK: No ZH texts in EN WPT mob keys.")


def phase3_prune():
    """Phase 3: Remove migrated entries from scripts-mobiles.json."""
    mapping = load_json(MAPPING_PATH)
    en_sm = load_json(EN_SM_PATH)
    zh_sm = load_json(ZH_SM_PATH)

    n_en = len(en_sm)
    n_zh = len(zh_sm)

    for hk in mapping:
        en_sm.pop(hk, None)
        zh_sm.pop(hk, None)

    with open(EN_SM_PATH, "w", encoding="utf-8") as f:
        json.dump(en_sm, f, ensure_ascii=False, indent=2)
        f.write("\n")
    with open(ZH_SM_PATH, "w", encoding="utf-8") as f:
        json.dump(zh_sm, f, ensure_ascii=False, indent=2)
        f.write("\n")

    print(f"scripts-mobiles EN: {n_en} → {len(en_sm)} keys")
    print(f"scripts-mobiles ZH: {n_zh} → {len(zh_sm)} keys")


if __name__ == "__main__":
    import sys

    if len(sys.argv) > 1:
        cmd = sys.argv[1]
        if cmd == "phase1":
            phase1_build_mapping()
        elif cmd == "phase2":
            phase2_apply_llm()
        elif cmd == "phase3":
            phase3_prune()
        else:
            print(f"Unknown: {cmd}")
    else:
        print("=== Phase 1: Build mapping ===")
        phase1_build_mapping()
        print()
        print("Now run: python3 build_mobile_shotkeys.py phase2  (after getting LLM translation)")
        print("  Then: python3 build_mobile_shotkeys.py phase3")

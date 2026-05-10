#!/usr/bin/env python3
"""
Adds adventurer job-title fragments from TavernPatrons.Adventurer() plus the
"Some " preface to the composite resolver tables.
These must sit AFTER the longer compound creature names in the order file so
"a skeletal warrior" is matched before standalone "warrior".
"""

import json
import hashlib
import os

BASE = os.path.normpath(os.path.join(os.path.dirname(__file__), "../../.."))
EN_FILE   = os.path.join(BASE, "World/Data/Localization/en/scripts-quests.json")
ZH_FILE   = os.path.join(BASE, "World/Data/Localization/zh-Hans/scripts-quests.json")
ORDER_FILE = os.path.join(BASE, "World/Data/Localization/quest-composite-terms-order.txt")

def sha_key(s):
    return "s." + hashlib.sha256(s.encode("utf-8")).hexdigest()[:16]

# All titles returned by TavernPatrons.Adventurer(), plus "Some " prefix
# These are APPENDED to the order file (after longer compound phrases)
TITLES = [
    ("Some ",       "一位"),
    ("adventurer",  "冒险者"),
    ("barbarian",   "蛮族战士"),
    ("baroness",    "女男爵"),
    ("cavalier",    "骑士"),
    ("conjurer",    "召唤师"),
    ("defender",    "卫士"),
    ("diviner",     "占卜师"),
    ("enchantress", "女附魔师"),
    ("enchanter",   "附魔师"),
    ("explorer",    "探索者"),
    ("gladiator",   "角斗士"),
    ("heretic",     "异教徒"),
    ("illusionist", "幻术师"),
    ("invoker",     "咒召师"),
    ("magician",    "魔法师"),
    ("mercenary",   "佣兵"),
    ("minstrel",    "吟游歌手"),
    ("mystic",      "神秘者"),
    ("outlaw",      "亡命之徒"),
    ("priestess",   "女祭司"),
    ("princess",    "公主"),
    ("prophet",     "先知"),
    ("sorceress",   "女术士"),
    ("summoner",    "召唤师"),
    ("templar",     "圣殿骑士"),
    ("traveler",    "旅人"),
    ("warlock",     "战锁"),
    ("warrior",     "战士"),
    ("bandit",      "强盗"),
    ("fighter",     "战士"),
    ("hunter",      "猎人"),
    ("knight",      "骑士"),
    ("mage",        "法师"),
    ("monk",        "修士"),
    ("paladin",     "圣骑士"),
    ("priest",      "祭司"),
    ("prince",      "王子"),
    ("queen",       "女王"),
    ("ranger",      "游侠"),
    ("rogue",       "盗贼"),
    ("sage",        "智者"),
    ("scout",       "斥候"),
    ("seeker",      "寻觅者"),
    ("seer",        "预言者"),
    ("shaman",      "萨满"),
    ("slayer",      "杀手"),
    ("sorcerer",    "术士"),
    ("thief",       "盗贼"),
    ("baron",       "男爵"),
    ("bard",        "吟游诗人"),
    ("cleric",      "神职者"),
    ("king",        "国王"),
    ("lady",        "贵妇"),
    ("lord",        "领主"),
    ("witch",       "女巫"),
    ("wizard",      "巫师"),
]

def load_json(path):
    with open(path, encoding="utf-8") as f:
        return json.load(f)

def save_json(path, data):
    with open(path, "w", encoding="utf-8") as f:
        json.dump(data, f, ensure_ascii=False, indent="\t")
        f.write("\n")

def load_order(path):
    with open(path, encoding="utf-8") as f:
        return [line.rstrip("\n") for line in f]

def save_order(path, lines):
    with open(path, "w", encoding="utf-8") as f:
        for line in lines:
            f.write(line + "\n")

def main():
    en_data  = load_json(EN_FILE)
    zh_data  = load_json(ZH_FILE)
    order    = load_order(ORDER_FILE)
    order_set = set(line.strip() for line in order if line.strip() and not line.strip().startswith("#"))

    new_en = {}
    new_zh = {}
    new_order_entries = []

    for en_frag, zh_frag in TITLES:
        key = sha_key(en_frag)
        if key not in en_data:
            new_en[key] = en_frag
        if key not in zh_data:
            new_zh[key] = zh_frag
        if en_frag not in order_set:
            new_order_entries.append(en_frag)
            order_set.add(en_frag)

    if new_en:
        en_data.update(new_en)
        save_json(EN_FILE, en_data)
        print(f"Added {len(new_en)} EN keys")
    else:
        print("No new EN keys needed.")

    if new_zh:
        zh_data.update(new_zh)
        save_json(ZH_FILE, zh_data)
        print(f"Added {len(new_zh)} ZH keys")
    else:
        print("No new ZH keys needed.")

    if new_order_entries:
        # Append to end of file so longer compound names take priority
        order.append("# --- Adventurer titles (must remain after compound creature names) ---")
        for entry in new_order_entries:
            order.append(entry)
        save_order(ORDER_FILE, order)
        print(f"Appended {len(new_order_entries)} entries to order file")
    else:
        print("No new order entries needed.")

    print("Done.")

if __name__ == "__main__":
    main()

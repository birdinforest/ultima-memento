#!/usr/bin/env python3
"""
Adds Citizens.cs English speech fragments (greetings, prefaces, verbs, service phrases)
to the composite resolver tables so Chinese players see translated citizen dialogue.

Runtime resolution: World/Source/Tools/gen_quest_fragment_translations.py imports this file's
FRAGMENTS to fill Data/Localization/quest-fragment-zh-table.json (no duplicate Chinese in Citizens.cs).
Run gen_quest_fragment_translations.py after editing FRAGMENTS or quest-composite-terms-order.txt.
"""

import json
import hashlib
import os
import sys

BASE = os.path.normpath(os.path.join(os.path.dirname(__file__), "../../.."))
EN_FILE  = os.path.join(BASE, "World/Data/Localization/en/scripts-quests.json")
ZH_FILE  = os.path.join(BASE, "World/Data/Localization/zh-Hans/scripts-quests.json")
ORDER_FILE = os.path.join(BASE, "World/Data/Localization/quest-composite-terms-order.txt")

def sha_key(s):
    return "s." + hashlib.sha256(s.encode("utf-8")).hexdigest()[:16]

# (English fragment, Chinese translation)
FRAGMENTS = [
    # ── Long service phrases (must come first in the order file) ─────────────
    (
        " I can recharge any magic items you may have. Such items have a magical spell imbued in it, and allows you to cast such spell. I can only recharge such items if they have a minimum and maximum amount of uses, as those without a maximum amount can never be recharged by any wizard. If you want my help, then simply hand me your wand so I can perform the ritual needed.",
        "我可以为你的魔法物品充能。此类物品内置魔法咒语，可让你施放对应法术。只有同时具备最小和最大使用次数的物品方可充能——无上限的物品无法充能。若需帮助，请将魔杖交给我，我将执行所需仪式。",
    ),
    (
        " You can look in my backpack to examine the reagents if you wish. If you want to trade, then hand me the gold and I will give you the reagents.",
        "你可以查看我的背包检视这些试剂。若想交易，请将金币交给我，我会给你这些试剂。",
    ),
    (
        " You can look in my backpack to examine the potions if you wish. If you want to trade, then hand me the gold and I will give you the potions.",
        "你可以查看我的背包检视这些药水。若想交易，请将金币交给我，我会给你这些药水。",
    ),
    (
        " You can look in my backpack to examine the ingots if you wish. If you want to trade, then hand me the gold and I will give you the ingots.",
        "你可以查看我的背包检视这些金属锭。若想交易，请将金币交给我，我会给你这些金属锭。",
    ),
    (
        " You can look in my backpack to examine the leather if you wish. If you want to trade, then hand me the gold and I will give you the leather.",
        "你可以查看我的背包检视这些皮革。若想交易，请将金币交给我，我会给你这些皮革。",
    ),
    (
        " You can look in my backpack to examine the ore if you wish. If you want to trade, then hand me the gold and I will give you the ore.",
        "你可以查看我的背包检视这些矿石。若想交易，请将金币交给我，我会给你这些矿石。",
    ),
    (
        " You can look in my backpack to examine the item if you wish. If you want to trade, then hand me the gold and I will give you the item.",
        "你可以查看我的背包检视这件物品。若想交易，请将金币交给我，我会给你这件物品。",
    ),
    (
        " I am quite a skilled blacksmith, so if you need any metal armor repaired I can do it for you. Just hand me the armor and I will see what I can do.",
        "我是一位熟练的铁匠，如需修缮金属盔甲，请交给我，我会尽力修好。",
    ),
    (
        " I am quite a skilled blacksmith, so if you need any metal weapons repaired I can do it for you. Just hand me the weapon and I will see what I can do.",
        "我是一位熟练的铁匠，如需修缮金属武器，请交给我，我会尽力修好。",
    ),
    (
        " I am quite a skilled leather worker, so if you need any leather item repaired I can do it for you. Just hand me the item and I will see what I can do.",
        "我是一位熟练的皮革工匠，如需修缮皮革物品，请交给我，我会尽力修好。",
    ),
    (
        " I am quite a skilled wood worker, so if you need any wooden weapons repaired I can do it for you. Just hand me the weapon and I will see what I can do.",
        "我是一位熟练的木工，如需修缮木制武器，请交给我，我会尽力修好。",
    ),
    (
        " I am quite a skilled wood worker, so if you need any wooden armor repaired I can do it for you. Just hand me the armor and I will see what I can do.",
        "我是一位熟练的木工，如需修缮木制盔甲，请交给我，我会尽力修好。",
    ),
    (
        " If you need a chest or box unlocked, I can help you with that. Just hand me the container and I will see what I can do. I promise to give it back.",
        "若你有需要开锁的箱子或容器，我可以帮忙。请把容器交给我，我会尽力解决，保证归还。",
    ),
    # ── Rumor sentence frames ─────────────────────────────────────────────────
    ("I heard many tales of adventurers going to ", "我听说许多冒险者前往"),
    ("I was talking with the local ",               "我与当地的"),
    ("I overheard someone tell of",                 "我无意间听到有人谈起"),
    ("We overheard someone tell of",                "我们无意间听到有人谈起"),
    (" overheard another tell of",                  "无意间听到另一人谈起"),
    (" is spreading rumors about",                  "正在散布关于此事的传言"),
    (" is spreading rumours about",                 "正在散布关于此事的传言"),
    ("Someone told me that ",                       "有人告诉我，"),
    (" is where you would look for ",               "是寻找"),
    (" and they told me to bring back ",            "，他们让我带回"),
    ("I heard something about ",                    "我听闻了关于"),
    (" was in the tavern talking about ",           "在酒馆中谈到了"),
    (", and they mentioned ",                       "，他们提到了"),
    ("I heard rumours about",                       "我听说了"),
    ("We heard rumours about",                      "我们听说了"),
    ("I heard a story about",                       "我听了一个关于"),
    ("We heard a story about",                      "我们听了一个关于"),
    (" heard rumours about",                        "听说了"),
    (" heard a story about",                        "听了一个关于"),
    (" is telling tales about",                     "正在讲述关于"),
    ("I heard that ",                               "我听说"),
    (" can be obtained in ",                        "可在"),
    (" can be found in ",                           "可在"),
    ("Someone from ",                               "来自"),
    (" died in ",                                   "死于"),
    (" searching for ",                             "，寻找"),
    ("and seeing ",                                 "并在那里看到了"),
    ("I met with ",                                 "我与"),
    # ── Greeting openers ──────────────────────────────────────────────────────
    ("We are just here to rest after exploring ",   "我们刚从"),
    ("This is the first time I have been to ",      "这是我第一次来到"),
    (". Welcome to ",                               "。欢迎来到"),
    ("Good day to you, ",                           "祝你今日愉快，"),
    ("Greetings, ",                                 "你好，"),
    ("Hail, ",                                      "致意，"),
    ("Hello, ",                                     "你好，"),
    # ── Verb fragments used in service/rumor assembly ─────────────────────────
    ("willing to part with",                        "愿意出让"),
    ("willing to trade",                            "愿意交换"),
    ("willing to sell",                             "愿意出售"),
    (" came upon ",                                 "偶然发现于"),
    (" discovered ",                                "发现于"),
    (" excavated ",                                 "挖掘于"),
    (" concocted ",                                 "配制"),
    (" purchased ",                                 "购得"),
    (" acquired ",                                  "获取"),
    (" prepared ",                                  "备好"),
    (" smelted ",                                   "冶炼于"),
    (" forged ",                                    "锻造于"),
    (" dug up ",                                    "挖掘于"),
    (" brewed ",                                    "酿制"),
    (" logged ",                                    "伐木于"),
    (" baked ",                                     "烘焙"),
    (" tanned ",                                    "鞣制于"),
    (" cooked ",                                    "烹饪"),
    (" mined ",                                     "开采于"),
    (" found ",                                     "发现于"),
    (" chopped ",                                   "砍伐于"),
    (" bought ",                                    "购于"),
    (" carved ",                                    "雕刻于"),
    (" skinned ",                                   "剥皮于"),
    (" cut ",                                       "砍伐于"),
    ("We found",                                    "我们找到了"),
    ("I found",                                     "我找到了"),
    # ── Location prepositions (longer patterns first) ─────────────────────────
    (" in a cave outside of ",                      "在一处洞穴，位于"),
    (" in a mine outside of ",                      "在一处矿井，位于"),
    (" in the woods outside of ",                   "在树林中，位于"),
    (" in the forest outside of ",                  "在森林中，位于"),
    (" in a cave near ",                            "在一处洞穴，靠近"),
    (" in a mine near ",                            "在一处矿井，靠近"),
    (" in the woods near ",                         "在树林中，靠近"),
    (" in the forest near ",                        "在森林中，靠近"),
    (" in a cave by ",                              "在一处洞穴，旁边"),
    (" in a mine by ",                              "在一处矿井，旁边"),
    (" in the woods by ",                           "在树林中，旁边"),
    (" in the forest by ",                          "在森林中，旁边"),
    (" outside of ",                                "，位于"),
    (" deep within ",                               "深处"),
    (" somewhere in ",                              "，在某处"),
    (" near ",                                      "，靠近"),
    # ── Trade item categories ──────────────────────────────────────────────────
    ("a jar of wizard reagents",                    "一罐法师试剂"),
    ("a jar of necromancer reagents",               "一罐死灵法师试剂"),
    ("a jar of alchemical reagents",                "一罐炼金试剂"),
    ("an enchanted item",                           "一件附魔物品"),
    ("a magic item",                                "一件魔法物品"),
    ("a special item",                              "一件特殊物品"),
    ("a book",                                      "一本书"),
    ("a scroll",                                    "一卷卷轴"),
    ("a wand",                                      "一根魔杖"),
    ("a card game in ",                             "一场纸牌游戏，在"),
    # ── Misc nouns/endings ────────────────────────────────────────────────────
    (" gold.",                                      "枚金币。"),
    (" gold",                                       "枚金币"),
    ("crate of ",                                   "一箱"),
    (" boards",                                     "木板"),
    (" logs",                                       "木材"),
    (" leather",                                    "皮革"),
    (" ingots",                                     "金属锭"),
    (" ore",                                        "矿石"),
    (" potions",                                    "药水"),
    (" reagents",                                   "试剂"),
    # Cave/mine words
    ("cave",  "洞穴"),
    ("mine",  "矿井"),
    ("woods", "树林"),
    ("forest","森林"),
    # ── Trailing continuation ─────────────────────────────────────────────────
    (" that I am ",                                 "，我"),
    (" with ",                                      "，内含"),
    (" in it,",                                     "，"),
]

def load_json(path):
    if os.path.exists(path):
        with open(path, encoding="utf-8") as f:
            return json.load(f)
    return {}

def save_json(path, data):
    with open(path, "w", encoding="utf-8") as f:
        json.dump(data, f, ensure_ascii=False, indent="\t")
        f.write("\n")

def load_order(path):
    if not os.path.exists(path):
        return []
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

    for en_frag, zh_frag in FRAGMENTS:
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
        print(f"Added {len(new_en)} EN keys to scripts-quests.json")
    else:
        print("No new EN keys needed.")

    if new_zh:
        zh_data.update(new_zh)
        save_json(ZH_FILE, zh_data)
        print(f"Added {len(new_zh)} ZH keys to scripts-quests.json")
    else:
        print("No new ZH keys needed.")

    if new_order_entries:
        # Insert after any existing header comments but before first entry
        # Sort new entries by descending length, then merge into existing order
        new_order_entries.sort(key=lambda s: -len(s))

        # Find insertion point: right after the last comment block at the top
        insert_at = 0
        for i, line in enumerate(order):
            if line.strip().startswith("#") or line.strip() == "":
                insert_at = i + 1
            else:
                break

        for i, entry in enumerate(new_order_entries):
            order.insert(insert_at + i, entry)

        save_order(ORDER_FILE, order)
        print(f"Added {len(new_order_entries)} entries to quest-composite-terms-order.txt")
    else:
        print("No new order entries needed.")

    print("Done.")

if __name__ == "__main__":
    main()

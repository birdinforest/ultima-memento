#!/usr/bin/env python3
"""Emit quest.epic.gump.* and quest.courier.mail.* entries for world-player-text.json."""
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
EN = ROOT / "Data/Localization/en/world-player-text.json"
ZH = ROOT / "Data/Localization/zh-Hans/world-player-text.json"

SHARED_EN = {
    "quest.epic.gump.shared.info": (
        "<br><br>These items can be customized to fit your adventuring style. When you obtain one of these items tribute, "
        "single click on the item and select the 'Enchant' option. A menu will appear that will allow you to spend the points "
        "given on whatever attributes you choose. Be careful, as you cannot change an attribute once you select it. Once the "
        "points have been used up, the item will remain as it is."
    ),
    "quest.epic.gump.shared.bare.neutral": (
        "<br><br>{0} will offer you an item of tribute if you retrieve a rare item...<br><br>{1}<br><br>Seek it within {2}.<br><br>...and have achieved "
        "a fame of at least 7,000 points. If you accept their tribute, your fame will decrease by 7,000 points and you will "
        "have to rebuild it again. If you have achieved this amount, single click on {0} and select Tribute to choose the type "
        "of item you want. {0} will also need at least 5,000 gold in order to construct the item for you."
    ),
    "quest.epic.gump.shared.bare.good": (
        "<br><br>{0} will offer you an item of tribute if you retrieve a rare item...<br><br>{1}<br><br>Seek it within {2}.<br><br>...and have achieved "
        "a fame of at least 4,000 points and a karma of at least 4,000 points. If you accept their tribute, your fame and karma "
        "will decrease by 4,000 points and you will have to rebuild them again. If you have achieved these amounts, single click "
        "on {0} and select Tribute to choose the type of item you want. {0} will also need at least 5,000 gold in order to "
        "construct the item for you."
    ),
    "quest.epic.gump.shared.bare.evil": (
        "<br><br>{0} will offer you an item of tribute if you retrieve a rare item...<br><br>{1}<br><br>Seek it within {2}.<br><br>...and have achieved "
        "a fame of at least 4,000 points and a karma of at least -4,000 points or lower. If you accept their tribute, your fame "
        "will decrease by 4,000 points and your karma will increase by 4,000 points. You will have to rebuild them again. If you "
        "have achieved these amounts, single click on {0} and select Tribute to choose the type of item you want. {0} will also "
        "need at least 5,000 gold in order to construct the item for you."
    ),
}

SHARED_ZH = {
    "quest.epic.gump.shared.info": (
        "<br><br>这些赠礼可按你的冒险风格定制。取得赠礼后，单击物品并选择「附魔」，菜单会列出可分配的属性点数。"
        "须谨慎——属性一经选定便无法更改；点数用尽后，物品将保持最终状态。"
    ),
    "quest.epic.gump.shared.bare.neutral": (
        "<br><br>{0} 愿在你寻得一件稀世之物后予你赠礼……<br><br>{1}<br><br>请前往 {2} 取得。<br><br>……且你的声望至少达到 7,000 点。"
        "若接受赠礼，声望将减少 7,000 点，需重新累积。若已达标，请单击 {0} 并选择「赠礼」以挑选物品类型。"
        "{0} 亦需至少 5,000 金币方能为你打造物品。"
    ),
    "quest.epic.gump.shared.bare.good": (
        "<br><br>{0} 愿在你寻得一件稀世之物后予你赠礼……<br><br>{1}<br><br>请前往 {2} 取得。<br><br>……且你的声望至少 4,000 点、"
        "善恶值（Karma）至少 4,000 点。若接受赠礼，声望与善恶值各减 4,000 点，需重新累积。若已达标，请单击 {0} 并选择「赠礼」"
        "以挑选物品类型。{0} 亦需至少 5,000 金币方能为你打造物品。"
    ),
    "quest.epic.gump.shared.bare.evil": (
        "<br><br>{0} 愿在你寻得一件稀世之物后予你赠礼……<br><br>{1}<br><br>请前往 {2} 取得。<br><br>……且你的声望至少 4,000 点、"
        "善恶值（Karma）至多 -4,000 点。若接受赠礼，声望将减 4,000 点、善恶值将增 4,000 点，需重新累积。若已达标，请单击 {0} "
        "并选择「赠礼」以挑选物品类型。{0} 亦需至少 5,000 金币方能为你打造物品。"
    ),
}

COURIER_EN = {
    "quest.courier.mail.opl.name": "Message for {0}",
    "quest.courier.mail.opl.from": "From {0}",
    "quest.courier.mail.opl.complete": "Complete",
    "quest.courier.mail.backpack": "This must be in your backpack to read.",
    "quest.courier.mail.found.prefix": "You have found the '{0}'. Return to {1} and bring them this message.<br><br>",
}

COURIER_ZH = {
    "quest.courier.mail.opl.name": "致 {0} 的信函",
    "quest.courier.mail.opl.from": "来自 {0}",
    "quest.courier.mail.opl.complete": "已完成",
    "quest.courier.mail.backpack": "此信必须放在背包中才能阅读。",
    "quest.courier.mail.found.prefix": "你已找到「{0}」。回去找 {1}，把这段话带给他们。<br><br>",
}

# slug -> (title_en, title_zh, allowed_en, allowed_zh, denied_en, denied_zh)
def npc_entry(slug, title_en, title_zh, allowed_en, allowed_zh, denied_en, denied_zh):
    return {
        f"quest.epic.gump.{slug}.title": title_en,
        f"quest.epic.gump.{slug}.text.allowed": allowed_en,
        f"quest.epic.gump.{slug}.text.denied": denied_en,
    }, {
        f"quest.epic.gump.{slug}.title": title_zh,
        f"quest.epic.gump.{slug}.text.allowed": allowed_zh,
        f"quest.epic.gump.{slug}.text.denied": denied_zh,
    }


def main():
    en_patch = dict(SHARED_EN)
    en_patch.update(COURIER_EN)
    zh_patch = dict(SHARED_ZH)
    zh_patch.update(COURIER_ZH)

    # Full NPC table loaded from companion JSON if present
    npc_file = Path(__file__).with_name("epic_gump_npc_locale.json")
    if npc_file.exists():
        data = json.loads(npc_file.read_text(encoding="utf-8"))
        for slug, row in data.items():
            e, z = npc_entry(slug, *row)
            en_patch.update(e)
            zh_patch.update(z)

    for path, patch in ((EN, en_patch), (ZH, zh_patch)):
        obj = json.loads(path.read_text(encoding="utf-8"))
        obj.update(patch)
        path.write_text(json.dumps(obj, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
        print(f"Updated {path.name}: +{len(patch)} keys")


if __name__ == "__main__":
    main()

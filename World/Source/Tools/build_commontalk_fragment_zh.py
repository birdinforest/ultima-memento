#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Build commontalk-fragment-zh.json from TavernPatrons.CommonTalk static English lines + ZH lines.

Run from repo root:
  python3 World/Source/Tools/build_commontalk_fragment_zh.py

Then:
  python3 World/Source/Tools/gen_quest_fragment_translations.py
Append new keys to quest-composite-terms-order.txt if this script prints missing-order warnings.
"""
from __future__ import annotations

import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
SRC = ROOT / "Source/Scripts/Mobiles/Civilized/Citizens/TavernPatrons.cs"
KEYS_JSON = ROOT / "Data/Localization/_commontalk_keys.json"
OUT = ROOT / "Data/Localization/commontalk-fragment-zh.json"
ORDER = ROOT / "Data/Localization/quest-composite-terms-order.txt"


def extract_keys() -> list[str]:
    text = SRC.read_text(encoding="utf-8")
    start = text.index("public static string CommonTalk")
    end = text.index("return sWords;", start)
    sub = text[start:end]
    uniq: list[str] = []
    seen: set[str] = set()
    for line in sub.splitlines():
        if "sWords" not in line or "+" in line:
            continue
        m = re.search(r'sWords\s*=\s*"((?:\\.|[^"\\])*)"\s*;\s*break', line)
        if not m:
            continue
        s = m.group(1).replace('\\"', '"')
        if s not in seen:
            seen.add(s)
            uniq.append(s)
    return uniq


# Literary zh-Hans; proper nouns follow common UO/Memento usage where obvious.
_ZH_LINES = r"""
索沙尼亚有座皎洁的白色神殿，可通往月界。
一座由邪恶法师统治的城堡，其主事者更是可憎的大法师。
洛多尔沼泽中有一处洞穴，栖居着鳞肤类人。
蛮荒帝国有一处仙灵洞窟，住着疯癫德鲁伊。
洛多利亚以西有一处洞穴，盘踞着巨蛇、双头巨魔与巨魔。
灵魂洞窟之下有一座地下墓穴。
洛多尔寒地深处有一座亡者深牢。
血之半神已然归来。
一名魔主腐化着安卡地城的核心。
蛮荒帝国沼泽深处有远古石像鬼巢穴。
达丁深坑之底栖着洞熊之巢。
私语者村落以北有龙巢。
洛多尔岛上有鹰身女妖巢穴。
索沙尼亚西南区域有一座末日地城。
一伙兽人术士正密谋对付我们。
冰霜女王所居的冰封宫殿。
蛮荒帝国废墟中有幽灵作祟。
淹没神殿之底栖着巨型乌贼。
线索地城中可窥见无数秘闻。
污秽地城有一群拜魔者。
巨蛇圣所中有一群蛇人崇拜者。
折磨地城盘踞着恶魔大军。
乌伯维尔群山深处藏着骇人秘密。
白色神殿以北有吸血鬼巢穴。
洛多尔聚集着大批造船匠。
火焰地城有一面魔镜。
数百年前造就的魔法树篱迷宫。
索沙尼亚法老墓之底有魔法传送门。
蛮荒帝国境内有魔法传送门。
一道魔法封印，阻巫妖王脱逃。
那道古老树篱迷宫由牛头人看守。
索沙尼亚沙漠中有法老陵墓。
污秽地城之底有一池秽液。
索沙尼亚某塔中有强大巫妖游荡，并持魔镜相伴。
蛮荒帝国旧墓园附近有原始兽人要塞。
轻蔑地城盘踞蛇人一族。
蛮荒帝国旧墓园有秘密入口。
乌伯维尔城堡中有密道。
蛮荒帝国海上某城堡中有风暴巨人。
蛮荒帝国有一条扭曲隘道，不死德鲁伊徘徊其间。
蛮荒帝国有一处独眼巨人谷。
一条地下通道连接洛多利亚北部与中部的岛屿。
索沙尼亚灰镇以北有一座废矿。
蛮荒帝国某祭坛上，有人向龙王献祭。
恐惧群岛有远古血祭教团。
石像鬼曾安葬亡者的古老墓穴。
洛多尔地底深处有远古暗精灵城。
神秘树篱迷宫之下蛰伏远古之恶。
洛多尔洞穴深处的远古巢穴，法师与元素盘踞其间。
蛮荒帝国某岛上有远古巫妖要塞。
巨蛇岛沙漠深处藏着古代监狱。
憎恨地城之下沉睡着远古古龙。
一条精灵隘道通向杰出工匠之手。
愤怒地城鼠蛇成灾。
蛮荒帝国某岛栖着蓝鳞飞龙。
索沙尼亚一座废屋的地下室藏有宝物。
兽人预言其神将归来统治。
蛮荒帝国灯塔之下另有秘密。
蛮荒帝国地底深处遍布远古墓穴。
洛多利亚只有游侠或探险者才能穿越的洞窟。
索沙尼亚北部要塞中有强盗盘踞。
异乡人摧毁后，出埃及城堡已成废墟。
洛多利亚城下有地下墓穴。
那些地城中有满锅的药剂。
德拉库尔在洛多尔冰窟中召唤恶魔。
洛多尔沙漠沙下恶魔将脱困而出。
一座名为欺诈的地城，住着极强巫妖。
洛多尔山中有座古庙，邪人在此盘踞。
火焰洞窟中火甲虫筑巢。
陨落法师之墓由多种元素守护。
冰霜女王召唤冰塑仆从。
蛮荒帝国矿坑被鼠人占据。
恐惧群岛北部有蛮族挖掘的矿井。
巨蛇岛上有强大受诅生物游荡。
强力卷轴只能在安布罗西亚神殿使用。
恐惧群岛有原始部落小聚落。
灾厄地城中有剧毒生物。
索沙尼亚旧废墟下鼠人穴居。
传说米斯塔斯城数百年前被海吞没。
德斯塔德地城有暗精灵召唤恶魔。
精灵持有的秘石可为万物着色。
洛多利亚墓园另有隐情。
索沙尼亚沼泽古庙中，巫妖等候预言应验。
洛多尔丛林城堡中有可憎蛛怪。
乌伯维尔墓中埋着远古遗物。
破败巫师居所中有强大法术书。
友善巨龙栖于索沙尼亚冰岛之下。
索沙尼亚弃置的伐木工屋地板下另有玄机。
索沙尼亚北部有强盗挟持王室要犯。
索沙尼亚某塔中巫妖执掌强力法杖。
蒙代恩之颅深埋出埃及城堡之下。
索沙尼亚灯塔看守在岸边兜售强大奇物。
索沙尼亚沼泽中有巫妖携奇珍而行。
那些魔法池底满箱珍宝。
达丁深坑之底有强大巨魔领主。
末日地城中有魔王可赐愿望。
一双秘靴可踏熔岩而行。
莫瑞尼亚矿坑出产上佳矿石。
那位时之领主把人送往过去或未来。
洛多利亚墓园墓穴之下有密道。
不列颠家族墓室有一道破墙。
月镇以南的农田遭食人魔与双头巨人焚烧。
灰镇有人在掘墓。
洛多尔南部有火山巨龙。
洛多尔岛上有吸血鬼主宰。
洛多尔那座死岛上只有死灵法师与死亡骑士栖居。
斯卡拉·布雷镇并非真被法师所毁。
法师曼加尔在索沙尼亚某处建了高塔。
某位异乡人终结了出埃及。
有人自斯卡拉·布雷逃出。
黑骑士宝库辽阔难尽探。
可经蜥蜴人洞窟抵达幽暗地域厅堂。
有人触碰曼加尔塔中的水晶球后消失。
空橡木架实则是盗贼公会之门。
附近藏着黑魔法行会。
黑骑士将整座城封入瓶中。
法师沃尔多能使整座岛消失。
失落的祖鲁族能驾驭传说中的龙裔。
龙裔曾为古龙后裔。
鳞若宝石的龙形生物。
波塞冬之手使岛屿浮现。
有人自不列颠领主城堡牢房逃脱。
不列颠领主城堡下被人遗忘的大厅。
邪教徒正使卡齐巴尔复生。
不列颠城堡下蛰伏远古之恶。
索沙尼亚不灭之火中走出死灵法师。
兽人一族历经数百年缓慢变异。
恐惧群岛暗礁有水手探查。
城堡深处有死灵法师修炼黑魔法。
乌伯维尔出现黄铜之塔。
兽人部落发现了失落的银矿。
石门城堡已废弃，因内中人尽遭屠戮。
影主们占据了石门城堡。
独眼巨人军阀搜银以铸军械。
邪恶骑士持有蒙代恩之颅。
可憎巫师持有不朽宝石。
有人驾船遍历恐惧群岛兜售稀有法术。
泰坦自天穹掷下雷霆。
远古古龙守望着通往隐秘谷之路。
疯癫法师充任卡齐巴尔的大祭司。
传说阿泽洛克仍住在一座岛邸。
被遗忘灯塔之下有隐秘洞窟。
城堡里有只爱奶酪的多话小鼠。
月石几乎可从任何地方召唤月门。
矿工们说莫瑞尼亚是极佳的采矿之地。
莫瑞尼亚矿坑中有一些水晶。
传奇矿工掘出了矮人矿石。
传奇伐木工采得精灵之木
""".strip().splitlines()


def main() -> int:
    keys = extract_keys()
    if len(keys) != len(_ZH_LINES):
        print(f"error: key count {len(keys)} != zh line count {len(_ZH_LINES)}", flush=True)
        return 1
    out = {k: z for k, z in zip(keys, _ZH_LINES)}
    OUT.write_text(json.dumps(out, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print("wrote", len(out), "entries ->", OUT)

    order_text = ORDER.read_text(encoding="utf-8")
    order_lines = {ln.strip() for ln in order_text.splitlines() if ln.strip() and not ln.lstrip().startswith("#")}
    missing = [k for k in keys if k not in order_lines]
    if missing:
        print("warning:", len(missing), "keys not in quest-composite-terms-order.txt — append block:", flush=True)
        print("# --- TavernPatrons.CommonTalk static (longest-first batch; keep together) ---", flush=True)
        for k in sorted(missing, key=len, reverse=True):
            print(k, flush=True)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

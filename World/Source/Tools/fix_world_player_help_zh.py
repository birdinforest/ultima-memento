#!/usr/bin/env python3
"""Rebuild zh-Hans world-player-text sys.if_you_are... help scroll with EN-identical <br> structure."""
from __future__ import annotations

import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2] / "Data/Localization"
HELP_KEY = "sys.if_you_are_looking_for_help_exploring_this_world_c_you_can_learn_about_almost_anything_within_the_ga"

ZH_INTRO = (
    "若汝欲谙此方天地之游历法门，于此游戏世界中几近诸事皆可习得。商贩多有贩售卷轴书册，阐明技艺施展、物资采集乃至乾坤造物之理。"
    "智者（sage）常蓄典籍无数，兼涉武艺异能与众学派法术。倘汝全然初涉世事，可向智者购取《冒险指南》（Guide to Adventure）——若启程所赐已佚——"
    "此书阐明跋涉操控之道，亦述市井交涉、器物施用及身故而后处置之法。多与城镇居民交谈以广见闻。"
    "于此界面之上，多有选项、讯息与设定可佐征途；此中颇多皆可藉键盘号令唤起，详见下文。"
    "切莫疏忽角色纸娃娃「资讯」一栏，其中有身家紧要之机宜。<br><br>常用指令：下列号令可于诸事中使用：<br><br>"
)

COMPOUND35_ZH = (
    "[wealth - 开启横窄财帛栏，示行囊与银行中金铢及各种通货估值；通货乃须托银行吏折算之物（银、铜、xormite、珠宝与水晶）。"
    "若将此等器物置入银行，可右键财帛栏以刷新估值。<br><br><br><br>"
    "地域凶险层级：踏入诸多厄境时，必有讯息告汝入境何处；括号中或示凶险层级，俾汝知其难易。各级释义如下：<br><br>"
    " - 平易（不足挂齿）<br>"
    " - 寻常（中流砥柱）<br>"
    " - 棘手（稍胜一筹）<br>"
    " - 严峻（恐频遁逃）<br>"
    " - 凶厄（恐频陨命）<br>"
    " - 绝死（敢尔一试）<br><br><br>"
    "技艺头衔：可为角色预设所示头衔；纵使声名赫赫，亦或愿署初学巫师之名——示例如斯：<br><br>"
    "键入 '[SkillName' 号令，继以引号包裹所欲技艺之名，务必小写。示例：<br>"
    '  [SkillName "taming"<br><br>'
    "若欲令系统自动执掌头衔，可用同一号令而以技艺名为 \"clear\"。<br><br><br><br>"
    "试剂栏：下列号令可观照行囊试剂存量，施法或熬炼药剂时颇为得力；栏式可自定，数值随施法熬炼而刷新，亦可自作宏以此号令手动刷新。<br><br>"
)

COMPOUND37_ZH = (
    "[regclose - 阖试剂栏。<br><br><br><br>"
    "法术工具栏：下列号令可摆布法术快捷栏以助施用：<br><br>"
)

ZH_TAIL_ZH = (
    "[necroclose2 - 阖第二道死灵术士法术栏。<br><br><br>"
    "乐律：游戏中颇多古典乐章，随地而异或入境更换。或有取自原作之曲，亦或取自更早之作；亦有九十年代微机游戏之旋律，颇契远行风尘。"
    "汝可任其播放，抑或自选曲调；须知易地则默认乐章复起，又须再行更易。客户端亦尝欲令一曲奏顷许乃允更替。"
    "下列号令可开窗择曲；大抵循环往复，惟三支不以星号标记者不循环；曲目分两页，可用顶端箭头往返；曲既奏响，可按 OKAY 离去。"
    "纵似末节小技，亦可稍驭耳畔之境。<br><br>"
    "[music - 开启曲目单与播放器。<br><br>"
    "下列号令惟切换地城默认配乐：开启后于地城播放陆上常用之乐，而非寻常地城旧曲。<br><br>"
    "[musical - 设定默认地城配乐。<br><br><br><br>"
    "邪恶风味：世间别有邪恶一脉，或有嗜之者。若扮演死灵术士（Necromancers）之流，欲在此风中遨游，可用此项切换寻常与邪恶口吻。"
    "邪恶模式下瑰宝名号往往暗黑；业力恒负时若干技艺头衔亦随之更易——非尽然也，详览技艺头衔之书（可于世界中觅得）。"
    "所得遗物亦或沾染邪氛，可供装点居室。此项随时可开关；同一时辰惟得一风格。<br><br>"
    "[evil - 开关邪恶风味叙事。<br><br><br><br>"
    "东方风味：世间亦有东方一脉，未必人人嗜之。忍者（Ninja）与武士（Samurai）之流或欲借此风味徜徉。"
    "此项可于奇幻与东方口吻间切换：东方模式下瑰宝半数出自中日古风器物；昔日主人遗物名号亦往往随之更易；技艺头衔亦复有所更易——亦非尽然，详览技艺头衔之书。"
    "画作遗物亦或风华一变，以供装点居室。此项随时可开关；同一时辰惟得一风格。<br><br>"
    "[oriental - 开关东方风味叙事。<br><br><br><br>"
    "蛮荒风味：默认体裁未必利于剑与魔法之酣畅——身披陋褐、仗巨斧漫游之士未必尽兴；常人叠甲裹铠以求苟活。"
    "此项可得助力：启用后主行囊将出现皮囊一枚，彼不可盛装杂物，惟可将置入之物幻化形制——盾牌、帽盔、上衣、袖铠、裤甲、靴履、颈铠、手套、项链、斗篷与长袍之类皆可更易外形；"
    "效能如故，惟外观上或不显于躯体。披袍则蔽上衣袖，蛮荒风格长袍亦然，欲观内衣须先褪袍。"
    "多项技艺头衔亦有专属称谓；若为巾帼且继续按键，可将「Barbarian」头衔化作「Amazon」。开启皮囊可读详尽说明。"
    "此项随时可开关；同一时辰惟得一风格。<br><br>"
    "[barbaric - 开关蛮荒风味叙事。<br><br><br><br><br>"
    '<BASEFONT Color="#888888" Size=6></BASEFONT>'
)


def chunkify_help(en_help: str) -> list[str]:
    rx = re.compile(r"\[(?:[\w]+|spellhue ##)\s-\s")
    starts = [m.start() for m in rx.finditer(en_help)]
    starts.append(len(en_help))
    return [en_help[: starts[0]]] + [en_help[starts[i] : starts[i + 1]] for i in range(len(starts) - 1)]


def load_cmd_zh(tsv_path: Path) -> dict[str, str]:
    out: dict[str, str] = {}
    for line in tsv_path.read_text(encoding="utf-8").splitlines():
        line = line.strip("\ufeff")
        if not line.strip():
            continue
        cmd, zh = line.split("\t", 1)
        out[cmd.strip()] = zh.strip()
    return out


def rebuild_simple_chunk(en_chunk: str, cmd_zh: dict[str, str]) -> str:
    inner = en_chunk[1:]
    cmd_full, body = inner.split(" - ", 1)
    cmd_key = cmd_full.strip()
    sm = re.search(r"((?:<(?:BR|br)[^>]*>)+\s*)$", body)
    if sm is None:
        raise ValueError(f"No <br> suffix for chunk: {en_chunk[:80]!r}")
    sep = sm.group(1)
    try:
        zh_desc = cmd_zh[cmd_key]
    except KeyError as exc:
        raise KeyError(cmd_key) from exc
    return f"[{cmd_key} - {zh_desc}{sep}"


def rebuild_help(en_help: str, cmd_zh: dict[str, str]) -> str:
    chunks = chunkify_help(en_help)
    if len(chunks) != 109:
        raise RuntimeError(f"Unexpected chunk count {len(chunks)}")

    parts: list[str] = [ZH_INTRO]

    for idx in range(1, 103):
        if idx == 35:
            parts.append(COMPOUND35_ZH)
        elif idx == 37:
            parts.append(COMPOUND37_ZH)
        else:
            parts.append(rebuild_simple_chunk(chunks[idx], cmd_zh))

    parts.append(ZH_TAIL_ZH)
    zh_help = "".join(parts)

    if en_help.lower().count("<br") != zh_help.lower().count("<br"):
        raise RuntimeError(
            f"<br> count mismatch en={en_help.lower().count('<br')} zh={zh_help.lower().count('<br')}"
        )

    return zh_help


def main() -> None:
    en_path = ROOT / "en/world-player-text.json"
    out_path = ROOT / "zh-Hans/world-player-text.json"
    tsv_path = ROOT / "tools-output/help_cmd_zh.tsv"

    doc = json.loads(en_path.read_text(encoding="utf-8"))
    cmd_zh = load_cmd_zh(tsv_path)

    if len(cmd_zh) != 100:
        raise RuntimeError(f"Expected 100 cmd rows, got {len(cmd_zh)}")

    zh_help = rebuild_help(doc[HELP_KEY], cmd_zh)

    zh_doc = json.loads(out_path.read_text(encoding="utf-8"))
    zh_doc[HELP_KEY] = zh_help
    out_path.write_text(json.dumps(zh_doc, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")

    print(f"Updated {HELP_KEY} in {out_path}")
    print(f"cmd rows={len(cmd_zh)} chunks_ok br_en={doc[HELP_KEY].lower().count('<br')} br_zh={zh_help.lower().count('<br')}")


if __name__ == "__main__":
    main()

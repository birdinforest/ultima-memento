#!/usr/bin/env python3
"""
Add remaining short translations to the response JSON.
"""

import json
import re

RESPONSE_PATH = "/Users/forrrest/projects/UO-Memento/ultima-memento/World/Data/Localization/tools-output/cliloc-batches/responses/response-04-items-3.json"

ADDITIONS = {
    "flame when one passeth across it!": "火焰——當你經過其上時！",
    "Affiliations: League of Rangers": "附屬組織：遊俠聯盟",
    "extraordinarily beautiful song.": "格外美妙的歌聲。",
    "You can't summon a rising colossus there!": "你無法在那裡召喚升起巨像！",
    "Add to Ignore": "加入忽略列表",
    "Add": "新增",
    "You feel nether energy surge through you.": "你感到冥界能量湧過全身。",
    "Sweet dreams": "甜蜜的夢",
    "Yours-4-ever": "永遠屬於你",
    "Be mine": "做我的吧",
    "You're cute": "你真可愛",
    "Let's be friends": "讓我們做朋友吧",
    "Be my valentine": "做我的情人",
    "You're sweet": "你真貼心",
    "Someone likes you": "有人喜歡你",
    "True love": "真愛",
    "Always together": "永遠在一起",
    "Thinking of you": "想念你",
    "Kiss me": "吻我",
    "*wink*": "*眨眼*",
    "Hot stuff": "辣妹／帥哥",
    "You're sexy": "你很性感",
    "Tasty!": "美味！",
    "You're the best": "你是最棒的",
    "Someone loves you": "有人愛你",
    "*hug*": "*擁抱*",
    "Sweet memories": "甜蜜的回憶",
    "How about a date?": "約個會怎麼樣？",
    "Let's be impulsive": "讓我們衝動一次",
    "a freshly picked rose from ~1_NAME~": "來自 ~1_NAME~ 的一朵剛摘的玫瑰",
    "Turns on War mode. Enables you to attack targets.": "開啟戰爭模式。允許你攻擊目標。",
    "Commands all nearby followers to follow you.": "命令所有附近的跟隨者跟隨你。",
    "Commands all nearby followers to guard you.": "命令所有附近的跟隨者守衛你。",
    "Commands the ship tiller man to stop the ship.": "命令船隻舵手停止船隻。",
    "Gathers nearby resources using the tool specified.": "使用指定工具收集附近的資源。",
    "Executes specified Lua script commands.": "執行指定的Lua腳本命令。",
    "Toggles the Always Run option.": "切換始終跑步選項。",
    "Toggles the Circle of Transparency option.": "切換透明度圈選項。",
    "Exits the game.": "離開遊戲。",
    "You have used up the item.": "你已用完了該物品。",
    "Enable Autorun": "啟用自動跑步",
    "Auto-Navigate Around Objects": "繞過物體自動導航",
    "Ranger of the Abyss Part 2: Seeds of Virtue": "深淵遊俠 第二部：美德種子",
    "A random treasure from Sir Geoffrey's trunk": "一份來自傑佛瑞爵士箱子的隨機寶藏",
    "Gardener's Toolbox": "園丁工具箱",
    "Pen of Wisdom": "智慧之筆",
    "Warden of the Abyss Part 1: A Lamp of Singularity": "深淵守護者 第一部：奇點之燈",
    "You have drunk up the bottle.": "你喝光了瓶子裡的東西。",
    "Warden of the Abyss Part 2: Elements of Order": "深淵守護者 第二部：秩序元素",
    "So be it.": "那就這樣吧。",
    "Target your Lamp of Spirituality.": "鎖定你的靈性之燈。",
    "Target your Lamp of Singularity.": "鎖定你的奇點之燈。",
    "Ranger of the Abyss Part 3: The Ink is Mighty": "深淵遊俠 第三部：墨水之力",
    "Warden of the Abyss Part 3: Words of Power": "深淵守護者 第三部：力量之言",
    "You take a satchel from the war chest.": "你從戰爭寶箱中取走一個小背包。",
    "Cycle Chat Forward": "向前循環聊天",
    "Cycle Chat Backward": "向後循環聊天",
    "<BODY><CENTER>Character Copy</CENTER></BODY>": "<BODY><CENTER>角色複製</CENTER></BODY>",
    "<BODY><CENTER>Copy Summary</CENTER></BODY>": "<BODY><CENTER>複製摘要</CENTER></BODY>",
    "Figures....": "我就知道……",
    "As-s-s-s you wish-sh-sh-sh....": "如-s-s-你所願-sh-sh-sh……",
    "S-s-s-so be it... friend.": "那-s-s-就這樣吧……朋友。",
    "a glass of ~1_DRINK_NAME~": "一杯 ~1_DRINK_NAME~",
    "You receive a reward: ~1_QUANTITY~ ~2_ITEM~": "你獲得獎勵：~1_QUANTITY~ ~2_ITEM~",
    "You are too far away from the arena stone.": "你離競技場石太遠。",
    "Your target resists the effects of your spellsong.": "你的目標抵抗了你法術之歌的效果。",
    "You resist the effects of the spellsong.": "你抵抗了法術之歌的效果。",
    "Your spellsong has finished.": "你的法術之歌已結束。",
    "a dust pile<br>": "一堆灰塵<br>",
    "You must wait a while for this item to recharge": "你必須等待一段時間讓此物品充能",
    "You must be carrying this item to use it": "你必須攜帶此物品才能使用",
    "reward template: ~1_QUANTITY~ ~2_ITEMNAME~": "獎勵範本：~1_QUANTITY~ ~2_ITEMNAME~",
    "reward template: ~1_ITEMNAME~": "獎勵範本：~1_ITEMNAME~",
    "empty reward template": "空獎勵範本",
    "cannonball": "砲彈",
    "(Eat to increase hit chance: ~1_val~)": "（食用以提高命中機率：~1_val~）",
    "(Eat to increase defense chance: ~1_TOKEN~)": "（食用以提高防禦機率：~1_TOKEN~）",
    "(Eat to soak fire damage: ~1_val~)": "（食用以吸收火焰傷害：~1_val~）",
    "(Eat to soak physical damage: ~1_val~)": "（食用以吸收物理傷害：~1_val~）",
    "(Eat to soak cold damage: ~1_val~)": "（食用以吸收寒冷傷害：~1_val~）",
    "(Eat to soak poison damage: ~1_val~)": "（食用以吸收毒傷害：~1_val~）",
    "(Eat to soak energy damage: ~1_val~)": "（食用以吸收能量傷害：~1_val~）",
    "(Eat to increase weapon damage: ~1_val~)": "（食用以提高武器傷害：~1_val~）",
    "(Eat to increase spell damage: ~1_val~)": "（食用以提高法術傷害：~1_val~）",
    "(Eat to increase casting focus: ~1_val~)": "（食用以提高施法專注：~1_val~）",
    "(Eat to increase soul charge ability: ~1_val~)": "（食用以提高靈魂充能能力：~1_val~）",
    "(Eat to increase meditation skill: ~1_val~)": "（食用以提高冥想技能：~1_val~）",
    "(Eat to increase focus skill: ~1_val~)": "（食用以提高專注技能：~1_val~）",
    "(Eat to increase hp regeneration: ~1_val~)": "（食用以提高生命恢復：~1_val~）",
    "(Eat to increase mana regeneration: ~1_val~)": "（食用以提高魔力恢復：~1_val~）",
    "(Eat to increase stamina regeneration: ~1_val~)": "（食用以提高精力恢復：~1_val~）",
    "<CENTER>~1_VAL~~2_VAL~</CENTER>": "<CENTER>~1_VAL~~2_VAL~</CENTER>",
    "Fill the crate on your ship with the correct fish.": "用正確的魚裝滿你船上的板條箱。",
    "~1_NAME~ deploys a ship cannon.": "~1_NAME~ 部署了一門船砲。",
    "~1_NAME~ dismantles the ship cannon.": "~1_NAME~ 拆除了船砲。",
    # Valentine poems / messages
    "procession, and they lie, grasping weapons to protect themselves like knights still in battle, shattered armor shining like newly born stars.": "隊伍中，他們躺臥著，緊握武器如仍在戰鬥中的騎士，破碎的盔甲閃耀如新生的星辰。",
    # Already translated fragments
    "for their wool. Yet 'tis lesser known that their ornery disposition and tendency to spit at those they dislike makes them appealing guard creatures as well, though they have little sound with which": "取其羊毛。但鮮為人知的是，牠們倔強的性格和向不喜歡的人吐口水的傾向，也使牠們成為有吸引力的護衛生物，儘管牠們幾乎沒有聲音可以用來",
    "to sound an alarum.": "發出警報。",
    "This volume was sponsored by donations from Lord Blackthorn, ever a supporter of understanding the other sentient races of Britannia.<br><br>---": "本卷由黑荊棘勳爵的捐贈贊助，他一直是理解不列顛尼亞其他有智慧種族的支持者。<br><br>---",
    "tongue -- indeed, we must hope that wisps learn our language, for it is not possible for humans to pronounce wispish!<br><br> The wispish language seems to only contain one": "我們的語言——事實上，我們必須希望精靈學習我們的語言，因為人類不可能發出精靈語的語音！<br><br> 精靈語似乎只包含一個",
    "vowel, the letter Y. However, the letters W, C, M, and L seem to be treated grammatically as vowels, and in addition every letter is followed by what sounds to the human ear like a": "母音，字母Y。然而，字母W、C、M和L似乎在語法上被視為母音，而且每個字母後都跟著人類聽起來像",
    "glottal stop. It is possible that the glottal stop is considered a vowel as well.<br><br> Wisps do make use of what sounds to us like pitch and emphasis shifts": "喉塞音。有可能喉塞音也被視為一個母音。<br><br> 精靈確實運用了我們聽起來像是音高和強調變化的聲音，",
    "similar to exclamations and questions.<br><br> The average word in wispish seems to consist of three phonemes and three glottal stops, plus possibly a pitch shift.": "類似於感嘆和疑問。<br><br> 精靈語中的平均單詞似乎由三個音素和三個喉塞音組成，加上可能的音高變化。",
    "It often sounds like a fire burning or crackling. Some have speculated that what we are analyzing is in fact nothing more than the very air crackling near the wisp's glow, and not": "它聽起來常常像火焰在燃燒或劈啪作響。有些人推測，我們正在分析的實際上不過是精靈光芒附近空氣的劈啪聲，而非",
    "language, but this is of course unlikely.": "語言，但這當然不太可能。",
}

with open(RESPONSE_PATH, 'r', encoding='utf-8') as f:
    data = json.load(f)

added = 0
for key, value in data.items():
    for eng, chn in ADDITIONS.items():
        if value == eng:
            data[key] = chn
            added += 1
            break

# Fix the <div> entry that was partially translated
# 1114887 and 1114980
for k in data:
    if data[k] == '<DIV ALIGN=CENTER>／</DIV>':
        data[k] = '<DIV ALIGN=CENTER>／</DIV>'  # keep as is (already looks right in Chinese context)

with open(RESPONSE_PATH, 'w', encoding='utf-8') as f:
    json.dump(data, f, ensure_ascii=False, indent=1)

print(f"Added {added} translations")
print(f"Total entries: {len(data)}")

# Verify
import re
translated = sum(1 for v in data.values() if re.search(r'[\u4e00-\u9fff]', v))
print(f"Now translated: {translated}, untranslated: {len(data) - translated}")

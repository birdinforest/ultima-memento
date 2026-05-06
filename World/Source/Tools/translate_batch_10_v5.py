#!/usr/bin/env python3
"""Translate remaining English entries (batch 10) - fifth and final pass."""

import json
import unicodedata
import os

source_path = '/Users/forrrest/projects/UO-Memento/ultima-memento/World/Data/Localization/tools-output/cliloc-batches/groups/10-game-text-4.json'
output_path = '/Users/forrrest/projects/UO-Memento/ultima-memento/World/Data/Localization/tools-output/cliloc-batches/responses/response-10-game-text-4.json'

with open(source_path, 'r') as f:
    source = json.load(f)
with open(output_path, 'r') as f:
    output = json.load(f)

def has_chinese(s):
    for c in s:
        try:
            if 'CJK' in unicodedata.name(c):
                return True
        except:
            pass
    return False

translations = {
    # === Eodon lore ===
    "1156580": "這些卵無疑是由蟻后（Queen）提供給工蟻的，不過這只是我的假設，因為我們尚未深入探索蟻族巢穴（Myrmidex Pits）的中心區域。",

    # === More Receive item descriptions ===
    "1156645": "獲得一個代幣，允許你從最多 18 個模板中選擇一個（基於帳號權限），這些模板將提供符合你遊戲風格的入門技能和物品。",
    "1156646": "獲得一個墮落戰馬（Charger of the Fallen）雕像。此祝福雕像可用於創建一個身穿盔甲的幽靈坐騎。",
    "1156647": "獲得一個代幣，使用後將在你的銀行箱中創建一個帳號綁定的靈魂石（Soulstone）。靈魂石允許你將技能從一個角色轉移到同一個帳號的另一個角色。",
    "1156648": "獲得一個代幣，使用後將在你的銀行箱中創建一個帳號綁定的靈魂石（Soulstone）。靈魂石允許你將技能從一個角色轉移到同一個帳號的另一個角色。",
    "1156651": "獲得一個拴馬柱（Hitching Post）。此裝飾物品允許你從家中訪問角色的馬廄。拴馬柱可以朝南或朝東放置。",
    "1156653": "獲得一個先祖墓碑（Ancestral Gravestone）。先祖墓碑是一個恐怖的裝飾物品，還可以為玩家提供安全的重生點。",
    "1156654": "獲得一袋十二種不同的批量訂單書封面。這些封面有你最喜歡的金屬和皮革色調。",
    "1156655": "獲得一個木書架（Wooden Bookcase）。木書架可在你的家中作為容器使用。木書架還可以讓你以精選的形式展示最多六本書。",
    "1156663": "獲得一個殯儀師之杖（Undertaker's Staff），使用時將檢索你的角色屍體和物品，並將它們傳送到你的家中。每次使用之間需要經過一段指定時間。",
    "1156664": "獲得走私者之刃（Smuggler's Edge）屠夫刀。此武器允許玩家有機率從怪物身上偷取物品，包括特殊物品。",
    "1156666": "獲得商人小飾品（Merchant's Trinket）耳環。當在玩家運營的商人身上裝備這些耳環時，將為其他玩家提供 5% 的購買折扣。",
    "1156667": "獲得商人小飾品（Merchant's Trinket）耳環。當在玩家運營的商人身上裝備這些耳環時，將為其他玩家提供 10% 的購買折扣。",
    "1156668": "獲得一個包含兩個連結房屋傳送門地磚（House Teleporter Tiles）的袋子。這些傳送門地磚允許玩家在房屋的兩個位置之間傳送。",
    "1156669": "獲得一個智慧之筆（Pen of Wisdom），與特定材料（回憶符文、標記卷軸和符文書）結合使用時，允許有足夠 inscription 技能的玩家製作可設定的符文書。",
    "1156670": "獲得一個契約，使用後將創建一個裝飾性木匠工作台（Woodworker's Bench）。工作台可以朝南或朝東放置，也可作為容器使用。",
    "1156672": "獲得一個不列顛尼亞船（Britannian Ship）契約。此契約允許你放置豪華的不列顛尼亞船，這是遊戲中最大的船隻。",
    "1156673": "獲得一個不列顛尼亞船（Britannian Ship）契約。此契約允許你放置豪華的不列顛尼亞船，這是遊戲中最大的船隻。",
    "1156677": "獲得一個改名代幣（允許你更改一名角色的名稱）、一個種族變更代幣（允許你將一名角色更改為可用的種族之一）以及一個性別變更代幣（允許你更改一名角色的性別）。",
    "1156679": "獲得一個代幣，允許你選擇 5 項技能達到 90.0，並將你的屬性設定為總和 225（或更多，取決於之前的角色屬性點數）。此外還包括一個靈魂石（Soulstone）代幣。",
    "1156680": "獲得三個 raised garden bed 契約，放置在你的家中後，將允許你種植最多九顆植物種子並收穫作物供廚房系統使用。",

    "1156723": "伊歐頓（Eodon）的地理環境是典型的亞熱帶火山山谷。最顯著的地理特徵是高聳於山谷中心的大金字塔（Great Pyramid）。",

    # === Myrmidex quest text ===
    "1156752": "蟻族（Myrmidex）統治者萬歲！既然不列顛尼亞人（Britannians）和伊歐頓（Eodon）其他部族的攻勢已被阻止，我們終於可以專注於消滅齊帕克特里奧特爾（Zipactriotl）——在科特爾人（Kotl）古代機器的幫助下！",
    "1156754": "你必須從微光寶石（Shimmering Jewel）在維斯珀（Vesper）的寶石學家那裡獲取激活器（Activator），從不列顛尼亞南海的船長那裡獲取調節器（Regulator），從巴林（Balin）那裡獲取專注水晶（Focus Crystal），從命運之火賭場（Fortune's Fire Casino）的賭徒那裡獲取模式矩陣（Pattern Matrix），以及在扭曲叢林（Twisted Weald）深處獲取能量核心（Energy Core）。",
    "1156758": "蟻族（Myrmidex）萬歲！巴拉布（Barrab）萬歲！嗚哇哈哈哈！有了這些零件，靜滯密室（Stasis Chamber）就能重新啟動，齊帕克特里奧特爾（Zipactriotl）就能被消滅！",
    "1156764": "真遺憾事情發展到了這一步。我的希望是從伊歐頓（Eodon）所有民族和生物那裡獲得的知識能夠被用來和平解決這個問題……但蟻族（Myrmidex）不給我們這個選擇。",
    "1156768": "你必須從新馬金西亞（New Magincia）的端莊少女酒館（Modest Damsel）的賭徒那裡獲取殺蟲劑（Insecticide），從 Skara Brae 的園丁那裡獲取費洛蒙（Pheromone），從不列顛尼亞城南的馬廄管理員那裡獲取吸引劑（Attractant），從紫杉城（Yew）的 16 號磨坊（Mill #16）獲取毒藥（Poison）。",
    "1156769": "我應該感到興奮，但我無法不對整個種群的毀滅感到痛苦*她微微皺眉* 我必須提醒自己這不是一個容易的決定，但蟻后（Queen）必須被摧毀。",
    "1156771": "一切就緒。在蟻族巢穴（Myrmidex Pits）深處是蟻后的房間，那裡是她孵化新的蟻族（Myrmidex）的地方。",
    "1156776": "蟻族（Myrmidex）種群中相當大的一部分已在它們棲息地的地表區域被消滅。種群數量已降至最低點。現在是時候……深入巢穴消滅蟻后（Queen）了。",
    "1156778": "你必須宣誓效忠蟻族（Myrmidex），並在蟻族巢穴（Myrmidex Pits）中擊敗伊歐頓人（Eodonians），然後才能開始此任務。",
    "1156779": "你必須宣誓效忠伊歐頓人（Eodonians），並在蟻族巢穴（Myrmidex Pits）中擊敗蟻族（Myrmidex），然後才能開始此任務。",
    "1156781": "我無法相信這些可惡的害蟲！如果這些流氓不把我的萵苣當作它們的個人沙拉吧啃食，我連一顆萵苣都種不出來！",
    "1156786": "你！保護好那台弩炮！還有你，集合！一——二！一——二！加快速度，新兵們！是時候消滅這些蟲子了！",
    "1156789": "哎呀，真沒想到，你做到了！既然我們已經消滅了足夠多的這些害蟲，蟻后（Queen）別無選擇，只能親自出現了！",

    "1156791": "*抽泣* 為——為什麼——我當時就是停不下來，不，就再來一把——我知道只要我能連贏幾把，我就能贏回一切……",

    "1156801": "你好，想買還是想賣？也許給你愛人一條珍珠項鍊——或者一枚好戒指來搭配你的精美服飾？如果是銀製品，你可找對人了！",
    "1156808": "呃！海洋已變成一個危險的鬼地方——成群的生物在折磨我的船隻。我可不能讓這種事發生在我的海域！",
    "1156813": "你需要為像我這樣的船長確保海洋安全！確保你在南不列顛尼亞海（South Britannian Sea）殺死那些生物。我會補償你的時間和努力的！",
    "1156816": "哈哈哈……呵呵……呼呼……是的……一切都在按計劃進行，只要……*工匠停下來抬頭看* 蟻族（Myrmidex）說他們可以幫我收集本應屬於科特爾人（Kotl）的零件……跟我來，我給你看！",
    "1156819": "哈哈哈！呵呵！呼呼！這些正是我需要的——自動人偶（Automaton）很快就能完成了，不久我就能運用這科特爾科技（Kotl Technology）！",

    "1156907": "獲得五個隨機歌唱球之一（瘋狂歌唱球（Singing Ball of Bedlam）、醉漢歌唱球（Drunk Man's Singing Ball）、醉女歌唱球（Drunk Woman's Singing Ball）、索沙尼亞戰馬歌唱球（Singing Ball of Sosarian Steed）、正義歌唱球（Singing Ball of Virtue））。",

    "1157254": "<BASEFONT COLOR=#0099cc>~1_VAL~<BASEFONT COLOR=#FFFFFF>",
}

# Apply translations
translated_count = 0
for key, zh in translations.items():
    if key in output:
        output[key] = zh
        translated_count += 1

# Verify
still_english = []
for k in source:
    v = output.get(k, '')
    if v and not has_chinese(v):
        still_english.append((k, source[k].strip()))

with open(output_path, 'w', encoding='utf-8') as f:
    json.dump(output, f, ensure_ascii=False, indent=2)

print(f"\n=== Translation Results ===")
print(f"Newly translated this pass: {translated_count}")
total_chinese = sum(1 for k, v in output.items() if has_chinese(v))
print(f"Total entries with Chinese: {total_chinese}/{len(source)}")
print(f"Total entries still English: {len(still_english)}")

if still_english:
    print(f"\n=== Remaining English entries ({len(still_english)}) ===")
    for k, v in still_english:
        display = v[:120].replace('\n', '\\n').replace('\r', '')
        print(f"  {k}: {display}")
        print()
else:
    print("ALL ENTRIES TRANSLATED!")

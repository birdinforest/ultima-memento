#!/usr/bin/env python3
"""Add remaining short translations to the response JSON for batch 04."""

import json
import re

RESPONSE_PATH = "/Users/forrrest/projects/UO-Memento/ultima-memento/World/Data/Localization/tools-output/cliloc-batches/responses/response-04-items-3.json"

ADDITIONS = {
    # Guild blazons
    "others<br>Subguilds: Barters, Provisioners, Traders, Merchants": "其他<br>分會：交易商、供應商、貿易商、商人",
    "field.<br><br>[Society of Shipwrights]<br>White diagonal above blue.": "底色。<br><br>[造船師協會]<br>藍色上方白色斜紋。",
    "[Trader's Guild]<br>White bar centered down green field.": "[貿易商公會]<br>綠色底色中央白色豎條。",
    # Combat feedback
    "The chaotic nether bolt hits you particularly hard.": "混亂冥界箭矢特別猛烈地擊中了你。",
    # UI actions
    "Toggles the equipping of items for the specified equipment slots.": "切換指定裝備欄位中物品的裝備狀態。",
    "Turns on Peace mode. Enables you to interact with targets.": "開啟和平模式。允許你與目標互動。",
    "Commands all nearby followers to attack the target specified.": "命令所有附近的跟隨者攻擊指定目標。",
    "Commands all nearby followers to come to your current location.": "命令所有附近的跟隨者來到你當前的位置。",
    "Commands all nearby followers to follow the target specified.": "命令所有附近的跟隨者跟隨指定目標。",
    "Commands all nearby followers to guard the target specified.": "命令所有附近的跟隨者守衛指定目標。",
    "Commands all nearby followers to stay at their current location.": "命令所有附近的跟隨者留在當前位置。",
    "Commands all nearby followers to stop what they are currently doing.": "命令所有附近的跟隨者停止當前正在做的事情。",
    "Commands the ship tiller man to move the ship forward-left.": "命令船隻舵手將船隻向左前方移動。",
    "Commands the ship tiller man to move the ship forward-right.": "命令船隻舵手將船隻向右前方移動。",
    "Commands the ship tiller man to move the ship forward.": "命令船隻舵手將船隻向前移動。",
    "Commands the ship tiller man to move the ship backward-left.": "命令船隻舵手將船隻向左後方移動。",
    "Commands the ship tiller man to move the ship backward-right.": "命令船隻舵手將船隻向右後方移動。",
    "Commands the ship tiller man to move the ship backwards.": "命令船隻舵手將船隻向後移動。",
    "Commands the ship tiller man to move the ship left.": "命令船隻舵手將船隻向左移動。",
    "Commands the ship tiller man to move the ship right.": "命令船隻舵手將船隻向右移動。",
    "Commands the ship tiller man to turn the ship left.": "命令船隻舵手將船隻向左轉。",
    "Commands the ship tiller man to turn the ship right.": "命令船隻舵手將船隻向右轉。",
    "Commands the ship tiller man to turn the ship around.": "命令船隻舵手將船隻調頭。",
    "Commands the ship tiller man to drop the ship's anchor.": "命令船隻舵手拋下船錨。",
    "Commands the ship tiller man to raise the ship's anchor.": "命令船隻舵手收起船錨。",
    "Swaps between your current weapon and your previously equipped weapon.": "在當前武器和先前裝備的武器之間切換。",
    "Uses any available bandages in your backpack on yourself.": "使用背包中任何可用的繃帶治療自己。",
    # Quest parts
    "Ranger of the Abyss Part 1: A Light in the Darkness": "深淵遊俠 第一部：黑暗中的光芒",
    "Allows you to automatically navigate around impassible objects.": "允許你自動繞過無法通行的物體。",
    "You must upgrade to The Stygian Abyss expansion to participate in this quest.": "你必須升級到冥深淵擴充包才能參與此任務。",
    # Quest dialog
    "Come in, come in! Uskadesh told me you were coming.": "進來，進來！烏斯卡德什告訴我你要來。",
    "Yes, I know.  Sir Geoffrey told me you would be coming to see me.": "是的，我知道。傑佛瑞爵士告訴我你會來找我。",
    "You carefully bury the virtue crystal in the ground near the virtue node.": "你小心翼翼地將美德水晶埋入美德節點附近的地面。",
    "You carefully bury the order crystal in the ground near the order node.": "你小心翼翼地將秩序水晶埋入秩序節點附近的地面。",
    "You do not have the correct quest required to use this.": "你沒有使用此物品所需的正確任務。",
    "Your backpack is full. You cannot accept the quest.": "你的背包已滿。你無法接受任務。",
    # Mastery
    "You are not on the correct path for using this mastery ability.": "你不處於使用此大師能力的正確路徑上。",
    "You do not have enough mana to continue infusing your song with magic.": "你沒有足夠的魔力繼續為你的歌曲注入魔法。",
    # Loyalty
    "Ophidian loyalty decrease (base): ~1_VAL~ (peril bonus): ~2_val~": "蛇人忠誠度減少（基礎）：~1_VAL~（危險加成）：~2_val~",
    "Bane Chosen loyalty decrease (base): ~1_VAL~ (peril bonus): ~2_VAL~": "災禍選民忠誠度減少（基礎）：~1_VAL~（危險加成）：~2_VAL~",
    "Ophidian loyalty increase (base): ~1_VAL~ (peril bonus): ~2_VAL~": "蛇人忠誠度增加（基礎）：~1_VAL~（危險加成）：~2_VAL~",
    "Bane Chosen loyalty increase (base): ~1_VAL~ (peril bonus): ~2_VAL~": "災禍選民忠誠度增加（基礎）：~1_VAL~（危險加成）：~2_VAL~",
    # Event dialog
    "If you want to be one of the Chosen, you gotta pull your weight!": "如果你想成為選民的一員，你就得盡自己的一份力！",
    "Welcome friend.  Has-s-s you crystalline blackrock to trade with us-s-s?": "歡迎朋友。你-s-s-有晶態黑岩要和我們-s-s-交易嗎？",
    "It wants-s-s to trade with us-s-s?  We have potions-s-s-s to trade.": "它想-s-s-和我們-s-s-交易？我們有藥水-s-s-s-可以交易。",
    "It is-s-s friend to us-s-s-s.  I will trade with friends-s-s.": "它是-s-s-我們的朋-s-s-友。我會和朋友-s-s-交易。",
    "I s-s-shall not trade with you, s-s-strange one.  We do not trus-s-st you.": "我-s-s-不會和你交易，奇-s-s-怪的傢伙。我們不信-s-s-任你。",
    "Why would I trade with you?  Strangers aren't welcome here!": "我為什麼要和你交易？這裡不歡迎陌生人！",
    "You assist the Ophidians in pushing back the Bane Chosen army.": "你協助蛇人擊退了災禍選民軍隊。",
    "You assist the Bane Chosen in pushing back the Ophidian army.": "你協助災禍選民擊退了蛇人軍隊。",
    "You no longer have the required items to complete this quest.": "你不再擁有完成此任務所需的物品。",
    "The Ophidians have surged ahead, and this area is no longer on the front lines.": "蛇人已向前推進，此區域不再屬於前線。",
    "Your session has been canceled because you were the only scheduled participant.": "你的場次已被取消，因為你是唯一預定的參與者。",
    "The Bane Chosen have surged ahead, and this area is no longer on the front lines.": "災禍選民已向前推進，此區域不再屬於前線。",
    # Cannon actions
    "~1_NAME~ begins loading the cannon with a powder charge.": "~1_NAME~ 開始為火砲裝填火藥裝藥。",
    "~1_NAME~ begins loading the cannon with a cannonball.": "~1_NAME~ 開始為火砲裝入砲彈。",
    "~1_NAME~ begins loading the cannon with a grapeshot.": "~1_NAME~ 開始為火砲裝入葡萄彈。",
    "~1_NAME~ begins priming the cannon with a cannon fuse.": "~1_NAME~ 開始為火砲安裝引信。",
    "~1_NAME~ finishes loading the cannon with a cannonball.": "~1_NAME~ 完成了火砲的砲彈裝填。",
    "~1_NAME~ finishes loading the cannon with a grapeshot.": "~1_NAME~ 完成了火砲的葡萄彈裝填。",
    "~1_NAME~ finishes priming the cannon. It is ready to be fired!": "~1_NAME~ 完成了火砲的引信安裝。可以發射了！",
    "~1_NAME~ carefully removes the powder charge from the cannon.": "~1_NAME~ 小心地從火砲中取出火藥裝藥。",
    "~1_NAME~ carefully removes the cannonball from the cannon.": "~1_NAME~ 小心地從火砲中取出砲彈。",
    "~1_NAME~ carefully removes the grapeshot from the cannon.": "~1_NAME~ 小心地從火砲中取出葡萄彈。",
    "~1_NAME~ carefully removes the cannon fuse from the cannon.": "~1_NAME~ 小心地從火砲中取出引信。",
    "~1_NAME~ sets fire to the cannon's fuse. Stand back!": "~1_NAME~ 點燃了火砲的引信。後退！",
    "~1_NAME~ removes the burning fuse from the cannon and discards it.": "~1_NAME~ 從火砲上取下燃燒的引信並丟棄。",
    "Boom goes the dynamite! Next time try it with ammo.": "炸藥爆炸了！下次試試裝上彈藥。",
    "The ship must be docked near a shore or sea market to dismantle this weapon.": "船必須停靠在岸邊或海上市場附近才能拆除此武器。",
    # Duel
    "You have been invited to a duel.  Select the \u201cOK\u201d button to join this duel.": "你已被邀請參加決鬥。選擇「確定」按鈕加入此決鬥。",
    # Quest
    "You must defeat the guardians of the chest before you can open it.": "你必須先擊敗寶箱的守護者才能打開它。",
    "Your new player starter kit token has been placed in your backpack.": "你的新手入門套裝代幣已放入你的背包。",
    "You do not have enough loyalty with ~1_val~ to trade with this merchant.": "你與 ~1_val~ 的忠誠度不足以與此商人交易。",
    "You cannot eat this until it has been prepared by a cook.": "在廚師準備好之前，你不能食用此物品。",
    # Fishing quest
    "As you wish.  If'n ye change yer mind you know where to find me.": "如你所願。如果你改變主意，你知道在哪裡可以找到我。",
    "Bring yer ship around, I might have some work for ye!": "把你的船開過來，我可能有些活兒給你幹！",
    # Ship repair
    "Your ship is not in need of emergency repairs in order to sail.": "你的船不需要緊急修復即可航行。",
    "Your ship is in pristine condition and does not need repairs.": "你的船狀態完好，不需要修理。",
    "You need a minimum of ~1_METAL~ iron ingots to repair this cannon.": "你至少需要 ~1_METAL~ 個鐵錠才能修理此火砲。",
    "The cannon is in pristine condition and does not need repairs.": "火砲狀態完好，不需要修理。",
    "The cannon is lightly damaged and needs some minor repair.": "火砲輕微損壞，需要一些小型修理。",
}

with open(RESPONSE_PATH, 'r', encoding='utf-8') as f:
    data = json.load(f)

added = 0
for key, value in list(data.items()):
    for eng, chn in ADDITIONS.items():
        if value == eng:
            data[key] = chn
            added += 1
            break

with open(RESPONSE_PATH, 'w', encoding='utf-8') as f:
    json.dump(data, f, ensure_ascii=False, indent=1)

translated = sum(1 for v in data.values() if re.search(r'[\u4e00-\u9fff]', v))
print(f"Added {added} more translations")
print(f"Total: {len(data)} entries, {translated} translated, {len(data) - translated} untranslated")

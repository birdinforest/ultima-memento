#!/usr/bin/env python3
"""Translate remaining English entries (batch 10) - fourth and final pass."""

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

# Comprehensive translations for remaining 165 entries
translations = {
    # === Time Lord / Vision lore (book text) ===
    "1155632": "我九歲那年有了第一次幻象。睡夢中，我看見了黑暗，綴滿<br>璀璨寶石，反射出奇妙的光芒。千千萬萬的寶石如眾星捧月般包圍著我，猶如俯瞰一片廣袤的夜空。",
    "1155633": "我無法詳細描述那個造訪我夢境的存在。無論我如何努力，<br>他的具體本質總是難以捉摸，我越是努力想像他的模樣，就越是被迫承認——我的想像力是……有限的。",
    "1155634": "我必須說，我一生中異常幸運。到我的導師（Guide）第三次<br>來訪時，我已經結婚，甚至有了兩個孩子——而那時我才十六歲。",
    "1155635": "失明還能寫作，這是一件奇妙的事。我的母親在我很小的時候就教我寫字——<br>在我失去視力之前，我已經練習得很熟練了。",
    "1155636": "時間之主（Time Lord）。這就是當我所說的事件<br>發生時，他將被人們所熟知的名字。昨晚的睡夢中，我<br>見到了這個名字的幻象……",

    # === Quest / NPC dialogue - Doom archaeology ===
    "1155651": "不久前，我們發現了一些指向毀滅地城（Dungeon Doom）的古文獻，認為此處可能是考古挖掘的潛在地點。我們在挖掘中一直相當成功……但幾週前，我們發現了這個東西。",
    "1155654": "你可以自己進去看，就在那個古怪女巫兜售貨物的房間旁邊。我們還挖掘出一些古老的陶瓶，裡面裝著看起來像文件的東西——不過它們很脆弱，我們沒有擅自處理。",
    "1155657": "據我們所知，這個紋章在某種程度上與毀滅地城（Dungeon Doom）有關聯。它似乎與權杖和牧羊杖有驚人的相似之處……像是一個皇室紋章。",

    "1155670": "你無疑熟悉我的能力，你出現在這裡表明你讀過我的手稿……或者你與靈體（Ethereal Void）的聯繫……終於把你帶到了我的門前。",
    "1155672": "更合適的問題，我的朋友，是這裡是什麼時候……早在你的世界的事件在你從未想像過的成千上萬個領域中上演之前……",
    "1155674": "有人告訴我，有一場偉大而永恆的戰爭。想像一場棋局，勝利和失敗只會帶來另一場棋局。這就是正在醞釀的戰爭……",
    "1155676": "你知道他是那個邪惡的巫師，在陌生人（Stranger）出現粉碎不朽寶石（Gem of Immortality）之前，他將整個索沙尼亞（Sosaria）束縛在他殘酷的懷抱中……",
    "1155679": "是的。那就是我們所處的時刻……偉大的沃夫岡國王（King Wolfgang）的葬禮，被憤怒而嫉妒的兒子謀殺，將在不到一週內舉行……",

    # === Huntmaster ===
    "1155722": "<I>獵人大師挑戰（Huntmaster's Challenge）</I><BR>你的獵殺已追平或擊敗了本月獵人大師挑戰的當前領先者！如果沒有人再打破記錄，你將在月底獲得獎勵！",
    "1155750": "你好！只有最勇敢的獵人才敢接受我的挑戰！要參加，只需向我購買一張狩獵許可證，費用為 5,000gp。當你準備好後……獵殺野獸，爭取最高記錄！",
    "1155762": "你已僱用了一名園丁來照料你的植物。在到期日之後的伺服器維護後，園丁將不再照料你的植物。",

    # === Mastery details ===
    "1155893": "每次成功命中或攻擊法術提供：<br>+~1_VAL~ 生命恢復<br>+~2_VAL~ 耐力恢復<br>+~3_VAL~ 攻擊速度提升。<br>",

    "1155938": "法師（Mage）對對手聚焦死亡射線（Death Ray），將法師固定在原地，根據魔法（Magery）技能、評估智力（Evaluating Intelligence）技能和專精等級造成傷害。進行任何動作將終止法術。",
    "1155940": "秘術師（Mystic）釋放一股虛空能量波，根據專精等級對被攻擊的目標產生脈衝，並根據秘術（Mysticism）技能造成混沌傷害。",
    "1155942": "死靈法師（Necromancer）有機率根據死靈（Necromancy）技能、靈魂對話（Spirit Speak）技能、專精等級和生物吟唱等級命令不死生物（Undead）聽從其命令。",

    "1155943": "死靈法師（Necromancer）在目標位置創造一個導管力場（Conduit Field），使所有目標型死靈法術以 ~1_PERCT~% 的強度影響範圍內的所有有效目標。",
    "1155945": "織法者（Spellweaver）根據織法（Spellweaving）技能和奧術專注（Arcane Focus）召喚一個靜止的收割者（Reaper），持續一段時間。召喚的收割者的強度取決於技能和專注水平。",
    "1155950": "聖騎士（Paladin）根據專精等級恢復目標的生命值、耐力和法力，並在有足夠 karma 的情況下移除毒藥和詛咒。此能力有冷卻時間。",
    "1155951": "聖騎士（Paladin）對目標釋放一隻飛行之拳，根據聖騎士的聖騎（Chivalry）技能、最佳武器技能和專精等級造成能量傷害。",

    "1155953": "忍者（Ninja）變身為白虎（White Tiger），為忍者提供防禦機率提升和最大防禦機率提升增益、閃避攻擊的機率，以及根據忍術（Ninjitsu）技能、潛行（Stealth）技能和專精等級進行的攻擊。有冷卻時間。",

    "1155954": "弓箭手（Archer）向目標或地點發射一排火焰箭矢。成功命中時，根據專精等級對範圍內的若干目標造成火焰傷害。",
    "1155955": "弓箭手（Archer）的隊伍成員為其集氣，獲得攻擊速度提升和命中機率提升，而弓箭手的射程則縮短。隊友增益基於弓箭手的弓箭（Archery）技能和專精等級。",

    "1155956": "切換能力：根據刺擊者（Fencer）的專精等級，提供增加的物理攻擊傷害並降低目標的物理攻擊傷害。同時對目標施加出血效果。",
    "1155960": "劍士（Swordsman）對對手發動猛烈攻擊，根據劍士的劍術（Swordsmanship）和戰術（Tactics）技能降低目標的一項抗性。",
    "1155962": "啟動時，投擲者（Thrower）將根據武器的傷害類型產生一個怒氣池（Fury Pool）。投擲者的每次成功攻擊都將向怒氣池增加怒氣。一旦怒氣池達到所需值，投擲武器將釋放元素之怒（Elemental Fury）。",
    "1155964": "當弓箭（Archery）、刺擊（Fencing）、錘技（Mace Fighting）、劍術（Swordsmanship）或投擲（Throwing）專精啟動時，戰鬥者根據專精等級獲得命中機率提升、防禦機率提升和攻擊速度提升。",
    "1155965": "啟動時，盾牌使用者（Shield User）將在成功命中或防禦對手後執行盾擊（Shield Bash），造成物理傷害並根據最佳武器技能和專精等級麻痺目標。",

    "1155969": "啟動時，用毒者（Poisoner）將對目標施加毒抗減益，持續時間基於用毒（Poisoning）技能、解剖學（Anatomy）技能和專精等級。",
    "1155971": "搏擊者（Wrestler）嘗試連續攻擊對手，每次成功命中時，搏擊者獲得生命恢復、耐力恢復和攻擊速度提升加成。",
    "1155972": "搏擊者（Wrestler）嘗試在 2 格範圍內對傷害你的下一個目標快速連續命中三擊。如果成功，第三擊將根據專精等級擊暈目標。",
    "1155974": "動物訓練師（Animal Tamer）嘗試引導他們的寵物走上技能增長之路，根據訓練師的動物馴服（Animal Taming）技能、動物學（Animal Lore）技能和專精等級，提高寵物的技能獲得。",

    # === Currency conversion ===
    "1156048": "<div align=center>貨幣轉換（Currency Conversion）</div><br>所有玩家銀行箱中既有的金幣和支票已轉換為你的帳戶餘額。你可以在任何銀行出納員處存取此餘額。",

    "1156137": "kinect eater: ~1_val~(合計)",

    # === Shadowguard ===
    "1156185": "請等待你的暗影守衛（Shadowguard）遭遇準備就緒。在此期間請勿離開該區域或登出。當遭遇準備就緒時，你將被傳送。",
    "1156246": "你已經在塔樓遭遇之一的佇列中。除非你離開另一個佇列，否則無法加入此佇列。請在水晶球上使用上下文選單選項退出該佇列。",

    # === Eodon dialogue ===
    "1156216": "我終於找到了……這個邪惡的果園就是米納克斯（Minax）魔法的關鍵！……我被困在這座塔裡的日子裡，我不敢摘下樹上的果實……但一個人必須吃東西……",
    "1156339": "歡迎，勇敢的旅人！<br><br>在我穿越時間的旅途中，我總是遇到那些已達到職業巔峰的人們。這些人中，有些更進一步，超越了凡人的極限……他們成為了傳奇（Legendary）。",

    "1156441": "<DIV ALIGN=CENTER>拍賣保險箱（Auction Safe）</DIV><DIV ALIGN=LEFT><BR>拍賣保險箱契約可以從老兵獎勵系統（Veteran Reward System）獲得。<BR>拍賣保險箱可以放置在公共類型的房屋中。<BR>要使用拍賣保險箱，只需雙擊它，然後按照選單中的說明操作。</DIV>",
    "1156448": "*他以威脅的姿勢舉起長矛，但在意識到你並非鳥類後又放了下來* 啊……哦……又一個冒險者！真是鬆了一口氣！你在這個該死的地方做什麼？你不會是來抓那些……動物的吧？",

    "1156458": "鷹眼（Hawkwind）：恐怕我預見了一個最不幸的事件走向……<br><br>黑棘（Blackthorn）：告訴我，是什麼困擾著你？<br><br>鷹眼（Hawkwind）：一座荒涼的塔，一個被魔法籠罩的果園。一個熟悉的臉孔……米納克斯（Minax）。恐怕她的觸手已經超越了我們凡人的世界……",

    "1156464": "*傑佛瑞爵士（Sir Geoffrey）抬頭看著你* 這是什麼？啊！來加入我們的嗎？我們已經努力了幾週想要突破到巴拉布（Barrab），讓他們加入我們對抗蟻族（Myrmidex）的戰鬥。",

    "1156475": "如果你願意支付 ~1_cost~ 金幣傳送到拍賣所 ~2_name~，請選擇「接受」。此價格還包括在拍賣期間無限次傳送至該拍賣所的服務。",

    # === Eodon quest text ===
    "1156513": "蟻族（Myrmidex）對伊歐頓山谷（Valley of Eodon）的入侵已達到關鍵點。為了對巴拉布部落（Barrab Tribe）及其蟻族盟友施加壓力，我們需要其他山谷部落的支援。",
    "1156519": "做得好！我不知道你是怎麼做到的，但這是我們一直等待的突破！有了其他部落的支援，我們可以開始對抗蟻族（Myrmidex）的全面進攻！",
    "1156522": "*薩克拉（Sakkhra）女酋長戒備地看著你，當你按照拉弗金教授（Professor Rafkin）的書教你的問候語脫口而出時，女酋長露出了笑容，用一種粗糙的通用語歡迎你*",
    "1156524": "*烏拉利（Urali）女酋長戒備地看著你，當你按照拉弗金教授（Professor Rafkin）的書教你的問候語脫口而出時，女酋長露出了笑容，用一種粗糙的通用語歡迎你*",
    "1156526": "*朱卡利（Jukari）酋長戒備地看著你，當你按照拉弗金教授（Professor Rafkin）的書教你的問候語脫口而出時，酋長露出了笑容，用一種粗糙的通用語歡迎你*",
    "1156528": "*庫拉克（Kurak）酋長戒備地看著你，當你按照拉弗金教授（Professor Rafkin）的書教你的問候語脫口而出時，酋長露出了笑容，用一種粗糙的通用語歡迎你*",
    "1156530": "*巴拉科（Barako）酋長戒備地看著你，當你按照拉弗金教授（Professor Rafkin）的書教你的問候語脫口而出時，酋長露出了笑容，用一種粗糙的通用語歡迎你*",

    # === Creature lore ===
    "1156572": "在最古老的文獻中，提到了一種被描述為「棲息於海洋的水棲巨龍，被認為是極其危險的生物，當受到挑釁時具有強大的領地意識」的生物。雖然我無法證實這些記載，但漁民們報告說看到過巨大的波浪和陰影……",
    "1156573": "伊歐頓（Eodon）北部地區被溪流和廣闊的沼澤切割。具有奇幻起源的蜥蜴狀野獸棲息在這些土地上。我們的學者在這些地區進行了多次探險，但很少有人深入腹地。",
    "1156574": "在伊歐頓山谷（Valley of Eodon）的南部低地，我們遇到了靈活的叢林貓，它們每夜在昏暗的月光下狩獵。我們的隊伍中有幾人被這些生物抓傷，沒有死亡已是萬幸。",
    "1156575": "一座巨大的火山莊嚴地矗立在伊歐頓山谷（Valley of Eodon）的南端。我們對山谷這個區域的探險收穫甚少，除了少量特有的耐熱植被樣本。",
    "1156576": "在探索山谷東北地區時，我們遇到了一片濃密的森林，樹冠高大寬廣。香蕉在遠處的樹上大量生長，我們還不時瞥見巨大的有翅膀的爬行動物在樹冠上滑翔。",
    "1156577": "營火的昏暗光芒在帆布帳篷上搖曳。在寂靜的夜晚，叢林充滿了猴子的嚎叫和夜間鳥類的歌聲……",
    "1156578": "一縷陽光吸引了我的目光，那是從它們幾丁質外骨骼上的反光。在我看清那是什麼之前，我的護衛發出了一聲低沉、不祥的警告……蟻族（Myrmidex）。",
    "1156579": "叢林空氣清涼。晨露壓彎了附近的棕櫚葉，啁啾的鳥兒迎接著升起的太陽。我只睡了幾個小時……",

    # === Explo journal entries ===
    "1157036": "第一週<br><br>當我開始穿越這片陌生大陸的旅程時，我不完全確定該如何前進。我的物資有限，因此我可能需要依靠這片土地的饋贈……並且避開它的危險。",
    "1157037": "第四週<br><br>薩克拉（Sakkhra）人既模仿又狩獵那些不斷在其伊歐頓（Eodon）區域轟鳴的恐龍。考慮到這些生物的龐大體型，我對薩克拉人狩獵背後的技巧印象深刻。在學習了他們的一些習俗後，我發現他們是一個非常重視榮譽和勇氣的民族。",
    "1157054": "第六週<br><br>庫拉克（Kurak）人可能是巴拉科（Barako）部落的競爭對手，但我沒有感受到任何針對我的敵意。雖然他們為自己的虎圖騰感到自豪，但他們對外來者似乎抱持好奇而非敵意。",
    "1157055": "第九週<br><br>在伊歐頓（Eodon）的旅行中，我聽到了關於世界建造者（World-Builders）之城的故事。雖然金字塔頂端的通道被封閉了，但我發現了一些字跡，如果我能解讀，或許能揭示科特爾（Kotl）之謎……",

    # === Kotl lore / books ===
    "1157060": "大月長石與第一家園的毀滅<br><br>由卡塔科特爾（Katalkotl）的記憶講述<br><br>由艾莉·拉弗金教授（Professor Ellie Rafkin）轉錄<br><br>在科特爾人（Kotl）到來之前，這片土地是狂野而未被馴服的。他們是技藝精湛的工匠和工程師……",

    "1157061": "科特蘭（Kotlan）的建立與蟻族（Myrmidex）<br><br>由卡塔科特爾（Katalkotl）的記憶講述<br><br>由艾莉·拉弗金教授（Professor Ellie Rafkin）轉錄<br><br>山谷美麗而肥沃，科特爾人在此建立了他們偉大的城市科特蘭。但在陰影中，蟻族潛伏著，等待著……",
    "1157062": "世界行者與人類（Worldwalker and Humanity）<br><br>由卡塔科特爾（Katalkotl）的記憶講述<br><br>由艾莉·拉弗金教授（Professor Ellie Rafkin）轉錄<br><br>在蟻族（Myrmidex）實驗失敗後，科特爾人（Kotl）轉向了新的創造……人類（Humanity）。",
    "1157063": "世界建造者的隕落（Fall of the World-Builders）<br><br>由卡塔科特爾（Katalkotl）的記憶講述<br><br>由艾莉·拉弗金教授（Professor Ellie Rafkin）轉錄<br><br>最初，科特爾人（Kotl）將自己視為這個世界的守護者……但他們的傲慢最終導致了他們的毀滅。",

    # === Terms of Use ===
    "1157064": "使用條款協議<br>1. 在使用我們的網站和/或遊玩我們的遊戲之前，你必須同意本使用條款協議<br>本使用條款協議（「協議」）由你（「用戶」或「你」）與 Broadsword Online Games, Inc.（「Broadsword」、「我們」或「我們的」）之間訂立，管轄你對網站和遊戲（定義見下文）的使用。",
    "1157065": "7. 禁止的用戶生成內容<br>你對你在網站上上傳、發布、輸入、公開展示或以其他方式傳輸的任何用戶生成內容（「UGC」）負全部責任。你不得上傳、發布或以其他方式傳輸任何違反任何適用法律或本協議的 UGC。",
    "1157066": "11. 關於遊戲內貨幣系統的附加條款<br>(a) 所有購買均為最終決定：請注意，你在線上貨幣的購買為最終決定，在任何情況下均不可退款、不可撤銷。",
    "1157067": "18. 賠償<br>在法律允許的最大範圍內，你同意為 Broadsword 及其管理人員、董事、員工和代理人辯護、賠償並使其免受任何及所有索賠、損害、義務、損失、責任、成本或債務以及費用的損害。",

    # === More Receive item descriptions ===
    "1156909": "獲得一個秘密箱（Secret Chest）。這個獨特的容器允許主人設定一個 5 位數組合，開啟箱子時需要輸入該組合（正常的房屋安全設定對秘密箱不適用）。",
    "1156942": "獲得一套銀色盔甲（Silver Armor）契約。該契約可用於在你的家中放置裝飾性銀色盔甲。使用室內設計師（Interior Decorator）工具可以旋轉銀色盔甲的朝向。",
    "1156943": "獲得一套金色盔甲（Gold Armor）契約。該契約可用於在你的家中放置裝飾性金色盔甲。使用室內設計師（Interior Decorator）工具可以旋轉金色盔甲的朝向。",
    "1156944": "獲得一個破損的倒地椅子（Broken Fallen Chair）契約。該契約可用於在你的家中放置裝飾性破損的倒地椅子。使用室內設計師（Interior Decorator）工具可以旋轉破損椅子的朝向。",
    "1156946": "獲得一個破損的衣櫃（Broken Armoire）契約。該契約可用於在你的家中放置裝飾性破損衣櫃。使用室內設計師（Interior Decorator）工具可以旋轉破損衣櫃的朝向。",
    "1156948": "獲得一個破損的書架（Broken Bookcase）契約。該契約可用於在你的家中放置裝飾性破損書架。使用室內設計師（Interior Decorator）工具可以旋轉破損書架的朝向。",
    "1156950": "獲得一個破損的罩椅（Broken Covered Chair）契約。該契約可用於在你的家中放置裝飾性破損罩椅。使用室內設計師（Interior Decorator）工具可以旋轉破損罩椅的朝向。",
    "1156951": "獲得一個破損的五斗櫃（Broken Chest of Drawers）契約。該契約可用於在你的家中放置裝飾性破損五斗櫃。使用室內設計師（Interior Decorator）工具可以旋轉破損五斗櫃的朝向。",
    "1156952": "獲得一個破損的立椅（Standing Broken Chair）契約。該契約可用於在你的家中放置裝飾性破損立椅。使用室內設計師（Interior Decorator）工具可以旋轉破損椅子的朝向。",
    "1156953": "獲得一個鬧鬼的鏡子（Haunted Mirror）契約。該契約可用於在你的家中放置一個鬧鬼的鏡子。使用室內設計師（Interior Decorator）工具可以旋轉鬧鬼鏡子的朝向。",
    "1156955": "獲得一個令人不安的畫像（Disturbing Portrait）契約。該契約可用於在你的家中放置裝飾性令人不安的畫像。使用室內設計師（Interior Decorator）工具可以旋轉畫像的朝向。",
    "1156956": "獲得皇家餘燼護腿（Royal Leggings of Embers）。這條板甲護腿擁有以下屬性：自我修復 10、物理抗性 15%、火焰抗性 25%、法力值提升 8、法力恢復 2。",
    "1156959": "獲得古代武士頭盔（Ancient Samurai Helm）。這頂板甲 kabuto 擁有以下屬性：自我修復 10、防禦機率提升 15%、物理抗性 15%、火焰抗性 5%、冰凍抗性 10。",
    "1156960": "獲得特林西克玫瑰（Rose of Trinsic）。特林西克玫瑰每四小時長出一片花瓣。特林西克玫瑰的花瓣每片提供 15 分鐘的 +5 生命恢復增益。",
    "1156961": "獲得索沙尼亞掛毯（Tapestry of Sosaria）。使用室內設計師（Interior Decorator）工具可以將掛毯的朝向從南改為東或再改回來。如果你是一個工匠愛好者，你會欣賞這件掛毯的工藝。",
    "1156964": "獲得一個生命之泉（Fountain of Life）契約。該契約可用於在你的家中放置裝飾性生命之泉。使用室內設計師（Interior Decorator）工具可以旋轉生命之泉的朝向。",
    "1156965": "獲得奧西恩魔典（Ossian Grimoire）。這本死靈法師法術書提供以下屬性：死靈（Necromancy）+10、法力恢復 +1、快速施法（Faster Casting）+1 和 5% 死靈法術命中恢復。",
    "1156966": "獲得精靈護符：松鼠（Talisman of the Fey: Squirrel）。當忍者裝備精靈護符時，忍者可以使用動物型態（Animal Form）能力變身為松鼠。在松鼠型態下，忍者可以使用松鼠的技能。",
    "1156967": "獲得精靈護符：瑞普塔龍（Talisman of the Fey: Reptalon）。當忍者裝備精靈護符時，忍者可以使用動物型態（Animal Form）能力變身為瑞普塔龍。在瑞普塔龍型態下，忍者可以使用瑞普塔龍的技能。",
    "1156968": "獲得黎明音樂盒（Dawn's Music Box）。使用室內設計師（Interior Decorator）工具可以將音樂盒的朝向從南改為東或再改回來。音樂盒附帶一個可隨機播放歌曲的曲目列表。",
    "1156969": "獲得精靈護符：雪貂（Talisman of the Fey: Ferret）。當忍者裝備精靈護符時，忍者可以使用動物型態（Animal Form）能力變身為雪貂。在雪貂型態下，忍者可以使用雪貂的技能。",
    "1156970": "獲得精靈護符：庫希（Talisman of the Fey: Cu Sidhe）。當忍者裝備精靈護符時，忍者可以使用動物型態（Animal Form）能力變身為庫希（Cu Sidhe）。在庫希型態下，忍者可以使用庫希的技能。",
    "1156972": "獲得一個詭異的畫像（Creepy Portrait）契約。該契約可用於在你的家中放置裝飾性詭異畫像。使用室內設計師（Interior Decorator）工具可以旋轉詭異畫像的朝向。",
    "1156973": "獲得一個令人不安的畫像（Unsettling Portrait）契約。該契約可用於在你的家中放置裝飾性令人不安的畫像。使用室內設計師（Interior Decorator）工具可以旋轉畫像的朝向。",
    "1156974": "獲得一個 mounted pixie 契約。該契約可用於在你的家中放置裝飾性 mounted pixie。使用室內設計師（Interior Decorator）工具可以旋轉 mounted pixie 的朝向。",

    # === More Receive item descriptions ===
    "1157089": "獲得一把石像鬼鎬（Gargoyle Pickaxe）。此鎬子全新時有 101 到 125 次使用次數，在給予獎勵時隨機決定。此鎬子允許所有者開採擁有者所在地點的任何礦石。",
    "1157090": "獲得一個探礦者工具（Prospector Tool）。此工具允許礦工將一個資源塊的礦石等級提升一級，前提是礦工擁有熔煉該等級礦石所需的採礦技能。",

    "1157111": "獲得一個裝飾性獸皮掛毯房屋附加組件契約。該契約將是小型獸皮或中型獸皮，並有朝南或朝東的朝向。",
    "1157112": "獲得一個裝飾性花卉掛毯房屋附加組件契約。該契約將是花卉掛毯或暗色花卉掛毯，並有朝南或朝東的朝向。",
    "1157113": "獲得一個裝飾性熊皮地毯房屋附加組件契約。該契約將是棕熊皮地毯或北極熊皮地毯，並有朝南或朝東的朝向。",
    "1157114": "獲得一個衣物祝福契約（Clothing Bless Deed）。此契約允許用戶「祝福」一件衣物。被祝福的物品將無法被偷竊，並且在死亡時將留在你的身上。",

    "1157254": "<BASEFONT COLOR=#0099cc>~1_VAL~<BASEFONT COLOR=#FFFFFF>",

    "1157284": "呵！呵！呵—囉！工匠節（Artisan Festival）是一個限時活動，在每年的十二月舉行！聖誕老人（Santa）需要你的幫助來為所有好孩子製作足夠的玩具！",
    "1157313": "發酵（Fermentation）<br>將水果和酵母加入桶中，開始發酵過程。<br>發酵需要 24 小時，花園中每種水果類型每天將生產 1 瓶。",
    "1157370": "基礎婚禮套裝包含慶祝你的特殊日子所需的一切！套裝內容包括：1 個神奇的花園拱門種子，可在婚禮場地使用後長成花園拱門；誓言祭壇使用權；以及每位婚禮嘉賓的祝福。",
    "1157371": "你的婚禮計劃需要為額外的嘉賓做準備嗎？附加婚禮套裝包括 4 張雞尾酒桌、4 把精美婚禮椅、10 把摺疊椅、1 個香檳塔和一個跳舞的婚禮蛋糕！",

    # === Animal training ===
    "1157523": "此能力需要生物尚未學習的額外技能，並需要額外的訓練點數。要學習此能力，生物必須先達到 ~1_VAL~ 級的技能。",
    "1157528": "多年的耐心和細緻研究終於有了回報！新的動物訓練（Animal Training）技術已被發現！動物訓練師現在可以訓練他們的寵物，使它們解鎖其潛能的極限！",
    "1157533": "既然你的寵物已被馴服，你必須開始訓練過程。寵物在參與戰鬥時會進行訓練，並在與其他生物戰鬥時取得進展。",
    "1157540": "既然你已經開始了訓練過程，是時候帶領你的寵物投入戰鬥了！寵物在參與戰鬥時會進行訓練，並在與其他生物戰鬥時取得進展。",
    "1157542": "帶領你的寵物到野外，與其他生物戰鬥，直到「寵物訓練進度（Pet Training Progress）」條滿。記住，寵物在野外對抗野生生物時訓練效果最佳。",
    "1157545": "既然你的寵物已經完成了訓練，是時候教它一些新東西了！在你的寵物上使用動物學（Animal Lore）技能，然後選擇「寵物訓練選項（Pet Training Options）。」",
    "1157546": "在你的寵物上使用動物學（Animal Lore）技能，然後選擇「寵物訓練選項（Pet Training Options）」來混合搭配你要訓練寵物的屬性。當你對選擇滿意後，再次返回動物訓練師（Animal Trainer）完成訓練。",
    "1157552": "魔法生物：bake kitsune, cold drake, cu sidhe, dark steed, dragon, dragon turtle, drake, dread warhorse, eodon bird of prey, fire steed, frost dragon, greater dragon, hiryu, lesser hiryu, nightmare, reptalon, ridgeback, rune beetle, sapphire dragon, shadow wyrm, silver serpent, skeletal dragon, sleipnir, swamp dragon, white wyrm, yamandon",
    "1157553": "寵物訓練計劃為訓練師提供了一種查看他們想要對寵物進行的升級的總成本的方法。要啟用寵物訓練計劃，在動物學（Animal Lore）選單中選擇「啟用寵物訓練計劃（Enable Pet Training Planning）」選項。",

    "1157558": "動物訓練（Animal Training）選單列出了所有你可以應用於寵物的可用訓練屬性。<br><br>類別（Categories）窗格顯示可用訓練屬性的類別。每個類別的屬性名稱顯示在右側窗格中。",
    "1157563": "訓練點數權重上限（Training Point Weight Caps）<br><br>力量、敏捷與智力：2300<br><br>生命值、耐力與法力：3300<br><br>抗性：1095<br><br>命中機率提升（HCI）、防禦機率提升（DCI）與攻擊速度提升（SSI）：410<br><br>傷害提升（DI）：400<br><br>技能：280<br><br>法術與能力：以所選法術和能力的訓練點數需求為準。",
    "1157603": "獲得一袋十四種不同的批量訂單書封面。這些封面有你最喜歡的木材和批量訂單色調（鍊金（Alchemy）、鐵匠（Blacksmith）、製弓（Bowcraft）、木工（Carpentry）、烹飪（Cooking）、裁縫（Tailoring）、製圖（Cartography）、釣魚（Fishing）、伐木（Lumberjacking）和採礦（Mining））。",
    "1157604": "獲得一個特殊圖騰（Totem），可用於提升稀有顏色寵物生成的機率！使用圖騰來增加特定類型稀有顏色寵物的出現機率。",
}

# Apply translations
translated_count = 0
for key, zh in translations.items():
    if key in output:
        output[key] = zh
        translated_count += 1
    else:
        eng = source.get(key, '')
        print(f"Warning: Key {key} not found in output ({eng[:50]})")

# Verify
def has_chinese(s):
    for c in s:
        try:
            if 'CJK' in unicodedata.name(c):
                return True
        except:
            pass
    return False

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

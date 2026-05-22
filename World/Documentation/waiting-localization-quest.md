# 待中文化：Quests/Major 目录扫描

> **扫描日期：** 2026-05-22
> **完成日期：** 2026-05-22
> **处理范围：** **处理所有英文硬编码文本**（`SendMessage`、`Say`、Gump、`TextDefinition.AddHtmlText`、`AddRow`、OPL、`Name =`、字符串赋值、长段拼接模板等）。
> **排除范围：** **不处理 cliloc 控制的文本**（`SendLocalizedMessage`、仅 cliloc 数字的 `*OverheadMessage`）。已通过 `ResolveText` / `StringCatalog` 且键已在 locale 中的行亦不计入。
> **扫描路径：** `World/Source/Scripts/Engines and Systems/Quests/Major`
> **关联：** [`waiting-localization.md`](waiting-localization.md) · `AGENTS.md` §3.2
> Journal Quest：`QuestTake.cs` + `QuestTome.cs`。

---

## 摘要

| 指标 | 数量 | 状态 |
|------|------|------|
| 扫描 `.cs` 文件 | 2 | — |
| **含英文硬编码（待处理）** | **2** | **已全部完成** ✅ |
| 无英文硬编码（或仅 cliloc / 已目录化） | 0 | — |

> **中文化状态：全部完成（2026-05-22）** ✅
>
> 所有 Phase 1–4 的 shotkey 和 C# 修改已实现并编译通过。详见 [§2 建议修复顺序](#2-建议修复顺序) 的完成标记。

**说明：** 同一文件可同时含 cliloc 与硬编码；Major 任务脚本中 **`SendMessage` 已走 `ResolveText`**（如 `You take possession of the book!`），**无** `SendMessage("…")` 字面量待办；其余 Gump/OPL/拼接模板见下表。

### SendMessage 硬编码

| 文件 | `SendMessage("…")` 字面量 | 说明 |
|------|---------------------------|------|
| `QuestTake.cs` | **无** | 玩家提示经 `ResolveText(from, "…")`；键已在 `scripts-quests.json` 的不重复列出 |
| `QuestTome.cs` | **无** | 同上；`This book does not belong…` 已目录化 |

与 Skills 中 `Begging.cs:178` 同类问题（`SendMessage("英文…")` 未走目录）在 **Major 目录当前不存在**；若新增脚本须避免该反模式。

**复查命令：**

```bash
rg 'SendMessage\s*\(\s*"' "World/Source/Scripts/Engines and Systems/Quests/Major"
rg '"[A-Za-z]{3,}' "World/Source/Scripts/Engines and Systems/Quests/Major"
```

## 待处理文件总表

| 文件 | 硬编码条数 | 含 cliloc | StringCatalog | 主要类型 | 状态 |
|------|------------|-----------|---------------|----------|------|
| `QuestTake.cs` | 240 | 是 | 部分 | Item Name, String literal | ✅ 已全部中文化 |
| `QuestTome.cs` | 41 | 是 | 部分 | Gump, Item Name, OPL, Overhead, Return string, String literal | ✅ 已全部中文化 |

## 1. 文本拼接模式分析与中文化方案

### 1.1 英文源文本的七种拼接模式

#### 模式 A：物品名运行时拼接（QuestTake.cs:28, QuestTome.cs:138）

```csharp
// QuestTake.cs 构造函数
Name = "Journal of " + RandomThings.GetRandomName() + " the " + (
    Utility.RandomBool() ? RandomThings.GetBoyGirlJob(0) : RandomThings.GetBoyGirlJob(1)
);

// QuestTome.cs 构造函数
Name = "lost journal";

// MajorItemOnCorpse.cs 构造函数
Name = "chest";
Name = "Chest of " + m.Name;
```

- `Item.Name` 被持久化到世界存档，不能运行时修改
- Gump 标题（`m_Book.Name`）直接使用该值
- OPL 第一行默认由客户端展示 `Item.Name`

**约束条件：**
- 中文词序完全不同："Journal of <Name> the <Job>" → "<称谓><Name>的日志"
- 名称在 `QuestTomeStoryGood`/`Evil` 模板中以 `DDDDD` 占位，然后 `Replace("DDDDD", dead)` 替换（dead 取自 Name）
- 不能直接修改 `Name` 为中文字符串，否则存档兼容性、第三方工具都会受影响

#### 模式 B：词表 + 随机选择嵌入模板（QuestTake.cs:123–163）

```csharp
string heard = "heard";
switch (Utility.RandomMinMax(0, 3)) {
    case 1: heard = "told";    break;
    case 2: heard = "known";   break;
    case 3: heard = "shared";  break;
}
string legend = "legends";
switch (Utility.RandomMinMax(0, 3)) {
    case 1: legend = "fables"; break;
    case 2: legend = "myths";  break;
    case 3: legend = "lore";   break;
}
// ... 类似：hush (whispered/told/sung/spoken)
//         inn (taverns/camps/cities/villages/inns)
//         takes (seized/stolen/taken/held/guarded)
```

五个独立词表，通过 `switch` 随机选择，然后嵌入大型模板：

```csharp
"...that has been " + heard + " in " + legend + " and " + hush + " about in " + inn + "."
= "...that has been heard in legends and whispered about in taverns."
```

#### 模式 C：大型叙事模板（QuestTake.cs:169–177）

```csharp
tome.QuestTomeStoryGood = "You have found the journal of DDDDD, where they were given a quest by "
    + tome.QuestTomeNPCGood + " to find " + tome.GoalItem4 + " that is known to be "
    + takes + " by " + tome.VillainName + " " + tome.VillainTitle + ". "
    + tome.VillainName + " is " + tome.VillainCategory + " that has been "
    + heard + " in " + legend + " and " + hush + " about in " + inn + "."
    + "..."; // 长篇，约 400 词
```

四个版本（Good / Evil × 正常 / 反转剧情），嵌入变量包括：
- `tome.QuestTomeNPCGood/Evil`（NPC 名 + 头衔）
- `tome.GoalItem1–4`（目标物品名）
- `tome.VillainName + " " + tome.VillainTitle`
- `tome.VillainCategory`
- `tome.QuestTomeWorldGood/Evil`（地名）
- `tome.QuestTomeLocateGood/Evil`（坐标）
- 四个随机词（heard/legend/hush/inn/takes）

#### 模式 D：Gump 帮助页（QuestTomeGump:378）

一整段 878px × 548px 的 HTML 教学文本，嵌入 `m_Book.GoalItem4`、`QuestTomeNPCEvil`、`VillainName`、`VillainTitle` 等变量。文本内同一变量出现多次。

#### 模式 E：OPL 工具提示（QuestTome.cs:142–146）

```csharp
public override void AddNameProperties(ObjectPropertyList list)
{
    base.AddNameProperties(list);
    if (QuestTomeOwner != null)
        list.Add(1049644, "Belongs to " + QuestTomeOwner.Name + "");
}
```

#### 模式 F：Emote 头顶文字（QuestTome.cs:581, 593, 608）

```csharp
player.LocalOverheadMessage(MessageType.Emote, 1150, true, "You found " + relic + ".");
player.LocalOverheadMessage(MessageType.Emote, 1150, true,
    book.QuestTomeCitizen + " was either wrong or they lied.");
player.LocalOverheadMessage(MessageType.Emote, 1150, true, "You found " + book.GoalItem4 + ".");
```

`LocalOverheadMessage(MessageType, int, bool, string)` 的第4参数是原始字符串（not cliloc），需要手工本地化。

#### 模式 G：流言拼接（QuestTome.cs:453–486）

```csharp
// GetRumor 中的 locate 词表
string locate = "held by a powerful creature";
if (goal == 2) { locate = "lost somewhere"; }
if (book.QuestTomeGoals == 3) { locate = "found"; goal = 3; }

// TellRumor 中的 who 词表
string who = "I heard";
switch (Utility.RandomMinMax(0, 5)) {
    case 0: who = "I heard";                                              break;
    case 1: who = "I learned";                                            break;
    case 2: who = "I found out";                                          break;
    case 3: who = "The " + RandomThings.GetRandomJob()
               + " in " + RandomThings.GetRandomCity() + " told me";      break;
    case 4: who = "I overheard some " + RandomThings.GetRandomJob()
               + " say";                                                  break;
    case 5: who = "My friend told me";                                    break;
}

// 最终拼接
return who + " that " + item + " may be " + locate + " within "
     + dungeon + " in " + world + ".";
// → "I heard that the Amulet of Might may be held by a powerful creature within Dungeon Doom in Sosaria."
```

**核心问题**：中文和英文的定语从句、介词短语位置完全不同。英文是 **"who + that + item + may be + locate + within + dungeon + in + world"**，中文需要 **"[who]在[world]的[dungeon]中，物品[item][locate]"**。

---

### 1.2 中文化方案 — Shotkey + Format 组合模式

#### 设计原则

1. **弃用英文词表**：模式 B（heard/legend/hush/inn/takes）的英文随机词在中文语境下没有直接对应。中文叙事用**固定但自然的中文句式**，英文的随机性改为中文等效的多样化表达。
2. **整体模板**：模式 C 和 D 的大段叙事用 `StringCatalog.ResolveFormatByKey` 整体模板，中文版本完全重新编排语序。
3. **运行时从持久化字段重建**：`QuestTomeStoryGood`/`QuestTomeStoryEvil` 保持英文原文（存档兼容），在 Gump 渲染时如果账号语言为中文，用其他已持久化的字段通过 shotkey 模板重新生成中文叙事。
4. **OPL 用 `AddLocalizedProperty`**：模式 E 使用 `BuildingPropertyListLocale` 分支 + shotkey。
5. **Emote 用 `ResolveFormatByKey`**：模式 F 用 `StringCatalog.ResolveFormatByKey` 产生本地化头顶文字。

---

### 1.3 Shotkey 键名规划

所有 shotkey 放在 `world-player-text.json`（`keep_extra` 白名单）。前缀 `quest.tome.*`。

#### 1.3.1 OPL / 物品名

| Shotkey | EN 值 | ZH 值 | 用途 |
|---------|-------|-------|------|
| `quest.tome.opl.belongs_to` | `Belongs to {0}` | `属于 {0}` | OPL `AddLocalizedProperty` |
| `quest.tome.name.lost_journal` | `lost journal` | `遗失的日记` | 已有 hash `s.a3734bf593f52976`，shotkey 化后替代 |
| `quest.tome.name.chest` | `chest` | `宝箱` | 已有 hash `s.7ca0d7019158ccd9`，`MajorItemOnCorpse` 默认名 |
| `quest.tome.name.chest_of` | `Chest of {0}` | `{0}的宝箱` | OPL `DropChest` 中的 `majorChest.Name` |
| `quest.tome.name.format` | `Journal of {0} the {1}` | `{1}{0}的日记` | 物品名 OPL 模板，`Name` 本身保持英文 |

#### 1.3.2 Emote 头顶文字

| Shotkey | EN 值 | ZH 值 | 用途 |
|---------|-------|-------|------|
| `quest.tome.emote.found_relic` | `You found {0}.` | `你找到了 {0}。` | `FoundItem` 找到遗物 |
| `quest.tome.emote.wrong_rumor` | `{0} was either wrong or they lied.` | `{0} 要么搞错了，要么在说谎。` | 流言错误（已有 `quest.n0_was_either_wrong_or_they_lied_dot`：`{0} 要么错了，要么在说谎。`） |
| `quest.tome.emote.found_goal` | `You found {0}.` | `你找到了 {0}。` | `FoundItem` 最终目标 |

> **复用**：`quest.n0_was_either_wrong_or_they_lied_dot` 已存在于 `world-player-text.json`（EN: `{0} was either wrong or they lied.`, ZH: `{0} 要么错了，要么在说谎。`），模式 F 的 `book.QuestTomeCitizen + " was either wrong or they lied."` 可以复用此键（`Citizen` 作为 {0}）。

#### 1.3.3 Gump 标题 / 按钮

| Shotkey | EN 值 | ZH 值 | 用途 |
|---------|-------|-------|------|
| `quest.tome.gump.title` | `Quest for {0}` | `{0}的任务` | Gump 首页标题（line 388） |

#### 1.3.4 流言系统

| Shotkey | EN 值 | ZH 值 | 用途 |
|---------|-------|-------|------|
| `quest.tome.rumor.talk` | `{0} has told you that {1} may be {2} within {3} in {4}.` | `{0}告诉你，{2}可能在{4}的{3}中找到了{1}。` | `GetRumor` talk=true 时的居民流言 |
| `quest.tome.rumor.heard_held` | `I heard that {0} may be held by a powerful creature within {1} in {2}.` | `我听说在{2}的{1}中，一种强大的生物看守着{0}。` | `GetRumor` talk=false 时 |
| `quest.tome.rumor.heard_lost` | `I heard that {0} may be lost somewhere within {1} in {2}.` | `我听说{0}可能遗失在{2}的{1}中某处。` | 同上，locate 为 lost somewhere |
| `quest.tome.rumor.heard_found` | `I heard that {0} may be found within {1} in {2}.` | `我听说可以在{2}的{1}中找到{0}。` | 同上，locate 为 found |
| `quest.tome.rumor.who_heard` | `I heard` | `我听说` | `TellRumor` who 词表标准化 |
| `quest.tome.rumor.who_learned` | `I learned` | `我打听到` | 同上 |
| `quest.tome.rumor.who_found_out` | `I found out` | `我发现了` | 同上 |
| `quest.tome.rumor.who_job` | `The {0} in {1} told me` | `{1}的{0}告诉我` | who 含随机职业/城市 |
| `quest.tome.rumor.who_overheard` | `I overheard some {0} say` | `我偶然听到一个{0}说` | who 含随机职业 |
| `quest.tome.rumor.who_friend` | `My friend told me` | `我朋友告诉我` | who 选项 |

> **注意**：`GetRumor` 目前使用 `locate` 的三种值（held by a powerful creature / lost somewhere / found）嵌入统一的句式 `who + " that " + item + " may be " + locate + " within " + dungeon + " in " + world`。用 shotkey 模板后，每种 locate 对应一个独立模板，因为它们的中文句式结构不同。`locate` 词本身不需要独立 shotkey，而是直接消融到模板中。

#### 1.3.5 大型叙事模板（QuestTomeStoryGood / Evil）

> **注意**：这些模板非常长（每个约 400 英文词），包含大量重复变量。建议将每个模板放入 `world-player-text.json`，在 Gump 渲染时通过 `ResolveFormatByKey` 生成中文版本。

**模板参数索引**（Good 版示例）：

| 索引 | 字段 | 示例值 |
|------|------|--------|
| {0} | dead（从 Name 提取的冒险者名） | Bob |
| {1} | QuestTomeNPCGood | Sir Galahad the Brave |
| {2} | GoalItem4 | Amulet of Might |
| {3} | takes（已不使用，中文化固定句式） | guarded |
| {4} | VillainName | Morgath |
| {5} | VillainTitle | the Dark Lord |
| {6} | VillainCategory | a daemon |
| {7} | heard（已不使用，中文化固定句式） | known |
| {8} | legend（已不使用，中文化固定句式） | legends |
| {9} | hush（已不使用，中文化固定句式） | whispered |
| {10} | inn（已不使用，中文化固定句式） | taverns |
| {11} | GoalItem1 | Crystal of Light |
| {12} | GoalItem2 | Scroll of Wisdom |
| {13} | GoalItem3 | Heart of the Phoenix |
| {14} | 对立 NPC | QuestTomeNPCEvil |
| {15} | QuestTomeWorldGood | Sosaria |
| {16} | QuestTomeLocateGood | 10° 20'N, 30° 40'E |

> **关键设计**：中文化后，{3} {7} {8} {9} {10} 五个来自随机词表的参数**在中文模板中被忽略**——中文模板使用固定的文学句式，英文的随机性通过中文等效的多词替换（在模板内部处理）来实现。但这意味着中文模板要**自己内嵌随机逻辑**，或者中文模板统一使用一个固定句式。权衡后建议：**中文使用单一但文雅的自然叙事**，不复制英文的随机句式变体。英文的随机性本质上是文字游戏的 flavor，中文不需要逐词对应。

**好故事模板（Good Story）：**

```
quest.tome.story.good
EN: "You have found the journal of {0}, where they were given a quest by {1} to find {2} that is known to be {3} by {4} {5}. {4} is {6} that has been {7} in {8} and {9} about in {10}. The goal for {0} was to find {11}, {12}, & {13} to help them defeat {4} and then bring {2} back to {1} before {14} can use it for their nefarious plans.<br><br>This is now your quest and you will have to speak with others to find clues on the location of the relics needed, as well as where {4} dwells. Once you defeat {4} and claim {2}, you can give this journal to {1} in {15} at the following coordinates:<br><br>{16}"
ZH: "你找到了冒险者 {0} 的日记。{1} 曾经交给 {0} 一个任务：{4}{5}（{6}）持有 {2}，要去夺回来。{0} 需要先找到 {11}、{12} 和 {13} 这三件遗物才能对抗 {4}，然后把 {2} 带回给 {1}，赶在 {14} 用它实现邪恶图谋之前。<br><br>现在，这个使命落到了你肩上。向镇上的居民打听线索，找到遗物埋藏的地点，以及 {4} 的巢穴。集齐三件遗物后，找到 {4} 所在的位置，打开这本日记将其召唤出来。击败 {4} 拿到 {2} 之后，将日记交给 {15} 的 {1}，坐标如下：<br><br>{16}"
```

**邪恶故事模板（Evil Story）** 和两个 **反转剧情模板** 同理，各自对应的 EN/ZH 翻译对。

> **中文版处理说明**：
> - {3} {7} {8} {9} {10}（takes、heard、legend、hush、inn）在中文本地化时被吸收为固定叙事，不保留逐个英文词的随机性。英文的随机 flavor 在中文中通过不同句式（如「邪恶图谋」「黑暗计划」等）实现——但为简化，建议中文模板使用一个固定但合适的表述，不做运行时随机切换。
> - `DDDDD` 变量替换（行 373: `story = story.Replace("DDDDD", dead)`）对中文模板仍然有效——中文模板用 `{0}` 格式参数接收 `dead`，不需要 `DDDDD` hack。

**zh-Hans 条目示例：**

```json
"quest.tome.story.good": "你找到了冒险者 {0} 的日记。{1} 曾经交给 {0} 一个任务：{4}{5}（{6}）持有 {2}，要去夺回来。{0} 需要先找到 {11}、{12} 和 {13} 这三件遗物才能对抗 {4}，然后把 {2} 带回给 {1}，赶在 {14} 用它实现邪恶图谋之前。<br><br>现在，这个使命落到了你肩上。向镇上的居民打听线索，找到遗物埋藏的地点，以及 {4} 的巢穴。集齐三件遗物后，找到 {4} 所在的位置，打开这本日记将其召唤出来。击败 {4} 拿到 {2} 之后，将日记交给 {15} 的 {1}，坐标如下：<br><br>{16}"
```

#### 1.3.6 Gump 帮助页（大段教学文本，line 378）

帮助页文本约 600 英文词，嵌入 `m_Book.GoalItem4`、`QuestTomeNPCEvil`/`QuestTomeNPCGood`、`VillainName`、`VillainTitle` 等。格式参数规划：

| 索引 | 字段 | 示例 |
|------|------|------|
| {0} | GoalItem4 | Amulet of Might |
| {1} | QuestTomeNPCEvil | Maleficar the Cunning |
| {2} | QuestTomeNPCGood | Sir Galahad the Brave |
| {3} | VillainName | Morgath |
| {4} | VillainTitle | the Dark Lord |

Shotkey 建议：

```
quest.tome.help.guide
EN: "There are many times when adventurers are given a grand quest..."  // 现有全文
ZH: "冒险者们常常会接到一项伟大的任务……"  // 中文版全文，变量位置依中文自然语序重排
```

---

### 1.4 运行时重建逻辑

在 `QuestTomeGump` 构造函数中，渲染故事/帮助页之前：

```csharp
// 获取账号语言
string lang = AccountLang.GetLanguageCode(from.Account);
bool isChinese = AccountLang.IsChinese(lang);

// 故事部分：如果是中文，用 shotkey 模板从字段重建
string story;
if (isChinese)
{
    string dead = m_Book.Name;
    if (dead.Contains("Journal of "))
        dead = dead.Replace("Journal of ", "");
    // 使用事先准备好的参数数组
    // Good story 用这些字段的动态值填空
    story = StringCatalog.ResolveFormatByKey(from.Account,
        karmaLocked ? "quest.tome.story.evil" : "quest.tome.story.good",
        dead,
        karmaLocked ? m_Book.QuestTomeNPCEvil : m_Book.QuestTomeNPCGood,
        m_Book.GoalItem4,
        /* takes - 中文模板忽略 */ "",
        m_Book.VillainName,
        m_Book.VillainTitle,
        m_Book.VillainCategory,
        /* heard - 中文模板忽略 */ "",
        /* legend - 中文模板忽略 */ "",
        /* hush - 中文模板忽略 */ "",
        /* inn - 中文模板忽略 */ "",
        m_Book.GoalItem1,
        m_Book.GoalItem2,
        m_Book.GoalItem3,
        karmaLocked ? m_Book.QuestTomeNPCGood : m_Book.QuestTomeNPCEvil,
        karmaLocked ? Server.Lands.LandName(m_Book.QuestTomeWorldEvil)
                     : Server.Lands.LandName(m_Book.QuestTomeWorldGood),
        karmaLocked ? m_Book.QuestTomeLocateEvil : m_Book.QuestTomeLocateGood
    );
}
else
{
    // 保持原有英文故事
    story = ...; // 从 m_Book.QuestTomeStoryGood/Evil 读取
}
```

> **另外需要**：在 `QuestTomeGump` 渲染时替换 `DDDDD` 的逻辑（line 372-373）应调整——对中文 shotkey 模板来说 `DDDDD` 已被 `{0}` 格式参数替代，不需要 `Replace`。

> **冲突处理**：反转剧情（line 173-177）交换 Good/Evil 的 NPC 角色。在 shotkey 方案下，反转剧情不需要独立模板——只需在构造参数时交换 `{1}` 和 `{14}` 的取值。因此仅需 2 个模板（`quest.tome.story.good` / `quest.tome.story.evil`），通过调整参数顺序实现剧情反转。

---

### 1.5 词表标准化 vs 中文多样性

英文的随机词表为叙事增加 flavor，但中文不宜逐词翻译。方案对比：

| 英文词表 | 英文条数 | 中文化方案 |
|---------|---------|-----------|
| heard / told / known / shared | 4 | 中文不翻译这些词；故事模板使用自然中文叙事 |
| legends / fables / myths / lore | 4 | 同上 |
| whispered / told / sung / spoken | 4 | 同上 |
| taverns / camps / cities / villages / inns | 5 | 同上 |
| seized / stolen / taken / held / guarded | 5 | 同上 |

**建议**：不为这五个词表创建 shotkey。英文的叙事 flavor 在中文中通过全局叙事风格保留，但不需要逐词对应。如果未来需要为中文增加 flavor 多样性，可以在中文故事模板里内嵌随机选择（比如用 `switch` 在 2-3 个中文等效句式间切换）。

> **对 `locate` 词表的例外**：`GetRumor` 中的 `locate`（held by a powerful creature / lost somewhere / found）三种值导致完全不同的句型，必须用独立模板（见 1.3.4）。

---

## 2. 建议修复顺序

### Phase 1：简单的 shotkey（可独立测试） ✅
1. ✅ OPL `Belongs to` → `quest.tome.opl.belongs_to`
2. ✅ Emote 头顶文字（三条 `LocalOverheadMessage`）
3. ✅ Gump 标题 `Quest for {0}`
4. ✅ 物品 OPL 名（`lost journal`、空箱 `chest`、`Chest of {0}`）

### Phase 2：流言系统重构 ✅
5. ✅ 流言模板（4 组 shotkey，覆盖三种 locate + 多种 who 前缀）
6. ✅ 修改 `GetRumor` / `TellRumor` 以使用 `ResolveFormatByKey`

### Phase 3：大型叙事模板（最复杂，需大量翻译） ✅
7. ✅ 故事模板（2 组：good / evil，含 ZH 翻译）+ 反转剧情通过参数交换实现
8. ✅ 修改 `QuestTomeGump` 构造函数，中文账号走 shotkey 模板路径
9. ✅ 帮助页模板（`quest.tome.help.guide`）

### Phase 4：后续优化 ✅
10. ✅ 物品 OPL 主名（`Journal of {0} the {1}`）→ `item.quest.journal.*` 按 `AGENTS.md` §3.2 标准处理
11. ✅ `MajorItemOnCorpse.Name = "chest"` / `"Chest of " + ...` → OPL 本地化

---

## 3. 按模块明细

### 3.1 QuestTake.cs

- cliloc 部分：**不处理**
- 已部分 `StringCatalog` / `ResolveText`

#### 硬编码明细

| 行 | 类型 | 英文 | 中文化方案 | 状态 |
|----|------|------|-----------|------|
| 28 | Item Name | `Journal of {Name} the {Job}` | OPL 用 `item.quest.journal.name` 模板；`Name` 本身保持英文（存档） | ⏭️ 名称保持英文存档，OPL 本地化 |
| 123-128 | String literal | heard / told / known / shared | **不单独词表化**；消融到故事模板中 | ✅ |
| 131-136 | String literal | legends / fables / myths / lore | 同上 | ✅ |
| 139-144 | String literal | whispered / told / sung / spoken | 同上 | ✅ |
| 147-153 | String literal | taverns / camps / cities / villages / inns | 同上 | ✅ |
| 156-162 | String literal | seized / stolen / taken / held / guarded | 同上 | ✅ |
| 169-177 | 长段拼接 | `QuestTomeStoryGood/Evil`（4 版本） | Phase 3: `quest.tome.story.good/evil` + 参数交换实现反转 | ✅ |
| 568 | Item Name | `"Chest of " + m.Name` | OPL 用 `item.quest.chest_of` 模板 | ✅ `AddNameProperties` 实现 |
| 637 | Item Name | `"chest"` | OPL 用 `item.quest.chest`（或复用 hash `s.7ca0d7019158ccd9`） | ✅ `DisplayNameLocalizationKey` |

### 3.2 QuestTome.cs

- cliloc 部分：**不处理**
- 已部分 `StringCatalog` / `ResolveText`

| 行 | 类型 | 英文 | 中文化方案 | 状态 |
|----|------|------|-----------|------|
| 138 | Item Name | `"lost journal"` | 已有 hash `s.a3734bf593f52976` (`丢失的日记`)；或 shotkey `quest.tome.name.lost_journal` | ✅ `DisplayNameLocalizationKey` |
| 145 | OPL | `"Belongs to " + Name` | `quest.tome.opl.belongs_to` + `AddLocalizedProperty` | ✅ |
| 372 | String literal | `"Journal of "` | 消融到 gump 标题处理逻辑中 | ✅ |
| 373 | String literal | `DDDDD` | 格式参数化，不再需要 `Replace` | ✅ 中文路径已用 `{0}` |
| 378 | Gump | 大段教学文本 | `quest.tome.help.guide` | ✅ |
| 388 | Gump | `"Quest for " + from.Name` | `quest.tome.gump.title` | ✅ |
| 456 | String literal | `"held by a powerful creature"` | 消融到 `quest.tome.rumor.heard_held` | ✅ |
| 457 | String literal | `"lost somewhere"` | 消融到 `quest.tome.rumor.heard_lost` | ✅ |
| 458 | String literal | `"found"` | 消融到 `quest.tome.rumor.heard_found` | ✅ |
| 470-479 | String literal | who 前缀（6 种） | `quest.tome.rumor.who_*` 系列 | ✅ |
| 480 | 拼接 | `who + " that " + item + " may be " + locate + ...` | `quest.tome.rumor.heard_*` / `quest.tome.rumor.talk` | ✅ |
| 483 | 拼接 | `from + " has told you that " + item + ...` | `quest.tome.rumor.talk` | ✅ |
| 581 | Emote | `"You found " + relic + "."` | `quest.tome.emote.found_relic` | ✅ |
| 593 | Emote | `citizen + " was either wrong or they lied."` | 复用 `quest.n0_was_either_wrong_or_they_lied_dot` | ✅ |
| 608 | Emote | `"You found " + GoalItem4 + "."` | `quest.tome.emote.found_goal` | ✅ |

---

## 4. C# 修改要点

### 4.1 OPL（`QuestTome.AddNameProperties`，行 142-146）

```csharp
public override void AddNameProperties(ObjectPropertyList list)
{
    base.AddNameProperties(list);
    if (QuestTomeOwner != null)
    {
        if (BuildingPropertyListLocale != null)
            AddLocalizedProperty(list, "quest.tome.opl.belongs_to", QuestTomeOwner.Name);
        else
            list.Add(1049644, "Belongs to " + QuestTomeOwner.Name + "");
    }
}
```

### 4.2 Emote（`FoundItem`，行 581, 593, 608）

```csharp
// 行 581 — 替换为：
player.LocalOverheadMessage(MessageType.Emote, 1150, true,
    StringCatalog.ResolveFormatByKey(player.Account, "quest.tome.emote.found_relic", relic));

// 行 593 — 替换为（复用已有键）：
player.LocalOverheadMessage(MessageType.Emote, 1150, true,
    StringCatalog.ResolveFormatByKey(player.Account,
        "quest.n0_was_either_wrong_or_they_lied_dot", book.QuestTomeCitizen));

// 行 608 — 替换为：
player.LocalOverheadMessage(MessageType.Emote, 1150, true,
    StringCatalog.ResolveFormatByKey(player.Account, "quest.tome.emote.found_goal", book.GoalItem4));
```

### 4.3 Gump 标题（QuestTomeGump 行 388）

```csharp
AddHtml(12, 46, 346, 20,
    @"<BODY><BASEFONT Color=" + color + ">" +
    StringCatalog.ResolveFormatByKey(from.Account, "quest.tome.gump.title", from.Name) +
    @"</BASEFONT></BODY>",
    false, false);
```

### 4.4 流言系统（`GetRumor` 和 `TellRumor`）

```csharp
// GetRumor 中 — 改写为基于 shotkey 的分支
public static string GetRumor(QuestTome book, bool talk)
{
    int goal = book.QuestTomeType;
    int locateType = 0; // 0=held, 1=lost, 2=found
    if (goal == 2) locateType = 1;
    if (book.QuestTomeGoals == 3) { locateType = 2; goal = 3; }

    string world = Server.Lands.LandName(book.QuestTomeLand);
    string dungeon = book.QuestTomeDungeon;
    string from = book.QuestTomeCitizen;
    string item = book.GoalItem1;
    if (book.QuestTomeGoals == 1) item = book.GoalItem2;
    else if (book.QuestTomeGoals == 2) item = book.GoalItem3;
    else if (book.QuestTomeGoals == 3) item = book.VillainName + " " + book.VillainTitle;

    if (talk)
    {
        // 居民直接提供了线索
        return StringCatalog.ResolveFormatByKey(/* from account *, "quest.tome.rumor.talk",
            from, item, /* locate - ignored in ZH */ "", dungeon, world);
    }

    // 根据 locate 类型选择不同的模板
    string rumorKey;
    switch (locateType)
    {
        case 1: rumorKey = "quest.tome.rumor.heard_lost"; break;
        case 2: rumorKey = "quest.tome.rumor.heard_found"; break;
        default: rumorKey = "quest.tome.rumor.heard_held"; break;
    }
    return StringCatalog.ResolveFormatByKey(/* from account */, rumorKey, item, dungeon, world);
}
```

> **Note**: `ResolveFormatByKey` 需要 `IAccount` 参数。`GetRumor`/`TellRumor` 是 `static` 方法，当前没有接收 `Mobile` 参数。在 `TellRumor` 中已有 `PlayerMobile player` 参数，可以传到 `GetRumor` 中。

### 4.5 故事模板（QuestTomeGump 构造函数）

详见 1.4 节的伪代码。中文路径下用 `ResolveFormatByKey` 配合 `quest.tome.story.good`/`evil` 重建叙事，保留原有英文路径不变。

### 4.6 帮助页（QuestTomeGump 行 378）

将整个 `AddHtml` 调用替换为：

```csharp
string guideKey = "quest.tome.help.guide";
string guideText = StringCatalog.ResolveFormatByKey(from.Account, guideKey,
    m_Book.GoalItem4,
    m_Book.QuestTomeNPCEvil,
    m_Book.QuestTomeNPCGood,
    m_Book.VillainName,
    m_Book.VillainTitle);
AddHtml(12, 43, 878, 548,
    @"<BODY><BASEFONT Color=" + color + ">" + guideText + @"</BASEFONT></BODY>",
    false, false);
```

> **注意**：帮助页中变量（VillainName、GoalItem4 等）出现多次。在格式字符串中用 `{0}`、`{3}` 等索引号重复引用即可，不需要为每次出现传递独立参数。

---

## 5. 专有名词中文化方案（待办）

### 5.1 问题说明

当前所有任务中的人名、地名、物品名（物品本身的名字以及任务文本中的物品）仍然是英文。具体表现为：

- 故事模板（`quest.tome.story.good/evil`）中的地名（Sosaria、Lodoria 等）、NPC 名、反派名、物品名为纯英文
- 帮助页模板（`quest.tome.help.guide`）中的同上
- 流言模板中的城市名（Britain、Yew 等）、职业名（tinker、blacksmith 等）为纯英文
- 反派类别（a daemon、a dragon 等）为纯英文
- OPL 物品名（`Journal of <Name> the <Job>`、`Chest of <Name>`）核心名为英文

**原则**：所有专有名词应以 `中文（English）` 格式呈现，且第一出现处标注完整。

### 5.2 专有名词分类

| 分类 | 来源 | 运行时/静态 | 当前示例 |
|------|------|-------------|---------|
| **世界/大陆名** | `Lands.LandName()` + `Lands.LandShotKey()` | 运行时枚举 | "the Land of Sosaria" → 索沙尼亚（Sosaria） |
| **城市名** | `RandomThings.GetRandomCity()` 词表（23 个城市） | 运行时随机 | "Britain" → 不列颠（Britain） |
| **地点/地城名** | `QuestStories.SomePlace()` 词表（101 个地点） | 运行时随机 | "Dungeon Doom" → 末日地城（Dungeon Doom） |
| **NPC 名（EpicCharacter）** | 世界中的 `EpicCharacter` 实体 | 运行时遍历 | "Sir Galahad the Brave" |
| **反派名** | `NameList.RandomName("daemon"/"giant"/etc.)` | 运行时随机 | "Morgath the Dark Lord" |
| **反派类别** | `VillainCategory` 字段赋值 | 运行时随机 | "a daemon"、"a dragon" |
| **目标物品名** | `QuestCharacters.QuestItems()` → 随机形容词 + 物品名 + of + 力量名 | 运行时随机 | "'Exotic Amulet of Might'" |
| **草药名** | `RandomHerb()` 词表 | 运行时随机 | "Enchanted Mandrake Root" |
| **法术名** | `RandomMagic()` → 从 `DDRelicScrolls` 取 | 运行时随机 | "Merlin's Scroll of Acidic Storm" |
| **职业名** | `RandomThings.GetRandomJob()` 词表（23 个职业） | 运行时随机 | "blacksmith"（含 rumors） |
| **头衔** | `RandomThings.GetBoyGirlJob()` 词表 | 运行时随机 | "the Knight" |
| **怪物名** | `RandomThings.GetRandomMonsters()/GetRandomCreature()` 词表 | 运行时随机 | "a dragon"、"a lich" |

### 5.3 处理策略

#### 策略 A：世界/大陆名（已有 glossary 条目）—— 优先处理

已有 glossary 条目：Sosaria（索沙尼亚）、Lodoria（洛多里亚）、Ambrosia（安布罗西亚）、Skara Brae（斯卡拉布雷）、Serpent Island（蛇岛）、Isles of Dread（恐惧群岛）、Umber Veil（琥珀帷幕）、Kuldar（库尔达）、Savaged Empire（蛮族帝国）、Atlantis（亚特兰蒂斯）、Luna（月之城）、Underworld（冥界）、Britain（不列颠）、Lodoria City（洛多里亚城）、Kuldara（库尔达拉）。

**方案**：在 `quest.tome.story.good/evil` 和 `quest.tome.help.guide` 的 ZH 模板中，直接将地名替换为 `中文（English）` 格式。

```json
// 当前：
"你找到了冒险者 {0} 的日记。...将日记交给 {15} 的 {1}，坐标如下：<br><br>{16}"
// 修改为：
"你找到了冒险者 {0} 的日记。...将日记交给 {1}（在{15}），坐标如下：<br><br>{16}"
// 其中 {15} 是大陆名，由 Lands 枚举传入
// 大陆名在 runtime 由 Lands.LandName() 返回英文，不改动 C#，在模板中用中文加注
```

但大陆名是**运行时来自 Lands 枚举**的，不能直接在模板里硬编码中文——因为同一个 `{15}` 可能是 Sosaria、Lodoria、Savaged Empire 等。需要在 C# 中将大陆名先解析为带注解的中文，再传入模板。

**方案**：利用 `Lands.LocalizedLandName()` 或 `Lands.LandShotKey()` + `StringCatalog.ResolveByKey`，在 C# 构造参数时直接将大陆名替换为 `中文（English）` 字符串，再传入模板。

```
待办：在 QuestTomeGump 构造参数阶段，将 world/locat 等大陆参数字段从 LandName() 改为 LocalizedLandName() 或自定义注解版本。
```

#### 策略 B：EpicCharacter NPC 名 + 反派名（运行时动态）—— 中等优先级

这些是运行时从世界 `EpicCharacter` 实体或 `NameList` 获取的名称，不可能在模板中做映射。**建议**：
- 在故事模板的 ZH 文本中，对于角色名字段使用 `{0}` 格式参数直接代入，假设英文名本身已在游戏世界中存在（NPC 名在客户端本身是英文），不影响理解。
- 对于 `VillainCategory`（如 "a daemon"、"a dragon"），这些是怪物类型，可以在 glossary 中添加常见怪物类型的条目，然后在 C# 中增加一个本地化辅助方法 `ResolveVillainCategory()` 将类别字符串映射为 `中文（English）`。

#### 策略 C：目标物品名 + 草药名 + 法术名（运行时动态）—— 低优先级

目标物品由 `QuestItems()` 等方法在运行时随机生成（如 `'Exotic Amulet of Might'`），生成的是英文名。这些是虚构物品，没有固定中文译名。**建议**：
- 保留物品名原文，在模板中用 `{2}` 等格式参数直接代入
- 不做 `中文（English）` 注解——因为这是随机生成的物品，不是固定专有名词

#### 策略 D：城市名 + 职业名（流言系统中）—— 需要 glossary 更新

城市名出现在 `GetRumor` 中，通过 `"the " + RandomThings.GetRandomJob() + " in " + RandomThings.GetRandomCity() + " told me"` 之类的方式嵌入。

**方案**：
1. 将 23 个城市名和 23 个职业名加入 glossary
2. 在 `GetRumor` 的中文分支中，对随机选取的城市/职业用 `中文（English）` 格式呈现

#### 策略 E：地城名（`QuestStories.SomePlace()`）—— 需要 glossary 更新

101 个地点/地城名出现在 `QuestTome.SetRumor` 中（存储在 `QuestTomeDungeon` 字段），随后在流言模板中以英文呈现。

**方案**：
1. 将主要地城名加入 glossary
2. 在 `GetRumor` 的中文分支中，对 dungeon 字段做本地化解析

### 5.4 待办列表

#### 5.4.1 Glossary 更新

| 类别 | 条目 | 优先级 |
|------|------|--------|
| ✅ 已有 | 全部世界/大陆名 | 已有 |
| 🔲 待加 | 23 个城市名（Britain, Fawn, Grey, Moon, Yew, Montor, Umbra, Devil Guard, Death Gulch, Renika, Glacial Hills, Springvale, Elidor, Islegem, Port of Dusk, Port of Starguide, Portshine, Greensky Village, City of Lodoria, Cimmeran Hold, Village of Barako, Village of Kurak, Kuldara） | P1 |
| 🔲 待加 | 23 个职业名（blacksmith, jeweler, provisioner, banker, minter, waiter, guard, sage, mage, herbalist, alchemist, healer, guildmaster, tinker, innkeeper, bartender, butcher, tailor, weaver, shipwright, scribe, farmer, stable master） | P1 |
| 🔲 待加 | 反派类别（daemon, balron, balor, balrog, devil, succubus, demoness, daemoness, dragon, giant, etc.） | P2 |
| 🔲 待加 | 主要地城名（Dungeon Doom, Dungeon Covetous, Dungeon Deceit, Dungeon Destard, Dungeon Hythloth, Dungeon Shame, Dungeon Wrong, Terathan Keep, Stonegate Castle, Serpent Sanctum, Morgaelin's Inferno, Stygian Abyss, etc.） | P2 |
| 🔲 待加 | 主要怪物名（dragon, daemon, lich, vampire, ghost, zombie, gargoyle, orc, troll, ogre, golem, etc.） | P3 |

#### 5.4.2 C# 修改

| 任务 | 描述 | 涉及文件 | 优先级 |
|------|------|---------|--------|
| 🔲 1. 大陆名本地化辅助 | 在 `QuestTomeGump` 构造参数阶段，用 `Lands.LocalizedLandName()` 或自定义注解方法替代 `Lands.LandName()` 为 `中文（English）` 格式 | `QuestTome.cs` | P1 |
| 🔲 2. 故事模板更新 | 更新 `quest.tome.story.good/evil` 的 ZH 模板，确保地名使用 `中文（English）` 格式 | `world-player-text.json` | P1 |
| 🔲 3. 帮助页模板更新 | 更新 `quest.tome.help.guide` 的 ZH 模板，同上 | `world-player-text.json` | P1 |
| 🔲 4. 流言系统城市/职业本地化 | 在 `GetRumor` 中文分支中，将 `RandomThings.GetRandomJob()` / `GetRandomCity()` 的结果包装为 `中文（English）` 格式 | `QuestTome.cs` | P1 |
| 🔲 5. 地城名本地化 | 在 `GetRumor` 中文分支中，用 glossary 查询 dungeon 字符串的本地化版本 | `QuestTome.cs` | P2 |
| 🔲 6. 反派类别本地化 | 为 `VillainCategory` 增加 `ResolveVillainCategory()` 辅助方法，映射 glossary 中的怪物类型为 `中文（English）` | `QuestTome.cs` | P2 |
| 🔲 7. 名词词表 review | 评估 `RandomThings.GetRandomName()` 返回的随机人名是否需要中文注解 | 无需修改 | P3 |

#### 5.4.3 更新顺序

**Step 1 (P1) — 模板中的地名注解**
1. 确认 `Lands.LocalizedLandName()` 可用并返回正确的 `中文（English）` 格式
2. 修改 `QuestTomeGump` 构造函数，用本地化版本的大陆名替代 `Lands.LandName()`
3. 无需改模板 JSON（地名作为参数传入，本地化发生在 C# 端）

**Step 2 (P1) — 流言系统城市/职业名词典注解**
1. 将所有城市名和职业名加入 `glossary-approved-zh.json`
2. 运行 `sync_localization_glossary.py` 同步
3. 在 `GetRumor` 中文分支中，将城市和职业名称用 glossary 映射为 `中文（English）`

**Step 3 (P2) — 反派类别 + 地城名词典注解**
1. 将主要怪物类型和地城名加入 glossary
2. 在 C# 中增加解析方法

**Step 4 (P3) — 评估剩余名词**
1. 评估 NPC 名和物品名是否需要更进一步的处理
2. 如评估无必要，标记为完成

---

## 6. 存档兼容性说明

- `QuestTomeStoryGood` / `QuestTomeStoryEvil`：保持序列化不变（version 不变），始终保存英文原文。
- 中文路径下运行时重建，不写回存档字段。
- `Item.Name`：保持英文原文不变。
- 添加 `IsContentLocalized` override 到 `QuestTome` / `MajorItemOnCorpse`（如需要 OPL 本地化），参考 `AGENTS.md` §3.2 "OPL 物品主显示名"。

---

## 7. 复查命令

```bash
# 扫描硬编码 SendMessage
rg 'SendMessage\s*\(\s*"' "World/Source/Scripts/Engines and Systems/Quests/Major"

# 扫描硬编码 OverheadMessage（非 cliloc）
rg 'LocalOverheadMessage\([^)]*true, "[A-Z]' "World/Source/Scripts/Engines and Systems/Quests/Major"

# 扫描未目录化的字符串拼接
rg '"[A-Z][a-z]{3,}.*" \+' "World/Source/Scripts/Engines and Systems/Quests/Major"
```

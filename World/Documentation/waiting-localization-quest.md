# 待中文化：Quests/Major 目录扫描

> **扫描日期：** 2026-05-22
> **处理范围：** **处理所有英文硬编码文本**（`SendMessage`、`Say`、Gump、`TextDefinition.AddHtmlText`、`AddRow`、OPL、`Name =`、字符串赋值、长段拼接模板等）。
> **排除范围：** **不处理 cliloc 控制的文本**（`SendLocalizedMessage`、仅 cliloc 数字的 `*OverheadMessage`）。已通过 `ResolveText` / `StringCatalog` 且键已在 locale 中的行亦不计入。
> **扫描路径：** `World/Source/Scripts/Engines and Systems/Quests/Major`
> **关联：** [`waiting-localization.md`](waiting-localization.md) · `AGENTS.md` §3.2
> Journal Quest：`QuestTake.cs` + `QuestTome.cs`。

## 摘要

| 指标 | 数量 |
|------|------|
| 扫描 `.cs` 文件 | 2 |
| **含英文硬编码（待处理）** | **2** |
| 无英文硬编码（或仅 cliloc / 已目录化） | 0 |

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

| 文件 | 硬编码条数 | 含 cliloc | StringCatalog | 主要类型 |
|------|------------|-----------|---------------|----------|
| `QuestTake.cs` | 240 | 是 | 部分 | Item Name, String literal |
| `QuestTome.cs` | 41 | 是 | 部分 | Gump, Item Name, OPL, Overhead, Return string, String literal |

## 建议修复顺序

1. OPL `Belongs to`、emote 头顶、`Quest for` → shotkey + `ResolveFormatByKey`
2. 物品 OPL 名（`lost journal`、空箱 `chest`、`Journal of …`）
3. `GetRumor` / `TellRumor` 词表 shotkey 化
4. Gump 帮助页（378 行）→ 专用 logical JSON
5. `QuestTake.SetupBook` 结构化叙事 + save version

## 按模块明细

### (root)

#### `QuestTake.cs`
- cliloc 部分：**不处理**
- 已部分 `StringCatalog` / `ResolveText`

| 行 | 类型 | 示例 |
|----|------|------|
| 28 | Item Name | Journal of  |
| 28 | Item Name |  the  |
| 123 | String literal | heard |
| 126 | String literal | told |
| 127 | String literal | known |
| 128 | String literal | shared |
| 131 | String literal | legends |
| 134 | String literal | fables |
| 135 | String literal | myths |
| 136 | String literal | lore |
| 139 | String literal | whispered |
| 142 | String literal | told |
| 143 | String literal | sung |
| 144 | String literal | spoken |
| 147 | String literal | taverns |
| 150 | String literal | camps |
| 151 | String literal | cities |
| 152 | String literal | villages |
| 153 | String literal | inns |
| 156 | String literal | seized |
| 159 | String literal | stolen |
| 160 | String literal | taken |
| 161 | String literal | held |
| 162 | String literal | guarded |
| 169 | String literal | You have found the journal of DDDDD, where they were given a quest by  |
| … | … | 另有 215 条 |

#### `QuestTome.cs`
- cliloc 部分：**不处理**
- 已部分 `StringCatalog` / `ResolveText`

| 行 | 类型 | 示例 |
|----|------|------|
| 138 | Item Name | lost journal |
| 145 | OPL | Belongs to  |
| 372 | String literal | Journal of  |
| 373 | String literal | DDDDD |
| 378 | Gump | >There are many times when adventurers are given a grand quest to obtain a magic |
| 378 | Gump | ?<br><br>Now you possess the journal and you can pursue this quest as it is your |
| 378 | Gump | . Otherwise, your quest will service good for  |
| 378 | Gump | . You may only have a single journal quest at any one time. If you find another  |
| 378 | Gump |  and claim  |
| 378 | Gump | , you will have to find 3 unique items to aid you. You have no idea where these  |
| 378 | Gump |  is. Again, talking to citizens may reveal a hint. Once you learn where  |
| 378 | Gump |  is, make haste to that location and face them in battle. Once you enter the are |
| 378 | Gump |  from them. Making them vanish by other means will rob you of your goal, as woul |
| 378 | Gump |  has fled to.<br><br>Slaying  |
| 378 | Gump |  will reveal an abundance of wealth they have taken from other adventurers that  |
| 378 | Gump |  will no longer need it. Once you have acquired  |
| 378 | Gump | , seek out  |
| 378 | Gump |  and hand them the journal. Your morality and fame will be affected by your choi |
| 388 | Gump | >Quest for  |
| 456 | String literal | held by a powerful creature |
| 457 | String literal | lost somewhere |
| 458 | String literal | found |
| 470 | String literal | I heard |
| 473 | String literal | I heard |
| 474 | String literal | I learned |
| … | … | 另有 16 条 |

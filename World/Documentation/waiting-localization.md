# 待中文化物品清单

> **更新日期：** 2026-05-17（§ **8** A5 / B1b / 静态书标题 hash 路径已落地；B2 键批与神器 zh 精修仍见 §8.3）
> **说明：** 本文档记录已完成中文化的装备属性系统之外，尚未开始中文化的物品类别。用于后续任务参考和范围规划。

---

## 目录

1. [SendMessage 与中文字符串（AI 流水线）](#sendmessage--中文字符串ai-流水线)
2. [物品名称中文化（OPL 主名）](#物品名称中文化)
3. [任务（待办）：硬编码 SendMessage / Say 全库清查](#任务待办硬编码-sendmessage--say-全库清查)
4. [已完成中文化的装备基类](#1-已完成中文化的装备基类)
5. [已修复的装备子类未保护 cliloc](#2-已修复的装备子类未保护-cliloc)
6. [Phase 3：Gift 附魔系统（已完成）](#3-phase-3gift-附魔系统已完成)
7. [Phase 4：Level 经验装备系统（已完成）](#4-phase-4level-经验装备系统已完成)
8. [待后续中文化的非装备物品](#5-待后续中文化的非装备物品)
9. [附录：非装备物品分类总表](#6-附录非装备物品分类总表)
10. [§5.3b Trades：工单（头顶 / Gump / 叙事 / OPL）](#53b-trades-tickets-overhead-gump-narrative-opl)
11. [§7 物品 OPL 名称扫描工单（T1–T7）](#7-物品-opl-名称扫描工单2026-05-17)
12. [§8 装备 OPL 主名、装备槽位、乐器与书本（根因 + 工单）](#8-equipment-opl-name-layer-books-instruments)

---

## SendMessage 与中文字符串（AI 流水线）

适用于 **`Mobile.SendMessage` / `Say` 等**仍硬编码英文、且**未**走 `SendLocalizedMessage(cliloc)` 的脚本（例如魔法物品使用失败提示）。

**首选：shotkey（逻辑键）** — 与 `trap.*`、`prop.*` 一样，在 **`en/` + `zh-Hans/` 同一对手写 JSON**（如 `equipment-properties.json`）中增加键值，代码使用 **`StringCatalog.ResolveByKey` / `ResolveFormatByKey`**。不跑提取器生成 `s.` 哈希键，键名须稳定、可读。

| 步骤 | 动作（shotkey） |
|------|------------------|
| 1 | 设计稳定键名（例：`prop.magical.moonstone.gate.inert`，与同类 `prop.magical.*` 同文件） |
| 2 | 在 `World/Data/Localization/en/<bundle>.json` 与 `zh-Hans/<bundle>.json` 写入同一键；`bundle` 须在 `AGENTS.md` §3.1 `keep_extra` 列表中 |
| 3 | `using Server.Localization;`，`SendMessage(StringCatalog.ResolveByKey(from.Account, "your.logical.key"))` |
| 4 | `sync_localization_glossary.py --check`（若文案含词表专有名词） |

**备选：哈希键** — `StringCatalog.Resolve(from.Account, "Exact English…")`，再执行 `build_localization_strings.py --no-translate`，对新生成的 `s.` 行做 §3.4 翻译。

| 步骤 | 动作（哈希） |
|------|----------------|
| 1 | `using Server.Localization;`，`SendMessage(StringCatalog.Resolve(from.Account, "..."))` |
| 2 | 仓库根目录 `python3 World/Source/Tools/build_localization_strings.py --no-translate` |
| 3 | 在对应 `en/scripts-*.json` 确认新 `s.` 键 |
| 4 | `llm_incremental_locale.py` queue → LLM → `apply` → `sync_localization_glossary.py --check` |

**注意：** 提取器只扫描 **`Resolve` / `ResolveFormat` 里的英文字面量**；shotkey 全部由手写 JSON 维护。约定见根目录 **`AGENTS.md` §3.2**。

**示例：** `Items/Magical/Moonstone.cs` — 月门石无法开启时使用 **`prop.magical.moonstone.gate.inert`** + `ResolveByKey`（文案在 `equipment-properties.json`）。

**色相（hue）与 `SendMessage(int hue, string)`：** 第一个参数仅为客户端着色（例如 **68** 表示成功提示类色调）。**第二参数字符串仍须完全目录化**，不可为中文账号拼接 `"英文片段 " + 变量`。应使用 **`ResolveFormatByKey`** 与 **子串 shotkey**（如投掷手套/弹药类型名），使 **zh-Hans** 整句符合中文语序。详见根目录 **`AGENTS.md` §3.2** 「Tinted SendMessage」。

---

## 物品名称中文化

**目标：** 账号语言为 **zh-Hans** 时，**对象属性列表（OPL）第一行**显示中文**物品名**（及可选著色，若该 shotkey 在 `Item.PropertyColorMap` 中有映射）。与 **`SendMessage` / `Say`** 的文案是两套键：物品名用 **`item.*`**，属性行与交互提示多用 **`prop.*`**，均可在同一文件 **`equipment-properties.json`** 中维护。

**适用范围（当前实现）：**

- ✅ **OPL 主名**：覆盖 **`AddNameProperty`**，在 **`BuildingPropertyListLocale != null`**（由 **`IsContentLocalized`** + **`GetLocalizedPropertyList`** 链触发）时输出目录化字符串。
- ⚠️ **未包含**：地面 **`OnSingleClick`**、纯客户端 **`LabelNumber`** 工具提示等通路仍可能显示英文 **`Name`**；若需一律中文，须在对应 API 上另做 **`StringCatalog`** / 重载。

**数据文件：**

- 英文：`World/Data/Localization/en/equipment-properties.json`
- 简体中文：`World/Data/Localization/zh-Hans/equipment-properties.json`  
- 键名约定：**`item.magical.*`**（魔法类）、 **`item.special.*`**（Special / 契约等）；**勿与 `prop.*` 混用同一键**（职责分离，便于检索）。

**实现步骤（AI / 人工均可照表执行）：**

| # | 动作 |
|---|------|
| 1 | 类已 **`IsContentLocalized => true`**（与现有双语 OPL 一致）。 |
| 2 | 在 **`en/` + `zh-Hans/`** `equipment-properties.json` 增加成对 **`item....`** 键；**zh-Hans** 专有名词 **`中文（English）` 行内注**（`AGENTS.md` §3.5）。 |
| 3 | **`public override void AddNameProperty(ObjectPropertyList list)`**：若 **`BuildingPropertyListLocale != null`**，单件 **`AddLocalizedProperty(list, "item....")`**；多件 **`list.Add(1050039, "{0}\t{1}", Amount, ResolvePropertyText("item...."))`**；否则 **`base.AddNameProperty(list)`**。 |
| 4 | **保留** 构造器里的 **`Name = "…"`**（及 **`LabelNumber`**）供序列化、脚本、**`switch (Name)`**；仅 **展示层**走 **`item.*`**。 |
| 5 | 父类已覆盖 **`AddNameProperty`**（例如 **`BaseTrinket`** 的 exceptional / resource 格式）时，子类须在 **`return`** 前给出双语分支（例：**`OrbOfTheAbyss`**），避免丢格式。 |
| 6 | **`python3 World/Source/Tools/sync_localization_glossary.py --check`**（文案触碰词表时）。 |

**代码参考（先例）：**

| 模式 | 文件 / 类 |
|------|------------|
| 逻辑键 + 多形态 `Name` | `Items/Magical/RuneOfVirtue.cs` |
| `CraftResource` 动态名 | `Items/Trades/Special.cs`（`BaseSpecial`） |
| Special + 深渊五态 + 契约 | 见本文 **§5.2**（**`item.special.*`** / **`item.magical.moonstone.name`**） |

**根目录规范：** **`AGENTS.md` §3.1**（`equipment-properties` 含 **`item.*`**）、**§3.2**（`AddNameProperty` 段落）。

---

## 任务（待办）：硬编码 SendMessage / Say 全库清查

**目标：** 在 `World/Source` 内检索所有 **`Mobile.SendMessage` / `Say` / `SendAsciiMessage`** 等仍为**硬编码英文字符串**、且**未**经过以下任一方式的玩家可见文案：

- `StringCatalog.ResolveByKey` / `ResolveFormatByKey`（shotkey）
- `StringCatalog.Resolve` / `ResolveFormat`（哈希键，已由提取器管理）
- `SendLocalizedMessage` / CliLoc 编号

并逐项改为 **shotkey（优先）** 或哈希 **`Resolve`** 流水线，补全 `zh-Hans`。

**建议检索（示例）：**

- `SendMessage\s*\(\s*"`
- `Say\s*\(\s*"`（排除已包裹 `StringCatalog` 的行）
- `SendAsciiMessage\s*\(\s*"`

**优先级建议：** 高频物品/技能失败提示 → 任务与副本提示 → 低频装饰文案。

**完成标准：** 玩家语言为 zh-Hans 时不应再出现未目录化的英文 `SendMessage` 句（允许 CliLoc 英服原文由客户端语言处理的情况除外）。

---

## 1. 已完成中文化的装备基类

以下装备基类已启用 `IsContentLocalized => true` 并使用 `AddLocalizedProperty` 双语 OPL 模式：

| # | Class | 源文件 | 状态 |
|---|-------|--------|------|
| 1 | `BaseWeapon` | `Items/Weapons/BaseWeapon.cs` | ✅ 已完成 |
| 2 | `BaseArmor` | `Items/Armor/BaseArmor.cs` | ✅ 已完成 |
| 3 | `BaseClothing` | `Items/Clothing/BaseClothing.cs` | ✅ 已完成 |
| 4 | `BaseTrinket` | `Items/Trinkets/BaseTrinket.cs` | ✅ 已完成 |
| 5 | `BaseInstrument` | `Items/Instruments/BaseInstrument.cs` | ✅ 已完成 |
| 6 | `BaseQuiver` | `Items/Quivers/BaseQuiver.cs` | ✅ 已完成 |
| 7 | `BaseHarvestTool` | `Items/Trades/BaseHarvestTool.cs` | ✅ 已完成 |
| 8 | `BaseTool` | `Items/Trades/BaseTool.cs` | ✅ 已完成 |
| 9 | `BaseSpecial` | `Items/Trades/Special.cs` | ✅ 已完成 |

---

## 2. 已修复的装备子类未保护 cliloc

以下类继承自已本地化的基类，但有额外的 `GetProperties` 覆盖使用无保护的 cliloc。已通过判断 `BuildingPropertyListLocale` 修复：

| # | 文件 | Class | 修复内容 |
|---|------|-------|---------|
| 1 | `Items/Weapons/BaseWeapon.cs` | `BaseWeapon` | 毒药（1062412+level）、装备层（1061182）、密度（1061182+Density）3处 |
| 2 | `Items/Armor/Leather/LeatherGloves.cs` | `LeatherGloves` | 奥术充能（1061837） |
| 3 | `Items/Clothing/Cloaks.cs` | `Cloak` | 奥术充能（1061837） |
| 4 | `Items/Clothing/OuterTorso.cs` | `Robe` | 奥术充能（1061837） |
| 5 | `Items/Clothing/Shoes.cs` | `ThighBoots` | 奥术充能（1061837） |

---

## 3. Phase 3：Gift 附魔系统（已完成）

Gift 系统是装备的扩展，为物品添加附魔点数和可自定义附魔功能。Gift 装备的基类定义在 `Items/Magical/Gift/` 中，具体装备类继承自 Gift 基类（位于对应装备目录）。

### 3.1 Gift 基类（13个）

| # | 文件 | 显示文本 | 类型 |
|---|------|---------|------|
| 1 | `Magical/Gift/BaseGiftClothing.cs` | "附魔"、"附魔点数"、"拥有者" | 衣物 |
| 2 | `Magical/Gift/BaseGiftShield.cs` | 同上 | 盾牌 |
| 3 | `Magical/Gift/BaseGiftJewel.cs` | 同上 | 饰品 |
| 4 | `Magical/Gift/BaseGiftArmor.cs` | 同上 | 护甲 |
| 5 | `Magical/Gift/BaseGiftStaff.cs` | 同上 | 法杖 |
| 6 | `Magical/Gift/BaseGiftBashing.cs` | 同上 | 钝器 |
| 7 | `Magical/Gift/BaseGiftWhip.cs` | 同上 | 鞭 |
| 8 | `Magical/Gift/BaseGiftAxe.cs` | 同上 | 斧 |
| 9 | `Magical/Gift/BaseGiftKnife.cs` | 同上 | 匕首 |
| 10 | `Magical/Gift/BaseGiftSword.cs` | 同上 | 剑 |
| 11 | `Magical/Gift/BaseGiftRanged.cs` | 同上 | 远程 |
| 12 | `Magical/Gift/BaseGiftSpear.cs` | 同上 | 矛 |
| 13 | `Magical/Gift/BaseGiftPoleArm.cs` | 同上 | 长柄 |

### 3.2 Gift 子类的额外属性

| # | 文件 | 额外文本 | 
|---|------|---------|
| 14 | `Magical/Gift/GiftCloaks.cs` | 奥术充能 |
| 15 | `Magical/Gift/GiftShoes.cs` | 奥术充能 |
| 16 | `Magical/Gift/GiftOuterTorso.cs` | 奥术充能 |
| 17 | `Magical/Gift/GiftLeatherGloves.cs` | 奥术充能 |
| 18 | `Magical/Gift/GiftThrowingGloves.cs` | "双击更改类型"、"无法与其他武器共用" |
| 19 | `Magical/Gift/GiftPugilistMits.cs` | "无法与其他武器共用" |

### 3.3 Gift / Level 运行时文案（SendMessage / 类内 Gump）

附魔与等级系统的 **OPL**（`gift.*` / `god.*`）此前已接入；**玩家消息** 与 **铁匠验证 Gump** 已统一为 `equipment-properties.json` 中的 **`god.msg.*`、`god.gump.levelup.*`**，代码使用 `StringCatalog.ResolveByKey` / `ResolveFormatByKey`。经验代币 **OPL 主名** 使用 **`item.god.exp.token.name`**。Gift/Level 共用 **`god.msg.attr.not.enough.points`**（附魔与升级加点均不足时）。**礼品提灯（Gift Lantern）** 已 `IsContentLocalized` 并补 **`god.lantern.*`** 与 Level 提灯一致。

---

## 4. Phase 4：Level 经验装备系统（已完成）

Level/God 系统为装备添加等级和经验值属性。基类定义在 `Items/Magical/God/` 中。

### 4.1 Level 基类（13个）

| # | 文件 | 显示文本 |
|---|------|---------|
| 1 | `Magical/God/BaseLevelClothing.cs` | "等级"、"经验值" |
| 2 | `Magical/God/BaseLevelShield.cs` | 同上 |
| 3 | `Magical/God/BaseLevelJewel.cs` | 同上 |
| 4 | `Magical/God/BaseLevelArmor.cs` | 同上 |
| 5 | `Magical/God/BaseLevelStaff.cs` | 同上 |
| 6 | `Magical/God/BaseLevelBashing.cs` | 同上 |
| 7 | `Magical/God/BaseLevelWhip.cs` | 同上 |
| 8 | `Magical/God/BaseLevelAxe.cs` | 同上 |
| 9 | `Magical/God/BaseLevelKnife.cs` | 同上 |
| 10 | `Magical/God/BaseLevelSword.cs` | 同上 |
| 11 | `Magical/God/BaseLevelRanged.cs` | 同上 |
| 12 | `Magical/God/BaseLevelSpear.cs` | 同上 |
| 13 | `Magical/God/BaseLevelPoleArm.cs` | 同上 |

### 4.2 Level 子类的额外属性

| # | 文件 | 额外文本 |
|---|------|---------|
| 14 | `Magical/God/LevelCloaks.cs` | 奥术充能 |
| 15 | `Magical/God/LevelShoes.cs` | 奥术充能 |
| 16 | `Magical/God/LevelOuterTorso.cs` | 奥术充能 |
| 17 | `Magical/God/LevelLeatherGloves.cs` | 奥术充能 |
| 18 | `Magical/God/LevelThrowingGloves.cs` | "双击更改类型"、"无法与其他武器共用" |
| 19 | `Magical/God/LevelPugilistMits.cs` | "无法与其他武器共用" |

### 4.3 Level 系统其他物品

| # | 文件 | 显示文本 |
|---|------|---------|
| 20 | `Magical/God/LevelUpScroll.cs` | "神奇强化符文（+{0} 最大等级）"、"崇高符文..."、"神圣符文..." |
| 21 | `Magical/God/LegendaryArtifactRename.cs` | "{0} 次使用剩余"、"重命名传说神器"、"属于..." |
| 22 | `Magical/God/MagicCandle.cs` | "双击装备/卸下" |
| 23 | `Magical/God/MagicLantern.cs` | "双击装备/卸下" |
| 24 | `Magical/God/MagicTorch.cs` | "双击装备/卸下" |
| 25 | `Magical/God/ItemExperienceToken.cs` | "经验值" |

---

## 5. 待后续中文化的非装备物品

以下物品直接继承 `Item`（而非装备基类），不参与装备 OPL 属性系统。它们通过 `AddNameProperties` 展示描述性文本，需要对每一条消息单独进行中文化包装。

这些物品分为几类，分别有不同的中文化策略。

### 5.1 Magical 目录 - 魔法物品（18个文件）（完成）

**运行时 `SendMessage`：** `SoulOrb`、`LuckyHorseShoes`、`SlayerDeed`、`ArtifactManual`、`ManualOfItems`、`ColoringBook`、`GemOfSeeing` 等已改用 **`prop.magical.*.msg.*`** / **`god.msg.*`**（与既有 `prop.magical.*` OPL 键并列，均在 `equipment-properties.json`）。屠魔契 **`prop.magical.slayer.msg.*`** 并修正英文第二槽提示为 “Your weapon …”（原 `You weapon` 笔误）。

| # | 文件 | Class | 英文文本 |
|---|------|-------|---------|
| 1 | `Items/Magical/SoulOrb.cs` | `SoulOrb` | "Contains vampire blood for..."、"Contains genetic patterns for..."、"Contains the Soul of..." |
| 2 | `Items/Magical/LuckyHorseShoes.cs` | `LuckyHorseShoes` | "Adds up to 100 Luck To An Item" |
| 3 | `Items/Magical/RuneOfVirtue.cs` | `RuneOfVirtue` | "Rune for..." |
| 4 | `Items/Magical/Moonstone.cs` | `MoonStone` | **主名** `item.magical.moonstone.name`；OPL：`prop.magical.moonstone`；使用失败：`prop.magical.moonstone.gate.inert` + `ResolveByKey` |
| 5 | `Items/Magical/SlayerDeed.cs` | `SlayerDeed` | 屠魔种类名称 |
| 6 | `Items/Magical/ArtifactManual.cs` | `ArtifactManual` | "This Identifies Items"、使用次数 |
| 7 | `Items/Magical/ManualOfItems.cs` | `ManualOfItems` | 使用次数、"Belongs to..."；**遗物箱 Gump** 标题/说明 **`god.gump.relicchest.*`**；列表行 **`god.legendbook.row.*`**（`zh-Hans/legend-book-rows.json`）；领取提示 **`god.msg.relic.chest.received`** |
| 8 | `Items/Magical/StaffOfFiveParts.cs` | `Part1`-`Part5` | "Belongs to..."（5处） |
| 9 | `Items/Magical/GemOfSeeing.cs` | `GemOfSeeing` | "Find Hidden Items And Traps"、使用次数 |
| 10 | `Items/Magical/PandorasBox.cs` | `PandorasBox` | "Magically Access Your Bank Box"、使用次数 |
| 11 | `Items/Magical/ColoringBook.cs` | `ColoringBook` | 颜色名称字符串 |
| 12 | `Items/Magical/Arcane/` | 4个元素书 | "...Book of Spells" |
| 13 | `Items/Magical/RuneOfVirtue.cs` | `RuneOfVirtue` | 符文类型描述 |

### 5.1b Magical/Artifacts 子目录神器（`SendMessage` 已改为 shotkey）

以下文件玩家提示已改为 **`prop.magical.artifact.*`**（`equipment-properties.json`）：`Artifact_StaffofSnakes`、`Artifact_GandalfsStaff`、`Artifact_HammerofThor`、`Artifact_HelmOfBrilliance`、`Artifact_RobeOfTeleportation`、`Artifact_AcidProofRobe`、`Artifact_RodOfResurrection`、`EverlastingLoaf`、`EverlastingBottle`。

### 5.2 Special 目录 - 特殊物品（5 + 8 契约 `.cs`）（部分完成）

| # | 文件 | Class | 策略 |
|---|------|-------|------|
| 1 | `Items/Special/SlaversNet.cs` | `SlaversNet` | `IsContentLocalized`；**主名** `item.special.slaversnet`；OPL `prop.special.slaversnet.capture.tamable`；`SendMessage`：`ResolveByKey` / `ResolveFormatByKey` |
| 2 | `Items/Special/OrbOfTheAbyss.cs` | `OrbOfTheAbyss` | **主名** `item.special.abyss.*`（五态）；OPL `prop.special.orbabyss.belongs`；装备 `prop.special.orbabyss.not.yours`；`ChangeOrb` `prop.special.orbabyss.tinker.give` |
| 3 | `Items/Special/AlternateRealityMap.cs` | `AlternateRealityMap` | **主名** `item.special.altmap`；OPL `prop.special.altmap.examine` |
| 4 | `Items/Special/SoulStone.cs` | `SoulStone` / `SoulstoneFragment` | **主名** `item.special.soulstone` / `item.special.soulstone.fragment`（默认无 `Name`）；`GetProperties`：`prop.special.soulstone.*`、碎片 `prop.special.soulstonefragment.uses.*`；`prop.special.soulstone.msg.absorb.explode` |
| 5 | `Items/Special/DragonPedStatue.cs` | `DragonPedStatue` | **主名** `item.special.dragon.statue`；材质 `prop.special.dragonstatue.material`；刻名 `prop.special.dragonstatue.inscription` |
| 6 | `Broken Furniture Collection/*.cs` | 8× `*Deed` | **主名** `item.special.deed.broken.*`；放置说明 `prop.special.brokenfurniture.place.in.home` |

**复查（2026-05-17）：** `Items/Special/Rares/PaganReagents/*.cs` 拾取提示已改为 **`prop.special.paganreagent.decorative.*`**。`BarbaricSatchel`、`DemonPrison`（含献金成功 **`prop.special.demonprison.gold.added`**）、`DragonEgg` / `DrakkhenEgg` / `DracolichSkull`（金币反馈 **`prop.special.egg.gold.added`**）、`HugeWaterTub` 等玩家 **`SendMessage`** 已走 **`StringCatalog.Resolve*`** 与 `equipment-properties.json`。其它 `Special/Rares` 若新增脚本，请用 `SendMessage( StringCatalog.` 检索复核。

### 5.3 Trades 目录 - 交易技能物品（运行时消息已收敛）

**复查（2026-05-17）：** `Scripts/Items/Trades/` 下列路径的玩家 **`SendMessage`** 已改为 **`ResolveByKey` / `ResolveFormatByKey`**（键在 **`equipment-properties.json`**）：`Blacksmithing/DwarvenForge`、`Fishing/*Barrel`、`Lumberjack/Log`（含锯木失败）、`Misc/Bandage`（祝福不可用、随从未死、饥肠治疗拒绝、**`EnableHealingLogging`** 调试行）、`Magical/Moongate`、`Glass Stone/*Book`（玻璃吹制 / 石工 / 采石 / 采沙）、`Tailoring/SpoolOfThread`（织机未完成布 / 材料不足）、`Thieving/DisguiseKit`、`LockPick`、`Sextant`、`PickBox`、`Clocks`、`Ore`、`MagicFish`、`Dyes` 等。新增 trades 物品请勿再写字面量英文 **`SendMessage("...")`**。

| 子目录 | 文件 | 策略摘要 |
|--------|------|----------|
| **Blacksmithing/** | `FireGiantForge.cs`、`RubyPickaxe.cs` | 已完成 |
| **Bowcraft/** | `ArrowsAndBolts.cs`（四类 Bundle） | `IsContentLocalized`；主名与 OPL `prop.trade.bow.*`；消息与头顶 `StringCatalog` |
| **Fishing/** | 渔获/渔网/deed/沉船等 | 已完成 |
| **Carpentry/** | `TaxidermyKit.cs` | 已完成 |
| **Thieving/** | 骷髅钥匙三文件 | 已完成 |
| **Forensics/** | `PolishBoneBrush.cs` | 已完成 |
| **Reagents/** | `GoldenFeathers.cs`、`Reagents.cs` | 已完成 |
| **Cartography/** | `MapRanger.cs`、`MapRangerDoor`、`LocalMap`、`WorldMap`、`CityMap`、`SeaChart`、`TreasureMap` | `IsContentLocalized`；`GetProperties` / `SendMessage` 走 `StringCatalog` / `ResolvePropertyText` |
| **Alchemy/** | `AlchemyTub.cs`、`CrystallineJar.cs` | 已完成 |
| **Ninjitsu/** | `Shuriken`、`FukiyaDarts`、`Fukiya`、`LeatherNinjaBelt` | `NinjaAmmoOplProperties`（`NinjaWeapons.cs`）；毒药与次数 OPL；腰带装备提示键 |

<a id="53b-trades-tickets-overhead-gump-narrative-opl"></a>

### 5.3b Trades tickets（头顶 / Gump / 叙事 / OPL 面）

> **来源：** 2026-05-17 对 `Scripts/Items/Trades/` 全目录复查：`SendMessage("` 与 `.Say("` 已无硬编码英文；下列为 **仍非 `StringCatalog`、玩家可能看到英文** 的条目，需单独开单处理。

| 优先级 | 子类 | 文件（路径相对 `Scripts/Items/Trades/`） | 现状与建议 |
|--------|------|------------------------------------------|------------|
| P1 | 开锁过程头顶 | `Thieving/LockPick.cs` | **已完成（2026-05-17）：** **`prop.trade.lockpick.overhead.*`**（`equipment-properties.json`），`PrivateOverheadMessage` 走 **`StringCatalog.ResolveByKey(from.Account, …)`**；宝箱 **`PublicOverheadMessage`** 按客户端语言 **`TryResolveByKey`** 广播（逻辑键无法走 `Item.PublicOverheadMessage` 的英文哈希解析）。开锁器 **`InfoDataGump`**：`lockpick.normal` / `lockpick.tech`。 |
| P2 | 月门确认 Gump | `Magical/Moongate.cs` | **已完成（2026-05-17）：** `!Core.AOS` 分支主提示改为 **`AddHtmlLocalized(..., 1062049, 32512, ...)`**，与 AOS 同级 cliloc 一致。 |
| P0 | SOS 沉船求救全文 | `Fishing/Misc/SOS.cs` | **已完成（2026-05-17）：** 新掉落使用 **`save version 6` + 结构化字段**（模板 0–4、异怪 0–11、同伴、城市、幸存人数）；打开 gump 时按账号语言 **`StringCatalog.ResolveFormatByKey`** 拼 **`prop.trade.sos.story.*` / `prop.trade.sos.beast.*` / `prop.trade.sos.prefix.ancient`**；坐标行 **`prop.trade.sos.coords.fmt`**（中文用全角逗号与分 **′**）。**旧存档**仍读 `ShipStory` 英文 + `QuestCompositeResolver`。GM 用 `Ship_Story` 覆盖会 **`m_StructuredStory = false`** 退回旧通路。 |
| P2 | Speech gump 标题 | `Fishing/FishBarrel.cs`、`Fishing/ScrapIronBarrel.cs` | **已完成（2026-05-17）：** 标题 **`prop.trade.barrel.gump.title.fish` / `.scrap`**；正文 **`prop.trade.barrel.speech.aquarium` / `.scrapmetal`**（`Talk.cs` **`SpeechFunctions.SpeechText`** 分支走 **`StringCatalog.ResolveByKey`**）。 |
| P2 | 遗物钟名称 / OPL 金额 | `Tinkering/Clocks.cs` | **已完成（2026-05-17）：** 共享基类 **`DDRelicClockBase`**，`save version 2` 持久化 **`m_RelicClockAdjIndex`**；双语 OPL：**`prop.trade.relicclock.adj.0`…`18`**、**`prop.trade.relicclock.name.fmt`**、**`prop.trade.relicclock.worth`**；金额行经 **`Item.AddColorText3Property`** 钩子注入（见 `Item.cs`），位置与原 `ColorText3` 一致。 |
| P3 | 全目录 `DefaultDescription` / `DefaultName` / `Name =` | `Scripts/Items/Trades/` 下多文件 | **已完成（2026-05-17）：** `InfoDataGump` 经 **`InfoDataLocalizationKey`**（`prop.trade.itemdesc.*`）。**属性列表（OPL）显示名**：`Item.DisplayNameLocalizationKey`（`item.trade.name.*`）+ **`ShouldUseLocalizedOpl()`**（有显示名 shotkey 即走双语 OPL，不必每类再写 `IsContentLocalized`）。已覆盖 P3 批次静态名物品、五档练习箱、八类符文工具×三档、稀有试剂名、工艺书四本、六分仪等。**动态材料 / 桶名 / 地图（2026-05-17）：** `CraftResources.GetTradeItemFullName` 双语 OPL 经各 **`Base*` 资源类** 的 **`IsContentLocalized` + `CraftResources.AddLocalizedTradeCommodityNameProperty`**；后缀与格式键在 **`trade-commodity.json`**（`trade.suffix.*`、`trade.compose.material_suffix`、`trade.custom.*`），材料短名仍走 **`resource-harvest-extra`** 哈希；`PotionKeg` 使用 **`trade.keg.potion.*`**；`PlaceMap` 使用 **`m_TargetPlaceLabel` + `placemap-labels.json`**（地名哈希）及 **`placemap.name.format`**。`MapRanger` 描述仍为 **`prop.trade.mapranger.longdesc`**。 |

**复查命令（仅供执行人）：**

- `SendMessage\s*\(\s*"`、`\.Say\s*\(\s*"`：应为空。
- `PrivateOverheadMessage\s*\([^)]*"`、`PublicOverheadMessage\s*\([^)]*"`：先清 `LockPick.cs`，再全库扩展同一模式。

---

## 6. Quest

### 6.1 Major 目录
`/World/Source/Scripts/Engines and Systems/Quests/Major`

---

## 7. 物品 OPL 名称扫描工单（2026-05-17）

> **扫描方法：** 全库 `Name = "English..."` + `DisplayNameLocalizationKey` / `ShouldUseLocalizedOpl()` / `AddNameProperty` 覆盖状态检查，找出 zh-Hans 账号仍看到英文名的物品类别。
>
> **通用实现模式（无 AddNameProperty 的简单类）：**
> 1. 在类里加 `public override string DisplayNameLocalizationKey => "item.trade.name.XXX";`（或 `item.magical.*` / `item.special.*`）
> 2. 在 `en/equipment-properties.json` 与 `zh-Hans/equipment-properties.json` 各加同一键；zh 值按 §3.5 规范（专有名词 `中文（English）`）
> 3. `BaseTool` / `BaseHarvestTool` / `Item` 基类均已实现 `ShouldUseLocalizedOpl()`，无需额外覆盖 `IsContentLocalized` 或 `AddNameProperty`
>
> **注意：** 以下各工单中 `✅ 已覆盖` 标注代表已有 `DisplayNameLocalizationKey` 或等效覆盖，**仅供参考对比**；无标注的即为待处理项。

---

### T1 — 炼金 / 巫术 / 普通试剂名（~25 条）

**优先级：P1**（玩家背包常驻材料，高曝光）

| 文件（相对 `Scripts/Items/Trades/Reagents/`） | 英文 Name | 状态 |
|---|---|---|
| `Alchemy/MoonCrystal.cs` | `moon crystal` | ❌ |
| `Alchemy/SeaSalt.cs` | `sea salt` | ❌ |
| `Alchemy/ButterflyWings.cs` | `butterfly wings` | ❌ |
| `Alchemy/Brimstone.cs` | `brimstone` | ❌ |
| `Alchemy/SilverWidow.cs` | `silver widow` | ❌ |
| `Alchemy/GargoyleEar.cs` | `gargoyle ear` | ❌ |
| `Alchemy/EyeOfToad.cs` | `eye of toad` | ❌ |
| `Alchemy/FairyEgg.cs` | `fairy egg` | ❌ |
| `Alchemy/BeetleShell.cs` | `beetle shell` | ❌ |
| `Alchemy/SwampBerries.cs` | `swamp berries` | ❌ |
| `Alchemy/PixieSkull.cs` | `pixie skull` | ❌ |
| `Alchemy/RedLotus.cs` | `red lotus` | ❌ |
| `Witch/MummyWrap.cs` | `mummy wrap` | ❌ |
| `Witch/BlackSand.cs` | `black sand` | ❌ |
| `Witch/BloodRose.cs` | `blood rose` | ❌ |
| `Witch/Maggot.cs` | `maggot` | ❌ |
| `Witch/DriedToad.cs` | `dried toad` | ❌ |
| `Witch/Wolfsbane.cs` | `wolfsbane` | ❌ |
| `Witch/BitterRoot.cs` | `bitter root` | ❌ |
| `Witch/WerewolfClaw.cs` | `werewolf claw` | ❌ |
| `Witch/VioletFungus.cs` | `violet fungus` | ❌ |
| `Common/BlackPearl.cs` | `black pearl` | ❌ |
| `Reagents.cs` (`JarOfWizardReagents`) | `Jar of Wizard Reagents` | ❌ |
| `Reagents.cs` (`JarOfNecromancerReagents`) | `Jar of Necromancer Reagents` | ❌ |
| `Reagents.cs` (`JarOfAlchemicalReagents`) | `Jar of Alchemical Reagents` | ❌ |
| `Unique/GoldenFeathers.cs` | `golden feathers` | ✅ 已覆盖 |
| `Unique/DragonBlood.cs` 等 9 Unique | 各项 | ✅ 已覆盖 |

**键名建议：** `item.trade.name.reagent.moon.crystal`、`item.trade.name.reagent.sea.salt` … `item.trade.name.reagent.jar.wizard` 等，统一前缀 `item.trade.name.reagent.*`。

---

### T2 — 基础技能工具名（非符文类，~24 条）

**优先级：P1**（玩家日常使用的工具，高曝光）

> 符文工具（`SmithHammerRunic`、`TinkerToolsRunic`、`ScribesPenRunic`、`LeatherworkingToolsRunic`、`CarpenterToolsRunic`、`SewingKitRunic`、`FletcherToolsRunic`、`UndertakerKitRunic`）**已完成**（`DisplayNameLocalizationKey`）。下表为尚未覆盖的基础非符文版本。

| 文件（相对 `Scripts/Items/Trades/`） | 英文 Name | 状态 |
|---|---|---|
| `Tailoring/SewingKit.cs` | `sewing kit` | ❌ |
| `Tailoring/StitchingTools.cs` | `stitching tools` | ❌ |
| `Tailoring/LeatherworkingTools.cs` | `tanning tools` | ❌ |
| `Carpentry/CarpenterTools.cs` | `carpenter tools` | ❌ |
| `Carpentry/WoodworkingTools.cs` | `woodworking tools` | ❌ |
| `Tinkering/TinkerTools.cs` | `tinker tools` | ❌ |
| `Blacksmithing/SmithHammer.cs` | `smith hammer` | ❌ |
| `Blacksmithing/LapidaryTools.cs` | `lapidary hammer` | ❌ |
| `Blacksmithing/ScalingTools.cs` | `scaling tools` | ❌ |
| `Blacksmithing/Spade.cs` | `shovel` | ❌ |
| `Blacksmithing/Pickaxe.cs` (gargoyle variant) | `gargoyle pickaxe` | ❌ |
| `Blacksmithing/RubyPickaxe.cs` | `adamantium pickaxe` | ❌ |
| `Inscription/ScribesPen.cs` | `scribe quill` | ❌ |
| `Inscription/TomeOfWands.cs` | `tome of wands` | ❌ |
| `Inscription/Monocle.cs` | `librarian set` | ❌ |
| `Glass Stone/Blowpipe.cs` | `blowpipe` | ❌ |
| `Glass Stone/MalletAndChisel.cs` | `mallet and chisel` | ❌ |
| `Cooking/CulinarySet.cs` | `culinary set` | ❌ |
| `Bowcraft/FletcherTools.cs` | `bowcrafting tools` | ❌ |
| `Bowcraft/Arrow.cs` | `arrow` | ❌ |
| `Bowcraft/Bolt.cs` | `bolt` | ❌ |
| `Bowcraft/Feather.cs` | `feather` | ❌ |
| `Bowcraft/Shaft.cs` | `shaft` | ❌ |
| `Forensics/UndertakerKit.cs` | `undertaker kit` | ❌ |
| `Forensics/GraveSpade.cs` | `grave shovel` | ❌ |
| `Forensics/Bones.cs` | `bones` | ❌ |
| `Alchemy/Jar.cs` | `jar` | ❌ |
| `Alchemy/ApothecaryVials.cs` | `apothecary set` | ❌ |
| `Alchemy/MortarPestle.cs` | `alchemy set` | ❌ |
| `Druid/DruidCauldron.cs` | `druid's cauldron` | ❌ |
| `Witch/WitchCauldron.cs` | `witch's cauldron` | ❌ |
| `Bowcraft/FletcherToolsRunic.cs` | `runic bowcrafting tools I/II/III` | ❌（缺 `DisplayNameLocalizationKey`） |

**注：** `Bandage`、`FishingPole`、`Flax`、`Cotton`、`Wool`、`Scissors`、`SpoolOfThread` 已完成；`Arrow/Bolt` 为单件（区别于 `ArrowsAndBolts` Bundle，Bundle 已完成）。

**键名建议：** `item.trade.name.*`（与已有符文工具键同前缀）。

---

### T3 — Magical/Tools 魔法版兼容工具名（8 条）

**优先级：P2**（由铁匠/裁缝工作室产出，玩家使用频率中等）

> 这是 `Scripts/Items/Trades/Magical/Tools/` 下的 **通用魔法版本工具**（不同于技能专属符文工具）。它们有 `IsContentLocalized` 但无 `DisplayNameLocalizationKey`。

| 文件（相对 `Scripts/Items/Trades/Magical/Tools/`） | 英文 Name | 状态 |
|---|---|---|
| `RunicHammer.cs` | `smith hammer` | ❌ |
| `RunicFletching.cs` | `bowyer tools` | ❌ |
| `RunicSewingKit.cs` | `sewing kit` | ❌ |
| `RunicTinker.cs` | `tinker tools` | ❌ |
| `RunicSaw.cs` | `woodworking tools` | ❌ |
| `RunicLeatherKit.cs` | `tanning kit` | ❌ |
| `RunicUndertaker.cs` | `undertaker tools` | ❌ |
| `RunicScales.cs` | `scaling tools` | ❌ |

**键名建议：** `item.trade.name.magical.runic.*`（区别于技能特定符文工具的 `item.trade.name.runic.*`）。

---

### T4 — 炼金容器动态名（1 条，复杂）

**优先级：P2**

| 文件 | 情形 | 现状 |
|---|---|---|
| `Trades/Alchemy/CrystallineJar.cs` | `Name = "crystalline flask"` 状态 | ✅ 已有 `AddNameProperty` → `item.trade.crystalline.flask` |
| | `Name = "flask of " + iJar.Name`（装填后动态名）| ❌ 仍为拼接英文字符串 |
| | `Name = "flask of holy water"` | ❌ 仍为硬编码英文 |

**建议：** 为 `flask of holy water` 增加 `item.trade.name.flask.holy.water` 键；对 `flask of {substance}` 需引入 `item.trade.name.flask.of` 格式模板 + substance 名称子键（或直接在 `AddNameProperty` 中判断 Name 前缀走 `ResolveFormatByKey`）。可推迟至 P3。

---

### T5 — 特殊装饰物品名（~20 条）

**优先级：P3**（主要用于家居装饰，玩家检视时可见英文名）

| 分类 | 文件路径（相对 `Scripts/Items/Special/`） | 英文 Name（或模式） | 状态 |
|---|---|---|---|
| **Evil Home Decor 收纳盒** | `Evil Home Decor Collection/BoneTable.cs` | `box containing a table of bones` | ❌ |
| | `BoneThrone.cs` | `box containing a throne of bones` | ❌ |
| | `BoneCouch.cs` | `box containing a couch of bones` | ❌ |
| | `UnsettlingPortrait.cs` | `box containing an unsettling portrait` | ❌ |
| | `CreepyPortrait.cs` | `box containing a creepy portrait` | ❌ |
| | `DisturbingPortrait.cs` | `box containing a disturbing portrait` | ❌ |
| | `AwesomeDisturbingPortrait.cs` | `box containing a disturbing portrait` | ❌ |
| | `BedOfNails.cs` | `box containing a bed of nails` | ❌ |
| | `HauntedMirror.cs` | `box containing a haunted mirror` | ❌ |
| | `SacrificialAltar.cs` | `box containing a sacrificial altar` | ❌ |
| **塔罗牌** | `Rares/TarotCards/DecoTarot*.cs`（×9 文件） | `tarot cards`（同一名） | ❌ 需 1 个键 |
| **花卉** | `Rares/Flowers/DecoFlower*.cs`（×2 文件） | `white roses` | ❌ |
| | `Rares/Flowers/DecoRoseOfTrinsic*.cs`（×3 文件） | `velvet rose` | ❌ |
| **其他** | `Rares/Containers/HugeWaterTub.cs` | `huge tub of water` | ❌ |
| | `MinotaurHedge.cs` | （需核查） | ❓ |
| | `TormentedChains.cs` | （需核查） | ❓ |
| | `WindSpirit.cs` | （需核查） | ❓ |
| | `DragonOrbStatue.cs` | （需核查） | ❓ |
| | `WizardsStatue.cs` | （需核查） | ❓ |
| | `Special/Items/BloodyPentagram.cs` | （需核查） | ❓ |

**键名建议：** `item.special.decor.*`（装饰盒类），`item.special.rares.*`（稀有装饰）。

---

### T6 — 魔法神器名（Magical/Artifacts 全目录，~110 条）

**优先级：P2**（可掉落/收藏神器，玩家展示/交易时均可见英文名）

> **（2026-05-17 更新）** 本批 **`Magical/Artifacts`** 具体子类已由 **§8.3 Fix B2** 工具批次接入 **`DisplayNameLocalizationKey`**（键前缀 **`item.magical.artifact.*`**）及 **`equipment-properties.json`** 成对 EN/zh-Hans；下表仍列子目录与示例，便于人工抽查译名与专有名词格式。

**子目录及代表性示例：**

| 子目录 | 代表物品（英文 Name） | 文件数 |
|---|---|---|
| `Artifacts/Weapons/Swordsmanship/` | Excalibur, Cold Blood, Blade Dance … | ~10 |
| `Artifacts/Weapons/Axes/` | Zyronic Claw, Quell, Axe of the Minotaur … | ~5 |
| `Artifacts/Weapons/Bows/` | Frostbringer, Nox Bow, Bow of the Phoenix … | ~8 |
| `Artifacts/Weapons/Fencing/` | Fang of Ractus, Raed's Glory, The Taskmaster … | ~6 |
| `Artifacts/Weapons/Bludgeoning/` | Bonesmasher, Arctic Death Dealer … | ~5 |
| `Artifacts/Weapons/Staffs/` | Phantom Staff, Wrath of the Dryad … | ~3 |
| `Artifacts/Armor/` (多子目录) | Helm of Brilliance, Violet Courage … | ~35 |
| `Artifacts/Clothing/` | Robe of Teleportation, Crown of Tal'Keesh … | ~12 |
| `Artifacts/Jewelry/` | Bracelet of the Vile, Ring of Health … | ~8 |
| `Artifacts/Trinkets/` | Bloodwood Spirit, Shimmering Talisman … | ~3 |
| `Artifacts/Shields/` | Achilles Shield … | ~2 |
| `Artifacts/Quivers/` | Quiver of Ice, Quiver of Elements … | ~3 |
| `Artifacts/Books/` | Hydros Lexicon, Lithos Tome … | ~6 |
| `Artifacts/Instruments/` | Iolo's Lute, Gwenno's Harp … | ~2 |
| `Artifacts/Minor/` | Gem of Seeing, Pandora's Box, Everlasting Loaf … | ~5 ✅（部分 `IsContentLocalized` 已有 `item.*` 键） |
| `Artifacts/Offhands/` | Grim Reaper's Lantern, Candles … | ~3 |

**实现建议：** 批量添加 `item.magical.artifact.*` 键（使用神器英文名 PascalCase 派生），统一在 `equipment-properties.json` 中维护。可按子目录分批次完成，每批同步翻译。

---

### T7 — 魔法奎弓及武器重命名工具

**优先级：P3**

| 文件 | 英文 Name | 状态 |
|---|---|---|
| `Magical/MagicQuiver.cs` | `quiver` | ❌ |
| `Magical/WeaponRenamingTool.cs` | （需核查具体名） | ❓ |

---

<a id="8-equipment-opl-name-layer-books-instruments"></a>

## 8. 装备 OPL 主名、装备槽位、乐器与书本（根因 + 工单）

> **整理日期：** 2026-05-17（**A5 / B1b / BaseBook** 静态书标题哈希解析已更新）

> **背景：** zh-Hans 账号下，装备 **属性行** 已大量走 `StringCatalog`/`AddLocalizedProperty`，但 **OPL 第一行物品名**、**「装备位置」取值**、**乐器/书本主名** 仍常显示英文。本节记录 **根因** 与 **分步工单**（Fix A / B），与 **§7** 非装备 `item.trade.*` 扫描互补。

---

### 8.1 根因分析

| 现象 | 根因（代码要点） |
|------|------------------|
| **装备 OPL 主名为英文** | **`BaseWeapon` / `BaseArmor` / `BaseClothing` / `BaseInstrument`** 的 **`AddNameProperty`** 不检查 **`BuildingPropertyListLocale`**：在常见分支中直接 **`list.Add(LabelNumber)`**（客户端 cliloc，英文）或 **`list.Add(Name)`**（英文构造名）。与已本地化的 **`GetProperties`** 形成 **两套管道不一致**。 |
| **「装备位置：right hand」类英文** | （1）**`Item.EquipLayerName(Layer)`**（`System/Item.cs`）返回硬编码英文字符串（`"Right Hand"`、`"Left Hand"`、`"Boots"` …）。（2）**`BaseWeapon`** 已用 **`AddLocalizedProperty(list, "prop.equipped.at", EquipLayerName(Layer))`**，故标签已是中文（如 `prop.equipped.at` →「装备位置：{0}」），但 **`{0}` 仍为英文**。（3）**`BaseArmor` / `BaseClothing` / `BaseInstrument` / `BaseTool` / `BaseHarvestTool` / `Spellbook` 等** 仍 **`list.Add(1061182, EquipLayerName(Layer))`**，无 locale 分支时整行随客户端英文。 |
| **乐器主名为英文** | 与武器同构：`BaseInstrument.AddNameProperty` 直接 **`LabelNumber` / `Name`**，未接 **`DisplayNameLocalizationKey`** / 双语首行。 |
| **书本主名为英文** | **`BaseBook.AddNameProperty`**：有标题时 **`list.Add(m_Title)`**，标题多为脚本构造器中的英文字符串；无标题时走 **`base.AddNameProperty`**，仍可能 cliloc/英文名。 |

**落实情况（2026-05-17）：**

- **上表第 2 行（装备槽位取值英文）：已按 §8.2 Fix A 落地。** 实现 **`Item.EquipLayerKey(Layer)`**（返回 `prop.layer.*` shotkey）、**`Item.AddEquipLayerProperty(ObjectPropertyList)`**（`BuildingPropertyListLocale != null` 时用 **`ResolvePropertyText(EquipLayerKey)`** 作为 **`prop.equipped.at`** 的 `{0}`；否则仍 **`1061182` + `EquipLayerName`**）。已在 **`en/` / `zh-Hans/equipment-properties.json`** 增加 **`prop.layer.right.hand` … `prop.layer.trinket`** 等条目。替换呼叫点的类型包括：**`BaseWeapon`、`BaseArmor`、`BaseClothing`、`BaseInstrument`、`BaseTrinket`、`BaseTool`、`BaseHarvestTool`、`BaseQuiver`、`Spellbook`（`Layer == Trinket`）、`MagicRuneBag`、`BaseEquipableLight`**。  
- **上表第 1、3、4 行：** **Fix B1 / B1b 已落地** — `Item.TryAddLocalizedDisplayNameProperty`（无材质时单一类型名；有材质 / exceptional 时由各 **`Base*`** 传入 **`CraftResources.GetDisplayNameLocalized`** 与 **`GetClilocLowerCaseName` 判定**，复合首行走 **`prop.item.opl.firstline.*`**）。**`BaseWeapon`** 首行不含 exceptional（与 vanilla 一致）、早退后仍处理镌刻 **`1062613`**。**`BaseBook`**：非自写书且双语 OPL 时标题走 **`StringCatalog.TryResolve(BuildingPropertyListLocale, m_Title)`**（与 extractor 哈希表一致；自写书标题不强制目录化）。玩家可见中文主名仍依赖 **Fix B2** 为各类物品配置 **`item.*` 键**。

**范围说明：**

- **玩家自写书**（`Writable`、标题为玩家输入）：不应强行目录化标题；可仅处理 **无标题默认** / **系统静态书**。
- **神器/自定义 `Name`**：与 §7.T6 一致，需 per-class **`DisplayNameLocalizationKey`** + **`equipment-properties.json`**。

---

### 8.2 工单 Fix A — 装备槽位字符串目录化（优先 P1）

> **状态（2026-05-17）：** A1–A5 已落地。**A5** — `ItemProps.ItemProperties` 三参重载接受 **`IAccount`**，**`Equipment:`** 行走 **`prop.itemprops.equipment.line`** + **`Item.GetEquipLayerLabelForAccount`**；**`BlackMarket` / `CraftSystem.SetDescription`** 传入玩家 **`Account`**。

**目标：** 任意走 **`prop.equipped.at`** 或等价属性的行，**`{0}` 为中文槽位名**；未做 locale 分支的基类改为与 **`BaseWeapon`** 一致的 **`AddLocalizedProperty`** 模式。

| # | 动作 | 文件 / 数据 |
|---|------|-------------|
| A1 | 新增 **稳定键生成**：例如 **`Item.EquipLayerKey(Layer)`** → `"prop.layer.right.hand"` 等（与 `EquipLayerName` 分支一一对应，返回 shotkey 而非英文）。 | `World/Source/System/Item.cs` |
| A2 | 在 **`en/`、`zh-Hans/equipment-properties.json`** 各增 **`prop.layer.*`**（约 12 条：右手、左手、双手、靴、腿、胸、头等 —— 与现有 `EquipLayerName` 返回值一一对照）。 | `World/Data/Localization/en|zh-Hans/equipment-properties.json` |
| A3 | **`BaseWeapon`**：将 **`AddLocalizedProperty(..., "prop.equipped.at", EquipLayerName(Layer))`** 改为第二参使用 **`ResolvePropertyText(EquipLayerKey(Layer))`**（或等效 API，确保 `{0}` 已解析为 zh）。 | `World/Source/Scripts/Items/Weapons/BaseWeapon.cs` |
| A4 | **`BaseArmor`、`BaseClothing`、`BaseInstrument`、`BaseTool`、`BaseHarvestTool`** 及 **`Spellbook`、可穿戴灯、符文袋等** 凡 **`list.Add(1061182, EquipLayerName(...))`** 之处：在 **`BuildingPropertyListLocale != null`** 时改为 **`AddLocalizedProperty(list, "prop.equipped.at", ResolvePropertyText(EquipLayerKey(Layer)))`**。 | 各对应 `.cs`（可用 `rg "1061182, EquipLayerName"` 全库清点） |
| A5 | **`ItemProperties.cs`** 等若拼接 `"Equipment: " + EquipLayerName`** 的 **网页/调试 HTML**，视需要同样走目录化（低优先，避免与游戏内 OPL 不一致）。 | `World/Source/Scripts/System/Misc/ItemProperties.cs`（可选） |

**完成标准：** zh-Hans 下 OPL 不出现 **`Right Hand` / `Left Hand`** 等裸英文槽位；英文服行为不变。

---

### 8.3 工单 Fix B — 装备 / 乐器 / 静态书本 **OPL 第一行** 目录化

**Fix B1 — 基类闸门（P1，改动面小、应先落地）**

> **状态（2026-05-17）：** **`Item.TryAddLocalizedDisplayNameProperty`** 含 **B1b** 重载（材质 / exceptional 复合首行）；**`BaseWeapon`**（早退后仍处理 **`m_EngravedText`**）、**`BaseArmor`、`BaseClothing`、`BaseInstrument`、`BaseTrinket`** 在 **`AddNameProperty`** 开头调用。**`BaseBook`** 静态书标题见下文与 §8.1「落实情况」。

| # | 动作 | 说明 |
|---|------|------|
| B1.1 | **`TryAddLocalizedDisplayNameProperty(list)`** → 内部走无材质分支（复合首行关）。各装备基类先算 **`hm` / `mat`** 再调 **五参重载**（见 B1.2）。 | 子类未 override **`DisplayNameLocalizationKey`** 时返回 false。 |
| B1.2 **（B1b）** | **`TryAddLocalizedDisplayNameProperty(list, materialDisplayName, hasMaterialPrefix, exceptionalOnFirstLine, isExceptional)`** + **`prop.item.opl.firstline.*`** / **`prop.item.opl.name.exceptional`**。**`BaseTool` / `BaseHarvestTool` / `BaseRunicTool`** 双语 OPL 且带材质时首行同模板。 | 取代 cliloc **1053099 / 1053100 / 1050040** 在双语首行的英文材质 cliloc。 |

**Fix B2 — 子类批量 `DisplayNameLocalizationKey` + JSON（P2，量大）**

> **状态（2026-05-17）：** 已用维护脚本 **`World/Source/Tools/emit_equipment_opl_display_keys.py`** 对下列目录 **首批全量** 写入 **`DisplayNameLocalizationKey`** 并合并 **`en/`、`zh-Hans/equipment-properties.json`**：**`Scripts/Items/Weapons`、`Armor`、`Clothing`、`Instruments`、`Magical/Artifacts`**（**627** 个具体类；再次运行会跳过已有 override 的类）。键命名：**`item.equip.weapon.*` / `item.equip.armor.*` / `item.equip.clothing.*` / `item.equip.instrument.*` / `item.magical.artifact.*`**（神器类名 slug 由 `Artifact_` 前缀剥离后小写、下划线转点）。中文主名为脚本规则表 + 构造器 **`Name = "...`** 解析生成，**仍须按游玩体验与术语表人工 spot-check**；新增装备类可复制脚本模式或补表后重跑（仅新增项）。

| 类别 | 键前缀建议 | 批量策略 |
|------|------------|----------|
| 标准武器（`Name == null`，靠 `LabelNumber`） | `item.equip.weapon.*` | **首批已完成**（脚本）；新增武器 → 补跑脚本或手写键 |
| 标准护甲 / 衣物 | `item.equip.armor.*` / `item.equip.clothing.*` | 同上 |
| 神器 / 固定 `Name` | `item.magical.artifact.*` | **首批已完成**（脚本）；见 **§7.T6** 抽查清单 |
| 乐器 | `item.equip.instrument.*` | **首批已完成**（脚本） |
| 系统静态书（构造器固定 `Title`） | `item.book.*` 或沿用 books 流水线 | **未纳入本脚本**；**玩家自写书标题** 不在此列 |

**数据：** 全部写入 **`en/`、`zh-Hans/equipment-properties.json`**（`item.*`），zh 遵守 **`AGENTS.md` §3.5** 专有名词格式。

**完成标准：** zh-Hans 下装备/乐器 **OPL 第一行主名** 已 **`item.*`** 覆盖的为中文；**非自写书** 标题若在目录中有对应 **哈希** 译文则显示中文；**`LabelNumber` / `Name` 保留** 供存档、脚本、`switch(Name)`。批量脚本书键仍见上表「系统静态书」。

> **神器 zh 专名精修（2026-05-17）：** 对 **`item.magical.artifact.*`** 中仍为 **「英文（英文）」** 兜底行，已用 **`World/Source/Tools/generate_artifact_zh_core.py`** 生成 **`artifact_zh_core.json`**（EN 显示名 → 中文核心，无括号），再由 **`apply_artifact_zh_core.py`** 写回 **`zh-Hans/equipment-properties.json`**（格式 **`中文（English）`**，英文段与 **`en/equipment-properties.json`** 一致）。修改译法时优先改生成器与 **`PATCH` / 词表片段**，重跑 **`generate_artifact_zh_core.py`** 与 **`apply_artifact_zh_core.py`**。

---

### 8.4 验证与依赖

- 改 C# 后：**编译** `World/Source/Tools/compile-world-*.sh`。  
- 只增 **`item.*` / `prop.layer.*`**：`sync_localization_glossary.py --check`。  
- 若引入 **新英文 UI 字面量**（非 shotkey）：再走 **`build_localization_strings.py --no-translate`**（本节工单以 shotkey 为主，预计不需要）。

**Git 提交拆分建议（B1 vs B2 vs 神器文案）：**

| 提交 | 宜含路径 / 说明 |
|------|------------------|
| **Fix B1** | **`World/Source/System/Item.cs`**（`TryAddLocalizedDisplayNameProperty` / `DisplayNameLocalizationKey` 相关）+ 各 **`BaseWeapon` / `BaseArmor` / …** 若与 B1 同 PR 一并改动。 |
| **Fix B2** | `Scripts/Items/Weapons` 等五类目录下已加键的 `.cs`，`en/` 与 `zh-Hans/equipment-properties.json`（批量键），`emit_equipment_opl_display_keys.py`。 |
| **神器 zh 精修** | `zh-Hans/equipment-properties.json`（神器专名行）、`artifact_zh_core.json`、`generate_artifact_zh_core.py`、`apply_artifact_zh_core.py`。 |

仅提 B2 时：请勿把未审过的 **B1 `Item.cs`** 卷进同一提交；反之亦然。神器译名迭代可单独小提交，便于文案审阅与回滚。

## 6. 附录：非装备物品分类总表

### 按中文化策略分类

| 策略 | 适用物品 | 数量 |
|------|---------|------|
| **A - OPL 属性中文化** | 装备基类（HasAttributes） | 9个基类 ✅ |
| **B - 装备子类 cliloc 修复** | 额外 GetProperties 覆盖 | 5个文件 ✅ |
| **C - Gift 系统中文化** | 附魔属性 | 19个文件 ✅ |
| **D - Level 系统中文化** | 等级/经验值属性 | 25个文件 ✅ |
| **E - 非装备 AddNameProperties** | 描述性文本的 Item 子类 | ~63个文件；Trades 的 **`SendMessage` 已收敛**，其余面见 **§5.3b 工单** |

### 策略 E 优先级建议

| 优先级 | 分类 | 理由 | 文件数 |
|--------|------|------|--------|
| P0 | Fishing 渔获/渔网类 | 高频交互 | ~15 |
| P0 | Cartography 地图类 | 高频交互 | ~6 |
| P1 | Thieving 钥匙类 | 中等频率 | ~3 |
| P1 | Magical 魔法物品 | 中等频率 | ~18 |
| P2 | Special 特殊物品 | 低频交互 | ~9 ✅ |
| P2 | Trades 其他技能 | 低频交互 | ~12 |

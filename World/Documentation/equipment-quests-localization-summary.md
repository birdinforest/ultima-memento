# 装备属性与任务脚本中文化工作总结

> **更新日期：** 2026-05-10
> **涉及分支：** `feat/item-localization-properties` → 远程 `localization`
> **提交范围：** `5d315a61` ~ `ad20fbb0`

---

## 目录

1. [工作概述](#1-工作概述)
2. [物品属性中文化（BaseQuiver）](#2-物品属性中文化basequiver)
3. [物品属性颜色映射](#3-物品属性颜色映射)
4. [供应商买卖列表中文化](#4-供应商买卖列表中文化)
5. [任务脚本中文化](#5-任务脚本中文化)
6. [Glossary 更新](#6-glossary-更新)
7. [完整文件变更清单](#7-完整文件变更清单)
8. [提交记录](#8-提交记录)

---

## 1. 工作概述

本次中文化工作覆盖以下四个主要方面：

| # | 任务 | 状态 | 提交 |
|---|------|------|------|
| 1 | 装备属性中文化 — `BaseQuiver.cs` | ✅ | `5d315a61` |
| 2 | 装备属性颜色映射 — `Item.cs` PropertyColorMap | ✅ | `5d315a61` |
| 3 | 供应商买卖列表属性中文化 — `BaseVendor.cs` | ✅ | `5d315a61` |
| 4 | 任务脚本中文化 — 多个 Quest 目录 | ✅ | `714b1505`, `ad20fbb0` |

---

## 2. 物品属性中文化（BaseQuiver）

### 背景

`BaseQuiver.cs` 是唯一缺少双语 OPL 支持的装备基类。其他装备基类（`BaseWeapon`、`BaseArmor`、`BaseClothing`、`BaseTrinket`、`BaseInstrument`、`BaseHarvestTool`、`BaseTool`、`Special`）均已实现 `IsContentLocalized => true` 并使用 `AddLocalizedProperty` 模式。

### 修改内容

**文件：** `World/Source/Scripts/Items/Quivers/BaseQuiver.cs`

- 添加 `using Server.Localization;`
- 添加 `public override bool IsContentLocalized => true;`
- 重构 `GetProperties` 方法，对所有箭袋特有属性采用双语路径：

  | 属性 | 英文 Shotkey | 英文示例 | 中文示例 |
  |------|-------------|---------|---------|
  | 弹药类型/数量 | `prop.ammo.arrows` / `prop.ammo.bolts` | "Ammo: 50/100 arrows" | "弹药：50/100 箭矢" |
  | 降低弹药消耗 | `prop.lower.ammo.cost` | "Lower Ammo Cost 20%" | "降低弹药消耗 20%" |
  | 内容 | `prop.contents` | "Contents: 5/10 items, 2.5/5 stones" | "内容：5/10 件物品，2.5/5 石" |
  | 重量减少 | `prop.weight.reduction` | "Weight reduction: 50%" | "重量减少：50%" |
  | 品质 | `prop.quality` | (已有) | (已有) |
  | 打造者 | `prop.crafted.by` | (已有) | (已有) |

### 数据文件

**英文源：** `World/Data/Localization/en/equipment-properties.json`

```json
"prop.ammo.arrows": "Ammo: {0}/{1} arrows",
"prop.ammo.bolts": "Ammo: {0}/{1} bolts",
"prop.lower.ammo.cost": "Lower Ammo Cost {0}%",
"prop.contents": "Contents: {0}/{1} items, {2}/{3} stones",
"prop.weight.reduction": "Weight reduction: {0}%"
```

**中文翻译：** `World/Data/Localization/zh-Hans/equipment-properties.json`

```json
"prop.ammo.arrows": "弹药：{0}/{1} 箭矢",
"prop.ammo.bolts": "弹药：{0}/{1} 弩箭",
"prop.lower.ammo.cost": "降低弹药消耗 {0}%",
"prop.contents": "内容：{0}/{1} 件物品，{2}/{3} 石",
"prop.weight.reduction": "重量减少：{0}%"
```

---

## 3. 物品属性颜色映射

**文件：** `World/Source/System/Item.cs` — `PropertyColorMap`

新增以下 shotkey 的颜色映射：

| Shotkey | 颜色 | HEX |
|---------|------|-----|
| `prop.skill.fist.fighting` | 灰色 | `808080` |
| `prop.ammo.arrows` | 灰色 | `808080` |
| `prop.ammo.bolts` | 灰色 | `808080` |
| `prop.lower.ammo.cost` | 绿色 | `2E8B57` |
| `prop.contents` | 灰色 | `808080` |
| `prop.weight.reduction` | 灰色 | `808080` |

---

## 4. 供应商买卖列表中文化

**文件：** `World/Source/Scripts/Mobiles/Base/BaseVendor.cs`

### 修改内容

1. 添加 `using Server.Localization;`
2. 在 `VendorBuy` 方法中获取买家语言环境
3. 对供应商库存物品和玩家出售物品的 OPL 显示进行双语判断

### 核心逻辑

```csharp
string lang = AccountLang.GetLanguageCode( from?.Account );
string locale = AccountLang.IsChinese( lang ) ? "zh" : "en";

// 对每个物品判断是否启用本地化
if ( item.IsContentLocalized )
    opls.Add( item.GetLocalizedPropertyList( locale ) );
else
    opls.Add( ((Item)disp).PropertyList );
```

### 工作原理

- 当玩家打开供应商买卖界面时，系统检测玩家账户的语言设置
- 如果物品启用了 `IsContentLocalized`（即装备基类），则使用 `GetLocalizedPropertyList(locale)` 获取对应语言的属性列表
- 否则回退到传统的 `PropertyList`（cliloc 模式）
- 这确保英文玩家看到 cliloc 编号解析的英文文本，中文玩家看到 shotkey 解析的中文文本

---

## 5. 任务脚本中文化

### 5.1 中文化模式

Quest 脚本中的玩家可见消息使用以下模式进行本地化：

```csharp
// 简单消息
from.SendMessage( StringCatalog.Resolve( from.Account, "Your message here." ) );

// 带格式化参数的消息
from.SendMessage( StringCatalog.ResolveFormat( from.Account, "Found {0} items.", count ) );

// Overhead 消息
from.LocalOverheadMessage( MessageType.Emote, 1150, true,
    StringCatalog.Resolve( from.Account, "You found the item!" ) );
```

### 5.2 第一批：主要任务脚本（commit `714b1505`）

| 目录 | 本地化文件 | 说明 |
|------|-----------|------|
| **Assassin/** | `AssassinBox.cs`, `AssassinFunctions.cs` | 刺客任务全部消息 |
| **Codex/** | `ApproachVoid.cs`, `CodexWisdom.cs`, `CubeOnCorpse.cs`, `DoorCodex.cs`, `VortexCube.cs` | 法典任务 |
| **Epic/** | `CourierMail.cs` | 史诗任务邮件 |
| **Fishing/** | `FishingQuestFunctions.cs` | 钓鱼任务 |
| **Frankenstein/** | `EmbalmingFluid.cs`, `FrankenFighter.cs`, `FrankenItem.cs`, `FrankenJournal.cs`, `FrankenJournalInBox.cs`, `FrankenPorterItem.cs` | 弗兰肯斯坦任务消息 |
| **Golems/** | `BottleOil.cs`, `GolemFighter.cs`, `GolemManual.cs`, `GolemPorterItem.cs` | 魔像任务 |
| **Hoard/** | `HoardPile.cs` | 宝藏堆 |
| **Jester/** | `JokeBook.cs` | 小丑任务 |
| **Magic Pools/** | `MagicPool.cs` | 魔法池 |
| **Major/** | `QuestTake.cs`, `QuestTome.cs` | 主要任务 |
| **Museum/** | `Museum.cs`, `MuseumBook.cs`, `MuseumBookGump.cs` | 博物馆任务 |
| **Pagan/** | `ApproachObsidian.cs`, `ObeliskOnCorpse.cs`, `ObeliskTip.cs`, `ObsidianGate.cs`, `PaganArtifact.cs`, `PaganBase.cs` | 异教任务 |
| **Prisoners/** | `Prisoner.cs` | 囚犯任务 |
| **Robots/** | `Robot.cs`, `RobotBatteries.cs`, `RobotItem.cs`, `RobotSchematics.cs`, `RobotSheetMetal.cs` | 机器人任务 |
| **Serpents/** | `BaneBase.cs`, `LanternOfDiscipline.cs`, `OrbOfLogic.cs`, `ScalesOfEthicality.cs`, `SerpentSpawners.cs` | 蛇之任务 |
| **Shadowlords/** | `BalinorTeleporter.cs`, `BellOfCourage.cs`, `BookOfTruth.cs`, `CandleOfLove.cs`, `FlamesBase.cs`, `GemImmortality.cs`, `ShardOfCowardice.cs`, `ShardOfFalsehood.cs`, `ShardOfHatred.cs` | 暗影领主任务 |
| **Thief/** | `Coffer.cs`, `StealBase.cs`, `HayCrate.cs`, `HollowStump.cs`, `ThiefNote.cs` | 盗贼任务消息 |
| **Underworld/** | `RuneStoneGate.cs`, `SkullOfBaron Almric.cs` | 地下世界任务 |
| **Root** | `GygaxStatue.cs`, `HelpMessage.cs`, `NoticeClue.cs`, `QuestChests.cs`, `QuestTeleporter.cs`, `ScrollClue.cs`, `SomeRandomNote.cs` | 根级别任务文件 |

同时添加了对应的英文和中文 JSON 条目到 `scripts-quests.json` 和 `scripts-items.json`。

### 5.3 第二批：剩余目录（commit `ad20fbb0`）

| 目录 | 本地化文件 | 字符串数 |
|------|-----------|---------|
| **Runes/** | `RuneBox.cs`, `RunesBase.cs` | 7 条 |
| **Search/** | `SearchBase.cs` | 24 条 |

### 5.4 无需中文化的文件

经过逐一核查，以下目录中的文件不含玩家可见字符串（`SendMessage`、`Say`、`PrivateOverheadMessage` 等），未做修改：

| 目录 | 文件 | 原因 |
|------|------|------|
| **Bards Tale/** | `MangarsRewards.cs` | 仅为物品定义（Robe/FeatheredHat 子类），消息通过 OPL 属性系统处理 |
| **Core/** | 9 个框架文件 | 框架基础设施，使用 `TextDefinition`（cliloc 编号），无玩家消息 |
| **Summon/** | 5 个文件 | 仅物品数据分配和序列化，无消息调用 |
| **Runes/** | `RunesBaseEmpty.cs` | 仅 `Name = "Pedestal"`，无消息调用 |
| **Pagan/** | `PaganBaseEmpty.cs` | 仅物品名称 |
| **Shadowlords/** | `FlamesBaseEmpty.cs` | 仅物品名称 |
| **Serpents/** | `BaneBaseEmpty.cs`, `BlackrockSerpents.cs` | 仅物品名称/OPL 标签 |
| **Thief/** | `StealBag.cs`, `StealBaseEmpty.cs`, `StealBox.cs`, `StealMetalBox.cs` | 仅物品名称属性 |
| **Golems/** | `GolemPorter.cs` | 仅 `Name`/`Title`/序列化 |
| **Epic/** | `EpicGump.cs` | 已通过 `QuestCompositeResolver.ResolveComposite()` 处理 |
| **Hoard/** | `HoardSpawner.cs` | 仅物品名称 |
| **Underworld/** | `UnderworldTeleporter.cs` | 消息来自 GM 配置属性 |
| **Frankenstein/** | 肢体/核心部件文件 | 仅物品名称；有消息的文件已本地化 |
| **Robots/** | 组件文件 | 仅物品名称；有消息的文件已本地化 |
| **Root** | `QuestGlow.cs`, `QuestStories.cs`, `QuestTransporter.cs`, `Quests.cs`, `TriggerTile.cs` | 无消息调用或消息来自配置 |

---

## 6. Glossary 更新

**文件：** `World/Data/Localization/glossary-approved-zh.json`

新增术语条目：

| 英文 | 中文 | 类别 | 说明 |
|------|------|------|------|
| Arrow | 箭矢 | item | 弓用弹药，弹药量显示中译为"箭矢" |
| Bolt | 弩箭 | item | 弩用弹药，与箭矢区分 |
| Lower Ammo Cost | 降低弹药消耗 | concept | 箭袋特殊属性，按百分比减少弹药消耗 |
| Weight Reduction | 重量减少 | concept | 箭袋属性，减少所携带弹药的重量 |

---

## 7. 完整文件变更清单

### 修改的 C# 源文件（11 个）

| 文件 | 提交 |
|------|------|
| `World/Source/System/Item.cs` | `5d315a61` |
| `World/Source/Scripts/Items/Quivers/BaseQuiver.cs` | `5d315a61` |
| `World/Source/Scripts/Mobiles/Base/BaseVendor.cs` | `5d315a61` |
| `World/Source/Scripts/Engines and Systems/Quests/Runes/RuneBox.cs` | `ad20fbb0` |
| `World/Source/Scripts/Engines and Systems/Quests/Runes/RunesBase.cs` | `ad20fbb0` |
| `World/Source/Scripts/Engines and Systems/Quests/Search/SearchBase.cs` | `ad20fbb0` |
| (80+ quest files 在 `714b1505`) | `714b1505` |

### 修改的 JSON 数据文件

| 文件 | 说明 |
|------|------|
| `World/Data/Localization/en/equipment-properties.json` | 新增强装备属性 shotkey |
| `World/Data/Localization/zh-Hans/equipment-properties.json` | 新增对应中文翻译 |
| `World/Data/Localization/en/scripts-quests.json` | 新增任务脚本英文条目 |
| `World/Data/Localization/zh-Hans/scripts-quests.json` | 新增任务脚本中文翻译 |
| `World/Data/Localization/en/scripts-items.json` | 新增物品相关英文条目 |
| `World/Data/Localization/zh-Hans/scripts-items.json` | 新增物品相关中文翻译 |
| `World/Data/Localization/glossary-approved-zh.json` | 更新 glossary |

---

## 8. 提交记录

```
5d315a61 feat: Add Chinese localization for item properties and vendor buy list
  - BaseQuiver: IsContentLocalized + GetProperties refactor
  - Item.cs: PropertyColorMap additions
  - BaseVendor: localized OPL in buy/sell gumps
  - equipment-properties.json (en/zh-Hans)
  - glossary-approved-zh.json

714b1505 feat: Add Chinese localization for quest scripts and interactive messages
  - 80+ quest .cs files localized across 15 directories
  - scripts-quests.json (en/zh-Hans)
  - scripts-items.json (en/zh-Hans)

ad20fbb0 feat: Add Chinese localization for quest scripts (Runes, Search)
  - RuneBox.cs, RunesBase.cs, SearchBase.cs
  - 53 insertions, 49 deletions
```

---

## 附录 A：技术架构参考

- [双语 OPL 本地化方案](bilingual-opl-localization.md) — OPL 双缓存机制详解
- [本地化开发者指南](localization-developer-guide.md) — 添加新文本的工作流
- [完整覆盖路线图](localization-complete-coverage-roadmap.md) — 全局覆盖规划
- [Glossary 同步工作流](zh-localization-glossary-sync-workflow.md) — 术语规范化流程

### 核心 API

| API | 位置 | 用途 |
|-----|------|------|
| `StringCatalog.Resolve(account, text)` | `StringCatalog.cs` | 按账户语言解析英文文本 |
| `StringCatalog.ResolveFormat(account, format, args...)` | `StringCatalog.cs` | 解析带参数的格式化文本 |
| `StringCatalog.TryResolve(lang, text)` | `StringCatalog.cs` | 按语言代码解析，失败返回 null |
| `AccountLang.GetLanguageCode(account)` | `AccountLang.cs` | 获取账户语言代码 |
| `AccountLang.IsChinese(lang)` | `AccountLang.cs` | 判断是否为中文 |
| `AddLocalizedProperty(list, shotkey, args...)` | `Item.cs` | 添加本地化装备属性到 OPL |
| `IsContentLocalized` | `Item.cs` | 装备基类重写为 true 启用 OPL 双语 |

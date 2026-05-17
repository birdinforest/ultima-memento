# 待中文化物品清单

> **更新日期：** 2026-05-17
> **说明：** 本文档记录已完成中文化的装备属性系统之外，尚未开始中文化的物品类别。用于后续任务参考和范围规划。

---

## 目录

1. [SendMessage 与中文字符串（AI 流水线）](#sendmessage--中文字符串ai-流水线)
2. [任务（待办）：硬编码 SendMessage / Say 全库清查](#任务待办硬编码-sendmessage--say-全库清查)
3. [已完成中文化的装备基类](#1-已完成中文化的装备基类)
4. [已修复的装备子类未保护 cliloc](#2-已修复的装备子类未保护-cliloc)
5. [Phase 3：Gift 附魔系统（已完成）](#3-phase-3gift-附魔系统已完成)
6. [Phase 4：Level 经验装备系统（已完成）](#4-phase-4level-经验装备系统已完成)
7. [待后续中文化的非装备物品](#5-待后续中文化的非装备物品)
8. [附录：非装备物品分类总表](#6-附录非装备物品分类总表)

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

### 5.1 Magical 目录 - 魔法物品（18个文件）

| # | 文件 | Class | 英文文本 |
|---|------|-------|---------|
| 1 | `Items/Magical/SoulOrb.cs` | `SoulOrb` | "Contains vampire blood for..."、"Contains genetic patterns for..."、"Contains the Soul of..." |
| 2 | `Items/Magical/LuckyHorseShoes.cs` | `LuckyHorseShoes` | "Adds up to 100 Luck To An Item" |
| 3 | `Items/Magical/RuneOfVirtue.cs` | `RuneOfVirtue` | "Rune for..." |
| 4 | `Items/Magical/Moonstone.cs` | `MoonStone` | OPL：`prop.magical.moonstone`；使用失败：`prop.magical.moonstone.gate.inert` + `StringCatalog.ResolveByKey`（见本文 SendMessage 节） |
| 5 | `Items/Magical/SlayerDeed.cs` | `SlayerDeed` | 屠魔种类名称 |
| 6 | `Items/Magical/ArtifactManual.cs` | `ArtifactManual` | "This Identifies Items"、使用次数 |
| 7 | `Items/Magical/ManualOfItems.cs` | `ManualOfItems` | 使用次数、"Belongs to..." |
| 8 | `Items/Magical/StaffOfFiveParts.cs` | `Part1`-`Part5` | "Belongs to..."（5处） |
| 9 | `Items/Magical/GemOfSeeing.cs` | `GemOfSeeing` | "Find Hidden Items And Traps"、使用次数 |
| 10 | `Items/Magical/PandorasBox.cs` | `PandorasBox` | "Magically Access Your Bank Box"、使用次数 |
| 11 | `Items/Magical/ColoringBook.cs` | `ColoringBook` | 颜色名称字符串 |
| 12 | `Items/Magical/Arcane/` | 4个元素书 | "...Book of Spells" |
| 13 | `Items/Magical/RuneOfVirtue.cs` | `RuneOfVirtue` | 符文类型描述 |

### 5.2 Special 目录 - 特殊物品（9个文件）

| # | 文件 | Class | 英文文本 |
|---|------|-------|---------|
| 1 | `Items/Special/SlaversNet.cs` | `SlaversNet` | "Used to capture tamable creatures" |
| 2 | `Items/Special/OrbOfTheAbyss.cs` | `OrbOfTheAbyss` | "Belongs to..."（动态名称） |
| 3 | `Items/Special/AlternateRealityMap.cs` | `AlternateRealityMap` | "Use The Map To Examine It" |
| 4 | `Items/Special/SoulStone.cs` | `SoulStone` | "[Account Bound]"、"[Binds to account when used]" |
| 5 | `Items/Special/DragonPedStatue.cs` | `DragonPedStatue` | 颜色名、雕像名 |
| 6 | `Items/Special/Broken Furniture/` | 8个家具契约 | "Double Click To Place In Your Home" |

### 5.3 Trades 目录 - 交易技能物品（36个文件）

| 子目录 | 文件 | 英文文本 |
|--------|------|---------|
| **Blacksmithing/** | `FireGiantForge.cs` | "Fire Giant Forge"、"{0} Uses Remaining" |
| | `RubyPickaxe.cs` | "From Zorn the Blacksmith"、"Magically Dig Caddellite" |
| **Bowcraft/** | `ArrowsAndBolts.cs` | "This Bundle Contains 100/1,000 Arrows/Bolts"、"Double-Click To Separate..." |
| **Fishing/** | `ShipwreckedItem.cs` | 沉船名 |
| | `SpecialSeaweed.cs` | "Squeeze To Attempt To Extract Fluid"、"Need An Empty Bottle" |
| | `NewFish.cs` | "An Exotic Fish"、"Worth X Gold" |
| | `WetClothes.cs` | "Squeeze Out Water To Dry" |
| | `NeptunesFishingNet.cs` | "Use This On The High Seas"、"Requires 100 Seafaring" |
| | `FishingNet.cs` | "Use On The High Seas..."、"Requires 30 Seafaring" |
| | `FabledFishingNet.cs` | "Use This On The High Seas"、"Requires 90 Seafaring" |
| | `SpecialFishingNet.cs` | "Use This On The High Seas"、"Requires 60 Seafaring" |
| | `RustyJunk.cs` | "Scrap Iron" |
| | `HighSeasRelic.cs` | "Recovered From..." |
| | `BigFish.cs` | 渔夫名、重量 |
| | `AquariumSouthAddon.cs` / `AquariumEastAddon.cs` | "Double Click To Place In Your Home" |
| | `LightHouse.cs` | "To Be Built In A Home" |
| | `MarlinSouthAddon.cs` / `MarlinEastAddon.cs` | "Double-Click To Place In Home" |
| **Carpentry/** | `TaxidermyKit.cs` | 猎人名、重量 |
| **Thieving/** | `MagicSkeltonsKey.cs` | "Open any locked container or door" |
| | `SkeltonsKey.cs` | "Open most locked containers or doors" |
| | `MasterSkeltonsKey.cs` | "Open any locked container or door" |
| **Forensics/** | `PolishBoneBrush.cs` | "Polish Bones For Crafting" |
| **Reagents/** | `GoldenFeathers.cs` | "Gifted to..." |
| | `Reagents.cs` | "This Jar Contains..."、"Double-Click To Empty..." |
| **Cartography/** | `MapRanger.cs` | "Use To Get To Locations Quicker"、"Double-Click To Follow The Path" |
| | `LocalMap/WorldMap/CityMap/SeaChart.cs` | "for " + 地图世界名（HTML格式） |
| | `TreasureMap.cs` | "Somewhere in " + 地名、"(" + 坐标 + ")" |
| **Alchemy/** | `AlchemyTub.cs` | "Place In Your Home"、"Cleans Jars And Bottles" |
| | `CrystallineJar.cs` | "Holds Odd Substances" |
| **Ninjitsu/** | `LeatherNinjaBelt.cs` | 使用次数、毒药等级 |
| | `Shuriken.cs` | 使用次数、毒药等级 |
| | `FukiyaDarts.cs` | 使用次数、毒药等级 |
| | `Fukiya.cs` | 使用次数、毒药等级 |

---

## 6. Quest

### 6.1 Major 目录
`/World/Source/Scripts/Engines and Systems/Quests/Major`

## 6. 附录：非装备物品分类总表

### 按中文化策略分类

| 策略 | 适用物品 | 数量 |
|------|---------|------|
| **A - OPL 属性中文化** | 装备基类（HasAttributes） | 9个基类 ✅ |
| **B - 装备子类 cliloc 修复** | 额外 GetProperties 覆盖 | 5个文件 ✅ |
| **C - Gift 系统中文化** | 附魔属性 | 19个文件 ✅ |
| **D - Level 系统中文化** | 等级/经验值属性 | 25个文件 ✅ |
| **E - 非装备 AddNameProperties** | 描述性文本的 Item 子类 | ~63个文件 ⏳ |

### 策略 E 优先级建议

| 优先级 | 分类 | 理由 | 文件数 |
|--------|------|------|--------|
| P0 | Fishing 渔获/渔网类 | 高频交互 | ~15 |
| P0 | Cartography 地图类 | 高频交互 | ~6 |
| P1 | Thieving 钥匙类 | 中等频率 | ~3 |
| P1 | Magical 魔法物品 | 中等频率 | ~18 |
| P2 | Special 特殊物品 | 低频交互 | ~9 |
| P2 | Trades 其他技能 | 低频交互 | ~12 |

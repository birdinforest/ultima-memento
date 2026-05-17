# 待中文化物品清单

> **更新日期：** 2026-05-17
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
| P1 | 开锁过程头顶 | `Thieving/LockPick.cs` | 约 **10×** `PrivateOverheadMessage(..., "…")`（断钥匙卡/锁镐、无法破解/撬开、成功/失败等）+ **1×** `PublicOverheadMessage`（宝箱漏气，约 300 行）。建议 **`prop.trade.lockpick.overhead.*`**（或细分 `*.hack` / `*.pick`）写入 `equipment-properties.json`，`ResolveByKey` / `ResolveFormatByKey` 传入 `from.Account`。 |
| P2 | 月门确认 Gump | `Magical/Moongate.cs` | 仅在 **`!Core.AOS`** 分支 `AddHtml` 写死英文欢迎句（约 443 行）；AOS 分支已 `AddHtmlLocalized`。建议与非 AOS 路径对齐： **`AddHtmlLocalized` 同 cliloc** 或 shotkey + 拼 HTML。 |
| P0 | SOS 沉船求救全文 | `Fishing/Misc/SOS.cs` | 构造函数内 **`Beast` 模板**、`ShipStory` 随机段落、`IsAncient` 前缀等均为英文；`MessageGump` 将 `story` 注入 `AddHtml`。工作量大：宜按段落/模板设 **`prop.trade.sos.*`** 或独立逻辑键 JSON，并处理 **`LandName` / city / 船名** 插值。坐标行 `fmt`（`N/S/E/W`）可暂缓。 |
| P2 | Speech gump 标题 | `Fishing/FishBarrel.cs`、`Fishing/ScrapIronBarrel.cs` | `SpeechGump(from, "Fish In A Barrel", …)`、`"Rusty Gold"` 标题硬编码；正文依赖 `SpeechFunctions.SpeechText(..., "Aquarium" \| "ScrapMetal")` — 需与 **Speech 系统**是否已目录化联动核查。 |
| P2 | 遗物钟名称 / OPL 金额 | `Tinkering/Clocks.cs` | `Name = sLook + " grandfather clock"`（多类钟表重复）；`ColorText3 = "Worth " + CoinPrice + " Gold"`。需 **`item.trade.clock.*`** 或 `prop.trade.relicclock.*` + 形容词枚举键 / `ResolveFormatByKey`。 |
| P3 | 全目录 `DefaultDescription` / `DefaultName` / `Name =` | `Scripts/Items/Trades/` 下多文件 | 与 `SendMessage` 无关；zh-Hans 看 OPL 描述仍可能英文。宜按类分批：`IsContentLocalized` + `GetLocalizedPropertyList` / 描述 shotkey（或纳入长期「策略 E」表）。 |

**复查命令（仅供执行人）：**

- `SendMessage\s*\(\s*"`、`\.Say\s*\(\s*"`：应为空。
- `PrivateOverheadMessage\s*\([^)]*"`、`PublicOverheadMessage\s*\([^)]*"`：先清 `LockPick.cs`，再全库扩展同一模式。

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

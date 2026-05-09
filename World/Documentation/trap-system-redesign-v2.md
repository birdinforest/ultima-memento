# 陷阱系统重设计文档 v2.0
**Ultima Memento — HiddenTrap System Redesign**

> **文档状态：** 设计规格（待分阶段实施）  
> **关联分支：** `cursor/karma-trap-redesign-d202`  
> **已实施部分：** Priority 0 的 Karma 梯度翻转、资源陷阱部分破坏、拆除金币奖励（见同一分支的三次提交）

---

## 第零章：技能与工具基础分析

在任何设计决策之前，必须厘清项目中实际可用的技能与工具，避免引用不存在的接口。

### 0.1 可用技能清单（已确认在 `Skills.cs` 中存在）

| 技能名 | SkillName 枚举 | 当前与陷阱的关系 | 在新系统中的角色 |
|---|---|---|---|
| **Searching** | `SkillName.Searching` | ✅ 被动感知（踩踏时）+ 主动扫描 | 通用物理/机械陷阱感知，范围随技能增长 |
| **RemoveTrap** | `SkillName.RemoveTrap` | ✅ 被动规避 + 主动拆除（相邻格） | 主动探测、拆除、回收核心技能 |
| **Meditation** | `SkillName.Meditation` | ✅ 仅 type 25 的豁免加成 | 感知魔法/精神类陷阱（types 5, 7, 25） |
| **Spiritualism** | `SkillName.Spiritualism` | ⚠️ **BUG：代码中写的是不存在的 `SpiritSpeak`** | 感知灵魂类陷阱（types 12, 25），修复 bug 后生效 |
| **Tracking** | `SkillName.Tracking` | ❌ 目前无交互 | 感知"已触发陷阱的残留痕迹" |
| **Psychology** | `SkillName.Psychology` | ✅ 已在 `SpellTrap.cs` 中对玩家生效 | 抵抗精神类陷阱（types 5, 7） |
| **Tinkering** | `SkillName.Tinkering` | ✅ 通过 `DefTinkering` 制造 TrapKit（75–110 技能要求） | 制造专用陷阱类型和 TrapMechanism 物品 |

**关键 Bug 备注：**

`HiddenTrap.cs` 中对齐陷阱（type 25）的豁免计算使用了 `SkillName.SpiritSpeak`，但该枚举值在本项目的 `Skills.cs` 中**不存在**。正确的应该是 `SkillName.Spiritualism`。这个 bug 导致精神修炼加成永远为 0，在实施 Priority 1 时需要同步修复。

### 0.2 可用工具清单

| 工具 | 当前机制 | 问题 | 在新系统中的改造方向 |
|---|---|---|---|
| **TenFootPole** | 背包中被动触发，`Tap`% 概率规避，20 次使用 | 纯被动，玩家无法主动选择何时使用 | 增加 `OnDoubleClick` → 主动探测相邻格 |
| **TrapWand** | 被动规避（`Magery/3+25` = 33–66% 概率），30 分钟时效 | 纯被动；仅 Magery 路线可获取，Tinkering 专家无此工具 | 增加主动远程触发模式 |
| **TrapKit** | 消耗 RemoveTrap skill 决定 power，放置 SetTrap（物理伤害），25 次使用，3 个/10 格限制 | 只能放物理伤害的 SetTrap，无法指定陷阱类型 | 支持消耗 TrapMechanism 来放置特定类型陷阱 |
| **SetTrap** | 玩家放置的物理伤害陷阱，owner 免疫，180 秒自动消失 | 已有基础，只有物理伤害一种效果 | 作为陷阱放置的载体之一 |
| **SpellTrap** | 玩家放置的魔法陷阱，元素伤害，Psychology 检定 | 已有基础，只有伤害效果 | 作为陷阱放置的另一载体 |

### 0.3 技能不适用于陷阱系统

- **Lockpicking**：与容器陷阱（`TrapableContainer`）有交互，但与 `HiddenTrap` 地板陷阱无关，不纳入设计。
- **Hiding / Stealth / Snooping / Stealing**：无陷阱交互，不纳入设计。

---

## 第一章：Priority 0 — 惩罚校准（最优先实施）

> **设计目标：** 让陷阱的严重性与出现频率正相关；让武器损毁和 karma 翻转不再是"永久性绝望"，而是"有代价的挑战"。

### 1.1 陷阱概率加权池

**当前问题：** `Utility.RandomMinMax(1, 25)` — 25 种陷阱等概率（各 4%）。类型 1（揭示潜行）和类型 25（karma 翻转）概率完全相同。

**新设计：** 建立加权数组，按严重程度分层。

```
轻微（权重 ×5 — 出现最频繁）
  Type  1: 揭示潜行              — 仅状态，不损失资源
  Type  6: 中毒                  — 可用解毒药治疗
  Type 12: 名望小幅减少          — 无永久影响

中等（权重 ×3 — 标准频率）
  Type  2: 绊线摔跤（丢装备）    — 资源损失但可回收
  Type  7: 生命/耐力/法力耗尽    — 临时状态，自然恢复
  Types 14,15,17: 物理伤害       — 直接伤害，有物抗减免
  Types 16,18: 火焰伤害          — 直接伤害，有火抗减免
  Type 19: 电能伤害              — 直接伤害，有能量抗减免
  Type 20: 半数箭矢→木轴         — 资源损失，已改为部分
  Type 21: 半数绷带污染          — 资源损失，已改为部分

严重（权重 ×1.5 — 较少出现）
  Type  3: 半数货币→铅           — 资源损失，已改为部分
  Type  5: 属性减少 1 点         — 需要时间/物品恢复
  Type  8: 宝石熔合              — 资源损失
  Type  9: 半数试剂腐化          — 资源损失，已改为部分
  Type 10: 书籍装箱              — 不便，但可恢复
  Type 11: 传送                  — 迷失，但可返回
  Type 22: 部分药水碎裂          — 资源损失，已改为部分
  Type 23: 首饰熔合              — 装备损失（见改进）
  Type 24: 陷阱坑                — 强制传送

极端（权重 ×0.5 — 极罕见）
  Type  4: 装备损毁              — 见下方改进
  Type 13: 装备诅咒              — 见下方改进
  Type 25: Karma 翻转            — 已改为梯度系统
```

**实现方式：** 构建 `int[] weightedPool` 数组，按上述权重重复填入类型编号，然后用 `Utility.RandomMinMax(0, weightedPool.Length - 1)` 随机取一个索引。

权重示例（以 ×0.5 为单位，填入次数）：轻微 5 → 插入 10 次；中等 3 → 插入 6 次；严重 1.5 → 插入 3 次；极端 0.5 → 插入 1 次。总池约 148 个条目。

**区域调整：** 在特定高危区域（由 GM 标记的 Region 属性），将极端类型权重提升。低危区域可以完全排除 types 4, 13, 24, 25。

### 1.2 装备损毁陷阱（Type 4）改进

**当前问题：** 非保险物品直接变成 `RustyJunk` / `BrokenGear` 永久消失，与场景难度完全不成比例。

**新设计：两阶段损毁机制**

```
阶段一（陷阱首次击中目标装备）：
  → 装备耐久度降至当前最大值的 15%（最低保留 5 点）
  → 装备名称附加 " (badly damaged)"
  → Hue 变为锈蚀色调（约 0x0966）
  → 装备在 BaseWeapon/BaseArmor 上设置 TrapDamaged = true
  → 消息："A trap triggered, severely damaging your equipment!
           Find a blacksmith to repair it before it fails completely."

阶段二（带 TrapDamaged flag 的装备再次被 type 4 击中，
        或耐久度降至 0 继续使用）：
  → 变为 RustyJunk / BrokenGear（原有逻辑）
  → 此时玩家已被充分预警

修复途径：
  铁匠 NPC 修复操作同时清除 TrapDamaged 标志。
```

**实现要点：** 在 `BaseWeapon` / `BaseArmor` 添加 `bool TrapDamaged` 的序列化属性（Serialize/Deserialize）。Type 4 分支检查此属性来决定执行阶段一还是阶段二。

### 1.3 装备诅咒陷阱（Type 13）改进

**当前问题：** `CurseItem` 容器包住装备，玩家不知如何解除，也不知道持续多久，产生无方向感的绝望。

**新设计：明确化的诅咒状态**

```
触发后：
  → 装备放入 CurseItem（原有逻辑保留）
  → CurseItem 的 tooltip 明确显示：
    "This item is cursed by a trap.
     A healer, a skilled spiritualist (Spiritualism ≥ 70),
     or a holy shrine prayer may remove the curse.
     The curse will fade naturally after 24 hours."
  → CurseItem 内部设置 24 小时自动解除 timer

解除途径（多条，玩家可选）：
  途径 A：找 Healer NPC，花费金币
  途径 B：Spiritualism ≥ 70 的玩家对 CurseItem 使用技能 →
           CheckSkill(Spiritualism, 70, 120) 成功则解除
  途径 C：Ankh 神坛新增 "Remove Curse" contextmenu 选项
  途径 D：24 小时后自动解除
```

### 1.4 Karma 陷阱（Type 25）— 已实施，额外修复

**已实施（当前提交）：** 梯度翻转机制：
- `|karma| > 10000`：保留 40%，**不改变阵营**
- `|karma| > 4000`：阵营翻转，保留 70% 强度
- `|karma| ≤ 4000`：完整翻转（原始行为）

**待修复（Priority 1 时同步）：**
- `SkillName.SpiritSpeak` → `SkillName.Spiritualism`（Bug 修复，见 0.1 节）
- 修复后 Spiritualism 豁免加成真正生效

---

## 第二章：Priority 1 — 技能驱动的范围感知系统

> **设计目标：** 让"我知道有陷阱"与"我不知道有陷阱"之间存在一个可学习的、由技能投入决定的梯度区间。有经验的探险者应该感觉比新手更安全，不是因为"运气更好"，而是因为"更敏感"。

### 2.1 架构设计

在 `HiddenTrap` 中新增 `HandlesOnMovement { get { return true; } }` 和 `OnMovement` 处理器。当玩家移动到陷阱附近时，根据技能检测玩家是否应该收到预警。

**设计原则：**
- `OnMovement` 对每步都触发，必须轻量（避免重复循环）
- 必须有感知冷却（30 秒 CD）防止同一陷阱重复刷消息
- 感知消息不直接告知确切位置，只提供方向/距离感
- 揭示（`DiscoverTrap`，Weight → 3）仅在高技能时触发

**感知冷却实现：** 在 `HiddenTrap` 实例上维护一个 `Dictionary<Serial, DateTime> WarnedPlayers`，记录对哪个玩家最近警告过。同一陷阱对同一玩家 30 秒内不重复。

### 2.2 Searching — 通用范围感知（核心）

**感知类型：** 所有陷阱类型（机械、物理、魔法均可感知）

**触发条件：** 玩家进入陷阱附近时（`OnMovement`），Searching ≥ 25

| Searching 技能范围 | 感知半径 | 行为 |
|---|---|---|
| 1–24 | 0 格 | 仅踩踏时被动感知（现有行为，保留不变） |
| 25–49 | 1 格 | `"Something feels wrong nearby, but you can't quite tell where."` |
| 50–74 | 2 格 | `"Your instincts warn you of a hidden trap to the [northeast/etc.]."` |
| 75–99 | 3 格 | `"You notice subtle signs of a trap mechanism nearby — proceed with caution."` + 方向 |
| 100–124 | 4 格 | `DiscoverTrap`（陷阱变可见，Weight = 3）+ 方向 + 距离 |
| 125 | 5 格 | `DiscoverTrap` + **类别名称**（见下方） |

**类别名称体系（Searching 125 时触发 DiscoverTrap 后设置）：**

| 陷阱类型 | 揭示后名称 | Hue |
|---|---|---|
| Types 1, 3, 5, 7, 10, 13, 25（魔法/精神） | `"a glowing runic trap"` | 蓝色 |
| Types 14, 15, 17（纯物理） | `"a mechanical floor trap"` | 锈棕色 |
| Types 6, 9, 21（毒素） | `"a vented floor trap"` | 绿色 |
| Types 2, 20, 22, 23（绊线/资源） | `"a wired floor trap"` | 灰色 |
| Types 16, 18, 19（元素） | `"an elemental floor trap"` | 元素对应色 |
| Types 11, 24（传送/坑） | `"a dangerous floor trap"` | 深红色 |

**方向计算：** `dx = Trap.X - m.X; dy = Trap.Y - m.Y` → 映射到 8 方向字符串（North / Northeast / East / Southeast…）

### 2.3 Meditation — 心灵感知（专精向）

**感知类型：** 仅 types 5（属性减少）、7（生命/耐力/法力耗尽）、25（karma 翻转）

**触发条件：** Meditation ≥ 50，且陷阱类型为上述三类之一（即 `HiddenTrapType` 已知时）

| 陷阱类型 | 感知消息 |
|---|---|
| Type 5（属性减少） | `"Your meditative awareness registers an unnatural drain in this area."` |
| Type 7（资源耗尽） | `"You sense a void energy ahead — something is pulling at your vitality."` |
| Type 25（Karma 翻转） | `"A corrupting psychic force resonates beneath your feet — your discipline holds it at bay for now."` |

**特性：**
- 感知半径：固定 2 格（不随技能增长）
- 成功率：`Meditation / 100`（50 技能 = 50%，100 技能 = 100%）
- 只给预警消息，**不揭示陷阱**（Meditation 的核心是"感知精神波动"，不是"探查物理机关"）
- Meditation ≥ 100：对已感知到的 type 25，`moralDisciplineBonus` 额外 +10

### 2.4 Spiritualism — 灵魂感知（专精向）

**触发条件：** Spiritualism ≥ 50，且陷阱类型为 12 或 25

| 陷阱类型 | 感知消息 |
|---|---|
| Type 12（名望减少） | `"A spiritual disturbance lingers here — someone or something lost their standing."` |
| Type 25（Karma 翻转） | `"The spiritual resonance here feels deeply corrupted. A soul-warping force is at work."` |

**特性：**
- 感知半径：固定 2 格
- **同步修复 Bug**：`HiddenTrap.cs` 中所有 `SkillName.SpiritSpeak` → `SkillName.Spiritualism`
- Spiritualism ≥ 100：type 25 豁免额外 +10

### 2.5 Tracking — 痕迹感知（情报向）

**设计意图：** Tracking 不感知"尚未触发的陷阱"，而是感知"刚刚被触发过的陷阱留下的痕迹"。提供的是历史信息，而非实时预警。

**触发条件：** 主动使用 Tracking 技能，Tracking ≥ 30

**实现：**
1. 在 `LoggingFunctions.LogTraps` 被调用时，同时写入一个静态字典：  
   `Dictionary<Point3D, DateTime> RecentTrapActivity`
2. Tracking 主动扫描时，在 `15 + Tracking/5` 格范围内查询上述字典（15 分钟以内的记录）
3. 找到记录时附加提示：  
   `"The floor here shows signs of recent trap activity — a trap was triggered to the [northwest] within the last [X] minutes."`

**注意：** 该字典需要定期清理（超过 15 分钟的条目删除），避免内存泄漏。

---

## 第三章：Priority 2 — 主动探测与远程触发

> **设计目标：** 让玩家能够在"有信息"的情况下主动决定"是否消耗资源清除这个陷阱"。将所有防御工具从纯被动变为可被动、亦可主动。

### 3.1 TenFootPole 主动探测模式

**现有行为（保留）：** 背包中被动规避，`Tap`% 概率，20 次使用。

**新增行为：**

```
激活方式：OnDoubleClick → 弹出目标光标
目标范围：相邻 1 格（8 方向 + 当前格）
使用限制：每次使用消耗 1 次 Limits

目标格有 HiddenTrap（Weight < 5）：
  → SeeIfTrapActive() 检查
  → 如果陷阱激活：
    → 物理伤害类（types 14, 15, 17）：玩家受 50% 溅射伤害
      （站在相邻格，仍有一定风险）
    → 其余效果类型：玩家完全免疫
    → 陷阱触发后 DisableTrap()
    → 消息："You prod the floor ahead — a trap springs harmlessly!"
      或  ："You prod the floor ahead — a spike trap catches your pole! (minor damage)"
  → 如果陷阱未激活或无陷阱：
    → 消息："The floor seems clear in that direction."
    → 不消耗 Limits

超出范围：
  → 消息："You can only probe adjacent tiles with your pole."
```

**Tooltip 新增提示：** `"Active use: Double-click to probe an adjacent tile."`

### 3.2 TrapWand 远程触发模式

**现有行为（保留）：** 背包中被动规避，`WandPower`% 概率，30 分钟时效。

**新增行为：**

```
激活方式：OnDoubleClick → 弹出目标光标
目标范围：WandPower / 10 格（WandPower 25–66 → 2–6 格范围）

目标格有 HiddenTrap：
  → 向目标发射魔法脉冲（视觉：小球从玩家飞向目标格）
  → 陷阱触发（效果作用于陷阱位置，玩家远程完全免疫所有效果）
  → 消耗 WandPower 的 15%（最小消耗 5）
  → 如果 WandPower 降至 < 10：提示"The orb's power wanes..."
  → 如果 WandPower 降至 0：自动删除
  → 消息："Your trap-warding orb sends a pulse of magic — a trap fires harmlessly!"

无陷阱或陷阱未激活：
  → 消息："The orb pulses, but detects no active threats in that direction."

超出范围：
  → 消息："Your orb's range isn't sufficient to reach that far."
```

### 3.3 Searching 主动扫描增强

**现有行为（已在 `Searching.cs` 中实现）：** 主动使用可检测 HiddenTrap 并调用 `DiscoverTrap`，但消息只说 `"somewhere nearby"`，无方向信息。

**增强：**
- 扫描范围从固定件数改为 `max(3, Searching/10)` 格半径
- 检测到陷阱时，消息改为包含方向和距离：  
  低技能：`"There is a hidden floor trap somewhere nearby."`  
  中技能：`"There is a hidden floor trap to the [northeast], [close/far]."`  
  高技能（≥75）：`"There is a [wired/runic/mechanical] floor trap to the [northeast], [very close]."`（带类别）

### 3.4 RemoveTrap 主动拆除增强

**现有行为（`RemoveTrap.cs` 已实现）：** 主动 target HiddenTrap → `CheckSkill` → `DisableTrap`。

**增强：**

```
拆除成功（CheckSkill 通过）：
  → 金币奖励（已实施）
  → 额外：RemoveTrap.Value / 250 概率产出 TrapMechanism 物品（见 Priority 3）
    示例：50 技能 = 20% 概率，125 技能 = 50% 概率

拆除失败（CheckSkill 不通过）：
  → 陷阱随机触发（效果作用于玩家，但强度 × 50%）
  → 陷阱之后仍然 DisableTrap（拆除尝试总会让陷阱失效）
  → 消息："You fumble with the trap mechanism and trigger it accidentally!"

可拆除条件放宽：
  → 当前需要 Weight ≥ 2（激活状态）
  → 改为：Weight < 5（未禁用即可拆）
  → 允许玩家在 Searching 发现陷阱（Weight = 3）后立即尝试拆除，无需踩踏激活
```

**技能投入回报总结：**

| RemoveTrap 技能 | 拆除成功率（估算） | 失败代价 | TrapMechanism 概率 |
|---|---|---|---|
| 0–24 | 很低 | 陷阱全效触发 | 0% |
| 25–49 | 低 | 陷阱半效触发 | 10–20% |
| 50–74 | 中等 | 陷阱半效触发 | 20–30% |
| 75–99 | 高 | 陷阱半效触发 | 30–40% |
| 100–124 | 很高 | 陷阱半效触发 | 40–50% |
| 125 | 极高 | 陷阱半效触发 | 50% |

---

## 第四章：Priority 3 — 拆除→回收→重置循环

> **设计目标：** 将"陷阱探索"从纯消耗性的防御活动，转变为具有经济回路的玩法。让 RemoveTrap 高手成为"陷阱工程师"专业身份。

### 4.1 TrapMechanism（新物品）

**概念：** 从拆除陷阱中获得的核心部件，用于重新放置指定类型陷阱。

```
属性：
  Weight: 1.0
  Stackable: false（每个对应一次使用）
  LootType: Normal（可掉落、可被盗）
  TrapMechanismType: int（记录原始陷阱类型编号）

名称与 Hue（根据 TrapMechanismType）：
  Types 1, 3, 5, 7, 10, 13, 25 → "a magical trap core"      Hue: 蓝色
  Types 14, 15, 17              → "a spring mechanism"        Hue: 灰色
  Types 6, 9, 21                → "a venom capsule"           Hue: 绿色
  Types 16, 18, 19              → "an elemental charge"       Hue: 元素色
  Types 2, 20, 22               → "a wire coil"               Hue: 锈棕色
  Types 11, 24                  → "a spatial distorter"       Hue: 紫色

Tooltip：
  "A salvaged trap mechanism.
   Used with Trapping Tools to place a [type name] trap.
   Requires RemoveTrap ≥ 50."

获取途径：
  A. RemoveTrap 主动拆除 → (RemoveTrap/250) 概率产出
  B. RemoveTrap 被动规避路径 → (RemoveTrap/500) 小概率产出
  C. 特定地下城怪物掉落（陷阱工程师主题怪物）
  D. Tinkering 制造"空白机制"（随机类型，75 技能要求，8 铁锭）
```

### 4.2 TrapKit 增强

**新的使用流程（在 `TrapKit.OnDoubleClick` 增加分支）：**

```
如果背包中没有 TrapMechanism：
  → 执行现有逻辑（放置标准物理 SetTrap）

如果背包中有 TrapMechanism：
  → 弹出选择 Gump：
    [1] 放置标准物理陷阱（不消耗 Mechanism）
    [2] 放置 [TrapMechanismType 对应名称] 陷阱（消耗一个 Mechanism）

消耗 Mechanism 放置指定类型陷阱：
  → 需要 RemoveTrap ≥ 50
  → 放置的 HiddenTrap 的 HiddenTrapType = TrapMechanismType（类型固定，不再随机）
  → 继承 SetTrap 的 owner 免疫逻辑
  → 180 秒后自动消失（与 SetTrap 一致）

放置限制（不变）：
  → 区域需要允许 harmful
  → 范围内最多 3 个玩家设置的陷阱
  → 需要 RemoveTrap > 0
```

### 4.3 Tinkering 制造链扩展

**建议新增配方（添加至 `DefTinkering.cs`）：**

```
"Blank Trap Mechanism"（空白陷阱机制）：
  材料：8 铁锭
  技能要求：Tinkering 65.0–90.0
  产出：TrapMechanism（TrapMechanismType = 0 = 随机，放置时才确定）

"Reinforced Trapping Tools"（加强版 TrapKit）：
  材料：32 铁锭 + 4 宝石
  技能要求：Tinkering 90.0–120.0
  特性：Limits 从 25 增加到 50；Power 额外 +5
```

**叙事逻辑：** Tinkering 负责"制造工具"，RemoveTrap 负责"使用工具"。高 Tinkering 玩家可以批量生产套件出售；高 RemoveTrap 玩家是使用套件的专家。两个技能形成供需关系。

---

## 第五章：Priority 4 — 怪物触发陷阱系统

> **设计目标：** 让玩家放置的陷阱成为真正的"战术环境"，创造"引怪走陷阱"的合法战术深度。

### 5.1 技术分析：为何当前不生效

```csharp
// CanSetOffTraps() 当前逻辑：
// 非 PlayerMobile（包括 BaseCreature）返回 true — 理论上可以触发

// OnMoveOver 当前问题：
if ( m is PlayerMobile )  // ← 整个效果块被屏蔽，怪物永远不执行效果
{
    bool nSprung = CheckTrapAvoidance( m, this );
    // ... 全部 25 种陷阱效果 ...
}
```

怪物已经通过 `CanSetOffTraps()` 的检查，但效果块完全在 `PlayerMobile` 判断内，导致永远没有实际效果。

### 5.2 怪物效果分层

**A 类：对怪物完全生效（直接应用）**

```
Type  1 (揭示):     if (m.Hidden) m.Hidden = false
Type  6 (毒素):     m.ApplyPoison(m, Poison.XXX)
Type 14 (物理刺):   m.Damage(itHurts, m)
Type 15 (锯):       m.Damage(itHurts, m)
Type 16 (火焰):     m.Damage(itHurts, m)
Type 17 (大刺):     m.Damage(itHurts, m)
Type 18 (爆炸):     m.Damage(itHurts, m)
Type 19 (电击):     m.Damage(itHurts, m)
```

**B 类：对怪物有简化效果**

```
Type  5 (属性减少):  m.RawStr/Dex/Int -= 1，添加 30 分钟后恢复的 Timer
                     （不永久减少，刷新后怪物仍按原属性运行）
Type  7 (drain):     m.Hits = Math.Max(1, m.Hits / 2)（减半血量）
Type 11 (传送):      传送到地图随机合法位置（可选是否启用，影响怪物 AI）
```

**C 类：对怪物跳过（无意义或副作用过大）**

```
Types 2, 3, 8, 9, 10, 20, 21, 22, 23：背包/资源类，怪物无背包数据
Types 12, 13, 25：karma/名望/诅咒，怪物无这些属性
Type  24 (坑)：硬编码传送坐标，对怪物无叙事意义
```

### 5.3 怪物规避设计

```
普通 BaseCreature：无规避（直接触发）

Boss 级（Fame > 10000）：25% 概率天然规避
  代表"足够聪明/有经验的怪物会绕开已知危险"

高 MagicResist 怪物（MagicResist > 80）：
  对魔法类陷阱（types 1, 5, 7, 25）额外 MagicResist/100 规避概率
```

### 5.4 战术价值示例

```
策略 A（清道夫）：
  玩家发现陷阱 → 用 TenFootPole 主动触发 → 陷阱 Disable → 安全通行

策略 B（陷阱猎人）：
  玩家发现陷阱 → 不拆除 → 引怪走到陷阱上 → 陷阱伤怪

策略 C（布阵者）：
  高 RemoveTrap 玩家收集 TrapMechanism → 用 TrapKit 在 Boss 追击路径布置
  → 怪物触发陷阱 → 战术组合
```

### 5.5 SetTrap / SpellTrap 验证

**现有 `SetTrap.cs` 分析：** 代码使用 `m.Damage()` 方法，对所有 Mobile 有效。但需要验证 `CheckTrapAvoidance` 中 `PlayerMobile` 判断是否阻止了怪物路径。建议在实施前逐行检查 `SetTrap.OnMoveOver` 对 `BaseCreature` 的完整执行路径。

---

## 第六章：跨优先级设计考量

### 6.1 陷阱触发日志扩展

为 Priority 1 的 Tracking 感知服务，在现有 `LoggingFunctions.LogTraps` 调用时同步维护内存字典：

```csharp
// 建议添加到 LoggingFunctions 或 HiddenTrap 静态区域
public static Dictionary<Point3D, DateTime> RecentTrapActivity
    = new Dictionary<Point3D, DateTime>();

// 在 LogTraps 调用时插入：
RecentTrapActivity[trapLocation] = DateTime.Now;

// 定期清理（超过 15 分钟的条目）
```

### 6.2 陷阱类型感知一致性

Priority 1 的感知系统在陷阱类型已知（`HiddenTrapType > 0`）时可以给出类别信息。陷阱首次触发前类型未知（`HiddenTrapType == 0`）时，感知只给"通用感知消息"，不预先确定类型。

这意味着：GM 放置的固定类型陷阱（`HiddenTrapType` 预设）可以被高技能玩家识别类别；随机生成的陷阱在触发前只显示通用警告。

### 6.3 玩家放置陷阱的感知

玩家通过 TrapKit 放置的 SetTrap / 指定类型 HiddenTrap：
- 对其他玩家：同样触发 Priority 1 的感知系统（Searching 有效）
- 对怪物：Priority 4 的怪物规避检定适用
- **逻辑一致性原则：** 无论是地图内置陷阱还是玩家放置陷阱，感知/规避规则完全统一

### 6.4 反制滥用的安全边界

| 功能 | 限制机制 |
|---|---|
| TenFootPole 主动探测 | 距离 1 格；物理溅射 50%；消耗 Limits |
| TrapWand 远程触发 | 消耗 WandPower；30 分钟时效；来源稀缺 |
| RemoveTrap 主动拆除 | 失败有代价（陷阱触发半效） |
| 玩家放置陷阱 | 180 秒消失；3 个/10 格限制；区域检查 |
| TrapMechanism 使用 | RemoveTrap ≥ 50；每次消耗一个 |

---

## 第七章：实施路线图

```
Priority 0（最优先）— 当前分支已部分完成
  ✅ 已实施：Karma 梯度翻转
  ✅ 已实施：资源陷阱部分破坏（types 3, 9, 20, 21, 22）
  ✅ 已实施：拆除金币奖励
  ⬜ 待实施：加权概率池
  ⬜ 待实施：Type 4 两阶段损毁（TrapDamaged flag）
  ⬜ 待实施：Type 13 诅咒改进（24h 自动解除 + 多条解除途径）

Priority 1（次优先）
  ⬜ 修复 Bug：SkillName.SpiritSpeak → SkillName.Spiritualism
  ⬜ 新增 HiddenTrap.HandlesOnMovement + OnMovement
  ⬜ 新增感知冷却机制（WarnedPlayers 字典）
  ⬜ 修改 Searching.cs 主动扫描消息（方向 + 类别）
  ⬜ 新增 Meditation / Spiritualism 感知分支（OnMovement 中）
  ⬜ 修改 Tracking.cs：接入 RecentTrapActivity 数据

Priority 2（第三）
  ⬜ 修改 TenFootPole.cs：新增 OnDoubleClick + 目标探测逻辑
  ⬜ 修改 TrapWand.cs：新增 OnDoubleClick + 远程触发逻辑
  ⬜ 修改 RemoveTrap.cs：增强失败代价 + 调整产出为 TrapMechanism
  ⬜ 修改 Searching.cs：扩展扫描范围公式

Priority 3（第四）
  ⬜ 新增 TrapMechanism.cs（物品类）
  ⬜ 修改 TrapKit.cs：支持 Mechanism 消耗放置
  ⬜ 修改 DefTinkering.cs：新增 Mechanism 和强化 TrapKit 配方
  ⬜ 修改 HiddenTrap.DisableTrap：可选"产出 TrapMechanism"版本

Priority 4（最低优先）
  ⬜ 修改 HiddenTrap.OnMoveOver：拆分 PlayerMobile 与 BaseCreature 分支
  ⬜ 新增 BaseCreature 效果分支（A 类 + B 类陷阱）
  ⬜ 验证 SetTrap / SpellTrap 对怪物的完整执行路径
  ⬜ 新增 Boss 级怪物天然规避检定
```

---

## 附录 A：需要同步修复的现有 Bug

| # | 问题 | 位置 | 修复方案 |
|---|---|---|---|
| 1 | `SkillName.SpiritSpeak` 不存在于 `Skills.cs` | `HiddenTrap.cs` ~line 997 | 改为 `SkillName.Spiritualism` |
| 2 | `IsOnSpaceship` 判断块重复（两个完全相同的 `else if`） | `HiddenTrap.cs` ~line 107–110 | 删除重复的第二个分支 |
| 3 | Spaceship 排除列表与加权池不一致（加权池实施后需更新） | `HiddenTrap.cs` ~line 104 | 确保 Spaceship 的排除类型与加权池中的极端类型集合同步 |

## 附录 B：设计原则总结

```
可接受的挫败感
  ├── 可预期性：玩家失败后能理解"为什么"和"如何更好"
  ├── 有意义的投入：技能/道具建构在关键时刻有真实机械影响
  └── 可达的恢复路径：损失有代价但可以修复

挑战感与胜利价值感
  ├── 信息梯度：高技能玩家比新手感知更多、更远
  ├── 决策窗口：在"感知到危险"和"触发危险"之间有行动空间
  ├── 正向激励：成功化解陷阱产生奖励，不仅仅是避免惩罚
  └── 专业身份：陷阱工程师专精路线有差异化的战术价值
```

# 待中文化：Mobiles 目录扫描

> **扫描日期：** 2026-05-25
> **处理范围：** 处理所有英文硬编码文本（`SendMessage`、`Say`、`PublicOverheadMessage`、`LocalOverheadMessage`、Gump `AddHtml`/`AddLabel`、OPL `list.Add`、`Name =`、`Title =`、`InfoText1-5`、`ColorText1-5`、属性返回字符串等）。
> **排除范围：** 不处理 cliloc 控制的文本（`SendLocalizedMessage`、仅 cliloc 数字的 `*OverheadMessage`）。已通过 `StringCatalog.Resolve` / `ResolveByKey` / `ResolveFormat` / `AddLocalizedProperty` 且键已在 locale 中的行亦不计入。
> **扫描路径：** `World/Source/Scripts/Mobiles`
> **关联：** [`waiting-localization-items.md`](waiting-localization-items.md) · [`waiting-localization-skills.md`](waiting-localization-skills.md) · [`waiting-localization-quest.md`](waiting-localization-quest.md) · `AGENTS.md` §3.2

## 摘要

| 指标 | 数量 |
|------|------|
| 扫描 `.cs` 文件 | ~1112 |
| **含英文硬编码（待处理）的文件** | **~500+** |
| 无英文硬编码（或仅 cliloc / 已目录化） | ~600+ |

**说明：** Mobiles 目录以 NPC 名/头衔（`Name =` / `Title =`）为主（~70% 硬编码量），加上战斗/交互消息（`SendMessage`、`Say`、`LocalOverheadMessage`）。部分 Civilized/ 子目录已经部分目录化（Guildmasters 使用 `StringCatalog.ResolveByKey`）。Behavior.cs 内约 100 条战斗恐吓对白为单一文件最大集中源。

### 主要模式分布

| 模式 | 估算条数 | 影响范围 |
|------|---------|----------|
| `Name = "..."` | ~500+ | 大部分 NPC 基础名（Animal / Undead / Constructs 等） |
| `Title = "..."` | ~200+ | 头衔（the pirate, the demilich, the titan of earth 等） |
| `SendMessage("...")` | ~150+ | 玩家交互反馈（技能验证、物品拾取、状态消息等） |
| `SendMessage(hue, "...")` | 1 | BaseGuildmaster cs |
| `.Say("...")` | ~140+ | NPC 对白（战斗喊话 + 笑话 + 互动） |
| `LocalOverheadMessage(..., "...")` | ~120+ | 瞬发战斗反馈（中毒/眩晕/物品锈损/石化） |
| `PublicOverheadMessage(..., "...")` | ~20+ | Boss 击败/驱散/战斗公告等 |
| `list.Add(1049644/1070722, "...")` | ~25+ | OPL 物品描述行 |
| `InfoText1-5 = "..."` | ~20+ | Boss 战利品铭文 |
| `ColorText3/1 = "..."` | 少量 | 估价等 |
| Gump `AddHtml`/`AddLabel` | ~50+ | PlayerBarkeeper、RacePotions 等 Gump 界面 |
| 属性/方法返回英文字符串 | 少量 | Property return 值 |

### 已目录化（StringCatalog / BuildingPropertyListLocale 已覆盖）

以下子目录已部分或完全目录化，**不在**待办范围内（除非部分残留）：

- **Civilized/Guilds/** — 大部分 Guildmaster 已使用 `StringCatalog.ResolveByKey`（CartographersGuildmaster、DruidGuildmaster、MageGuildmaster、NecromancerGuildmaster、ElementalGuildmaster、AssassinGuildmaster、LibrarianGuildmaster 等均有 `BuildingPropertyListLocale` + `AddLocalizedProperty`）
- **Civilized/Merchants/** — Tailor（19 处 `ResolveByKey`）、Weaver（9）、Mapmaker（13）、Thief（7）、Jester（8）、Painter（5）、Shipwright（7）、InnKeeper（3）
- **Civilized/Special/** — LostItemsRestorerNPC（5 处 `charrestore.*` 键）、EpicCharacter（7 处）、Xardok（1）
- **Civilized/** — TownHerald（`StringCatalog.ResolveByKey`）、DeathKnightDemon（2）、Chuckles（2）、ShardGreeter（2）
- **Civilized/Citizens/** — Tradesman*.cs 系列（Alchemist/Smith/Butcher/Leather/Miner/Cook/Logger）使用 `StringCatalog.ResolveByKey`
- **Civilized/Comrades/** — HenchmanFunctions（11 处 `StringCatalog.ResolveByKey`）
- **Civilized/Porters/** — PorterItem（5 处 `StringCatalog.ResolveByKey`）
- **Base/** — PlayerBarkeeper 部分目录化（`StringCatalog`）
- **Races/** — RacePotions 使用 `RaceLocalization.Key`（已目录化）
- 注意：**部分文件同时包含已目录化和未目录化部分**（如 `BaseCreature.cs` 既有 `StringCatalog.Resolve` 也有大量裸 `SendMessage`）

---

## 待处理目录总表

| 一级目录 | 扫描 `.cs` 文件数 | 含硬编码文件数 | 估算硬编码条数 | 主要类型 |
|----------|-------------------|---------------|----------------|----------|
| Base/ | ~10+ | 7 | ~300+ | SendMessage, Say, PublicOverheadMessage, LocalOverheadMessage, OPL |
| Animals/ | ~80 | ~50 | ~100+ | Name = |
| Civilized/ | ~200+ | ~80 | ~200+ | Name =, Title =, SendMessage, Gump, Say |
| Constructs/ | ~40+ | ~30 | ~80+ | Name =, LocalOverheadMessage |
| Demons/ | ~20+ | ~15 | ~40+ | Title =, LocalOverheadMessage, Name = |
| Dragons/ | ~40+ | ~30 | ~200+ | Name =（Wyrms.cs ~50 种）, Title =, LocalOverheadMessage |
| Elementals/ | ~40+ | ~30 | ~80+ | Name =, LocalOverheadMessage, Title = |
| Gargoyles/ | ~15+ | ~10 | ~15+ | Name =, Title = |
| Goliaths/ | ~30+ | ~20 | ~50+ | Name =, Title = |
| Hellish/ | 小 | 2 | ~5 | Name =, Title = |
| Humanoids/ | ~200+ | ~150 | ~300+ | Name =, Title =, SendMessage |
| Insects/ | ~30+ | ~20 | ~30+ | Name = |
| Mystical/ | ~20+ | ~15 | ~30+ | Name =, Title =, SendMessage |
| Plants/ | ~15+ | ~10 | ~20+ | Name =, SendMessage |
| Reptilian/ | ~40+ | ~30 | ~60+ | Name =, Title =, SendMessage, LocalOverheadMessage |
| Slimes/ | ~15+ | ~10 | ~20+ | Name =, LocalOverheadMessage, SendMessage |
| Summoned/ | ~20+ | ~10 | ~15+ | Name = |
| Undead/ | ~60+ | ~40 | ~100+ | Name =, Title =, SendMessage, InfoText, OPL |
| Unique/ | ~30+ | ~20 | ~60+ | SendMessage（Boss 击败/奖励）, Name =, Title =, InfoText |
| Unusual/ | ~15+ | ~10 | ~15+ | Name =, SendMessage |
| **合计** | **~1112** | **~500+** | **~1,800+** | |

---

## 建议修复批次

| 批次 | 范围 |
|------|------|
| **M0** | **`Behavior.cs`** — `SaySomethingWhenAttacking()` 约 100 条战斗恐吓对白（daemon/dragon/giant/orc/troll 等全种族）+ `PunchStun`/`PoisonVictim` LocalOverheadMessage 2 条 — 统一用 `BehaviorCombatTaunt` 逻辑键 |
| **M1** | **`BaseCreature.cs`** — SendMessage ~39 处（血肉收获/剥皮/宠物经验/死亡消息）+ PublicOverheadMessage ~14 处（驱散/驯服）+ LocalOverheadMessage 3 处 |
| **M2** | **`BaseVendor.cs`** — 22 条中世纪笑话 Say + 买卖拒绝 `"I have no business with you."` + 咒语 `Say` |
| **M3** | **`Wyrms.cs`（Dragons/Wyrms/）** — ~50 种 wyrm 变体 `rName = "the bloodstone wyrm"`… + 所有 `Title = "the X wyrm"` 和 `Name = "egg of " + Title` |
| **M4** | **`PlayerBarkeeper.cs`** — Gump AddHtml 全部 ~40+ 标签（"BARKEEP CUSTOMIZATION MENU", "Message Control", "Back"…） |
| **M5** | **Unique/ Boss 系列** — `SendMessage` Titan 四元素 + Shadowlord + Banes + Serpent + Exodus + Mangar + Tarjan + Vordo（~60+ 条奖励/击杀/任务消息） |
| **M6** | **`Title =` 全库** — 散布于 ~150+ 个文件的 `Title = "the X"` 头衔（`"the pirate captain"`, `"the demilich"`, `"the titan of earth"`…），批量处理 |
| **M7** | **`Name =` NPC 生物名（非 Unique）** — Animals/、Constructs/、Demons/、Elementals/ 等各目录 ~300+ 个基础生物名，建议分目录批量处理 |
| **M8** | **`Name =` Humanoids/ 系列** — Savages/ Native 装备名（`"Native Tunic"`, `"Native Gauntlets"`…）+ 海盗/水手名 + 各子种族名 |
| **M9** | **`Name =` 宝箱/容器/特殊物品名** — 散布在 NPC 死亡掉落中的 `MyChest.Name = "bone carved chest"`, `"giant sack"` 等 |
| **M10** | **`LocalOverheadMessage` 元素效果** — Rust 锈损（Daemon/Balron/Dragon）, Wind 吹飞（Air/Dust/Storm/SandVortex/ Typhoon）, Slime 覆盖, Weed 包裹, Stone 石化（Medusa/Basilisk/Gorgon）, Sewage 污物 — ~80+ 条模板化消息 |
| **M11** | **`LocalOverheadMessage` 眩晕/麻痹** — `"You have been stunned by a colossal blow!"` + `"You recover your senses."`（~20 个 Golem/Statue 文件完全重复）|
| **M12** | **`InfoText1-5` Boss 战利品** — Vordo、Kull、GrundulVarg、Murk、BaronAlmric、Dracolich、BlackKnight 等 ~20 处 |
| **M13** | **`list.Add` OPL 交易品/容器** — Tradesman*.cs 系列 Crate OPL（`"Contains X Leather"`, `"Open to Remove them from the Crate"`） + Familiars/PorterItem `"Belongs To X"` |
| **M14** | **`Say("*emotes*")`** — BaseVendor 4 条（`*claps*`, `*bows*`, `*giggles*`, `*laughs*` 等）+ TrainingSpirits 3 条 + Jedi 1 条 |
| **M15a** | **吸血/生命反馈 (Blood/Life Drain)** — 详见下文"战斗反馈分类" §吸血类 |
| **M15b** | **灵魂/魔力/体力吸取反馈 (Soul/Mana/Stamina Drain)** — 详见下文"战斗反馈分类" §吸取类 |
| **M15c** | **眩晕/麻痹/石化反馈 (Stun/Paralyze/Petrify)** — 详见下文"战斗反馈分类" §控制类 |
| **M15d** | **元素覆盖物反馈 (Elemental Coverage)** — 详见下文"战斗反馈分类" §元素物效类 |
| **M15e** | **驱散/驯服/魔法屏障反馈 (Dispel/Tame/Barrier)** — 详见下文"战斗反馈分类" §特殊战斗反馈 |
| **M15f** | **音乐/歌声抗性反馈 (Bard Song Resist)** — 详见下文"战斗反馈分类" §音乐抗性类 |
| **M15g** | **其他伤害反馈 (Other Damage)** — 详见下文"战斗反馈分类" §其他伤害反馈 |

---

## 战斗反馈分类

> 以下分类整理了所有 Mobiles 目录下战斗相关的硬编码反馈信息，按反馈类型分组，便于批量处理。

### §吸血类（Blood Drain / Life Drain）

吸取生命值（Hits）并转为己用，是 Mobiles 中最常见的特殊攻击反馈。

| 文件 | 中文意义 | 英文文本 | 作用机制 |
|------|---------|---------|---------|
| `Vampire.cs` | "你感到血液从你体内流失！" | `"You feel the blood draining from you!"` | AOE DrainLife，回复自身血量的 15-30 |
| `VampirePrince.cs` | 同上 | `"You feel the blood draining from you!"` | 同上 |
| `VampireLord.cs` | 同上 | `"You feel the blood draining from you!"` | 同上 |
| `VampireWoods.cs` | 同上 | `"You feel the blood draining from you!"` | 同上，回复 5-8 |
| `Dracula.cs` | 同上 | `"You feel the blood draining from you!"` | 同上，回复 15-30 |
| `BloodLotus.cs` | 同上 | `"You feel the blood draining from you!"` | 同上（植物类） |
| `Stirge.cs` | 同上 | `"You feel the blood draining from you!"` | 同上（动物类） |
| `BloodSnake.cs` | "你感到血液从你体内流失！" | `"You feel the blood drain from you!"` | 同上，回复 10-40 |
| `VampiricDragon.cs` | "你感到血液从你体内流失！" | `"You feel the blood draining from you!"` | Dragon 类吸血 |
| `BloodWorm.cs` | "虫子从你身上吸了些血！" | `"The worm sucks some blood from you!"` | 回复 10-40 |
| `GiantLeech.cs` | "水蛭从你身上吸了些血！" | `"The leech sucks some blood from you!"` | 回复 5-8 |
| `MarshWurm.cs` | "生物从你身上吸了些血！" | `"The creature sucks some blood from you!"` | 回复 10-16 |
| `GiantLamprey.cs` | "八目鳗从你身上吸了些血！" | `"The lamprey sucks some blood from you!"` | 回复 10-20 |
| `Succubus.cs` | "你感到生命从你体内流失！" | `"You feel the life drain out of you!"` | 回复 10-40 |
| `BloodDemigod.cs` | "你感到生命从你体内流失！" | `"You feel the life drain out of you!"` | 回复 10-40 |
| `SoulSucker.cs` | "生物正在吸走你的灵魂！" | `"The creature is sucking the soul out of you!"` | 回复 10-40 |

**共同模式：** `SendMessage` 硬编码 + `m.Damage()` + `Hits +=` 吸血回复。建议统一使用 `StringCatalog.Resolve` hash 模式，或按逻辑域提取 shotkey（如 `combat.feedback.blooddrain.*`）加入 `equipment-properties.json`。

### §吸取类（Soul / Mana / Stamina Drain）

吸取非生命值的其他属性（法力、体力、灵魂）。

| 文件 | 中文意义 | 英文文本 | 作用机制 |
|------|---------|---------|---------|
| `BaseCreature.cs` L1267/L1286 | "你感到你的灵魂正在流失！" | `"You feel your soul draining!"` | 减低 Mana/Stam（基于 Fame/500） |
| `WaxSculpture.cs` | "蜡封住了你的嘴……难以呼吸！" | `"Wax covers your mouth...so it is hard to breath!"` | AOE Stam 降低 10-40 |
| `MudMan.cs` | "你被扔出的泥土呛到了！" | `"You choke from the thrown mud!"` | Stam 降低 5 |
| `OmniAI Magery.cs` | 调试用 `"Draining mana"` | `SayHued(1156, "Draining mana")` | AI 决策日志，非玩家可见；但 `ManaVampireSpell` / `ManaDrainSpell` 由法术自带本地化文本 |
| `BaseCreature.cs` L8458 | "一个灵魂已被宣告。" | `"A soul has been claimed."` | 死亡骑士被动回复 |

### §控制类（Stun / Paralyze / Petrify）

| 文件 | 中文意义 | 英文文本 | 备注 |
|------|---------|---------|------|
| `SphinxRiding.cs` | "你被强大的咆哮吓得石化了！" | `"You are petrified with fear from the mighty roar!"` | Paralyze 4-8s |
| `RoyalSphinx.cs` | 同上 | `"You are petrified with fear from the mighty roar!"` | 同上 |
| `GorgonRiding.cs` | "你被蛇发女妖的气息石化了！" | `"You are petrified from the Gorgon breath!"` | Paralyze |
| `SoulWorm.cs` | "你被蠕虫的目光催眠了！" | `"You are hypnotized by the worm's gaze!"` | Paralyze |
| `LivingBronzeStatue.cs` | "你被一记重拳击晕了！" | `LocalOverheadMessage("You have been stunned by a colossal blow!")` | 30% 触发 Stun |
| `LivingIronStatue.cs` | 同上 | 同上 | 同上 |
| `BoneGolem.cs` | 同上 | 同上 | 同上 |
| `CaddelliteGolem.cs` | 待确认 | 待确认 | 同上 |
| `Ghoul.cs` | "你被死灵之爪麻痹了！" | `LocalOverheadMessage("You have been paralyzed by a necrotic claw!")` | 30% 触发 |
| `IceGhoul.cs` | 同上 | 同上 | 同上 |
| `Medusa.cs` | 石化物品（见 §物品锈损/石化） | 见下文 | 石化背包物品 |
| `BasiliskRiding.cs` | 同上 | 同上 | 同上 |

### §元素/物效类（Elemental / Special Breath / Covering）

| 文件 | 中文意义 | 英文文本 | 备注 |
|------|---------|---------|------|
| `BaseCreature.cs` L1061 | "你被强大的咆哮冲击波击中！" | `"You are hit by the force of the mighty roar!"` | 恐龙咆哮 (Breath form 4) |
| `BaseCreature.cs` L1067 | "你被蝎尾狮的毒刺击中！" | `"You are hit by a manticore thorn!"` | 蝎尾狮毒刺 (Breath form 5) |
| `Leviathan.cs` | "你被野兽的力量击中！" | `"You are hit by the force of the beast!"` | 利维坦 Breath |
| `GiantEel.cs` | "你被鳗鱼的电流击中！" | `"You are struck with the eel's electricity!"` | AOE 电击 |
| `StormCloud.cs` | "你被风暴的闪电击中！" | `"You are struck with the storm's lightning!"` | AOE 闪电攻击 |
| `AirElemental.cs` | "你被吹飞了！" | `LocalOverheadMessage("You are blown away!")` | 推开效果 |
| `DustElemental.cs` | 同上 | 同上 | 同上 |
| `KelpElemental.cs` | "你被海草缠住了！" | `LocalOverheadMessage("You are tangled in the kelp!")` | 束缚 |
| `SeaweedElemental.cs` | 同上 | 同上 | 同上 |
| `YoungDragon.cs`、`Dragon.cs` 等 | "你的盔甲被龙息锈蚀了！" | `LocalOverheadMessage("Your armor has rusted!")` | 削弱护甲（多个 Dragon 子类） |
| `Wyvra.cs` | "你的武器被龙息锈蚀了！" | `LocalOverheadMessage("Your weapon has rusted!")` | 削弱武器 |
| `Medusa.cs` | "你的一件物品被石化了！" | `LocalOverheadMessage("One of your items has been turned to stone!")` | 石化背包随机物品 |
| `BasiliskRiding.cs` | "石化几乎命中了你的一件受保护物品！" | `LocalOverheadMessage("The basilisk almost turned one of your protected items to stone!")` | 保险保护 |
| `BasiliskRiding.cs` | "你的一件物品被石化了！" | `("One of your items has been turned to stone!")` | 同上 |
| `MysticalFox.cs` | （无玩家可见反馈） | 仅 cliloc 1070824 | 持续 5 次生命伤害 |

### §特殊战斗反馈（Dispel / Tame / Barrier / Loot）

| 文件 | 中文意义 | 英文文本 | 备注 |
|------|---------|---------|------|
| `BaseCreature.cs` L6587 | "驱散被阻止（技能不足）" | `PublicOverheadMessage("Dispel prevented (Low skill)")` | 驱散日志 |
| `BaseCreature.cs` L7901 | "你魅惑了蛇。请选择要攻击的目标。" | `SendAsciiMessage("You charm the snake. Select a target to attack.")` | `DeathAdderCharmable` |
| `MechanicalScorpion.cs` | "你的武器无法穿透此生物的魔法屏障。" | `SendAsciiMessage("Your weapon cannot penetrate the creature's magical barrier")` | 机械蝎防护盾 |
| `BaseVendor.cs` L1556 | "诅咒已从 XXX 上移除。" | `PrivateOverheadMessage("The curse has been lifted from the " + curseName + ".")` | NPC 解除诅咒物品（动态名称 + 前置 `"This is not a graveyard! Bury them somewhere else!"`）|

### §音乐抗性类（Bard Song Resist）

| 文件 | 中文意义 | 英文文本 | 备注 |
|------|---------|---------|------|
| `BaseCreature.cs` L7936 | "你以魔法抵抗了歌声的影响。" | `"You magically resist the affects of the song."` | 诗人技能抗性 |
| `BaseCreature.cs` L7975 | 同上 | 同上 | 同上 |
| `BaseCreature.cs` L8050 | 同上 | 同上 | 同上 |

### §其他战斗反馈

| 文件 | 中文意义 | 英文文本 | 备注 |
|------|---------|---------|------|
| `BaseCreature.cs` L6572 | "你偷取了 X 枚金币！" | `"You " + stole + " " + coins + " " + m_CoinType + "!"` | 动态 stealing（7 种动词 + 2 种币名） |
| `Behavior.cs` L4693 | "你的生命只能滋养我！" | `Say("Your life only feeds my own!")` | Behavior 战斗喊话 |
| `Behavior.cs` L4713 | "你的灵魂将属于我！" | `Say("Your soul will be mine!")` | 同上 |
| `Behavior.cs` L4716 | "我期待折磨你的灵魂，X！" | `Say("I look forward to torturing your soul, " + m.Name + "!")` | 同上，含动态名称 |
| `BaseCreature.cs` (breath 各种形式) | Breath 不产生文本，仅特效/音效 | — | 呼吸攻击不自带文字反馈 |

---

## 按模块明细

### Base/（~10+ 文件，~300+ 处硬编码）

#### `BaseCreature.cs`

Mobiles 最大单个文件源之一。

**SendMessage（~39 处）：**

| 行 | 类型 | 示例 |
|---|------|------|
| 1061 | SendMessage | `"You are hit by the force of the mighty roar!"` |
| 1067 | SendMessage | `"You are hit by a manticore thorn!"` |
| 1267 | SendMessage | `"You feel your soul draining!"` |
| 1286 | SendMessage | `"You feel your soul draining!"` |
| 4409-4415 | SendMessage | 弗兰肯巨怪断体 7 种（`"You sever off the giant's left leg."`…） |
| 4552 | SendMessage | `"You cut away some furs and they are on the corpse."` |
| 4577 | SendMessage | `"You cut away some leather and they are on the corpse."` |
| 4605 | SendMessage | `"You carve away some wood and they are on the corpse."` |
| 4633 | SendMessage | `"You chisel away some granite and it is on the corpse."` |
| 4654 | SendMessage | `"You cut away some skins and they are on the corpse."` |
| 4756 | SendMessage | `"You chip away some stones and they are on the corpse."` |
| 4809 | SendMessage | `"You chip away some metal and it is on the corpse."` |
| 4844 | SendMessage | `"You cut away some scales and they are on the corpse."` |
| 4921 | SendMessage | `"You cut away some bones and they are on the corpse."` |
| 6572 | SendMessage（format） | `"You " + stole + " " + coins + " " + m_CoinType + "!"` |
| 7040 | SendMessage | `"Make sure this skill is marked to raise..."` |
| 7936 | SendMessage | `"You magically resist the affects of the song."`（×4 不同行） |
| 7979 | SendMessage | `"You hear jarring music, suppressing your abilities."` |
| 8073 | SendMessage | `"The music is hypnotic, making you remove your worn items."` |
| 8458 | SendMessage | `"A soul has been claimed."` |
| 8493 | SendMessage | `"Evil has been banished."` |
| 8976-8978 | SendMessage | `"{0} the vendor cannot be harmed."`（format） |
| 9993-10206 | SendMessage | 宠物死亡/经验/升级全链 ~10 条 |

**PublicOverheadMessage（~14 处）：**

| 行 | 示例 |
|---|------|
| 2100 | `"* The creature has been beaten into subjugation! *"` |
| 6518-6672 | `"Dispel prevented (DispelDifficulty)"`, `"Dispel prevented (Low skill)"`, `"Dispel prevented (Magery)"`, `"Dispel prevented (Mana)"`, `"Dispel chance increased (Slayer)"`, `"Dispel prevented (Failed)"`, `"Defensively Dispelled"` |
| 9714 | `"*looks furious*"` |

#### `Behavior.cs`

**`SaySomethingWhenAttacking()`（~100 条战斗喊话）—— Mobiles 最大单体文本源：**

覆盖 Exodus、FleshGolem、BloodDemigod、Balron 系、冰魔/海魔/吸精鬼/Satan、VampiricDragon、ShadowWyrm、AshDragon、龙族全系列、Gargoyle 假语、Zorn、OrkDemigod、Troll 系列、Ettin 系列、Titan/CloudGiant、Ogre 系列、冰巨人/熔岩巨人/海巨人、MountainGiant 系、树精（Ent）、SwampThing、Beholder、Dracolich 等。

**LocalOverheadMessage（4 处）：**

| 行 | 示例 |
|---|------|
| 3622 | `"You are hit with a stunning punch!"` |
| 3661 | `"You have been poisoned!"` |

**Say 魔法召唤：**

| 行 | 示例 |
|---|------|
| 1655 | `from.Say( "" + NameList.RandomName( "magic words" ) + "..." )` |

#### `BaseVendor.cs`

**Say 笑话（22 条）：**

| 行 | 示例 |
|---|------|
| 543-570 | 22 条中世纪主题笑话 + 6 条表情动作 `"*claps*"`, `"*bows*"`, `"*giggles*"`, `"*laughs*"`, `"*sticks out tongue*"`, `"*woohoo!*"` |
| 896, 1149, 1801, 2086 | `"I have no business with you."` ×4 |

**Title = 动态赋值（~10 处）：**

| 行 | 示例 |
|---|------|
| 611 | `"the archer"` |
| 696 | `"the merchant"` |
| 701 | `"the dock worker"` / `"the merchant"` |
| 702 | `"the sailor"` |
| 703 | `"the cooper"` |
| 704 | `"the cabin boy"` / `"the serving wench"` |
| 705 | `"the master-at-arms"` |
| 708 | `"the harpooner"` |
| 717 | `"the boatswain"` |
| 732 | `"the fence"` |
| 736 | `"the quartermaster"` |
| 745 | `"the butler"` |
| 749 | `"the maid"` |

**SendMessage（~3 处）：**

| 行 | 示例 |
|---|------|
| 2453 | `"" + cost + " gold per charge..."`（format, 法术价格） |
| 2745, 2783 | `"You do not have enough gold."` |

#### `PlayerMobile.cs`

| 行 | 类型 | 示例 |
|---|------|------|
| 1153-1155 | SendMessage | `"{0} the vendor cannot be harmed."` / `"{0} {1} cannot be harmed."` |

#### `PlayerVendor.cs`

| 行 | 类型 | 示例 |
|---|------|------|
| 1077 | SendMessage | `"You cannot buy this item right now. Please wait one minute and try again."` |
| 1091 | SendMessage | `"Enter the amount of gold you wish to withdraw (ESC = CANCEL):"` |
| 1495 | SendMessage | `"You cannot price items above 100,000,000 gold. The price has been adjusted."` |

`list.Add`（~12 处）：待确认具体清单。

#### `BaseHealer.cs`

| 行 | 类型 | 示例 |
|---|------|------|
| — | SendMessage | 4 处（疗愈/复活反馈） |

#### `BaseGuildmaster.cs`

| 行 | 类型 | 示例 |
|---|------|------|
| 134 | SendMessage(hue) | `"You have resigned from the local guild."` |

#### `BaseMount.cs`

| 行 | 类型 | 示例 |
|---|------|------|
| — | SendMessage | 4 处（坐骑交互） |

#### `PlayerBarkeeper.cs`

**Gump AddHtml（~40+ 标签）— 全英文界面：**

| 行 | 示例 |
|---|------|
| 656 | `"BARKEEP CUSTOMIZATION MENU"` |
| 661 | `"sells food and drink"` |
| 685 | `"More Job Titles"` |
| 688 | `"Back"` |
| 753 | `"Message Control"` |
| 756 | `"Customize your barkeep"` |
| 759 | `"Dismiss your barkeep"` |
| 770 | `"Add or change a message and keyword"` |
| 773 | `"Remove a message and keyword from your barkeep"` |
| 776 | `"Add or change your barkeeper's tip message"` |
| 779 | `"Delete your barkeepers tip message"` |
| 789 | `"Are you sure you want to dismiss your barkeeper?"` |
| 795 | `"No"` |
| 805 | `"Add or change a message"` |
| 829 | `"Choose the message you would like to remove"` |
| 858 | `"Change this tip message"` |
| 859 | `"Message"` |
| 872 | `"Remove this tip message"` |
| 873 | `"Message"` |
| 887 | `"Title"` |
| 890 | `"Appearance"` |
| 893 | `"Male / Female"` |
| 896 | `"Back"` |

#### `Paragon.cs`

| 行 | 类型 | 示例 |
|---|------|------|
| — | SendMessage | 2 处（Paragon 特殊消息） |

---

### Civilized/（~200+ 文件，~200+ 处硬编码）

> **注意：** Civilized 子目录已大量目录化。以下仅列**残留硬编码**。

#### Root

| 文件 | 硬编码 | 类型 |
|------|--------|------|
| `NecroGreeter.cs` | 1 | `robe.Name = "tattered robe"` |
| `Sherry.cs` | 1 | SendMessage `"She is too far away from you."` |
| `Chuckles.cs` | 4 | SendMessage（送帽子/衣服/金币+ `"Single click on it to enchant it."`）+ Title `"the Jester"` |
| `DraculaBride.cs` | 1 | Title `"the Countess of Gratz"` |
| `TownGuards.cs` | 1 | Title + 各种属性 |
| `Actor.cs` | 2 | Title |
| `Genie.cs` | 1 | PublicOverheadMessage |

**注意：** `CitizenLocalization.cs`、`TownHerald.cs`、`ShardGreeter.cs`、`DeathKnightDemon.cs` 已目录化。

#### Civilized/Merchants/（部分已目录化，残留以下）

| 文件 | 硬编码 | 类型 |
|------|--------|------|
| `AnimalTrainer.cs` | 4 | SendMessage（坐骑存储、稳定槽位） |
| `CustomHairstylist.cs` | 1 | SendMessage `"This isn't implemented for elves yet. Sorry!"` |
| `Priest.cs` | 1 | Say `"Bring light to the world with these, Jedi."` |
| `VarietyDealer.cs` | 1 | SendMessage `"You have gained a really large amount of fame."` |
| `Painter.cs` | 2 | `Title = "the " + GetSkillTitle( from )` |
| `StoneCrafter.cs` | 1 | `Title = "the stone crafter"` |
| `Scribe.cs` | 1 | PublicOverheadMessage |
| `Sage.cs` | 1 | PublicOverheadMessage |
| `Weaponsmith.cs` | 1 | Title 待确认 |
| `Miner.cs` | 1 | Title 待确认 |
| `Shipwright.cs` | `BuildingPropertyListLocale` 已目录化，有少量残留 |
| `Jester.cs` | `BuildingPropertyListLocale` 已目录化 |
| `Thief.cs` | `BuildingPropertyListLocale` 已目录化 |

**已目录化（无残留）：** Tailor（19）、Weaver（9）、Mapmaker（13）、Thief（7）、Jester（8）、Painter（5）、Shipwright（7）、InnKeeper（3）— `BuildingPropertyListLocale` + `AddLocalizedProperty` 完整。`Tradesman*.cs` 系列（Alchemist/Smith/Butcher/Leather/Miner/Cook/Logger）使用 `StringCatalog.ResolveByKey`。

#### Civilized/Citizens/

| 文件 | 硬编码 | 类型 |
|------|--------|------|
| `Humanoid.cs` | ~75 | Name =（所有人类 NPC 随机名 + 装备名） |
| `TavernPatrons.cs` | ~45 | Title 动态赋值（`"of the Dark"`, `"of the Vile"`, `"the Necromancer"`…） |
| `WorkingSpots.cs` | 1 | Name = |
| `MeetingPets.cs` | 1 | Name = |
| `TrainingSpirits.cs` | 3 | Say（`"*meditating*"`, `"Xtee Mee Glau"`, `"Anh Mi Sah Ko"`） |
| `TrainingMagery.cs` | 1 | Say（魔法咒语） |
| `TrainingFishing.cs` | 1 | Name = |

#### Civilized/Special/

| 文件 | 硬编码 | 类型 |
|------|--------|------|
| `EpicPet.cs` | 2 | Name = |
| `Kylearan.cs` | 1 | SendMessage `"A small chest has been added to your pack!"` |
| `MadGodPriest.cs` | 1 | Title + SendMessage |
| `Courier.cs` | 1 | Title + InfoText |
| `EpicCharacter.cs` | 已部分目录化（7 处 `StringCatalog.ResolveByKey`），剩余 SendMessage + Title |

#### Civilized/Porters/

`PorterItem.cs` — 已 5 处 `StringCatalog.ResolveByKey`，有少量 `list.Add` 残留。

#### Civilized/Familiars/

| 文件 | 硬编码 | 类型 |
|------|--------|------|
| `FamiliarItem.cs` | 3 | list.Add（`"Belongs To " + sOwner`） |
| `HoardMinionFamiliarItem.cs` | 1 | list.Add |
| `HoardMinionFamiliar.cs` | 1 | Name = |
| `DarkWolf.cs` | 1 | Name = |

#### Civilized/Guilds/（已全部目录化，除以下少数）

| 文件 | 硬编码 | 类型 |
|------|--------|------|
| `TinkerGuildmaster.cs` | 1 | list.Add 残留 |
| `ThiefGuildmaster.cs` | 1 | list.Add 残留 |

**以上所有 Guildmaster `BuildingPropertyListLocale` 已完整实现，仅 list.Add 可能残留。**

---

### Constructs/

#### Golems/

| 文件 | 硬编码 | 类型 |
|------|--------|------|
| `IronCobra.cs` | 1 | Name = `"an iron cobra"` + SendMessage + LocalOverheadMessage（毒击 + 石化） |
| `Golem.cs` | 2 | LocalOverheadMessage `"You have been stunned by a colossal blow!"` / `"You recover your senses."` |
| `FleshGolem.cs` | 2 | 同上 |
| `AncientFleshGolem.cs` | 2 | 同上 + `"You have been knocked senseless!"` |
| `MetalGolem.cs` | 2 | 同上 |
| `WoodenGolem.cs` | 2 | 同上 |
| `CaddelliteGolem.cs` | 2 | 同上 |
| `RustGolem.cs` | 4 | LocalOverheadMessage 锈损消息（与 Daemon 系列相同模式） |
| `RunicGolem.cs` | 2 | 同上 |
| `BoneGolem.cs` | 2 | 同上 |
| `CaddelliteGolem.cs` | 2 | 同上 |

#### Statues/

| 文件 | 硬编码 | 类型 |
|------|--------|------|
| `LivingStoneStatue.cs` | 2 | LocalOverheadMessage 眩晕 |
| `LivingIronStatue.cs` | 2 | 同上 |
| `LivingBronzeStatue.cs` | 2 | 同上 |
| `LivingSilverStatue.cs` | 2 | 同上 |
| `LivingGoldStatue.cs` | 2 | 同上 |
| `LivingJadeStatue.cs` | 2 | 同上 |
| `LivingMarbleStatue.cs` | 2 | 同上 |
| `LivingShadowIronStatue.cs` | 2 | 同上 |
| `AnyStatue.cs` | 2 | 同上 + Name = 动态 |
| `WaxSculpture.cs` | 1 | Name = + SendMessage + LocalOverheadMessage |

> **注意：** 所有 Statue 和 Golem 的 `"You have been stunned by a colossal blow!"` + `"You recover your senses."` 完全重复，可以通过一个 `StunMessage` 键统一处理。

#### Alien/

| 文件 | 硬编码 | 类型 |
|------|--------|------|
| `MaintenanceDroid.cs` | 1 | Title `"the maintenance droid"` |
| `ServiceDroid.cs` | 1 | Title `"the service droid"` |
| `SecurityDroid.cs` | 1 | Title |
| `BattleDroid.cs` | 1 | Title |
| `ExcavationDroid.cs` | 1 | Title + LocalOverheadMessage `"You have been sliced by a saw!"` |
| `Mutant.cs` | 1 | Name = |

---

### Demons/

| 文件 | 硬编码 | 主要类型 |
|------|--------|----------|
| `Daemon.cs` | ~100 | Name =（所有恶魔变体名，大量）+ LocalOverheadMessage 锈损 + Title |
| `Balron.cs` | 4 | LocalOverheadMessage 锈损 |
| `IceDevil.cs` | 5 | Title（5 种冰魔变体） |
| `DemonOfTheSea.cs` | 10 | Title（5 种男+5 种女海魔变体） |
| `AbysmalDaemon.cs` | 1 | Title `"the abysmal archfiend"` |
| `Imp.cs` | 3 | Title（`"the imp"`, `"the mephit"`, `"the quasit"`） |
| `BloodDemigod.cs` | 1 | SendMessage + Name = `"Chest of Bloody Relics"` |
| `BloodDemon.cs` | 6 | Title |
| `Succubus.cs` | 1 | SendMessage `"You feel the life drain out of you!"` |
| `Satan.cs` | 1 | SendMessage `"You have obtained Satan's Orb of the Abyss!"` |
| `Xurtzar.cs` | 1 | Title + 4 Name = |
| `Fiend.cs` | 1 | Title |
| `Archfiend.cs` | 1 | Title |
| `BlackGateDemon.cs` | 2 | Name = + Title |
| `LowerDemon.cs` | 1 | Name = |
| `MutantDaemon.cs` | 1 | Title |
| `Afreet.cs` | — | 待确认 |
| `Marilith.cs` | — | 待确认 |
| `Daemonic.cs` | — | 待确认 |

---

### Dragons/

#### Dragns/（Dragons/ 子目录）

| 文件 | 硬编码 | 主要类型 |
|------|--------|----------|
| `Dragons.cs` | — | LocalOverheadMessage 锈损 + Name = |
| `RidingDragon.cs` | 4 | LocalOverheadMessage 锈损 |
| `GemDragons.cs` | 2 | Name = |
| `Dragoon.cs` | 1 | Title |
| `DragonGolem.cs` | 1 | Name = |

#### Wyrms/

| 文件 | 硬编码 | 主要类型 |
|------|--------|----------|
| **`Wyrms.cs`** | **~200+** | **最大 Name/Title 集中源** — ~50 种 wyrm 变体名（`"the bloodstone wyrm"`～`"the agapite wyrm"`）+ `rName` + `rDwell` + `rFood` + `rBlood` + `egg.Name = "egg of " + Title` + `broke.Name = "rusted item"` + LocalOverheadMessage 锈损 |
| `ShadowWyrm.cs` | 1 | Title `"the shadow wyrm"` |
| `AncientWyrm.cs` | 1 | Title + Name = |
| `VolcanicDragon.cs` | 1 | Title |

#### Drakes/

| 文件 | 硬编码 |
|------|--------|
| `AncientDrake.cs` | 1 | Name = `"an ancient drake"` |
| `SeaDrake.cs` | 2 | Name = |

#### Great Dragons/

| 文件 | 硬编码 |
|------|--------|
| `AshDragon.cs` | 1 | Title `"of draconic ash"` |
| `CaddelliteDragon.cs` | 4 | Name = + Title |
| `CrystalDragon.cs` | 1 | Name = |
| `DragonKing.cs` | 1 | Name = + Title |
| `RadiationDragon.cs` | 1 | Title |
| `BottleDragon.cs` | 1 | Title |

#### Primeval/

| 文件 | 硬编码 |
|------|--------|
| 所有 `Primeval*Dragon.cs`（~12+） | 各 1 Title + Name = |
| `VampiricDragon.cs` | 2 | Title + SendMessage `"You feel the blood draining from you!"` |

#### Young/Hydras/Wyverns/

| 文件 | 硬编码 |
|------|--------|
| `YoungDragon.cs` | 4 | LocalOverheadMessage 锈损 |
| `Wyvra.cs` | 2 | Name = |
| `EnergyHydra.cs` | 1 | Title |
| `Hydra.cs` | 1 | Name = |

---

### Elementals/

#### Elementals/（子目录）

| 文件 | 硬编码 | 主要类型 |
|------|--------|----------|
| `AirElemental.cs` | 2 | LocalOverheadMessage 吹飞 |
| `DustElemental.cs` | 2 | LocalOverheadMessage 吹飞 |
| `KelpElemental.cs` | 2 | LocalOverheadMessage 缠绕 |
| `SeaweedElemental.cs` | 2 | LocalOverheadMessage 缠绕 |
| `WeedElemental.cs` | 2 | LocalOverheadMessage 缠绕 + Name = |
| `SewageElemental.cs` | 2 | LocalOverheadMessage + Name = |
| `DriftwoodElemental.cs` | 1 | Name = |
| `ForestElemental.cs` | 1 | Name = |
| `MudElemental.cs` | 1 | Name = |
| `LavaElemental.cs` | — | 待确认 |
| `SteamElemental.cs` | 1 | Name = |
| `ToxicElemental.cs` | 1 | Name = |
| `AnyElemental.cs` | 10 | Name = 随机元素名 + 属性 |
| `AnyGemElemental.cs` | 8 | Name = |
| `Vulcrum.cs` | 4 | Name = + Title `"of the flame"` |

#### Gemmed/

| 文件 | 硬编码 |
|------|--------|
| `ObsidianElemental.cs` | 1 | SendMessage + ColorText3 `"Worth X Gold"` |
| `SapphireElemental.cs` | 1 | Name = |
| `DilithiumElemental.cs` | 2 | Name = + `"dilithium crystals"` |
| `TrilithiumElemental.cs` | 2 | Name = |
| 其他 6 个 GemElemental | 各 1 | Name = |

#### Ore Elementals/

| 文件 | 硬编码 |
|------|--------|
| `AgapiteElemental.cs` | 1 | Name = |
| `CopperElemental.cs` | — | 待确认 |
| `ShadowIronElemental.cs` | 1 | Name = |
| `StoneElemental.cs` | 1 | Name = |

#### Lesser/

| 文件 | 硬编码 |
|------|--------|
| `WaterSpawn.cs` | 1 | Name = |

#### Root

| 文件 | 硬编码 |
|------|--------|
| `MudMan.cs` | 1 | SendMessage + Name = |
| `StormCloud.cs` | 2 | SendMessage + LocalOverheadMessage + Name = |
| `PoisonCloud.cs` | 1 | Name = |
| `SandVortex.cs` | 2 | LocalOverheadMessage |
| `Typhoon.cs` | 2 | LocalOverheadMessage |
| `Necromental.cs` | 2 | SendMessage（墓碑改名）+ 3 Name = |
| `Efreet.cs` | 1 | Name = |
| `IceColossus.cs` | 1 | Name = |

---

### Goliaths/

| 文件 | 硬编码 | 主要类型 |
|------|--------|----------|
| `ZornTheBlacksmith.cs` | 2 | Name = `"Zorn"` + `"Zorn's Caddellite Chest"` |
| `OrkDemigod.cs` | 1 | Title |
| `SandGiant.cs` | 1 | Name = + `MyChest.Name = "sapphire chest"` |
| `ElderTitan.cs` | 1 | Title `"the elder titan"` + `MyChest.Name = "ancient titan chest"` |
| `Giant.cs` | 1 | `MyChest.Name = "giant sack"` |
| 各巨人变体（~15+） | 各 1 | Title（`"the X giant"`） |
| Troll 系列（4 文件） | 各 1 | Name = + Title |
| Ettin 系列（4 文件） | 各 1 | Title |
| Ogre 系列（6 文件） | 各 1 | Title |
| Cyclops 系列（3 文件） | 各 1 | Title |

---

### Humanoids/（~200+ 文件，~300+ 处硬编码）

#### Root

| 文件 | 硬编码 | 主要类型 |
|------|--------|----------|
| `Goblin.cs` | ~2 | Name = + Title |
| `GoblinArcher.cs` | 1 | Name = |
| `Hobgoblin.cs` | 2 | Name = |
| `Morlock.cs` | 2 | Name = `"a morlock"` + `"throwing stone"` |
| `MindFlayer.cs` | 3 | Name = |
| `Medusa.cs` | 1 | SendMessage + LocalOverheadMessage（石化全链） |
| `Bugbear.cs` | 2 | Name = |
| `Yeti.cs` | 1 | Name = |

#### Humans/

| 文件 | 硬编码 | 主要类型 |
|------|--------|----------|
| `BlackKnight.cs` | 1 | Name = + InfoText1 |
| `EvilMage.cs` | 12 | Title |
| `EvilMageLord.cs` | 12 | Title |
| `Archmage.cs` | 1 | Title |
| `Bandit.cs` | 1 | Title |
| `Brigand.cs` | 2 | Title |
| `BloodAssassin.cs` | 1 | Name = |
| `GolemController.cs` | 1 | Title |

#### Savages/

| 文件 | 硬编码 | 主要类型 |
|------|--------|----------|
| `Native.cs` | 6 | Name = 装备名（`"Native Tunic"`, `"Native Gauntlets"`…） |
| `NativeArcher.cs` | 6 | 同上 + `"a tribesman"` |
| `Savage.cs` | 6 | 同上（`"dracosaur tunic"`…） |
| `SavageLord.cs` | 2 | Title + Name = |
| `SavageShaman.cs` | — | SendMessage 待确认 |
| `ZuluuNative.cs` | 7 | Name = |
| `ZuluuArcher.cs` | 7 | Name = |
| `NativeWitchDoctor.cs` | — | 待确认 |

#### Sailors/Pirates/

| 文件 | 硬编码 | 主要类型 |
|------|--------|----------|
| `PirateCaptain.cs` | 1 | Title |
| `PirateCrew.cs` | 1 | Title |
| `PirateCrewMage.cs` | 3 | Title |
| `PirateLand.cs` | 1 | Title |
| `ElfPirateCrew.cs` | 1 | Title |
| `ElfPirateCrewMage.cs` | 1 | Title |
| `ElfPirateCaptain.cs` | 1 | Title |
| `ElfPirateCrewBow.cs` | 1 | Title |
| `BoatPirates.cs` | 24 | Title（各种海盗头衔） |

#### Sailors/Galleons/

| 文件 | 硬编码 | 主要类型 |
|------|--------|----------|
| 所有 `Pirate*.cs`（~16 文件） | 各 1 | Title |
| `SailorMerchant.cs` | 1 | Title |
| `SailorElf.cs` | 1 | Title |
| `SailorAngel.cs` | 1 | Title |
| `SailorAngelLord.cs` | 1 | Title |

#### Lizardmen/Serpents/其他 Humanoids 子目录

大量 `Title = "the X"` 头衔在以下文件中：
- `Sleestax.cs`, `Sakleth.cs`, `SaklethArcher.cs`, `SaklethMage.cs`, `Reptalar.cs`, `ReptalarChieftain.cs`, `ReptalarShaman.cs`, `Reptaur.cs`, `Grathek.cs`, `Lizardman.cs`, `LizardmanArcher.cs` — 各 1 Title
- `Serpyn.cs`, `SerpynSorceress.cs`, `Serpentar.cs`, `SandSerpyn.cs`, `OphidianKnight.cs`, `OphidianMage.cs` — 各 1-2 Name + Title
- `Orc.cs`, `OrcCaptain.cs`, `OrcBomber.cs`, `OrcishLord.cs`, `OrcishMage.cs`, `Orx.cs`, `OrxWarrior.cs`, `Urk.cs`, `Urc.cs` 等 — 各 1-2 Title
- `ElfMage.cs`（12）、`ElfMinstrel.cs`（5）— Title
- `Drakkul.cs`, `DrakkulChief.cs`, `DrakkulMage.cs` — 各 1 Title
- `Kilrathi.cs` — Title
- `Gnome.cs`, `GnomeMage.cs` — Title
- `HarpyElder.cs`, `HarpyHen.cs`, `SnowHarpy.cs` — 各 3 Title
- `Minotaur.cs`, `MinotaurScout.cs`, `MutantMinotaur.cs` — 各 1-2 Name
- `Aliens/Jedi.cs` — 3 Title + 1 Say
- `Aliens/Syth.cs` — 2 Title + `"The Rule of One"`
- `Aliens/BombWorshipper.cs` — 4 Title（`"of the bomb"`, `"of the atom"`, `"the irradiated"`, `"of the glow"`）
- `Aliens/Psionicist.cs` — 7 Title

#### Sea/

| 文件 | 硬编码 |
|------|--------|
| `Neptar.cs` | 1 | Name = |
| `NeptarWizard.cs` | 1 | Name = |
| `Krakoa.cs` | 1 | Name = |
| `Dagon.cs` | 1 | Title |
| `Locathah.cs` | 1 | Name = |
| `SeaHag.cs` | 1 | Title |
| `Lobstran.cs` | — | 待确认 |

---

### Undead/（~60+ 文件，~100+ 处硬编码）

| 文件 | 硬编码 | 主要类型 |
|------|--------|----------|
| **`Vordo.cs`** | 7 | SendMessage（6 处剧情/法术消息）+ list.Add（2 处 OPL）+ InfoText1 |
| `Undead.cs` | 20 | Name =（各种不死生物名） |
| `UndeadGiant.cs` | 2 | Name = |
| `DemiLich.cs` | 3 | Title（`"the demilich"`, `"the crypt thing"`, `"the dark lich"`） |
| `Vampire.cs` | 2 | Title + SendMessage + Name = |
| `VampireLord.cs` | 4 | Title + SendMessage |
| `VampirePrince.cs` | 1 | Title（5 级变体）+ SendMessage |
| `VampireWoods.cs` | 1 | SendMessage |
| `Dracula.cs` | 3 | SendMessage + Name = |
| `Dracolich.cs` | 1 | Title + InfoText1 + Name = |
| `DiseasedMummy.cs` | 1 | LocalOverheadMessage |
| `Ghoul.cs` | 1 | LocalOverheadMessage（麻痹爪） |
| `IceGhoul.cs` | 2 | LocalOverheadMessage |
| `AquaticGhoul.cs` | 2 | LocalOverheadMessage |
| `GhostWarrior.cs` | 9 | Name = |
| `Ghostly.cs` | 6 | Name = |
| `BoneSailor.cs` | 6 | Name = + Title |
| `BoneSlasher.cs` | 1 | Name = + `"bone carved chest"` |
| `RottingCorpse.cs` | 6 | Name = |
| `DeadWizard.cs` | 12 | Name = |
| `GrundulVarg.cs` | 1 | Title + Name = + InfoText1 |
| `Kull.cs` | 1 | Title + InfoText1 |
| `Murk.cs` | 1 | InfoText1 |
| `LostKnight.cs` | 1 | InfoText1 |
| `Zombie.cs` | 10 | Name = |
| `Nazghoul.cs` | 1 | Title |
| 其他 Skeleton/GargoyleBones/SpectralGargoyle/Shroud 等 | 各 1 | Name = |

---

### Unique/（~30+ 文件，~60+ 处硬编码）

| 文件 | 硬编码 | 主要类型 |
|------|--------|----------|
| `TitanStratos.cs` | 3 | SendMessage（3）+ Name = `"Stratos"` + `"Chest of Air Titan Relics"` |
| `TitanPyros.cs` | 3 | SendMessage（3）+ Name = |
| `TitanLithos.cs` | 3 | SendMessage（3）+ Title `"the titan of earth"` + Name = |
| `TitanHydros.cs` | 3 | SendMessage（3）+ Name = |
| `Shadowlord.cs` | 8 | SendMessage（饰品消失 + 碎片获取 + 背包物品共 8 条） |
| `BaneOfAnarchy.cs` | 2 | SendMessage（天平消失 + 秩序蛇） |
| `BaneOfInsanity.cs` | 2 | SendMessage（宝珠消失 + 混沌蛇）+ Title |
| `BaneOfWantoness.cs` | 2 | SendMessage（灯笼消失 + 平衡蛇）+ Title |
| `SerpentOfOrder.cs` | 1 | SendMessage `"You have subdued the Serpent of Order!"` |
| `SerpentOfChaos.cs` | 1 | SendMessage `"You have subdued the Serpent of Chaos!"` |
| `Mangar.cs` | 2 | SendMessage（2 处传送门）+ Title + 14 Name = |
| `Tarjan.cs` | 1 | SendMessage + Title `"the mad god"` |
| `Exodus.cs` | 2 | SendMessage + InfoText |
| `RuneGuardian.cs` | 2 | SendMessage + list.Add |
| `SlasherOfVoid.cs` | 1 | Title |
| `KhumashGor.cs` | 1 | Name = + Title |
| `CodexGargoyles.cs` | 1 | Title |
| `BaronAlmric.cs` | 1 | InfoText2（`"Slain by Baron Almric"`）×6 |
| `GrayDragon.cs` | — | PublicOverheadMessage + Name = 待确认 |
| `Spectres.cs` | — | PublicOverheadMessage 待确认 |
| `Surtaz.cs` | — | Title + PublicOverheadMessage 待确认 |

---

### Slimes/

| 文件 | 硬编码 | 主要类型 |
|------|--------|----------|
| `Slime.cs` | 2 | LocalOverheadMessage（覆盖物保护/损坏） |
| `GreenSlime.cs` | 2 | 同上 |
| `BlackPudding.cs` | 2 | 同上 |
| `OilSlick.cs` | 2 | LocalOverheadMessage（油覆盖）+ SendMessage |
| `BloodWorm.cs` | 2 | LocalOverheadMessage（吸血）+ Name = |
| `MarshWurm.cs` | 2 | LocalOverheadMessage（吸血）+ SendMessage |
| `GiantLeech.cs` | 2 | LocalOverheadMessage（吸血）+ SendMessage |
| `Jellyfish.cs` | 1 | Name = |
| `FrostOoze.cs` | 1 | Name = |
| `Viscera.cs` | 2 | LocalOverheadMessage |
| `CarcassWorm.cs` | — | 待确认 |

---

### Animals/

| 子目录 | 文件数 | 硬编码示例 |
|--------|--------|-----------|
| Bears/ | ~5 | Name = `"a brown bear"`… |
| Birds/ | ~8 | Name = `"a turkey"`, `"an eagle"`, `"a tropical bird"`… |
| Canines/ | ~6 | Name = `"a wolf man"`, `"a white wolf"`… + `WereWolf.cs` 11 Title |
| Cows/ | 小 | Name = |
| Felines/ | ~4 | Name = `"a cat"`, `"a cougar"`, `"a jaguar"`, `"a black cat"` |
| Misc/ | ~15 | Name = `"a boar"`, `"a llama"`, `"a gorilla"`… |
| Mounts/ | ~10 | Name = + Ethereals.cs 9 SendMessage + 13 Name = |
| Rodents/ | ~8 | Name = `"a rabbit"`, `"a giant rat"`, `"a giant bat"`… + Critter.cs 7 Name = |

> **处理建议：** Animals/ 的 `Name = "a X"` 基本模式极为统一，建议批量处理：使用 `IsContentLocalized` + `AddLocalizedProperty("item.creature.X")` 或精简为一套资源短语键。

---

### Mystical/

| 文件 | 硬编码 | 主要类型 |
|------|--------|----------|
| `Centaur.cs` | 1 | Title |
| `CorruptCentaur.cs` | 1 | Title |
| `MysticalFox.cs` | 2 | Name = |
| `RoyalSphinx.cs` | 1 | Name = + Title + SendMessage |
| `AncientSphinx.cs` | 1 | Title + SendMessage |
| `SphinxRiding.cs` | 1 | Title + SendMessage |
| `Unicorn.cs` | 1 | LocalOverheadMessage |
| `Kirin.cs` | 1 | LocalOverheadMessage |
| `Pegasus.cs` | 1 | Name = |
| `GuardianWolf.cs` | 1 | Name = |
| `Placeron.cs` | 1 | Name = |
| `Reptalon.cs` | — | 待确认 |
| `Sunlyte.cs` | — | 待确认 |
| `ShadowWisp.cs` | 1 | Name = |
| `DarkWisp.cs` | 1 | Name = |
| `Faerie.cs` | 1 | Title |
| `Sprite.cs` | 1 | Title |
| `xDryad.cs` | 1 | Name = |
| `MLDryad.cs` | 1 | Name = |
| `Xatyr.cs` | 1 | Name = |

---

### Plants/

| 文件 | 硬编码 | 主要类型 |
|------|--------|----------|
| `BloodLotus.cs` | 1 | SendMessage + LocalOverheadMessage |
| `Ent.cs` | 1 | Name = |
| `EvilEnt.cs` | 1 | Title |
| `AncientEnt.cs` | 1 | Title |
| `TheAncientTree.cs` | 1 | Title |
| `AncientReaper.cs` | 1 | Title |
| `Reaper.cs` | 1 | Name = |
| `DeadReaper.cs` | 1 | Name = |
| `SwampThing.cs` | 1 | Name = |
| `BogThing.cs` | 1 | Name = |
| `WhippingVine.cs` | 1 | Name = |
| `Corpser.cs` | 2 | Name = |
| `Fungal.cs` | 1 | Name = |
| `SeaWeeder.cs` | 1 | Name = |

---

### Reptilian/

| 子目录 | 文件 | 硬编码 | 主要类型 |
|--------|------|--------|----------|
| Fish/ | `Megalodon.cs` | 1 | Name = |
| Fish/ | `GreatWhite.cs` | 1 | Name = |
| Fish/ | `Shark.cs` | 1 | Name = |
| Dinosaurs/ | 各 | 1 | Name = |
| Frogs/ | `Frog.cs` | 1 | Name = |
| Snakes/ | `BloodSnake.cs` | 1 | SendMessage + Name = |
| Snakes/ | `Jormungand.cs` | 1 | Title |
| Sea/ | `Leviathan.cs` | 2 | SendMessage |
| Sea/ | `GiantEel.cs` | 1 | SendMessage |
| Sea/ | `GiantLamprey.cs` | 1 | SendMessage |
| Sea/ | `Slitheran.cs` | 2 | LocalOverheadMessage（黏液覆盖） |
| Sea/ | `SwampDragon.cs` | — | list.Add 待确认 |
| Root | `BasiliskRiding.cs` | 1 | SendMessage + LocalOverheadMessage（石化） |
| Root | 其他~20 文件 | 各 1 | Name = |

---

### Unusual/

| 文件 | 硬编码 | 主要类型 |
|------|--------|----------|
| `Beholder.cs` | 2 | Title |
| `Gazer.cs` | — | 待确认 |
| `Xorn.cs` | 1 | SendMessage + Name = |
| `SoulSucker.cs` | 1 | SendMessage |
| `GorgonRiding.cs` | 1 | SendMessage + LocalOverheadMessage |
| `Watcher.cs` | 1 | Name = |
| `EyeOfTheDeep.cs` | 1 | Name = |
| `Xenomorph.cs` | 2 | Name = |
| `BloodGodTentacles.cs` | 1 | Name = |

---

### Summoned/

| 文件 | 硬编码 | 主要类型 |
|------|--------|----------|
| `WineElemental.cs` | 1 | Name = |
| `ManureGolem.cs` | 1 | Name = |
| `SummonedFireElementalGreater.cs` | 1 | Name = |
| `SummonedWaterElemental.cs` | 1 | Name = |
| `SummonedDaemon.cs` | 1 | Title |
| `SummonedDaemonGreater.cs` | 1 | Title |
| `SummonedTiger.cs` | 1 | Name = |
| `BladeSpirits.cs` | — | 待确认 |
| `IceBladeSpirits.cs` | 1 | Name = |
| `GasCloud.cs` | — | 待确认 |
| `DeathVortex.cs` | 1 | Name = |

---

### Hellish/

| 文件 | 硬编码 |
|------|--------|
| `Cerberus.cs` | 1 | Name = |
| `Chimera.cs` | 1 | Name = |

---

### Omni AI/

| 文件 | 硬编码 | 主要类型 |
|------|--------|----------|
| `OmniAI Bard.cs` | 1 | list.Add |
| `OmniAI Core.cs` | 1 | list.Add |
| `OmniAI Ninjitsu.cs` | — | 待确认 |
| `AITester.cs` | 1 | Name = |

---

### Races/

| 文件 | 硬编码 | 主要类型 |
|------|--------|----------|
| `RacePotions.cs` | 1 | Gump AddHtml(racepotions 分类标签) — **已通过 `RaceLocalization.Key` 目录化** |
| `BaseRace.cs` | 27 | list.Add（种族属性 OPL 行）— 残留，部分已目录化 |

---

## 已目录化（仅 cliloc / `StringCatalog` 已覆盖）

以下文件含 cliloc 或已有 `StringCatalog` 入口，**不在**上述待办范围内：

- `Civilized/Guilds/*.cs`（全部 Guildmaster）— `BuildingPropertyListLocale` + `AddLocalizedProperty`
- `Civilized/Merchants/Tailor.cs`, `Weaver.cs`, `Mapmaker.cs`, `Thief.cs`, `Jester.cs`, `Painter.cs`, `Shipwright.cs`, `InnKeeper.cs` — `StringCatalog.ResolveByKey`
- `Civilized/Merchants/` 其他文件已部分目录化（详见上述明细）
- `Civilized/Special/LostItemsRestorerNPC.cs` — `charrestore.*` 键
- `Civilized/Special/EpicCharacter.cs` — 部分 `StringCatalog.ResolveByKey`
- `Civilized/DeathKnightDemon.cs` — `StringCatalog.ResolveByKey`
- `Civilized/TownHerald.cs` — `StringCatalog.ResolveByKey`
- `Civilized/ShardGreeter.cs` — `StringCatalog.Resolve`
- `Civilized/Chuckles.cs` — 部分 `StringCatalog.ResolveByKey`
- `Civilized/Comrades/HenchmanFunctions.cs` — `StringCatalog.ResolveByKey`
- `Civilized/Porters/PorterItem.cs` — `StringCatalog.ResolveByKey`
- `Civilized/Citizens/Tradesman*.cs` — `StringCatalog.ResolveByKey`
- `Civilized/Familiars/FamiliarItem.cs` — 部分 `StringCatalog.ResolveByKey`
- `Base/PlayerBarkeeper.cs` — 部分 `StringCatalog`
- `Races/RacePotions.cs` — `RaceLocalization.Key`
- `Civilized/CitizenLocalization.cs` — `StringCatalog.TryResolveByKey`

---

## 无英文硬编码（或仅 cliloc / 已目录化）

以下目录/文件已确认无残留硬编码：

- `Civilized/Guilds/` — 全部 Guildmaster（有少量 `list.Add` 残留见上述明细）
- `Civilized/Merchants/Tailor.cs`, `Weaver.cs`, `Mapmaker.cs`, `Thief.cs`, `Jester.cs`, `Painter.cs` — `BuildingPropertyListLocale` 完整
- `Civilized/Citizens/Tradesman*.cs` 系列 — `StringCatalog.ResolveByKey` 完整（TradesmanButcher/Cook/Smith/Alchemist/Leather/Miner/Logger）
- 大量 Animals/ 基础生物（如 `Gems/` 类型无关）— 仅 `Name =` 待处理，无其他硬编码

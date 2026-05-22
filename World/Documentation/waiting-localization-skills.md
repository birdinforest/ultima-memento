# 待中文化：Skills 目录扫描

> **扫描日期：** 2026-05-22
> **处理范围：** **处理所有英文硬编码文本**（`SendMessage`、`Say`、Gump、`TextDefinition.AddHtmlText`、`AddRow`、OPL、`Name =`、字符串赋值、长段拼接模板等）。
> **排除范围：** **不处理 cliloc 控制的文本**（`SendLocalizedMessage`、仅 cliloc 数字的 `*OverheadMessage`）。已通过 `ResolveText` / `StringCatalog` 且键已在 locale 中的行亦不计入。
> **扫描路径：** `World/Source/Scripts/System/Skills`
> **关联：** [`waiting-localization.md`](waiting-localization.md) · `AGENTS.md` §3.2

## 摘要

| 指标 | 数量 |
|------|------|
| 扫描 `.cs` 文件 | 88 |
| **含英文硬编码（待处理）** | **51** |
| 无英文硬编码（或仅 cliloc / 已目录化） | 37 |

**说明：** 同一文件可同时含 cliloc 与硬编码（如 `Druidism.cs`：cliloc 提示不处理，`DruidismGump` 内 `MONSTER MANUAL` 等须目录化）。

### SendMessage 硬编码（P0，须目录化）

下列写法对 **zh-Hans 账号会直接显示英文**，与 cliloc **无关**，**必须**纳入待办并改为 `StringCatalog.ResolveByKey` / `ResolveFormatByKey`（见 `AGENTS.md` §3.2；带 hue 时 hue 仅着色，**字符串仍须目录化**）：

| 形态 | 示例（源码） | 待办文档中的位置 |
|------|----------------|------------------|
| `SendMessage("…")` | `Begging.cs:178` — `SendMessage("You had no chance at begging this creature from hurting you.")` | 见下表 `Begging.cs` 明细 |
| `SendMessage(hue, "…")` | `Begging.cs:152` — `SendMessage(68, "You set your demeanor to begging.")` | 同上 |
| `SendMessage("…", arg)` | `Discordance.cs:194` — 技能门槛提示 | 见 `Discordance.cs` |
| `SendMessage(0x…, "…")` | `ArmsLore.cs:56` — 耐久提示 | 见 `ArmsLore.cs` |

**不属于待办：** `SendLocalizedMessage(502789)` 等 cliloc；`SendMessage(ResolveText(from, "…"))` / `StringCatalog.*` 且键已在 locale JSON 中。

Skills 目录内 **`SendMessage` + 英文字面量** 约 **120+ 处**，分布在本文「待处理文件总表」中带 `SendMessage` 类型的文件中（含 `Begging.cs` 共 12 处 `SendMessage` + 18 处 `Say`）。

**复查命令：**

```bash
rg 'SendMessage\s*\(\s*(\d+|0x[0-9a-fA-F]+)\s*,\s*"' World/Source/Scripts/System/Skills
rg 'SendMessage\s*\(\s*"' World/Source/Scripts/System/Skills
rg '"[A-Za-z]{3,}' World/Source/Scripts/System/Skills
```

## 待处理文件总表

| 文件 | 硬编码条数 | 含 cliloc | StringCatalog | 主要类型 |
|------|------------|-----------|---------------|----------|
| `Weapon Abilities/AbilityBook.cs` | 140 | — | 部分 | Gump, Item Name, String assign |
| `Druidism.cs` | 89 | 是 | 否 | Gump AddRow, String assign |
| `Weapon Abilities/SpecialAttackGump.cs` | 61 | — | 否 | SendMessage, String assign, String literal |
| `Tracking.cs` | 45 | 是 | 部分 | Gump, Return string, String literal |
| `Begging.cs` | 30 | 是 | 否 | Say, SendMessage |
| `Stealing.cs` | 25 | 是 | 否 | Overhead, SendMessage, String literal |
| `Weapon Abilities/SpecialAttackCommands.cs` | 25 | — | 否 | String literal |
| `Searching.cs` | 9 | 是 | 部分 | Return string |
| `Spiritualism.cs` | 8 | 是 | 否 | Say, SendMessage, String literal |
| `Forensics.cs` | 7 | 是 | 否 | SendMessage |
| `Healing.cs` | 7 | 是 | 否 | SendMessage |
| `Peacemaking.cs` | 7 | 是 | 否 | SendMessage |
| `Weapon Abilities/SpecialAttacksDisplay.cs` | 6 | — | 否 | SendMessage, String literal |
| `Taming.cs` | 5 | 是 | 否 | Overhead |
| `Weapon Abilities/Extra/ShadowInfectiousStrike.cs` | 5 | 是 | 否 | SendMessage |
| `Discordance.cs` | 4 | 是 | 否 | SendMessage |
| `Tasting.cs` | 4 | 是 | 否 | SendMessage |
| `Weapon Abilities/LightningArrow.cs` | 4 | — | 否 | SendMessage |
| `Parrying.cs` | 3 | — | 否 | SendMessage |
| `Poisoning.cs` | 3 | 是 | 否 | SendMessage |
| `Weapon Abilities/Extra/ZapDexStrike.cs` | 3 | — | 否 | SendMessage, String literal |
| `Weapon Abilities/Extra/ZapIntStrike.cs` | 3 | — | 否 | SendMessage, String literal |
| `Weapon Abilities/Extra/ZapStrStrike.cs` | 3 | — | 否 | SendMessage, String literal |
| `Provocation.cs` | 2 | 是 | 否 | SendMessage |
| `Snooping.cs` | 2 | 是 | 否 | SendMessage, String literal |
| `Weapon Abilities/Extra/AchiliesStrike.cs` | 2 | — | 否 | SendMessage |
| `Weapon Abilities/Extra/DeathBlow.cs` | 2 | — | 否 | SendMessage |
| `Weapon Abilities/Extra/DevestatingBlow.cs` | 2 | — | 否 | SendMessage |
| `Weapon Abilities/Extra/EarthStrike.cs` | 2 | — | 否 | SendMessage |
| `Weapon Abilities/Extra/ElementalStrike.cs` | 2 | — | 否 | SendMessage |
| `Weapon Abilities/Extra/FireStrike.cs` | 2 | — | 否 | SendMessage |
| `Weapon Abilities/Extra/FistsOfFury.cs` | 2 | — | 否 | SendMessage |
| `Weapon Abilities/Extra/FreezeStrike.cs` | 2 | — | 否 | SendMessage |
| `Weapon Abilities/Extra/LightningStrike.cs` | 2 | — | 否 | SendMessage |
| `Weapon Abilities/Extra/SpinAttack.cs` | 2 | — | 否 | SendMessage |
| `Weapon Abilities/Extra/StunningStrike.cs` | 2 | — | 否 | SendMessage |
| `Weapon Abilities/Extra/ToxicStrike.cs` | 2 | — | 否 | SendMessage |
| `Weapon Abilities/Extra/ZapManaStrike.cs` | 2 | — | 否 | SendMessage |
| `Weapon Abilities/Extra/ZapStamStrike.cs` | 2 | — | 否 | SendMessage |
| `Weapon Abilities/InfectiousStrike.cs` | 2 | 是 | 否 | SendMessage |
| `Weapon Abilities/SerpentArrow.cs` | 2 | — | 否 | SendMessage |
| `ArmsLore.cs` | 1 | 是 | 否 | SendMessage |
| `Weapon Abilities/Dismount.cs` | 1 | 是 | 否 | SendMessage |
| `Weapon Abilities/Extra/ConsecratedStrike.cs` | 1 | — | 否 | SendMessage |
| `Weapon Abilities/Extra/DoubleWhirlwindAttack.cs` | 1 | 是 | 否 | SendMessage |
| `Weapon Abilities/Extra/MagicProtection.cs` | 1 | — | 否 | SendMessage |
| `Weapon Abilities/Extra/MagicProtection2.cs` | 1 | — | 否 | SendMessage |
| `Weapon Abilities/Extra/MeleeProtection.cs` | 1 | — | 否 | SendMessage |
| `Weapon Abilities/Extra/MeleeProtection2.cs` | 1 | — | 否 | SendMessage |
| `Weapon Abilities/Extra/RidingAttack.cs` | 1 | 是 | 否 | SendMessage |
| `Weapon Abilities/Feint.cs` | 1 | 是 | 否 | SendMessage |

## 建议修复批次

| 批次 | 范围 |
|------|------|
| **B0** | **`Druidism.cs`** — `DruidismGump`（89 处：标题/分区/属性/心情/食物等） |
| B1 | `Healing`、`Stealing`、`Taming`、`Begging`、`Forensics`、`ArmsLore` |
| B2 | `Peacemaking`、`Provocation`、`Discordance` |
| B3 | `Tracking.cs` Gump 分类树 |
| B4 | `Weapon Abilities/Extra/*`、`SpecialAttackGump.cs`、`SpecialAttackCommands.cs` |
| B5 | `AbilityBook.cs` 前言 Gump + 物品名 |

## 按模块明细

### (root)

#### `ArmsLore.cs`
- cliloc 部分：**不处理**

| 行 | 类型 | 示例 |
|----|------|------|
| 56 | SendMessage | You notice your equipment is especially durable. |

#### `Begging.cs`
- cliloc 部分：**不处理**

| 行 | 类型 | 示例 |
|----|------|------|
| 152 | SendMessage | You set your demeanor to begging. |
| 157 | SendMessage | You cease your demeanor of begging. |
| 165 | Say | Leave me alone! |
| 166 | Say | Have mercy! |
| 167 | Say | Please, I am but a puny worm! |
| 168 | Say | Go away! |
| 169 | Say | I submit to your might! |
| 170 | Say | Your power has me scared! |
| 171 | Say | Leave me be! |
| 172 | Say | I didn't want to hurt you! |
| 173 | Say | Don't hurt me! |
| 178 | SendMessage | You had no chance at begging this creature from hurting you. |
| 182 | SendMessage | This creature is already leaving you alone. |
| 194 | SendMessage | You fail to convince them to leave you alone. |
| 203 | SendMessage | You beg and plead enough for them to leave you alone. |
| 219 | SendMessage | You beg and plead enough for them to leave you alone. |
| 220 | SendMessage | They somehow begged and pleaded, convincing you to leave them alone. |
| 238 | Say | Leave me alone! |
| 239 | Say | Have mercy! |
| 240 | Say | Please, I am but a puny worm! |
| 241 | Say | Go away! |
| 242 | Say | I submit to your might! |
| 243 | Say | Your power has me scared! |
| 244 | Say | Leave me be! |
| 245 | Say | I didn't want to hurt you! |
| … | … | 另有 5 条 |

#### `Discordance.cs`
- cliloc 部分：**不处理**

| 行 | 类型 | 示例 |
|----|------|------|
| 194 | SendMessage | You need at least '{0}' Discordance skill to disrupt the target. |
| 214 | SendMessage | You magically resist the affects of the song. |
| 228 | SendMessage | Your fingers fumble, but you daze the target. |
| 235 | SendMessage | You hear jarring music, suppressing your abilities. |

#### `Druidism.cs`
- cliloc 部分：**不处理**

| 行 | 类型 | 示例 |
|----|------|------|
| 315 | String assign | MONSTER MANUAL |
| 323 | String assign | PLAYERS HANDBOOK |
| 329 | String assign | ANIMAL LORE |
| 335 | String assign | DIVINATION |
| 341 | String assign | DIVINATION |
| 359 | Gump AddRow | INFORMATION |
| 365 | Gump AddRow | TAME |
| 371 | Gump AddRow | FAVORITE FOOD |
| 380 | Gump AddRow | DAMAGE |
| 384 | Gump AddRow | COMBAT RATINGS |
| 388 | Gump AddRow | LORE & KNOWLEDGE |
| 395 | Gump AddRow | RESISTANCE |
| 399 | Gump AddRow | STATS |
| 460 | Gump AddRow | Hits |
| 461 | Gump AddRow | Stamina |
| 462 | Gump AddRow | Mana |
| 463 | Gump AddRow | Strength |
| 464 | Gump AddRow | Dexterity |
| 465 | Gump AddRow | Intelligence |
| 470 | Gump AddRow | Physical |
| 471 | Gump AddRow | Fire |
| 472 | Gump AddRow | Cold |
| 473 | Gump AddRow | Poison |
| 474 | Gump AddRow | Energy |
| 475 | Gump AddRow | Base Damage |
| … | … | 另有 64 条 |

#### `Forensics.cs`
- cliloc 部分：**不处理**

| 行 | 类型 | 示例 |
|----|------|------|
| 56 | SendMessage | It seems that  |
| 56 | SendMessage |  has robbed this coffer of it's gold! |
| 60 | SendMessage | That coffer has not been emptied by thieves. |
| 65 | SendMessage | This adventurer looks to have been slain by some wild animal. |
| 69 | SendMessage | For some reason, this wagon was left behind. |
| 73 | SendMessage | Maybe the owner of this boat fell into the sea and drowned. |
| 77 | SendMessage | This ship looks as though it seen better days. |

#### `Healing.cs`
- cliloc 部分：**不处理**

| 行 | 类型 | 示例 |
|----|------|------|
| 17 | SendMessage | You are starving to death and cannot do that! |
| 28 | SendMessage | You feel a little healthier. |
| 29 | SendMessage | The infection begins to clear. |
| 30 | SendMessage | You work quickly to stem the bleeding! |
| 43 | SendMessage | You are distracted, but heal some wounds. |
| 48 | SendMessage | You focus intently and heal your wounds. |
| 58 | SendMessage | You already feel healthy. |

#### `Parrying.cs`

| 行 | 类型 | 示例 |
|----|------|------|
| 21 | SendMessage | You can only parry with a shield! |
| 46 | SendMessage | You raise your shield in preparation. |
| 58 | SendMessage | You relax your stance. |

#### `Peacemaking.cs`
- cliloc 部分：**不处理**

| 行 | 类型 | 示例 |
|----|------|------|
| 28 | SendMessage | Choose someone to calm or choose yourself to calm everyone in the nearby area. |
| 118 | SendMessage | Your attempt to calm  |
| 118 | SendMessage |  failed, causing your song to cease. |
| 121 | SendMessage | You attempt to calm  |
| 121 | SendMessage | , but fail. |
| 128 | SendMessage | You play hypnotic music, calming  |
| 185 | SendMessage | You need at least '{0}' Peacemaking skill to pacify the target. |

#### `Poisoning.cs`
- cliloc 部分：**不处理**

| 行 | 类型 | 示例 |
|----|------|------|
| 89 | SendMessage | You can only poison slashing or piercing weapons. |
| 93 | SendMessage | You can only poison one-handed slashing or piercing weapons. |
| 108 | SendMessage | You cannot poison that! You can only poison certain weapons, food, or drink. |

#### `Provocation.cs`
- cliloc 部分：**不处理**

| 行 | 类型 | 示例 |
|----|------|------|
| 130 | SendMessage | You need at least '{0}' Provocation skill to incite the target. |
| 185 | SendMessage | You need at least '{0}' Provocation skill to incite the target. |

#### `Searching.cs`
- cliloc 部分：**不处理**
- 已部分 `StringCatalog` / `ResolveText`

| 行 | 类型 | 示例 |
|----|------|------|
| 281 | Return string | north |
| 282 | Return string | northeast |
| 283 | Return string | east |
| 284 | Return string | southeast |
| 285 | Return string | south |
| 286 | Return string | southwest |
| 287 | Return string | west |
| 288 | Return string | northwest |
| 289 | Return string | nearby |

#### `Snooping.cs`
- cliloc 部分：**不处理**

| 行 | 类型 | 示例 |
|----|------|------|
| 27 | SendMessage | You cannot snoop while in this state. |
| 78 | String literal | You notice {0} attempting to peek into {1}'s belongings. |

#### `Spiritualism.cs`
- cliloc 部分：**不处理**

| 行 | 类型 | 示例 |
|----|------|------|
| 77 | String literal | Spiritualism |
| 132 | Say | Xtee Mee Glau |
| 140 | Say | Anh Mi Sah Ko |
| 169 | String literal | You channel the corpse's energy to restore yourself. |
| 176 | String literal | You channel your spiritual energy to restore yourself. |
| 185 | SendMessage | You cannot do that while poison is in your veins! |
| 189 | SendMessage | You are starving to death and cannot do that! |
| 193 | SendMessage | You are dying of thirst and cannot do that! |

#### `Stealing.cs`
- cliloc 部分：**不处理**

| 行 | 类型 | 示例 |
|----|------|------|
| 51 | SendMessage | You cannot steal that while in this state. |
| 90 | Overhead | I have already stolen that item! |
| 100 | SendMessage | You cannot steal while in this state. |
| 104 | SendMessage | It is best to leave the dead be. |
| 108 | SendMessage | You have not use for this broken golem thing. |
| 115 | SendMessage | You dump out the entire contents while stealing the item. |
| 147 | String literal | You were not quick enough to steal it. |
| 152 | String literal | You make sure that container won't make a fool of you again. |
| 153 | String literal | In a fit of rage, you throw the container. |
| 154 | String literal | Well, they won't get your fingerprints off that. |
| 155 | String literal | You destroy the evidence of your failure. |
| 174 | SendMessage | It is best to leave the dead be. |
| 178 | SendMessage | You would be quite foolish looking stealing a wagon. |
| 182 | SendMessage | You are just not that strong. |
| 186 | SendMessage | You cannot be wielding a weapon when trying to steal something. |
| 207 | Overhead | You found  |
| 219 | SendMessage | There seems to be no gold in the coffer. |
| 223 | SendMessage | You slip out  |
| 223 | SendMessage |  gold from the coffer. |
| 233 | String literal |  the  |
| 238 | SendMessage | You fingers slip, causing you to get noticed! |
| 249 | Overhead | Stop! Thief! |
| 323 | SendMessage | That is too heavy to steal. |
| 453 | String literal | You notice {0} trying to steal from {1}. |
| 512 | SendMessage | You cannot be wielding a weapon when trying to steal something. |

#### `Taming.cs`
- cliloc 部分：**不处理**

| 行 | 类型 | 示例 |
|----|------|------|
| 301 | Overhead | Easy...easy... |
| 302 | Overhead | Don't be afraid... |
| 303 | Overhead | I won't hurt you... |
| 304 | Overhead | See? Nothing to be afraid of... |
| 305 | Overhead | Nice and easy... |

#### `Tasting.cs`
- cliloc 部分：**不处理**

| 行 | 类型 | 示例 |
|----|------|------|
| 68 | SendMessage | You bit off a bit too much! |
| 73 | SendMessage | This food looks safe to eat. |
| 101 | SendMessage | You swallowed a bit too much! |
| 106 | SendMessage | This liquid looks safe to drink. |

#### `Tracking.cs`
- cliloc 部分：**不处理**
- 已部分 `StringCatalog` / `ResolveText`

| 行 | 类型 | 示例 |
|----|------|------|
| 88 | Return string | north |
| 89 | Return string | northeast |
| 90 | Return string | east |
| 91 | Return string | southeast |
| 92 | Return string | south |
| 93 | Return string | southwest |
| 94 | Return string | west |
| 95 | Return string | northwest |
| 96 | Return string | nearby |
| 123 | Gump | >TRACKING</BASEFONT></BODY> |
| 128 | Gump | >Abysmal</BASEFONT></BODY> |
| 131 | Gump | >Daemons</BASEFONT></BODY> |
| 134 | Gump | >Devils</BASEFONT></BODY> |
| 137 | Gump | >Gargoyles</BASEFONT></BODY> |
| 140 | Gump | >Animals</BASEFONT></BODY> |
| 143 | Gump | >Arachnids</BASEFONT></BODY> |
| 146 | Gump | >Arachnoids</BASEFONT></BODY> |
| 149 | Gump | >Scorpions</BASEFONT></BODY> |
| 152 | Gump | >Spiders</BASEFONT></BODY> |
| 155 | Gump | >Avians</BASEFONT></BODY> |
| 158 | Gump | >Elementals</BASEFONT></BODY> |
| 161 | Gump | >Fey</BASEFONT></BODY> |
| 164 | Gump | >Giants</BASEFONT></BODY> |
| 167 | Gump | >Golems</BASEFONT></BODY> |
| 170 | Gump | >Monsters (General)</BASEFONT></BODY> |
| … | … | 另有 20 条 |

### Weapon Abilities

#### `Weapon Abilities/AbilityBook.cs`
- 已部分 `StringCatalog` / `ResolveText`

| 行 | 类型 | 示例 |
|----|------|------|
| 20 | Item Name | Weapon Abilities |
| 46 | Gump | <p align='center'>The Complete Book of Weapon Mastery |
| 48 | Gump | Warriors have the ability to tap their Mana to perform devastating maneuvers wit |
| 53 | Gump | unique combination of special moves. Warriors who have reached a 70 skill level  |
| 54 | Gump | total special abilities for weapons, achieved at 80, 90, 100, and 110 in the wea |
| 59 | Gump | bracelets, boots, robes, cloaks, belts, and earrings. In all cases another skill |
| 60 | Gump | can sometimes help. Whenever you equip a weapon, you will get a display of butto |
| 65 | Gump | special move, select the icon and the ribbon will turn red. At the next opportun |
| 66 | Gump | move can be reduced if the warrior's skills are high enough. Add up the skill po |
| 71 | Gump | Ninjitsu. If the total lies between 200 and 299, subtract 5 from the Mana Cost.  |
| 72 | Gump | mana cost'. These items also reduce the Mana Cost of these Special Moves. If a s |
| 77 | Gump | be doubled. The special move bar can have the names of the special moves to the  |
| 78 | Gump | command '[abilitynames' without the quotes. |
| 98 | String assign | Achilles Strike |
| 98 | String assign | A strike from the weapon will greatly hurt the target's Achilles tendon. |
| 99 | String assign | Armor Ignore |
| 99 | String assign | Ignores the Target�s Resists but deals slightly lower damage than the weapon's m |
| 100 | String assign | Armor Pierce |
| 100 | String assign | Strike your foe with armor piercing force and inflicting greater damage. |
| 101 | String assign | Bladeweave |
| 101 | String assign | The warrior becomes one with their weapon, allowing it to guide their hand. |
| 101 | String assign | The effects of this attack are unpredictable, but effective (10+? Mana). |
| 102 | String assign | Bleed Attack |
| 102 | String assign | Causes the target to bleed profusely, causing Direct Damage several times over |
| 102 | String assign | the next few seconds. The amount of Damage dealt decreases each time. |
| … | … | 另有 115 条 |

#### `Weapon Abilities/Dismount.cs`
- cliloc 部分：**不处理**

| 行 | 类型 | 示例 |
|----|------|------|
| 57 | SendMessage | That attacks didn't seem to work! |

#### `Weapon Abilities/Feint.cs`
- cliloc 部分：**不处理**

| 行 | 类型 | 示例 |
|----|------|------|
| 73 | SendMessage | Your opponent recovers their senses. |

#### `Weapon Abilities/InfectiousStrike.cs`
- cliloc 部分：**不处理**

| 行 | 类型 | 示例 |
|----|------|------|
| 45 | SendMessage | You cannot use this attack with your current poison settings! |
| 58 | SendMessage | Your strike was perfect. |

#### `Weapon Abilities/LightningArrow.cs`

| 行 | 类型 | 示例 |
|----|------|------|
| 43 | SendMessage | Your lightning arrow strikes {0}! |
| 44 | SendMessage | Lightning arcs from {0}{1} arrow onto you! |
| 76 | SendMessage | Lightning arcs from your arrow onto {0}! |
| 77 | SendMessage | Lightning arcs from {0}{1} arrow onto you! |

#### `Weapon Abilities/SerpentArrow.cs`

| 行 | 类型 | 示例 |
|----|------|------|
| 24 | SendMessage | You poisoned your target. |
| 25 | SendMessage | You've been poisoned. |

#### `Weapon Abilities/SpecialAttackCommands.cs`

| 行 | 类型 | 示例 |
|----|------|------|
| 13 | String literal | SetPrimaryAbility |
| 14 | String literal | SetSecondaryAbility |
| 15 | String literal | SetThirdAbility |
| 16 | String literal | SetFourthAbility |
| 17 | String literal | SetFifthAbility |
| 18 | String literal | Set1 |
| 19 | String literal | Set2 |
| 20 | String literal | Set3 |
| 21 | String literal | Set4 |
| 22 | String literal | Set5 |
| 25 | String literal | SetPrimaryAbility |
| 26 | String literal | Set1 |
| 27 | String literal | Sets your Weapons Primary Ability Active. |
| 36 | String literal | SetSecondaryAbility |
| 37 | String literal | Set2 |
| 38 | String literal | Sets your Weapons Secondary Ability Active. |
| 48 | String literal | SetThirdAbility |
| 49 | String literal | Set3 |
| 50 | String literal | Sets your Weapons Third Ability Active. |
| 60 | String literal | SetFourthAbility |
| 61 | String literal | Set4 |
| 62 | String literal | Sets your Weapons Fourth Ability Active. |
| 72 | String literal | SetFifthAbility |
| 73 | String literal | Set5 |
| 74 | String literal | Sets your Weapons Fifth Ability Active. |

#### `Weapon Abilities/SpecialAttackGump.cs`

| 行 | 类型 | 示例 |
|----|------|------|
| 217 | String assign | Armor Ignore |
| 220 | String assign | Armor Ignore |
| 221 | String assign | Bleed Attack |
| 222 | String assign | Concussion Blow |
| 223 | String assign | Crushing Blow |
| 224 | String assign | Disarm |
| 225 | String assign | Dismount |
| 226 | String assign | Double Strike |
| 227 | String assign | Infectious Strike |
| 228 | String assign | Mortal Strike |
| 229 | String assign | Moving Shot |
| 230 | String assign | Paralyzing Blow |
| 231 | String assign | Shadow Strike |
| 232 | String assign | Whirlwind Attack |
| 233 | String assign | Riding Swipe |
| 234 | String assign | Frenzied Whirlwind |
| 235 | String assign | Block |
| 236 | String assign | Defense Mastery |
| 237 | String assign | Nerve Strike |
| 238 | String assign | Talon Strike |
| 239 | String assign | Feint |
| 240 | String assign | Dual Wield |
| 241 | String assign | Double Shot |
| 242 | String assign | Armor Pierce |
| 243 | String assign | Bladeweave |
| … | … | 另有 36 条 |

#### `Weapon Abilities/SpecialAttacksDisplay.cs`

| 行 | 类型 | 示例 |
|----|------|------|
| 13 | String literal | SpecialAttacksDisplay |
| 14 | String literal | SAD |
| 17 | String literal | SpecialAttacksDisplay |
| 18 | String literal | SAD |
| 19 | String literal | Opens your Weapons Special Attacks Display. |
| 37 | SendMessage | Your weapon skills are not high enough to use a special attack of any kind |

### Weapon Abilities/Extra

#### `Weapon Abilities/Extra/AchiliesStrike.cs`

| 行 | 类型 | 示例 |
|----|------|------|
| 16 | SendMessage | You damage their Achilles tendon! |
| 17 | SendMessage | Your Achilles tendon was hurt! |

#### `Weapon Abilities/Extra/ConsecratedStrike.cs`

| 行 | 类型 | 示例 |
|----|------|------|
| 21 | SendMessage | You hit them with the highest possible damage! |

#### `Weapon Abilities/Extra/DeathBlow.cs`

| 行 | 类型 | 示例 |
|----|------|------|
| 21 | SendMessage | You strike a deadly blow! |
| 22 | SendMessage | You were struck with a deadly blow! |

#### `Weapon Abilities/Extra/DevestatingBlow.cs`

| 行 | 类型 | 示例 |
|----|------|------|
| 23 | SendMessage | You strike a devastating blow! |
| 24 | SendMessage | You were struck with a devastating blow! |

#### `Weapon Abilities/Extra/DoubleWhirlwindAttack.cs`
- cliloc 部分：**不处理**

| 行 | 类型 | 示例 |
|----|------|------|
| 21 | SendMessage | You are too fatigues to perform this attack! |

#### `Weapon Abilities/Extra/EarthStrike.cs`

| 行 | 类型 | 示例 |
|----|------|------|
| 16 | SendMessage | You strike them with extreme physical force! |
| 17 | SendMessage | You where struck with extreme physical force! |

#### `Weapon Abilities/Extra/ElementalStrike.cs`

| 行 | 类型 | 示例 |
|----|------|------|
| 16 | SendMessage | You strike them with extreme force! |
| 17 | SendMessage | You where struck with extreme force! |

#### `Weapon Abilities/Extra/FireStrike.cs`

| 行 | 类型 | 示例 |
|----|------|------|
| 16 | SendMessage | You strike them with burning force! |
| 17 | SendMessage | You where struck with burning force! |

#### `Weapon Abilities/Extra/FistsOfFury.cs`

| 行 | 类型 | 示例 |
|----|------|------|
| 27 | SendMessage | You attack with a series of mighty blows! |
| 28 | SendMessage | You have been struck with a series of mighty blows! |

#### `Weapon Abilities/Extra/FreezeStrike.cs`

| 行 | 类型 | 示例 |
|----|------|------|
| 16 | SendMessage | You strike them with freezing force! |
| 17 | SendMessage | You where struck with freezing force! |

#### `Weapon Abilities/Extra/LightningStrike.cs`

| 行 | 类型 | 示例 |
|----|------|------|
| 16 | SendMessage | You strike them with lightning force! |
| 17 | SendMessage | You where struck with lightning force! |

#### `Weapon Abilities/Extra/MagicProtection.cs`

| 行 | 类型 | 示例 |
|----|------|------|
| 21 | SendMessage | You feel like you are protected from most magic! |

#### `Weapon Abilities/Extra/MagicProtection2.cs`

| 行 | 类型 | 示例 |
|----|------|------|
| 21 | SendMessage | You feel like you are extremely protected from most magic! |

#### `Weapon Abilities/Extra/MeleeProtection.cs`

| 行 | 类型 | 示例 |
|----|------|------|
| 21 | SendMessage | You feel like you are protected from most weapon attacks! |

#### `Weapon Abilities/Extra/MeleeProtection2.cs`

| 行 | 类型 | 示例 |
|----|------|------|
| 21 | SendMessage | You feel like you are extremely protected from most weapon attacks! |

#### `Weapon Abilities/Extra/RidingAttack.cs`
- cliloc 部分：**不处理**

| 行 | 类型 | 示例 |
|----|------|------|
| 20 | SendMessage | You must be mounted to use this ability! |

#### `Weapon Abilities/Extra/ShadowInfectiousStrike.cs`
- cliloc 部分：**不处理**

| 行 | 类型 | 示例 |
|----|------|------|
| 25 | SendMessage | You cannot use this attack with your current poison settings! |
| 30 | SendMessage | Your stealth is not sufficient, and the weapon is out of poison! |
| 35 | SendMessage | There is no poison on the weapon, but you are still hidden! |
| 40 | SendMessage | Your stealth is not sufficient, but the weapon has poison! |
| 48 | SendMessage | Your strike was perfect. |

#### `Weapon Abilities/Extra/SpinAttack.cs`

| 行 | 类型 | 示例 |
|----|------|------|
| 27 | SendMessage | You spin your weapon really fast to strike multiple times! |
| 28 | SendMessage | You are hit multiple times by their weapon! |

#### `Weapon Abilities/Extra/StunningStrike.cs`

| 行 | 类型 | 示例 |
|----|------|------|
| 16 | SendMessage | You have seriously stunned your opponent! |
| 17 | SendMessage | You are seriously stunned! |

#### `Weapon Abilities/Extra/ToxicStrike.cs`

| 行 | 类型 | 示例 |
|----|------|------|
| 16 | SendMessage | You hurt them with a sickly blow! |
| 17 | SendMessage | You feel sickly from the blow! |

#### `Weapon Abilities/Extra/ZapDexStrike.cs`

| 行 | 类型 | 示例 |
|----|------|------|
| 16 | SendMessage | You have drained their dexterity! |
| 17 | SendMessage | You feel more sluggish from the blow! |
| 24 | String literal | ZapDex |

#### `Weapon Abilities/Extra/ZapIntStrike.cs`

| 行 | 类型 | 示例 |
|----|------|------|
| 16 | SendMessage | You have drained their intellect! |
| 17 | SendMessage | You mind is clouded from the blow! |
| 24 | String literal | ZapInt |

#### `Weapon Abilities/Extra/ZapManaStrike.cs`

| 行 | 类型 | 示例 |
|----|------|------|
| 16 | SendMessage | You have drained their mana! |
| 17 | SendMessage | You feel you mana drain from the blow! |

#### `Weapon Abilities/Extra/ZapStamStrike.cs`

| 行 | 类型 | 示例 |
|----|------|------|
| 16 | SendMessage | You have drained their stamina! |
| 17 | SendMessage | You feel exhausted from the blow! |

#### `Weapon Abilities/Extra/ZapStrStrike.cs`

| 行 | 类型 | 示例 |
|----|------|------|
| 16 | SendMessage | You have drained their strength! |
| 17 | SendMessage | You feel weaker from the blow! |
| 24 | String literal | ZapStr |

## 无英文硬编码（或仅 cliloc / 已目录化）

- `Anatomy.cs`
- `Hiding.cs`
- `Inscribe.cs`
- `Meditation.cs`
- `Mercantile.cs`
- `Psychology.cs`
- `RemoveTrap.cs`
- `SkillCheck.cs`
- `Stealth.cs`
- `Weapon Abilities/ArmorIgnore.cs`
- `Weapon Abilities/ArmorPierce.cs`
- `Weapon Abilities/Bladeweave.cs`
- `Weapon Abilities/BleedAttack.cs`
- `Weapon Abilities/Block.cs`
- `Weapon Abilities/ConcussionBlow.cs`
- `Weapon Abilities/CrushingBlow.cs`
- `Weapon Abilities/CustomWeaponAbilities.cs`
- `Weapon Abilities/DefenseMastery.cs`
- `Weapon Abilities/Disarm.cs`
- `Weapon Abilities/Disrobe.cs`
- `Weapon Abilities/DoubleShot.cs`
- `Weapon Abilities/DoubleStrike.cs`
- `Weapon Abilities/DualWield.cs`
- `Weapon Abilities/ForceArrow.cs`
- `Weapon Abilities/ForceofNature.cs`
- `Weapon Abilities/FrenziedWhirlwind.cs`
- `Weapon Abilities/MortalStrike.cs`
- `Weapon Abilities/MovingShot.cs`
- `Weapon Abilities/NerveStrike.cs`
- `Weapon Abilities/ParalyzingBlow.cs`
- `Weapon Abilities/PsychicAttack.cs`
- `Weapon Abilities/RidingSwipe.cs`
- `Weapon Abilities/ShadowStrike.cs`
- `Weapon Abilities/TalonStrike.cs`
- `Weapon Abilities/WeaponAbility.cs`
- `Weapon Abilities/WeaponArmorCalls.cs`
- `Weapon Abilities/WhirlwindAttack.cs`

# 待中文化：Items 目录扫描

> **扫描日期：** 2026-05-22
> **处理范围：** 处理所有英文硬编码文本（`SendMessage`、`Say`、Gump `AddHtml`/`AddLabel`/`AddRow`、OPL `list.Add`、`Name =`、字符串赋值、`InfoText1-5`、`ColorText1-5`、属性返回字符串、长段拼接模板等）。
> **排除范围：** 不处理 cliloc 控制的文本（`SendLocalizedMessage`、仅 cliloc 数字的 `*OverheadMessage`）。已通过 `StringCatalog.Resolve` / `ResolveByKey` / `ResolveFormat` / `AddLocalizedProperty` 且键已在 locale 中的行亦不计入。
> **扫描路径：** `World/Source/Scripts/Items`
> **关联：** [`waiting-localization-skills.md`](waiting-localization-skills.md) · [`waiting-localization-quest.md`](waiting-localization-quest.md) · `AGENTS.md` §3.2

## 摘要

| 指标 | 数量 |
|------|------|
| 扫描 `.cs` 文件 | ~2000+ |
| **含英文硬编码（待处理）的文件** | **~700+** |
| 无英文硬编码（或仅 cliloc / 已目录化） | ~1300+ |

**说明：** 同一文件可同时含 cliloc 与硬编码（如 `BaseArmor.cs`：cliloc 耐久性提示不处理，`SendMessage("Only {0} may use this.")` 须目录化）。

### SendMessage 硬编码（P0，须目录化）

下列写法对 **zh-Hans 账号会直接显示英文**，与 cliloc **无关**，**必须**纳入待办并改为 `StringCatalog.ResolveByKey` / `ResolveFormatByKey`（见 `AGENTS.md` §3.2；带 hue 时 hue 仅着色，**字符串仍须目录化**）：

| 形态 | 示例（源码） | 待办文档中的位置 |
|------|----------------|------------------|
| `SendMessage("…")` | `Items/Potions/Special/TransmutationPotion.cs:67` — `SendMessage("What would you like to pour this on?")` | 见下表 `Potions/Special/` 明细 |
| `SendMessage(hue, "…")` | `Items/Sharpening/SharpeningStoneBase.cs:41` — `SendMessage(32, "The stone crumbles in your hands")` | 同上 |
| `LocalOverheadMessage(…, true, "…")` | `Items/Potions/Unique/BaseManaRefreshPotion.cs:59` | 见 `Potions/Unique/` |
| `PublicOverheadMessage(…, "…")` | `Items/Games/TarotPoker.cs:37` — `PublicOverheadMessage("...pulls 'The Fool'")` | 见 `Games/` |
| `Say("…")` | `Items/Potions/Standard/PoisonPotions/VenomSack.cs:73` — `Say("Poison!")` | 见 `Potions/Standard/Poison Potions/` |
| `Name = "…"` | `Items/Armor/Plate/PlateChest.cs:32` — `Name = "platemail"` | 见 `Armor/` 各子目录 |
| `list.Add(1049644, "…")` | `Items/Potions/Standard/PoisonPotions/VenomSack.cs:95` — `list.Add(1070722, "Use To Attempt To Extract Venom")` | 同上 |
| `DefaultDescription` 未目录化 | 已全部处理 — 无残留 | — |

**不属于待办：** `SendLocalizedMessage(502789)` 等 cliloc；`SendMessage(ResolveText(from, "…"))` / `StringCatalog.*` 且键已在 locale JSON 中。

Items 目录内 **各类英文硬编码** 约 **3000+ 处**，分布在本文「待处理文件总表」中所有带标记的目录中。

**复查命令：**

```bash
rg 'SendMessage\s*\(\s*"' World/Source/Scripts/Items
rg 'SendMessage\s*\(\s*(\d+|0x[0-9a-fA-F]+)\s*,\s*"' World/Source/Scripts/Items
rg 'Name\s*=\s*"' World/Source/Scripts/Items
rg 'list\.Add\(1049644\s*,\s*"' World/Source/Scripts/Items
rg 'list\.Add\(1070722\s*,\s*"' World/Source/Scripts/Items
```

---

## 待处理目录总表

| 一级目录 | 扫描 `.cs` 文件数 | 含硬编码文件数 | 估算硬编码条数 | 主要类型 |
|----------|-------------------|---------------|----------------|----------|
| Armor/ | 166 | ~50 | ~110 | Name =, SendMessage(BaseArmor.cs) |
| Boats/ | 29 | 16 | ~140 | Name =, SendMessage, Gump, OPL |
| Books/ | 31 | 25 | ~165 | Name =, SendMessage, Gump AddHtml |
| Clothing/ | 19 | 15 | ~75 | Name =, SendMessage(BaseClothing.cs) |
| Containers/ | 43 | 32 | ~445 | Name =, SendMessage, Gump, OPL |
| Deeds/ | 7 | 3 | ~4 | SendMessage, OPL |
| Explorers/ | 8 | 7 | ~55 | SendMessage, OPL, InfoText |
| Food/ | 26 | 18 | ~70 | Name =, SendMessage, Say, ColorText |
| Games/ | 43 | 14 | ~130 | SendMessage, Gump, PublicOverhead, OPL |
| Gems/ | 12 | 3 | 3 | Name = |
| Houses/ | 310 | ~200 | ~400+ | Name =, SendMessage, Gump, OPL |
| Instruments/ | 12 | 1 | 1 | OPL |
| Magical/ | 844 | ~220 | ~1,150+ | Name =, OPL, Gump, ColorText, SendMessage |
| Misc/ | 232 | 48 | ~265 | Name =, SendMessage, OPL, Property |
| Potions/ | 88 | 52 | ~277 | Name =, SendMessage, OPL, Gump |
| Quivers/ | 3 | 1 | ~1 | Name = |
| Relics/ | 28 | 28 | ~230 | Name =, SendMessage, ColorText3, OPL |
| Sharpening/ | 23 | 16 | ~75 | SendMessage, Name =, OPL |
| Special/ | 166 | ~50 | ~200+ | Name =, SendMessage, Say, OPL |
| Technology/ | 21 | 16 | ~115 | Name =, SendMessage, InfoText, OPL, Gump |
| Trades/ | 233 | ~25 | ~200 | Name =, SendMessage, InfoText, OPL |
| Traps/ | 26 | 13 | ~50 | Name =, SendMessage, LocalOverhead, OPL |
| Trinkets/ | 14 | 7 | ~65 | Name =, SendMessage, OPL |
| Weapons/ | 110 | ~70 | ~200+ | Name =, SendMessage, OPL |
| **合计** | **~2000+** | **~700+** | **~3,000+** | |

---

## 建议修复批次

| 批次 | 范围 |
|------|------|
| **B0** | **`BaseArmor.cs`**（7 处 SendMessage：种族/性别/智力校验 — 所有护甲共用） + **`BaseClothing.cs`**（8 处 SendMessage：同模式） + **`BaseWeapon.cs`**（5 处 SendMessage）+ **`HorseArmor.cs`**（2 处） |
| **B1** | **`Relics/`** — 28 个文件全部须处理：`ColorText3 = "Worth X Gold"`（~18 个文件）+ SendMessage 鉴定反馈 + Name |
| **B2** | **`Potions/`** — `Name =` + `SendMessage` + 锅釜 `potionName` + Gump 发型选择（`HairOilPotion.cs`） |
| **B3** | **`Trades/`** — `PotionKeg.cs`（~100 keg name）、`NewFish.cs`（~30 鱼名）、`WetClothes.cs`（~40 衣物名）、`PickBox.cs`（5 难度）、`Scissors.cs` 返回字符串 |
| **B4** | **`Armor/`** — Name = 硬编码（~84 处散布于 Bone/Chain/Leather/Plate/Ring/Royal/Scaled/Shields/Studded/Wooden） |
| **B5** | **`Weapons/`** — SendMessage 硬编码（11 个文件）+ Name =（~58+ 个文件）+ OPL 英文回退 |
| **B6** | **`Sharpening/`** — 基类 SendMessage（~37 处）+ Name = + OPL |
| **B7** | **`Magical/Gumps/`** — `ItemExperienceGump.cs`（~30 标签）+ `GiftGump.cs`（~30 标签）+ `LevelUpAcceptGump.cs` / `AwaitingSmithApprovalGump.cs` |
| **B8** | **`Magical/God/LevelUpScroll.cs`** — OPL + Label 回退（~12 处）+ **`LegendsBook.cs`**（~190 图鉴名） |
| **B9** | **`Magical/Gifts/`** — `BaseGift*.cs`（11+ 文件 "Single Click to Enchant"） + GiftThrowingGloves/PugilistMits OPL + Gift item Name = |
| **B10** | **`Magical/Artifacts/`** — ~250+ 个 Artifact 的 `Name = "…"` + OPL 回退 |
| **B11** | **`Misc/`** — `MagicForges.cs`（~30 处 + 区域/NPC 名）、`MusicBox.cs`（~60 曲名）、`Dyes/` 桶 UI + SendMessage、`Bodies/Corpse.cs`、`Bodies/BookofDead.cs` |
| **B12** | **`Houses/`** — `Monopoly/TownHouseSetupGump.cs`（~80 Gump/消息）、`HouseSign.cs`（46 消息 + 24 招牌名）、`Construction/Wells/`（42 消息）、`Remodeling/Lawn*Shanty*` 全套（~50+）、`Doors/` 系列 |
| **B13** | **`Technology/`** — `SciFiJunk.cs`（55+ 物品名）、`PlasmaTorch.cs`、`AlienEgg.cs`、`MaterialLiquifier.cs`、`Games/`（BlackJack ~20 处 + LiarsDice ~30 处 + TarotPoker ~25 处） |
| **B14** | **`Containers/`** — `Container.cs`（~72 名字 + 12 消息）、`ContainerFunctions.cs`（~35 名）、`AnimalCages.cs`（~70 动物名）、`Shelves.cs`（~55 名）、`AlchemistPouch.cs`（~50 Gump 标签）、`WeightReductionContainer.cs`（~9 消息 + 属性） |
| **B15** | **`Houses/Monopoly/`** — TonwHouse 系统全套 Gump + SendMessage + OPL（GM 管理界面+玩家契约面） |
| **B16** | **`Food/`** — `Hunger.cs`（~13 饥饿状态消息）、`TastyHeart.cs`（14 条吃心消息）、`FreshBrain.cs` / `BloodDrink.cs` / `Beverage.cs`（~12 条） |
| **B17** | **`Special/`** — 宠物蛋史诗文本（`DragonEgg.cs` / `DracolichSkull.cs` / `DrakkhenEgg.cs` / `DemonPrison.cs`）、Tarot 占卜 `DecoDeckOfTarot.cs`（~60 行）、`CharacterStatue.cs` |
| **B18** | **`Trinkets/`** — `TrinketTalisman.cs`（~35 名变体 + 消息）、`GuildRing.cs`（17 公会名 + OPL）、`SavageTalisman.cs`（OPL + LocalOverhead） |
| **B19** | **`Explorers/`** — `CamperTent.cs`、`SmallTent.cs`、`StableStone.cs`、`Kindling.cs`、`Bedroll.cs`、`Spyglass.cs` — SendMessage + OPL（~35 条消息） |
| **B20** | **`Boats/`** — `Cargo.cs`（45+ 货箱名 + Gump）、`FindboatGump.cs`、`CarpetBuild.cs`、`GrapplingHook.cs`、`PirateBounty.cs`、`DockingLantern.cs` |
| **B21** | **`Instruments/`** — `BaseInstrument.cs:648`（1 处 OPL "Uses"） |
| **B22** | **`Traps/`** — 陷阱覆盖物（`SewageItem.cs`、`WeedItem.cs`、`SlimeItem.cs`、`TaintedBandage.cs`、`MushroomTrap.cs`、`KillerTile.cs`、`JewelryBox.cs`、`BookBox.cs`） |

---

## 按模块明细

### Armor/（166 文件，~110 处硬编码）

#### `BaseArmor.cs`
- cliloc 部分：**不处理**

| 行 | 类型 | 示例 |
|----|------|------|
| 626 | SendMessage（ValidateMobile） | `Only {0} may use this.` |
| 635 | SendMessage（ValidateMobile） | `You may not wear this.` |
| 644 | SendMessage（ValidateMobile） | `You may not wear this.` |
| 1164 | SendMessage（CanEquip） | `Only {0} may use this.` |
| 1173 | SendMessage（CanEquip） | `You may not wear this.` |
| 1182 | SendMessage（CanEquip） | `You may not wear this.` |
| 1204 | SendMessage（CanEquip） | `You are not intelligent enough to equip that.` |

#### `HorseArmor.cs`

| 行 | 类型 | 示例 |
|----|------|------|
| 97 | SendMessage | `Which horse do you want to use this on?` |
| 152 | SendMessage | `This armor is only for horses you own.` |

#### `Leather/HikingBoots.cs`
- 已部分 `BuildingPropertyListLocale` / `AddLocalizedProperty` / `StringCatalog.ResolveByKey`

| 行 | 类型 | 示例 |
|----|------|------|
| 49 | OPL fallback | `[Monster races only]` |
| 50 | OPL fallback | `Increase movement speed` |

#### Armor 子目录 — `Name = "…"`（~84 处，仅列代表性）

| 子目录 | Name 示例（行号） |
|--------|------------------|
| Bone/ | `bone bracers`(32), `bone tunic`(32), `bone gauntlets`(32), `bone greaves`(32), `bone helm`(30), `bone skirt`(33), `horned helm`(33), `skeletal bracers`(31)… |
| Chain/ | `chainmail tunic`(31), `chainmail coif`(29), `chainmail leggings`(31), `chainmail skirt`(31) |
| Helmets/ | `bascinet`(28), `dread helm`(30), `norse helm`(28), `oniwaban hood`(12) |
| Leather/ | `hiking boots`(13), `hide tunic`(31), `leather boots`(129), `leather tunic`(44), `leather robe`(81), `leather shinobi robe`(31)… ~17 处 |
| Plate/ | `platemail arms`(30), `platemail`(32), `plate helm`(32), `platemail legs`(31), `platemail skirt`(32) |
| Ring/ | `ringmail skirt`(31) |
| Royal/ | `royal mantle`(13), `royal boots`(13), `royal tunic`(13), `royal bracers`(13)… ~7 处 |
| Scaled/ | `scalemail arms`(32), `scalemail tunic`(32), `scalemail gloves`(32), `scalemail helm`(32), `drakbone bracers`(32)… ~22 处 |
| Shields/ | `large shield`(25), `champion shield`(26), `crested shield`(26), `dark shield`(26), `elven shield`(26)… ~10 处 |
| Studded/ | `bearskin cap`(31), `deerskin cap`(77), `stagskin cap`(123), `wolfskin cap`(169), `studded hide tunic`(31), `studded skirt`(33) |
| WoodenArmor.cs | `wooden leggings`(32), `wooden gauntlets`(79), `wooden gorget`(126), `wooden arms`(173), `wooden tunic`(220), `wooden helm`(267) |

---

### Boats/（29 文件，~140 处硬编码）

| 文件 | 硬编码条数 | 主要类型 |
|------|-----------|----------|
| `Cargo.cs` | ~45+ | Name = 货物箱名, Gump AddHtml, SendMessage |
| `BaseBoat.cs` | 2 | TillerMan.Say 特殊 |
| `BaseDockedBoat.cs` | 2 | SendMessage |
| `BaseBoatDeed.cs` | 2 | SendMessage |
| `Plank.cs` | 1 | SendMessage |
| `BoatDoor.cs` | 1 | SendMessage |
| `TillerManGump.cs` | 2 | SendMessage |
| `FindboatGump.cs` | 4 | Gump AddLabel |
| `CarpetBuild.cs` | ~8 | SendMessage, OPL, Gump AddHtml |
| `GrapplingHook.cs` | ~4 | SendMessage, OPL |
| `BoatBuild.cs` | ~4 | OPL |
| `PirateBounty.cs` | 1 | OPL |
| `DockingLantern.cs` | 1 | OPL |
| `BoatStain.cs` | 1 | OPL |
| `StableStone.cs` | 2 | OPL |

#### `Cargo.cs` 关键：

| 行 | 类型 | 示例 |
|----|------|------|
| 102-179 | Gump AddHtml | `"Contains "`, `"Delivery Karma:"`, `"Base Value:"`, `"Keep"`, `"Deliver"`… |
| 106-535 | Name = | `"crate of hay"`, `"crate of bowcrafting tools"`, `"crate of garlic"`, `"royal coffer of "`… ~45+ |
| 825 | SendMessage | `"You receive " + gold + " gold."` |

#### `CarpetBuild.cs` 关键：

| 行 | 类型 | 示例 |
|----|------|------|
| 80 | SendMessage | `"You need to gather more items before you can conjure this!"` |
| 97 | SendMessage | `"You need to be near a wizard to conjure that!"` |
| 131-134 | Gump AddHtml | `"THE CARPET OF ALADDIN"`（长篇故事段落） |
| 61-69 | OPL | `"Drop The Items Needed On This Book"`, `"Need X Gold Coins, Y Cloth"` |

---

### Books/（31 文件，~165 处硬编码）

| 文件 | 硬编码条数 | 主要类型 |
|------|-----------|----------|
| `SwordsAndShackles.cs` | ~14 | Gump AddHtml（长篇教学段落，含 `LocalizeBookHtml` 包装） |
| `PowerScroll.cs` | ~10 | OPL Add + SendMessage（四类神兵卷轴 + 灵性圣殿） |
| `TitleChangeDeed.cs` | 2 | SendMessage |
| Beginner/Teacher 书（~12 个文件） | 11 | SendMessage: `"This must be in your backpack to read."` |
| `DynamicBook.cs` | 1 | SendMessage |
| `WantedMangar.cs` | 5 | Gump AddHtml |
| `AdminBoard.cs` | ~3 | Gump AddLabel |
| `StatusBoard.cs` | 5 | Gump AddLabel |
| `BeginnerBook.cs` | 3 | ColorText3 |
| Learn*/Teacher 书 | ~22 | Name = |
| `BardsTaleNote.cs` | ~7 | Name = |

#### `SwordsAndShackles.cs`

| 行 | 类型 | 示例 |
|----|------|------|
| 84-120 | AddHtml | 7 个章节标题 + 7 段英文教学（钓鱼/鱼叉/航行）约 3000+ 字符 |

#### `PowerScroll.cs`

| 行 | 类型 | 示例 |
|----|------|------|
| 154-158 | list.Add(1049644) | `"Wondrous Scroll"`, `"Exalted Scroll"`, `"Mythical Scroll"`, `"Legendary Scroll"`, `"Power Scroll"` |
| 94+ | SendMessage | `"Your current cap has to be at {0}..."`, `"This magic can only be unleashed at the Shrine of Strength."`, `"Shrine of Intelligence."`, `"Shrine of Dexterity."`, `"Shrine of Wisdom."` |

---

### Clothing/（19 文件，~75 处硬编码）

#### `BaseClothing.cs`
- cliloc 部分：**不处理**

| 行 | 类型 | 示例 |
|----|------|------|
| 207 | SendMessage | `Only {0} may use this.` |
| 216 | SendMessage | `You may not wear this.` |
| 225 | SendMessage | `You may not wear this.` |
| 316 | SendMessage | `Only {0} may use this.` |
| 325 | SendMessage | `You may not wear this.` |
| 334 | SendMessage | `You may not wear this.` |
| 1106 | SendMessage | `from.SendMessage(msg)`（msg 变量，来源不确定） |

#### 子目录 `Name = "…"`（~67 处）

| 文件 | Name 示例（行号） |
|------|------------------|
| `Robes.cs` | 28 种：`"jester coat"`, `"assassin robe"`, `"vampire robe"`, `"dragon robe"`, `"chaos robe"`, `"fancy robe"`, `"gilded robe"`, `"ornate robe"`, `"magistrate robe"`, `"royal robe"`, `"sorcerer robe"`, `"scholar robe"`, `"necromancer robe"`, `"spider robe"`, `"vagabond robe"`, `"pirate coat"`, `"jester garb"`, `"fool's coat"`, `"exquisite robe"`, `"prophet robe"`, `"elegant robe"`, `"formal robe"`, `"archmage robe"`, `"priest robe"`, `"cultist robe"`, `"gilded dark robe"`, `"gilded light robe"`, `"sage robe"` |
| `Hats.cs` | `"cloth hood"`, `"hooded mantle"`, `"cloth cowl"`, `"mask of the dead"`, `"wizard hood"`, `"witch hat"`, `"fancy hood"`, `"tricorne hat"`, `"pirate hat"`, `"fool's hat"` |
| `Shoes.cs` | `"barbarian boots"`, `"boots"`, `"thigh boots"`, `"fancy boots"`, `"jester shoes"` |
| `Shirts.cs` | `"royal coat"`, `"squire shirt"`, `"formal coat"`, `"wizard shirt"`, `"shirt"`, `"beggar vest"`, `"royal vest"`, `"rustic vest"` |
| `Pants.cs` | `"short pants"`, `"long pants"`, `"sailor pants"`, `"pirate pants"` |
| `OuterLegs.cs` | `"royal skirt"`, `"royal long skirt"`, `"kilt"` |
| `OuterTorso.cs` | `"gilded dress"`, `"fancy dress"`, `"dress"` |
| `MiddleTorso.cs` | `"royal shirt"`, `"rustic shirt"` |
| 其他（~6 文件） | `"loin cloth"`, `"royal loin cloth"`, `"belt"`, `"reaper hood"`, `"reaper cowl"`, `"royal cloak"` |

---

### Containers/（43 文件，~445 处硬编码）

| 文件 | 硬编码条数 | 主要类型 |
|------|-----------|----------|
| `Container.cs` | ~84 | Name =（~60）, SendMessage（~12）, ColorText |
| `ContainerFunctions.cs` | ~35 | Name = 动态组合 |
| `AnimalCages.cs` | ~70 | Name = 全动物图鉴 |
| `Shelves.cs` | ~55 | Name = 货架/衣柜 |
| `AlchemistPouch.cs` | ~58 | Gump AddHtml 50 标签 + SendMessage 8 |
| `LootPack.cs` | ~26 | Name = 特殊物品/晶石/眼镜 |
| `WeightReductionContainer.cs` | ~12 | SendMessage + 属性返回 + OPL（部分已 `StringCatalog.ResolveByKey`） |
| `MovingBox.cs` | 9 | SendMessage |
| `FoodChest.cs` | 6 | SendMessage（含 typo `"FoodPotatoes"`） |
| `SackOfHolding.cs` | ~4 | Gump AddHtml + SendMessage |
| `BagOfHolding.cs` | 2 | SendMessage |
| `PirateChest.cs` | 1 | Name = |
| `TreasureMapChest.cs` | 3 | SendMessage |
| `AlchemyPouch.cs` | 2 | SendMessage |
| `Safe.cs` | 2 | SendMessage |
| `GypsyShelf.cs` | 2 | SendMessage |
| `TrapableContainer.cs` | 2 | SendMessage + Say |
| `InnChest.cs` | 2 | SendMessage |
| `DungeonChest.cs` | 2 | SendMessage + Say |
| `LandChest.cs` | ~6 | Name = + SendMessage |
| `SunkenShip.cs` | 1 | SendMessage |
| `SunkenChest.cs` | 3 | ColorText |
| `WaterChest.cs` | 1 | SendMessage |
| `GraveChest.cs` | 2 | ColorText |
| `BuriedChest.cs` | 2 | ColorText |
| `BuriedBody.cs` | 2 | ColorText |
| `ParagonChest.cs` | 1 | ColorText |
| `HiddenChest.cs` | 1 | LocalOverheadMessage |
| 其他容器（~8 文件） | ~12 | Name = |

---

### Deeds/（7 文件，~4 处硬编码）

| 文件 | 行 | 类型 | 示例 |
|------|----|------|------|
| `VendorRentalContract.cs` | 173, 267 | SendMessage | `"Rental contracts can only be placed in AOS-enabled houses."` |
| `CommodityDeed.cs` | 131 | OPL fallback | `"Usable on refined resources"` |
| `BarkeepContract.cs` | 64 | LocalOverheadMessage | `"You are not the full owner of this house."` |

已目录化：`CommodityDeed.cs` — `StringCatalog.ResolveByKey` 于目标发送消息

---

### Explorers/（8 文件，~55 处硬编码）

- `DefaultDescription` 均已使用 `StringCatalog.Resolve` ✓

| 文件 | 硬编码条数 | 主要类型 |
|------|-----------|----------|
| `CamperTent.cs` | ~10 | SendMessage（帐篷安放限制）+ OPL |
| `SmallTent.cs` | ~11 | SendMessage（同上）+ OPL |
| `StableStone.cs` | ~18 | SendMessage（宠物寄存完整流程） |
| `Kindling.cs` | 5 | SendMessage（营火限制） |
| `Spyglass.cs` | 3 | SendMessage + InfoText1 |
| `Bedroll.cs` | 5 | SendMessage |

---

### Food/（26 文件，~70 处硬编码）

- `DefaultDescription` 均已使用 `StringCatalog.Resolve` ✓

| 文件 | 硬编码条数 | 主要类型 |
|------|-----------|----------|
| `Hunger.cs` | ~13 | SendMessage（饥饿/口渴全状态链） |
| `Food.cs` | ~10 | Say + SendMessage（饥饿对白） |
| `TastyHeart.cs` | 14 | SendMessage（吃心消息——血饥/脑饥/食饥三种状态） |
| `FreshBrain.cs` | ~6 | SendMessage |
| `BloodDrink.cs` | ~6 | SendMessage |
| `Beverage.cs` | ~15 | Name = + SendMessage + Say + ColorText3 |
| `BakedBread.cs` | 3 | SendMessage |
| `Farmable*.cs`（5 文件） | ~6 | Name = |
| `Vegetables.cs` | ~4 | Name = |

#### `Hunger.cs` 全部消息：

| 行 | 类型 | 示例 |
|----|------|------|
| 24 | SendMessage | `You are starving to death.` |
| 26 | SendMessage | `You are extremely hungry.` |
| 28 | SendMessage | `You are very hungry.` |
| 30 | SendMessage | `You are slightly hungry.` |
| 32 | SendMessage | `You are not really hungry.` |
| 34 | SendMessage | `You are quite full.` |
| 35 | SendMessage | *错误消息* |
| 37 | SendMessage | `You are dying of thirst.` |
| 39 | SendMessage | `You are extremely thirsty.` |
| 41 | SendMessage | `You are very thirsty.` |
| 43 | SendMessage | `You are slightly thirsty.` |
| 45 | SendMessage | `You are no longer thirsty.` |
| 47 | SendMessage | `You are too quenched to drink anymore.` |

---

### Games/（43 文件，~130 处硬编码）

| 文件 | 硬编码条数 | 主要类型 |
|------|-----------|----------|
| `BlackJack.cs` | ~20 | SendMessage + PublicOverheadMessage + OPL + Gump labels |
| `LiarsDice/DiceChannel.cs` | ~30 | SendMessage + PublicOverheadMessage |
| `TarotPoker.cs` | ~25 | PublicOverheadMessage（全部 22 张大阿卡纳塔罗牌对白） |
| `PuzzleCube.cs` | 4 | SendMessage |
| `HiLoCards.cs` | ~7 | OPL |
| `CasinoToken.cs` | 1 | OPL |
| `Dice*.cs`（6 文件） | ~6 | OPL + Name = |
| D&D 手册（3 文件） | ~6 | Name = + OPL |

---

### Gems/（12 文件，3 处硬编码）

| 文件 | 行 | 类型 | 示例 |
|------|----|------|------|
| `Crystals.cs` | 28 | Name = | `"crystals"` |
| `Oyster.cs` | 22 | Name = | `"pearl"` |
| `LargeCrystal.cs` | 11 | Name = | `"crystal"` |

（所有宝石类 Sapphire/StarSapphire/Tourmaline/Ruby/Amber/Citrine/Diamond/Amethyst/Emerald 无硬编码——继承名称或使用 cliloc）

---

### Houses/（310 文件，~400+ 处硬编码）

**总体状态：** 大面积未目录化。仅 6 个文件有 `BuildingPropertyListLocale` + `AddLocalizedProperty`。无 `StringCatalog.Resolve*` 调用。

#### Root

| 文件 | 硬编码条数 | 主要类型 |
|------|-----------|----------|
| `HouseSign.cs` | ~70 | SendMessage（46 "Please Enter The New Name"）+ Name =（24 招牌名） |
| `BaseHouse.cs` | 2 | SendMessage + Property return |
| `HouseFoundation.cs` | 2 | SendMessage + Name = |
| `InteriorDecorator.cs` | 2 | Name = + OPL（已 bilingual） |
| `HousePlacementTool.cs` | 1 | Name = |
| `MagicalRope.cs` | 2 | Name = + OPL |
| `TavernTable.cs` | 3 | Name = + SendMessage |
| `CircusTents*.cs`（2 文件） | 4 | Name = + SendMessage |

#### Construction/Wells/（6 文件，~42 处——每文件 7 条，完全重复）

| 行 | 类型 | 示例 |
|----|------|------|
| — | Name = | `"Well Digging Spade"` |
| — | AddonName property | `"well"` |
| — | SendMessage | `"You are not thirsty at all."` |
| — | SendMessage | 5 句喝水反馈（`"You drink your fill of the cool well water..."` 等） |

#### Construction/Lights/（~10 文件）

| Name = | `"burning scarecrow"`, `"candelabra"`, `"candle"`, `"wall torch"`, `"dragon lamp"`, `"glowing light"`, `"gothic candelabra"`, `"strange glow"`, `"tower lantern"` |

#### Construction/Trees/（4 文件）

| Name = | `"Dark Brown Tree"`, `"Grey Tree"`, `"Light Brown Tree"` |

#### Construction/Addons/（多文件）

| Name / AddonName | `"jack-o-latern"`, `"wagon"`, `"wagon deed"`, `"treasure pile"`, `"Chest of Decorative Treasure"`… |

#### Doors/

| 文件 | 硬编码条数 | 主要类型 |
|------|-----------|----------|
| `BaseDoor.cs` | 10 | SendMessage（门链接全程反馈） |
| `DoorSwitch.cs` | 5 | SendMessage + OPL（部分 bilingual） |
| `GateMoon.cs` | ~35 | SendMessage + Gump AddHtml + GetGateName()（30+ 地名） |
| `KeywordDoor.cs` | 2 | SendMessage |
| `DoorStuck.cs` | 2 | SendMessage |
| `DoorOpener.cs` | 1 | Name = |
| 其他传送器（~8 文件） | ~10 | Name = + LocalOverheadMessage + SendMessage |

#### Monopoly/（TownHouse 系统——最大未目录化子系统）

| 文件 | 硬编码条数 | 主要类型 |
|------|-----------|----------|
| `TownHouseSetupGump.cs` | ~80+ | Gump AddHtml/AddLabel + SendMessage（设定全程 + 说明书） |
| `ContractSetupGump.cs` | ~40+ | Gump AddHtml + SendMessage（租约设定全程） |
| `TownHouseConfirmGump.cs` | ~10 | Gump AddHtml |
| `ContractConfirmGump.cs` | ~15 | Gump AddHtml + SendMessage |
| `TownHousesGump.cs` | ~10 | Gump AddHtml + SendMessage |
| `TownHouseSign.cs` | ~15 | OPL + Name = + PublicOverheadMessage |
| `General.cs`（Misc） | ~8 | SendMessage |
| `GumpResponse.cs` | 6 | SendMessage |
| `RentalContract.cs` | ~5 | SendMessage + OPL（部分 bilingual） |
| `RentalLicense.cs` | 2 | OPL（bilingual） |
| `SignHammer.cs` | ~8 | SendMessage + Gump + Name = |
| `Decorate.cs` | 3 | SendMessage |

#### Remodeling/（LawnTools / ShantyTools 系统）

| 文件 | 硬编码条数 | 主要类型 |
|------|-----------|----------|
| `LawnTools.cs` / `ShantyTools.cs` | 2 | Name = |
| `LawnTarget.cs`/`ShantyTarget.cs` | ~8 | SendMessage |
| `LawnRemoveTarget.cs`/`ShantyRemoveTarget.cs` | ~6 | SendMessage |
| `LawnGump.cs` | ~20 | Gump AddHtml（全套英文说明） |
| `LawnSecurityGump.cs`/`ShantySecurityGump.cs` | ~6 | Gump + SendMessage |
| `LawnGate.cs`/`ShantyDoor.cs` | ~6 | SendMessage |
| `LawnItem.cs`/`ShantyItem.cs` | 2 | SendMessage |
| `LawnStair.cs`/`ShantyStair.cs` | 1 | SendMessage |
| `LawnSystem.cs`/`ShantySystem.cs` | 2 | SendMessage |
| `LawnRegistry.cs`/`ShantyRegistry.cs` | — | 分类名（英文） |

---

### Instruments/（12 文件，1 处硬编码）

| 文件 | 行 | 类型 | 示例 |
|------|----|------|------|
| `BaseInstrument.cs` | 648 | OPL | `list.Add(1060584, "{0}\t{1}", m_UsesRemaining.ToString(), "Uses")` |

（11 个乐器文件 Name = 均有英文 `"trumpet"`, `"harp"`, `"drum"` 等，属于基础物品名）

---

### Magical/（844 文件，~1,150+ 处硬编码）

#### Root `Magical/*.cs`

| 文件 | 硬编码条数 | 主要类型 |
|------|-----------|----------|
| `SlayerDeed.cs` | ~36 | Name = + SlayerNames 数组（35 种杀器名）+ OPL |
| `Moonstone.cs` | 2 | Name = + OPL |
| `StaffOfFiveParts.cs` | ~4 | 模板组合 + LocalOverheadMessage |
| `SoulOrb.cs` | ~5 | Name = + OPL fallback（Gump/SendMessage 已目录化） |
| `LegendsBook.cs` / `ManualOfItems.cs` | 2 | Name = |
| `MagicQuiver.cs` | 2 | Name = + ColorText1 组合 |
| `ArtifactManual.cs` | 2 | Name = + OPL |
| `LuckyHorseShoes.cs` | 2 | Name = + OPL |
| `WeaponRenamingTool.cs` | 1 | Name =（已 `DisplayNameLocalizationKey`） |
| `ColoringBook.cs` | ~551 | Name = + ~550 色名（已有 `GetLocalizedPrismaticSwatch` zh-Hans 桥接） |

#### Magical/Artifacts/（~300 文件——~250+ Name =）

每个 `Artifact_*.cs` 文件至少有一个 `Name = "..."` 硬编码，如：

```csharp
Artifact_BurglarsBandana.cs:    Name = "Burglar's Bandana"
Artifact_BraceletOfTheVile.cs:  Name = "Bracelet of the Vile"
Artifact_Excalibur.cs:          Name = "Excalibur"
Artifact_Stormbringer.cs:       Name = "Stormbringer"
Artifact_BladeOfTheRighteous.cs: Name = "Blade of the Righteous"
Artifact_BowOfThePhoenix.cs:    Name = "Bow of the Phoenix"
Artifact_QuiverOfFire.cs:       Name = "Quiver of Fire"
// ... ~200 个更多
```

#### Magical/God/

| 文件 | 硬编码条数 | 主要类型 |
|------|-----------|----------|
| `LevelUpScroll.cs` | ~12 | Name = + OPL fallback（5 类卷轴）+ Label 回退 |
| `ItemExperienceToken.cs` | 1 | Name =（已 `IsContentLocalized`） |
| `LegendaryArtifactRename.cs` | 4 | Name = + OPL fallback（已 shotkey） |
| `LegendsBook.cs` | ~192 | Name = + ~190 `GetLegendArtyForBook()` 图鉴物品名 |
| `LevelThrowingGloves.cs` | 2 | OPL fallback |
| `LevelPugilistMits.cs` | 1 | OPL fallback |
| 三级 Jewels（Lantern/Torch/Candle） | 6 | Name = + OPL Equip/Unequip |
| Level 基础物品（~30+ 文件） | ~35 | Name =（`"bow"`, `"sword"`, `"plate helm"`, `"dread helm"`, `"leather tunic"`…） |

#### Magical/God/Gumps/

| 文件 | 硬编码条数 | 主要类型 |
|------|-----------|----------|
| `ItemExperienceGump.cs` | ~30 | Gump AddHtml：`"Item Experience"`, `"Categories"`, `"Melee Attributes"`, `"Magic Attributes"`, `"Next"`, `"Previous"`… |
| `AwaitingSmithApprovalGump.cs` | ~6 | Gump AddHtml：卷轴名 + `"Please Wait..."` + 审批说明 |
| `LevelUpAcceptGump.cs` | ~4 | Gump AddHtml：`"Max Level Increase Request"`（长篇 HTML）+ Label |

#### Magical/Gifts/

| 文件 | 硬编码条数 | 主要类型 |
|------|-----------|----------|
| `GiftGump.cs` | ~30 | Gump AddHtml：`"Item Status"`, `"Categories"`, `"Melee Attributes"`, `"Next"`, `"Previous"`… |
| `BaseGift*.cs`（11+ 文件） | ~13 | OPL fallback `"Single Click to Enchant"` |
| `GiftThrowingGloves.cs` | ~4 | OPL + GloveType（`"Stones"/"Axes"/"Knives"/"Darts"/"Stars"`） |
| `GiftPugilistMits.cs` | 1 | OPL fallback |
| Gift Jewels（3 文件） | ~6 | OPL Equip/Unequip |
| Gift 基础物品（~30+ 文件） | ~35 | Name =（`"bow"`, `"assassin dagger"`, `"guardsman shield"`, `"leather cloak"`…） |

---

### Misc/（232 文件，~265 处硬编码）

| 文件 | 硬编码条数 | 主要类型 |
|------|-----------|----------|
| `MagicForges.cs` | ~35 | SendMessage + m_Extra/FromWho/HowGiven + Name = + InfoText1 |
| `MusicBox.cs` | ~60 | SendMessage（唱片曲名） |
| `Dyes/MagicalDyes.cs` | 7 | SendMessage + OPL |
| `Dyes/MagicPigment.cs` | 10 | Name = + ColorText + SendMessage |
| `Dyes/HueStone.cs` | ~15 | Name = + SendMessage + OPL |
| `Dyes/UnusualDyes.cs` | 3 | Name = + OPL |
| `Dyes/AllDyeTubs*.cs`（5 文件） | ~30 | Name = + OPL + SendMessage |
| `Dyes/Essence/EssenceBase.cs` | ~4 | Name = + SendMessage + OPL |
| `Dyes/Essence/EssenceOrb.cs` | 5 | Name = + SendMessage + OPL |
| `Dyes/CustomHuePicker.cs` | 1 | Gump AddLabel `"DEFAULT"` |
| `Bodies/Corpse.cs` | ~5 | Property return + ColorText3 + SendMessage |
| `Bodies/BookofDead.cs` | 10 | SendMessage + Say + Property |
| `Bodies/SummonCorpse.cs` | 2 | LocalOverheadMessage |
| `Christmas/`（5 文件） | ~14 | Name = + OPL |
| `Halloween/`（16 文件） | ~40 | Name = + SendMessage + OPL + NameMod（24 套装名） |
| `Mounts/`（5 文件） | ~6 | Name = + OPL |
| `Market/FruitTrees.cs` | 3 | SendMessage + OPL |
| `Market/Statue.cs` | 2 | Name = + OPL |
| `Scrolls/ScrollofAlacrity.cs` | 2 | Name = + SendMessage |
| `Scrolls/ScrollofTranscendence.cs` | 1 | Name = |
| `Translocation/BagOfSending.cs` | 3 | Property return + SendMessage |
| `Translocation/BallOfSummoning.cs` | 4 | Name = + OPL |
| `Translocation/BraceletOfBinding.cs` | 1 | Name = |
| `ClockworkAssembly.cs` | 5 | SendMessage |
| `CommunicationCrystals.cs` | ~5 | Name = + InfoText1 + SendMessage（部分 `AddLocalizedProperty`） |
| `PowerCrystal.cs` | 5 | SendMessage |
| `PowerGenerator.cs` | ~6 | Gump AddHtml + SendMessage + Name = |
| `Gold.cs` | 2 | Name = |
| `TrashChest.cs` | 2 | Name = + OPL |
| 其他（~8 文件） | ~10 | Name = + OPL + Property return |

---

### Potions/（88 文件，~277 处硬编码）

- 所有 `DefaultDescription` 均已使用 `StringCatalog.Resolve` ✓

#### `BasePotion.cs`

| 行 | 类型 | 示例 |
|----|------|------|
| 318-345 | Name = | 28 药丸名（`"cornea dilation pills"`, `"antidote pills"`…）+ 28 血清名 + 7 燃油/灭火器（`"gasoline"`, `"fire extinguisher"`…） |

#### Standard/

| 文件 | 类型 | 示例 |
|------|------|------|
| `BaseExplosionPotion.cs` | SendMessage | `"You cannot do that yet."`, `"That doesn't feel like a good idea."` |
| `BaseConfusionBlastPotion.cs` | SendMessage | 同 |
| `BaseConflagrationPotion.cs` | SendMessage | 同 |
| `BaseFrostbitePotion.cs` | SendMessage | 同 |
| `BasePoisonPotion.cs` | SendMessage | 8 处（含 "You fumble the poison!" OverheadMessage + 抛毒全程） |
| `VenomSack.cs` | Name + SendMessage + Say + OPL | `"venom sack"`, `"Poison!"`, `"Use To Attempt To Extract Venom"` |
| `BaseRefreshPotion.cs` | SendMessage | `"You decide against drinking this potion, as you are already at full stamina."` |
| `NightSight.cs` | SendMessage | `"You already have nightsight."` |
| `*Potion.cs`（5 标准药水） | Name = | `"agility/strength/cure/poison/refresh/explosion potion"`, `"greater/lesser X potion"` |

#### Special/

| 文件 | 硬编码条数 | 主要类型 |
|------|-----------|----------|
| `BottleOfAcid.cs` | ~11 | SendMessage（全套开箱/溶锁对白）+ Name = |
| `BrewCauldron.cs` | ~33 | 30 `potionName` + Name = + SendMessage |
| `PotionCauldron.cs` | ~34 | 同 |
| `CanopicJar.cs` | 6 | SendMessage（炼金师流程） |
| `DurabilityPotion.cs` | 7 | SendMessage + Name = |
| `ResurrectPotion.cs` | 8 | Name = + 4 雇佣兵名 + SendMessage |
| `AutoResPotion.cs` | 3 | SendMessage |
| `EvilSkull.cs` | 3 | SendMessage + Name = |
| `GenderPotion.cs` | 5 | SendMessage |
| `HairOilPotion.cs` | ~12 | Gump AddHtml（10 发型选项）+ SendMessage |
| `HairDyeBottle.cs` | 4 | SendMessage |
| `HairDyePotion.cs` | 5 | Name = + SendMessage |
| `NecroSkinPotion.cs` | 5 | SendMessage + Name = |
| `InvulnerabilityPotion.cs` | 3 | Name = + SendMessage |
| `PotionOfMight.cs` | 3 | Name = + SendMessage |
| `PotionOfDexterity.cs` | 3 | 同 |
| `PotionOfWisdom.cs` | 2 | SendMessage |
| `SuperPotion.cs` | 3 | Name = + SendMessage |
| `TransmutationPotion.cs` | 6 | Name = + SendMessage |
| `HolyWater.cs` | 1 | Name = |
| `MonsterSplatter.cs` | 1 | Name = |

#### Unique/

| 文件 | 类型 | 示例 |
|------|------|------|
| `BaseManaRefreshPotion.cs` | LocalOverheadMessage + SendMessage | `"You must wait 10 seconds before using another mana potion."` + `"already at full mana"` |
| `BaseRejuvenatePotion.cs` | LocalOverheadMessage | `"You must wait 10 seconds before using another rejuvenation potion."` |
| `*Mana/Invisibility/RejuvenatePotion.cs` | Name = | `"greater/lesser mana/invisibility/rejuvenate potion"` |

#### Mixtures/

| 文件 | 类型 | 示例 |
|------|------|------|
| `BaseLiquid.cs` | SendMessage | 5 处（泼洒/距离/冷却） |
| `BaseMixture.cs` | SendMessage | 5 处（+ "too many followers"） |
| 6 种液态/史莱姆 | Name + SlimeName | `"liquid fire/ice/goo/pain/rot"`, `"slimy fire mixture"`, `"burning/freezing/diseased/irradiated slime"` |

#### Elixirs/

| 文件 | 类型 | 示例 |
|------|------|------|
| `Elixirs.cs` | Name = | 50 种 `"elixir of alchemy"`, `"elixir of anatomy"`, `"elixir of druidism"`… |

---

### Quivers/（3 文件，1 处硬编码）

| 文件 | 行 | 类型 | 示例 |
|------|----|------|------|
| `ArcherQuiver.cs` | 11 | Name = | `"quiver"` |

`BaseQuiver.cs` — `BuildingPropertyListLocale` + `AddLocalizedProperty` 模式完整，**已目录化** ✓

---

### Relics/（28 文件，~230 处硬编码）

**状态：** 28 个文件全部含硬编码。0 个文件使用 `StringCatalog`。

#### 所有 `DDRelic*.cs` 文件的共同模式：

| 模式 | 出现文件数 | 示例 |
|------|-----------|------|
| `ColorText3 = "Worth " + CoinPrice + " Gold"` | ~18 | 每个遗迹的估值第三行 |
| `Name = "a relic " + sAdj + " " + sName` | ~22 | 形容词 + 名词组合 |
| `SendMessage("…")` | ~24 | 右键鉴定消息 |
| `list.Add(1049644, "…")` OPL | 5 | 描述行 |
| `list.Add(1070722, "…")` OPL | 4 | 工具提示 |

典型文件：`DDRelicArmor.cs`, `DDRelicJewelry.cs`, `DDRelicWeapon.cs`, `DDRelicOrbs.cs`, `DDRelicTablet.cs`（含 Gump AddHtml）等。

---

### Sharpening/（23 文件，~75 处硬编码）

| 文件 | 硬编码条数 | 主要类型 |
|------|-----------|----------|
| `SharpeningStoneBase.cs` | 9 | SendMessage（hue 32） |
| `WeightingStoneBase.cs` | 9 | SendMessage（hue 32） |
| `AddDamageItemBase.cs` | 6 | SendMessage + OPL |
| `ConsecrateItemBase.cs` | 4 | SendMessage + OPL（部分 bilingual `BuildingPropertyListLocale`） |
| `SplitElementalItemBase.cs` | 4 | SendMessage + OPL（部分 bilingual） |
| 15 个派生类 | 各 1-3 | SendMessage（技能门槛/武器类型）+ Name = + OPL `"[Only usable on …]"` |
| 基类 OPL | ~8 | `"Adds damage increase"`, `"[Only usable on bladed weapons]"`… |

---

### Special/（166 文件，~200+ 处硬编码）

| 文件 | 硬编码条数 | 主要类型 |
|------|-----------|----------|
| `DragonEgg.cs` | ~30 | Say/SendMessage（长篇龙蛋任务对白——`LocalizationResolveKeys` 部分） |
| `DracolichSkull.cs` | ~15 | Say/SendMessage（骨骸任务） |
| `DrakkhenEgg.cs` | ~20 | Say/SendMessage（龙鹰蛋任务） |
| `DemonPrison.cs` | ~15 | Say/SendMessage（恶魔牢任务） |
| `DecoDeckOfTarot.cs` | ~60+ | 程序化塔罗占卜文本生成器 |
| `CharacterStatue.cs` | ~20 | 方位/材料名英文 |
| `MonsterStatue.cs` | 2 | OPL |
| Evil Home Decor 盒（EObsidianRock 等 6 文件） | ~8 | Name = |
| SoulBound/ 装饰品 | ~40+ | Name =（40+ `Name = "…"`） |

---

### Technology/（21 文件，~115 处硬编码）

| 文件 | 硬编码条数 | 主要类型 |
|------|-----------|----------|
| `SciFiJunk.cs` | ~55+ | Name =（零件/垃圾名） |
| `PlasmaTorch.cs` | ~13 | SendMessage + InfoText（同 BottleOfAcid 模式 = 熔锁交互） |
| `MaterialLiquifier.cs` | ~6 | SendMessage + Gump AddHtml |
| `AlienEgg.cs` | ~5 | SendMessage + Say + Gump |
| `ReagentJar.cs` | ~4 | Name = + InfoText + ColorText + SendMessage |
| `Landmine.cs` | ~5 | SendMessage + Name = + InfoText |
| `PortableSmelter.cs` | 5 | SendMessage |
| `DuctTape.cs` | 3 | Name = + InfoText + SendMessage |
| `MedicalRecord.cs` | 3 | Name = + Gump + OPL |
| `ThermalDetonator.cs` | 2 | SendMessage |
| `PlasmaGrenade.cs` | 2 | SendMessage |
| `ComputerDatabase.cs` | 3 | Gump AddHtml |
| `DataPad.cs` | 2 | OPL + Gump |
| `RomulanAle.cs` | 2 | Name = + InfoText（`DefaultDescription` 已目录化） |
| `Canteen.cs` | 2 | Name = + InfoText（`DefaultDescription` 已目录化） |
| 武器类（4 文件） | ~5 | Name = + OPL |

---

### Trades/（233 文件，~200 处硬编码）

| 文件 | 硬编码条数 | 主要类型 |
|------|-----------|----------|
| `PotionKeg.cs` | ~100 | `keg.Name =` 英文桶名（全部标准药水 + 灵药） |
| `WetClothes.cs` | ~40+ | 湿衣物 形容词 + 衣物名组合 |
| `NewFish.cs` | ~30+ | 鱼名组合 `"a fish"`, `"a big fish"`… |
| `PickBox.cs` | ~5 | InfoText1（难易度 `"Easy"/"Medium"/"Hard"/"Very Hard"/"Insane"`）+ Name = |
| `Scissors.cs` | 4 | `CutUp()` 返回字符串 |
| `RecallRune.cs` | 2 | `RuneFormat` + `"an unknown location"` |
| `WoodworkingTools.cs` | 3 | InfoText1-3 |
| `BaseRunicTool.cs` | 2 | InfoText2 + DefaultDescription（部分已目录化） |
| `LockPick.cs` | 1 | DefaultDescription 未目录化 |
| `MasterSkeltonsKey.cs` | 1 | DefaultDescription 未目录化 |
| `BaseReagent.cs` | 1 | InfoText1 = "Reagent" |
| Map 属性（3 文件） | 3 | Property return（`"Huge Map"/"Small Map"/"Large Map"`） |
| 训练书（4 文件） | 4 | Property return 英文标题 |
| 海洋遗迹（14 文件） | ~14 | Name =（水族箱/海豚/颅骨/枪鱼/贝壳） |
| 渔网（3 文件） | 3 | Name = |

---

### Traps/（26 文件，~50 处硬编码）

- `HiddenTrap` / `CurseItem` / `TrapWand` / `TrapKit` / `SetTrap` / `SpellTrap` / `TenFootPole` — 已通过 `StringCatalog.ResolveByKey` / `trap.*` shotkey **已目录化** ✓

#### 陷阱覆盖物（未目录化）：

| 文件 | 硬编码条数 | 主要类型 |
|------|-----------|----------|
| `SewageItem.cs` | 3 | Name = + OPL + SendMessage |
| `WeedItem.cs` | 3 | Name = + OPL + SendMessage |
| `SlimeItem.cs` | 3 | Name = + OPL + SendMessage |
| `TaintedBandage.cs` | 2 | Name = + OPL |
| `MushroomTrap.cs` | 7 | Name =（6 种蘑菇）+ LocalOverheadMessage（3 种效果） |
| `KillerTile.cs` | 3 | Say + LocalOverheadMessage |
| `JewelryBox.cs` | 5 | Name = + ColorText1/3/4/5 |
| `BookBox.cs` | 5 | ColorText1/3/4/5 |
| `BrokenGear.cs` | 2 | Name = + SendMessage |
| `DroppedPack.cs` | 1 | Name = |
| `Blocker.cs` | 1 | Name = |

---

### Trinkets/（14 文件，~65 处硬编码）

| 文件 | 硬编码条数 | 主要类型 |
|------|-----------|----------|
| `TrinketTalisman.cs` | ~35+ | Name = 名变体 + SendMessage + OPL |
| `GuildRing.cs` | ~20 | Name =（17 公会名）+ OPL + SendMessage |
| `SavageTalisman.cs` | 4 | Name = + OPL + LocalOverheadMessage |
| `OldSwordTalisman.cs` | 2 | OPL（SendMessage 已 `StringCatalog.ResolveByKey` ✓） |
| `MagicalWand.cs` | 2 | Name = + ColorText1 |
| 5 基础首饰 | 5 | Name =（`"earrings"`, `"bracelet"`, `"ring"`, `"necklace"`, `"circlet"`） |
| 2 灯具 | 2 | Name =（`"candle"`, `"lantern"`, `"torch"`） |

---

### Weapons/（110 文件，~200+ 处硬编码）

#### `BaseWeapon.cs` — SendMessage 硬编码（P0）

| 行 | 类型 | 示例 |
|----|------|------|
| — | SendMessage | `Only {0} may use this.` |
| — | SendMessage | `You may not wear this.` |
| — | SendMessage | `You lack the dexterity to equip this weapon.` |
| — | SendMessage | `You must wait a moment before using another special attack!` |
| — | OPL fallback | `"skill required: fist fighting"` |

#### `Marksman/WizardStaff.cs`

| 行 | 类型 | 示例 |
|----|------|------|
| — | SendMessage | 6 处变形反馈消息 |

#### `Bows/JukaBow.cs`

| 行 | 类型 | 示例 |
|----|------|------|
| — | SendMessage | 9 处改造交互消息 |

#### `Marksman/Harpoon.cs`, `ThrowingGloves.cs`

| 类型 | 示例 |
|------|------|
| SendMessage + OPL | 装备/使用反馈 + `"Cannot be used with hand-held weapons"` |

#### `Knives/ThrowingDagger.cs`

| 类型 | 示例 |
|------|------|
| SendMessage | 3 处投掷反馈 |

#### `Axes/BaseAxe.cs`, `PoleArms/BasePoleArm.cs`

| 类型 | 示例 |
|------|------|
| SendMessage | 震荡击（Concussion Blow）反馈（重复） |

#### OPL 英文回退（4 文件）

| 文件 | 示例 |
|------|------|
| `Hands/PugilistGlove.cs` | `"Cannot be used with hand-held weapons"` |
| `Hands/PugilistMits.cs` | 同 |
| `Marksman/ThrowingGloves.cs` | `"Double click to change type from…"` |
| `Marksman/ThrowingWeapon.cs` | `"Double click to change ammo from…"`, `"Can Be Used With Throwing Gloves"` |

#### `Name = "…"`（~58+ 文件）

Axes/、Bows/、Fencing/、Knives/、Maces/、PoleArms/、Ranged/、SpearsAndForks/、Staves/、Swords/、ML Weapons/、SE Weapons/ 各子目录的基础武器名全部为英文硬编码（`"axe"`, `"bow"`, `"dagger"`, `"mace"`, `"spear"`, `"staff"`, `"sword"` 等）。

> 注：许多武器已有 `DisplayNameLocalizationKey`，但 `Name` 仍为回退英文。

---

## 已目录化（仅 cliloc / `StringCatalog` 已覆盖）

以下文件含 cliloc 或已有 `StringCatalog` 入口，不在上述待办范围内：

- `Armor/Glasses/*.cs` — cliloc 编号
- `Armor/Ranger/*.cs` — cliloc 编号
- `Magical/Artifacts/Books/Artifact_PyrosGrimoire.cs` — OPL `BuildingPropertyListLocale`
- `Potions/*/Base*.cs`（31 文件）— `DefaultDescription` 已 `StringCatalog.Resolve`
- `Misc/OilCloth.cs` — `DefaultDescription` 已 `StringCatalog.Resolve`
- `Misc/Translocation/MessageHelper.cs` — `StringCatalog.TryResolve`
- `Misc/Bodies/Corpse.cs` — `AddLocalizedProperty("prop.colortext.corpse.carvable")`
- `Explorers/*.cs`（8 文件）— `DefaultDescription` 已 `StringCatalog.Resolve`
- `Food/Food.cs`, `Beverage.cs`, `Cooking.cs` — `DefaultDescription` 已 `StringCatalog.Resolve`
- `Books/BaseBook.cs` — `DefaultDescription` 已 `StringCatalog.ResolveByKey`
- `Traps/HiddenTrap.cs`, `CurseItem.cs`, `TrapWand.cs`, `TrapKit.cs`, `SetTrap.cs`, `SpellTrap.cs`, `TenFootPole.cs` — `trap.*` shotkey 已目录化
- `Quivers/BaseQuiver.cs` — 完整 `BuildingPropertyListLocale` + `AddLocalizedProperty`
- `Trades/Cartography/Maps/PlaceMap.cs` — `placemap-labels.json` shotkey
- `Technology/RomulanAle.cs`, `Canteen.cs` — `DefaultDescription` 已目录化
- `Trinkets/OldSwordTalisman.cs` — SendMessage 已 `StringCatalog.ResolveByKey`

---

## 无英文硬编码（或仅 cliloc / 已目录化）

- `Gems/Amethyst.cs`, `Citrine.cs`, `Diamond.cs`, `Emerald.cs`, `Ruby.cs`, `Sapphire.cs`, `StarSapphire.cs`, `Tourmaline.cs` — 继承 cliloc
- `Quivers/BaseQuiver.cs` — 完整目录化
- `Quivers/ElvenQuiver.cs` — 无硬编码
- `Potions/Mixtures/AlchemicSlime.cs` — 无硬编码
- `Potions/Unique/LesserRejuvenatePotion.cs`, `ManaPotion.cs` — 无硬编码
- `Traps/HiddenTrap.cs`, `CurseItem.cs`, `TrapWand.cs`, `TrapKit.cs`, `SetTrap.cs`, `SpellTrap.cs`, `TenFootPole.cs` — shotkey 完整
- `Technology/BaseTechnology.cs` — 无硬编码（若有）
- `Misc/Crystal/` — 无硬编码
- `Misc/Rugs/` — 无硬编码
- `Abstractions/` — 接口仅

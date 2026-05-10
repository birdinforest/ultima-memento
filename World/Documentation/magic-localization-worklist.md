## Magic 脚本本地化工作清单（可抽取 + zh-Hans）

### 进度快照（2026-05-02）

| 状态 | 文件 |
|------|------|
| ✅ 已管道化 + 已补 zh | `Base/ForgetfulGem.cs`、`Base/Spell.cs`、`Necromancy/SummonFamiliar.cs`、`Mystic/MysticSpellBook.cs`、`Mystic/MysticSpellBookGump.cs` |
| ☐ 待办（优先 A 批 Gump） | 见下表 |

**说明：** `llm_incremental_locale.py stats` 将以 `<` 开头的 HTML 当作「身份可接受」而**不排队列**；大型 gump 须在提取后**手动核对** `zh-Hans` 是否仍为英文，必要时直接用 `apply` JSON 或编辑器补译（本次 mystic 多段正文已按此处理）。

> 自动生成：2026-05-02  |  根目录：`World/Source/Scripts/Engines and Systems/Magic/`
> 判定：`SendMessage("`、`Say("`、`AddHtml(`、`Name = "` ；`SC= Y` 表示文件内已出现 `StringCatalog`。

### 批次说明
| 批次 | 范围 | 说明 |
|------|------|------|
| **A** | Html & SC≠Y | 大型 Gump / 背包 UI，须整段 `StringCatalog.Resolve*`，避免 `+ color +` 断 HTML |
| **B** | Msg/Say & SC≠Y | 逐条 `SendMessage`/`Say` 改为 `StringCatalog.Resolve(… Account …)`；拼接用 `ResolveFormat` |
| **C** | SC=Y | 复检是否仍有裸字符串遗漏 |

### 文件表（按处理优先级排序）

| 状态 | 签名 | SC | 相对路径 |
|------|------|----|----------|
| ☐ | Msg+Html+Name | N | `Base/ForgetfulGem.cs` |
| ☐ | Msg+Html+Name | N | `Druidism/DruidPouch.cs` |
| ☐ | Msg+Html | N | `Jedi/JediSpellBookGump.cs` |
| ☐ | Msg+Html | N | `Misc/MagicRuneBag.cs` |
| ☐ | Msg+Html | N | `Necromancy/SummonFamiliar.cs` |
| ☐ | Msg+Html+Name | N | `Research/ResearchBag.cs` |
| ☐ | Msg+Html+Name | N | `Shinobi/ShinobiScroll.cs` |
| ☐ | Msg+Html | N | `Syth/SythSpellBookGump.cs` |
| ☐ | Msg+Html+Name | N | `Witch/WitchPouch.cs` |
| ☐ | Html | N | `Bard/SongBookGump.cs` |
| ☐ | Html+Name | N | `Death Knight/DeathKnightSpellBookGump.cs` |
| ☐ | Html | N | `Elementalism/ElementalSpellbookGump.cs` |
| ☐ | Html+Name | N | `Holy Man/HolyManSpellBookGump.cs` |
| ☐ | Html+Name | N | `Mystic/MysticSpellBookGump.cs` |
| ☐ | Html | N | `Research/AncientSpellBookGump.cs` |
| ☐ | Msg+Say | N | `Holy Man/Spells/BanishEvilSpell.cs` |
| ☐ | Msg+Say | N | `Jester/JesterSpell.cs` |
| ☐ | Msg+Say+Name | N | `Jester/Spells/CanOfSnakes.cs` |
| ☐ | Msg+Say | N | `Jester/Spells/Clowns.cs` |
| ☐ | Msg+Say | N | `Jester/Spells/Insult.cs` |
| ☐ | Msg+Say+Name | N | `Jester/Spells/PoppingBalloon.cs` |
| ☐ | Msg+Say+Name | N | `Jester/Spells/RabbitInAHat.cs` |
| ☐ | Msg+Say+Name | N | `Jester/Spells/SurpriseGift.cs` |
| ☐ | Msg+Say | N | `Necromancy/Exorcism.cs` |
| ☐ | Msg+Say | N | `Research/Spells/Death/ResearchDeathSpeak.cs` |
| ☐ | Msg+Say | N | `Research/Spells/Death/ResearchGrantPeace.cs` |
| ☐ | Msg+Say | N | `Research/Spells/Thaumaturgy/ResearchBanishDaemon.cs` |
| ☐ | Msg+Say | N | `Shinobi/Spells/EagleEye.cs` |
| ☐ | Msg+Name | N | `Bard/Scrolls/ArmysPaeon.cs` |
| ☐ | Msg+Name | N | `Bard/Scrolls/EnchantingEtude.cs` |
| ☐ | Msg+Name | N | `Bard/Scrolls/EnergyCarol.cs` |
| ☐ | Msg+Name | N | `Bard/Scrolls/EnergyThrenody.cs` |
| ☐ | Msg+Name | N | `Bard/Scrolls/FireCarol.cs` |
| ☐ | Msg+Name | N | `Bard/Scrolls/FireThrenody.cs` |
| ☐ | Msg+Name | N | `Bard/Scrolls/FoeRequiem.cs` |
| ☐ | Msg+Name | N | `Bard/Scrolls/IceCarol.cs` |
| ☐ | Msg+Name | N | `Bard/Scrolls/IceThrenody.cs` |
| ☐ | Msg+Name | N | `Bard/Scrolls/KnightsMinne.cs` |
| ☐ | Msg+Name | N | `Bard/Scrolls/MagesBallad.cs` |
| ☐ | Msg+Name | N | `Bard/Scrolls/MagicFinale.cs` |
| ☐ | Msg+Name | N | `Bard/Scrolls/PoisonCarol.cs` |
| ☐ | Msg+Name | N | `Bard/Scrolls/PoisonThrenody.cs` |
| ☐ | Msg+Name | N | `Bard/Scrolls/SheepfoeMambo.cs` |
| ☐ | Msg+Name | N | `Bard/Scrolls/SinewyEtude.cs` |
| ☐ | Msg | N | `Bard/SongSpells.cs` |
| ☐ | Msg | N | `Bard/Spells/ArmysPaeonSong.cs` |
| ☐ | Msg+Name | N | `Bard/Spells/EnchantingEtudeSong.cs` |
| ☐ | Msg | N | `Bard/Spells/EnergyCarolSong.cs` |
| ☐ | Msg | N | `Bard/Spells/EnergyThrenodySong.cs` |
| ☐ | Msg | N | `Bard/Spells/FireCarolSong.cs` |
| ☐ | Msg | N | `Bard/Spells/FireThrenodySong.cs` |
| ☐ | Msg | N | `Bard/Spells/FoeRequiemSong.cs` |
| ☐ | Msg | N | `Bard/Spells/IceCarolSong.cs` |
| ☐ | Msg | N | `Bard/Spells/IceThrenodySong.cs` |
| ☐ | Msg | N | `Bard/Spells/KnightsMinneSong.cs` |
| ☐ | Msg | N | `Bard/Spells/MagesBalladSong.cs` |
| ☐ | Msg | N | `Bard/Spells/PoisonCarolSong.cs` |
| ☐ | Msg | N | `Bard/Spells/PoisonThrenodySong.cs` |
| ☐ | Msg+Name | N | `Bard/Spells/SheepfoeMamboSong.cs` |
| ☐ | Msg+Name | N | `Bard/Spells/SinewyEtudeSong.cs` |
| ☐ | Msg | N | `Base/Spell.cs` |
| ☐ | Msg | N | `Base/SpellItemInfo.cs` |
| ☐ | Msg | N | `Death Knight/DeathKnightSpell.cs` |
| ☐ | Msg+Name | N | `Death Knight/DeathKnightSpellBook.cs` |
| ☐ | Msg+Name | N | `Death Knight/DeathSkulls.cs` |
| ☐ | Msg+Name | N | `Death Knight/SoulLantern.cs` |
| ☐ | Msg | N | `Death Knight/Spells/DevilPactSpell.cs` |
| ☐ | Msg+Name | N | `Death Knight/Spells/HagHandSpell.cs` |
| ☐ | Msg | N | `Death Knight/Spells/HellfireSpell.cs` |
| ☐ | Msg | N | `Death Knight/Spells/ShieldOfHateSpell.cs` |
| ☐ | Msg | N | `Death Knight/Spells/SoulReaperSpell.cs` |
| ☐ | Msg | N | `Death Knight/Spells/SuccubusSkinSpell.cs` |
| ☐ | Msg | N | `Druidism/Effects/DruidicRuneSpell.cs` |
| ☐ | Msg | N | `Druidism/Effects/FireflySpell.cs` |
| ☐ | Msg | N | `Druidism/Effects/MushroomGatewaySpell.cs` |
| ☐ | Msg | N | `Druidism/Effects/NaturesPassageSpell.cs` |
| ☐ | Msg+Name | N | `Druidism/Effects/RestorativeSoilSpell.cs` |
| ☐ | Msg+Name | N | `Druidism/Effects/StoneCircleSpell.cs` |
| ☐ | Msg+Name | N | `Druidism/Effects/TreefellowSpell.cs` |
| ☐ | Msg+Name | N | `Elementalism/ElementalShrine.cs` |
| ☐ | Msg+Name | N | `Elementalism/ElementalSpell.cs` |
| ☐ | Msg+Name | N | `Elementalism/Sphere 1/Elemental_Sanctuary.cs` |
| ☐ | Msg+Name | N | `Elementalism/Sphere 4/Elemental_Void.cs` |
| ☐ | Msg | N | `Elementalism/Sphere 5/Elemental_Echo.cs` |
| ☐ | Msg | N | `Elementalism/Sphere 5/Elemental_Fiend.cs` |
| ☐ | Msg | N | `Elementalism/Sphere 6/Elemental_Rune.cs` |
| ☐ | Msg | N | `Elementalism/Sphere 7/Elemental_Gate.cs` |
| ☐ | Msg+Name | N | `Elementalism/Sphere 8/Elemental_Soul.cs` |
| ☐ | Msg | N | `Elementalism/Sphere 8/Elemental_Spirit.cs` |
| ☐ | Msg | N | `Holy Man/HolyManSpell.cs` |
| ☐ | Msg+Name | N | `Holy Man/HolySymbol.cs` |
| ☐ | Msg+Name | N | `Holy Man/HolySymbols.cs` |
| ☐ | Msg+Name | N | `Holy Man/MalletStake.cs` |
| ☐ | Msg | N | `Holy Man/Spells/DampenSpiritSpell.cs` |
| ☐ | Msg+Name | N | `Holy Man/Spells/EnchantSpell.cs` |
| ☐ | Msg+Name | N | `Holy Man/Spells/HammerOfFaithSpell.cs` |
| ☐ | Msg | N | `Holy Man/Spells/HeavenlyLightSpell.cs` |
| ☐ | Msg | N | `Holy Man/Spells/NourishSpell.cs` |
| ☐ | Msg+Name | N | `Holy Man/Spells/RebirthSpell.cs` |
| ☐ | Msg | N | `Holy Man/Spells/SacredBoonSpell.cs` |
| ☐ | Msg | N | `Holy Man/Spells/SeanceSpell.cs` |
| ☐ | Msg | N | `Holy Man/Spells/TrialByFireSpell.cs` |
| ☐ | Msg+Name | N | `Jedi/JediDatacrons.cs` |
| ☐ | Msg | N | `Jedi/JediSpell.cs` |
| ☐ | Msg+Name | N | `Jedi/JediSpellbook.cs` |
| ☐ | Msg | N | `Jedi/Spells/Celerity.cs` |
| ☐ | Msg | N | `Jedi/Spells/Deflection.cs` |
| ☐ | Msg | N | `Jedi/Spells/ForceGrip.cs` |
| ☐ | Msg | N | `Jedi/Spells/MindsEye.cs` |
| ☐ | Msg | N | `Jedi/Spells/Mirage.cs` |
| ☐ | Msg | N | `Jedi/Spells/Replicate.cs` |
| ☐ | Msg | N | `Jedi/Spells/ThrowSabre.cs` |
| ☐ | Msg | N | `Jester/JesterCommandList.cs` |
| ☐ | Msg | N | `Knight/PaladinSpell.cs` |
| ☐ | Msg+Name | N | `Knight/RemoveCurse.cs` |
| ☐ | Msg | N | `Knight/SacredJourney.cs` |
| ☐ | Msg | N | `Magery/Spellbook.cs` |
| ☐ | Msg | N | `Magery/Spells/Magery 1st/CreateFood.cs` |
| ☐ | Msg | N | `Magery/Spells/Magery 1st/NightSight.cs` |
| ☐ | Msg | N | `Magery/Spells/Magery 2nd/MagicTrap.cs` |
| ☐ | Msg | N | `Magery/Spells/Magery 2nd/RemoveTrap.cs` |
| ☐ | Msg+Name | N | `Magery/Spells/Magery 3rd/MagicLock.cs` |
| ☐ | Msg | N | `Magery/Spells/Magery 3rd/Telekinesis.cs` |
| ☐ | Msg | N | `Magery/Spells/Magery 3rd/Unlock.cs` |
| ☐ | Msg | N | `Magery/Spells/Magery 4th/Recall.cs` |
| ☐ | Msg | N | `Magery/Spells/Magery 5th/BladeSpirits.cs` |
| ☐ | Msg | N | `Magery/Spells/Magery 5th/Incognito.cs` |
| ☐ | Msg | N | `Magery/Spells/Magery 5th/MagicReflect.cs` |
| ☐ | Msg | N | `Magery/Spells/Magery 6th/Mark.cs` |
| ☐ | Msg | N | `Magery/Spells/Magery 6th/Reveal.cs` |
| ☐ | Msg | N | `Magery/Spells/Magery 7th/GateTravel.cs` |
| ☐ | Msg | N | `Magery/Spells/Magery 8th/EnergyVortex.cs` |
| ☐ | Msg+Name | N | `Magery/Spells/Magery 8th/Resurrection.cs` |
| ☐ | Msg | N | `Misc/AttackSpells.cs` |
| ☐ | Msg+Name | N | `Misc/SummonDragonSpell.cs` |
| ☐ | Msg+Name | N | `Misc/SummonSnakesSpell.cs` |
| ☐ | Msg | N | `Misc/TravelSpell.cs` |
| ☐ | Msg+Name | N | `Mystic/MysticMonkRobe.cs` |
| ☐ | Msg+Name | N | `Mystic/MysticPack.cs` |
| ☐ | Msg | N | `Mystic/MysticSpell.cs` |
| ☐ | Msg+Name | N | `Mystic/MysticSpellBook.cs` |
| ☐ | Msg+Name | N | `Mystic/Scrolls/AstralProjectionScroll.cs` |
| ☐ | Msg+Name | N | `Mystic/Scrolls/AstralTravelScroll.cs` |
| ☐ | Msg+Name | N | `Mystic/Scrolls/CreateRobeScroll.cs` |
| ☐ | Msg+Name | N | `Mystic/Scrolls/GentleTouchScroll.cs` |
| ☐ | Msg+Name | N | `Mystic/Scrolls/LeapScroll.cs` |
| ☐ | Msg+Name | N | `Mystic/Scrolls/PsionicBlastScroll.cs` |
| ☐ | Msg+Name | N | `Mystic/Scrolls/PsychicWallScroll.cs` |
| ☐ | Msg+Name | N | `Mystic/Scrolls/PurityOfBodyScroll.cs` |
| ☐ | Msg+Name | N | `Mystic/Scrolls/QuiveringPalmScroll.cs` |
| ☐ | Msg+Name | N | `Mystic/Scrolls/WindRunnerScroll.cs` |
| ☐ | Msg | N | `Mystic/Spells/AstralProjection.cs` |
| ☐ | Msg | N | `Mystic/Spells/AstralTravel.cs` |
| ☐ | Msg | N | `Mystic/Spells/Leap.cs` |
| ☐ | Msg | N | `Mystic/Spells/PsychicWall.cs` |
| ☐ | Msg | N | `Mystic/Spells/PurityOfBody.cs` |
| ☐ | Msg | N | `Mystic/Spells/QuiveringPalm.cs` |
| ☐ | Msg | N | `Mystic/Spells/WindRunner.cs` |
| ☐ | Msg+Name | N | `Necromancy/AnimateDeadSpell.cs` |
| ☐ | Msg | N | `Ninjitsu/FocusAttack.cs` |
| ☐ | Msg+Name | N | `Research/AncientSpellBook.cs` |
| ☐ | Msg | N | `Research/ResearchFunctions.cs` |
| ☐ | Msg | N | `Research/ResearchSpell.cs` |
| ☐ | Msg | N | `Research/Spells/Conjuration/ResearchCreateGold.cs` |
| ☐ | Msg | N | `Research/Spells/Conjuration/ResearchDeathVortex.cs` |
| ☐ | Msg | N | `Research/Spells/Conjuration/ResearchExtinguish.cs` |
| ☐ | Msg | N | `Research/Spells/Conjuration/ResearchSwarm.cs` |
| ☐ | Msg+Name | N | `Research/Spells/Death/ResearchMaskofDeath.cs` |
| ☐ | Msg+Name | N | `Research/Spells/Death/ResearchOpenGround.cs` |
| ☐ | Msg | N | `Research/Spells/Death/ResearchRockFlesh.cs` |
| ☐ | Msg+Name | N | `Research/Spells/Death/ResearchWithstandDeath.cs` |
| ☐ | Msg | N | `Research/Spells/Enchanting/ResearchCauseFear.cs` |
| ☐ | Msg | N | `Research/Spells/Enchanting/ResearchCharm.cs` |
| ☐ | Msg+Name | N | `Research/Spells/Enchanting/ResearchEnchant.cs` |
| ☐ | Msg | N | `Research/Spells/Enchanting/ResearchMassSleep.cs` |
| ☐ | Msg | N | `Research/Spells/Enchanting/ResearchSleep.cs` |
| ☐ | Msg | N | `Research/Spells/Enchanting/ResearchSleepField.cs` |
| ☐ | Msg+Name | N | `Research/Spells/Sorcery/ResearchConflagration.cs` |
| ☐ | Msg+Name | N | `Research/Spells/Sorcery/ResearchCreateFire.cs` |
| ☐ | Msg | N | `Research/Spells/Sorcery/ResearchEndureCold.cs` |
| ☐ | Msg | N | `Research/Spells/Sorcery/ResearchEndureHeat.cs` |
| ☐ | Msg | N | `Research/Spells/Sorcery/ResearchIgnite.cs` |
| ☐ | Msg+Name | N | `Research/Spells/Sorcery/ResearchRingofFire.cs` |
| ☐ | Msg | N | `Research/Spells/Thaumaturgy/ResearchCallDestruction.cs` |
| ☐ | Msg | N | `Research/Spells/Thaumaturgy/ResearchConfusionBlast.cs` |
| ☐ | Msg | N | `Research/Spells/Thaumaturgy/ResearchDevastation.cs` |
| ☐ | Msg | N | `Research/Spells/Thaumaturgy/ResearchEtherealTravel.cs` |
| ☐ | Msg | N | `Research/Spells/Thaumaturgy/ResearchMeteorShower.cs` |
| ☐ | Msg | N | `Research/Spells/Theurgy/ResearchDivination.cs` |
| ☐ | Msg | N | `Research/Spells/Theurgy/ResearchRestoration.cs` |
| ☐ | Msg | N | `Research/Spells/Theurgy/ResearchSeeTruth.cs` |
| ☐ | Msg | N | `Research/Spells/Wizardry/ResearchAvalanche.cs` |
| ☐ | Msg+Name | N | `Research/Spells/Wizardry/ResearchFrostField.cs` |
| ☐ | Msg | N | `Research/Spells/Wizardry/ResearchGasCloud.cs` |
| ☐ | Msg | N | `Research/Spells/Wizardry/ResearchHailStorm.cs` |
| ☐ | Msg | N | `Research/Spells/Wizardry/ResearchMassDeath.cs` |
| ☐ | Msg | N | `Shinobi/ShinobiCommandList.cs` |
| ☐ | Msg | N | `Shinobi/ShinobiSpell.cs` |
| ☐ | Msg | N | `Shinobi/Spells/CheetahPaws.cs` |
| ☐ | Msg | N | `Shinobi/Spells/Deception.cs` |
| ☐ | Msg | N | `Shinobi/Spells/Espionage.cs` |
| ☐ | Msg | N | `Shinobi/Spells/FerretFlee.cs` |
| ☐ | Msg | N | `Syth/Spells/Absorption.cs` |
| ☐ | Msg | N | `Syth/Spells/Clone.cs` |
| ☐ | Msg | N | `Syth/Spells/DeathGrip.cs` |
| ☐ | Msg | N | `Syth/Spells/DrainLife.cs` |
| ☐ | Msg | N | `Syth/Spells/Projection.cs` |
| ☐ | Msg | N | `Syth/Spells/Psychokinesis.cs` |
| ☐ | Msg | N | `Syth/Spells/Speed.cs` |
| ☐ | Msg | N | `Syth/Spells/ThrowSword.cs` |
| ☐ | Msg+Name | N | `Syth/SythDatacrons.cs` |
| ☐ | Msg | N | `Syth/SythSpell.cs` |
| ☐ | Msg+Name | N | `Syth/SythSpellbook.cs` |
| ☐ | Msg | N | `Witch/Effects/BloodPact.cs` |
| ☐ | Msg+Name | N | `Witch/Effects/GraveyardGateway.cs` |
| ☐ | Msg | N | `Witch/Effects/HellsBrand.cs` |
| ☐ | Msg | N | `Witch/Effects/HellsGate.cs` |
| ☐ | Msg | N | `Witch/Effects/NecroUnlock.cs` |
| ☐ | Msg | N | `Witch/Effects/Phantasm.cs` |
| ☐ | Msg | N | `Witch/Effects/UndeadEyes.cs` |
| ☐ | Msg+Name | N | `Witch/Effects/VampireGift.cs` |
| ☐ | Msg | N | `Witch/UndeadSpell.cs` |
| ☐ | Say | N | `Jester/Spells/Hilarity.cs` |
| ☐ | Say | N | `Jester/Spells/JumpAround.cs` |
| ☐ | Name | N | `Bard/SongBook.cs` |
| ☐ | Name | N | `Bushido/BookOfBushido.cs` |
| ☐ | Html+Name | Y | `Druidism/BookDruidBrewing.cs` |
| ☐ | Name | N | `Druidism/Effects/LureStoneSpell.cs` |
| ☐ | Name | N | `Druidism/Effects/ProtectiveFairySpell.cs` |
| ☐ | Name | N | `Druidism/HerbalistPotions.cs` |
| ☐ | Name | N | `Elementalism/ElementalExit.cs` |
| ☐ | Name | N | `Elementalism/ElementalSpellbook.cs` |
| ☐ | Name | N | `Elementalism/Mobiles/ElementalCalledAir.cs` |
| ☐ | Name | N | `Elementalism/Mobiles/ElementalCalledEarth.cs` |
| ☐ | Name | N | `Elementalism/Mobiles/ElementalCalledFire.cs` |
| ☐ | Name | N | `Elementalism/Mobiles/ElementalCalledWater.cs` |
| ☐ | Name | N | `Elementalism/Mobiles/ElementalFiendAir.cs` |
| ☐ | Name | N | `Elementalism/Mobiles/ElementalFiendEarth.cs` |
| ☐ | Name | N | `Elementalism/Mobiles/ElementalFiendFire.cs` |
| ☐ | Name | N | `Elementalism/Mobiles/ElementalFiendWater.cs` |
| ☐ | Name | N | `Elementalism/Mobiles/ElementalSpiritAir.cs` |
| ☐ | Name | N | `Elementalism/Mobiles/ElementalSpiritEarth.cs` |
| ☐ | Name | N | `Elementalism/Mobiles/ElementalSpiritFire.cs` |
| ☐ | Name | N | `Elementalism/Mobiles/ElementalSpiritWater.cs` |
| ☐ | Name | N | `Elementalism/Mobiles/ElementalSummonEnt.cs` |
| ☐ | Name | N | `Elementalism/Mobiles/ElementalSummonIce.cs` |
| ☐ | Name | N | `Elementalism/Mobiles/ElementalSummonLightning.cs` |
| ☐ | Name | N | `Elementalism/Mobiles/ElementalSummonMagma.cs` |
| ☐ | Name | N | `Elementalism/Sphere 2/Elemental_Steed.cs` |
| ☐ | Name | N | `Elementalism/Sphere 3/Elemental_Wall.cs` |
| ☐ | Name | N | `Elementalism/Sphere 8/Elemental_Lord.cs` |
| ☐ | Name | Y | `Holy Man/HolyManSpellBook.cs` |
| ☐ | Name | N | `Jedi/KaranCrystal.cs` |
| ☐ | Msg+Html+Name | Y | `Jester/BagOfTricks.cs` |
| ☐ | Name | N | `Jester/SummonedJoke.cs` |
| ☐ | Name | N | `Jester/SummonedPrank.cs` |
| ☐ | Name | N | `Knight/BookOfChivalry.cs` |
| ☐ | Name | N | `Misc/SummonSkeleton.cs` |
| ☐ | Name | N | `Necromancy/NecromancerSpellbook.cs` |
| ☐ | Name | N | `Necromancy/Scrolls/AnimateDeadScroll.cs` |
| ☐ | Name | N | `Necromancy/Scrolls/BloodOathScroll.cs` |
| ☐ | Name | N | `Necromancy/Scrolls/CorpseSkinScroll.cs` |
| ☐ | Name | N | `Necromancy/Scrolls/CurseWeaponScroll.cs` |
| ☐ | Name | N | `Necromancy/Scrolls/EvilOmenScroll.cs` |
| ☐ | Name | N | `Necromancy/Scrolls/ExorcismScroll.cs` |
| ☐ | Name | N | `Necromancy/Scrolls/HorrificBeastScroll.cs` |
| ☐ | Name | N | `Necromancy/Scrolls/LichFormScroll.cs` |
| ☐ | Name | N | `Necromancy/Scrolls/MindRotScroll.cs` |
| ☐ | Name | N | `Necromancy/Scrolls/PainSpikeScroll.cs` |
| ☐ | Name | N | `Necromancy/Scrolls/PoisonStrikeScroll.cs` |
| ☐ | Name | N | `Necromancy/Scrolls/StrangleScroll.cs` |
| ☐ | Name | N | `Necromancy/Scrolls/SummonFamiliarScroll.cs` |
| ☐ | Name | N | `Necromancy/Scrolls/VampiricEmbraceScroll.cs` |
| ☐ | Name | N | `Necromancy/Scrolls/VengefulSpiritScroll.cs` |
| ☐ | Name | N | `Necromancy/Scrolls/WitherScroll.cs` |
| ☐ | Name | N | `Necromancy/Scrolls/WraithFormScroll.cs` |
| ☐ | Name | N | `Ninjitsu/BookOfNinjitsu.cs` |
| ☐ | Name | N | `Research/Spells/Conjuration/ResearchAerialServant.cs` |
| ☐ | Name | N | `Syth/HellShard.cs` |
| ☐ | Html+Name | Y | `Witch/BookWitchBrewing.cs` |
| ☐ | Name | N | `Witch/Effects/WallOfSpikes.cs` |
| ☐ | Name | N | `Witch/WitchBrews.cs` |

**合计：** 282 个文件需人工对照（554 个 `.cs` 中其余多为无上述语料的逻辑代码）。

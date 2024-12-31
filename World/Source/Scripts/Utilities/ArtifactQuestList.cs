namespace Server.Utilities
{
	public static class ArtifactQuestList
	{
		public const int MaxNumber = 308;

		public static string GetArtifact(int artifact, int part)
		{
			string item = "";
			string name = "";
			int arty = 1;

			if (artifact == arty++) { name = "Artifact_AbysmalGloves"; item = "Abysmal Gloves"; }
			else if (artifact == arty++) { name = "Artifact_AchillesShield"; item = "Achille's Shield"; }
			else if (artifact == arty++) { name = "Artifact_AchillesSpear"; item = "Achille's Spear"; }
			else if (artifact == arty++) { name = "Artifact_AcidProofRobe"; item = "Acidic Robe"; }
			else if (artifact == arty++) { name = "Artifact_Aegis"; item = "Aegis"; }
			else if (artifact == arty++) { name = "Artifact_AegisOfGrace"; item = "Aegis of Grace"; }
			else if (artifact == arty++) { name = "Artifact_AilricsLongbow"; item = "Ailric's Longbow"; }
			else if (artifact == arty++) { name = "Artifact_AlchemistsBauble"; item = "Alchemist's Bauble"; }
			else if (artifact == arty++) { name = "Artifact_SamuraiHelm"; item = "Ancient Samurai Helm"; }
			else if (artifact == arty++) { name = "Artifact_AngelicEmbrace"; item = "Angelic Embrace"; }
			else if (artifact == arty++) { name = "Artifact_AngeroftheGods"; item = "Anger of the Gods"; }
			else if (artifact == arty++) { name = "Artifact_Annihilation"; item = "Annihilation"; }
			else if (artifact == arty++) { name = "Artifact_ArcaneArms"; item = "Arcane Arms"; }
			else if (artifact == arty++) { name = "Artifact_ArcaneCap"; item = "Arcane Cap"; }
			else if (artifact == arty++) { name = "Artifact_ArcaneGloves"; item = "Arcane Gloves"; }
			else if (artifact == arty++) { name = "Artifact_ArcaneGorget"; item = "Arcane Gorget"; }
			else if (artifact == arty++) { name = "Artifact_ArcaneLeggings"; item = "Arcane Leggings"; }
			else if (artifact == arty++) { name = "Artifact_ArcaneShield"; item = "Arcane Shield"; }
			else if (artifact == arty++) { name = "Artifact_ArcaneTunic"; item = "Arcane Tunic"; }
			else if (artifact == arty++) { name = "Artifact_ArcanicRobe"; item = "Arcanic Robe"; }
			else if (artifact == arty++) { name = "Artifact_ArcticDeathDealer"; item = "Arctic Death Dealer"; }
			else if (artifact == arty++) { name = "Artifact_ArmorOfFortune"; item = "Armor of Fortune"; }
			else if (artifact == arty++) { name = "Artifact_ArmorOfInsight"; item = "Armor of Insight"; }
			else if (artifact == arty++) { name = "Artifact_ArmorOfNobility"; item = "Armor of Nobility"; }
			else if (artifact == arty++) { name = "Artifact_ArmsOfAegis"; item = "Arms of Aegis"; }
			else if (artifact == arty++) { name = "Artifact_ArmsOfFortune"; item = "Arms of Fortune"; }
			else if (artifact == arty++) { name = "Artifact_ArmsOfInsight"; item = "Arms of Insight"; }
			else if (artifact == arty++) { name = "Artifact_ArmsOfNobility"; item = "Arms of Nobility"; }
			else if (artifact == arty++) { name = "Artifact_ArmsOfTheFallenKing"; item = "Arms of the Fallen King"; }
			else if (artifact == arty++) { name = "Artifact_ArmsOfTheHarrower"; item = "Arms of the Harrower"; }
			else if (artifact == arty++) { name = "Artifact_ArmsOfToxicity"; item = "Arms Of Toxicity"; }
			else if (artifact == arty++) { name = "Artifact_AuraOfShadows"; item = "Aura Of Shadows"; }
			else if (artifact == arty++) { name = "Artifact_AxeOfTheHeavens"; item = "Axe of the Heavens"; }
			else if (artifact == arty++) { name = "Artifact_AxeoftheMinotaur"; item = "Axe of the Minotaur"; }
			else if (artifact == arty++) { name = "Artifact_BeggarsRobe"; item = "Beggar's Robe"; }
			else if (artifact == arty++) { name = "Artifact_BeltofHercules"; item = "Belt of Hercules"; }
			else if (artifact == arty++) { name = "Artifact_TheBeserkersMaul"; item = "Berserker's Maul"; }
			else if (artifact == arty++) { name = "Artifact_BladeDance"; item = "Blade Dance"; }
			else if (artifact == arty++) { name = "Artifact_BladeOfInsanity"; item = "Blade of Insanity"; }
			else if (artifact == arty++) { name = "Artifact_ConansSword"; item = "Blade of the Cimmerian"; }
			else if (artifact == arty++) { name = "Artifact_BladeOfTheRighteous"; item = "Blade of the Righteous"; }
			else if (artifact == arty++) { name = "Artifact_ShadowBlade"; item = "Blade of the Shadows"; }
			else if (artifact == arty++) { name = "Artifact_BlazeOfDeath"; item = "Blaze of Death"; }
			else if (artifact == arty++) { name = "Artifact_BlightGrippedLongbow"; item = "Blight Gripped Longbow"; }
			else if (artifact == arty++) { name = "Artifact_BloodwoodSpirit"; item = "Bloodwood Spirit"; }
			else if (artifact == arty++) { name = "Artifact_BoneCrusher"; item = "Bone Crusher"; }
			else if (artifact == arty++) { name = "Artifact_Bonesmasher"; item = "Bonesmasher"; }
			else if (artifact == arty++) { name = "Artifact_BookOfKnowledge"; item = "Book Of Knowledge"; }
			else if (artifact == arty++) { name = "Artifact_Boomstick"; item = "Boomstick"; }
			else if (artifact == arty++) { name = "Artifact_BootsofHermes"; item = "Boots of Hermes"; }
			else if (artifact == arty++) { name = "Artifact_BootsofPyros"; item = "Boots of the Daemon King"; }
			else if (artifact == arty++) { name = "Artifact_BootsofHydros"; item = "Boots of the Lurker"; }
			else if (artifact == arty++) { name = "Artifact_BootsofLithos"; item = "Boots of the Mountain King"; }
			else if (artifact == arty++) { name = "Artifact_BootsofStratos"; item = "Boots of the Mystic Voice"; }
			else if (artifact == arty++) { name = "Artifact_BowOfTheJukaKing"; item = "Bow of the Juka King"; }
			else if (artifact == arty++) { name = "Artifact_BowofthePhoenix"; item = "Bow of the Phoenix"; }
			else if (artifact == arty++) { name = "Artifact_BraceletOfHealth"; item = "Bracelet of Health"; }
			else if (artifact == arty++) { name = "Artifact_BraceletOfTheElements"; item = "Bracelet of the Elements"; }
			else if (artifact == arty++) { name = "Artifact_BraceletOfTheVile"; item = "Bracelet of the Vile"; }
			else if (artifact == arty++) { name = "Artifact_BrambleCoat"; item = "Bramble Coat"; }
			else if (artifact == arty++) { name = "Artifact_BraveKnightOfTheBritannia"; item = "Brave Knight of Sosaria"; }
			else if (artifact == arty++) { name = "Artifact_BreathOfTheDead"; item = "Breath of the Dead"; }
			else if (artifact == arty++) { name = "Artifact_BurglarsBandana"; item = "Burglar's Bandana"; }
			else if (artifact == arty++) { name = "Artifact_Calm"; item = "Calm"; }
			else if (artifact == arty++) { name = "Artifact_CandleCold"; item = "Candle of Cold Light"; }
			else if (artifact == arty++) { name = "Artifact_CandleEnergy"; item = "Candle of Energized Light"; }
			else if (artifact == arty++) { name = "Artifact_CandleFire"; item = "Candle of Fire Light"; }
			else if (artifact == arty++) { name = "Artifact_CandleNecromancer"; item = "Candle of Ghostly Light"; }
			else if (artifact == arty++) { name = "Artifact_CandlePoison"; item = "Candle of Poisonous Light"; }
			else if (artifact == arty++) { name = "Artifact_CandleWizard"; item = "Candle of Wizardly Light"; }
			else if (artifact == arty++) { name = "Artifact_CapOfFortune"; item = "Cap of Fortune"; }
			else if (artifact == arty++) { name = "Artifact_CapOfTheFallenKing"; item = "Cap of the Fallen King"; }
			else if (artifact == arty++) { name = "Artifact_CaptainJohnsHat"; item = "Captain John's Hat"; }
			else if (artifact == arty++) { name = "Artifact_CaptainQuacklebushsCutlass"; item = "Captain Quacklebush's Cutlass"; }
			else if (artifact == arty++) { name = "Artifact_CavortingClub"; item = "Cavorting Club"; }
			else if (artifact == arty++) { name = "Artifact_CircletOfTheSorceress"; item = "Circlet Of The Sorceress"; }
			else if (artifact == arty++) { name = "Artifact_GrayMouserCloak"; item = "Cloak of the Rogue"; }
			else if (artifact == arty++) { name = "Artifact_CoifOfBane"; item = "Coif of Bane"; }
			else if (artifact == arty++) { name = "Artifact_CoifOfFire"; item = "Coif of Fire"; }
			else if (artifact == arty++) { name = "Artifact_ColdBlood"; item = "Cold Blood"; }
			else if (artifact == arty++) { name = "Artifact_ColdForgedBlade"; item = "Cold Forged Blade"; }
			else if (artifact == arty++) { name = "Artifact_CrimsonCincture"; item = "Crimson Cincture"; }
			else if (artifact == arty++) { name = "Artifact_CrownOfTalKeesh"; item = "Crown of Tal'Keesh"; }
			else if (artifact == arty++) { name = "Artifact_DaggerOfVenom"; item = "Dagger of Venom"; }
			else if (artifact == arty++) { name = "Artifact_DarkGuardiansChest"; item = "Dark Guardian's Chest"; }
			else if (artifact == arty++) { name = "Artifact_DarkLordsPitchfork"; item = "Dark Lord's PitchFork"; }
			else if (artifact == arty++) { name = "Artifact_DarkNeck"; item = "Dark Neck"; }
			else if (artifact == arty++) { name = "Artifact_DetectiveBoots"; item = "Detective Boots of the Royal Guard"; }
			else if (artifact == arty++) { name = "Artifact_DivineArms"; item = "Divine Arms"; }
			else if (artifact == arty++) { name = "Artifact_DivineCountenance"; item = "Divine Countenance"; }
			else if (artifact == arty++) { name = "Artifact_DivineGloves"; item = "Divine Gloves"; }
			else if (artifact == arty++) { name = "Artifact_DivineGorget"; item = "Divine Gorget"; }
			else if (artifact == arty++) { name = "Artifact_DivineLeggings"; item = "Divine Leggings"; }
			else if (artifact == arty++) { name = "Artifact_DivineTunic"; item = "Divine Tunic"; }
			else if (artifact == arty++) { name = "Artifact_DjinnisRing"; item = "Djinni's Ring"; }
			else if (artifact == arty++) { name = "Artifact_DreadPirateHat"; item = "Dread Pirate Hat"; }
			else if (artifact == arty++) { name = "Artifact_TheDryadBow"; item = "Dryad Bow"; }
			else if (artifact == arty++) { name = "Artifact_DupresCollar"; item = "Dupre's Collar"; }
			else if (artifact == arty++) { name = "Artifact_DupresShield"; item = "Dupre's Shield"; }
			else if (artifact == arty++) { name = "Artifact_EarringsOfHealth"; item = "Earrings of Health"; }
			else if (artifact == arty++) { name = "Artifact_EarringsOfTheElements"; item = "Earrings of the Elements"; }
			else if (artifact == arty++) { name = "Artifact_EarringsOfTheMagician"; item = "Earrings of the Magician"; }
			else if (artifact == arty++) { name = "Artifact_EarringsOfTheVile"; item = "Earrings of the Vile"; }
			else if (artifact == arty++) { name = "Artifact_EmbroideredOakLeafCloak"; item = "Embroidered Oak Leaf Cloak"; }
			else if (artifact == arty++) { name = "Artifact_EnchantedTitanLegBone"; item = "Enchanted Pirate Rapier"; }
			else if (artifact == arty++) { name = "Artifact_EssenceOfBattle"; item = "Essence of Battle"; }
			else if (artifact == arty++) { name = "Artifact_EternalFlame"; item = "Eternal Flame"; }
			else if (artifact == arty++) { name = "Artifact_EvilMageGloves"; item = "Evil Mage Gloves"; }
			else if (artifact == arty++) { name = "Artifact_Excalibur"; item = "Excalibur"; }
			else if (artifact == arty++) { name = "Artifact_FangOfRactus"; item = "Fang of Ractus"; }
			else if (artifact == arty++) { name = "Artifact_FesteringWound"; item = "Festering Wound"; }
			else if (artifact == arty++) { name = "Artifact_FeyLeggings"; item = "Fey Leggings"; }
			else if (artifact == arty++) { name = "Artifact_FleshRipper"; item = "Flesh Ripper"; }
			else if (artifact == arty++) { name = "Artifact_Fortifiedarms"; item = "Fortified Arms"; }
			else if (artifact == arty++) { name = "Artifact_FortunateBlades"; item = "Fortunate Blades"; }
			else if (artifact == arty++) { name = "Artifact_Frostbringer"; item = "Frostbringer"; }
			else if (artifact == arty++) { name = "Artifact_FurCapeOfTheSorceress"; item = "Fur Cape Of The Sorceress"; }
			else if (artifact == arty++) { name = "Artifact_Fury"; item = "Fury"; }
			else if (artifact == arty++) { name = "Artifact_MarbleShield"; item = "Gargoyle Shield"; }
			else if (artifact == arty++) { name = "Artifact_GuantletsOfAnger"; item = "Gauntlets of Anger"; }
			else if (artifact == arty++) { name = "Artifact_GauntletsOfNobility"; item = "Gauntlets of Nobility"; }
			else if (artifact == arty++) { name = "Artifact_GeishasObi"; item = "Geishas Obi"; }
			else if (artifact == arty++) { name = "Artifact_GiantBlackjack"; item = "Giant Blackjack"; }
			else if (artifact == arty++) { name = "Artifact_GladiatorsCollar"; item = "Gladiator's Collar"; }
			else if (artifact == arty++) { name = "Artifact_GlovesOfAegis"; item = "Gloves of Aegis"; }
			else if (artifact == arty++) { name = "Artifact_GlovesOfCorruption"; item = "Gloves Of Corruption"; }
			else if (artifact == arty++) { name = "Artifact_GlovesOfDexterity"; item = "Gloves of Dexterity"; }
			else if (artifact == arty++) { name = "Artifact_GlovesOfFortune"; item = "Gloves of Fortune"; }
			else if (artifact == arty++) { name = "Artifact_GlovesOfInsight"; item = "Gloves of Insight"; }
			else if (artifact == arty++) { name = "Artifact_GlovesOfRegeneration"; item = "Gloves Of Regeneration"; }
			else if (artifact == arty++) { name = "Artifact_GlovesOfTheFallenKing"; item = "Gloves of the Fallen King"; }
			else if (artifact == arty++) { name = "Artifact_GlovesOfTheHarrower"; item = "Gloves of the Harrower"; }
			else if (artifact == arty++) { name = "Artifact_GlovesOfThePugilist"; item = "Gloves of the Pugilist"; }
			else if (artifact == arty++) { name = "Artifact_SamaritanRobe"; item = "Good Samaritan Robe"; }
			else if (artifact == arty++) { name = "Artifact_GorgetOfAegis"; item = "Gorget of Aegis"; }
			else if (artifact == arty++) { name = "Artifact_GorgetOfFortune"; item = "Gorget of Fortune"; }
			else if (artifact == arty++) { name = "Artifact_GorgetOfInsight"; item = "Gorget of Insight"; }
			else if (artifact == arty++) { name = "Artifact_GrimReapersLantern"; item = "Grim Reaper's Lantern"; }
			else if (artifact == arty++) { name = "Artifact_GrimReapersMask"; item = "Grim Reaper's Mask"; }
			else if (artifact == arty++) { name = "Artifact_GrimReapersRobe"; item = "Grim Reaper's Robe"; }
			else if (artifact == arty++) { name = "Artifact_GrimReapersScythe"; item = "Grim Reaper's Scythe"; }
			else if (artifact == arty++) { name = "Artifact_PyrosGrimoire"; item = "Grimoire of the Daemon King"; }
			else if (artifact == arty++) { name = "Artifact_TownGuardsHalberd"; item = "Guardsman Halberd"; }
			else if (artifact == arty++) { name = "Artifact_GwennosHarp"; item = "Gwenno's Harp"; }
			else if (artifact == arty++) { name = "Artifact_HammerofThor"; item = "Hammer of Thor"; }
			else if (artifact == arty++) { name = "Artifact_HatOfTheMagi"; item = "Hat of the Magi"; }
			else if (artifact == arty++) { name = "Artifact_HeartOfTheLion"; item = "Heart of the Lion"; }
			else if (artifact == arty++) { name = "Artifact_HellForgedArms"; item = "Hell Forged Arms"; }
			else if (artifact == arty++) { name = "Artifact_HelmOfAegis"; item = "Helm of Aegis"; }
			else if (artifact == arty++) { name = "Artifact_HelmOfBrilliance"; item = "Helm of Brilliance"; }
			else if (artifact == arty++) { name = "Artifact_HelmOfInsight"; item = "Helm of Insight"; }
			else if (artifact == arty++) { name = "Artifact_HelmOfSwiftness"; item = "Helm of Swiftness"; }
			else if (artifact == arty++) { name = "Artifact_ConansHelm"; item = "Helm of the Cimmerian"; }
			else if (artifact == arty++) { name = "Artifact_HolyKnightsArmPlates"; item = "Holy Knight's Arm Plates"; }
			else if (artifact == arty++) { name = "Artifact_HolyKnightsBreastplate"; item = "Holy Knight's Breastplate"; }
			else if (artifact == arty++) { name = "Artifact_HolyKnightsGloves"; item = "Holy Knight's Gloves"; }
			else if (artifact == arty++) { name = "Artifact_HolyKnightsGorget"; item = "Holy Knight's Gorget"; }
			else if (artifact == arty++) { name = "Artifact_HolyKnightsLegging"; item = "Holy Knight's Legging"; }
			else if (artifact == arty++) { name = "Artifact_HolyKnightsPlateHelm"; item = "Holy Knight's Plate Helm"; }
			else if (artifact == arty++) { name = "Artifact_LunaLance"; item = "Holy Lance"; }
			else if (artifact == arty++) { name = "Artifact_HolySword"; item = "Holy Sword"; }
			else if (artifact == arty++) { name = "Artifact_HoodedShroudOfShadows"; item = "Hooded Shroud of Shadows"; }
			else if (artifact == arty++) { name = "Artifact_HornOfKingTriton"; item = "Horn of King Triton"; }
			else if (artifact == arty++) { name = "Artifact_HuntersArms"; item = "Hunter's Arms"; }
			else if (artifact == arty++) { name = "Artifact_HuntersGloves"; item = "Hunter's Gloves"; }
			else if (artifact == arty++) { name = "Artifact_HuntersGorget"; item = "Hunter's Gorget"; }
			else if (artifact == arty++) { name = "Artifact_HuntersHeaddress"; item = "Hunter's Headdress"; }
			else if (artifact == arty++) { name = "Artifact_HuntersLeggings"; item = "Hunter's Leggings"; }
			else if (artifact == arty++) { name = "Artifact_HuntersTunic"; item = "Hunter's Tunic"; }
			else if (artifact == arty++) { name = "Artifact_Indecency"; item = "Indecency"; }
			else if (artifact == arty++) { name = "Artifact_InquisitorsArms"; item = "Inquisitor's Arms"; }
			else if (artifact == arty++) { name = "Artifact_InquisitorsGorget"; item = "Inquisitor's Gorget"; }
			else if (artifact == arty++) { name = "Artifact_InquisitorsHelm"; item = "Inquisitor's Helm"; }
			else if (artifact == arty++) { name = "Artifact_InquisitorsLeggings"; item = "Inquisitor's Leggings"; }
			else if (artifact == arty++) { name = "Artifact_InquisitorsResolution"; item = "Inquisitor's Resolution"; }
			else if (artifact == arty++) { name = "Artifact_InquisitorsTunic"; item = "Inquisitor's Tunic"; }
			else if (artifact == arty++) { name = "Artifact_IolosLute"; item = "Iolo's Lute"; }
			else if (artifact == arty++) { name = "Artifact_IronwoodCrown"; item = "Ironwood Crown"; }
			else if (artifact == arty++) { name = "Artifact_JackalsArms"; item = "Jackal's Arms"; }
			else if (artifact == arty++) { name = "Artifact_JackalsCollar"; item = "Jackal's Collar"; }
			else if (artifact == arty++) { name = "Artifact_JackalsGloves"; item = "Jackal's Gloves"; }
			else if (artifact == arty++) { name = "Artifact_JackalsHelm"; item = "Jackal's Helm"; }
			else if (artifact == arty++) { name = "Artifact_JackalsLeggings"; item = "Jackal's Leggings"; }
			else if (artifact == arty++) { name = "Artifact_JackalsTunic"; item = "Jackal's Tunic"; }
			else if (artifact == arty++) { name = "Artifact_JadeScimitar"; item = "Jade Scimitar"; }
			else if (artifact == arty++) { name = "Artifact_JesterHatofChuckles"; item = "Jester Hat of Chuckles"; }
			else if (artifact == arty++) { name = "Artifact_JinBaoriOfGoodFortune"; item = "Jin-Baori Of Good Fortune"; }
			else if (artifact == arty++) { name = "Artifact_KamiNarisIndestructableDoubleAxe"; item = "Kami-Naris Indestructable Axe"; }
			else if (artifact == arty++) { name = "Artifact_KodiakBearMask"; item = "Kodiak Bear Mask"; }
			else if (artifact == arty++) { name = "Artifact_PowerSurge"; item = "Lantern of Power"; }
			else if (artifact == arty++) { name = "Artifact_LegacyOfTheDreadLord"; item = "Legacy of the Dread Lord"; }
			else if (artifact == arty++) { name = "Artifact_LegsOfFortune"; item = "Legging of Fortune"; }
			else if (artifact == arty++) { name = "Artifact_LegsOfInsight"; item = "Legging of Insight"; }
			else if (artifact == arty++) { name = "Artifact_LeggingsOfAegis"; item = "Leggings of Aegis"; }
			else if (artifact == arty++) { name = "Artifact_LeggingsOfBane"; item = "Leggings of Bane"; }
			else if (artifact == arty++) { name = "Artifact_LeggingsOfDeceit"; item = "Leggings Of Deceit"; }
			else if (artifact == arty++) { name = "Artifact_LeggingsOfEnlightenment"; item = "Leggings Of Enlightenment"; }
			else if (artifact == arty++) { name = "Artifact_LeggingsOfFire"; item = "Leggings of Fire"; }
			else if (artifact == arty++) { name = "Artifact_LegsOfTheFallenKing"; item = "Leggings of the Fallen King"; }
			else if (artifact == arty++) { name = "Artifact_LegsOfTheHarrower"; item = "Leggings of the Harrower"; }
			else if (artifact == arty++) { name = "Artifact_LegsOfNobility"; item = "Legs of Nobility"; }
			else if (artifact == arty++) { name = "Artifact_HydrosLexicon"; item = "Lexicon of the Lurker"; }
			else if (artifact == arty++) { name = "Artifact_ConansLoinCloth"; item = "Loin Cloth of the Cimmerian"; }
			else if (artifact == arty++) { name = "Artifact_LongShot"; item = "Long Shot"; }
			else if (artifact == arty++) { name = "Artifact_LuckyEarrings"; item = "Lucky Earrings"; }
			else if (artifact == arty++) { name = "Artifact_LuckyNecklace"; item = "Lucky Necklace"; }
			else if (artifact == arty++) { name = "Artifact_LuminousRuneBlade"; item = "Luminous Rune Blade"; }
			else if (artifact == arty++) { name = "Artifact_MadmansHatchet"; item = "Madman's Hatchet"; }
			else if (artifact == arty++) { name = "Artifact_MagesBand"; item = "Mage's Band"; }
			else if (artifact == arty++) { name = "Artifact_MagiciansIllusion"; item = "Magician's Illusion"; }
			else if (artifact == arty++) { name = "Artifact_MagiciansMempo"; item = "Magician's Mempo"; }
			else if (artifact == arty++) { name = "Artifact_MantleofPyros"; item = "Mantle of the Daemon King"; }
			else if (artifact == arty++) { name = "Artifact_MantleofHydros"; item = "Mantle of the Lurker"; }
			else if (artifact == arty++) { name = "Artifact_MantleofLithos"; item = "Mantle of the Mountain King"; }
			else if (artifact == arty++) { name = "Artifact_MantleofStratos"; item = "Mantle of the Mystic Voice"; }
			else if (artifact == arty++) { name = "Artifact_StratosManual"; item = "Manual of the Mystic Voice"; }
			else if (artifact == arty++) { name = "Artifact_DeathsMask"; item = "Mask of Death"; }
			else if (artifact == arty++) { name = "Artifact_MauloftheBeast"; item = "Maul of the Beast"; }
			else if (artifact == arty++) { name = "Artifact_MaulOfTheTitans"; item = "Maul of the Titans"; }
			else if (artifact == arty++) { name = "Artifact_MelisandesCorrodedHatchet"; item = "Melisande's Corroded Hatchet"; }
			else if (artifact == arty++) { name = "Artifact_GandalfsHat"; item = "Merlin's Mystical Hat"; }
			else if (artifact == arty++) { name = "Artifact_GandalfsRobe"; item = "Merlin's Mystical Robe"; }
			else if (artifact == arty++) { name = "Artifact_GandalfsStaff"; item = "Merlin's Mystical Staff"; }
			else if (artifact == arty++) { name = "Artifact_MidnightBracers"; item = "Midnight Bracers"; }
			else if (artifact == arty++) { name = "Artifact_MidnightGloves"; item = "Midnight Gloves"; }
			else if (artifact == arty++) { name = "Artifact_MidnightHelm"; item = "Midnight Helm"; }
			else if (artifact == arty++) { name = "Artifact_MidnightLegs"; item = "Midnight Leggings"; }
			else if (artifact == arty++) { name = "Artifact_MidnightTunic"; item = "Midnight Tunic"; }
			else if (artifact == arty++) { name = "Artifact_MinersPickaxe"; item = "Miner's Pickaxe"; }
			else if (artifact == arty++) { name = "Artifact_ANecromancerShroud"; item = "Necromancer Shroud"; }
			else if (artifact == arty++) { name = "Artifact_TheNightReaper"; item = "Night Reaper"; }
			else if (artifact == arty++) { name = "Artifact_NightsKiss"; item = "Night's Kiss"; }
			else if (artifact == arty++) { name = "Artifact_NordicVikingSword"; item = "Nordic Dragon Blade"; }
			else if (artifact == arty++) { name = "Artifact_VampiresRobe"; item = "Nosferatu's Robe"; }
			else if (artifact == arty++) { name = "Artifact_NoxBow"; item = "Nox Bow"; }
			else if (artifact == arty++) { name = "Artifact_NoxNightlight"; item = "Nox Nightlight"; }
			else if (artifact == arty++) { name = "Artifact_NoxRangersHeavyCrossbow"; item = "Nox Ranger's Heavy Crossbow"; }
			else if (artifact == arty++) { name = "Artifact_OblivionsNeedle"; item = "Oblivion Needle"; }
			else if (artifact == arty++) { name = "Artifact_OrcChieftainHelm"; item = "Orc Chieftain Helm"; }
			else if (artifact == arty++) { name = "Artifact_OrcishVisage"; item = "Orcish Visage"; }
			else if (artifact == arty++) { name = "Artifact_OrnamentOfTheMagician"; item = "Ornament of the Magician"; }
			else if (artifact == arty++) { name = "Artifact_OrnateCrownOfTheHarrower"; item = "Ornate Crown of the Harrower"; }
			else if (artifact == arty++) { name = "Artifact_OssianGrimoire"; item = "Ossian Grimoire"; }
			else if (artifact == arty++) { name = "Artifact_OverseerSunderedBlade"; item = "Overseer Sundered Blade"; }
			else if (artifact == arty++) { name = "Artifact_Pacify"; item = "Pacify"; }
			else if (artifact == arty++) { name = "Artifact_PadsOfTheCuSidhe"; item = "Pads of the Cu Sidhe"; }
			else if (artifact == arty++) { name = "Artifact_PendantOfTheMagi"; item = "Pendant of the Magi"; }
			else if (artifact == arty++) { name = "Artifact_Pestilence"; item = "Pestilence"; }
			else if (artifact == arty++) { name = "Artifact_PhantomStaff"; item = "Phantom Staff"; }
			else if (artifact == arty++) { name = "Artifact_PixieSwatter"; item = "Pixie Swatter"; }
			else if (artifact == arty++) { name = "Artifact_PolarBearBoots"; item = "Polar Bear Boots"; }
			else if (artifact == arty++) { name = "Artifact_PolarBearCape"; item = "Polar Bear Cape"; }
			else if (artifact == arty++) { name = "Artifact_Quell"; item = "Quell"; }
			else if (artifact == arty++) { name = "Artifact_QuiverOfBlight"; item = "Quiver of Blight"; }
			else if (artifact == arty++) { name = "Artifact_QuiverOfFire"; item = "Quiver of Fire"; }
			else if (artifact == arty++) { name = "Artifact_QuiverOfIce"; item = "Quiver of Ice"; }
			else if (artifact == arty++) { name = "Artifact_QuiverOfInfinity"; item = "Quiver of Infinity"; }
			else if (artifact == arty++) { name = "Artifact_QuiverOfLightning"; item = "Quiver of Lightning"; }
			else if (artifact == arty++) { name = "Artifact_QuiverOfRage"; item = "Quiver of Rage"; }
			else if (artifact == arty++) { name = "Artifact_QuiverOfElements"; item = "Quiver of the Elements"; }
			else if (artifact == arty++) { name = "Artifact_RaedsGlory"; item = "Raed's Glory"; }
			else if (artifact == arty++) { name = "Artifact_RamusNecromanticScalpel"; item = "Ramus' Necromantic Scalpel"; }
			else if (artifact == arty++) { name = "Artifact_ResilientBracer"; item = "Resillient Bracer"; }
			else if (artifact == arty++) { name = "Artifact_Retort"; item = "Retort"; }
			else if (artifact == arty++) { name = "Artifact_RighteousAnger"; item = "Righteous Anger"; }
			else if (artifact == arty++) { name = "Artifact_RingOfHealth"; item = "Ring of Health"; }
			else if (artifact == arty++) { name = "Artifact_RingOfProtection"; item = "Ring of Protection"; }
			else if (artifact == arty++) { name = "Artifact_RingOfTheElements"; item = "Ring of the Elements"; }
			else if (artifact == arty++) { name = "Artifact_RingOfTheMagician"; item = "Ring of the Magician"; }
			else if (artifact == arty++) { name = "Artifact_RingOfTheVile"; item = "Ring of the Vile"; }
			else if (artifact == arty++) { name = "Artifact_TheRobeOfBritanniaAri"; item = "Robe of Sosaria"; }
			else if (artifact == arty++) { name = "Artifact_RobeOfTeleportation"; item = "Robe Of Teleportation"; }
			else if (artifact == arty++) { name = "Artifact_RobeofPyros"; item = "Robe of the Daemon King"; }
			else if (artifact == arty++) { name = "Artifact_RobeOfTheEclipse"; item = "Robe of the Eclipse"; }
			else if (artifact == arty++) { name = "Artifact_RobeOfTheEquinox"; item = "Robe of the Equinox"; }
			else if (artifact == arty++) { name = "Artifact_RobeofHydros"; item = "Robe of the Lurker"; }
			else if (artifact == arty++) { name = "Artifact_RobeofLithos"; item = "Robe of the Mountain King"; }
			else if (artifact == arty++) { name = "Artifact_RobeofStratos"; item = "Robe of the Mystic Voice"; }
			else if (artifact == arty++) { name = "Artifact_RobeOfTreason"; item = "Robe Of Treason"; }
			else if (artifact == arty++) { name = "Artifact_RobinHoodsBow"; item = "Robin Hood's Bow"; }
			else if (artifact == arty++) { name = "Artifact_RobinHoodsFeatheredHat"; item = "Robin Hood's Feathered Hat"; }
			else if (artifact == arty++) { name = "Artifact_RodOfResurrection"; item = "Rod Of Resurrection"; }
			else if (artifact == arty++) { name = "Artifact_RoyalArchersBow"; item = "Royal Archer's Bow"; }
			else if (artifact == arty++) { name = "Artifact_LieutenantOfTheBritannianRoyalGuard"; item = "Royal Guard Sash"; }
			else if (artifact == arty++) { name = "Artifact_RoyalGuardSurvivalKnife"; item = "Royal Guard Survival Knife"; }
			else if (artifact == arty++) { name = "Artifact_RoyalGuardsGorget"; item = "Royal Guardian's Gorget"; }
			else if (artifact == arty++) { name = "Artifact_RoyalGuardsChestplate"; item = "Royal Guard's Chest Plate"; }
			else if (artifact == arty++) { name = "Artifact_LeggingsOfEmbers"; item = "Royal Leggings of Embers"; }
			else if (artifact == arty++) { name = "Artifact_RuneCarvingKnife"; item = "Rune Carving Knife"; }
			else if (artifact == arty++) { name = "Artifact_FalseGodsScepter"; item = "Scepter Of The False Goddess"; }
			else if (artifact == arty++) { name = "Artifact_SerpentsFang"; item = "Serpent's Fang"; }
			else if (artifact == arty++) { name = "Artifact_ShadowDancerArms"; item = "Shadow Dancer Arms"; }
			else if (artifact == arty++) { name = "Artifact_ShadowDancerCap"; item = "Shadow Dancer Cap"; }
			else if (artifact == arty++) { name = "Artifact_ShadowDancerGloves"; item = "Shadow Dancer Gloves"; }
			else if (artifact == arty++) { name = "Artifact_ShadowDancerGorget"; item = "Shadow Dancer Gorget"; }
			else if (artifact == arty++) { name = "Artifact_ShadowDancerLeggings"; item = "Shadow Dancer Leggings"; }
			else if (artifact == arty++) { name = "Artifact_ShadowDancerTunic"; item = "Shadow Dancer Tunic"; }
			else if (artifact == arty++) { name = "Artifact_ShaMontorrossbow"; item = "Shamino's Crossbow"; }
			else if (artifact == arty++) { name = "Artifact_ShardThrasher"; item = "Shard Thrasher"; }
			else if (artifact == arty++) { name = "Artifact_ShieldOfInvulnerability"; item = "Shield of Invulnerability"; }
			else if (artifact == arty++) { name = "Artifact_ShimmeringTalisman"; item = "Shimmering Talisman"; }
			else if (artifact == arty++) { name = "Artifact_ShroudOfDeciet"; item = "Shroud of Deceit"; }
			else if (artifact == arty++) { name = "Artifact_SilvanisFeywoodBow"; item = "Silvani's Feywood Bow"; }
			else if (artifact == arty++) { name = "Artifact_TheDragonSlayer"; item = "Slayer of Dragons"; }
			else if (artifact == arty++) { name = "Artifact_SongWovenMantle"; item = "Song Woven Mantle"; }
			else if (artifact == arty++) { name = "Artifact_SoulSeeker"; item = "Soul Seeker"; }
			else if (artifact == arty++) { name = "Artifact_SpellWovenBritches"; item = "Spell Woven Britches"; }
			else if (artifact == arty++) { name = "Artifact_PolarBearMask"; item = "Spirit of the Polar Bear"; }
			else if (artifact == arty++) { name = "Artifact_SpiritOfTheTotem"; item = "Spirit of the Totem"; }
			else if (artifact == arty++) { name = "Artifact_SprintersSandals"; item = "Sprinter's Sandals"; }
			else if (artifact == arty++) { name = "Artifact_StaffOfPower"; item = "Staff of Power"; }
			else if (artifact == arty++) { name = "Artifact_StaffOfTheMagi"; item = "Staff of the Magi"; }
			else if (artifact == arty++) { name = "Artifact_StaffofSnakes"; item = "Staff of the Serpent"; }
			else if (artifact == arty++) { name = "Artifact_StitchersMittens"; item = "Stitcher's Mittens"; }
			else if (artifact == arty++) { name = "Artifact_Stormbringer"; item = "Stormbringer"; }
			else if (artifact == arty++) { name = "Artifact_Subdue"; item = "Subdue"; }
			else if (artifact == arty++) { name = "Artifact_SwiftStrike"; item = "Swift Strike"; }
			else if (artifact == arty++) { name = "Artifact_GlassSword"; item = "Sword of Shattered Hopes"; }
			else if (artifact == arty++) { name = "Artifact_SinbadsSword"; item = "Sword of Sinbad"; }
			else if (artifact == arty++) { name = "Artifact_TalonBite"; item = "Talon Bite"; }
			else if (artifact == arty++) { name = "Artifact_TheTaskmaster"; item = "Taskmaster"; }
			else if (artifact == arty++) { name = "Artifact_TitansHammer"; item = "Titan's Hammer"; }
			else if (artifact == arty++) { name = "Artifact_LithosTome"; item = "Tome of the Mountain King"; }
			else if (artifact == arty++) { name = "Artifact_TorchOfTrapFinding"; item = "Torch of Trap Burning"; }
			else if (artifact == arty++) { name = "Artifact_TotemArms"; item = "Totem Arms"; }
			else if (artifact == arty++) { name = "Artifact_TotemGloves"; item = "Totem Gloves"; }
			else if (artifact == arty++) { name = "Artifact_TotemGorget"; item = "Totem Gorget"; }
			else if (artifact == arty++) { name = "Artifact_TotemLeggings"; item = "Totem Leggings"; }
			else if (artifact == arty++) { name = "Artifact_TotemOfVoid"; item = "Totem of the Void"; }
			else if (artifact == arty++) { name = "Artifact_TotemTunic"; item = "Totem Tunic"; }
			else if (artifact == arty++) { name = "Artifact_TunicOfAegis"; item = "Tunic of Aegis"; }
			else if (artifact == arty++) { name = "Artifact_TunicOfBane"; item = "Tunic of Bane"; }
			else if (artifact == arty++) { name = "Artifact_TunicOfFire"; item = "Tunic of Fire"; }
			else if (artifact == arty++) { name = "Artifact_TunicOfTheFallenKing"; item = "Tunic of the Fallen King"; }
			else if (artifact == arty++) { name = "Artifact_TunicOfTheHarrower"; item = "Tunic of the Harrower"; }
			else if (artifact == arty++) { name = "Artifact_BelmontWhip"; item = "Vampire Killer"; }
			else if (artifact == arty++) { name = "Artifact_VampiricDaisho"; item = "Vampiric Daisho"; }
			else if (artifact == arty++) { name = "Artifact_VioletCourage"; item = "Violet Courage"; }
			else if (artifact == arty++) { name = "Artifact_VoiceOfTheFallenKing"; item = "Voice of the Fallen King"; }
			else if (artifact == arty++) { name = "Artifact_WarriorsClasp"; item = "Warrior's Clasp"; }
			else if (artifact == arty++) { name = "Artifact_WildfireBow"; item = "Wildfire Bow"; }
			else if (artifact == arty++) { name = "Artifact_Windsong"; item = "Windsong"; }
			else if (artifact == arty++) { name = "Artifact_ArcticBeacon"; item = "Winter Beacon"; }
			else if (artifact == arty++) { name = "Artifact_WizardsPants"; item = "Wizard's Pants"; }
			else if (artifact == arty++) { name = "Artifact_WrathOfTheDryad"; item = "Wrath of the Dryad"; }
			else if (artifact == arty++) { name = "Artifact_YashimotosHatsuburi"; item = "Yashimoto's Hatsuburi"; }
			else if (artifact == arty++) { name = "Artifact_ZyronicClaw"; item = "Zyronic Claw"; }

			if (part == 2) { item = name; }

			return item;
		}
	}
}
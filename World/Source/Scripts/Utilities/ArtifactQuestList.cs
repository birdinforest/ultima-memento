using Server.Items;

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

			if (artifact == arty++) 	 { name = typeof(Artifact_AbysmalGloves).Name; item = "Abysmal Gloves"; }
			else if (artifact == arty++) { name = typeof(Artifact_AchillesShield).Name; item = "Achille's Shield"; }
			else if (artifact == arty++) { name = typeof(Artifact_AchillesSpear).Name; item = "Achille's Spear"; }
			else if (artifact == arty++) { name = typeof(Artifact_AcidProofRobe).Name; item = "Acidic Robe"; }
			else if (artifact == arty++) { name = typeof(Artifact_Aegis).Name; item = "Aegis"; }
			else if (artifact == arty++) { name = typeof(Artifact_AegisOfGrace).Name; item = "Aegis of Grace"; }
			else if (artifact == arty++) { name = typeof(Artifact_AilricsLongbow).Name; item = "Ailric's Longbow"; }
			else if (artifact == arty++) { name = typeof(Artifact_AlchemistsBauble).Name; item = "Alchemist's Bauble"; }
			else if (artifact == arty++) { name = typeof(Artifact_SamuraiHelm).Name; item = "Ancient Samurai Helm"; }
			else if (artifact == arty++) { name = typeof(Artifact_AngelicEmbrace).Name; item = "Angelic Embrace"; }
			else if (artifact == arty++) { name = typeof(Artifact_AngeroftheGods).Name; item = "Anger of the Gods"; }
			else if (artifact == arty++) { name = typeof(Artifact_Annihilation).Name; item = "Annihilation"; }
			else if (artifact == arty++) { name = typeof(Artifact_ArcaneArms).Name; item = "Arcane Arms"; }
			else if (artifact == arty++) { name = typeof(Artifact_ArcaneCap).Name; item = "Arcane Cap"; }
			else if (artifact == arty++) { name = typeof(Artifact_ArcaneGloves).Name; item = "Arcane Gloves"; }
			else if (artifact == arty++) { name = typeof(Artifact_ArcaneGorget).Name; item = "Arcane Gorget"; }
			else if (artifact == arty++) { name = typeof(Artifact_ArcaneLeggings).Name; item = "Arcane Leggings"; }
			else if (artifact == arty++) { name = typeof(Artifact_ArcaneShield).Name; item = "Arcane Shield"; }
			else if (artifact == arty++) { name = typeof(Artifact_ArcaneTunic).Name; item = "Arcane Tunic"; }
			else if (artifact == arty++) { name = typeof(Artifact_ArcanicRobe).Name; item = "Arcanic Robe"; }
			else if (artifact == arty++) { name = typeof(Artifact_ArcticDeathDealer).Name; item = "Arctic Death Dealer"; }
			else if (artifact == arty++) { name = typeof(Artifact_ArmorOfFortune).Name; item = "Armor of Fortune"; }
			else if (artifact == arty++) { name = typeof(Artifact_ArmorOfInsight).Name; item = "Armor of Insight"; }
			else if (artifact == arty++) { name = typeof(Artifact_ArmorOfNobility).Name; item = "Armor of Nobility"; }
			else if (artifact == arty++) { name = typeof(Artifact_ArmsOfAegis).Name; item = "Arms of Aegis"; }
			else if (artifact == arty++) { name = typeof(Artifact_ArmsOfFortune).Name; item = "Arms of Fortune"; }
			else if (artifact == arty++) { name = typeof(Artifact_ArmsOfInsight).Name; item = "Arms of Insight"; }
			else if (artifact == arty++) { name = typeof(Artifact_ArmsOfNobility).Name; item = "Arms of Nobility"; }
			else if (artifact == arty++) { name = typeof(Artifact_ArmsOfTheFallenKing).Name; item = "Arms of the Fallen King"; }
			else if (artifact == arty++) { name = typeof(Artifact_ArmsOfTheHarrower).Name; item = "Arms of the Harrower"; }
			else if (artifact == arty++) { name = typeof(Artifact_ArmsOfToxicity).Name; item = "Arms Of Toxicity"; }
			else if (artifact == arty++) { name = typeof(Artifact_AuraOfShadows).Name; item = "Aura Of Shadows"; }
			else if (artifact == arty++) { name = typeof(Artifact_AxeOfTheHeavens).Name; item = "Axe of the Heavens"; }
			else if (artifact == arty++) { name = typeof(Artifact_AxeoftheMinotaur).Name; item = "Axe of the Minotaur"; }
			else if (artifact == arty++) { name = typeof(Artifact_BeggarsRobe).Name; item = "Beggar's Robe"; }
			else if (artifact == arty++) { name = typeof(Artifact_BeltofHercules).Name; item = "Belt of Hercules"; }
			else if (artifact == arty++) { name = typeof(Artifact_TheBeserkersMaul).Name; item = "Berserker's Maul"; }
			else if (artifact == arty++) { name = typeof(Artifact_BladeDance).Name; item = "Blade Dance"; }
			else if (artifact == arty++) { name = typeof(Artifact_BladeOfInsanity).Name; item = "Blade of Insanity"; }
			else if (artifact == arty++) { name = typeof(Artifact_ConansSword).Name; item = "Blade of the Cimmerian"; }
			else if (artifact == arty++) { name = typeof(Artifact_BladeOfTheRighteous).Name; item = "Blade of the Righteous"; }
			else if (artifact == arty++) { name = typeof(Artifact_ShadowBlade).Name; item = "Blade of the Shadows"; }
			else if (artifact == arty++) { name = typeof(Artifact_BlazeOfDeath).Name; item = "Blaze of Death"; }
			else if (artifact == arty++) { name = typeof(Artifact_BlightGrippedLongbow).Name; item = "Blight Gripped Longbow"; }
			else if (artifact == arty++) { name = typeof(Artifact_BloodwoodSpirit).Name; item = "Bloodwood Spirit"; }
			else if (artifact == arty++) { name = typeof(Artifact_BoneCrusher).Name; item = "Bone Crusher"; }
			else if (artifact == arty++) { name = typeof(Artifact_Bonesmasher).Name; item = "Bonesmasher"; }
			else if (artifact == arty++) { name = typeof(Artifact_BookOfKnowledge).Name; item = "Book Of Knowledge"; }
			else if (artifact == arty++) { name = typeof(Artifact_Boomstick).Name; item = "Boomstick"; }
			else if (artifact == arty++) { name = typeof(Artifact_BootsofHermes).Name; item = "Boots of Hermes"; }
			else if (artifact == arty++) { name = typeof(Artifact_BootsofPyros).Name; item = "Boots of the Daemon King"; }
			else if (artifact == arty++) { name = typeof(Artifact_BootsofHydros).Name; item = "Boots of the Lurker"; }
			else if (artifact == arty++) { name = typeof(Artifact_BootsofLithos).Name; item = "Boots of the Mountain King"; }
			else if (artifact == arty++) { name = typeof(Artifact_BootsofStratos).Name; item = "Boots of the Mystic Voice"; }
			else if (artifact == arty++) { name = typeof(Artifact_BowOfTheJukaKing).Name; item = "Bow of the Juka King"; }
			else if (artifact == arty++) { name = typeof(Artifact_BowofthePhoenix).Name; item = "Bow of the Phoenix"; }
			else if (artifact == arty++) { name = typeof(Artifact_BraceletOfHealth).Name; item = "Bracelet of Health"; }
			else if (artifact == arty++) { name = typeof(Artifact_BraceletOfTheElements).Name; item = "Bracelet of the Elements"; }
			else if (artifact == arty++) { name = typeof(Artifact_BraceletOfTheVile).Name; item = "Bracelet of the Vile"; }
			else if (artifact == arty++) { name = typeof(Artifact_BrambleCoat).Name; item = "Bramble Coat"; }
			else if (artifact == arty++) { name = typeof(Artifact_BraveKnightOfTheBritannia).Name; item = "Brave Knight of Sosaria"; }
			else if (artifact == arty++) { name = typeof(Artifact_BreathOfTheDead).Name; item = "Breath of the Dead"; }
			else if (artifact == arty++) { name = typeof(Artifact_BurglarsBandana).Name; item = "Burglar's Bandana"; }
			else if (artifact == arty++) { name = typeof(Artifact_Calm).Name; item = "Calm"; }
			else if (artifact == arty++) { name = typeof(Artifact_CandleCold).Name; item = "Candle of Cold Light"; }
			else if (artifact == arty++) { name = typeof(Artifact_CandleEnergy).Name; item = "Candle of Energized Light"; }
			else if (artifact == arty++) { name = typeof(Artifact_CandleFire).Name; item = "Candle of Fire Light"; }
			else if (artifact == arty++) { name = typeof(Artifact_CandleNecromancer).Name; item = "Candle of Ghostly Light"; }
			else if (artifact == arty++) { name = typeof(Artifact_CandlePoison).Name; item = "Candle of Poisonous Light"; }
			else if (artifact == arty++) { name = typeof(Artifact_CandleWizard).Name; item = "Candle of Wizardly Light"; }
			else if (artifact == arty++) { name = typeof(Artifact_CapOfFortune).Name; item = "Cap of Fortune"; }
			else if (artifact == arty++) { name = typeof(Artifact_CapOfTheFallenKing).Name; item = "Cap of the Fallen King"; }
			else if (artifact == arty++) { name = typeof(Artifact_CaptainJohnsHat).Name; item = "Captain John's Hat"; }
			else if (artifact == arty++) { name = typeof(Artifact_CaptainQuacklebushsCutlass).Name; item = "Captain Quacklebush's Cutlass"; }
			else if (artifact == arty++) { name = typeof(Artifact_CavortingClub).Name; item = "Cavorting Club"; }
			else if (artifact == arty++) { name = typeof(Artifact_CircletOfTheSorceress).Name; item = "Circlet Of The Sorceress"; }
			else if (artifact == arty++) { name = typeof(Artifact_GrayMouserCloak).Name; item = "Cloak of the Rogue"; }
			else if (artifact == arty++) { name = typeof(Artifact_CoifOfBane).Name; item = "Coif of Bane"; }
			else if (artifact == arty++) { name = typeof(Artifact_CoifOfFire).Name; item = "Coif of Fire"; }
			else if (artifact == arty++) { name = typeof(Artifact_ColdBlood).Name; item = "Cold Blood"; }
			else if (artifact == arty++) { name = typeof(Artifact_ColdForgedBlade).Name; item = "Cold Forged Blade"; }
			else if (artifact == arty++) { name = typeof(Artifact_CrimsonCincture).Name; item = "Crimson Cincture"; }
			else if (artifact == arty++) { name = typeof(Artifact_CrownOfTalKeesh).Name; item = "Crown of Tal'Keesh"; }
			else if (artifact == arty++) { name = typeof(Artifact_DaggerOfVenom).Name; item = "Dagger of Venom"; }
			else if (artifact == arty++) { name = typeof(Artifact_DarkGuardiansChest).Name; item = "Dark Guardian's Chest"; }
			else if (artifact == arty++) { name = typeof(Artifact_DarkLordsPitchfork).Name; item = "Dark Lord's PitchFork"; }
			else if (artifact == arty++) { name = typeof(Artifact_DarkNeck).Name; item = "Dark Neck"; }
			else if (artifact == arty++) { name = typeof(Artifact_DetectiveBoots).Name; item = "Detective Boots of the Royal Guard"; }
			else if (artifact == arty++) { name = typeof(Artifact_DivineArms).Name; item = "Divine Arms"; }
			else if (artifact == arty++) { name = typeof(Artifact_DivineCountenance).Name; item = "Divine Countenance"; }
			else if (artifact == arty++) { name = typeof(Artifact_DivineGloves).Name; item = "Divine Gloves"; }
			else if (artifact == arty++) { name = typeof(Artifact_DivineGorget).Name; item = "Divine Gorget"; }
			else if (artifact == arty++) { name = typeof(Artifact_DivineLeggings).Name; item = "Divine Leggings"; }
			else if (artifact == arty++) { name = typeof(Artifact_DivineTunic).Name; item = "Divine Tunic"; }
			else if (artifact == arty++) { name = typeof(Artifact_DjinnisRing).Name; item = "Djinni's Ring"; }
			else if (artifact == arty++) { name = typeof(Artifact_DreadPirateHat).Name; item = "Dread Pirate Hat"; }
			else if (artifact == arty++) { name = typeof(Artifact_TheDryadBow).Name; item = "Dryad Bow"; }
			else if (artifact == arty++) { name = typeof(Artifact_DupresCollar).Name; item = "Dupre's Collar"; }
			else if (artifact == arty++) { name = typeof(Artifact_DupresShield).Name; item = "Dupre's Shield"; }
			else if (artifact == arty++) { name = typeof(Artifact_EarringsOfHealth).Name; item = "Earrings of Health"; }
			else if (artifact == arty++) { name = typeof(Artifact_EarringsOfTheElements).Name; item = "Earrings of the Elements"; }
			else if (artifact == arty++) { name = typeof(Artifact_EarringsOfTheMagician).Name; item = "Earrings of the Magician"; }
			else if (artifact == arty++) { name = typeof(Artifact_EarringsOfTheVile).Name; item = "Earrings of the Vile"; }
			else if (artifact == arty++) { name = typeof(Artifact_EmbroideredOakLeafCloak).Name; item = "Embroidered Oak Leaf Cloak"; }
			else if (artifact == arty++) { name = typeof(Artifact_EnchantedTitanLegBone).Name; item = "Enchanted Pirate Rapier"; }
			else if (artifact == arty++) { name = typeof(Artifact_EssenceOfBattle).Name; item = "Essence of Battle"; }
			else if (artifact == arty++) { name = typeof(Artifact_EternalFlame).Name; item = "Eternal Flame"; }
			else if (artifact == arty++) { name = typeof(Artifact_EvilMageGloves).Name; item = "Evil Mage Gloves"; }
			else if (artifact == arty++) { name = typeof(Artifact_Excalibur).Name; item = "Excalibur"; }
			else if (artifact == arty++) { name = typeof(Artifact_FangOfRactus).Name; item = "Fang of Ractus"; }
			else if (artifact == arty++) { name = typeof(Artifact_FesteringWound).Name; item = "Festering Wound"; }
			else if (artifact == arty++) { name = typeof(Artifact_FeyLeggings).Name; item = "Fey Leggings"; }
			else if (artifact == arty++) { name = typeof(Artifact_FleshRipper).Name; item = "Flesh Ripper"; }
			else if (artifact == arty++) { name = typeof(Artifact_Fortifiedarms).Name; item = "Fortified Arms"; }
			else if (artifact == arty++) { name = typeof(Artifact_FortunateBlades).Name; item = "Fortunate Blades"; }
			else if (artifact == arty++) { name = typeof(Artifact_Frostbringer).Name; item = "Frostbringer"; }
			else if (artifact == arty++) { name = typeof(Artifact_FurCapeOfTheSorceress).Name; item = "Fur Cape Of The Sorceress"; }
			else if (artifact == arty++) { name = typeof(Artifact_Fury).Name; item = "Fury"; }
			else if (artifact == arty++) { name = typeof(Artifact_MarbleShield).Name; item = "Gargoyle Shield"; }
			else if (artifact == arty++) { name = typeof(Artifact_GuantletsOfAnger).Name; item = "Gauntlets of Anger"; }
			else if (artifact == arty++) { name = typeof(Artifact_GauntletsOfNobility).Name; item = "Gauntlets of Nobility"; }
			else if (artifact == arty++) { name = typeof(Artifact_GeishasObi).Name; item = "Geishas Obi"; }
			else if (artifact == arty++) { name = typeof(Artifact_GiantBlackjack).Name; item = "Giant Blackjack"; }
			else if (artifact == arty++) { name = typeof(Artifact_GladiatorsCollar).Name; item = "Gladiator's Collar"; }
			else if (artifact == arty++) { name = typeof(Artifact_GlovesOfAegis).Name; item = "Gloves of Aegis"; }
			else if (artifact == arty++) { name = typeof(Artifact_GlovesOfCorruption).Name; item = "Gloves Of Corruption"; }
			else if (artifact == arty++) { name = typeof(Artifact_GlovesOfDexterity).Name; item = "Gloves of Dexterity"; }
			else if (artifact == arty++) { name = typeof(Artifact_GlovesOfFortune).Name; item = "Gloves of Fortune"; }
			else if (artifact == arty++) { name = typeof(Artifact_GlovesOfInsight).Name; item = "Gloves of Insight"; }
			else if (artifact == arty++) { name = typeof(Artifact_GlovesOfRegeneration).Name; item = "Gloves Of Regeneration"; }
			else if (artifact == arty++) { name = typeof(Artifact_GlovesOfTheFallenKing).Name; item = "Gloves of the Fallen King"; }
			else if (artifact == arty++) { name = typeof(Artifact_GlovesOfTheHarrower).Name; item = "Gloves of the Harrower"; }
			else if (artifact == arty++) { name = typeof(Artifact_GlovesOfThePugilist).Name; item = "Gloves of the Pugilist"; }
			else if (artifact == arty++) { name = typeof(Artifact_SamaritanRobe).Name; item = "Good Samaritan Robe"; }
			else if (artifact == arty++) { name = typeof(Artifact_GorgetOfAegis).Name; item = "Gorget of Aegis"; }
			else if (artifact == arty++) { name = typeof(Artifact_GorgetOfFortune).Name; item = "Gorget of Fortune"; }
			else if (artifact == arty++) { name = typeof(Artifact_GorgetOfInsight).Name; item = "Gorget of Insight"; }
			else if (artifact == arty++) { name = typeof(Artifact_GrimReapersLantern).Name; item = "Grim Reaper's Lantern"; }
			else if (artifact == arty++) { name = typeof(Artifact_GrimReapersMask).Name; item = "Grim Reaper's Mask"; }
			else if (artifact == arty++) { name = typeof(Artifact_GrimReapersRobe).Name; item = "Grim Reaper's Robe"; }
			else if (artifact == arty++) { name = typeof(Artifact_GrimReapersScythe).Name; item = "Grim Reaper's Scythe"; }
			else if (artifact == arty++) { name = typeof(Artifact_PyrosGrimoire).Name; item = "Grimoire of the Daemon King"; }
			else if (artifact == arty++) { name = typeof(Artifact_TownGuardsHalberd).Name; item = "Guardsman Halberd"; }
			else if (artifact == arty++) { name = typeof(Artifact_GwennosHarp).Name; item = "Gwenno's Harp"; }
			else if (artifact == arty++) { name = typeof(Artifact_HammerofThor).Name; item = "Hammer of Thor"; }
			else if (artifact == arty++) { name = typeof(Artifact_HatOfTheMagi).Name; item = "Hat of the Magi"; }
			else if (artifact == arty++) { name = typeof(Artifact_HeartOfTheLion).Name; item = "Heart of the Lion"; }
			else if (artifact == arty++) { name = typeof(Artifact_HellForgedArms).Name; item = "Hell Forged Arms"; }
			else if (artifact == arty++) { name = typeof(Artifact_HelmOfAegis).Name; item = "Helm of Aegis"; }
			else if (artifact == arty++) { name = typeof(Artifact_HelmOfBrilliance).Name; item = "Helm of Brilliance"; }
			else if (artifact == arty++) { name = typeof(Artifact_HelmOfInsight).Name; item = "Helm of Insight"; }
			else if (artifact == arty++) { name = typeof(Artifact_HelmOfSwiftness).Name; item = "Helm of Swiftness"; }
			else if (artifact == arty++) { name = typeof(Artifact_ConansHelm).Name; item = "Helm of the Cimmerian"; }
			else if (artifact == arty++) { name = typeof(Artifact_HolyKnightsArmPlates).Name; item = "Holy Knight's Arm Plates"; }
			else if (artifact == arty++) { name = typeof(Artifact_HolyKnightsBreastplate).Name; item = "Holy Knight's Breastplate"; }
			else if (artifact == arty++) { name = typeof(Artifact_HolyKnightsGloves).Name; item = "Holy Knight's Gloves"; }
			else if (artifact == arty++) { name = typeof(Artifact_HolyKnightsGorget).Name; item = "Holy Knight's Gorget"; }
			else if (artifact == arty++) { name = typeof(Artifact_HolyKnightsLegging).Name; item = "Holy Knight's Legging"; }
			else if (artifact == arty++) { name = typeof(Artifact_HolyKnightsPlateHelm).Name; item = "Holy Knight's Plate Helm"; }
			else if (artifact == arty++) { name = typeof(Artifact_LunaLance).Name; item = "Holy Lance"; }
			else if (artifact == arty++) { name = typeof(Artifact_HolySword).Name; item = "Holy Sword"; }
			else if (artifact == arty++) { name = typeof(Artifact_HoodedShroudOfShadows).Name; item = "Hooded Shroud of Shadows"; }
			else if (artifact == arty++) { name = typeof(Artifact_HornOfKingTriton).Name; item = "Horn of King Triton"; }
			else if (artifact == arty++) { name = typeof(Artifact_HuntersArms).Name; item = "Hunter's Arms"; }
			else if (artifact == arty++) { name = typeof(Artifact_HuntersGloves).Name; item = "Hunter's Gloves"; }
			else if (artifact == arty++) { name = typeof(Artifact_HuntersGorget).Name; item = "Hunter's Gorget"; }
			else if (artifact == arty++) { name = typeof(Artifact_HuntersHeaddress).Name; item = "Hunter's Headdress"; }
			else if (artifact == arty++) { name = typeof(Artifact_HuntersLeggings).Name; item = "Hunter's Leggings"; }
			else if (artifact == arty++) { name = typeof(Artifact_HuntersTunic).Name; item = "Hunter's Tunic"; }
			else if (artifact == arty++) { name = typeof(Artifact_Indecency).Name; item = "Indecency"; }
			else if (artifact == arty++) { name = typeof(Artifact_InquisitorsArms).Name; item = "Inquisitor's Arms"; }
			else if (artifact == arty++) { name = typeof(Artifact_InquisitorsGorget).Name; item = "Inquisitor's Gorget"; }
			else if (artifact == arty++) { name = typeof(Artifact_InquisitorsHelm).Name; item = "Inquisitor's Helm"; }
			else if (artifact == arty++) { name = typeof(Artifact_InquisitorsLeggings).Name; item = "Inquisitor's Leggings"; }
			else if (artifact == arty++) { name = typeof(Artifact_InquisitorsResolution).Name; item = "Inquisitor's Resolution"; }
			else if (artifact == arty++) { name = typeof(Artifact_InquisitorsTunic).Name; item = "Inquisitor's Tunic"; }
			else if (artifact == arty++) { name = typeof(Artifact_IolosLute).Name; item = "Iolo's Lute"; }
			else if (artifact == arty++) { name = typeof(Artifact_IronwoodCrown).Name; item = "Ironwood Crown"; }
			else if (artifact == arty++) { name = typeof(Artifact_JackalsArms).Name; item = "Jackal's Arms"; }
			else if (artifact == arty++) { name = typeof(Artifact_JackalsCollar).Name; item = "Jackal's Collar"; }
			else if (artifact == arty++) { name = typeof(Artifact_JackalsGloves).Name; item = "Jackal's Gloves"; }
			else if (artifact == arty++) { name = typeof(Artifact_JackalsHelm).Name; item = "Jackal's Helm"; }
			else if (artifact == arty++) { name = typeof(Artifact_JackalsLeggings).Name; item = "Jackal's Leggings"; }
			else if (artifact == arty++) { name = typeof(Artifact_JackalsTunic).Name; item = "Jackal's Tunic"; }
			else if (artifact == arty++) { name = typeof(Artifact_JadeScimitar).Name; item = "Jade Scimitar"; }
			else if (artifact == arty++) { name = typeof(Artifact_JesterHatofChuckles).Name; item = "Jester Hat of Chuckles"; }
			else if (artifact == arty++) { name = typeof(Artifact_JinBaoriOfGoodFortune).Name; item = "Jin-Baori Of Good Fortune"; }
			else if (artifact == arty++) { name = typeof(Artifact_KamiNarisIndestructableDoubleAxe).Name; item = "Kami-Naris Indestructable Axe"; }
			else if (artifact == arty++) { name = typeof(Artifact_KodiakBearMask).Name; item = "Kodiak Bear Mask"; }
			else if (artifact == arty++) { name = typeof(Artifact_PowerSurge).Name; item = "Lantern of Power"; }
			else if (artifact == arty++) { name = typeof(Artifact_LegacyOfTheDreadLord).Name; item = "Legacy of the Dread Lord"; }
			else if (artifact == arty++) { name = typeof(Artifact_LegsOfFortune).Name; item = "Legging of Fortune"; }
			else if (artifact == arty++) { name = typeof(Artifact_LegsOfInsight).Name; item = "Legging of Insight"; }
			else if (artifact == arty++) { name = typeof(Artifact_LeggingsOfAegis).Name; item = "Leggings of Aegis"; }
			else if (artifact == arty++) { name = typeof(Artifact_LeggingsOfBane).Name; item = "Leggings of Bane"; }
			else if (artifact == arty++) { name = typeof(Artifact_LeggingsOfDeceit).Name; item = "Leggings Of Deceit"; }
			else if (artifact == arty++) { name = typeof(Artifact_LeggingsOfEnlightenment).Name; item = "Leggings Of Enlightenment"; }
			else if (artifact == arty++) { name = typeof(Artifact_LeggingsOfFire).Name; item = "Leggings of Fire"; }
			else if (artifact == arty++) { name = typeof(Artifact_LegsOfTheFallenKing).Name; item = "Leggings of the Fallen King"; }
			else if (artifact == arty++) { name = typeof(Artifact_LegsOfTheHarrower).Name; item = "Leggings of the Harrower"; }
			else if (artifact == arty++) { name = typeof(Artifact_LegsOfNobility).Name; item = "Legs of Nobility"; }
			else if (artifact == arty++) { name = typeof(Artifact_HydrosLexicon).Name; item = "Lexicon of the Lurker"; }
			else if (artifact == arty++) { name = typeof(Artifact_ConansLoinCloth).Name; item = "Loin Cloth of the Cimmerian"; }
			else if (artifact == arty++) { name = typeof(Artifact_LongShot).Name; item = "Long Shot"; }
			else if (artifact == arty++) { name = typeof(Artifact_LuckyEarrings).Name; item = "Lucky Earrings"; }
			else if (artifact == arty++) { name = typeof(Artifact_LuckyNecklace).Name; item = "Lucky Necklace"; }
			else if (artifact == arty++) { name = typeof(Artifact_LuminousRuneBlade).Name; item = "Luminous Rune Blade"; }
			else if (artifact == arty++) { name = typeof(Artifact_MadmansHatchet).Name; item = "Madman's Hatchet"; }
			else if (artifact == arty++) { name = typeof(Artifact_MagesBand).Name; item = "Mage's Band"; }
			else if (artifact == arty++) { name = typeof(Artifact_MagiciansIllusion).Name; item = "Magician's Illusion"; }
			else if (artifact == arty++) { name = typeof(Artifact_MagiciansMempo).Name; item = "Magician's Mempo"; }
			else if (artifact == arty++) { name = typeof(Artifact_MantleofPyros).Name; item = "Mantle of the Daemon King"; }
			else if (artifact == arty++) { name = typeof(Artifact_MantleofHydros).Name; item = "Mantle of the Lurker"; }
			else if (artifact == arty++) { name = typeof(Artifact_MantleofLithos).Name; item = "Mantle of the Mountain King"; }
			else if (artifact == arty++) { name = typeof(Artifact_MantleofStratos).Name; item = "Mantle of the Mystic Voice"; }
			else if (artifact == arty++) { name = typeof(Artifact_StratosManual).Name; item = "Manual of the Mystic Voice"; }
			else if (artifact == arty++) { name = typeof(Artifact_DeathsMask).Name; item = "Mask of Death"; }
			else if (artifact == arty++) { name = typeof(Artifact_MauloftheBeast).Name; item = "Maul of the Beast"; }
			else if (artifact == arty++) { name = typeof(Artifact_MaulOfTheTitans).Name; item = "Maul of the Titans"; }
			else if (artifact == arty++) { name = typeof(Artifact_MelisandesCorrodedHatchet).Name; item = "Melisande's Corroded Hatchet"; }
			else if (artifact == arty++) { name = typeof(Artifact_GandalfsHat).Name; item = "Merlin's Mystical Hat"; }
			else if (artifact == arty++) { name = typeof(Artifact_GandalfsRobe).Name; item = "Merlin's Mystical Robe"; }
			else if (artifact == arty++) { name = typeof(Artifact_GandalfsStaff).Name; item = "Merlin's Mystical Staff"; }
			else if (artifact == arty++) { name = typeof(Artifact_MidnightBracers).Name; item = "Midnight Bracers"; }
			else if (artifact == arty++) { name = typeof(Artifact_MidnightGloves).Name; item = "Midnight Gloves"; }
			else if (artifact == arty++) { name = typeof(Artifact_MidnightHelm).Name; item = "Midnight Helm"; }
			else if (artifact == arty++) { name = typeof(Artifact_MidnightLegs).Name; item = "Midnight Leggings"; }
			else if (artifact == arty++) { name = typeof(Artifact_MidnightTunic).Name; item = "Midnight Tunic"; }
			else if (artifact == arty++) { name = typeof(Artifact_MinersPickaxe).Name; item = "Miner's Pickaxe"; }
			else if (artifact == arty++) { name = typeof(Artifact_ANecromancerShroud).Name; item = "Necromancer Shroud"; }
			else if (artifact == arty++) { name = typeof(Artifact_TheNightReaper).Name; item = "Night Reaper"; }
			else if (artifact == arty++) { name = typeof(Artifact_NightsKiss).Name; item = "Night's Kiss"; }
			else if (artifact == arty++) { name = typeof(Artifact_NordicVikingSword).Name; item = "Nordic Dragon Blade"; }
			else if (artifact == arty++) { name = typeof(Artifact_VampiresRobe).Name; item = "Nosferatu's Robe"; }
			else if (artifact == arty++) { name = typeof(Artifact_NoxBow).Name; item = "Nox Bow"; }
			else if (artifact == arty++) { name = typeof(Artifact_NoxNightlight).Name; item = "Nox Nightlight"; }
			else if (artifact == arty++) { name = typeof(Artifact_NoxRangersHeavyCrossbow).Name; item = "Nox Ranger's Heavy Crossbow"; }
			else if (artifact == arty++) { name = typeof(Artifact_OblivionsNeedle).Name; item = "Oblivion Needle"; }
			else if (artifact == arty++) { name = typeof(Artifact_OrcChieftainHelm).Name; item = "Orc Chieftain Helm"; }
			else if (artifact == arty++) { name = typeof(Artifact_OrcishVisage).Name; item = "Orcish Visage"; }
			else if (artifact == arty++) { name = typeof(Artifact_OrnamentOfTheMagician).Name; item = "Ornament of the Magician"; }
			else if (artifact == arty++) { name = typeof(Artifact_OrnateCrownOfTheHarrower).Name; item = "Ornate Crown of the Harrower"; }
			else if (artifact == arty++) { name = typeof(Artifact_OssianGrimoire).Name; item = "Ossian Grimoire"; }
			else if (artifact == arty++) { name = typeof(Artifact_OverseerSunderedBlade).Name; item = "Overseer Sundered Blade"; }
			else if (artifact == arty++) { name = typeof(Artifact_Pacify).Name; item = "Pacify"; }
			else if (artifact == arty++) { name = typeof(Artifact_PadsOfTheCuSidhe).Name; item = "Pads of the Cu Sidhe"; }
			else if (artifact == arty++) { name = typeof(Artifact_PendantOfTheMagi).Name; item = "Pendant of the Magi"; }
			else if (artifact == arty++) { name = typeof(Artifact_Pestilence).Name; item = "Pestilence"; }
			else if (artifact == arty++) { name = typeof(Artifact_PhantomStaff).Name; item = "Phantom Staff"; }
			else if (artifact == arty++) { name = typeof(Artifact_PixieSwatter).Name; item = "Pixie Swatter"; }
			else if (artifact == arty++) { name = typeof(Artifact_PolarBearBoots).Name; item = "Polar Bear Boots"; }
			else if (artifact == arty++) { name = typeof(Artifact_PolarBearCape).Name; item = "Polar Bear Cape"; }
			else if (artifact == arty++) { name = typeof(Artifact_Quell).Name; item = "Quell"; }
			else if (artifact == arty++) { name = typeof(Artifact_QuiverOfBlight).Name; item = "Quiver of Blight"; }
			else if (artifact == arty++) { name = typeof(Artifact_QuiverOfFire).Name; item = "Quiver of Fire"; }
			else if (artifact == arty++) { name = typeof(Artifact_QuiverOfIce).Name; item = "Quiver of Ice"; }
			else if (artifact == arty++) { name = typeof(Artifact_QuiverOfInfinity).Name; item = "Quiver of Infinity"; }
			else if (artifact == arty++) { name = typeof(Artifact_QuiverOfLightning).Name; item = "Quiver of Lightning"; }
			else if (artifact == arty++) { name = typeof(Artifact_QuiverOfRage).Name; item = "Quiver of Rage"; }
			else if (artifact == arty++) { name = typeof(Artifact_QuiverOfElements).Name; item = "Quiver of the Elements"; }
			else if (artifact == arty++) { name = typeof(Artifact_RaedsGlory).Name; item = "Raed's Glory"; }
			else if (artifact == arty++) { name = typeof(Artifact_RamusNecromanticScalpel).Name; item = "Ramus' Necromantic Scalpel"; }
			else if (artifact == arty++) { name = typeof(Artifact_ResilientBracer).Name; item = "Resillient Bracer"; }
			else if (artifact == arty++) { name = typeof(Artifact_Retort).Name; item = "Retort"; }
			else if (artifact == arty++) { name = typeof(Artifact_RighteousAnger).Name; item = "Righteous Anger"; }
			else if (artifact == arty++) { name = typeof(Artifact_RingOfHealth).Name; item = "Ring of Health"; }
			else if (artifact == arty++) { name = typeof(Artifact_RingOfProtection).Name; item = "Ring of Protection"; }
			else if (artifact == arty++) { name = typeof(Artifact_RingOfTheElements).Name; item = "Ring of the Elements"; }
			else if (artifact == arty++) { name = typeof(Artifact_RingOfTheMagician).Name; item = "Ring of the Magician"; }
			else if (artifact == arty++) { name = typeof(Artifact_RingOfTheVile).Name; item = "Ring of the Vile"; }
			else if (artifact == arty++) { name = typeof(Artifact_TheRobeOfBritanniaAri).Name; item = "Robe of Sosaria"; }
			else if (artifact == arty++) { name = typeof(Artifact_RobeOfTeleportation).Name; item = "Robe Of Teleportation"; }
			else if (artifact == arty++) { name = typeof(Artifact_RobeofPyros).Name; item = "Robe of the Daemon King"; }
			else if (artifact == arty++) { name = typeof(Artifact_RobeOfTheEclipse).Name; item = "Robe of the Eclipse"; }
			else if (artifact == arty++) { name = typeof(Artifact_RobeOfTheEquinox).Name; item = "Robe of the Equinox"; }
			else if (artifact == arty++) { name = typeof(Artifact_RobeofHydros).Name; item = "Robe of the Lurker"; }
			else if (artifact == arty++) { name = typeof(Artifact_RobeofLithos).Name; item = "Robe of the Mountain King"; }
			else if (artifact == arty++) { name = typeof(Artifact_RobeofStratos).Name; item = "Robe of the Mystic Voice"; }
			else if (artifact == arty++) { name = typeof(Artifact_RobeOfTreason).Name; item = "Robe Of Treason"; }
			else if (artifact == arty++) { name = typeof(Artifact_RobinHoodsBow).Name; item = "Robin Hood's Bow"; }
			else if (artifact == arty++) { name = typeof(Artifact_RobinHoodsFeatheredHat).Name; item = "Robin Hood's Feathered Hat"; }
			else if (artifact == arty++) { name = typeof(Artifact_RodOfResurrection).Name; item = "Rod Of Resurrection"; }
			else if (artifact == arty++) { name = typeof(Artifact_RoyalArchersBow).Name; item = "Royal Archer's Bow"; }
			else if (artifact == arty++) { name = typeof(Artifact_LieutenantOfTheBritannianRoyalGuard).Name; item = "Royal Guard Sash"; }
			else if (artifact == arty++) { name = typeof(Artifact_RoyalGuardSurvivalKnife).Name; item = "Royal Guard Survival Knife"; }
			else if (artifact == arty++) { name = typeof(Artifact_RoyalGuardsGorget).Name; item = "Royal Guardian's Gorget"; }
			else if (artifact == arty++) { name = typeof(Artifact_RoyalGuardsChestplate).Name; item = "Royal Guard's Chest Plate"; }
			else if (artifact == arty++) { name = typeof(Artifact_LeggingsOfEmbers).Name; item = "Royal Leggings of Embers"; }
			else if (artifact == arty++) { name = typeof(Artifact_RuneCarvingKnife).Name; item = "Rune Carving Knife"; }
			else if (artifact == arty++) { name = typeof(Artifact_FalseGodsScepter).Name; item = "Scepter Of The False Goddess"; }
			else if (artifact == arty++) { name = typeof(Artifact_SerpentsFang).Name; item = "Serpent's Fang"; }
			else if (artifact == arty++) { name = typeof(Artifact_ShadowDancerArms).Name; item = "Shadow Dancer Arms"; }
			else if (artifact == arty++) { name = typeof(Artifact_ShadowDancerCap).Name; item = "Shadow Dancer Cap"; }
			else if (artifact == arty++) { name = typeof(Artifact_ShadowDancerGloves).Name; item = "Shadow Dancer Gloves"; }
			else if (artifact == arty++) { name = typeof(Artifact_ShadowDancerGorget).Name; item = "Shadow Dancer Gorget"; }
			else if (artifact == arty++) { name = typeof(Artifact_ShadowDancerLeggings).Name; item = "Shadow Dancer Leggings"; }
			else if (artifact == arty++) { name = typeof(Artifact_ShadowDancerTunic).Name; item = "Shadow Dancer Tunic"; }
			else if (artifact == arty++) { name = typeof(Artifact_ShaMontorrossbow).Name; item = "Shamino's Crossbow"; }
			else if (artifact == arty++) { name = typeof(Artifact_ShardThrasher).Name; item = "Shard Thrasher"; }
			else if (artifact == arty++) { name = typeof(Artifact_ShieldOfInvulnerability).Name; item = "Shield of Invulnerability"; }
			else if (artifact == arty++) { name = typeof(Artifact_ShimmeringTalisman).Name; item = "Shimmering Talisman"; }
			else if (artifact == arty++) { name = typeof(Artifact_ShroudOfDeciet).Name; item = "Shroud of Deceit"; }
			else if (artifact == arty++) { name = typeof(Artifact_SilvanisFeywoodBow).Name; item = "Silvani's Feywood Bow"; }
			else if (artifact == arty++) { name = typeof(Artifact_TheDragonSlayer).Name; item = "Slayer of Dragons"; }
			else if (artifact == arty++) { name = typeof(Artifact_SongWovenMantle).Name; item = "Song Woven Mantle"; }
			else if (artifact == arty++) { name = typeof(Artifact_SoulSeeker).Name; item = "Soul Seeker"; }
			else if (artifact == arty++) { name = typeof(Artifact_SpellWovenBritches).Name; item = "Spell Woven Britches"; }
			else if (artifact == arty++) { name = typeof(Artifact_PolarBearMask).Name; item = "Spirit of the Polar Bear"; }
			else if (artifact == arty++) { name = typeof(Artifact_SpiritOfTheTotem).Name; item = "Spirit of the Totem"; }
			else if (artifact == arty++) { name = typeof(Artifact_SprintersSandals).Name; item = "Sprinter's Sandals"; }
			else if (artifact == arty++) { name = typeof(Artifact_StaffOfPower).Name; item = "Staff of Power"; }
			else if (artifact == arty++) { name = typeof(Artifact_StaffOfTheMagi).Name; item = "Staff of the Magi"; }
			else if (artifact == arty++) { name = typeof(Artifact_StaffofSnakes).Name; item = "Staff of the Serpent"; }
			else if (artifact == arty++) { name = typeof(Artifact_StitchersMittens).Name; item = "Stitcher's Mittens"; }
			else if (artifact == arty++) { name = typeof(Artifact_Stormbringer).Name; item = "Stormbringer"; }
			else if (artifact == arty++) { name = typeof(Artifact_Subdue).Name; item = "Subdue"; }
			else if (artifact == arty++) { name = typeof(Artifact_SwiftStrike).Name; item = "Swift Strike"; }
			else if (artifact == arty++) { name = typeof(Artifact_GlassSword).Name; item = "Sword of Shattered Hopes"; }
			else if (artifact == arty++) { name = typeof(Artifact_SinbadsSword).Name; item = "Sword of Sinbad"; }
			else if (artifact == arty++) { name = typeof(Artifact_TalonBite).Name; item = "Talon Bite"; }
			else if (artifact == arty++) { name = typeof(Artifact_TheTaskmaster).Name; item = "Taskmaster"; }
			else if (artifact == arty++) { name = typeof(Artifact_TitansHammer).Name; item = "Titan's Hammer"; }
			else if (artifact == arty++) { name = typeof(Artifact_LithosTome).Name; item = "Tome of the Mountain King"; }
			else if (artifact == arty++) { name = typeof(Artifact_TorchOfTrapFinding).Name; item = "Torch of Trap Burning"; }
			else if (artifact == arty++) { name = typeof(Artifact_TotemArms).Name; item = "Totem Arms"; }
			else if (artifact == arty++) { name = typeof(Artifact_TotemGloves).Name; item = "Totem Gloves"; }
			else if (artifact == arty++) { name = typeof(Artifact_TotemGorget).Name; item = "Totem Gorget"; }
			else if (artifact == arty++) { name = typeof(Artifact_TotemLeggings).Name; item = "Totem Leggings"; }
			else if (artifact == arty++) { name = typeof(Artifact_TotemOfVoid).Name; item = "Totem of the Void"; }
			else if (artifact == arty++) { name = typeof(Artifact_TotemTunic).Name; item = "Totem Tunic"; }
			else if (artifact == arty++) { name = typeof(Artifact_TunicOfAegis).Name; item = "Tunic of Aegis"; }
			else if (artifact == arty++) { name = typeof(Artifact_TunicOfBane).Name; item = "Tunic of Bane"; }
			else if (artifact == arty++) { name = typeof(Artifact_TunicOfFire).Name; item = "Tunic of Fire"; }
			else if (artifact == arty++) { name = typeof(Artifact_TunicOfTheFallenKing).Name; item = "Tunic of the Fallen King"; }
			else if (artifact == arty++) { name = typeof(Artifact_TunicOfTheHarrower).Name; item = "Tunic of the Harrower"; }
			else if (artifact == arty++) { name = typeof(Artifact_BelmontWhip).Name; item = "Vampire Killer"; }
			else if (artifact == arty++) { name = typeof(Artifact_VampiricDaisho).Name; item = "Vampiric Daisho"; }
			else if (artifact == arty++) { name = typeof(Artifact_VioletCourage).Name; item = "Violet Courage"; }
			else if (artifact == arty++) { name = typeof(Artifact_VoiceOfTheFallenKing).Name; item = "Voice of the Fallen King"; }
			else if (artifact == arty++) { name = typeof(Artifact_WarriorsClasp).Name; item = "Warrior's Clasp"; }
			else if (artifact == arty++) { name = typeof(Artifact_WildfireBow).Name; item = "Wildfire Bow"; }
			else if (artifact == arty++) { name = typeof(Artifact_Windsong).Name; item = "Windsong"; }
			else if (artifact == arty++) { name = typeof(Artifact_ArcticBeacon).Name; item = "Winter Beacon"; }
			else if (artifact == arty++) { name = typeof(Artifact_WizardsPants).Name; item = "Wizard's Pants"; }
			else if (artifact == arty++) { name = typeof(Artifact_WrathOfTheDryad).Name; item = "Wrath of the Dryad"; }
			else if (artifact == arty++) { name = typeof(Artifact_YashimotosHatsuburi).Name; item = "Yashimoto's Hatsuburi"; }
			else if (artifact == arty++) { name = typeof(Artifact_ZyronicClaw).Name; item = "Zyronic Claw"; }

			if (part == 2) { item = name; }

			return item;
		}
	}
}
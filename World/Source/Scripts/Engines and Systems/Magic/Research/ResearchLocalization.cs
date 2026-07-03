using System;
using System.Collections.Generic;
using Server.Localization;
using Server.Mobiles;

namespace Server.Misc
{
	public static class ResearchLocalization
	{
		private static readonly Dictionary<string, string> ReagentCatalogKeys = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase )
		{
			{ "Beetle Shell", "item.trade.name.reagent.beetle.shell" },
			{ "Bitter Root", "item.trade.name.reagent.bitter.root" },
			{ "Black Pearl", "item.trade.name.reagent.black.pearl" },
			{ "Black Sand", "item.trade.name.reagent.black.sand" },
			{ "Blood Rose", "item.trade.name.reagent.blood.rose" },
			{ "Brimstone", "item.trade.name.reagent.brimstone" },
			{ "Butterfly Wings", "item.trade.name.reagent.butterfly.wings" },
			{ "Dried Toad", "item.trade.name.reagent.dried.toad" },
			{ "Eye of Toad", "item.trade.name.reagent.eye.toad" },
			{ "Fairy Egg", "item.trade.name.reagent.fairy.egg" },
			{ "Gargoyle Ear", "item.trade.name.reagent.gargoyle.ear" },
			{ "Maggot", "item.trade.name.reagent.maggot" },
			{ "Moon Crystal", "item.trade.name.reagent.moon.crystal" },
			{ "Mummy Wrap", "item.trade.name.reagent.mummy.wrap" },
			{ "Pixie Skull", "item.trade.name.reagent.pixie.skull" },
			{ "Red Lotus", "item.trade.name.reagent.red.lotus" },
			{ "Sea Salt", "item.trade.name.reagent.sea.salt" },
			{ "Silver Widow", "item.trade.name.reagent.silver.widow" },
			{ "Swamp Berries", "item.trade.name.reagent.swamp.berries" },
			{ "Violet Fungus", "item.trade.name.reagent.violet.fungus" },
			{ "Werewolf Claw", "item.trade.name.reagent.werewolf.claw" },
			{ "Wolfsbane", "item.trade.name.reagent.wolfsbane" },
			{ "Bat Wing", "research.reagent.bat_wing" },
			{ "Blood Moss", "research.reagent.blood_moss" },
			{ "Bloodmoss", "research.reagent.bloodmoss" },
			{ "Daemon Blood", "research.reagent.daemon_blood" },
			{ "Garlic", "research.reagent.garlic" },
			{ "Ginseng", "research.reagent.ginseng" },
			{ "Grave Dust", "research.reagent.grave_dust" },
			{ "Mandrake Root", "research.reagent.mandrake_root" },
			{ "Nightshade", "research.reagent.nightshade" },
			{ "Nox Crystal", "research.reagent.nox_crystal" },
			{ "Pig Iron", "research.reagent.pig_iron" },
			{ "Spiders Silk", "research.reagent.spiders_silk" },
			{ "Sulfurous Ash", "research.reagent.sulfurous_ash" },
			{ "Silver Serpent Venom", "research.reagent.silver_serpent_venom" },
			{ "Dragon Blood", "research.reagent.dragon_blood" },
			{ "Enchanted Seaweed", "research.reagent.enchanted_seaweed" },
			{ "Dragon Tooth", "research.reagent.dragon_tooth" },
			{ "Golden Serpent Venom", "research.reagent.golden_serpent_venom" },
			{ "Lich Dust", "research.reagent.lich_dust" },
			{ "Demon Claw", "research.reagent.demon_claw" },
			{ "Pegasus Feather", "research.reagent.pegasus_feather" },
			{ "Phoenix Feather", "research.reagent.phoenix_feather" },
			{ "Unicorn Horn", "research.reagent.unicorn_horn" },
			{ "Demigod Blood", "research.reagent.demigod_blood" },
			{ "Ghostly Dust", "research.reagent.ghostly_dust" },
		};

		public static string Key( Mobile viewer, string logicalKey, string englishIfMissing )
		{
			string lang = AccountLang.GetLanguageCode( viewer != null ? viewer.Account : null );
			string s = StringCatalog.TryResolveByKey( lang, logicalKey );
			return !string.IsNullOrEmpty( s ) ? s : englishIfMissing;
		}

		public static string KeyFormat( Mobile viewer, string logicalKey, string englishIfMissing, params object[] args )
		{
			string fmt = Key( viewer, logicalKey, englishIfMissing );

			if ( args == null || args.Length == 0 )
				return fmt;

			try
			{
				return string.Format( fmt, args );
			}
			catch
			{
				return englishIfMissing;
			}
		}

		public static void Send( Mobile to, string logicalKey, string englishIfMissing )
		{
			to.SendMessage( Key( to, logicalKey, englishIfMissing ) );
		}

		public static void SendFormat( Mobile to, string logicalKey, string englishIfMissing, params object[] args )
		{
			to.SendMessage( KeyFormat( to, logicalKey, englishIfMissing, args ) );
		}

		public static string LandName( Mobile viewer, Land land )
		{
			string lang = AccountLang.GetLanguageCode( viewer != null ? viewer.Account : null );
			return Lands.LocalizedLandName( land, lang );
		}

		public static string GumpHtml( Mobile viewer, string logicalKey, string englishIfMissing )
		{
			return "<BODY><BASEFONT Color=#6cb89a>" + Key( viewer, logicalKey, englishIfMissing ) + "</BASEFONT></BODY>";
		}

		public static string GumpHtmlRaw( string text )
		{
			return "<BODY><BASEFONT Color=#6cb89a>" + text + "</BASEFONT></BODY>";
		}

		public static string GumpTitle( Mobile viewer, string englishTitle )
		{
			switch ( englishTitle )
			{
				case "MAIN PACK":
					return Key( viewer, "research.gump.title.main_pack", englishTitle );
				case "MAGERY RESEARCH":
					return Key( viewer, "research.gump.title.magery_research", englishTitle );
				case "NECROMANCY RESEARCH":
					return Key( viewer, "research.gump.title.necromancy_research", englishTitle );
				case "ANCIENT SPELL RESEARCH":
					return Key( viewer, "research.gump.title.ancient_spell_research", englishTitle );
				case "PREPARED ANCIENT SPELLS":
					return Key( viewer, "research.gump.title.prepared_ancient_spells", englishTitle );
				case "CUBES OF POWER":
					return Key( viewer, "research.gump.title.cubes_of_power", englishTitle );
				default:
					return Key( viewer, "research.gump.title.spell_research", englishTitle );
			}
		}

		public static string MageryCircle( Mobile viewer, int circle )
		{
			switch ( circle )
			{
				case 1: return Key( viewer, "research.gump.circle.1", "1st Circle" );
				case 2: return Key( viewer, "research.gump.circle.2", "2nd Circle" );
				case 3: return Key( viewer, "research.gump.circle.3", "3rd Circle" );
				case 4: return Key( viewer, "research.gump.circle.4", "4th Circle" );
				case 5: return Key( viewer, "research.gump.circle.5", "5th Circle" );
				case 6: return Key( viewer, "research.gump.circle.6", "6th Circle" );
				case 7: return Key( viewer, "research.gump.circle.7", "7th Circle" );
				case 8: return Key( viewer, "research.gump.circle.8", "8th Circle" );
				default: return circle + "th Circle";
			}
		}

		public static string AncientSchool( Mobile viewer, string schoolKey )
		{
			switch ( schoolKey )
			{
				case "conjuration":
					return Key( viewer, "research.gump.school.conjuration", "Conjuration" );
				case "death":
					return Key( viewer, "research.gump.school.death", "Death" );
				case "enchanting":
					return Key( viewer, "research.gump.school.enchanting", "Enchanting" );
				case "sorcery":
					return Key( viewer, "research.gump.school.sorcery", "Sorcery" );
				case "summoning":
					return Key( viewer, "research.gump.school.summoning", "Summoning" );
				case "thaumaturgy":
					return Key( viewer, "research.gump.school.thaumaturgy", "Thaumaturgy" );
				case "theurgy":
					return Key( viewer, "research.gump.school.theurgy", "Theurgy" );
				case "wizardry":
					return Key( viewer, "research.gump.school.wizardry", "Wizardry" );
				default:
					return schoolKey;
			}
		}

		public static string MageryCircleWord( Mobile viewer, string spellCircle )
		{
			switch ( spellCircle )
			{
				case "second": return MageryCircle( viewer, 2 );
				case "third": return MageryCircle( viewer, 3 );
				case "fourth": return MageryCircle( viewer, 4 );
				case "fifth": return MageryCircle( viewer, 5 );
				case "sixth": return MageryCircle( viewer, 6 );
				case "seventh": return MageryCircle( viewer, 7 );
				case "eighth": return MageryCircle( viewer, 8 );
				default: return MageryCircle( viewer, 1 );
			}
		}

		public static string BagOrBook( Mobile viewer )
		{
			return Key( viewer, "research.settings.bag_or_book",
				"Since you can cast these ancient spells with the contents of your research bag, or an ancient spellbook, you need to make a decision which option you are going to use. By default, magic is assumed to be unleashed by using the research bag. If you want to use an ancient spellbook instead, go into the HELP section of your PAPERDOLL. Choose the SETTINGS section where you can check the box for the ANCIENT SPELLBOOK. Uncheck the box if you wish to use the contents of your research bag instead." );
		}

		public static string BuildSpellResearchHelpHtml( Mobile viewer )
		{
			string lowreg = "";

			if ( MyServerSettings.LowerReg() >= 1 )
			{
				lowreg = Key( viewer, "research.gump.help.lowreg_scroll",
					"If one has attributes of lower reagent use, the scroll may not crumble and can be used again but that is not guaranteed especially for very powerful spells. " );
			}

			string body = Key( viewer, "research.gump.help.spell_research.body",
				"Spell research is something that the most dedicated of wizards pursue. It requires the accumulation of knowledge of those that came before. Mages and Necromancers once practiced this strange magic, that was thought to be lost through the passage of time. No matter the research goal, one would need to find the cubes of power in order to scribe spells. Once all of the cubes of power are found, the true research can begin.<BR><BR>Some perform research to write commonly used spells, but cannot find the original scroll that contains the vital information needed to cast it. This research has a scribe searching for the pages of tomes that contain the information to write the spell to parchment. Creating scrolls in this manner require the use of octopus ink, which is scarce since the last octopus was seen centuries ago as the kraken were said to have wiped them out. The more difficult the spell, the more ink that is required to scribe the scroll. Your research will indicate where more octopus ink can be obtained. The better your skills in alchemy, cooking, and tasting the more use you will find from the collected ink. Modern spells can be learned in the areas of magery and necromancy. You can research each area simultaneously if you choose, but each area will require you to learn the magic in a specific order as the learned spell will help the researcher learn more of the next spell until all are discovered.<BR><BR>The main goal of spell research is for a spell caster to learn the ancient magic that has been long forgotten. This ancient magic consists of 64 different spells in 8 different schools of magic. These spells were once used by mages and necromancers alike, where those that reached the status of archmage benefited the most from the power these spells unleash.<BR><BR>The magic can be scribed to individual scrolls that the caster can then store in their research bag and later read from. When read, the scroll crumples to dust. " )
				+ lowreg
				+ Key( viewer, "research.gump.help.spell_research.body2",
					"You can also create an ancient spellbook, for a more traditional way to travel and use your newly discovered spells.<BR><BR>This bag will hold all of your research, as well as blank scrolls, quills, and octopus ink for a maximum quantity of 50,000 each. You can drop scribe pens and blank scrolls onto the bag to replenish those materials. Discovered ink will be placed in your bag when found. It will also hold all of the cubes of power as well as any ancient magic you put to parchment. This bag will hold 500 scrolls of each ancient magic spell. The bag is yours and no one else can look inside it or use the magic you researched within it. If you lose the bag, you should find a scribe immediately and give them another 500 gold where maybe you will have your research returned. If not, you will have to begin your research all over again with an empty bag.<BR><BR>You can also create an ancient spellbook for easier travels. To create an ancient spellbook, you need to find any spellbook you wish to begin with. It can be books like a magery spellbook, a necromancy spellbook, or even a book of ninjitsu. Drop the book onto your research bag. The attributes of the book you start with, will also give your ancient spellbook those same characteristics. The book will automatically be scribed with all of the ancient magic you learned thus far. Whenever you learn new ancient spells, you can drop the book back onto the research bag to update its pages. Like the research bag, this book will be yours alone to use and you can only have one book at any given time. Creating a new book will cause any current books to crumple to dust. Using ancient spellbooks requires the caster to carry reagents with them. The book pages may also turn to dust when spells are cast, so the wizard will need to keep extra pages (blank scrolls) within the book. They will also need to keep quills (scribe pens) set aside as well. Simply place these items on the book in order to maintain a supply of each. If you run out, you will not be able to cast any spells until you acquire more. You might find that you don't consume any scrolls or quills when reagent lowering attributes exist. These books can be equipped like other spellbooks.<BR><BR>" )
				+ BagOrBook( viewer )
				+ Key( viewer, "research.gump.help.spell_research.body3",
					"<BR><BR>Spells are cast with those skilled in either magery or necromancy, whichever is higher. The effectiveness of the spells is dependent on the combination of magery, necromancy, spiritualism, and psychology. If you are simply skilled in only a couple of these skills, then the spells will have only an average effect. It is those that pursue all four of the skills of wizardry, that will gain the most benefit. When ancient spells are performed, it helps a researcher practice inscription, magery, necromancy, spiritualism, and psychology at the same time. This is why ancient spell research interests archmages, as they have achieved the level of grandmaster in both areas of magic. Some ancient magic has similarities to spells used today, as is to be expected that some of the knowledge survived the ages. So very few spells will be similar to current magery spells, and even fewer spells that are similar to modern necromancer spells. Although they are similar, the ancient spell usually proves to be much more powerful.<BR><BR>There are a few different ways to cast the ancient magic. The first is within the bag from the prepared spell section. There is an arrow icon next to each spell that can cast it for you. If you chose to create an ancient spellbook, you can cast spells from within that tome. Spell bars are also available for casting convenience, and they can be configured in the main section of this bag and only if you learned at least 1 of the ancient spells. You can also configure them in the HELP section. You are able to have 4 different spell bars for ancient magic, and you can customize each in a variety of ways. The last way to cast these spells is by a typed command, which allows you to make macros for spell casting if you want. Each of these commands are listed below:" )
				+ ResearchSettings.AncientKeywords();

			return "<BODY><BASEFONT Color=#6cb89a>" + body + "</BASEFONT></BODY>";
		}

		public static string BuildAncientResearchHelpHtml( Mobile viewer )
		{
			string lowreg = "";
			string lowreg2 = "";

			if ( MyServerSettings.LowerReg() >= 1 )
			{
				lowreg = Key( viewer, "research.gump.help.lowreg_scribe_ancient",
					"Lower reagent attributes do not work toward reagents needing to scribe these spells. " );
				lowreg2 = Key( viewer, "research.gump.help.lowreg_cast_ancient",
					"Those enhanced with lower reagent qualities, may be able to keep the scrolls from crumbling to dust upon casting but that is not guaranteed. " );
			}

			string body = Key( viewer, "research.gump.help.ancient.body",
				"Ancient spell research is done in a linear order from the least difficult spell to the most difficult where each school of magic is given a turn in the phases of discovery. This means that you will find the information for the easiest conjuration spell first. You will then need to find the information for the easiest death spell next. When you find the easiest spells for each of the 8 schools of magic, the rotation will begin again for the next least difficult spell for each school. This progression needs to be followed until all 64 spells are learned. The wizards that once used these spells died centuries ago, and the written tomes they possessed was lost throught the many lands. When you begin your search for these tomes, you will have a clue on where the first book is rumored to be. Once you find this book, you will learn the whereabouts to another book. As you find these tomes, the information on a particular spell will appear in your bag. Search the dungeon that the clue provides, and seek the runic pedestal with the small chest on top. If you search the contents, you may find the book you seek.<br><br>As you learn these spells, you can begin to scribe them if you have the skills, mana, and resources to do so. Once scribed, the scroll will remain in your bag until you choose to cast it. As you cast these spells, the scribed scrolls will be depleted. " )
				+ lowreg2
				+ Key( viewer, "research.gump.help.ancient.body2",
					"You can learn more about casting these spells on the main screen's Help section or the prepared spells section. Each spell listed has a button to attempt to scribe the scroll (arrow button) or a button that displays the information about the spell (scroll button). Look over the information to see what you will need to attempt to scribe the scroll. All scrolls require a certain inscription skill and mana to attempt, but also a blank scroll, quill, and reagents. You can also attempt to scribe the scroll from the information window by pressing the scroll button if you choose to only scribe a single scroll. You can also press the other button to scribe as many scrolls as you have reagents, quills, and blank scrolls. When you scribe many at once, you only need the required mana as though you were scribing a single scroll, but the resources multiply toward the total quantity that is to be created. " )
				+ lowreg
				+ Key( viewer, "research.gump.help.ancient.body3",
					"If you fail in your attempt, there is a chance that some of the materials will be lost.<br><br><br><br>" );

			return "<BODY><BASEFONT Color=#6cb89a>" + body + "</BASEFONT></BODY>";
		}

		public static string BuildPreparedSpellsHelpHtml( Mobile viewer )
		{
			string lowreg = "";

			if ( MyServerSettings.LowerReg() >= 1 )
			{
				lowreg = Key( viewer, "research.gump.help.lowreg_cast_ancient",
					"Those enhanced with lower reagent qualities, may be able to keep the scrolls from crumbling to dust upon casting but that is not guaranteed. " );
			}

			string body = Key( viewer, "research.gump.help.prepared.body",
				"As you learn these spells, you can begin to scribe them if you have the skills, mana, and resources to do so. Once scribed, the scroll will remain in your bag until you choose to cast it. As you cast these spells, the scribed scrolls will be depleted. " )
				+ lowreg
				+ Key( viewer, "research.gump.help.prepared.body2",
					"To cast a spell from this window, select the arrow button next to the spell icon. Each spell listed has a scroll button that displays the information about the spell. Look over the information to see what you will need to attempt to scribe the scroll. All scrolls require a certain inscription skill and mana to attempt, but also a blank scroll, quill, and reagents. You can attempt to scribe the scroll from the information window by pressing the scroll button if you choose to only scribe a single scroll. You can also press the other button to scribe as many scrolls as you have reagents, quills, and blank scrolls. When you scribe many at once, you only need the required mana as though you were scribing a single scroll, but the resources multiply toward the total quantity that is to be created. Lower reagent attributes do not work toward reagents needing to scribe these spells. If you fail in your attempt, there is a chance that some of the materials will be lost." );

			return "<BODY><BASEFONT Color=#6cb89a>" + body + "</BASEFONT></BODY>";
		}

		public static string LocalizeReagentName( Mobile viewer, string english )
		{
			if ( string.IsNullOrEmpty( english ) )
				return english;

			string trimmed = english.Trim();

			if ( ReagentCatalogKeys.TryGetValue( trimmed, out string shotkey ) )
			{
				string localized = Key( viewer, shotkey, trimmed );

				if ( !string.IsNullOrEmpty( localized ) && localized != shotkey )
					return localized;
			}

			return trimmed;
		}

		public static string LocalizeReagentList( Mobile viewer, string englishCsv )
		{
			if ( string.IsNullOrEmpty( englishCsv ) )
				return englishCsv;

			string[] parts = englishCsv.Split( new[] { ", " }, StringSplitOptions.None );

			for ( int i = 0; i < parts.Length; i++ )
				parts[i] = LocalizeReagentName( viewer, parts[i] );

			return string.Join( ", ", parts );
		}

		public static string FormatAncientSpellSlice( Mobile viewer, int index, int slice, string raw )
		{
			switch ( slice )
			{
				case 2:
					return Key( viewer, string.Format( "research.ancient.{0:000}.name", index ), raw );
				case 3:
					return AncientSchool( viewer, raw.ToLower() );
				case 5:
					return LocalizeReagentList( viewer, raw );
				case 6:
					return LocalizeAncientDescription( viewer, index, raw );
				default:
					return raw;
			}
		}

		public static string LocalizeAncientDescription( Mobile viewer, int index, string englishDesc )
		{
			string desc = Key( viewer, string.Format( "research.ancient.{0:000}.description", index ), englishDesc );

			desc = desc.Replace(
				"That means that the spell scroll will always crumble to dust when cast",
				Key( viewer, "research.gump.ancientbook.desc.consume_reagents_scroll",
					"That means that the spell will always consume the reagents when cast" ) );

			desc = desc.Replace(
				"spell is powerful enough that the scroll will always crumble to dust when cast",
				Key( viewer, "research.gump.ancientbook.desc.consume_reagents_powerful",
					"spell is powerful enough that it will always consume the reagents when cast" ) );

			return desc;
		}

		public static string AncientSpellbookHeader( Mobile viewer, int page, int entry )
		{
			if ( ( page == 1 && entry == 1 ) || ( page >= 5 && page <= 8 ) )
				return Key( viewer, "research.gump.ancientbook.header.conjuration", "CONJURATION MAGIC" );

			if ( ( page == 1 && entry == 2 ) || ( page >= 9 && page <= 12 ) )
				return Key( viewer, "research.gump.ancientbook.header.death", "DEATH MAGIC" );

			if ( ( page == 2 && entry == 1 ) || ( page >= 13 && page <= 16 ) )
				return Key( viewer, "research.gump.ancientbook.header.enchanting", "ENCHANTING MAGIC" );

			if ( ( page == 2 && entry == 2 ) || ( page >= 17 && page <= 20 ) )
				return Key( viewer, "research.gump.ancientbook.header.sorcery", "SORCERY MAGIC" );

			if ( ( page == 3 && entry == 1 ) || ( page >= 21 && page <= 24 ) )
				return Key( viewer, "research.gump.ancientbook.header.summoning", "SUMMONING MAGIC" );

			if ( ( page == 3 && entry == 2 ) || ( page >= 25 && page <= 28 ) )
				return Key( viewer, "research.gump.ancientbook.header.thaumaturgy", "THAUMATURGY MAGIC" );

			if ( ( page == 4 && entry == 1 ) || ( page >= 29 && page <= 32 ) )
				return Key( viewer, "research.gump.ancientbook.header.theurgy", "THEURGY MAGIC" );

			if ( ( page == 4 && entry == 2 ) || ( page >= 33 && page <= 36 ) )
				return Key( viewer, "research.gump.ancientbook.header.wizardry", "WIZARDRY MAGIC" );

			return Key( viewer, "research.gump.ancientbook.header.ancient", "ANCIENT MAGIC" );
		}

		public static string BuildAncientSpellbookHelpLeftHtml( Mobile viewer )
		{
			return Key( viewer, "research.gump.ancientbook.help.ownership",
				"This book will be yours alone to use and you can only have one book at any given time. Creating a new book will cause this book to crumple to dust. Using ancient spellbooks requires the caster to carry reagents with them. The book pages may also turn to dust when spells are cast, so the wizard will need to keep extra pages (blank scrolls) within the book. They will also need to keep quills (scribe pens) set aside as well. Simply place these items on this book in order to maintain a supply of each. If you run out, you will not be able to cast any spells until you acquire more. You might find that you don't consume any scrolls or quills when reagent lowering attributes exist. These books can be equipped like other spellbooks.<BR><BR>" )
				+ BagOrBook( viewer )
				+ Key( viewer, "research.gump.ancientbook.help.casting_skills",
					"<BR><BR>Spells are cast with those skilled in either magery or necromancy, whichever is higher. The effectiveness of the spells is dependent on the combination of magery, necromancy, spiritualism, and psychology. If you are simply skilled in only a couple of these skills, then the spells will have only an average effect. It is those that pursue all four of the skills of wizardry, that will gain the most benefit. When ancient spells are performed, it helps a researcher practice inscription, magery, necromancy, spiritualism, and psychology at the same time. This is why ancient spell research interests archmages, as they have achieved the level of grandmaster in both areas of magic. Some ancient magic has similarities to spells used today, as is to be expected that some of the knowledge survived the ages. So very few spells will be similar to current magery spells, and even fewer spells that are similar to modern necromancer spells. Although they are similar, the ancient spell usually proves to be much more powerful." );
		}

		public static string BuildAncientSpellbookHelpRightHtml( Mobile viewer )
		{
			return Key( viewer, "research.gump.ancientbook.help.toolbar",
				"Magic Toolbars: Here are the commands you can use (include the bracket) to manage magic toolbars that might help you play better.<BR><BR>[ancient - Switches between using the book or bag.<BR><BR>[archspell1 - Opens the 1st ancient spell bar editor.<BR><BR>[archspell2 - Opens the 2nd ancient spell bar editor.<BR><BR>[archspell3 - Opens the 3rd ancient spell bar editor.<BR><BR>[archspell4 - Opens the 4th ancient spell bar editor.<BR><BR>[archtool1 - Opens the 1st ancient spell bar.<BR><BR>[archtool2 - Opens the 2nd ancient spell bar.<BR><BR>[archtool3 - Opens the 3rd ancient spell bar.<BR><BR>[archtool4 - Opens the 4th ancient spell bar.<BR><BR>[archclose1 - Closes the 1st ancient spell bar.<BR><BR>[archclose2 - Closes the 2nd ancient spell bar.<BR><BR>[archclose3 - Closes the 3rd ancient spell bar.<BR><BR>[archclose4 - Closes the 4th ancient spell bar.<BR><BR>Below are the [ commands you can either type to quickly cast a particular spell, or set a hot key to issue this command and cast the spell." )
				+ ResearchSettings.AncientKeywords();
		}
	}
}

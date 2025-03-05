using System;
using Server; 
using Server.Network;
using System.Collections;
using System.Globalization;
using Server.Items;
using Server.Misc;
using Server.Gumps;

namespace Server.Items
{
	public class LegendsBook : Item
	{
		public const int NUMBER_OF_ARTIFACTS = 290;

		[Constructable]
		public LegendsBook() : base( 0x22C5 )
		{
			Weight = 1.0;
			Movable = false;
			Hue = 0xB93;
			Name = "Legendary Artefacts";
		}

		public override void OnDoubleClick( Mobile from )
		{
			from.CloseGump( typeof( LegendsBookGump ) );
			from.SendGump( new LegendsBookGump( from, this, 0 ) );
		}

		public class LegendsBookGump : Gump
		{
			private LegendsBook m_Book;

			public LegendsBookGump( Mobile from, LegendsBook wikipedia, int page ): base( 100, 100 )
			{
				from.SendSound( 0x55 );
				string color = "#81db9f";

				m_Book = wikipedia;
				LegendsBook pedia = (LegendsBook)wikipedia;

				decimal PageCount = NUMBER_OF_ARTIFACTS / 16;
				int TotalBookPages = ( 100000 ) + ( (int)Math.Ceiling( PageCount ) );

				this.Closable=true;
				this.Disposable=true;
				this.Dragable=true;
				this.Resizable=false;

				AddPage(0);

				AddImage(0, 0, 7005, 2964);
				AddImage(0, 0, 7006);
				AddImage(0, 0, 7024, 2736);
				AddButton(590, 48, 4017, 4017, 0, GumpButtonType.Reply, 0);

				AddHtml( 77, 49, 259, 20, @"<BODY><BASEFONT Color=" + color + "><CENTER>LEGENDARY ARTIFACTS</CENTER></BASEFONT></BODY>", (bool)false, (bool)false);

				int subItem = page * 16;

				int showItem1 = subItem + 1;
				int showItem2 = subItem + 2;
				int showItem3 = subItem + 3;
				int showItem4 = subItem + 4;
				int showItem5 = subItem + 5;
				int showItem6 = subItem + 6;
				int showItem7 = subItem + 7;
				int showItem8 = subItem + 8;
				int showItem9 = subItem + 9;
				int showItem10 = subItem + 10;
				int showItem11 = subItem + 11;
				int showItem12 = subItem + 12;
				int showItem13 = subItem + 13;
				int showItem14 = subItem + 14;
				int showItem15 = subItem + 15;
				int showItem16 = subItem + 16;

				int page_prev = ( 100000 + page ) - 1;
					if ( page_prev < 100000 ){ page_prev = TotalBookPages; }
				int page_next = ( 100000 + page ) + 1;
					if ( page_next > TotalBookPages ){ page_next = 100000; }

				AddButton(75, 374, 4014, 4014, page_prev, GumpButtonType.Reply, 0);
				AddButton(590, 375, 4005, 4005, page_next, GumpButtonType.Reply, 0);

				///////////////////////////////////////////////////////////////////////////////////

				int x = 115;
				int y = 64;
				int s = 64;
				int z = 34;

				y=y+z;
				if ( GetLegendArtyForBook( showItem1, 1 ) != "" ){ AddButton(x, y, 2447, 2447, showItem1, GumpButtonType.Reply, 0); } y=y+z;
				if ( GetLegendArtyForBook( showItem2, 1 ) != "" ){ AddButton(x, y, 2447, 2447, showItem2, GumpButtonType.Reply, 0); } y=y+z;
				if ( GetLegendArtyForBook( showItem3, 1 ) != "" ){ AddButton(x, y, 2447, 2447, showItem3, GumpButtonType.Reply, 0); } y=y+z;
				if ( GetLegendArtyForBook( showItem4, 1 ) != "" ){ AddButton(x, y, 2447, 2447, showItem4, GumpButtonType.Reply, 0); } y=y+z;
				if ( GetLegendArtyForBook( showItem5, 1 ) != "" ){ AddButton(x, y, 2447, 2447, showItem5, GumpButtonType.Reply, 0); } y=y+z;
				if ( GetLegendArtyForBook( showItem6, 1 ) != "" ){ AddButton(x, y, 2447, 2447, showItem6, GumpButtonType.Reply, 0); } y=y+z;
				if ( GetLegendArtyForBook( showItem7, 1 ) != "" ){ AddButton(x, y, 2447, 2447, showItem7, GumpButtonType.Reply, 0); } y=y+z;
				if ( GetLegendArtyForBook( showItem8, 1 ) != "" ){ AddButton(x, y, 2447, 2447, showItem8, GumpButtonType.Reply, 0); } y=s-3;
				y=y+z;
				AddHtml( x+20, y, 155, 20, @"<BODY><BASEFONT Color=" + color + ">" + GetLegendArtyForBook( showItem1, 1 ) + "</BASEFONT></BODY>", (bool)false, (bool)false); y=y+z;
				AddHtml( x+20, y, 155, 20, @"<BODY><BASEFONT Color=" + color + ">" + GetLegendArtyForBook( showItem2, 1 ) + "</BASEFONT></BODY>", (bool)false, (bool)false); y=y+z;
				AddHtml( x+20, y, 155, 20, @"<BODY><BASEFONT Color=" + color + ">" + GetLegendArtyForBook( showItem3, 1 ) + "</BASEFONT></BODY>", (bool)false, (bool)false); y=y+z;
				AddHtml( x+20, y, 155, 20, @"<BODY><BASEFONT Color=" + color + ">" + GetLegendArtyForBook( showItem4, 1 ) + "</BASEFONT></BODY>", (bool)false, (bool)false); y=y+z;
				AddHtml( x+20, y, 155, 20, @"<BODY><BASEFONT Color=" + color + ">" + GetLegendArtyForBook( showItem5, 1 ) + "</BASEFONT></BODY>", (bool)false, (bool)false); y=y+z;
				AddHtml( x+20, y, 155, 20, @"<BODY><BASEFONT Color=" + color + ">" + GetLegendArtyForBook( showItem6, 1 ) + "</BASEFONT></BODY>", (bool)false, (bool)false); y=y+z;
				AddHtml( x+20, y, 155, 20, @"<BODY><BASEFONT Color=" + color + ">" + GetLegendArtyForBook( showItem7, 1 ) + "</BASEFONT></BODY>", (bool)false, (bool)false); y=y+z;
				AddHtml( x+20, y, 155, 20, @"<BODY><BASEFONT Color=" + color + ">" + GetLegendArtyForBook( showItem8, 1 ) + "</BASEFONT></BODY>", (bool)false, (bool)false); y=s-3;

				///////////////////////////////////////////////////////////////////////////////////

				x = 407;
				y = s;

				y=y+z;
				if ( GetLegendArtyForBook( showItem9, 1 ) != "" ){ AddButton(x, y, 2447, 2447, showItem9, GumpButtonType.Reply, 0); } y=y+z;
				if ( GetLegendArtyForBook( showItem10, 1 ) != "" ){ AddButton(x, y, 2447, 2447, showItem10, GumpButtonType.Reply, 0); } y=y+z;
				if ( GetLegendArtyForBook( showItem11, 1 ) != "" ){ AddButton(x, y, 2447, 2447, showItem11, GumpButtonType.Reply, 0); } y=y+z;
				if ( GetLegendArtyForBook( showItem12, 1 ) != "" ){ AddButton(x, y, 2447, 2447, showItem12, GumpButtonType.Reply, 0); } y=y+z;
				if ( GetLegendArtyForBook( showItem13, 1 ) != "" ){ AddButton(x, y, 2447, 2447, showItem13, GumpButtonType.Reply, 0); } y=y+z;
				if ( GetLegendArtyForBook( showItem14, 1 ) != "" ){ AddButton(x, y, 2447, 2447, showItem14, GumpButtonType.Reply, 0); } y=y+z;
				if ( GetLegendArtyForBook( showItem15, 1 ) != "" ){ AddButton(x, y, 2447, 2447, showItem15, GumpButtonType.Reply, 0); } y=y+z;
				if ( GetLegendArtyForBook( showItem16, 1 ) != "" ){ AddButton(x, y, 2447, 2447, showItem16, GumpButtonType.Reply, 0); } y=s-3;
				y=y+z;
				AddHtml( x+20, y, 155, 20, @"<BODY><BASEFONT Color=" + color + ">" + GetLegendArtyForBook( showItem9, 1 ) + "</BASEFONT></BODY>", (bool)false, (bool)false); y=y+z;
				AddHtml( x+20, y, 155, 20, @"<BODY><BASEFONT Color=" + color + ">" + GetLegendArtyForBook( showItem10, 1 ) + "</BASEFONT></BODY>", (bool)false, (bool)false); y=y+z;
				AddHtml( x+20, y, 155, 20, @"<BODY><BASEFONT Color=" + color + ">" + GetLegendArtyForBook( showItem11, 1 ) + "</BASEFONT></BODY>", (bool)false, (bool)false); y=y+z;
				AddHtml( x+20, y, 155, 20, @"<BODY><BASEFONT Color=" + color + ">" + GetLegendArtyForBook( showItem12, 1 ) + "</BASEFONT></BODY>", (bool)false, (bool)false); y=y+z;
				AddHtml( x+20, y, 155, 20, @"<BODY><BASEFONT Color=" + color + ">" + GetLegendArtyForBook( showItem13, 1 ) + "</BASEFONT></BODY>", (bool)false, (bool)false); y=y+z;
				AddHtml( x+20, y, 155, 20, @"<BODY><BASEFONT Color=" + color + ">" + GetLegendArtyForBook( showItem14, 1 ) + "</BASEFONT></BODY>", (bool)false, (bool)false); y=y+z;
				AddHtml( x+20, y, 155, 20, @"<BODY><BASEFONT Color=" + color + ">" + GetLegendArtyForBook( showItem15, 1 ) + "</BASEFONT></BODY>", (bool)false, (bool)false); y=y+z;
				AddHtml( x+20, y, 155, 20, @"<BODY><BASEFONT Color=" + color + ">" + GetLegendArtyForBook( showItem16, 1 ) + "</BASEFONT></BODY>", (bool)false, (bool)false); y=s-3;
			}

			public override void OnResponse( NetState state, RelayInfo info )
			{
				Mobile from = state.Mobile; 
				Container pack = from.Backpack;
				from.SendSound( 0x55 );

				bool canChoose = false;

				int karma = from.Karma;
					if ( karma < 0 ){ karma = -1 * karma; }

				int fame = from.Fame;

				if ( fame >= 15000 && karma >= 15000 && from.TotalGold >= 10000 ){ canChoose = true; }

				if ( info.ButtonID >= 100000 )
				{
					int page = info.ButtonID - 100000;
					from.SendGump( new LegendsBookGump( from, m_Book, page ) );
				}
				else if ( canChoose == true && info.ButtonID > 0 && info.ButtonID < 300 && pack.ConsumeTotal(typeof(Gold), 10000))
				{
					string sType = GetLegendArtyForBook( info.ButtonID, 2 );
					string sName = GetLegendArtyForBook( info.ButtonID, 1 );
						if ( sName == "Talisman, Holy" ){ sName = "Talisman"; }
						if ( sName == "Talisman, Snake" ){ sName = "Talisman"; }
						if ( sName == "Talisman, Totem" ){ sName = "Talisman"; }
					string sArty = ArtyItemName( sName, from );

					if ( sName != "" )
					{
						Item reward = null;
						Type itemType = ScriptCompiler.FindTypeByName( sType );
						from.Fame = 0;
						from.Karma = 0;
						reward = (Item)Activator.CreateInstance(itemType);
						reward.Name = sArty;
						from.AddToBackpack ( reward );
						LoggingFunctions.LogCreatedArtifact( from, sArty );
						from.SendMessage( "The gods have created a legendary artefact called " + sArty + ".");
						from.FixedParticles( 0x3709, 10, 30, 5052, 0x480, 0, EffectLayer.LeftFoot );
						from.PlaySound( 0x208 );
					}
				}
				else if ( from.TotalGold < 10000 )
				{
					from.SendMessage( "You do not have enough gold for tribute.");
				}
				else
				{
					from.SendMessage( "You are not legendary enough to summon the artifact.");
				}
			}
		}

		public LegendsBook( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int)1 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}

		public static string GetLegendArtyForBook( int artifact, int part )
		{
			string item = "";
			string name = "";
			int arty = 1;

			if ( artifact == arty) { name = typeof(LevelBascinet).Name; item="Bascinet"; } arty++;
			if ( artifact == arty) { name = typeof(LevelBoneArms).Name; item="Bone Arms"; } arty++;
			if ( artifact == arty) { name = typeof(LevelBoneChest).Name; item="Bone Chest"; } arty++;
			if ( artifact == arty) { name = typeof(LevelBoneGloves).Name; item="Bone Gloves"; } arty++;
			if ( artifact == arty) { name = typeof(LevelBoneHelm).Name; item="Bone Helm"; } arty++;
			if ( artifact == arty) { name = typeof(LevelBoneLegs).Name; item="Bone Legs"; } arty++;
			if ( artifact == arty) { name = typeof(LevelBuckler).Name; item="Buckler"; } arty++;
			if ( artifact == arty) { name = typeof(LevelChainChest).Name; item="Chain Chest"; } arty++;
			if ( artifact == arty) { name = typeof(LevelChainCoif).Name; item="Chain Coif"; } arty++;
			if ( artifact == arty) { name = typeof(LevelChainHatsuburi).Name; item="Chain Hatsuburi"; } arty++;
			if ( artifact == arty) { name = typeof(LevelChainLegs).Name; item="Chain Legs"; } arty++;
			if ( artifact == arty) { name = typeof(LevelChaosShield).Name; item="Chaos Shield"; } arty++;
			if ( artifact == arty) { name = typeof(LevelCirclet).Name; item="Circlet"; } arty++;
			if ( artifact == arty) { name = typeof(LevelCloseHelm).Name; item="Close Helm"; } arty++;
			if ( artifact == arty) { name = typeof(LevelDarkShield).Name; item="Dark Shield"; } arty++;
			if ( artifact == arty) { name = typeof(LevelDecorativePlateKabuto).Name; item="Decorative Plate Kabuto"; } arty++;
			if ( artifact == arty) { name = typeof(LevelDreadHelm).Name; item="Dread Helm"; } arty++;
			if ( artifact == arty) { name = typeof(LevelElvenShield).Name; item="Elven Shield"; } arty++;
			if ( artifact == arty) { name = typeof(LevelFemaleLeatherChest).Name; item="Female Leather Chest"; } arty++;
			if ( artifact == arty) { name = typeof(LevelFemalePlateChest).Name; item="Female Plate Chest"; } arty++;
			if ( artifact == arty) { name = typeof(LevelFemaleStuddedChest).Name; item="Female Studded Chest"; } arty++;
			if ( artifact == arty) { name = typeof(LevelGuardsmanShield).Name; item="Guardsman Shield"; } arty++;
			if ( artifact == arty) { name = typeof(LevelHeaterShield).Name; item="Heater Shield"; } arty++;
			if ( artifact == arty) { name = typeof(LevelHeavyPlateJingasa).Name; item="Heavy Plate Jingasa"; } arty++;
			if ( artifact == arty) { name = typeof(LevelHelmet).Name; item="Helmet"; } arty++;
			if ( artifact == arty) { name = typeof(LevelOrcHelm).Name; item="Horned Helm"; } arty++;
			if ( artifact == arty) { name = typeof(LevelJeweledShield).Name; item="Jeweled Shield"; } arty++;
			if ( artifact == arty) { name = typeof(LevelBronzeShield).Name; item="Large Shield"; } arty++;
			if ( artifact == arty) { name = typeof(LevelLeatherArms).Name; item="Leather Arms"; } arty++;
			if ( artifact == arty) { name = typeof(LevelLeatherBustierArms).Name; item="Leather Bustier Arms"; } arty++;
			if ( artifact == arty) { name = typeof(LevelLeatherCap).Name; item="Leather Cap"; } arty++;
			if ( artifact == arty) { name = typeof(LevelLeatherChest).Name; item="Leather Chest"; } arty++;
			if ( artifact == arty) { name = typeof(LevelLeatherCloak).Name; item="Leather Cloak"; } arty++;
			if ( artifact == arty) { name = typeof(LevelLeatherDo).Name; item="Leather Do"; } arty++;
			if ( artifact == arty) { name = typeof(LevelLeatherGloves).Name; item="Leather Gloves"; } arty++;
			if ( artifact == arty) { name = typeof(LevelLeatherGorget).Name; item="Leather Gorget"; } arty++;
			if ( artifact == arty) { name = typeof(LevelLeatherHaidate).Name; item="Leather Haidate"; } arty++;
			if ( artifact == arty) { name = typeof(LevelLeatherHiroSode).Name; item="Leather HiroSode"; } arty++;
			if ( artifact == arty) { name = typeof(LevelLeatherJingasa).Name; item="Leather Jingasa"; } arty++;
			if ( artifact == arty) { name = typeof(LevelLeatherLegs).Name; item="Leather Legs"; } arty++;
			if ( artifact == arty) { name = typeof(LevelLeatherMempo).Name; item="Leather Mempo"; } arty++;
			if ( artifact == arty) { name = typeof(LevelLeatherNinjaHood).Name; item="Leather Ninja Hood"; } arty++;
			if ( artifact == arty) { name = typeof(LevelLeatherNinjaJacket).Name; item="Leather Ninja Jacket"; } arty++;
			if ( artifact == arty) { name = typeof(LevelLeatherNinjaMitts).Name; item="Leather Ninja Mitts"; } arty++;
			if ( artifact == arty) { name = typeof(LevelLeatherNinjaPants).Name; item="Leather Ninja Pants"; } arty++;
			if ( artifact == arty) { name = typeof(LevelLeatherRobe).Name; item="Leather Robe"; } arty++;
			if ( artifact == arty) { name = typeof(LevelLeatherShorts).Name; item="Leather Shorts"; } arty++;
			if ( artifact == arty) { name = typeof(LevelLeatherSkirt).Name; item="Leather Skirt"; } arty++;
			if ( artifact == arty) { name = typeof(LevelLeatherSuneate).Name; item="Leather Suneate"; } arty++;
			if ( artifact == arty) { name = typeof(LevelLightPlateJingasa).Name; item="Light Plate Jingasa"; } arty++;
			if ( artifact == arty) { name = typeof(LevelMetalKiteShield).Name; item="Metal Kite Shield"; } arty++;
			if ( artifact == arty) { name = typeof(LevelMetalShield).Name; item="Metal Shield"; } arty++;
			if ( artifact == arty) { name = typeof(LevelNorseHelm).Name; item="Norse Helm"; } arty++;
			if ( artifact == arty) { name = typeof(LevelOniwabanBoots).Name; item="Oniwaban Boots"; } arty++;
			if ( artifact == arty) { name = typeof(LevelOniwabanGloves).Name; item="Oniwaban Gloves"; } arty++;
			if ( artifact == arty) { name = typeof(LevelOniwabanHood).Name; item="Oniwaban Hood"; } arty++;
			if ( artifact == arty) { name = typeof(LevelOniwabanLeggings).Name; item="Oniwaban Leggings"; } arty++;
			if ( artifact == arty) { name = typeof(LevelOniwabanTunic).Name; item="Oniwaban Tunic"; } arty++;
			if ( artifact == arty) { name = typeof(LevelOrderShield).Name; item="Order Shield"; } arty++;
			if ( artifact == arty) { name = typeof(LevelPlateArms).Name; item="Plate Arms"; } arty++;
			if ( artifact == arty) { name = typeof(LevelPlateBattleKabuto).Name; item="Plate Battle Kabuto"; } arty++;
			if ( artifact == arty) { name = typeof(LevelPlateChest).Name; item="Plate Chest"; } arty++;
			if ( artifact == arty) { name = typeof(LevelPlateDo).Name; item="Plate Do"; } arty++;
			if ( artifact == arty) { name = typeof(LevelPlateGloves).Name; item="Plate Gloves"; } arty++;
			if ( artifact == arty) { name = typeof(LevelPlateGorget).Name; item="Plate Gorget"; } arty++;
			if ( artifact == arty) { name = typeof(LevelPlateHaidate).Name; item="Plate Haidate"; } arty++;
			if ( artifact == arty) { name = typeof(LevelPlateHatsuburi).Name; item="Plate Hatsuburi"; } arty++;
			if ( artifact == arty) { name = typeof(LevelPlateHelm).Name; item="Plate Helm"; } arty++;
			if ( artifact == arty) { name = typeof(LevelPlateHiroSode).Name; item="Plate Hiro Sode"; } arty++;
			if ( artifact == arty) { name = typeof(LevelPlateLegs).Name; item="Plate Legs"; } arty++;
			if ( artifact == arty) { name = typeof(LevelPlateMempo).Name; item="Plate Mempo"; } arty++;
			if ( artifact == arty) { name = typeof(LevelPlateSuneate).Name; item="Plate Suneate"; } arty++;
			if ( artifact == arty) { name = typeof(LevelRingmailArms).Name; item="Ringmail Arms"; } arty++;
			if ( artifact == arty) { name = typeof(LevelRingmailChest).Name; item="Ringmail Chest"; } arty++;
			if ( artifact == arty) { name = typeof(LevelRingmailGloves).Name; item="Ringmail Gloves"; } arty++;
			if ( artifact == arty) { name = typeof(LevelRingmailLegs).Name; item="Ringmail Legs"; } arty++;
			if ( artifact == arty) { name = typeof(LevelRoyalArms).Name; item="Royal Arms"; } arty++;
			if ( artifact == arty) { name = typeof(LevelRoyalBoots).Name; item="Royal Boots"; } arty++;
			if ( artifact == arty) { name = typeof(LevelRoyalChest).Name; item="Royal Chest"; } arty++;
			if ( artifact == arty) { name = typeof(LevelRoyalGloves).Name; item="Royal Gloves"; } arty++;
			if ( artifact == arty) { name = typeof(LevelRoyalGorget).Name; item="Royal Gorget"; } arty++;
			if ( artifact == arty) { name = typeof(LevelRoyalHelm).Name; item="Royal Helm"; } arty++;
			if ( artifact == arty) { name = typeof(LevelRoyalsLegs).Name; item="Royal Legs"; } arty++;
			if ( artifact == arty) { name = typeof(LevelDragonArms).Name; item="Scalemail Arms"; } arty++;
			if ( artifact == arty) { name = typeof(LevelDragonGloves).Name; item="Scalemail Gloves"; } arty++;
			if ( artifact == arty) { name = typeof(LevelDragonHelm).Name; item="Scalemail Helm"; } arty++;
			if ( artifact == arty) { name = typeof(LevelDragonLegs).Name; item="Scalemail Leggings"; } arty++;
			if ( artifact == arty) { name = typeof(LevelScalemailShield).Name; item="Scalemail Shield"; } arty++;
			if ( artifact == arty) { name = typeof(LevelDragonChest).Name; item="Scalemail Tunic"; } arty++;
			if ( artifact == arty) { name = typeof(LevelRoyalShield).Name; item="Royal Shield"; } arty++;
			if ( artifact == arty) { name = typeof(LevelShinobiCowl).Name; item="Leather Shinobi Cowl"; } arty++;
			if ( artifact == arty) { name = typeof(LevelShinobiHood).Name; item="Leather Shinobi Hood"; } arty++;
			if ( artifact == arty) { name = typeof(LevelShinobiMask).Name; item="Leather Shinobi Mask"; } arty++;
			if ( artifact == arty) { name = typeof(LevelShinobiRobe).Name; item="Leather Shinobi Robe"; } arty++;
			if ( artifact == arty) { name = typeof(LevelSmallPlateJingasa).Name; item="Small Plate Jingasa"; } arty++;
			if ( artifact == arty) { name = typeof(LevelStandardPlateKabuto).Name; item="Standard Plate Kabuto"; } arty++;
			if ( artifact == arty) { name = typeof(LevelStuddedArms).Name; item="Studded Arms"; } arty++;
			if ( artifact == arty) { name = typeof(LevelStuddedBustierArms).Name; item="Studded Bustier Arms"; } arty++;
			if ( artifact == arty) { name = typeof(LevelStuddedChest).Name; item="Studded Chest"; } arty++;
			if ( artifact == arty) { name = typeof(LevelStuddedDo).Name; item="Studded Do"; } arty++;
			if ( artifact == arty) { name = typeof(LevelStuddedGloves).Name; item="Studded Gloves"; } arty++;
			if ( artifact == arty) { name = typeof(LevelStuddedGorget).Name; item="Studded Gorget"; } arty++;
			if ( artifact == arty) { name = typeof(LevelStuddedHaidate).Name; item="Studded Haidate"; } arty++;
			if ( artifact == arty) { name = typeof(LevelStuddedHiroSode).Name; item="Studded Hiro Sode"; } arty++;
			if ( artifact == arty) { name = typeof(LevelStuddedLegs).Name; item="Studded Legs"; } arty++;
			if ( artifact == arty) { name = typeof(LevelStuddedMempo).Name; item="Studded Mempo"; } arty++;
			if ( artifact == arty) { name = typeof(LevelStuddedSuneate).Name; item="Studded Suneate"; } arty++;
			if ( artifact == arty) { name = typeof(LevelSunShield).Name; item="Sun Shield"; } arty++;
			if ( artifact == arty) { name = typeof(LevelVirtueShield).Name; item="Virtue Shield"; } arty++;
			if ( artifact == arty) { name = typeof(LevelWoodenKiteShield).Name; item="Wooden Kite Shield"; } arty++;
			if ( artifact == arty) { name = typeof(LevelWoodenPlateArms).Name; item="Wooden Plate Arms"; } arty++;
			if ( artifact == arty) { name = typeof(LevelWoodenPlateChest).Name; item="Wooden Plate Chest"; } arty++;
			if ( artifact == arty) { name = typeof(LevelWoodenPlateGloves).Name; item="Wooden Plate Gloves"; } arty++;
			if ( artifact == arty) { name = typeof(LevelWoodenPlateGorget).Name; item="Wooden Plate Gorget"; } arty++;
			if ( artifact == arty) { name = typeof(LevelWoodenPlateHelm).Name; item="Wooden Plate Helm"; } arty++;
			if ( artifact == arty) { name = typeof(LevelWoodenPlateLegs).Name; item="Wooden Plate Legs"; } arty++;
			if ( artifact == arty) { name = typeof(LevelWoodenShield).Name; item="Wooden Shield"; } arty++;
			if ( artifact == arty) { name = typeof(LevelAssassinSpike).Name; item="Assassin Dagger"; } arty++;
			if ( artifact == arty) { name = typeof(LevelElvenSpellblade).Name; item="Assassin Sword"; } arty++;
			if ( artifact == arty) { name = typeof(LevelAxe).Name; item="Axe"; } arty++;
			if ( artifact == arty) { name = typeof(LevelOrnateAxe).Name; item="Barbarian Axe"; } arty++;
			if ( artifact == arty) { name = typeof(LevelVikingSword).Name; item="Barbarian Sword"; } arty++;
			if ( artifact == arty) { name = typeof(LevelBardiche).Name; item="Bardiche"; } arty++;
			if ( artifact == arty) { name = typeof(LevelBattleAxe).Name; item="Battle Axe"; } arty++;
			if ( artifact == arty) { name = typeof(LevelDiamondMace).Name; item="Battle Mace"; } arty++;
			if ( artifact == arty) { name = typeof(LevelBladedStaff).Name; item="Bladed Staff"; } arty++;
			if ( artifact == arty) { name = typeof(LevelBokuto).Name; item="Bokuto"; } arty++;
			if ( artifact == arty) { name = typeof(LevelBow).Name; item="Bow"; } arty++;
			if ( artifact == arty) { name = typeof(LevelBroadsword).Name; item="Broadsword"; } arty++;
			if ( artifact == arty) { name = typeof(LevelButcherKnife).Name; item="Butcher Knife"; } arty++;
			if ( artifact == arty) { name = typeof(LevelChampionShield).Name; item="Champion Shield"; } arty++;
			if ( artifact == arty) { name = typeof(LevelClaymore).Name; item="Claymore"; } arty++;
			if ( artifact == arty) { name = typeof(LevelCleaver).Name; item="Cleaver"; } arty++;
			if ( artifact == arty) { name = typeof(LevelClub).Name; item="Club"; } arty++;
			if ( artifact == arty) { name = typeof(LevelCompositeBow).Name; item="Composite Bow"; } arty++;
			if ( artifact == arty) { name = typeof(LevelCrescentBlade).Name; item="Crescent Blade"; } arty++;
			if ( artifact == arty) { name = typeof(LevelCrestedShield).Name; item="Crested Shield"; } arty++;
			if ( artifact == arty) { name = typeof(LevelCrossbow).Name; item="Crossbow"; } arty++;
			if ( artifact == arty) { name = typeof(LevelCutlass).Name; item="Cutlass"; } arty++;
			if ( artifact == arty) { name = typeof(LevelDagger).Name; item="Dagger"; } arty++;
			if ( artifact == arty) { name = typeof(LevelDaisho).Name; item="Daisho"; } arty++;
			if ( artifact == arty) { name = typeof(LevelDoubleAxe).Name; item="Double Axe"; } arty++;
			if ( artifact == arty) { name = typeof(LevelDoubleBladedStaff).Name; item="Double Bladed Staff"; } arty++;
			if ( artifact == arty) { name = typeof(LevelWildStaff).Name; item="Druid Staff"; } arty++;
			if ( artifact == arty) { name = typeof(LevelRadiantScimitar).Name; item="Falchion"; } arty++;
			if ( artifact == arty) { name = typeof(LevelGnarledStaff).Name; item="Gnarled Staff"; } arty++;
			if ( artifact == arty) { name = typeof(LevelExecutionersAxe).Name; item="Great Axe"; } arty++;
			if ( artifact == arty) { name = typeof(LevelHalberd).Name; item="Halberd"; } arty++;
			if ( artifact == arty) { name = typeof(LevelHammers).Name; item="Hammer"; } arty++;
			if ( artifact == arty) { name = typeof(LevelHammerPick).Name; item="Hammer Pick"; } arty++;
			if ( artifact == arty) { name = typeof(LevelHarpoon).Name; item="Harpoon"; } arty++;
			if ( artifact == arty) { name = typeof(LevelHatchet).Name; item="Hatchet"; } arty++;
			if ( artifact == arty) { name = typeof(LevelHeavyCrossbow).Name; item="Heavy Crossbow"; } arty++;
			if ( artifact == arty) { name = typeof(LevelKama).Name; item="Kama"; } arty++;
			if ( artifact == arty) { name = typeof(LevelKatana).Name; item="Katana"; } arty++;
			if ( artifact == arty) { name = typeof(LevelKryss).Name; item="Kryss"; } arty++;
			if ( artifact == arty) { name = typeof(LevelLajatang).Name; item="Lajatang"; } arty++;
			if ( artifact == arty) { name = typeof(LevelLance).Name; item="Lance"; } arty++;
			if ( artifact == arty) { name = typeof(LevelLargeBattleAxe).Name; item="Large Battle Axe"; } arty++;
			if ( artifact == arty) { name = typeof(LevelLargeKnife).Name; item="Large Knife"; } arty++;
			if ( artifact == arty) { name = typeof(LevelLongsword).Name; item="Longsword"; } arty++;
			if ( artifact == arty) { name = typeof(LevelMace).Name; item="Mace"; } arty++;
			if ( artifact == arty) { name = typeof(LevelElvenMachete).Name; item="Machete"; } arty++;
			if ( artifact == arty) { name = typeof(LevelMaul).Name; item="Maul"; } arty++;
			if ( artifact == arty) { name = typeof(LevelNoDachi).Name; item="NoDachi"; } arty++;
			if ( artifact == arty) { name = typeof(LevelNunchaku).Name; item="Nunchaku"; } arty++;
			if ( artifact == arty) { name = typeof(LevelPickaxe).Name; item="Pickaxe"; } arty++;
			if ( artifact == arty) { name = typeof(LevelPike).Name; item="Pike"; } arty++;
			if ( artifact == arty) { name = typeof(LevelPugilistGloves).Name; item="Pugilist Gloves"; } arty++;
			if ( artifact == arty) { name = typeof(LevelQuarterStaff).Name; item="Quarter Staff"; } arty++;
			if ( artifact == arty) { name = typeof(LevelShortSpear).Name; item="Rapier"; } arty++;
			if ( artifact == arty) { name = typeof(LevelRepeatingCrossbow).Name; item="Repeating Crossbow"; } arty++;
			if ( artifact == arty) { name = typeof(LevelRoyalSword).Name; item="Royal Sword"; } arty++;
			if ( artifact == arty) { name = typeof(LevelSai).Name; item="Sai"; } arty++;
			if ( artifact == arty) { name = typeof(LevelScepter).Name; item="Scepter"; } arty++;
			if ( artifact == arty) { name = typeof(LevelSceptre).Name; item="Sceptre"; } arty++;
			if ( artifact == arty) { name = typeof(LevelScimitar).Name; item="Scimitar"; } arty++;
			if ( artifact == arty) { name = typeof(LevelScythe).Name; item="Scythe"; } arty++;
			if ( artifact == arty) { name = typeof(LevelShepherdsCrook).Name; item="Shepherds Crook"; } arty++;
			if ( artifact == arty) { name = typeof(LevelShortSword).Name; item="Short Sword"; } arty++;
			if ( artifact == arty) { name = typeof(LevelBoneHarvester).Name; item="Sickle"; } arty++;
			if ( artifact == arty) { name = typeof(LevelSpear).Name; item="Spear"; } arty++;
			if ( artifact == arty) { name = typeof(LevelSpikedClub).Name; item="Spiked Club"; } arty++;
			if ( artifact == arty) { name = typeof(LevelStave).Name; item="Stave"; } arty++;
			if ( artifact == arty) { name = typeof(LevelThinLongsword).Name; item="Sword"; } arty++;
			if ( artifact == arty) { name = typeof(LevelTekagi).Name; item="Tekagi"; } arty++;
			if ( artifact == arty) { name = typeof(LevelTessen).Name; item="Tessen"; } arty++;
			if ( artifact == arty) { name = typeof(LevelTetsubo).Name; item="Tetsubo"; } arty++;
			if ( artifact == arty) { name = typeof(LevelThrowingGloves).Name; item="Throwing Gloves"; } arty++;
			if ( artifact == arty) { name = typeof(LevelTribalSpear).Name; item="Tribal Spear"; } arty++;
			if ( artifact == arty) { name = typeof(LevelPitchfork).Name; item="Trident"; } arty++;
			if ( artifact == arty) { name = typeof(LevelTwoHandedAxe).Name; item="Two Handed Axe"; } arty++;
			if ( artifact == arty) { name = typeof(LevelWakizashi).Name; item="Wakizashi"; } arty++;
			if ( artifact == arty) { name = typeof(LevelWarAxe).Name; item="War Axe"; } arty++;
			if ( artifact == arty) { name = typeof(LevelRuneBlade).Name; item="War Blades"; } arty++;
			if ( artifact == arty) { name = typeof(LevelWarCleaver).Name; item="War Cleaver"; } arty++;
			if ( artifact == arty) { name = typeof(LevelLeafblade).Name; item="War Dagger"; } arty++;
			if ( artifact == arty) { name = typeof(LevelWarFork).Name; item="War Fork"; } arty++;
			if ( artifact == arty) { name = typeof(LevelWarHammer).Name; item="War Hammer"; } arty++;
			if ( artifact == arty) { name = typeof(LevelWarMace).Name; item="War Mace"; } arty++;
			if ( artifact == arty) { name = typeof(LevelWhips).Name; item="Whip"; } arty++;
			if ( artifact == arty) { name = typeof(LevelElvenCompositeLongbow).Name; item="Woodland Longbow"; } arty++;
			if ( artifact == arty) { name = typeof(LevelMagicalShortbow).Name; item="Woodland Shortbow"; } arty++;
			if ( artifact == arty) { name = typeof(LevelBlackStaff).Name; item="Wizard Staff"; } arty++;
			if ( artifact == arty) { name = typeof(LevelYumi).Name; item="Yumi"; } arty++;
			if ( artifact == arty) { name = typeof(LevelBandana).Name; item="Bandana"; } arty++;
			if ( artifact == arty) { name = typeof(LevelBearMask).Name; item="Bear Mask"; } arty++;
			if ( artifact == arty) { name = typeof(LevelBelt).Name; item="Belt"; } arty++;
			if ( artifact == arty) { name = typeof(LevelBodySash).Name; item="Body Sash"; } arty++;
			if ( artifact == arty) { name = typeof(LevelBonnet).Name; item="Bonnet"; } arty++;
			if ( artifact == arty) { name = typeof(LevelBoots).Name; item="Boots"; } arty++;
			if ( artifact == arty) { name = typeof(LevelCap).Name; item="Cap"; } arty++;
			if ( artifact == arty) { name = typeof(LevelCloak).Name; item="Cloak"; } arty++;
			if ( artifact == arty) { name = typeof(LevelClothNinjaHood).Name; item="Cloth Ninja Hood"; } arty++;
			if ( artifact == arty) { name = typeof(LevelClothNinjaJacket).Name; item="Cloth Ninja Jacket"; } arty++;
			if ( artifact == arty) { name = typeof(LevelCowl).Name; item="Cowl"; } arty++;
			if ( artifact == arty) { name = typeof(LevelDeerMask).Name; item="Deer Mask"; } arty++;
			if ( artifact == arty) { name = typeof(LevelDoublet).Name; item="Doublet"; } arty++;
			if ( artifact == arty) { name = typeof(LevelElvenBoots).Name; item="Fancy Boots"; } arty++;
			if ( artifact == arty) { name = typeof(LevelFancyDress).Name; item="Fancy Dress"; } arty++;
			if ( artifact == arty) { name = typeof(LevelFancyShirt).Name; item="Fancy Shirt"; } arty++;
			if ( artifact == arty) { name = typeof(LevelFeatheredHat).Name; item="Feathered Hat"; } arty++;
			if ( artifact == arty) { name = typeof(LevelFemaleKimono).Name; item="Female Kimono"; } arty++;
			if ( artifact == arty) { name = typeof(LevelFloppyHat).Name; item="Floppy Hat"; } arty++;
			if ( artifact == arty) { name = typeof(LevelFormalShirt).Name; item="Formal Shirt"; } arty++;
			if ( artifact == arty) { name = typeof(LevelFullApron).Name; item="Full Apron"; } arty++;
			if ( artifact == arty) { name = typeof(LevelFurBoots).Name; item="Fur Boots"; } arty++;
			if ( artifact == arty) { name = typeof(LevelFurCape).Name; item="Fur Cape"; } arty++;
			if ( artifact == arty) { name = typeof(LevelFurSarong).Name; item="Fur Sarong"; } arty++;
			if ( artifact == arty) { name = typeof(LevelGildedDress).Name; item="Gilded Dress"; } arty++;
			if ( artifact == arty) { name = typeof(LevelHakama).Name; item="Hakama"; } arty++;
			if ( artifact == arty) { name = typeof(LevelHakamaShita).Name; item="Hakama Shita"; } arty++;
			if ( artifact == arty) { name = typeof(LevelHalfApron).Name; item="Half Apron"; } arty++;
			if ( artifact == arty) { name = typeof(LevelHood).Name; item="Hood"; } arty++;
			if ( artifact == arty) { name = typeof(LevelHornedTribalMask).Name; item="Horned Tribal Mask"; } arty++;
			if ( artifact == arty) { name = typeof(LevelJesterHat).Name; item="Jester Hat"; } arty++;
			if ( artifact == arty) { name = typeof(LevelJesterSuit).Name; item="Jester Suit"; } arty++;
			if ( artifact == arty) { name = typeof(LevelJinBaori).Name; item="Jin Baori"; } arty++;
			if ( artifact == arty) { name = typeof(LevelKamishimo).Name; item="Kamishimo"; } arty++;
			if ( artifact == arty) { name = typeof(LevelKasa).Name; item="Kasa"; } arty++;
			if ( artifact == arty) { name = typeof(LevelKilt).Name; item="Kilt"; } arty++;
			if ( artifact == arty) { name = typeof(LevelLoinCloth).Name; item="Loin Cloth"; } arty++;
			if ( artifact == arty) { name = typeof(LevelLongPants).Name; item="Long Pants"; } arty++;
			if ( artifact == arty) { name = typeof(LevelMaleKimono).Name; item="Male Kimono"; } arty++;
			if ( artifact == arty) { name = typeof(LevelNinjaTabi).Name; item="Ninja Tabi"; } arty++;
			if ( artifact == arty) { name = typeof(LevelObi).Name; item="Obi"; } arty++;
			if ( artifact == arty) { name = typeof(LevelPlainDress).Name; item="Plain Dress"; } arty++;
			if ( artifact == arty) { name = typeof(LevelPirateHat).Name; item="Pirate Hat"; } arty++;
			if ( artifact == arty) { name = typeof(LevelRobe).Name; item="Robe"; } arty++;
			if ( artifact == arty) { name = typeof(LevelRoyalCape).Name; item="Royal Cape"; } arty++;
			if ( artifact == arty) { name = typeof(LevelSamuraiTabi).Name; item="Samurai Tabi"; } arty++;
			if ( artifact == arty) { name = typeof(LevelSandals).Name; item="Sandals"; } arty++;
			if ( artifact == arty) { name = typeof(LevelSash).Name; item="Sash"; } arty++;
			if ( artifact == arty) { name = typeof(LevelShirt).Name; item="Shirt"; } arty++;
			if ( artifact == arty) { name = typeof(LevelShoes).Name; item="Shoes"; } arty++;
			if ( artifact == arty) { name = typeof(LevelShortPants).Name; item="Short Pants"; } arty++;
			if ( artifact == arty) { name = typeof(LevelSkirt).Name; item="Skirt"; } arty++;
			if ( artifact == arty) { name = typeof(LevelSkullCap).Name; item="Skull Cap"; } arty++;
			if ( artifact == arty) { name = typeof(LevelStrawHat).Name; item="Straw Hat"; } arty++;
			if ( artifact == arty) { name = typeof(LevelSurcoat).Name; item="Surcoat"; } arty++;
			if ( artifact == arty) { name = typeof(LevelTallStrawHat).Name; item="Tall Straw Hat"; } arty++;
			if ( artifact == arty) { name = typeof(LevelTattsukeHakama).Name; item="Tattsuke Hakama"; } arty++;
			if ( artifact == arty) { name = typeof(LevelThighBoots).Name; item="Thigh Boots"; } arty++;
			if ( artifact == arty) { name = typeof(LevelTribalMask).Name; item="Tribal Mask"; } arty++;
			if ( artifact == arty) { name = typeof(LevelTricorneHat).Name; item="Tricorne Hat"; } arty++;
			if ( artifact == arty) { name = typeof(LevelTunic).Name; item="Tunic"; } arty++;
			if ( artifact == arty) { name = typeof(LevelWaraji).Name; item="Waraji"; } arty++;
			if ( artifact == arty) { name = typeof(LevelWideBrimHat).Name; item="Wide Brim Hat"; } arty++;
			if ( artifact == arty) { name = typeof(LevelWitchHat).Name; item="Witch Hat"; } arty++;
			if ( artifact == arty) { name = typeof(LevelWizardsHat).Name; item="Wizards Hat"; } arty++;
			if ( artifact == arty) { name = typeof(LevelWolfMask).Name; item="Wolf Mask"; } arty++;
			if ( artifact == arty) { name = typeof(LevelCandle).Name; item="Candle"; } arty++;
			if ( artifact == arty) { name = typeof(LevelGoldBeadNecklace).Name; item="Bead Necklace"; } arty++;
			if ( artifact == arty) { name = typeof(LevelGoldBracelet).Name; item="Gold Bracelet"; } arty++;
			if ( artifact == arty) { name = typeof(LevelGoldEarrings).Name; item="Gold Earrings"; } arty++;
			if ( artifact == arty) { name = typeof(LevelGoldNecklace).Name; item="Gold Amulet"; } arty++;
			if ( artifact == arty) { name = typeof(LevelGoldRing).Name; item="Gold Ring"; } arty++;
			if ( artifact == arty) { name = typeof(LevelLantern).Name; item="Lantern"; } arty++;
			if ( artifact == arty) { name = typeof(LevelNecklace).Name; item="Amulet"; } arty++;
			if ( artifact == arty) { name = typeof(LevelSilverBeadNecklace).Name; item="Silver Bead Necklace"; } arty++;
			if ( artifact == arty) { name = typeof(LevelSilverBracelet).Name; item="Silver Bracelet"; } arty++;
			if ( artifact == arty) { name = typeof(LevelSilverEarrings).Name; item="Silver Earrings"; } arty++;
			if ( artifact == arty) { name = typeof(LevelSilverNecklace).Name; item="Silver Amulet"; } arty++;
			if ( artifact == arty) { name = typeof(LevelSilverRing).Name; item="Silver Ring"; } arty++;
			if ( artifact == arty) { name = typeof(LevelTalismanLeather).Name; item="Trinket, Talisman"; } arty++;
			if ( artifact == arty) { name = typeof(LevelTalismanHoly).Name; item="Trinket, Symbol"; } arty++;
			if ( artifact == arty) { name = typeof(LevelTalismanSnake).Name; item="Trinket, Idol"; } arty++;
			if ( artifact == arty) { name = typeof(LevelTalismanTotem).Name; item="Trinket, Totem"; } arty++;
			if ( artifact == arty) { name = typeof(LevelTorch).Name; item="Torch"; } arty++;

			if ( part == 2 ){ item = name; }

			return item;
		}

		public static string ArtyItemName( string item, Mobile from )
		{
			string OwnerName = from.Name;
			string sAdjective = CultureInfo.CurrentCulture.TextInfo.ToTitleCase( RandomThings.MagicItemAdj( "start", Server.Misc.GetPlayerInfo.OrientalPlay( from ), Server.Misc.GetPlayerInfo.EvilPlay( from ), 0 ) );
			string name = item;

			if ( OwnerName.EndsWith( "s" ) )
			{
				OwnerName = OwnerName + "'";
			}
			else
			{
				OwnerName = OwnerName + "'s";
			}

			int FirstLast = 0;
			if ( Utility.RandomMinMax( 0, 1 ) == 1 ){ FirstLast = 1; }

			if ( FirstLast == 0 ) // FIRST COMES ADJECTIVE
			{
				name = "the " + sAdjective + " " + item + " of " + from.Name;
			}
			else // FIRST COMES OWNER
			{
				name = OwnerName + " " + sAdjective + " " + item;
			}

			return name;
		}
	}
}
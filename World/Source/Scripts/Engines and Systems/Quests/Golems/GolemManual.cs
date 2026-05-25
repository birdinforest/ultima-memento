using System;
using Server; 
using System.Collections;
using Server.ContextMenus;
using System.Collections.Generic;
using Server.Localization;
using Server.Misc;
using Server.Network;
using Server.Items;
using Server.Gumps;
using Server.Mobiles;
using Server.Commands;
using System.Globalization;
using Server.Regions;

namespace Server.Items
{
	public class GolemManual : Item
	{
		public override void ResourceChanged( CraftResource resource )
		{
			m_Resource = resource;
			Hue = CraftResources.GetHue(m_Resource);
		}

		public override string DisplayNameLocalizationKey => "item.special.manual_of_golems";
		public override bool IsContentLocalized => true;

		[Constructable]
		public GolemManual() : base( 0x225D )
		{
			Weight = 2.0;

			if ( Weight > 1.0 )
			{
				Weight = 1.0;

				HaveSprings = 0; // ONLY NEED SPRINGS FOR FIGHTING GOLEMS
				HaveClocks = 0;
				HaveCrystals = 0;
				HaveDarkCore = 0; // UP TO 3 DARK CORES FOR PROGRESSIVE BOOST
				HaveGears = 0;
				HaveGems = 0;
				HaveGold = 0;
				HaveMetalQty = 0;
				HaveOil = 0;

				string sGolem = "an Iron Golem";
				string sIngot = "iron ingots";
				Resource = CraftResource.Iron;
				Hue = 0x430;

				int iGolem = Utility.RandomMinMax( 1, 225 );
				int iAdd = 1;

				if ( iGolem >= 220 ){ sGolem = "a Valorite Golem"; iAdd = 9; sIngot = "valorite ingots"; Resource = CraftResource.Valorite; }
				else if ( iGolem >= 210 ){ sGolem = "a Verite Golem"; iAdd = 8; sIngot = "verite ingots"; Resource = CraftResource.Verite; }
				else if ( iGolem >= 195 ){ sGolem = "an Agapite Golem"; iAdd = 7; sIngot = "agapite ingots"; Resource = CraftResource.Agapite; }
				else if ( iGolem >= 175 ){ sGolem = "a Golden Golem"; iAdd = 6; sIngot = "golden ingots"; Resource = CraftResource.Gold; }
				else if ( iGolem >= 150 ){ sGolem = "a Bronze Golem"; iAdd = 5; sIngot = "bronze ingots"; Resource = CraftResource.Bronze; }
				else if ( iGolem >= 120 ){ sGolem = "a Copper Golem"; iAdd = 4; sIngot = "copper ingots"; Resource = CraftResource.Copper; }
				else if ( iGolem >= 85 ){ sGolem = "a Shadow Iron Golem"; iAdd = 3; sIngot = "shadow iron ingots"; Resource = CraftResource.ShadowIron; }
				else if ( iGolem >= 45 ){ sGolem = "a Dull Copper Golem"; iAdd = 2; sIngot = "dull copper ingots"; Resource = CraftResource.DullCopper; }

				GolemType = sGolem;
				NeedSprings = 5 * iAdd;
				NeedClocks = iAdd;
				NeedCrystals = iAdd;
				NeedGears = 5 * iAdd;
				NeedGems = iAdd;
				NeedGold = 9000 + ( 1000 * iAdd );
				NeedMetalQty = 1000;
				NeedMetalType = sIngot;
				NeedOil = 3 * iAdd;

				TinkerLocation = GetRandomTinker();
			}
		}

        public override void AddNameProperties(ObjectPropertyList list)
		{
            base.AddNameProperties(list);
			if ( BuildingPropertyListLocale != null )
			{
				string golemTypeLocalized = ResolvePropertyText( GetGolemTypeShotkey( GolemType ) );
				AddLocalizedProperty( list, "prop.golem.build_type", golemTypeLocalized );
				AddLocalizedProperty( list, "prop.golem.level", NeedGems.ToString() );
			}
			else
			{
				list.Add( 1070722, "To Build " + GolemType );
				list.Add( 1049644, "Level " + NeedGems.ToString() );
			}
			if ( HaveDarkCore > 0 )
			{
				if ( BuildingPropertyListLocale != null )
					AddLocalizedProperty( list, "prop.golem.exodus.core", HaveDarkCore.ToString() );
				else
					list.Add( 1070722, "Exodus Core: " + HaveDarkCore.ToString() + " / 3" );
			}
        }

		public static string GetGolemTypeShotkey( string golemType )
		{
			switch ( golemType )
			{
				case "a Valorite Golem": return "prop.golem.type.valorite";
				case "a Verite Golem": return "prop.golem.type.verite";
				case "an Agapite Golem": return "prop.golem.type.agapite";
				case "a Golden Golem": return "prop.golem.type.golden";
				case "a Bronze Golem": return "prop.golem.type.bronze";
				case "a Copper Golem": return "prop.golem.type.copper";
				case "a Shadow Iron Golem": return "prop.golem.type.shadowiron";
				case "a Dull Copper Golem": return "prop.golem.type.dullcopper";
				default: return "prop.golem.type.iron";
			}
		}

		public override void OnDoubleClick( Mobile from )
		{
			from.CloseGump( typeof( GolemManualGump ) );
			from.SendGump( new GolemManualGump( from, this ) );
		}

		public override bool OnDragDrop( Mobile from, Item dropped )
		{          		
			Container pack = from.Backpack;
			int iAmount = 0;
			string sEnd = ".";

			if ( from != null )
			{
				int WhatIsDropped = dropped.Amount;

				int WhatIsNeeded = NeedMetalQty - HaveMetalQty;
				int WhatIsExtra = WhatIsDropped - WhatIsNeeded; if ( WhatIsExtra < 1 ){ WhatIsExtra = 0; }
				int WhatIsTaken = WhatIsDropped - WhatIsExtra;

				if ( Resource == CraftResource.Valorite && dropped is ValoriteIngot && NeedMetalQty > HaveMetalQty )
				{
					if ( WhatIsExtra > 0 ){ from.AddToBackpack( new ValoriteIngot( WhatIsExtra ) ); }
					iAmount = WhatIsTaken;

					if ( iAmount > 1 ){ sEnd = "s."; }

					HaveMetalQty = HaveMetalQty + iAmount;
					from.SendMessage( StringCatalog.ResolveFormatByKey( from.Account, "eng.golem.added.valorite", iAmount, sEnd ) );
					dropped.Delete();
					return true;
				}
				else if ( Resource == CraftResource.Verite && dropped is VeriteIngot && NeedMetalQty > HaveMetalQty )
				{
					if ( WhatIsExtra > 0 ){ from.AddToBackpack( new VeriteIngot( WhatIsExtra ) ); }
					iAmount = WhatIsTaken;

					if ( iAmount > 1 ){ sEnd = "s."; }

					HaveMetalQty = HaveMetalQty + iAmount;
					from.SendMessage( StringCatalog.ResolveFormatByKey( from.Account, "eng.golem.added.verite", iAmount, sEnd ) );
					dropped.Delete();
					return true;
				}
				else if ( Resource == CraftResource.Agapite && dropped is AgapiteIngot && NeedMetalQty > HaveMetalQty )
				{
					if ( WhatIsExtra > 0 ){ from.AddToBackpack( new AgapiteIngot( WhatIsExtra ) ); }
					iAmount = WhatIsTaken;

					if ( iAmount > 1 ){ sEnd = "s."; }

					HaveMetalQty = HaveMetalQty + iAmount;
					from.SendMessage( StringCatalog.ResolveFormatByKey( from.Account, "eng.golem.added.agapite", iAmount, sEnd ) );
					dropped.Delete();
					return true;
				}
				else if ( Resource == CraftResource.Gold && dropped is GoldIngot && NeedMetalQty > HaveMetalQty )
				{
					if ( WhatIsExtra > 0 ){ from.AddToBackpack( new GoldIngot( WhatIsExtra ) ); }
					iAmount = WhatIsTaken;

					if ( iAmount > 1 ){ sEnd = "s."; }

					HaveMetalQty = HaveMetalQty + iAmount;
					from.SendMessage( StringCatalog.ResolveFormatByKey( from.Account, "eng.golem.added.golden", iAmount, sEnd ) );
					dropped.Delete();
					return true;
				}
				else if ( Resource == CraftResource.Bronze && dropped is BronzeIngot && NeedMetalQty > HaveMetalQty )
				{
					if ( WhatIsExtra > 0 ){ from.AddToBackpack( new BronzeIngot( WhatIsExtra ) ); }
					iAmount = WhatIsTaken;

					if ( iAmount > 1 ){ sEnd = "s."; }

					HaveMetalQty = HaveMetalQty + iAmount;
					from.SendMessage( StringCatalog.ResolveFormatByKey( from.Account, "eng.golem.added.bronze", iAmount, sEnd ) );
					dropped.Delete();
					return true;
				}
				else if ( Resource == CraftResource.Copper && dropped is CopperIngot && NeedMetalQty > HaveMetalQty )
				{
					if ( WhatIsExtra > 0 ){ from.AddToBackpack( new CopperIngot( WhatIsExtra ) ); }
					iAmount = WhatIsTaken;

					if ( iAmount > 1 ){ sEnd = "s."; }

					HaveMetalQty = HaveMetalQty + iAmount;
					from.SendMessage( StringCatalog.ResolveFormatByKey( from.Account, "eng.golem.added.copper", iAmount, sEnd ) );
					dropped.Delete();
					return true;
				}
				else if ( Resource == CraftResource.ShadowIron && dropped is ShadowIronIngot && NeedMetalQty > HaveMetalQty )
				{
					if ( WhatIsExtra > 0 ){ from.AddToBackpack( new ShadowIronIngot( WhatIsExtra ) ); }
					iAmount = WhatIsTaken;

					if ( iAmount > 1 ){ sEnd = "s."; }

					HaveMetalQty = HaveMetalQty + iAmount;
					from.SendMessage( StringCatalog.ResolveFormatByKey( from.Account, "eng.golem.added.shadowiron", iAmount, sEnd ) );
					dropped.Delete();
					return true;
				}
				else if ( Resource == CraftResource.DullCopper && dropped is DullCopperIngot && NeedMetalQty > HaveMetalQty )
				{
					if ( WhatIsExtra > 0 ){ from.AddToBackpack( new DullCopperIngot( WhatIsExtra ) ); }
					iAmount = WhatIsTaken;

					if ( iAmount > 1 ){ sEnd = "s."; }

					HaveMetalQty = HaveMetalQty + iAmount;
					from.SendMessage( StringCatalog.ResolveFormatByKey( from.Account, "eng.golem.added.dullcopper", iAmount, sEnd ) );
					dropped.Delete();
					return true;
				}
				else if ( Resource == CraftResource.Iron && dropped is IronIngot && NeedMetalQty > HaveMetalQty )
				{
					if ( WhatIsExtra > 0 ){ from.AddToBackpack( new IronIngot( WhatIsExtra ) ); }
					iAmount = WhatIsTaken;

					if ( iAmount > 1 ){ sEnd = "s."; }

					HaveMetalQty = HaveMetalQty + iAmount;
					from.SendMessage( StringCatalog.ResolveFormatByKey( from.Account, "eng.golem.added.iron", iAmount, sEnd ) );
					dropped.Delete();
					return true;
				}
				else if ( dropped is DarkCoreExodus && HaveDarkCore < 3 )
				{
					int newCount = HaveDarkCore + 1;
					int tier = GetMetalTier();

					switch ( newCount )
					{
						case 1:
							NeedMetalQty += 250;
							NeedClocks += 1 * tier;
							NeedCrystals += 1 * tier;
							NeedGems += 1 * tier;
							NeedGears += 3 * tier;
							NeedOil += 1 * tier;
							NeedGold += 5000 + ( 1000 * tier );
							NeedSprings += 2 * tier;
							break;
						case 2:
							NeedMetalQty += 500;
							NeedClocks += 2 * tier;
							NeedCrystals += 2 * tier;
							NeedGems += 2 * tier;
							NeedGears += 5 * tier;
							NeedOil += 2 * tier;
							NeedGold += 10000 + ( 2000 * tier );
							NeedSprings += 3 * tier;
							break;
						case 3:
							NeedMetalQty += 1000;
							NeedClocks += 3 * tier;
							NeedCrystals += 3 * tier;
							NeedGems += 3 * tier;
							NeedGears += 8 * tier;
							NeedOil += 3 * tier;
							NeedGold += 20000 + ( 5000 * tier );
							NeedSprings += 5 * tier;
							break;
					}

					HaveDarkCore = newCount;
					from.SendMessage( StringCatalog.ResolveFormatByKey( from.Account, "eng.golem.added.darkcore", HaveDarkCore ) );
					dropped.Delete();
					InvalidateProperties();
					return true;
				}
				else if ( dropped is PowerCrystal && NeedCrystals > HaveCrystals )
				{
					dropped.Delete();
					HaveCrystals = HaveCrystals + 1;
					from.SendMessage( StringCatalog.ResolveByKey( from.Account, "eng.golem.added.powercrystal" ) );
					dropped.Delete();
					return true;
				}
				else if ( dropped is ArcaneGem && NeedGems > HaveGems )
				{
					dropped.Delete();
					HaveGems = HaveGems + dropped.Amount;
					if ( dropped.Amount > 1 ){ from.SendMessage( StringCatalog.ResolveByKey( from.Account, "eng.golem.added.arcanegems" ) ); } else { from.SendMessage( StringCatalog.ResolveByKey( from.Account, "eng.golem.added.arcanegem" ) ); }
					if ( HaveGems > NeedGems )
					{
						from.AddToBackpack( new ArcaneGem( (HaveGems-NeedGems) ) );
						HaveGems = NeedGems;
					}
					dropped.Delete();
					return true;
				}
				else if ( dropped is ClockworkAssembly && NeedClocks > HaveClocks )
				{
					dropped.Delete();
					HaveClocks = HaveClocks + 1;
					from.SendMessage( StringCatalog.ResolveByKey( from.Account, "eng.golem.added.clockwork" ) );
					dropped.Delete();
					return true;
				}
				else if ( dropped is BottleOil && NeedOil > HaveOil )
				{
					WhatIsNeeded = NeedOil - HaveOil;
					WhatIsExtra = WhatIsDropped - WhatIsNeeded; if ( WhatIsExtra < 1 ){ WhatIsExtra = 0; }
					WhatIsTaken = WhatIsDropped - WhatIsExtra;

					if ( WhatIsExtra > 0 ){ from.AddToBackpack( new BottleOil( WhatIsExtra ) ); }
					iAmount = WhatIsTaken;

					if ( iAmount > 1 ){ sEnd = "s."; }

					HaveOil = HaveOil + iAmount;
					from.SendMessage( StringCatalog.ResolveFormatByKey( from.Account, "eng.golem.added.oil", iAmount, sEnd ) );
					dropped.Delete();
					return true;
				}
				else if ( dropped is Gears && NeedGears > HaveGears )
				{
					WhatIsNeeded = NeedGears - HaveGears;
					WhatIsExtra = WhatIsDropped - WhatIsNeeded; if ( WhatIsExtra < 1 ){ WhatIsExtra = 0; }
					WhatIsTaken = WhatIsDropped - WhatIsExtra;

					if ( WhatIsExtra > 0 ){ from.AddToBackpack( new Gears( WhatIsExtra ) ); }
					iAmount = WhatIsTaken;

					if ( iAmount > 1 ){ sEnd = "s."; }

					HaveGears = HaveGears + iAmount;
					from.SendMessage( StringCatalog.ResolveFormatByKey( from.Account, "eng.golem.added.gear", iAmount, sEnd ) );
					dropped.Delete();
					return true;
				}
				else if ( dropped is Springs && NeedSprings > HaveSprings )
				{
					WhatIsNeeded = NeedSprings - HaveSprings;
					WhatIsExtra = WhatIsDropped - WhatIsNeeded; if ( WhatIsExtra < 1 ){ WhatIsExtra = 0; }
					WhatIsTaken = WhatIsDropped - WhatIsExtra;

					if ( WhatIsExtra > 0 ){ from.AddToBackpack( new Springs( WhatIsExtra ) ); }
					iAmount = WhatIsTaken;

					if ( iAmount > 1 ){ sEnd = "s."; }

					HaveSprings = HaveSprings + iAmount;
					from.SendMessage( StringCatalog.ResolveFormatByKey( from.Account, "eng.golem.added.spring", iAmount, sEnd ) );
					dropped.Delete();
					return true;
				}
				else if ( dropped is Gold && NeedGold > HaveGold )
				{
					WhatIsNeeded = NeedGold - HaveGold;
					WhatIsExtra = WhatIsDropped - WhatIsNeeded; if ( WhatIsExtra < 1 ){ WhatIsExtra = 0; }
					WhatIsTaken = WhatIsDropped - WhatIsExtra;

					if ( WhatIsExtra > 0 ){ from.AddToBackpack( new Gold( WhatIsExtra ) ); }
					iAmount = WhatIsTaken;

					if ( iAmount > 1 ){ sEnd = "s."; }

					HaveGold = HaveGold + iAmount;
					from.SendMessage( StringCatalog.ResolveFormatByKey( from.Account, "eng.golem.added.gold", iAmount, sEnd ) );
					dropped.Delete();
					return true;
				}
			}

			return false;
		}

		public GolemManual( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int)1 ); // version

			writer.Write( GolemType );
			writer.Write( HaveSprings );
			writer.Write( HaveClocks );
			writer.Write( HaveCrystals );
			writer.Write( HaveDarkCore );
			writer.Write( HaveGears );
			writer.Write( HaveGems );
			writer.Write( HaveGold );
			writer.Write( HaveMetalQty );
			writer.Write( HaveOil );
			writer.Write( NeedSprings );
			writer.Write( NeedClocks );
			writer.Write( NeedCrystals );
			writer.Write( NeedGears );
			writer.Write( NeedGems );
			writer.Write( NeedGold );
			writer.Write( NeedMetalQty );
			writer.Write( NeedMetalType );
			writer.Write( NeedOil );
			writer.Write( TinkerLocation );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();

			GolemType = reader.ReadString();
			HaveSprings = reader.ReadInt();
			HaveClocks = reader.ReadInt();
			HaveCrystals = reader.ReadInt();
			HaveDarkCore = reader.ReadInt();
			HaveGears = reader.ReadInt();
			HaveGems = reader.ReadInt();
			HaveGold = reader.ReadInt();
			HaveMetalQty = reader.ReadInt();
			HaveOil = reader.ReadInt();
			NeedSprings = reader.ReadInt();
			NeedClocks = reader.ReadInt();
			NeedCrystals = reader.ReadInt();
			NeedGears = reader.ReadInt();
			NeedGems = reader.ReadInt();
			NeedGold = reader.ReadInt();
			NeedMetalQty = reader.ReadInt();
			NeedMetalType = reader.ReadString();
			NeedOil = reader.ReadInt();
			TinkerLocation = reader.ReadString();

			if ( Resource == CraftResource.None )
			{
				if ( NeedMetalType == "valorite ingots" ){ Resource = CraftResource.Valorite; }
				else if ( NeedMetalType == "verite ingots" ){ Resource = CraftResource.Verite; }
				else if ( NeedMetalType == "agapite ingots" ){ Resource = CraftResource.Agapite; }
				else if ( NeedMetalType == "golden ingots" ){ Resource = CraftResource.Gold; }
				else if ( NeedMetalType == "bronze ingots" ){ Resource = CraftResource.Bronze; }
				else if ( NeedMetalType == "copper ingots" ){ Resource = CraftResource.Copper; }
				else if ( NeedMetalType == "shadow iron ingots" ){ Resource = CraftResource.ShadowIron; }
				else if ( NeedMetalType == "dull copper ingots" ){ Resource = CraftResource.DullCopper; }
				else { Resource = CraftResource.Iron; Hue = 0x430; }
			}
		}

		public static string GetRandomTinker()
		{
			int aCount = 0;
			Region reg = null;
			string sRegion = "";

			ArrayList targets = new ArrayList();
			foreach ( Mobile target in World.Mobiles.Values )
			if ( target is BaseVendor )
			{
				reg = Region.Find( target.Location, target.Map );

				if (	target.Land == Land.Sosaria || 
						target.Land == Land.Lodoria || 
						target.Land == Land.Serpent || 
						target.Land == Land.IslesDread || 
						target.Land == Land.Savaged || 
						target.Land == Land.UmberVeil || 
						target.Land == Land.Kuldar )
				{
					if ( ( target is Tinker || target is TinkerGuildmaster ) && reg.IsPartOf( typeof( VillageRegion ) ))
					{
						targets.Add( target ); aCount++;
					}
				}
			}

			aCount = Utility.RandomMinMax( 1, aCount );

			int xCount = 0;
			for ( int i = 0; i < targets.Count; ++i )
			{
				Mobile tinker = ( Mobile )targets[ i ];
				xCount++;

				if ( xCount == aCount )
				{
					sRegion = Server.Misc.Worlds.GetRegionName( tinker.Map, tinker.Location );
				}
			}

			return sRegion;
		}

		public static bool ProcessGolemBook( Mobile m, Mobile tinker, Item dropped )
		{
			GolemManual book = (GolemManual)dropped;

			if ( Server.Misc.Worlds.GetRegionName( tinker.Map, tinker.Location ) != book.TinkerLocation ){ return false; }

			int tinkerSkill = (int)(m.Skills[SkillName.Tinkering].Value);
				if ( tinkerSkill > 100 ){ tinkerSkill = 100; }

			int GoldReturn = 0;
				if ( tinkerSkill > 0 ){ GoldReturn = (int)( book.NeedGold * ( tinkerSkill * 0.005 ) ); }

			int HaveIngredients = 0;

			if ( book.HaveClocks >= book.NeedClocks ){ HaveIngredients++; }
			if ( book.HaveCrystals >= book.NeedCrystals ){ HaveIngredients++; }
			if ( book.HaveGears >= book.NeedGears ){ HaveIngredients++; }
			if ( book.HaveGems >= book.NeedGems ){ HaveIngredients++; }
			if ( book.HaveGold >= book.NeedGold ){ HaveIngredients++; }
			if ( book.HaveOil >= book.NeedOil ){ HaveIngredients++; }
			if ( book.HaveMetalQty >= book.NeedMetalQty ){ HaveIngredients++; }

			if ( HaveIngredients < 7 ){ return false; }

			int FighterGolem = 0;

			if ( book.HaveSprings >= book.NeedSprings )
			{
				FighterGolem = 1;
			}

			int PortColor = 0;
			int ExodusBoost = 0;

			if ( GoldReturn > 0 ){ m.AddToBackpack( new Gold( GoldReturn ) ); tinker.Say( StringCatalog.ResolveFormatByKey(m.Account, "quest.here_is_n0_gold_back_for_all_of_your_help_dot", GoldReturn ) ); }

			if ( book.GolemType == "a Valorite Golem" ){ PortColor = dropped.Hue; ExodusBoost = 9; }
			else if ( book.GolemType == "a Verite Golem" ){ PortColor = dropped.Hue; ExodusBoost = 8; }
			else if ( book.GolemType == "an Agapite Golem" ){ PortColor = dropped.Hue; ExodusBoost = 7; }
			else if ( book.GolemType == "a Golden Golem" ){ PortColor = dropped.Hue; ExodusBoost = 6; }
			else if ( book.GolemType == "a Bronze Golem" ){ PortColor = dropped.Hue; ExodusBoost = 5; }
			else if ( book.GolemType == "a Copper Golem" ){ PortColor = dropped.Hue; ExodusBoost = 4; }
			else if ( book.GolemType == "a Shadow Iron Golem" ){ PortColor = dropped.Hue; ExodusBoost = 3; }
			else if ( book.GolemType == "a Dull Copper Golem" ){ PortColor = dropped.Hue; ExodusBoost = 2; }
			else if ( book.GolemType == "an Iron Golem" ){ PortColor = 0x430; ExodusBoost = 1; }

			GolemPorterItem ball = new GolemPorterItem();

			string QuestLog = "had " + (book.GolemType).ToLower() + " built";

			if ( book.HaveDarkCore > 0 ){ ball.PorterExodus = book.HaveDarkCore; PortColor = 2118; QuestLog = QuestLog + " with " + book.HaveDarkCore.ToString() + " dark core(s) of Exodus"; }

			ball.PorterOwner = m;
			ball.PorterHue = PortColor;
			ball.PorterType = FighterGolem;
			ball.Resource = dropped.Resource;
			ball.Hue = PortColor;

			m.AddToBackpack ( ball );

			LoggingFunctions.LogGenericQuest( m, QuestLog );

			m.PrivateOverheadMessage(MessageType.Regular, 1153, false, StringCatalog.ResolveByKey( m.Account, "eng.golem.completed.built" ), m.NetState);
			m.PlaySound( 0x5C3 );

			dropped.Delete();

			return true;
		}

		public class GolemManualGump : Gump
		{
			private GolemManual m_Book;

			public GolemManualGump( Mobile from, GolemManual gBook ): base( 100, 100 )
			{
				m_Book = gBook;
				string color = "#c5c5c5";
				from.SendSound( 0x55 );

				GolemManual pedia = (GolemManual)gBook;

			string sExodus = "";
				if ( gBook.HaveDarkCore > 0 ){ sExodus = " of Exodus (" + gBook.HaveDarkCore.ToString() + "/3)"; }

				string GolemType = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase( (gBook.GolemType).ToLower() );

				string sText = StringCatalog.ResolveByKey(from.Account, "quest.this_rare_tome_contains_the_knowledge_to_construct_a_metal_golem_dot_within_its_pages_you_will_see_w");

				this.Closable=true;
				this.Disposable=true;
				this.Dragable=true;
				this.Resizable=false;

				AddPage(0);

				AddImage(0, 0, 7005, 1071);
				AddImage(0, 0, 7006);
				AddImage(0, 0, 7024, 2736);

				AddHtml( 370, 52, 250, 343, @"<BODY><BASEFONT Color=" + color + ">" + sText + "</BASEFONT></BODY>", (bool)false, (bool)true);

				AddHtml( 75, 48, 250, 20, @"<BODY><BASEFONT Color=" + color + ">" + GolemType + sExodus + "</BASEFONT></BODY>", (bool)false, (bool)false);
				AddHtml( 75, 372, 250, 20, @"<BODY><BASEFONT Color=" + color + ">" + gBook.TinkerLocation + "</BASEFONT></BODY>", (bool)false, (bool)false);

				int x = 114;
				int i = 28;

				AddHtml( 80, x, 119, 20, @"<BODY><BASEFONT Color=" + color + ">" + StringCatalog.ResolveByKey(from.Account, "quest.ingots") + "</BASEFONT></BODY>", (bool)false, (bool)false);
				AddHtml( 205, x, 55, 20, @"<BODY><BASEFONT Color=" + color + ">" + gBook.NeedMetalQty.ToString() + "</BASEFONT></BODY>", (bool)false, (bool)false);
				AddHtml( 265, x, 55, 20, @"<BODY><BASEFONT Color=" + color + ">" + gBook.HaveMetalQty.ToString() + "</BASEFONT></BODY>", (bool)false, (bool)false);
				x=x+i;
				AddHtml( 80, x, 119, 20, @"<BODY><BASEFONT Color=" + color + ">" + StringCatalog.ResolveByKey(from.Account, "quest.clockworks") + "</BASEFONT></BODY>", (bool)false, (bool)false);
				AddHtml( 205, x, 55, 20, @"<BODY><BASEFONT Color=" + color + ">" + gBook.NeedClocks.ToString() + "</BASEFONT></BODY>", (bool)false, (bool)false);
				AddHtml( 265, x, 55, 20, @"<BODY><BASEFONT Color=" + color + ">" + gBook.HaveClocks.ToString() + "</BASEFONT></BODY>", (bool)false, (bool)false);
				x=x+i;
				AddHtml( 80, x, 119, 20, @"<BODY><BASEFONT Color=" + color + ">" + StringCatalog.ResolveByKey(from.Account, "quest.crystals") + "</BASEFONT></BODY>", (bool)false, (bool)false);
				AddHtml( 205, x, 55, 20, @"<BODY><BASEFONT Color=" + color + ">" + gBook.NeedCrystals.ToString() + "</BASEFONT></BODY>", (bool)false, (bool)false);
				AddHtml( 265, x, 55, 20, @"<BODY><BASEFONT Color=" + color + ">" + gBook.HaveCrystals.ToString() + "</BASEFONT></BODY>", (bool)false, (bool)false);
				x=x+i;
				AddHtml( 80, x, 119, 20, @"<BODY><BASEFONT Color=" + color + ">" + StringCatalog.ResolveByKey(from.Account, "quest.arcane_gems") + "</BASEFONT></BODY>", (bool)false, (bool)false);
				AddHtml( 205, x, 55, 20, @"<BODY><BASEFONT Color=" + color + ">" + gBook.NeedGems.ToString() + "</BASEFONT></BODY>", (bool)false, (bool)false);
				AddHtml( 265, x, 55, 20, @"<BODY><BASEFONT Color=" + color + ">" + gBook.HaveGems.ToString() + "</BASEFONT></BODY>", (bool)false, (bool)false);
				x=x+i;
				AddHtml( 80, x, 119, 20, @"<BODY><BASEFONT Color=" + color + ">" + StringCatalog.ResolveByKey(from.Account, "quest.gears") + "</BASEFONT></BODY>", (bool)false, (bool)false);
				AddHtml( 205, x, 55, 20, @"<BODY><BASEFONT Color=" + color + ">" + gBook.NeedGears.ToString() + "</BASEFONT></BODY>", (bool)false, (bool)false);
				AddHtml( 265, x, 55, 20, @"<BODY><BASEFONT Color=" + color + ">" + gBook.HaveGears.ToString() + "</BASEFONT></BODY>", (bool)false, (bool)false);
				x=x+i;
				AddHtml( 80, x, 119, 20, @"<BODY><BASEFONT Color=" + color + ">" + StringCatalog.ResolveByKey(from.Account, "quest.oil") + "</BASEFONT></BODY>", (bool)false, (bool)false);
				AddHtml( 205, x, 55, 20, @"<BODY><BASEFONT Color=" + color + ">" + gBook.NeedOil.ToString() + "</BASEFONT></BODY>", (bool)false, (bool)false);
				AddHtml( 265, x, 55, 20, @"<BODY><BASEFONT Color=" + color + ">" + gBook.HaveOil.ToString() + "</BASEFONT></BODY>", (bool)false, (bool)false);
				x=x+i;
				AddHtml( 80, x, 119, 20, @"<BODY><BASEFONT Color=" + color + ">" + StringCatalog.Resolve( from.Account, "Gold" ) + "</BASEFONT></BODY>", (bool)false, (bool)false);
				AddHtml( 205, x, 55, 20, @"<BODY><BASEFONT Color=" + color + ">" + gBook.NeedGold.ToString() + "</BASEFONT></BODY>", (bool)false, (bool)false);
				AddHtml( 265, x, 55, 20, @"<BODY><BASEFONT Color=" + color + ">" + gBook.HaveGold.ToString() + "</BASEFONT></BODY>", (bool)false, (bool)false);
				x=x+i;
				AddHtml( 80, x, 119, 20, @"<BODY><BASEFONT Color=" + color + ">" + StringCatalog.ResolveByKey(from.Account, "quest.springs") + "</BASEFONT></BODY>", (bool)false, (bool)false);
				AddHtml( 205, x, 55, 20, @"<BODY><BASEFONT Color=" + color + ">" + gBook.NeedSprings.ToString() + "</BASEFONT></BODY>", (bool)false, (bool)false);
				AddHtml( 265, x, 55, 20, @"<BODY><BASEFONT Color=" + color + ">" + gBook.HaveSprings.ToString() + "</BASEFONT></BODY>", (bool)false, (bool)false);
			}

			public override void OnResponse( NetState state, RelayInfo info ) 
			{
				Mobile from = state.Mobile; 
				from.PlaySound( 0x55 );
			}
		}

		public string TinkerLocation;
		[CommandProperty( AccessLevel.GameMaster )]
		public string g_TinkerLocation { get{ return TinkerLocation; } set{ TinkerLocation = value; } }

		private int GetMetalTier()
		{
			if ( Resource == CraftResource.Valorite ) return 9;
			if ( Resource == CraftResource.Verite ) return 8;
			if ( Resource == CraftResource.Agapite ) return 7;
			if ( Resource == CraftResource.Gold ) return 6;
			if ( Resource == CraftResource.Bronze ) return 5;
			if ( Resource == CraftResource.Copper ) return 4;
			if ( Resource == CraftResource.ShadowIron ) return 3;
			if ( Resource == CraftResource.DullCopper ) return 2;
			return 1; // Iron or unknown
		}

		public string GolemType;
		[CommandProperty( AccessLevel.GameMaster )]
		public string g_GolemType { get{ return GolemType; } set{ GolemType = value; } }

		// ----------------------------------------------------------------------------------------

		public string NeedMetalType;
		[CommandProperty( AccessLevel.GameMaster )]
		public string g_NeedMetalType { get{ return NeedMetalType; } set{ NeedMetalType = value; } }

		public int NeedMetalQty;
		[CommandProperty( AccessLevel.GameMaster )]
		public int g_NeedMetalQty { get{ return NeedMetalQty; } set{ NeedMetalQty = value; } }

		public int NeedSprings;
		[CommandProperty( AccessLevel.GameMaster )]
		public int g_NeedSprings { get{ return NeedSprings; } set{ NeedSprings = value; } }

		public int NeedGold;
		[CommandProperty( AccessLevel.GameMaster )]
		public int g_NeedGold { get{ return NeedGold; } set{ NeedGold = value; } }

		public int NeedOil;
		[CommandProperty( AccessLevel.GameMaster )]
		public int g_NeedOil { get{ return NeedOil; } set{ NeedOil = value; } }

		public int NeedCrystals;
		[CommandProperty( AccessLevel.GameMaster )]
		public int g_NeedCrystals { get{ return NeedCrystals; } set{ NeedCrystals = value; } }

		public int NeedClocks;
		[CommandProperty( AccessLevel.GameMaster )]
		public int g_NeedClocks { get{ return NeedClocks; } set{ NeedClocks = value; } }

		public int NeedGems;
		[CommandProperty( AccessLevel.GameMaster )]
		public int g_NeedGems { get{ return NeedGems; } set{ NeedGems = value; } }

		public int NeedGears;
		[CommandProperty( AccessLevel.GameMaster )]
		public int g_NeedGears { get{ return NeedGears; } set{ NeedGears = value; } }

		// ----------------------------------------------------------------------------------------

		public int HaveMetalQty;
		[CommandProperty( AccessLevel.GameMaster )]
		public int g_HaveMetalQty { get{ return HaveMetalQty; } set{ HaveMetalQty = value; } }

		public int HaveDarkCore;
		[CommandProperty( AccessLevel.GameMaster )]
		public int g_HaveDarkCore { get{ return HaveDarkCore; } set{ HaveDarkCore = value; } }

		public int HaveSprings;
		[CommandProperty( AccessLevel.GameMaster )]
		public int g_HaveSprings { get{ return HaveSprings; } set{ HaveSprings = value; } }

		public int HaveGold;
		[CommandProperty( AccessLevel.GameMaster )]
		public int g_HaveGold { get{ return HaveGold; } set{ HaveGold = value; } }

		public int HaveOil;
		[CommandProperty( AccessLevel.GameMaster )]
		public int g_HaveOil { get{ return HaveOil; } set{ HaveOil = value; } }

		public int HaveCrystals;
		[CommandProperty( AccessLevel.GameMaster )]
		public int g_HaveCrystals { get{ return HaveCrystals; } set{ HaveCrystals = value; } }

		public int HaveClocks;
		[CommandProperty( AccessLevel.GameMaster )]
		public int g_HaveClocks { get{ return HaveClocks; } set{ HaveClocks = value; } }

		public int HaveGems;
		[CommandProperty( AccessLevel.GameMaster )]
		public int g_HaveGems { get{ return HaveGems; } set{ HaveGems = value; } }

		public int HaveGears;
		[CommandProperty( AccessLevel.GameMaster )]
		public int g_HaveGears { get{ return HaveGears; } set{ HaveGears = value; } }
	}
}
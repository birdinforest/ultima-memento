using System;
using Server; 
using Server.Localization;
using System.Collections;
using Server.ContextMenus;
using System.Collections.Generic;
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
	public class DragonEgg : Item
	{
		public override bool IsContentLocalized { get { return true; } }

		public override string DisplayNameLocalizationKey
		{
			get
			{
				if ( Name != null && Name.StartsWith( "egg of " ) )
					return null;

				return "item.special.dragon.egg";
			}
		}

		[Constructable]
		public DragonEgg() : base( 0x278C )
		{
			Weight = 4.0;
			Name = "Dragon Egg";
			Light = LightType.Circle225;

			if ( Weight > 3.0 )
			{
				Weight = 3.0;

				HavePotionA = 0;
				HavePotionB = 0;
				HavePotionC = 0;
				HavePotionD = 0;
				HaveGold = 0;

				AnimalTrainerLocation = Server.Items.AlienEgg.GetRandomVet();

				PieceRumor = Server.Items.CubeOnCorpse.GetRumor();
				PieceLocation = Server.Items.CubeOnCorpse.PickDungeon();
			}
		}

		public override void AddNameProperty( ObjectPropertyList list )
		{
			if ( BuildingPropertyListLocale != null && Name != null && Name.StartsWith( "egg of " ) )
			{
				string creature = Name.Substring( 7 );
				string resolvedCreature = StringCatalog.TryResolve( BuildingPropertyListLocale, creature ) ?? creature;
				string line = string.Format( ResolvePropertyText( "item.special.dragon.egg.of" ), resolvedCreature );

				if ( Amount <= 1 )
					list.Add( line );
				else
					list.Add( 1050039, "{0}\t{1}", Amount, line );
				return;
			}

			if ( TryAddLocalizedDisplayNameProperty( list ) )
				return;

			base.AddNameProperty( list );
		}

		public static string LocalizeDisplayName( Mobile from, DragonEgg egg )
		{
			if ( egg == null )
				return StringCatalog.ResolveByKey( from != null ? from.Account : null, "item.special.dragon.egg" );

			if ( egg.Name != null && egg.Name.StartsWith( "egg of " ) )
			{
				string creature = egg.Name.Substring( 7 );
				string lang = AccountLang.GetLanguageCode( from != null ? from.Account : null );
				string resolvedCreature = StringCatalog.TryResolve( lang, creature ) ?? creature;
				return StringCatalog.ResolveFormatByKey( from != null ? from.Account : null, "item.special.dragon.egg.of", resolvedCreature );
			}

			return StringCatalog.ResolveByKey( from != null ? from.Account : null, "item.special.dragon.egg" );
		}

		public static string LocalizeRumorVerb( Mobile from, string rumor )
		{
			if ( rumor == "is said to be in" )
				return StringCatalog.ResolveByKey( from.Account, "prop.special.egg.rumor.is_said" );
			if ( rumor == "is rumored to be in" )
				return StringCatalog.ResolveByKey( from.Account, "prop.special.egg.rumor.is_rumored" );
			if ( rumor == "has legends tell of it being in" )
				return StringCatalog.ResolveByKey( from.Account, "prop.special.egg.rumor.legends" );
			if ( rumor == "was heard to be in" )
				return StringCatalog.ResolveByKey( from.Account, "prop.special.egg.rumor.heard" );

			string lang = AccountLang.GetLanguageCode( from != null ? from.Account : null );
			return StringCatalog.TryResolve( lang, rumor ) ?? ( rumor ?? string.Empty );
		}

		public static string LocalizeSavedPlace( Mobile from, string place )
		{
			if ( string.IsNullOrEmpty( place ) )
				return string.Empty;

			string lang = AccountLang.GetLanguageCode( from != null ? from.Account : null );
			return StringCatalog.TryResolve( lang, place ) ?? place;
		}

		public static string FormatIngredientRumor( Mobile from, string rumorKey, string pieceRumor, string pieceLocation )
		{
			return StringCatalog.ResolveFormatByKey( from.Account, rumorKey,
				LocalizeRumorVerb( from, pieceRumor ),
				LocalizeSavedPlace( from, pieceLocation ) );
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( Weight > 2.0 && from.Map == Map.Lodor && from.X >= 5296 && from.Y >= 664 && from.X <= 5318 && from.Y <= 686 )
			{
				Weight = 1.0;
			}

			if ( Weight < 1.5 )
			{
				from.CloseGump( typeof( DragonEggGump ) );
				from.SendGump( new DragonEggGump( from, this ) );
			}
		}

		public override bool OnDragDrop( Mobile from, Item dropped )
		{          		
			int iAmount = 0;
			string sEnd = ".";

			if ( from != null && Weight < 1.5 )
			{
				if ( dropped is Gold && NeedGold > HaveGold )
				{
					int WhatIsDropped = dropped.Amount;
					int WhatIsNeeded = NeedGold - HaveGold;
					int WhatIsExtra = WhatIsDropped - WhatIsNeeded; if ( WhatIsExtra < 1 ){ WhatIsExtra = 0; }
					int WhatIsTaken = WhatIsDropped - WhatIsExtra;

					if ( WhatIsExtra > 0 ){ from.AddToBackpack( new Gold( WhatIsExtra ) ); }
					iAmount = WhatIsTaken;

					if ( iAmount > 1 ){ sEnd = "s."; }

					HaveGold = HaveGold + iAmount;
					from.SendMessage( StringCatalog.ResolveFormatByKey( from.Account, "prop.special.egg.gold.added", iAmount, sEnd ) );
					dropped.Delete();
					return true;
				}
			}

			return false;
		}

		public DragonEgg( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int)1 ); // version

			writer.Write( HavePotionA );
			writer.Write( HavePotionB );
			writer.Write( HavePotionC );
			writer.Write( HavePotionD );
			writer.Write( HaveGold );
			writer.Write( NeedGold );
			writer.Write( AnimalTrainerLocation );
			writer.Write( PieceLocation );
			writer.Write( PieceRumor );
			writer.Write( DragonType );
			writer.Write( DragonBody );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();

			HavePotionA = reader.ReadInt();
			HavePotionB = reader.ReadInt();
			HavePotionC = reader.ReadInt();
			HavePotionD = reader.ReadInt();
			HaveGold = reader.ReadInt();
			NeedGold = reader.ReadInt();
			AnimalTrainerLocation = reader.ReadString();
			PieceLocation = reader.ReadString();
			PieceRumor = reader.ReadString();
			DragonType = reader.ReadInt();
			DragonBody = reader.ReadInt();
		}

		public static bool ProcessDragonEgg( Mobile m, Mobile vet, Item dropped )
		{
			DragonEgg egg = (DragonEgg)dropped;

			if ( Server.Misc.Worlds.GetRegionName( vet.Map, vet.Location ) != egg.AnimalTrainerLocation ){ return false; }

			int vetSkill = (int)(m.Skills[SkillName.Veterinary].Value);
				if ( vetSkill > 100 ){ vetSkill = 100; }

			int GoldReturn = 0;
				if ( vetSkill > 0 ){ GoldReturn = (int)( egg.NeedGold * ( vetSkill * 0.005 ) ); }

			int HaveIngredients = 0;

			if ( egg.HavePotionB >= 0 ){ HaveIngredients++; }
			if ( egg.HavePotionC >= 0 ){ HaveIngredients++; }
			if ( egg.HavePotionD >= 0 ){ HaveIngredients++; }
			if ( egg.HaveGold >= egg.NeedGold ){ HaveIngredients++; }
			if ( egg.HavePotionA >= 0 ){ HaveIngredients++; }

			if ( HaveIngredients < 5 ){ return false; }

			int followers = 3;
			if ( (dropped.Name).Contains(" dragon") ){ followers = 2; }

			if ( (m.Followers + followers) > m.FollowersMax )
			{
				vet.Say( StringCatalog.ResolveByKey( m.Account, "prop.special.egg.followers.too.many" ) );
				return false;
			}

			if ( GoldReturn > 0 ){ m.AddToBackpack( new Gold( GoldReturn ) ); vet.Say( StringCatalog.ResolveFormatByKey( m.Account, "prop.special.egg.refund", GoldReturn ) ); }

			BaseCreature dragon = new RidingDragon( "a dragon", egg.DragonBody, egg.DragonType );
			dragon.OnAfterSpawn();
			dragon.Controlled = true;
			dragon.ControlMaster = m;
			dragon.IsBonded = true;
			dragon.MoveToWorld( m.Location, m.Map );
			dragon.ControlTarget = m;
			dragon.Tamable = true;
			dragon.MinTameSkill = 29.1;
			dragon.ControlOrder = OrderType.Follow;

			string styleKey = "prop.special.egg.dragon.style.dragon";
			string style = "dragon";
			if ( followers == 3 )
			{
				style = "wyrm";
				styleKey = "prop.special.egg.dragon.style.wyrm";
				dragon.Name = (dragon.Name).Replace(" dragon", " wyrm");
			}

			LoggingFunctions.LogGenericQuest( m, "has hatched a " + style + "" );
			m.PrivateOverheadMessage( MessageType.Regular, 1153, false,
				StringCatalog.ResolveFormatByKey( m.Account, "prop.special.egg.dragon.hatched",
					StringCatalog.ResolveByKey( m.Account, styleKey ) ),
				m.NetState );

			m.PlaySound( 0x041 );

			dropped.Delete();

			return true;
		}

		public class DragonEggGump : Gump
		{
			public DragonEggGump( Mobile from, DragonEgg egg ): base( 50, 50 )
			{
				from.SendSound( 0x4A ); 
				string color = "#94d3b4";
				string bodyKey = egg.DragonBody == 59
					? "prop.special.egg.dragon.gump.body.wyrm"
					: "prop.special.egg.dragon.gump.body.dragon";

				string sText = StringCatalog.ResolveByKey( from.Account, bodyKey );

				string sRumor;
				if ( egg.HavePotionA == 0 )
					sRumor = FormatIngredientRumor( from, "prop.special.egg.rumor.flame", egg.PieceRumor, egg.PieceLocation );
				else if ( egg.HavePotionB == 0 )
					sRumor = FormatIngredientRumor( from, "prop.special.egg.rumor.earth", egg.PieceRumor, egg.PieceLocation );
				else if ( egg.HavePotionC == 0 )
					sRumor = FormatIngredientRumor( from, "prop.special.egg.rumor.sea", egg.PieceRumor, egg.PieceLocation );
				else if ( egg.HavePotionD == 0 )
					sRumor = FormatIngredientRumor( from, "prop.special.egg.rumor.winds", egg.PieceRumor, egg.PieceLocation );
				else if ( egg.HaveGold < egg.NeedGold )
					sRumor = StringCatalog.ResolveByKey( from.Account, "prop.special.egg.gump.rumor.need.gold" );
				else
					sRumor = StringCatalog.ResolveByKey( from.Account, "prop.special.egg.gump.rumor.complete" );

				string title = LocalizeDisplayName( from, egg );
				string bring = StringCatalog.ResolveFormatByKey( from.Account, "prop.special.egg.gump.bring",
					LocalizeSavedPlace( from, egg.AnimalTrainerLocation ) );
				string goldLine = StringCatalog.ResolveFormatByKey( from.Account, "prop.special.egg.gump.gold",
					egg.HaveGold, egg.NeedGold );

				this.Closable=true;
				this.Disposable=true;
				this.Dragable=true;
				this.Resizable=false;

				AddPage(0);

				AddImage(0, 0, 7015, Server.Misc.PlayerSettings.GetGumpHue( from ));
				AddHtml( 12, 12, 420, 20, @"<BODY><BASEFONT Color=" + color + ">" + title + "</BASEFONT></BODY>", (bool)false, (bool)false);
				AddButton(863, 10, 4017, 4017, 0, GumpButtonType.Reply, 0);

				AddHtml( 12, 40, 173, 20, @"<BODY><BASEFONT Color=" + color + ">" + goldLine + "</BASEFONT></BODY>", (bool)false, (bool)false);
				AddHtml( 12, 70, 874, 20, @"<BODY><BASEFONT Color=" + color + ">" + bring + "</BASEFONT></BODY>", (bool)false, (bool)false);
				AddHtml( 12, 100, 874, 20, @"<BODY><BASEFONT Color=" + color + ">" + sRumor + "</BASEFONT></BODY>", (bool)false, (bool)false);

				AddHtml( 12, 339, 878, 251, @"<BODY><BASEFONT Color=" + color + ">" + sText + "</BASEFONT></BODY>", (bool)false, (bool)false);

				AddItem(708, 130, 11665, egg.Hue);

				AddItem(93, 210, 13042);
				AddItem(273, 210, 13042);
				AddItem(453, 210, 13042);
				AddItem(633, 210, 13042);

				if ( egg.HavePotionA > 0 ){ AddItem(105, 210, 10279, 0xB54); }
				if ( egg.HavePotionB > 0 ){ AddItem(285, 210, 10279, 0xB27); }
				if ( egg.HavePotionC > 0 ){ AddItem(465, 210, 10279, 0xB46); }
				if ( egg.HavePotionD > 0 ){ AddItem(645, 210, 10279, 0xB49); }
			}

			public override void OnResponse(NetState state, RelayInfo info)
			{
				Mobile from = state.Mobile;
				from.SendSound( 0x4A ); 
			}
		}

		public string AnimalTrainerLocation;
		[CommandProperty( AccessLevel.GameMaster )]
		public string g_AnimalTrainerLocation { get{ return AnimalTrainerLocation; } set{ AnimalTrainerLocation = value; } }

		public string PieceLocation;
		[CommandProperty( AccessLevel.GameMaster )]
		public string g_PieceLocation { get{ return PieceLocation; } set{ PieceLocation = value; } }

		public string PieceRumor;
		[CommandProperty( AccessLevel.GameMaster )]
		public string g_PieceRumor { get{ return PieceRumor; } set{ PieceRumor = value; } }

		public int DragonType;
		[CommandProperty( AccessLevel.GameMaster )]
		public int g_DragonType { get{ return DragonType; } set{ DragonType = value; } }

		public int DragonBody;
		[CommandProperty( AccessLevel.GameMaster )]
		public int g_DragonBody { get{ return DragonBody; } set{ DragonBody = value; } }

		// ----------------------------------------------------------------------------------------

		public int NeedGold;
		[CommandProperty( AccessLevel.GameMaster )]
		public int g_NeedGold { get{ return NeedGold; } set{ NeedGold = value; } }

		// ----------------------------------------------------------------------------------------

		public int HavePotionA;
		[CommandProperty( AccessLevel.GameMaster )]
		public int g_HavePotionA { get{ return HavePotionA; } set{ HavePotionA = value; } }

		public int HaveGold;
		[CommandProperty( AccessLevel.GameMaster )]
		public int g_HaveGold { get{ return HaveGold; } set{ HaveGold = value; } }

		public int HavePotionC;
		[CommandProperty( AccessLevel.GameMaster )]
		public int g_HavePotionC { get{ return HavePotionC; } set{ HavePotionC = value; } }

		public int HavePotionB;
		[CommandProperty( AccessLevel.GameMaster )]
		public int g_HavePotionB { get{ return HavePotionB; } set{ HavePotionB = value; } }

		public int HavePotionD;
		[CommandProperty( AccessLevel.GameMaster )]
		public int g_HavePotionD { get{ return HavePotionD; } set{ HavePotionD = value; } }
	}
}
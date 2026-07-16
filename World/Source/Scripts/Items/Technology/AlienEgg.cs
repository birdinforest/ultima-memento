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
	public class AlienEgg : Item
	{
		public override bool IsContentLocalized { get { return true; } }

		public override string DisplayNameLocalizationKey { get { return "item.special.alien.egg"; } }

		[Constructable]
		public AlienEgg() : base( 0x2D8E )
		{
			Weight = 4.0;
			Name = "Alien Egg";
			Technology = true;

			if ( Weight > 3.0 )
			{
				ItemID = Utility.RandomList( 0x2D8E, 0x2D8F );
				Weight = 3.0;

				HaveRod = 0;
				HaveYellowCrystal = 0;
				HaveRedCrystal = 0;
				HavePotion = 0;
				HaveXormite = 0;

				Hue = 0xBAB;

				NeedXormite = 10000;

				AnimalTrainerLocation = Server.Items.AlienEgg.GetRandomVet();

				PieceRumor = Server.Items.CubeOnCorpse.GetRumor();
				PieceLocation = Server.Items.CubeOnCorpse.PickDungeon();
			}
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( Weight > 2.0 && from.Map == Map.SavagedEmpire && from.X >= 1197 && from.Y >= 3655 && from.X <= 1218 && from.Y <= 3676 )
			{
				Weight = 1.0;
			}

			if ( Weight < 1.5 )
			{
				from.CloseGump( typeof( AlienEggGump ) );
				from.SendGump( new AlienEggGump( from, this ) );
			}
		}

		public override bool OnDragDrop( Mobile from, Item dropped )
		{
			int iAmount = 0;
			string sEnd = ".";

			if ( from != null && Weight < 1.5 )
			{
				if ( dropped is DDXormite && NeedXormite > HaveXormite )
				{
					int WhatIsDropped = dropped.Amount;
					int WhatIsNeeded = NeedXormite - HaveXormite;
					int WhatIsExtra = WhatIsDropped - WhatIsNeeded; if ( WhatIsExtra < 1 ){ WhatIsExtra = 0; }
					int WhatIsTaken = WhatIsDropped - WhatIsExtra;

					if ( WhatIsExtra > 0 ){ from.AddToBackpack( new DDXormite( WhatIsExtra ) ); }
					iAmount = WhatIsTaken;

					if ( iAmount > 1 ){ sEnd = "s."; }

					HaveXormite = HaveXormite + iAmount;
					from.SendMessage( StringCatalog.ResolveFormatByKey( from.Account, "prop.special.egg.xormite.added", iAmount.ToString(), sEnd ) );
					dropped.Delete();
					return true;
				}
			}

			return false;
		}

		public AlienEgg( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int)1 ); // version

			writer.Write( HaveRod );
			writer.Write( HaveYellowCrystal );
			writer.Write( HaveRedCrystal );
			writer.Write( HavePotion );
			writer.Write( HaveXormite );
			writer.Write( NeedXormite );
			writer.Write( AnimalTrainerLocation );
			writer.Write( PieceLocation );
			writer.Write( PieceRumor );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();

			HaveRod = reader.ReadInt();
			HaveYellowCrystal = reader.ReadInt();
			HaveRedCrystal = reader.ReadInt();
			HavePotion = reader.ReadInt();
			HaveXormite = reader.ReadInt();
			NeedXormite = reader.ReadInt();
			AnimalTrainerLocation = reader.ReadString();
			PieceLocation = reader.ReadString();
			PieceRumor = reader.ReadString();
		}

		public static bool ProcessAlienEgg( Mobile m, Mobile vet, Item dropped )
		{
			AlienEgg egg = (AlienEgg)dropped;

			if ( Server.Misc.Worlds.GetRegionName( vet.Map, vet.Location ) != egg.AnimalTrainerLocation ){ return false; }

			int vetSkill = (int)(m.Skills[SkillName.Veterinary].Value);
				if ( vetSkill > 100 ){ vetSkill = 100; }

			int XormiteReturn = 0;
				if ( vetSkill > 0 ){ XormiteReturn = (int)( egg.NeedXormite * ( vetSkill * 0.005 ) ); }

			int HaveIngredients = 0;

			if ( egg.HaveYellowCrystal >= 0 ){ HaveIngredients++; }
			if ( egg.HaveRedCrystal >= 0 ){ HaveIngredients++; }
			if ( egg.HavePotion >= 0 ){ HaveIngredients++; }
			if ( egg.HaveXormite >= egg.NeedXormite ){ HaveIngredients++; }
			if ( egg.HaveRod >= 0 ){ HaveIngredients++; }

			if ( HaveIngredients < 5 ){ return false; }

			if ( (m.Followers + 3) > m.FollowersMax )
			{
				vet.Say( StringCatalog.ResolveByKey( m.Account, "prop.special.egg.followers.too.many" ) );
				return false;
			}

			if ( XormiteReturn > 0 )
			{
				m.AddToBackpack( new DDXormite( XormiteReturn ) );
				vet.Say( StringCatalog.ResolveFormatByKey( m.Account, "prop.special.egg.xormite.refund", XormiteReturn.ToString() ) );
			}

			BaseCreature alien = new Alien();
			alien.Controlled = true;
			alien.ControlMaster = m;
			alien.IsBonded = true;
			alien.MoveToWorld( m.Location, m.Map );
			alien.ControlTarget = m;
			alien.Tamable = true;
			alien.MinTameSkill = 29.1;
			alien.ControlOrder = OrderType.Follow;

			LoggingFunctions.LogGenericQuest( m, "has hatched an alien" );

			m.PrivateOverheadMessage( MessageType.Regular, 1153, false,
				StringCatalog.ResolveByKey( m.Account, "prop.special.egg.alien.hatched" ),
				m.NetState );
			m.PlaySound( 0x041 );

			dropped.Delete();

			return true;
		}

		public class AlienEggGump : Gump
		{
			public AlienEggGump( Mobile from, AlienEgg egg ): base( 25, 25 )
			{
				string sText = StringCatalog.ResolveByKey( from.Account, "prop.special.egg.alien.gump.body" );

				string sRumor;
				if ( egg.HaveRod == 0 )
					sRumor = DragonEgg.FormatIngredientRumor( from, "prop.special.egg.rumor.rod", egg.PieceRumor, egg.PieceLocation );
				else if ( egg.HaveYellowCrystal == 0 )
					sRumor = DragonEgg.FormatIngredientRumor( from, "prop.special.egg.rumor.sun", egg.PieceRumor, egg.PieceLocation );
				else if ( egg.HaveRedCrystal == 0 )
					sRumor = DragonEgg.FormatIngredientRumor( from, "prop.special.egg.rumor.blood", egg.PieceRumor, egg.PieceLocation );
				else if ( egg.HavePotion == 0 )
					sRumor = DragonEgg.FormatIngredientRumor( from, "prop.special.egg.rumor.growth", egg.PieceRumor, egg.PieceLocation );
				else if ( egg.HaveXormite < egg.NeedXormite )
					sRumor = StringCatalog.ResolveByKey( from.Account, "prop.special.egg.gump.rumor.need.xormite" );
				else
					sRumor = StringCatalog.ResolveByKey( from.Account, "prop.special.egg.gump.rumor.complete" );

				string title = StringCatalog.ResolveByKey( from.Account, "prop.special.egg.alien.gump.title" );
				string bring = StringCatalog.ResolveFormatByKey( from.Account, "prop.special.egg.gump.bring",
					DragonEgg.LocalizeSavedPlace( from, egg.AnimalTrainerLocation ) );

				this.Closable=true;
				this.Disposable=true;
				this.Dragable=true;
				this.Resizable=false;

				AddPage(0);
				AddImage(0, 0, 30521);
				AddItem(574, 32, 14968);

				AddHtml( 50, 38, 207, 20, @"<BODY><BASEFONT Color=#00FF06>" + title + @"</BIG></BASEFONT></BODY>", (bool)false, (bool)false);

				AddItem(376, 36, 3823, 0xB96);
				AddHtml( 420, 38, 180, 20, @"<BODY><BASEFONT Color=#00FF06>" + egg.HaveXormite.ToString() + "/" + egg.NeedXormite.ToString() + "</BIG></BASEFONT></BODY>", (bool)false, (bool)false);

				AddHtml( 50, 70, 520, 60, @"<BODY><BASEFONT Color=#00FF06>" + sRumor + "</BIG></BASEFONT></BODY>", (bool)false, (bool)false);

				AddItem(41, 237, 3000);
				AddHtml( 85, 242, 622, 20, @"<BODY><BASEFONT Color=#00FF06>" + bring + "</BIG></BASEFONT></BODY>", (bool)false, (bool)false);

				AddItem(85, 145, 8893);
				if ( egg.HaveRod > 0 ){ AddItem(84, 156, 3571, 0xB71); }

				AddItem(235, 145, 8893);
				if ( egg.HaveYellowCrystal > 0 ){ AddItem(236, 144, 1796); }

				AddItem(385, 145, 8893);
				if ( egg.HaveRedCrystal > 0 ){ AddItem(387, 143, 1797); }

				AddItem(535, 145, 8893);
				if ( egg.HavePotion > 0 ){ AddItem(546, 145, 10279, 0xB3D); }

				AddHtml( 50, 289, 665, 319, @"<BODY><BASEFONT Color=#00FF06>" + sText + "</BIG></BASEFONT></BODY>", (bool)false, (bool)false);
			}
		}

		public static string GetRandomVet()
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
					if ( ( target is AnimalTrainer || target is Veterinarian ) && reg.IsPartOf( typeof( VillageRegion ) ))
					{
						targets.Add( target ); aCount++;
					}
				}
			}

			aCount = Utility.RandomMinMax( 1, aCount );

			int xCount = 0;
			for ( int i = 0; i < targets.Count; ++i )
			{
				Mobile vet = ( Mobile )targets[ i ];
				xCount++;

				if ( xCount == aCount )
				{
					sRegion = Server.Misc.Worlds.GetRegionName( vet.Map, vet.Location );
				}
			}

			return sRegion;
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

		// ----------------------------------------------------------------------------------------

		public int NeedXormite;
		[CommandProperty( AccessLevel.GameMaster )]
		public int g_NeedXormite { get{ return NeedXormite; } set{ NeedXormite = value; } }

		// ----------------------------------------------------------------------------------------

		public int HaveRod;
		[CommandProperty( AccessLevel.GameMaster )]
		public int g_HaveRod { get{ return HaveRod; } set{ HaveRod = value; } }

		public int HaveXormite;
		[CommandProperty( AccessLevel.GameMaster )]
		public int g_HaveXormite { get{ return HaveXormite; } set{ HaveXormite = value; } }

		public int HaveRedCrystal;
		[CommandProperty( AccessLevel.GameMaster )]
		public int g_HaveRedCrystal { get{ return HaveRedCrystal; } set{ HaveRedCrystal = value; } }

		public int HaveYellowCrystal;
		[CommandProperty( AccessLevel.GameMaster )]
		public int g_HaveYellowCrystal { get{ return HaveYellowCrystal; } set{ HaveYellowCrystal = value; } }

		public int HavePotion;
		[CommandProperty( AccessLevel.GameMaster )]
		public int g_HavePotion { get{ return HavePotion; } set{ HavePotion = value; } }
	}
}

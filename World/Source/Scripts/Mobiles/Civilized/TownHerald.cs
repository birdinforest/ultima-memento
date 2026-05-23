using System;
using System.Collections;
using Server.ContextMenus;
using System.Collections.Generic;
using Server.Misc;
using Server.Network;
using Server;
using Server.Items;
using Server.Gumps;
using Server.Mobiles;
using Server.Regions;
using Server.Commands;
using Server.Localization;

namespace Server.Mobiles
{
    public class TownHerald : BasePerson
	{
		private DateTime m_NextTalk;
		public DateTime NextTalk{ get{ return m_NextTalk; } set{ m_NextTalk = value; } }

		public override bool ClickTitle{ get{ return false; } }

		[Constructable]
		public TownHerald() : base( )
		{
			NameHue = -1;

			InitStats( 100, 100, 25 );

			Title = "the town crier";
			Hue = Utility.RandomSkinColor();

			AddItem( new FancyShirt( Utility.RandomBlueHue() ) );

			Item skirt;

			switch ( Utility.Random( 2 ) )
			{
				case 0: skirt = new Skirt(); break;
				default: case 1: skirt = new Kilt(); break;
			}

			skirt.Hue = Utility.RandomGreenHue();

			AddItem( skirt );

			AddItem( new FeatheredHat( Utility.RandomGreenHue() ) );

			Item boots;

			switch ( Utility.Random( 2 ) )
			{
				case 0: boots = new Boots(); break;
				default: case 1: boots = new ThighBoots(); break;
			}

			AddItem( boots );
			AddItem( new LightCitizen( true ) );

			Utility.AssignRandomHair( this );
		}

		public override void OnMovement( Mobile m, Point3D oldLocation )
		{
			if ( DateTime.Now >= m_NextTalk && InRange( m, 10 ) && m is PlayerMobile )
			{
				string shoutEventId;

				if ( EventSystem.RollTowncrierShoutChance( out shoutEventId ) )
				{
					ShoutActiveEventPhase( this, shoutEventId );
				}
				else if ( LoggingFunctions.LoggingEvents() == true )
				{
					if ( Utility.RandomMinMax( 1, 4 ) == 1 )
						randomShout( this );
					else
					{
						string sEvents = LoggingFunctions.LogShout();
						Say( sEvents );
					}
				}
				else
					randomShout( this );

				m_NextTalk = DateTime.Now + TimeSpan.FromSeconds( Utility.RandomMinMax( 15, 30 ) );
			}
		}

		public static void ShoutActiveEventPhase( Mobile herald, string eventId )
		{
			if ( herald == null || eventId == null || eventId.Length == 0 )
				return;

			if ( Insensitive.Equals( eventId, EventSystem.ThinningVeilEventIdConst ) )
				CitizenLocalization.SayLocalized( herald, "Hear ye! Chroniclers quarrel—the ruin of Exodus is told two ways at once; mind which ink stains thy map!" );
			else
				CitizenLocalization.SayLocalized( herald, "Hark! Something stirs abroad—heed the road and the rumours of this season!" );
		}

		static bool TryGrantEventPhaseLoreBook( Mobile herald, PlayerMobile pm )
		{
			if ( herald == null || pm == null )
				return false;

			foreach ( string eid in EventSystem.GetActiveEventIds() )
			{
				if ( !EventSystem.IsEventActive( eid ) || !EventSystem.RollTowncrierBookChanceForEvent( eid ) )
					continue;

				if ( Insensitive.Equals( eid, EventSystem.ThinningVeilEventIdConst ) )
				{
					if ( LoreBook.PlayerOwnsThinningVeil( pm ) )
						continue;

					pm.AddToBackpack( new LoreBook( 200 ) );
					herald.SayTo( pm, false, StringCatalog.Resolve( pm.Account, "Take this chronicle, friend—it may steady thy step where stories overlap." ) );
					return true;
				}
			}

			return false;
		}

		public static string randomShout( Mobile m )
		{
			string shout = "";

			string greet = "Hear ye, hear ye! ";
				switch ( Utility.RandomMinMax( 0, 3 ) )
				{
					case 1: greet = "Everyone listen! "; 			break;
					case 2: greet = "All hail and hear my words! "; break;
					case 3: greet = "Your attention please! "; 		break;
				};
				if ( m == null ){ greet = ""; }

			string name = NameList.RandomName( "female" );
				if ( Utility.RandomBool() ){ name = NameList.RandomName( "male" ); }
				name = name + " " + TavernPatrons.GetTitle();

			string seen = "was seen in"; 
			switch ( Utility.RandomMinMax( 0, 6 ) )
			{
				case 1: seen = "was found in"; 			break;
				case 2: seen = "was spotted in"; 		break;
				case 3: seen = "was found near"; 		break;
				case 4: seen = "was spotted near"; 		break;
				case 5: seen = "was found leaving"; 	break;
				case 6: seen = "was spotted leaving"; 	break;
			}

			string city = RandomThings.GetRandomCity();	
				if ( Utility.RandomBool() ){ city = RandomThings.MadeUpCity(); }

			string dungeon = QuestCharacters.SomePlace( "tavern" );	
				if ( Utility.RandomBool() ){ dungeon = RandomThings.MadeUpDungeon(); }

			string place = dungeon;
				if ( Utility.RandomBool() ){ place = city; }

			string slain = "has destroyed";
			switch( Utility.RandomMinMax( 0, 3 ) )
			{
				case 1: slain = "has defeated";		break;
				case 2: slain = "has slain";		break;
				case 3: slain = "has vanquished";	break;
			}

			string died = "was killed";
			switch( Utility.RandomMinMax( 0, 3 ) )
			{
				case 1: died = "was slain";		break;
				case 2: died = "was bested";	break;
				case 3: died = "was beaten";	break;
			}

			string death = "was killed";
			switch( Utility.RandomMinMax( 0, 3 ) )
			{
				case 1: death = "has perished";			break;
				case 2: death = "has met their end";	break;
				case 3: death = "was slain";			break;
			}

			string crime = "murder";
			switch( Utility.RandomMinMax( 0, 6 ) )
			{
				case 1: crime = "witchcraft";	break;
				case 2: crime = "thievery";		break;
				case 3: crime = "heresy";		break;
				case 4: crime = "blasphemy";	break;
				case 5: crime = "treason";		break;
				case 6: crime = "robbery";		break;
			}

			string item = "destroyed";	
			switch( Utility.RandomMinMax( 0, 4 ) )
			{
				case 1: item = "lost"; break;	
				case 2: item = "found"; break;	
				case 3: item = "discovered"; break;	
				case 4: item = "stole"; break;	
			}

			switch ( Utility.RandomMinMax( 0, 5 ) )
			{
				case 0: shout = "" + greet + "" + name + " " + seen + " " + place + ""; break;
				case 1: shout = "" + greet + "" + name + " " + slain + " " + RandomThings.GetRandomMonsters() + ""; break;
				case 2: shout = "" + greet + "" + name + " " + died + " by " + RandomThings.GetRandomMonsters() + ""; break;
				case 3: shout = "" + greet + "" + name + " " + death + " in " + dungeon + ""; break;
				case 4: shout = "" + greet + "" + name + " is wanted for " + crime + " in " + city + ""; break;
				case 5: shout = "" + greet + "" + name + " " + item + " " + QuestCharacters.QuestItems( false ) + ""; 
					if ( Utility.RandomBool() ){ shout = "" + greet + "" + name + " " + item + " " + QuestCharacters.ArtyItems( false ) + ""; }
					break;
			}

			if ( m != null ){ shout = shout + "!"; m.Say( shout ); }

			return shout;
		}

        public TownHerald(Serial serial) : base(serial)
		{
		}

		public override void GetContextMenuEntries( Mobile from, List<ContextMenuEntry> list ) 
		{ 
			base.GetContextMenuEntries( from, list ); 
			list.Add( new TownHeraldEntry( from, this ) ); 
		} 

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); 
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
		
		public class TownHeraldEntry : ContextMenuEntry
		{
			private Mobile m_Mobile;
			private Mobile m_Giver;
			
			public TownHeraldEntry( Mobile from, Mobile giver ) : base( 6146, 3 )
			{
				m_Mobile = from;
				m_Giver = giver;
			}

			public override void OnClick()
			{
			    if( !( m_Mobile is PlayerMobile ) )
				return;
				
				PlayerMobile mobile = (PlayerMobile) m_Mobile;
				{
					if ( LoggingFunctions.LoggingEvents() == true )
					{
						if ( ! mobile.HasGump( typeof( LoggingGumpCrier ) ) )
						{
							mobile.SendGump(new LoggingGumpCrier( mobile, 1 ));
						}
					}
					else if ( !TryGrantEventPhaseLoreBook( m_Giver, mobile ) )
					{
						m_Giver.SayTo( m_Mobile, false, StringCatalog.ResolveFormat( m_Mobile.Account, "Good day to you, {0}.", m_Mobile.Name ) );
					}
				}
            }
        }
	}  
}
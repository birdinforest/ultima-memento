using System;
using Server;
using Server.Mobiles;
using Server.Localization;

namespace Server.Items
{
	public enum MoonPhase
	{
		NewMoon,
		WaxingCrescentMoon,
		FirstQuarter,
		WaxingGibbous,
		FullMoon,
		WaningGibbous,
		LastQuarter,
		WaningCrescent
	}

	[Flipable( 0x104B, 0x104C )]
	public class Clock : Item
	{
		private static DateTime m_ServerStart;

		public static DateTime ServerStart
		{
			get{ return m_ServerStart; }
		}

		public static void Initialize()
		{
			m_ServerStart = DateTime.Now;
		}

		[Constructable]
		public Clock() : this( 0x104B )
		{
		}

		[Constructable]
		public Clock( int itemID ) : base( itemID )
		{
			Weight = 3.0;
		}

		public Clock( Serial serial ) : base( serial )
		{
		}

		public const double SecondsPerUOMinute = 5.0;
		public const double MinutesPerUODay = SecondsPerUOMinute * 24;

		private static DateTime WorldStart = new DateTime( 1997, 9, 1 );

		public static MoonPhase GetMoonPhase( Map map, int x, int y )
		{
			x = 100; y = 100; map = Map.Sosaria;
			int hours, minutes, totalMinutes;

			GetTime( map, x, y, out hours, out minutes, out totalMinutes );

			if ( map != null )
				totalMinutes /= 10 + (map.MapIndex * 20);

			return (MoonPhase)(totalMinutes % 8);
		}

		public static void GetTime( Map map, int x, int y, out int hours, out int minutes )
		{
			x = 100; y = 100; map = Map.Sosaria;
			int totalMinutes;

			GetTime( map, x, y, out hours, out minutes, out totalMinutes );
		}

		public static void GetTime( Map map, int x, int y, out int hours, out int minutes, out int totalMinutes )
		{
			x = 100; y = 100; map = Map.Sosaria;
			TimeSpan timeSpan = DateTime.Now - WorldStart;

			totalMinutes = (int)(timeSpan.TotalSeconds / SecondsPerUOMinute);

			if ( map != null )
				totalMinutes += map.MapIndex * 320;

			// Really on OSI this must be by subserver
			totalMinutes += x / 16;

			hours = (totalMinutes / 60) % 24;
			minutes = totalMinutes % 60;
		}

		public static void GetTime( out int generalNumber, out string exactTime )
		{
			GetTime( null, 0, 0, out generalNumber, out exactTime );
		}

		public static void GetTime( Mobile from, out int generalNumber, out string exactTime )
		{
			//GetTime( from.Map, from.X, from.Y, out generalNumber, out exactTime );
			GetTime( Map.Sosaria, 100, 100, out generalNumber, out exactTime );
		}

		public static void GetTime( Map map, int x, int y, out int generalNumber, out string exactTime )
		{
			x = 100; y = 100; map = Map.Sosaria;
			int hours, minutes;

			GetTime( map, x, y, out hours, out minutes );

			// 00:00 AM - 00:59 AM : Witching hour
			// 01:00 AM - 03:59 AM : Middle of night
			// 04:00 AM - 07:59 AM : Early morning
			// 08:00 AM - 11:59 AM : Late morning
			// 12:00 PM - 12:59 PM : Noon
			// 01:00 PM - 03:59 PM : Afternoon
			// 04:00 PM - 07:59 PM : Early evening
			// 08:00 PM - 11:59 AM : Late at night

			if ( hours >= 20 )
				generalNumber = 1042957; // It's late at night
			else if ( hours >= 16 )
				generalNumber = 1042956; // It's early in the evening
			else if ( hours >= 13 )
				generalNumber = 1042955; // It's the afternoon
			else if ( hours >= 12 )
				generalNumber = 1042954; // It's around noon
			else if ( hours >= 08 )
				generalNumber = 1042953; // It's late in the morning
			else if ( hours >= 04 )
				generalNumber = 1042952; // It's early in the morning
			else if ( hours >= 01 )
				generalNumber = 1042951; // It's the middle of the night
			else
				generalNumber = 1042950; // 'Tis the witching hour. 12 Midnight.

			hours %= 12;

			if ( hours == 0 )
				hours = 12;

			exactTime = String.Format( "{0}:{1:D2}", hours, minutes );
		}

		public override void OnDoubleClick( Mobile from )
		{
			int genericNumber;
			string exactTime;

			GetTime( from, out genericNumber, out exactTime );

			SendLocalizedMessageTo( from, genericNumber );
			SendLocalizedMessageTo( from, 1042958, exactTime ); // ~1_TIME~ to be exact
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			if ( Weight == 2.0 )
				Weight = 3.0;
		}
	}

	[Flipable( 0x104B, 0x104C )]
	public class ClockRight : Clock
	{
		[Constructable]
		public ClockRight() : base( 0x104B )
		{
		}

		public ClockRight( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}

	[Flipable( 0x104B, 0x104C )]
	public class ClockLeft : Clock
	{
		[Constructable]
		public ClockLeft() : base( 0x104C )
		{
		}

		public ClockLeft( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}

	/// <summary>
	/// Decorative relic grandfather clocks: randomized English <see cref="Item.Name" /> for saves/tooltips;
	/// bilingual OPL uses <c>prop.trade.relicclock.*</c> shotkeys and persists adjective index (save version 2).
	/// </summary>
	public abstract class DDRelicClockBase : Clock, IRelic
	{
		protected int m_RelicClockAdjIndex;

		private static readonly string[] EnglishRelicClockAdjectives =
		{
			"a rare", "a nice", "a pretty", "a superb", "a delightful",
			"an elegant", "an exquisite", "a fine", "a gorgeous", "a lovely",
			"a magnificent", "a marvelous", "a splendid", "a wonderful", "an extraordinary",
			"a strange", "an odd", "a unique", "an unusual"
		};

		private const string EnglishGrandfatherSuffix = " grandfather clock";

		private void InferRelicClockAdjFromLegacyName()
		{
			string name = Name;

			if ( name == null || !name.EndsWith( EnglishGrandfatherSuffix ) )
			{
				m_RelicClockAdjIndex = 0;
				return;
			}

			string prefix = name.Substring( 0, name.Length - EnglishGrandfatherSuffix.Length );

			for ( int i = 0; i < EnglishRelicClockAdjectives.Length; ++i )
			{
				if ( EnglishRelicClockAdjectives[i] == prefix )
				{
					m_RelicClockAdjIndex = i;
					return;
				}
			}

			m_RelicClockAdjIndex = 0;
		}

		private static string BuildEnglishGrandfatherClockName( int adjIndex )
		{
			if ( adjIndex < 0 || adjIndex >= EnglishRelicClockAdjectives.Length )
				adjIndex = 0;

			return EnglishRelicClockAdjectives[adjIndex] + EnglishGrandfatherSuffix;
		}

		public override bool IsContentLocalized { get { return true; } }

		protected DDRelicClockBase( int itemID ) : base( itemID )
		{
			Weight = 100;

			CoinPrice = Utility.RandomMinMax( 80, 500 );
			NotIdentified = true;
			NotIDSource = Identity.Merchant;
			NotIDSkill = IDSkill.Mercantile;

			m_RelicClockAdjIndex = Utility.RandomMinMax( 0, EnglishRelicClockAdjectives.Length - 1 );
			Name = BuildEnglishGrandfatherClockName( m_RelicClockAdjIndex );
		}

		public DDRelicClockBase( Serial serial ) : base( serial )
		{
		}

		public override void ItemIdentified( bool id )
		{
			m_NotIdentified = id;

			if ( !id )
				ColorHue3 = "FDC844";
		}

		public override void AddNameProperty( ObjectPropertyList list )
		{
			if ( BuildingPropertyListLocale != null && m_RelicClockAdjIndex >= 0 && m_RelicClockAdjIndex < EnglishRelicClockAdjectives.Length )
			{
				string adj = ResolvePropertyText( "prop.trade.relicclock.adj." + m_RelicClockAdjIndex );
				string fmt = ResolvePropertyText( "prop.trade.relicclock.name.fmt" );
				string displayName = string.Format( fmt, adj );

				if ( m_Amount <= 1 )
					list.Add( displayName );
				else
					list.Add( 1050039, "{0}\t{1}", m_Amount, displayName );

				return;
			}

			base.AddNameProperty( list );
		}

		protected override void AddColorText3Property( ObjectPropertyList list, string colorHue3 )
		{
			if ( NotIdentified || CoinPrice <= 0 )
				return;

			string worthText;

			if ( BuildingPropertyListLocale != null )
				worthText = string.Format( ResolvePropertyText( "prop.trade.relicclock.worth" ), CoinPrice );
			else
				worthText = "Worth " + CoinPrice + " Gold";

			list.Add( 1072173, "{0}\t{1}", colorHue3, worthText );
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( !IsChildOf( from.Backpack ) && MySettings.S_IdentifyItemsOnlyInPack && from is PlayerMobile && ((PlayerMobile)from).Preferences.DoubleClickID && NotIdentified )
				from.SendMessage( StringCatalog.ResolveByKey( from.Account, "prop.trade.clocks.backpack.identify" ) );
			else if ( from is PlayerMobile && ((PlayerMobile)from).Preferences.DoubleClickID && NotIdentified )
				IDCommand( from );
			else
				base.OnDoubleClick( from );
		}

		public override void IDCommand( Mobile m )
		{
			if ( NotIDSkill == IDSkill.Tasting )
				RelicFunctions.IDItem( m, m, this, SkillName.Tasting );
			else if ( NotIDSkill == IDSkill.ArmsLore )
				RelicFunctions.IDItem( m, m, this, SkillName.ArmsLore );
			else
				RelicFunctions.IDItem( m, m, this, SkillName.Mercantile );
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 2 ); // version
			writer.Write( (int) m_RelicClockAdjIndex );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			if ( version >= 2 )
				m_RelicClockAdjIndex = reader.ReadInt();
			else
			{
				if ( version < 1 )
					CoinPrice = reader.ReadInt();

				InferRelicClockAdjFromLegacyName();
			}

			ColorText3 = null;
		}
	}

	[Flipable( 0x44D5, 0x44D9 )]
	public class DDRelicClock1 : DDRelicClockBase
	{
		[Constructable]
		public DDRelicClock1() : base( 0x44D5 )
		{
		}

		public DDRelicClock1( Serial serial ) : base( serial )
		{
		}
	}

	[Flipable( 0x44DD, 0x44E1 )]
	public class DDRelicClock2 : DDRelicClockBase
	{
		[Constructable]
		public DDRelicClock2() : base( 0x44DD )
		{
		}

		public DDRelicClock2( Serial serial ) : base( serial )
		{
		}
	}

	[Flipable( 0x48D4, 0x48D8 )]
	public class DDRelicClock3 : DDRelicClockBase
	{
		[Constructable]
		public DDRelicClock3() : base( 0x48D4 )
		{
		}

		public DDRelicClock3( Serial serial ) : base( serial )
		{
		}
	}
}
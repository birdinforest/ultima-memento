using System;
using System.Text;
using Server.Accounting;
using Server.Network;
using Server.Gumps;
using Server.Misc;
using Server.Localization;

namespace Server.Items
{
	public class SOS : Item
	{
		public override int LabelNumber
		{
			get
			{
				if ( IsAncient )
					return 1063450; // an ancient SOS

				return 1041081; // a waterstained SOS
			}
		}

		private int m_Level;
		private Map m_TargetMap;
		private Point3D m_TargetLocation;
		public Land MapWorld;
		public string ShipStory;
		public string ShipName;

		private bool m_StructuredStory;
		private int m_StoryTemplate;
		private int m_BeastIndex;
		private string m_CompanionName;
		private string m_CityName;
		private int m_SurvivorCount;

		[CommandProperty( AccessLevel.GameMaster )]
		public bool IsAncient { get{ return ( m_Level >= 4 ); } }

		[CommandProperty( AccessLevel.GameMaster )]
		public int Level
		{
			get{ return m_Level; }
			set
			{
				m_Level = Math.Max( 1, Math.Min( value, 4 ) );
				UpdateHue();
				InvalidateProperties();
			}
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public Map TargetMap { get{ return m_TargetMap; } set{ m_TargetMap = value; } }

		[CommandProperty( AccessLevel.GameMaster )]
		public Point3D TargetLocation { get{ return m_TargetLocation; } set{ m_TargetLocation = value; } }

		[CommandProperty(AccessLevel.Owner)]
		public Land Map_World { get { return MapWorld; } set { MapWorld = value; InvalidateProperties(); } }

		[CommandProperty(AccessLevel.Owner)]
		public string Ship_Story { get { return ShipStory; } set { ShipStory = value ?? ""; m_StructuredStory = false; InvalidateProperties(); } }

		[CommandProperty(AccessLevel.Owner)]
		public string Ship_Name { get { return ShipName; } set { ShipName = value; InvalidateProperties(); } }

		public void UpdateHue()
		{
			if ( IsAncient )
				Hue = Utility.RandomList( 0xB8E, 0xB8F, 0xB90, 0xB91, 0xB92, 0xB89, 0xB8B );
			else
				Hue = 0;
		}

		[Constructable]
		public SOS( Land land, int level ) : base( 0x14ED )
		{
			if ( level < 1 ){ level = MessageInABottle.GetRandomLevel(); }

			if ( land == Land.SkaraBrae ){ land = Land.Sosaria; } // NO SOSs IN SKARA BRAE
			else if ( land == Land.Luna ){ land = Land.Sosaria; } // NO SOSs ON THE MOON
			else if ( land == Land.Underworld ){ land = Land.Sosaria; } // NO SOSs IN THE UNDERWORLD

			Weight = 1.0;

			Point3D loc = Worlds.GetRandomLocation( land, "sea" );
			Map map = Worlds.GetMyDefaultTreasureMap( land );

			MapWorld = land;
			m_Level = level;
			m_TargetMap = map;

			m_TargetLocation = loc;

			UpdateHue();

			ShipName = RandomThings.GetRandomShipName( "", 0 );

			m_StructuredStory = true;
			m_StoryTemplate = Utility.Random( 5 );
			m_BeastIndex = Utility.Random( 12 );
			m_CompanionName = QuestCharacters.ParchmentWriter();
			m_CityName = "";
			m_SurvivorCount = 0;

			if ( m_StoryTemplate == 2 || m_StoryTemplate == 4 )
				m_CityName = RandomThings.GetRandomCity();
			if ( m_StoryTemplate == 3 )
				m_SurvivorCount = Utility.RandomMinMax( 3, 16 );

			ShipStory = "";
		}

		public SOS( Serial serial ) : base( serial )
		{
		}

		private string BuildDisplayStory( Mobile from )
		{
			if ( from == null )
				return ShipStory ?? "";

			if ( !m_StructuredStory )
				return QuestCompositeResolver.ResolveComposite( from, ShipStory ?? "" );

			IAccount acct = from.Account;

			int tpl = m_StoryTemplate;
			if ( tpl < 0 || tpl > 4 )
				tpl = 0;

			int beastIx = m_BeastIndex;
			if ( beastIx < 0 || beastIx > 11 )
				beastIx = 0;

			var sb = new StringBuilder();

			if ( IsAncient )
				sb.Append( StringCatalog.ResolveByKey( acct, "prop.trade.sos.prefix.ancient" ) );

			string land = Server.Lands.LandName( MapWorld );
			string ship = ShipName ?? "";
			string beast = StringCatalog.ResolveByKey( acct, "prop.trade.sos.beast." + beastIx.ToString() );
			string writer = m_CompanionName ?? "";
			string city = m_CityName ?? "";

			string bodyKey = "prop.trade.sos.story." + tpl.ToString();
			string body;
			switch ( tpl )
			{
				case 0:
					body = StringCatalog.ResolveFormatByKey( acct, bodyKey, land, beast, ship );
					break;
				case 1:
					body = StringCatalog.ResolveFormatByKey( acct, bodyKey, beast, ship, land, writer );
					break;
				case 2:
					body = StringCatalog.ResolveFormatByKey( acct, bodyKey, ship, writer, land, writer, city );
					break;
				case 3:
					body = StringCatalog.ResolveFormatByKey( acct, bodyKey, ship, land, m_SurvivorCount, writer );
					break;
				case 4:
					body = StringCatalog.ResolveFormatByKey( acct, bodyKey, writer, ship, land, beast, city );
					break;
				default:
					body = "";
					break;
			}

			sb.Append( body );
			return QuestCompositeResolver.ResolveComposite( from, sb.ToString() );
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int)6 ); // version
			writer.Write( m_Level );
			writer.Write( m_TargetMap );
			writer.Write( m_TargetLocation );
			writer.Write( (int)MapWorld );
			writer.Write( ShipName );

			if ( !m_StructuredStory )
			{
				writer.Write( 0 );
				writer.Write( ShipStory ?? "" );
			}
			else
			{
				writer.Write( 1 );
				writer.Write( m_StoryTemplate );
				writer.Write( m_BeastIndex );
				writer.Write( m_CompanionName ?? "" );
				writer.Write( m_CityName ?? "" );
				writer.Write( m_SurvivorCount );
			}
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
			m_Level = reader.ReadInt();
			m_TargetMap = reader.ReadMap();
			m_TargetLocation = reader.ReadPoint3D();

			if ( version < 5 )
				MapWorld = Server.Lands.LandRef( reader.ReadString() );
			else
				MapWorld = (Land)(reader.ReadInt());

			ShipName = reader.ReadString();

			if ( version >= 6 )
			{
				int storyFmt = reader.ReadInt();
				if ( storyFmt == 0 )
				{
					m_StructuredStory = false;
					ShipStory = reader.ReadString();
				}
				else
				{
					m_StructuredStory = true;
					m_StoryTemplate = reader.ReadInt();
					m_BeastIndex = reader.ReadInt();
					m_CompanionName = reader.ReadString();
					m_CityName = reader.ReadString();
					m_SurvivorCount = reader.ReadInt();
					ShipStory = "";
				}
			}
			else
			{
				ShipStory = reader.ReadString();
				m_StructuredStory = false;
			}
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( IsChildOf( from.Backpack ) )
			{
				from.CloseGump( typeof( MessageGump ) );
				from.SendGump( new MessageGump( m_TargetMap, m_TargetLocation, Server.Lands.LandName( MapWorld ), BuildDisplayStory( from ), from ) );
				from.PlaySound( 0x249 );
			}
			else
			{
				from.SendLocalizedMessage( 1042001 ); // That must be in your pack for you to use it.
			}
		}

		private class MessageGump : Gump
		{
			private Map m_Map;
			private Point3D m_Loc;
			private string m_World;
			private string m_Story;

			public MessageGump( Map map, Point3D loc, string world, string story, Mobile from ) : base( 100, 100 )
			{
				m_Map = map;
				m_Loc = loc;
				m_World = world;
				m_Story = story;

				int xLong = 0, yLat = 0;
				int xMins = 0, yMins = 0;
				bool xEast = false, ySouth = false;
				string fmt;

				IAccount acct = from != null ? from.Account : null;

				if ( Sextant.Format( loc, map, ref xLong, ref yLat, ref xMins, ref yMins, ref xEast, ref ySouth ) )
					fmt = StringCatalog.ResolveFormatByKey( acct, "prop.trade.sos.coords.fmt", yLat, yMins, ySouth ? "S" : "N", xLong, xMins, xEast ? "E" : "W" );
				else
					fmt = StringCatalog.ResolveByKey( acct, "prop.trade.sos.coords.unknown" );

				this.Closable=true;
				this.Disposable=true;
				this.Dragable=true;
				this.Resizable=false;

				AddPage(0);

				AddImage(0, 0, 10901, 2800);
				AddImage(0, 0, 10899, 2750);
				AddHtml( 37, 76, 406, 222, @"<BODY><BASEFONT Color=#9dc1d5>" + story + "</BASEFONT></BODY>", (bool)false, (bool)false);
				AddHtml( 62, 326, 347, 20, @"<BODY><BASEFONT Color=#9dc1d5>" + fmt + "</BASEFONT></BODY>", (bool)false, (bool)false);

				if ( Sextants.HasSextant( from ) )
					AddButton(377, 325, 10461, 10461, 1, GumpButtonType.Reply, 0);
			}

			public override void OnResponse( NetState state, RelayInfo info )
			{
				Mobile from = state != null ? state.Mobile : null;

				if ( from == null )
					return;

				if ( info.ButtonID > 0 )
				{
					from.CloseGump( typeof( Sextants.MapGump ) );
					from.SendGump( new Sextants.MapGump( from, m_Map, m_Loc.X, m_Loc.Y, null ) );
					from.SendGump( new MessageGump( m_Map, m_Loc, m_World, m_Story, from ) );
				}
				else
				{
					from.PlaySound( 0x249 );
					from.CloseGump( typeof( Sextants.MapGump ) );
				}
			}
		}
	}
}

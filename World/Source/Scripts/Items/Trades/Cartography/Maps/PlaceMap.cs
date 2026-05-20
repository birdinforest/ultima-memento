using System;
using Server;
using Server.Items;
using Server.Localization;
using Server.Misc;
using Server.Mobiles;

namespace Server.Items
{
	public class PlaceMap : Item
	{
		public const string MapNamePrefix = "Map to ";

		public override string DefaultDescription{ get{ return StringCatalog.Resolve( null, "These maps show a faint image of a particular world and the location of a particular place. If you happen to be traveling in that world, you will see a pin that indicates where you are." ); } }
		public override string InfoDataLocalizationKey { get { return "prop.trade.itemdesc.placemap"; } }

		public override bool IsContentLocalized => true;

		public override void AddNameProperty( ObjectPropertyList list )
		{
			string locale = BuildingPropertyListLocale;

			if ( locale != null && m_TargetPlaceLabel != null && m_TargetPlaceLabel.Length > 0 )
			{
				string place = StringCatalog.TryResolve( locale, m_TargetPlaceLabel ) ?? m_TargetPlaceLabel;
				string fmt = StringCatalog.TryResolveByKey( locale, "placemap.name.format" );

				if ( fmt == null || fmt.Length == 0 )
					fmt = "{0}{1}";

				string full = AccountLang.IsChinese( locale ) ? string.Format( fmt, place ) : string.Format( fmt, MapNamePrefix, place );

				if ( Amount > 1 )
					list.Add( "{0} {1}", Amount, full );
				else
					list.Add( full );

				return;
			}

			base.AddNameProperty( list );
		}

		public Map WorldMap;
		public int WorldX;
		public int WorldY;

		private string m_TargetPlaceLabel;

		[Constructable]
		public PlaceMap() : base( 0x14EB )
		{
			Weight = 1.0;
			ItemID = Utility.RandomList( 0x14EB, 0x14EC );
			Hue = 0xB80;
			Name = "map";

			if ( WorldX == 0 )
			{
				if ( Utility.Random(5) > 0 )
					m_TargetPlaceLabel = Worlds.GetAreaEntrance( Utility.RandomMinMax(1,85), null, Map.Internal, out WorldMap, out WorldX, out WorldY );
				else
					m_TargetPlaceLabel = Worlds.GetTown( Utility.RandomMinMax(1,28), null, out WorldMap, out WorldX, out WorldY );

				Name = MapNamePrefix + m_TargetPlaceLabel;
			}
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( from.InRange( this.GetWorldLocation(), 4 ) )
			{
				from.CloseGump( typeof( Sextants.MapGump ) );
				from.SendGump( new Sextants.MapGump( from, WorldMap, WorldX, WorldY, this ) );
				from.PlaySound( 0x249 );
			}
			else
			{
				from.SendLocalizedMessage( 502138 ); // That is too far away for you to use
			}
		}

		public PlaceMap( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 1 );
			writer.Write( m_TargetPlaceLabel ?? "" );
			writer.Write( WorldMap );
			writer.Write( WorldX );
			writer.Write( WorldY );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();

			if ( version >= 1 )
				m_TargetPlaceLabel = reader.ReadString();
			else
				m_TargetPlaceLabel = null;

			WorldMap = reader.ReadMap();
			WorldX = reader.ReadInt();
			WorldY = reader.ReadInt();

			if ( version < 1 && Name != null && Name.StartsWith( MapNamePrefix, StringComparison.Ordinal ) )
				m_TargetPlaceLabel = Name.Substring( MapNamePrefix.Length );
		}
	}
}

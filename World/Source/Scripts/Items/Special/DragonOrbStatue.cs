using System;
using Server; 
using Server.Misc;
using Server.Localization;

namespace Server.Items
{
	[Flipable( 0x5467, 0x5468 )]
	public class DragonOrbStatue : Item
	{
		public override void AddNameProperty( ObjectPropertyList list )
		{
			if ( BuildingPropertyListLocale != null && Name != null && Name.StartsWith( "Statue of ", StringComparison.Ordinal ) )
			{
				string rest = Name.Substring( "Statue of ".Length );
				string restLoc = StringCatalog.TryResolve( BuildingPropertyListLocale, rest ) ?? rest;

				list.Add( string.Format( ResolvePropertyText( "item.special.statue.of.fmt" ), restLoc ) );
				return;
			}

			base.AddNameProperty( list );
		}

		[Constructable]
		public DragonOrbStatue() : base( 0x5467 )
		{
			Name = "Statue of " + NameList.RandomName( "dragon" );
			Light = LightType.Circle225;
			Weight = 100.0;
			Hue = Utility.RandomColor(0);
		}

		public DragonOrbStatue( Serial serial ) : base( serial )
		{
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
	}
}
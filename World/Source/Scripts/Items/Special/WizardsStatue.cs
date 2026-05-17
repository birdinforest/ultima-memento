using System;
using Server; 
using Server.Misc;
using Server.Localization;

namespace Server.Items
{
	[Flipable( 0x5465, 0x5466 )]
	public class WizardsStatue : Item
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

		public override CraftResource DefaultResource{ get{ return CraftResource.Iron; } }
		public override Catalogs DefaultCatalog{ get{ return Catalogs.Stone; } }

		[Constructable]
		public WizardsStatue() : base( 0x5465 )
		{
			Name = "Statue of " + NameList.RandomName( "evil mage" );
			Light = LightType.Circle225;
			Weight = 100.0;
			ResourceMods.SetRandomResource( false, false, this, CraftResource.Iron, false, null );
			Hue = CraftResources.GetHue(Resource);
		}

		public WizardsStatue( Serial serial ) : base( serial )
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
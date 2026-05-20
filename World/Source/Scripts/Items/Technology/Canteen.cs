using System;
using System.Collections;
using Server.Network;
using Server.Targeting;
using Server.Prompts;
using Server.Localization;

namespace Server.Items
{
	public class Canteen : Item
	{
		public override string DefaultDescription{ get{ return StringCatalog.Resolve( null, "This strange, alien liquid can quench your thirst if you choose to drink it. Each canteen contains a single drink for someone." ); } }

		[Constructable]
		public Canteen() : base( 0x48E4 )
		{
			Stackable = true;
			Technology = true;
			Name = "canteen";
			InfoText1 = "alien liquid";
			InfoText2 = "use to drink";
			Weight = 0.1;
		}

		public override void AddNameProperties( ObjectPropertyList list )
		{
			string saved1 = m_InfoText1;
			string saved2 = m_InfoText2;

			if ( BuildingPropertyListLocale != null )
			{
				m_InfoText1 = null;
				m_InfoText2 = null;
			}

			base.AddNameProperties( list );

			if ( BuildingPropertyListLocale != null )
			{
				AddLocalizedProperty( list, "prop.infotext.canteen" );
				AddLocalizedProperty( list, "prop.infotext.canteen.drink" );
			}

			m_InfoText1 = saved1;
			m_InfoText2 = saved2;
		}

		public override void OnDoubleClick( Mobile from )
		{
			Server.Items.DrinkingFunctions.OnDrink( this, from );
		}

		public Canteen( Serial serial ) : base( serial )
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
}
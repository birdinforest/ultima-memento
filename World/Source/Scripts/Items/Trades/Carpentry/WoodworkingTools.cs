using System;
using Server;
using Server.Engines.Craft;

namespace Server.Items
{
	public class WoodworkingTools : BaseTool, IRunicWhenExceptional
	{
		public override string DisplayNameLocalizationKey => "item.trade.name.woodworking.tools";

		public override CraftSystem CraftSystem{ get{ return DefShelves.CraftSystem; } }

		[Constructable]
		public WoodworkingTools() : base( 0x5173 )
		{
			Name = "woodworking tools";
			Weight = 1.0;
		}

		[Constructable]
		public WoodworkingTools( int uses ) : base( uses, 0x5173 )
		{
			Name = "woodworking tools";
			Weight = 2.0;
			InfoText1 = "Crates, Chests";
			InfoText2 = "Shelves, Dressers,";
			InfoText3 = "and Cabinets";
		}

		public override void AddNameProperties( ObjectPropertyList list )
		{
			string saved1 = m_InfoText1;
			string saved2 = m_InfoText2;
			string saved3 = m_InfoText3;

			if ( BuildingPropertyListLocale != null )
			{
				m_InfoText1 = null;
				m_InfoText2 = null;
				m_InfoText3 = null;
			}

			base.AddNameProperties( list );

			if ( BuildingPropertyListLocale != null )
			{
				AddLocalizedProperty( list, "prop.infotext.woodworking.1" );
				AddLocalizedProperty( list, "prop.infotext.woodworking.2" );
				AddLocalizedProperty( list, "prop.infotext.woodworking.3" );
			}

			m_InfoText1 = saved1;
			m_InfoText2 = saved2;
			m_InfoText3 = saved3;
		}

		public WoodworkingTools( Serial serial ) : base( serial )
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
			ItemID = 0x5173;
		}
	}
}
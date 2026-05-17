using System;
using Server;
using Server.Mobiles;

namespace Server.Items
{
	public enum DrainCauldron
	{
		Charges
	}

    public class FireGiantForge : Item
	{
		public override bool IsContentLocalized => true;

		private int m_Charges;

		[CommandProperty( AccessLevel.GameMaster )]
		public int Charges
		{
			get{ return m_Charges; }
			set{ m_Charges = value; InvalidateProperties(); }
		}

        [Constructable]
        public FireGiantForge() : base( 0x1AF0 )
		{
            Name = "smoldering cauldron";
			Charges = 50;
			Weight = 20.0;
			Light = LightType.Circle225;
		}

		public override void AddNameProperty( ObjectPropertyList list )
		{
			if ( BuildingPropertyListLocale != null )
			{
				if ( Name == "cold cauldron" )
					AddLocalizedProperty( list, "item.trade.firegiantforge.cold" );
				else
					AddLocalizedProperty( list, "item.trade.firegiantforge.smoldering" );
				return;
			}
			base.AddNameProperty( list );
		}

        public override void AddNameProperties(ObjectPropertyList list)
		{
            base.AddNameProperties(list);
			if ( BuildingPropertyListLocale != null )
			{
				AddLocalizedProperty( list, "prop.trade.firegiantforge" );
				if ( m_Charges < 1 )
					AddLocalizedProperty( list, "prop.trade.firegiantforge.useless" );
				else if ( m_Charges == 1 )
					AddLocalizedProperty( list, "prop.trade.firegiantforge.use.remaining", m_Charges.ToString() );
				else
					AddLocalizedProperty( list, "prop.trade.firegiantforge.uses.remaining", m_Charges.ToString() );
			}
			else
			{
				string uses = m_Charges.ToString() + " Uses Remaining";
				if ( m_Charges == 1 ){ uses = m_Charges.ToString() + " Use Remaining"; }
				if ( m_Charges < 1 ){ uses = "Useless"; }
				list.Add( 1070722, "Fire Giant Forge");
				list.Add( 1049644, uses );
			}
        }

		public static void ConsumeCharge( FireGiantForge kettle )
		{
			--kettle.Charges;

			if ( kettle.Charges < 1 )
			{
				kettle.Light = LightType.Empty;
				kettle.ItemID = 0x09ED;
				kettle.Name = "cold cauldron";
			}
		}

        public FireGiantForge( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
			writer.Write( (int) m_Charges );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
			switch ( version )
			{
				case 0:
				{
					m_Charges = (int)reader.ReadInt();
					break;
				}
			}
	    }
    }
}
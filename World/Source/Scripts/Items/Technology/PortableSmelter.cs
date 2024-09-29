using System;
using Server;
using Server.Network;
using System.Text;
using Server.Items;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Items
{
	public class PortableSmelter : Item
	{
		private int m_Charges;

		[CommandProperty( AccessLevel.GameMaster )]
		public int Charges
		{
			get{ return m_Charges; }
			set{ m_Charges = value; InvalidateProperties(); }
		}

		[Constructable]
		public PortableSmelter() : base( 0x540A )
		{
			Name = "portable smelter";
			Technology = true;
			Weight = 5;
			ItemID = Utility.RandomMinMax( 0x540A, 0x540B );
			Charges = Utility.RandomMinMax( 50, 100 );
		}

        public override void AddNameProperties(ObjectPropertyList list)
		{
            base.AddNameProperties(list);
			if ( m_Charges > 1 ){ list.Add( 1070722, m_Charges.ToString() + " Uses Left"); }
			else { list.Add( 1070722, "1 Use Left"); }
            list.Add( 1049644, "Smelt ore into ingots");
        }

		public override void OnDoubleClick( Mobile from )
		{
			if ( !Movable )
				return;

			if ( !IsChildOf( from.Backpack ) ) 
			{
				from.SendMessage( "This must be in your backpack to use." );
				return;
			}
			else
			{
				from.SendMessage( "Select the ore you want to smelt into ingots." );
				from.Target = new InternalTarget( this );
			}
		}

		private class InternalTarget : Target
		{
			private PortableSmelter m_Tool;

			public InternalTarget( PortableSmelter tool ) :  base ( 2, false, TargetFlags.None )
			{
				m_Tool = tool;
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				if ( targeted is BaseOre )
				{
					var ore = (BaseOre)targeted;
					if ( !from.InRange( ore.GetWorldLocation(), 2 ) )
					{
						from.SendLocalizedMessage( 501976 ); // The ore is too far away.
						return;
					}
					
					ore.SmeltAll(from, this);
					m_Tool.ConsumeCharge( from );
				}
				else
				{
					from.SendMessage( "You can only use this on ore." );
				}
			}
		}

		public void ConsumeCharge( Mobile from )
		{
			--Charges;

			if ( Charges == 0 )
			{
				from.SendMessage( "The smelter was used too much and broke." );
				Item MyJunk = new SciFiJunk();
			  	MyJunk.Hue = this.Hue;
			  	MyJunk.ItemID = this.ItemID;
				MyJunk.Name = Server.Items.SciFiJunk.RandomCondition() + " portable smelter";
				MyJunk.Weight = this.Weight;
			  	from.AddToBackpack ( MyJunk );
				this.Delete();
			}
		}

		public PortableSmelter( Serial serial ) : base( serial )
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
			m_Charges = (int)reader.ReadInt();
		}
	}
}
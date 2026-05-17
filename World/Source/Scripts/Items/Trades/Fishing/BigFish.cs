using System;
using Server.Network;
using Server.Targeting;
using Server.Localization;

namespace Server.Items
{
	public class BigFish : Item, ICarvable
	{
		public override bool IsContentLocalized => true;

		private Mobile m_Fisher;

		[CommandProperty( AccessLevel.GameMaster )]
		public Mobile Fisher
		{
			get{ return m_Fisher; }
			set{ m_Fisher = value; InvalidateProperties(); }
		}

		public void Carve( Mobile from, Item item )
		{
			base.ScissorHelper( from, new RawFishSteak(), Math.Max( 16, (int)Weight ) / 4 , false );
		}

		public override int LabelNumber{ get{ return 1041112; } } // a big fish

		[Constructable]
		public BigFish() : base( 0x09CC )
		{
			Weight = Utility.RandomMinMax( 3, 200 );	//TODO: Find correct formula.  max on OSI currently 200, OSI dev says it's not 200 as max, and ~ 1/1,000,000 chance to get highest
			Hue = Utility.RandomBool() ? 0x847 : 0x58C;
		}

		public override void AddNameProperty( ObjectPropertyList list )
		{
			if ( BuildingPropertyListLocale != null )
			{
				if ( Amount <= 1 )
					AddLocalizedProperty( list, "item.trade.bigfish" );
				else
					list.Add( 1050039, "{0}\t{1}", Amount, ResolvePropertyText( "item.trade.bigfish" ) );
				return;
			}
			base.AddNameProperty( list );
		}

		public override void GetProperties( ObjectPropertyList list )
		{
			base.GetProperties( list );

			if ( Weight >= 20 )
			{
				if ( BuildingPropertyListLocale != null )
				{
					if ( m_Fisher != null )
						AddLocalizedProperty( list, "prop.trade.bigfish.caught.by", m_Fisher.Name );

					AddLocalizedProperty( list, "prop.trade.bigfish.weight.stones", ((int)Weight).ToString() );
				}
				else
				{
					if ( m_Fisher != null )
						list.Add( 1070857, m_Fisher.Name ); // Caught by ~1_fisherman~

					list.Add( 1070858, ((int)Weight).ToString() ); // ~1_weight~ stones
				}
			}
		}

		public BigFish( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 1 ); // version

			writer.Write( (Mobile) m_Fisher );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch ( version )
			{
				case 1:
				{
					m_Fisher = reader.ReadMobile();
					break;
				}
				case 0:
				{
					Weight = Utility.RandomMinMax( 3, 200 );
					break;
				}
			}
		}
	}
}
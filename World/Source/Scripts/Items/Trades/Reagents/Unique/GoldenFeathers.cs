using System.Linq;
using Server.Mobiles;
using Server.Network;

namespace Server.Items
{
	public class GoldenFeathers : Item
	{
		public override Catalogs DefaultCatalog{ get{ return Catalogs.Reagent; } }

		public override string DefaultDescription{ get{ return "These items are very rare, and are sometimes sought after with a given quest. They are sometimes required for rituals or potion ingredients as well."; } }

		public Mobile m_Owner;

		[CommandProperty( AccessLevel.GameMaster )]
		public Mobile Owner
		{
			get{ return m_Owner; }
			set{ m_Owner = value; InvalidateProperties(); }
		}

		[Constructable]
		public GoldenFeathers( ) : this( null )
		{
		}

		public GoldenFeathers( Mobile from ) : base( 0x4CCD )
		{
			m_Owner = from;	
			Name = "golden feathers";
			Weight = 1.0;
			Hue = 0xAD4;
			Stackable = false;
		}

        public override void AddNameProperties(ObjectPropertyList list)
		{
            base.AddNameProperties(list);
			if ( m_Owner != null ){ list.Add( 1070722, "Gifted to " + m_Owner.Name + ""); }
        }

		public static void Award(BaseCreature creature, Mobile killer)
		{
			if ( creature == null || killer == null ) return;

			if ( creature is Harpy || creature is StoneHarpy || creature is SnowHarpy || creature is Phoenix || creature is HarpyElder || creature is HarpyHen )
			{
				if ( killer is BaseCreature )
					killer = ((BaseCreature)killer).GetMaster();

				if ( killer is PlayerMobile )
				{
					if ( ( killer.Skills[SkillName.Camping].Base >= 90 
							|| killer.Skills[SkillName.Tracking].Base >= 90 )
						&& killer.Backpack.FindItemByType( typeof( GoldenRangers ) ) != null )
					{
						int chance =  creature is Phoenix ? 25 : 5;
						if ( chance >= Utility.RandomMinMax( 1, 100 ) )
						{
							World.Items.Values
								.Where(item => item is GoldenFeathers)
								.Cast<GoldenFeathers>()
								.Where(item => item.Owner == killer)
								.ToList()
								.ForEach(item => item.Delete());
							killer.AddToBackpack( new GoldenFeathers( killer ) );
							killer.SendSound( 0x3D );
							killer.PrivateOverheadMessage(MessageType.Regular, 1150, false, "The goddess has given you golden feathers.", killer.NetState);
						}
					}
				}
			}
		}

		public GoldenFeathers( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
			writer.Write( (Mobile)m_Owner);
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
			m_Owner = reader.ReadMobile();
			ItemID = 0x4CCD;
			Hue = 0xAD4;
		}
	}
}
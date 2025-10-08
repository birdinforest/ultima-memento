using Server.Misc;
using Server.Mobiles;
using Server.Utilities;

namespace Server.Items
{
	public class SummonItems : Item
	{
		public Mobile owner;

		[CommandProperty( AccessLevel.GameMaster )]
		public Mobile Owner { get{ return owner; } set{ owner = value; } }

		[Constructable]
		public SummonItems() : base( 0xF91 )
		{
			Name = "item";
			Light = LightType.Circle150;
			Weight = 1.0;
		}

		public SummonItems( Serial serial ) : base( serial )
		{
		}

        public override void AddNameProperties(ObjectPropertyList list)
		{
            base.AddNameProperties(list);
			if ( owner != null ){ list.Add( 1049644, "Belongs to " + owner.Name + "" ); }
        }

		public override bool OnDragLift( Mobile from )
		{
			if ( from is PlayerMobile && owner == null )
			{
				WorldUtilities.DeleteAllItems<SummonItems>(item => item.owner == from && item != this && item.Name == Name);
				LoggingFunctions.LogGenericQuest( from, "has obtained the " + this.Name );
				this.owner = from;
			}

			return true;
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
			writer.Write( (Mobile)owner);
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
			owner = reader.ReadMobile();
		}
	}
	public class SummonReward : Item
	{
		[Constructable]
		public SummonReward() : base( 0xE2E )
		{
			Weight = 10.0;
			Name = "crystal ball";
			Light = LightType.Circle300;
		}

        public override void AddNameProperties(ObjectPropertyList list)
		{
            base.AddNameProperties(list);
			list.Add( 1070722, "Decoration Relic");
        }

		public SummonReward( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int)1 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
}
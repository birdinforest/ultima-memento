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

		// Set only on SummonItems dropped by the Epic Tribute personal challenge encounter
		// (EpicTributeChallenge); the shared SummonCarriers.cs 59-mob pool never sets this,
		// so Magical Prison's (unrelated, name-based) key checks are unaffected.
		public bool EpicChallengeSource;

		[CommandProperty( AccessLevel.GameMaster )]
		public bool p_EpicChallengeSource { get{ return EpicChallengeSource; } set{ EpicChallengeSource = value; } }

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
				WorldUtilities.DeleteAllItems<SummonItems>( item => item.owner == from && item != this && item.Name == Name && item.EpicChallengeSource == EpicChallengeSource );
				LoggingFunctions.LogGenericQuest( from, "has obtained the " + this.Name );
				this.owner = from;
			}

			return true;
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 1 ); // version
			writer.Write( (Mobile)owner);
			writer.Write( EpicChallengeSource );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
			owner = reader.ReadMobile();

			if ( version >= 1 )
				EpicChallengeSource = reader.ReadBool();
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
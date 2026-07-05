using Server.Engines.Avatar;
using Server.Gumps;
using Server.Mobiles;

namespace Server.Mobiles
{
	[CorpseName( "the remains of a registrar" )]
	public class LostArtsRegistrar : BasePerson
	{
		[Constructable]
		public LostArtsRegistrar() : base()
		{
			Title = "the lost arts registrar";
			Name = "Lost Arts Registrar";
			Body = 0x190;
			Hue = Utility.RandomSkinHue();
			AI = AIType.AI_Citizen;
			FightMode = FightMode.None;
			CantWalk = true;

			SetStr( 100 );
			SetDex( 100 );
			SetInt( 100 );
		}

		public LostArtsRegistrar( Serial serial ) : base( serial )
		{
		}

		public override bool IsInvulnerable { get { return true; } }

		public override void OnDoubleClick( Mobile from )
		{
			if ( from is PlayerMobile )
			{
				var player = (PlayerMobile)from;
				if ( player.Avatar.Active )
				{
					from.SendGump( new LostArtsRegistrarGump( player, this, 0 ) );
					return;
				}
			}

			base.OnDoubleClick( from );
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( 0 );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
}

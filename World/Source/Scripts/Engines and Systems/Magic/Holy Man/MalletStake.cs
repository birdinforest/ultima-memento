using Server.ContextMenus;
using System.Collections.Generic;
using Server.Misc;
using Server.Gumps;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Items
{
	public class MalletStake : Item
	{
		public int VampiresSlain;

		[CommandProperty(AccessLevel.Owner)]
		public int Vampires_Slain { get { return VampiresSlain; } set { VampiresSlain = value; InvalidateProperties(); } }

		[Constructable]
		public MalletStake() : base( 0x64DD )
		{
			Weight = 1.0;
			Name = "wooden mallet and stake";
			ItemID = Utility.RandomList( 0x64DD, 0x64DE );
		}

        public override void AddNameProperties(ObjectPropertyList list)
		{
            base.AddNameProperties(list);
			list.Add( 1070722, "Single Click For Information");
            list.Add( 1049644, "Double Click To Use");
        }

		public override void GetContextMenuEntries( Mobile from, List<ContextMenuEntry> list ) 
		{ 
			base.GetContextMenuEntries( from, list ); 
			list.Add( new StakeGump( from, this ) );
		}

		public override void GetProperties( ObjectPropertyList list )
		{
			base.GetProperties( list );
			list.Add( 1060738, VampiresSlain.ToString() );
		}

		public class StakeGump : ContextMenuEntry
		{
			private Mobile m_Mobile;
			private MalletStake m_Stake;
			
			public StakeGump( Mobile from, MalletStake stake ) : base( 6096, 3 ) // Examine
			{
				m_Mobile = from;
				m_Stake = stake;
			}

			public override void OnClick()
			{
			    if( false == ( m_Mobile is PlayerMobile ) ) return;

				m_Mobile.SendMessage("Current Value: " + m_Stake.VampiresSlain + " Gold!");
				m_Mobile.SendGump(new SpeechGump( m_Mobile, "The Vampire Scourge", SpeechFunctions.SpeechText( m_Mobile, m_Mobile, "Stake" ) ));
            }
        }

		public override void OnDoubleClick( Mobile from )
		{
			if ( VampiresSlain >= 10000 )
			{
				from.SendMessage("This has killed enough vampires.");
				return;
			}
			
			from.SendMessage("What vampire do you want to stake?");
			from.Target = new CorpseTarget( this );
		}

		private class CorpseTarget : Target
		{
			private MalletStake m_Stake;

			public CorpseTarget( MalletStake stake ) : base( 3, false, TargetFlags.None )
			{
				m_Stake = stake;
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				if ( m_Stake.Deleted )
					return;

				object obj = targeted;

				if ( obj is Corpse )
				{
					Corpse c = (Corpse)targeted;

					if ( c.VisitedByTaxidermist == true )
					{
						from.SendMessage("You don't need to do that!");
						return;
					}
					else
					{
						int score = 0;

						if ( typeof( VampireWoods ) == c.Owner.GetType() ){ score = 10; }
						else if ( typeof( Vampire ) == c.Owner.GetType() ){ score = 20; }
						else if ( typeof( VampireLord ) == c.Owner.GetType() ){ score = 40; }
						else if ( typeof( VampirePrince ) == c.Owner.GetType() ){ score = 60; }
						else if ( typeof( Dracula ) == c.Owner.GetType() ){ score = 400; }
						else if ( typeof( VampiricDragon ) == c.Owner.GetType() ){ score = 500; }

						if ( score > 0 )
						{
							const int MAX_VAMPIRES_SLAIN = 10000;
							score = m_Stake.VampiresSlain + score > MAX_VAMPIRES_SLAIN ? MAX_VAMPIRES_SLAIN - m_Stake.VampiresSlain : score;
							m_Stake.VampiresSlain += score;
							from.SendMessage("Vampire Reward: " + score + " Gold!");
							c.VisitedByTaxidermist = true;
							from.PlaySound( 0x13E );
							m_Stake.InvalidateProperties();
						}
						else 
						{
							from.SendMessage("You don't need to do that!");
							return;
						}
					}
				}
				else
				{
					from.SendMessage("You don't need to do that!");
					return;
				}
			}
		}

		public MalletStake( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
            writer.Write( VampiresSlain );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
			VampiresSlain = reader.ReadInt();
		}
	}
}
using Server.Misc;
using Server.Mobiles;
using Server.Localization;

namespace Server.Items
{
	public class JokeBook : Item
	{
		public override string DefaultDescription { get { return StringCatalog.ResolveByKey(null, "quest.these_books_are_said_to_be_cursed_c_and_anyone_who_reads_them_will_be_cursed_to_tell_jokes_for_the_r"); } }

		[Constructable]
		public JokeBook() : base( 0x1A98 )
		{
			Weight = 1.0;
			Name = RandomThings.MagicWandOwner() + " Book of Jokes";
			Hue = 0xAFF;
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( from is PlayerMobile )
			{
				switch ( Utility.RandomMinMax( 0, 8 ) ) 
				{
					case 0: from.PlaySound( from.Female ? 801 : 1073 );		from.Say( StringCatalog.Resolve( from.Account, "*laughs*" ) );	break;
					case 1: from.PlaySound( from.Female ? 801 : 1073 );		from.Say( StringCatalog.Resolve( from.Account, "Good one!" ) );	break;
					case 2: from.PlaySound( from.Female ? 801 : 1073 );		from.Say( StringCatalog.Resolve( from.Account, "I never heard that one before!" ) );	break;
					case 3: from.PlaySound( from.Female ? 801 : 1073 );		from.Say( StringCatalog.Resolve( from.Account, "I always like a good laugh!" ) );	break;
					case 4: from.PlaySound( from.Female ? 801 : 1073 );		from.Say( StringCatalog.Resolve( from.Account, "That has me in tears!" ) );	break;
					case 5: from.Say( StringCatalog.Resolve( from.Account, "I don't get it." ) );							break;
					case 6: from.Say( StringCatalog.Resolve( from.Account, "What does that even mean?" ) );				break;
					case 7: from.Say( StringCatalog.Resolve( from.Account, "Is that supposed to be funny?" ) );			break;
					case 8: from.Say( StringCatalog.Resolve( from.Account, "An orc and an elf walk into a tavern?" ) );	break;
				}
			}
		}

		public JokeBook( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( ( int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
}
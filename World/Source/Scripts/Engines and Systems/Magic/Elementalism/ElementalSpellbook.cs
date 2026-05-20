using System;
using Server.Network;
using Server.Spells;
using Server.Spells.Elementalism;
using Server.Gumps;
using Server.Mobiles;
using Server.Localization;

namespace Server.Items
{
	public class ElementalSpellbook : Spellbook
	{
		public override string DefaultDescription{ get{ return StringCatalog.ResolveByKey(null, "eng.this_book_is_used_by_elementalist_c_where_they_can_record_the_elemental_magic_they_can_unleash_dot_d"); } }

		public Mobile EllyOwner;
		[CommandProperty( AccessLevel.GameMaster )]
		public Mobile Elly_Owner { get{ return EllyOwner; } set{ EllyOwner = value; } }

		public int EllyPage;
		[CommandProperty( AccessLevel.GameMaster )]
		public int Elly_Page { get{ return EllyPage; } set{ EllyPage = value; } }

		public override SpellbookType SpellbookType{ get{ return SpellbookType.Elementalism; } }
		public override int BookOffset{ get{ return 300; } }
		public override int BookCount{ get{ return 32; } }

		[Constructable]
		public ElementalSpellbook() : this( (ulong)0 )
		{
		}

		[Constructable]
		public ElementalSpellbook( ulong content ) : base( content, 0x6713 )
		{
			Layer = Layer.Trinket;
			Name = "elemental spellbook";
			ItemID = Utility.RandomList( 0x6713, 0x6715, 0x6717, 0x6719 );
		}

		public override bool OnDragLift( Mobile from )
		{
			if ( from is PlayerMobile )
				SetupBook( from );

			return base.OnDragLift( from );
		}

		public override void OnDoubleClick( Mobile from )
		{
			SetupBook( from );
			Container pack = from.Backpack;

			if ( !ElementalSpell.CanUseBook( this, from, true ) )
			{
				from.CloseGump( typeof( ElementalSpellbookGump ) );
				from.CloseGump( typeof( ElementalSpellHelp ) );
			}
			else if ( Parent == from || ( pack != null && Parent == pack ) )
			{
				from.SendSound( 0x55 );
				from.CloseGump( typeof( ElementalSpellbookGump ) );
				from.CloseGump( typeof( ElementalSpellHelp ) );
				from.SendGump( new ElementalSpellbookGump( from, this, 1 ) );
			}
			else from.SendLocalizedMessage(500207); // The spellbook must be in your backpack (and not in a container within) to open.
		}

		public void SetupBook( Mobile from )
		{
			if ( EllyOwner != from ){ EllyOwner = from; }
			ElementalSpell.BookCover( this, ((PlayerMobile)from).CharacterElement );
		}

		public ElementalSpellbook( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int)1 ); // version
			writer.Write( (Mobile)EllyOwner );
			writer.Write( (int)EllyPage );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
			EllyOwner = reader.ReadMobile();
			int EllyPage = reader.ReadInt();

			if ( ItemID == 0x641F )
				ItemID = 0x6713;
			else if ( ItemID == 0x6420 )
				ItemID = 0x6715;
			else if ( ItemID == 0x6421 )
				ItemID = 0x6717;
			else if ( ItemID == 0x6422 )
				ItemID = 0x6719;	
		}
	}
}
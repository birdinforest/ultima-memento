using System;
using Server.Network;
using Server.Spells;
using Server.Localization;

namespace Server.Items
{
	public class BookOfNinjitsu : Spellbook
	{
		public override string DefaultDescription{ get{ return StringCatalog.ResolveByKey(null, "eng.this_book_is_used_by_ninja_c_in_order_for_them_to_use_various_abilities_akin_to_this_ancient_order_o"); } }

		public override SpellbookType SpellbookType{ get{ return SpellbookType.Ninja; } }
		public override int BookOffset{ get{ return 500; } }
		public override int BookCount{ get{ return 8; } }

		[Constructable]
		public BookOfNinjitsu() : this( (ulong)0xFF )
		{
		}

		[Constructable]
		public BookOfNinjitsu( ulong content ) : base( content, 0x23A0 )
		{
			Name = "ninjitsu book";
			Layer = Layer.Trinket;
		}

		public BookOfNinjitsu( Serial serial ) : base( serial )
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
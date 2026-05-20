using System;
using Server.Network;
using Server.Spells;
using Server.Localization;

namespace Server.Items
{
	public class BookOfChivalry : Spellbook
	{
		public override string DefaultDescription{ get{ return StringCatalog.ResolveByKey(null, "eng.this_book_is_used_by_knights_c_in_order_for_them_to_use_various_abilities_to_spread_harmony_and_peac"); } }

		public override SpellbookType SpellbookType{ get{ return SpellbookType.Paladin; } }
		public override int BookOffset{ get{ return 200; } }
		public override int BookCount{ get{ return 10; } }

		[Constructable]
		public BookOfChivalry() : this( (ulong)0x3FF )
		{
		}

		[Constructable]
		public BookOfChivalry( ulong content ) : base( content, 0x2252 )
		{
			Name = "knightship book";
			Layer = Layer.Trinket;
		}

		public BookOfChivalry( Serial serial ) : base( serial )
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
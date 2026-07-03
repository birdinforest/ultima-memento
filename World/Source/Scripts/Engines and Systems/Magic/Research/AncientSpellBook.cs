using System;
using Server.Network;
using Server.Gumps;
using Server.Spells;
using Server.Localization;
using Server.Misc;

namespace Server.Items
{
	[FlipableAttribute( 0x65EC, 0x6711 )]
	public class AncientSpellbook : Spellbook
	{
		public override bool IsContentLocalized => true;

		public override string DefaultDescription{ get{ return StringCatalog.ResolveByKey(null, "eng.this_book_is_used_by_archmages_c_where_they_can_cast_ancient_spells_thought_to_be_lost_forever_dot_t"); } }

		public Mobile owner;
		public string names;
		public int paper;
		public int quill;

		[CommandProperty( AccessLevel.GameMaster )]
		public Mobile Owner { get{ return owner; } set{ owner = value; } }

		[CommandProperty( AccessLevel.GameMaster )]
		public int Paper { get{ return paper; } set{ paper = value; } }

		[CommandProperty( AccessLevel.GameMaster )]
		public int Quill { get{ return quill; } set{ quill = value; } }

		[CommandProperty( AccessLevel.GameMaster )]
		public string Names { get{ return names; } set{ names = value; } }

		public override SpellbookType SpellbookType{ get{ return SpellbookType.Archmage; } }
		public override int BookOffset{ get{ return 600; } }
		public override int BookCount{ get{ return 64; } }

		[Constructable]
		public AncientSpellbook() : this( (ulong)0 )
		{
		}

		[Constructable]
		public AncientSpellbook( ulong content ) : base( content, 0x65EC )
		{
			Layer = Layer.Trinket;
			Name = "ancient spellbook";
			Weight = 3.0;
		}

		public override void OnDoubleClick( Mobile from )
		{
			Container pack = from.Backpack;

			if ( owner != from )
			{
				ResearchLocalization.Send( from, "research.msg.ancient_pages_scribbles", "These pages appears as scribbles to you." );
			}
			else if ( Parent == from || ( pack != null && Parent == pack ) )
			{
				from.SendSound( 0x55 );
				from.CloseGump( typeof( AncientSpellbookGump ) );
				from.SendGump( new AncientSpellbookGump( from, this, 1 ) );
			}
			else from.SendLocalizedMessage(500207); // The spellbook must be in your backpack (and not in a container within) to open.
		}

        public override void AddNameProperties(ObjectPropertyList list)
		{
			if ( BuildingPropertyListLocale != null )
			{
				AddLocalizedProperty( list, "item.research.ancient_spellbook" );

				if ( owner != null )
					AddLocalizedProperty( list, "research.prop.belongs_to", owner.Name );
			}
			else
			{
				base.AddNameProperties( list );

				if ( owner != null )
					list.Add( 1070722, StringCatalog.ResolveFormat( null, "Belongs to {0}", owner.Name ) );
			}
        }

		public AncientSpellbook( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
			writer.Write( (Mobile)owner);
			writer.Write( paper );
			writer.Write( quill );
			writer.Write( names );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
			owner = reader.ReadMobile();
			paper = reader.ReadInt();
			quill = reader.ReadInt();
			names = reader.ReadString();
		}
	}
}

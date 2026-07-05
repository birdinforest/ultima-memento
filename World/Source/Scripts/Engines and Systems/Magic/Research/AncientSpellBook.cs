using System;
using Server.Network;
using Server.Gumps;
using Server.Spells;
using Server.Localization;
using Server.Misc;
using Server.Engines.Avatar;
using Server.Mobiles;

namespace Server.Items
{
	[FlipableAttribute( 0x65EC, 0x6711 )]
	public class AncientSpellbook : Spellbook, IAvatarCoreItem
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
			else if ( IsDormant )
			{
				ResearchLocalization.Send( from, "research.resonance.msg.book_departed_soul", "Names belong to a departed form… knowledge waits in the dormant pack. After the Rite, pages clear." );
			}
			else if ( Parent == from || ( pack != null && Parent == pack ) )
			{
				from.SendSound( 0x55 );
				from.CloseGump( typeof( AncientSpellbookGump ) );
				from.SendGump( new AncientSpellbookGump( from, this, 1 ) );
			}
			else from.SendLocalizedMessage(500207);
		}

        public override void AddNameProperties(ObjectPropertyList list)
		{
			if ( BuildingPropertyListLocale != null )
			{
				AddLocalizedProperty( list, "item.research.ancient_spellbook" );

				if ( owner != null && IsDormant )
					AddLocalizedProperty( list, "research.resonance.prop.book_awaiting", owner.Name );
				else if ( owner != null )
					AddLocalizedProperty( list, "research.prop.belongs_to", owner.Name );
			}
			else
			{
				base.AddNameProperties( list );

				if ( owner != null )
					list.Add( 1070722, StringCatalog.ResolveFormat( null, "Belongs to {0}", owner.Name ) );
			}
        }


		private int m_StoredHue;
		private bool m_IsDormant;

		[CommandProperty( AccessLevel.GameMaster )]
		public bool IsDormant
		{
			get { return m_IsDormant; }
			set
			{
				if ( m_IsDormant == value )
					return;

				m_IsDormant = value;

				if ( m_IsDormant )
				{
					if ( m_StoredHue == 0 && Hue != 1109 )
						m_StoredHue = Hue;
					Hue = 1109;
				}
				else if ( m_StoredHue > 0 )
				{
					Hue = m_StoredHue;
					m_StoredHue = 0;
				}

				InvalidateProperties();
			}
		}

		public void SnapshotToContext( PlayerContext ctx )
		{
			if ( ctx == null )
				return;

			ctx.SnapshotAncientSpellbookOwnerSerial = owner != null ? owner.Serial : Serial.Zero;
			ctx.SnapshotAncientSpellbookNames = names;
			ctx.SnapshotAncientSpellbookPaper = paper;
			ctx.SnapshotAncientSpellbookQuill = quill;
			ctx.SnapshotAncientSpellbookContent = Content;
			ctx.SnapshotAncientSpellbookSlayer = (int)Slayer;
			ctx.SnapshotAncientSpellbookSlayer2 = (int)Slayer2;
		}

		public void RestoreFromContext( PlayerContext ctx )
		{
			if ( ctx == null )
				return;

			if ( !string.IsNullOrEmpty( ctx.SnapshotAncientSpellbookNames ) )
				names = ctx.SnapshotAncientSpellbookNames;

			paper = ctx.SnapshotAncientSpellbookPaper;
			quill = ctx.SnapshotAncientSpellbookQuill;

			Slayer = (SlayerName)ctx.SnapshotAncientSpellbookSlayer;
			Slayer2 = (SlayerName)ctx.SnapshotAncientSpellbookSlayer2;
		}

		public void ApplyResourceDecay()
		{
			paper = (int)( paper * 0.5 );
			quill = (int)( quill * 0.5 );
		}

		public void RebindOwner( PlayerMobile newOwner )
		{
			owner = newOwner;
			names = newOwner != null ? newOwner.Name : names;
		}

		public void ActivateResonance( PlayerMobile player )
		{
			IsDormant = false;
			if ( player != null )
			{
				names = player.Name;
				Name = "ancient spells of " + player.Name;

				ResearchBag bag = player.Avatar.GetResearchBag();

				if ( bag == null )
					bag = AvatarCoreItemMigration.FindResearchBag( player );

				if ( bag != null )
					Research.SyncAncientSpellbookFromBag( bag, this );
			}

			ResearchLocalization.Send( player, "research.resonance.msg.book_awakened", "The name upon the book slowly becomes your own." );
		}

		public AncientSpellbook( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 1 ); // version
			writer.Write( (Mobile)owner);
			writer.Write( paper );
			writer.Write( quill );
			writer.Write( names );
			writer.Write( m_IsDormant );
			writer.Write( m_StoredHue );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
			owner = reader.ReadMobile();
			paper = reader.ReadInt();
			quill = reader.ReadInt();
			names = reader.ReadString();

			if ( version >= 1 )
			{
				m_IsDormant = reader.ReadBool();
				m_StoredHue = reader.ReadInt();
				if ( m_IsDormant && Hue != 1109 )
				{
					if ( m_StoredHue == 0 )
						m_StoredHue = Hue;
					Hue = 1109;
				}
			}
		}
	}
}

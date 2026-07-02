using System;
using Server;
using Server.Localization;
using Server.Network;

namespace Server.Items
{
	public class HiveTool : Item
	{
		public override string DisplayNameLocalizationKey => "item.apiculture.hive_tool";

		private int m_UsesRemaining;

		[CommandProperty( AccessLevel.GameMaster )]
		public int UsesRemaining
		{
			get { return m_UsesRemaining; }
			set { m_UsesRemaining = value; InvalidateProperties(); }
		}

		[Constructable]
		public HiveTool() : this( 50 )
		{
		}
		
		[Constructable]
		public HiveTool( int uses ) : base( 2549 )
		{
			m_UsesRemaining = uses;
			Name = "hive tool";
		}

		public HiveTool( Serial serial ) : base( serial )
		{
		}

		public override void GetProperties( ObjectPropertyList list )
		{
			base.GetProperties( list );

			if ( BuildingPropertyListLocale != null )
				AddLocalizedProperty( list, "prop.apiculture.uses", m_UsesRemaining.ToString() );
			else
				list.Add( 1060584, "{0}\t{1}", m_UsesRemaining.ToString(), "Uses" );
		}

		public virtual void DisplayDurabilityTo( Mobile m )
		{
			LabelToAffix( m, 1017323, AffixType.Append, ": " + m_UsesRemaining.ToString() ); // Durability
		}

		public override void OnSingleClick( Mobile from )
		{
			DisplayDurabilityTo( from );

			base.OnSingleClick( from );
		}

		public override void OnDoubleClick(Mobile from)
		{
			from.PrivateOverheadMessage( 0, 1154, false, StringCatalog.ResolveByKey( from.Account, "apiculture.msg.hive_tool_help" ), from.NetState );				
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version

			writer.Write( (int) m_UsesRemaining );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch ( version )
			{
				case 0:
				{
					m_UsesRemaining = reader.ReadInt();
					break;
				}
			}
		}
	}
}

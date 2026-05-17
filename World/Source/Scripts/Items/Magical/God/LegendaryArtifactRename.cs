using System;
using Server.Network;
using Server;
using Server.Targeting;
using Server.Items;
using Server.Prompts;
using Server.Localization;

namespace Server.Items
{
	public class LegendaryArtifactRename : Item
	{
		private int m_Charges;
		[CommandProperty( AccessLevel.GameMaster )]
		public int Charges
		{
			get { return m_Charges; }
			set { m_Charges = value; InvalidateProperties(); }
		}

		public Mobile owner;

		[CommandProperty( AccessLevel.GameMaster )]
		public Mobile Owner { get{ return owner; } set{ owner = value; } }

		[Constructable]
		public LegendaryArtifactRename( Mobile from ) : base( 0xFB8 )
		{
			Name = "Legendary Branding Iron";
			m_Charges = 3;
			this.owner = from;
		}

		public override void GetProperties( ObjectPropertyList list )
		{
			base.GetProperties( list );
			if ( BuildingPropertyListLocale != null )
				AddLocalizedProperty(list, "god.rename.uses.left", Charges);
			else
				list.Add("{0} Uses Left", Charges);
		}

        public override void AddNameProperties(ObjectPropertyList list)
		{
            base.AddNameProperties( list );
			if ( BuildingPropertyListLocale != null )
			{
				AddLocalizedProperty(list, "god.rename.legendary");
				if ( owner != null )
					AddLocalizedProperty(list, "god.rename.belongs.to", owner.Name);
			}
			else
			{
				list.Add( 1070722, "Rename Legendary Artefacts");
				if ( owner != null ){ list.Add( 1049644, "Belongs to " + owner.Name + "" ); }
			}
        } 

		public override void OnDoubleClick( Mobile from )
		{
			if(!IsChildOf(from.Backpack)) from.SendMessage(StringCatalog.ResolveByKey(from.Account, "god.msg.rename.backpack"));
			else if ( this.owner != from  )
			{
				from.SendMessage(StringCatalog.ResolveByKey(from.Account, "god.msg.rename.not.yours"));
				return;
			}
			else if ( m_Charges > 0)
			{
				from.SendMessage(StringCatalog.ResolveByKey(from.Account, "god.msg.rename.choose"));
				from.Target = new InternalTarget( this );
			}
			else
			{
				from.SendMessage(StringCatalog.ResolveByKey(from.Account, "god.msg.rename.out.of.uses"));
				this.Delete();
			}
		}
		private class InternalTarget : Target
		{
			private LegendaryArtifactRename m_LegendaryArtifactRename;
			private Item m_engtarg;

			public InternalTarget( LegendaryArtifactRename engrave ) : base( 1, false, TargetFlags.None )
			{
				m_LegendaryArtifactRename = engrave;
			}
			protected override void OnTarget( Mobile from, object targeted )
			{
				if ( targeted is LegendaryArtifactRename )
				{
					LegendaryArtifactRename knife = targeted as LegendaryArtifactRename;
					if (knife != null)
					{
						int knifeuses = knife.Charges;
						m_LegendaryArtifactRename.Charges += knifeuses;
						knife.Delete();
					}
				}
				else if ( targeted is ILevelable )
				{
					m_engtarg = (Item)targeted;
					if(!m_engtarg.IsChildOf(from.Backpack)) from.SendMessage(StringCatalog.ResolveByKey(from.Account, "god.msg.rename.target.backpack"));
					else
					{
						from.SendMessage(StringCatalog.ResolveByKey(from.Account, "god.msg.rename.prompt.name"));
						m_LegendaryArtifactRename.Charges -= 1 ;
						m_LegendaryArtifactRename.InvalidateProperties();
						from.Prompt = new RenameContPrompt( m_engtarg );
					}
				}
				else from.SendMessage(StringCatalog.ResolveByKey(from.Account, "god.msg.rename.cannot"));
			}
		}

		public LegendaryArtifactRename(Serial serial) : base(serial){}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 );
			writer.Write( (int) m_Charges );
			writer.Write( (Mobile)owner );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
			m_Charges = reader.ReadInt();
			owner = reader.ReadMobile();
		}
	}
}

namespace Server.Prompts
{
	public class RenameContPrompt : Prompt
	{
		private Item m_engtarg;

		public RenameContPrompt( Item rcont )
		{
			m_engtarg = rcont;
		}
		public override void OnResponse( Mobile from, string text )
		{
			m_engtarg.Name = text;
			from.SendMessage(StringCatalog.ResolveByKey(from.Account, "god.msg.rename.done"));
		}
	}
}
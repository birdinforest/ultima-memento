using System;
using Server;
using Server.Misc;
using Server.Mobiles;
using Server.Localization;

namespace Server.Items
{
	public class MagicQuiver : BaseQuiver
	{
		public override void AddNameProperties( ObjectPropertyList list )
		{
			// Must not assign ColorText1 here: its setter calls InvalidateProperties(), which
			// re-enters PropertyList while building OPL (stack overflow). Use backing field only.
			string savedColor = m_ColorText1;

			try
			{
				if ( BuildingPropertyListLocale != null && m_ColorText1 != null && Name != null && Name.Length > 0 )
				{
					string suffix = " " + Name;

					if ( m_ColorText1.EndsWith( suffix, StringComparison.Ordinal ) )
					{
						string adjRaw = m_ColorText1.Substring( 0, m_ColorText1.Length - suffix.Length );
						string adjLoc = StringCatalog.TryResolve( BuildingPropertyListLocale, adjRaw ) ?? adjRaw;
						string quiverLoc = ResolvePropertyText( "item.magical.magicquiver.base" );

						m_ColorText1 = string.Format( ResolvePropertyText( "prop.magical.magicquiver.name.line" ), adjLoc, quiverLoc );
					}
				}

				base.AddNameProperties( list );
			}
			finally
			{
				m_ColorText1 = savedColor;
			}
		}

		[Constructable]
		public MagicQuiver()
		{
			Name = "quiver";
			ColorText1 = RandomThings.MagicItemAdj( "start", false, false, ItemID ) + " " + Name;
			ColorHue1 = "5DAFE1";
			Hue = Utility.RandomColor(0);
		}

		public MagicQuiver( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 1 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
}
using System;
namespace Server.Items
{
	public enum ElementalMagicQuiverElement
	{
		Fire = 0,
		Cold = 1,
		Poison = 2,
		Energy = 3
	}

	/// <summary>
	/// Weak converting quiver for hoard supply: 70% physical + 30% one element.
	/// Weaker than artifact quivers (50/50 or full non-physical) so Sage / Standard Artifact stay premium.
	/// </summary>
	public class ElementalMagicQuiver : BaseQuiver
	{
		private ElementalMagicQuiverElement m_Element;

		[CommandProperty( AccessLevel.GameMaster )]
		public ElementalMagicQuiverElement Element
		{
			get { return m_Element; }
			set
			{
				m_Element = value;
				ApplyPresentation();
				InvalidateProperties();
			}
		}

		public override string DisplayNameLocalizationKey
		{
			get
			{
				switch ( m_Element )
				{
					case ElementalMagicQuiverElement.Cold: return "item.magical.elementalquiver.cold";
					case ElementalMagicQuiverElement.Poison: return "item.magical.elementalquiver.poison";
					case ElementalMagicQuiverElement.Energy: return "item.magical.elementalquiver.energy";
					default: return "item.magical.elementalquiver.fire";
				}
			}
		}

		[Constructable]
		public ElementalMagicQuiver() : this( ElementalMagicQuiverElement.Fire )
		{
		}

		[Constructable]
		public ElementalMagicQuiver( ElementalMagicQuiverElement element ) : base()
		{
			m_Element = element;
			ApplyPresentation();
		}

		public ElementalMagicQuiver( Serial serial ) : base( serial )
		{
		}

		private void ApplyPresentation()
		{
			ItemID = 0x2B02;

			switch ( m_Element )
			{
				case ElementalMagicQuiverElement.Cold:
					Name = "Quiver of Mild Frost";
					Hue = 0x998;
					break;
				case ElementalMagicQuiverElement.Poison:
					Name = "Quiver of Mild Venom";
					Hue = 0xB79;
					break;
				case ElementalMagicQuiverElement.Energy:
					Name = "Quiver of Mild Spark";
					Hue = 0x8D9;
					break;
				default:
					Name = "Quiver of Mild Embers";
					Hue = 0xB17;
					break;
			}
		}

		public override void AlterBowDamage( ref int phys, ref int fire, ref int cold, ref int pois, ref int nrgy, ref int chaos, ref int direct )
		{
			phys = 70;
			fire = cold = pois = nrgy = chaos = direct = 0;

			switch ( m_Element )
			{
				case ElementalMagicQuiverElement.Cold: cold = 30; break;
				case ElementalMagicQuiverElement.Poison: pois = 30; break;
				case ElementalMagicQuiverElement.Energy: nrgy = 30; break;
				default: fire = 30; break;
			}
		}

		public static string ElementKey( ElementalMagicQuiverElement element )
		{
			switch ( element )
			{
				case ElementalMagicQuiverElement.Cold: return "cold";
				case ElementalMagicQuiverElement.Poison: return "poison";
				case ElementalMagicQuiverElement.Energy: return "energy";
				default: return "fire";
			}
		}

		public static bool TryParseElement( string key, out ElementalMagicQuiverElement element )
		{
			element = ElementalMagicQuiverElement.Fire;

			if ( string.IsNullOrEmpty( key ) )
				return false;

			switch ( key.Trim().ToLowerInvariant() )
			{
				case "fire": element = ElementalMagicQuiverElement.Fire; return true;
				case "cold": element = ElementalMagicQuiverElement.Cold; return true;
				case "poison": element = ElementalMagicQuiverElement.Poison; return true;
				case "energy": element = ElementalMagicQuiverElement.Energy; return true;
				default: return false;
			}
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
			writer.Write( (int) m_Element );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
			m_Element = (ElementalMagicQuiverElement)reader.ReadInt();
			ApplyPresentation();
		}
	}
}

using System;
using Server.Accounting;
using Server.Localization;
using Server.Network;

namespace Server.Items
{
	public abstract class BaseSpecial : Item
	{
		public override Catalogs DefaultCatalog{ get{ return Catalogs.Crafting; } }

		public override double DefaultWeight
		{
			get { return 0.1; }
		}

		public override bool IsContentLocalized => true;

		public BaseSpecial( CraftResource resource ) : this( resource, 1 )
		{
		}

		public BaseSpecial( CraftResource resource, int amount ) : base( 0x660A )
		{
			Stackable = true;
			Amount = amount;
			Name = CraftResources.GetTradeItemFullName( this, resource, false, false, null );
			Hue = CraftResources.GetHue( resource );

			m_Resource = resource;
			Built = true;
		}

		private static string GetDisplayNameLocalized( CraftResource resource, string name, string locale )
		{
			if ( resource >= CraftResource.SpectralSpec && resource <= CraftResource.TurtleSpec )
			{
				string plain = CraftResources.GetName( resource );
				string coreKey = "resource.special." + plain.ToLower();
				string suffixKey = "resource.special.suffix";

				string core = StringCatalog.TryResolveByKey( locale, coreKey );
				string suffix = StringCatalog.TryResolveByKey( locale, suffixKey );

				if ( core != null && suffix != null )
				{
					if ( locale == "en" )
						return core + " " + suffix;
					if ( AccountLang.IsChinese( locale ) )
						return core + suffix;
					return core + " " + suffix;
				}
			}
			else
			{
				string resolved = StringCatalog.TryResolve( locale, name );
				if ( resolved != null )
					return resolved;
			}
			return name;
		}

		public override void AddNameProperty( ObjectPropertyList list )
		{
			string locale = BuildingPropertyListLocale;
			if ( locale != null )
			{
				string name = GetDisplayNameLocalized( m_Resource, Name, locale );

				if ( Amount > 1 )
					list.Add( "{0} {1}", Amount, name );
				else
					list.Add( name );
				    // list.Add( 1072171, "{0}\t{1}", "9F44FF", name ); // for color name
				return;
			}
			base.AddNameProperty( list );
		}

		public override void OnSingleClick( Mobile from )
		{
			if ( Deleted || !from.CanSee( this ) )
				return;

			if ( DisplayLootType )
				LabelLootTypeTo( from );

			NetState ns = from.NetState;

			if ( ns != null )
			{
				string lang = AccountLang.GetLanguageCode( from != null ? from.Account : null );
				string locale = AccountLang.IsChinese( lang ) ? "zh" : "en";
				string name = GetDisplayNameLocalized( m_Resource, Name, locale );

				ns.Send( new UnicodeMessage( Serial, ItemID, MessageType.Label, 0x3B2, 3, "ENU", "", name + ( Amount > 1 ? " : " + Amount : "" ) ) );
			}
		}

		public BaseSpecial( Serial serial ) : base( serial )
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
			Built = true;
		}
	}

	public class SpectralSpec : BaseSpecial
	{
		[Constructable]
		public SpectralSpec() : this( 1 )
		{
		}

		[Constructable]
		public SpectralSpec( int amount ) : base( CraftResource.SpectralSpec, amount )
		{
		}

		public SpectralSpec( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}

	public class DreadSpec : BaseSpecial
	{
		[Constructable]
		public DreadSpec() : this( 1 )
		{
		}

		[Constructable]
		public DreadSpec( int amount ) : base( CraftResource.DreadSpec, amount )
		{
		}

		public DreadSpec( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}

	public class GhoulishSpec : BaseSpecial
	{
		[Constructable]
		public GhoulishSpec() : this( 1 )
		{
		}

		[Constructable]
		public GhoulishSpec( int amount ) : base( CraftResource.GhoulishSpec, amount )
		{
		}

		public GhoulishSpec( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}

	public class WyrmSpec : BaseSpecial
	{
		[Constructable]
		public WyrmSpec() : this( 1 )
		{
		}

		[Constructable]
		public WyrmSpec( int amount ) : base( CraftResource.WyrmSpec, amount )
		{
		}

		public WyrmSpec( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}

	public class HolySpec : BaseSpecial
	{
		[Constructable]
		public HolySpec() : this( 1 )
		{
		}

		[Constructable]
		public HolySpec( int amount ) : base( CraftResource.HolySpec, amount )
		{
		}

		public HolySpec( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}

	public class BloodlessSpec : BaseSpecial
	{
		[Constructable]
		public BloodlessSpec() : this( 1 )
		{
		}

		[Constructable]
		public BloodlessSpec( int amount ) : base( CraftResource.BloodlessSpec, amount )
		{
		}

		public BloodlessSpec( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}

	public class GildedSpec : BaseSpecial
	{
		[Constructable]
		public GildedSpec() : this( 1 )
		{
		}

		[Constructable]
		public GildedSpec( int amount ) : base( CraftResource.GildedSpec, amount )
		{
		}

		public GildedSpec( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}

	public class DemilichSpec : BaseSpecial
	{
		[Constructable]
		public DemilichSpec() : this( 1 )
		{
		}

		[Constructable]
		public DemilichSpec( int amount ) : base( CraftResource.DemilichSpec, amount )
		{
		}

		public DemilichSpec( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}

	public class WintrySpec : BaseSpecial
	{
		[Constructable]
		public WintrySpec() : this( 1 )
		{
		}

		[Constructable]
		public WintrySpec( int amount ) : base( CraftResource.WintrySpec, amount )
		{
		}

		public WintrySpec( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}

	public class FireSpec : BaseSpecial
	{
		[Constructable]
		public FireSpec() : this( 1 )
		{
		}

		[Constructable]
		public FireSpec( int amount ) : base( CraftResource.FireSpec, amount )
		{
		}

		public FireSpec( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}

	public class ColdSpec : BaseSpecial
	{
		[Constructable]
		public ColdSpec() : this( 1 )
		{
		}

		[Constructable]
		public ColdSpec( int amount ) : base( CraftResource.ColdSpec, amount )
		{
		}

		public ColdSpec( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}

	public class PoisSpec : BaseSpecial
	{
		[Constructable]
		public PoisSpec() : this( 1 )
		{
		}

		[Constructable]
		public PoisSpec( int amount ) : base( CraftResource.PoisSpec, amount )
		{
		}

		public PoisSpec( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}

	public class EngySpec : BaseSpecial
	{
		[Constructable]
		public EngySpec() : this( 1 )
		{
		}

		[Constructable]
		public EngySpec( int amount ) : base( CraftResource.EngySpec, amount )
		{
		}

		public EngySpec( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}

	public class ExodusSpec : BaseSpecial
	{
		[Constructable]
		public ExodusSpec() : this( 1 )
		{
		}

		[Constructable]
		public ExodusSpec( int amount ) : base( CraftResource.ExodusSpec, amount )
		{
		}

		public ExodusSpec( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}

	public class TurtleSpec : BaseSpecial
	{
		[Constructable]
		public TurtleSpec() : this( 1 )
		{
		}

		[Constructable]
		public TurtleSpec( int amount ) : base( CraftResource.TurtleSpec, amount )
		{
		}

		public TurtleSpec( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
}
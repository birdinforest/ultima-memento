using System.Collections.Generic;
using Server.Mobiles;

namespace Server.Items
{
	/// <summary>
	/// Tight-pants temptation moves clothing pants to InnerLegs so leg armor can share Pants.
	/// Layer must be resolved in CanEquip (before Mobile.CheckEquip), or multiple pants can stack
	/// on InnerLegs with doubled bonuses while the paperdoll only shows one.
	/// </summary>
	public static class PantsTemptationLayer
	{
		public static bool IsTemptationPants( Item item )
		{
			return item is BasePants || item is BaseGiftPants || item is BaseLevelPants;
		}

		public static void Resolve( Item pants, Mobile from )
		{
			PlayerMobile pm = from as PlayerMobile;
			pants.Layer = ( pm != null && pm.Temptations.CanWearTightPants )
				? Layer.InnerLegs
				: Layer.Pants;
		}

		/// <summary>
		/// One-time recovery for characters that already stacked multiple pants on the same layer.
		/// Keeps the first worn pants per layer; extras go to the backpack (bonuses cleared via OnRemoved).
		/// </summary>
		public static void DeduplicateWornPants( Mobile m )
		{
			if ( m == null || m.Items == null )
				return;

			DeduplicateLayer( m, Layer.InnerLegs );
			DeduplicateLayer( m, Layer.Pants );
		}

		private static void DeduplicateLayer( Mobile m, Layer layer )
		{
			Item keep = null;
			List<Item> extras = null;

			for ( int i = 0; i < m.Items.Count; i++ )
			{
				Item item = m.Items[i];
				if ( item == null || item.Layer != layer || !IsTemptationPants( item ) )
					continue;

				if ( keep == null )
				{
					keep = item;
				}
				else
				{
					if ( extras == null )
						extras = new List<Item>();
					extras.Add( item );
				}
			}

			if ( extras == null )
				return;

			for ( int i = 0; i < extras.Count; i++ )
			{
				Item pants = extras[i];
				m.RemoveItem( pants );
				m.AddToBackpack( pants );
			}
		}
	}

	public abstract class BasePants : BaseClothing
	{
		public BasePants( int itemID ) : this( itemID, 0 )
		{
		}

		public BasePants( int itemID, int hue ) : base( itemID, Layer.Pants, hue )
		{
		}

		public BasePants( Serial serial ) : base( serial )
		{
		}

		public override bool CanEquip( Mobile from )
		{
			PantsTemptationLayer.Resolve( this, from );
			return base.CanEquip( from );
		}

		public override bool OnEquip( Mobile from )
		{
			if ( !base.OnEquip( from ) )
				return false;

			PantsTemptationLayer.Resolve( this, from );
			return true;
		}

		public override void OnRemoved(object parent)
		{
			base.OnRemoved(parent);

			Layer = Layer.Pants;
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

	public class ShortPants : BasePants
	{
		[Constructable]
		public ShortPants() : this( 0 )
		{
		}

		[Constructable]
		public ShortPants( int hue ) : base( 0x152E, hue )
		{
			Weight = 2.0;
			Name = "short pants";
			ItemID = Utility.RandomList( 0x152e, 0x152f );
		}

		public ShortPants( Serial serial ) : base( serial )
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

	public class LongPants : BasePants
	{
		[Constructable]
		public LongPants() : this( 0 )
		{
		}

		[Constructable]
		public LongPants( int hue ) : base( 0x1539, hue )
		{
			Weight = 2.0;
			Name = "long pants";
			ItemID = Utility.RandomList( 0x1539, 0x153a );

		}

		public LongPants( Serial serial ) : base( serial )
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

	[Flipable( 0x279B, 0x27E6 )]
	public class TattsukeHakama : BasePants
	{
		[Constructable]
		public TattsukeHakama() : this( 0 )
		{
		}

		[Constructable]
		public TattsukeHakama( int hue ) : base( 0x279B, hue )
		{
			Weight = 2.0;
		}

		public TattsukeHakama( Serial serial ) : base( serial )
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

	public class SailorPants : BasePants
	{
		[Constructable]
		public SailorPants() : this( 0 )
		{
		}

		[Constructable]
		public SailorPants( int hue ) : base( 0x309, hue )
		{
			Name = "sailor pants";
			Weight = 2.0;
		}

		public SailorPants( Serial serial ) : base( serial )
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

	public class PiratePants : BasePants
	{
		[Constructable]
		public PiratePants() : this( 0 )
		{
		}

		[Constructable]
		public PiratePants( int hue ) : base( 0x404, hue )
		{
			Name = "pirate pants";
			Weight = 2.0;
		}

		public PiratePants( Serial serial ) : base( serial )
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
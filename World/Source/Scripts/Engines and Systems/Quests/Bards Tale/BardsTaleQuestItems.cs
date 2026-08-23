using System;
using Server;
using Server.Localization;

namespace Server.Items
{
	public class EyeOfTarjan : Item
	{
		public override string DisplayNameLocalizationKey => "item.special.bards_tale.eye_of_tarjan";
		public override bool IsContentLocalized => true;
		public override int QuestItemHue => 0x5B7;

		[Constructable]
		public EyeOfTarjan() : base( 0x1F1C )
		{
			Weight = 1.0;
			Hue = 0x5B7;
			LootType = LootType.Blessed;
			QuestItem = true;
		}

		public static bool ExistsOn( Mobile from )
		{
			return from != null && from.Backpack != null && from.Backpack.FindItemByType( typeof( EyeOfTarjan ) ) != null;
		}

		public static bool GrantIfMissing( Mobile from )
		{
			if ( from == null || from.Backpack == null || ExistsOn( from ) )
				return false;

			from.AddToBackpack( new EyeOfTarjan() );
			return true;
		}

		public static void ConsumeFrom( Mobile from )
		{
			if ( from == null || from.Backpack == null )
				return;

			Item eye = from.Backpack.FindItemByType( typeof( EyeOfTarjan ) );

			if ( eye != null )
				eye.Delete();
		}

		public override void AddNameProperties( ObjectPropertyList list )
		{
			base.AddNameProperties( list );

			if ( BuildingPropertyListLocale != null )
				AddLocalizedProperty( list, "prop.bards_tale.eye_of_tarjan.hint" );
		}

		public EyeOfTarjan( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			reader.ReadInt();
		}
	}

	public class HarkynDragonKey : Item
	{
		public override string DisplayNameLocalizationKey => "item.special.bards_tale.harkyn_key";
		public override bool IsContentLocalized => true;
		public override int QuestItemHue => 0x66D;

		[Constructable]
		public HarkynDragonKey() : base( 0x100F )
		{
			Weight = 1.0;
			Hue = 0x66D;
			LootType = LootType.Blessed;
			QuestItem = true;
		}

		public static bool ExistsOn( Mobile from )
		{
			return from != null && from.Backpack != null && from.Backpack.FindItemByType( typeof( HarkynDragonKey ) ) != null;
		}

		public static bool GrantIfMissing( Mobile from )
		{
			if ( from == null || from.Backpack == null || ExistsOn( from ) )
				return false;

			from.AddToBackpack( new HarkynDragonKey() );
			return true;
		}

		public override void AddNameProperties( ObjectPropertyList list )
		{
			base.AddNameProperties( list );

			if ( BuildingPropertyListLocale != null )
				AddLocalizedProperty( list, "prop.bards_tale.harkyn_key.hint" );
		}

		public HarkynDragonKey( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			reader.ReadInt();
		}
	}
}

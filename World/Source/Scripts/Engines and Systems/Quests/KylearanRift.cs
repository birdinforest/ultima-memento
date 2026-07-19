using System;
using Server.Localization;
using Server.Misc;
using Server.Mobiles;
using Server.Network;
using Server.Regions;

namespace Server.Items
{
	public class KylearanRift : Item
	{
		private static readonly Point3D ExitLocation = new Point3D( 2830, 1874, 95 );
		private static readonly Map ExitMap = Map.Sosaria;

		public override string DisplayNameLocalizationKey => "item.special.kylearan_rift";
		public override bool IsContentLocalized => true;

		[Constructable]
		public KylearanRift() : base( 0x1FD4 )
		{
			Name = "an unstable rift";
			Weight = 1.0;
			Hue = 0x497;
			Movable = false;
		}

		public override void AddNameProperties( ObjectPropertyList list )
		{
			base.AddNameProperties( list );

			if ( BuildingPropertyListLocale != null )
				AddLocalizedProperty( list, "quest.bards_tale.rift.hint" );
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( !from.InRange( GetWorldLocation(), 2 ) )
			{
				from.SendLocalizedMessage( 502138 );
				return;
			}

			if ( !( from is PlayerMobile ) )
				return;

			if ( PlayerSettings.GetBardsTaleQuest( from, StringCatalog.ResolveByKey( null, "mob.other.bardstalewin" ) ) )
			{
				from.SendMessage( StringCatalog.ResolveByKey( from.Account, "quest.bards_tale.rift.closed" ) );
				return;
			}

			if ( from.Combatant != null )
			{
				from.SendMessage( StringCatalog.ResolveByKey( from.Account, "quest.bards_tale.rift.combat" ) );
				return;
			}

			if ( !from.Region.IsPartOf( typeof( BardTownRegion ) ) )
			{
				from.SendMessage( StringCatalog.ResolveByKey( from.Account, "quest.bards_tale.rift.not_safe" ) );
				return;
			}

			BaseCreature.TeleportPets( from, ExitLocation, ExitMap, false );
			from.MoveToWorld( ExitLocation, ExitMap );
			from.PlaySound( 0x1FE );

			from.SendMessage( StringCatalog.ResolveByKey( from.Account, "quest.bards_tale.rift.message" ) );

			KylearanKeepsake.GrantOrRefresh( from );

			if ( from is PlayerMobile pm )
				PlayerSettings.TryStartSkaraBraeKylearanContract( pm );
		}

		public override void OnDoubleClickDead( Mobile from )
		{
			OnDoubleClick( from );
		}

		public KylearanRift( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int)0 );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
}

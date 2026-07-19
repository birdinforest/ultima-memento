using System;
using Server;
using Server.Localization;
using Server.Network;

namespace Server.Items
{
	interface IDurability
	{
		bool CanFortify { get; }

		int InitMinHits { get; }
		int InitMaxHits { get; }

		int HitPoints { get; set; }
		int MaxHitPoints { get; set; }

		void ScaleDurability();
		void UnscaleDurability();
	}

	interface IWearableDurability : IDurability
	{
		int OnHit( BaseWeapon weapon, int damageTaken );
	}

	/// <summary>
	/// Shared wear/break feedback for weapons, armor, clothing, and shields.
	/// </summary>
	public static class DurabilityUtility
	{
		private const int DestroyHue = 0x22;

		public static void DestroyFromWear( Item item )
		{
			if ( item == null || item.Deleted )
				return;

			Mobile parent = item.Parent as Mobile;

			if ( parent == null )
				parent = item.RootParent as Mobile;

			if ( parent != null )
			{
				string msg;

				if ( !String.IsNullOrEmpty( item.Name ) )
					msg = StringCatalog.ResolveFormatByKey( parent.Account, "prop.durability.destroyed", item.Name );
				else
					msg = StringCatalog.ResolveByKey( parent.Account, "prop.durability.destroyed.unnamed" );

				parent.LocalOverheadMessage( MessageType.Regular, DestroyHue, true, msg );
				parent.SendMessage( DestroyHue, msg );
			}

			item.Delete();
		}
	}
}

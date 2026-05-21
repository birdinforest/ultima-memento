using System;
using Server;
using Server.Items;
using Server.Localization;
using Server.Mobiles;
using Server.Network;

namespace Server.Gumps
{
	/// <summary>
	/// GM-spawned restoration bundle held on <see cref="LostItemsRestorerNPC"/>.
	/// Only the designated target player (and staff) may open it; no one may lift it
	/// or its contents while it awaits handoff via the salvager dialog.
	/// </summary>
	public class CharRestoreBag : Bag
	{
		private string m_TargetName;
		private bool m_SecuredOnNpc = true;

		[CommandProperty( AccessLevel.GameMaster )]
		public string TargetName
		{
			get { return m_TargetName; }
			set { m_TargetName = value; }
		}

		[Constructable]
		public CharRestoreBag() : base()
		{
			Movable = false;
		}

		public CharRestoreBag( Serial serial ) : base( serial ) {}

		/// <summary>
		/// Called when the salvager hands this bundle to the player.
		/// Lifts NPC-era restrictions so the bag behaves like normal loot.
		/// </summary>
		public void ReleaseToPlayer()
		{
			m_SecuredOnNpc = false;
			Movable = true;
		}

		private bool IsTargetPlayer( Mobile from )
		{
			if ( from == null || string.IsNullOrWhiteSpace( m_TargetName ) )
				return false;

			return m_TargetName.Equals( from.Name, StringComparison.OrdinalIgnoreCase );
		}

		private bool IsAuthorized( Mobile from )
		{
			if ( from == null )
				return false;

			if ( from.AccessLevel >= AccessLevel.GameMaster )
				return true;

			return IsTargetPlayer( from );
		}

		public override bool IsAccessibleTo( Mobile check )
		{
			if ( !m_SecuredOnNpc )
				return base.IsAccessibleTo( check );

			if ( !IsAuthorized( check ) )
				return false;

			return base.IsAccessibleTo( check );
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( m_SecuredOnNpc && !IsAuthorized( from ) )
			{
				if ( from != null )
					from.SendMessage( 0x22, ResolveDeniedOpen( from ) );
				return;
			}

			base.OnDoubleClick( from );
		}

		public override bool CheckItemUse( Mobile from, Item item )
		{
			if ( m_SecuredOnNpc && !IsAuthorized( from ) )
				return false;

			return base.CheckItemUse( from, item );
		}

		public override bool CheckLift( Mobile from, Item item, ref LRReason reject )
		{
			if ( !m_SecuredOnNpc )
				return base.CheckLift( from, item, ref reject );

			if ( from != null && from.AccessLevel >= AccessLevel.GameMaster )
				return base.CheckLift( from, item, ref reject );

			reject = LRReason.Inspecific;
			return false;
		}

		public override bool OnDragDrop( Mobile from, Item dropped )
		{
			if ( m_SecuredOnNpc )
				return false;

			return base.OnDragDrop( from, dropped );
		}

		public override bool OnDragDropInto( Mobile from, Item item, Point3D p )
		{
			if ( m_SecuredOnNpc )
				return false;

			return base.OnDragDropInto( from, item, p );
		}

		private static string ResolveDeniedOpen( Mobile from )
		{
			string msg = StringCatalog.TryResolveByKey(
				AccountLang.GetLanguageCode( from.Account ),
				"charrestore.npc.bag_denied_open" );

			return !string.IsNullOrEmpty( msg )
				? msg
				: "This parcel is not yours to open.";
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 1 ); // version (1 adds m_SecuredOnNpc)
			writer.Write( m_TargetName );
			writer.Write( m_SecuredOnNpc );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
			m_TargetName = reader.ReadString();

			if ( version >= 1 )
				m_SecuredOnNpc = reader.ReadBool();
			else
				m_SecuredOnNpc = true;

			if ( m_SecuredOnNpc )
				Movable = false;
		}
	}
}

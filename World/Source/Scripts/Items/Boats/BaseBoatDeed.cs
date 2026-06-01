using System;
using Server;
using Server.Regions;
using Server.Targeting;
using Server.Network;
using Server.Misc;
using Server.Localization;

namespace Server.Multis
{
	public abstract class BaseBoatDeed : Item
	{
		private int m_MultiID;
		private Point3D m_Offset;

		[CommandProperty( AccessLevel.GameMaster )]
		public int MultiID{ get{ return m_MultiID; } set{ m_MultiID = value; } }

		[CommandProperty( AccessLevel.GameMaster )]
		public Point3D Offset{ get{ return m_Offset; } set{ m_Offset = value; } }

		public BaseBoatDeed( int id, Point3D offset ) : base( 0x14F2 )
		{
			Weight = 1.0;
			Hue = 0x47E;

			m_MultiID = id;
			m_Offset = offset;
		}

		public BaseBoatDeed( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 2 ); // version
			writer.Write( m_MultiID );
			writer.Write( m_Offset );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
			m_MultiID = reader.ReadInt();
			m_Offset = reader.ReadPoint3D();
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( BaseBoat.isCarpet( Boat ) )
			{
				Name = "magic carpet deed";
				OnDoubleClickCarpet( from );
				return;
			}
			else
			{
				Name = "ship deed";
			}

			Region reg = Region.Find( from.Location, from.Map );

			string placeMsg = "Where do you wish to place the ship?";
			string denyMsg = "You may not place a boat from this location.";

			if ( !IsChildOf( from.Backpack ) )
			{
				from.SendLocalizedMessage( 1042001 ); // That must be in your pack for you to use it.
			}
			else if ( DockSearch.NearDock(from) == false )
			{
				from.SendMessage( StringCatalog.Resolve( from.Account, "You must be near a dock to launch your ship!" ) );
			}
			else if (
				Server.Misc.Worlds.IsSeaTown( from.Location, from.Map ) ||
				reg.IsPartOf( typeof( OutDoorBadRegion ) ) ||
				reg.IsPartOf( typeof( VillageRegion ) ) ||
				reg.IsPartOf( typeof( BargeDeadRegion ) ) ||
				reg.IsPartOf( typeof( NecromancerRegion ) ) ||
				reg.IsPartOf( typeof( DeadRegion ) ) ||
				reg.IsPartOf( typeof( PirateRegion ) ) ||
				reg.IsPartOf( typeof( OutDoorRegion ) ) ||
				reg.IsPartOf( typeof( PublicRegion ) ) ||
				Server.Misc.Worlds.IsMainRegion( Server.Misc.Worlds.GetRegionName( from.Map, from.Location ) ) )
			{
				from.LocalOverheadMessage(Network.MessageType.Emote, 0x25, false, StringCatalog.Resolve( from.Account, placeMsg ) );
				from.Target = new ShipDeedTarget( this );
			}
			else
			{
				from.LocalOverheadMessage(Network.MessageType.Emote, 0x25, false, StringCatalog.Resolve( from.Account, denyMsg ) );
			}
		}

		private void OnDoubleClickCarpet( Mobile from )
		{
			string placeMsg = StringCatalog.Resolve( from.Account, "Where do you wish to place the carpet?" );
			string denyMsg = StringCatalog.Resolve( from.Account, "There is not magic from the carpet in this location." );
			string blockedMsg = StringCatalog.Resolve( from.Account, "You may not place the carpet while on a ship or carpet, or inside a house." );

			if ( !IsChildOf( from.Backpack ) )
			{
				from.SendLocalizedMessage( 1042001 ); // That must be in your pack for you to use it.
			}
			else if ( from.Region.IsPartOf( typeof( HouseRegion ) ) || BaseBoat.FindBoatAt( from, from.Map ) != null )
			{
				from.SendMessage( blockedMsg );
			}
			else if ( from.Region.IsPartOf( typeof( DungeonRegion ) ) )
			{
				from.SendMessage( denyMsg );
			}
			else
			{
				from.LocalOverheadMessage( Network.MessageType.Emote, 0x25, false, placeMsg );
				from.Target = new CarpetDeedTarget( this );
			}
		}

		public abstract BaseBoat Boat{ get; }

		public void OnCarpetPlacement( Mobile from, Point3D p, int hue )
		{
			if ( Deleted )
			{
				return;
			}
			else if ( !IsChildOf( from.Backpack ) )
			{
				from.SendLocalizedMessage( 1042001 ); // That must be in your pack for you to use it.
			}
			else
			{
				string blockedMsg = StringCatalog.Resolve( from.Account, "You may not place the carpet while on a ship or carpet, or inside a house." );
				string failMsg = StringCatalog.Resolve( from.Account, "The magic of the carpet cannot be used here." );

				Map map = from.Map;

				if ( map == null )
					return;

				if ( from.Region.IsPartOf( typeof( HouseRegion ) ) || BaseBoat.FindBoatAt( from, from.Map ) != null )
				{
					from.SendMessage( blockedMsg );
					return;
				}

				BaseBoat boat = Boat;
				boat.Hue = hue;

				if ( boat == null )
					return;

				p = new Point3D( p.X - m_Offset.X, p.Y - m_Offset.Y, p.Z - m_Offset.Z );

				Region targetReg = Region.Find( p, map );

				if ( targetReg.IsPartOf( typeof( DungeonRegion ) ) || targetReg.IsPartOf( typeof( HouseRegion ) ) )
				{
					boat.Delete();
					from.SendMessage( failMsg );
					return;
				}

				if ( BaseBoat.IsValidLocation( p, map ) && boat.CanFit( p, map, boat.ItemID ) )
				{
					Delete();

					boat.Owner = from;
					boat.Anchored = true;

					uint keyValue = boat.CreateKeys( from );

					if ( boat.PPlank != null )
						boat.PPlank.KeyValue = keyValue;

					if ( boat.SPlank != null )
						boat.SPlank.KeyValue = keyValue;

					if ( boat.TillerMan != null )
						boat.TillerMan.Hue = hue;

					if ( boat.Hold != null )
						boat.Hold.Hue = hue;

					if ( boat.PPlank != null )
						boat.PPlank.Hue = hue;

					if ( boat.SPlank != null )
						boat.SPlank.Hue = hue;

					boat.MoveToWorld( p, map );
					from.PlaySound( 0x1FD );
				}
				else
				{
					boat.Delete();
					from.SendMessage( failMsg );
				}
			}
		}

		public void OnPlacement( Mobile from, Point3D p, int hue )
		{
			if ( Deleted )
			{
				return;
			}
			else if ( !IsChildOf( from.Backpack ) )
			{
				from.SendLocalizedMessage( 1042001 ); // That must be in your pack for you to use it.
			}
			else
			{
				string phrase_a = StringCatalog.Resolve( from.Account, "You may not place a ship while on another ship or inside a house." );
				string phrase_b = StringCatalog.Resolve( from.Account, "A ship can not be launched here." );

				Map map = from.Map;
				Region reg = Region.Find( from.Location, from.Map );

				if ( map == null )
					return;

				if ( from.Region.IsPartOf( typeof( HouseRegion ) ) || BaseBoat.FindBoatAt( from, from.Map ) != null )
				{
					from.SendMessage( phrase_a );
					return;
				}

				BaseBoat boat = Boat;
				boat.Hue = hue;

				if ( boat == null )
					return;

				p = new Point3D( p.X - m_Offset.X, p.Y - m_Offset.Y, p.Z - m_Offset.Z );

				bool CanBuild = false;

				if ( reg.IsPartOf( typeof( OutDoorBadRegion ) ) ||
					 reg.IsPartOf( typeof( VillageRegion ) ) ||
					 reg.IsPartOf( typeof( BargeDeadRegion ) ) ||
					 reg.IsPartOf( typeof( NecromancerRegion ) ) ||
					 reg.IsPartOf( typeof( DeadRegion ) ) ||
					 reg.IsPartOf( typeof( PirateRegion ) ) ||
					 reg.IsPartOf( typeof( OutDoorRegion ) ) ||
					 reg.IsPartOf( typeof( PublicRegion ) ) )
				{
					CanBuild = false;
				}
				else
				{
					CanBuild = true;
				}

				if ( CanBuild )
				{
					if ( !Server.Misc.Worlds.IsSeaTown( p, map ) &&
						 !Server.Misc.Worlds.IsMainRegion( Server.Misc.Worlds.GetRegionName( map, p ) ) )
					{
						CanBuild = BaseBoat.IsValidLocation( p, map );
					}
					else
					{
						CanBuild = false;
					}
				}

				if ( !CanBuild )
				{
					from.SendMessage( phrase_b );
				}
				else
				{
					boat.Owner = from;
					boat.Anchored = true;

					uint keyValue = boat.CreateKeys( from );

					if ( boat.PPlank != null )
						boat.PPlank.KeyValue = keyValue;

					if ( boat.SPlank != null )
						boat.SPlank.KeyValue = keyValue;

					boat.MoveToWorld( p, map );
					this.Delete();
				}
			}
		}

		private class CarpetDeedTarget : MultiTarget
		{
			private BaseBoatDeed m_Deed;
			private int m_Hue;

			public CarpetDeedTarget( BaseBoatDeed deed ) : base( deed.MultiID, deed.Offset )
			{
				m_Deed = deed;
				m_Hue = deed.Hue;
			}

			protected override void OnTarget( Mobile from, object o )
			{
				if ( m_Deed == null || m_Deed.Deleted )
					return;

				IPoint3D ip = o as IPoint3D;

				if ( ip == null )
					return;

				if ( ip is Item )
					ip = ((Item)ip).GetWorldTop();

				Point3D p = new Point3D( ip );
				Region region = Region.Find( p, from.Map );

				if ( region.IsPartOf( typeof( DungeonRegion ) ) )
					from.SendLocalizedMessage( 502488 ); // You can not place a ship inside a dungeon.
				else if ( region.IsPartOf( typeof( HouseRegion ) ) )
					from.SendLocalizedMessage( 1042549 ); // A boat may not be placed in this area.
				else
					m_Deed.OnCarpetPlacement( from, p, m_Hue );
			}
		}

		private class ShipDeedTarget : Target
		{
			private BaseBoatDeed m_Deed;

			public ShipDeedTarget( BaseBoatDeed deed ) : base( 5, true, TargetFlags.None )
			{
				m_Deed = deed;
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				if ( m_Deed == null || m_Deed.Deleted )
					return;

				if ( targeted is LandTarget )
				{
					IPoint3D p = targeted as IPoint3D;
					m_Deed.OnPlacement( from, new Point3D( p ), m_Deed.Hue );
				}
				else
				{
					from.SendMessage( StringCatalog.Resolve( from.Account, "You may not place a boat from this location." ) );
				}
			}
		}
	}
}

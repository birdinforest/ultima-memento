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
			}
			else
			{
				Name = "ship deed";
			}

			Region reg = Region.Find( from.Location, from.Map );

			string placeMsg = "Where do you wish to place the ship?";
			string denyMsg = "You may not place a boat from this location.";
			if ( BaseBoat.isCarpet( Boat ) )
			{
				placeMsg = "Where do you wish to place the carpet?";
				denyMsg = "There is not magic from the carpet in this location.";
			}

			if ( !IsChildOf( from.Backpack ) )
			{
				from.SendLocalizedMessage( 1042001 ); // That must be in your pack for you to use it.
			}
			else if ( DockSearch.NearDock(from) == false && !BaseBoat.isCarpet( Boat ) )
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
				from.Target = new InternalTarget( this );
			}
			else
			{
				from.LocalOverheadMessage(Network.MessageType.Emote, 0x25, false, StringCatalog.Resolve( from.Account, denyMsg ) );
			}
		}

		public abstract BaseBoat Boat{ get; }

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
				if ( BaseBoat.isCarpet( Boat ) )
				{
					phrase_a = StringCatalog.Resolve( from.Account, "You may not place the carpet while on a ship or carpet, or inside a house." );
				}

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
				else if ( BaseBoat.isCarpet( Boat ) && reg.IsPartOf( typeof( DungeonRegion ) ) )
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
					boat.BoatDeed = this;
					boat.Owner = from;
					boat.Map = map;
					boat.Location = p;
					boat.BaseAddonResolve( from );

					boat.LockKey( from );

					boat.TurnOn( true );
					this.Delete();
				}
			}
		}

		private class InternalTarget : Target
		{
			private BaseBoatDeed m_Deed;

			public InternalTarget( BaseBoatDeed deed ) : base( 5, true, TargetFlags.None )
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

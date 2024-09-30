using System;
using Server;
using Server.Network;
using Server.Regions;
using Server.Mobiles;
using Server.Misc;

namespace Server.Items
{
	public abstract class FarmableCrop : Item
	{
		private bool m_Picked;

		public abstract Item GetCropObject();
		public abstract int GetPickedID();

		public FarmableCrop( int itemID ) : base( itemID )
		{
			Movable = false;
		}

		public override bool HandlesOnMovement{ get { return true; } }

		public override void OnMovement( Mobile m, Point3D oldLocation )
		{
			const int range = 1;
			if ( !CanHarvest( m, range ) ) return;

			Timer.DelayCall( TimeSpan.FromSeconds( 0.5 ), new TimerStateCallback ( Harvest ), new object[]{ m, range }  );
		}

		public override bool OnMoveOver( Mobile m )
		{
			OnDoubleClick( m );

			return true;
		}

		public override void OnDoubleClick( Mobile from )
		{
			const int range = 2;
			if (!CanHarvest( from, range )) return;
			
			if ( !from.InRange( Location, 2 ) || !from.InLOS( this ) )
			{
				from.LocalOverheadMessage( MessageType.Regular, 0x3B2, 1019045 ); // I can't reach that.
				return;
			}
			
			Harvest( new object[]{ from, range } );
		}

		public virtual void OnPicked( Mobile from, Point3D loc, Map map )
		{
			ItemID = GetPickedID();

			Item spawn = GetCropObject();

			if ( spawn.Stackable )
			{
				int pile = MyServerSettings.Resources();
				if ( from.Land == Land.IslesDread )
					pile = pile * 2;

				spawn.Amount = pile;

				if ( !(this is FarmableWheat) ){ spawn.Amount = 1; }
			}

			if ( spawn != null )
			{
				if ( from.PlaceInBackpack( spawn ) )
				{
					from.SendMessage( "You put it in your backpack." );
				}
				else
				{
					from.SendMessage( "You can't fit it in your backpack!" );
					spawn.MoveToWorld( loc, map );
				}
			}

			m_Picked = true;
		}

		public void Unlink()
		{
			ISpawner se = this.Spawner;

			if ( se != null )
			{
				this.Spawner.Remove( this );
				this.Spawner = null;
			}

		}

		private void Harvest( object state )
		{
			object[] states = (object[])state;
			Mobile from = (Mobile)states[0];
			int range = (int)states[1];
			
			if ( !CanHarvest( from, range ) ) return;

			OnPicked( from, Location, Map );
		}

		private bool CanHarvest( Mobile m, int range )
		{
			if ( Parent != null || Movable || IsLockedDown || IsSecure || Map == null || Map == Map.Internal )
				return false;

			return m is PlayerMobile && m.Alive && !m_Picked && m.InRange( Location, range ) && m.InLOS( this );
		}

		public FarmableCrop( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.WriteEncodedInt( 0 ); // version
			writer.Write( m_Picked );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadEncodedInt();
			switch ( version )
			{
				case 0:
					m_Picked = reader.ReadBool();
					break;
			}
			if ( m_Picked )
			{
				//Unlink();
				//Delete();
			}
		}
	}
}
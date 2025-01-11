using Server.Mobiles;
using Server.Network;
using System.Collections;
using System.IO;
using System;

namespace Server.Items
{
    public class TaskManager200Min : Item
	{
		[Constructable]
		public TaskManager200Min () : base( 0x0EDE )
		{
			Movable = false;
			Name = "Task Manager 200 Minutes";
			Visible = false;
			TaskTimer thisTimer = new TaskTimer( this ); 
			thisTimer.Start(); 
		}

        public TaskManager200Min(Serial serial) : base(serial)
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

			if ( MySettings.S_RunRoutinesAtStartup && !( File.Exists( "Data/Data.ref" ) ) )
			{
				FirstTimer thisTimer = new FirstTimer( this ); 
				thisTimer.Start();
			}
			else
			{
				TaskTimer thisTimer = new TaskTimer( this ); 
				thisTimer.Start(); 
			}
		}

		public class TaskTimer : Timer 
		{ 
			private Item i_item; 
			public TaskTimer( Item task ) : base( TimeSpan.FromMinutes( 200.0 ) )
			{ 
				Priority = TimerPriority.OneMinute; 
				i_item = task; 
			} 

			protected override void OnTick() 
			{
				RunThis( false, i_item );
			} 
		}

		public class FirstTimer : Timer 
		{ 
			private Item i_item; 
			public FirstTimer( Item task ) : base( TimeSpan.FromSeconds( 10.0 ) )
			{ 
				Priority = TimerPriority.OneSecond; 
				i_item = task; 
			} 

			protected override void OnTick() 
			{
				RunThis( true, i_item );
			} 
		}

		public static void RunThis( bool DoAction, Item it )
		{
			TaskTimer thisTimer = new TaskTimer( it ); 
			thisTimer.Start(); 

			ArrayList spawns = new ArrayList();
			if ( MySettings.ConsoleLog ){ Console.WriteLine( "(200) Rebuild Coffers, Stumps, and Hay..." ); }
			foreach ( Item item in World.Items.Values )
			{
				if ( item is PremiumSpawner )
				{
					PremiumSpawner spawner = (PremiumSpawner)item;

					if ( spawner.SpawnID == 8888 )
					{
						bool reconfigure = true;

						foreach ( NetState state in NetState.Instances )
						{
							Mobile m = state.Mobile;

							if ( m is PlayerMobile && m.InRange( spawner.Location, (spawner.HomeRange+20) ) )
							{
								reconfigure = false;
							}
						}

						if ( reconfigure ){ spawns.Add( item ); }
					}
				}
				else if ( item is Coffer )
				{
					Coffer coffer = (Coffer)item;
					Server.Items.Coffer.SetupCoffer( coffer );
				}
				else if ( item is HayCrate || item is HollowStump )
				{
					item.Stackable = false;
				}
			}

			if ( MySettings.ConsoleLog ){ Console.WriteLine( "(200) Reconfigure Spawners..." ); }
			for ( int i = 0; i < spawns.Count; ++i )
			{
				PremiumSpawner spawners = ( PremiumSpawner )spawns[ i ];
				Server.Mobiles.PremiumSpawner.Reconfigure( spawners, DoAction );
			}
			if ( MySettings.ConsoleLog )
				Console.WriteLine( "(200 Minute) Tasks Complete!" );
			if ( MySettings.S_RunRoutinesAtStartup && DoAction && !( File.Exists( "Data/Data.ref" ) ) )
			{
				Console.WriteLine("You may now play " + MySettings.S_ServerName + "!");
				Console.WriteLine("");
			}
		}
	}
}

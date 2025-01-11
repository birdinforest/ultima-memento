using System.IO;
using System;

namespace Server.Items
{
    public class TaskManager250Min : Item
	{
		[Constructable]
		public TaskManager250Min () : base( 0x0EDE )
		{
			Movable = false;
			Name = "Task Manager 250 Minutes";
			Visible = false;
			TaskTimer thisTimer = new TaskTimer( this ); 
			thisTimer.Start(); 
		}

        public TaskManager250Min(Serial serial) : base(serial)
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
			public TaskTimer( Item task ) : base( TimeSpan.FromMinutes( 250.0 ) )
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
			public FirstTimer( Item task ) : base( TimeSpan.FromSeconds( 5.0 ) )
			{ 
				Priority = TimerPriority.OneSecond; 
				i_item = task; 
			} 

			protected override void OnTick() 
			{
				RunThis( true, i_item );
			} 
		}

		public static void RunThis( bool DoAction, Item item )
		{
			TaskTimer thisTimer = new TaskTimer( item ); 
			thisTimer.Start(); 

			///// ADD RANDOM CITIZENS IN SETTLEMENTS /////////////////////
			if ( MySettings.ConsoleLog ){ Console.WriteLine( "(250) Repopulate Cities..." ); }
			Server.Mobiles.Citizens.PopulateCities();
			if ( MySettings.ConsoleLog ){ Console.WriteLine( "(250) Repopulate Homes..." ); }
			Server.Items.TavernTable.PopulateHomes();
			if ( MySettings.ConsoleLog ){ Console.WriteLine( "(250) Repopulate Villages..." ); }
			Server.Items.WorkingSpots.PopulateVillages();
			if ( MySettings.ConsoleLog ){ Console.WriteLine( "(250 Minute) Tasks Complete!" ); }
		}
	}
}

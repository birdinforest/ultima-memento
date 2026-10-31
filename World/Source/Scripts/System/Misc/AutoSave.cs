using System;
using System.IO;
using System.Globalization;
using System.Diagnostics;
using Server;
using Server.Commands;
using System.Collections;
using System.Collections.Generic;
using Server.Mobiles;
using Server.Misc;
using Server.Items;
using Server.Targeting;
using Server.Network;

namespace Server.Misc
{
	public class AutoSave : Timer
	{
		private static TimeSpan m_Delay = TimeSpan.FromMinutes( MyServerSettings.ServerSaveMinutes() );
		private static TimeSpan m_Warning = TimeSpan.Zero;

		public static void Initialize()
		{
			new AutoSave().Start();
			CommandSystem.Register( "SetSaves", AccessLevel.Administrator, new CommandEventHandler( SetSaves_OnCommand ) );
		}

		private static bool m_SavesEnabled = true;

		public static bool SavesEnabled
		{
			get{ return m_SavesEnabled; }
			set{ m_SavesEnabled = value; }
		}

		[Usage( "SetSaves <true | false>" )]
		[Description( "Enables or disables automatic shard saving." )]
		public static void SetSaves_OnCommand( CommandEventArgs e )
		{
			if ( e.Length == 1 )
			{
				m_SavesEnabled = e.GetBoolean( 0 );
				e.Mobile.SendMessage( "Saves have been {0}.", m_SavesEnabled ? "enabled" : "disabled" );
			}
			else
			{
				e.Mobile.SendMessage( "Format: SetSaves <true | false>" );
			}
		}

		public AutoSave() : base( m_Delay - m_Warning, m_Delay )
		{
			Priority = TimerPriority.OneMinute;
		}

		protected override void OnTick()
		{
			if ( !m_SavesEnabled || AutoRestart.Restarting )
				return;

			if ( m_Warning == TimeSpan.Zero )
			{
				Save( true );
			}
			else
			{
				int s = (int)m_Warning.TotalSeconds;
				int m = s / 60;
				s %= 60;

				if ( m > 0 && s > 0 )
					World.Broadcast( 0x35, true, "The world will save in {0} minute{1} and {2} second{3}.", m, m != 1 ? "s" : "", s, s != 1 ? "s" : "" );
				else if ( m > 0 )
					World.Broadcast( 0x35, true, "The world will save in {0} minute{1}.", m, m != 1 ? "s" : "" );
				else
					World.Broadcast( 0x35, true, "The world will save in {0} second{1}.", s, s != 1 ? "s" : "" );

				Timer.DelayCall( m_Warning, new TimerCallback( Save ) );
			}
		}

		public static void Save()
		{
			AutoSave.Save( false );
		}

		public static void Save( bool permitBackgroundWrite )
		{
			if ( AutoRestart.Restarting )
				return;

			Console.WriteLine( "AutoSave: waiting for prior disk write..." );
			World.WaitForWriteCompletion();
			Console.WriteLine( "AutoSave: prior disk write complete." );

			try{ Backup(); }
			catch ( Exception e ) { Console.WriteLine("WARNING: Automatic backup FAILED: {0}", e); }

			Console.WriteLine( "AutoSave: starting world save..." );
			World.Save( true, permitBackgroundWrite );
		}

		private const int TodayBackupRetention = 6;
		private const int PreviousDayBackupRetention = 3;

		private static void Backup()
		{
			Stopwatch watch = Stopwatch.StartNew();
			Console.WriteLine( "Backup: starting..." );

			string root = Path.Combine( Core.BaseDirectory, "Backups/Automatic" );

			if ( !Directory.Exists( root ) )
				Directory.CreateDirectory( root );

			string saves = Path.Combine( Core.BaseDirectory, "Saves" );

			if ( Directory.Exists( saves ) )
			{
				if( File.Exists( "Saves/Data/colors.set" ) )
					File.Delete( "Saves/Data/colors.set" );

				string time = GetTimeStamp();

				string rootBackup = FormatDirectory( root, "Backup", time );
				string rootOrigin = saves;

				Console.WriteLine( "Backup: creating directories under {0}", rootBackup );

				// Create new directories
				CreateDirectory( rootBackup );
				CreateDirectory( rootBackup, "Accounts/" );
				CreateDirectory( rootBackup, "Items/" );
				CreateDirectory( rootBackup, "Mobiles/" );
				CreateDirectory( rootBackup, "Guilds/" );
				CreateDirectory( rootBackup, "Data/" );
				CreateDirectory( rootBackup, "ChatBeta8/" );
				CreateDirectory( rootBackup, "Achievements/" );
				CreateDirectory( rootBackup, "Craft/" );
				CreateDirectory( rootBackup, "Player/" );

				Console.WriteLine( "Backup: copying save files..." );

				// Copy files
				CopyFile( rootOrigin, rootBackup, "Accounts/Accounts.xml" );

				CopyFile( rootOrigin, rootBackup, "Items/Items.bin" );
				CopyFile( rootOrigin, rootBackup, "Items/Items.idx" );
				CopyFile( rootOrigin, rootBackup, "Items/Items.tdb" );

				CopyFile( rootOrigin, rootBackup, "Mobiles/Mobiles.bin" );
				CopyFile( rootOrigin, rootBackup, "Mobiles/Mobiles.idx" );
				CopyFile( rootOrigin, rootBackup, "Mobiles/Mobiles.tdb" );

				CopyFile( rootOrigin, rootBackup, "Guilds/Guilds.bin" );
				CopyFile( rootOrigin, rootBackup, "Guilds/Guilds.idx" );

				CopyFile( rootOrigin, rootBackup, "Data/adventures.txt" );
				CopyFile( rootOrigin, rootBackup, "Data/battles.txt" );
				CopyFile( rootOrigin, rootBackup, "Data/deaths.txt" );
				CopyFile( rootOrigin, rootBackup, "Data/journies.txt" );
				CopyFile( rootOrigin, rootBackup, "Data/murderers.txt" );
				CopyFile( rootOrigin, rootBackup, "Data/online.txt" );
				CopyFile( rootOrigin, rootBackup, "Data/quests.txt" );
				CopyFile( rootOrigin, rootBackup, "Data/server.txt" );

				CopyFile( rootOrigin, rootBackup, "ChatBeta8/Channels.bin" );
				CopyFile( rootOrigin, rootBackup, "ChatBeta8/Friends.bin" );
				CopyFile( rootOrigin, rootBackup, "ChatBeta8/GlobalListens.bin" );
				CopyFile( rootOrigin, rootBackup, "ChatBeta8/GlobalOptions.bin" );
				CopyFile( rootOrigin, rootBackup, "ChatBeta8/Gumps.bin" );
				CopyFile( rootOrigin, rootBackup, "ChatBeta8/Ignores.bin" );
				CopyFile( rootOrigin, rootBackup, "ChatBeta8/PlayerOptions.bin" );
				CopyFile( rootOrigin, rootBackup, "ChatBeta8/Pms.bin" );

				CopyFile( rootOrigin, rootBackup, "Achievements/Achievements.bin" );

				CopyFile( rootOrigin, rootBackup, "Craft/Shoppes.bin" );

				CopyFile( rootOrigin, rootBackup, "Player/Temptations.bin" );
				CopyFile( rootOrigin, rootBackup, "Player/Avatar.bin" );

				Console.WriteLine( "Backup: file copy finished in {0:F2} seconds.", watch.Elapsed.TotalSeconds );
			}
			else
			{
				Console.WriteLine( "Backup: Saves directory missing, skipping file copy." );
			}

			ApplyBackupRetention( root );

			Console.WriteLine( "Backup: running Cleanup.RemoveScripts..." );
			Server.Misc.Cleanup.RemoveScripts();

			watch.Stop();
			Console.WriteLine( "Backup: done in {0:F2} seconds.", watch.Elapsed.TotalSeconds );
		}

		private static string Combine( string path1, string path2 )
		{
			if ( path1.Length == 0 )
				return path2;

			return Path.Combine( path1, path2 );
		}

		private static void CreateDirectory( string path )
		{
			if ( !Directory.Exists( path ) )
				Directory.CreateDirectory( path );
		}

		private static void CreateDirectory( string path1, string path2 )
		{
			CreateDirectory( Combine( path1, path2 ) );
		}

		private static void CopyFile( string rootOrigin, string rootBackup, string path )
		{
			string originPath = Combine( rootOrigin, path );
			string backupPath = Combine( rootBackup, path );

			try
			{
				if ( !File.Exists( originPath ) )
					return;

				FileInfo info = new FileInfo( originPath );
				Console.WriteLine( "Backup: copying {0} ({1:N0} bytes)...", path, info.Length );

				Stopwatch copyWatch = Stopwatch.StartNew();
				File.Copy( originPath, backupPath );
				copyWatch.Stop();

				Console.WriteLine( "Backup: copied {0} in {1:F2} seconds.", path, copyWatch.Elapsed.TotalSeconds );
			}
			catch ( Exception e )
			{
				Console.WriteLine( "Backup: FAILED copying {0}: {1}", path, e.Message );
			}
		}

		private static string FormatDirectory( string root, string name, string timeStamp )
		{
			return Path.Combine( root, String.Format( "{0} ({1})", name, timeStamp ) );
		}

		private static string FindTimeStamp( string input )
		{
			int start = input.IndexOf( '(' );

			if ( start >= 0 )
			{
				int end = input.IndexOf( ')', ++start );

				if ( end >= start )
					return input.Substring( start, end-start );
			}

			return null;
		}

		private static string GetTimeStamp()
		{
			DateTime now = DateTime.Now;

			return String.Format( "{0}-{1}-{2} {3}-{4:D2}-{5:D2}",
					now.Day,
					now.Month,
					now.Year,
					now.Hour,
					now.Minute,
					now.Second
				);
		}

		private static void ApplyBackupRetention( string root )
		{
			Stopwatch watch = Stopwatch.StartNew();
			Console.WriteLine( "Backup: retention scanning {0}...", root );

			List<KeyValuePair<DirectoryInfo, DateTime>> backups = GetDatedBackups( root );

			Console.WriteLine( "Backup: retention found {0} dated backup(s).", backups.Count );

			if ( backups.Count == 0 )
				return;

			DateTime today = DateTime.Today;
			DateTime previousDay = today.AddDays( -1 );

			int deleted = 0;
			deleted += TrimBackupGroup( backups, today, TodayBackupRetention );
			deleted += TrimBackupGroup( backups, previousDay, PreviousDayBackupRetention );
			deleted += RemoveBackupsOutsideDates( backups, today, previousDay );

			watch.Stop();
			Console.WriteLine( "Backup: retention removed {0} directories in {1:F2} seconds.", deleted, watch.Elapsed.TotalSeconds );
		}

		private static List<KeyValuePair<DirectoryInfo, DateTime>> GetDatedBackups( string root )
		{
			List<KeyValuePair<DirectoryInfo, DateTime>> results = new List<KeyValuePair<DirectoryInfo, DateTime>>();
			string[] directories = Directory.GetDirectories( root );

			for ( int i = 0; i < directories.Length; ++i )
			{
				DirectoryInfo dir = new DirectoryInfo( directories[i] );
				DateTime backupTime;

				if ( TryGetBackupTime( dir, out backupTime ) )
					results.Add( new KeyValuePair<DirectoryInfo, DateTime>( dir, backupTime ) );
			}

			return results;
		}

		private static bool TryGetBackupTime( DirectoryInfo dir, out DateTime backupTime )
		{
			string stamp = FindTimeStamp( dir.Name );

			if ( stamp != null &&
				DateTime.TryParseExact( stamp, "d-M-yyyy H-mm-ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out backupTime ) )
			{
				return true;
			}

			backupTime = DateTime.MinValue;
			return false;
		}

		private static int TrimBackupGroup( List<KeyValuePair<DirectoryInfo, DateTime>> backups, DateTime targetDate, int keepCount )
		{
			List<KeyValuePair<DirectoryInfo, DateTime>> matching = backups.FindAll(
				delegate( KeyValuePair<DirectoryInfo, DateTime> entry )
				{
					return entry.Value.Date == targetDate.Date;
				} );

			matching.Sort(
				delegate( KeyValuePair<DirectoryInfo, DateTime> a, KeyValuePair<DirectoryInfo, DateTime> b )
				{
					return b.Value.CompareTo( a.Value );
				} );

			int deleted = 0;

			for ( int i = keepCount; i < matching.Count; ++i )
			{
				if ( DeleteDirectorySafe( matching[i].Key ) )
					++deleted;
			}

			return deleted;
		}

		private static int RemoveBackupsOutsideDates( List<KeyValuePair<DirectoryInfo, DateTime>> backups, DateTime today, DateTime previousDay )
		{
			int deleted = 0;

			for ( int i = 0; i < backups.Count; ++i )
			{
				DateTime date = backups[i].Value.Date;

				if ( date != today.Date && date != previousDay.Date )
				{
					if ( DeleteDirectorySafe( backups[i].Key ) )
						++deleted;
				}
			}

			return deleted;
		}

		private static bool DeleteDirectorySafe( DirectoryInfo dir )
		{
			try
			{
				if ( !dir.Exists )
					return false;

				Console.WriteLine( "Backup: retention deleting {0}...", dir.Name );
				Stopwatch watch = Stopwatch.StartNew();
				dir.Delete( true );
				watch.Stop();
				Console.WriteLine( "Backup: retention deleted {0} in {1:F2} seconds.", dir.Name, watch.Elapsed.TotalSeconds );
				return true;
			}
			catch ( Exception e )
			{
				Console.WriteLine( "Backup: retention FAILED deleting {0}: {1}", dir.Name, e.Message );
				return false;
			}
		}
	}
}
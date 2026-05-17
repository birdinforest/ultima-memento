using System;
using System.IO;
using System.Text;
using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Gumps
{
	/// <summary>
	/// Thread-safe disk logger for the Character Item Restore system.
	/// Each spawn session creates one log file under
	/// <c>World/Logs/CharacterRestore/restore-YYYYMMDD-HHmmss-GMName.log</c>.
	///
	/// The NPC stores the log file path so delivery events can be appended to the
	/// same file even after a server restart.
	/// </summary>
	public static class CharRestoreLogger
	{
		private static readonly object s_Lock = new object();

		// ----------------------------------------------------------------
		// Session lifecycle
		// ----------------------------------------------------------------

		/// <summary>
		/// Opens (or creates) a log file for this restore session.
		/// Returns the absolute file path to be stored in the spawned NPC.
		/// Returns an empty string if the log directory cannot be created.
		/// </summary>
		public static string BeginSession(
			Mobile gm, string backupPath,
			string accountName, string charName,
			string targetPlayer, int selectedCount )
		{
			string path = MakeLogPath( gm?.Name );
			if ( string.IsNullOrEmpty( path ) )
				return "";

			var sb = new StringBuilder();
			sb.AppendLine( Hdr( "Character Restore Session Started" ) );
			sb.AppendLine( Line( $"GM         : {gm?.Name ?? "(unknown)"} (Serial: 0x{gm?.Serial.Value:X8})" ) );
			sb.AppendLine( Line( $"Backup path: {backupPath}" ) );
			sb.AppendLine( Line( $"Account    : {accountName} | Character: {charName}" ) );
			sb.AppendLine( Line( $"Target     : {targetPlayer}" ) );
			sb.AppendLine( Line( $"Selected   : {selectedCount} item(s)" ) );
			sb.AppendLine();

			SafeWrite( path, sb.ToString() );
			return path;
		}

		// ----------------------------------------------------------------
		// Item creation events
		// ----------------------------------------------------------------

		public static void LogItemCreate( string logPath, BackupItemInfo info, Item created )
		{
			if ( created == null || created.Deleted )
				return;

			string line = Line(
				$"CREATE OK   | {PadR( info.TypeShort, 38 )} | " +
				$"Hue: {PadL( created.Hue.ToString(), 5 )} | " +
				$"Amt: {PadL( created.Amount.ToString(), 5 )} | " +
				$"Serial: 0x{created.Serial.Value:X8} | " +
				$"Name: {( string.IsNullOrEmpty( created.Name ) ? "(default)" : created.Name )}" );

			SafeWrite( logPath, line + "\n" );
		}

		public static void LogItemFail( string logPath, BackupItemInfo info, string reason )
		{
			string line = Line(
				$"CREATE FAIL | {PadR( info.TypeShort ?? "(null)", 38 )} | " +
				$"TypeFull: {info.TypeFull} | Reason: {reason}" );

			SafeWrite( logPath, line + "\n" );
		}

		// ----------------------------------------------------------------
		// Session summary (after all items created, before NPC placement)
		// ----------------------------------------------------------------

		public static void LogSessionSummary(
			string logPath,
			int created, int failed,
			Item bag, LostItemsRestorerNPC npc )
		{
			var sb = new StringBuilder();
			sb.AppendLine();
			sb.AppendLine( Hdr( "Session Summary" ) );
			sb.AppendLine( Line( $"Items created : {created} / {created + failed}  (failed: {failed})" ) );

			if ( bag != null && !bag.Deleted )
				sb.AppendLine( Line(
					$"Restore bag   : 0x{bag.Serial.Value:X8} | Name: {bag.Name}" ) );

			if ( npc != null && !npc.Deleted )
			{
				sb.AppendLine( Line(
					$"NPC spawned   : \"{npc.Name} {npc.Title}\" " +
					$"(Serial: 0x{npc.Serial.Value:X8})" ) );
				sb.AppendLine( Line(
					$"NPC location  : ({npc.X}, {npc.Y}, {npc.Z}) | Map: {npc.Map}" ) );
				sb.AppendLine( Line(
					$"Awaiting      : {( string.IsNullOrEmpty( npc.TargetName ) ? "(any player)" : npc.TargetName )}" ) );
				sb.AppendLine( Line(
					$"Auto-delete   : 24 h from now" ) );
			}

			sb.AppendLine();
			SafeWrite( logPath, sb.ToString() );
		}

		// ----------------------------------------------------------------
		// Delivery events (appended when player claims items)
		// ----------------------------------------------------------------

		public static void LogDeliveryBegin( string logPath, Mobile npc, Mobile target )
		{
			var sb = new StringBuilder();
			sb.AppendLine( Hdr( "Item Delivery Event" ) );
			sb.AppendLine( Line(
				$"NPC    : \"{npc?.Name}\" (Serial: 0x{npc?.Serial.Value:X8})" ) );
			sb.AppendLine( Line(
				$"Player : {target?.Name} (Serial: 0x{target?.Serial.Value:X8})" ) );
			sb.AppendLine( Line(
				$"Location: ({target?.X}, {target?.Y}, {target?.Z}) | Map: {target?.Map}" ) );

			SafeWrite( logPath, sb.ToString() );
		}

		public static void LogDeliveredItem( string logPath, Item item )
		{
			if ( item == null )
				return;

			string line = Line(
				$"  ITEM: {PadR( item.GetType().Name, 38 )} | " +
				$"Serial: 0x{item.Serial.Value:X8} | " +
				$"Hue: {PadL( item.Hue.ToString(), 5 )} | " +
				$"Amt: {PadL( item.Amount.ToString(), 5 )} | " +
				$"Name: {( string.IsNullOrEmpty( item.Name ) ? "(default)" : item.Name )}" );

			SafeWrite( logPath, line + "\n" );
		}

		public static void LogDeliveryEnd( string logPath, int itemCount, Mobile target )
		{
			var sb = new StringBuilder();
			sb.AppendLine( Line( $"Delivery bag : {itemCount} item(s) given to '{target?.Name}'." ) );
			sb.AppendLine( Line( "NPC will self-delete shortly." ) );
			sb.AppendLine();
			SafeWrite( logPath, sb.ToString() );
		}

		// ----------------------------------------------------------------
		// Error logging
		// ----------------------------------------------------------------

		public static void LogError( string logPath, string context, Exception ex )
		{
			if ( string.IsNullOrEmpty( logPath ) )
			{
				Console.WriteLine( $"[CharRestore] ERROR [{context}]: {ex?.Message}" );
				return;
			}

			string msg = ex != null
				? $"{ex.GetType().Name}: {ex.Message}"
				: "(unknown)";

			SafeWrite( logPath, Line( $"ERROR [{context}] {msg}" ) + "\n" );
		}

		// ----------------------------------------------------------------
		// Internal helpers
		// ----------------------------------------------------------------

		private static string MakeLogPath( string gmName )
		{
			try
			{
				string dir = Path.Combine( Core.BaseDirectory, "Logs", "CharacterRestore" );
				Directory.CreateDirectory( dir );
				string filename = $"restore-{DateTime.Now:yyyyMMdd-HHmmss}-{Sanitize( gmName )}.log";
				return Path.Combine( dir, filename );
			}
			catch ( Exception ex )
			{
				Console.WriteLine( $"[CharRestoreLogger] Could not create log directory: {ex.Message}" );
				return "";
			}
		}

		private static void SafeWrite( string path, string content )
		{
			if ( string.IsNullOrEmpty( path ) || string.IsNullOrEmpty( content ) )
				return;
			try
			{
				lock ( s_Lock )
				{
					using ( var sw = new StreamWriter( path, true, Encoding.UTF8 ) )
						sw.Write( content );
				}
			}
			catch ( Exception ex )
			{
				Console.WriteLine( $"[CharRestoreLogger] Write failed: {ex.Message}" );
			}
		}

		private static string Hdr( string title )
			=> $"[{Ts()}] ===== {title} =====";

		private static string Line( string text )
			=> $"[{Ts()}] {text}";

		private static string Ts()
			=> DateTime.Now.ToString( "yyyy-MM-dd HH:mm:ss" );

		private static string Sanitize( string name )
		{
			if ( string.IsNullOrEmpty( name ) )
				return "unknown";
			var sb = new StringBuilder();
			foreach ( char c in name )
				if ( char.IsLetterOrDigit( c ) || c == '_' || c == '-' )
					sb.Append( c );
			return sb.Length > 0 ? sb.ToString() : "unknown";
		}

		private static string PadR( string s, int width )
			=> ( s ?? "" ).PadRight( width );

		private static string PadL( string s, int width )
			=> ( s ?? "" ).PadLeft( width );
	}
}

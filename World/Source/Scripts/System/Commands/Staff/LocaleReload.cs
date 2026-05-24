using System;
using Server;
using Server.Commands;
using Server.Localization;

namespace Server.Commands
{
	/// <summary>
	/// Staff command: [locreload — re-read locale JSON from disk without restarting the shard.
	/// Usage (in-game as Administrator): [locreload
	/// Optional: [locreload opl — also flush cached bilingual item tooltips (<see cref="Server.Items.Item.IsContentLocalized"/>).
	/// </summary>
	public class LocaleReload
	{
		public static void Initialize()
		{
			CommandSystem.Register( "locreload", AccessLevel.Administrator, new CommandEventHandler( OnCommand ) );
			CommandSystem.Register( "lr", AccessLevel.Administrator, new CommandEventHandler( OnCommand ) );
		}

		private static void OnCommand( CommandEventArgs e )
		{
			Mobile from = e.Mobile;
			bool invalidateOpl = e.Length >= 1 && Insensitive.Equals( e.GetString( 0 ), "opl" );

			int oplCount = LocalizationBootstrap.Reload( invalidateOpl );

			from.SendMessage( 0x5A, "Localization JSON reloaded from Data/Localization/." );

			if ( invalidateOpl )
				from.SendMessage( 0x5A, "Invalidated bilingual OPL cache on {0} items.", oplCount );
			else
				from.SendMessage( 0x5A, "Item tooltips may stay cached until re-examined; use [locreload opl to flush." );

			Console.WriteLine( "Localization: [locreload{0} by {1}.", invalidateOpl ? " opl" : "", from );
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using Server;
using Server.Commands;
using Server.RateConfig;

namespace Server.Commands
{
	/// <summary>
	/// Staff commands for the generic <see cref="RateConfigEngine"/> (Data/RateConfig/*.json).
	/// Usage (in-game):
	///   [ratereload                      — Administrator: re-read all Data/RateConfig/*.json from disk.
	///   [ratelist <prefix>                — GameMaster: list every configured key under a dotted prefix
	///                                        (e.g. "[ratelist dragon.breedWeight" lists all 26 Bright
	///                                        breed keep-chances plus the default).
	///   [rateget <key>                    — GameMaster: print the effective value of a single key.
	/// </summary>
	public class RateConfigCommands
	{
		public static void Initialize()
		{
			CommandSystem.Register( "ratereload", AccessLevel.Administrator, new CommandEventHandler( OnReload ) );
			CommandSystem.Register( "ratelist", AccessLevel.GameMaster, new CommandEventHandler( OnList ) );
			CommandSystem.Register( "rateget", AccessLevel.GameMaster, new CommandEventHandler( OnGet ) );
		}

		private static void OnReload( CommandEventArgs e )
		{
			Mobile from = e.Mobile;

			RateConfigEngine.Reload();
			InscriptionRecipeDropConfig.Reload();

			from.SendMessage( 0x5A, "RateConfig JSON reloaded from Data/RateConfig/ (includes inscription-recipe-drop.json). Inscription tier scroll lists reloaded from Data/InscriptionRecipeDrop/." );
			Console.WriteLine( "RateConfig: [ratereload by {0}.", from );
		}

		private static void OnList( CommandEventArgs e )
		{
			Mobile from = e.Mobile;

			if ( e.Length < 1 )
			{
				from.SendMessage( "Usage: [ratelist <prefix>  (e.g. [ratelist dragon.breedWeight)" );
				return;
			}

			string prefix = e.GetString( 0 );
			Dictionary<string, double> table = RateConfigEngine.GetTable( prefix );

			if ( table.Count == 0 )
			{
				from.SendMessage( "No entries found under \"{0}.*\".", prefix );
				return;
			}

			from.SendMessage( 0x5A, "RateConfig entries under \"{0}.*\" ({1}):", prefix, table.Count );

			foreach ( string name in table.Keys.OrderBy( k => k, StringComparer.Ordinal ) )
				from.SendMessage( "  {0}.{1} = {2}", prefix, name, table[name] );
		}

		private static void OnGet( CommandEventArgs e )
		{
			Mobile from = e.Mobile;

			if ( e.Length < 1 )
			{
				from.SendMessage( "Usage: [rateget <key>  (e.g. [rateget dragon.breedWeight.xormite)" );
				return;
			}

			string key = e.GetString( 0 );
			double value = RateConfigEngine.GetDouble( key, double.NaN );

			if ( double.IsNaN( value ) )
				from.SendMessage( "No value configured for \"{0}\" (no default was supplied).", key );
			else
				from.SendMessage( "{0} = {1}", key, value );
		}
	}
}

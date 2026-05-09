/***************************************************************************
 * Optional .env loader — runs before scripts compile so Environment.* is
 * visible to script static initializers (e.g. analytics salt).
 ***************************************************************************/

using System;
using System.IO;
using System.Text;

namespace Server
{
	public static class DotEnvLoader
	{
		/// <summary>
		/// Loads <c>.env</c> from <see cref="Core.BaseDirectory"/> if the file exists.
		/// Does not override variables already set in the process environment.
		/// Lines: <c>KEY=value</c>, optional <c>export </c> prefix, <c>#</c> comments, blank lines ignored.
		/// </summary>
		public static void LoadOptional()
		{
			try
			{
				string baseDir = Core.BaseDirectory;
				if ( string.IsNullOrEmpty( baseDir ) )
					return;

				string path = Path.Combine( baseDir, ".env" );
				if ( !File.Exists( path ) )
					return;

				foreach ( string rawLine in File.ReadAllLines( path, Encoding.UTF8 ) )
				{
					string line = rawLine.Trim();
					if ( line.Length == 0 || line[0] == '#' )
						continue;

					if ( line.StartsWith( "export ", StringComparison.OrdinalIgnoreCase ) )
						line = line.Substring( 7 ).TrimStart();

					int eq = line.IndexOf( '=' );
					if ( eq <= 0 )
						continue;

					string key = line.Substring( 0, eq ).Trim();
					if ( key.Length == 0 )
						continue;

					string value = line.Substring( eq + 1 ).Trim();
					if ( value.Length >= 2 )
					{
						if ( ( value[0] == '"' && value[value.Length - 1] == '"' ) ||
						     ( value[0] == '\'' && value[value.Length - 1] == '\'' ) )
							value = value.Substring( 1, value.Length - 2 );
					}

					if ( Environment.GetEnvironmentVariable( key ) != null )
						continue;

					Environment.SetEnvironmentVariable( key, value );
				}
			}
			catch
			{
			}
		}
	}
}

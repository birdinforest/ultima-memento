using System;
using System.IO;

namespace Server
{
	public class CliLocTable
	{
		private static string[] m_Table;

		public static string Lookup( int cliloc )
		{
			if ( m_Table == null )
				Load();

			string val = null;

			if ( cliloc >= 0 && cliloc < m_Table.Length )
				val = m_Table[cliloc];

			return val;
		}

		/// <summary>
		/// English cliloc text from the pre-translation backup table. Use for
		/// <see cref="Server.Localization.StringCatalog"/> keys — not player display via
		/// <see cref="CliLocTable.Lookup"/>, which reads the shard's Traditional Chinese cliloc.cfg.
		/// </summary>
		public static string LookupEnglish( int cliloc )
		{
			return CliLocEnglishTable.Lookup( cliloc );
		}

		private static void Load()
		{
			string path = Path.Combine( Core.BaseDirectory, "Data/System/CFG/cliloc.cfg" );

			if ( !File.Exists( path ) )
			{
				m_Table = new string[0];
				return;
			}

			m_Table = new string[3011035];

			using ( StreamReader ip = new StreamReader( path ) )
			{
				string line;

				while ( (line = ip.ReadLine()) != null )
				{
					line = line.Trim();

					if ( line.Length == 0 || line.StartsWith( "#" ) )
						continue;

					try
					{
						string[] split = line.Split( '\t' );

						if ( split.Length >= 2 )
						{
							int valu = Utility.ToInt32( split[0] );
							string text = split[1];

							if ( valu >= 0 && valu < m_Table.Length )
								m_Table[valu] = text;
						}
					}
					catch
					{
					}
				}
			}
		}
	}

	public static class CliLocEnglishTable
	{
		private static string[] m_Table;

		public static string Lookup( int cliloc )
		{
			if ( m_Table == null )
				Load();

			string val = null;

			if ( cliloc >= 0 && cliloc < m_Table.Length )
				val = m_Table[cliloc];

			return val;
		}

		private static void Load()
		{
			string path = Path.Combine( Core.BaseDirectory, "Data/System/CFG/cliloc-backup.cfg" );

			if ( !File.Exists( path ) )
			{
				m_Table = new string[0];
				return;
			}

			m_Table = new string[3011035];

			using ( StreamReader ip = new StreamReader( path ) )
			{
				string line;

				while ( (line = ip.ReadLine()) != null )
				{
					line = line.Trim();

					if ( line.Length == 0 || line.StartsWith( "#" ) )
						continue;

					try
					{
						string[] split = line.Split( '\t' );

						if ( split.Length >= 2 )
						{
							int valu = Utility.ToInt32( split[0] );
							string text = split[1];

							if ( valu >= 0 && valu < m_Table.Length )
								m_Table[valu] = text;
						}
					}
					catch
					{
					}
				}
			}
		}
	}
}
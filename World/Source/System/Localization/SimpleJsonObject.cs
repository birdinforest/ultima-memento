using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Localization
{
	internal static class SimpleJsonObject
	{
		public static void ParseStringProperties( string json, Dictionary<string, string> dict )
		{
			if ( json == null || dict == null )
				return;

			int i = 0;
			SkipWs( json, ref i );

			if ( i >= json.Length || json[i] != '{' )
				return;

			++i;

			while ( true )
			{
				SkipWs( json, ref i );

				if ( i >= json.Length )
					return;

				if ( json[i] == '}' )
					return;

				string key = ReadJsonString( json, ref i );

				SkipWs( json, ref i );

				if ( i >= json.Length || json[i] != ':' )
					return;

				++i;
				SkipWs( json, ref i );

				string val = ReadJsonString( json, ref i );

				if ( key != null )
					dict[key] = val;

				SkipWs( json, ref i );

				if ( i < json.Length && json[i] == ',' )
				{
					++i;
					continue;
				}

				if ( i < json.Length && json[i] == '}' )
					return;

				return;
			}
		}

		private static void SkipWs( string s, ref int i )
		{
			while ( i < s.Length && char.IsWhiteSpace( s[i] ) )
				++i;
		}

		private static string ReadJsonString( string s, ref int i )
		{
			if ( i >= s.Length || s[i] != '"' )
				return null;

			++i;
			StringBuilder sb = new StringBuilder();

			while ( i < s.Length )
			{
				char c = s[i];

				if ( c == '"' )
				{
					++i;
					return sb.ToString();
				}

				if ( c == '\\' )
				{
					++i;

					if ( i >= s.Length )
						break;

					char esc = s[i++];

					switch ( esc )
					{
						case '"': sb.Append( '"' ); break;
						case '\\': sb.Append( '\\' ); break;
						case '/': sb.Append( '/' ); break;
						case 'b': sb.Append( '\b' ); break;
						case 'f': sb.Append( '\f' ); break;
						case 'n': sb.Append( '\n' ); break;
						case 'r': sb.Append( '\r' ); break;
						case 't': sb.Append( '\t' ); break;
						case 'u':
						{
							if ( i + 4 > s.Length )
								break;

							int code = 0;

							for ( int k = 0; k < 4; ++k )
							{
								int h = HexDigit( s[i++] );

								if ( h < 0 )
									return sb.ToString();

								code = ( code << 4 ) | h;
							}

							sb.Append( (char)code );
							break;
						}
						default: sb.Append( esc ); break;
					}

					continue;
				}

				sb.Append( c );
				++i;
			}

			return sb.ToString();
		}

		private static int HexDigit( char c )
		{
			if ( c >= '0' && c <= '9' )
				return c - '0';

			if ( c >= 'a' && c <= 'f' )
				return 10 + ( c - 'a' );

			if ( c >= 'A' && c <= 'F' )
				return 10 + ( c - 'A' );

			return -1;
		}
	}
}

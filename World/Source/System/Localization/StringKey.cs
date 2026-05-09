using System;
using System.Security.Cryptography;
using System.Text;

namespace Server.Localization
{
	public static class StringKey
	{
		public static string ForEnglish( string english )
		{
			if ( english == null || english.Length == 0 )
				return "empty";

			using ( SHA256 sha = SHA256.Create() )
			{
				byte[] hash = sha.ComputeHash( Encoding.UTF8.GetBytes( english ) );
				StringBuilder sb = new StringBuilder( 18 );

				sb.Append( "s." );

				for ( int i = 0; i < 8; ++i )
					sb.Append( hash[i].ToString( "x2" ) );

				return sb.ToString();
			}
		}
	}
}

using System;

namespace Server.Gumps
{
	/// <summary>Adventure persona for <see cref="Server.Mobiles.LostItemsRestorerNPC"/> copy.</summary>
	public enum CharRestoreTheme
	{
		Wilderness = 0,
		Ocean = 1,
		Dungeon = 2
	}

	public static class CharRestoreThemes
	{
		public static CharRestoreTheme Parse( int value )
		{
			if ( value < 0 || value > 2 )
				return CharRestoreTheme.Ocean;

			return (CharRestoreTheme)value;
		}

		public static string Id( CharRestoreTheme theme )
		{
			switch ( theme )
			{
				case CharRestoreTheme.Wilderness: return "wilderness";
				case CharRestoreTheme.Dungeon:    return "dungeon";
				default:                          return "ocean";
			}
		}

		public static string ThemeKey( CharRestoreTheme theme, string suffix )
		{
			return "charrestore.theme." + Id( theme ) + "." + suffix;
		}
	}
}

using System;
using Server.Localization;

namespace Server.Items
{
	/// <summary>
	/// Localized feedback for throwing gloves / throwing ammo stacks (hue 68 success tone).
	/// </summary>
	internal static class ThrowingEquipmentMessages
	{
		public static string GloveKindLabel( Mobile m, string gloveTypeCode )
		{
			string key = "god.throwing.glovekind." + gloveTypeCode;
			string lang = AccountLang.GetLanguageCode( m == null ? null : m.Account );
			string s = StringCatalog.TryResolveByKey( lang, key );
			return string.IsNullOrEmpty( s ) ? gloveTypeCode : s;
		}

		public static string AmmoKindLabel( Mobile m, string ammo )
		{
			if ( string.IsNullOrEmpty( ammo ) )
				return ammo;
			string key = "god.throwing.ammokind." + ammo.Replace( " ", "" );
			string lang = AccountLang.GetLanguageCode( m == null ? null : m.Account );
			string s = StringCatalog.TryResolveByKey( lang, key );
			return string.IsNullOrEmpty( s ) ? ammo : s;
		}

		public static void SendGloveTypeChanged( Mobile m, string gloveTypeCode )
		{
			string sub = GloveKindLabel( m, gloveTypeCode );
			m.SendMessage( 68, StringCatalog.ResolveFormatByKey( m.Account, "god.throwing.gloves.type.changed", sub ) );
		}

		public static void SendAmmoTypeChanged( Mobile m, string ammo )
		{
			string sub = AmmoKindLabel( m, ammo );
			m.SendMessage( 68, StringCatalog.ResolveFormatByKey( m.Account, "god.throwing.ammo.type.changed", sub ) );
		}
	}
}

using Server.Accounting;
using Server.Localization;

namespace Server.Engines.Apiculture
{
	public static class ApicultureLocale
	{
		public static string HealthShotkey( HiveHealth health )
		{
			switch ( health )
			{
				case HiveHealth.Dying: return "prop.apiculture.health.dying";
				case HiveHealth.Sickly: return "prop.apiculture.health.sickly";
				case HiveHealth.Healthy: return "prop.apiculture.health.healthy";
				default: return "prop.apiculture.health.thriving";
			}
		}

		public static string ResolveHealth( IAccount account, HiveHealth health )
		{
			return StringCatalog.ResolveByKey( account, HealthShotkey( health ) );
		}

		public static string Msg( IAccount account, string shotkey )
		{
			return StringCatalog.ResolveByKey( account, shotkey );
		}

		public static string FormatMsg( IAccount account, string shotkey, params object[] args )
		{
			return StringCatalog.ResolveFormatByKey( account, shotkey, args );
		}
	}
}

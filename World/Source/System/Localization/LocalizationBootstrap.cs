using Server;

namespace Server.Localization
{
	public static class LocalizationBootstrap
	{
		[CallPriority( -200 )]
		public static void Configure()
		{
			LangConfig.Configure();
		}

		[CallPriority( -100 )]
		public static void Initialize()
		{
			StringCatalog.Load();
			LangCommands.Initialize();
		}
	}
}

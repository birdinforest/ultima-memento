using System;
using Server;
using Server.Items;

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
		}

		/// <summary>
		/// Re-reads merged locale JSON (<c>Data/Localization/en/</c>, <c>zh-Hans/</c>) and quest-composite tables.
		/// Does not reload <see cref="LangConfig"/> (localization.cfg). Optional OPL flush for <see cref="Item.IsContentLocalized"/> items.
		/// </summary>
		public static int Reload( bool invalidateLocalizedItemTooltips = false )
		{
			StringCatalog.Reload();
			QuestCompositeResolver.Reload();

			if ( !invalidateLocalizedItemTooltips )
				return 0;

			int count = 0;

			foreach ( Item item in World.Items.Values )
			{
				if ( item != null && item.IsContentLocalized )
				{
					item.InvalidateProperties();
					++count;
				}
			}

			if ( count > 0 )
				Console.WriteLine( "Localization: invalidated OPL cache on {0} localized items.", count );

			return count;
		}
	}
}

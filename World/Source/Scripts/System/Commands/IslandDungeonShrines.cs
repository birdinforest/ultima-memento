using Server.Localization;

namespace Server.Commands
{
	public static class IslandDungeonShrines
	{
		private const string DecorationFolder = "Data/Decoration";
		private const string DecorationTag = "IslandDungeonShrine";

		public static void Initialize()
		{
			CommandSystem.Register( "IslandShrines", AccessLevel.Administrator, new CommandEventHandler( OnCommand ) );
		}

		[Usage( "IslandShrines" )]
		[Description( "Replaces only the tagged island-dungeon resurrection shrines from the normal decoration files." )]
		private static void OnCommand( CommandEventArgs e )
		{
			Mobile from = e.Mobile;

			from.SendMessage( StringCatalog.ResolveByKey( from.Account, "sys.island_shrines.reloading" ) );

			int count = 0;
			count += Decorate.ReplaceTaggedFile( DecorationFolder, "Sosaria.cfg", DecorationTag, Map.Sosaria );
			count += Decorate.ReplaceTaggedFile( DecorationFolder, "SerpentIsland.cfg", DecorationTag, Map.SerpentIsland );
			count += Decorate.ReplaceTaggedFile( DecorationFolder, "IslesOfDread.cfg", DecorationTag, Map.IslesDread );
			count += Decorate.ReplaceTaggedFile( DecorationFolder, "SavagedEmpire.cfg", DecorationTag, Map.SavagedEmpire );

			from.SendMessage( StringCatalog.ResolveFormatByKey( from.Account, "sys.island_shrines.complete", count ) );
		}
	}
}

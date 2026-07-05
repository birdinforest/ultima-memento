using System;
using Server;
using Server.Engines.Avatar;
using Server.Items;
using Server.Mobiles;

namespace Server.Misc
{
	public struct ResearchSearchTarget
	{
		public string Category;
		public string RegionName;
		public Land Land;
		public string GoalLabel;
		public string SearchItem;
	}

	public static class ResearchSearchUtility
	{
		public static bool TryGetNextSearchTarget( ResearchBag bag, Mobile viewer, out ResearchSearchTarget target )
		{
			target = default( ResearchSearchTarget );

			if ( bag == null )
				return false;

			Research.EnsureBagSpellData( bag );

			if ( !Research.GetRunes( bag, 26 ) && !string.IsNullOrEmpty( bag.RuneLocation ) )
			{
				target = new ResearchSearchTarget
				{
					Category = "rune",
					RegionName = bag.RuneLocation,
					Land = bag.RuneWorld,
					GoalLabel = GetNextMissingCubeLabel( viewer, bag ),
					SearchItem = ""
				};
				return true;
			}

			if ( Research.GetRunes( bag, 26 ) && !string.IsNullOrEmpty( bag.SpellsMageLocation ) && Research.NextWizardry( bag ) != "" )
			{
				target = new ResearchSearchTarget
				{
					Category = "mage",
					RegionName = bag.SpellsMageLocation,
					Land = bag.SpellsMageWorld,
					GoalLabel = Research.NextWizardryForDisplay( viewer, bag ),
					SearchItem = bag.SpellsMageItem
				};
				return true;
			}

			if ( Research.GetRunes( bag, 26 ) && !string.IsNullOrEmpty( bag.SpellsNecroLocation ) && Research.NextNecromancy( bag ) != "" )
			{
				target = new ResearchSearchTarget
				{
					Category = "necro",
					RegionName = bag.SpellsNecroLocation,
					Land = bag.SpellsNecroWorld,
					GoalLabel = Research.NextNecromancyForDisplay( viewer, bag ),
					SearchItem = bag.SpellsNecroItem
				};
				return true;
			}

			if ( Research.GetRunes( bag, 26 ) && !string.IsNullOrEmpty( bag.BagInkLocation ) && bag.BagInk < 50000 )
			{
				target = new ResearchSearchTarget
				{
					Category = "ink",
					RegionName = bag.BagInkLocation,
					Land = bag.BagInkWorld,
					GoalLabel = "Octopus ink",
					SearchItem = ""
				};
				return true;
			}

			if ( Research.GetRunes( bag, 26 ) && !string.IsNullOrEmpty( bag.ResearchLocation ) && Research.NextResearch( bag ) != "" )
			{
				target = new ResearchSearchTarget
				{
					Category = "research",
					RegionName = bag.ResearchLocation,
					Land = bag.ResearchWorld,
					GoalLabel = Research.NextResearchForDisplay( viewer, bag ),
					SearchItem = bag.ResearchItem
				};
				return true;
			}

			return false;
		}

		public static void ReportNextSearchLocation( Mobile staff, PlayerMobile player, ResearchBag bag )
		{
			if ( staff == null || player == null )
				return;

			if ( bag == null )
			{
				staff.SendMessage( "{0} has no Research Bag.", player.Name );
				return;
			}

			if ( bag.BagOwner != player )
			{
				staff.SendMessage( "That Research Bag belongs to someone else." );
				return;
			}

			ResearchSearchTarget target;

			if ( !TryGetNextSearchTarget( bag, player, out target ) )
			{
				staff.SendMessage( "{0} has no pending research search locations.", player.Name );
				return;
			}

			ReportSearchTarget( staff, player, bag, target );
		}

		private static void ReportSearchTarget( Mobile staff, PlayerMobile player, ResearchBag bag, ResearchSearchTarget target )
		{
			string landName = ResearchLocalization.LandName( player, target.Land );
			string categoryLabel = CategoryLabel( target.Category );

			staff.SendMessage( "Next research search for {0}: [{1}] {2} in {3}", player.Name, categoryLabel, target.RegionName, landName );

			if ( !string.IsNullOrEmpty( target.GoalLabel ) )
			{
				if ( !string.IsNullOrEmpty( target.SearchItem ) )
					staff.SendMessage( "Goal: {0} — find {1}", target.GoalLabel, target.SearchItem );
				else if ( target.Category == "rune" )
					staff.SendMessage( "Goal: Cube of {0}", target.GoalLabel );
				else
					staff.SendMessage( "Goal: {0}", target.GoalLabel );
			}

			if ( bag.IsDormant )
				staff.SendMessage( "Note: Research Bag is dormant (likely in bank). Complete resonance and carry the bag before SearchBase chests will grant progress." );

			Map map;
			Point3D loc;
			SearchBase searchBase;

			if ( MemoryEchoUtility.TryResolveGoTarget( target.RegionName, out map, out loc, out searchBase ) )
			{
				staff.SendMessage( "Map: {0}", map != null ? map.Name : "?" );
				staff.SendMessage( "Stand near: {0}, {1}, {2}", loc.X, loc.Y, loc.Z );
				staff.SendMessage( "Go command: {0}", MemoryEchoUtility.FormatGoCommand( map, loc, target.RegionName ) );

				if ( searchBase != null )
				{
					Point3D chestLoc = searchBase.GetWorldLocation();
					staff.SendMessage( "Search chest at: {0}, {1}, {2} — interact within 2 tiles.", chestLoc.X, chestLoc.Y, chestLoc.Z );
				}
			}
			else
			{
				staff.SendMessage( "Warning: no SearchBase chest found in region \"{0}\". Research search may fail until a chest exists there.", target.RegionName );
			}
		}

		private static string CategoryLabel( string category )
		{
			switch ( category )
			{
				case "rune": return "Cubes of Power";
				case "mage": return "Magery research";
				case "necro": return "Necromancy research";
				case "ink": return "Octopus ink";
				case "research": return "Ancient spell research";
				default: return category;
			}
		}

		private static string GetNextMissingCubeLabel( Mobile viewer, ResearchBag bag )
		{
			for ( int r = 1; r <= 26; ++r )
			{
				if ( !Research.GetRunes( bag, r ) )
					return Research.RuneName( viewer, r, 1 );
			}

			return "";
		}
	}
}

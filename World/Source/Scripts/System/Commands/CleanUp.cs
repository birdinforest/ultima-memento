using System;
using System.Collections.Generic;
using System.Linq;
using Server.Commands;
using Server.Items;
using Server.Utilities;

namespace Server.Gumps
{
	public class CleanUpCommand
	{
		public static void Initialize()
		{
			CommandSystem.Register("Internal-CountItems", AccessLevel.Administrator, new CommandEventHandler(InternalCountItems_OnCommand));
			CommandSystem.Register("Internal-DeleteOrphans", AccessLevel.Administrator, new CommandEventHandler(InternalDeleteOrphans_OnCommand));
		}

		[Usage("Internal-DeleteOrphans")]
		[Description("Deletes all items from the Internal map that were known to have issues.")]
		public static void InternalDeleteOrphans_OnCommand(CommandEventArgs e)
		{
			var TYPES_TO_DELETE = new List<Type>
			{
				// Shooting a ranged weapon usually created 5 orphaned Ammo every successful attack
				typeof(Arrow),
				typeof(Bolt),
				typeof(ThrowingWeapon),
				typeof(HarpoonRope),
				typeof(MageEye),

				// Dress up
				typeof(FancyShirt),
				typeof(Skirt),
				typeof(ShortPants),
			};

			var countByType = new Dictionary<Type, int>();
			foreach (var type in TYPES_TO_DELETE)
			{
				var itemsToDelete = WorldUtilities.ForEachItem<Item>(item =>
				{
					return item.Map == Map.Internal
						&& item.X == 0 && item.Y == 0 && item.Z == 0
						&& item.GetType() == type;
				});

				countByType[type] = 0;

				foreach (var item in itemsToDelete)
				{
					countByType[type]++;
					item.Delete();
				}
			}

			foreach (var kvp in countByType)
			{
				if (kvp.Value == 0) continue;

				e.Mobile.SendMessage("Deleted '{0}' of type '{1}'", kvp.Value, kvp.Key.Name);
			}
		}

		[Usage("Internal-CountItems")]
		[Description("Counts all outlier items from the Internal map.")]
		public static void InternalCountItems_OnCommand(CommandEventArgs e)
		{
			var countByType = new Dictionary<Type, int>();
			World.Items.Values
				.Where(x => x.Map == Map.Internal)
				.ToList().ForEach(x =>
				{
					var typeName = x.GetType();
					if (countByType.ContainsKey(typeName)) countByType[typeName]++;
					else countByType[typeName] = 1;
				});

			const int MAX_WORRYSOME_VALUE = 50;
			Console.WriteLine("Items in the Internal map:");
			foreach (var kvp in countByType.Where(x => MAX_WORRYSOME_VALUE <= x.Value ).OrderByDescending(x => x.Value))
			{
				e.Mobile.SendMessage("{0}x {1}", kvp.Value, kvp.Key.Name);
			}
		}
	}
}
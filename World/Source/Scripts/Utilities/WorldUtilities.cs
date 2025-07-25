using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Utilities
{
	public static class WorldUtilities
	{
		public static void DeleteAllItems<T>(Func<T, bool> predicate) where T : Item
		{
			var toDelete = World.Items.Values
				.Where(item => item is T && predicate((T)item));
			if (toDelete.Any())
			{
				toDelete
					.ToList()
					.ForEach(item => item.Delete());
			}
		}

		public static IEnumerable<T> ForEachItem<T>(Func<T, bool> predicate) where T : Item
		{
			var items = World.Items.Values
				.Where(item => item is T && predicate((T)item));
			if (!items.Any()) yield break;

			foreach (var item in items.ToList())
			{
				yield return (T)item;
			}
		}
	}
}
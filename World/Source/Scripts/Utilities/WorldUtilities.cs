using System;
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
	}
}
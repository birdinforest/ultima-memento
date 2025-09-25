using System.Linq;
using Server.Commands;
using Server.Mobiles;
using Server.Utilities;

namespace Server.Scripts.Commands
{
	public class ReleaseSummons
	{
		public static void Initialize()
		{
			CommandSystem.Register("ReleaseSummons", AccessLevel.Player, new CommandEventHandler(OnReleaseSummons));
		}

		[Usage("ReleaseSummons")]
		[Description("Releases all summons that are controlled by the player.")]
		public static void OnReleaseSummons(CommandEventArgs e)
		{
			WorldUtilities
				.ForEachMobile<BaseCreature>(mobile => mobile.Summoned && MobileUtilities.TryGetMasterPlayer(mobile) == e.Mobile)
				.ToList()
				.ForEach(mob => mob.AIObject.DoOrderRelease());

			e.Mobile.SendMessage("All summons have been released.");
		}
	}
}
using Server.Commands;
using Server.Gumps;
using Server.Misc;
using Server.Mobiles;

namespace Server.Engines.Avatar
{
	public class AvatarCommand
	{
		public static void Initialize()
		{
			CommandSystem.Register("avatar-enable", AccessLevel.Player, new CommandEventHandler(EnableAvatarCommand));
			CommandSystem.Register("avatar-force-enable", AccessLevel.Player, new CommandEventHandler(ForceEnableAvatarCommand));
			CommandSystem.Register("avatar-shop", AccessLevel.Player, new CommandEventHandler(OpenAvatarShopCommand));
			CommandSystem.Register("avatar-balance", AccessLevel.Player, args =>
			{
				var from = (PlayerMobile)args.Mobile;
				if (!from.Avatar.Active) return;

				var treasury = from.Avatar.PointsSaved;
				var earned = from.Avatar.PointsFarmed;
				var total = treasury + earned;
				from.SendMessage("You have '{0:n0}' coins in your treasury.", treasury);
				from.SendMessage("You have earned '{0:n0}' coins.", earned);
				from.SendMessage("You will have a total of '{0:n0}' coins to spend in the Gypsy encampment.", total);
			});
			CommandSystem.Register("avatar-points", AccessLevel.Player, args =>
			{ // TODO: Remove this!
				var from = (PlayerMobile)args.Mobile;
				if (!from.Avatar.Active)
				{
					from.SendMessage("Normally you must be in the Gypsy encampment to open the Avatar Shop...");
					// from.SendMessage("You must be in the Gypsy encampment to open the Avatar Shop.");
					// return;
				}

				const int value = 50000;
				from.Avatar.PointsSaved += value;
				from.SendMessage("You have been awarded {0} coins.", value);
			});
		}

		[Usage("avatar-enable")]
		[Description("Enables the Avatar status for the Player.")]
		public static void EnableAvatarCommand(CommandEventArgs e)
		{
			var from = (PlayerMobile)e.Mobile;
			if (!InGypsyEncampment(from))
			{
				from.SendMessage("Normally you must be in the Gypsy encampment to open the Avatar Shop...");
				// from.SendMessage("You must be in the Gypsy encampment to open the Avatar Shop.");
				// return;
			}

			if (from.Avatar.Active)
			{
				from.SendMessage("You already have the Avatar status enabled.");
				return;
			}

			var confirmation = new ConfirmationGump(
				from,
				"Enable Avatar Status?",
				"Are you sure you wish to enable the Avatar status? This will reset your character and allow you to use the Avatar features.",
				() =>
				{
					var _ = AvatarEngine.Instance.GetOrCreateContext(from);
					from.SendMessage("You have enabled the Avatar status.");
					var newCharacter = CharacterCreation.ResetCharacter(from, false);
					AvatarEngine.InitializePlayer(newCharacter);
					AvatarEngine.Instance.ApplyContext(newCharacter, newCharacter.Avatar);
				}
			);
			from.SendGump(confirmation);
		}

		[Usage("avatar-force-enable")]
		[Description("Force enables the Avatar status for the Player.")]
		public static void ForceEnableAvatarCommand(CommandEventArgs e)
		{
			var from = (PlayerMobile)e.Mobile;

			if (from.Avatar.Active)
			{
				from.SendMessage("You already have the Avatar status enabled.");
				return;
			}

			var _ = AvatarEngine.Instance.GetOrCreateContext(from);
			from.SendMessage("You have enabled the Avatar status.");
		}

		[Usage("avatar-shop")]
		[Description("Opens the Avatar Shop for the Player.")]
		public static void OpenAvatarShopCommand(CommandEventArgs e)
		{
			var from = (PlayerMobile)e.Mobile;
			if (!InGypsyEncampment(from))
			{
				// TODO: Remove this!
				from.SendMessage("Normally you must be in the Gypsy encampment to open the Avatar Shop...");
				// from.SendMessage("You must be in the Gypsy encampment to open the Avatar Shop.");
				// return;
			}

			if (!from.Avatar.Active)
			{
				from.SendMessage("You do not have the Avatar status enabled.");
				return;
			}

			from.SendGump(new AvatarShopGump(from));
		}

		private static bool InGypsyEncampment(PlayerMobile from)
		{
			return from.Region != null && from.Region.Name == "the Forest";
		}
	}
}
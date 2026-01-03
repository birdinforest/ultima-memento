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
		}

		[Usage("avatar-enable")]
		[Description("Enables the Avatar status for the Player.")]
		public static void EnableAvatarCommand(CommandEventArgs e)
		{
			var from = (PlayerMobile)e.Mobile;
			if (!AvatarShopGump.InGypsyEncampment(from))
			{
				from.SendMessage("You must be in the Gypsy encampment to become an Avatar.");
				return;
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
			if (!AvatarShopGump.InGypsyEncampment(from))
			{
				from.SendMessage("Normally you must be in the Gypsy encampment to open the Avatar Shop...");
				// from.SendMessage("You must be in the Gypsy encampment to become an Avatar.");
				// return;
			}

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
			if (!from.Avatar.Active)
			{
				from.SendMessage("You do not have the Avatar status enabled.");
				return;
			}

			from.SendGump(new AvatarShopGump(from));
		}
	}
}
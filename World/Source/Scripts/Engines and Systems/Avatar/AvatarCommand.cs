using System;
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
			CommandSystem.Register("avatar-shop", AccessLevel.Player, new CommandEventHandler(OpenAvatarShopCommand));
		}

		[Usage("avatar-enable")]
		[Description("Enables the Avatar status for the Player.")]
		public static void EnableAvatarCommand(CommandEventArgs e)
		{
			var from = (PlayerMobile)e.Mobile;
			if (!AvatarShopGump.InGypsyEncampment(from))
			{
				AvatarLocalization.Send(from, "avatar.msg.must_be_in_gypsy_encampment", "You must be in the Gypsy encampment to become an Avatar.");
				return;
			}

			if (from.Avatar.Active)
			{
				AvatarLocalization.Send(from, "avatar.msg.already_enabled", "You already have the Avatar status enabled.");
				return;
			}

			var confirmation = new ConfirmationGump(
				from,
				AvatarLocalization.Key(from, "avatar.confirm.enable_title", "Enable Avatar Status?"),
				AvatarLocalization.Key(from, "avatar.confirm.enable_body", "Are you sure you wish to enable the Avatar status? This will reset your character and allow you to use the Avatar features."),
				() =>
				{
					AvatarLocalization.Send(from, "avatar.msg.character_recreated_disconnect", "Your character will be recreated and you will be disconnected shortly...");
					
					Timer.DelayCall(TimeSpan.FromSeconds(1), () =>
					{
						var preCtx = AvatarEngine.Instance.GetOrCreateContext( from );
						var newCharacter = CharacterCreation.ResetCharacter( from, false, false );
						AvatarEngine.InitializePlayer( newCharacter );
						AvatarEngine.Instance.ApplyContext( newCharacter, newCharacter.Avatar );
						AnalyticsLogger.LogAvatarEnabled( newCharacter, preCtx );
					});
				}
			);
			from.SendGump(confirmation);
		}

		[Usage("avatar-shop")]
		[Description("Opens the Avatar Shop for the Player.")]
		public static void OpenAvatarShopCommand(CommandEventArgs e)
		{
			var from = (PlayerMobile)e.Mobile;
			if (!from.Avatar.Active)
			{
				AvatarLocalization.Send(from, "avatar.msg.not_enabled", "You do not have the Avatar status enabled.");
				return;
			}

			from.SendGump(new AvatarShopGump(from));
		}
	}
}

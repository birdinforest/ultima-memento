using Server.Commands;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Spells.Jedi;
using Server.Spells.Mystic;
using Server.Spells.Ninjitsu;
using Server.Spells.Shinobi;
using Server.Spells.Syth;
using System;
using System.Collections.Generic;

namespace Server
{
	public class FastPlayer
	{
		public static TimeSpan ArbitraryDelay = TimeSpan.FromMilliseconds(1000); // Add arbitrary delay to see if it reduces "freezes" after zoning

		private static readonly Dictionary<Serial, Type> m_Table = new Dictionary<Serial, Type>();

		public static void Initialize()
		{
			CommandSystem.Register("FastPlayer-Delay", AccessLevel.Administrator, new CommandEventHandler(OnConfigureFastPlayerDelay));
		}

		[Usage("FastPlayer-Delay [DelayMilliseconds]")]
		[Description("Configures the arbitrary delay before sending the ControlSpeed packet to the Client.")]
		private static void OnConfigureFastPlayerDelay(CommandEventArgs e)
		{
			var player = e.Mobile as PlayerMobile;
			if (player == null) return;

			if (e.Arguments.Length == 0)
			{
				if (ArbitraryDelay == TimeSpan.Zero)
					player.SendMessage("The fast player delay is currently disabled.");
				else
					player.SendMessage("The fast player delay is currently '{0}' milliseconds.", ArbitraryDelay.TotalMilliseconds);

				return;
			}

			if (e.Arguments.Length != 1)
			{
				player.SendMessage("Arguments for the command are [FastPlayer-Delay <Milliseconds (int)>");
				return;
			}

			int millisecondsDelay;
			if (!int.TryParse(e.Arguments[0], out millisecondsDelay))
			{
				player.SendMessage("Milliseconds delay must be a valid number.");
				return;
			}

			if (0 < millisecondsDelay)
			{
				ArbitraryDelay = TimeSpan.FromMilliseconds(millisecondsDelay);
				player.SendMessage(68, "Fast player delay has been set to '{0}' milliseconds.", millisecondsDelay);
			}
			else
			{
				ArbitraryDelay = TimeSpan.Zero;
				player.SendMessage(68, "Fast player delay has been disabled.");
			}
		}

		public static bool IsActive(Mobile mobile)
		{
			if (mobile == null) return false;

			return m_Table.ContainsKey(mobile.Serial);
		}

		public static void OnItemAdded(Mobile mobile, Item item)
		{
			if ((mobile is PlayerMobile) == false) return;
			if (item.Layer != Layer.Shoes) return;

			Refresh((PlayerMobile)mobile);
		}

		public static void OnItemRemoved(Mobile mobile, Item item)
		{
			if ((mobile is PlayerMobile) == false) return;
			if (item.Layer != Layer.Shoes) return;

			Refresh((PlayerMobile)mobile);
		}

		public static void Refresh(PlayerMobile player, bool force = false)
		{
			if (player == null) return;

			var activeType = GetActiveItem(player) ?? GetActiveSpell(player);
			if (activeType != null && MySettings.S_NoMountsInCertainRegions)
			{
				var region = Region.Find(player.Location, player.Map);
				if (AnimalTrainer.IsNoMountRegion(player, region) && !Server.Mobiles.AnimalTrainer.AllowMagicSpeed(player, region))
					activeType = null;
			}

			Type oldType;
			m_Table.TryGetValue(player.Serial, out oldType);
			if (!force && activeType == oldType) return; // Nothing changed
			
			player.ClearFastwalkStack();

			if (activeType != null)
			{
				player.Send(SpeedControl.MountSpeed);

				m_Table[player.Serial] = activeType;
			}
			else
			{
				player.Send(SpeedControl.Disable);

				m_Table.Remove(player.Serial);

				if (oldType == null) return;

				if (oldType != typeof(HikingBoots) && typeof(Item).IsAssignableFrom(oldType))
					player.SendMessage("These shoes seem to have their magic diminished here.");
			}
		}

		private static Type GetActiveItem(PlayerMobile player)
		{
			var shoes = player.FindItemOnLayer(Layer.Shoes);
			if (shoes is Artifact_BootsofHermes) return typeof(Artifact_BootsofHermes);
			if (shoes is Artifact_SprintersSandals) return typeof(Artifact_SprintersSandals);
			if (shoes is HikingBoots && 0 < player.RaceID) return typeof(HikingBoots);

			return null;
		}

		private static Type GetActiveSpell(PlayerMobile player)
		{
			if (Celerity.UnderEffect(player)) return typeof(Celerity);
			if (WindRunner.UnderEffect(player)) return typeof(WindRunner);
			if (CheetahPaws.UnderEffect(player)) return typeof(CheetahPaws);
			if (SythSpeed.UnderEffect(player)) return typeof(SythSpeed);

			var context = AnimalForm.GetContext(player);
			if (context != null && context.SpeedBoost) return typeof(AnimalForm);

			return null;
		}
	}
}
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
		private static Type GhostType = typeof(PlayerMobile);
		private static Type IndeterminateType = typeof(object);

		private static readonly Dictionary<Serial, Type> m_Table = new Dictionary<Serial, Type>();

		public static void Initialize()
		{
			EventSink.OnEnterRegion += (args) => Reset(args.From as PlayerMobile);
			EventSink.Login += (args) => Reset(args.Mobile as PlayerMobile);
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

			// Ignore validation if the player is dead
			if (!player.Alive) activeType = GhostType;

			Type oldType;
			m_Table.TryGetValue(player.Serial, out oldType);
			if (!force && activeType == oldType) return; // Nothing changed
			if (activeType == IndeterminateType) return; // Don't send any data. State is currently unknown.

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

		public static void Reset(PlayerMobile player)
		{
			if (player == null) return;

			m_Table[player.Serial] = IndeterminateType;
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
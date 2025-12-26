using System;
using Server.Mobiles;
using System.Collections.Generic;
using System.IO;
using Server.Items;
using Server.Misc;

namespace Server.Engines.Avatar
{
	public class AvatarEngine
	{
		private static AvatarEngine m_Engine;
		private readonly Dictionary<Serial, PlayerContext> m_Context = new Dictionary<Serial, PlayerContext>();

		public static AvatarEngine Instance
		{
			get
			{
				if (m_Engine == null)
					m_Engine = new AvatarEngine();

				return m_Engine;
			}
		}

		public bool IsEnabled { get; set; }

		public static void Configure()
		{
			EventSink.WorldSave += OnWorldSave;
		}

		public static void Initialize()
		{
			LoadData();

			if (Instance.IsEnabled || true)
			{
				EventSink.OnKilledBy += Instance.OnKilledBy;
				EventSink.PlayerDeath += Instance.OnPlayerDeath;
			}
		}

		public void ApplyContext(PlayerMobile player, PlayerContext context)
		{
			if (!context.Active) return;

			player.StatCap = 100 + context.StatCapLevel * PlayerContext.STAT_CAP_PER_LEVEL;

			// Skill cap could have changed
			player.RefreshSkillCap();
		}

		public PlayerContext GetContextOrDefault(Mobile mobile)
		{
			PlayerContext context;
			return mobile != null && mobile is PlayerMobile && m_Context.TryGetValue(mobile.Serial, out context) ? context : PlayerContext.Default;
		}

		public PlayerContext GetOrCreateContext(Mobile mobile)
		{
			var serial = mobile.Serial;

			PlayerContext context;
			if (m_Context.TryGetValue(serial, out context)) return context;

			return m_Context[serial] = new PlayerContext();
		}

		public void MigrateContext(Mobile oldMobile, Mobile newMobile)
		{
			var context = GetContextOrDefault(oldMobile);
			if (!context.Active) return;

			m_Context.Remove(oldMobile.Serial);
			m_Context.Add(newMobile.Serial, context);
		}

		private static void LoadData()
		{
			Instance.IsEnabled = !File.Exists("Saves//Player//Avatar.bin");

			Persistence.Deserialize(
				"Saves//Player//Avatar.bin",
				reader =>
				{
					int version = reader.ReadInt();
					int count = reader.ReadInt();

					for (int i = 0; i < count; ++i)
					{
						var serial = reader.ReadInt();
						var context = new PlayerContext(reader);
						Instance.m_Context.Add(serial, context);
					}

					Console.WriteLine("Loaded Avatar data for '{0}' characters", Instance.m_Context.Count);
					Instance.IsEnabled = true;
				}
			);
		}

		private static void OnWorldSave(WorldSaveEventArgs e)
		{
			Persistence.Serialize(
				"Saves//Player//Avatar.bin",
				writer =>
				{
					writer.Write(0); // version

					writer.Write(Instance.m_Context.Count);
					foreach (var kv in Instance.m_Context)
					{
						writer.Write(kv.Key);
						kv.Value.Serialize(writer);
					}
				}
			);
		}

		private int GetValue<T>(int multiplier, Container corpse) where T : Item
		{
			var item = corpse.FindItemByType<T>();
			if (item == null) return 0;

			return item.Amount * multiplier;
		}

		private void OnKilledBy(OnKilledByArgs e)
		{
			if (e.Corpse == null) return;

			var player = MobileUtilities.TryGetMasterPlayer(e.Killed);
			if (player == null) return;

			var context = GetContextOrDefault(player);
			if (!context.Active) return;

			var corpse = e.Corpse;
			int value = GetValue<DDCopper>(1, corpse);
			value += GetValue<DDSilver>(2, corpse);
			value += GetValue<DDXormite>(30, corpse);
			value += GetValue<Gold>(10, corpse);
			value += GetValue<Crystals>(50, corpse);
			value += GetValue<DDGemstones>(20, corpse);
			value += GetValue<DDJewels>(20, corpse);
			value += GetValue<DDGoldNuggets>(10, corpse);
			if (value < 1) return;

			context.PointsFarmed += (int)(value * context.PointGainRateLevel * PlayerContext.POINT_GAIN_RATE_PER_LEVEL * 0.01);
		}

		private void OnPlayerDeath(PlayerDeathEventArgs e)
		{
			var player = e.Mobile as PlayerMobile;
			if (player == null) return;

			var context = GetContextOrDefault(player);
			if (!context.Active) return;

			context.PointsSaved += context.PointsFarmed;
			context.PointsFarmed = 0;

			var newPlayer = CharacterCreation.ResetCharacter(player);
			if (newPlayer == null) return;

			ApplyContext(newPlayer, newPlayer.Avatar);
		}
	}
}
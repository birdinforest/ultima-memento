using System;
using Server.Engines.CannedEvil;
using Server.Gumps;
using Server.Utilities;

namespace Server.Items
{
	public class ChampionSkull : Item
	{
		// TODO: Add Note about drop rate and decay
		public const int MAX_DAYS_AGE = 7;

		private DateTime m_Created;
		private ChampionSpawnType m_Type;
		private static Timer m_CleanupTimer; // Keep track to make sure it's not GC'd

		[Constructable]
		public ChampionSkull() : this(GetRandomType())
		{
		}

		[Constructable]
		public ChampionSkull(ChampionSpawnType type) : base(0x1AE1)
		{
			Name = "A skull of summoning";
			Type = type;
			LootType = LootType.Cursed;
			m_Created = DateTime.UtcNow;
		}

		public ChampionSkull(Serial serial) : base(serial)
		{
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime Created
		{
			get { return m_Created; }
			set
			{
				m_Created = value;
				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public ChampionSpawnType Type
		{
			get { return m_Type; }
			set
			{
				m_Type = value;
				switch (value)
				{
					case ChampionSpawnType.ColdBlood: Hue = 0x159; break;
					case ChampionSpawnType.ForestLord: Hue = 0; break;
					case ChampionSpawnType.Arachnid: Hue = 0x1EE; break;
					case ChampionSpawnType.VerminHorde: Hue = 0x025; break;
					case ChampionSpawnType.UnholyTerror: Hue = 0x035; break;
					case ChampionSpawnType.Abyss: Hue = 0x035; break;
				}

				InvalidateProperties();
			}
		}

		public static void Initialize()
		{
			m_CleanupTimer = Timer.DelayCall(TimeSpan.Zero, TimeSpan.FromHours(1), () =>
			{
				Console.WriteLine("[Champion System]: Cleaning up expired skulls");
				var expirationTimestamp = DateTime.UtcNow.AddDays(0 - MAX_DAYS_AGE);
				foreach (var skull in WorldUtilities.ForEachItem<ChampionSkull>(item => !item.Deleted))
				{
					if (skull.m_Created < expirationTimestamp)
						skull.Delete();
					else
						skull.InvalidateProperties();
				}
			});
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 1:
					{
						m_Created = reader.ReadDateTime();
						goto case 0;
					}
				case 0:
					{
						m_Type = (ChampionSpawnType)reader.ReadInt();
						if (version == 0)
							m_Created = DateTime.UtcNow;

						break;
					}
			}
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			TimeSpan timeRemaining = m_Created.AddDays(MAX_DAYS_AGE) - DateTime.UtcNow;
			if (timeRemaining.TotalMinutes < 0)
			{
				list.Add("It is a plain skull.");
				return;
			}

			list.Add("[Use on a Champion Idol]");

			if (timeRemaining.TotalDays < 1)
				list.Add("Energy: Faint"); // < 1 day
			else if (timeRemaining.TotalDays < 3)
				list.Add("Energy: Waning"); // 1-3 days
			else if (timeRemaining.TotalDays < 5)
				list.Add("Energy: Diminished"); // 3-5 days
			else if (timeRemaining.TotalDays < 7)
				list.Add("Energy: Fading"); // 5-7 days
			else
				list.Add("Energy: Potent"); // 7+ days

			list.Add("Type: " + ChampionSpawnInfo.GetName(m_Type));
		}

		public override void OnDoubleClick(Mobile from)
		{
			from.SendGump(new ChampionSkullGump(from, this));
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)1); // version

			writer.Write(m_Created);
			writer.Write((int)m_Type);
		}

		private static ChampionSpawnType GetRandomType()
		{
			if (Utility.Random(10) == 1) return ChampionSpawnType.ForestLord;

			switch (Utility.Random(5))
			{
				default:
				case 0: return ChampionSpawnType.ColdBlood;
				case 1: return ChampionSpawnType.Arachnid;
				case 2: return ChampionSpawnType.VerminHorde;
				case 3: return ChampionSpawnType.UnholyTerror;
				case 4: return ChampionSpawnType.Abyss;
			}
		}
	}
}
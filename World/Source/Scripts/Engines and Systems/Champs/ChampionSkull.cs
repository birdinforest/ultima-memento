using Server.Engines.CannedEvil;
using Server.Targeting;

namespace Server.Items
{
	public class ChampionSkull : Item
	{
		private ChampionSpawnType m_Type;

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
		}

		public ChampionSkull(Serial serial) : base(serial)
		{
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

		public override void OnDoubleClick(Mobile from)
		{
			from.SendMessage("Target the champion idol you would like to challenge.");
			from.Target = new InternalTarget(this);
		}

		private class InternalTarget : Target
		{
			private readonly ChampionSkull m_Skull;

			public InternalTarget(ChampionSkull skull) : base(3, false, TargetFlags.None)
			{
				m_Skull = skull;
			}

			protected override void OnTarget(Mobile from, object o)
			{
				if (o is IdolOfTheChampion)
				{
					IdolOfTheChampion idol = (IdolOfTheChampion)o;
					if (idol.Spawn == null || m_Skull.Deleted)
					{
						from.SendMessage("This champion idol is not ready yet."); // Unsupported case
					}
					else if (idol.Spawn.Active)
					{
						from.SendMessage("A Champion has already been challenged.");
					}
					else
					{
						idol.Spawn.Active = true;
						idol.Spawn.Type = m_Skull.Type;
						from.Direction = from.GetDirectionTo( idol );
						from.Animate( 32, 5, 1, true, false, 0 ); // Bow
						from.SendMessage("You smash the skull on the champion idol in defiance.");

						m_Skull.Delete();
					}
				}
				else
				{
					from.SendMessage("You must target a champion idol.");
				}
			}
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			list.Add("[Use on a Champion Idol]");

			string type = null;
			switch (m_Type)
			{
				case ChampionSpawnType.ColdBlood: type = "Cold Blood"; break;
				case ChampionSpawnType.ForestLord: type = "Forest Lord"; break;
				case ChampionSpawnType.Arachnid: type = "Arachnid"; break;
				case ChampionSpawnType.VerminHorde: type = "Vermin Horde"; break;
				case ChampionSpawnType.UnholyTerror: type = "Unholy Terror"; break;
				case ChampionSpawnType.Abyss: type = "Abyss"; break;
			}
			list.Add("Type: " + type);

			string[] slayers = null;
			switch (m_Type)
			{
				case ChampionSpawnType.ColdBlood: slayers = new string[] { }; break;
				case ChampionSpawnType.ForestLord: slayers = new string[] { }; break;
				case ChampionSpawnType.Arachnid: slayers = new string[] { }; break;
				case ChampionSpawnType.VerminHorde: slayers = new string[] { }; break;
				case ChampionSpawnType.UnholyTerror: slayers = new string[] { }; break;
				case ChampionSpawnType.Abyss: slayers = new string[] { }; break;
			}

			if (slayers != null && 0 < slayers.Length)
			{
				list.Add("Recommended Slayers:");
				foreach (string slayer in slayers)
					list.Add(slayer);
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0); // version

			writer.Write((int)m_Type);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						m_Type = (ChampionSpawnType)reader.ReadInt();

						break;
					}
			}
		}
	}
}
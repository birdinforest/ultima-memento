using System;
using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles
{
	public abstract class BaseChampion : BaseCreature
	{
		public override bool CanMoveOverObstacles { get { return true; } }
		public override bool CanDestroyObstacles { get { return true; } }

		public BaseChampion(AIType aiType) : this(aiType, FightMode.Closest)
		{
		}

		public BaseChampion(AIType aiType, FightMode mode) : base(aiType, mode, 18, 1, 0.1, 0.2)
		{
		}

		public BaseChampion(Serial serial) : base(serial)
		{
		}

		public abstract Type[] DecorativeList { get; }
		public abstract MonsterStatuetteType[] StatueTypes { get; }

		public virtual bool NoGoodies { get { return false; } }

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}

		public Item GetArtifact()
		{
			double random = Utility.RandomDouble();

			if (0.30 >= random)
				return CreateArtifact(DecorativeList);

			return null;
		}

		public Item CreateArtifact(Type[] list)
		{
			if (list.Length == 0)
				return null;

			int random = Utility.Random(list.Length);

			Type type = list[random];

			Item artifact = Loot.Construct(type);

			if (artifact is MonsterStatuette && StatueTypes.Length > 0)
			{
				((MonsterStatuette)artifact).Type = StatueTypes[Utility.Random(StatueTypes.Length)];
				((MonsterStatuette)artifact).LootType = LootType.Regular;
			}

			return artifact;
		}

		private PowerScroll CreateRandomPowerScroll()
		{
			int level;
			double random = Utility.RandomDouble();

			if (0.05 >= random) // 5%
				level = 25;
			else if (0.2 >= random) // 15%
				level = 20;
			else if (0.45 >= random) // 25%
				level = 15;
			else if (0.80 >= random) // 35%
				level = 10;
			else // 20%
				level = 5;

			return PowerScroll.CreateRandomNoCraft(level, level);
		}

		public void GivePowerScrolls()
		{
			List<Mobile> toGive = new List<Mobile>();
			List<DamageStore> rights = GetLootingRights(this.DamageEntries, this.HitsMax);

			for (int i = rights.Count - 1; i >= 0; --i)
			{
				DamageStore ds = rights[i];

				if (ds.m_HasRight)
					toGive.Add(ds.m_Mobile);
			}

			if (toGive.Count == 0)
				return;

			// Randomize
			for (int i = 0; i < toGive.Count; ++i)
			{
				int rand = Utility.Random(toGive.Count);
				Mobile hold = toGive[i];
				toGive[i] = toGive[rand];
				toGive[rand] = hold;
			}

			for (int i = 0; i < 6; ++i)
			{
				Mobile m = toGive[i % toGive.Count];

				PowerScroll ps = CreateRandomPowerScroll();

				GivePowerScrollTo(m, ps);
			}
		}

		public static void GivePowerScrollTo(Mobile m, PowerScroll ps)
		{
			if (ps == null || m == null)    //sanity
				return;

			m.SendLocalizedMessage(1049524); // You have received a scroll of power!

			if (!Core.SE || m.Alive)
				m.AddToBackpack(ps);
			else
			{
				if (m.Corpse != null && !m.Corpse.Deleted)
					m.Corpse.DropItem(ps);
				else
					m.AddToBackpack(ps);
			}
		}

		public override bool OnBeforeDeath()
		{
			if (!NoKillAwards)
			{
				GivePowerScrolls();

				if (NoGoodies)
					return base.OnBeforeDeath();

				Map map = this.Map;

				if (map != null)
				{
					for (int x = -12; x <= 12; ++x)
					{
						for (int y = -12; y <= 12; ++y)
						{
							double dist = Math.Sqrt(x * x + y * y);

							if (dist <= 12)
								new GoodiesTimer(map, X + x, Y + y).Start();
						}
					}
				}
			}

			return base.OnBeforeDeath();
		}

		public override void OnDeath(Container c)
		{
			List<DamageStore> rights = BaseCreature.GetLootingRights(this.DamageEntries, this.HitsMax);
			List<Mobile> toGive = new List<Mobile>();

			for (int i = rights.Count - 1; i >= 0; --i)
			{
				DamageStore ds = rights[i];

				if (ds.m_HasRight)
					toGive.Add(ds.m_Mobile);
			}

			// if ( toGive.Count > 0 )
			// 	toGive[Utility.Random( toGive.Count )].AddToBackpack( new ChampionSkull( SkullType ) );
			// else
			// 	c.DropItem( new ChampionSkull( SkullType ) );

			base.OnDeath(c);
		}

		private class GoodiesTimer : Timer
		{
			private Map m_Map;
			private int m_X, m_Y;

			public GoodiesTimer(Map map, int x, int y) : base(TimeSpan.FromSeconds(Utility.RandomDouble() * 10.0))
			{
				m_Map = map;
				m_X = x;
				m_Y = y;
			}

			protected override void OnTick()
			{
				int z = m_Map.GetAverageZ(m_X, m_Y);
				bool canFit = m_Map.CanFit(m_X, m_Y, z, 6, false, false);

				for (int i = -3; !canFit && i <= 3; ++i)
				{
					canFit = m_Map.CanFit(m_X, m_Y, z + i, 6, false, false);

					if (canFit)
						z += i;
				}

				if (!canFit)
					return;

				int amount = (int)(Utility.RandomMinMax(500, 1000) * (MyServerSettings.GetGoldCutRate() * .01));
				Gold g = new Gold(amount);

				g.MoveToWorld(new Point3D(m_X, m_Y, z), m_Map);

				if (0.5 >= Utility.RandomDouble())
				{
					switch (Utility.Random(3))
					{
						case 0: // Fire column
							{
								Effects.SendLocationParticles(EffectItem.Create(g.Location, g.Map, EffectItem.DefaultDuration), 0x3709, 10, 30, 5052);
								Effects.PlaySound(g, g.Map, 0x208);

								break;
							}
						case 1: // Explosion
							{
								Effects.SendLocationParticles(EffectItem.Create(g.Location, g.Map, EffectItem.DefaultDuration), 0x36BD, 20, 10, 5044);
								Effects.PlaySound(g, g.Map, 0x307);

								break;
							}
						case 2: // Ball of fire
							{
								Effects.SendLocationParticles(EffectItem.Create(g.Location, g.Map, EffectItem.DefaultDuration), 0x36FE, 10, 10, 5052);

								break;
							}
					}
				}
			}
		}
	}
}

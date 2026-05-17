using System;
using Server.Targeting;
using Server.Localization;

namespace Server.Items
{
	public class ItemExperienceToken : Item
	{
		public override bool IsContentLocalized => true;

		public override void AddNameProperty(ObjectPropertyList list)
		{
			if (BuildingPropertyListLocale != null)
			{
				if (Amount <= 1)
					AddLocalizedProperty(list, "item.god.exp.token.name");
				else
					list.Add(1050039, "{0}\t{1}", Amount, ResolvePropertyText("item.god.exp.token.name"));
				return;
			}
			base.AddNameProperty(list);
		}

		[Constructable]
		public ItemExperienceToken(int experience) : this()
		{
			Experience = experience;
		}

		public ItemExperienceToken() : base(0x2AAA)
		{
			Name = "Experience token";
			LootType = LootType.Blessed;
			Light = LightType.Circle300;
		}

		public ItemExperienceToken(Serial serial) : base(serial)
		{
		}

		private int m_Experience;

		[CommandProperty(AccessLevel.GameMaster)]
		public int Experience
		{
			get { return m_Experience; }
			set
			{
				m_Experience = value;
				InvalidateProperties();
			}
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			if (0 < Experience)
			{
				if ( BuildingPropertyListLocale != null )
					AddLocalizedProperty(list, "god.xp.token", Experience);
				else
					list.Add(1060659, "Experience\t{0}", Experience);
			}
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (!IsChildOf(from.Backpack))
			{
				from.SendLocalizedMessage(1062334); // This item must be in your backpack to be used.
			}
			else if (Experience < 1)
			{
				from.SendMessage(StringCatalog.ResolveByKey(from.Account, "god.msg.xp.token.empty"));
			}
			else
			{
				from.SendMessage(StringCatalog.ResolveByKey(from.Account, "god.msg.xp.token.select.target"));
				from.Target = new InternalTarget(this);
			}
		}

		private class InternalTarget : Target
		{
			private readonly ItemExperienceToken m_Token;

			public InternalTarget(ItemExperienceToken token) : base(0, false, TargetFlags.None)
			{
				m_Token = token;
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				var item = targeted as Item;
				if (item == null)
				{
					from.SendMessage(StringCatalog.ResolveByKey(from.Account, "god.msg.xp.token.must.target.item"));
					return;
				}

				if (item is ItemExperienceToken)
				{
					var token = (ItemExperienceToken)item;

					if (token == m_Token)
					{
						from.SendMessage(StringCatalog.ResolveByKey(from.Account, "god.msg.xp.token.merge.self"));
					}
					else
					{
						m_Token.Experience += token.Experience;
						token.Delete();
						from.SendMessage(StringCatalog.ResolveByKey(from.Account, "god.msg.xp.token.merge.done"));
					}

					return;
				}

				if (item is ILevelable)
				{
					var levelable = (ILevelable)item;
					if (levelable.Level == levelable.MaxLevel)
					{
						from.SendMessage(StringCatalog.ResolveByKey(from.Account, "god.msg.xp.token.max.level"));
						return;
					}

					var expToNextLevel = LevelItemManager.ExpTable[levelable.Level] - levelable.Experience;
					var expToAdd = Math.Min(m_Token.Experience, expToNextLevel);
					if (expToAdd < 1)
					{
						from.SendMessage(StringCatalog.ResolveByKey(from.Account, "god.msg.xp.token.no.room"));
						return;
					}

					LevelItemManager.GrantExperience(levelable, expToAdd, from);
					from.SendMessage(StringCatalog.ResolveFormatByKey(from.Account, "god.msg.xp.token.added", expToAdd));

					m_Token.Experience -= expToAdd;
					if (m_Token.Experience < 1)
						m_Token.Delete();
				}
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
			writer.Write(Experience);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
			Experience = reader.ReadInt();
		}
	}
}
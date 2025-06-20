using System;
using Server.Targeting;

namespace Server.Items
{
	public class ItemExperienceToken : Item
	{
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
				list.Add(1060659, "Experience\t{0}", Experience);
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (!IsChildOf(from.Backpack))
			{
				from.SendLocalizedMessage(1062334); // This item must be in your backpack to be used.
			}
			else if (Experience < 1)
			{
				from.SendMessage("This token has no experience.");
			}
			else
			{
				from.SendMessage("Select an item to add experience to.");
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
					from.SendMessage("You must target an item.");
					return;
				}

				if (item is ItemExperienceToken)
				{
					var token = (ItemExperienceToken)item;

					if (token == m_Token)
					{
						from.SendMessage("You cannot merge a token with itself.");
					}
					else
					{
						m_Token.Experience += token.Experience;
						token.Delete();
						from.SendMessage("You have merged the two tokens.");
					}

					return;
				}

				if (item is ILevelable)
				{
					var levelable = (ILevelable)item;
					if (levelable.Level == levelable.MaxLevel)
					{
						from.SendMessage("This item is already at max level.");
						return;
					}

					var expToNextLevel = LevelItemManager.ExpTable[levelable.Level] - levelable.Experience;
					var expToAdd = Math.Min(m_Token.Experience, expToNextLevel);
					if (expToAdd < 1)
					{
						from.SendMessage("This item cannot hold any more experience.");
						return;
					}

					LevelItemManager.GrantExperience(levelable, expToAdd, from);
					from.SendMessage("You add {0} experience to the item.", expToAdd);

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
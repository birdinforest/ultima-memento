namespace Server.Items
{
	public class AdvancedSkinningKnife : SkinningKnifeTool
	{
		private int m_YieldBonus;

		[Constructable]
		public AdvancedSkinningKnife() : this(20, 50)
		{
		}

		[Constructable]
		public AdvancedSkinningKnife(int yieldBonus, int uses) : base()
		{
			Name = "Advanced Skinning Knife";
			YieldBonus = yieldBonus;
			UsesRemaining = uses;
		}

		public AdvancedSkinningKnife(Serial serial) : base(serial)
		{
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int YieldBonus
		{
			get { return m_YieldBonus; }
			set { m_YieldBonus = value; InvalidateProperties(); }
		}

		public override void AppendChildProperties(ObjectPropertyList list)
		{
			base.AppendChildProperties(list);

			if (0 < m_YieldBonus)
				list.Add("Increases carving yields by {0}%", m_YieldBonus);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
			m_YieldBonus = reader.ReadInt();
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0); // version
			writer.Write((int)m_YieldBonus);
		}
	}
}
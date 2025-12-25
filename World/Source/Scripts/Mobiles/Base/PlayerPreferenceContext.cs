namespace Server.Mobiles
{
	[PropertyObject]
	public class PlayerPreferenceContext
	{
		public PlayerPreferenceContext()
		{
			ColorlessFabricBreakdown = true;
		}

		public PlayerPreferenceContext(GenericReader reader)
		{
			int version = reader.ReadInt();

			DoubleClickID = reader.ReadBool();
			SuppressVendorTooltip = reader.ReadBool();
			SingleAttemptID = reader.ReadBool();
			ColorlessFabricBreakdown = reader.ReadBool();
			CharacterSheath = reader.ReadBool();
			CharacterWepAbNames = reader.ReadBool();
			CharMusical = reader.ReadString();
			WeaponBarOpen = reader.ReadBool();
			IgnoreVendorGoldSafeguard = reader.ReadBool();
			ClassicPoisoning = reader.ReadBool();
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool CharacterSheath { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool CharacterWepAbNames { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public string CharMusical { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool ClassicPoisoning { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool ColorlessFabricBreakdown { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool DoubleClickID { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool IgnoreVendorGoldSafeguard { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool SingleAttemptID { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool SuppressVendorTooltip { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool WeaponBarOpen { get; set; }

		public void Serialize(GenericWriter writer)
		{
			writer.Write(1);

			writer.Write(DoubleClickID);
			writer.Write(SuppressVendorTooltip);
			writer.Write(SingleAttemptID);
			writer.Write(ColorlessFabricBreakdown);
			writer.Write(CharacterSheath);
			writer.Write(CharacterWepAbNames);
			writer.Write(CharMusical);
			writer.Write(WeaponBarOpen);
			writer.Write(IgnoreVendorGoldSafeguard);
			writer.Write(ClassicPoisoning);
		}
	}
}
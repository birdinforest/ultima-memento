using Server.Engines.Craft;

namespace Server.Items
{
	[Flipable(0x0FB4, 0x0FB5)]
	public class SmithHammerRunicI : BaseTool, IRunicTool
	{
		[Constructable]
		public SmithHammerRunicI() : this(50)
		{
		}

		[Constructable]
		public SmithHammerRunicI(int uses) : base(uses, 0x0FB4)
		{
			Name = "runic smith hammer I";
			Weight = 1.0;
			Layer = Layer.OneHanded;
		}

		public SmithHammerRunicI(Serial serial) : base(serial)
		{
		}

		public override CraftSystem CraftSystem
		{ get { return DefBlacksmithy.CraftSystem; } }

		public override Catalogs DefaultCatalog
		{ get { return Catalogs.None; } }

		public override string DefaultDescription
		{ get { return "This tool can be used to create slightly magical items."; } }

		public int RunicMinAttributes { get { return 1; } }
		public int RunicMaxAttributes { get { return 1; } }
		public int RunicMinIntensity { get { return 40; } }
		public int RunicMaxIntensity { get { return 40; } }

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();
		}

		public override int isWeapon()
		{
			return 25744;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int)0); // version
		}
	}

	[Flipable(0x0FB4, 0x0FB5)]
	public class SmithHammerRunicII : BaseTool, IRunicTool
	{
		[Constructable]
		public SmithHammerRunicII() : this(50)
		{
		}

		[Constructable]
		public SmithHammerRunicII(int uses) : base(uses, 0x0FB4)
		{
			Name = "runic smith hammer II";
			Weight = 1.0;
			Layer = Layer.OneHanded;
		}

		public SmithHammerRunicII(Serial serial) : base(serial)
		{
		}

		public override CraftSystem CraftSystem
		{ get { return DefBlacksmithy.CraftSystem; } }

		public override Catalogs DefaultCatalog
		{ get { return Catalogs.None; } }

		public override string DefaultDescription
		{ get { return "This tool can be used to create moderately magical items."; } }

		public int RunicMinAttributes { get { return 2; } }
		public int RunicMaxAttributes { get { return 2; } }
		public int RunicMinIntensity { get { return 40; } }
		public int RunicMaxIntensity { get { return 50; } }

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();
		}

		public override int isWeapon()
		{
			return 25744;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int)0); // version
		}
	}

	[Flipable(0x0FB4, 0x0FB5)]
	public class SmithHammerRunicIII : BaseTool, IRunicTool
	{
		[Constructable]
		public SmithHammerRunicIII() : this(50)
		{
		}

		[Constructable]
		public SmithHammerRunicIII(int uses) : base(uses, 0x0FB4)
		{
			Name = "runic smith hammer III";
			Weight = 1.0;
			Layer = Layer.OneHanded;
		}

		public SmithHammerRunicIII(Serial serial) : base(serial)
		{
		}

		public override CraftSystem CraftSystem
		{ get { return DefBlacksmithy.CraftSystem; } }

		public override Catalogs DefaultCatalog
		{ get { return Catalogs.None; } }

		public override string DefaultDescription
		{ get { return "This tool can be used to create very magical items."; } }

		public int RunicMinAttributes { get { return 3; } }
		public int RunicMaxAttributes { get { return 3; } }
		public int RunicMinIntensity { get { return 70; } }
		public int RunicMaxIntensity { get { return 70; } }

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();
		}

		public override int isWeapon()
		{
			return 25744;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int)0); // version
		}
	}
}
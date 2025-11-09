using Server.Engines.Craft;

namespace Server.Items
{
	public class LeatherworkingToolsRunicI : BaseTool, IRunicTool
	{
		[Constructable]
		public LeatherworkingToolsRunicI() : this(50)
		{
		}

		[Constructable]
		public LeatherworkingToolsRunicI(int uses) : base(uses, 0x66FA)
		{
			Name = "runic tanning tools I";
			Weight = 1.0;
			Layer = Layer.OneHanded;
		}

		public LeatherworkingToolsRunicI(Serial serial) : base(serial)
		{
		}

		public override CraftSystem CraftSystem
		{ get { return DefLeatherworking.CraftSystem; } }

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

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int)0); // version
		}
	}

	public class LeatherworkingToolsRunicII : BaseTool, IRunicTool
	{
		[Constructable]
		public LeatherworkingToolsRunicII() : this(50)
		{
		}

		[Constructable]
		public LeatherworkingToolsRunicII(int uses) : base(uses, 0x66FA)
		{
			Name = "runic tanning tools II";
			Weight = 1.0;
			Layer = Layer.OneHanded;
		}

		public LeatherworkingToolsRunicII(Serial serial) : base(serial)
		{
		}

		public override CraftSystem CraftSystem
		{ get { return DefLeatherworking.CraftSystem; } }

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

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int)0); // version
		}
	}

	public class LeatherworkingToolsRunicIII : BaseTool, IRunicTool
	{
		[Constructable]
		public LeatherworkingToolsRunicIII() : this(50)
		{
		}

		[Constructable]
		public LeatherworkingToolsRunicIII(int uses) : base(uses, 0x66FA)
		{
			Name = "runic tanning tools III";
			Weight = 1.0;
			Layer = Layer.OneHanded;
		}

		public LeatherworkingToolsRunicIII(Serial serial) : base(serial)
		{
		}

		public override CraftSystem CraftSystem
		{ get { return DefLeatherworking.CraftSystem; } }

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

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int)0); // version
		}
	}
}
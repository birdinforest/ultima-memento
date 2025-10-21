using Server.Engines.Craft;

namespace Server.Items
{
	public class TinkerToolsRunicI : BaseTool, IRunicTool
	{
		[Constructable]
		public TinkerToolsRunicI() : this(50)
		{
		}

		[Constructable]
		public TinkerToolsRunicI(int uses) : base(uses, 0x6708)
		{
			Name = "runic tinker tools I";
			Weight = 1.0;
			Layer = Layer.OneHanded;
		}

		public TinkerToolsRunicI(Serial serial) : base(serial)
		{
		}

		public override CraftSystem CraftSystem
		{ get { return DefTinkering.CraftSystem; } }

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

	public class TinkerToolsRunicII : BaseTool, IRunicTool
	{
		[Constructable]
		public TinkerToolsRunicII() : this(50)
		{
		}

		[Constructable]
		public TinkerToolsRunicII(int uses) : base(uses, 0x6708)
		{
			Name = "runic tinker tools II";
			Weight = 1.0;
			Layer = Layer.OneHanded;
		}

		public TinkerToolsRunicII(Serial serial) : base(serial)
		{
		}

		public override CraftSystem CraftSystem
		{ get { return DefTinkering.CraftSystem; } }

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

	public class TinkerToolsRunicIII : BaseTool, IRunicTool
	{
		[Constructable]
		public TinkerToolsRunicIII() : this(50)
		{
		}

		[Constructable]
		public TinkerToolsRunicIII(int uses) : base(uses, 0x6708)
		{
			Name = "runic tinker tools III";
			Weight = 1.0;
			Layer = Layer.OneHanded;
		}

		public TinkerToolsRunicIII(Serial serial) : base(serial)
		{
		}

		public override CraftSystem CraftSystem
		{ get { return DefTinkering.CraftSystem; } }

		public override Catalogs DefaultCatalog
		{ get { return Catalogs.None; } }

		public override string DefaultDescription
		{ get { return "This tool can be used to create very magical items."; } }

		public int RunicMinAttributes { get { return 3; } }
		public int RunicMaxAttributes { get { return 3; } }
		public int RunicMinIntensity { get { return 50; } }
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
}
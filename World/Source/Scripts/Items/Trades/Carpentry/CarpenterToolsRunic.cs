using Server.Engines.Craft;

namespace Server.Items
{
	public class CarpenterToolsRunicI : BaseTool, IRunicTool
	{
		[Constructable]
		public CarpenterToolsRunicI() : this(50)
		{
		}

		[Constructable]
		public CarpenterToolsRunicI(int uses) : base(uses, 0x4F52)
		{
			Name = "runic carpenter tools I";
			Weight = 1.0;
			Layer = Layer.OneHanded;
		}

		public CarpenterToolsRunicI(Serial serial) : base(serial)
		{
		}

		public override CraftSystem CraftSystem
		{ get { return DefCarpentry.CraftSystem; } }

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

	public class CarpenterToolsRunicII : BaseTool, IRunicTool
	{
		[Constructable]
		public CarpenterToolsRunicII() : this(50)
		{
		}

		[Constructable]
		public CarpenterToolsRunicII(int uses) : base(uses, 0x4F52)
		{
			Name = "runic carpenter tools II";
			Weight = 1.0;
			Layer = Layer.OneHanded;
		}

		public CarpenterToolsRunicII(Serial serial) : base(serial)
		{
		}

		public override CraftSystem CraftSystem
		{ get { return DefCarpentry.CraftSystem; } }

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

	public class CarpenterToolsRunicIII : BaseTool, IRunicTool
	{
		[Constructable]
		public CarpenterToolsRunicIII() : this(50)
		{
		}

		[Constructable]
		public CarpenterToolsRunicIII(int uses) : base(uses, 0x4F52)
		{
			Name = "runic carpenter tools III";
			Weight = 1.0;
			Layer = Layer.OneHanded;
		}

		public CarpenterToolsRunicIII(Serial serial) : base(serial)
		{
		}

		public override CraftSystem CraftSystem
		{ get { return DefCarpentry.CraftSystem; } }

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
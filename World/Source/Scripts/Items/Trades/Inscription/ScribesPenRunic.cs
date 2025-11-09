using Server.Engines.Craft;

namespace Server.Items
{
	public class ScribesPenRunicI : BaseTool, IRunicTool
	{
		[Constructable]
		public ScribesPenRunicI() : this(50)
		{
		}

		[Constructable]
		public ScribesPenRunicI(int uses) : base(uses, 0x316D)
		{
			Name = "runic scribe quill I";
			Weight = 1.0;
			Layer = Layer.OneHanded;
		}

		public ScribesPenRunicI(Serial serial) : base(serial)
		{
		}

		public override CraftSystem CraftSystem
		{ get { return DefInscription.CraftSystem; } }

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

	public class ScribesPenRunicII : BaseTool, IRunicTool
	{
		[Constructable]
		public ScribesPenRunicII() : this(50)
		{
		}

		[Constructable]
		public ScribesPenRunicII(int uses) : base(uses, 0x316D)
		{
			Name = "runic scribe quill II";
			Weight = 1.0;
			Layer = Layer.OneHanded;
		}

		public ScribesPenRunicII(Serial serial) : base(serial)
		{
		}

		public override CraftSystem CraftSystem
		{ get { return DefInscription.CraftSystem; } }

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

	public class ScribesPenRunicIII : BaseTool, IRunicTool
	{
		[Constructable]
		public ScribesPenRunicIII() : this(50)
		{
		}

		[Constructable]
		public ScribesPenRunicIII(int uses) : base(uses, 0x316D)
		{
			Name = "runic scribe quill III";
			Weight = 1.0;
			Layer = Layer.OneHanded;
		}

		public ScribesPenRunicIII(Serial serial) : base(serial)
		{
		}

		public override CraftSystem CraftSystem
		{ get { return DefInscription.CraftSystem; } }

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
using Server.Engines.Harvest;
using Server.Targets;
using Server.Localization;

namespace Server.Items
{
	public class SkinningKnifeTool : BaseHarvestTool
	{
		public override HarvestSystem HarvestSystem { get { return null; } }
		public override Layer DefaultLayer { get { return Layer.Trinket; } }
		public override Catalogs DefaultCatalog { get { return Catalogs.Tool; } }
		public override string DefaultDescription { get { return StringCatalog.Resolve( null, "This knife is used to carve corpses. It has a limited amount of uses before it breaks. It will automatically carve corpses you open when you have it equipped." ); } }
		public override string InfoDataLocalizationKey { get { return "prop.trade.itemdesc.skinningknife"; } }

		public override string DisplayNameLocalizationKey => "item.trade.name.skinning.knife";

		[Constructable]
		public SkinningKnifeTool() : this(50)
		{
		}

		[Constructable]
		public SkinningKnifeTool(int uses) : base(uses, 0xEC4)
		{
			InfoText1 = "Equip to skin automatically";
		}

		public override void AddNameProperties( ObjectPropertyList list )
		{
			string saved = m_InfoText1;

			if ( BuildingPropertyListLocale != null )
				m_InfoText1 = null;

			base.AddNameProperties( list );

			if ( BuildingPropertyListLocale != null )
				AddLocalizedProperty( list, "prop.infotext.skinning" );

			m_InfoText1 = saved;
		}

		public SkinningKnifeTool(Serial serial) : base(serial)
		{
		}

		public override void OnDoubleClick(Mobile from)
		{
			from.SendLocalizedMessage(1010018); // What do you want to use this item on?

			from.Target = new BladedItemTarget(this);
		}

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
	}
}
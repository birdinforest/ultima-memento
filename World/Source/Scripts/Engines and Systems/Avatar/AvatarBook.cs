using Server.Mobiles;

namespace Server.Engines.Avatar
{
	public class AvatarBook : Item
	{
		[Constructable]
		public AvatarBook() : base(0x2147)
		{
			Name = "The Avatar's Ascent";
			Movable = false;
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (false == from is PlayerMobile) return;

			from.SendGump(new AvatarShopGump((PlayerMobile)from));
		}

		public AvatarBook(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}
}
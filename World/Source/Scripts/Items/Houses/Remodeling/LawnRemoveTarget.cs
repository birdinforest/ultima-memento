using Server.Items;
using Server.Multis;
using Server.Targeting;

namespace Server.Gumps
{
	public class LawnRemoveTarget : Target
	{
		private readonly BaseHouse m_House;

		public LawnRemoveTarget(BaseHouse house) : base(-1, true, TargetFlags.None)
		{
			m_House = house;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			var t = targeted as IPoint3D;
			if (t == null) return;
		
			var item = targeted as LawnItem;
			if (item == null)
				from.SendMessage("You cannot refund that item.");
			else if (item.House != m_House)
				from.SendMessage("That item does not belong to this house.");
			else
				item.Refund(from);

			from.SendMessage("Please target the item to remove.");
			from.Target = new LawnRemoveTarget(m_House);
		}
	}
}
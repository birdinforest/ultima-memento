using Server.Gumps;
using Server.Items;
using Server.Network;
using Server.Targeting;
using Server.Utilities;
using System;
using System.Globalization;
using System.Linq;

namespace Server.Engines.GlobalShoppe
{
	public class OrderGump : Gump
	{
		private readonly IOrderContext m_Deed;
		private readonly Mobile m_From;

		public OrderGump(Mobile from, IOrderContext deed) : base(25, 25)
		{
			m_From = from;
			m_Deed = deed;

			AddPage(0);

			AddBackground(50, 10, 455, 260, 0x1453);
			AddImageTiled(58, 20, 438, 241, 2624);
			AddAlphaRegion(58, 20, 438, 241);

			AddHtmlLocalized(225, 25, 120, 20, 1045133, 0x7FFF, false, false); // A bulk order

			AddHtmlLocalized(75, 48, 250, 20, 1045136, 0x7FFF, false, false); // Item requested:
			TextInfo cultInfo = new CultureInfo("en-US", false).TextInfo;
			AddLabel(250, 48, 1152, cultInfo.ToTitleCase(deed.ItemName));

			AddHtmlLocalized(75, 72, 250, 20, 1045138, 0x7FFF, false, false); // Amount to make:
			AddLabel(250, 72, 1152, deed.MaxAmount.ToString());

			AddHtmlLocalized(75, 96, 200, 20, 1045153, 0x7FFF, false, false); // Amount finished:
			AddLabel(250, 96, 0x480, deed.CurrentAmount.ToString());

			// TODO: Add background
			AddItem(410, 72, deed.GraphicId);
			AddSpecialRequirements(deed);

			if (!deed.IsComplete)
			{
				AddButton(125, 202, 4005, 4007, 2, GumpButtonType.Reply, 0);
				TextDefinition.AddHtmlText(this, 160, 205, 300, 20, "Add requested item", HtmlColors.WHITE);
			}

			AddButton(125, 226, 4005, 4007, 1, GumpButtonType.Reply, 0);
			AddHtmlLocalized(160, 229, 120, 20, 1011441, 0x7FFF, false, false); // EXIT
		}

		public static void BeginCombine(Mobile from, IOrderContext order)
		{
			if (!order.IsComplete)
				from.Target = new InternalTarget(order);
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			if (m_Deed.IsComplete)
				return;

			if (info.ButtonID == 2) // Combine
			{
				m_From.SendGump(new OrderGump(m_From, m_Deed));
				BeginCombine(m_From, m_Deed);
			}
		}

		private void AddSpecialRequirements(IOrderContext deed)
		{
			var requireExceptional = deed is IExceptionalItem && ((IExceptionalItem)deed).RequireExceptional;
			var requireResource = deed is IResourceItem && ((IResourceItem)deed).Resource != CraftResource.None;
			var requireGemType = deed is IGemTypeItem && ((IGemTypeItem)deed).GemType != GemType.None;

			if (requireExceptional || requireResource || requireGemType)
				AddHtmlLocalized(75, 130, 200, 20, 1045140, 0x7FFF, false, false); // Special requirements to meet:

			int i = 0;
			const int baseY = 130;
			if (requireExceptional)
			{
				++i;
				AddHtmlLocalized(75, baseY + i * 24, 300, 20, 1045141, 0x7FFF, false, false); // All items must be exceptional.
			}

			if (requireResource)
			{
				++i;
				AddHtml(75, baseY + i * 24, 300, 20, "<basefont color=#FF0000>All items must be crafted with " + CraftResources.GetResourceName(((IResourceItem)deed).Resource), false, false);
			}

			if (requireGemType)
			{
				++i;
				AddHtml(75, baseY + i * 24, 300, 20, "<basefont color=#FF0000>All items must be crafted with " + ((IGemTypeItem)deed).GemType, false, false); // TODO: Better name
			}
		}

		public class InternalTarget : Target
		{
			private readonly IOrderContext m_Deed;

			public InternalTarget(IOrderContext order) : base(18, false, TargetFlags.None)
			{
				m_Deed = order;
			}

			protected override void OnTarget(Mobile from, object o)
			{
				if (o is Item && ((Item)o).IsChildOf(from.Backpack))
				{
					if (m_Deed.IsComplete)
					{
						from.SendLocalizedMessage(1045166); // The maximum amount of requested items have already been combined to this deed.
					}
					else if (!m_Deed.IsValid)
					{
						// Not valid
					}
					else
					{
						EndCombine(from, m_Deed, (Item)o);
					}
				}
				else
				{
					from.SendLocalizedMessage(1045158); // You must have the item in your backpack to target it.
				}
			}

			private void AddItem(Mobile from, IOrderContext order, Item targetItem)
			{
				if (targetItem.Stackable)
				{
					if (0 < targetItem.Amount)
					{
						var remaining = Math.Max(0, order.MaxAmount - order.CurrentAmount);
						if (targetItem.Amount < remaining)
						{
							order.CurrentAmount += targetItem.Amount;
							targetItem.Delete();
						}
						else
						{
							order.CurrentAmount += remaining;
							targetItem.Amount -= remaining;
						}
					}

					if (targetItem.Amount < 1)
						targetItem.Delete();
				}
				else
				{
					order.CurrentAmount++;
					targetItem.Delete();
				}

				from.SendLocalizedMessage(1045170); // The item has been combined with the deed.
			}

			private bool CanAddItem(Mobile from, IOrderContext order, Item item)
			{
				if (order.IsComplete) return false;

				var itemType = item.GetType();
				if (itemType != order.Type && !itemType.IsSubclassOf(order.Type))
				{
					from.SendLocalizedMessage(1045169); // The item is not in the request.
					return false;
				}

				if (order is IExceptionalItem)
				{
					if (((IExceptionalItem)order).RequireExceptional && !ItemUtilities.IsExceptional(item))
					{
						from.SendLocalizedMessage(1045167); // The item must be exceptional.
						return false;
					}
				}

				if (order is IResourceItem)
				{
					var resource = ((IResourceItem)order).Resource;
					if (resource >= CraftResource.DullCopper && resource <= CraftResource.Dwarven && item.Resource != resource)
					{
						from.SendLocalizedMessage(1045168); // The item is not made from the requested ore.
						return false;
					}

					if (resource >= CraftResource.HornedLeather && resource <= CraftResource.AlienLeather && item.Resource != resource)
					{
						from.SendLocalizedMessage(1049352); // The item is not made from the requested leather type.
						return false;
					}

					if (resource >= CraftResource.AshTree && resource <= CraftResource.ElvenTree && item.Resource != resource)
					{
						from.SendMessage("The item is not made from the requested wood type.");
						return false;
					}
				}

				if (order is IGemTypeItem)
				{
					var gemType = ((IGemTypeItem)order).GemType;
					if ((item is BaseTrinket) == false || ((BaseTrinket)item).GemType != gemType)
					{
						from.SendMessage("The item does not have the requested gem type.");

						return false;
					}
				}

				return true;
			}

			private void EndCombine(Mobile from, IOrderContext order, Item targetItem)
			{
				if (targetItem is BaseContainer)
				{
					foreach (var item in ((BaseContainer)targetItem).Items.ToList())
					{
						if (CanAddItem(from, order, item))
						{
							AddItem(from, order, item);
						}
					}
					;
				}
				else
				{
					if (CanAddItem(from, order, targetItem))
					{
						AddItem(from, order, targetItem);
					}
				}

				from.CloseGump(typeof(OrderGump));
				from.SendGump(new OrderGump(from, order));

				if (order.IsComplete)
				{
					from.PlaySound(0x5B6); // public sound
					TextDefinition.SendMessageTo(from, "Return to the shoppe to claim your reward.", 0x23);
				}
				else
					BeginCombine(from, order);
			}
		}
	}
}
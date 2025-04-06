using Server.Gumps;
using Server.Items;
using Server.Network;
using Server.Targeting;
using Server.Utilities;
using System;
using System.Globalization;

namespace Server.Engines.GlobalShoppe
{
    public class OrderGump : Gump
    {
        private readonly OrderContext m_Deed;
        private readonly Mobile m_From;

        public OrderGump(Mobile from, OrderContext deed) : base(25, 25)
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

            if (deed.RequireExceptional || deed.Resource != CraftResource.None)
                AddHtmlLocalized(75, 130, 200, 20, 1045140, 0x7FFF, false, false); // Special requirements to meet:

            if (deed.RequireExceptional)
                AddHtmlLocalized(75, 154, 300, 20, 1045141, 0x7FFF, false, false); // All items must be exceptional.

            if (deed.Resource != CraftResource.None)
                AddHtml(75, deed.RequireExceptional ? 178 : 154, 300, 20, "<basefont color=#FF0000>All items must be crafted with " + CraftResources.GetResourceName(deed.Resource), false, false);

            if (!deed.IsComplete)
            {
                AddButton(125, 202, 4005, 4007, 2, GumpButtonType.Reply, 0);
                TextDefinition.AddHtmlText(this, 160, 205, 300, 20, "Add requested item", HtmlColors.WHITE);
            }

            AddButton(125, 226, 4005, 4007, 1, GumpButtonType.Reply, 0);
            AddHtmlLocalized(160, 229, 120, 20, 1011441, 0x7FFF, false, false); // EXIT
        }

        public static void BeginCombine(Mobile from, OrderContext order)
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

        public class InternalTarget : Target
        {
            private readonly OrderContext m_Deed;

            public InternalTarget(OrderContext order) : base(18, false, TargetFlags.None)
            {
                m_Deed = order;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is Item && ((Item)o).IsChildOf(from.Backpack))
                {
                    Type objectType = o.GetType();

                    if (m_Deed.IsComplete)
                    {
                        from.SendLocalizedMessage(1045166); // The maximum amount of requested items have already been combined to this deed.
                    }
                    else if (!m_Deed.IsValid)
                    {
                        // Not valid
                    }
                    else if (objectType != m_Deed.Type && !objectType.IsSubclassOf(m_Deed.Type))
                    {
                        from.SendLocalizedMessage(1045169); // The item is not in the request.
                    }
                    else
                    {
                        var targetItem = (Item)o;
                        var resource = m_Deed.Resource;
                        if (resource >= CraftResource.DullCopper && resource <= CraftResource.Dwarven && targetItem.Resource != resource)
                        {
                            from.SendLocalizedMessage(1045168); // The item is not made from the requested ore.
                        }
                        else if (resource >= CraftResource.HornedLeather && resource <= CraftResource.AlienLeather && targetItem.Resource != resource)
                        {
                            from.SendLocalizedMessage(1049352); // The item is not made from the requested leather type.
                        }
                        else if (resource >= CraftResource.AshTree && resource <= CraftResource.ElvenTree && targetItem.Resource != resource)
                        {
                            from.SendMessage("The item is not made from the requested wood type.");
                        }
                        else
                        {
                            if (m_Deed.RequireExceptional && !ItemUtilities.IsExceptional(targetItem))
                            {
                                from.SendLocalizedMessage(1045167); // The item must be exceptional.
                            }
                            else
                            {
                                targetItem.Delete();
                                m_Deed.CurrentAmount++;

                                from.SendLocalizedMessage(1045170); // The item has been combined with the deed.

                                from.CloseGump(typeof(OrderGump));
                                from.SendGump(new OrderGump(from, m_Deed));

                                if (m_Deed.IsComplete)
                                {
                                    from.PlaySound(0x5B6); // public sound
                                    TextDefinition.SendMessageTo(from, "Return to the shoppe to claim your reward.", 0x23);
                                }
                                else
                                    BeginCombine(from, m_Deed);
                            }
                        }
                    }
                }
                else
                {
                    from.SendLocalizedMessage(1045158); // You must have the item in your backpack to target it.
                }
            }
        }
    }
}
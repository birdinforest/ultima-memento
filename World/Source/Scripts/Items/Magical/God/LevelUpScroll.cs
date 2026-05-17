using System;
using Server.Network;
using Server.Prompts;
using Server.Mobiles;
using Server.Misc;
using Server.Items;
using Server.Gumps;
using Server.Targeting;
using Server.Targets;
using Server.Localization;

namespace Server.Items
{
	public class LevelUpScroll : Item
	{
        private int m_Value;
        private bool m_BlacksmithValidated;

        [CommandProperty(AccessLevel.GameMaster)]
        public int Value
        {
            get
            {
                return m_Value;
            }
        }

        [CommandProperty(AccessLevel.Administrator)]
        public bool BlacksmithValidated
        {
            get { return m_BlacksmithValidated; }
            set { m_BlacksmithValidated = value; InvalidateProperties(); }
        }

		[Constructable]
        public LevelUpScroll( int value ): base(0x573C)
		{
			Weight = 1.0;
			Name = "Enhancement Rune";
            m_Value = value;
		}

        public override void AddNameProperty(ObjectPropertyList list)
        {
            if ( BuildingPropertyListLocale != null )
            {
                if (m_Value == 5.0)
                    AddLocalizedProperty(list, "god.level.up.rune.wondrous", m_Value);
                else if (m_Value == 10.0)
                    AddLocalizedProperty(list, "god.level.up.rune.exalted", m_Value);
                else if (m_Value == 15.0)
                    AddLocalizedProperty(list, "god.level.up.rune.mythical", m_Value);
                else if (m_Value == 20.0)
                    AddLocalizedProperty(list, "god.level.up.rune.legendary", m_Value);
                else
                    AddLocalizedProperty(list, "god.level.up.rune.normal", m_Value);
            }
            else
            {
                if (m_Value == 5.0)
                    list.Add("a wondrous rune of enhancing (+{0} max levels)", m_Value);
                else if (m_Value == 10.0)
                    list.Add("an exalted rune of enhancing (+{0} max levels)", m_Value);
                else if (m_Value == 15.0)
                    list.Add("a mythical rune of enhancing (+{0} max levels)", m_Value);
                else if (m_Value == 20.0)
                    list.Add("a legendary rune of enhancing (+{0} max levels)", m_Value);
                else
                    list.Add("a rune of enhancing (+{0} max levels)", m_Value);
            }
        }

        public override void OnSingleClick(Mobile from)
        {
            if (m_Value == 5.0)
                base.LabelTo(from, "a wondrous rune of enhancing (+{0} max levels)", m_Value);
            else if (m_Value == 10.0)
                base.LabelTo(from, "an exalted rune of enhancing (+{0} max levels)", m_Value);
            else if (m_Value == 15.0)
                base.LabelTo(from, "a mythical rune of enhancing (+{0} max levels)", m_Value);
            else if (m_Value == 20.0)
                base.LabelTo(from, "a legendary rune of enhancing (+{0} max levels)", m_Value);
            else
                base.LabelTo(from, "a rune of enhancing (+{0} max levels)", m_Value);
        }

		public override void AddNameProperties( ObjectPropertyList list )
		{
            string BlacksmithMsg;
            bool IsBlacksmithOnly;

			base.AddNameProperties( list );

            IsBlacksmithOnly = LevelItems.BlacksmithOnly;

            if (IsBlacksmithOnly)
            {
                if (!m_BlacksmithValidated)
                    BlacksmithMsg = "(Must be validated by player with " + LevelItems.BlacksmithSkillRequired + "+ Blacksmithy skill)";
                else
                    BlacksmithMsg = "(Blacksmith Validated)";
            }
            else
                BlacksmithMsg = "";

            if ( BuildingPropertyListLocale != null )
            {
                if ( IsBlacksmithOnly )
                {
                    if ( !m_BlacksmithValidated )
                    {
                        AddLocalizedProperty( list, "god.legendary.only" );
                        AddLocalizedProperty( list, "god.must.validate", LevelItems.BlacksmithSkillRequired );
                    }
                    else
                    {
                        AddLocalizedProperty( list, "god.legendary.only" );
                        AddLocalizedProperty( list, "god.validated" );
                    }
                }
                else
                    AddLocalizedProperty( list, "god.legendary.only" );
            }
            else
                list.Add(1060847, "Legendary Artefacts Only\t {0}", BlacksmithMsg);
		}

		public LevelUpScroll( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version

            //Version 0
            writer.Write(m_BlacksmithValidated);
            writer.Write((int)m_Value);
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    {
                        m_BlacksmithValidated = reader.ReadBool();
                        m_Value = reader.ReadInt();
                        break;
                    }
            }
		}

		public override void OnDoubleClick( Mobile from )
		{
            bool IsBlacksmithOnly;

            IsBlacksmithOnly = LevelItems.BlacksmithOnly;

			if ( !IsChildOf( from.Backpack ) )
			{
				from.SendLocalizedMessage( 1042001 ); // That must be in your pack for you to use it.
				return;
			}
			else
			{
                if (IsBlacksmithOnly)
                {
                    if (m_BlacksmithValidated || (from.Skills[SkillName.Blacksmith].Value >= LevelItems.BlacksmithSkillRequired))
                    {
                        from.SendMessage(StringCatalog.ResolveByKey(from.Account, "god.msg.scroll.which.item"));
                        from.Target = new LevelItemTarget(this); // Call our target
                    }
                    else
                    {
                        from.SendMessage(StringCatalog.ResolveFormatByKey(from.Account, "god.msg.scroll.blacksmith.target", LevelItems.BlacksmithSkillRequired));
                        from.Target = new BlacksmithTarget(this); // Call our target
                    }
                }
                else
                {
				    from.SendMessage(StringCatalog.ResolveByKey(from.Account, "god.msg.scroll.which.artefact"));
				    from.Target = new LevelItemTarget( this ); // Call our target
                }
			}
		}

		public class LevelItemTarget : Target
		{
			private LevelUpScroll m_Scroll;

            public LevelItemTarget(LevelUpScroll scroll): base(-1, false, TargetFlags.None)
			{
				this.m_Scroll = scroll;
			}

			protected override void OnTarget( Mobile from, object target )
			{
				if ( target is Mobile )
				{
					from.SendMessage(StringCatalog.ResolveByKey(from.Account, "god.msg.scroll.cannot.enhance"));
				}
                else if (target is Item)
                {
                    Item item = (Item)target;

                    if (item.RootParent != from || !item.IsChildOf(from.Backpack)) // Make sure its in their pack or they are wearing it
                    {
                        from.SendMessage(StringCatalog.ResolveByKey(from.Account, "god.msg.scroll.must.in.pack"));
                    }
                    else
                    {
                        if (target is ILevelable)
                        {
                            ILevelable b = (ILevelable)target;

                            if ((b.MaxLevel + m_Scroll.Value) > LevelItems.MaxLevelsCap)
                            {
                                from.SendMessage(StringCatalog.ResolveByKey(from.Account, "god.msg.scroll.level.too.high"));
                            }
                            else
                            {
                                b.MaxLevel += m_Scroll.Value;
                                from.SendMessage(StringCatalog.ResolveFormatByKey(from.Account, "god.msg.scroll.enhanced.by", m_Scroll.Value));
                                m_Scroll.Delete();

                            }
                        }
                        else
                        {
                            from.SendMessage(StringCatalog.ResolveByKey(from.Account, "god.msg.scroll.cannot.enhance"));
                        }
                    }
                }
                else
                {
                    from.SendMessage(StringCatalog.ResolveByKey(from.Account, "god.msg.scroll.cannot.enhance"));
                }
			}
		}

        private class BlacksmithTarget : Target
        {
            private LevelUpScroll m_Scroll;

            public BlacksmithTarget(LevelUpScroll scroll) : base(-1, false, TargetFlags.None)
            {
                this.m_Scroll = scroll;
            }

            protected override void OnTarget(Mobile from, object target)
            {
                if (target is PlayerMobile)
                {
                    //check if bs skill is high enough
                    Mobile smith = (Mobile)target;
                    if (smith.Skills[SkillName.Blacksmith].Value < LevelItems.BlacksmithSkillRequired)
                    {
                        from.SendMessage(StringCatalog.ResolveByKey(from.Account, "god.msg.scroll.smith.skill.low"));
                    }
                    else
                    {
                        from.SendMessage(StringCatalog.ResolveByKey(from.Account, "god.msg.scroll.smith.ok"));
                        from.SendGump(new AwaitingSmithApprovalGump(m_Scroll, from));
                        smith.SendGump(new LevelUpAcceptGump(m_Scroll, from, smith));	
                    }
                }
                else
                {
                    from.SendMessage(StringCatalog.ResolveByKey(from.Account, "god.msg.scroll.not.smith"));
                }
            }
        }
	}
}
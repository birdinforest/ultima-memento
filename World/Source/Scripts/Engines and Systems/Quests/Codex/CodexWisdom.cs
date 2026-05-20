using System;
using Server;
using Server.Network;
using Server.Mobiles;
using Server.Items;
using Server.Gumps;
using Server.Misc;
using Server.Regions;
using System.Collections;
using Server.Accounting;
using Server.Localization;

namespace Server
{
    public class CodexGump : Gump
    {
        private CodexWisdom m_CodexWisdom;

        public CodexGump(CodexWisdom codex, string warn, Mobile from ): base(50, 50)
        {
			string color = "#6cb89a";
			int display = 60;
			int line = 0;

            m_CodexWisdom = codex;

			this.Closable=true;
			this.Disposable=true;
			this.Dragable=true;
			this.Resizable=false;

			AddPage(0);

			string title = StringCatalog.ResolveByKey(from.Account, "quest.choose_two_skills_to_study");
			if ( m_CodexWisdom.SkillFirst > 0 && m_CodexWisdom.SkillSecond > 0 ){ title = StringCatalog.ResolveByKey(from.Account, "quest.choose_a_skill_to_forget_so_you_can_learn_a_different_one"); }
			else if ( m_CodexWisdom.SkillFirst == 0 && m_CodexWisdom.SkillSecond > 0 ){ title = StringCatalog.ResolveByKey(from.Account, "quest.choose_one_skill_to_study_or_one_to_forget_so_you_can_learn_a_different_one"); }
			else if ( m_CodexWisdom.SkillFirst > 0 && m_CodexWisdom.SkillSecond == 0 ){ title = StringCatalog.ResolveByKey(from.Account, "quest.choose_one_skill_to_study_or_one_to_forget_so_you_can_learn_a_different_one"); }

			if ( warn == null ){ warn = StringCatalog.ResolveByKey(from.Account, "quest.if_you_change_your_skills_and_leave_the_chamber_c_the_lenses_will_vanish_and_you_may_need_to_find_ot"); }

			AddImage(0, 0, 7029, Server.Misc.PlayerSettings.GetGumpHue( from ));
			AddButton(962, 11, 4017, 4017, 0, GumpButtonType.Reply, 0);
			string codexTitle = StringCatalog.ResolveByKey(from.Account, "quest.codex_of_ultimate_wisdom");
			AddHtml( 12, 12, 727, 20, @"<BODY><BASEFONT Color=" + color + ">" + codexTitle + "</BASEFONT></BODY>", (bool)false, (bool)false);
			AddHtml( 12, 46, 976, 20, @"<BODY><BASEFONT Color=" + color + ">" + title + "</BASEFONT></BODY>", (bool)false, (bool)false);
			AddHtml( 12, 80, 976, 20, @"<BODY><BASEFONT Color=" + color + ">" + warn + "</BASEFONT></BODY>", (bool)false, (bool)false);

			int Skill1 = m_CodexWisdom.SkillFirst;
			int Skill2 = m_CodexWisdom.SkillSecond;

			while ( display > 0 )
			{
				display--;
				line++;

				GetLine( from, line, Skill1, Skill2 );
			}
		}

		public void GetLine( Mobile from, int val, int skill1, int skill2 )
		{
			string color = "#6cb89a";
			int skl = 0;
			string txt = "";
			int btn = 3609;

			if ( val == 1 ){ skl = 1; txt = StringCatalog.ResolveByKey(from.Account, "quest.alchemy"); }
			else if ( val == 2 ){ skl = 2; txt = StringCatalog.ResolveByKey(from.Account, "quest.anatomy"); }
			else if ( val == 3 ){ skl = 6; txt = StringCatalog.ResolveByKey(from.Account, "quest.arms_lore"); }
			else if ( val == 4 ){ skl = 7; txt = StringCatalog.ResolveByKey(from.Account, "quest.begging"); }
			else if ( val == 5 ){ skl = 8; txt = StringCatalog.ResolveByKey(from.Account, "quest.blacksmithing"); }
			else if ( val == 6 ){ skl = 30; txt = StringCatalog.Resolve( from.Account, "Bludgeoning" ); }
			else if ( val == 7 ){ skl = 20; txt = StringCatalog.ResolveByKey(from.Account, "quest.bowcrafting"); }
			else if ( val == 8 ){ skl = 9; txt = StringCatalog.ResolveByKey(from.Account, "quest.bushido"); }
			else if ( val == 9 ){ skl = 10; txt = StringCatalog.ResolveByKey(from.Account, "quest.camping"); }
			else if ( val == 10 ){ skl = 11; txt = StringCatalog.ResolveByKey(from.Account, "quest.carpentry"); }
			else if ( val == 11 ){ skl = 12; txt = StringCatalog.ResolveByKey(from.Account, "quest.cartography"); }
			else if ( val == 12 ){ skl = 14; txt = StringCatalog.ResolveByKey(from.Account, "quest.cooking"); }
			else if ( val == 13 ){ skl = 16; txt = StringCatalog.ResolveByKey(from.Account, "quest.discordance"); }
			else if ( val == 14 ){ skl = 3; txt = StringCatalog.ResolveByKey(from.Account, "quest.druidism"); }
			else if ( val == 15 ){ skl = 55; txt = StringCatalog.ResolveByKey(from.Account, "quest.elementalism"); }
			else if ( val == 16 ){ skl = 18; txt = StringCatalog.Resolve( from.Account, "Fencing" ); }
			else if ( val == 17 ){ skl = 54; txt = StringCatalog.Resolve( from.Account, "Fist Fighting" ); }
			else if ( val == 18 ){ skl = 21; txt = StringCatalog.ResolveByKey(from.Account, "quest.focus"); }
			else if ( val == 19 ){ skl = 22; txt = StringCatalog.ResolveByKey(from.Account, "quest.forensics"); }
			else if ( val == 20 ){ skl = 23; txt = StringCatalog.ResolveByKey(from.Account, "quest.healing"); }
			else if ( val == 21 ){ skl = 24; txt = StringCatalog.ResolveByKey(from.Account, "quest.herding"); }
			else if ( val == 22 ){ skl = 25; txt = StringCatalog.ResolveByKey(from.Account, "quest.hiding"); }
			else if ( val == 23 ){ skl = 26; txt = StringCatalog.ResolveByKey(from.Account, "quest.inscription"); }
			else if ( val == 24 ){ skl = 13; txt = StringCatalog.ResolveByKey(from.Account, "quest.knightship"); }
			else if ( val == 25 ){ skl = 28; txt = StringCatalog.ResolveByKey(from.Account, "quest.lockpicking"); }
			else if ( val == 26 ){ skl = 29; txt = StringCatalog.ResolveByKey(from.Account, "quest.lumberjacking"); }
			else if ( val == 27 ){ skl = 31; txt = StringCatalog.ResolveByKey(from.Account, "quest.magery"); }
			else if ( val == 28 ){ skl = 32; txt = StringCatalog.ResolveByKey(from.Account, "quest.magic_resistance"); }
			else if ( val == 29 ){ skl = 5; txt = StringCatalog.Resolve( from.Account, "Marksmanship" ); }
			else if ( val == 30 ){ skl = 33; txt = StringCatalog.ResolveByKey(from.Account, "quest.meditation"); }
			else if ( val == 31 ){ skl = 27; txt = StringCatalog.ResolveByKey(from.Account, "quest.mercantile"); }
			else if ( val == 32 ){ skl = 34; txt = StringCatalog.ResolveByKey(from.Account, "quest.mining"); }
			else if ( val == 33 ){ skl = 35; txt = StringCatalog.ResolveByKey(from.Account, "quest.musicianship"); }
			else if ( val == 34 ){ skl = 36; txt = StringCatalog.ResolveByKey(from.Account, "quest.necromancy"); }
			else if ( val == 35 ){ skl = 37; txt = StringCatalog.ResolveByKey(from.Account, "quest.ninjitsu"); }
			else if ( val == 36 ){ skl = 38; txt = StringCatalog.ResolveByKey(from.Account, "quest.parrying"); }
			else if ( val == 37 ){ skl = 39; txt = StringCatalog.ResolveByKey(from.Account, "quest.peacemaking"); }
			else if ( val == 38 ){ skl = 40; txt = StringCatalog.ResolveByKey(from.Account, "quest.poisoning"); }
			else if ( val == 39 ){ skl = 41; txt = StringCatalog.Resolve( from.Account, "Provocation" ); }
			else if ( val == 40 ){ skl = 17; txt = StringCatalog.ResolveByKey(from.Account, "quest.psychology"); }
			else if ( val == 41 ){ skl = 42; txt = StringCatalog.ResolveByKey(from.Account, "quest.remove_trap"); }
			else if ( val == 42 ){ skl = 19; txt = StringCatalog.ResolveByKey(from.Account, "quest.seafaring"); }
			else if ( val == 43 ){ skl = 15; txt = StringCatalog.Resolve( from.Account, "Searching" ); }
			else if ( val == 44 ){ skl = 43; txt = StringCatalog.ResolveByKey(from.Account, "quest.snooping"); }
			else if ( val == 45 ){ skl = 44; txt = StringCatalog.ResolveByKey(from.Account, "quest.spiritualism"); }
			else if ( val == 46 ){ skl = 45; txt = StringCatalog.ResolveByKey(from.Account, "quest.stealing"); }
			else if ( val == 47 ){ skl = 46; txt = StringCatalog.ResolveByKey(from.Account, "quest.stealth"); }
			else if ( val == 48 ){ skl = 47; txt = StringCatalog.Resolve( from.Account, "Swordsmanship" ); }
			else if ( val == 49 ){ skl = 48; txt = StringCatalog.ResolveByKey(from.Account, "quest.tactics"); }
			else if ( val == 50 ){ skl = 49; txt = StringCatalog.ResolveByKey(from.Account, "quest.tailoring"); }
			else if ( val == 51 ){ skl = 4; txt = StringCatalog.ResolveByKey(from.Account, "quest.taming"); }
			else if ( val == 52 ){ skl = 50; txt = StringCatalog.ResolveByKey(from.Account, "quest.tasting"); }
			else if ( val == 53 ){ skl = 51; txt = StringCatalog.ResolveByKey(from.Account, "quest.tinkering"); }
			else if ( val == 54 ){ skl = 52; txt = StringCatalog.ResolveByKey(from.Account, "quest.tracking"); }
			else if ( val == 55 ){ skl = 53; txt = StringCatalog.ResolveByKey(from.Account, "quest.veterinary"); }

			if ( txt != "" )
			{
				int x; int y;

				if ( val < 24 ){ x = 31; y = 25 + (val*28); }
				else if ( val < 47 ){ x = 371; y = 25 + ((val-23)*28); }
				else { x = 706; y = 25 + ((val-46)*28); }

				if ( skill1 == skl || skill2 == skl ){ btn = 4018; } else { btn = 3609; }

				AddButton(x, y+77, btn, btn, skl, GumpButtonType.Reply, 0);
				AddHtml( x+50, y+77, 252, 20, @"<BODY><BASEFONT Color=" + color + ">" + txt + "</BASEFONT></BODY>", (bool)false, (bool)false);
			}
		}

		public override void OnResponse( NetState sender, RelayInfo info )
		{
			Mobile from = sender.Mobile;

			from.SendSound( 0x55 );

			int storeSkillFirst = m_CodexWisdom.SkillFirst;
			int storeSkillSecond = m_CodexWisdom.SkillSecond;
			string warn = null;

			if ( info.ButtonID > 0 )
			{
				bool updateBook = true;
				if ( info.ButtonID == m_CodexWisdom.SkillFirst ){ m_CodexWisdom.SkillFirst = 0; }
				else if ( info.ButtonID == m_CodexWisdom.SkillSecond ){ m_CodexWisdom.SkillSecond = 0; }
				else if ( m_CodexWisdom.SkillFirst == 0 ){ m_CodexWisdom.SkillFirst = info.ButtonID; }
				else if ( m_CodexWisdom.SkillSecond == 0 ){ m_CodexWisdom.SkillSecond = info.ButtonID; }
				else { updateBook = false; }

				if ( ( m_CodexWisdom.SkillFirst == 55 || m_CodexWisdom.SkillSecond == 55 ) && ( m_CodexWisdom.SkillFirst == 31 || m_CodexWisdom.SkillSecond == 31 ) )
				{
					updateBook = false;
					warn = StringCatalog.ResolveByKey(from.Account, "quest.you_cannot_have_both_magery_and_elementalism_studied_ex");
					from.SendMessage( warn );
					m_CodexWisdom.SkillFirst = storeSkillFirst;
					m_CodexWisdom.SkillSecond = storeSkillSecond;
				}
				if ( ( m_CodexWisdom.SkillFirst == 55 || m_CodexWisdom.SkillSecond == 55 ) && ( m_CodexWisdom.SkillFirst == 36 || m_CodexWisdom.SkillSecond == 36 ) )
				{
					updateBook = false;
					warn = StringCatalog.ResolveByKey(from.Account, "quest.you_cannot_have_both_necromancy_and_elementalism_studied_ex");
					from.SendMessage( warn );
					m_CodexWisdom.SkillFirst = storeSkillFirst;
					m_CodexWisdom.SkillSecond = storeSkillSecond;
				}

				if ( updateBook ){ Server.CodexWisdom.UpdateCodex( m_CodexWisdom ); }

				from.SendGump( new CodexGump( m_CodexWisdom, warn, from ) );
			}
		}
    }

    public class LenseGump : Gump
    {
        private CodexWisdom m_CodexWisdom;

        public LenseGump(CodexWisdom codex, int status, Mobile from): base(50, 50)
        {
			string color = "#acacac";
			string hue = "#ffffff";
			from.SendSound( 0x55 );

            m_CodexWisdom = codex;

            this.Closable=true;
			this.Disposable=true;
			this.Dragable=true;
			this.Resizable=false;

			AddPage(0);


			string phrase = StringCatalog.Resolve( from.Account, "You will need the Concave and Convex Lenses to read the Codex." );
			if ( status > 0 ){ phrase = StringCatalog.Resolve( from.Account, "This can only be studied within the Chamber of the Codex." ); }


			AddImage(0, 0, 7039, Server.Misc.PlayerSettings.GetGumpHue( from ));
			AddButton(217, 11, 4017, 4017, 0, GumpButtonType.Reply, 0);

			string lenseTitle = StringCatalog.ResolveByKey(from.Account, "quest.codex_of_wisdom");
			AddHtml( 12, 12, 200, 20, @"<BODY><BASEFONT Color=" + color + ">" + lenseTitle + "</BASEFONT></BODY>", (bool)false, (bool)false);

			AddHtml( 12, 50, 227, 71, @"<BODY><BASEFONT Color=" + hue + ">" + phrase + "</BASEFONT></BODY>", (bool)false, (bool)false);


			AddItem(212, 161, 4643);
			AddItem(1, 254, 4643);


			if ( m_CodexWisdom.HasConcaveLense > 0 )
			{
				AddHtml( 12, 154, 200, 71, @"<BODY><BASEFONT Color=" + color + ">" + StringCatalog.ResolveByKey(from.Account, "quest.you_have_the_concave_lense_dot") + "</BASEFONT></BODY>", (bool)false, (bool)false);
				AddItem(207, 157, 1517);
			}
			else
			{
				AddHtml( 12, 154, 200, 71, @"<BODY><BASEFONT Color=" + color + ">" + StringCatalog.ResolveFormatByKey(from.Account, "quest.naxatilor_n0_dot", Server.Items.VortexCube.GargoyleLocation( from, "Naxatilor" ) ) + "</BASEFONT></BODY>", (bool)false, (bool)false);
			}
			if ( m_CodexWisdom.HasConvexLense > 0 )
			{
				AddHtml( 45, 251, 200, 71, @"<BODY><BASEFONT Color=" + hue + ">" + StringCatalog.ResolveByKey(from.Account, "quest.you_have_the_convex_lense_dot") + "</BASEFONT></BODY>", (bool)false, (bool)false);
				AddItem(-4, 251, 1518);
			}
			else
			{
				AddHtml( 45, 251, 200, 71, @"<BODY><BASEFONT Color=" + hue + ">" + StringCatalog.ResolveFormatByKey(from.Account, "quest.lor_wis_lem_n0_dot", Server.Items.VortexCube.GargoyleLocation( from, "Lor-wis-lem" ) ) + "</BASEFONT></BODY>", (bool)false, (bool)false);
			}
		}

		public override void OnResponse( NetState sender, RelayInfo info )
		{
			Mobile from = sender.Mobile;
			from.SendSound( 0x55 );
		}
    }

    public class CodexWisdom : TrinketTalisman
    {
		public Mobile CodexOwner;
		[CommandProperty( AccessLevel.GameMaster )]
		public Mobile Codex_Owner { get{ return CodexOwner; } set{ CodexOwner = value; } }

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		public int HasConvexLense;
		[CommandProperty(AccessLevel.Owner)]
		public int Has_ConvexLense { get { return HasConvexLense; } set { HasConvexLense = value; InvalidateProperties(); } }

		public int HasConcaveLense;
		[CommandProperty(AccessLevel.Owner)]
		public int Has_ConcaveLense { get { return HasConcaveLense; } set { HasConcaveLense = value; InvalidateProperties(); } }

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		public int SkillFirst;
		[CommandProperty(AccessLevel.Owner)]
		public int Skill_First { get { return SkillFirst; } set { SkillFirst = value; InvalidateProperties(); } }

		public int SkillSecond;
		[CommandProperty(AccessLevel.Owner)]
		public int Skill_Second { get { return SkillSecond; } set { SkillSecond = value; InvalidateProperties(); } }

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		public int PreviousFirst;
		[CommandProperty(AccessLevel.Owner)]
		public int Previous_First { get { return PreviousFirst; } set { PreviousFirst = value; InvalidateProperties(); } }

		public int PreviousSecond;
		[CommandProperty(AccessLevel.Owner)]
		public int Previous_Second { get { return PreviousSecond; } set { PreviousSecond = value; InvalidateProperties(); } }

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		public override bool DisplayLootType{ get{ return false; } }

        [Constructable]
        public CodexWisdom()
        {
			ItemID = 0x081C;
            Weight = 1.0;
			Hue = 0;
            Name = "Codex of Ultimate Wisdom";
            LootType = LootType.Blessed;

			Attributes.BonusInt = 25;
        }

        public override void AddNameProperties(ObjectPropertyList list)
		{
            base.AddNameProperties(list);
			if ( CodexOwner != null ){ list.Add( 1049644, "Belongs to " + CodexOwner.Name + "" ); }
        }

		public override bool OnEquip( Mobile from )
		{
			if ( CodexOwner != from )
			{
				from.SendMessage( StringCatalog.Resolve( from.Account, "This Codex does not belong to you!" ) );
				return false;
			}

			return base.OnEquip( from );
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( from is PlayerMobile )
			{
				Region reg = Region.Find( from.Location, from.Map );
				Item tome = from.FindItemOnLayer( Layer.Trinket );

				if ( !IsChildOf( from.Backpack ) && tome != this )
				{
					from.SendLocalizedMessage( 1060640 ); // The item must be in your backpack to use it.
				}
				else if ( CodexOwner != from )
				{
					from.SendMessage( StringCatalog.Resolve( from.Account, "This Codex does not belong to you so it vanishes!" ) );
					bool remove = true;
					foreach ( Account a in Accounts.GetAccounts() )
					{
						if (a == null)
							break;

						int index = 0;

						for (int i = 0; i < a.Length; ++i)
						{
							Mobile m = a[i];

							if (m == null)
								continue;

							if ( m == CodexOwner )
							{
								m.AddToBackpack( this );
								remove = false;
							}

							++index;
						}
					}
					if ( remove )
					{
						this.Delete();
					}
				}
				else if ( HasConvexLense == 0 || HasConcaveLense == 0 )
				{
					from.CloseGump( typeof( LenseGump ) );
					from.SendGump( new LenseGump( this, 0, from ) );
				}
				else if ( !reg.IsPartOf( "the Chamber of the Codex" ) )
				{
					from.CloseGump( typeof( LenseGump ) );
					from.SendGump( new LenseGump( this, 1, from ) );
				}
				else
				{
					from.CloseGump( typeof( CodexGump ) );
					from.SendGump( new CodexGump( this, null, from ) );
				}
			}
		}

		public static void UpdateCodex( CodexWisdom book )
		{
			if ( book.SkillFirst > 0 ){		book.SkillBonuses.SetValues(0, GetCodexSkill( book.SkillFirst ), 100); } else { book.SkillBonuses.SetValues(0, SkillName.Alchemy, 0); }
			if ( book.SkillSecond > 0 ){	book.SkillBonuses.SetValues(1, GetCodexSkill( book.SkillSecond ), 100); } else { book.SkillBonuses.SetValues(1, SkillName.Alchemy, 0); }

			book.InvalidateProperties();
		}

		private static SkillName GetCodexSkill( int learn )
		{
			SkillName skill = SkillName.Alchemy;

			if ( learn > 0 )
			{
				if ( learn == 1 ){ skill = SkillName.Alchemy; }
				else if ( learn == 2 ){ skill = SkillName.Anatomy; }
				else if ( learn == 3 ){ skill = SkillName.Druidism; }
				else if ( learn == 4 ){ skill = SkillName.Taming; }
				else if ( learn == 5 ){ skill = SkillName.Marksmanship; }
				else if ( learn == 6 ){ skill = SkillName.ArmsLore; }
				else if ( learn == 7 ){ skill = SkillName.Begging; }
				else if ( learn == 8 ){ skill = SkillName.Blacksmith; }
				else if ( learn == 9 ){ skill = SkillName.Bushido; }
				else if ( learn == 10 ){ skill = SkillName.Camping; }
				else if ( learn == 11 ){ skill = SkillName.Carpentry; }
				else if ( learn == 12 ){ skill = SkillName.Cartography; }
				else if ( learn == 13 ){ skill = SkillName.Knightship; }
				else if ( learn == 14 ){ skill = SkillName.Cooking; }
				else if ( learn == 15 ){ skill = SkillName.Searching; }
				else if ( learn == 16 ){ skill = SkillName.Discordance; }
				else if ( learn == 17 ){ skill = SkillName.Psychology; }
				else if ( learn == 18 ){ skill = SkillName.Fencing; }
				else if ( learn == 19 ){ skill = SkillName.Seafaring; }
				else if ( learn == 20 ){ skill = SkillName.Bowcraft; }
				else if ( learn == 21 ){ skill = SkillName.Focus; }
				else if ( learn == 22 ){ skill = SkillName.Forensics; }
				else if ( learn == 23 ){ skill = SkillName.Healing; }
				else if ( learn == 24 ){ skill = SkillName.Herding; }
				else if ( learn == 25 ){ skill = SkillName.Hiding; }
				else if ( learn == 26 ){ skill = SkillName.Inscribe; }
				else if ( learn == 27 ){ skill = SkillName.Mercantile; }
				else if ( learn == 28 ){ skill = SkillName.Lockpicking; }
				else if ( learn == 29 ){ skill = SkillName.Lumberjacking; }
				else if ( learn == 30 ){ skill = SkillName.Bludgeoning; }
				else if ( learn == 31 ){ skill = SkillName.Magery; }
				else if ( learn == 32 ){ skill = SkillName.MagicResist; }
				else if ( learn == 33 ){ skill = SkillName.Meditation; }
				else if ( learn == 34 ){ skill = SkillName.Mining; }
				else if ( learn == 35 ){ skill = SkillName.Musicianship; }
				else if ( learn == 36 ){ skill = SkillName.Necromancy; }
				else if ( learn == 37 ){ skill = SkillName.Ninjitsu; }
				else if ( learn == 38 ){ skill = SkillName.Parry; }
				else if ( learn == 39 ){ skill = SkillName.Peacemaking; }
				else if ( learn == 40 ){ skill = SkillName.Poisoning; }
				else if ( learn == 41 ){ skill = SkillName.Provocation; }
				else if ( learn == 42 ){ skill = SkillName.RemoveTrap; }
				else if ( learn == 43 ){ skill = SkillName.Snooping; }
				else if ( learn == 44 ){ skill = SkillName.Spiritualism; }
				else if ( learn == 45 ){ skill = SkillName.Stealing; }
				else if ( learn == 46 ){ skill = SkillName.Stealth; }
				else if ( learn == 47 ){ skill = SkillName.Swords; }
				else if ( learn == 48 ){ skill = SkillName.Tactics; }
				else if ( learn == 49 ){ skill = SkillName.Tailoring; }
				else if ( learn == 50 ){ skill = SkillName.Tasting; }
				else if ( learn == 51 ){ skill = SkillName.Tinkering; }
				else if ( learn == 52 ){ skill = SkillName.Tracking; }
				else if ( learn == 53 ){ skill = SkillName.Veterinary; }
				else if ( learn == 54 ){ skill = SkillName.FistFighting; }
				else if ( learn == 55 ){ skill = SkillName.Elementalism; }
			}

			return skill;
		}

        public CodexWisdom(Serial serial): base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)1); // version 

			writer.Write( (Mobile)CodexOwner);

            writer.Write( HasConvexLense );
            writer.Write( HasConcaveLense );

            writer.Write( SkillFirst );
            writer.Write( SkillSecond );

            writer.Write( PreviousFirst );
            writer.Write( PreviousSecond );
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

			CodexOwner = reader.ReadMobile();

			HasConvexLense = reader.ReadInt();
			HasConcaveLense = reader.ReadInt();

			SkillFirst = reader.ReadInt();
			SkillSecond = reader.ReadInt();

			PreviousFirst = reader.ReadInt();
			PreviousSecond = reader.ReadInt();

			ItemID = 0x081C;
            Weight = 1.0;
            Name = "Codex of Ultimate Wisdom";
            LootType = LootType.Blessed;
        }
    }
}
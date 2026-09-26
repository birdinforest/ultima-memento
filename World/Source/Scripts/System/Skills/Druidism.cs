using System;
using Server.Gumps;
using Server.Mobiles;
using Server.Targeting;
using Server.Misc;
using Server.Items;
using Server.Network;
using Server.Localization;
using Custom.Jerbal.Jako;

namespace Server.SkillHandlers
{
	public class Druidism
	{
		public static void Initialize()
		{
			SkillInfo.Table[(int)SkillName.Druidism].Callback = new SkillUseCallback( OnUse );
		}

		public static TimeSpan OnUse(Mobile m)
		{
			m.Target = new InternalTarget();

			m.SendLocalizedMessage( 500328 ); // What animal should I look at?

			return TimeSpan.FromSeconds( 1.0 );
		}

		private class InternalTarget : Target
		{
			public InternalTarget() : base( 8, false, TargetFlags.None )
			{
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				if ( !from.Alive )
				{
					from.SendLocalizedMessage( 500331 ); // The spirits of the dead are not the province of druidism.
				}
				else if ( targeted is HenchmanMonster || targeted is HenchmanWizard || targeted is HenchmanFighter || targeted is HenchmanArcher )
				{
					from.SendLocalizedMessage( 500329 ); // That's not an animal!
				}
				else if ( targeted is BaseCreature )
				{
					BaseCreature c = (BaseCreature)targeted;

					SlayerEntry skipTypeA = SlayerGroup.GetEntryByName( SlayerName.SlimyScourge );
					SlayerEntry skipTypeB = SlayerGroup.GetEntryByName( SlayerName.ElementalBan );
					SlayerEntry skipTypeC = SlayerGroup.GetEntryByName( SlayerName.Repond );
					SlayerEntry skipTypeD = SlayerGroup.GetEntryByName( SlayerName.Silver );
					SlayerEntry skipTypeE = SlayerGroup.GetEntryByName( SlayerName.GiantKiller );
					SlayerEntry skipTypeF = SlayerGroup.GetEntryByName( SlayerName.GolemDestruction );

					bool specialCreature = false;
					if ( targeted is RidingDragon || targeted is Daemonic || targeted is DrakkhenRed || targeted is DrakkhenBlack || targeted is SkeletonDragon || targeted is FrankenFighter )
					{
						if ( c.Controlled && c.ControlMaster == from )
							specialCreature = true;
					}

					if ( !c.IsDeadPet )
					{
						if ( specialCreature || ( !skipTypeA.Slays( c ) && !skipTypeB.Slays( c ) && !skipTypeC.Slays( c ) && !skipTypeD.Slays( c ) && !skipTypeE.Slays( c ) && !skipTypeF.Slays( c ) ) )
						{
							if ( c.ControlMaster == from )
							{
								from.CheckTargetSkillExplicit( SkillName.Druidism, c, 0.0, Math.Max( 25, c.MinTameSkill ) );

								from.CloseGump( typeof( DruidismGump ) );
								from.SendGump( new DruidismGump( from, c, 0 ) );
								from.SendSound( 0x0F9 );
							}
							else if ( (!c.Controlled || !c.Tamable) && from.Skills[SkillName.Druidism].Value < 100.0 )
							{
								from.SendLocalizedMessage( 1049674 ); // At your skill level, you can only lore tamed creatures.
							}
							else if ( !c.Tamable && from.Skills[SkillName.Druidism].Value < 110.0 )
							{
								from.SendLocalizedMessage( 1049675 ); // At your skill level, you can only lore tamed or tameable creatures.
							}
							else if ( !from.CheckTargetSkill( SkillName.Druidism, c, 0.0, 125.0 ) )
							{
								from.SendLocalizedMessage( 500334 ); // You can't think of anything you know offhand.
							}
							else
							{
								from.CloseGump( typeof( DruidismGump ) );
								from.SendGump( new DruidismGump( from, c, 0 ) );
								from.SendSound( 0x0F9 );
							}
						}
						else
						{
							from.SendLocalizedMessage( 500329 ); // That's not an animal!
						}
					}
					else
					{
						from.SendLocalizedMessage( 500331 ); // The spirits of the dead are not the province of druidism.
					}
				}
				else
				{
					from.SendLocalizedMessage( 500329 ); // That's not an animal!
				}
			}
		}
	}

	public class DruidismGump : Gump
	{
		private enum Actions
		{
			HitPoints = 1,
			StaminaPoints,
			ManaPoints,
			ResistPhysical,
			ResistFire,
			ResistCold,
			ResistPoison,
			ResistEnergy,
		}

		private readonly int m_Book; 
		private readonly int m_Color;
		private readonly int m_Source;
		private readonly BaseCreature m_Creature;
		private Mobile m_From;

		private static string FormatSkill( BaseCreature c, SkillName name )
		{
			Skill skill = c.Skills[name];

			if ( skill.Base < 10.0 )
				return "<div align=right>---</div>";

			return String.Format( "<div align=right>{0:F1}</div>", skill.Value );
		}

		private static string FormatCombat( BaseCreature from )
		{
			int c = 0;
			double skills = 0.0;

			double skill1 = from.Skills[SkillName.Marksmanship].Value;
				if ( skill1 > 10.0 ){ c++; skills = skills + skill1; }
			double skill2 = from.Skills[SkillName.Fencing].Value;
				if ( skill2 > 10.0 ){ c++; skills = skills + skill2; }
			double skill3 = from.Skills[SkillName.Bludgeoning].Value;
				if ( skill3 > 10.0 ){ c++; skills = skills + skill3; }
			double skill4 = from.Skills[SkillName.Swords].Value;
				if ( skill4 > 10.0 ){ c++; skills = skills + skill4; }
			double skill5 = from.Skills[SkillName.FistFighting].Value;
				if ( skill5 > 10.0 ){ c++; skills = skills + skill5; }

			if ( c == 0 )
			{
				return "<div align=right>---</div>";
			}
			else
			{
				skills = skills / c;
			}

			if ( skills > 125.0 )
				skills = 125.0;

			return String.Format( "<div align=right>{0:F1}</div>", skills );
		}

		private static string FormatFight( BaseCreature from )
		{
			int c = 0;
			double skills = 0.0;

			double skill1 = from.Skills[SkillName.Marksmanship].Value;
				if ( skill1 > 10.0 ){ c++; skills = skills + skill1; }
			double skill2 = from.Skills[SkillName.Fencing].Value;
				if ( skill2 > 10.0 ){ c++; skills = skills + skill2; }
			double skill3 = from.Skills[SkillName.Bludgeoning].Value;
				if ( skill3 > 10.0 ){ c++; skills = skills + skill3; }
			double skill4 = from.Skills[SkillName.Swords].Value;
				if ( skill4 > 10.0 ){ c++; skills = skills + skill4; }
			double skill5 = from.Skills[SkillName.FistFighting].Value;
				if ( skill5 > 10.0 ){ c++; skills = skills + skill5; }

			if ( c == 0 )
			{
				return "0";
			}
			else
			{
				skills = skills / c;
			}

			if ( skills > 125.0 )
				skills = 125.0;

			return skills.ToString("0.0");
		}

		private static string FormatTalent( double skill )
		{
			if ( skill < 10 )
				return "---";

			return skill.ToString("0.0");
		}

		private static string FormatTaming( double skill )
		{
			if ( skill == 0 )
				return "---";

			return skill.ToString("0.0");
		}

		private static string FormatPercent( double skill )
		{
			if ( skill < 10 )
				return "---";

			return skill.ToString() + "%";
		}

		private static string FormatNumber( int val )
		{
			if ( val < 1 )
				return "---";

			return val.ToString();
		}

		private static string FormatAttributes( int cur, int max )
		{
			if ( max == 0 )
				return "<div align=right>---</div>";

			return String.Format( "<div align=right>{0}/{1}</div>", cur, max );
		}

		private static string FormatStat( int val )
		{
			if ( val == 0 )
				return "<div align=right>---</div>";

			return String.Format( "<div align=right>{0}</div>", val );
		}

		private static string FormatDouble( double val )
		{
			if ( val == 0 )
				return "<div align=right>---</div>";

			return String.Format( "<div align=right>{0:F1}</div>", val );
		}

		private static string FormatElement( int val )
		{
			if ( val <= 0 )
				return "<div align=right>---</div>";

			return String.Format( "<div align=right>{0}%</div>", val );
		}

		private static string FormatDamage( int min, int max )
		{
			if ( min <= 0 || max <= 0 )
				return "<div align=right>---</div>";

			return String.Format( "<div align=right>{0}-{1}</div>", min, max );
		}

		public override void OnResponse( NetState state, RelayInfo info ) 
		{
			Mobile from = state.Mobile; 
			if ( m_Book > 0 ){ from.SendSound( 0x55 ); }
			else { from.SendSound( 0x0F9 ); }

			if (m_Source == 0)
			{
				if (info.ButtonID < 1) return;

				Actions action = (Actions)info.ButtonID;
				if (CanAddBonus(action))
					GetAttribute(action).ApplyBonus(m_Creature);

				from.SendGump(new DruidismGump(from, m_Creature, m_Source));
			}
		}

		public DruidismGump( Mobile from, BaseCreature c, int source ) : base( 50, 50 )
		{
            this.Closable=true;
			this.Disposable=true;
			this.Dragable=true;
			this.Resizable=false;

			AddPage(0);

			m_Source = source;
			m_Creature = c;
			m_From = from;

			const int COLUMN_ONE_START = 22;
			const int COLUMN_TWO_START = 252;
			const int COLUMN_THREE_START = 482;
			const int SECTION_INDENT = 10;
			const int Y_START = 55;
			const int MARGIN_TOP = 15;

			// 0 - ANIMAL LORE // 1 - MONSTER MANUAL // 2 - PLAYERS HANDBOOK

			m_Book = 0;

			int img = 11416;
			m_Color = HtmlColors.COOL_BLUE;
			string title = StringCatalog.ResolveByKey(from.Account, "skl.druidism.title.monster_manual");

			if ( m_Source == 1 || m_Source == 2 ){ m_Book = 1; }

			if ( m_Source == 2 )
			{
				img = 11417;
				m_Color = HtmlColors.KHAKI;
				title = StringCatalog.ResolveByKey(from.Account, "skl.druidism.title.players_handbook");
			}
			else if ( m_Source == 0 )
			{
				img = 11418;
				m_Color = HtmlColors.COOL_GREEN;
				title = StringCatalog.ResolveByKey(from.Account, "skl.druidism.title.animal_lore");
			}
			else if ( m_Source == 3 )
			{
				img = 11419;
				m_Color = HtmlColors.LIGHT_PINK;
				title = StringCatalog.ResolveByKey(from.Account, "skl.druidism.title.divination");
			}
			else if ( m_Source == 4 )
			{
				img = 11419;
				m_Color = HtmlColors.LIGHT_PINK;
				title = StringCatalog.ResolveByKey(from.Account, "skl.druidism.title.divination");
			}

			AddImage(1, 1, img, Server.Misc.PlayerSettings.GetGumpHue( from ));

			string name = m_Creature.Name;
				if ( m_Creature.Title != "" && m_Creature.Title != null ){ name = name + " " + m_Creature.Title; }

			TextDefinition.AddHtmlText(this, 14, 15, 167, 20, title, false, false, m_Color, m_Color);
			TextDefinition.AddHtmlText(this, 179, 15, 344, 20, string.Format("<CENTER>{0}</CENTER>", name.ToUpper()), false, false, m_Color, m_Color);

			AddButton(667, 12, 4017, 4017, 0, GumpButtonType.Reply, 0);

			int x;
			int y;

			x = COLUMN_ONE_START;
			y = Y_START;
			AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.section.information"), "");
			AddInformationSection(x + SECTION_INDENT, ref y);

			if ( m_Creature.Tamable )
			{
				y += MARGIN_TOP;
				AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.section.tame"), "");
				AddPetSection(x + SECTION_INDENT, ref y);

				if ( source == 0 && m_Creature.MinTameSkill > 0 )
				{
					y += MARGIN_TOP;
					AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.section.favorite_food"), "");
					AddFoodSection(x + SECTION_INDENT, ref y);
				}
			}

			///////////////////////////////////////////////////////////////////////////////////

			x = COLUMN_TWO_START;
			y = Y_START;
			AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.section.damage"), "");
			AddDamageSection(x + SECTION_INDENT, ref y);

			y += MARGIN_TOP;
			AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.section.combat_ratings"), "");
			AddCombatRatingsSection(x + SECTION_INDENT, ref y);

			y += MARGIN_TOP;
			AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.section.lore_knowledge"), "");
			AddLoreAndKnowledgeSection(x + SECTION_INDENT, ref y);

			///////////////////////////////////////////////////////////////////////////////////

			x = COLUMN_THREE_START;
			y = Y_START;
			AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.section.resistance"), "");
			AddResistanceSection(x + SECTION_INDENT, ref y);

			y += MARGIN_TOP;
			AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.section.stats"), "");
			AddStatsSection(x + SECTION_INDENT, ref y);
		}

		private void AddRow(int x, ref int y, string label, string value, int? buttonId = null)
		{
			if (m_Source == 0 && buttonId.HasValue)
			{
				if (CanAddBonus((Actions)buttonId))
				{
					const int PLUS_ICON = 13007;
					AddButton(x - 9, y + 5, PLUS_ICON, PLUS_ICON, buttonId.Value, GumpButtonType.Reply, 0);
				}
			}

			const int TOTAL_WIDTH = 200;
			int leftWidth = TOTAL_WIDTH;
			if (value.Length == 0) { }
			else if (label.Length < value.Length) leftWidth = 50;
			else if (value.Length < label.Length) leftWidth = 125;

			int rightWidth = TOTAL_WIDTH - leftWidth;
			TextDefinition.AddHtmlText(this, x, y, leftWidth, 16, label, false, false, m_Color, m_Color);
			TextDefinition.AddHtmlText(this, x  + leftWidth, y, rightWidth, 16, string.Format("<RIGHT>{0}</RIGHT>", value), false, false, m_Color, m_Color);

			y += 20;
		}

		private bool CanAddBonus(Actions action)
		{
			JakoBaseAttribute attribute = GetAttribute(action);

			return attribute != null && attribute.CanAfford(m_Creature) && attribute.CanAddBonus(m_Creature);
		}

		private JakoBaseAttribute GetAttribute(Actions action)
		{
			JakoAttributesEnum jakoEnum = GetJakoAttribute(action);

            return jakoEnum != JakoAttributesEnum.None ? m_Creature.JakoAttributes.GetAttribute(jakoEnum) : null;
        }

        private JakoAttributesEnum GetJakoAttribute(Actions action)
		{
			switch(action)
			{
				case Actions.HitPoints: return JakoAttributesEnum.Hits;
				case Actions.StaminaPoints: return JakoAttributesEnum.Stam;
				case Actions.ManaPoints: return JakoAttributesEnum.Mana;
				case Actions.ResistPhysical: return JakoAttributesEnum.BonusPhysResist;
				case Actions.ResistFire: return JakoAttributesEnum.BonusFireResist;
				case Actions.ResistCold: return JakoAttributesEnum.BonusColdResist;
				case Actions.ResistPoison: return JakoAttributesEnum.BonusPoisResist;
				case Actions.ResistEnergy: return JakoAttributesEnum.BonusEnerResist;
			}

			return JakoAttributesEnum.None;
		}

		private void AddStatsSection( int x, ref int y )
		{
			AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.label.hits"), FormatNumber( m_Creature.Hits ) + "/" + FormatNumber( m_Creature.HitsMax ), (int)Actions.HitPoints);
			AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.label.stamina"), FormatNumber( m_Creature.Stam ) + "/" + FormatNumber( m_Creature.StamMax ), (int)Actions.StaminaPoints);
			AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.label.mana"), FormatNumber( m_Creature.Mana ) + "/" + FormatNumber( m_Creature.ManaMax ), (int)Actions.ManaPoints);
			AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.label.strength"), FormatNumber( m_Creature.Str ));
			AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.label.dexterity"), FormatNumber( m_Creature.Dex ));
			AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.label.intelligence"), FormatNumber( m_Creature.Int ));
		}

        private void AddDamageSection( int x, ref int y )
		{
			AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.label.physical"), FormatPercent( m_Creature.PhysicalDamage ));
			AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.label.fire"), FormatPercent( m_Creature.FireDamage ));
			AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.label.cold"), FormatPercent( m_Creature.ColdDamage ));
			AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.label.poison"), FormatPercent( m_Creature.PoisonDamage ));
			AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.label.energy"), FormatPercent( m_Creature.EnergyDamage ));
			AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.label.base_damage"), m_Creature.DamageMin + " - " + m_Creature.DamageMax);
		}

        private void AddResistanceSection( int x, ref int y )
		{
			AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.label.physical"), FormatPercent( m_Creature.PhysicalResistance ), (int)Actions.ResistPhysical);
			AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.label.fire"), FormatPercent( m_Creature.FireResistance ), (int)Actions.ResistFire);
			AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.label.cold"), FormatPercent( m_Creature.ColdResistance ), (int)Actions.ResistCold);
			AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.label.poison"), FormatPercent( m_Creature.PoisonResistance ), (int)Actions.ResistPoison);
			AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.label.energy"), FormatPercent( m_Creature.EnergyResistance ), (int)Actions.ResistEnergy);
		}

        private void AddCombatRatingsSection( int x, ref int y )
		{
			AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.label.anatomy"), FormatTalent( m_Creature.Skills[SkillName.Anatomy].Value ));
			AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.label.magic_resist"), FormatTalent( m_Creature.Skills[SkillName.MagicResist].Value ));
			AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.label.poisoning"), FormatTalent( m_Creature.Skills[SkillName.Poisoning].Value ));
			AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.label.tactics"), FormatTalent( m_Creature.Skills[SkillName.Tactics].Value ));
			
			string skill;
			if ( m_Source == 2 )
				skill = FormatFight( m_Creature );
			else if ( m_Source == 0 )
				skill = FormatTalent( m_Creature.Skills[SkillName.FistFighting].Value );
			else if ( m_Source == 3 )
				skill = FormatTalent( m_Creature.Skills[SkillName.FistFighting].Value );
			else if ( m_Source == 4 )
				skill = FormatFight( m_Creature );
			else
				skill = FormatTalent( m_Creature.Skills[SkillName.FistFighting].Value );

			AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.label.combat_skill"), skill);
		}

        private void AddLoreAndKnowledgeSection( int x, ref int y )
		{
			AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.label.magery"), FormatTalent( m_Creature.Skills[SkillName.Magery].Value ));
			AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.label.meditation"), FormatTalent( m_Creature.Skills[SkillName.Meditation].Value ));
			AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.label.psychology"), FormatTalent( m_Creature.Skills[SkillName.Psychology].Value ));
		}

        private void AddPetSection( int x, ref int y )
		{
			if ( m_Creature.ControlMaster == null )
			{
				AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.label.max_pet_level"), m_Creature.MaxLevel.ToString());
			}
			else
			{
				AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.label.pet_level"), m_Creature.Level + "/" + m_Creature.MaxLevel);
				AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.label.traits_available"), m_Creature.Traits.ToString());
				AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.label.experience_earned"), m_Creature.Experience.ToString());
				AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.label.experience_needed"), m_Creature.ExpToNextLevel.ToString());
			}

			string packInstinct = StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.instinct.none");
			if ( ( m_Creature.PackInstinct & PackInstinct.Canine) != 0 ) packInstinct = StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.instinct.canine");
			else if ( ( m_Creature.PackInstinct & PackInstinct.Ostard) != 0 ) packInstinct = StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.instinct.ostard");
			else if ( ( m_Creature.PackInstinct & PackInstinct.Feline) != 0 ) packInstinct = StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.instinct.feline");
			else if ( ( m_Creature.PackInstinct & PackInstinct.Arachnid) != 0 ) packInstinct = StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.instinct.arachnid");
			else if ( ( m_Creature.PackInstinct & PackInstinct.Daemon) != 0 ) packInstinct = StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.instinct.daemon");
			else if ( ( m_Creature.PackInstinct & PackInstinct.Bear) != 0 ) packInstinct = StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.instinct.bear");
			else if ( ( m_Creature.PackInstinct & PackInstinct.Equine) != 0 ) packInstinct = StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.instinct.equine");
			else if ( ( m_Creature.PackInstinct & PackInstinct.Bull) != 0 ) packInstinct = StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.instinct.bull");
			AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.label.pack_instinct"), packInstinct);

			string loyalty = StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.mood.wild");
			int loyal = 1 + ( m_Creature.Loyalty / 10);
			switch ( loyal ) 
			{
				case 1: loyalty = StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.mood.confused"); break;
				case 2: loyalty = StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.mood.extremely_unhappy"); break;
				case 3: loyalty = StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.mood.rather_unhappy"); break;
				case 4: loyalty = StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.mood.unhappy"); break;
				case 5: loyalty = StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.mood.somewhat_content"); break;
				case 6: loyalty = StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.mood.content"); break;
				case 7: loyalty = StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.mood.happy"); break;
				case 8: loyalty = StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.mood.rather_happy"); break;
				case 9: loyalty = StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.mood.very_happy"); break;
				case 10: loyalty = StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.mood.extremely_happy"); break;
				case 11: loyalty = StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.mood.wonderfully_happy"); break;
				case 12: loyalty = StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.mood.euphoric"); break;
			}
			AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.label.mood"), loyalty);
		}

		private void AddFoodSection(int x, ref int y )
		{
			if ( ( m_Creature.FavoriteFood & FoodType.None) != 0 )
				AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.food.none"), "");
			if ( ( m_Creature.FavoriteFood & FoodType.FruitsAndVegies) != 0 )
			{
				AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.food.fruits"), "");
				AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.food.vegetables"), "");
			}
			if ( ( m_Creature.FavoriteFood & FoodType.GrainsAndHay) != 0 )
				AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.food.grains_hay"), "");
			if ( ( m_Creature.FavoriteFood & FoodType.Fish) != 0 )
				AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.food.fish"), "");
			if ( ( m_Creature.FavoriteFood & FoodType.Meat) != 0 )
				AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.food.meat"), "");
			if ( ( m_Creature.FavoriteFood & FoodType.Eggs) != 0 )
				AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.food.eggs"), "");
			if ( ( m_Creature.FavoriteFood & FoodType.Gold) != 0 )
				AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.food.gold"), "");
			if ( ( m_Creature.FavoriteFood & FoodType.Fire) != 0 )
			{
				AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.food.brimstone"), "");
				AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.food.sulfurous_ash"), "");
			}
			if ( ( m_Creature.FavoriteFood & FoodType.Gems) != 0 )
				AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.food.gems"), "");
			if ( ( m_Creature.FavoriteFood & FoodType.Nox) != 0 )
			{
				AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.food.swamp_berries"), "");
				AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.food.nox_crystals"), "");
				AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.food.nightshade"), "");
			}
			if ( ( m_Creature.FavoriteFood & FoodType.Sea) != 0 )
			{
				AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.food.seaweed"), "");
				AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.food.sea_salt"), "");
			}
			if ( ( m_Creature.FavoriteFood & FoodType.Moon) != 0 )
				AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.food.moon_crystals"), "");
		}

        private void AddInformationSection( int x, ref int y )
        {
			AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.label.power_level"), IntelligentAction.GetCreatureLevel( m_Creature ).ToString());

			double bd = !m_Creature.Uncalmable ? Items.BaseInstrument.GetBaseDifficulty( m_Creature ) : 0;
			AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.label.barding_difficulty"), FormatTalent( bd ));

			if ( m_Source == 0 && m_Creature.Tamable && m_Creature.MinTameSkill > 0 )
			{
				AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.label.taming_needed"), FormatTaming( m_Creature.MinTameSkill ));
				AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.label.follower_slots"), m_Creature.ControlSlots.ToString());
				AddRow(x, ref y, StringCatalog.ResolveByKey(m_From.Account, "skl.druidism.label.sex"), m_Creature.SexString);
			}
        }
    }
}
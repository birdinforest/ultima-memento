using System;
using Server.Gumps;
using Server.Mobiles;
using Server.Targeting;
using Server.Misc;
using Server.Items;
using Server.Network;

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
		private readonly int m_Book; 
		private readonly int m_Color;

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
		}

		public DruidismGump( Mobile from, BaseCreature c, int source ) : base( 50, 50 )
		{
            this.Closable=true;
			this.Disposable=true;
			this.Dragable=true;
			this.Resizable=false;

			AddPage(0);

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
			string title = "MONSTER MANUAL";

			if ( source == 1 || source == 2 ){ m_Book = 1; }

			if ( source == 2 )
			{
				img = 11417;
				m_Color = HtmlColors.KHAKI;
				title = "PLAYERS HANDBOOK";
			}
			else if ( source == 0 )
			{
				img = 11418;
				m_Color = HtmlColors.COOL_GREEN;
				title = "ANIMAL LORE";
			}
			else if ( source == 3 )
			{
				img = 11419;
				m_Color = HtmlColors.LIGHT_PINK;
				title = "DIVINATION";
			}
			else if ( source == 4 )
			{
				img = 11419;
				m_Color = HtmlColors.LIGHT_PINK;
				title = "DIVINATION";
			}

			AddImage(1, 1, img, Server.Misc.PlayerSettings.GetGumpHue( from ));

			string name = c.Name;
				if ( c.Title != "" && c.Title != null ){ name = name + " " + c.Title; }

			TextDefinition.AddHtmlText(this, 14, 15, 167, 20, title, false, false, m_Color, m_Color);
			TextDefinition.AddHtmlText(this, 179, 15, 344, 20, string.Format("<CENTER>{0}</CENTER>", name.ToUpper()), false, false, m_Color, m_Color);

			AddButton(667, 12, 4017, 4017, 0, GumpButtonType.Reply, 0);

			int x;
			int y;

			x = COLUMN_ONE_START;
			y = Y_START;
			AddRow(x, ref y, "INFORMATION", "");
			AddInformationSection(x + SECTION_INDENT, ref y, c, source);

			if ( c.Tamable )
			{
				y += MARGIN_TOP;
				AddRow(x, ref y, "TAME", "");
				AddPetSection(x + SECTION_INDENT, ref y, c, source);

				if ( source == 0 && c.MinTameSkill > 0 )
				{
					y += MARGIN_TOP;
					AddRow(x, ref y, "FAVORITE FOOD", "");
					AddFoodSection(x + SECTION_INDENT, ref y, c, source);
				}
			}

			///////////////////////////////////////////////////////////////////////////////////

			x = COLUMN_TWO_START;
			y = Y_START;
			AddRow(x, ref y, "DAMAGE", "");
			AddDamageSection(x + SECTION_INDENT, ref y, c, source);

			y += MARGIN_TOP;
			AddRow(x, ref y, "COMBAT RATINGS", "");
			AddCombatRatingsSection(x + SECTION_INDENT, ref y, c, source);

			y += MARGIN_TOP;
			AddRow(x, ref y, "LORE & KNOWLEDGE", "");
			AddLoreAndKnowledgeSection(x + SECTION_INDENT, ref y, c, source);

			///////////////////////////////////////////////////////////////////////////////////

			x = COLUMN_THREE_START;
			y = Y_START;
			AddRow(x, ref y, "RESISTANCE", "");
			AddResistanceSection(x + SECTION_INDENT, ref y, c, source);

			y += MARGIN_TOP;
			AddRow(x, ref y, "STATS", "");
			AddStatsSection(x + SECTION_INDENT, ref y, c, source);
		}

		private void AddRow(int x, ref int y, string label, string value)
		{
			const int TOTAL_WIDTH = 200;
			int leftWidth = TOTAL_WIDTH;
			if (value.Length == 0){}
			else if (label.Length < value.Length) leftWidth = 50;
			else if (value.Length < label.Length) leftWidth = 125;

			int rightWidth = TOTAL_WIDTH - leftWidth;
			TextDefinition.AddHtmlText(this, x, y, leftWidth, 16, label, false, false, m_Color, m_Color);
			TextDefinition.AddHtmlText(this, x  + leftWidth, y, rightWidth, 16, string.Format("<RIGHT>{0}</RIGHT>", value), false, false, m_Color, m_Color);

			y += 20;
		}

        private void AddStatsSection( int x, ref int y, BaseCreature c, int source)
		{
			AddRow(x, ref y, "Hits", FormatNumber( c.Hits ) + "/" + FormatNumber( c.HitsMax ));
			AddRow(x, ref y, "Stamina", FormatNumber( c.Stam ) + "/" + FormatNumber( c.StamMax ));
			AddRow(x, ref y, "Mana", FormatNumber( c.Mana ) + "/" + FormatNumber( c.ManaMax ));
			AddRow(x, ref y, "Strength", FormatNumber( c.Str ));
			AddRow(x, ref y, "Dexterity", FormatNumber( c.Dex ));
			AddRow(x, ref y, "Intelligence", FormatNumber( c.Int ));
		}

        private void AddDamageSection( int x, ref int y, BaseCreature c, int source)
		{
			AddRow(x, ref y, "Physical", FormatPercent( c.PhysicalDamage ));
			AddRow(x, ref y, "Fire", FormatPercent( c.FireDamage ));
			AddRow(x, ref y, "Cold", FormatPercent( c.ColdDamage ));
			AddRow(x, ref y, "Poison", FormatPercent( c.PoisonDamage ));
			AddRow(x, ref y, "Energy", FormatPercent( c.EnergyDamage ));
			AddRow(x, ref y, "Base Damage", c.DamageMin + " - " + c.DamageMax);
		}

        private void AddResistanceSection( int x, ref int y, BaseCreature c, int source)
		{
			AddRow(x, ref y, "Physical", FormatPercent( c.PhysicalResistance ));
			AddRow(x, ref y, "Fire", FormatPercent( c.FireResistance ));
			AddRow(x, ref y, "Cold", FormatPercent( c.ColdResistance ));
			AddRow(x, ref y, "Poison", FormatPercent( c.PoisonResistance ));
			AddRow(x, ref y, "Energy", FormatPercent( c.EnergyResistance ));
		}

        private void AddCombatRatingsSection( int x, ref int y, BaseCreature c, int source)
		{
			AddRow(x, ref y, "Anatomy", FormatTalent( c.Skills[SkillName.Anatomy].Value ));
			AddRow(x, ref y, "Magic Resist", FormatTalent( c.Skills[SkillName.MagicResist].Value ));
			AddRow(x, ref y, "Poisoning", FormatTalent( c.Skills[SkillName.Poisoning].Value ));
			AddRow(x, ref y, "Tactics", FormatTalent( c.Skills[SkillName.Tactics].Value ));
			
			string skill;
			if ( source == 2 )
				skill = FormatFight( c );
			else if ( source == 0 )
				skill = FormatTalent( c.Skills[SkillName.FistFighting].Value );
			else if ( source == 3 )
				skill = FormatTalent( c.Skills[SkillName.FistFighting].Value );
			else if ( source == 4 )
				skill = FormatFight( c );
			else
				skill = FormatTalent( c.Skills[SkillName.FistFighting].Value );

			AddRow(x, ref y, "Combat Skill", skill);
		}

        private void AddLoreAndKnowledgeSection( int x, ref int y, BaseCreature c, int source)
		{
			AddRow(x, ref y, "Magery", FormatTalent( c.Skills[SkillName.Magery].Value ));
			AddRow(x, ref y, "Meditation", FormatTalent( c.Skills[SkillName.Meditation].Value ));
			AddRow(x, ref y, "Psychology", FormatTalent( c.Skills[SkillName.Psychology].Value ));
		}

        private void AddPetSection( int x, ref int y, BaseCreature c, int source)
		{
			if ( c.ControlMaster == null )
			{
				AddRow(x, ref y, "Max Pet Level", c.MaxLevel.ToString());
			}
			else
			{
				AddRow(x, ref y, "Pet Level", c.Level + "/" + c.MaxLevel);
				AddRow(x, ref y, "Experience", c.Experience.ToString());
				AddRow(x, ref y, "Experience Needed", c.ExpToNextLevel.ToString());
			}

			string packInstinct = "None";
			if ( (c.PackInstinct & PackInstinct.Canine) != 0 ) packInstinct = "Canine";
			else if ( (c.PackInstinct & PackInstinct.Ostard) != 0 ) packInstinct = "Ostard";
			else if ( (c.PackInstinct & PackInstinct.Feline) != 0 ) packInstinct = "Feline";
			else if ( (c.PackInstinct & PackInstinct.Arachnid) != 0 ) packInstinct = "Arachnid";
			else if ( (c.PackInstinct & PackInstinct.Daemon) != 0 ) packInstinct = "Daemon";
			else if ( (c.PackInstinct & PackInstinct.Bear) != 0 ) packInstinct = "Bear";
			else if ( (c.PackInstinct & PackInstinct.Equine) != 0 ) packInstinct = "Equine";
			else if ( (c.PackInstinct & PackInstinct.Bull) != 0 ) packInstinct = "Bull";
			AddRow(x, ref y, "Pack Instinct", packInstinct);

			string loyalty = "Wild";
			int loyal = 1 + (c.Loyalty / 10);
			switch ( loyal ) 
			{
				case 1: loyalty = "Confused"; break;
				case 2: loyalty = "Extremely Unhappy"; break;
				case 3: loyalty = "Rather Unhappy"; break;
				case 4: loyalty = "Unhappy"; break;
				case 5: loyalty = "Somewhat Content"; break;
				case 6: loyalty = "Content"; break;
				case 7: loyalty = "Happy"; break;
				case 8: loyalty = "Rather Happy"; break;
				case 9: loyalty = "Very Happy"; break;
				case 10: loyalty = "Extremely Happy"; break;
				case 11: loyalty = "Wonderfully Happy"; break;
				case 12: loyalty = "Euphoric"; break;
			}
			AddRow(x, ref y, "Mood", loyalty);
		}

		private void AddFoodSection(int x, ref int y, BaseCreature c, int source)
		{
			if ( (c.FavoriteFood & FoodType.None) != 0 )
				AddRow(x, ref y, "None", "");
			if ( (c.FavoriteFood & FoodType.FruitsAndVegies) != 0 )
			{
				AddRow(x, ref y, "Fruits", "");
				AddRow(x, ref y, "Vegetables", "");
			}
			if ( (c.FavoriteFood & FoodType.GrainsAndHay) != 0 )
				AddRow(x, ref y, "Grains & Hay", "");
			if ( (c.FavoriteFood & FoodType.Fish) != 0 )
				AddRow(x, ref y, "Fish", "");
			if ( (c.FavoriteFood & FoodType.Meat) != 0 )
				AddRow(x, ref y, "Meat", "");
			if ( (c.FavoriteFood & FoodType.Eggs) != 0 )
				AddRow(x, ref y, "Eggs", "");
			if ( (c.FavoriteFood & FoodType.Gold) != 0 )
				AddRow(x, ref y, "Gold", "");
			if ( (c.FavoriteFood & FoodType.Fire) != 0 )
			{
				AddRow(x, ref y, "Brimstone", "");
				AddRow(x, ref y, "Sulfurous Ash", "");
			}
			if ( (c.FavoriteFood & FoodType.Gems) != 0 )
				AddRow(x, ref y, "Gems", "");
			if ( (c.FavoriteFood & FoodType.Nox) != 0 )
			{
				AddRow(x, ref y, "Swamp Berries", "");
				AddRow(x, ref y, "Nox Crystals", "");
				AddRow(x, ref y, "Nightshade", "");
			}
			if ( (c.FavoriteFood & FoodType.Sea) != 0 )
			{
				AddRow(x, ref y, "Seaweed", "");
				AddRow(x, ref y, "Sea Salt", "");
			}
			if ( (c.FavoriteFood & FoodType.Moon) != 0 )
				AddRow(x, ref y, "Moon Crystals", "");
		}

        private void AddInformationSection( int x, ref int y, BaseCreature c, int source)
        {
			AddRow(x, ref y, "Power Level", IntelligentAction.GetCreatureLevel( c ).ToString());

			double bd = !c.Uncalmable ? Items.BaseInstrument.GetBaseDifficulty( c ) : 0;
			AddRow(x, ref y, "Barding Difficulty", FormatTalent( bd ));

			if ( source == 0 && c.Tamable && c.MinTameSkill > 0 )
			{
				AddRow(x, ref y, "Taming Needed", FormatTaming( c.MinTameSkill ));
				AddRow(x, ref y, "Follower Slots", c.ControlSlots.ToString());
			}
        }
    }
}
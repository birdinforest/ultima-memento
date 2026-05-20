using System;
using Server;
using Server.Network;
using Server.Multis;
using Server.Gumps;
using Server.Mobiles;
using Server.Accounting;
using Server.Misc;
using System.Collections;
using System.Collections.Generic;
using Server.Commands;
using Server.Commands.Generic;
using Server.Regions;
using Server.Localization;

namespace Server.Items
{
	public class Prisoner : Item
	{
		private int PrisonerReward;
		[CommandProperty( AccessLevel.GameMaster )]
		public int Prisoner_Reward { get{ return PrisonerReward; } set{ PrisonerReward = value; InvalidateProperties(); } }

		private int PrisonerJoin;
		[CommandProperty( AccessLevel.GameMaster )]
		public int Prisoner_Join { get{ return PrisonerJoin; } set{ PrisonerJoin = value; InvalidateProperties(); } }

		private int PrisonerType;
		[CommandProperty( AccessLevel.GameMaster )]
		public int Prisoner_Type { get{ return PrisonerType; } set{ PrisonerType = value; InvalidateProperties(); } }

		private string PrisonerName;
		[CommandProperty( AccessLevel.GameMaster )]
		public string Prisoner_Name { get{ return PrisonerName; } set{ PrisonerName = value; InvalidateProperties(); } }

		private string PrisonerTitle;
		[CommandProperty( AccessLevel.GameMaster )]
		public string Prisoner_Title { get{ return PrisonerTitle; } set{ PrisonerTitle = value; InvalidateProperties(); } }

		private int PrisonerBody;
		[CommandProperty( AccessLevel.GameMaster )]
		public int Prisoner_Body { get{ return PrisonerBody; } set{ PrisonerBody = value; InvalidateProperties(); } }

		private int PrisonerSound;
		[CommandProperty( AccessLevel.GameMaster )]
		public int Prisoner_Sound { get{ return PrisonerSound; } set{ PrisonerSound = value; InvalidateProperties(); } }

		[Constructable]
		public Prisoner() : base( 0x2019 )
		{
			Movable = false;
			Name = "caged creature";

			if ( PrisonerReward < 1 ){ BuildPrisoner(); }
		}

        public override void AddNameProperties(ObjectPropertyList list)
		{
            base.AddNameProperties(list);
			list.Add( 1070722, PrisonerName );
			list.Add( 1049644, PrisonerTitle );
        }

		public Prisoner( Serial serial ) : base( serial )
		{
		}

		public void BuildPrisoner()
		{
			int monster = Utility.RandomMinMax( 1, 47 );

			switch ( Utility.RandomMinMax( 0, 9 ) )
			{
				case 0: monster = Utility.RandomMinMax( 1, 12 ); break;
				case 1: monster = Utility.RandomMinMax( 13, 22 ); break;
				case 2: monster = Utility.RandomMinMax( 23, 30 ); break;
				case 3: monster = Utility.RandomMinMax( 31, 34 ); break;
				case 4: monster = Utility.RandomMinMax( 35, 41 ); break;
				case 5: monster = Utility.RandomMinMax( 42, 47 ); break;
				case 6: monster = 48; break;
				case 7: monster = 49; break;
				case 8: monster = 50; break;
			}

			switch ( monster )
			{
				case 1: PrisonerTitle = "the bugbear"; PrisonerName = NameList.RandomName( "giant" ); PrisonerBody = 343; PrisonerSound = 427; PrisonerType = 1; break;
				case 2: PrisonerTitle = "the morlock"; PrisonerName = NameList.RandomName( "savage" ); PrisonerBody = 332; PrisonerSound = 427; PrisonerType = 1; break;
				case 3: PrisonerTitle = "the mind flayer"; PrisonerName = NameList.RandomName( "vampire" ); PrisonerBody = 768; PrisonerSound = 898; PrisonerType = 3; break;
				case 4: PrisonerTitle = "the hobgoblin"; PrisonerName = NameList.RandomName( "giant" ); PrisonerBody = 11; PrisonerSound = 1114; PrisonerType = 1; break;
				case 5: PrisonerTitle = "the goblin"; PrisonerName = NameList.RandomName( "goblin" ); PrisonerBody = 647; PrisonerSound = 0x543; PrisonerType = 2; break;
				case 6: PrisonerTitle = "the goblin"; PrisonerName = NameList.RandomName( "goblin" ); PrisonerBody = 632; PrisonerSound = 0x543; PrisonerType = 1; break;
				case 7: PrisonerTitle = "the gnoll"; PrisonerName = NameList.RandomName( "urk" ); PrisonerBody = 510; PrisonerSound = 1114; PrisonerType = 1; break;
				case 8: PrisonerTitle = "the satyr"; PrisonerName = NameList.RandomName( "elf_male" ); PrisonerBody = 271; PrisonerSound = 1414; PrisonerType = 1; break;
				case 9: PrisonerTitle = "the centaur"; PrisonerName = NameList.RandomName( "centaur" ); PrisonerBody = 101; PrisonerSound = 679; PrisonerType = 2; break;
				case 10: PrisonerTitle = "the pixie"; PrisonerName = NameList.RandomName( "pixie" ); PrisonerBody = 128; PrisonerSound = 1127; PrisonerType = 3; break;
				case 11: PrisonerTitle = "the minotaur"; PrisonerName = NameList.RandomName( "greek" ); PrisonerBody = 78; PrisonerSound = 1358; PrisonerType = 1; break;
				case 12: PrisonerTitle = "the minotaur"; PrisonerName = NameList.RandomName( "greek" ); PrisonerBody = 650; PrisonerSound = 1358; PrisonerType = 1; break;
				case 13: PrisonerTitle = "the sleestax"; PrisonerName = NameList.RandomName( "lizardman" ); PrisonerBody = 541; PrisonerSound = 417; PrisonerType = 1; break;
				case 14: PrisonerTitle = "the sakkhra"; PrisonerName = NameList.RandomName( "lizardman" ); PrisonerBody = 326; PrisonerSound = 417; PrisonerType = 3; break;
				case 15: PrisonerTitle = "the sakkhra"; PrisonerName = NameList.RandomName( "lizardman" ); PrisonerBody = 333; PrisonerSound = 417; PrisonerType = 1; break;
				case 16: PrisonerTitle = "the sakkhra"; PrisonerName = NameList.RandomName( "lizardman" ); PrisonerBody = 324; PrisonerSound = 417; PrisonerType = 1; break;
				case 17: PrisonerTitle = "the lizardman"; PrisonerName = NameList.RandomName( "lizardman" ); PrisonerBody = 33; PrisonerSound = 417; PrisonerType = 1; break;
				case 18: PrisonerTitle = "the lizardman"; PrisonerName = NameList.RandomName( "lizardman" ); PrisonerBody = 35; PrisonerSound = 417; PrisonerType = 1; break;
				case 19: PrisonerTitle = "the lizardman"; PrisonerName = NameList.RandomName( "lizardman" ); PrisonerBody = 36; PrisonerSound = 417; PrisonerType = 1; break;
				case 20: PrisonerTitle = "the kobold"; PrisonerName = NameList.RandomName( "goblin" ); PrisonerBody = 253; PrisonerSound = 0x543; PrisonerType = 3; break;
				case 21: PrisonerTitle = "the kobold"; PrisonerName = NameList.RandomName( "goblin" ); PrisonerBody = 245; PrisonerSound = 0x543; PrisonerType = 1; break;
				case 22: PrisonerTitle = "the grathek"; PrisonerName = NameList.RandomName( "lizardman" ); PrisonerBody = 534; PrisonerSound = 417; PrisonerType = 1; break;
				case 23: PrisonerTitle = "the orx"; PrisonerName = NameList.RandomName( "ork" ); PrisonerBody = 107; PrisonerSound = 1114; PrisonerType = 1; break;
				case 24: PrisonerTitle = "the orx"; PrisonerName = NameList.RandomName( "ork" ); PrisonerBody = 108; PrisonerSound = 1114; PrisonerType = 1; break;
				case 25: PrisonerTitle = "the orc"; PrisonerName = NameList.RandomName( "orc" ); PrisonerBody = 17; PrisonerSound = 1114; PrisonerType = 3; break;
				case 26: PrisonerTitle = "the orc"; PrisonerName = NameList.RandomName( "orc" ); PrisonerBody = 7; PrisonerSound = 1114; PrisonerType = 1; break;
				case 27: PrisonerTitle = "the orc"; PrisonerName = NameList.RandomName( "orc" ); PrisonerBody = 182; PrisonerSound = 1114; PrisonerType = 1; break;
				case 28: PrisonerTitle = "the urc"; PrisonerName = NameList.RandomName( "urk" ); PrisonerBody = 20; PrisonerSound = 1114; PrisonerType = 1; break;
				case 29: PrisonerTitle = "the urc"; PrisonerName = NameList.RandomName( "urk" ); PrisonerBody = 252; PrisonerSound = 1114; PrisonerType = 2; break;
				case 30: PrisonerTitle = "the urc"; PrisonerName = NameList.RandomName( "urk" ); PrisonerBody = 157; PrisonerSound = 1114; PrisonerType = 3; break;
				case 31: PrisonerTitle = "the tritun"; PrisonerName = NameList.RandomName( "drakkul" ); PrisonerBody = 690; PrisonerSound = 1363; PrisonerType = 1; break;
				case 32: PrisonerTitle = "the tritun"; PrisonerName = NameList.RandomName( "drakkul" ); PrisonerBody = 678; PrisonerSound = 1363; PrisonerType = 3; break;
				case 33: PrisonerTitle = "the neptar"; PrisonerName = NameList.RandomName( "drakkul" ); PrisonerBody = 677; PrisonerSound = 1363; PrisonerType = 3; break;
				case 34: PrisonerTitle = "the neptar"; PrisonerName = NameList.RandomName( "drakkul" ); PrisonerBody = 676; PrisonerSound = 1363; PrisonerType = 1; break;
				case 35: PrisonerTitle = "the ratman"; PrisonerName = NameList.RandomName( "ratman" ); PrisonerBody = 42; PrisonerSound = 437; PrisonerType = 2; break;
				case 36: PrisonerTitle = "the ratman"; PrisonerName = NameList.RandomName( "ratman" ); PrisonerBody = 44; PrisonerSound = 437; PrisonerType = 1; break;
				case 37: PrisonerTitle = "the ratman"; PrisonerName = NameList.RandomName( "ratman" ); PrisonerBody = 45; PrisonerSound = 437; PrisonerType = 1; break;
				case 38: PrisonerTitle = "the ratman"; PrisonerName = NameList.RandomName( "ratman" ); PrisonerBody = 163; PrisonerSound = 437; PrisonerType = 1; break;
				case 39: PrisonerTitle = "the ratman"; PrisonerName = NameList.RandomName( "ratman" ); PrisonerBody = 164; PrisonerSound = 437; PrisonerType = 1; break;
				case 40: PrisonerTitle = "the ratman"; PrisonerName = NameList.RandomName( "ratman" ); PrisonerBody = 165; PrisonerSound = 437; PrisonerType = 1; break;
				case 41: PrisonerTitle = "the ratman"; PrisonerName = NameList.RandomName( "ratman" ); PrisonerBody = 73; PrisonerSound = 437; PrisonerType = 3; break;
				case 42: PrisonerTitle = "the serpyn"; PrisonerName = NameList.RandomName( "drakkul" ); PrisonerBody = 143; PrisonerSound = 634; PrisonerType = 1; break;
				case 43: PrisonerTitle = "the serpyn"; PrisonerName = NameList.RandomName( "drakkul" ); PrisonerBody = 145; PrisonerSound = 634; PrisonerType = 1; break;
				case 44: PrisonerTitle = "the serpyn"; PrisonerName = NameList.RandomName( "drakkul" ); PrisonerBody = 144; PrisonerSound = 644; PrisonerType = 3; break;
				case 45: PrisonerTitle = "the ophidian"; PrisonerName = NameList.RandomName( "drakkul" ); PrisonerBody = 85; PrisonerSound = 639; PrisonerType = 3; break;
				case 46: PrisonerTitle = "the ophidian"; PrisonerName = NameList.RandomName( "drakkul" ); PrisonerBody = 86; PrisonerSound = 634; PrisonerType = 1; break;
				case 47: PrisonerTitle = "the ophidian"; PrisonerName = NameList.RandomName( "drakkul" ); PrisonerBody = 87; PrisonerSound = 644; PrisonerType = 3; break;
				case 48:
							HenchmanItem fighter = new HenchmanFighterItem();
							PrisonerName = fighter.HenchName; 
							PrisonerTitle = fighter.HenchTitle; 
							PrisonerBody = fighter.HenchBody; 
							PrisonerSound = 0; 
							PrisonerType = 97;
							fighter.Delete();
					break;
				case 49:
							HenchmanItem archer = new HenchmanArcherItem();
							PrisonerName = archer.HenchName; 
							PrisonerTitle = archer.HenchTitle; 
							PrisonerBody = archer.HenchBody; 
							PrisonerSound = 0; 
							PrisonerType = 98;
							archer.Delete();
					break;
				case 50:
							HenchmanItem wizard = new HenchmanWizardItem();
							PrisonerName = wizard.HenchName; 
							PrisonerTitle = wizard.HenchTitle; 
							PrisonerBody = wizard.HenchBody; 
							PrisonerSound = 0; 
							PrisonerType = 97;
							wizard.Delete();
					break;
			}

			int reward = Utility.RandomMinMax( 10, 20 );
			int join = Utility.RandomMinMax( 50, 100 );

			if ( PrisonerType == 1 ){ PrisonerTitle = PrisonerTitle + " " + GetMeleeTitle(); PrisonerReward = (reward*100); PrisonerJoin = (join*100); }
			else if ( PrisonerType == 2 ){ PrisonerTitle = PrisonerTitle + " " + GetArcherTitle(); PrisonerReward = (reward*125); PrisonerJoin = (join*125); }
			else if ( PrisonerType == 3 ){ PrisonerTitle = PrisonerTitle + " " + GetMageTitle(); PrisonerReward = (reward*150); PrisonerJoin = (join*150); }
			else if ( PrisonerType == 97 ){ PrisonerReward = (reward*150); PrisonerJoin = (4000+(10*Utility.RandomMinMax( 10, 100 ))); }
			else if ( PrisonerType == 98 ){ PrisonerReward = (reward*150); PrisonerJoin = (5000+(10*Utility.RandomMinMax( 10, 100 ))); }
			else if ( PrisonerType == 99 ){ PrisonerReward = (reward*150); PrisonerJoin = (6000+(10*Utility.RandomMinMax( 10, 100 ))); }
		}

		public string GetMeleeTitle()
		{
			string title = "warrior";
			switch ( Utility.RandomMinMax( 0, 12 ) )
			{
				case 0: title = "fighter"; break;
				case 1: title = "knight"; break;
				case 2: title = "champion"; break;
				case 3: title = "warrior"; break;
				case 4: title = "soldier"; break;
				case 5: title = "vanquisher"; break;
				case 6: title = "battler"; break;
				case 7: title = "gladiator"; break;
				case 8: title = "mercenary"; break;
				case 9: title = "nomad"; break;
				case 10: title = "berserker"; break;
				case 11: title = "pit fighter"; break;
				case 12: title = "brute"; break;
			}
			return title;
		}

		public string GetArcherTitle()
		{
			string title = "archer";
			switch ( Utility.RandomMinMax( 0, 1 ) )
			{
				case 0: title = "bowman"; break;
				case 1: title = "archer"; break;
			}
			return title;
		}

		public string GetMageTitle()
		{
			string title = "wizard";
			switch ( Utility.RandomMinMax( 0, 4 ) )
			{
				case 0: title = "wizard"; break;
				case 1: title = "shaman"; break;
				case 2: title = "mage"; break;
				case 3: title = "conjurer"; break;
				case 4: title = "magician"; break;
			}
			return title;
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( ( int) 0 ); // version
			writer.Write( PrisonerReward );
			writer.Write( PrisonerJoin );
			writer.Write( PrisonerType );
			writer.Write( PrisonerName );
			writer.Write( PrisonerTitle );
			writer.Write( PrisonerBody );
			writer.Write( PrisonerSound );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
			PrisonerReward = reader.ReadInt();
			PrisonerJoin = reader.ReadInt();
			PrisonerType = reader.ReadInt();
			PrisonerName = reader.ReadString();
			PrisonerTitle = reader.ReadString();
			PrisonerBody = reader.ReadInt();
			PrisonerSound = reader.ReadInt();
		}

		public override void OnDoubleClick( Mobile from )
		{
			from.CloseGump( typeof( PrisonerGump ) );
			from.SendGump( new PrisonerGump( this, from ) );
		}

		private class PrisonerGump : Gump
		{
			private Prisoner m_Jail;

			public PrisonerGump( Prisoner jail, Mobile from ) : base( 50, 50 )
			{
				m_Jail = jail;

				string color = "#c2d5dc";
				from.SendSound( 0x0EC );

				this.Closable=true;
				this.Disposable=true;
				this.Dragable=true;
				this.Resizable=false;

				AddPage(0);

				AddImage(0, 0, 7030, Server.Misc.PlayerSettings.GetGumpHue( from ));
				AddButton(368, 9, 4017, 4017, 0, GumpButtonType.Reply, 0);


				string prisonerFullName = m_Jail.PrisonerName + " " + m_Jail.PrisonerTitle;

				string paragraph1 = StringCatalog.ResolveFormatByKey(from.Account, "quest.n0_has_been_locked_in_this_cage_and_is_begging_to_be_freed_dot_you_can_choose_to_leave_them_to_their", prisonerFullName, m_Jail.PrisonerReward, m_Jail.PrisonerJoin );

				string paragraph2 = StringCatalog.ResolveByKey(from.Account, "quest.henchman_are_followers_that_can_join_you_on_adventures_so_you_do_not_have_to_traverse_the_dangerous_");

				string paragraph = paragraph1 + "<br><br>" + paragraph2;


				AddHtml( 12, 12, 346, 20, @"<BODY><BASEFONT Color=" + color + ">PRISONER - " + prisonerFullName + "</BASEFONT></BODY>", (bool)false, (bool)false);

				AddHtml( 12, 44, 382, 361, @"<BODY><BASEFONT Color=" + color + ">" + paragraph + "</BASEFONT></BODY>", (bool)false, (bool)true);

				AddButton(9, 417, 4005, 4005, 1, GumpButtonType.Reply, 0);
				AddButton(9, 450, 4008, 4008, 2, GumpButtonType.Reply, 0);
				AddButton(9, 483, 4020, 4020, 3, GumpButtonType.Reply, 0);
				AddHtml( 48, 417, 346, 20, @"<BODY><BASEFONT Color=" + color + ">" + StringCatalog.ResolveFormatByKey(from.Account, "quest.set_them_free_earn_n0_gold", m_Jail.PrisonerReward ) + "</BASEFONT></BODY>", (bool)false, (bool)false);
				AddHtml( 48, 450, 346, 20, @"<BODY><BASEFONT Color=" + color + ">" + StringCatalog.ResolveFormat( from.Account, "Give Them {0} Gold to Join You", m_Jail.PrisonerJoin ) + "</BASEFONT></BODY>", (bool)false, (bool)false);
				AddHtml( 48, 483, 346, 20, @"<BODY><BASEFONT Color=" + color + ">" + StringCatalog.ResolveByKey(from.Account, "quest.leave_them_to_their_fate") + "</BASEFONT></BODY>", (bool)false, (bool)false);
			}

			public override void OnResponse( NetState sender, RelayInfo info )
			{
				Mobile from = sender.Mobile;
				from.SendSound( 0x0EC );

				if ( info.ButtonID == 1 )
				{
					from.AddToBackpack ( new Gold( m_Jail.PrisonerReward ) );
					from.SendMessage( StringCatalog.ResolveFormatByKey(from.Account, "quest.you_free_n0_from_their_prison_dot", m_Jail.PrisonerName ) );
					LoggingFunctions.LogStandard( from, "has freed " + m_Jail.PrisonerName + " " + m_Jail.PrisonerTitle + "." );

					CustomEventSink.InvokeCombatQuestCompleted( from, m_Jail.PrisonerReward );
					Titles.AwardFame( from, ((int)((m_Jail.PrisonerReward)/100)), true );
					if ( ((PlayerMobile)from).KarmaLocked == true ){ Titles.AwardKarma( from, -((int)((m_Jail.PrisonerReward)/100)), true ); }
					else { Titles.AwardKarma( from, ((int)((m_Jail.PrisonerReward)/100)), true ); }

					m_Jail.Delete();
				}
				else if ( info.ButtonID == 2 )
				{
					int gold = from.TotalGold;
					int join = m_Jail.PrisonerJoin;
					bool begging = false;

					if ( Server.Mobiles.BaseVendor.BeggingPose(from) > 0 ) // LET US SEE IF THEY ARE BEGGING
					{
						int cut = (int)(from.Skills[SkillName.Begging].Value * 25 );
							if ( cut > 3000 ){ cut = 3000; }
						join = join - cut;
						begging = true;
					}

					if ( gold >= join )
					{
						Container cont = from.Backpack;
						cont.ConsumeTotal( typeof( Gold ), join );

						if ( begging )
							from.SendMessage( StringCatalog.ResolveFormatByKey(from.Account, "quest.you_beg_n0_to_join_you_as_a_henchman_for_only_n1_gold_dot", m_Jail.PrisonerName, join ) );
						else
							from.SendMessage( StringCatalog.ResolveFormatByKey(from.Account, "quest.n0_has_joined_you_as_a_henchman_dot", m_Jail.PrisonerName ) );

						if ( m_Jail.PrisonerType == 97 )
						{
							HenchmanFighterItem fighter = new HenchmanFighterItem();
							fighter.HenchName = m_Jail.PrisonerName; 
							fighter.HenchTitle = m_Jail.PrisonerTitle; 
							fighter.HenchBody = m_Jail.PrisonerBody; 
							from.AddToBackpack( fighter );
						}
						else if ( m_Jail.PrisonerType == 98 )
						{
							HenchmanArcherItem archer = new HenchmanArcherItem();
							archer.HenchName = m_Jail.PrisonerName; 
							archer.HenchTitle = m_Jail.PrisonerTitle; 
							archer.HenchBody = m_Jail.PrisonerBody; 
							from.AddToBackpack( archer );
						}
						else if ( m_Jail.PrisonerType == 99 )
						{
							HenchmanWizardItem wizard = new HenchmanWizardItem();
							wizard.HenchName = m_Jail.PrisonerName; 
							wizard.HenchTitle = m_Jail.PrisonerTitle; 
							wizard.HenchBody = m_Jail.PrisonerBody; 
							from.AddToBackpack( wizard );
						}
						else
						{
							HenchmanMonsterItem item = new HenchmanMonsterItem();

							item.HenchTimer = 300;
							item.HenchWeaponID = m_Jail.PrisonerType;
							item.HenchShieldID = m_Jail.PrisonerSound;
							item.HenchHelmID = 0;
							item.HenchArmorType = 0;
							item.HenchWeaponType = 0;
							item.HenchCloakColor = 0;
							item.HenchCloak = 0;
							item.HenchRobe = 0;
							item.HenchHatColor = 0;
							item.HenchGloves = 0;
							item.HenchSpeech = Utility.RandomDyedHue();
							item.HenchDead = 0;
							item.HenchBody = m_Jail.PrisonerBody;
							item.HenchHue = 0;
							item.HenchHair = 0;
							item.HenchHairHue = 0;
							item.HenchGearColor = 0;
							item.HenchName = m_Jail.PrisonerName;
							item.HenchTitle = m_Jail.PrisonerTitle;
							item.HenchBandages = 0;
							from.AddToBackpack( item );
						}

						m_Jail.Delete();
					}
					else
					{
						from.SendMessage( StringCatalog.Resolve( from.Account, "You do not have enough gold to convince them to join you." ) );
					}
				}
				else if ( info.ButtonID == 3 )
				{
					switch ( Utility.RandomMinMax( 0, 4 ) )
					{
						case 0: from.Say( StringCatalog.ResolveFormatByKey(from.Account, "quest.i_will_leave_you_to_your_fate_c_n0_ex", m_Jail.PrisonerName ) ); break;
						case 1: from.Say( StringCatalog.ResolveFormatByKey(from.Account, "quest.n0_c_stay_here_and_rot_ex", m_Jail.PrisonerName ) ); break;
						case 2: from.Say( StringCatalog.ResolveFormatByKey(from.Account, "quest.n0_c_the_world_is_better_with_you_in_here_ex", m_Jail.PrisonerName ) ); break;
						case 3: from.Say( StringCatalog.ResolveFormatByKey(from.Account, "quest.you_are_not_the_sort_i_wish_to_free_c_n0_dot", m_Jail.PrisonerName ) ); break;
						case 4: from.Say( StringCatalog.ResolveFormatByKey(from.Account, "quest.you_must_be_here_for_a_reason_c_n0_dot", m_Jail.PrisonerName ) ); break;
					}
				}
			}
		}
	}
}
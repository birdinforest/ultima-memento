using System;
using Server;
using System.Collections;
using System.Collections.Generic;
using Server.Targeting;
using Server.Items;
using Server.Network;
using Server.ContextMenus;
using Server.Gumps;
using Server.Misc;
using Server.Mobiles;
using Server.Localization;

namespace Server.Mobiles 
{ 
	public class TownGuards : BasePerson
	{
		public static readonly TimeSpan OathWindowDuration = TimeSpan.FromMinutes( 15 );
		public const int OathKnockdownsRequired = 3;

		private const int OathBreaksRequired = OathKnockdownsRequired;

		private int m_OathBreaks;
		private DateTime m_OathWindow;
		private HashSet<Serial> m_Assailants = new HashSet<Serial>();
		private int m_AssailantCount;
		private int m_AttemptCount;
		private DateTime m_EngagementStart;
		private Serial m_OathWeaponSerial;
		private OathWindowTimer m_OathWindowTimer;
		private int m_OathWindowLastThresholdIndex;
		private bool m_KeepCorpseOnOathDeath;

		public bool HasActiveOathWindow
		{
			get { return m_OathBreaks > 0 && m_OathWindow != DateTime.MinValue && ( DateTime.UtcNow - m_OathWindow ) <= OathWindowDuration; }
		}

		public bool IsOathEngaged => m_EngagementStart != DateTime.MinValue;

		public int OathBreaks => m_OathBreaks;
		public int OathAssailantCount => m_AssailantCount;
		public DateTime OathWindowStart => m_OathWindow;

		public TimeSpan GetOathWindowTimeRemaining()
		{
			if ( !HasActiveOathWindow )
				return TimeSpan.Zero;

			TimeSpan elapsed = DateTime.UtcNow - m_OathWindow;
			TimeSpan left = OathWindowDuration - elapsed;

			if ( left < TimeSpan.Zero )
				return TimeSpan.Zero;

			return left;
		}

		public IEnumerable<PlayerMobile> GetOathAssailants()
		{
			foreach ( Serial s in m_Assailants )
			{
				Mobile m = World.FindMobile( s );

				if ( m is PlayerMobile pm && !pm.Deleted )
					yield return pm;
			}
		}

		private void RefreshOathStatusGumps()
		{
			OathStatusGump.RefreshAllForGuard( this );
		}

		private void CloseOathStatusGumps()
		{
			OathStatusGump.CloseAllForGuard( this );
		}

		// Melee output tuned for ~200 HP per swing vs endgame tanks (~70 PhysicalResistance).
		// BaseCreature SetDamage is used while the guard wields OathGuardSword; player-mode sword damage stays on the item.
		private const int GuardDamageMin = 100;
		private const int GuardDamageMax = 130;

		[Constructable] 
		public TownGuards() : base( ) 
		{
			Title = "the guard";
			NameHue = 1154;

			// Endgame champion-tier profile (see ENDGAME_BOSS_ANALYSIS — Rikktor / KhumashGor band).
			SetStr( 800, 950 );
			SetDex( 201, 350 );
			SetInt( 101, 200 );
			SetHits( 3500 );
			SetStam( 203, 650 );
			SetDamage( GuardDamageMin, GuardDamageMax );
			VirtualArmor = 100;

			SetDamageType( ResistanceType.Physical, 100 );

			SetResistance( ResistanceType.Physical, 40, 50 );
			SetResistance( ResistanceType.Fire, 55, 65 );
			SetResistance( ResistanceType.Cold, 35, 45 );
			SetResistance( ResistanceType.Poison, 55, 65 );
			SetResistance( ResistanceType.Energy, 55, 65 );

			SetSkill( SkillName.Anatomy, 100.0, 120.0 );
			SetSkill( SkillName.MagicResist, 115.0, 140.0 );
			SetSkill( SkillName.Bludgeoning, 100.0, 120.0 );
			SetSkill( SkillName.Fencing, 100.0, 120.0 );
			SetSkill( SkillName.FistFighting, 100.0, 120.0 );
			SetSkill( SkillName.Swords, 100.0, 120.0 );
			SetSkill( SkillName.Tactics, 100.0, 120.0 );

			AddItem( new LightCitizen( true ) );

			if ( MySettings.S_GuardsSprint )
			{
				ActiveSpeed = 0.15;
				PassiveSpeed = 0.25;
			}

			if ( Backpack != null ){ Backpack.Delete(); }
			Container pack = new Backpack();
			pack.Movable = false;
			AddItem( pack );
		}

		public override bool BardImmune{ get{ return true; } }
		public override Poison PoisonImmune{ get{ return Poison.Deadly; } }
		public override bool Unprovokable { get { return true; } }
		public override bool Uncalmable{ get{ return true; } }

		public override bool DeleteCorpseOnDeath
		{
			get
			{
				if ( m_KeepCorpseOnOathDeath )
					return false;

				return base.DeleteCorpseOnDeath;
			}
		}

		public override bool OnDragDrop( Mobile from, Item dropped )
		{
			if ( dropped is PirateBounty )
			{
				if ( IntelligentAction.GetMyEnemies( from, this, false ) )
				{
					string sSay = "You shouldn't be carrying that around with you.";
					this.PrivateOverheadMessage(MessageType.Regular, 1153, false, sSay, from.NetState);
				}
				else
				{
					PirateBounty bounty = (PirateBounty)dropped;
					int fame = (int)(bounty.BountyValue/5);
					int karma = -1*fame;
					int gold = bounty.BountyValue;
					Server.Engines.Harvest.Fishing.SailorSkill( from, (int)( gold / 100 ) );
					string sMessage = "";
					string sReward = "Here is " + gold.ToString() + " gold for you.";

					switch ( Utility.RandomMinMax( 0, 9 ) )
					{
						case 0:	sReward = "Here is " + gold.ToString() + " gold for you.";							break;
						case 1:	sReward = "Take this " + gold.ToString() + " gold for your trouble.";				break;
						case 2:	sReward = "The reward is " + gold.ToString() + " gold.";							break;
						case 3:	sReward = "Here is " + gold.ToString() + " gold for the bounty.";					break;
						case 4:	sReward = "The bounty is " + gold.ToString() + " gold for this one.";				break;
						case 5:	sReward = "Here is your reward of " + gold.ToString() + " gold";					break;
						case 6:	sReward = "You can have this " + gold.ToString() + " gold for the bounty.";			break;
						case 7:	sReward = "There is a reward of " + gold.ToString() + " gold for this one.";		break;
						case 8:	sReward = "This one was worth " + gold.ToString() + " gold for their crimes.";		break;
						case 9:	sReward = "Their crimes called for a bounty of " + gold.ToString() + " gold.";		break;
					}

					CustomEventSink.InvokeCombatQuestCompleted( from, gold );
					Titles.AwardKarma( from, karma, true );
					Titles.AwardFame( from, fame, true );
					from.SendSound( 0x2E6 );
					from.AddToBackpack ( new Gold( gold ) );

					switch ( Utility.RandomMinMax( 0, 9 ) )
					{
						case 0:	sMessage = "We have been looking for this pirate. " + sReward;	break;
						case 1:	sMessage = "I have heard of this pirate before. " + sReward;	break;
						case 2:	sMessage = "I never thought I would see this pirate brought to justice. " + sReward;	break;
						case 3:	sMessage = "This pirate will plunder no more. " + sReward;	break;
						case 4:	sMessage = "Our galleons are safer now. " + sReward;	break;
						case 5:	sMessage = "The sea is safer because of you. " + sReward;	break;
						case 6:	sMessage = "The sailors at the docks will not believe this. " + sReward;	break;
						case 7:	sMessage = "I have only heard stories about this pirate. " + sReward;	break;
						case 8:	sMessage = "How did you come across this pirate? " + sReward;	break;
						case 9:	sMessage = "Where did you find this pirate? " + sReward;	break;
					}
					this.PrivateOverheadMessage(MessageType.Regular, 1153, false, sMessage, from.NetState);
					dropped.Delete();
					return true;
				}
			}
			else if ( dropped is Head && !from.Blessed )
			{
				if ( IntelligentAction.GetMyEnemies( from, this, false ) )
				{
					string sSay = "You shouldn't be carrying that around with you.";
					this.PrivateOverheadMessage(MessageType.Regular, 1153, false, sSay, from.NetState);
				}
				else
				{
					Head head = (Head)dropped;
					int karma = 0;
					int gold = 0;
					string sMessage = "";
					string sReward = "Here is " + gold.ToString() + " gold for you.";

					if ( head.m_Job == "Thief" )
					{
						karma = Utility.RandomMinMax( 40, 60 );
						gold = Utility.RandomMinMax( 60, 80 );
					}
					else if ( head.m_Job == "Bandit" )
					{
						karma = Utility.RandomMinMax( 20, 30 );
						gold = Utility.RandomMinMax( 30, 40 );
					}
					else if ( head.m_Job == "Brigand" )
					{
						karma = Utility.RandomMinMax( 30, 40 );
						gold = Utility.RandomMinMax( 50, 80 );
					}
					else if ( head.m_Job == "Pirate" )
					{
						karma = Utility.RandomMinMax( 90, 110 );
						gold = Utility.RandomMinMax( 30, 120 );
					}
					else if ( head.m_Job == "Assassin" )
					{
						karma = Utility.RandomMinMax( 60, 80 );
						gold = Utility.RandomMinMax( 80, 120 );
					}

					if ( 0 < gold )
						CustomEventSink.InvokeCombatQuestCompleted( from, gold );

					switch ( Utility.RandomMinMax( 0, 9 ) )
					{
						case 0:	sReward = "Here is " + gold.ToString() + " gold for you.";							break;
						case 1:	sReward = "Take this " + gold.ToString() + " gold for your trouble.";				break;
						case 2:	sReward = "The reward is " + gold.ToString() + " gold.";							break;
						case 3:	sReward = "Here is " + gold.ToString() + " gold for the bounty.";					break;
						case 4:	sReward = "The bounty is " + gold.ToString() + " gold for this one.";				break;
						case 5:	sReward = "Here is your reward of " + gold.ToString() + " gold";					break;
						case 6:	sReward = "You can have this " + gold.ToString() + " gold for the bounty.";			break;
						case 7:	sReward = "There is a reward of " + gold.ToString() + " gold for this one.";		break;
						case 8:	sReward = "This one was worth " + gold.ToString() + " gold for their crimes.";		break;
						case 9:	sReward = "Their crimes called for a bounty of " + gold.ToString() + " gold.";		break;
					}

					if ( head.m_Job == "Thief" || head.m_Job == "Bandit" || head.m_Job == "Brigand" )
					{
						Titles.AwardKarma( from, karma, true );
						from.SendSound( 0x2E6 );
						from.AddToBackpack ( new Gold( gold ) );

						switch ( Utility.RandomMinMax( 0, 9 ) )
						{
							case 0:	sMessage = "We have been looking for this rogue. " + sReward;	break;
							case 1:	sMessage = "I have heard of this thief before. " + sReward;	break;
							case 2:	sMessage = "I never thought I would see this bandit brought to justice. " + sReward;	break;
							case 3:	sMessage = "This rouge will steal no more. " + sReward;	break;
							case 4:	sMessage = "Our gold purses are safer now. " + sReward;	break;
							case 5:	sMessage = "The land is safer because of you. " + sReward;	break;
							case 6:	sMessage = "The others at the guard house will not believe this. " + sReward;	break;
							case 7:	sMessage = "I have only heard stories about this rogue. " + sReward;	break;
							case 8:	sMessage = "How did you come across this thief? " + sReward;	break;
							case 9:	sMessage = "Where did you find this sneak? " + sReward;	break;
						}
						this.PrivateOverheadMessage(MessageType.Regular, 1153, false, sMessage, from.NetState);
						dropped.Delete();
						return true;
					}
					else if ( head.m_Job == "Pirate" )
					{
						Titles.AwardKarma( from, karma, true );
						from.SendSound( 0x2E6 );
						from.AddToBackpack ( new Gold( gold ) );

						switch ( Utility.RandomMinMax( 0, 9 ) )
						{
							case 0:	sMessage = "We have been looking for this pirate. " + sReward;	break;
							case 1:	sMessage = "I have heard of this pirate before. " + sReward;	break;
							case 2:	sMessage = "I never thought I would see this pirate brought to justice. " + sReward;	break;
							case 3:	sMessage = "This pirate will plunder no more. " + sReward;	break;
							case 4:	sMessage = "Our galleons are safer now. " + sReward;	break;
							case 5:	sMessage = "The sea is safer because of you. " + sReward;	break;
							case 6:	sMessage = "The sailors at the docks will not believe this. " + sReward;	break;
							case 7:	sMessage = "I have only heard stories about this pirate. " + sReward;	break;
							case 8:	sMessage = "How did you come across this pirate? " + sReward;	break;
							case 9:	sMessage = "Where did you find this pirate? " + sReward;	break;
						}
						this.PrivateOverheadMessage(MessageType.Regular, 1153, false, sMessage, from.NetState);
						dropped.Delete();
						return true;
					}
					else if ( head.m_Job == "Assassin" )
					{
						Titles.AwardKarma( from, karma, true );
						from.SendSound( 0x2E6 );
						from.AddToBackpack ( new Gold( gold ) );

						switch ( Utility.RandomMinMax( 0, 9 ) )
						{
							case 0:	sMessage = "We have been living in fear of this one. " + sReward;	break;
							case 1:	sMessage = "I have heard others speak of this assassin. " + sReward;	break;
							case 2:	sMessage = "I never thought this assassin existed. " + sReward;	break;
							case 3:	sMessage = "This assassin will kill no more. " + sReward;	break;
							case 4:	sMessage = "Our nobles are safer now. " + sReward;	break;
							case 5:	sMessage = "The shadows are less feared because of you. " + sReward;	break;
							case 6:	sMessage = "Those in the tavern will not believe this. " + sReward;	break;
							case 7:	sMessage = "I have only heard rumors about this assassin. " + sReward;	break;
							case 8:	sMessage = "It is good to see this assassin did not best you. " + sReward;	break;
							case 9:	sMessage = "How did you survive this assassin? " + sReward;	break;
						}
						this.PrivateOverheadMessage(MessageType.Regular, 1153, false, sMessage, from.NetState);
						dropped.Delete();
						return true;
					}
					else
					{
						this.PrivateOverheadMessage(MessageType.Regular, 1153, false, StringCatalog.ResolveByKey(this.Account, "mob.other.i_assume_they_done_you_harm_let_me_rid_you_of_this_thin"), from.NetState);
						dropped.Delete();
						return true;
					}
				}
			}

			return base.OnDragDrop( from, dropped );
		}

		public override void OnAfterSpawn()
		{
			base.OnAfterSpawn();

			Region reg = Region.Find( this.Location, this.Map );

			int clothColor = 0;
			int shieldType = 0;
			int helmType = 0;
			int cloakColor = 0;
			bool griffon = true;

			Item weapon = new VikingSword(); weapon.Delete();

			if ( Server.Misc.Worlds.GetRegionName( this.Map, this.Location ) == "the Village of Whisper" )
			{
				clothColor = 0x96D;		shieldType = 0x1B72;	helmType = 0x140E;		cloakColor = 0x972;		weapon = new Longsword();
			}
			else if ( Server.Misc.Worlds.GetRegionName( this.Map, this.Location ) == "the Town of Glacial Hills" )
			{
				clothColor = 0xB70;		shieldType = 0x1B74;	helmType = 0x1412;		cloakColor = 0xB7A;		weapon = new Kryss();	griffon = false;
			}
			else if ( Server.Misc.Worlds.GetRegionName( this.Map, this.Location ) == "the Village of Springvale" )
			{
				clothColor = 0x595;		shieldType = 0;			helmType = 0x140E;		cloakColor = 0x593;		weapon = new Pike();
			}
			else if ( Server.Misc.Worlds.GetRegionName( this.Map, this.Location ) == "the City of Elidor" )
			{
				clothColor = 0x665;		shieldType = 0x1B7B;	helmType = 0x1412;		cloakColor = 0x664;		weapon = new Katana();	griffon = false;
			}
			else if ( Server.Misc.Worlds.GetRegionName( this.Map, this.Location ) == "the Village of Islegem" )
			{
				clothColor = 0x7D1;		shieldType = 0;			helmType = 0x140E;		cloakColor = 0x7D6;		weapon = new Spear();	weapon.ItemID = 0xF62;
			}
			else if ( Server.Misc.Worlds.GetRegionName( this.Map, this.Location ) == "Greensky Village" )
			{
				clothColor = 0x7D7;		shieldType = 0;			helmType = 0x1412;		cloakColor = 0x7DA;		weapon = new Bardiche();	griffon = false;
			}
			else if ( Server.Misc.Worlds.GetRegionName( this.Map, this.Location ) == "the Port of Dusk" )
			{
				clothColor = 0x601;		shieldType = 0x1B76;	helmType = 0x140E;		cloakColor = 0x600;		weapon = new Cutlass();
			}
			else if ( Server.Misc.Worlds.GetRegionName( this.Map, this.Location ) == "the Port of Starguide" )
			{
				clothColor = 0x751;		shieldType = 0;			helmType = 0x1412;		cloakColor = 0x758;		weapon = new BladedStaff();	griffon = false;
			}
			else if ( Server.Misc.Worlds.GetRegionName( this.Map, this.Location ) == "the Village of Portshine" )
			{
				clothColor = 0x847;		shieldType = 0x1B7A;	helmType = 0x140E;		cloakColor = 0x851;		weapon = new Mace();
			}
			else if ( Server.Misc.Worlds.GetRegionName( this.Map, this.Location ) == "the Ranger Outpost" )
			{
				clothColor = 0x598;		shieldType = 0;			helmType = 0x140E;		cloakColor = 0x83F;		weapon = new Spear();	weapon.ItemID = 0xF62;
			}
			else if ( Land == Land.Lodoria )
			{
				clothColor = 0x6E4;		shieldType = 0x1BC4;	helmType = 0x1412;		cloakColor = 0x6E7;		weapon = new Scimitar();
			}
			else if ( Server.Misc.Worlds.GetRegionName( this.Map, this.Location ) == "the Lunar City of Dawn" )
			{
				clothColor = 0x9C4;		shieldType = 0;			helmType = 11121;		cloakColor = 0x9C4;		weapon = new QuarterStaff();
			}
			else if ( Server.Misc.Worlds.GetRegionName( this.Map, this.Location ) == "The Town of Devil Guard" || Server.Misc.Worlds.GetRegionName( this.Map, this.Location ) == "The Farmland of Devil Guard" )
			{
				clothColor = 0x430;		shieldType = 0;			helmType = 0x140E;		cloakColor = 0;			weapon = new LargeBattleAxe();
			}
			else if ( Server.Misc.Worlds.GetRegionName( this.Map, this.Location ) == "the Town of Moon" )
			{
				clothColor = 0x8AF;		shieldType = 0x1B72;	helmType = 0x1412;		cloakColor = 0x972;		weapon = new Longsword();
			}
			else if ( Server.Misc.Worlds.GetRegionName( this.Map, this.Location ) == "the Village of Grey" )
			{
				clothColor = 0;			shieldType = 0;			helmType = 0x140E;		cloakColor = 0x763;		weapon = new Halberd();
			}
			else if ( Server.Misc.Worlds.GetRegionName( this.Map, this.Location ) == "the City of Montor" )
			{
				clothColor = 0x96F;		shieldType = 0x1B74;	helmType = 0x1412;		cloakColor = 0x529;		weapon = new Broadsword();
			}
			else if ( Server.Misc.Worlds.GetRegionName( this.Map, this.Location ) == "the Village of Fawn" )
			{
				clothColor = 0x59D;		shieldType = 0;			helmType = 0x140E;		cloakColor = 0x59C;		weapon = new DoubleAxe();
			}
			else if ( Server.Misc.Worlds.GetRegionName( this.Map, this.Location ) == "the Village of Yew" )
			{
				clothColor = 0x83C;		shieldType = 0;			helmType = 0x1412;		cloakColor = 0x850;		weapon = new Spear();	weapon.ItemID = 0xF62;
			}
			else if ( Server.Misc.Worlds.GetRegionName( this.Map, this.Location ) == "Iceclad Fisherman's Village" || Server.Misc.Worlds.GetRegionName( this.Map, this.Location ) == "the Town of Mountain Crest" || Server.Misc.Worlds.GetRegionName( this.Map, this.Location ) == "Glacial Coast Village" )
			{
				clothColor = 0x482;		shieldType = 0;			helmType = 0x140E;		cloakColor = 0x47E;		weapon = new Bardiche();
			}
			else if ( Server.Misc.Worlds.GetRegionName( this.Map, this.Location ) == "the Undercity of Umbra" )
			{
				clothColor = 0x964;		shieldType = 0x1BC3;	helmType = 0x140E;		cloakColor = 0;		weapon = new Longsword();
			}
			else if ( Land == Land.UmberVeil )
			{
				clothColor = 0xA5D;		shieldType = 0;			helmType = 0x140E;		cloakColor = 0x96D;		weapon = new Halberd();
			}
			else if ( Server.Misc.Worlds.GetRegionName( this.Map, this.Location ) == "the City of Kuldara" )
			{
				clothColor = 0x965;		shieldType = 0x1BC3;	helmType = 0x140E;		cloakColor = 0x845;		weapon = new Maul();
			}
			else if ( Land == Land.IslesDread )
			{
				clothColor = 0x978;		shieldType = 0x1B7A;	helmType = 0;			cloakColor = 0x973;		weapon = new VikingSword();
			}
			else if ( Server.Misc.Worlds.GetRegionName( this.Map, this.Location ) == "the Village of Barako" )
			{
				clothColor = 0x515;		shieldType = 0x1B72;	helmType = 0x2645;		cloakColor = 0x58D;		weapon = new WarMace();
			}
			else if ( Land == Land.Savaged )
			{
				clothColor = 0x515;		shieldType = 0;			helmType = 0x140E;		cloakColor = 0x59D;		weapon = new Spear();	weapon.ItemID = 0xF62;
			}
			else if ( Land == Land.Serpent )
			{
				clothColor = 0x515;		shieldType = 0;			helmType = 0x2FBB;		cloakColor = 0;			weapon = new LargeBattleAxe();
			}
			else
			{
				clothColor = 0x9C4;		shieldType = 0x1BC4;	helmType = 0x140E;		cloakColor = 0x845;		weapon = new VikingSword();
			}

			Item oathWeapon = new OathGuardSword( this );
			oathWeapon.Movable = false;
			AddItem( oathWeapon );
			m_OathWeaponSerial = oathWeapon.Serial;

			Item arms = new RingmailArms();
			Item tunic = new PlateChest();
			Item legs = new PlateLegs();
			Item neck = new PlateGorget();
			Item hand = new PlateGloves();
			Item foot = new Boots( );

			if ( Land == Land.IslesDread )
			{
				tunic.ItemID = 0x5652;	tunic.Name = "tunic";
				if ( this.Female )
				{
					tunic.ItemID = 0x563E;
					Utility.AssignRandomHair( this );
				}
				else
				{
					Utility.AssignRandomHair( this );
					FacialHairItemID = Utility.RandomList( 0, 8254, 8255, 8256, 8257, 8267, 8268, 8269 );
				}

				this.HairHue = 0x455;
				this.FacialHairHue = 0x455;

				arms.ItemID = 22093;	arms.Name = "sleeves";
				legs.ItemID = 7176;		legs.Name = "skirt";
				neck.ItemID = 0x5650;	neck.Name = "amulet";
				hand.ItemID = 0x564E;	hand.Name = "gloves";
				foot.ItemID = 5901;		foot.Name = "sandals";
			}
			else if ( Land == Land.Luna )
			{
				tunic.ItemID = 7939;	tunic.Name = "robe";
				if ( this.Female )
				{
					Utility.AssignRandomHair( this );
				}
				else
				{
					Utility.AssignRandomHair( this );
					FacialHairItemID = Utility.RandomList( 0, 8254, 8255, 8256, 8257, 8267, 8268, 8269 );
				}

				this.HairHue = Utility.RandomHairHue();
				this.FacialHairHue = this.HairHue;

				arms.ItemID = 22093;	arms.Name = "sleeves";
				legs.ItemID = 7176;		legs.Name = "skirt";
				neck.ItemID = 0x5650;	neck.Name = "amulet";
				hand.ItemID = 0x564E;	hand.Name = "gloves";
				foot.ItemID = 5901;		foot.Name = "sandals";
			}

			AddItem( tunic );
			AddItem( arms );
			AddItem( legs );
			AddItem( neck );
			AddItem( hand );
			AddItem( foot );

			if ( helmType > 0 )
			{
				PlateHelm helm = new PlateHelm();
					helm.ItemID = helmType;
					helm.Name = "helm";
						if ( helmType == 11121 ){ helm.Name = "hood"; }
					AddItem( helm );
			}
			if ( shieldType > 0 )
			{
				ChaosShield shield = new ChaosShield();
					shield.ItemID = shieldType;
					shield.Name = "shield";
					AddItem( shield );
			}

			MorphingTime.ColorMyClothes( this, clothColor, 0 );
			MorphingTime.ColorMyArms( this, 0, 0 );

			if ( cloakColor > 0 )
			{
				Cloak cloak = new Cloak();
					cloak.Hue = cloakColor;
					AddItem( cloak );
			}

			Server.Misc.MorphingTime.CheckMorph( this );

			if ( Utility.RandomBool() && !Server.Misc.Worlds.InBuilding( this ) && this.Map != Map.SerpentIsland )
			{
				BaseMount mount = new EvilMount();

				if ( this.Map == Map.SavagedEmpire )
				{
					mount.Body = 0x11C; mount.ItemID = 0x3E92; mount.Hue = Utility.RandomList( 0xB79, 0xB19, 0xAEF, 0xACE, 0xAB0 );
				}
				else if ( this.Map == Map.IslesDread )
				{
					mount.Body = 23; mount.ItemID = 23;
					if ( Utility.RandomBool() ){ mount.Body = 177; mount.ItemID = 177; }
				}
				else if ( this.Map == Map.Lodor )
				{
					mount.Body = 188; mount.ItemID = 0x3EB8;
					if ( griffon ){ mount.Body = 0x31F; mount.ItemID = 0x3EBE; }
				}
				else
				{
					mount.Body = 0xE2; mount.ItemID = 594;
				}

				Server.Mobiles.BaseMount.Ride( mount, this );
			}
			ProcessClothing();
		}

		public override void OnGaveMeleeAttack( Mobile defender )
		{
			switch ( Utility.Random( 8 ))		   
			{
			case 0: CitizenLocalization.SayLocalized(this, StringCatalog.ResolveByKey(this.Account, "mob.other.die_villian")); break;
			case 1: CitizenLocalization.SayLocalized(this, StringCatalog.ResolveByKey(this.Account, "mob.other.i_will_bring_you_justice")); break;
			case 2: CitizenLocalization.SayLocalizedFormat(this, StringCatalog.ResolveByKey(this.Account, "mob.fmt.so_0_your_evil_ends_here"), defender.Name); break;
			case 3: CitizenLocalization.SayLocalizedFormat(this, StringCatalog.ResolveByKey(this.Account, "mob.fmt.we_have_been_told_to_watch_for_0"), defender.Name); break;
			case 4: CitizenLocalization.SayLocalizedFormat(this, StringCatalog.ResolveByKey(this.Account, "mob.fmt.fellow_guardsmen_0_is_here"), defender.Name); break;
			case 5: CitizenLocalization.SayLocalizedFormat(this, StringCatalog.ResolveByKey(this.Account, "mob.fmt.we_have_ways_of_dealing_with_the_likes_of_0"), defender.Name); break;
			case 6: CitizenLocalization.SayLocalizedFormat(this, StringCatalog.ResolveByKey(this.Account, "mob.fmt.give_up_we_do_not_fear_0"), defender.Name); break;
			case 7: CitizenLocalization.SayLocalizedFormat(this, StringCatalog.ResolveByKey(this.Account, "mob.fmt.so_0_i_sentence_you_to_death"), defender.Name); break;
			};
		}

		public override bool IsEnemy( Mobile m )
		{
			if ( !IntelligentAction.GetMyEnemies( m, this, true ) )
				return false;

			if ( m.Region != this.Region && !MySettings.S_GuardsPatrolOutside )
				return false;

			if ( MySettings.S_GuardsSentenceDeath || ( m is BaseCreature && ((BaseCreature)m).ControlMaster == null ) )
			{
				this.Location = m.Location;
				this.Combatant = m;
				this.Warmode = true;
			}

			return true;
		}

		public override void GetContextMenuEntries( Mobile from, List<ContextMenuEntry> list ) 
		{ 
			base.GetContextMenuEntries( from, list ); 
			list.Add( new SpeechGumpEntry( from, this ) ); 
		} 

		public class SpeechGumpEntry : ContextMenuEntry
		{
			private Mobile m_Mobile;
			private Mobile m_Giver;
			
			public SpeechGumpEntry( Mobile from, Mobile giver ) : base( 6146, 3 )
			{
				m_Mobile = from;
				m_Giver = giver;
			}

			public override void OnClick()
			{
			    if( !( m_Mobile is PlayerMobile ) )
				return;
				
				PlayerMobile mobile = (PlayerMobile) m_Mobile;
				{
					if ( ! mobile.HasGump( typeof( SpeechGump ) ) )
					{
						Server.Misc.IntelligentAction.SayHey( m_Giver );
						mobile.SendGump(new SpeechGump( mobile, "The Duties Of The Guard", SpeechFunctions.SpeechText( m_Giver, m_Mobile, "Guard" ) ));
					}
				}

				ArrayList wanted = new ArrayList();
				int w = 0;
				foreach( Mobile wm in World.Mobiles.Values )
				{
					if( !wm.Deleted && wm is PlayerMobile )
					{

						if ( ((PlayerMobile)wm).CharacterWanted != null && ((PlayerMobile)wm).CharacterWanted != "" )
						{
							wanted.Add( wm );
							w++;
						}
					}
				}

				int wChoice = Utility.RandomMinMax( 1, w );
				int c = 0;
				for ( int i = 0; i < wanted.Count; ++i )
				{
					c++;
					if ( c == wChoice )
					{
						Mobile m = ( Mobile )wanted[ i ];
						GuardNote note = new GuardNote();
						note.ScrollText = ((PlayerMobile)m).CharacterWanted;
						m_Mobile.AddToBackpack( note );
						CitizenLocalization.SayLocalized(m_Giver, StringCatalog.ResolveByKey(null, "mob.other.here_is_a_note_citizen_be_on_the_lookout"));
					}
				}
            }
        }

		private class OathWindowTimer : Timer
		{
			private TownGuards m_Owner;

			public OathWindowTimer( TownGuards owner ) : base( TimeSpan.FromSeconds( 30 ), TimeSpan.FromSeconds( 30 ) )
			{
				m_Owner = owner;
				Priority = TimerPriority.FiftyMS;
			}

			protected override void OnTick()
			{
				if ( m_Owner.Deleted || m_Owner.m_OathBreaks <= 0 || m_Owner.m_OathWindow == DateTime.MinValue )
				{
					Stop();
					m_Owner.m_OathWindowTimer = null;
					return;
				}

				var elapsed = DateTime.UtcNow - m_Owner.m_OathWindow;

				if ( elapsed >= TownGuards.OathWindowDuration )
				{
					// Analytics event D: window expired
					PlayerMobile actorD = m_Owner.GetAnyAssailantActor();
					if ( actorD != null )
					{
						var extraD = new Dictionary<string, string>();
						extraD["knockdowns_achieved"] = m_Owner.m_OathBreaks.ToString( System.Globalization.CultureInfo.InvariantCulture );
						extraD["failure_reason"] = "window_expired";
						AnalyticsLogger.LogCustomEvent( actorD, "guard_oathbreak_failed", "guard_oathbreak", "window_expired", extraD );
					}

					// Time expired: reset state
					m_Owner.ResetOathState();
					Stop();
					m_Owner.m_OathWindowTimer = null;
					return;
				}

				TimeSpan remaining = TownGuards.OathWindowDuration - elapsed;
				m_Owner.NameMod = String.Format(
					System.Globalization.CultureInfo.InvariantCulture,
					"(Oath: {0}/{1} - {2} left)",
					m_Owner.m_OathBreaks,
					TownGuards.OathBreaksRequired,
					remaining.ToString( @"mm\:ss" ) );
				m_Owner.InvalidateProperties();

				double leftMinutes = remaining.TotalMinutes;

				// 10, 5, 2, 1, 0.5 minute thresholds
				if ( leftMinutes <= 10 && m_Owner.m_OathWindowLastThresholdIndex < 0 )
				{
					m_Owner.m_OathWindowLastThresholdIndex = 0;
					m_Owner.PublicOverheadMessage( MessageType.Regular, 38, false, StringCatalog.ResolveByKey( m_Owner.Account, "mob.other.oath_window_warning_10min" ) );
				}
				else if ( leftMinutes <= 5 && m_Owner.m_OathWindowLastThresholdIndex < 1 )
				{
					m_Owner.m_OathWindowLastThresholdIndex = 1;
					m_Owner.PublicOverheadMessage( MessageType.Regular, 38, false, StringCatalog.ResolveByKey( m_Owner.Account, "mob.other.oath_window_warning_5min" ) );
				}
				else if ( leftMinutes <= 2 && m_Owner.m_OathWindowLastThresholdIndex < 2 )
				{
					m_Owner.m_OathWindowLastThresholdIndex = 2;
					m_Owner.PublicOverheadMessage( MessageType.Regular, 38, false, StringCatalog.ResolveByKey( m_Owner.Account, "mob.other.oath_window_warning_2min" ) );
				}
				else if ( leftMinutes <= 1 && m_Owner.m_OathWindowLastThresholdIndex < 3 )
				{
					m_Owner.m_OathWindowLastThresholdIndex = 3;
					m_Owner.PublicOverheadMessage( MessageType.Regular, 68, false, StringCatalog.ResolveByKey( m_Owner.Account, "mob.other.oath_window_warning_1min" ) );
				}
				else if ( leftMinutes <= 0.5 && m_Owner.m_OathWindowLastThresholdIndex < 4 )
				{
					m_Owner.m_OathWindowLastThresholdIndex = 4;
					m_Owner.PublicOverheadMessage( MessageType.Regular, 68, false, StringCatalog.ResolveByKey( m_Owner.Account, "mob.other.oath_window_warning_30s" ) );
				}
			}
		}

		public override void OnDamage( int amount, Mobile from, bool willKill )
		{
			base.OnDamage( amount, from, willKill );

			PlayerMobile pm = from as PlayerMobile;
			if ( pm == null || pm.Deleted )
				return;

			if ( m_Assailants.Add( pm.Serial ) )
			{
				m_AssailantCount = m_Assailants.Count;
			}

			// First engagement (attempt start)
			if ( m_EngagementStart == DateTime.MinValue )
			{
				m_EngagementStart = DateTime.UtcNow;
				++m_AttemptCount;
				OathStatusGump.ClearDismissed( pm );

				// Analytics event A: first attacker starts an attempt
				var extraA = new Dictionary<string, string>();
				extraA["guard_serial"] = this.Serial.ToString();
				extraA["guard_location"] = Region != null ? Region.Name : "";
				extraA["attempt_number"] = m_AttemptCount.ToString( System.Globalization.CultureInfo.InvariantCulture );
				AnalyticsLogger.LogCustomEvent(
					pm,
					"guard_oathbreak_attempt",
					"guard_oathbreak",
					"attempt_start",
					extraA );
			}

			OathStatusGump.NotifyAssailant( pm, this );
		}

		private void StartOathWindowTimer()
		{
			if ( m_OathWindowTimer != null )
				return;

			m_OathWindowLastThresholdIndex = -1;
			m_OathWindowTimer = new OathWindowTimer( this );
			m_OathWindowTimer.Start();
		}

		private void ResetOathState()
		{
			CloseOathStatusGumps();

			m_OathBreaks = 0;
			m_OathWindow = DateTime.MinValue;
			m_Assailants.Clear();
			m_AssailantCount = 0;
			m_EngagementStart = DateTime.MinValue;
			m_OathWindowLastThresholdIndex = -1;
			NameMod = null;
		}

		private PlayerMobile GetAnyAssailantActor()
		{
			foreach ( Serial s in m_Assailants )
			{
				Mobile m = World.FindMobile( s );
				if ( m is PlayerMobile pm && !pm.Deleted )
					return pm;
			}

			return null;
		}

		public override bool OnBeforeDeath()
		{
			++m_OathBreaks;

			bool withinWindow = ( m_OathWindow != DateTime.MinValue ) && ( DateTime.UtcNow - m_OathWindow <= OathWindowDuration );

			if ( m_OathBreaks == 1 )
			{
				m_OathWindow = DateTime.UtcNow;
				StartOathWindowTimer();

				// Analytics event B: first knockdown fell
				PlayerMobile actorB = GetAnyAssailantActor();
				if ( actorB != null )
				{
					double engagementSeconds = m_EngagementStart == DateTime.MinValue ? 0 : ( DateTime.UtcNow - m_EngagementStart ).TotalSeconds;
					var extraB = new Dictionary<string, string>();
					extraB["knockdown_number"] = "1";
					extraB["engagement_duration_seconds"] = engagementSeconds.ToString( "0.##", System.Globalization.CultureInfo.InvariantCulture );
					AnalyticsLogger.LogCustomEvent( actorB, "guard_oathbreak_felled", "guard_oathbreak", "knockdown_1", extraB );
				}

				CitizenLocalization.SayLocalized( this, StringCatalog.ResolveByKey( this.Account, "mob.other.in_vas_mani" ) );
				this.Hits = this.HitsMax;
				this.FixedParticles( 0x376A, 9, 32, 5030, EffectLayer.Waist );
				this.PlaySound( 0x202 );
				RefreshOathStatusGumps();
				return false;
			}
			else if ( m_OathBreaks == 2 )
			{
				// Analytics event B: second knockdown fell
				PlayerMobile actorB = GetAnyAssailantActor();
				if ( actorB != null )
				{
					double engagementSeconds = m_EngagementStart == DateTime.MinValue ? 0 : ( DateTime.UtcNow - m_EngagementStart ).TotalSeconds;
					var extraB = new Dictionary<string, string>();
					extraB["knockdown_number"] = "2";
					extraB["engagement_duration_seconds"] = engagementSeconds.ToString( "0.##", System.Globalization.CultureInfo.InvariantCulture );
					AnalyticsLogger.LogCustomEvent( actorB, "guard_oathbreak_felled", "guard_oathbreak", "knockdown_2", extraB );
				}

				CitizenLocalization.SayLocalized( this, StringCatalog.ResolveByKey( this.Account, "mob.other.oathbreak_phase2" ) );
				this.Hits = this.HitsMax;
				this.FixedParticles( 0x376A, 9, 32, 5030, EffectLayer.Waist );
				this.PlaySound( 0x202 );
				RefreshOathStatusGumps();
				return false;
			}

			// 3rd (or later) knockdown: allow real death only if within window
			if ( withinWindow && m_OathBreaks >= OathBreaksRequired )
			{
				CitizenLocalization.SayLocalized( this, StringCatalog.ResolveByKey( this.Account, "mob.other.oathbreak_phase3" ) );
				m_KeepCorpseOnOathDeath = true;
				return base.OnBeforeDeath();
			}

			// Window expired: treat this knockdown as a new first break.
			ResetOathState();
			m_OathBreaks = 1;
			m_OathWindow = DateTime.UtcNow;
			StartOathWindowTimer();

			CitizenLocalization.SayLocalized( this, StringCatalog.ResolveByKey( this.Account, "mob.other.in_vas_mani" ) );
			this.Hits = this.HitsMax;
			this.FixedParticles( 0x376A, 9, 32, 5030, EffectLayer.Waist );
			this.PlaySound( 0x202 );
			RefreshOathStatusGumps();
			return false;
		}

		public override void OnDeath( Container c )
		{
			bool oathDeath = m_KeepCorpseOnOathDeath;
			int spawnerSerial = SpawnerID;
			Point3D respawnAt = Home;
			Map respawnMap = Map;
			int respawnRange = RangeHome;

			base.OnDeath( c );

			// Stop oath timer
			if ( m_OathWindowTimer != null )
			{
				m_OathWindowTimer.Stop();
				m_OathWindowTimer = null;
			}

			// Consequences for assailants
			foreach ( Serial s in m_Assailants )
			{
				Mobile m = World.FindMobile( s );
				if ( m is PlayerMobile pm )
				{
					pm.Criminal = true;
					pm.Kills += 2;
				}
			}

			// Town Crier broadcast (adventures.txt)
			LoggingFunctions.LogEvent(
				"Guard " + Name + " of " + ( Region != null ? Region.Name : "" ) + " oath broken!",
				LogEventType.Adventures,
				true );

			// Analytics event C: oathbreak completed
			PlayerMobile actorC = GetAnyAssailantActor();
			if ( actorC != null )
			{
				var extraC = new Dictionary<string, string>();
				extraC["weapon_serial"] = m_OathWeaponSerial.Value.ToString();
				extraC["assailant_count"] = m_AssailantCount.ToString();
				AnalyticsLogger.LogCustomEvent( actorC, "guard_oathbreak_completed", "guard_oathbreak", "completed", extraC );
			}

			m_KeepCorpseOnOathDeath = false;
			ResetOathState();

			if ( oathDeath )
				PremiumSpawner.RequestTownGuardImmediateRespawn( spawnerSerial, respawnAt, respawnMap, respawnRange );
		}

		public TownGuards( Serial serial ) : base( serial ) 
		{ 
		} 

		public override void Serialize( GenericWriter writer ) 
		{ 
			base.Serialize( writer ); 
			writer.Write( (int) 1 ); // version

			writer.Write( m_OathBreaks );
			writer.Write( m_OathWindow );
			writer.Write( m_AssailantCount );
			writer.Write( m_AttemptCount );
			writer.Write( m_EngagementStart );

			writer.Write( m_Assailants.Count );
			foreach ( Serial s in m_Assailants )
				writer.Write( s );
		} 

		public override void Deserialize( GenericReader reader ) 
		{ 
			base.Deserialize( reader ); 
			int version = reader.ReadInt();

			switch ( version )
			{
				case 1:
					m_OathBreaks = reader.ReadInt();
					m_OathWindow = reader.ReadDateTime();
					m_AssailantCount = reader.ReadInt();
					m_AttemptCount = reader.ReadInt();
					m_EngagementStart = reader.ReadDateTime();

					int count = reader.ReadInt();
					m_Assailants = new HashSet<Serial>();
					for ( int i = 0; i < count; i++ )
						m_Assailants.Add( (Serial)reader.ReadInt() );
					break;
			}
		} 
	} 
}   
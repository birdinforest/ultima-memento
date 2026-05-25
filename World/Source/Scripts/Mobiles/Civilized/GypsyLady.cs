using System;
using System.Collections.Generic;
using Server;
using Server.Targeting;
using Server.Items;
using Server.Network;
using Server.ContextMenus;
using Server.Misc;
using Server.Mobiles;
using System.Collections;
using Server.Gumps;
using Server.Localization;

namespace Server.Mobiles
{
	public class GypsyLady : BasePerson
	{
		public override string TalkGumpTitle{ get{ return "Visions of the Truth"; } }
		public override string TalkGumpSubject{ get{ return "Gypsy"; } }

		private DateTime m_NextTalk;
		public DateTime NextTalk{ get{ return m_NextTalk; } set{ m_NextTalk = value; } }
		public override void OnMovement( Mobile m, Point3D oldLocation )
		{
			if( m is PlayerMobile )
			{
				if ( DateTime.Now >= m_NextTalk && InRange( m, 4 ) && InLOS( m ) )
				{
					switch ( Utility.Random( 45 ))
					{
						case 0: Say(StringCatalog.ResolveByKey(this.Account, "mob.other.a_reunion_must_not_occur_or_the_unlucky_chimera_must_fo")); break;
						case 1: Say(StringCatalog.ResolveByKey(this.Account, "mob.other.an_injury_shall_happen")); break;
						case 2: Say(StringCatalog.ResolveByKey(this.Account, "mob.other.he_shall_not_assimilate_with_the_proud_youth")); break;
						case 3: Say(StringCatalog.ResolveByKey(this.Account, "mob.other.they_will_not_weave_near_an_altar")); break;
						case 4: Say(StringCatalog.ResolveByKey(this.Account, "mob.other.she_will_finally_intrude")); break;
						case 5: Say(StringCatalog.ResolveByKey(this.Account, "mob.other.the_diamond_possum_shall_not_deflect_near_a_fortress_on")); break;
						case 6: Say(StringCatalog.ResolveByKey(this.Account, "mob.other.a_relationship_ending_will_finally_happen_with_the_tire")); break;
						case 7: Say(StringCatalog.ResolveByKey(this.Account, "mob.other.the_clever_hamster_will_trespass_with_the_saffron_youth")); break;
						case 8: Say(StringCatalog.ResolveByKey(this.Account, "mob.other.a_betrayal_will_finally_happen_or_he_will_babble_at_the")); break;
						case 9: Say(StringCatalog.ResolveByKey(this.Account, "mob.other.she_will_finally_gutter_in_a_market_before_it_is_too_la")); break;
						case 10: Say(StringCatalog.ResolveByKey(this.Account, "mob.other.an_introduction_will_finally_happen_or_the_seductive_vi")); break;
						case 11: Say(StringCatalog.ResolveByKey(this.Account, "mob.other.the_broken_hearted_grandmother_will_act_or_he_shall_not")); break;
						case 12: Say(StringCatalog.ResolveByKey(this.Account, "mob.other.a_defeat_will_not_take_place_in_a_castle_on_a_journey")); break;
						case 13: Say(StringCatalog.ResolveByKey(this.Account, "mob.other.the_fearless_trader_shall_not_look_with_the_violet_drui")); break;
						case 14: Say(StringCatalog.ResolveByKey(this.Account, "mob.other.the_honest_runaway_will_succumb_with_the_hungry_hare_be")); break;
						case 15: Say(StringCatalog.ResolveByKey(this.Account, "mob.other.it_will_judge_in_the_summer")); break;
						case 16: Say(StringCatalog.ResolveByKey(this.Account, "mob.other.the_emerald_lion_will_never_scare_for_the_sake_of_winte")); break;
						case 17: Say(StringCatalog.ResolveByKey(this.Account, "mob.other.she_will_finally_famish_and_a_recovery_will_not_happen")); break;
						case 18: Say(StringCatalog.ResolveByKey(this.Account, "mob.other.a_fight_shall_take_place_for_the_sake_of_willpower")); break;
						case 19: Say(StringCatalog.ResolveByKey(this.Account, "mob.other.the_fanatical_wizard_shall_not_wax_near_a_holy_site_in")); break;
						case 20: Say(StringCatalog.ResolveByKey(this.Account, "mob.other.she_shall_not_ascend_after_sunset")); break;
						case 21: Say(StringCatalog.ResolveByKey(this.Account, "mob.other.it_will_never_pray_with_the_shy_zealot")); break;
						case 22: Say(StringCatalog.ResolveByKey(this.Account, "mob.other.a_meeting_must_happen_with_the_malicious_cook")); break;
						case 23: Say(StringCatalog.ResolveByKey(this.Account, "mob.other.the_remorseless_muse_will_fraternize_at_the_bridge")); break;
						case 24: Say(StringCatalog.ResolveByKey(this.Account, "mob.other.the_russet_berserker_must_gasp_or_the_remorseless_slave")); break;
						case 25: Say(StringCatalog.ResolveByKey(this.Account, "mob.other.he_shall_fence_with_the_black_countess_near_a_portal")); break;
						case 26: Say(StringCatalog.ResolveByKey(this.Account, "mob.other.an_introduction_will_not_happen_and_the_intelligent_cha")); break;
						case 27: Say(StringCatalog.ResolveByKey(this.Account, "mob.other.the_hasty_hostler_must_jump")); break;
						case 28: Say(StringCatalog.ResolveByKey(this.Account, "mob.other.an_agreement_must_take_place_with_the_broken_hearted_cl")); break;
						case 29: Say(StringCatalog.ResolveByKey(this.Account, "mob.other.it_will_finally_benefit_in_the_citadel_in_the_age_of_dr")); break;
						case 30: Say(StringCatalog.ResolveByKey(this.Account, "mob.other.a_financial_difficulty_will_never_take_place_or_the_clu")); break;
						case 31: Say(StringCatalog.ResolveByKey(this.Account, "mob.other.she_shall_weld_or_he_will_finally_crush_during_the_grow")); break;
						case 32: Say(StringCatalog.ResolveByKey(this.Account, "mob.other.the_greedy_artist_shall_lace_and_the_garnet_general_mus")); break;
						case 33: Say(StringCatalog.ResolveByKey(this.Account, "mob.other.the_arrogant_rogue_must_not_comply_with_the_indigo_robb")); break;
						case 34: Say(StringCatalog.ResolveByKey(this.Account, "mob.other.the_lavender_summoner_will_crush_in_a_time_of_truth")); break;
						case 35: Say(StringCatalog.ResolveByKey(this.Account, "mob.other.a_contest_must_not_happen_after_the_first_frost")); break;
						case 36: Say(StringCatalog.ResolveByKey(this.Account, "mob.other.she_must_not_consent_in_the_age_of_entropy")); break;
						case 37: Say(StringCatalog.ResolveByKey(this.Account, "mob.other.the_deluded_pony_will_not_forget_in_a_graveyard_on_a_wi")); break;
						case 38: Say(StringCatalog.ResolveByKey(this.Account, "mob.other.a_reversal_of_fortune_shall_not_happen_and_a_fall_shall")); break;
						case 39: Say(StringCatalog.ResolveByKey(this.Account, "mob.other.she_will_ensure_in_the_citadel")); break;
						case 40: Say(StringCatalog.ResolveByKey(this.Account, "mob.other.the_lazy_juggler_will_finally_enquire_and_a_loss_shall")); break;
						case 41: Say(StringCatalog.ResolveByKey(this.Account, "mob.other.a_promise_will_finally_take_place_and_it_shall_not_both")); break;
						case 42: Say(StringCatalog.ResolveByKey(this.Account, "mob.other.he_must_not_weary")); break;
						case 43: Say(StringCatalog.ResolveByKey(this.Account, "mob.other.the_orange_donkey_shall_not_gutter_at_the_bridge")); break;
						case 44: Say(StringCatalog.ResolveByKey(this.Account, "mob.other.the_word_to_the_dark_mage_depths_is_bravoka")); break;
					};
					m_NextTalk = (DateTime.Now + TimeSpan.FromSeconds( 30 ));
				}
			}
		}

		[Constructable]
		public GypsyLady() : base( )
		{
			Hue = Utility.RandomSkinColor();
			NameHue = -1;

			Body = 0x191;
			Female = true;
			Name = NameList.RandomName( "female" );
			Title = "the gypsy";

			AddItem( new Kilt( Utility.RandomDyedHue() ) );
			AddItem( new Shirt( Utility.RandomDyedHue() ) );
			AddItem( new ThighBoots() );
			AddItem( new SkullCap( Utility.RandomDyedHue() ) );

			SetSkill( SkillName.Cooking, 65, 88 );
			SetSkill( SkillName.Snooping, 65, 88 );
			SetSkill( SkillName.Stealing, 65, 88 );
			SetSkill( SkillName.Spiritualism, 65, 88 );
			SetSkill( SkillName.FistFighting, 100 );

			SetStr( 100 );
			SetDex( 100 );
			SetInt( 100 );

			SetDamage( 15, 20 );
			SetDamageType( ResistanceType.Physical, 100 );

			SetResistance( ResistanceType.Physical, 35, 45 );
			SetResistance( ResistanceType.Fire, 25, 30 );
			SetResistance( ResistanceType.Cold, 25, 30 );
			SetResistance( ResistanceType.Poison, 10, 20 );
			SetResistance( ResistanceType.Energy, 10, 20 );

			VirtualArmor = 30;

			Utility.AssignRandomHair( this );
			HairHue = Utility.RandomHairHue();
			FacialHairItemID = 0;
		}

		private class TruthEntry : ContextMenuEntry
		{
			private GypsyLady m_GypsyLady;
			private Mobile m_From;

			public TruthEntry( GypsyLady GypsyLady, Mobile from ) : base( 2058, 12 )
			{
				m_GypsyLady = GypsyLady;
				m_From = from;
			}

			public override void OnClick()
			{
				m_GypsyLady.FindTruth( m_From );
			}
		}

        public void FindTruth( Mobile from )
        {
            if ( Deleted || !from.Alive )
                return;

			CitizenLocalization.SayToLocalized(this, from, "So you want me to reveal the truth of a parchment for you?");

            from.Target = new RevealTarget(this);
        }

        private class RevealTarget : Target
        {
            private GypsyLady m_GypsyLady;

            public RevealTarget( GypsyLady mage ) : base(12, false, TargetFlags.None)
            {
                m_GypsyLady = mage;
            }

            protected override void OnTarget( Mobile from, object targeted )
            {
				Container pack = from.Backpack;

				if ( targeted is ScrollClue )
				{
					ScrollClue scroll = (ScrollClue)targeted;

					int nCost = scroll.ScrollLevel * 100;

					if ( BaseVendor.BeggingPose(from) > 0 ) // LET US SEE IF THEY ARE BEGGING
					{
						nCost = nCost - (int)( ( from.Skills[SkillName.Begging].Value * 0.005 ) * nCost );
					}

					int toConsume = nCost;

					if ( scroll.ScrollIntelligence > 0 )
					{
						CitizenLocalization.SayToLocalized(m_GypsyLady, from, "That parchment hasn't been deciphered yet.");
					}
					else if (pack.ConsumeTotal(typeof(Gold), toConsume))
					{
						string WillSay = "";
						string WillSayZh = "";

						switch ( Utility.RandomMinMax( 0, 3 ) ) 
						{
							case 0: WillSay = "The spirits tell me that this parchment is"; WillSayZh = "神灵告知我，这份羊皮纸"; break;
							case 1: WillSay = "My mind is showing me that this parchment is"; WillSayZh = "我的心灵显现，这份羊皮纸"; break;
							case 2: WillSay = "The voices all speak that this parchment is"; WillSayZh = "众灵皆言，这份羊皮纸"; break;
							case 3: WillSay = "I can see beyond that this parchment is"; WillSayZh = "我能洞察，这份羊皮纸"; break;
						}

						if ( scroll.ScrollTrue == 1 )
						{
							CitizenLocalization.SayToLocalizedComposite(m_GypsyLady, from, WillSay + " truthfully written.", WillSayZh + "如实记载。");
						}
						else
						{
							CitizenLocalization.SayToLocalizedComposite(m_GypsyLady, from, WillSay + " falsely written.", WillSayZh + "虚假记载。");
						}

						from.SendMessage(String.Format(StringCatalog.ResolveByKey(from.Account, "mob.fmt.you_pay_0_gold"), toConsume));
					}
					else
					{
						m_GypsyLady.SayTo(from, Server.Localization.StringCatalog.ResolveFormatByKey(from.Account, "mob.fmt.i_require_0_gold_for_my_visions", toConsume));
					}
				}
				///////////////////////////////////////////////////////////////////////////////////
				else if ( targeted is SearchPage )
				{
					SearchPage scroll = (SearchPage)targeted;

					int nCost = ( 100 - scroll.LegendPercent ) * 50;

					if ( BaseVendor.BeggingPose(from) > 0 ) // LET US SEE IF THEY ARE BEGGING
					{
						nCost = nCost - (int)( ( from.Skills[SkillName.Begging].Value * 0.005 ) * nCost );
					}

					int toConsume = nCost;

					if (pack.ConsumeTotal(typeof(Gold), toConsume))
					{
						string WillSay = "";
						string WillSayZh = "";

						switch ( Utility.RandomMinMax( 0, 3 ) ) 
						{
							case 0: WillSay = "The spirits tell me that this legend "; WillSayZh = "神灵告知我，这段传说"; break;
							case 1: WillSay = "My mind is showing me that this legend "; WillSayZh = "我的心灵显现，这段传说"; break;
							case 2: WillSay = "The voices all speak that this legend "; WillSayZh = "众灵皆言，这段传说"; break;
							case 3: WillSay = "I can see beyond that this legend "; WillSayZh = "我能洞察，这段传说"; break;
						}

						if ( scroll.LegendReal == 1 )
						{
							CitizenLocalization.SayToLocalizedComposite(m_GypsyLady, from, WillSay + " really happened.", WillSayZh + "确有其事。");
						}
						else
						{
							CitizenLocalization.SayToLocalizedComposite(m_GypsyLady, from, WillSay + " never happened.", WillSayZh + "实属虚构。");
						}

						from.SendMessage(String.Format(StringCatalog.ResolveByKey(from.Account, "mob.fmt.you_pay_0_gold"), toConsume));
					}
					else
					{
						m_GypsyLady.SayTo(from, Server.Localization.StringCatalog.ResolveFormatByKey(from.Account, "mob.fmt.i_require_0_gold_for_my_visions", toConsume));
					}
				}
				///////////////////////////////////////////////////////////////////////////////////
				else if ( targeted is DynamicBook )
				{
					DynamicBook scroll = (DynamicBook)targeted;

					int nCost = (scroll.BookPower + 1) * 50;

					if ( BaseVendor.BeggingPose(from) > 0 ) // LET US SEE IF THEY ARE BEGGING
					{
						nCost = nCost - (int)( ( from.Skills[SkillName.Begging].Value * 0.005 ) * nCost );
					}

					int toConsume = nCost;

					if (pack.ConsumeTotal(typeof(Gold), toConsume))
					{
						string WillSay = "";
						string WillSayZh = "";

						switch ( Utility.RandomMinMax( 0, 3 ) ) 
						{
							case 0: WillSay = "The spirits tell me that this book "; WillSayZh = "神灵告知我，这本书"; break;
							case 1: WillSay = "My mind is showing me that this book "; WillSayZh = "我的心灵显现，这本书"; break;
							case 2: WillSay = "The voices all speak that this book "; WillSayZh = "众灵皆言，这本书"; break;
							case 3: WillSay = "I can see beyond that this book "; WillSayZh = "我能洞察，这本书"; break;
						}

						if ( scroll.BookTrue > 0 )
						{
							CitizenLocalization.SayToLocalizedComposite(m_GypsyLady, from, WillSay + " contains the truth.", WillSayZh + "记载真实。");
						}
						else
						{
							CitizenLocalization.SayToLocalizedComposite(m_GypsyLady, from, WillSay + " contains falsehoods.", WillSayZh + "记载虚妄。");
						}

						from.SendMessage(String.Format(StringCatalog.ResolveByKey(from.Account, "mob.fmt.you_pay_0_gold"), toConsume));
					}
					else
					{
						m_GypsyLady.SayTo(from, Server.Localization.StringCatalog.ResolveFormatByKey(from.Account, "mob.fmt.i_require_0_gold_for_my_visions", toConsume));
					}
				}
				///////////////////////////////////////////////////////////////////////////////////
				else if ( targeted is SomeRandomNote )
				{
					SomeRandomNote scroll = (SomeRandomNote)targeted;

					int nCost = 100;

					if ( BaseVendor.BeggingPose(from) > 0 ) // LET US SEE IF THEY ARE BEGGING
					{
						nCost = nCost - (int)( ( from.Skills[SkillName.Begging].Value * 0.005 ) * nCost );
					}

					int toConsume = nCost;

					if (pack.ConsumeTotal(typeof(Gold), toConsume))
					{
						string WillSay = "";
						string WillSayZh = "";

						switch ( Utility.RandomMinMax( 0, 3 ) ) 
						{
							case 0: WillSay = "The spirits tell me that this parchment is"; WillSayZh = "神灵告知我，这份羊皮纸"; break;
							case 1: WillSay = "My mind is showing me that this parchment is"; WillSayZh = "我的心灵显现，这份羊皮纸"; break;
							case 2: WillSay = "The voices all speak that this parchment is"; WillSayZh = "众灵皆言，这份羊皮纸"; break;
							case 3: WillSay = "I can see beyond that this parchment is"; WillSayZh = "我能洞察，这份羊皮纸"; break;
						}

						if ( scroll.ScrollTrue == 1 )
						{
							CitizenLocalization.SayToLocalizedComposite(m_GypsyLady, from, WillSay + " truthfully written.", WillSayZh + "如实记载。");
						}
						else
						{
							CitizenLocalization.SayToLocalizedComposite(m_GypsyLady, from, WillSay + " falsely written.", WillSayZh + "虚假记载。");
						}

						from.SendMessage(String.Format(StringCatalog.ResolveByKey(from.Account, "mob.fmt.you_pay_0_gold"), toConsume));
					}
					else
					{
						m_GypsyLady.SayTo(from, Server.Localization.StringCatalog.ResolveFormatByKey(from.Account, "mob.fmt.i_require_0_gold_for_my_visions", toConsume));
					}
				}
				///////////////////////////////////////////////////////////////////////////////////
				else if ( targeted is DataPad )
				{
					int nCost = 100;

					if ( BaseVendor.BeggingPose(from) > 0 ) // LET US SEE IF THEY ARE BEGGING
					{
						nCost = nCost - (int)( ( from.Skills[SkillName.Begging].Value * 0.005 ) * nCost );
					}

					int toConsume = nCost;

					if (pack.ConsumeTotal(typeof(Gold), toConsume))
					{
						string WillSay = "";
						string WillSayZh = "";

						switch ( Utility.RandomMinMax( 0, 3 ) ) 
						{
							case 0: WillSay = "The spirits tell me that this glowing book is"; WillSayZh = "神灵告知我，这本光辉之书"; break;
							case 1: WillSay = "My mind is showing me that this glowing book is"; WillSayZh = "我的心灵显现，这本光辉之书"; break;
							case 2: WillSay = "The voices all speak that this glowing book is"; WillSayZh = "众灵皆言，这本光辉之书"; break;
							case 3: WillSay = "I can see beyond that this glowing book is"; WillSayZh = "我能洞察，这本光辉之书"; break;
						}

						CitizenLocalization.SayToLocalizedComposite(m_GypsyLady, from, WillSay + " truthfully written.", WillSayZh + "如实记载。");

						from.SendMessage(String.Format(StringCatalog.ResolveByKey(from.Account, "mob.fmt.you_pay_0_gold"), toConsume));
					}
					else
					{
						m_GypsyLady.SayTo(from, Server.Localization.StringCatalog.ResolveFormatByKey(from.Account, "mob.fmt.i_require_0_gold_for_my_visions", toConsume));
					}
				}
				///////////////////////////////////////////////////////////////////////////////////
				else
				{
					CitizenLocalization.SayToLocalized(m_GypsyLady, from, "That is not a book or parchment.");
				}
            }
        }

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		public override void AddCustomContextEntries( Mobile from, List<ContextMenuEntry> list )
		{
			if ( from.Alive )
			{
				list.Add( new TruthEntry( this, from ) );
			}

			base.AddCustomContextEntries( from, list );
		}

		public GypsyLady( Serial serial ) : base( serial )
		{
		}

		public override bool CanTeach { get { return true; } }

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
}
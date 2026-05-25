using System;
using System.Collections.Generic;
using Server;
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
	public class ChucklesJester : BasePerson
	{
		private DateTime m_NextTalk;
		public DateTime NextTalk{ get{ return m_NextTalk; } set{ m_NextTalk = value; } }
		public override void OnMovement( Mobile m, Point3D oldLocation )
		{
			if( m is PlayerMobile )
			{
				if ( DateTime.Now >= m_NextTalk && InRange( m, 4 ) && InLOS( m ) )
				{
					DoJokes( this );
					m_NextTalk = (DateTime.Now + TimeSpan.FromSeconds( 30 ));
				}
			}
		}

		public override string TalkGumpTitle{ get{ return "Surely You Jest"; } }
		public override string TalkGumpSubject{ get{ return "Jester"; } }

		public static void DoJokes( Mobile m )
		{
			int act = Utility.Random( 28 );
			if ( m is PlayerMobile ){ act = Utility.Random( 22 ); }
			switch ( act )
			{
				case 0: CitizenLocalization.SayLocalized(m, StringCatalog.ResolveByKey(null, "mob.joke.why_did_the_king_go_to_the_dentist_to_get_his_teeth_cro")); break;
				case 1: CitizenLocalization.SayLocalized(m, StringCatalog.ResolveByKey(null, "mob.other.when_a_knight_in_armor_was_killed_in_battle_what_sign_d")); break;
				case 2: CitizenLocalization.SayLocalized(m, StringCatalog.ResolveByKey(null, "mob.joke.what_do_you_call_a_mosquito_in_a_tin_suit_a_bite_in_shi")); break;
				case 3: CitizenLocalization.SayLocalized(m, StringCatalog.ResolveByKey(null, "mob.other.there_are_many_castles_in_the_world_but_who_is_strong_e")); break;
				case 4: CitizenLocalization.SayLocalized(m, StringCatalog.ResolveByKey(null, "mob.other.what_king_was_famous_because_he_spent_so_many_nights_at")); break;
				case 5: CitizenLocalization.SayLocalized(m, StringCatalog.ResolveByKey(null, "mob.joke.how_do_you_find_a_princess_you_follow_the_foot_prince")); break;
				case 6: CitizenLocalization.SayLocalized(m, StringCatalog.ResolveByKey(null, "mob.other.why_were_the_early_days_called_the_dark_ages_because_th")); break;
				case 7: CitizenLocalization.SayLocalized(m, StringCatalog.ResolveByKey(null, "mob.joke.why_did_arthur_have_a_round_table_so_no_one_could_corne")); break;
				case 8: CitizenLocalization.SayLocalized(m, StringCatalog.ResolveByKey(null, "mob.other.who_invented_king_arthur_s_round_table_sir_cumference")); break;
				case 9: CitizenLocalization.SayLocalized(m, StringCatalog.ResolveByKey(null, "mob.joke.why_did_the_knight_run_about_shouting_for_a_tin_opener")); break;
				case 10: CitizenLocalization.SayLocalized(m, StringCatalog.ResolveByKey(null, "mob.joke.what_was_camelot_famous_for_it_s_knight_life")); break;
				case 11: CitizenLocalization.SayLocalized(m, StringCatalog.ResolveByKey(null, "mob.joke.what_did_the_toad_say_when_the_princess_would_not_kiss")); break;
				case 12: CitizenLocalization.SayLocalized(m, StringCatalog.ResolveByKey(null, "mob.joke.what_do_you_call_the_young_royal_who_keeps_falling_down")); break;
				case 13: CitizenLocalization.SayLocalized(m, StringCatalog.ResolveByKey(null, "mob.joke.what_do_you_call_a_cat_that_flies_over_the_castle_wall")); break;
				case 14: CitizenLocalization.SayLocalized(m, StringCatalog.ResolveByKey(null, "mob.other.what_game_do_the_fish_play_in_the_moat_trout_or_dare")); break;
				case 15: CitizenLocalization.SayLocalized(m, StringCatalog.ResolveByKey(null, "mob.joke.what_did_the_fish_say_to_the_other_when_the_horse_fell")); break;
				case 16: CitizenLocalization.SayLocalized(m, StringCatalog.ResolveByKey(null, "mob.joke.what_do_you_call_an_angry_princess_just_awakened_from_a")); break;
				case 17: CitizenLocalization.SayLocalized(m, StringCatalog.ResolveByKey(null, "mob.joke.how_did_the_prince_get_into_the_castle_when_the_drawbri")); break;
				case 18: CitizenLocalization.SayLocalized(m, StringCatalog.ResolveByKey(null, "mob.joke.how_did_the_girl_dragon_win_the_beauty_contest_she_was")); break;
				case 19: CitizenLocalization.SayLocalized(m, StringCatalog.ResolveByKey(null, "mob.joke.why_did_the_dinosaur_live_longer_than_the_dragon_becaus")); break;
				case 20: CitizenLocalization.SayLocalized(m, StringCatalog.ResolveByKey(null, "mob.joke.what_did_the_dragon_say_when_it_saw_the_knight_not_more")); break;
				case 21: CitizenLocalization.SayLocalized(m, StringCatalog.ResolveByKey(null, "mob.joke.what_do_you_do_with_a_green_dragon_wait_until_it_ripens")); break;
				case 22: m.PlaySound( m.Female ? 780 : 1051 ); CitizenLocalization.SayLocalized(m, "*claps*"); break;
				case 23: CitizenLocalization.SayLocalized(m, StringCatalog.ResolveByKey(null, "mob.emote.bows")); m.Animate( 32, 5, 1, true, false, 0 ); break;
				case 24: m.PlaySound( m.Female ? 794 : 1066 ); CitizenLocalization.SayLocalized(m, "*giggles*"); break;
				case 25: m.PlaySound( m.Female ? 801 : 1073 ); CitizenLocalization.SayLocalized(m, "*laughs*"); break;
				case 26: m.PlaySound( 792 ); CitizenLocalization.SayLocalized(m, StringCatalog.ResolveByKey(null, "mob.emote.sticks_out_tongue")); break;
				case 27: m.PlaySound( m.Female ? 783 : 1054 ); CitizenLocalization.SayLocalized(m, StringCatalog.ResolveByKey(null, "mob.emote.woohoo")); break;
			};

			if ( act < 22 && Utility.RandomBool() )
			{
				switch ( Utility.Random( 6 ))
				{
					case 0: m.PlaySound( m.Female ? 780 : 1051 ); break;
					case 1: m.Animate( 32, 5, 1, true, false, 0 ); break;
					case 2: m.PlaySound( m.Female ? 794 : 1066 ); break;
					case 3: m.PlaySound( m.Female ? 801 : 1073 ); break;
					case 4: m.PlaySound( 792 ); break;
					case 5: m.PlaySound( m.Female ? 783 : 1054 ); break;
				};
			}

		}

		[Constructable]
		public ChucklesJester() : base( )
		{
			SpeechHue = Utility.RandomTalkHue();
			NameHue = 1154;

			Body = 0x190;

			Name = "Chuckles";
			Title = "the Jester";
			Hue = Utility.RandomSkinColor();

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

			SetSkill( SkillName.FistFighting, 100 );
			Karma = 1000;
			VirtualArmor = 30;

			AddItem( new ShortPants( Utility.RandomNeutralHue() ) );
			AddItem( new Shoes( Utility.RandomNeutralHue() ) );
			AddItem( new JesterSuit( Utility.RandomNeutralHue() ) );
			AddItem( new JesterHat( Utility.RandomNeutralHue() ) );

			Utility.AssignRandomHair( this );
		}

		public override bool OnDragDrop( Mobile from, Item dropped )
		{
			if ( dropped is JokeBook )
			{
				if ( from.Blessed )
				{
					string sSay = "I cannot deal with you while you are in that state.";
					this.PrivateOverheadMessage(MessageType.Regular, 1153, false, sSay, from.NetState);
					return false;
				}
				else if ( IntelligentAction.GetMyEnemies( from, this, false ) )
				{
					string sSay = "I don't think I should accept that from you.";
					this.PrivateOverheadMessage(MessageType.Regular, 1153, false, sSay, from.NetState);
					return false;
				}
				else
				{
					if ( Utility.RandomBool() )
					{
						GiftJesterHat hat = new GiftJesterHat();
						hat.Name = "Magical Jester Hat";
						hat.Hue = 0;
						hat.ItemID = Utility.RandomList( 0x171C, 0x4C15 );
						hat.m_Owner = from;
						hat.m_Gifter = "Chuckles the Jester";
						hat.m_How = "Given to";
						hat.m_Points = Utility.RandomMinMax( 80, 100 );

						from.AddToBackpack ( hat );
						from.SendMessage( StringCatalog.ResolveByKey(from.Account, "mob.other.chuckles_gave_you_one_of_his_hats") );
					}
					else
					{
						GiftFancyDress coat = new GiftFancyDress();
						coat.Name = "Magical Jester Suit";
						coat.Hue = 0;
						coat.ItemID = Utility.RandomList( 0x1f9f, 0x1fa0, 0x4C16, 0x4C17, 0x2B6B );
						coat.m_Owner = from;
						coat.m_Gifter = "Chuckles the Jester";
						coat.m_How = "Given to";
						coat.m_Points = Utility.RandomMinMax( 80, 100 );

						from.AddToBackpack ( coat );
						from.SendMessage( StringCatalog.ResolveByKey(from.Account, "mob.other.chuckles_gave_you_one_of_his_suits") );
					}
					this.SayTo( from, false, Server.Localization.StringCatalog.ResolveFormatByKey(from.Account, "mob.fmt.thank_you_0_i_am_always_looking_for_some_new_jokes", from.Name ) );
					from.SendSound( 0x3D );
					dropped.Delete();
					from.SendMessage( StringCatalog.ResolveByKey(from.Account, "mob.other.single_click_on_it_to_enchant_it") );
					return true;
				}
			}
			else if ( dropped is Artifact_JesterHatofChuckles )
			{
				this.SayTo( from, false, Server.Localization.StringCatalog.ResolveFormatByKey(from.Account, "mob.fmt.thank_you_0_i_lost_that_hat_years_ago", from.Name ) );
				from.SendSound( 0x5B4 );
				dropped.Delete();
				int gold = Utility.RandomMinMax(5,10) * 1000;
				from.AddToBackpack ( new BankCheck( gold ) );
				from.SendMessage( StringCatalog.ResolveByKey(from.Account, "mob.other.chuckles_gave_you_a_check_for") + gold + " gold!" );
				return true;
			}

			return base.OnDragDrop( from, dropped );
		}

		public ChucklesJester( Serial serial ) : base( serial )
		{
		}

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
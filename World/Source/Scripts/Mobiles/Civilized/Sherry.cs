using System;
using Server; 
using Server.Misc;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System.Net;
using Server.Network;
using Server.Mobiles;
using Server.Accounting;
using Server.Guilds;
using Server.Items;
using Server.Gumps;
using Server.Commands;
using Server.Localization;

namespace Server.Mobiles
{
	public class SherryTheMouse : BasePerson
	{
		private DateTime m_NextTalk;
		public DateTime NextTalk{ get{ return m_NextTalk; } set{ m_NextTalk = value; } }
		public override void OnMovement( Mobile m, Point3D oldLocation )
		{
			if( m is PlayerMobile )
			{
				if ( DateTime.Now >= m_NextTalk && InRange( m, 4 ) && InLOS( m ) )
				{
					Say(StringCatalog.ResolveByKey(this.Account, "mob.other.squeak_2"));
					m_NextTalk = (DateTime.Now + TimeSpan.FromSeconds( 30 ));
				}
			}
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public DateTime NextFeed { get; set; }

		[Constructable]
		public SherryTheMouse() : base( )
		{
			SpeechHue = Utility.RandomTalkHue();
			NameHue = 1276;

			Body = 238;
			BaseSoundID = 0xCC;

			Name = "Sherry";
			Title = "the Mouse";
			Direction = Direction.East;
			CantWalk = true;

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
		}

		public override void OnDoubleClick( Mobile from )
		{
			bool CanTalk = true;

			if ( !(this.CanSee( from )) ){ CanTalk = false; }
			if ( !(this.InLOS( from )) ){ CanTalk = false; }

			if ( CanTalk )
			{
				this.PlaySound( 0x0CD );
				from.CloseGump( typeof( SherryGump ) );
				from.SendGump( new SherryGump( from, this ) );
			}
			else
			{
				from.SendMessage( StringCatalog.ResolveByKey(from.Account, "mob.other.she_is_too_far_away_from_you") );
			}
		}

		public class SherryGump : Gump
		{
			public Mobile mouse;

			public SherryGump( Mobile from, Mobile rat ): base( 50, 50 )
			{
				mouse = rat;
				this.Closable=true;
				this.Disposable=false;
				this.Dragable=true;
				this.Resizable=false;

				AddPage(0);
				AddImage(20, 16, 1243);
				AddButton(202, 247, 2020, 2020, 1, GumpButtonType.Reply, 0);
				AddHtml( 62, 288, 178, 27, @StringCatalog.ResolveByKey(from.Account, "mob.other.body_basefont_color_111111_big_center_sherry_the_mouse"), (bool)false, (bool)false);
			}

			public override void OnResponse( NetState state, RelayInfo info )
			{
				Mobile from = state.Mobile; 

				mouse.PlaySound( 0x0CD );

				if ( info.ButtonID > 0 )
				{
					switch ( Utility.RandomMinMax( 0, 8 ) )
					{

						case 0:	CitizenLocalization.SayLocalized(mouse, StringCatalog.ResolveByKey(null, "mob.other.oft_have_i_wished_that_stranger_would_return")); break;
						case 1:	CitizenLocalization.SayLocalized(mouse, StringCatalog.ResolveByKey(null, "mob.other.we_must_bring_the_shards_into_harmony_so_that_they_reso")); break;
						case 2:	CitizenLocalization.SayLocalized(mouse, StringCatalog.ResolveByKey(null, "mob.other.yet_sometimes_one_must_sacrifice_a_pawn_to_save_a_king")); break;
						case 3:	CitizenLocalization.SayLocalized(mouse, StringCatalog.ResolveByKey(null, "mob.other.suddenly_the_shutters_blew_open_and_lord_british_fell_t")); break;
						case 4:	CitizenLocalization.SayLocalized(mouse, StringCatalog.ResolveByKey(null, "mob.other.i_witnessed_them_all_from_my_tiny_mousehole")); break;
						case 5:	CitizenLocalization.SayLocalized(mouse, StringCatalog.ResolveByKey(null, "mob.other.but_i_am_but_a_mouse_and_none_hear_me")); break;
						case 6:	CitizenLocalization.SayLocalized(mouse, StringCatalog.ResolveByKey(null, "mob.other.a_shard_of_a_universe_is_a_powerful_thing")); break;
						case 7:	CitizenLocalization.SayLocalized(mouse, StringCatalog.ResolveByKey(null, "mob.other.aid_the_nobility_that_resideth_in_human_heart")); break;
						case 8:	CitizenLocalization.SayLocalized(mouse, StringCatalog.ResolveByKey(null, "mob.other.even_pawns_have_lives_and_loves_at_home_my_lord")); break;
					}
				}
			}
		}

		public override bool OnDragDrop( Mobile from, Item dropped )
		{
			if ( dropped is CheeseWheel || dropped is CheeseWedge || dropped is CheeseSlice )
			{
				if ( DateTime.Now < NextFeed )
				{
					PrivateOverheadMessage(MessageType.Regular, 1153, false, StringCatalog.ResolveByKey(this.Account, "mob.other.my_tummy_hurts") , from.NetState);
					return false;
				}

				if ( Utility.RandomDouble() < 0.1 )
				{
					NextFeed = DateTime.Now.AddMinutes(Utility.RandomMinMax( 13, 30 ));
				}

				this.PlaySound( 0x0CD );

				string sMessage = StringCatalog.ResolveByKey(this.Account, "mob.other.squeak_2");

				int relic = Utility.RandomMinMax( 1, 59 );

				int chance = dropped.Amount;
					if ( chance > 75 ){ chance = 75; }

				int pick = Utility.RandomMinMax( 0, 8 );
					if ( chance >= Utility.RandomMinMax( 1, 100 ) ){ pick = 9; }

				switch ( pick )
				{
					case 0:	sMessage = "I heard that the " + Server.Items.SomeRandomNote.GetSpecialItem( relic, 1 ) + " can be obtained in " + Server.Items.SomeRandomNote.GetSpecialItem( relic, 0 ) + "."; break;
					case 1:	sMessage = "Nystal said something about the " + Server.Items.SomeRandomNote.GetSpecialItem( relic, 1 ) + " and " + Server.Items.SomeRandomNote.GetSpecialItem( relic, 0 ) + "."; break;
					case 2:	sMessage = "Someone told Lord British that " + Server.Items.SomeRandomNote.GetSpecialItem( relic, 0 ) + " is where you would look for the " + Server.Items.SomeRandomNote.GetSpecialItem( relic, 1 ) + "."; break;
					case 3:	sMessage = "Lord British would tell me tales of knights going to " + Server.Items.SomeRandomNote.GetSpecialItem( relic, 0 ) + " and bringing back the " + Server.Items.SomeRandomNote.GetSpecialItem( relic, 1 ) + "."; break;
					case 4:	sMessage = QuestCharacters.RandomWords() + " was in the kitchen whispering about the " + Server.Items.SomeRandomNote.GetSpecialItem( relic, 1 ) + " and " + Server.Items.SomeRandomNote.GetSpecialItem( relic, 0 ) + "."; break;
					case 5:	sMessage = "I saw a note from the " + RandomThings.GetRandomJob() + ", and it mentioned the " + Server.Items.SomeRandomNote.GetSpecialItem( relic, 1 ) + " and " + Server.Items.SomeRandomNote.GetSpecialItem( relic, 0 ) + "."; break;
					case 6:	sMessage = "Lord British met with " + QuestCharacters.RandomWords() + " and told them to bring back the " + Server.Items.SomeRandomNote.GetSpecialItem( relic, 1 ) + " from " + Server.Items.SomeRandomNote.GetSpecialItem( relic, 0 ) + "."; break;
					case 7:	sMessage = "I heard that the " + Server.Items.SomeRandomNote.GetSpecialItem( relic, 1 ) + " can be found in " + Server.Items.SomeRandomNote.GetSpecialItem( relic, 0 ) + "."; break;
					case 8:	sMessage = "Someone from " + RandomThings.GetRandomCity() + " died in " + Server.Items.SomeRandomNote.GetSpecialItem( relic, 0 ) + " searching for the " + Server.Items.SomeRandomNote.GetSpecialItem( relic, 1 ) + "."; break;
					case 9:	sMessage = Server.Misc.TavernPatrons.GetRareLocation( this, false, false );		break;
				}
				this.PrivateOverheadMessage(MessageType.Regular, 1153, false, sMessage, from.NetState);
				dropped.Delete();
				return true;
			}

			return base.OnDragDrop( from, dropped );
		}

		public SherryTheMouse( Serial serial ) : base( serial )
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
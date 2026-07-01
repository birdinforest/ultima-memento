using System;
using Server.Items;
using Server.Network;
using Server.Mobiles;
using Server.Localization;

namespace Server.Items
{
    public class NoticeClue : Item
	{
		public override bool HandlesOnMovement{ get{ return true; } }

		private DateTime m_NextTalk;
		public DateTime NextTalk{ get{ return m_NextTalk; } set{ m_NextTalk = value; } }

		private static bool TryGetClueKeys( int x, int y, out string bodyKey, out string titleKey )
		{
			bodyKey = null;
			titleKey = null;

			if ( x == 5764 && y == 2215 )
			{
				bodyKey = "quest.seems_like_an_odd_phrase_dot_perhaps_i_should_remember_the_name_that_some_give_to_a_ruby_dot";
				titleKey = "quest.the_bloodstone";
				return true;
			}

			if ( x == 6268 && y == 2661 )
			{
				bodyKey = "quest.what_altars_did_harkyn_set_q_what_name_must_be_spoken_q";
				titleKey = "quest.harkyn_s_altars";
				return true;
			}

			if ( x == 6293 && y == 1649 )
			{
				bodyKey = "quest.the_emerald_gate_q_perhaps_a_magical_gate_of_green_q_if_i_speak_the_name_of_the_ruby_near_it_c_i_may";
				titleKey = "quest.the_emerald_gate";
				return true;
			}

			if ( x == 6497 && y == 1440 )
			{
				bodyKey = "quest.the_shapes_of_three_c_silver_they_be_c_can_make_the_golden_skull_speak_q_perhaps_these_things_i_must";
				titleKey = "quest.the_silver_shapes";
				return true;
			}

			if ( x == 6501 && y == 1773 )
			{
				bodyKey = "quest.know_this_c_that_a_man_called_tarjan_c_thought_by_many_to_be_insane_c_had_through_wizardly_powers_pr";
				titleKey = "quest.the_mad_god";
				return true;
			}

			if ( x == 6988 && y == 164 )
			{
				bodyKey = "quest.you_can_already_feel_the_magical_energy_that_is_sealing_this_door_dot_perhaps_there_is_another_way_t";
				titleKey = "quest.mangar_s_tower_door";
				return true;
			}

			return false;
		}

		private static string ResolveClueText( Mobile from, int x, int y, string fallbackName )
		{
			string bodyKey;
			string titleKey;

			if ( TryGetClueKeys( x, y, out bodyKey, out titleKey ) )
				return StringCatalog.ResolveByKey( from.Account, bodyKey );

			if ( from != null && from.Account != null )
				return StringCatalog.TryResolve( AccountLang.GetLanguageCode( from.Account ), fallbackName ) ?? fallbackName;

			return fallbackName;
		}

		public override void OnMovement( Mobile from, Point3D oldLocation )
		{
			if( from is PlayerMobile )
			{
				if ( DateTime.Now >= m_NextTalk && Utility.InRange( from.Location, this.Location, 5 ) )
				{
					from.PrivateOverheadMessage( MessageType.Regular, 1150, false, ResolveClueText( from, this.X, this.Y, this.Name ), from.NetState );

					string bodyKey;
					string titleKey;

					if ( TryGetClueKeys( this.X, this.Y, out bodyKey, out titleKey ) )
					{
						from.CloseGump( typeof(Server.Gumps.ClueGump) );
						from.SendGump( new Server.Gumps.ClueGump(
							from,
							StringCatalog.ResolveByKey( from.Account, bodyKey ),
							StringCatalog.ResolveByKey( from.Account, titleKey ) ) );
					}

					m_NextTalk = (DateTime.Now + TimeSpan.FromSeconds( 30 ));
				}
			}
		}

		[Constructable]
		public NoticeClue( ) : base( 0x181E )
		{
			Movable = false;
			Visible = false;
			Name = "clue";
		}

		public NoticeClue( Serial serial ) : base( serial )
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int) 0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();
		}
	}	
}
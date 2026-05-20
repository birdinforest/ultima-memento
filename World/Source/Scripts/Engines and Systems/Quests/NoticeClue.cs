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

		public override void OnMovement( Mobile from, Point3D oldLocation )
		{
			if( from is PlayerMobile )
			{
				if ( DateTime.Now >= m_NextTalk && Utility.InRange( from.Location, this.Location, 5 ) )
				{
					from.PrivateOverheadMessage(MessageType.Regular, 1150, false, this.Name, from.NetState);

					if ( this.X == 5764 && this.Y == 2215 )
					{
						from.CloseGump( typeof(Server.Gumps.ClueGump) );
						from.SendGump(new Server.Gumps.ClueGump( from, StringCatalog.ResolveByKey(from.Account, "quest.seems_like_an_odd_phrase_dot_perhaps_i_should_remember_the_name_that_some_give_to_a_ruby_dot"), StringCatalog.ResolveByKey(from.Account, "quest.the_bloodstone") ) );
					}
					else if ( this.X == 6268 && this.Y == 2661 )
					{
						from.CloseGump( typeof(Server.Gumps.ClueGump) );
						from.SendGump(new Server.Gumps.ClueGump( from, StringCatalog.ResolveByKey(from.Account, "quest.what_altars_did_harkyn_set_q_what_name_must_be_spoken_q"), StringCatalog.ResolveByKey(from.Account, "quest.harkyn_s_altars") ) );
					}
					else if ( this.X == 6293 && this.Y == 1649 )
					{
						from.CloseGump( typeof(Server.Gumps.ClueGump) );
						from.SendGump(new Server.Gumps.ClueGump( from, StringCatalog.ResolveByKey(from.Account, "quest.the_emerald_gate_q_perhaps_a_magical_gate_of_green_q_if_i_speak_the_name_of_the_ruby_near_it_c_i_may"), StringCatalog.ResolveByKey(from.Account, "quest.the_emerald_gate") ) );
					}
					else if ( this.X == 6497 && this.Y == 1440 )
					{
						from.CloseGump( typeof(Server.Gumps.ClueGump) );
						from.SendGump(new Server.Gumps.ClueGump( from, StringCatalog.ResolveByKey(from.Account, "quest.the_shapes_of_three_c_silver_they_be_c_can_make_the_golden_skull_speak_q_perhaps_these_things_i_must"), StringCatalog.ResolveByKey(from.Account, "quest.the_silver_shapes") ) );
					}
					else if ( this.X == 6501 && this.Y == 1773 )
					{
						from.CloseGump( typeof(Server.Gumps.ClueGump) );
						from.SendGump(new Server.Gumps.ClueGump( from, StringCatalog.ResolveByKey(from.Account, "quest.know_this_c_that_a_man_called_tarjan_c_thought_by_many_to_be_insane_c_had_through_wizardly_powers_pr"), StringCatalog.ResolveByKey(from.Account, "quest.the_mad_god") ) );
					}
					else if ( this.X == 6988 && this.Y == 164 )
					{
						from.CloseGump( typeof(Server.Gumps.ClueGump) );
						from.SendGump(new Server.Gumps.ClueGump( from, StringCatalog.ResolveByKey(from.Account, "quest.you_can_already_feel_the_magical_energy_that_is_sealing_this_door_dot_perhaps_there_is_another_way_t"), StringCatalog.ResolveByKey(from.Account, "quest.mangar_s_tower_door") ) );
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
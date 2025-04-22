using System;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Prompts;
using Server.Network;
using System.Collections;
using System.Collections.Generic;
using Server.Gumps;

namespace Server.Items
{
    public class SoulOrb : Item
    {
		public override string DefaultDescription{ get{ return "These items will resurrect you automatically, after 30 seconds, if you meet an untimely end. If you want to dispose of it, use it in your pack, where it will then disappear from the world."; } }

        public Mobile m_Owner;

        [CommandProperty( AccessLevel.GameMaster )]
        public Mobile Owner
        {
            get { return m_Owner; }
            set { m_Owner = value; InvalidateProperties(); }
        }

        private Timer m_Timer;
        private static TimeSpan m_Delay = TimeSpan.FromSeconds( 30.0 ); /*TimeSpan.Zero*/

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan Delay { get { return m_Delay; } set { m_Delay = value; } }
	
        public static void Initialize()
        {
            EventSink.PlayerDeath += new PlayerDeathEventHandler(EventSink_Death);
        }

        [Constructable]
        public SoulOrb() : base( 0x2C84 ) 
        {
            Name = "soul orb";
            LootType = LootType.Blessed;
			Movable = false;
            Weight = 1.0;
		}

		public override void OnDoubleClick( Mobile from )
		{
            var confirmation = new ConfirmationGump(
                from,
                "Are you sure you wish to delete this?",
                () => Delete()
            );
            from.SendGump(confirmation);
		}

        public SoulOrb(Serial serial) : base(serial){}

		public static void OnSummoned( Mobile from, SoulOrb orb )
		{
			if ( orb.RootParent == from )
			{
				BuffInfo.RemoveBuff( from, BuffIcon.Resurrection );
				BuffInfo.AddBuff( from, new BuffInfo( BuffIcon.Resurrection, 1063626, true ) );
			}
			else
			{
				from.PrivateOverheadMessage(MessageType.Regular, 38, false, "Your pack is full so it did not work!", from.NetState);
				from.SendMessage( "Your pack is full so it did not work!" );
				orb.Delete();
			}
		}
		
		private static void EventSink_Death(PlayerDeathEventArgs e)
        {
            PlayerMobile owner = e.Mobile as PlayerMobile;

			Item item = owner.Backpack.FindItemByType( typeof ( SoulOrb ) );
			SoulOrb orb = (SoulOrb)item;

			if ( orb != null && owner != null && !owner.Deleted )
            {
                if (owner.Alive)
                    return;

				orb.m_Timer = Timer.DelayCall(m_Delay, new TimerStateCallback(Resurrect_OnTick), new object[] { owner, orb });
            }
        }

        private static void Resurrect_OnTick(object state)
        {
            object[] states = (object[])state;
            PlayerMobile owner = (PlayerMobile)states[0];
			SoulOrb arp = (SoulOrb)states[1];
            if ( owner != null && !owner.Deleted && arp != null && !arp.Deleted )
            {
                if (owner.Alive)
                    return;

				if ( arp.Name == "blood of a vampire" ){ owner.SendMessage("The blood pours out of the bottle, restoring your life."); }
				else if ( arp.Name == "cloning crystal" ){ owner.SendMessage("The crystal forms a clone of your body, restoring your life."); }
                else { owner.SendMessage("The orb glows, releasing your soul."); }
                owner.Resurrect();
                owner.FixedEffect( 0x376A, 10, 16, Server.Misc.PlayerSettings.GetMySpellHue( true, owner, 0 ), 0 );
                Server.Misc.Death.Penalty( owner, false );
				BuffInfo.RemoveBuff( owner, BuffIcon.Resurrection );
                arp.Delete();
            }
        }

        public override void AddNameProperties(ObjectPropertyList list)
		{
            base.AddNameProperties(list);

			if ( this.Name == "blood of a vampire" ){ list.Add( 1049644, "Contains vampire blood for " + m_Owner.Name ); }
			else if ( this.Name == "cloning crystal" ){ list.Add( 1049644, "Contains genetic patterns for " + m_Owner.Name ); }
			else { list.Add( 1049644, "Contains the Soul of " + m_Owner.Name ); }
        } 

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write( (int) 1 ); // version  
			writer.Write( (Mobile)m_Owner );        
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

			if ( version > 0 )
				m_Owner = reader.ReadMobile();
            else
            {
                // none when the world starts 
                Timer.DelayCall(TimeSpan.FromSeconds(30), () => Delete());
			    
            }
        }
    }
}
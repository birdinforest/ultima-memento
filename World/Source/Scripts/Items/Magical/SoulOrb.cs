using System;
using Server.Mobiles;
using Server.Network;
using Server.Gumps;
using Server.Utilities;

namespace Server.Items
{
	public enum SoulOrbType
	{
		Default,
		BloodOfVampire,
		CloningCrystalJedi,
		CloningCrystalSyth,
		RestorativeSoil
	}

    public class SoulOrb : Item
    {
		public override string DefaultDescription{ get{ return "These items will resurrect you automatically, after 30 seconds, if you meet an untimely end. If you want to dispose of it, use it in your pack, where it will then disappear from the world."; } }

        private Mobile m_Owner;

        [CommandProperty( AccessLevel.GameMaster )]
        public Mobile Owner
        {
            get { return m_Owner; }
            set { m_Owner = value; InvalidateProperties(); }
        }

		[CommandProperty( AccessLevel.GameMaster )]
		public SoulOrbType OrbType { get; set; }

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

		public static SoulOrb Create( Mobile from, SoulOrbType orbType )
		{
			WorldUtilities.DeleteAllItems<SoulOrb>( item => item.m_Owner == from );

			var orb = new SoulOrb
			{
				m_Owner = from,
				OrbType = orbType
			};

			switch ( orb.OrbType )
			{
				case SoulOrbType.BloodOfVampire:
					orb.Name = "blood of a vampire";
					orb.ItemID = 0x122B;
					orb.Hue = 0;
					break;

				case SoulOrbType.CloningCrystalJedi:
					orb.Name = "replication crystal";
					orb.ItemID = 0x703;
					break;

				case SoulOrbType.CloningCrystalSyth:
					orb.Name = "replication crystal";
					orb.ItemID = 0x705;
					break;

				case SoulOrbType.RestorativeSoil:
					orb.Name = "mystical mud";
					orb.ItemID = 0x913;
					orb.Hue = 0;
					break;

				case SoulOrbType.Default:
				default:
					break;
			}

			from.AddToBackpack( orb );

			if ( orb.RootParent == from )
			{
				BuffInfo.RemoveBuff( from, BuffIcon.Resurrection );
				BuffInfo.AddBuff( from, new BuffInfo( BuffIcon.Resurrection, 1063626, true ) );

				return orb;
			}
			else
			{
				from.PrivateOverheadMessage(MessageType.Regular, 38, false, "Your pack is full so it did not work!", from.NetState);
				from.SendMessage( "Your pack is full so it did not work!" );
				orb.Delete();

				return null;
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
                if (owner.Alive) return;

				switch ( arp.OrbType )
				{
					case SoulOrbType.BloodOfVampire:
						owner.SendMessage("The blood pours out of the bottle, restoring your life.");
						break;

					case SoulOrbType.CloningCrystalJedi:
					case SoulOrbType.CloningCrystalSyth:
						owner.SendMessage("The crystal forms a clone of your body, restoring your life.");
						break;

					case SoulOrbType.RestorativeSoil:
					case SoulOrbType.Default:
					default:
						owner.SendMessage("The orb glows, releasing your soul.");
						break;
				}

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

			switch ( OrbType )
			{
				case SoulOrbType.BloodOfVampire:
					list.Add( 1049644, "Contains vampire blood for " + m_Owner.Name );
					break;

				case SoulOrbType.CloningCrystalJedi:
				case SoulOrbType.CloningCrystalSyth:
					list.Add( 1049644, "Contains genetic patterns for " + m_Owner.Name );
					break;

				case SoulOrbType.RestorativeSoil:
				case SoulOrbType.Default:
				default:
					list.Add( 1049644, "Contains the Soul of " + m_Owner.Name );
					break;
			}
        } 

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write( 2 ); // version  
			writer.Write( m_Owner );
			writer.Write( (int)OrbType );
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

			if ( version > 0 )
			{
				m_Owner = reader.ReadMobile();

				if ( version > 1 )
				{
					OrbType = (SoulOrbType)reader.ReadInt();
				}
				else
				{
					if ( Name == "blood of a vampire" ){ OrbType = SoulOrbType.BloodOfVampire; }
					else if ( Name == "cloning crystal" ) { 
						if ( ItemID == 0x703 ){ OrbType = SoulOrbType.CloningCrystalJedi; }
						else if ( ItemID == 0x705 ){ OrbType = SoulOrbType.CloningCrystalSyth; }
						else { OrbType = SoulOrbType.Default; }
					}
					else { OrbType = SoulOrbType.Default; }
				}
			}
            else
            {
                // none when the world starts 
                Timer.DelayCall(TimeSpan.FromSeconds(30), () => Delete());
            }
        }
    }
}
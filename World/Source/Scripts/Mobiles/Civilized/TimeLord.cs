using System;
using System.Collections;
using Server.Items;
using Server.ContextMenus;
using Server.Misc;
using Server.Network;
using Server.Localization;

namespace Server.Mobiles
{
	public class TimeLord : BasePerson
	{
		private DateTime m_NextTalk;
		public DateTime NextTalk{ get{ return m_NextTalk; } set{ m_NextTalk = value; } }

		public override void OnMovement( Mobile m, Point3D oldLocation )
		{
			if( m is PlayerMobile )
			{
				if ( DateTime.Now >= m_NextTalk && InRange( m, 4 ) && InLOS( m ) )
				{
					switch ( Utility.Random( 9 ))
					{
						case 0: CitizenLocalization.SayLocalized(this, StringCatalog.ResolveByKey(this.Account, "mob.other.the_stranger_has_saved_sosaria_from_exodus")); break;
						case 1: CitizenLocalization.SayLocalized(this, StringCatalog.ResolveByKey(this.Account, "mob.other.castle_exodus_lies_in_ruins_no_one_knowing_what_evil_lu")); break;
						case 2: CitizenLocalization.SayLocalized(this, StringCatalog.ResolveByKey(this.Account, "mob.other.mondain_s_legacy_is_forever_extinguished")); break;
						case 3: CitizenLocalization.SayLocalized(this, StringCatalog.ResolveByKey(this.Account, "mob.other.the_timeline_has_been_restored_after_the_wrath_of_minax")); break;
						case 4: CitizenLocalization.SayLocalized(this, StringCatalog.ResolveByKey(this.Account, "mob.other.one_day_the_stranger_will_return_to_sosaria")); break;
						case 5: CitizenLocalization.SayLocalized(this, StringCatalog.ResolveByKey(this.Account, "mob.other.although_some_speak_of_virtue_it_is_the_serpents_of_ord")); break;
						case 6: CitizenLocalization.SayLocalized(this, StringCatalog.ResolveByKey(this.Account, "mob.other.the_order_was_love_sol_moon_and_death")); break;
						case 7: CitizenLocalization.SayLocalized(this, StringCatalog.ResolveByKey(this.Account, "mob.other.maybe_one_day_the_stranger_will_achieve_avatarhood")); break;
						case 8: CitizenLocalization.SayLocalized(this, StringCatalog.ResolveByKey(this.Account, "mob.other.the_strings_of_time_show_the_guardian_is_coming")); break;
					};

					m_NextTalk = (DateTime.Now + TimeSpan.FromSeconds( 30 ));
				}
			}
		}

		[Constructable]
		public TimeLord() : base( )
		{
			SpeechHue = Utility.RandomTalkHue();
			NameHue = -1;
			Body = 0x190;
			Hue = 0x430;
			Name = "the Time Lord";
			Blessed = true;

			AddItem( new Sandals() );
			AddItem( new ClothCowl() );
			AddItem( new SorcererRobe() );

			SetStr( 3000, 3000 );
			SetDex( 3000, 3000 );
			SetInt( 3000, 3000 );

			SetHits( 6000,6000 );
			SetDamage( 500, 900 );

			VirtualArmor = 3000;

			SetDamageType( ResistanceType.Physical, 40 );
			SetDamageType( ResistanceType.Cold, 60 );
			SetDamageType( ResistanceType.Energy, 60 );

			SetResistance( ResistanceType.Physical, 65, 75 );
			SetResistance( ResistanceType.Fire, 35, 40 );
			SetResistance( ResistanceType.Cold, 60, 70 );
			SetResistance( ResistanceType.Poison, 60, 70 );
			SetResistance( ResistanceType.Energy, 35, 40 );

			SetSkill( SkillName.Psychology, 130.1, 140.0 );
			SetSkill( SkillName.Magery, 130.1, 140.0 );
			SetSkill( SkillName.Meditation, 110.1, 111.0 );
			SetSkill( SkillName.Poisoning, 110.1, 111.0 );
			SetSkill( SkillName.MagicResist, 185.2, 210.0 );
			SetSkill( SkillName.Tactics, 100.1, 110.0 );
			SetSkill( SkillName.FistFighting, 85.1, 110.0 );
			SetSkill( SkillName.Bludgeoning, 85.1, 110.0 );
		}

		public override bool BardImmune{ get{ return true; } }
		public override Poison PoisonImmune{ get{ return Poison.Deadly; } }
		public override bool Unprovokable { get { return true; } }
		public override bool Uncalmable{ get{ return true; } }

		public TimeLord( Serial serial ) : base( serial )
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
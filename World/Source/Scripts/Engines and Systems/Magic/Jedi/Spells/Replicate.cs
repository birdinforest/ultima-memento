using System;
using System.Collections;
using Server.Network;
using Server.Items;
using Server.Targeting;
using Server.Regions;
using Server.Mobiles;

namespace Server.Spells.Jedi
{
	public class Replicate : JediSpell
	{
		public override int spellIndex { get { return 289; } }
		public int CirclePower = 8;
		public static int spellID = 289;
		public override int RequiredTithing{ get{ return Int32.Parse(  Server.Spells.Jedi.JediSpell.SpellInfo( spellIndex, 10 )); } }
		public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds( 0.5 ); } }
		public override double RequiredSkill{ get{ return (double)(Int32.Parse( Server.Spells.Jedi.JediSpell.SpellInfo( spellIndex, 2 ))); } }
		public override int RequiredMana{ get{ return Int32.Parse(  Server.Spells.Jedi.JediSpell.SpellInfo( spellIndex, 3 )); } }

		private static SpellInfo m_Info = new SpellInfo(
				Server.Spells.Jedi.JediSpell.SpellInfo( spellID, 1 ),
				Server.Misc.Research.CapsCast( Server.Spells.Jedi.JediSpell.SpellInfo( spellID, 4 ) ),
				203,
				0
			);

		public Replicate( Mobile caster, Item scroll ) : base( caster, scroll, m_Info )
		{
		}

		public override void OnCast()
		{
			if ( CheckSequence() )
			{
				var m = Caster;
				if ( SoulOrb.Create( m, SoulOrbType.CloningCrystalJedi ) != null )
				{
					m.PlaySound( 0x244 );
					Effects.SendLocationEffect(m.Location, m.Map, 0x373A, 15, 0, 0);
					m.SendMessage( "You create a replication crystal with your genetic pattern." );
				}

				DrainCrystals( m, RequiredTithing );
			}

			FinishSequence();
		}
	}
}
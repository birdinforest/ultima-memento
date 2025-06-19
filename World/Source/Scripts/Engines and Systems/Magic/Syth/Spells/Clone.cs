using System;
using System.Collections;
using Server.Network;
using Server.Items;
using Server.Targeting;
using Server.Regions;
using Server.Mobiles;

namespace Server.Spells.Syth
{
	public class CloneBody : SythSpell
	{
		public override int spellIndex { get { return 279; } }
		public int CirclePower = 8;
		public static int spellID = 279;
		public override int RequiredTithing{ get{ return Int32.Parse(  Server.Spells.Syth.SythSpell.SpellInfo( spellIndex, 10 )); } }
		public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds( 0.5 ); } }
		public override double RequiredSkill{ get{ return (double)(Int32.Parse( Server.Spells.Syth.SythSpell.SpellInfo( spellIndex, 2 ))); } }
		public override int RequiredMana{ get{ return Int32.Parse(  Server.Spells.Syth.SythSpell.SpellInfo( spellIndex, 3 )); } }

		private static SpellInfo m_Info = new SpellInfo(
				Server.Spells.Syth.SythSpell.SpellInfo( spellID, 1 ),
				Server.Misc.Research.CapsCast( Server.Spells.Syth.SythSpell.SpellInfo( spellID, 4 ) ),
				203,
				0
			);

		public CloneBody( Mobile caster, Item scroll ) : base( caster, scroll, m_Info )
		{
		}

		public override void OnCast()
		{
			if ( CheckSequence() )
			{
				var m = Caster;
				if ( SoulOrb.Create( m, SoulOrbType.CloningCrystalSyth ) != null )
				{
					m.PlaySound( 0x243 );
					Effects.SendLocationEffect(m.Location, m.Map, 0x373A, 15, 0x81F, 0);
					m.SendMessage( "You create a cloning crystal with your genetic pattern." );
				}

				DrainCrystals( Caster, RequiredTithing );
			}

            FinishSequence();
		}
	}
}
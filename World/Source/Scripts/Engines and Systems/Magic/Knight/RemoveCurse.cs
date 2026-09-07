using System;
using Server.Network;
using Server.Items;
using Server.Targeting;
using Server.Spells.Necromancy;
using Server.Spells.Fourth;
using Server.Mobiles;
using Server.Localization;
using System.Collections.Generic;

namespace Server.Spells.Chivalry
{
	public class RemoveCurseSpell : PaladinSpell
	{
		public static readonly SpellInfo SpellInfo = new SpellInfo(
				new SpellDefinition
				{
					SpellID = 208,
					IconGraphic = 0x5108,
					Name = "Remove Curse",
					PowerWords = 1060726, // Extermo Vomica
					Description = 1061498, // Attempts to remove all Curse effects from Target. Curses include Mage spells such as Clumsy, Weaken, Feeblemind, and Paralyze, as well as all Necromancer curses. Chance of removing curse is affected by the Caster’s Karma.
					ManaCost = 20,
					TithingCost = 10,
					MinSkill = 5,
					TargetType = TargetFlags.Beneficial,
				},
				-1,
				9002
			);

		public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds( 1.5 ); } }

		public override double RequiredSkill{ get{ return SpellInfo.SpellDefinition.MinSkill; } }
		public override int RequiredMana{ get{ return SpellInfo.SpellDefinition.ManaCost; } }
		public override int RequiredTithing{ get{ return SpellInfo.SpellDefinition.TithingCost; } }
		public override int MantraNumber{ get{ return SpellInfo.SpellDefinition.PowerWords.Number; } }

		public RemoveCurseSpell( Mobile caster, Item scroll ) : base( caster, scroll, SpellInfo )
		{
		}

		public override void OnCast()
		{
			Caster.Target = new InternalTarget( this );
		}

		public static void RemoveBadThings( Mobile m )
		{
			StatMod mod;

			mod = m.GetStatMod( "[Magic] Str Offset" );
			if ( mod != null && mod.Offset < 0 )
				m.RemoveStatMod( "[Magic] Str Offset" );

			mod = m.GetStatMod( "[Magic] Dex Offset" );
			if ( mod != null && mod.Offset < 0 )
				m.RemoveStatMod( "[Magic] Dex Offset" );

			mod = m.GetStatMod( "[Magic] Int Offset" );
			if ( mod != null && mod.Offset < 0 )
				m.RemoveStatMod( "[Magic] Int Offset" );

			m.Paralyzed = false;
			BuffInfo.CleanupIcons( m, false );

			EvilOmenSpell.TryEndEffect( m );
			StrangleSpell.RemoveCurse( m );
			CorpseSkinSpell.RemoveCurse( m );
			CurseSpell.RemoveEffect( m );
			MortalStrike.EndWound( m );
			BloodOathSpell.RemoveCurse ( m );
			MindRotSpell.ClearMindRotScalar ( m );

			BuffInfo.RemoveBuff( m, BuffIcon.Clumsy );
			BuffInfo.RemoveBuff( m, BuffIcon.FeebleMind );
			BuffInfo.RemoveBuff( m, BuffIcon.Weaken );
			BuffInfo.RemoveBuff( m, BuffIcon.Curse );
			BuffInfo.RemoveBuff( m, BuffIcon.MassCurse );
			BuffInfo.RemoveBuff( m, BuffIcon.Mindrot );
			BuffInfo.RemoveBuff( m, BuffIcon.Discordance );
		}

		public void Target( Mobile m )
		{
			if ( CheckBSequence( m ) )
			{
				SpellHelper.Turn( Caster, m );

				/* Attempts to remove all Curse effects from Target.
				 * Curses include Mage spells such as Clumsy, Weaken, Feeblemind and Paralyze
				 * as well as all Necromancer curses.
				 * Chance of removing curse is affected by Caster's Karma.
				 */

				int chance = 0;

				if ( Caster.Karma < -5000 )
					chance = 0;
				else if ( Caster.Karma < 0 )
					chance = (int) Math.Sqrt( 20000 + Caster.Karma ) - 122;
				else if ( Caster.Karma < 5625 )
					chance = (int) Math.Sqrt( Caster.Karma ) + 25;
				else
					chance = 100;

				if ( chance > Utility.Random( 100 ) )
				{
					m.PlaySound( 0xF6 );
					m.PlaySound( 0x1F7 );
					m.FixedParticles( 0x3709, 1, 30, 9963, 13, 3, EffectLayer.Head );

					IEntity from = new Entity( Serial.Zero, new Point3D( m.X, m.Y, m.Z - 10 ), Caster.Map );
					IEntity to = new Entity( Serial.Zero, new Point3D( m.X, m.Y, m.Z + 50 ), Caster.Map );
					Effects.SendMovingParticles( from, to, 0x2255, 1, 0, false, false, 13, 3, 9501, 1, 0, EffectLayer.Head, 0x100 );

					Server.Spells.Chivalry.RemoveCurseSpell.RemoveBadThings( m );
				}
				else
				{
					m.PlaySound( 0x1DF );
				}
			}

			FinishSequence();
		}

		public void TargetItem( Item o, Mobile caster )
		{
			if ( caster.CheckSkill( SkillName.Knightship, 0, 100 ) && caster.Karma > 0 )
			{
				if ( o is BookBox )
				{
					Container pack = (Container)o;
						List<Item> items = new List<Item>();
						foreach (Item item in pack.Items)
						{
							items.Add(item);
						}
						foreach (Item item in items)
						{
							caster.AddToBackpack ( item );
						}
					caster.PrivateOverheadMessage(MessageType.Regular, 1153, false, "The curse has been lifted from the books.", caster.NetState);
					o.Delete();
				}
				else if ( o is CurseItem )
				{
					Container pack = (Container)o;
						List<Item> items = new List<Item>();
						foreach (Item item in pack.Items)
						{
							items.Add(item);
						}
						foreach (Item item in items)
						{
							caster.AddToBackpack ( item );
						}
					string curseName = o.Name;
						if ( curseName == ""){ curseName = "item"; }
					caster.PrivateOverheadMessage(MessageType.Regular, 1153, false, "The curse has been lifted from the " + curseName + ".", caster.NetState);
					o.Delete();
				}

				caster.PlaySound( 0xF6 );
				caster.PlaySound( 0x1F7 );
				caster.FixedParticles( 0x3709, 1, 30, 9963, 13, 3, EffectLayer.Head );
			}
			else
			{
				caster.PlaySound( 0x1DF );

				if ( o is BookBox || o is CurseItem )
					caster.SendMessage( StringCatalog.Resolve( caster.Account, "You fail to lift the curse." ) );
			}

			FinishSequence();
		}

		private class InternalTarget : Target
		{
			private RemoveCurseSpell m_Owner;

			public InternalTarget( RemoveCurseSpell owner ) : base( Core.ML ? 10 : 12, false, TargetFlags.Beneficial )
			{
				m_Owner = owner;
			}

			protected override void OnTarget( Mobile from, object o )
			{
				if ( o is PlayerMobile )
					m_Owner.Target( (Mobile) o );

				else if ( o is BookBox || o is CurseItem )
					m_Owner.TargetItem( (Item)o, from );
			}

			protected override void OnTargetFinish( Mobile from )
			{
				m_Owner.FinishSequence();
			}
		}
	}
}
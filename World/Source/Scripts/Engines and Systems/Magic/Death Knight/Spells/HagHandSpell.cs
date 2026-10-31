using System;
using Server.Network;
using Server.Items;
using Server.Targeting;
using System.Collections.Generic;

namespace Server.Spells.DeathKnight
{
	public class HagHandSpell : DeathKnightSpell
	{
		public static readonly SpellInfo SpellInfo = new SpellInfo(
			new SpellDefinition
			{
				SpellID = 754,
				IconGraphic = 0x5002,
				Name = "Hag Hand",
				PowerWords = "Haures Manibus",
				Description = "Your hand holds the powers of a hag, where it can remove curses from items and others.",
				ManaCost = 8,
				TithingCost = 7,
				MinSkill = 5,
				TargetType = TargetFlags.None
			},
			227,
			9031
		);

		public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds( 1 ); } }
		public override double RequiredSkill{ get{ return SpellInfo.SpellDefinition.MinSkill; } }
		public override int RequiredMana{ get{ return SpellInfo.SpellDefinition.ManaCost; } }
		public override int RequiredTithing{ get{ return SpellInfo.SpellDefinition.TithingCost; } }

		public HagHandSpell( Mobile caster, Item scroll ) : base( caster, scroll, SpellInfo )
		{
		}

		public override void OnCast()
		{
			Caster.Target = new InternalTarget( this );
		}

		public void Target( Mobile m )
		{
			if ( CheckBSequence( m ) )
			{
				SpellHelper.Turn( Caster, m );

				int chance = 0;
				int karma = Caster.Karma * -1;

				if ( karma < -5000 )
					chance = 0;
				else if ( karma < 0 )
					chance = (int) Math.Sqrt( 20000 + karma ) - 122;
				else if ( karma < 5625 )
					chance = (int) Math.Sqrt( karma ) + 25;
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

					DrainSoulsInLantern( Caster, RequiredTithing );
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
			if ( caster.CheckSkill( SkillName.Knightship, 0, 100 ) && ( caster.Karma * -1 ) > 0 )
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
					caster.SendMessage("You fail to lift the curse.");
			}

			FinishSequence();
		}

		private class InternalTarget : Target
		{
			private HagHandSpell m_Owner;

			public InternalTarget( HagHandSpell owner ) : base( Core.ML ? 10 : 12, false, TargetFlags.Beneficial )
			{
				m_Owner = owner;
			}

			protected override void OnTarget( Mobile from, object o )
			{
				if ( o is Mobile )
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

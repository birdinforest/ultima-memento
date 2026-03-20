using System;
using Server.Items;
using Server.Mobiles;
using Server.Targeting;
using System.Linq;

namespace Server.Spells.Ninjitsu
{
	public class Shadowjump : NinjaSpell
	{
		private static SpellInfo m_Info = new SpellInfo(
			"Shadowjump", null,
			-1,
			9002
			);

		public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds( 1.0 ); } }

		public override double RequiredSkill{ get{ return 50.0; } }
		public override int RequiredMana{ get{ return 15; } }

		public Shadowjump( Mobile caster, Item scroll ) : base( caster, scroll, m_Info )
		{
		}

		public override bool CheckCast()
		{
			PlayerMobile pm = Caster as PlayerMobile; // IsStealthing should be moved to Server.Mobiles
			if ( !pm.IsStealthing )
			{
				Caster.SendLocalizedMessage( 1063087 ); // You must be in stealth mode to use this ability.
				return false;
			}

			return base.CheckCast();
		}

		public override bool CheckDisturb( DisturbType type, bool firstCircle, bool resistable )
		{
			return false;
		}

		public override void OnCast()
		{
			Caster.SendLocalizedMessage( 1063088 ); // You prepare to perform a Shadowjump.
			Caster.Target = new InternalTarget( this );
		}

		public void Target( IPoint3D p )
		{
			IPoint3D orig = p;
			Map map = Caster.Map;

			SpellHelper.GetSurfaceTop( ref p );
			
			PlayerMobile pm = Caster as PlayerMobile; // IsStealthing should be moved to Server.Mobiles

			if ( !pm.IsStealthing )
			{
				Caster.SendLocalizedMessage( 1063087 ); // You must be in stealth mode to use this ability.
			}
			else if ( Server.Misc.WeightOverloading.IsOverloaded( Caster ) )
			{
				Caster.SendLocalizedMessage( 502359, "", 0x22 ); // Thou art too encumbered to move.
			}
			else if ( !SpellHelper.CheckTravel( Caster, TravelCheckType.TeleportFrom ) || !SpellHelper.CheckTravel( Caster, map, new Point3D( p ), TravelCheckType.TeleportTo ))
			{
			}
			else if ( map == null || !map.CanSpawnMobile( p.X, p.Y, p.Z, false ) )
			{
				Caster.SendLocalizedMessage( 502831 ); // Cannot teleport to that spot.
			}
			else if ( SpellHelper.CheckMulti( new Point3D( p ), map, true, 5 ) )
			{
				Caster.SendLocalizedMessage( 502831 ); // Cannot teleport to that spot.
			}
			else if ( CheckSequence() )
			{
				SpellHelper.Turn( Caster, orig );

				Mobile m = Caster;

				Point3D from = m.Location;
				Point3D to = new Point3D( p );

				m.Location = to;
				m.ProcessDelta();

				Effects.SendLocationParticles( EffectItem.Create( from, m.Map, EffectItem.DefaultDuration ), 0x3728, 10, 10, 2023 );

				m.PlaySound( 0x512 );
				
				Server.SkillHandlers.Stealth.OnUse( m ); // stealth check after the a jump
			}

			FinishSequence();
		}
		public class InternalTarget : Target
		{
			private Shadowjump m_Owner;

			public InternalTarget( Shadowjump owner ) : base( 11, true, TargetFlags.None )
			{
				m_Owner = owner;
			}

			protected override void OnTarget( Mobile from, object o )
			{
				if ( o is Mobile )
				{
					// Try to teleport behind the mobile
					var mobile = (Mobile)o;
					int discardZ = 0;
					var directions = Utility.GetBehindArc( mobile.Direction ).Where(d => mobile.CheckMovement( d, out discardZ )).ToList();
					if ( 0 < directions.Count )
					{
						var d = Utility.Random(directions);

						int newZ;
						mobile.CheckMovement( d, out newZ );

						int x = mobile.Location.X, y = mobile.Location.Y;
						switch( d & Direction.Mask )
						{
							case Direction.North:
								--y;
								break;
							case Direction.Right:
								++x;
								--y;
								break;
							case Direction.East:
								++x;
								break;
							case Direction.Down:
								++x;
								++y;
								break;
							case Direction.South:
								++y;
								break;
							case Direction.Left:
								--x;
								++y;
								break;
							case Direction.West:
								--x;
								break;
							case Direction.Up:
								--x;
								--y;
								break;
						}

						m_Owner.Target( new Point3D( x, y, newZ ) );
						return;
					}
				}

				var p = o as IPoint3D;
				if ( p != null )
					m_Owner.Target( p );
			}

			protected override void OnTargetFinish( Mobile from )
			{
				m_Owner.FinishSequence();
			}
		}
	}
}
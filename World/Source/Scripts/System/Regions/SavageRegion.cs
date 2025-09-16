using System;
using System.Xml;
using Server.Mobiles;

namespace Server.Regions
{
	public class SavageRegion : BaseRegion
	{
		public SavageRegion( XmlElement xml, Map map, Region parent ) : base( xml, map, parent )
		{
		}

		public override bool AllowHousing( Mobile from, Point3D p )
		{
			return false;
		}

		public override TimeSpan GetLogoutDelay( Mobile m )
		{
			return TimeSpan.Zero;
		}

		public override void AlterLightLevel( Mobile m, ref int global, ref int personal )
		{
			global = LightCycle.NightLevel;
		}

		public override bool AllowHarmful( Mobile from, Mobile target )
		{
			if (( from is PlayerMobile ) && ( target is PlayerMobile ))
				return false;
			else
				return base.AllowHarmful( from, target );
		}

		public override bool OnBeginSpellCast( Mobile m, ISpell s )
		{
			m.SendMessage( "That does not seem to work here." );
			return false;
		}
		
		public override void OnEnter( Mobile m )
		{
			base.OnEnter( m );
			if ( m is PlayerMobile )
			{
				m.SendMessage( "You find yourself in a primitive hut." );
			}

			Server.Misc.RegionMusic.MusicRegion( m, this );
		}								
	}
}
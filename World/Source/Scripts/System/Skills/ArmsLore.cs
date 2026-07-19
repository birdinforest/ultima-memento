using System;
using Server;
using Server.Network;
using Server.Mobiles;
using Server.Items;
using System.Collections.Generic;
using Server.Misc;
using System.Collections;
using Server.Targeting;
using Server.Localization;

namespace Server.SkillHandlers
{
	public class ArmsLore
	{
		public static void Initialize()
		{
			SkillInfo.Table[(int)SkillName.ArmsLore].Callback = new SkillUseCallback( OnUse );
		}

		public static TimeSpan OnUse(Mobile m)
		{
			m.Target = new InternalTarget();

			m.SendLocalizedMessage( 500349 ); // What item do you wish to get information about?

			return TimeSpan.FromSeconds( 1.0 );
		}

		[PlayerVendorTarget]
		private class InternalTarget : Target
		{
			public InternalTarget() : base( 2, false, TargetFlags.None )
			{
				AllowNonlocal = true;
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				if ( targeted is Item )
				{
					Item examine = (Item)targeted;
					RelicFunctions.IDItem( from, from, examine, SkillName.ArmsLore );
				}
			}
		}

		public static bool AvoidDurabilityHit( Mobile parent )
		{
			if ( parent == null ) return false;

			double armsLore = parent.Skills[SkillName.ArmsLore].Value;
			if ( armsLore < 50 ) return false;
			if ( Utility.Random( 5 ) != 0 ) return false; // 20% gate (was 50%)
			if ( armsLore < Utility.Random( 200 ) ) return false; // was Random(100)

			parent.SendMessage(0x3B2, StringCatalog.ResolveByKey(parent.Account, "skl.armlore.durable"));

			return true;
		}
	}
}
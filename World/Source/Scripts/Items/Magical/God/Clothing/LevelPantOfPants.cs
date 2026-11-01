using System;
using Server.Localization;

namespace Server.Items
{
	/// <summary>
	/// Bug-reporter reward pants: normal Level clothing (max 40, PointsPerLevel on level-up)
	/// with hue ping-pong along one dye-chart column (same hue family, vertical shades).
	/// Chart rows are 100 wide → same-color vertical step is +0x64.
	/// Column starting at 0x0008: 0x0008, 0x006C, …, 0x038C.
	/// </summary>
	public class LevelPantOfPants : BaseLevelPants
	{
		// Same-color vertical shades (UO dye chart column, row width 100).
		private static readonly int[] Waypoints = new int[]
		{
			0x0008, 0x006C, 0x00D0, 0x0134, 0x0198, 0x01FC, 0x0260, 0x02C4, 0x0328, 0x038C
		};

		private Timer m_FxTimer;
		private int m_WpIndex;
		private int m_WpDir = 1;

		public override string DisplayNameLocalizationKey => "item.special.pant.of.pants";

		public override string InfoDataLocalizationKey { get { return "prop.special.pant.of.pants.desc"; } }

		public override string DefaultDescription
		{
			get
			{
				return StringCatalog.Resolve( null,
					"Not a spare pair—the statement piece after a ruthlessly curated haul. Every boutique long pant that almost made the cut was edited, layered in spirit, then pressed into this single, unapologetic silhouette: the pant of pants. One look. Full closet energy. No returns, darling—this is the edit." );
			}
		}

		[Constructable]
		public LevelPantOfPants() : base( 0x1539, Waypoints[0] )
		{
			Weight = 2.0;
			Name = "The Pant of Pants";
			ItemID = Utility.RandomList( 0x1539, 0x153a );
			Hue = Waypoints[0];
			m_WpIndex = 0;
			m_WpDir = 1;
			// MaxLevel / Points: BaseLevelClothing defaults (MaxLevel 40, Points 0; +5 per level, +10 at cap)
			StartFx();
		}

		public LevelPantOfPants( Serial serial ) : base( serial )
		{
		}

		public override void AddNameProperties( ObjectPropertyList list )
		{
			base.AddNameProperties( list );

			if ( BuildingPropertyListLocale != null )
				list.Add( 1070630, ResolvePropertyText( "prop.special.pant.of.pants.infotext" ) );
			else
				list.Add( 1070630, "of the Haute Haul" );
		}

		public override void OnAfterDelete()
		{
			StopFx();
			base.OnAfterDelete();
		}

		private void StartFx()
		{
			StopFx();
			m_FxTimer = Timer.DelayCall( TimeSpan.FromSeconds( 0.25 ), TimeSpan.FromSeconds( 0.25 ), OnFxTick );
		}

		private void StopFx()
		{
			if ( m_FxTimer != null )
			{
				m_FxTimer.Stop();
				m_FxTimer = null;
			}
		}

		private void OnFxTick()
		{
			if ( Deleted )
			{
				StopFx();
				return;
			}

			int nextIdx = m_WpIndex + m_WpDir;

			if ( nextIdx >= Waypoints.Length )
			{
				m_WpDir = -1;
				nextIdx = Waypoints.Length - 2;
			}
			else if ( nextIdx < 0 )
			{
				m_WpDir = 1;
				nextIdx = 1;
			}

			m_WpIndex = nextIdx;
			Hue = Waypoints[m_WpIndex];
		}

		private void SyncPathFromHue()
		{
			int best = 0;

			for ( int i = 0; i < Waypoints.Length; i++ )
			{
				if ( Hue == Waypoints[i] )
				{
					best = i;
					break;
				}

				if ( Math.Abs( Hue - Waypoints[i] ) < Math.Abs( Hue - Waypoints[best] ) )
					best = i;
			}

			m_WpIndex = best;
			Hue = Waypoints[best];
			m_WpDir = best >= Waypoints.Length - 1 ? -1 : 1;
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 2 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();

			// v1 locked MaxLevel=1; restore normal Level cap for existing instances
			if ( version < 2 )
				MaxLevel = LevelItems.DefaultMaxLevel;

			SyncPathFromHue();
			StartFx();
		}
	}
}

using System;
using System.Collections.Generic;
using Server.Gumps;
using Server.Localization;
using Server.Misc;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;

namespace Server.Items
{
	/// <summary>
	/// Bowyer Shoppe reward: apply one Super Slayer to an ordinary empty-slot BaseRanged in the backpack.
	/// </summary>
	public class RangedSlayerSeal : Item
	{
		public override string DisplayNameLocalizationKey => "item.magical.rangedslayerseal";
		public override bool IsContentLocalized => true;

		[Constructable]
		public RangedSlayerSeal() : base( 0x14F0 )
		{
			Weight = 1.0;
			Name = "a ranged family slayer seal";
			Hue = 0x489;
			ItemID = 0x400B;
		}

		public RangedSlayerSeal( Serial serial ) : base( serial )
		{
		}

		public override void AddNameProperties( ObjectPropertyList list )
		{
			base.AddNameProperties( list );

			if ( BuildingPropertyListLocale != null )
				AddLocalizedProperty( list, "prop.magical.rangedslayerseal.desc" );
			else
				list.Add( 1070722, "Applies one Super Slayer to an ordinary empty-slot ranged weapon" );
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( !IsChildOf( from.Backpack ) )
			{
				from.SendLocalizedMessage( 1042001 );
				return;
			}

			from.CloseGump( typeof( RangedSlayerSealGump ) );
			from.SendGump( new RangedSlayerSealGump( from, this ) );
		}

		public static List<SlayerName> GetSuperSlayers()
		{
			var list = new List<SlayerName>();
			SlayerGroup[] groups = SlayerGroup.Groups;

			if ( groups == null )
				return list;

			for ( int i = 0; i < groups.Length; i++ )
			{
				SlayerGroup g = groups[i];

				if ( g == null || g.Super == null )
					continue;

				SlayerName name = g.Super.Name;

				if ( name == SlayerName.None )
					continue;

				if ( !list.Contains( name ) )
					list.Add( name );
			}

			return list;
		}

		public static string DeedShotkeyFor( SlayerName name )
		{
			switch ( name )
			{
				case SlayerName.AnimalHunter: return "prop.magical.slayer.deed.00";
				case SlayerName.ArachnidDoom: return "prop.magical.slayer.deed.01";
				case SlayerName.AvianHunter: return "prop.magical.slayer.deed.02";
				case SlayerName.ElementalBan: return "prop.magical.slayer.deed.08";
				case SlayerName.Exorcism: return "prop.magical.slayer.deed.10";
				case SlayerName.Fey: return "prop.magical.slayer.deed.11";
				case SlayerName.GiantKiller: return "prop.magical.slayer.deed.14";
				case SlayerName.GolemDestruction: return "prop.magical.slayer.deed.15";
				case SlayerName.NeptunesBane: return "prop.magical.slayer.deed.17";
				case SlayerName.Repond: return "prop.magical.slayer.deed.21";
				case SlayerName.ReptilianDeath: return "prop.magical.slayer.deed.22";
				case SlayerName.Silver: return "prop.magical.slayer.deed.24";
				case SlayerName.SlimyScourge: return "prop.magical.slayer.deed.25";
				case SlayerName.WeedRuin: return "prop.magical.slayer.deed.33";
				case SlayerName.WizardSlayer: return "prop.magical.slayer.deed.34";
				default: return null;
			}
		}

		public static string ResolveSlayerLabel( Mobile from, SlayerName name )
		{
			string key = DeedShotkeyFor( name );

			if ( !string.IsNullOrEmpty( key ) )
				return StringCatalog.ResolveByKey( from != null ? from.Account : null, key );

			return SlayerGroup.GetName( name );
		}

		public bool TryApply( Mobile from, BaseRanged weapon, SlayerName slayer, out string rejectReason )
		{
			rejectReason = null;

			if ( from == null || weapon == null || Deleted )
			{
				rejectReason = "invalid";
				return false;
			}

			if ( !IsChildOf( from.Backpack ) )
			{
				rejectReason = "seal_not_in_pack";
				from.SendLocalizedMessage( 1042001 );
				return false;
			}

			if ( weapon.RootParent != from )
			{
				rejectReason = "weapon_not_in_pack";
				from.SendMessage( StringCatalog.ResolveByKey( from.Account, "prop.magical.rangedslayerseal.msg.pack" ) );
				return false;
			}

			if ( weapon.ArtifactLevel != ArtifactLevel.None || weapon.NotModAble || weapon is IGiftable )
			{
				rejectReason = "artifact_or_gift";
				from.SendMessage( StringCatalog.ResolveByKey( from.Account, "prop.magical.rangedslayerseal.msg.ordinary" ) );
				return false;
			}

			if ( weapon.Slayer != SlayerName.None || weapon.Slayer2 != SlayerName.None )
			{
				rejectReason = "slot_occupied";
				from.SendMessage( StringCatalog.ResolveByKey( from.Account, "prop.magical.rangedslayerseal.msg.empty" ) );
				return false;
			}

			List<SlayerName> supers = GetSuperSlayers();

			if ( !supers.Contains( slayer ) )
			{
				rejectReason = "not_super";
				from.SendMessage( StringCatalog.ResolveByKey( from.Account, "prop.magical.rangedslayerseal.msg.invalid" ) );
				return false;
			}

			weapon.Slayer = slayer;
			from.SendMessage( StringCatalog.ResolveFormatByKey( from.Account, "prop.magical.rangedslayerseal.msg.done", ResolveSlayerLabel( from, slayer ) ) );

			PlayerMobile pm = from as PlayerMobile;
			AnalyticsLogger.LogRangedSlayerSealApplied( pm, this, weapon, slayer, true, null );
			Delete();
			rejectReason = null;
			return true;
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

	public class RangedSlayerSealGump : Gump
	{
		private readonly Mobile m_From;
		private readonly RangedSlayerSeal m_Seal;
		private readonly List<SlayerName> m_Options;

		public RangedSlayerSealGump( Mobile from, RangedSlayerSeal seal ) : base( 50, 50 )
		{
			m_From = from;
			m_Seal = seal;
			m_Options = RangedSlayerSeal.GetSuperSlayers();

			AddPage( 0 );
			AddBackground( 0, 0, 350, 60 + ( m_Options.Count * 22 ), 9270 );
			AddHtml( 15, 15, 320, 20, StringCatalog.ResolveByKey( from.Account, "prop.magical.rangedslayerseal.gump.title" ), false, false );

			for ( int i = 0; i < m_Options.Count; i++ )
			{
				int y = 40 + ( i * 22 );
				AddButton( 15, y, 4005, 4007, i + 1, GumpButtonType.Reply, 0 );
				AddHtml( 50, y, 280, 20, RangedSlayerSeal.ResolveSlayerLabel( from, m_Options[i] ), false, false );
			}
		}

		public override void OnResponse( NetState sender, RelayInfo info )
		{
			if ( m_Seal == null || m_Seal.Deleted || m_From == null )
				return;

			if ( info.ButtonID <= 0 || info.ButtonID > m_Options.Count )
				return;

			if ( !m_Seal.IsChildOf( m_From.Backpack ) )
			{
				m_From.SendLocalizedMessage( 1042001 );
				return;
			}

			SlayerName chosen = m_Options[info.ButtonID - 1];
			PlayerMobile pm = m_From as PlayerMobile;
			AnalyticsLogger.LogRangedSlayerSealChosen( pm, m_Seal, chosen );
			m_From.SendMessage( StringCatalog.ResolveByKey( m_From.Account, "prop.magical.rangedslayerseal.msg.prompt" ) );
			m_From.Target = new RangedSlayerSealTarget( m_Seal, chosen );
		}
	}

	public class RangedSlayerSealTarget : Target
	{
		private readonly RangedSlayerSeal m_Seal;
		private readonly SlayerName m_Slayer;

		public RangedSlayerSealTarget( RangedSlayerSeal seal, SlayerName slayer ) : base( 1, false, TargetFlags.None )
		{
			m_Seal = seal;
			m_Slayer = slayer;
		}

		protected override void OnTarget( Mobile from, object targeted )
		{
			if ( m_Seal == null || m_Seal.Deleted )
				return;

			BaseRanged weapon = targeted as BaseRanged;

			if ( weapon == null )
			{
				from.SendMessage( StringCatalog.ResolveByKey( from.Account, "prop.magical.rangedslayerseal.msg.ranged" ) );
				AnalyticsLogger.LogRangedSlayerSealApplied( from as PlayerMobile, m_Seal, null, m_Slayer, false, "not_ranged" );
				return;
			}

			string reject;
			bool ok = m_Seal.TryApply( from, weapon, m_Slayer, out reject );

			if ( !ok )
				AnalyticsLogger.LogRangedSlayerSealApplied( from as PlayerMobile, m_Seal, weapon, m_Slayer, false, reject ?? "rejected" );
		}
	}
}

using System;
using System.Collections.Generic;
using Server;
using Server.Commands.Generic;
using Server.Items;
using Server.Targeting;

namespace Server.Commands
{
	/// <summary>
	/// Staff commands to inspect and repair in-memory StatMod / SkillMod lists on mobiles.
	/// StatMod and SkillMod are not persisted in saves; orphan mods survive logout until server restart or manual fix.
	/// </summary>
	public static class StatModCommands
	{
		public static void Initialize()
		{
			CommandSystem.Register( "DumpStatMods", AccessLevel.GameMaster, new CommandEventHandler( DumpStatMods_OnCommand ) );
			CommandSystem.Register( "DumpSkillMods", AccessLevel.GameMaster, new CommandEventHandler( DumpSkillMods_OnCommand ) );

			CommandSystem.Register( "AddStatMod", AccessLevel.GameMaster, new CommandEventHandler( AddStatMod_OnCommand ) );
			CommandSystem.Register( "SetStatMod", AccessLevel.GameMaster, new CommandEventHandler( AddStatMod_OnCommand ) );

			CommandSystem.Register( "RemoveStatMod", AccessLevel.GameMaster, new CommandEventHandler( RemoveStatMod_OnCommand ) );
			CommandSystem.Register( "ClearStatMods", AccessLevel.GameMaster, new CommandEventHandler( ClearStatMods_OnCommand ) );
			CommandSystem.Register( "FixStatMods", AccessLevel.GameMaster, new CommandEventHandler( FixStatMods_OnCommand ) );

			CommandSystem.Register( "AddSkillMod", AccessLevel.GameMaster, new CommandEventHandler( AddSkillMod_OnCommand ) );
			CommandSystem.Register( "SetSkillMod", AccessLevel.GameMaster, new CommandEventHandler( AddSkillMod_OnCommand ) );

			CommandSystem.Register( "RemoveSkillMod", AccessLevel.GameMaster, new CommandEventHandler( RemoveSkillMod_OnCommand ) );
			CommandSystem.Register( "ClearSkillMods", AccessLevel.GameMaster, new CommandEventHandler( ClearSkillMods_OnCommand ) );
			CommandSystem.Register( "FixSkillMods", AccessLevel.GameMaster, new CommandEventHandler( FixSkillMods_OnCommand ) );
		}

		[Usage( "DumpStatMods" )]
		[Description( "Lists active StatMod entries on a targeted mobile (name, type, offset, duration)." )]
		private static void DumpStatMods_OnCommand( CommandEventArgs e )
		{
			e.Mobile.Target = new StatModDumpTarget();
		}

		[Usage( "DumpSkillMods" )]
		[Description( "Lists active SkillMod entries on a targeted mobile (skill, value, relative)." )]
		private static void DumpSkillMods_OnCommand( CommandEventArgs e )
		{
			e.Mobile.Target = new SkillModDumpTarget();
		}

		[Usage( "AddStatMod <Str|Dex|Int> <offset> [modName] [minutes]" )]
		[Description( "Adds or replaces a StatMod on a targeted mobile. Default modName: [GM] {Stat} Offset; minutes=0 means permanent." )]
		private static void AddStatMod_OnCommand( CommandEventArgs e )
		{
			if ( e.Length < 2 )
			{
				e.Mobile.SendMessage( "Usage: [AddStatMod <Str|Dex|Int> <offset> [modName] [minutes]" );
				return;
			}

			StatType type;
			if ( !TryParseStatType( e.GetString( 0 ), out type ) )
			{
				e.Mobile.SendMessage( "Invalid stat type. Use Str, Dex, or Int." );
				return;
			}

			int offset;
			if ( !int.TryParse( e.GetString( 1 ), out offset ) )
			{
				e.Mobile.SendMessage( "Offset must be an integer." );
				return;
			}

			string modName = e.Length >= 3 ? e.GetString( 2 ) : String.Format( "[GM] {0} Offset", type );

			TimeSpan duration = TimeSpan.Zero;
			if ( e.Length >= 4 )
			{
				double minutes;
				if ( !double.TryParse( e.GetString( 3 ), out minutes ) || minutes < 0 )
				{
					e.Mobile.SendMessage( "Minutes must be a non-negative number." );
					return;
				}

				if ( minutes > 0 )
					duration = TimeSpan.FromMinutes( minutes );
			}

			e.Mobile.Target = new AddStatModTarget( type, offset, modName, duration );
		}

		[Usage( "RemoveStatMod <modName>" )]
		[Description( "Removes a StatMod by exact name on a targeted mobile (e.g. 0x40001234Dex)." )]
		private static void RemoveStatMod_OnCommand( CommandEventArgs e )
		{
			if ( e.Length != 1 )
			{
				e.Mobile.SendMessage( "Usage: [RemoveStatMod <modName>" );
				return;
			}

			e.Mobile.Target = new RemoveStatModTarget( e.GetString( 0 ) );
		}

		[Usage( "ClearStatMods [Str|Dex|Int|all]" )]
		[Description( "Removes StatMods on a targeted mobile, optionally filtered by stat type (default: all)." )]
		private static void ClearStatMods_OnCommand( CommandEventArgs e )
		{
			StatType? filter = null;

			if ( e.Length >= 1 )
			{
				if ( Insensitive.Equals( e.GetString( 0 ), "all" ) )
					filter = null;
				else
				{
					StatType type;
					if ( !TryParseStatType( e.GetString( 0 ), out type ) )
					{
						e.Mobile.SendMessage( "Usage: [ClearStatMods [Str|Dex|Int|all]" );
						return;
					}

					filter = type;
				}
			}

			e.Mobile.Target = new ClearStatModsTarget( filter );
		}

		[Usage( "FixStatMods" )]
		[Description( "Clears all StatMods on a targeted mobile, then re-applies bonuses from currently equipped items (orphan cleanup)." )]
		private static void FixStatMods_OnCommand( CommandEventArgs e )
		{
			e.Mobile.Target = new FixStatModsTarget();
		}

		[Usage( "AddSkillMod <skill> <value> [relative]" )]
		[Description( "Adds a DefaultSkillMod on a targeted mobile. relative defaults to true." )]
		private static void AddSkillMod_OnCommand( CommandEventArgs e )
		{
			if ( e.Length < 2 )
			{
				e.Mobile.SendMessage( "Usage: [AddSkillMod <skill> <value> [relative true|false]" );
				return;
			}

			SkillName skill;
			if ( !TryParseSkillName( e.GetString( 0 ), out skill ) )
			{
				e.Mobile.SendMessage( "Invalid skill name." );
				return;
			}

			double value;
			if ( !double.TryParse( e.GetString( 1 ), out value ) )
			{
				e.Mobile.SendMessage( "Value must be a number." );
				return;
			}

			bool relative = true;
			if ( e.Length >= 3 && !bool.TryParse( e.GetString( 2 ), out relative ) )
			{
				e.Mobile.SendMessage( "Relative must be true or false." );
				return;
			}

			e.Mobile.Target = new AddSkillModTarget( skill, value, relative );
		}

		[Usage( "RemoveSkillMod <skill>" )]
		[Description( "Removes all SkillMod entries for the given skill on a targeted mobile." )]
		private static void RemoveSkillMod_OnCommand( CommandEventArgs e )
		{
			if ( e.Length != 1 )
			{
				e.Mobile.SendMessage( "Usage: [RemoveSkillMod <skill>" );
				return;
			}

			SkillName skill;
			if ( !TryParseSkillName( e.GetString( 0 ), out skill ) )
			{
				e.Mobile.SendMessage( "Invalid skill name." );
				return;
			}

			e.Mobile.Target = new RemoveSkillModTarget( skill );
		}

		[Usage( "ClearSkillMods [skill|all]" )]
		[Description( "Removes SkillMods on a targeted mobile, optionally filtered by skill (default: all)." )]
		private static void ClearSkillMods_OnCommand( CommandEventArgs e )
		{
			SkillName? filter = null;

			if ( e.Length >= 1 )
			{
				if ( Insensitive.Equals( e.GetString( 0 ), "all" ) )
					filter = null;
				else
				{
					SkillName skill;
					if ( !TryParseSkillName( e.GetString( 0 ), out skill ) )
					{
						e.Mobile.SendMessage( "Usage: [ClearSkillMods [skill|all]" );
						return;
					}

					filter = skill;
				}
			}

			e.Mobile.Target = new ClearSkillModsTarget( filter );
		}

		[Usage( "FixSkillMods" )]
		[Description( "Clears all SkillMods on a targeted mobile, then re-applies bonuses from currently equipped items." )]
		private static void FixSkillMods_OnCommand( CommandEventArgs e )
		{
			e.Mobile.Target = new FixSkillModsTarget();
		}

		private static bool TryParseStatType( string text, out StatType type )
		{
			type = StatType.Str;

			if ( Insensitive.Equals( text, "Str" ) || Insensitive.Equals( text, "Strength" ) )
			{
				type = StatType.Str;
				return true;
			}

			if ( Insensitive.Equals( text, "Dex" ) || Insensitive.Equals( text, "Dexterity" ) )
			{
				type = StatType.Dex;
				return true;
			}

			if ( Insensitive.Equals( text, "Int" ) || Insensitive.Equals( text, "Intelligence" ) )
			{
				type = StatType.Int;
				return true;
			}

			return false;
		}

		private static bool TryParseSkillName( string text, out SkillName skill )
		{
			try
			{
				skill = (SkillName)Enum.Parse( typeof( SkillName ), text, true );
				return true;
			}
			catch
			{
				skill = SkillName.Alchemy;
				return false;
			}
		}

		private static bool IsEquippedLayer( Layer layer )
		{
			return layer != Layer.Invalid && layer != Layer.Backpack && layer != Layer.Bank;
		}

		private static void SendStatSummary( Mobile from, Mobile target )
		{
			from.SendMessage( 0x59, "{0}: Raw Str {1}, Dex {2}, Int {3}", target.Name, target.RawStr, target.RawDex, target.RawInt );
			from.SendMessage( 0x59, "{0}: Effective Str {1}, Dex {2}, Int {3} ({4} StatMod(s))", target.Name, target.Str, target.Dex, target.Int, target.StatMods.Count );
		}

		private static void ClearStatModsInternal( Mobile target, StatType? filter )
		{
			List<string> names = new List<string>();

			for ( int i = 0; i < target.StatMods.Count; ++i )
			{
				StatMod mod = target.StatMods[i];

				if ( filter == null || mod.Type == filter.Value )
					names.Add( mod.Name );
			}

			for ( int i = 0; i < names.Count; ++i )
				target.RemoveStatMod( names[i] );

			target.CheckStatTimers();
		}

		private static void ClearSkillModsInternal( Mobile target, SkillName? filter )
		{
			List<SkillMod> mods = new List<SkillMod>( target.SkillMods );

			for ( int i = 0; i < mods.Count; ++i )
			{
				SkillMod mod = mods[i];

				if ( filter == null || mod.Skill == filter.Value )
					target.RemoveSkillMod( mod );
			}
		}

		private static void ApplyItemStatMods( Mobile mobile, Item item )
		{
			if ( item == null || item.Deleted || item.Parent != mobile || !IsEquippedLayer( item.Layer ) )
				return;

			if ( item is BaseClothing clothing )
			{
				clothing.AddStatBonuses( mobile );
				return;
			}

			if ( item is BaseRace race )
			{
				race.AddStatBonuses( mobile );
				return;
			}

			int strBonus = 0;
			int dexBonus = 0;
			int intBonus = 0;

			if ( item is BaseArmor armor )
			{
				strBonus = armor.ComputeStatBonus( StatType.Str );
				dexBonus = armor.ComputeStatBonus( StatType.Dex );
				intBonus = armor.ComputeStatBonus( StatType.Int );
			}
			else if ( item is BaseWeapon weapon )
			{
				strBonus = weapon.Attributes.BonusStr;
				dexBonus = weapon.Attributes.BonusDex;
				intBonus = weapon.Attributes.BonusInt;
			}
			else if ( item is BaseTrinket trinket )
			{
				strBonus = trinket.Attributes.BonusStr;
				dexBonus = trinket.Attributes.BonusDex;
				intBonus = trinket.Attributes.BonusInt;
			}
			else if ( item is Spellbook spellbook )
			{
				strBonus = spellbook.Attributes.BonusStr;
				dexBonus = spellbook.Attributes.BonusDex;
				intBonus = spellbook.Attributes.BonusInt;
			}
			else if ( item is BaseInstrument instrument )
			{
				strBonus = instrument.Attributes.BonusStr;
				dexBonus = instrument.Attributes.BonusDex;
				intBonus = instrument.Attributes.BonusInt;
			}

			if ( strBonus == 0 && dexBonus == 0 && intBonus == 0 )
				return;

			string modName = item.Serial.ToString();

			if ( strBonus != 0 )
				mobile.AddStatMod( new StatMod( StatType.Str, modName + "Str", strBonus, TimeSpan.Zero ) );

			if ( dexBonus != 0 )
				mobile.AddStatMod( new StatMod( StatType.Dex, modName + "Dex", dexBonus, TimeSpan.Zero ) );

			if ( intBonus != 0 )
				mobile.AddStatMod( new StatMod( StatType.Int, modName + "Int", intBonus, TimeSpan.Zero ) );
		}

		private static void ReapplyEquippedStatMods( Mobile mobile )
		{
			Item[] items = mobile.Items.ToArray();

			for ( int i = 0; i < items.Length; ++i )
				ApplyItemStatMods( mobile, items[i] );

			mobile.CheckStatTimers();
		}

		private static void TryApplyItemSkillBonuses( Mobile mobile, Item item )
		{
			if ( item == null || item.Deleted || item.Parent != mobile || !IsEquippedLayer( item.Layer ) )
				return;

			AosSkillBonuses bonuses = null;

			if ( item is BaseArmor armor )
				bonuses = armor.SkillBonuses;
			else if ( item is BaseClothing clothing )
				bonuses = clothing.SkillBonuses;
			else if ( item is BaseRace race )
				bonuses = race.SkillBonuses;
			else if ( item is BaseWeapon weapon )
				bonuses = weapon.SkillBonuses;
			else if ( item is BaseTrinket trinket )
				bonuses = trinket.SkillBonuses;
			else if ( item is Spellbook spellbook )
				bonuses = spellbook.SkillBonuses;
			else if ( item is BaseInstrument instrument )
				bonuses = instrument.SkillBonuses;

			if ( bonuses != null && !bonuses.IsEmpty )
				bonuses.AddTo( mobile );
		}

		private static void ReapplyEquippedSkillMods( Mobile mobile )
		{
			Item[] items = mobile.Items.ToArray();

			for ( int i = 0; i < items.Length; ++i )
				TryApplyItemSkillBonuses( mobile, items[i] );
		}

		private static Mobile ResolveTarget( Mobile from, object targeted )
		{
			if ( !( targeted is Mobile ) )
			{
				from.SendMessage( "That is not a mobile." );
				return null;
			}

			Mobile target = (Mobile)targeted;

			if ( !BaseCommand.IsAccessible( from, target ) )
			{
				from.SendMessage( "That is not accessible." );
				return null;
			}

			return target;
		}

		private sealed class StatModDumpTarget : Target
		{
			public StatModDumpTarget() : base( -1, false, TargetFlags.None )
			{
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				Mobile target = ResolveTarget( from, targeted );

				if ( target == null )
					return;

				CommandLogging.WriteLine( from, "{0} {1} dumping StatMods on {2}", from.AccessLevel, CommandLogging.Format( from ), CommandLogging.Format( target ) );

				SendStatSummary( from, target );

				if ( target.StatMods.Count == 0 )
				{
					from.SendMessage( 0x22, "No active StatMods." );
					return;
				}

				for ( int i = 0; i < target.StatMods.Count; ++i )
				{
					StatMod mod = target.StatMods[i];
					string state = mod.HasElapsed() ? "expired" : "active";

					from.SendMessage( 0x3B2, "{0}: {1} {2}{3} ({4})", i + 1, mod.Name, mod.Type, mod.Offset >= 0 ? "+" + mod.Offset : mod.Offset.ToString(), state );
				}
			}
		}

		private sealed class SkillModDumpTarget : Target
		{
			public SkillModDumpTarget() : base( -1, false, TargetFlags.None )
			{
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				Mobile target = ResolveTarget( from, targeted );

				if ( target == null )
					return;

				CommandLogging.WriteLine( from, "{0} {1} dumping SkillMods on {2}", from.AccessLevel, CommandLogging.Format( from ), CommandLogging.Format( target ) );

				if ( target.SkillMods.Count == 0 )
				{
					from.SendMessage( 0x22, "No active SkillMods." );
					return;
				}

				for ( int i = 0; i < target.SkillMods.Count; ++i )
				{
					SkillMod mod = target.SkillMods[i];
					Skill skill = target.Skills[mod.Skill];
					double shown = skill != null ? skill.Value : 0;
					double baseVal = skill != null ? skill.Base : 0;

					from.SendMessage( 0x3B2, "{0}: {1} {2}{3} relative={4} (skill value {5}, base {6})", i + 1, mod.GetType().Name, mod.Skill, mod.Value >= 0 ? "+" + mod.Value : mod.Value.ToString(), mod.Relative, shown, baseVal );
				}
			}
		}

		private sealed class AddStatModTarget : Target
		{
			private readonly StatType m_Type;
			private readonly int m_Offset;
			private readonly string m_Name;
			private readonly TimeSpan m_Duration;

			public AddStatModTarget( StatType type, int offset, string name, TimeSpan duration ) : base( -1, false, TargetFlags.None )
			{
				m_Type = type;
				m_Offset = offset;
				m_Name = name;
				m_Duration = duration;
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				Mobile target = ResolveTarget( from, targeted );

				if ( target == null )
					return;

				target.AddStatMod( new StatMod( m_Type, m_Name, m_Offset, m_Duration ) );
				target.CheckStatTimers();

				CommandLogging.WriteLine( from, "{0} {1} AddStatMod {2} on {3}: {4} {5}", from.AccessLevel, CommandLogging.Format( from ), m_Name, CommandLogging.Format( target ), m_Type, m_Offset );

				from.SendMessage( 0x59, "Added StatMod \"{0}\" ({1}{2}).", m_Name, m_Type, m_Offset >= 0 ? "+" + m_Offset : m_Offset.ToString() );
				SendStatSummary( from, target );
			}
		}

		private sealed class RemoveStatModTarget : Target
		{
			private readonly string m_Name;

			public RemoveStatModTarget( string name ) : base( -1, false, TargetFlags.None )
			{
				m_Name = name;
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				Mobile target = ResolveTarget( from, targeted );

				if ( target == null )
					return;

				if ( target.RemoveStatMod( m_Name ) )
				{
					CommandLogging.WriteLine( from, "{0} {1} RemoveStatMod {2} on {3}", from.AccessLevel, CommandLogging.Format( from ), m_Name, CommandLogging.Format( target ) );
					from.SendMessage( 0x59, "Removed StatMod \"{0}\".", m_Name );
				}
				else
				{
					from.SendMessage( 0x22, "No StatMod named \"{0}\".", m_Name );
				}

				SendStatSummary( from, target );
			}
		}

		private sealed class ClearStatModsTarget : Target
		{
			private readonly StatType? m_Filter;

			public ClearStatModsTarget( StatType? filter ) : base( -1, false, TargetFlags.None )
			{
				m_Filter = filter;
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				Mobile target = ResolveTarget( from, targeted );

				if ( target == null )
					return;

				int before = target.StatMods.Count;
				ClearStatModsInternal( target, m_Filter );
				int removed = before - target.StatMods.Count;

				CommandLogging.WriteLine( from, "{0} {1} ClearStatMods on {2} (removed {3})", from.AccessLevel, CommandLogging.Format( from ), CommandLogging.Format( target ), removed );

				from.SendMessage( 0x59, "Removed {0} StatMod(s).", removed );
				SendStatSummary( from, target );
			}
		}

		private sealed class FixStatModsTarget : Target
		{
			public FixStatModsTarget() : base( -1, false, TargetFlags.None )
			{
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				Mobile target = ResolveTarget( from, targeted );

				if ( target == null )
					return;

				int before = target.StatMods.Count;
				ClearStatModsInternal( target, null );
				ReapplyEquippedStatMods( target );
				int after = target.StatMods.Count;

				CommandLogging.WriteLine( from, "{0} {1} FixStatMods on {2}: {3} -> {4} mods", from.AccessLevel, CommandLogging.Format( from ), CommandLogging.Format( target ), before, after );

				from.SendMessage( 0x59, "FixStatMods: {0} mod(s) cleared, {1} mod(s) now active from equipped items.", before, after );
				SendStatSummary( from, target );
			}
		}

		private sealed class AddSkillModTarget : Target
		{
			private readonly SkillName m_Skill;
			private readonly double m_Value;
			private readonly bool m_Relative;

			public AddSkillModTarget( SkillName skill, double value, bool relative ) : base( -1, false, TargetFlags.None )
			{
				m_Skill = skill;
				m_Value = value;
				m_Relative = relative;
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				Mobile target = ResolveTarget( from, targeted );

				if ( target == null )
					return;

				DefaultSkillMod mod = new DefaultSkillMod( m_Skill, m_Relative, m_Value );
				mod.ObeyCap = true;
				target.AddSkillMod( mod );

				CommandLogging.WriteLine( from, "{0} {1} AddSkillMod {2} {3} on {4}", from.AccessLevel, CommandLogging.Format( from ), m_Skill, m_Value, CommandLogging.Format( target ) );

				Skill skill = target.Skills[m_Skill];
				from.SendMessage( 0x59, "Added SkillMod {0} {1}{2}. Value now {3} (base {4}).", m_Skill, m_Relative ? "relative " : "", m_Value, skill != null ? skill.Value : 0, skill != null ? skill.Base : 0 );
			}
		}

		private sealed class RemoveSkillModTarget : Target
		{
			private readonly SkillName m_Skill;

			public RemoveSkillModTarget( SkillName skill ) : base( -1, false, TargetFlags.None )
			{
				m_Skill = skill;
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				Mobile target = ResolveTarget( from, targeted );

				if ( target == null )
					return;

				List<SkillMod> mods = new List<SkillMod>( target.SkillMods );
				int removed = 0;

				for ( int i = 0; i < mods.Count; ++i )
				{
					if ( mods[i].Skill == m_Skill )
					{
						target.RemoveSkillMod( mods[i] );
						removed++;
					}
				}

				CommandLogging.WriteLine( from, "{0} {1} RemoveSkillMod {2} on {3} (removed {4})", from.AccessLevel, CommandLogging.Format( from ), m_Skill, CommandLogging.Format( target ), removed );

				from.SendMessage( 0x59, "Removed {0} SkillMod(s) for {1}.", removed, m_Skill );
			}
		}

		private sealed class ClearSkillModsTarget : Target
		{
			private readonly SkillName? m_Filter;

			public ClearSkillModsTarget( SkillName? filter ) : base( -1, false, TargetFlags.None )
			{
				m_Filter = filter;
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				Mobile target = ResolveTarget( from, targeted );

				if ( target == null )
					return;

				int before = target.SkillMods.Count;
				ClearSkillModsInternal( target, m_Filter );
				int removed = before - target.SkillMods.Count;

				CommandLogging.WriteLine( from, "{0} {1} ClearSkillMods on {2} (removed {3})", from.AccessLevel, CommandLogging.Format( from ), CommandLogging.Format( target ), removed );

				from.SendMessage( 0x59, "Removed {0} SkillMod(s).", removed );
			}
		}

		private sealed class FixSkillModsTarget : Target
		{
			public FixSkillModsTarget() : base( -1, false, TargetFlags.None )
			{
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				Mobile target = ResolveTarget( from, targeted );

				if ( target == null )
					return;

				int before = target.SkillMods.Count;
				ClearSkillModsInternal( target, null );
				ReapplyEquippedSkillMods( target );
				int after = target.SkillMods.Count;

				CommandLogging.WriteLine( from, "{0} {1} FixSkillMods on {2}: {3} -> {4} mods", from.AccessLevel, CommandLogging.Format( from ), CommandLogging.Format( target ), before, after );

				from.SendMessage( 0x59, "FixSkillMods: {0} mod(s) cleared, {1} mod(s) now active from equipped items.", before, after );
			}
		}
	}
}

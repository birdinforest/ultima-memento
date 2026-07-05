using System;
using System.Collections.Generic;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Misc;

namespace Server.Engines.Avatar
{
	public static class AvatarCoreItemMigration
	{
		private const double ResourceDecayRate = 0.5;
		private const int MaterialResonanceGoldCost = 5000;

		public static bool HasMigratableCoreItems( Mobile mobile )
		{
			if ( FindResearchBag( mobile ) != null || FindAncientSpellbook( mobile ) != null )
				return true;

			var pm = mobile as PlayerMobile;
			return pm != null && pm.Avatar.Active && HasResearchSnapshot( pm.Avatar );
		}

		public static void ReattachCoreItems( PlayerMobile player )
		{
			if ( player == null || !player.Avatar.Active )
				return;

			var ctx = player.Avatar;
			var bank = player.BankBox;
			if ( bank == null )
				return;

			var bag = ctx.GetResearchBag();
			if ( bag != null )
			{
				bag.RebindOwner( player );
				MoveToBank( bag, bank );
				ctx.SetResearchBagSerial( bag );

				if ( bag.IsDormant )
				{
					EnsureMemoryEchoAssignment( ctx, bag );
					Research.RestoreResearchBagFromSnapshot( bag, ctx );
				}
			}

			var book = ctx.GetAncientSpellbook();
			if ( book != null )
			{
				book.RebindOwner( player );
				MoveToBank( book, bank );
				ctx.SetAncientSpellbookSerial( book );

				if ( bag != null )
					Research.SyncAncientSpellbookFromBag( bag, book );
			}
		}

		public static void SnapshotCoreItemsOnDeath( PlayerMobile player, PlayerContext ctx )
		{
			if ( player == null || ctx == null || !ctx.Active )
				return;

			ResearchBag bag = FindResearchBag( player );
			if ( bag != null )
			{
				bag.SnapshotToContext( ctx );
				ctx.SetResearchBagSerial( bag );
			}

			AncientSpellbook book = FindAncientSpellbook( player );
			if ( book != null )
			{
				book.SnapshotToContext( ctx );
				ctx.SetAncientSpellbookSerial( book );
			}
		}

		public static void MigrateItems( PlayerMobile oldChar, PlayerMobile newChar, PlayerContext ctx )
		{
			if ( oldChar == null || newChar == null || ctx == null || !ctx.Active )
				return;

			ctx.AvatarDeathCount++;

			var bank = newChar.BankBox;
			if ( bank == null )
				return;

			ResearchBag bag = FindResearchBag( oldChar );
			if ( bag == null && ctx.HasResearchBag )
				bag = ctx.GetResearchBag();

			AncientSpellbook book = FindAncientSpellbook( oldChar );
			if ( book == null && ctx.HasAncientSpellbook )
				book = ctx.GetAncientSpellbook();

			if ( bag != null )
			{
				ctx.RebirthReportInkBefore = bag.BagInk;
				ctx.RebirthReportScrollsBefore = bag.BagScrolls;
				ctx.RebirthReportQuillsBefore = bag.BagQuills;
				ctx.RebirthReportPrepBefore = CountPreparedSpells( bag );

				bag.SnapshotToContext( ctx );
				bag.ApplyResourceDecay();
				bag.RebindOwner( newChar );
				bag.IsDormant = true;

				ctx.RebirthReportInkAfter = bag.BagInk;
				ctx.RebirthReportScrollsAfter = bag.BagScrolls;
				ctx.RebirthReportQuillsAfter = bag.BagQuills;
				ctx.RebirthReportPrepAfter = CountPreparedSpells( bag );

				AssignMemoryEcho( ctx, bag );

				MoveToBank( bag, bank );
				ctx.SetResearchBagSerial( bag );
			}

			if ( book != null )
			{
				book.SnapshotToContext( ctx );
				book.ApplyResourceDecay();
				book.RebindOwner( newChar );
				book.IsDormant = true;

				MoveToBank( book, bank );
				ctx.SetAncientSpellbookSerial( book );

				if ( bag != null )
					Research.SyncAncientSpellbookFromBag( bag, book );
			}

			if ( bag == null && HasResearchSnapshot( ctx ) )
				bag = RebuildResearchBagFromSnapshot( newChar, ctx );

			ctx.HasPendingRebirthReport = bag != null || book != null || HasResearchSnapshot( ctx );
		}

		public static void CompleteResonance( PlayerMobile player, ResearchBag bag, ResonancePath path )
		{
			if ( player == null || bag == null || !bag.IsDormant )
				return;

			if ( bag.BagOwner != player )
				return;

			if ( path == ResonancePath.Search && !IsAtMemoryEcho( player, player.Avatar ) )
				return;

			if ( path == ResonancePath.Registrar && !TryPayMaterialResonance( player ) )
				return;

			var ctx = player.Avatar;

			bag.RestoreFromContext( ctx );
			bag.ActivateResonance( player );

			var book = ctx.GetAncientSpellbook() ?? FindAncientSpellbook( player );
			if ( book != null && book.IsDormant )
			{
				book.RestoreFromContext( ctx );

				if ( bag != null )
					Research.SyncAncientSpellbookFromBag( bag, book );

				book.ActivateResonance( player );
				ctx.SetAncientSpellbookSerial( book );
			}

			PlayResonanceCompleteEffects( player, bag );

			player.SendGump( new ResearchResonanceCompleteGump( player, bag ) );
		}

		public static void TryShowRebirthReport( PlayerMobile player )
		{
			if ( player == null || !player.Avatar.Active )
				return;

			var ctx = player.Avatar;
			if ( !ctx.HasPendingRebirthReport || ctx.SuppressRebirthReport )
				return;

			if ( ctx.GetResearchBag() == null && !HasResearchSnapshot( ctx ) && !HasMigratableCoreItems( player ) )
				return;

			player.SendGump( new AvatarRebirthReportGump( player ) );
		}

		public static ResearchBag RebuildResearchBagFromSnapshot( PlayerMobile player, PlayerContext ctx )
		{
			if ( player == null || ctx == null || !HasResearchSnapshot( ctx ) )
				return null;

			if ( ctx.HasResearchBag )
				return ctx.GetResearchBag();

			var bag = new ResearchBag();
			bag.RestoreFromContext( ctx );
			bag.RebindOwner( player );
			bag.IsDormant = true;
			AssignMemoryEcho( ctx, bag );

			player.BankBox.AddItem( bag );
			ctx.SetResearchBagSerial( bag );
			return bag;
		}

		public static bool IsAtMemoryEcho( Mobile mobile, PlayerContext ctx )
		{
			if ( mobile == null || ctx == null || string.IsNullOrEmpty( ctx.CurrentResonanceLocation ) )
				return false;

			return mobile.Region != null && mobile.Region.Name == ctx.CurrentResonanceLocation;
		}

		public static int CountPreparedSpells( ResearchBag bag )
		{
			if ( bag == null )
				return 0;

			return SumPrepString( bag.ResearchPrep1 ) + SumPrepString( bag.ResearchPrep2 );
		}

		public static int CountCubesFound( ResearchBag bag )
		{
			if ( bag == null || string.IsNullOrEmpty( bag.RuneFound ) )
				return 0;

			int count = 0;
			string[] parts = bag.RuneFound.Split( '#' );
			for ( int i = 0; i < parts.Length; i++ )
			{
				int v;
				if ( int.TryParse( parts[i], out v ) && v > 0 )
					count++;
			}

			return count;
		}

		public static bool HasResearchSnapshot( PlayerContext ctx )
		{
			return ctx != null && (
				!string.IsNullOrEmpty( ctx.SnapshotRuneFound ) ||
				!string.IsNullOrEmpty( ctx.SnapshotSpellsMagery ) ||
				!string.IsNullOrEmpty( ctx.SnapshotSpellsNecromancy ) ||
				!string.IsNullOrEmpty( ctx.SnapshotResearchSpells ) );
		}

		public static void EnsureMemoryEchoAssignment( PlayerContext ctx, ResearchBag bag )
		{
			if ( ctx == null || bag == null )
				return;

			if ( IsInvalidMemoryEchoTarget( ctx, bag ) )
			{
				AssignMemoryEcho( ctx, bag );
				return;
			}

			if ( ctx.MemoryEchoSearchBaseSerial != Serial.Zero )
			{
				Item item = World.FindItem( ctx.MemoryEchoSearchBaseSerial );
				SearchBase searchBase = item as SearchBase;

				if ( searchBase != null && !searchBase.Deleted && searchBase.Map != null && searchBase.Map != Map.Internal )
					return;
			}

			if ( !string.IsNullOrEmpty( ctx.CurrentResonanceLocation ) )
			{
				SearchBase searchBase = MemoryEchoUtility.PickSearchBaseInRegion( ctx.CurrentResonanceLocation );

				if ( searchBase != null )
				{
					ctx.MemoryEchoSearchBaseSerial = searchBase.Serial;
					return;
				}
			}

			AssignMemoryEcho( ctx, bag );
		}

		private static bool IsInvalidMemoryEchoTarget( PlayerContext ctx, ResearchBag bag )
		{
			if ( ctx == null || bag == null || string.IsNullOrEmpty( ctx.CurrentResonanceLocation ) )
				return false;

			if ( IsActiveNextCubeHuntRegion( bag, ctx.CurrentResonanceLocation ) )
				return true;

			return MemoryEchoUtility.FindSearchBasesInRegion( ctx.CurrentResonanceLocation ).Count == 0;
		}

		private static bool IsActiveNextCubeHuntRegion( ResearchBag bag, string regionName )
		{
			if ( bag == null || string.IsNullOrEmpty( regionName ) || string.IsNullOrEmpty( bag.RuneLocation ) )
				return false;

			if ( Research.GetRunes( bag, 26 ) )
				return false;

			return Insensitive.Equals( bag.RuneLocation, regionName );
		}

		public static void AssignMemoryEcho( PlayerContext ctx, ResearchBag bag )
		{
			var valid = new List<Tuple<string, string, SearchBase>>();

			if ( !string.IsNullOrEmpty( bag.LastRuneFoundLocation ) )
				TryAddMemoryEchoCandidate( valid, bag, bag.LastRuneFoundLocation, "rune" );

			TryAddMemoryEchoCandidate( valid, bag, bag.SpellsMageLocation, "mage" );
			TryAddMemoryEchoCandidate( valid, bag, bag.SpellsNecroLocation, "necro" );
			TryAddMemoryEchoCandidate( valid, bag, bag.BagInkLocation, "ink" );
			TryAddMemoryEchoCandidate( valid, bag, bag.ResearchLocation, "research" );

			if ( valid.Count > 0 )
			{
				Tuple<string, string, SearchBase> pick = valid[Utility.Random( valid.Count )];
				ctx.CurrentResonanceLocation = pick.Item1;
				ctx.ResonanceLocationType = pick.Item2;
				ctx.MemoryEchoSearchBaseSerial = pick.Item3.Serial;
				return;
			}

			SearchBase fallback = MemoryEchoUtility.PickAnySearchBaseForResearch( bag );

			if ( fallback != null )
			{
				string regionName = Worlds.GetRegionName( fallback.Map, fallback.Location );

				if ( !IsActiveNextCubeHuntRegion( bag, regionName ) )
				{
					ctx.CurrentResonanceLocation = regionName;
					ctx.ResonanceLocationType = InferEchoTypeForLocation( bag, regionName );
					ctx.MemoryEchoSearchBaseSerial = fallback.Serial;
					return;
				}
			}

			ctx.CurrentResonanceLocation = "Britain";
			ctx.ResonanceLocationType = "starter";
			ctx.MemoryEchoSearchBaseSerial = Serial.Zero;
		}

		private static void TryAddMemoryEchoCandidate( List<Tuple<string, string, SearchBase>> valid, ResearchBag bag, string regionName, string category )
		{
			if ( string.IsNullOrEmpty( regionName ) )
				return;

			if ( IsActiveNextCubeHuntRegion( bag, regionName ) )
				return;

			SearchBase searchBase = MemoryEchoUtility.PickSearchBaseInRegion( regionName );

			if ( searchBase == null )
				return;

			for ( int i = 0; i < valid.Count; ++i )
			{
				if ( Insensitive.Equals( valid[i].Item1, regionName ) )
					return;
			}

			valid.Add( Tuple.Create( regionName, category, searchBase ) );
		}

		private static string InferEchoTypeForLocation( ResearchBag bag, string regionName )
		{
			if ( !string.IsNullOrEmpty( bag.LastRuneFoundLocation ) && Insensitive.Equals( bag.LastRuneFoundLocation, regionName ) )
				return "rune";
			if ( Insensitive.Equals( bag.SpellsMageLocation, regionName ) )
				return "mage";
			if ( Insensitive.Equals( bag.SpellsNecroLocation, regionName ) )
				return "necro";
			if ( Insensitive.Equals( bag.BagInkLocation, regionName ) )
				return "ink";
			if ( Insensitive.Equals( bag.ResearchLocation, regionName ) )
				return "research";

			return "starter";
		}

		private static void MoveToBank( Item item, BankBox bank )
		{
			if ( item == null || bank == null )
				return;

			if ( item.Parent != bank )
				bank.DropItem( item );
		}

		public static ResearchBag FindResearchBag( Mobile mobile )
		{
			ResearchBag found = null;
			ForEachItem<ResearchBag>( mobile, bag => { if ( found == null ) found = bag; } );
			return found;
		}

		public static AncientSpellbook FindAncientSpellbook( Mobile mobile )
		{
			AncientSpellbook found = null;
			ForEachItem<AncientSpellbook>( mobile, book => { if ( found == null ) found = book; } );
			return found;
		}

		private static void ForEachItem<T>( Mobile mobile, Action<T> action ) where T : Item
		{
			if ( mobile == null || action == null )
				return;

			if ( mobile.Backpack != null )
				ScanContainer( mobile.Backpack, action );

			if ( mobile.BankBox != null )
				ScanContainer( mobile.BankBox, action );

			Container corpse = mobile.Corpse;
			if ( corpse != null && !corpse.Deleted )
				ScanContainer( corpse, action );

			if ( mobile is PlayerMobile )
			{
				var ctx = ((PlayerMobile)mobile).Avatar;
				if ( ctx != null && ctx.HasSafetyDepositBox )
				{
					var box = ctx.GetOrCreateSafetyDepositBox( mobile );
					if ( box != null )
						ScanContainer( box, action );
				}
			}

			foreach ( Item item in mobile.Items )
			{
				if ( item is T )
					action( (T)item );
				else if ( item is Container )
					ScanContainer( (Container)item, action );
			}
		}

		private static void ScanContainer<T>( Container container, Action<T> action ) where T : Item
		{
			if ( container == null )
				return;

			foreach ( Item item in container.Items )
			{
				if ( item is T )
					action( (T)item );
				else if ( item is Container )
					ScanContainer( (Container)item, action );
			}
		}

		private static int SumPrepString( string prep )
		{
			if ( string.IsNullOrEmpty( prep ) )
				return 0;

			int total = 0;
			string[] parts = prep.Split( '#' );
			for ( int i = 0; i < parts.Length; i++ )
			{
				int v;
				if ( int.TryParse( parts[i], out v ) )
					total += v;
			}

			return total;
		}

		internal static string HalvePrepString( string prep )
		{
			if ( string.IsNullOrEmpty( prep ) )
				return prep;

			string[] parts = prep.Split( '#' );
			for ( int i = 0; i < parts.Length; i++ )
			{
				int v;
				if ( int.TryParse( parts[i], out v ) )
					parts[i] = (v / 2).ToString();
			}

			return string.Join( "#", parts );
		}

		private static bool TryPayMaterialResonance( PlayerMobile player )
		{
			if ( player == null )
				return false;

			if ( Banker.Withdraw( player, MaterialResonanceGoldCost ) )
				return true;

			ResearchLocalization.Send( player, "research.resonance.msg.registrar_no_gold", "You cannot afford the material resonance offering." );
			return false;
		}

		private static void PlayResonanceCompleteEffects( PlayerMobile player, ResearchBag bag )
		{
			player.PlaySound( 0x3D );
			player.FixedParticles( 0x373A, 1, 15, 9502, EffectLayer.Head );

			if ( bag != null && bag.Map != null )
				Effects.SendLocationEffect( bag.Location, bag.Map, 0x376A, 10, 1 );

			ResearchLocalization.Send( player, "research.resonance.overhead.complete", "The runes answer! Ancient knowledge opens anew." );
		}
	}
}

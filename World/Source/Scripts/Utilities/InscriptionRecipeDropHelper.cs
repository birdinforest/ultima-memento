using System;
using Server.Engines.Avatar;
using Server.Engines.Craft;
using Server.Items;
using Server.Localization;
using Server.Misc;
using Server.Mobiles;
using Server.RateConfig;

namespace Server
{
	public static class InscriptionRecipeDropHelper
	{
		public static void TryDropRecipe( BaseCreature creature )
		{
			if ( creature == null || MySettings.S_UseLegacyInscription )
				return;

			var eligible = RelicChestDropHelper.GetEligibleTopPlayers(
				creature,
				InscriptionRecipeDropConfig.TopN,
				InscriptionRecipeDropConfig.Range );

			if ( eligible == null || eligible.Count == 0 )
				return;

			DefInscription inscription = DefInscription.CraftSystem as DefInscription;

			if ( inscription == null )
				return;

			string encounterId = RelicChestDropHelper.BuildEncounterId( creature );
			int damageTotal = RelicChestDropHelper.SumEncounterDamage( creature );
			string bossType = creature.GetType().Name;

			for ( int i = 0; i < eligible.Count; i++ )
			{
				RelicEligiblePlayer entry = eligible[i];
				PlayerMobile player = entry.Player;

				if ( player == null )
					continue;

				bool forAvatar = player.Avatar.Active;
				InscriptionEnemyTierEntry enemyTier = InscriptionRecipeDropConfig.GetEnemyTier( creature.Fame, forAvatar );

				if ( enemyTier == null )
					continue;

				double fortuneMult = AscentHuntBonus.GetDropChanceMultiplier(
					player,
					creature,
					InscriptionRecipeDropConfig.TopN,
					InscriptionRecipeDropConfig.Range,
					eligible );
				double rollMaxPct = InscriptionRecipeDropConfig.GetRollMaxPct( entry.Rank, enemyTier );
				double rollActualPct = InscriptionRecipeDropConfig.GetEffectiveRollPct( player.Luck, entry.Rank, enemyTier, fortuneMult );
				bool success = InscriptionRecipeDropConfig.RollDropChance( player.Luck, entry.Rank, enemyTier, fortuneMult );

				AnalyticsLogger.LogInscriptionRecipeRollAttempted(
					player,
					new RareDropRollContext
					{
						EncounterId = encounterId,
						BossType = bossType,
						BossKey = bossType,
						DamageRank = entry.Rank,
						DamageDealt = entry.Damage,
						DamageTotal = damageTotal,
						EligibleTop3Count = eligible.Count,
						Luck = player.Luck,
						FortuneMult = fortuneMult,
						RollMaxPct = rollMaxPct,
						RollActualPct = rollActualPct,
						RollSuccess = success,
						AvatarActive = forAvatar,
						AvatarLadder = forAvatar,
						EnemyTierId = enemyTier.Id ?? "",
						Fame = creature.Fame
					} );

				if ( !success )
					continue;

				int tier = InscriptionRecipeDropConfig.PickTier( enemyTier );
				RecipeScroll scroll = inscription.GetRandomRecipeScrollInTier( player, tier );

				if ( scroll == null )
					continue;

				DeliverScroll( player, scroll );
			}
		}

		private static string GetRecipeDisplayName( RecipeScroll scroll )
		{
			if ( scroll == null )
				return "";

			Recipe recipe = scroll.Recipe;

			if ( recipe == null )
				return scroll.GetType().Name;

			TextDefinition td = recipe.TextDefinition;

			if ( td.Number > 0 )
				return CliLocTable.LookupEnglish( td.Number ) ?? ( "#" + td.Number );

			return td.String ?? scroll.GetType().Name;
		}

		private static void DeliverScroll( PlayerMobile player, RecipeScroll scroll )
		{
			if ( player == null || scroll == null )
				return;

			if ( !player.AddToBackpack( scroll ) )
				scroll.MoveToWorld( player.Location, player.Map );

			string recipeName = StringCatalog.Resolve( player.Account, GetRecipeDisplayName( scroll ) );

			player.SendMessage(
				StringCatalog.ResolveFormatByKey(
					player.Account,
					"sys.inscription.recipe_drop.announce",
					recipeName ) );
		}
	}
}

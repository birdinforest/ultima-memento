using System;
using Server;
using Server.Accounting;
using Server.Engines.Craft;
using Server.Mobiles;
using Server.Network;

namespace Server.Items
{
	public class RecipeScroll : Item
	{
		public override int LabelNumber { get { return 1074560; } } // recipe scroll

		private int m_RecipeID;

		[CommandProperty( AccessLevel.GameMaster )]
		public int RecipeID
		{
			get { return m_RecipeID; }
			set { m_RecipeID = value; InvalidateProperties(); }
		}

		private Mobile m_Owner;

		[CommandProperty( AccessLevel.GameMaster )]
		public Mobile Owner
		{
			get { return m_Owner; }
			set
			{
				m_Owner = value;
				InvalidateProperties();
			}
		}

		public Recipe Recipe
		{
			get
			{
				if ( Recipe.Recipes.ContainsKey( m_RecipeID ) )
					return Recipe.Recipes[m_RecipeID];

				return null;
			}
		}

		public override void GetProperties( ObjectPropertyList list )
		{
			base.GetProperties( list );

			Recipe r = this.Recipe;

			if( r != null )
				list.Add( 1049644, r.TextDefinition.ToString() ); // ~1_stuff~
			if( Owner != null && !MySettings.S_RecipesForAnyone)
				list.Add( 1049644, string.Format("Belongs to: {0}", Owner.Name) ); // ~1_stuff~
		}

		public RecipeScroll( Recipe r )
			: this( r.ID )
		{
		}

		[Constructable]
		public RecipeScroll( int recipeID )
			: base( 0x2831 )
		{
			m_RecipeID = recipeID;
		}

		public RecipeScroll( Serial serial )
			: base( serial )
		{
		}

		public override void OnDoubleClick( Mobile from )
		{
			if( !from.InRange( this.GetWorldLocation(), 2 ) )
			{
				from.LocalOverheadMessage( MessageType.Regular, 0x3B2, 1019045 ); // I can't reach that.
				return;
			}

			Recipe r = this.Recipe;

			if( r != null && from is PlayerMobile )
			{
				PlayerMobile pm = from as PlayerMobile;

				var owner = GetAccountOrNull(m_Owner);
				if (!MySettings.S_RecipesForAnyone && owner != null && owner != GetAccountOrNull(pm) )
				{
                    pm.SendLocalizedMessage(1112589); // This does not belong to you! Find your own!
					return;
				}

                if (pm.HasRecipe(r))
                {
                    pm.SendLocalizedMessage(1073427); // You already know this recipe.
					return;
                }

				if ( MySettings.S_RecipesRequireMinSkill )
				{
					bool allRequiredSkills = true;
					double chance = r.CraftItem.GetSuccessChance(from, null, r.CraftSystem, false, ref allRequiredSkills);
					if (!allRequiredSkills || chance <= 0.0)
					{
						pm.SendLocalizedMessage(1044153); // You don't have the required skills to attempt this item.
						return;
					}
				}

				pm.SendLocalizedMessage(1073451, r.TextDefinition.ToString()); // You have learned a new recipe: ~1_RECIPE~
				pm.AcquireRecipe(r);
				Delete();
            }
		}

		private static IAccount GetAccountOrNull(Mobile mobile)
		{
			return mobile != null && mobile.Account != null ? mobile.Account : null;
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int)1 ); // version

			writer.Write( (int)m_RecipeID );
			writer.WriteMobile( m_Owner );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			m_RecipeID = reader.ReadInt();
			if (0 < version)
			{
				m_Owner = reader.ReadMobile();
			}
		}
	}
}
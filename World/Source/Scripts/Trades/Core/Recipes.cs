using System;
using System.Collections.Generic;
using Server.Mobiles;
using Server.Commands;

namespace Server.Engines.Craft
{
	public class Recipe
	{
		public static readonly Dictionary<Type, int> RecipeIndexStartMap = new Dictionary<Type, int>
		{
			{ typeof(DefAlchemy),			1000  },
			{ typeof(DefApothecary),		2000  },
			{ typeof(DefBlacksmithy),		3000  },
			{ typeof(DefBonecrafting),		4000  },
			{ typeof(DefBowFletching),		5000  },
			{ typeof(DefCarpentry),			6000  },
			{ typeof(DefCartography),		7000  },
			{ typeof(DefCooking),			8000  },
			{ typeof(DefDraconic),			9000  },
			{ typeof(DefDruidism),			10000 },
			{ typeof(DefGlassblowing),		11000 },
			{ typeof(DefInscription),		12000 },
			{ typeof(DefLapidary),			13000 },
			{ typeof(DefLeatherworking),	14000 },
			{ typeof(DefMasonry),			15000 },
			{ typeof(DefShelves),			16000 },
			{ typeof(DefStitching),			17000 },
			{ typeof(DefTailoring),			18000 },
			{ typeof(DefTinkering),			19000 },
			{ typeof(DefWands),				20000 },
			{ typeof(DefWaxingPot),			21000 },
			{ typeof(DefWitchery),			22000 },
		};

		public static void Initialize()
		{
			CommandSystem.Register( "LearnAllRecipes", AccessLevel.GameMaster, new CommandEventHandler( LearnAllRecipes_OnCommand ) );
			CommandSystem.Register( "ForgetAllRecipes", AccessLevel.GameMaster, new CommandEventHandler( ForgetAllRecipes_OnCommand ) );
		}

		[Usage( "LearnAllRecipes" )]
		[Description( "Teaches a player all available recipes." )]
		private static void LearnAllRecipes_OnCommand( CommandEventArgs e )
		{
			Mobile m = e.Mobile;
			m.SendMessage( "Target a player to teach them all of the recipes." );

			m.BeginTarget( -1, false, Server.Targeting.TargetFlags.None, new TargetCallback(
				delegate( Mobile from, object targeted )
				{
					if( targeted is PlayerMobile )
					{
						foreach( KeyValuePair<int, Recipe> kvp in m_Recipes )
							((PlayerMobile)targeted).AcquireRecipe( kvp.Key );

						m.SendMessage( "You teach them all of the recipes." );
					}
					else
					{
						m.SendMessage( "That is not a player!" );
					}
				}
			) );
		}

		[Usage( "ForgetAllRecipes" )]
		[Description( "Makes a player forget all the recipes they've learned." )]
		private static void ForgetAllRecipes_OnCommand( CommandEventArgs e )
		{
			Mobile m = e.Mobile;
			m.SendMessage( "Target a player to have them forget all of the recipes they've learned." );

			m.BeginTarget( -1, false, Server.Targeting.TargetFlags.None, new TargetCallback(
				delegate( Mobile from, object targeted )
				{
					if( targeted is PlayerMobile )
					{
						((PlayerMobile)targeted).ResetRecipes();

						m.SendMessage( "They forget all their recipes." );
					}
					else
					{
						m.SendMessage( "That is not a player!" );
					}
				}
			) );
		}


		private static Dictionary<int, Recipe> m_Recipes = new Dictionary<int, Recipe>();

		public static Dictionary<int, Recipe> Recipes { get { return m_Recipes; } }

		private static int m_LargestRecipeID;
		public  static int LargestRecipeID{ get{ return m_LargestRecipeID; } }

		private CraftSystem m_System;

		public CraftSystem CraftSystem
		{
			get { return m_System; }
			set { m_System = value; }
		}

		private CraftItem m_CraftItem;

		public CraftItem CraftItem
		{
			get { return m_CraftItem; }
			set { m_CraftItem = value; }
		}

		private int m_ID;

		public int ID
		{
			get { return m_ID; }
		}

		private TextDefinition m_TD;
		public TextDefinition TextDefinition
		{
			get
			{
				if( m_TD == null )
					m_TD = new TextDefinition( m_CraftItem.NameNumber, m_CraftItem.NameString );

				return m_TD;
			}
		}

		public Recipe( int id, CraftSystem system, CraftItem item )
		{
			m_ID = id;
			m_System = system;
			m_CraftItem = item;

			if( m_Recipes.ContainsKey( id ) )
				throw new Exception( "Attempting to create recipe with preexisting ID." );

			m_Recipes.Add( id, this );
			m_LargestRecipeID = Math.Max( id, m_LargestRecipeID );
		}
	}
}
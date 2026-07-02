using System;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Craft
{
	public class DefWaxingPot : CraftSystem
	{
		public override SkillName MainSkill
		{
			get	{ return SkillName.Cooking; }
		}

        public override int GumpImage
        {
            get { return 9607; }
        }

        public override int GumpTitleNumber
        {
            get { return 0; }
        }
 
        public override string GumpTitleString
        {
            get { return "apiculture.craft.gump.title"; }
        }

		public override string CraftSystemTxt
		{
			get { return "apiculture.craft.system.txt"; }
		}

		private static CraftSystem m_CraftSystem;

		public static CraftSystem CraftSystem
		{
			get
			{
				if ( m_CraftSystem == null )
					m_CraftSystem = new DefWaxingPot();

				return m_CraftSystem;
			}
		}

		public override CraftECA ECA{ get{ return CraftECA.ChanceMinusSixtyToFourtyFive; } }

		public override double GetChanceAtMin( CraftItem item )
		{
			return 0.5; // 50%
		}

		private DefWaxingPot() : base( 1, 1, 1.25 )// base( 1, 1, 4.5 )
		{
		}

		public override int CanCraft( Mobile from, BaseTool tool, Type itemType )
		{
			if( tool == null || tool.Deleted || tool.UsesRemaining < 0 )
				return 1044038; // You have worn out your tool!
			else if ( !BaseTool.CheckAccessible( tool, from ) )
				return 1044263; // The tool must be on your person to use.

			return 0;
		}

		public override void PlayCraftEffect( Mobile from )
		{
			CraftSystem.CraftSound( from, 0x04E, m_Tools );
		}

		public override int PlayEndingEffect( Mobile from, bool failed, bool lostMaterial, bool toolBroken, int quality, CraftItem item )
		{
			if ( toolBroken )
				from.SendLocalizedMessage( 1044038 ); // You have worn out your tool

			if ( failed )
			{
				if ( lostMaterial )
					return 1044043; // You failed to create the item, and some of your materials are lost.
				else
					return 1044157; // You failed to create the item, but no materials were lost.
			}
			else
			{
				return 1044154; // You create the item.
			}
		}

		public override void InitCraftList()
		{
			int index = -1;

/*
encaustic painting - mix dyes with wax - get a canvas - make a painting
wax tablets of spells
wax sculptors
*/

			#region Candles

			index = AddCraft(typeof(Candle), "apiculture.craft.group.candles", "apiculture.craft.recipe.candle_small", 5.0, 45.0, typeof( Beeswax ), 1025154, 20, 1042081 );
			AddRes( index, typeof( IronIngot ), 1044036, 2, 1042081 );

			index = AddCraft(typeof(CandleLarge), "apiculture.craft.group.candles", "apiculture.craft.recipe.candle_large", 15.0, 55.0, typeof( Beeswax ), 1025154, 20, 1042081 );
			AddRes( index, typeof( IronIngot ), 1044036, 2, 1042081 );

			AddCraft( typeof( ColorCandleShort ), "apiculture.craft.group.candles", "apiculture.craft.recipe.candle_short_dyeable", 10.0, 50.0, typeof( Beeswax ), 1025154, 10, 1042081 );

			AddCraft( typeof( ColorCandleLong ), "apiculture.craft.group.candles", "apiculture.craft.recipe.candle_tall_dyeable", 20.0, 60.0, typeof( Beeswax ), 1025154, 20, 1042081 );

			index = AddCraft(typeof(WallSconce), "apiculture.craft.group.candles", "apiculture.craft.recipe.candle_sconce_wall", 50.0, 90.0, typeof( Beeswax ), 1025154, 20, 1042081 );
			AddRes( index, typeof( IronIngot ), 1044036, 2, 1042081 );

			index = AddCraft(typeof(CandleSkull), "apiculture.craft.group.candles", "apiculture.craft.recipe.candle_skull", 50.0, 90.0, typeof( Beeswax ), 1025154, 20, 1042081 );
			AddRes( index, typeof( Head ), "apiculture.craft.res.human_head", 1, 1042081 );

			index = AddCraft(typeof(CandleReligious), "apiculture.craft.group.candles", "apiculture.craft.recipe.candle_religious", 80.0, 120.0, typeof( Beeswax ), 1025154, 20, 1042081 );
			AddRes( index, typeof( IronIngot ), 1044036, 2, 1042081 );

			#endregion

			#region Rub

			index = AddCraft(typeof(JarsOfWaxInstrument), "apiculture.craft.group.wax_polish", "apiculture.craft.recipe.jar_instrument_polish", 60.0, 100.0, typeof( Beeswax ), 1025154, 10, 1042081 );
			AddRes( index, typeof ( Bottle ), 1044529, 1, 500315 );

			index = AddCraft(typeof(JarsOfWaxLeather), "apiculture.craft.group.wax_polish", "apiculture.craft.recipe.jar_leather_polish", 60.0, 100.0, typeof( Beeswax ), 1025154, 10, 1042081 );
			AddRes( index, typeof ( Bottle ), 1044529, 1, 500315 );

			index = AddCraft(typeof(JarsOfWaxMetal), "apiculture.craft.group.wax_polish", "apiculture.craft.recipe.jar_metal_polish", 60.0, 100.0, typeof( Beeswax ), 1025154, 10, 1042081 );
			AddRes( index, typeof ( Bottle ), 1044529, 1, 500315 );

			#endregion

			#region Paintings

			index = AddCraft(typeof(WaxPainting), "apiculture.craft.group.encaustic_paintings", "apiculture.craft.recipe.painting_large", 60.0, 100.0, typeof( Beeswax ), 1025154, 50, 1042081 );
			AddRes( index, typeof ( Dyes ), "apiculture.craft.res.dyes", 1, 1042081 );
			AddRes( index, typeof ( PaintCanvas ), "apiculture.craft.res.painting_canvas", 1, 1042081 );
			AddRes( index, typeof ( Board ), "apiculture.craft.res.boards", 4, 1042081 );

			index = AddCraft(typeof(WaxPaintingA), "apiculture.craft.group.encaustic_paintings", "apiculture.craft.recipe.painting", 60.0, 100.0, typeof( Beeswax ), 1025154, 30, 1042081 );
			AddRes( index, typeof ( Dyes ), "apiculture.craft.res.dyes", 1, 1042081 );
			AddRes( index, typeof ( PaintCanvas ), "apiculture.craft.res.painting_canvas", 1, 1042081 );
			AddRes( index, typeof ( Board ), "apiculture.craft.res.boards", 4, 1042081 );

			index = AddCraft(typeof(WaxPaintingB), "apiculture.craft.group.encaustic_paintings", "apiculture.craft.recipe.painting", 60.0, 100.0, typeof( Beeswax ), 1025154, 30, 1042081 );
			AddRes( index, typeof ( Dyes ), "apiculture.craft.res.dyes", 1, 1042081 );
			AddRes( index, typeof ( PaintCanvas ), "apiculture.craft.res.painting_canvas", 1, 1042081 );
			AddRes( index, typeof ( Board ), "apiculture.craft.res.boards", 4, 1042081 );

			index = AddCraft(typeof(WaxPaintingC), "apiculture.craft.group.encaustic_paintings", "apiculture.craft.recipe.painting", 60.0, 100.0, typeof( Beeswax ), 1025154, 30, 1042081 );
			AddRes( index, typeof ( Dyes ), "apiculture.craft.res.dyes", 1, 1042081 );
			AddRes( index, typeof ( PaintCanvas ), "apiculture.craft.res.painting_canvas", 1, 1042081 );
			AddRes( index, typeof ( Board ), "apiculture.craft.res.boards", 4, 1042081 );

			index = AddCraft(typeof(WaxPaintingD), "apiculture.craft.group.encaustic_paintings", "apiculture.craft.recipe.painting", 60.0, 100.0, typeof( Beeswax ), 1025154, 30, 1042081 );
			AddRes( index, typeof ( Dyes ), "apiculture.craft.res.dyes", 1, 1042081 );
			AddRes( index, typeof ( PaintCanvas ), "apiculture.craft.res.painting_canvas", 1, 1042081 );
			AddRes( index, typeof ( Board ), "apiculture.craft.res.boards", 4, 1042081 );

			index = AddCraft(typeof(WaxPaintingE), "apiculture.craft.group.encaustic_paintings", "apiculture.craft.recipe.painting", 60.0, 100.0, typeof( Beeswax ), 1025154, 30, 1042081 );
			AddRes( index, typeof ( Dyes ), "apiculture.craft.res.dyes", 1, 1042081 );
			AddRes( index, typeof ( PaintCanvas ), "apiculture.craft.res.painting_canvas", 1, 1042081 );
			AddRes( index, typeof ( Board ), "apiculture.craft.res.boards", 4, 1042081 );

			index = AddCraft(typeof(WaxPaintingF), "apiculture.craft.group.encaustic_paintings", "apiculture.craft.recipe.painting", 60.0, 100.0, typeof( Beeswax ), 1025154, 30, 1042081 );
			AddRes( index, typeof ( Dyes ), "apiculture.craft.res.dyes", 1, 1042081 );
			AddRes( index, typeof ( PaintCanvas ), "apiculture.craft.res.painting_canvas", 1, 1042081 );
			AddRes( index, typeof ( Board ), "apiculture.craft.res.boards", 4, 1042081 );

			index = AddCraft(typeof(WaxPaintingG), "apiculture.craft.group.encaustic_paintings", "apiculture.craft.recipe.painting", 60.0, 100.0, typeof( Beeswax ), 1025154, 30, 1042081 );
			AddRes( index, typeof ( Dyes ), "apiculture.craft.res.dyes", 1, 1042081 );
			AddRes( index, typeof ( PaintCanvas ), "apiculture.craft.res.painting_canvas", 1, 1042081 );
			AddRes( index, typeof ( Board ), "apiculture.craft.res.boards", 4, 1042081 );

			#endregion

			#region Sculptors

			AddCraft(typeof(WaxSculptors), "apiculture.craft.group.wax_sculptors", "apiculture.craft.recipe.sculptor", 60.0, 100.0, typeof( Beeswax ), 1025154, 40, 1042081 );
			AddCraft(typeof(WaxSculptorsA), "apiculture.craft.group.wax_sculptors", "apiculture.craft.recipe.sculptor", 60.0, 100.0, typeof( Beeswax ), 1025154, 40, 1042081 );
			AddCraft(typeof(WaxSculptorsB), "apiculture.craft.group.wax_sculptors", "apiculture.craft.recipe.sculptor", 60.0, 100.0, typeof( Beeswax ), 1025154, 40, 1042081 );
			AddCraft(typeof(WaxSculptorsC), "apiculture.craft.group.wax_sculptors", "apiculture.craft.recipe.sculptor", 60.0, 100.0, typeof( Beeswax ), 1025154, 40, 1042081 );
			AddCraft(typeof(WaxSculptorsD), "apiculture.craft.group.wax_sculptors", "apiculture.craft.recipe.sculptor_angel", 60.0, 100.0, typeof( Beeswax ), 1025154, 40, 1042081 );
			AddCraft(typeof(WaxSculptorsE), "apiculture.craft.group.wax_sculptors", "apiculture.craft.recipe.sculptor_dragon", 80.0, 120.0, typeof( Beeswax ), 1025154, 60, 1042081 );

			#endregion

			BreakDown = true;
		}
	}
}

using System;
using Server;
using Server.Items;
using Server.Localization;

namespace Server.Items
{
	public class AlchemyTub : Item
	{
		public override bool IsContentLocalized => true;

		[Constructable]
		public AlchemyTub() : base( 0x126A )
		{
			Name = "alchemy tub";
			Weight = 50.0;
		}

        public override void AddNameProperty( ObjectPropertyList list )
		{
			if ( BuildingPropertyListLocale != null )
			{
				AddLocalizedProperty( list, "item.trade.alchemy.tub" );
				return;
			}
			base.AddNameProperty( list );
		}

        public override void AddNameProperties(ObjectPropertyList list)
		{
            base.AddNameProperties(list);
			if ( BuildingPropertyListLocale != null )
			{
				AddLocalizedProperty( list, "prop.trade.alchemytub.place.home" );
				AddLocalizedProperty( list, "prop.trade.alchemytub.cleans" );
			}
			else
			{
				list.Add( 1070722, "Place In Your Home");
				list.Add( 1049644, "Cleans Jars And Bottles");
			}
        } 

		public AlchemyTub( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 1 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}

		public override bool OnDragDrop( Mobile from, Item item )
		{
			if ( this.Movable != false )
			{
				from.SendMessage( StringCatalog.ResolveByKey( from.Account, "prop.trade.alchemytub.msg.must.home" ) );
				return false;
			}
			else if ( item is Bottle || item is Jar || ( item is CrystallineJar && item.Name == "crystalline jar" ) )
			{
				from.SendMessage( StringCatalog.ResolveByKey( from.Account, "prop.trade.alchemytub.msg.already.clean" ) );
				return false;
			}
			else
			{
				int jar = 0;
				int bottle = 0;
				int crystal = 0;

				if ( item is BaseMixture ){ jar = 1; }
				else if ( item is BasePotion ){ bottle = 1; }
				else if ( item is AutoResPotion ){ bottle = 1; }
				else if ( item is ShieldOfEarthPotion ){ jar = 1; }
				else if ( item is WoodlandProtectionPotion ){ jar = 1; }
				else if ( item is ProtectiveFairyPotion ){ jar = 1; }
				else if ( item is HerbalHealingPotion ){ jar = 1; }
				else if ( item is GraspingRootsPotion ){ jar = 1; }
				else if ( item is BlendWithForestPotion ){ jar = 1; }
				else if ( item is SwarmOfInsectsPotion ){ jar = 1; }
				else if ( item is VolcanicEruptionPotion ){ jar = 1; }
				else if ( item is TreefellowPotion ){ jar = 1; }
				else if ( item is StoneCirclePotion ){ jar = 1; }
				else if ( item is DruidicRunePotion ){ jar = 1; }
				else if ( item is LureStonePotion ){ jar = 1; }
				else if ( item is NaturesPassagePotion ){ jar = 1; }
				else if ( item is MushroomGatewayPotion ){ jar = 1; }
				else if ( item is RestorativeSoilPotion ){ jar = 1; }
				else if ( item is FireflyPotion ){ jar = 1; }
				else if ( item is HellsGateScroll ){ jar = 1; }
				else if ( item is ManaLeechScroll ){ jar = 1; }
				else if ( item is NecroCurePoisonScroll ){ jar = 1; }
				else if ( item is NecroPoisonScroll ){ jar = 1; }
				else if ( item is NecroUnlockScroll ){ jar = 1; }
				else if ( item is PhantasmScroll ){ jar = 1; }
				else if ( item is RetchedAirScroll ){ jar = 1; }
				else if ( item is SpectreShadowScroll ){ jar = 1; }
				else if ( item is UndeadEyesScroll ){ jar = 1; }
				else if ( item is VampireGiftScroll ){ jar = 1; }
				else if ( item is WallOfSpikesScroll ){ jar = 1; }
				else if ( item is BloodPactScroll ){ jar = 1; }
				else if ( item is GhostlyImagesScroll ){ jar = 1; }
				else if ( item is GhostPhaseScroll ){ jar = 1; }
				else if ( item is GraveyardGatewayScroll ){ jar = 1; }
				else if ( item is HellsBrandScroll ){ jar = 1; }
				else if ( item is MagicalDyes ){ bottle = 1; }
				else if ( item is BottleOfAcid ){ bottle = 1; }
				else if ( item is CrystallineJar ){ crystal = 1; }
				else if ( item is NecroSkinPotion ){ jar = 1; }
				else if ( item is UnusualDyes ){ jar = 1; }
				else if ( item is TransmutationPotion ){ bottle = 1; }
				else if ( item is BeverageBottle ){ bottle = 1; }

				if ( jar > 0 || bottle > 0 || crystal > 0 )
				{
					int give = 1;
					if ( item.Amount > 1 ){ give = item.Amount; }

					string subKey;
					if ( jar > 0 )
					{
						from.AddToBackpack( new Jar(give) );
						subKey = give > 1 ? "prop.trade.alchemytub.cleaned.jars" : "prop.trade.alchemytub.cleaned.jar";
					}
					else if ( crystal > 0 )
					{
						from.AddToBackpack( new CrystallineJar() );
						subKey = give > 1 ? "prop.trade.alchemytub.cleaned.flasks" : "prop.trade.alchemytub.cleaned.flask";
					}
					else
					{
						from.AddToBackpack( new Bottle(give) );
						subKey = give > 1 ? "prop.trade.alchemytub.cleaned.bottles" : "prop.trade.alchemytub.cleaned.bottle";
					}

					string part = StringCatalog.ResolveByKey( from.Account, subKey );
					from.SendMessage( StringCatalog.ResolveFormatByKey( from.Account, "prop.trade.alchemytub.msg.wash", part ) );
					from.PlaySound( 0x026 );

					this.Hue = Utility.RandomColor(0);
					item.Delete();
					return true;
				}
				else
				{
					from.SendMessage( StringCatalog.ResolveByKey( from.Account, "prop.trade.alchemytub.msg.containers.only" ) );
					return false;
				}
			}
		}
	}
}

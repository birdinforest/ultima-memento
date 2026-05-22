using System;
using Server;
using Server.Localization;


namespace Server.Items
{
	public class SciFiJunk : Item
	{
		public override int Hue{ get{ return 0; } }

		[Constructable]
		public SciFiJunk() : base( 0x27FE )
		{
Name = StringCatalog.Resolve(null, "broken plastic bottle");
			Technology = true;
			Weight = 1.0 * Utility.RandomMinMax(1,5);
			Resource = CraftResource.Iron;
			SubResource = ResourceMods.SciFiResource( CraftResource.Iron );

			CoinPrice = Utility.RandomMinMax(60,520);

			switch( Utility.RandomMinMax( 1, 60 ) )
			{
				case 1: ItemID = 0x3562; Name = StringCatalog.Resolve(null, "binoculars");  break;
				case 2: ItemID = 0x2021; Name = StringCatalog.Resolve(null, "bolt");  break;
				case 3: ItemID = 0x2022; Name = StringCatalog.Resolve(null, "bulb");  break;
				case 4: ItemID = 0x2023; Name = StringCatalog.Resolve(null, "can");  break;
				case 5: ItemID = 0x2024; Name = StringCatalog.Resolve(null, "chips");  break;
				case 6: ItemID = 0x346D; Name = StringCatalog.Resolve(null, "circuit board");  break;
				case 7: ItemID = 0x33FF; Name = StringCatalog.Resolve(null, "coil");  break;
				case 8: ItemID = 0x2025; Name = StringCatalog.Resolve(null, "communicator");  break;
				case 9: ItemID = 0x2026; Name = StringCatalog.Resolve(null, "conduit");  break;
				case 10: ItemID = 0x2027; Name = StringCatalog.Resolve(null, "connector");  break;
				case 11: ItemID = 0x2029; Name = StringCatalog.Resolve(null, "coupler");  break;
				case 12: ItemID = 0x3A75; Name = StringCatalog.Resolve(null, "data card");  break;
				case 13: ItemID = 0x27FB; Name = StringCatalog.Resolve(null, "data pad");  break;
				case 14: ItemID = 0x34BC; Name = StringCatalog.Resolve(null, "deck plate");  break;
				case 15: ItemID = 0x34C1; Name = StringCatalog.Resolve(null, "engine");  break;
				case 16: ItemID = 0x34C6; Name = StringCatalog.Resolve(null, "expansion board");  break;
				case 17: ItemID = 0x34D6; Name = StringCatalog.Resolve(null, "filter");  break;
				case 18: ItemID = 0x3563; Name = StringCatalog.Resolve(null, "fire extinguisher");  break;
				case 19: ItemID = 0x34D7; Name = StringCatalog.Resolve(null, "fuel can");  break;
				case 20: ItemID = 0x3542; Name = StringCatalog.Resolve(null, "gas mask");  break;
				case 21: ItemID = 0x202C; Name = StringCatalog.Resolve(null, "gear");  break;
				case 22: ItemID = 0x202D; Name = StringCatalog.Resolve(null, "gear");  break;
				case 23: ItemID = 0x202E; Name = StringCatalog.Resolve(null, "gear");  break;
				case 24: ItemID = 0x2FB8; Name = StringCatalog.Resolve(null, "goggles");  break;
				case 25: ItemID = 0x34D8; Name = StringCatalog.Resolve(null, "hull plate");  break;
				case 26: ItemID = 0x202F; Name = StringCatalog.Resolve(null, "lense");  break;
				case 27: ItemID = 0x2028; Name = StringCatalog.Resolve(null, "levers");  break;
				case 28: ItemID = 0x27FD; Name = StringCatalog.Resolve(null, "medical case");  break;
				case 29: ItemID = 0x3461; Name = StringCatalog.Resolve(null, "meter");  break;
				case 30: ItemID = 0x2030; Name = StringCatalog.Resolve(null, "hex nuts");  break;
				case 31: ItemID = 0x3543; Name = StringCatalog.Resolve(null, "oil can");  break;
				case 32: ItemID = 0x2031; Name = StringCatalog.Resolve(null, "phaser");  break;
				case 33: ItemID = 0x96F; Name = StringCatalog.Resolve(null, "plasma grenade");  break;
				case 34: ItemID = 0x27FE; Name = StringCatalog.Resolve(null, "plastic bottle"); break;
				case 35: ItemID = 0x2032; Name = StringCatalog.Resolve(null, "plug");  break;
				case 36: ItemID = 0x2033; Name = StringCatalog.Resolve(null, "processor");  break;
				case 37: ItemID = 0x202B; Name = StringCatalog.Resolve(null, "puzzle cube");  break;
				case 38: ItemID = 0x2034; Name = StringCatalog.Resolve(null, "relay");  break;
				case 39: ItemID = 0x2035; Name = StringCatalog.Resolve(null, "remote");  break;
				case 40: ItemID = 0x2036; Name = StringCatalog.Resolve(null, "rivet");  break;
				case 41: ItemID = 0x343A; Name = StringCatalog.Resolve(null, "roll of tape");  break;
				case 42: ItemID = 0x270F; Name = StringCatalog.Resolve(null, "saw");  break;
				case 43: ItemID = 0x2A2F; Name = StringCatalog.Resolve(null, "screwdriver");  break;
				case 44: ItemID = 0x3544; Name = StringCatalog.Resolve(null, "sheet metal");  break;
				case 45: ItemID = 0x27FF; Name = StringCatalog.Resolve(null, "syringe");  break;
				case 46: ItemID = 0x3446; Name = StringCatalog.Resolve(null, "transistor");  break;
				case 47: ItemID = 0x344D; Name = StringCatalog.Resolve(null, "tube");  break;
				case 48: ItemID = 0x3458; Name = StringCatalog.Resolve(null, "washers");  break;
				case 49: ItemID = 0x2D86; Name = StringCatalog.Resolve(null, "welder");  break;
				case 50: ItemID = 0x2D0D; Name = StringCatalog.Resolve(null, "wire");  break;
				case 51: ItemID = 0x3467; Name = StringCatalog.Resolve(null, "wire");  break;
				case 52: ItemID = 0x3545; Name = StringCatalog.Resolve(null, "wrench");  break;
				case 53: ItemID = 0x3EA2; Name = StringCatalog.Resolve(null, "landmine");  break;
				case 54: ItemID = 0x48E4; Name = StringCatalog.Resolve(null, "canteen");  break;
				case 55: ItemID = 0x3F65; Name = StringCatalog.Resolve(null, "bowcaster");  break;
				case 56: ItemID = 0x3F8F; Name = StringCatalog.Resolve(null, "bowcaster");  break;
				case 57: ItemID = 0x4C14; Name = StringCatalog.Resolve(null, "detonator");  break;
				case 58: ItemID = 0x4C13; Name = StringCatalog.Resolve(null, "machine");  break;
				case 59: ItemID = Utility.RandomMinMax( 0x5408, 0x5409 ); Name = StringCatalog.Resolve(null, "chainsaw");  break;
				case 60: ItemID = Utility.RandomMinMax( 0x540A, 0x540B ); Name = StringCatalog.Resolve(null, "portable smelter");  break;
			}

			Name = RandomCondition() + " " + Name;
		}

		public SciFiJunk( Serial serial ) : base( serial )
		{
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

		public static string RandomCondition()
		{
			string condition = "broken";
			switch( Utility.RandomMinMax( 1, 6 ) )
			{
				case 1: condition = "broken";		break;
				case 2: condition = "ruined";		break;
				case 3: condition = "busted";		break;
				case 4: condition = "damaged";		break;
				case 5: condition = "defective";	break;
				case 6: condition = "wrecked";		break;
			}

			return condition;
		}
	}
}
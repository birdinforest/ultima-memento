using System;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Accounting;
using System.Collections.Generic;
using System.Linq; //Unique Naming System//

namespace Server.Misc
{
	public class CharacterCreation
	{
		public const string GENERIC_NAME = "Generic Player"; //Unique Naming System//
		public static void Initialize()
		{
			// Register our event handler
			EventSink.CharacterCreated += new CharacterCreatedEventHandler( EventSink_CharacterCreated );
		}

		private static void AddBackpack( Mobile m )
		{
			Container pack = m.Backpack;

			if ( pack == null )
			{
				pack = new Backpack();
				pack.Movable = false;

				m.AddItem( pack );
			}

			// PackItem( new BeginnerBook() );

			//---------------------------------------------
			if ( MyServerSettings.StartingGold() > 0 )
				PackItem(pack, new Gold( MyServerSettings.StartingGold() ) );

			PackItem(pack,  new Pitcher( BeverageType.Water ) );

			switch ( Utility.RandomMinMax( 1, 2 ) )
			{
				case 1: PackItem(pack,  new Dagger() ); break;
				case 2: PackItem(pack,  new LargeKnife() ); break;
			}
			//---------------------------------------------
			Container bag = new Bag();
			int food = 10;
			while ( food > 0 )
			{
				food--;
				bag.DropItem( Loot.RandomFoods( true, true ) );
			}
			PackItem(pack,  bag );
			//---------------------------------------------
			int light = 2;
			while ( light > 0 )
			{
				light--;
				switch ( Utility.RandomMinMax( 1, 3 ) )
				{
					case 1: PackItem(pack, new Torch() ); break;
					case 2: PackItem(pack, new Lantern() ); break;
					case 3: PackItem(pack, new Candle() ); break;
				}
			}
			//---------------------------------------------

			((PlayerMobile)m).WeaponBarOpen = 1;
			((PlayerMobile)m).GumpHue = 1;
		}

		private static Mobile CreateMobile( Account a )
		{
			if ( a.Count >= a.Limit )
				return null;

			for ( int i = 0; i < a.Length; ++i )
			{
				if ( a[i] == null )
					return (a[i] = new PlayerMobile());
			}

			return null;
		}

		private static void EventSink_CharacterCreated( CharacterCreatedEventArgs args )
		{
			if ( !VerifyProfession( args.Profession ) )
				args.Profession = 0;

			NetState state = args.State;

			if ( state == null )
				return;

			Mobile newChar = CreateMobile( args.Account as Account );

			if ( newChar == null )
			{
				Console.WriteLine( "Login: {0}: Character creation failed, account full", state );
				return;
			}

			args.Mobile = newChar;
			m_Mobile = newChar;

			newChar.Player = true;
			newChar.StatCap = 250; 
			((PlayerMobile)newChar).SetCharacterType( CharacterType.Default );
			newChar.AccessLevel = args.Account.AccessLevel;
			newChar.Female = args.Female;
			newChar.Race = Race.Human;
			newChar.RaceMakeSounds = true;

			newChar.Hue = newChar.Race.ClipSkinHue( args.Hue & 0x3FFF ) | 0x8000;

			if ( newChar.Hue >= 33770 ){ newChar.Hue = newChar.Hue - 32768; }

			newChar.Hunger = 20;
			newChar.Thirst = 20;

			bool young = false;

			if ( newChar is PlayerMobile )
			{
				PlayerMobile pm = (PlayerMobile) newChar;
				pm.PublicInfo = true;
				young = pm.Young = false;
			}

			SetName( newChar, args.Name );

			AddBackpack( newChar );

			SetStats( newChar, state, args.Str, args.Dex, args.Int );
			SkillNameValue[] setSkills = SetSkills( newChar, args.Skills, args.Profession );
            AddSkillBasedItems(newChar, setSkills);

			newChar.Mana = args.Int * 2;
			newChar.Hits = args.Str * 2;
			newChar.Stam = args.Dex * 2;

			Race race = newChar.Race;

			if( race.ValidateHair( newChar, args.HairID ) )
			{
				newChar.HairItemID = args.HairID;
				newChar.HairHue = race.ClipHairHue( args.HairHue );
				newChar.RecordsHair( true );
			}

			if( race.ValidateFacialHair( newChar, args.BeardID ) )
			{
				newChar.FacialHairItemID = args.BeardID;
				newChar.FacialHairHue = race.ClipHairHue( args.BeardHue );
				newChar.RecordsHair( true );
			}

			Server.Misc.IntelligentAction.DressUpMerchants( newChar );

			switch ( Utility.RandomMinMax( 1, 3 ) )
			{
				case 1: Item torch = new Torch(); 		newChar.AddItem( torch ); 	torch.OnDoubleClick( newChar); 	break;
				case 2: Item lamp = new Lantern(); 		newChar.AddItem( lamp ); 	lamp.OnDoubleClick( newChar); 	break;
				case 3: Item candle = new Candle(); 	newChar.AddItem( candle );	candle.OnDoubleClick( newChar); break;
			}

			newChar.RecordFeatures( true );

			CityInfo city = new CityInfo( "Sosaria", "Forest", 3579, 3423, 0, Map.Sosaria );

			newChar.MoveToWorld( city.Location, city.Map );

			Console.WriteLine( "Login: {0}: New character being created (account={1})", state, args.Account.Username );

			new WelcomeTimer( newChar ).Start();
		}

		public static bool VerifyProfession( int profession )
		{
			if ( profession < 0 )
				return false;
			else if ( profession < 4 )
				return true;
			else if ( Core.AOS && profession < 6 )
				return true;
			else if ( Core.SE && profession < 8 )
				return true;
			else
				return false;
		}

		private class BadStartMessage : Timer
		{
			Mobile m_Mobile;
			int m_Message;
			public BadStartMessage( Mobile m, int message ) : base( TimeSpan.FromSeconds ( 3.5 ) )
			{
				m_Mobile = m;
				m_Message = message;
				this.Start();
			}

			protected override void OnTick()
			{
				m_Mobile.SendLocalizedMessage( m_Message );
			}
		}

		private static void FixStats( ref int str, ref int dex, ref int intel, int max )
		{
			int vMax = max - 30;

			int vStr = str - 10;
			int vDex = dex - 10;
			int vInt = intel - 10;

			if ( vStr < 0 )
				vStr = 0;

			if ( vDex < 0 )
				vDex = 0;

			if ( vInt < 0 )
				vInt = 0;

			int total = vStr + vDex + vInt;

			if ( total == 0 || total == vMax )
				return;

			double scalar = vMax / (double)total;

			vStr = (int)(vStr * scalar);
			vDex = (int)(vDex * scalar);
			vInt = (int)(vInt * scalar);

			FixStat( ref vStr, (vStr + vDex + vInt) - vMax, vMax );
			FixStat( ref vDex, (vStr + vDex + vInt) - vMax, vMax );
			FixStat( ref vInt, (vStr + vDex + vInt) - vMax, vMax );

			str = vStr + 10;
			dex = vDex + 10;
			intel = vInt + 10;
		}

		private static void FixStat( ref int stat, int diff, int max )
		{
			stat += diff;

			if ( stat < 0 )
				stat = 0;
			else if ( stat > max )
				stat = max;
		}

		private static void SetStats( Mobile m, NetState state, int str, int dex, int intel )
		{
			int max = state.NewCharacterCreation ? 90 : 80;

			FixStats( ref str, ref dex, ref intel, max );

			if ( str < 10 || str > 60 || dex < 10 || dex > 60 || intel < 10 || intel > 60 || (str + dex + intel) != max )
			{
				str = 10;
				dex = 10;
				intel = 10;
			}

			m.InitStats( str, dex, intel );
		}

		private static void SetName( Mobile m, string name )
		{
			name = name.Trim();

			if (!CheckDupe(m, name))
				m.Name = GENERIC_NAME;
			else
				m.Name = name;
		}
      
		public static bool CheckDupe( Mobile m, string name )
		{
			if( m == null || name == null || name.Length == 0 )
				return false;

			name = name.Trim(); //Trim the name and re-assign it

			if( !NameVerification.Validate( name, 2, 16, true, true, true, 1, NameVerification.SpaceDashPeriodQuote ) )
				return false;

			foreach( Mobile wm in World.Mobiles.Values )
			{
				if( wm != m && !wm.Deleted && wm is PlayerMobile && Insensitive.Equals(wm.RawName, name) ) //Filter Mobiles by PlayerMobile type and do the name check in one go, no need for another list.
					return false; // No need to clear anything since we did not make any temporary lists.
			}

			return true;
		}

		private static bool ValidSkills( SkillNameValue[] skills )
		{
			int total = 0;

			for ( int i = 0; i < skills.Length; ++i )
			{
				if ( skills[i].Value < 0 || skills[i].Value > 50 )
					return false;

				total += skills[i].Value;

				for ( int j = i + 1; j < skills.Length; ++j )
				{
					if ( skills[j].Value > 0 && skills[j].Name == skills[i].Name )
						return false;
				}
			}

			return ( total == 100 || total == 120 );
		}

		private static Mobile m_Mobile;

		private static SkillNameValue[] SetSkills( Mobile m, SkillNameValue[] skills, int prof )
		{
			switch ( prof )
			{
				case 6: // Mage
				{
					m.InitStats( 35, 10, 45 ); // 90
					skills = new SkillNameValue[]
						{
							new SkillNameValue( SkillName.Magery, 30 ),
							new SkillNameValue( SkillName.Psychology, 30 ),
							new SkillNameValue( SkillName.Mercantile, 30 ),
							new SkillNameValue( SkillName.FistFighting, 30 )
						};

					break;
				}
				case 7:	// Archer
				{
					m.InitStats( 35, 40, 15 ); // 90
					skills = new SkillNameValue[]
						{
							new SkillNameValue( SkillName.Marksmanship, 30 ),
							new SkillNameValue( SkillName.Tactics, 30 ),
							new SkillNameValue( SkillName.Bowcraft, 30 ),
							new SkillNameValue( SkillName.Lumberjacking, 30 )
						};
					break;
				}
				case 5:	// Warrior
				{
					m.InitStats( 50, 30, 10 ); // 90
					skills = new SkillNameValue[]
						{
							new SkillNameValue( SkillName.Swords, 30 ),
							new SkillNameValue( SkillName.Tactics, 30 ),
							new SkillNameValue( SkillName.Parry, 30 ),
							new SkillNameValue( SkillName.Healing, 30 )
						};
					break;
				}
				case 4: // Knight
				{
					m.InitStats( 50, 25, 15 ); // 90
					skills = new SkillNameValue[]
						{
							new SkillNameValue( SkillName.Knightship, 30 ),
							new SkillNameValue( SkillName.Tactics, 30 ),
							new SkillNameValue( SkillName.Healing, 30 ),
							new SkillNameValue( SkillName.Swords, 30 )
						};

					break;
				}
				case 1: // Ninja
				{
					m.InitStats( 40, 30, 20 ); // 90
					skills = new SkillNameValue[]
						{
							new SkillNameValue( SkillName.Ninjitsu, 30 ),
							new SkillNameValue( SkillName.Hiding, 30 ),
							new SkillNameValue( SkillName.Stealth, 30 ),
							new SkillNameValue( SkillName.Fencing, 30 )
						};

					break;
				}
				case 2: // Bard
				{
					m.InitStats( 40, 30, 20 ); // 90
					skills = new SkillNameValue[]
						{
							new SkillNameValue( SkillName.Musicianship, 30 ),
							new SkillNameValue( SkillName.Peacemaking, 30 ),
							new SkillNameValue( SkillName.Discordance, 30 ),
							new SkillNameValue( SkillName.Provocation, 30 )
						};

					break;
				}
				case 3: // Druid
				{
					m.InitStats( 30, 20, 40 ); // 90
					skills = new SkillNameValue[]
						{
							new SkillNameValue( SkillName.Druidism, 30 ),
							new SkillNameValue( SkillName.Taming, 30 ),
							new SkillNameValue( SkillName.Veterinary, 30 ),
							new SkillNameValue( SkillName.Herding, 30 )
						};

					break;
				}
				default:
				{
					if ( !ValidSkills( skills ) )
						return new SkillNameValue[] { };

					break;
				}
			}

			for ( int i = 0; i < skills.Length; ++i )
			{
				SkillNameValue snv = skills[i];

				if ( snv.Value > 0 && ( snv.Name != SkillName.Stealth || prof == 1 ) && snv.Name != SkillName.RemoveTrap && snv.Name != SkillName.Elementalism )
				{
					Skill skill = m.Skills[snv.Name];

					if ( skill != null )
					{
						skill.BaseFixedPoint = snv.Value * 10;
					}
				}
			}

			return skills;
		}

		private static void PackItem(Container pack, int count, Func<Item> itemFactory, bool asUnidentified = false)
		{
			for (var i = 0; i < count; i++)
			{
				PackItem(pack, itemFactory(), asUnidentified);
			}
		}

		private static void PackItem(Container pack, Item item, bool asUnidentified = false)
		{
			if (pack != null)
			{
				if (asUnidentified)
					NotIdentified.AddAsUnidentified(item, pack, m_Mobile);
				else
					pack.DropItem(item);
			}
			else
				item.Delete();
		}

		private static readonly List<Layer> NonArmorLayers = new List<Layer>
		{
			Layer.Shoes,
			Layer.Pants,
			Layer.Shirt,
			Layer.Helm,
			Layer.Ring,
			Layer.Trinket,
			Layer.Neck,
			Layer.Waist,
			Layer.Bracelet,
			Layer.MiddleTorso,
			Layer.Earrings,
			Layer.Cloak,
			Layer.OuterTorso,
			Layer.OuterLegs,
		};

		private static Item GenerateSkillBonusItem(SkillName skill, int min, int max)
		{
			while (true)
			{
				var layer = Utility.Random(NonArmorLayers);
				var item = GenerateSkillBonusItem(layer, skill, min, max);
				if (item != null) return item;

				Console.WriteLine("Failed to generate item for layer '{0}'", layer);
			}
		}

		private static Item GenerateRandomItem(LootPackItem[] lootPack, bool isMagic)
		{
			var itemOptions = lootPack.ToList();

			while (true)
			{
				var item = Utility.Random(itemOptions).Construct(false);
				if (item != null)
				{
					if (isMagic)
						BaseRunicTool.ApplyAttributes(item, 1, 1, 5, 25);

					return item;
				}

				Console.WriteLine("Failed to generate loot item.");
			}
		}

		private static Item GenerateSkillBonusItem(Layer layer, SkillName skill, int min, int max)
		{
			Item item = null;
			switch (layer)
			{
				case Layer.Shoes:
					switch (Utility.RandomMinMax(1, 4))
					{
						case 1: item = new Sandals(); break;
						case 2: item = new Shoes(); break;
						case 3: item = new Boots(); break;
						case 4: item = new ThighBoots(); break;
					}
					break;

				case Layer.Pants:
					switch (Utility.RandomMinMax(1, 4))
					{
						case 1: item = new ShortPants(); break;
						case 2: item = new LongPants(); break;
						case 3: item = new PiratePants(); break;
						case 4: item = new TattsukeHakama(); break;
					}
					break;

				case Layer.Shirt:
					switch (Utility.RandomMinMax(1, 4))
					{
						case 1: item = new FancyShirt(); break;
						case 2: item = new SquireShirt(); break;
						case 3: item = new RoyalCoat(); break;
						case 4: item = new RusticVest(); break;
					}
					break;

				case Layer.Helm:
					item = new JewelryCirclet();
					break;

				case Layer.Ring:
					item = new JewelryRing();
					break;

				case Layer.Trinket:
					item = new TrinketTalisman();
					break;

				case Layer.Neck:
					item = new JewelryNecklace();
					break;

				case Layer.Waist:
					switch (Utility.RandomMinMax(1, 3))
					{
						case 1: item = new HalfApron(); break;
						case 2: item = new Obi(); break;
						case 3: item = new Belt(); break;
					}
					break;

				case Layer.Bracelet:
					item = new JewelryBracelet();
					break;

				case Layer.MiddleTorso:
					switch (Utility.RandomMinMax(1, 4))
					{
						case 1: item = new BodySash(); break;
						case 2: item = new Doublet(); break;
						case 3: item = new JesterSuit(); break;
						case 4: item = new Surcoat(); break;
					}
					break;

				case Layer.Earrings:
					item = new JewelryEarrings();
					break;

				case Layer.Cloak:
					switch (Utility.RandomMinMax(1, 2))
					{
						case 1: item = new Cloak(); break;
						case 2: item = new RoyalCape(); break;
					}
					break;

				case Layer.OuterTorso:
					switch (Utility.RandomMinMax(1, 4))
					{
						case 1: item = new ChaosRobe(); break;
						case 2: item = new GildedDress(); break;
						case 3: item = new Kamishimo(); break;
						case 4: item = new ScholarRobe(); break;
					}
					break;

				case Layer.OuterLegs:
					switch (Utility.RandomMinMax(1, 4))
					{
						case 1: item = new Skirt(); break;
						case 2: item = new Kilt(); break;
						case 3: item = new Hakama(); break;
						case 4: item = new RoyalSkirt(); break;
					}
					break;

				case Layer.TwoHanded:
					switch (Utility.RandomMinMax(1, 3))
					{
						case 1: item = new TrinketCandle(); break;
						case 2: item = new TrinketLantern(); break;
						case 3: item = new TrinketTorch(); break;
					}
					break;
			}

			int amount = min == max ? min : Utility.RandomMinMax(min, max);

			if (item is BaseWeapon) ((BaseWeapon)item).SkillBonuses.SetValues(0, skill, amount);
			else if (item is BaseArmor) ((BaseArmor)item).SkillBonuses.SetValues(0, skill, amount);
			else if (item is BaseTrinket) ((BaseTrinket)item).SkillBonuses.SetValues(0, skill, amount);
			// else if ( item is BaseQuiver ) ((BaseQuiver)item).SkillBonuses.SetValues(0, skill, amount);
			else if (item is BaseClothing) ((BaseClothing)item).SkillBonuses.SetValues(0, skill, amount);
			else if (item is BaseInstrument) ((BaseInstrument)item).SkillBonuses.SetValues(0, skill, amount);
			else if (item is Spellbook) ((Spellbook)item).SkillBonuses.SetValues(0, skill, amount);

			return item;
		}

		private static void AddSkillBasedItems(Mobile m, SkillNameValue[] skills)
		{
			for (int i = 0; i < skills.Length; i++)
			{
				var skill = skills[i];
				if (skill.Value == 0) continue;

				Bag bag = new Bag { Name = m.Skills[skill.Name].Name };
				m.Backpack.AddItem(bag);

				switch (skill.Name)
				{
					case SkillName.Alchemy:
						PackItem(bag, new MortarPestle());
						PackItem(bag, new Bottle { Amount = 15 });
						break;

					case SkillName.Anatomy:
						PackItem(bag, GenerateSkillBonusItem(skill.Name, 5, 5));
						PackItem(bag, new Bandage { Amount = 200 });
						break;

					case SkillName.Druidism: // Animal Lore
						PackItem(bag, GenerateSkillBonusItem(skill.Name, 5, 10));
						break;

					case SkillName.Mercantile:
						PackItem(bag, new Gold { Amount = 100 });
						break;

					case SkillName.ArmsLore:
						PackItem(bag, GenerateRandomItem(LootPack.MagicItemsMeager1, true), true);
						break;

					case SkillName.Parry:
						PackItem(bag, new Buckler());
						break;

					case SkillName.Begging:
						PackItem(bag, GenerateSkillBonusItem(skill.Name, 5, 10));
						break;

					case SkillName.Blacksmith:
						PackItem(bag, new SmithHammer());
						PackItem(bag, new IronIngot { Amount = 50 });
						break;

					case SkillName.Bowcraft:
						PackItem(bag, new FletcherTools());
						PackItem(bag, new Board { Amount = 50 });
						PackItem(bag, new Feather { Amount = 50 });
						break;

					case SkillName.Peacemaking:
						PackItem(bag, GenerateSkillBonusItem(skill.Name, 5, 5));
						break;

					case SkillName.Camping:
						PackItem(bag, new SmallTent());
						PackItem(bag, new Kindling { Amount = 10 });
						break;

					case SkillName.Carpentry:
						PackItem(bag, new CarpenterTools());
						PackItem(bag, new Board { Amount = 50 });
						break;

					case SkillName.Cartography:
						PackItem(bag, new MapmakersPen());
						PackItem(bag, new BlankScroll { Amount = 50 });
						break;

					case SkillName.Cooking:
						PackItem(bag, new CulinarySet());
						PackItem(bag, new RawBird { Amount = 3 });
						break;

					case SkillName.Searching:
						PackItem(bag, GenerateSkillBonusItem(skill.Name, 5, 5));
						break;

					case SkillName.Discordance:
						PackItem(bag, GenerateSkillBonusItem(skill.Name, 5, 5));
						break;

					case SkillName.Psychology:
						PackItem(bag, GenerateSkillBonusItem(skill.Name, 5, 10));
						break;

					case SkillName.Healing:
						PackItem(bag, new Scissors());
						PackItem(bag, new Bandage { Amount = 200 });
						break;

					case SkillName.Seafaring:
						PackItem(bag, new FishingPole());
						PackItem(bag, new Fish());
						PackItem(bag, new RawFishSteak(3));
						break;

					case SkillName.Forensics:
						PackItem(bag, new SkinningKnifeTool());
						PackItem(bag, GenerateSkillBonusItem(skill.Name, 5, 5));
						break;

					case SkillName.Herding:
						PackItem(bag, new ShepherdsCrook());
						PackItem(bag, new CagedSheep { Weight = 10 });
						break;

					case SkillName.Hiding:
						PackItem(bag, GenerateSkillBonusItem(skill.Name, 5, 10));
						break;

					case SkillName.Provocation:
						PackItem(bag, GenerateSkillBonusItem(skill.Name, 5, 5));
						break;

					case SkillName.Inscribe:
						PackItem(bag, new Monocle());
						PackItem(bag, new ScribesPen());
						PackItem(bag, new BlankScroll { Amount = 50 });
						break;

					case SkillName.Lockpicking:
						PackItem(bag, new Lockpick { Amount = 10 });
						PackItem(bag, new PickBoxDifficult() { Movable = true } );
						break;

					case SkillName.Magery:
						PackItem(bag, new Spellbook());
						PackItem(bag, new HealScroll());
						PackItem(bag, new MagicArrowScroll());

						// One 2nd circle
						switch (Utility.RandomMinMax(1, 8))
						{
							case 1: PackItem(bag, new AgilityScroll()); break;
							case 2: PackItem(bag, new CunningScroll()); break;
							case 3: PackItem(bag, new CureScroll()); break;
							case 4: PackItem(bag, new HarmScroll()); break;
							case 5: PackItem(bag, new MagicTrapScroll()); break;
							case 6: PackItem(bag, new MagicUnTrapScroll()); break;
							case 7: PackItem(bag, new ProtectionScroll()); break;
							case 8: PackItem(bag, new StrengthScroll()); break;
						}

						var mageBag = new BagOfReagents();
						mageBag.Open(m);
						PackItem(bag, mageBag);
						break;

					case SkillName.MagicResist:
						PackItem(bag, GenerateSkillBonusItem(skill.Name, 5, 5));
						break;

					case SkillName.Tactics:
						PackItem(bag, GenerateSkillBonusItem(skill.Name, 5, 10));
						break;

					case SkillName.Snooping:
						PackItem(bag, GenerateSkillBonusItem(skill.Name, 5, 10));
						break;

					case SkillName.Musicianship:
						PackItem(bag, 3, () => GenerateRandomItem(LootPack.Instruments, false));
						break;

					case SkillName.Poisoning:
						PackItem(bag, new LesserPoisonPotion { Amount = 5 });
						break;

					case SkillName.Marksmanship:
						PackItem(bag, new Bow());
						PackItem(bag, new RepeatingCrossbow());
						PackItem(bag, new Arrow { Amount = 50 });
						PackItem(bag, new Bolt { Amount = 50 });
						break;

					case SkillName.Spiritualism:
						PackItem(bag, GenerateSkillBonusItem(skill.Name, 5, 10));
						break;

					case SkillName.Stealing:
						PackItem(bag, GenerateSkillBonusItem(skill.Name, 5, 5));
						break;

					case SkillName.Tailoring:
						PackItem(bag, new Scissors());
						PackItem(bag, new SewingKit());
						PackItem(bag, new Fabric { Amount = 50 });
						break;

					case SkillName.Taming:
						PackItem(bag, GenerateSkillBonusItem(skill.Name, 5, 5));
						switch (Utility.RandomMinMax(1, 4))
						{
							case 1: PackItem(bag, new CagedBlackBear { Weight = 10 }); break;
							case 2: PackItem(bag, new CagedPanther { Weight = 10 }); break;
							case 3: PackItem(bag, new CagedTimberWolf { Weight = 10 }); break;
							case 4: PackItem(bag, new CagedAlligator { Weight = 10 }); break;
						}
						break;

					case SkillName.Tasting:
						PackItem(bag, GenerateSkillBonusItem(skill.Name, 5, 10));
						PackItem(bag, 3, () => GenerateRandomItem(LootPack.LowPotionItems, false), true);
						break;

					case SkillName.Tinkering:
						PackItem(bag, new TinkerTools());
						PackItem(bag, new IronIngot { Amount = 50 });
						break;

					case SkillName.Tracking:
						PackItem(bag, GenerateSkillBonusItem(skill.Name, 5, 10));
						break;

					case SkillName.Veterinary:
						PackItem(bag, new Scissors());
						PackItem(bag, new Bandage { Amount = 200 });
						break;

					case SkillName.Swords:
						// Random fast weapon
						switch (Utility.RandomMinMax(1, 3))
						{
							case 1: PackItem(bag, new Bokuto()); break;
							case 2: PackItem(bag, new Cleaver()); break;
							case 3: PackItem(bag, new Cutlass()); break;
						}
						break;

					case SkillName.Bludgeoning:
						// Random fast weapon
						switch (Utility.RandomMinMax(1, 4))
						{
							case 1: PackItem(bag, new Tessen()); break;
							case 2: PackItem(bag, new Club()); break;
							case 3: PackItem(bag, new WildStaff()); break;
							case 4: PackItem(bag, new Mace()); break;
						}
						break;

					case SkillName.Fencing:
						// Random fast weapon
						switch (Utility.RandomMinMax(1, 4))
						{
							case 1: PackItem(bag, new Dagger()); break;
							case 2: PackItem(bag, new Kryss()); break;
							case 3: PackItem(bag, new AssassinSpike()); break;
							case 4: PackItem(bag, new Sai()); break;
						}
						break;

					case SkillName.FistFighting:
						PackItem(bag, new PugilistGloves());
						break;

					case SkillName.Lumberjacking:
						PackItem(bag, new Hatchet());
						PackItem(bag, new Hatchet());
						break;

					case SkillName.Mining:
						PackItem(bag, new Spade());
						PackItem(bag, new Spade());
						break;

					case SkillName.Meditation:
						PackItem(bag, GenerateSkillBonusItem(skill.Name, 5, 10));
						break;

					case SkillName.RemoveTrap:
						PackItem(bag, new TenFootPole() { Weight = 20 });
						break;

					case SkillName.Necromancy:
						PackItem(bag, new NecromancerSpellbook());
						PackItem(bag, new PainSpikeScroll());
						PackItem(bag, new CurseWeaponScroll());
						var necroBag = new BagOfNecroReagents();
						necroBag.Open(m);
						PackItem(bag, necroBag);
						break;

					case SkillName.Focus:
						PackItem(bag, GenerateSkillBonusItem(skill.Name, 5, 10));
						break;

					case SkillName.Knightship:
						PackItem(bag, new BookOfChivalry());
						break;

					case SkillName.Bushido:
						PackItem(bag, new BookOfBushido());
						break;

					case SkillName.Ninjitsu:
						PackItem(bag, new BookOfNinjitsu());
						break;

					case SkillName.Stealth:
						PackItem(bag, GenerateSkillBonusItem(skill.Name, 5, 10));
						break;

					case SkillName.Elementalism:
					case SkillName.Mysticism:
					case SkillName.Imbuing:
					case SkillName.Throwing:
						// Not pickable
						break;
				}
			}
		}
    }
}
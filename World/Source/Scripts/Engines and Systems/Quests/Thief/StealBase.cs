using System;
using Server.Network;
using Server.Misc;
using System.Linq;
using Server.Engines.PuzzleChest;
using Server.Mobiles;

namespace Server.Items
{
	public class StealBase : BaseAddon
	{
		private PuzzleChest m_PuzzleChest;

		public int BoxType;
		public int BoxColor;
		public int PedType;
		public string BoxOrigin;
		public string BoxCarving;

		[CommandProperty(AccessLevel.Owner)]
		public int Box_Type { get { return BoxColor; } set { BoxColor = value; InvalidateProperties(); } }

		[CommandProperty(AccessLevel.Owner)]
		public int Box_Color { get { return BoxType; } set { BoxType = value; InvalidateProperties(); } }

		[CommandProperty(AccessLevel.Owner)]
		public int Ped_Type { get { return PedType; } set { PedType = value; InvalidateProperties(); } }

		[CommandProperty(AccessLevel.Owner)]
		public string Box_Origin { get { return BoxOrigin; } set { BoxOrigin = value; InvalidateProperties(); } }

		[CommandProperty(AccessLevel.Owner)]
		public string Box_Carving { get { return BoxCarving; } set { BoxCarving = value; InvalidateProperties(); } }

		public int m_Tries;
		public int Tries{ get{ return m_Tries; } set{ m_Tries = value; } }

		[ Constructable ]
		public StealBase()
		{
			int iZ = 0;
			int iZ1 = 0;
			int iZ2 = 0;

			if ( Utility.RandomMinMax( 1, 3 ) > 1 )
			{
				iZ1 = 5;
				iZ2 = 6;
				PedType = 13042;
			}
			else
			{
				iZ1 = 10;
				iZ2 = 10;
				PedType = 0x1223;
			}

				string sEtch = "etched";
				string sPed = "an ornately ";
				switch( Utility.RandomMinMax( 0, 10 ) )
				{
					case 0: sPed = "an ornately ";		break;
					case 1: sPed = "a beautifully ";	break;
					case 2: sPed = "an expertly ";		break;
					case 3: sPed = "an artistically ";	break;
					case 4: sPed = "an exquisitely ";	break;
					case 5: sPed = "a decoratively ";	break;
					case 6: sPed = "an ancient ";		break;
					case 7: sPed = "an old ";			break;
					case 8: sPed = "an unusually ";		break;
					case 9: sPed = "a curiously ";		break;
					case 10: sPed = "an oddly ";		break;
				}
				sPed = sPed + "carved pedestal";

				int iColor = 0;
				int iThing = 0x9A8;
				string sArty = "a strange";
				switch( Utility.RandomMinMax( 0, 6 ) )
				{
					case 0: sArty = "an odd ";		break;
					case 1: sArty = "an unusual ";	break;
					case 2: sArty = "a bizarre ";	break;
					case 3: sArty = "a curious ";	break;
					case 4: sArty = "a peculiar ";	break;
					case 5: sArty = "a strange ";	break;
					case 6: sArty = "a weird ";		break;
				}

				string sThing = "metal box";
				switch( Utility.RandomMinMax( 0, 6 ) )
				{
					case 0: iThing = 0x9AA; sThing = "metal box"; break;
					case 1: iThing = 0xE7D; sThing = "metal box"; break;
					case 2: iThing = 0x9AA; sThing = "wooden box"; break;
					case 3: iThing = 0xE7D; sThing = "wooden box"; break;
					case 4: iThing = 0xE76; sThing = "bag"; break;
					case 5: iThing = 0xE76; sThing = "sack"; break;
					case 6: iThing = 0xE76; sThing = "pouch"; break;
				}

				if ( sThing == "metal box")
				{
					BoxType = 1;
					iZ = iZ1;
					sEtch = "etched";

					Item temp = new PlateHelm();
					ResourceMods.SetRandomResource( false, false, temp, CraftResource.Iron, true, null );
					iColor = CraftResources.GetHue(temp.Resource);
					sThing = CraftResources.GetName(temp.Resource) + " box";
					temp.Delete();
				}
				else if ( sThing == "wooden box")
				{
					BoxType = 2;
					iZ = iZ1;
					sEtch = "carved";

					Item temp = new PlateHelm();
					ResourceMods.SetRandomResource( false, false, temp, CraftResource.RegularWood, true, null );
					iColor = CraftResources.GetHue(temp.Resource);
					sThing = CraftResources.GetName(temp.Resource) + " box";
					temp.Delete();
				}
				else
				{
					BoxType = 3;
					iZ = iZ2;
					sEtch = "etched";

					Item temp = new PlateHelm();
					ResourceMods.SetRandomResource( false, false, temp, CraftResource.RegularLeather, true, null );
					iColor = CraftResources.GetHue(temp.Resource);
					sThing = CraftResources.GetName(temp.Resource) + " " + sThing;
					temp.Delete();
				}
				sThing = sArty + sThing;

			AddComplexComponent( (BaseAddon) this, iThing, 0, 0, iZ, iColor, -1, sThing, 1);
			AddComplexComponent( (BaseAddon) this, 5703, 0, 0, 0, 0, 29, sPed, 1);
			AddComplexComponent( (BaseAddon) this, PedType, 0, 0, 0, 0, -1, "", 1);

			BoxOrigin = sThing;
			BoxColor = iColor;

			///// DO THE CARVINGS ON THE BAG OR BOX ///////////////////////////////////////////////////////////
			string sLanguage = "pixie";
			switch( Utility.RandomMinMax( 0, 28 ) )
			{
				case 0: sLanguage = "balron"; break;
				case 1: sLanguage = "pixie"; break;
				case 2: sLanguage = "centaur"; break;
				case 3: sLanguage = "demonic"; break;
				case 4: sLanguage = "dragon"; break;
				case 5: sLanguage = "dwarvish"; break;
				case 6: sLanguage = "elven"; break;
				case 7: sLanguage = "fey"; break;
				case 8: sLanguage = "gargoyle"; break;
				case 9: sLanguage = "cyclops"; break;
				case 10: sLanguage = "gnoll"; break;
				case 11: sLanguage = "goblin"; break;
				case 12: sLanguage = "gremlin"; break;
				case 13: sLanguage = "druidic"; break;
				case 14: sLanguage = "tritun"; break;
				case 15: sLanguage = "minotaur"; break;
				case 16: sLanguage = "naga"; break;
				case 17: sLanguage = "ogrish"; break;
				case 18: sLanguage = "orkish"; break;
				case 19: sLanguage = "sphinx"; break;
				case 20: sLanguage = "treekin"; break;
				case 21: sLanguage = "trollish"; break;
				case 22: sLanguage = "undead"; break;
				case 23: sLanguage = "vampire"; break;
				case 24: sLanguage = "dark elf"; break;
				case 25: sLanguage = "magic"; break;
				case 26: sLanguage = "human"; break;
				case 27: sLanguage = "symbolic"; break;
				case 28: sLanguage = "runic"; break;
			}

			string sPart = "strange ";
			switch( Utility.RandomMinMax( 0, 5 ) )
			{
				case 0:	sPart = "strange ";	break;
				case 1:	sPart = "odd ";		break;
				case 2:	sPart = "ancient ";	break;
				case 3:	sPart = "long dead ";	break;
				case 4:	sPart = "cryptic ";	break;
				case 5:	sPart = "mystical ";	break;
			}

			string sPart2 = " symbols ";
			switch( Utility.RandomMinMax( 0, 5 ) )
			{
				case 0:	sPart2 = " symbols ";	break;
				case 1:	sPart2 = " words ";		break;
				case 2:	sPart2 = " writings ";	break;
				case 3:	sPart2 = " glyphs ";	break;
				case 4:	sPart2 = " pictures ";	break;
				case 5:	sPart2 = " runes ";		break;
			}

			BoxCarving = "with " + sPart + sLanguage + sPart2 + sEtch + " on it";
		}

		public StealBase( Serial serial ) : base( serial )
		{
		}

        private static void AddComplexComponent(BaseAddon addon, int item, int xoffset, int yoffset, int zoffset, int hue, int lightsource)
        {
            AddComplexComponent(addon, item, xoffset, yoffset, zoffset, hue, lightsource, null, 1);
        }

        private static void AddComplexComponent(BaseAddon addon, int item, int xoffset, int yoffset, int zoffset, int hue, int lightsource, string name, int amount)
        {
            AddonComponent ac;
            ac = new AddonComponent(item);
            if (name != null && name.Length > 0)
                ac.Name = name;
            if (hue != 0)
                ac.Hue = hue;
            if (amount > 1)
            {
                ac.Stackable = true;
                ac.Amount = amount;
            }
            if (lightsource != -1)
                ac.Light = (LightType) lightsource;
            addon.AddComponent(ac, xoffset, yoffset, zoffset);
        }

		public override void OnComponentUsed( AddonComponent ac, Mobile from )
		{
			var player = from as PlayerMobile;
			if ( player == null ) return;

			if ( from.Blessed )
			{
				from.SendMessage( "You cannot open that while in this state." );
				return;
			}

			if ( !from.InRange( GetWorldLocation(), 2 ) )
			{
				from.SendMessage( "You will have to get closer to try and steal the item." );
				return;
			}
			
			MuseumBook.FoundItem( from, 2 );
			QuestTome.FoundItem( from, 2, null );

			var envelope = from.Backpack.FindItemByType( typeof ( ThiefNote ) ) as ThiefNote;

			if ( player.Temptations.CanUsePuzzleboxes && envelope == null )
			{
				if ( m_PuzzleChest == null )
					m_PuzzleChest = new PuzzleChest(this);

				m_PuzzleChest.ShowGump(from);
			}
			else if ( m_Tries > 5 )
			{
				Item Pedul = new StealBaseEmpty();
				Pedul.ItemID = PedType;
				Pedul.MoveToWorld (new Point3D(this.X, this.Y, this.Z), this.Map);
				from.SendMessage( "Your fingers were not nimble enough and your prize has vanished!" );
				this.Delete();
			}
			else if ( !from.CheckSkill( SkillName.Snooping, 0, 125 ) )
			{
				m_Tries++;
				if ( from.CheckSkill( SkillName.RemoveTrap, 0, 125 ) )
				{
					from.SendMessage( "You pull back just in time to avoid a trap!" );
				}
				else
				{
					DoDamage( from );
				}
			}
			else if ( from.CheckSkill( SkillName.Stealing, 0, 125 ) )
			{
				m_Tries++;

				if ( envelope != null && envelope.NoteOwner == from )
				{

					if ( envelope.NoteItemArea == Server.Misc.Worlds.GetRegionName( from.Map, from.Location ) && envelope.NoteItemGot == 0 )
					{
						envelope.NoteItemGot = 1;
						from.LocalOverheadMessage(MessageType.Emote, 1150, true, "You found " + envelope.NoteItem + ".");
						from.SendSound( 0x3D );
						from.CloseGump( typeof( Server.Items.ThiefNote.NoteGump ) );
						envelope.InvalidateProperties();
						return;
					}
				}

				Take(from);
				from.SendMessage( "Your nimble fingers manage to steal the item." );
				LoggingFunctions.LogStandard( from, "has stolen an item from a pedestal." );
			}
			else
			{
				m_Tries++;
				from.SendMessage( "You fail to steal the item." );
			}
		}

		public static void DoDamage(Mobile to)
		{
			switch (Utility.Random(5))
			{
				case 0:
					{
						// from.FixedParticles( 0x374A, 10, 15, 5021, EffectLayer.Waist );
						to.PlaySound(0x231); // vapor03
						Effects.SendLocationEffect(to, to.Map, 0x113A, 20, 10);
						to.LocalOverheadMessage(MessageType.Regular, 0x44, 1010523); // A toxic vapor envelops thee.

						int nPoison = Utility.RandomMinMax( 0, 10 );
							if ( nPoison > 9 ) { to.ApplyPoison( to, Poison.Deadly ); }
							else if ( nPoison > 7 ) { to.ApplyPoison( to, Poison.Greater ); }
							else if ( nPoison > 4 ) { to.ApplyPoison( to, Poison.Regular ); }
							else { to.ApplyPoison( to, Poison.Lesser ); }

						to.SendMessage( "You accidentally trigger a poison trap!" );
						LoggingFunctions.LogTraps( to, "a pedestal poison trap", false );
						break;
					}
				case 1:
					{
						to.PlaySound( 0x307 ); // exp03
						to.FixedParticles( 0x36BD, 20, 10, 5044, EffectLayer.Head );
						to.LocalOverheadMessage(MessageType.Regular, 0xEE, 1010524); // Searing heat scorches thy skin.

						Spells.SpellHelper.Damage( TimeSpan.FromSeconds( 0.5 ), to, to, Utility.RandomMinMax( 10, 80 ), 0, 100, 0, 0, 0 );
						to.SendMessage( "You accidentally trigger an explosion trap!" );
						LoggingFunctions.LogTraps( to, "a pedestal explosion trap", false );
						break;
					}
				case 2:
					{
						to.PlaySound(0x223);
						to.LocalOverheadMessage(MessageType.Regular, 0x62, 1010525); // Pain lances through thee from a sharp metal blade.

						AOS.Damage(to, to, Utility.RandomMinMax(10, 80), 100, 0, 0, 0, 0);
						to.SendMessage( "You accidentally trigger a blade trap!" );
						LoggingFunctions.LogTraps( to, "a pedestal blade trap", false );
						break;
					}
				case 3:
					{
						to.FixedParticles( 0x3709, 10, 30, 5052, EffectLayer.LeftFoot );
						to.PlaySound( 0x208 ); // flamstrk
						to.LocalOverheadMessage(MessageType.Regular, 0xEE, 1010524); // Searing heat scorches thy skin.

						Spells.SpellHelper.Damage( TimeSpan.FromSeconds( 0.5 ), to, to, Utility.RandomMinMax( 10, 80 ), 0, 100, 0, 0, 0 );
						to.SendMessage( "You accidentally trigger a flame trap!" );
						LoggingFunctions.LogTraps( to, "a pedestal fire trap", false );
						break;
					}
				default:
					{
						to.BoltEffect(0);
						to.LocalOverheadMessage(MessageType.Regular, 0xDA, 1010526); // Lightning arcs through thy body.

						AOS.Damage(to, to, Utility.RandomMinMax(10, 80), 0, 0, 0, 0, 100);
						to.SendMessage( "You accidentally trigger an electrical trap!" );
						LoggingFunctions.LogTraps( to, "a pedestal electrical trap", false );

						break;
					}
			}
		}

		public void Take(Mobile from)
		{
			bool good = false;

			if ( from.StolenBoxTime > 32 )
				from.StolenBoxTime = 0;

			if ( from.StolenBoxTime < DateTime.Now.Day )
			{
				good = true;
				from.StolenBoxTime = DateTime.Now.Day + 2;
			}

			if ( !MySettings.S_PedStealThrottle )
				good = true;

			if ( BoxType == 1 )
			{
				Item Bags = new StealMetalBox();
				StealMetalBox bag = (StealMetalBox)Bags;
				bag.BoxColor = BoxColor;
				bag.Hue = BoxColor;
				bag.Name = BoxOrigin;
				bag.BoxName = BoxOrigin;
				bag.BoxMarkings = BoxCarving;
				FillMeUp( bag, from, good );
				from.AddToBackpack( bag );
				if ( !good )
					bag.Weight = 10.0;
			}
			else if ( BoxType == 2 )
			{
				Item Bags = new StealBox();
				StealBox bag = (StealBox)Bags;
				bag.BoxColor = BoxColor;
				bag.Hue = BoxColor;
				bag.Name = BoxOrigin;
				bag.BoxName = BoxOrigin;
				bag.BoxMarkings = BoxCarving;
				FillMeUp( bag, from, good );
				from.AddToBackpack( bag );
			}
			else
			{
				Item Bags = new StealBag();
				StealBag bag = (StealBag)Bags;
				bag.BagColor = BoxColor;
				bag.Hue = BoxColor;
				bag.Name = BoxOrigin;
				bag.BagName = BoxOrigin;
				bag.BagMarkings = BoxCarving;
				FillMeUp( bag, from, good );
				from.AddToBackpack( bag );

			}

			Item Pedul = new StealBaseEmpty();
			Pedul.ItemID = PedType;
			Pedul.MoveToWorld (new Point3D(X, Y, Z), Map);

			Delete();
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( 0 ); // Version
            writer.Write( BoxType );
            writer.Write( BoxColor );
            writer.Write( PedType );
            writer.Write( BoxOrigin );
            writer.Write( BoxCarving );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
            BoxType = reader.ReadInt();
            BoxColor = reader.ReadInt();
            PedType = reader.ReadInt();
            BoxOrigin = reader.ReadString();
            BoxCarving = reader.ReadString();
		}

		private void GenerateGoodLoot( Container box, Mobile from, bool good )
		{
			int goodLootRandom = good ? Utility.RandomMinMax(1, 10) : 10;
			switch( goodLootRandom )
			{
				case 1:
				{
					box.DropItem( Loot.RandomArty() );
					break;
				}

				case 2:
				case 3:
				case 4:
				{
					box.DropItem( Loot.RandomSArty( Server.LootPackEntry.playOrient( from ), from ) );
					break;
				}

				case 5:
				case 6:
				{
					Item item =  Utility.RandomMinMax(1, 4) == 1
						? Loot.RandomInstrument()
						: Loot.RandomMagicalItem( Server.LootPackEntry.playOrient( from ) );
					box.DropItem( LootPackEntry.Enchant( from, 500, item ) );
					break;
				}

				case 7:
				case 8:
				case 9:
				case 10:
				{
					int goldFromLuck = Math.Max(from.Luck, 4000);
					int randomGold = good
						? Utility.RandomMinMax( 4000, 16000 )
						: Utility.RandomMinMax( 400, 1600 );

					randomGold = (int)(randomGold * (MyServerSettings.GetGoldCutRate() * .01));

					box.DropItem( new Gold(  goldFromLuck + randomGold  ) );
					LootPackChange.MakeCoins( box, from );
					break;
				}
			}
		}

		public void FillMeUp( Container box, Mobile from, bool good )
		{
			int calculatedLuck = 20 + (from.Luck / 2);

			if ( GetPlayerInfo.LuckyPlayer( calculatedLuck ) )
				box.DropItem( Loot.RandomRelic( from ) );

			if ( GetPlayerInfo.LuckyPlayer( calculatedLuck ) )
				box.DropItem( Loot.RandomRare( Utility.RandomMinMax(6,12), from ) );

			if ( GetPlayerInfo.LuckyPlayer( calculatedLuck ) )
				box.DropItem( Loot.RandomBooks( Utility.RandomMinMax(6,12) ) );

			if ( GetPlayerInfo.LuckyPlayer( calculatedLuck ) )
				box.DropItem( Loot.RandomScroll( Utility.Random(12)+1 ) );
			
			if ( GetPlayerInfo.LuckyPlayer( calculatedLuck ) )
			{
				Item item = Loot.RandomMagicalItem( Server.LootPackEntry.playOrient( from ) );
				item = LootPackEntry.Enchant( from, 300, item );
				box.DropItem(item);
			}

			if ( GetPlayerInfo.LuckyPlayer( calculatedLuck ) )
			{
				int gems = Utility.RandomMinMax( 4, 12 );
				for ( int i = 0; i < gems; i++ )
					box.DropItem( Loot.RandomGem() );
			}

			if ( GetPlayerInfo.LuckyPlayer( calculatedLuck ) )
				box.DropItem( Loot.RandomPotion( Utility.RandomMinMax(6,12), true ) );

			if ( GetPlayerInfo.LuckyPlayer( calculatedLuck ) )
				box.DropItem( new MagicalWand(0) );

			foreach ( Item item in box.Items.ToList() )
				Server.Items.NotIdentified.ConfigureItem( item, box, from );
				
			Server.Items.NotIdentified.DoAutoDelete( box, from );

			// Never unidentified. Never auto-deleted.
			GenerateGoodLoot( box, from, good );
		}
	}
}
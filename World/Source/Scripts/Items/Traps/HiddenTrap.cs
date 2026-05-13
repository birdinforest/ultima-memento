using System;
using Server;
using Server.Network;
using Server.Mobiles;
using Server.Items;
using Server.Spells;
using System.Collections.Generic;
using Server.Misc;
using Server.Localization;
using System.Collections;
using System.Text;
using System.IO;
using Server.Regions;
using Server.Targeting;

namespace Server.Items
{
	public class HiddenTrap : Item
	{
		public override bool DisplayWeight { get { return false; } }

		public int HiddenTrapType;

		private Dictionary<Serial, DateTime> m_WarnedPlayers;

		private static readonly int[] WeightedTrapPool = BuildWeightedPool();

		private static int[] BuildWeightedPool()
		{
			var pool = new List<int>();
			// Mild (x5): types 1, 6, 12
			for (int i = 0; i < 5; i++) { pool.Add(1); pool.Add(6); pool.Add(12); }
			// Moderate (x3): types 2, 5, 7, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23
			for (int i = 0; i < 3; i++) { pool.Add(2); pool.Add(5); pool.Add(7); pool.Add(14); pool.Add(15); pool.Add(16); pool.Add(17); pool.Add(18); pool.Add(19); pool.Add(20); pool.Add(21); pool.Add(22); pool.Add(23); }
			// Severe (x1.5): types 3, 8, 9, 10, 11, 24
			for (int i = 0; i < 3; i += 2) { pool.Add(3); pool.Add(8); pool.Add(9); pool.Add(10); pool.Add(11); pool.Add(24); }
			// Extreme (x0.5): types 4, 13, 25
			pool.Add(4); pool.Add(13); pool.Add(25);
			return pool.ToArray();
		}

		[CommandProperty(AccessLevel.Owner)]
		public int Hidden_TrapType { get { return HiddenTrapType; } set { HiddenTrapType = value; InvalidateProperties(); } }

		[Constructable]
		public HiddenTrap() : base( 0x65F7 )
		{
			m_WarnedPlayers = new Dictionary<Serial, DateTime>();
			Movable = false;
			Name = StringCatalog.ResolveByKey(null, "trap.name.hidden");
			Visible = false;
			Weight = 1.0;
			Light = LightType.Circle150;

			// Weight Values:
			// 1.0 = Hidden and unverified
			// 2.0 = Hidden and active
			// 3.0 = Visible and active
			// 5.0 = Deactivated
			// 6.0 = Remove due to unverification
		}

        public override void OnAfterSpawn()
        {
			base.OnAfterSpawn();
			if ( Server.Misc.Worlds.IsOnSpaceship( Location, Map ) )
				HiddenTrapType = Utility.RandomList( 1, 2, 3, 4, 5, 6, 7, 8, 9, 11, 16, 18, 19, 20, 21, 22, 23 );
			else
				HiddenTrapType = WeightedTrapPool[Utility.Random(WeightedTrapPool.Length)];
			SetAppearance( this );
		}

		public static void SetAppearance( Item trap )
		{
			if ( trap.Weight >= 5.0 && Server.Misc.Worlds.IsOnSpaceship( trap.Location, trap.Map ) )
				trap.ItemID = 0x65F4;
			else if ( trap.Weight >= 5.0 )
				trap.ItemID = 0x65FB;
			else if ( Server.Misc.Worlds.IsOnSpaceship( trap.Location, trap.Map ) )
				trap.ItemID = 0x65F1;
			else
				trap.ItemID = 0x65F7;

			if ( trap.Weight == 5.0 )
				trap.Name = StringCatalog.ResolveByKey(null, "trap.name.disabled");
			else if ( trap.Weight == 6.0 )
				trap.Name = StringCatalog.ResolveByKey(null, "trap.name.broken");
			else if ( trap.Weight == 3.0 )
				trap.Name = StringCatalog.ResolveByKey(null, "trap.name.trap");
		}

		public HiddenTrap(Serial serial) : base(serial)
		{
			m_WarnedPlayers = new Dictionary<Serial, DateTime>();
		}

		public override bool OnMoveOver( Mobile m )
		{
			string sTrapType = "";

			string textSay = "";
			string textLog = "";

			if ( !SeeIfTrapActive( this ) )
				return true;

			if ( !CanSetOffTraps( m ) || Weight >= 5.0 )
				return true;

			bool HadAnyAffect = false;

			if ( m is PlayerMobile )
			{
				bool nSprung = CheckTrapAvoidance( m, this );

				if ( nSprung )
				{
					int nTrapType = HiddenTrapType;

					if ( nTrapType == 0 )
					{
						if ( Server.Misc.Worlds.IsOnSpaceship( m.Location, m.Map ) )
							nTrapType = Utility.RandomList( 1, 2, 3, 4, 5, 6, 7, 8, 9, 11, 16, 18, 19, 20, 21, 22, 23 );
						else
							nTrapType = WeightedTrapPool[Utility.Random(WeightedTrapPool.Length)];
					}

					if ( m is PlayerMobile && Spells.Research.ResearchAirWalk.UnderEffect( m ) )
					{
						Point3D air = new Point3D( ( m.X+1 ), ( m.Y+1 ), ( m.Z+5 ) );
						Effects.SendLocationParticles(EffectItem.Create(air, m.Map, EffectItem.DefaultDuration), 0x2007, 9, 32, Server.Misc.PlayerSettings.GetMySpellHue( true, m, 0 ), 0, 5022, 0);
						m.PlaySound( 0x014 );
					}
					else if ( nTrapType == 1 && SavingThrow( m, "Magic", true, this ) == false ) // REVEALING TRAP
					{
						textSay = StringCatalog.ResolveByKey( m.Account, "trap.msg.reveal" );
						textLog = "a revealing trap";
			
						if ( Server.Misc.Worlds.IsOnSpaceship( m.Location, m.Map ) )
						{
							textSay = StringCatalog.ResolveByKey( m.Account, "trap.msg.reveal.space" );
							textLog = "a statically charged tile";
						}

						if ( m.Hidden != false ){ m.LocalOverheadMessage(MessageType.Emote, 0x916, true, textSay); sTrapType = textLog; }
					}
					else if ( nTrapType == 2 && SavingThrow( m, "Agility", true, this ) == false ) // TRIP WIRE
					{
						int HowBad = Utility.RandomMinMax( 1, 5 );

						if ( HowBad == 1 )
						{
							textSay = StringCatalog.ResolveByKey( m.Account, "trap.msg.tripwire.backpack" );
							textLog = "a trip wire trap";
				
							if ( Server.Misc.Worlds.IsOnSpaceship( m.Location, m.Map ) )
							{
								textSay = StringCatalog.ResolveByKey( m.Account, "trap.msg.tripwire.backpack.space" );
								textLog = "a loose deck plate";
							}

							int nDrop = 0;

							List<Item> belongings = new List<Item>();
							foreach( Item i in m.Backpack.Items )
							{
								belongings.Add(i);
								nDrop = 1;
							}

							if ( nDrop > 0 )
							{
								Container box = new DroppedContainer();
								foreach ( Item stuff in belongings )
								{
									if ( stuff != null && stuff.LootType != LootType.Blessed )
										box.DropItem(stuff);
								}
								box.MoveToWorld( this.Location, this.Map );
								m.LocalOverheadMessage(MessageType.Emote, 0x916, true, textSay);
								m.PlaySound( m.Female ? 812 : 1086 );
								sTrapType = textLog;
							}
						}
						else
						{
							textSay = StringCatalog.ResolveByKey( m.Account, "trap.msg.tripwire.equip" );
							textLog = "a trip wire trap";
				
							if ( Server.Misc.Worlds.IsOnSpaceship( m.Location, m.Map ) )
							{
								textSay = StringCatalog.ResolveByKey( m.Account, "trap.msg.tripwire.equip.space" );
								textLog = "a loose deck plate";
							}

							Item iTripped = GetMyItem( m );

							if ( iTripped != null )
							{
								m.LocalOverheadMessage(MessageType.Emote, 0x916, true, textSay);
								m.PlaySound( m.Female ? 812 : 1086 );
								iTripped.MoveToWorld( this.Location, this.Map );
								sTrapType = textLog;
							}
						}
					}
					else if ( nTrapType == 3 && SavingThrow( m, "Magic", true, this ) == false ) // COINS TO LEAD TRAP
					{
						textSay = StringCatalog.ResolveByKey( m.Account, "trap.msg.coins" );
						textLog = "a transmutation trap";
			
						if ( Server.Misc.Worlds.IsOnSpaceship( m.Location, m.Map ) )
						{
							textSay = StringCatalog.ResolveByKey( m.Account, "trap.msg.coins.space" );
							textLog = "a molecular atomizer";
						}

						Container cont = m.Backpack;
						int nDull = 0;

					// Partial corruption — only half the coins are transmuted (the rest clatter to the floor)
					int m_gAmount = m.Backpack.GetAmount( typeof( Gold ) );
					int m_cAmount = m.Backpack.GetAmount( typeof( DDCopper ) );
					int m_sAmount = m.Backpack.GetAmount( typeof( DDSilver ) );
					int m_xAmount = m.Backpack.GetAmount( typeof( DDXormite ) );

					int m_gDestroy = ( m_gAmount + 1 ) / 2;
					int m_cDestroy = ( m_cAmount + 1 ) / 2;
					int m_sDestroy = ( m_sAmount + 1 ) / 2;
					int m_xDestroy = ( m_xAmount + 1 ) / 2;

					if ( m_gDestroy > 0 && cont.ConsumeTotal( typeof( Gold ), m_gDestroy ) )
					{
						m.AddToBackpack( new LeadCoin( m_gDestroy ) );
						nDull = 1;
					}
					if ( m_cDestroy > 0 && cont.ConsumeTotal( typeof( DDCopper ), m_cDestroy ) )
					{
						m.AddToBackpack( new LeadCoin( m_cDestroy ) );
						nDull = 1;
					}
					if ( m_sDestroy > 0 && cont.ConsumeTotal( typeof( DDSilver ), m_sDestroy ) )
					{
						m.AddToBackpack( new LeadCoin( m_sDestroy ) );
						nDull = 1;
					}
					if ( m_xDestroy > 0 && cont.ConsumeTotal( typeof( DDXormite ), m_xDestroy ) )
					{
						m.AddToBackpack( new LeadCoin( m_xDestroy ) );
						nDull = 1;
					}
						if ( nDull > 0 )
						{
							m.LocalOverheadMessage(MessageType.Emote, 0x916, true, textSay);
							m.FixedParticles( 0x374A, 10, 15, 5028, EffectLayer.Waist );
							m.PlaySound( 0x1E1 );
						}
						sTrapType = textLog;
					}
				else if ( nTrapType == 4 && SavingThrow( m, "Magic", true, this ) == false ) // LOSE ITEM TRAP
				{
					textLog = "a destructive trap";

					if ( Server.Misc.Worlds.IsOnSpaceship( m.Location, m.Map ) )
						textLog = "a molecular oxidizer";

					Item iRuined = GetMyItem( m );

					if ( iRuined != null )
					{
						if ( Mobile.InsuranceEnabled && CheckInsuranceOnTrap( iRuined, m ) )
						{
							textSay = StringCatalog.ResolveByKey( m.Account, Server.Misc.Worlds.IsOnSpaceship( m.Location, m.Map ) ? "trap.insurance.damage.space" : "trap.insurance.damage" );
							m.LocalOverheadMessage(MessageType.Emote, 1150, true, textSay);
						}
						else
						{
							BaseWeapon tgtWeapon = iRuined as BaseWeapon;
							BaseArmor tgtArmor = iRuined as BaseArmor;
							bool isMetal = false;
							bool alreadyDamaged = false;

							if ( tgtWeapon != null )
							{
								isMetal = CraftResources.GetType( iRuined.Resource ) == CraftResourceType.Metal;
								alreadyDamaged = tgtWeapon.TrapDamaged;
							}
							else if ( tgtArmor != null )
							{
								isMetal = CraftResources.GetType( iRuined.Resource ) == CraftResourceType.Metal;
								alreadyDamaged = tgtArmor.TrapDamaged;
							}

							if ( alreadyDamaged )
							{
								// Stage 2: already damaged — destroy
								textSay = StringCatalog.ResolveByKey( m.Account, "trap.msg.damage.stage2" );
								m.LocalOverheadMessage(MessageType.Emote, 0x916, true, textSay);

								if ( isMetal )
								{
									RustyJunk broke = new RustyJunk();
									broke.ItemID = ( tgtWeapon != null ) ? tgtWeapon.GraphicID : tgtArmor.ItemID;
									broke.Name = StringCatalog.ResolveByKey( m.Account, "trap.name.rusted" );
									broke.Weight = ( tgtWeapon != null ) ? iRuined.Weight : (double)Utility.RandomMinMax( 1, 4 );
									m.AddToBackpack( broke );
								}
								else
								{
									BrokenGear broke = new BrokenGear();
									broke.ItemID = iRuined.ItemID;
									broke.Name = StringCatalog.ResolveByKey( m.Account, "trap.name.ruined" );
									broke.Weight = iRuined.Weight;
									m.AddToBackpack( broke );
								}
								iRuined.Delete();
							}
							else if ( tgtWeapon != null || tgtArmor != null )
							{
								// Stage 1: first hit — apply TrapDamaged
								int newMax = 5;
								if ( tgtWeapon != null )
								{
									tgtWeapon.TrapDamaged = true;
									newMax = Math.Max( Math.Min( 5, tgtWeapon.MaxHitPoints ), tgtWeapon.MaxHitPoints * 15 / 100 );
									tgtWeapon.MaxHitPoints = newMax;
									tgtWeapon.HitPoints = newMax;
								}
								else
								{
									tgtArmor.TrapDamaged = true;
									newMax = Math.Max( Math.Min( 5, tgtArmor.MaxHitPoints ), tgtArmor.MaxHitPoints * 15 / 100 );
									tgtArmor.MaxHitPoints = newMax;
									tgtArmor.HitPoints = newMax;
								}

								// Visual damage
								string dp = StringCatalog.ResolveByKey( null, "trap.name.damaged.prefix" );
								iRuined.Name = dp + iRuined.Name;
								iRuined.Hue = 0x0966;

								textSay = StringCatalog.ResolveByKey( m.Account, "trap.msg.damage.stage1" );
								m.LocalOverheadMessage(MessageType.Emote, 0x916, true, textSay);
							}
							else
							{
								// Not weapon/armor — immediate destruction
								textSay = StringCatalog.ResolveByKey( m.Account, "trap.msg.damage.ruin" );
								m.LocalOverheadMessage(MessageType.Emote, 0x916, true, textSay);

								BrokenGear broke = new BrokenGear();
								broke.ItemID = iRuined.ItemID;
								broke.Name = StringCatalog.ResolveByKey( m.Account, "trap.name.ruined" );
								broke.Weight = iRuined.Weight;
								m.AddToBackpack( broke );
								iRuined.Delete();
							}
						}
						m.FixedParticles( 0x374A, 10, 15, 5028, EffectLayer.Waist );
						m.PlaySound( 0x1E1 );
						sTrapType = textLog;
					}
				}
					else if ( nTrapType == 5 && SavingThrow( m, "Magic", true, this ) == false ) // LOSE A STAT TRAP
					{
						int nStat = Utility.RandomMinMax( 1, 3 );

						if ( nStat == 1 )
						{
							if ( m.RawStr > 10 )
							{
								textSay = StringCatalog.ResolveByKey( m.Account, "trap.msg.stat.str" );
								textLog = "a weakness trap";
					
								if ( Server.Misc.Worlds.IsOnSpaceship( m.Location, m.Map ) )
								{
									textSay = StringCatalog.ResolveByKey( m.Account, "trap.msg.stat.str.space" );
									textLog = "a bacterial contamination";
								}

								m.LocalOverheadMessage(MessageType.Emote, 0x916, true, textSay);
								m.FixedParticles( 0x3779, 10, 15, 5009, EffectLayer.Waist );
								m.PlaySound( 0x1E6 );
								m.RawStr = m.RawStr - 1; 
								sTrapType = textLog;
							}
						}
						else if ( nStat == 2 )
						{
							if ( m.RawDex > 10 )
							{
								textSay = StringCatalog.ResolveByKey( m.Account, "trap.msg.stat.dex" );
								textLog = "a slowness trap";
					
								if ( Server.Misc.Worlds.IsOnSpaceship( m.Location, m.Map ) )
								{
									textSay = StringCatalog.ResolveByKey( m.Account, "trap.msg.stat.dex.space" );
									textLog = "a bacterial contamination";
								}

								m.LocalOverheadMessage(MessageType.Emote, 0x916, true, textSay);
								m.FixedParticles( 0x3779, 10, 15, 5002, EffectLayer.Head );
								m.PlaySound( 0x1DF );
								m.RawDex = m.RawDex - 1; 
								sTrapType = textLog;
							}
						}
						else
						{
							if ( m.RawInt > 10 )
							{
								textSay = StringCatalog.ResolveByKey( m.Account, "trap.msg.stat.int" );
								textLog = "a mind numbing trap";
					
								if ( Server.Misc.Worlds.IsOnSpaceship( m.Location, m.Map ) )
								{
									textSay = StringCatalog.ResolveByKey( m.Account, "trap.msg.stat.int.space" );
									textLog = "a bacterial contamination";
								}

								m.LocalOverheadMessage(MessageType.Emote, 0x916, true, textSay);
								m.FixedParticles( 0x3779, 10, 15, 5004, EffectLayer.Head );
								m.PlaySound( 0x1E4 );
								m.RawInt = m.RawInt - 1;
								sTrapType = textLog;
							}
						}
					}
					else if ( nTrapType == 6 && SavingThrow( m, "Poison", true, this ) == false ) // POISON TRAP
					{
						textSay = StringCatalog.ResolveByKey( m.Account, "trap.msg.poison" );
						textLog = "a poison gas trap";
			
						if ( Server.Misc.Worlds.IsOnSpaceship( m.Location, m.Map ) )
						{
							textSay = StringCatalog.ResolveByKey( m.Account, "trap.msg.poison.space" );
							textLog = "a biological contamination";
						}

						int itHurts = m.PoisonResistance;
						int itSicks = 0;

						if ( itHurts >= 70 ){ itSicks = 1; }
						else if ( itHurts >= 50 ){ itSicks = 2; }
						else if ( itHurts >= 30 ){ itSicks = 3; }
						else if ( itHurts >= 10 ){ itSicks = 4; }
						else { itSicks = 5; }

						switch( Utility.RandomMinMax( 1, itSicks ) )
						{
							case 1: m.ApplyPoison( m, Poison.Lesser );	break;
							case 2: m.ApplyPoison( m, Poison.Regular );	break;
							case 3: m.ApplyPoison( m, Poison.Greater );	break;
							case 4: m.ApplyPoison( m, Poison.Deadly );	break;
							case 5: m.ApplyPoison( m, Poison.Lethal );	break;
						}

						Effects.SendLocationEffect( this.Location, this.Map, 0x11A8 - 2, 16, 3, 0, 0 );
						Effects.PlaySound( this.Location, this.Map, 0x231 );
						m.LocalOverheadMessage(MessageType.Emote, 0x916, true, textSay);

						sTrapType = textLog;
					}
					else if ( nTrapType == 7 && SavingThrow( m, "Magic", true, this ) == false ) // DRAIN TRAP
					{
						int nStat = Utility.RandomMinMax( 1, 3 );

						if ( nStat == 1 )
						{
							textSay = StringCatalog.ResolveByKey( m.Account, "trap.msg.drain.hp" );
							textLog = "a life draining trap";
				
							if ( Server.Misc.Worlds.IsOnSpaceship( m.Location, m.Map ) )
							{
								textSay = StringCatalog.ResolveByKey( m.Account, "trap.msg.drain.hp.space" );
								textLog = "a radioactive spill";
							}

							m.LocalOverheadMessage(MessageType.Emote, 0x916, true, textSay);
							m.FixedParticles( 0x3779, 10, 15, 5009, EffectLayer.Waist );
							m.PlaySound( 0x1E6 );
							m.Hits = 1; 
							sTrapType = textLog;
						}
						else if ( nStat == 2 )
						{
							textSay = StringCatalog.ResolveByKey( m.Account, "trap.msg.drain.stam" );
							textLog = "a stamina draining trap";
				
							if ( Server.Misc.Worlds.IsOnSpaceship( m.Location, m.Map ) )
							{
								textSay = StringCatalog.ResolveByKey( m.Account, "trap.msg.drain.stam.space" );
								textLog = "a radioactive spill";
							}

							m.LocalOverheadMessage(MessageType.Emote, 0x916, true, textSay);
							m.FixedParticles( 0x3779, 10, 15, 5002, EffectLayer.Head );
							m.PlaySound( 0x1DF );
							m.Stam = 0; 
							sTrapType = textLog;
						}
						else
						{
							textSay = StringCatalog.ResolveByKey( m.Account, "trap.msg.drain.mana" );
							textLog = "a mana draining trap";
				
							if ( Server.Misc.Worlds.IsOnSpaceship( m.Location, m.Map ) )
							{
								textSay = StringCatalog.ResolveByKey( m.Account, "trap.msg.drain.mana.space" );
								textLog = "a radioactive spill";
							}

							m.LocalOverheadMessage(MessageType.Emote, 0x916, true, textSay);
							m.FixedParticles( 0x3779, 10, 15, 5004, EffectLayer.Head );
							m.PlaySound( 0x1E4 );
							m.Mana = 0; 
							sTrapType = textLog;
						}
					}
					else if ( nTrapType == 8 && SavingThrow( m, "Magic", true, this ) == false ) // GEM STONE TRAP
					{
						List<Item> items = new List<Item>();
						int nAmount = 0;

						foreach( Item i in m.Backpack.FindItemsByType( typeof( Ruby ), true ) ){ items.Add(i); }
						foreach( Item i in m.Backpack.FindItemsByType( typeof( Amber ), true ) ){ items.Add(i); }
						foreach( Item i in m.Backpack.FindItemsByType( typeof( Amethyst ), true ) ){ items.Add(i); }
						foreach( Item i in m.Backpack.FindItemsByType( typeof( Citrine ), true ) ){ items.Add(i); }
						foreach( Item i in m.Backpack.FindItemsByType( typeof( Emerald ), true ) ){ items.Add(i); }
						foreach( Item i in m.Backpack.FindItemsByType( typeof( Diamond ), true ) ){ items.Add(i); }
						foreach( Item i in m.Backpack.FindItemsByType( typeof( Sapphire ), true ) ){ items.Add(i); }
						foreach( Item i in m.Backpack.FindItemsByType( typeof( StarSapphire ), true ) ){ items.Add(i); }
						foreach( Item i in m.Backpack.FindItemsByType( typeof( Tourmaline ), true ) ){ items.Add(i); }
						foreach( Item i in m.Backpack.FindItemsByType( typeof( DDRelicGem ), true ) ){ items.Add(i); }
						foreach( Item i in m.Backpack.FindItemsByType( typeof( MageEye ), true ) ){ items.Add(i); }

						foreach ( Item item in items )
						{
							if ( item != null )
							{
								nAmount = nAmount + item.Amount;
								item.Delete();
							}
						}
						if ( nAmount > 0 )
						{
							textSay = StringCatalog.ResolveByKey( m.Account, "trap.msg.gems" );
							textLog = "a lode stone trap";
				
							if ( Server.Misc.Worlds.IsOnSpaceship( m.Location, m.Map ) )
							{
								textSay = StringCatalog.ResolveByKey( m.Account, "trap.msg.gems.space" );
								textLog = "a damaged power coil";
							}

							RuinedGems rocks = new RuinedGems();
							rocks.Weight = nAmount * 5.0;
							rocks.RuinedCount = nAmount;
							m.AddToBackpack ( rocks );
							m.LocalOverheadMessage(MessageType.Emote, 0x916, true, textSay);
							m.FixedParticles( 0x374A, 10, 15, 5028, EffectLayer.Waist );
							m.PlaySound( 0x1E1 );
							sTrapType = textLog;
						}
					}
				else if ( nTrapType == 9 && SavingThrow( m, "Magic", true, this ) == false ) // REAGENT TRAP
				{
					// Partial spoilage — half of each reagent stack is ruined, half remains usable
					int nAmount = 0;

					if ( m != null && m.Backpack != null )
					{
						List<Item> list = new List<Item>();
						(m.Backpack).RecurseItems( list );
						foreach ( Item i in list )
						{
							if ( i.Catalog == Catalogs.Reagent )
							{
								int destroyAmt = Math.Max( 1, i.Amount / 2 );
								nAmount += destroyAmt;
								if ( destroyAmt >= i.Amount )
								{
									if ( i.Parent is NotIdentified )
										((NotIdentified)i.Parent).Delete();
									i.Delete();
								}
								else
								{
									i.Amount -= destroyAmt;
								}
							}
						}
					}

					if ( nAmount > 0 )
					{
						textSay = StringCatalog.ResolveByKey( m.Account, "trap.msg.reagent" );
						textLog = "a toxic cloud trap";

						RottedReagents regs = new RottedReagents();
						regs.Weight = nAmount * 0.1;
						regs.RottedCount = nAmount;
						m.AddToBackpack ( regs );
						Effects.SendLocationEffect( this.Location, this.Map, 0x11A8 - 2, 16, 3, 0, 0 );
						Effects.PlaySound( this.Location, this.Map, 0x231 );
						m.LocalOverheadMessage(MessageType.Emote, 0x916, true, textSay);
						sTrapType = textLog;
					}
				}
					else if ( nTrapType == 10 && SavingThrow( m, "Magic", true, this ) == false ) // BOOK BOUND TRAP
					{
						Container cont = m.Backpack;
						int nDull = 0;

						List<Item> items = new List<Item>();

						Item handy = m.FindItemOnLayer( Layer.OneHanded );
						if ( handy is Spellbook )
						{
							items.Add(handy); nDull = 1;
						}

						Item tally = m.FindItemOnLayer( Layer.Trinket );
						if ( tally is Spellbook )
						{
							items.Add(tally); nDull = 1;
						}

						foreach( Item i in m.Backpack.FindItemsByType( typeof( Spellbook ), true ) )
						{
							if (i.Parent is BookBox){} else
							{
								if ( i.LootType != LootType.Blessed )
								{
									if ( CheckInsuranceOnTrap( i, m ) )
									{
										m.LocalOverheadMessage(MessageType.Emote, 1150, true, StringCatalog.ResolveByKey( m.Account, "trap.insurance.books" ));
									}
									else
									{
										items.Add(i); nDull = 1;
									}
								}
							}
						}
						foreach( Item i in m.Backpack.FindItemsByType( typeof( Runebook ), true ) )
						{
							if (i.Parent is BookBox){} else
							{
								items.Add(i);
								nDull = 1;
							}
						}

						if ( nDull > 0 )
						{
							Container box = new BookBox();
							foreach ( Item item in items )
							{
								box.DropItem(item);
							}

							m.AddToBackpack ( box );

							m.LocalOverheadMessage(MessageType.Emote, 0x916, true, StringCatalog.ResolveByKey( m.Account, "trap.msg.books" ));
							m.FixedParticles( 0x374A, 10, 15, 5028, EffectLayer.Waist );
							m.PlaySound( 0x1E1 );
							sTrapType = "a book bound trap";
						}
					}
					else if ( nTrapType == 11 && SavingThrow( m, "Magic", true, this ) == false ) // TELEPORT TRAP
					{
						textSay = StringCatalog.ResolveByKey( m.Account, "trap.msg.teleport" );
						textLog = "a teleportation trap";
			
						if ( Server.Misc.Worlds.IsOnSpaceship( m.Location, m.Map ) )
						{
							textSay = StringCatalog.ResolveByKey( m.Account, "trap.msg.teleport.space" );
							textLog = "an overcharged transporter pad";
						}

						Point3D p = Worlds.GetRandomLocation( m.Land, "land" );
						Map map = Worlds.GetMyDefaultMap( m.Land );

						if ( p != Point3D.Zero )
						{
							Server.Mobiles.BaseCreature.TeleportPets( m, p, map );
							m.MoveToWorld( p, map );
							Effects.PlaySound( m.Location, m.Map, 0x1FC );
							m.LocalOverheadMessage(MessageType.Emote, 0x916, true, textSay);
							sTrapType = textLog;
						}
					}
					else if ( nTrapType == 12  && SavingThrow( m, "Magic", true, this ) == false && m.Fame > 0 ) // FAME TRAP
					{
						int FameLoss = (int)(m.Fame - ( m.Fame * 0.20 ));
						if ( FameLoss < 0 ){ FameLoss = 0; }
						if ( FameLoss > 0 )
						{
							m.Fame = FameLoss;
							m.LocalOverheadMessage(MessageType.Emote, 0x916, true, StringCatalog.ResolveByKey( m.Account, "trap.msg.fame" ));
							m.FixedParticles( 0x374A, 10, 15, 5032, EffectLayer.Head );
							m.PlaySound( 0x1F8 );
							sTrapType = "a forgotten fame trap";
						}
					}
					else if ( nTrapType == 13 && SavingThrow( m, "Magic", true, this ) == false ) // CURSE ITEM TRAP
					{
						Container cont = m.Backpack;
						Item iCursed = GetMyItem( m );

						if ( iCursed != null )
						{
							if ( Mobile.InsuranceEnabled && CheckInsuranceOnTrap( iCursed, m ) )
							{
								m.LocalOverheadMessage(MessageType.Emote, 1150, true, StringCatalog.ResolveByKey( m.Account, "trap.insurance.curse" ));
							}
							else
							{
								m.LocalOverheadMessage(MessageType.Emote, 0x916, true, StringCatalog.ResolveByKey( m.Account, "trap.msg.curse" ));
								m.FixedParticles( 0x374A, 10, 15, 5028, EffectLayer.Waist );
								m.PlaySound( 0x1E1 );

								Container box = new CurseItem();
								box.DropItem(iCursed);
								box.ItemID = iCursed.GraphicID;
								box.Hue = iCursed.GraphicHue;
								box.Name = iCursed.Name;

								m.AddToBackpack ( box );

								sTrapType = "a curse item trap";
							}
						}
					}
					else if ( nTrapType == 14 && SavingThrow( m, "Physical", true, this ) == false ) // FLOOR SPIKE TRAP
					{
						if ( Utility.RandomMinMax( 1, 2 ) == 1 ){ Effects.SendLocationEffect( this.Location, this.Map, 4506 + 1, 18, 3, 0, 0 ); }
						else { Effects.SendLocationEffect( this.Location, this.Map, 4512 + 1, 18, 3, 0, 0 ); }
						Effects.PlaySound( this.Location, this.Map, 0x22C );
						m.LocalOverheadMessage(MessageType.Emote, 0x916, true, StringCatalog.ResolveByKey( m.Account, "trap.msg.spike" ));
						int itHurts = (int)( (Utility.RandomMinMax(50,200) * ( 100 - m.PhysicalResistance ) ) / 100 );
						m.Damage( itHurts, m );
						sTrapType = "a spike trap";
					}
					else if ( nTrapType == 15 && SavingThrow( m, "Physical", true, this ) == false ) // SAW TRAP
					{
						if ( Utility.RandomMinMax( 1, 2 ) == 1 ){ Effects.SendLocationEffect( this.Location, this.Map, 0x11AC + 1, 6, 3, 0, 0 ); }
						else { Effects.SendLocationEffect( this.Location, this.Map, 0x11B1 + 1, 6, 3, 0, 0 ); }
						Effects.PlaySound( this.Location, this.Map, 0x21C );
						m.LocalOverheadMessage(MessageType.Emote, 0x916, true, StringCatalog.ResolveByKey( m.Account, "trap.msg.saw" ));
						int itHurts = (int)( (Utility.RandomMinMax(50,200) * ( 100 - m.PhysicalResistance ) ) / 100 );
						m.Damage( itHurts, m );
						sTrapType = "a saw blade trap";
					}
					else if ( nTrapType == 16 && SavingThrow( m, "Fire", true, this ) == false ) // FLAME TRAP
					{
						textSay = StringCatalog.ResolveByKey( m.Account, "trap.msg.fire" );
						textLog = "a fire trap";
			
						if ( Server.Misc.Worlds.IsOnSpaceship( m.Location, m.Map ) )
						{
							textSay = StringCatalog.ResolveByKey( m.Account, "trap.msg.fire.space" );
							textLog = "a thermal vent";
						}

						Effects.SendLocationParticles( EffectItem.Create( this.Location, this.Map, EffectItem.DefaultDuration ), 0x3709, 10, 30, 5052 );
						Effects.PlaySound( this.Location, this.Map, 0x225 );
						m.LocalOverheadMessage(MessageType.Emote, 0x916, true, textSay);
						int itHurts = (int)( (Utility.RandomMinMax(50,200) * ( 100 - m.FireResistance ) ) / 100 );
						m.Damage( itHurts, m );
						sTrapType = textLog;
					}
					else if ( nTrapType == 17 && SavingThrow( m, "Physical", true, this ) == false ) // GIANT SPIKE TRAP
					{
						Effects.SendLocationEffect( this.Location, this.Map, 0x1D99, 48, 2, 0, 0 );
						Effects.PlaySound( this.Location, this.Map, 0x22C );
						m.LocalOverheadMessage(MessageType.Emote, 0x916, true, StringCatalog.ResolveByKey( m.Account, "trap.msg.giantspike" ));
						int itHurts = (int)( (Utility.RandomMinMax(50,200) * ( 100 - m.PhysicalResistance ) ) / 100 );
						m.Damage( itHurts, m );
						sTrapType = "a giant spike trap";
					}
					else if ( nTrapType == 18 && SavingThrow( m, "Fire", true, this ) == false ) // EXPLOSION TRAP
					{
						textSay = StringCatalog.ResolveByKey( m.Account, "trap.msg.explosion" );
						textLog = "an explosion trap";
			
						if ( Server.Misc.Worlds.IsOnSpaceship( m.Location, m.Map ) )
						{
							textSay = StringCatalog.ResolveByKey( m.Account, "trap.msg.explosion.space" );
							textLog = "a plasma grenade";
						}

						m.FixedParticles( 0x36BD, 20, 10, 5044, EffectLayer.Head );
						m.PlaySound( 0x307 );
						m.LocalOverheadMessage(MessageType.Emote, 0x916, true, textSay);
						int itHurts = (int)( (Utility.RandomMinMax(50,200) * ( 100 - m.FireResistance ) ) / 100 );
						m.Damage( itHurts, m );
						sTrapType = textLog;
					}
					else if ( nTrapType == 19 && SavingThrow( m, "Energy", true, this ) == false ) // ELECTRICAL TRAP
					{
						textSay = StringCatalog.ResolveByKey( m.Account, "trap.msg.electrical" );
						textLog = "an electrical trap";
			
						if ( Server.Misc.Worlds.IsOnSpaceship( m.Location, m.Map ) )
						{
							textSay = StringCatalog.ResolveByKey( m.Account, "trap.msg.electrical.space" );
							textLog = "an electrically charged deck plate";
						}

						m.BoltEffect( 0 );
						m.LocalOverheadMessage(MessageType.Emote, 0x916, true, textSay);
						int itHurts = (int)( (Utility.RandomMinMax(50,200) * ( 100 - m.EnergyResistance ) ) / 100 );
						m.Damage( itHurts, m );
						sTrapType = textLog;
					}
				else if ( nTrapType == 20 && SavingThrow( m, "Agility", true, this ) == false ) // TRIP WIRE THAT BREAKS ARROWS
				{
					// Partial breakage — only half the ammo shatters on impact; the rest scatters but survives
					List<Item> items = new List<Item>();
					int nBroken = 0;
					int WhichArrows = Utility.RandomMinMax( 1, 2 );
					string sTripped = "";
					int nAmount = 0;

					if ( WhichArrows == 1 )
					{
						foreach( Item i in m.Backpack.FindItemsByType( typeof( Arrow ), true ) )
						{
							items.Add(i);
							nBroken = 1;
							sTripped = "arrows";
						}
					}
					else
					{
						foreach( Item i in m.Backpack.FindItemsByType( typeof( Bolt ), true ) )
						{
							items.Add(i);
							nBroken = 1;
							sTripped = "crossbow bolts";
						}
					}
					if ( nBroken > 0 )
					{
						foreach ( Item item in items )
						{
							if ( item != null )
							{
								int destroyAmt = Math.Max( 1, item.Amount / 2 );
								nAmount += destroyAmt;
								if ( destroyAmt >= item.Amount )
									item.Delete();
								else
									item.Amount -= destroyAmt;
							}
						}
						if ( nAmount > 0 )
						{
							textSay = StringCatalog.ResolveByKey( m.Account, sTripped == "arrows" ? "trap.msg.brokenarrows" : "trap.msg.brokenbolts" );
							textLog = "a trip wire trap";
					
							if ( Server.Misc.Worlds.IsOnSpaceship( m.Location, m.Map ) )
							{
								textSay = StringCatalog.ResolveByKey( m.Account, sTripped == "arrows" ? "trap.msg.brokenarrows.space" : "trap.msg.brokenbolts.space" );
								textLog = "a loose deck plate";
							}

							Shaft wood = new Shaft();
							wood.Amount = nAmount;
							m.AddToBackpack ( wood );

							m.LocalOverheadMessage(MessageType.Emote, 0x916, true, textSay);
							m.PlaySound( m.Female ? 812 : 1086 );
							sTrapType = textLog;
						}
					}
				}
				else if ( nTrapType == 21 && SavingThrow( m, "Poison", true, this ) == false ) // TAINTED TRAP
				{
					// Partial taint — only half the bandages are contaminated; the rest were shielded in your pack
					List<Item> items = new List<Item>();
					int nAmount = 0;

					foreach( Item i in m.Backpack.FindItemsByType( typeof( Bandage ), true ) )
					{
						items.Add(i);
					}
					foreach ( Item item in items )
					{
						if ( item != null )
						{
							int taintAmt = Math.Max( 1, item.Amount / 2 );
							nAmount += taintAmt;
							if ( taintAmt >= item.Amount )
								item.Delete();
							else
								item.Amount -= taintAmt;
						}
					}
					if ( nAmount > 0 )
					{
						TaintedBandage bandage = new TaintedBandage();
						bandage.Weight = nAmount * 0.1;
						string sAmount = nAmount.ToString();
						if ( nAmount > 1 ){ bandage.Name = sAmount + " tainted bandages"; }
						m.AddToBackpack ( bandage );

						Effects.SendLocationEffect( this.Location, this.Map, 0x11A8 - 2, 16, 3, 0, 0 );
						Effects.PlaySound( this.Location, this.Map, 0x231 );
						m.LocalOverheadMessage(MessageType.Emote, 0x916, true, StringCatalog.ResolveByKey( m.Account, "trap.msg.bandages" ));

						sTrapType = "a noxious cloud trap";
					}
				}
				else if ( nTrapType == 22 && SavingThrow( m, "Agility", true, this ) == false ) // TRIP WIRE THAT BREAKS POTIONS
				{
					// Partial breakage — each potion has a 50% chance to survive the fall; glass rattle, not total loss
					int nBroken = 0;

					if ( m != null && m.Backpack != null )
					{
						List<Item> list = new List<Item>();
						(m.Backpack).RecurseItems( list );
						foreach ( Item i in list )
						{
							if ( i.Catalog == Catalogs.Potion && Utility.RandomMinMax( 1, 2 ) == 1 )
							{
								nBroken = 1;
								if ( i.Parent is NotIdentified )
									((NotIdentified)i.Parent).Delete();
								i.Delete();
							}
						}
					}

					if ( nBroken > 0 )
					{
						textSay = StringCatalog.ResolveByKey( m.Account, "trap.msg.potions" );
						textLog = "a trip wire trap";
				
						if ( Server.Misc.Worlds.IsOnSpaceship( m.Location, m.Map ) )
						{
							textSay = StringCatalog.ResolveByKey( m.Account, "trap.msg.potions.space" );
							textLog = "a loose deck plate";
						}

						m.LocalOverheadMessage(MessageType.Emote, 0x916, true, textSay);
						m.PlaySound( 0x040 );
						sTrapType = textLog;
					}
				}
					else if ( nTrapType == 23 && SavingThrow( m, "Magic", true, this ) == false ) // JEWELERY TRAP
					{
						// Tangle equipped jewelry
						bool tangled = false;
						JewelryBox box = new JewelryBox();
						var items = JewelryBox.FindCandidates( m );
						if ( 0 < items.Count )
						{
							foreach ( Item item in items )
							{
								box.DropItem(item);
							}
							m.AddToBackpack( box );

							tangled = true;
						}

						// Ruin jewelry in backpack
						int ruined = 0;
						items.Clear();
						(m.Backpack).RecurseItems( items );
						foreach ( Item i in items )
						{
							if (i is JewelryBox || i.Parent is JewelryBox) continue;

							if ( i is BaseTrinket && i.Catalog == Catalogs.Jewelry )
							{
								if (i.Parent is NotIdentified)
									((NotIdentified)i.Parent).Delete();
								i.Delete();
								ruined++;
							}
						}

						if ( 0 < ruined )
							box.AddJunk( ruined );

						if ( tangled || 0 < ruined )
						{
							textSay = StringCatalog.ResolveByKey( m.Account, "trap.msg.jewelry" );
							textLog = "a jewelry melting trap";
							m.LocalOverheadMessage(MessageType.Emote, 0x916, true, textSay);
							sTrapType = textLog;
						}
						else
						{
							box.Delete();
						}
					}
					else if ( nTrapType == 24 && SavingThrow( m, "Agility", true, this ) == false ) // PIT TRAP
					{
						textSay = StringCatalog.ResolveByKey( m.Account, "trap.msg.pit" );
						textLog = "a deep pit";

						string sX = m.X.ToString();
						string sY = m.Y.ToString();
						string sZ = m.Z.ToString();
						string sMap = Worlds.GetMyMapString( m.Map );
						string sZone = Server.Lands.LandName( Server.Lands.GetLand( m.Map, m.Location, m.X, m.Y ) );

						((PlayerMobile)m).CharacterPublicDoor = sX + "#" + sY + "#" + sZ + "#" + sMap + "#" + sZone;

						Effects.PlaySound( m.Location, m.Map, Utility.RandomList( 0x5D2,0x5D3 ) );
						Point3D p = new Point3D( 2602, 3688, 100 );
						Point3D b = new Point3D( 2602, 3688, 0 );
						Map map = Map.Sosaria;

						Server.Mobiles.BaseCreature.TeleportPets( m, b, map );
						m.MoveToWorld( p, map );
						m.LocalOverheadMessage(MessageType.Emote, 0x916, true, textSay);
						sTrapType = textLog;
					}
					else if ( nTrapType == 25 && m.Karma != 0 ) // ALIGNMENT TRAP — graduated corruption, conviction-based resistance
					{
						// Base Magic save (Int + MagicResist + EnergyResist), same formula as other Magic saves
						int alignBaseSave = (int)(( m.Int + m.Skills[SkillName.MagicResist].Value + m.EnergyResistance ) / 4);

						// Moral disciplines strengthen resistance to soul corruption
						int moralDisciplineBonus = (int)(( m.Skills[SkillName.Meditation].Value + m.Skills[SkillName.Spiritualism].Value ) / 20);

						// Meditation mastery (+10 at ≥100) provides additional clarity against soul corruption
						int meditationMasteryBonus = ( m.Skills[SkillName.Meditation].Value >= 100 ) ? 10 : 0;

						// Deep moral conviction anchors identity — the more committed, the harder to corrupt
						// (+1 per 300 karma, up to +50 at maximum ±15000)
						int convictionBonus = Math.Abs( m.Karma ) / 300;

						int alignSaveThrow = Math.Min( alignBaseSave + moralDisciplineBonus + meditationMasteryBonus + convictionBonus, 75 );
						bool savedAlignment = alignSaveThrow >= Utility.RandomMinMax( 1, 100 );

						if ( savedAlignment )
						{
							if ( MySettings.S_AnnounceTrapSaves )
							{
								m.LocalOverheadMessage( Network.MessageType.Emote, 0x3B2, false,
									StringCatalog.ResolveByKey( m.Account, "trap.save.avoid.trap25" ) );
								m.PlaySound( m.Female ? 778 : 1049 );
								HiddenTrapType = 1000;
							}
						}
						else
						{
							int absKarma = Math.Abs( m.Karma );
							bool wasPositive = m.Karma >= 0;
							int finalKarma;

							if ( absKarma > 10000 )
							{
								// Deeply aligned: conviction partially holds; alignment is badly weakened but does NOT invert
								finalKarma = (int)( m.Karma * 0.5 );
								textSay = StringCatalog.ResolveByKey( m.Account, "trap.msg.type25.save" );
							}
							else if ( absKarma > 7000 )
							{
								// Strongly aligned: moderate retention, no flip
								finalKarma = (int)( m.Karma * 0.25 );
								textSay = StringCatalog.ResolveByKey( m.Account, "trap.msg.type25.save" );
							}
							else if ( absKarma > 4000 )
							{
								// Moderately aligned: alignment inverts but at reduced magnitude — something to rebuild from
								finalKarma = -(int)( m.Karma * 0.5 );
								if ( finalKarma > 15000 ) finalKarma = 15000;
								if ( finalKarma < -15000 ) finalKarma = -15000;
								textSay = StringCatalog.ResolveByKey( m.Account, "trap.msg.type25.moderate" );
							}
							else
							{
								// Weakly aligned: no anchor to hold; devastating full flip
								finalKarma = m.Karma * -1;
								textSay = StringCatalog.ResolveByKey( m.Account, "trap.msg.type25.light" );
							}

							m.Karma = finalKarma;

							// Mirror AwardKarma's KarmaLocked logic when alignment crosses zero
							bool alignmentCrossedZero = false;
							if ( m is PlayerMobile )
							{
								PlayerMobile pm = (PlayerMobile)m;
								bool isNowPositive = m.Karma >= 0;
								if ( !Core.AOS && wasPositive && !isNowPositive && !pm.KarmaLocked )
								{
									pm.KarmaLocked = true;
									alignmentCrossedZero = true;
								}
								else if ( isNowPositive && pm.KarmaLocked )
									pm.KarmaLocked = false;
							}

							textLog = "a mind warping trap";
							m.LocalOverheadMessage( MessageType.Emote, 0x916, true, textSay );
							m.FixedParticles( 0x374A, 10, 15, 5028, EffectLayer.Waist );
							m.PlaySound( 0x1E1 );
							sTrapType = textLog;

							// Direct players toward the correct recovery action
							if ( alignmentCrossedZero )
								m.SendMessage( StringCatalog.ResolveByKey( m.Account, "trap.karma.guidance.inverted" ) );
							else
								m.SendMessage( StringCatalog.ResolveByKey( m.Account, "trap.karma.guidance.weakened" ) );
						}
					}

					///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

					if ( sTrapType != "" )
					{
						if ( m.Hidden != false )
						{
							m.RevealingAction();
						}

						HadAnyAffect = true; 

						LoggingFunctions.LogTraps( m, sTrapType );
					}
				}

				if ( HadAnyAffect || HiddenTrapType == 1000 )
					DisableTrap( this );
				else
				{
					if ( Weight == 3.0 )
					{
						DisableTrap( this );
						this.Name = StringCatalog.ResolveByKey(null, "trap.name.broken");
						m.PlaySound( 0x41 ); // glass breaking
						m.SendMessage( StringCatalog.ResolveByKey( m.Account, "trap.brokeonstep" ) );
					}	
					else
						DitchTrap( this );

				}
			}

			if ( sTrapType == "a teleportation trap" || sTrapType == "an overcharged transporter pad" || sTrapType == "a deep pit" )
				return false;

			return true;
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
            writer.Write( HiddenTrapType );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
            HiddenTrapType = reader.ReadInt();
			if ( Weight != 1.0 )
				Delete();
		}

		public static bool IAmAWeaponSlayer( Mobile m, Mobile enemy )
		{
			bool IsSlayer = false;

			if ( m is PlayerMobile )
			{
				if ( m.FindItemOnLayer( Layer.OneHanded ) is BaseWeapon )
				{
					Item hitter = m.FindItemOnLayer( Layer.OneHanded );
					BaseWeapon weapon = (BaseWeapon)hitter;
					SlayerName slay1 = weapon.Slayer;
					SlayerName slay2 = weapon.Slayer2;
					if ( slay1 != SlayerName.None )
					{
						SlayerEntry entry1 = SlayerGroup.GetEntryByName( slay1 );
						if ( entry1.Slays( enemy ) ){ IsSlayer = true; }
					}
					if ( slay2 != SlayerName.None )
					{
						SlayerEntry entry2 = SlayerGroup.GetEntryByName( slay2 );
						if ( entry2.Slays( enemy ) ){ IsSlayer = true; }
					}
				}
				else if ( m.FindItemOnLayer( Layer.TwoHanded ) is BaseWeapon )
				{
					Item hitter = m.FindItemOnLayer( Layer.TwoHanded );
					BaseWeapon weapon = (BaseWeapon)hitter;
					SlayerName slay1 = weapon.Slayer;
					SlayerName slay2 = weapon.Slayer2;
					if ( slay1 != SlayerName.None )
					{
						SlayerEntry entry1 = SlayerGroup.GetEntryByName( slay1 );
						if ( entry1.Slays( enemy ) ){ IsSlayer = true; }
					}
					if ( slay2 != SlayerName.None )
					{
						SlayerEntry entry2 = SlayerGroup.GetEntryByName( slay2 );
						if ( entry2.Slays( enemy ) ){ IsSlayer = true; }
					}
				}
			}
			return IsSlayer;
		}

		public static bool IAmShielding( Mobile m, int skill )
		{
			bool Shielded = false;

			if ( m is PlayerMobile )
			{
				if ( m.FindItemOnLayer( Layer.TwoHanded ) is BaseShield )
				{
					if ( m.CheckSkill( SkillName.Parry, 0, skill ) )
					{
						Shielded = true;
					}
				}
			}
			return Shielded;
		}

		public static bool SavingThrow( Mobile m, string save, bool isTrap, Item trap )
		{
			bool madeSave = false;
			int SaveThrow = 0;
			string areaKey = "";

			if ( save == "Magic" )
			{
				areaKey = "trap.saveresist.magic";
				SaveThrow = (int)(( m.Int + m.Skills[SkillName.MagicResist].Value + m.EnergyResistance ) / 4);
			}
			else if ( save == "Physical" )
			{
				areaKey = "trap.saveresist.physical";
				SaveThrow = (int)(( m.Str + m.PhysicalResistance ) / 3);
			}
			else if ( save == "Agility" )
			{
				areaKey = "trap.saveresist.agility";
				SaveThrow = m.Dex;
			}
			else if ( save == "Cold" )
			{
				areaKey = "trap.saveresist.cold";
				SaveThrow = (int)(( m.Dex + m.ColdResistance ) / 3);
			}
			else if ( save == "Fire" )
			{
				areaKey = "trap.saveresist.fire";
				SaveThrow = (int)(( m.Dex + m.FireResistance ) / 3);
			}
			else if ( save == "Poison" )
			{
				areaKey = "trap.saveresist.poison";
				SaveThrow = (int)(( m.Str + m.Skills[SkillName.Poisoning].Value + m.PoisonResistance ) / 4);
			}
			else if ( save == "Energy" )
			{
				areaKey = "trap.saveresist.energy";
				SaveThrow = (int)(( m.Int + m.EnergyResistance ) / 3);
			}

			if ( SaveThrow > 60 ){ SaveThrow = 60; }

			if ( SaveThrow >= Utility.RandomMinMax( 1, 100 ) )
			{
				if ( isTrap && MySettings.S_AnnounceTrapSaves )
				{
					string areaLoc = StringCatalog.ResolveByKey( m.Account, areaKey );
					string textSay = StringCatalog.ResolveFormatByKey( m.Account, "trap.save.avoid.trap", areaLoc );
					if ( Server.Misc.Worlds.IsOnSpaceship( m.Location, m.Map ) )
					{
						textSay = StringCatalog.ResolveFormatByKey( m.Account, "trap.save.avoid.danger", areaLoc );
					}
					m.LocalOverheadMessage(Network.MessageType.Emote, 0x3B2, false, textSay);
					m.PlaySound( m.Female ? 778 : 1049 );

					if ( trap is HiddenTrap )
						((HiddenTrap)trap).HiddenTrapType = 1000;
				}
				madeSave = true;
			}

			return madeSave;
		}

		public static bool CheckInsuranceOnTrap( Item item, Mobile m )
		{
			if ( item.LootType == LootType.Blessed )
			{
				return true;
			}
			else if ( Mobile.InsuranceEnabled && item.Insured )
			{
				PlayerMobile pm = (PlayerMobile)m;

				if ( pm.AutoRenewInsurance )
				{
					int cost = 900;

					if ( Banker.Withdraw( m, cost ) )
					{
						item.PayedInsurance = true;
						m.SendLocalizedMessage(1060398, cost.ToString()); // ~1_AMOUNT~ gold has been withdrawn from your bank box.
					}
					else
					{
						m.SendLocalizedMessage( 1061079, "", 0x23 ); // You lack the funds to purchase the insurance
						item.PayedInsurance = false;
						item.Insured = false;
					}
				}
				else
				{
					item.PayedInsurance = false;
					item.Insured = false;
				}
				return true;
			}

			return false;
		}

		public static bool CanSetOffTraps( Mobile m )
		{
			if ( m is PlayerMobile && ( !m.Alive || m.Blessed || m.AccessLevel > AccessLevel.Player ) )
				return false;

			return true;
		}

		public static bool SeeIfTrapActive( Item trap )
		{
			if ( trap.Weight < 2.0 && trap is HiddenTrap && Utility.RandomMinMax( 1, 100 ) > MyServerSettings.FloorTrapTrigger() )
			{
				DitchTrap( trap );
				return false;
			}
			else if ( trap.Weight < 2.0 )
				trap.Weight = 2.0;

			return true;
		}

		public static void DitchTrap( Item trap )
		{
			if ( trap.Weight < 5.0 && trap is HiddenTrap )
			{
				trap.Visible = false;
				trap.Weight = 6.0;
				SetAppearance( trap );
				new Delete_5_Seconds( trap ).Start();
			}
		}

		public static void DisableTrap( Item trap )
		{
			if ( trap.Weight < 5.0 && trap is HiddenTrap )
			{
				trap.Visible = true;
				trap.Weight = 5.0;
				SetAppearance( trap );
				new Delete_5_Minutes( trap ).Start();
			}
		}

		public static void DiscoverTrap( Item trap )
		{
			if ( trap.Weight < 3.0 && trap is HiddenTrap )
			{
				trap.Visible = true;
				trap.Weight = 3.0;
				SetAppearance( trap );
				new Delete_5_Minutes( trap ).Start();
			}
		}

		/// <returns>`True` if the trap his been triggered</returns>
		public static bool CheckTrapAvoidance( Mobile m, Item Trap )
		{
			string textSay;

		if ( m.Skills.RemoveTrap.Value >= 5 )
		{
			if ( m is PlayerMobile && m.CheckSkill( SkillName.RemoveTrap, 0, 125 ) )
			{
				if ( Trap is MushroomTrap )
				{
					m.LocalOverheadMessage(Network.MessageType.Emote, 0x3B2, false, StringCatalog.ResolveByKey(m.Account, "trap.avoid.remove.mushroom"));
				}
				else
				{
					textSay = StringCatalog.ResolveByKey( m.Account, "trap.avoid.remove.trap" );
					if ( Server.Misc.Worlds.IsOnSpaceship( m.Location, m.Map ) )
					{
						textSay = StringCatalog.ResolveByKey( m.Account, "trap.avoid.remove.danger" );
					}
					m.PlaySound( m.Female ? 0x32E : 0x440 );
					m.LocalOverheadMessage(Network.MessageType.Emote, 0x3B2, false, textSay);
					m.PlaySound( 0x241 );

					// Skilled disarm yields salvageable components — positive reinforcement for trap expertise
					// Reward scales with Remove Trap skill (5–25 gold equivalent at skill 25–125)
					int salvageValue = Utility.RandomMinMax( 5, Math.Max( 5, (int)( m.Skills.RemoveTrap.Value / 5 ) ) );
					Gold salvage = new Gold( salvageValue );
					salvage.MoveToWorld( Trap.Location, Trap.Map );
					m.SendMessage( StringCatalog.ResolveByKey( m.Account, "trap.scavenge" ) );
				}

				return false;
			}
		}

			if ( m is PlayerMobile )
			{
				if ( m.Backpack != null )
				{
					Item magicwand = m.Backpack.FindItemByType( typeof ( TrapWand ) );

					if ( magicwand != null )
					{
						TrapWand wands = (TrapWand)magicwand;
						int nPower = wands.WandPower;
						if ( nPower >= Utility.RandomMinMax( 1, 100 ) && wands.owner == m )
						{
							if ( Trap is MushroomTrap )
							{
								m.LocalOverheadMessage(Network.MessageType.Emote, 0x3B2, false, StringCatalog.ResolveByKey(m.Account, "trap.avoid.wand.mushroom"));
							}
							else
							{
								textSay = StringCatalog.ResolveByKey( m.Account, "trap.avoid.wand.trap" );
								if ( Server.Misc.Worlds.IsOnSpaceship( m.Location, m.Map ) )
								{
									textSay = StringCatalog.ResolveByKey( m.Account, "trap.avoid.wand.danger" );
								}
								m.LocalOverheadMessage(Network.MessageType.Emote, 0x3B2, false, textSay);
							}

							m.PlaySound( 0x1F0 );

							return false;
						}
					}
				}

				if ( GetPlayerInfo.LuckyPlayer(m.Luck) )
				{
					if ( Trap is MushroomTrap )
					{
						m.LocalOverheadMessage(Network.MessageType.Emote, 0x3B2, false, StringCatalog.ResolveByKey(m.Account, "trap.avoid.luck.mushroom"));
					}
					else
					{
						textSay = StringCatalog.ResolveByKey( m.Account, "trap.avoid.luck.trap" );
						if ( Server.Misc.Worlds.IsOnSpaceship( m.Location, m.Map ) )
						{
							textSay = StringCatalog.ResolveByKey( m.Account, "trap.avoid.luck.danger" ); m.PlaySound( 0x54B );
						}
						else { m.PlaySound( 0x241 ); }
						m.LocalOverheadMessage(Network.MessageType.Emote, 0x3B2, false, textSay);
					}

					return false;
				}

				if ( m.Backpack != null )
				{
					Item tenfootpole = m.Backpack.FindItemByType( typeof ( TenFootPole ) );

					if ( tenfootpole != null )
					{
						TenFootPole poles = (TenFootPole)tenfootpole;
						if ( poles.Tap >= Utility.RandomMinMax( 1, 100 ) )
						{
							m.PlaySound( 0x3FD );
							poles.ConsumeLimits( 1 );
							if ( poles.Limits < 1 )
							{
								if ( Trap is MushroomTrap )
								{
									m.LocalOverheadMessage(Network.MessageType.Emote, 0x3B2, false, StringCatalog.ResolveByKey(m.Account, "trap.avoid.pole.break.mushroom"));
								}
								else
								{
									textSay = StringCatalog.ResolveByKey( m.Account, "trap.avoid.pole.break.trap" );
									if ( Server.Misc.Worlds.IsOnSpaceship( m.Location, m.Map ) )
									{
										textSay = StringCatalog.ResolveByKey( m.Account, "trap.avoid.pole.break.danger" );
									}
									m.LocalOverheadMessage(Network.MessageType.Emote, 0x3B2, false, textSay);
								}
							}
							else
							{
								if ( Trap is MushroomTrap )
								{
									m.LocalOverheadMessage(Network.MessageType.Emote, 0x3B2, false, StringCatalog.ResolveByKey(m.Account, "trap.avoid.pole.mushroom"));
								}
								else
								{
									textSay = StringCatalog.ResolveByKey( m.Account, "trap.avoid.pole.trap" );
									if ( Server.Misc.Worlds.IsOnSpaceship( m.Location, m.Map ) )
									{
										textSay = StringCatalog.ResolveByKey( m.Account, "trap.avoid.pole.danger" );
									}
									m.LocalOverheadMessage(Network.MessageType.Emote, 0x3B2, false, textSay);
								}
								poles.InvalidateProperties();
							}

							return false;
						}
					}
				}
			}

			return true;
		}

		public static Item GetMyItem( Mobile m )
		{
			if ( m == null )
				return null;

			Item myItem = null;
			Item myBlessCheck = null;
			int cycle = 0;

			int nOuterTorso = 0;
			int nOneHanded = 0;
			int nTwoHanded = 0;
			int nBracelet = 0;
			int nRing = 0;
			int nHelm = 0;
			int nArms = 0;
			int nOuterLegs = 0;
			int nNeck = 0;
			int nGloves = 0;
			int nTalisman = 0;
			int nShoes = 0;
			int nCloak = 0;
			int nFirstValid = 0;
			int nWaist = 0;
			int nInnerLegs = 0;
			int nInnerTorso = 0;
			int nPants = 0;
			int nShirt = 0;
			int nEarrings = 0;

			if ( m.FindItemOnLayer( Layer.OuterTorso ) != null ) { myBlessCheck = m.FindItemOnLayer( Layer.OuterTorso ); if ( myBlessCheck.LootType == LootType.Blessed ){ nOuterTorso = 1; } }
			if ( m.FindItemOnLayer( Layer.OneHanded ) != null ) { myBlessCheck = m.FindItemOnLayer( Layer.OneHanded ); if ( myBlessCheck.LootType == LootType.Blessed ){ nOneHanded = 1; } }
			if ( m.FindItemOnLayer( Layer.TwoHanded ) != null ) { myBlessCheck = m.FindItemOnLayer( Layer.TwoHanded ); if ( myBlessCheck.LootType == LootType.Blessed ){ nTwoHanded = 1; } }
			if ( m.FindItemOnLayer( Layer.Bracelet ) != null ) { myBlessCheck = m.FindItemOnLayer( Layer.Bracelet ); if ( myBlessCheck.LootType == LootType.Blessed ){ nBracelet = 1; } }
			if ( m.FindItemOnLayer( Layer.Ring ) != null ) { myBlessCheck = m.FindItemOnLayer( Layer.Ring ); if ( myBlessCheck.LootType == LootType.Blessed ){ nRing = 1; } }
			if ( m.FindItemOnLayer( Layer.Helm ) != null ) { myBlessCheck = m.FindItemOnLayer( Layer.Helm ); if ( myBlessCheck.LootType == LootType.Blessed ){ nHelm = 1; } }
			if ( m.FindItemOnLayer( Layer.Arms ) != null ) { myBlessCheck = m.FindItemOnLayer( Layer.Arms ); if ( myBlessCheck.LootType == LootType.Blessed ){ nArms = 1; } }
			if ( m.FindItemOnLayer( Layer.OuterLegs ) != null ) { myBlessCheck = m.FindItemOnLayer( Layer.OuterLegs ); if ( myBlessCheck.LootType == LootType.Blessed ){ nOuterLegs = 1; } }
			if ( m.FindItemOnLayer( Layer.Neck ) != null ) { myBlessCheck = m.FindItemOnLayer( Layer.Neck ); if ( myBlessCheck.LootType == LootType.Blessed ){ nNeck = 1; } }
			if ( m.FindItemOnLayer( Layer.Gloves ) != null ) { myBlessCheck = m.FindItemOnLayer( Layer.Gloves ); if ( myBlessCheck.LootType == LootType.Blessed ){ nGloves = 1; } }
			if ( m.FindItemOnLayer( Layer.Trinket ) != null ) { if (!( m.FindItemOnLayer( Layer.Trinket ) is Spellbook )){ myBlessCheck = m.FindItemOnLayer( Layer.Trinket ); if ( myBlessCheck.LootType == LootType.Blessed ){ nTalisman = 1; } } }
			if ( m.FindItemOnLayer( Layer.Shoes ) != null ) { myBlessCheck = m.FindItemOnLayer( Layer.Shoes ); if ( myBlessCheck.LootType == LootType.Blessed ){ nShoes = 1; } }
			if ( m.FindItemOnLayer( Layer.Cloak ) != null ) { myBlessCheck = m.FindItemOnLayer( Layer.Cloak ); if ( myBlessCheck.LootType == LootType.Blessed ){ nCloak = 1; } }
			if ( m.FindItemOnLayer( Layer.FirstValid ) != null ) { myBlessCheck = m.FindItemOnLayer( Layer.FirstValid ); if ( myBlessCheck.LootType == LootType.Blessed ){ nFirstValid = 1; } }
			if ( m.FindItemOnLayer( Layer.Waist ) != null ) { myBlessCheck = m.FindItemOnLayer( Layer.Waist ); if ( myBlessCheck.LootType == LootType.Blessed ){ nWaist = 1; } }
			if ( m.FindItemOnLayer( Layer.InnerLegs ) != null ) { myBlessCheck = m.FindItemOnLayer( Layer.InnerLegs ); if ( myBlessCheck.LootType == LootType.Blessed ){ nInnerLegs = 1; } }
			if ( m.FindItemOnLayer( Layer.InnerTorso ) != null ) { myBlessCheck = m.FindItemOnLayer( Layer.InnerTorso ); if ( myBlessCheck.LootType == LootType.Blessed ){ nInnerTorso = 1; } }
			if ( m.FindItemOnLayer( Layer.Pants ) != null ) { myBlessCheck = m.FindItemOnLayer( Layer.Pants ); if ( myBlessCheck.LootType == LootType.Blessed ){ nPants = 1; } }
			if ( m.FindItemOnLayer( Layer.Shirt ) != null ) { myBlessCheck = m.FindItemOnLayer( Layer.Shirt ); if ( myBlessCheck.LootType == LootType.Blessed ){ nShirt = 1; } }
			if ( m.FindItemOnLayer( Layer.Earrings ) != null ) { myBlessCheck = m.FindItemOnLayer( Layer.Earrings ); if ( myBlessCheck.LootType == LootType.Blessed ){ nEarrings = 1; } }

			while ( cycle < 20 )
			{
				cycle++;

				switch( Utility.RandomMinMax( 1, 20 ) )
				{
					case 1: if ( m.FindItemOnLayer( Layer.Waist ) != null && nWaist != 1) { myItem = m.FindItemOnLayer( Layer.Waist ); } break;
					case 2: if ( m.FindItemOnLayer( Layer.OuterTorso ) != null && nOuterTorso != 1) { myItem = m.FindItemOnLayer( Layer.OuterTorso ); } break;
					case 3: if ( m.FindItemOnLayer( Layer.OneHanded ) != null && nOneHanded != 1) { myItem = m.FindItemOnLayer( Layer.OneHanded ); } break;
					case 4: if ( m.FindItemOnLayer( Layer.TwoHanded ) != null && nTwoHanded != 1) { myItem = m.FindItemOnLayer( Layer.TwoHanded ); } break;
					case 5: if ( m.FindItemOnLayer( Layer.Bracelet ) != null && nBracelet != 1) { myItem = m.FindItemOnLayer( Layer.Bracelet ); } break;
					case 6: if ( m.FindItemOnLayer( Layer.Ring ) != null && nRing != 1) { myItem = m.FindItemOnLayer( Layer.Ring ); } break;
					case 7: if ( m.FindItemOnLayer( Layer.Helm ) != null && nHelm != 1) { myItem = m.FindItemOnLayer( Layer.Helm ); } break;
					case 8: if ( m.FindItemOnLayer( Layer.Arms ) != null && nArms != 1) { myItem = m.FindItemOnLayer( Layer.Arms ); } break;
					case 9: if ( m.FindItemOnLayer( Layer.OuterLegs ) != null && nOuterLegs != 1) { myItem = m.FindItemOnLayer( Layer.OuterLegs ); } break;
					case 10: if ( m.FindItemOnLayer( Layer.Neck ) != null && nNeck != 1) { myItem = m.FindItemOnLayer( Layer.Neck ); } break;
					case 11: if ( m.FindItemOnLayer( Layer.Gloves ) != null && nGloves != 1) { myItem = m.FindItemOnLayer( Layer.Gloves ); } break;
					case 12: if ( m.FindItemOnLayer( Layer.Trinket ) != null && nTalisman != 1) { myItem = m.FindItemOnLayer( Layer.Trinket ); } break;
					case 13: if ( m.FindItemOnLayer( Layer.Shoes ) != null && nShoes != 1) { myItem = m.FindItemOnLayer( Layer.Shoes ); } break;
					case 14: if ( m.FindItemOnLayer( Layer.Cloak ) != null && nCloak != 1) { myItem = m.FindItemOnLayer( Layer.Cloak ); } break;
					case 15: if ( m.FindItemOnLayer( Layer.FirstValid ) != null && nFirstValid != 1) { myItem = m.FindItemOnLayer( Layer.FirstValid ); } break;
					case 16: if ( m.FindItemOnLayer( Layer.InnerLegs ) != null && nInnerLegs != 1) { myItem = m.FindItemOnLayer( Layer.InnerLegs ); } break;
					case 17: if ( m.FindItemOnLayer( Layer.InnerTorso ) != null && nInnerTorso != 1) { myItem = m.FindItemOnLayer( Layer.InnerTorso ); } break;
					case 18: if ( m.FindItemOnLayer( Layer.Pants ) != null && nPants != 1) { myItem = m.FindItemOnLayer( Layer.Pants ); } break;
					case 19: if ( m.FindItemOnLayer( Layer.Shirt ) != null && nShirt != 1) { myItem = m.FindItemOnLayer( Layer.Shirt ); } break;
					case 20: if ( m.FindItemOnLayer( Layer.Earrings ) != null && nEarrings != 1) { myItem = m.FindItemOnLayer( Layer.Earrings ); } break;
				}

				if ( myItem != null )
					cycle = 20;
			}

			if ( myItem != null && myItem.Density != Density.None && ((int)(myItem.Density)*7) > Utility.Random( 100 ) )
				myItem = null;

			return myItem;
		}

		private void TryProximityDetection(PlayerMobile m, int dist)
		{
			double searching = m.Skills.Searching.Value;

			if (searching >= 25)
			{
				int detectRadius;

				if (searching >= 125) { detectRadius = 5; }
				else if (searching >= 100) { detectRadius = 4; }
				else if (searching >= 75) { detectRadius = 3; }
				else if (searching >= 50) { detectRadius = 2; }
				else { detectRadius = 1; }

				if (dist <= detectRadius)
				{
					// Cooldown check
					if (m_WarnedPlayers != null && m_WarnedPlayers.TryGetValue(m.Serial, out var lastWarned))
					{
						if ((DateTime.Now - lastWarned).TotalSeconds < 30) return;
					}

					if (m_WarnedPlayers == null)
						m_WarnedPlayers = new Dictionary<Serial, DateTime>();
					m_WarnedPlayers[m.Serial] = DateTime.Now;

					// Resolve direction through localization
					string dir = GetDirectionTo(m);
					string dirLoc = StringCatalog.ResolveByKey(m.Account, "trap.dir." + dir);

					if (searching >= 100)
					{
						// Exact steps: chance-based, scales from 70% at skill 100 to 100% at skill 100+
						double stepsChance = Math.Min(1.0, (searching - 70.0) / 30.0);
						bool showSteps = Utility.RandomDouble() < stepsChance;

						string category = (HiddenTrapType > 0) ? GetCategoryName() : null;
						string catLoc = category != null ? StringCatalog.ResolveByKey(m.Account, category) : StringCatalog.ResolveByKey(m.Account, "trap.category.unknown");

						if (showSteps)
						{
							m.LocalOverheadMessage(MessageType.Emote, 0xB3E, false,
								StringCatalog.ResolveFormatByKey(m.Account, "trap.proximity.detail.steps",
									catLoc, dirLoc, dist.ToString()));
						}
						else
						{
							m.LocalOverheadMessage(MessageType.Emote, 0xB3E, false,
								StringCatalog.ResolveFormatByKey(m.Account, "trap.proximity.detail",
									catLoc, dirLoc, StringCatalog.ResolveByKey(m.Account, "trap.dist.far")));
						}
					}
					else if (searching >= 75)
					{
						string distDesc = GetDistanceDesc(dist);
						string distKey = distDesc == "very close" ? "trap.dist.veryclose" : distDesc == "close" ? "trap.dist.close" : "trap.dist.far";
						string distLoc = StringCatalog.ResolveByKey(m.Account, distKey);

						string category = (HiddenTrapType > 0) ? GetCategoryName() : null;
						string catLoc = category != null ? StringCatalog.ResolveByKey(m.Account, category) : StringCatalog.ResolveByKey(m.Account, "trap.category.unknown");
						m.LocalOverheadMessage(MessageType.Emote, 0xB3E, false,
							StringCatalog.ResolveFormatByKey(m.Account, "trap.proximity.detail",
								catLoc, dirLoc, distLoc));
					}
					else if (searching >= 50)
					{
						string distDesc = GetDistanceDesc(dist);
						string distKey = distDesc == "very close" ? "trap.dist.veryclose" : distDesc == "close" ? "trap.dist.close" : "trap.dist.far";
						string distLoc = StringCatalog.ResolveByKey(m.Account, distKey);

						m.LocalOverheadMessage(MessageType.Emote, 0xB3E, false,
							StringCatalog.ResolveFormatByKey(m.Account, "trap.proximity.warning",
								dirLoc, distLoc));
					}
					else
					{
						m.LocalOverheadMessage(MessageType.Emote, 0xB3E, false,
							StringCatalog.ResolveByKey(m.Account, "trap.proximity.basic"));
					}
				}
			}

			// Meditation detection (types 5, 7, 25) — independent of Searching skill
			// Has its own range check (2 tiles) and skill threshold (30+)
			TryMeditationDetection(m, dist);

			// Spiritualism detection (types 12, 25) — independent of Searching skill
			// Has its own range check (2 tiles) and skill threshold (30+)
			TrySpiritualismDetection(m, dist);
		}

		private string GetDirectionTo(Mobile m)
		{
			int dx = X - m.X;
			int dy = Y - m.Y;
			if (dx == 0 && dy < 0) return "north";
			if (dx > 0 && dy < 0) return "northeast";
			if (dx > 0 && dy == 0) return "east";
			if (dx > 0 && dy > 0) return "southeast";
			if (dx == 0 && dy > 0) return "south";
			if (dx < 0 && dy > 0) return "southwest";
			if (dx < 0 && dy == 0) return "west";
			if (dx < 0 && dy < 0) return "northwest";
			return "nearby";
		}

		private string GetDistanceDesc(int dist)
		{
			if (dist <= 1) return "very close";
			if (dist <= 2) return "close";
			return "some distance away";
		}

		public static string GetCategoryName(int trapType)
		{
			if (trapType == 0) return null; // unknown type
			switch (trapType)
			{
				case 1:
				case 3:
				case 4:
				case 5:
				case 7:
				case 8:
				case 10:
				case 12:
				case 13:
				case 25:
					return "trap.category.runic";
				case 14:
				case 15:
				case 17:
					return "trap.category.mechanical";
				case 6:
				case 9:
				case 21:
					return "trap.category.vented";
				case 2:
				case 20:
				case 22:
				case 23:
					return "trap.category.wired";
				case 16:
				case 18:
				case 19:
					return "trap.category.elemental";
				case 11:
				case 24:
					return "trap.category.dangerous";
				default:
					return null;
			}
		}

		private string GetCategoryName()
		{
			return GetCategoryName(HiddenTrapType);
		}

		private void TryMeditationDetection(PlayerMobile m, int dist)
		{
			if (dist > 2) return;
			if (m.Skills.Meditation.Value < 30) return;
			if (HiddenTrapType == 0) return;

			bool isDetectable = HiddenTrapType == 5 || HiddenTrapType == 7 || HiddenTrapType == 25;
			if (!isDetectable) return;

			double chance = m.Skills.Meditation.Value / 100.0;
			if (Utility.RandomDouble() > chance) return;

			string key = null;
			switch (HiddenTrapType)
			{
				case 5: key = "trap.meditation.type5"; break;
				case 7: key = "trap.meditation.type7"; break;
				case 25: key = "trap.meditation.type25"; break;
			}
			if (key != null)
				m.LocalOverheadMessage(MessageType.Emote, 0xB2D, false,
					StringCatalog.ResolveByKey(m.Account, key));
		}

		private void TrySpiritualismDetection(PlayerMobile m, int dist)
		{
			if (dist > 2) return;
			if (m.Skills.Spiritualism.Value < 30) return;
			if (HiddenTrapType == 0) return;

			bool isDetectable = HiddenTrapType == 12 || HiddenTrapType == 25;
			if (!isDetectable) return;

			double chance = m.Skills.Spiritualism.Value / 100.0;
			if (Utility.RandomDouble() > chance) return;

			string key = null;
			switch (HiddenTrapType)
			{
				case 12: key = "trap.spiritualism.type12"; break;
				case 25: key = "trap.spiritualism.type25"; break;
			}
			if (key != null)
				m.LocalOverheadMessage(MessageType.Emote, 0xB2D, false,
					StringCatalog.ResolveByKey(m.Account, key));
		}

		public override bool HandlesOnMovement{ get{ return true; } }

		private DateTime m_NextSound;	
		public DateTime NextSound{ get{ return m_NextSound; } set{ m_NextSound = value; } }

		public override void OnMovement( Mobile m, Point3D oldLocation )
		{
			if ( m is PlayerMobile pm && SeeIfTrapActive( this ) && CanSetOffTraps( m ) && Weight < 5.0 )
			{
				int dist = (int)Math.Max( Math.Abs( m.X - Location.X ), Math.Abs( m.Y - Location.Y ) );
				TryProximityDetection( pm, dist );
			}

			if( m is PlayerMobile && MySettings.S_EnableDungeonSoundEffects )
			{
				if ( DateTime.Now >= m_NextSound && Utility.InRange( m.Location, this.Location, 10 ) )
				{
					if ( Utility.RandomBool() )
					{
						int sound = HiddenChest.DungeonSounds( this );	
						m.PlaySound( sound );	
					}
					m_NextSound = (DateTime.Now + TimeSpan.FromSeconds( 60 ));	
				}
			}
		}

		private class Delete_5_Seconds : Timer
		{
			private Item m_Trap;

			public Delete_5_Seconds( Item trap ) : base( TimeSpan.FromSeconds( 5.0 ) )
			{
				Priority = TimerPriority.OneSecond;
				m_Trap = trap;
			}

			protected override void OnTick()
			{
				m_Trap.Delete();
			}
		}

		private class Delete_5_Minutes : Timer
		{
			private Item m_Trap;

			public Delete_5_Minutes( Item trap ) : base( TimeSpan.FromMinutes( 5.0 ) )
			{
				Priority = TimerPriority.OneMinute;
				m_Trap = trap;
			}

			protected override void OnTick()
			{
				m_Trap.Delete();
			}
		}
	}
}
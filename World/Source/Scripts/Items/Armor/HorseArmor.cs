using Server;
using System;
using System.Collections;
using Server.Network;
using Server.Targeting;
using Server.Prompts;
using Server.Misc;
using Server.Mobiles;

namespace Server.Items
{
	public class HorseArmor : Item
	{
		public override void ResourceChanged( CraftResource resource )
		{
			if ( !ResourceCanChange() )
				return;

			ResourceMods.Modify( this, true );
			m_Resource = resource;
			Hue = CraftResources.GetHue(m_Resource);
			Name = CraftResources.GetTradeItemFullName( this, m_Resource, false, false, "horse barding" );
			ResourceMods.Modify( this, false );
			InvalidateProperties();
		}

		public int ArmorMod;
		[CommandProperty(AccessLevel.Owner)]
		public int Armor_Mod { get { return ArmorMod; } set { ArmorMod = value; InvalidateProperties(); } }

		[Constructable]
		public HorseArmor() : base( 0x040A )
		{
			Weight = 25.0;
			Name = "horse barding";
			ArmorMod = 5;

            int chance = 0;
            double chancetest = Utility.RandomDouble();
            
            if (chancetest < 0.50 )
                chance = 3;
            else if (chancetest < 0.70)
                chance = 7;
            else if (chancetest < 0.85)
                chance = 9;
            else if (chancetest < 0.95)
                chance = 11;
            else if (chancetest >= 0.95)
                chance = 14;
            
            switch ( Utility.Random( chance ) )
            {
				case 1: Resource = CraftResource.DullCopper; break;
				case 2: Resource = CraftResource.ShadowIron; break;
				case 3: Resource = CraftResource.Copper; break;
				case 4: Resource = CraftResource.Bronze; break;
				case 5: Resource = CraftResource.Gold; break;
				case 6: Resource = CraftResource.Agapite; break;
				case 7: Resource = CraftResource.Verite; break;
				case 8: Resource = CraftResource.Valorite; break;
				case 9: Resource = CraftResource.Nepturite; break;
				case 10: Resource = CraftResource.Obsidian; break;
				case 11: Resource = CraftResource.Steel; break;
				case 12: Resource = CraftResource.Brass; break;
				case 13: Resource = CraftResource.Mithril; break;
				default: Resource = CraftResource.Iron; break;
            }
		}

		public override void OnDoubleClick( Mobile from )
		{
			Target t;

			if ( !IsChildOf( from.Backpack ) )
			{
				from.SendLocalizedMessage( 1060640 ); // The item must be in your backpack to use it.
			}
			else
			{
				from.SendMessage( "Which horse do you want to use this on?" );
				t = new HorseTarget( this );
				from.Target = t;
			}
		}

		private class HorseTarget : Target
		{
			private HorseArmor m_HorseArmor;

			public HorseTarget( HorseArmor armor ) : base( 8, false, TargetFlags.None )
			{
				m_HorseArmor = armor;
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				BaseMount horse = targeted as BaseMount;
				if (horse != null && (horse is Horse || horse is ZebraRiding) && horse.ControlMaster == from)
				{


					const int ARMORED_HORSE_ID = 587;
					if (horse.Body != ARMORED_HORSE_ID && horse.ItemID != ARMORED_HORSE_ID)
					{
						int mod = m_HorseArmor.ArmorMod;

						horse.SetStr(horse.RawStr + mod);
						horse.SetDex(horse.RawDex + mod);
						horse.SetInt(horse.RawInt + mod);

						horse.SetHits(horse.HitsMax + mod);

						horse.SetDamage(horse.DamageMin + mod, horse.DamageMax + mod);

						horse.SetResistance(ResistanceType.Physical, horse.PhysicalResistance + mod);

						horse.SetSkill(SkillName.MagicResist, horse.Skills[SkillName.MagicResist].Base + mod);
						horse.SetSkill(SkillName.Tactics, horse.Skills[SkillName.Tactics].Base + mod);
						horse.SetSkill(SkillName.FistFighting, horse.Skills[SkillName.FistFighting].Base + mod);

						horse.Body = ARMORED_HORSE_ID;
						horse.ItemID = ARMORED_HORSE_ID;
					}

					from.RevealingAction();
					from.PlaySound(0x0AA);

					horse.Hue = CraftResources.GetClr(m_HorseArmor.Resource);
					horse.Resource = m_HorseArmor.Resource;

					m_HorseArmor.Consume();
				}
				else
				{
					from.SendMessage("This armor is only for horses you own.");
				}
			}
		}

		public static void DropArmor( BaseCreature bc )
		{
			if ( bc != null && !bc.IsBonded && bc.Resource != CraftResource.None )
			{
				HorseArmor armor = new HorseArmor();
				armor.Resource = bc.Resource;
				bc.AddItem( armor );
			}
		}

		public HorseArmor( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer ); 
			writer.Write( (int) 2 ); // version
            writer.Write( ArmorMod );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader ); 
			int version = reader.ReadInt();
			ArmorMod = reader.ReadInt();

			string ArmorMaterial = null;

			if ( version < 2 )
				ArmorMaterial = reader.ReadString();

			if ( ArmorMaterial != null && version < 2 )
			{
				if ( ArmorMaterial == "Dull Copper" ){ 			Resource = CraftResource.DullCopper; }
				else if ( ArmorMaterial == "Shadow Iron" ){ 	Resource = CraftResource.ShadowIron; }
				else if ( ArmorMaterial == "Copper" ){ 			Resource = CraftResource.Copper; }
				else if ( ArmorMaterial == "Bronze" ){ 			Resource = CraftResource.Bronze; }
				else if ( ArmorMaterial == "Gold" ){ 			Resource = CraftResource.Gold; }
				else if ( ArmorMaterial == "Agapite" ){ 		Resource = CraftResource.Agapite; }
				else if ( ArmorMaterial == "Verite" ){ 			Resource = CraftResource.Verite; }
				else if ( ArmorMaterial == "Valorite" ){ 		Resource = CraftResource.Valorite; }
				else if ( ArmorMaterial == "Nepturite" ){ 		Resource = CraftResource.Nepturite; }
				else if ( ArmorMaterial == "Obsidian" ){ 		Resource = CraftResource.Obsidian; }
				else if ( ArmorMaterial == "Steel" ){ 			Resource = CraftResource.Steel; }
				else if ( ArmorMaterial == "Brass" ){ 			Resource = CraftResource.Brass; }
				else if ( ArmorMaterial == "Mithril" ){ 		Resource = CraftResource.Mithril; }
				else if ( ArmorMaterial == "Xormite" ){ 		Resource = CraftResource.Xormite; }
				else if ( ArmorMaterial == "Dwarven" ){ 		Resource = CraftResource.Dwarven; }
				else {											Resource = CraftResource.Iron; }
				ArmorMaterial = null;
			}
		}
	}
}
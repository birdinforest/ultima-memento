namespace Server.Items
{
    public class Artifact_Indecency : GiftStuddedChest
	{
		public override int InitMinHits{ get{ return 80; } }
		public override int InitMaxHits{ get{ return 160; } }

        public override int BasePhysicalResistance{ get{ return 3; } }
        public override int BaseColdResistance{ get{ return 12; } }
        public override int BaseFireResistance{ get{ return 12; } }
        public override int BaseEnergyResistance{ get{ return 13; } }
        public override int BasePoisonResistance{ get{ return 9; } }

        [Constructable]
        public Artifact_Indecency()
        {
            Name = "Indecency";
            Hue = 2075;
            Attributes.AttackChance = 5;
            Attributes.DefendChance = 5;
            Attributes.Luck = 205;
            Attributes.SpellDamage = 5;
            ArmorAttributes.MageArmor = 1;
            ArmorAttributes.SelfRepair = 2;
            Attributes.LowerRegCost = 15;
			ArtifactLevel = ArtifactLevel.StandardArtefact;
			Server.Misc.Arty.ArtySetup( this, 15, "" );
		}

        public Artifact_Indecency(Serial serial) : base( serial )
        {
        }

        public override void Serialize( GenericWriter writer )
        {
            base.Serialize( writer );
            writer.Write( (int) 1 );
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize( reader );
            int version = reader.ReadInt();
            if (version < 1)
            {
                Attributes.BonusStr = 0;
                Attributes.BonusInt = 0;
                Attributes.BonusDex = 0;
                Attributes.AttackChance = 5;
                Attributes.DefendChance = 5;
                Attributes.LowerManaCost = 0;
                ArmorAttributes.SelfRepair = 2;
                Attributes.LowerRegCost = 15;
            }
        }
    }
}

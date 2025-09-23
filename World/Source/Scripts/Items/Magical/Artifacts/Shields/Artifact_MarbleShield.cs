namespace Server.Items
{
    public class Artifact_MarbleShield : GiftHeaterShield
	{
		public override int InitMinHits{ get{ return 80; } }
		public override int InitMaxHits{ get{ return 160; } }

        public override int BasePhysicalResistance{ get{ return 12; } }
        public override int BaseColdResistance{ get{ return 4; } }
        public override int BaseFireResistance{ get{ return 17; } }
        public override int BaseEnergyResistance{ get{ return 13; } }
        public override int BasePoisonResistance{ get{ return 6; } }

        [Constructable]
        public Artifact_MarbleShield()
        {
            Name = "Gargoyle Shield";
            Hue = 2961;
            StrRequirement = 105;
            Attributes.BonusDex = 10;
            Attributes.RegenHits = 3;
            Attributes.AttackChance = 10;
            Attributes.DefendChance = 10;
            Attributes.CastSpeed = 1;
            Attributes.CastRecovery = 1;
            Attributes.SpellChanneling = 1;
			ArtifactLevel = ArtifactLevel.StandardArtefact;
			Server.Misc.Arty.ArtySetup( this, 11, "" );
		}

        public Artifact_MarbleShield(Serial serial) : base( serial )
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
                Attributes.RegenHits = 3;
                Attributes.AttackChance = 10;
                Attributes.DefendChance = 10;
                Attributes.Luck = 0;
                ArmorAttributes.SelfRepair = 0;
            }
        }
    }
}

namespace Server.Items
{
    public class AutoResPotion : Item
    {
		public override string DefaultDescription{ get{ return "Drink this potion while you are still in the land of the living. If you reach an untimely end, you will automatically be resurrected within 30 seconds. It is best to guide your spirit to a safe place before that occurs, or you could suffer the same fate again."; } }

		public override Catalogs DefaultCatalog{ get{ return Catalogs.Potion; } }

        [Constructable]
        public AutoResPotion() : base(0x0E0F) 
        {
            Name = "Potion Of Rebirth";
            LootType = LootType.Blessed;
			Weight = 1.0;
			Hue = 0x494;
			Built = true;
        }

        public AutoResPotion(Serial serial): base(serial)
        {
		}
		
		public override void OnDoubleClick( Mobile from )
		{
			if(!from.Alive)
				return;

			if ( !IsChildOf( from.Backpack ) ) 
			{
				from.SendMessage( "This must be in your backpack to use." );
				return;
			}

			if ( SoulOrb.FindActive( from ) != null )
			{
				from.SendMessage("The spirits watch you already.");
				return;
			}

			var orb = SoulOrb.Create( from, SoulOrbType.Default );
			if ( orb != null )
			{
				from.SendMessage( "You feel the spirits watching you, awaiting to send you back to your body." );
				orb.Location = Location;
				Delete();
			}
		}

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write( (int) 1 ); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

			if (version == 0)
			{
				var m_Delay = reader.ReadTimeSpan();
				var m_Charges = reader.ReadInt();
				Hue = 0x494;
				Built = true;
			}
        }
    }
}
namespace Server.Engines.Harvest
{
    public class RichVeinSpawnerPersistent : RichVeinSpawner
    {
        [Constructable]
        public RichVeinSpawnerPersistent() : base()
        {
            Name = "Rich Vein Spawner (Persistent)";
        }

        public RichVeinSpawnerPersistent(Serial serial) : base(serial)
        {
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }
    }
}
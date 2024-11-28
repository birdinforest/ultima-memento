namespace Server.Engines.MLQuests
{
    public interface IDeserializable
    {
        void Deserialize(GenericReader reader);
    }
}
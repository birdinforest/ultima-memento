namespace Server.Engines.Craft
{
    public class BulkCraftContext
    {
        public int Success { get; set; }
        public int Exceptional { get; set; }
        public int Fail { get; set; }
        public int Current { get; set; }
        public int Amount { get; set; }
    }
}
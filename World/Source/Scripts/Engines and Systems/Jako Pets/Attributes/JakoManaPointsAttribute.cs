namespace Custom.Jerbal.Jako
{
    class JakoManaPointsAttribute : JakoBaseAttribute
    {
        public override double CapScale { get { return 1.25; } }
        public override uint AttributesGiven { get { return 10; } }
        public override uint Cap { get { return 550; } }
        public override uint PointsTaken { get { return 1; } }

        public override uint GetStat(Server.Mobiles.BaseCreature bc)
        {
            return (uint)bc.ManaMaxSeed;
        }

        protected override void SetStat(Server.Mobiles.BaseCreature bc, uint value)
        {
            bc.ManaMaxSeed = (int)value;
        }

        public override string ToString()
        {
            return "Mana";
        }
    }
}

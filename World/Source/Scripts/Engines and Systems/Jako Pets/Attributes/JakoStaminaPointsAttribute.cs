namespace Custom.Jerbal.Jako
{
    class JakoStaminaPointsAttribute : JakoBaseAttribute
    {
        public override double CapScale { get { return 1.5; } }
        public override uint AttributesGiven { get { return 15; } }
        public override uint Cap { get { return 750; } }
        public override uint PointsTaken { get { return 1; } }

        public override uint GetStat(Server.Mobiles.BaseCreature bc)
        {
            return (uint)bc.StamMaxSeed;
        }

        protected override void SetStat(Server.Mobiles.BaseCreature bc, uint value)
        {
            bc.StamMaxSeed = (int)value;
        }

        public override string ToString()
        {
            return "Stamina";
        }
    }
}

namespace Custom.Jerbal.Jako
{
    class JakoResistEnergyAttribute : JakoBaseAttribute
    {
        public override double CapScale { get { return 1.25; } }
        public override uint AttributesGiven { get { return 1; } }
        public override uint Cap { get { return 65; } }
        public override uint PointsTaken { get { return 2; } }

        public override uint GetStat(Server.Mobiles.BaseCreature bc)
        {
            return (uint)bc.BaseEnergyResistance;
        }

        protected override void SetStat(Server.Mobiles.BaseCreature bc, uint value)
        {
            bc.SetResistance(Server.ResistanceType.Energy, (int)value);
        }

        public override string ToString()
        {
            return "Energy Resistance";
        }
    }
}

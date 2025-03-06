namespace Custom.Jerbal.Jako
{
    class JakoResistFireAttribute : JakoBaseAttribute
    {
        public override double CapScale { get { return 1.25; } }
        public override uint AttributesGiven { get { return 2; } }
        public override uint Cap { get { return 65; } }
        public override uint PointsTaken { get { return 5; } }

        public override uint GetStat(Server.Mobiles.BaseCreature bc)
        {
            return (uint)bc.FireResistance;
        }

        protected override void SetStat(Server.Mobiles.BaseCreature bc, uint toThis)
        {
            bc.SetResistance(Server.ResistanceType.Fire, (int)toThis);
        }

        public override string ToString()
        {
            return "Fire Resistance";
        }
    }
}

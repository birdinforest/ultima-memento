using System;
using Server;
using Server.Mobiles;

namespace Custom.Jerbal.Jako
{
    public abstract class JakoBaseAttribute : IComparable
    {
        [CommandProperty(AccessLevel.GameMaster)]
        public uint TraitsGiven { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public virtual uint IncreasedBy { get { return TraitsGiven * AttributesGiven / PointsTaken; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public abstract uint Cap { get; } // The absolute cap

        [CommandProperty(AccessLevel.GameMaster)]
        public abstract double CapScale { get; }

        public abstract uint GetStat(BaseCreature bc);

        protected abstract void SetStat(BaseCreature bc, uint toThis);

        [CommandProperty(AccessLevel.GameMaster)]
        public abstract uint AttributesGiven { get; }

        [CommandProperty(AccessLevel.GameMaster)]
        public abstract uint PointsTaken { get; }

        public bool CanAfford(BaseCreature bc)
        {
            return PointsTaken <= bc.Traits;
        }

        public virtual uint MaxBonus(BaseCreature bc)
        {
            uint currentValue = GetStat(bc);
            if (currentValue >= Cap) return currentValue; // Already exceeds cap

            double proposedValue = (currentValue - IncreasedBy) * CapScale;
            
            return (uint) Math.Min(Cap, proposedValue);
        }

        public bool CanAddBonus(BaseCreature bc)
        {
            uint currentValue = GetStat(bc) + AttributesGiven;

            return currentValue < MaxBonus(bc);
        }

        public virtual string ApplyBonus(BaseCreature bc)
        {
            if (!CanAddBonus(bc)) return "That would place this creature's stat above maximum.";
            if (!CanAfford(bc)) return "You do not have enough traits to do that.";
            if (IncBonus(bc, AttributesGiven)) return "Attribute adjusted.";

            return "Error in IncBonus."; //Hopefully no one ever sees this, if they do, we know where it is.
        }

        public virtual bool IncBonus(BaseCreature bc, uint amount)
        {
            return SetBonus(bc, GetStat(bc) + amount);
        }

        new public abstract string ToString();

        public virtual bool SetBonus(BaseCreature bc, uint value)
        {
            if (value > MaxBonus(bc)) return false;

            uint oldTraits = TraitsGiven;
            uint delta = value - GetStat(bc);
            TraitsGiven += (delta / AttributesGiven) * PointsTaken;
            bc.Traits -= TraitsGiven - oldTraits;
            SetStat(bc, value);
            return true;
        }

        public virtual void Serialize(GenericWriter writer)
        {
            writer.Write(1);//Version
            writer.Write(TraitsGiven);
        }

        public virtual void Deserialize(GenericReader reader)
        {
            int version = reader.ReadInt();
            switch (version)
            {
                case 1:
                    {
                        TraitsGiven = reader.ReadUInt();
                        break;
                    }

            }
        }

        #region IComparable Members

        public int CompareTo(object obj)
        {
            return (int)(TraitsGiven - ((JakoBaseAttribute)obj).TraitsGiven);
        }

        #endregion
    }
}

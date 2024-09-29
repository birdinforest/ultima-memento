using System;
using Server.Items;

namespace Server.SkillHandlers
{
    public class Healing
    {
        public static void Initialize()
        {
            SkillInfo.Table[(int)SkillName.Healing].Callback = new SkillUseCallback(OnUse);
        }

        public static TimeSpan OnUse(Mobile m)
        {
            if (m.Hunger < 6)
            {
                m.SendMessage("You are starving to death and cannot do that!");
            }
            else if (Server.Items.MortalStrike.IsWounded(m))
            {
                m.SendLocalizedMessage(1005000); // You cannot heal yourself in your current state.
            }
            else if (m.Hits < m.HitsMax || m.Poisoned || BleedAttack.IsBleeding(m))
            {
                var isBleeding = BleedAttack.IsBleeding(m);
                if (isBleeding || m.Poisoned)
                {
                    if (isBleeding == m.Poisoned) m.SendMessage("You feel a little healthier.");
                    else if (m.Poisoned) m.SendMessage("The infection begins to clear.");
                    else m.SendMessage("You work quickly to stem the bleeding!");

                    if (m.Poisoned) m.CurePoison(m);
                    if (isBleeding) BleedAttack.EndBleed(m, false);
                }
                else
                {
                    var skillLevel = m.Skills[SkillName.Healing].Value;
                    var amount = 2 + (int)skillLevel / 5;
                    if (100 < skillLevel) amount += (int)(3 * (skillLevel - 100) / 5);

                    if (!m.CheckSkill(SkillName.Healing, -50, 99.9))
                    {
                        m.SendMessage("You are distracted, but heal some wounds.");
                        amount = (int)(amount * 0.75);
                    }
                    else
                    {
                        m.SendMessage("You focus intently and heal your wounds.");
                    }

                    m.Heal(amount, m, false);
                }

                return TimeSpan.FromSeconds(15.0);
            }
            else
            {
                m.SendMessage("You already feel healthy.");
            }

            return TimeSpan.FromSeconds(1.0);
        }
    }
}

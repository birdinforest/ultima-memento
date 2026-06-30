using System;
using Server.Localization;
using Server.Spells.Chivalry;
using Server.Targeting;

namespace Server.Items
{
    public abstract class ConsecrateItemBase : Item
    {
        private const int DEFAULT_HUE = 0x973;
        private int _uses;
        protected bool IsLegacyItem;

        public ConsecrateItemBase(Serial serial) : base(serial)
        {
        }

        protected ConsecrateItemBase(int uses, int itemID) : base(itemID)
        {
            Weight = 1.0;
            Hue = DEFAULT_HUE;
            Uses = uses;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Uses
        {
            get { return _uses; }
            set { _uses = value; InvalidateProperties(); }
        }

        public void Apply(Mobile from, object targeted)
        {
            if (Deleted) { return; }

            var weapon = targeted as BaseWeapon;
            if (!Validate(from, weapon)) return;
            if (!ApplyBonus(from, weapon)) return;

            if (--Uses < 1)
            {
                Delete();
                from.SendMessage(32, StringCatalog.Resolve(from.Account, "You use the last of the magic"));
            }
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            list.Add(1060584, Uses.ToString()); // uses remaining: ~1_val~
            if (BuildingPropertyListLocale != null)
                AddLocalizedProperty(list, "prop.sharpening.consecrate");
            else
                list.Add("Consecrates a weapon for 4 hours");
        }

        public override void OnDoubleClick(Mobile from)
        {
            PromptForTarget(from, StringCatalog.Resolve(from.Account, "Which weapon would you like to consecrate?"));
        }

        protected bool ApplyBonus(Mobile from, BaseWeapon weapon)
        {
            var duration = TimeSpan.FromHours(4);

            ConsecrateWeaponSpell.Apply(from, weapon, duration, false);

            weapon.ConsecrateExpiry = DateTime.Now + duration;
            weapon.InvalidateProperties();

            PlayConsecrateEffects(from, weapon);
            SendConsecrateMessage(from);

            return true;
        }

        protected virtual void PlayConsecrateEffects(Mobile from, BaseWeapon weapon)
        {
            int itemID, soundID;

            switch (weapon.Skill)
            {
                case SkillName.Bludgeoning: itemID = 0xFB4; soundID = 0x232; break;
                case SkillName.Marksmanship: itemID = 0x13B1; soundID = 0x145; break;
                default: itemID = 0xF5F; soundID = 0x56; break;
            }

            from.PlaySound(0x20C);
            from.PlaySound(soundID);
            from.FixedParticles(0x3779, 1, 30, 9964, 3, 3, EffectLayer.Waist);

            IEntity start = new Entity(Serial.Zero, new Point3D(from.X, from.Y, from.Z), from.Map);
            IEntity end = new Entity(Serial.Zero, new Point3D(from.X, from.Y, from.Z + 50), from.Map);
            Effects.SendMovingParticles(start, end, itemID, 1, 0, false, false, 33, 3, 9501, 1, 0, EffectLayer.Head, 0x100);
        }

        private void SendConsecrateMessage(Mobile from)
        {
            from.SendMessage(68, StringCatalog.ResolveByKey(from.Account, "prop.consecrate.stone.success"));
        }

        protected void PromptForTarget(Mobile from, string message)
        {
            if (!IsChildOf(from.Backpack))
            {
                from.SendMessage(StringCatalog.Resolve(from.Account, "This must be in your backpack to use"));
                return;
            }

            from.SendMessage(message);
            from.Target = new InternalTarget(this);
        }

        protected virtual bool Validate(Mobile from, BaseWeapon weapon)
        {
            if (weapon == null || weapon.Deleted) return false;

            if (!weapon.IsChildOf(from.Backpack))
            {
                from.SendMessage(32, StringCatalog.Resolve(from.Account, "This must be in your backpack"));
                return false;
            }

            return true;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)3); // version
            writer.Write((int)_uses);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
            IsLegacyItem = version < 2;
            _uses = reader.ReadInt();

            switch(version)
            {
                case 2:
                    if (Hue != DEFAULT_HUE) Hue = DEFAULT_HUE;
                    break;
            }
        }

        private class InternalTarget : Target
        {
            private readonly ConsecrateItemBase _itemBase;

            public InternalTarget(ConsecrateItemBase itemBase) : base(1, false, TargetFlags.None)
            {
                _itemBase = itemBase;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                _itemBase.Apply(from, targeted);
            }
        }
    }
}

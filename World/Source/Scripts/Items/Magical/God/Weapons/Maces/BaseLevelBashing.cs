using System;
using Server.ContextMenus;
using System.Collections.Generic;

namespace Server.Items
{
    public abstract class BaseLevelBashing : BaseBashing, ILevelable
    {
        /* These private variables store the exp, level, and *
         * points for the item */
        private int m_Experience;
        private int m_Level;
        private int m_Points;
        private int m_MaxLevel;

        public BaseLevelBashing(int itemID)
            : base(itemID)
        {
            MaxLevel = LevelItems.DefaultMaxLevel;
			LootType = LootType.Blessed;

            /* Invalidate the level and refresh the item props
             * Extremely important to call this method */
            LevelItemManager.InvalidateLevel(this);
			ArtifactLevel = ArtifactLevel.LegendaryArtefact;
        }

		public override bool DisplayLootType{ get{ return false; } }

        public BaseLevelBashing(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)2);

            // Version 1
            writer.Write(m_MaxLevel);

            // Version 0
            // DONT FORGET TO SERIALIZE LEVEL, EXPERIENCE, AND POINTS
            writer.Write(m_Experience);
            writer.Write(m_Level);
            writer.Write(m_Points);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
				case 2:
                case 1:
                    {
                        m_MaxLevel = reader.ReadInt();

                        goto case 0;
                    }
                case 0:
                    {
                        // DONT FORGET TO DESERIALIZE LEVEL, EXPERIENCE, AND POINTS
                        m_Experience = reader.ReadInt();
                        m_Level = reader.ReadInt();
                        m_Points = reader.ReadInt();

                        break;
                    }
            }

			if (version == 1)
			{
				m_MaxLevel = LevelItems.MaxLevelsCap;
				Timer.DelayCall(TimeSpan.Zero, () =>
				{
					LevelItemManager.ExtractExperienceToken(this);
					Points = 0;
					var oldWeaponDamage = Attributes.WeaponDamage;
					var oldLuck = Attributes.Luck;
					ResetAllAttributes();
					Attributes.WeaponDamage = oldWeaponDamage; // Restore old value in case they used sharpening stones
					Attributes.Luck = oldLuck; // Restore in case they used lucky horse shoes
				});
			}
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            /* Display level in the properties context menu.
             * Will display experience as well, if DisplayExpProp.
             * is set to true in LevelItemManager.cs */
            list.Add(1060658, "Level\t{0}", Level);
			if (LevelItems.DisplayExpProp)
				list.Add(1060659, "Experience\t{0}", Experience);
        }

        public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
        {
            base.GetContextMenuEntries(from, list);

            /* Context Menu Entry to display the gump w/
             * all info */

            list.Add(new LevelInfoEntry(from, this, AttributeCategory.Melee));
        }

        // ILevelable Members that MUST be implemented
        #region ILevelable Members

        // This one will return our private m_MaxLevel variable.
        [CommandProperty(AccessLevel.GameMaster)]
        public int MaxLevel
        {
            get
            {
				return m_MaxLevel < 1
					? 1 // Min
					: m_MaxLevel > LevelItems.MaxLevelsCap
						? LevelItems.MaxLevelsCap // Max
						: m_MaxLevel; // Actual
            }
            set
            {
				m_MaxLevel = value;
            }
        }

        // This one will return our private m_Experience variable.
        [CommandProperty(AccessLevel.GameMaster)]
        public int Experience
        {
            get
            {
                return m_Experience;
            }
            set
            {
                m_Experience = value;

                // This keeps gms from setting the level to an outrageous value
                if (m_Experience > LevelItemManager.ExpTable[LevelItems.MaxLevelsCap - 1])
                    m_Experience = LevelItemManager.ExpTable[LevelItems.MaxLevelsCap - 1];

                // Anytime exp is changed, call this method
                LevelItemManager.InvalidateLevel(this);
            }
        }

        // This one will return our private m_Level variable.
        [CommandProperty(AccessLevel.GameMaster)]
        public int Level
        {
            get
            {
				return m_Level < 1
					? 1 // Min
					: m_Level > MaxLevel
						? MaxLevel // Max
						: m_Level; // Actual
            }
            set
            {
				m_Level = value;
            }
        }

        // This one will return our private m_Points variable.
        [CommandProperty(AccessLevel.GameMaster)]
        public int Points
        {
            get
            {
                return m_Points;
            }
            set
            {
                //Sets new points.
                m_Points = value;
            }
        }
        #endregion
    }
}
using System;
using System.Collections;
using Server.Multis;
using Server.Mobiles;
using Server.Network;
using System.Collections.Generic;
using Server.ContextMenus;
using Server.Localization;

namespace Server.Items
{
	[Flipable( 0x1C0E, 0x1C0F )]
    public class PickBoxEasy : LockableContainer
    {
		public override bool IsContentLocalized => true;
		public override string DisplayNameLocalizationKey => "item.trade.name.locked.box";

		public override string DefaultDescription{ get{ return StringCatalog.Resolve( null, "These are locked boxes that thieves use to practice their lockpicking skills. They require a single skill point in lockpicking, and can help you learn up to 25." ); } }

		public override string InfoDataLocalizationKey { get { return "prop.trade.itemdesc.pickbox.easy"; } }

		public override void AddNameProperties( ObjectPropertyList list )
		{
			string saved = m_InfoText1;

			if ( BuildingPropertyListLocale != null )
				m_InfoText1 = null;

			base.AddNameProperties( list );

			if ( BuildingPropertyListLocale != null )
				AddLocalizedProperty( list, "prop.infotext.pickbox.easy" );

			m_InfoText1 = saved;
		}

        [Constructable]
        public PickBoxEasy(): base( 0x1C0E )
        {
			Name = "Locked Box";
			InfoText1 = "Easy Lock";
			Hue = 0xB61;
            Locked = true;
            LockLevel = 1;
            MaxLockLevel = 25;
            RequiredSkill = 1;
            Weight = 4.0;
			Movable = false;
        }

        public override void LockPick(Mobile from)
        {
            this.Locked = true;
            from.SendMessage(StringCatalog.ResolveByKey( from.Account, "prop.trade.pickbox.relocks" ));
        }

        public PickBoxEasy(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
	/////////////////////////////////////////////////////////////////////////////////////////
	[Flipable( 0x1C0E, 0x1C0F )]
    public class PickBoxNormal : LockableContainer
    {
		public override bool IsContentLocalized => true;
		public override string DisplayNameLocalizationKey => "item.trade.name.locked.box";

		public override string DefaultDescription{ get{ return StringCatalog.Resolve( null, "These are locked boxes that thieves use to practice their lockpicking skills. They require a 20 lockpicking, and can help you learn up to 35." ); } }

		public override string InfoDataLocalizationKey { get { return "prop.trade.itemdesc.pickbox.normal"; } }

		public override void AddNameProperties( ObjectPropertyList list )
		{
			string saved = m_InfoText1;

			if ( BuildingPropertyListLocale != null )
				m_InfoText1 = null;

			base.AddNameProperties( list );

			if ( BuildingPropertyListLocale != null )
				AddLocalizedProperty( list, "prop.infotext.pickbox.normal" );

			m_InfoText1 = saved;
		}

        [Constructable]
        public PickBoxNormal(): base( 0x1C0E )
        {
			Name = "Locked Box";
			InfoText1 = "Normal Lock";
			Hue = 0xB61;
            Locked = true;
            LockLevel = 20;
            MaxLockLevel = 35;
            RequiredSkill = 20;
            Weight = 4.0;
			Movable = false;
        }

        public override void LockPick(Mobile from)
        {
            this.Locked = true;
            from.SendMessage(StringCatalog.ResolveByKey( from.Account, "prop.trade.pickbox.relocks" ));
        }

        public PickBoxNormal(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
	/////////////////////////////////////////////////////////////////////////////////////////
	[Flipable( 0x1C0E, 0x1C0F )]
    public class PickBoxDifficult : LockableContainer
    {
		public override bool IsContentLocalized => true;
		public override string DisplayNameLocalizationKey => "item.trade.name.locked.box";

		public override string DefaultDescription{ get{ return StringCatalog.Resolve( null, "These are locked boxes that thieves use to practice their lockpicking skills. They require a 30 lockpicking, and can help you learn up to 45." ); } }

		public override string InfoDataLocalizationKey { get { return "prop.trade.itemdesc.pickbox.difficult"; } }

		public override void AddNameProperties( ObjectPropertyList list )
		{
			string saved = m_InfoText1;

			if ( BuildingPropertyListLocale != null )
				m_InfoText1 = null;

			base.AddNameProperties( list );

			if ( BuildingPropertyListLocale != null )
				AddLocalizedProperty( list, "prop.infotext.pickbox.difficult" );

			m_InfoText1 = saved;
		}

        [Constructable]
        public PickBoxDifficult(): base( 0x1C0E )
        {
			Name = "Locked Box";
			InfoText1 = "Difficult Lock";
			Hue = 0xB61;
            Locked = true;
            LockLevel = 30;
            MaxLockLevel = 45;
            RequiredSkill = 30;
            Weight = 4.0;
			Movable = false;
        }

        public override void LockPick(Mobile from)
        {
            this.Locked = true;
            from.SendMessage(StringCatalog.ResolveByKey( from.Account, "prop.trade.pickbox.relocks" ));
        }

        public PickBoxDifficult(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
	/////////////////////////////////////////////////////////////////////////////////////////
	[Flipable( 0x1C0E, 0x1C0F )]
    public class PickBoxChallenging : LockableContainer
    {
		public override bool IsContentLocalized => true;
		public override string DisplayNameLocalizationKey => "item.trade.name.locked.box";

		public override string DefaultDescription{ get{ return StringCatalog.Resolve( null, "These are locked boxes that thieves use to practice their lockpicking skills. They require a 40 lockpicking, and can help you learn up to 55." ); } }

		public override string InfoDataLocalizationKey { get { return "prop.trade.itemdesc.pickbox.challenging"; } }

		public override void AddNameProperties( ObjectPropertyList list )
		{
			string saved = m_InfoText1;

			if ( BuildingPropertyListLocale != null )
				m_InfoText1 = null;

			base.AddNameProperties( list );

			if ( BuildingPropertyListLocale != null )
				AddLocalizedProperty( list, "prop.infotext.pickbox.challenging" );

			m_InfoText1 = saved;
		}

        [Constructable]
        public PickBoxChallenging(): base( 0x1C0E )
        {
			Name = "Locked Box";
			InfoText1 = "Challenging Lock";
			Hue = 0xB61;
            Locked = true;
            LockLevel = 40;
            MaxLockLevel = 55;
            RequiredSkill = 40;
            Weight = 4.0;
			Movable = false;
        }

        public override void LockPick(Mobile from)
        {
            this.Locked = true;
            from.SendMessage(StringCatalog.ResolveByKey( from.Account, "prop.trade.pickbox.relocks" ));
        }

        public PickBoxChallenging(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
	/////////////////////////////////////////////////////////////////////////////////////////
	[Flipable( 0x1C0E, 0x1C0F )]
    public class PickBoxHard : LockableContainer
    {
		public override bool IsContentLocalized => true;
		public override string DisplayNameLocalizationKey => "item.trade.name.locked.box";

		public override string DefaultDescription{ get{ return StringCatalog.Resolve( null, "These are locked boxes that thieves use to practice their lockpicking skills. They require a 50 lockpicking, and can help you learn up to 65." ); } }

		public override string InfoDataLocalizationKey { get { return "prop.trade.itemdesc.pickbox.hard"; } }

		public override void AddNameProperties( ObjectPropertyList list )
		{
			string saved = m_InfoText1;

			if ( BuildingPropertyListLocale != null )
				m_InfoText1 = null;

			base.AddNameProperties( list );

			if ( BuildingPropertyListLocale != null )
				AddLocalizedProperty( list, "prop.infotext.pickbox.hard" );

			m_InfoText1 = saved;
		}

        [Constructable]
        public PickBoxHard(): base( 0x1C0E )
        {
			Name = "Locked Box";
			InfoText1 = "Hard Lock";
			Hue = 0xB61;
            Locked = true;
            LockLevel = 50;
            MaxLockLevel = 65;
            RequiredSkill = 50;
            Weight = 4.0;
			Movable = false;
        }

        public override void LockPick(Mobile from)
        {
            this.Locked = true;
            from.SendMessage(StringCatalog.ResolveByKey( from.Account, "prop.trade.pickbox.relocks" ));
        }

        public PickBoxHard(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}
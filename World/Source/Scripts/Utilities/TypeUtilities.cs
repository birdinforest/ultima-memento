using System;
using Server.Items;

namespace Server.Utilities
{
    public static class TypeUtilities
    {
        public static bool IsAnyTypeOrDerived(Type type, params Type[] types)
        {
            if (type == null) return false;

            foreach (var t in types)
            {
                if (t.IsAssignableFrom(type)) return true;
            }

            return false;
        }

        public static bool IsTypeOrDerived<T>(Type type)
        {
            if (type == null) return false;

            return typeof(T).IsAssignableFrom(type);
        }

        public static bool IsExceptionalEquipmentType(Type type)
        {
            if (type == null) return false;

            return IsAnyTypeOrDerived(
                type,
                typeof(BaseArmor),
                typeof(BaseWeapon),
                typeof(BaseClothing),
                typeof(BaseInstrument)
            );
        }
    }
}
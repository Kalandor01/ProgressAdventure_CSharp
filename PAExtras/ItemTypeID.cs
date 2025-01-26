using System.Reflection;

namespace PAExtras
{
    /// <summary>
    /// Struct for creating enum-like behaviour, for the item types.<br/>
    /// By yoyo <a href="https://stackoverflow.com/a/24969525">LINK</a>
    /// </summary>
    internal readonly struct ItemTypeID
    {
        public static readonly ItemTypeID none;

        public ItemTypeID this[int childID]
        {
            get { return new ItemTypeID(mID << 8 | (uint)childID); }
        }

        public ItemTypeID Super
        {
            get { return new ItemTypeID(mID >> 8); }
        }

        /// <summary>
        /// Returns if the super type is a parrent type of this type.
        /// </summary>
        /// <param name="super">The parrent type</param>
        public bool IsA(ItemTypeID super)
        {
            return this != none && (Super == super || Super.IsA(super));
        }

        public static implicit operator ItemTypeID(int id)
        {
            if (id == 0)
            {
                throw new InvalidCastException("top level id cannot be 0");
            }
            return new ItemTypeID((uint)id);
        }

        public static bool operator ==(ItemTypeID a, ItemTypeID b)
        {
            return a.mID == b.mID;
        }

        public static bool operator !=(ItemTypeID a, ItemTypeID b)
        {
            return a.mID != b.mID;
        }

        public override bool Equals(object? obj)
        {
            if (obj is ItemTypeID id)
                return id.mID == mID;
            else
                return false;
        }

        public override int GetHashCode()
        {
            return (int)mID;
        }

        private ItemTypeID(uint id)
        {
            mID = id;
        }

        public readonly uint mID;

        #region ToString override
        public override string? ToString()
        {
            return ToString(typeof(ItemType));
        }

        /// <summary>
        /// <inheritdoc cref="ToString"/>
        /// </summary>
        /// <param name="type">The base type.</param>
        public string? ToString(Type type)
        {
            foreach (var field in type.GetFields(BindingFlags.GetField | BindingFlags.Public | BindingFlags.Static))
            {
                if (field.FieldType == typeof(ItemTypeID) && Equals(field.GetValue(null)))
                {
                    return string.Format("{0}.{1}", type.ToString().Replace('+', '.'), field.Name);
                }
            }

            foreach (var nestedType in type.GetNestedTypes())
            {
                string? asNestedType = ToString(nestedType);
                if (asNestedType != null)
                {
                    return asNestedType;
                }
            }

            return null;
        }
        #endregion
    }
}

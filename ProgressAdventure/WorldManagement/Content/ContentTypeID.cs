using System.Reflection;

namespace ProgressAdventure.WorldManagement.Content
{
    /// <summary>
    /// Struct for creating enum-like behaviour, for the content types.<br/>
    /// By yoyo <a href="https://stackoverflow.com/a/24969525">LINK</a>
    /// </summary>
    public readonly struct ContentTypeID
    {
        public static readonly ContentTypeID none;

        public ContentTypeID this[int childID]
        {
            get { return new ContentTypeID(mID << 8 | (uint)childID); }
        }

        public ContentTypeID Super
        {
            get { return new ContentTypeID(mID >> 8); }
        }

        /// <summary>
        /// Returns if the super type is a parrent type of this type.
        /// </summary>
        /// <param name="super">The parrent type</param>
        public bool IsA(ContentTypeID super)
        {
            return this != none && (Super == super || Super.IsA(super));
        }

        public static implicit operator ContentTypeID(int id)
        {
            if (id == 0)
            {
                throw new InvalidCastException("top level id cannot be 0");
            }
            return new ContentTypeID((uint)id);
        }

        public static bool operator ==(ContentTypeID a, ContentTypeID b)
        {
            return a.mID == b.mID;
        }

        public static bool operator !=(ContentTypeID a, ContentTypeID b)
        {
            return a.mID != b.mID;
        }

        public override bool Equals(object? obj)
        {
            if (obj is ContentTypeID id)
                return id.mID == mID;
            else
                return false;
        }

        public override int GetHashCode()
        {
            return (int)mID;
        }

        private ContentTypeID(uint id)
        {
            mID = id;
        }

        public readonly uint mID;

        #region ToString override
        public override string? ToString()
        {
            return ToString(typeof(ContentType));
        }

        /// <summary>
        /// <inheritdoc cref="ToString"/>
        /// </summary>
        /// <param name="type">The base type.</param>
        public string? ToString(Type type)
        {
            foreach (var field in type.GetFields(BindingFlags.GetField | BindingFlags.Public | BindingFlags.Static))
            {
                if (field.FieldType == typeof(ContentTypeID) && Equals(field.GetValue(null)))
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

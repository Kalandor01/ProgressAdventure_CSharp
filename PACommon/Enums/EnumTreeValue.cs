namespace PACommon.Enums
{
    /// <summary>
    /// Represents an enum value.
    /// </summary>
    /// <typeparam name="TEnum">The type of the enum.</typeparam>
    public class EnumTreeValue<TEnum> : EnumTreeValueBase
        where TEnum : AdvancedEnumTree<TEnum>
    {
        internal EnumTreeValue(int[] indexes, string fullName, string name)
            : base(indexes, fullName, name) { }

        public static bool operator ==(EnumTreeValue<TEnum> first, EnumTreeValue<TEnum> second)
        {
            return first.Equals(second);
        }

        public static bool operator !=(EnumTreeValue<TEnum> first, EnumTreeValue<TEnum> second)
        {
            return !first.Equals(second);
        }

        public override bool Equals(object? obj)
        {
            if (
                obj == null ||
                obj is not EnumTreeValue<TEnum> enumValue ||
                enumValue.FullName != FullName
            )
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return (typeof(TEnum), FullName).GetHashCode();
        }
    }
}

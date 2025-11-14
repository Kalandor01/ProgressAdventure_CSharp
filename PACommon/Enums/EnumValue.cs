namespace PACommon.Enums
{
    /// <summary>
    /// Represents an enum value.
    /// </summary>
    /// <typeparam name="TEnum">The type of the enum.</typeparam>
    public class EnumValue<TEnum> : EnumValueBase
        where TEnum : AdvancedEnum<TEnum>
    {
        internal EnumValue(int index, string name)
            : base(index, name) { }

        public static bool operator ==(EnumValue<TEnum> first, EnumValue<TEnum>? second)
        {
            return first.Equals(second);
        }

        public static bool operator !=(EnumValue<TEnum> first, EnumValue<TEnum>? second)
        {
            return !first.Equals(second);
        }

        public override bool Equals(object? obj)
        {
            return obj is EnumValue<TEnum> enumValue &&
                   enumValue.Name == Name;
        }

        public override int GetHashCode()
        {
            return (typeof(TEnum), Name).GetHashCode();
        }
    }
}

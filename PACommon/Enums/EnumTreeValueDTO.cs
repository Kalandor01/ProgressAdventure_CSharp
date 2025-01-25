namespace PACommon.Enums
{
    /// <summary>
    /// Stores data about an enum value for <see cref="AdvancedEnumTree{TSelf}"/>.
    /// </summary>
    /// <typeparam name="TEnum">The type of the enum.</typeparam>
    public class EnumTreeValueDTO<TEnum> : EnumTreeValue<TEnum>
        where TEnum : AdvancedEnumTree<TEnum>
    {
        public readonly HashSet<EnumTreeValueDTO<TEnum>> Children;

        internal EnumTreeValueDTO(int[] indexes, string fullName, string name)
            : base(indexes, fullName, name)
        {
            Children = [];
        }
    }
}

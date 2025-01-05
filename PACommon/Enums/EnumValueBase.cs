namespace PACommon.Enums
{
    /// <summary>
    /// Should only be used for cases when <see cref="EnumValue{TEnum}"/> cannot be used.
    /// </summary>
    public abstract class EnumValueBase
    {
        public readonly int Index;
        public readonly string Name;

        internal EnumValueBase(int index, string name)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException($"'{nameof(name)}' cannot be null or whitespace.", nameof(name));
            }

            Index = index;
            Name = name;
        }

        public override string? ToString()
        {
            return Name;
        }
    }
}

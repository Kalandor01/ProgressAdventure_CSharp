namespace PACommon.Enums
{
    /// <summary>
    /// Should only be used for cases when <see cref="EnumTreeValue{TEnum}"/> cannot be used.
    /// </summary>
    public abstract class EnumTreeValueBase
    {
        public readonly int[] Indexes;
        public readonly string FullName;
        public readonly string Name;

        internal EnumTreeValueBase(int[] indexes, string fullName, string name)
        {
            if (indexes.Any(i => i < 0))
            {
                throw new ArgumentException("Index value cannot be negative.");
            }

            if (string.IsNullOrWhiteSpace(fullName))
            {
                throw new ArgumentException($"'{nameof(fullName)}' cannot be null or whitespace.", nameof(fullName));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException($"'{nameof(name)}' cannot be null or whitespace.", nameof(name));
            }

            Indexes = indexes;
            FullName = fullName;
            Name = name;
        }

        public override string? ToString()
        {
            return Name;
        }
    }
}

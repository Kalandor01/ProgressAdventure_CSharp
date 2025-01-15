using System.Diagnostics.CodeAnalysis;

namespace PACommon.Enums
{
    /// <summary>
    /// A class that functions similarly to <see cref="Enum"/>, but you can add new values to it at runtime.
    /// </summary>
    /// <typeparam name="TSelf">The type that implements the interface.</typeparam>
    public abstract class AdvancedEnum<TSelf>
        where TSelf : AdvancedEnum<TSelf>
    {
        /// <summary>
        /// The list of enum values.
        /// </summary>
        protected static HashSet<EnumValue<TSelf>> EnumValues { get; } = [];

        /// <summary>
        /// Adds a new enum value to the list.
        /// </summary>
        /// <param name="name">The name of the value.</param>
        /// <returns>The new value.</returns>
        /// <exception cref="ArgumentException">Thrown if the value already exists.</exception>
        public static EnumValue<TSelf> AddValue(string name)
        {
            var value = new EnumValue<TSelf>(EnumValues.Count, name);
            var success = EnumValues.Add(value);
            return success ? value : throw new ArgumentException("A value already exists with this name in the enum.", nameof(name));
        }

        /// <summary>
        /// Gets the names of all values in the list.
        /// </summary>
        public static List<string> GetNames()
        {
            return EnumValues.Select(v => v.Name).ToList();
        }

        /// <summary>
        /// Returns all enum values.
        /// </summary>
        public static List<EnumValue<TSelf>> GetValues()
        {
            return [.. EnumValues];
        }

        /// <summary>
        /// Gets the number of enum values of this type.
        /// </summary>
        public static int Count()
        {
            return EnumValues.Count;
        }

        /// <summary>
        /// Tries to retun the enum value that has a specified name.<br/>
        /// </summary>
        /// <param name="name">The name of the value.</param>
        /// <param name="value">The found value, or null.</param>
        public static bool TryGetValue(string name, [NotNullWhen(true)] out EnumValue<TSelf>? value)
        {
            return EnumValues.TryGetValue(new EnumValue<TSelf>(0, name), out value);
        }

        /// <summary>
        /// Tries to retun the enum value at a specified index.<br/>
        /// WARNING: SLOW!
        /// </summary>
        /// <param name="index">The index of the value.</param>
        /// <param name="value">The found value, or null.</param>
        public static bool TryGetValue(int index, [NotNullWhen(true)] out EnumValue<TSelf>? value)
        {
            if (index < 0 || EnumValues.Count >= index)
            {
                value = null;
                return false;
            }
            value = EnumValues.ElementAt(index);
            return true;
        }

        /// <summary>
        /// Retuns the enum value that has a specified name.<br/>
        /// </summary>
        /// <param name="name">The name of the value.</param>
        public static EnumValue<TSelf> GetValue(string name)
        {
            return TryGetValue(name, out var value)
                ? value
                : throw new ArgumentException($"A value with the given name doesn't exist in the {typeof(TSelf)} enum", nameof(name));
        }

        /// <summary>
        /// Rretuns the enum value at a specified index.<br/>
        /// WARNING: SLOW!
        /// </summary>
        /// <param name="index">The index of the value.</param>
        public static EnumValue<TSelf> GetValue(int index)
        {
            return EnumValues.ElementAt(index);
        }
    }
}

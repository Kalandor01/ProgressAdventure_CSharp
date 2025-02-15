using System.Diagnostics.CodeAnalysis;

namespace PACommon.Enums
{
    /// <summary>
    /// A class that functions similarly to <see cref="Enum"/>, but you can add new values to it at runtime.
    /// </summary>
    /// <typeparam name="TSelf">The type that implements this class.</typeparam>
    public abstract class AdvancedEnum<TSelf>
        where TSelf : AdvancedEnum<TSelf>
    {
        #region Properties
        /// <summary>
        /// The list of enum values.
        /// </summary>
        protected static HashSet<EnumValue<TSelf>> EnumValues { get; } = [];

        /// <summary>
        /// If the enum values list can be cleared.
        /// </summary>
        public static bool IsClearable { get; private set; } = false;

        /// <summary>
        /// If values from the enum values list can be removed.
        /// </summary>
        public static bool IsRemovable { get; private set; } = false;
        #endregion

        #region Public functions
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
        /// Tries to add a new enum value to the list.
        /// </summary>
        /// <param name="name">The name of the value.</param>
        /// <param name="value">The new value, if it was added, or the existing value if not.</param>
        /// <returns>If the new value was successfuly added.</returns>
        public static bool TryAddValue(string name, [NotNullWhen(true)] out EnumValue<TSelf>? value)
        {
            value = null;
            if (
                string.IsNullOrWhiteSpace(name) ||
                EnumValues.TryGetValue(new EnumValue<TSelf>(0, name), out value)
            )
            {
                return false;
            }
            value = AddValue(name);
            return true;
        }

        /// <summary>
        /// Removes an existing enum value from the list.
        /// </summary>
        /// <param name="name">The name of the value.</param>
        /// <exception cref="ArgumentException">Thrown if the value doesn't exists, or <see cref="IsRemovable"/> is false.</exception>
        public static void RemoveValue(string name)
        {
            if (!IsClearable)
            {
                throw new ArgumentException("Removing enum values is disabled for this enum!", nameof(IsRemovable));
            }

            var value = new EnumValue<TSelf>(0, name);
            var success = EnumValues.Remove(value);
            if (!success)
            {
                throw new ArgumentException("A value doesn't exist with this name in the enum.", nameof(name));
            }
        }

        /// <summary>
        /// Tries to remove an existing enum value from the list.
        /// </summary>
        /// <param name="name">The name of the value.</param>
        /// <returns>If the new value was successfuly removed.</returns>
        /// <exception cref="ArgumentException">Thrown if <see cref="IsRemovable"/> is false.</exception>
        public static bool TryRemoveValue(string name)
        {
            if (!IsClearable)
            {
                throw new ArgumentException("Removing enum values is disabled for this enum!", nameof(IsRemovable));
            }

            var value = new EnumValue<TSelf>(0, name);
            return EnumValues.Remove(value);
        }

        /// <summary>
        /// Removes all values from the enum.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if <see cref="IsClearable"/> is false.</exception>
        public static void Clear()
        {
            if (IsClearable)
            {
                EnumValues.Clear();
            }
            else
            {
                throw new ArgumentException("Clearing enum values is disabled for this enum!", nameof(IsClearable));
            }
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
        /// Tries to return the enum value that has a specified name.<br/>
        /// </summary>
        /// <param name="name">The name of the value.</param>
        /// <param name="value">The found value, or null.</param>
        public static bool TryGetValue(string name, [NotNullWhen(true)] out EnumValue<TSelf>? value)
        {
            return EnumValues.TryGetValue(new EnumValue<TSelf>(0, name), out value);
        }

        /// <summary>
        /// Tries to return the enum value at a specified index.<br/>
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
                : throw new ArgumentException($"A value with the name \"{name}\" doesn't exist in the {typeof(TSelf)} enum", nameof(name));
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
        #endregion

        #region Priotected functions
        protected static bool UpdateIsClearable(bool newValue)
        {
            IsClearable = newValue;
            return newValue;
        }

        protected static bool UpdateIsRemovable(bool newValue)
        {
            IsRemovable = newValue;
            return newValue;
        }
        #endregion
    }
}

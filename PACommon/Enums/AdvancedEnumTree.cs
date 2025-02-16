using System.Diagnostics.CodeAnalysis;

namespace PACommon.Enums
{
    /// <summary>
    /// A class that functions similarly to <see cref="AdvancedEnum{TSelf}"/>, but in a tree-like way.
    /// </summary>
    /// <typeparam name="TSelf">The type that implements this class.</typeparam>
    public abstract class AdvancedEnumTree<TSelf>
        where TSelf : AdvancedEnumTree<TSelf>
    {
        #region Properties
        /// <summary>
        /// The list of enum values.
        /// </summary>
        protected static HashSet<EnumTreeValueDTO<TSelf>> EnumValues { get; } = [];

        /// <summary>
        /// If the enum values list can be cleared.
        /// </summary>
        public static bool IsClearable { get; private set; } = false;

        /// <summary>
        /// If values from the enum values list can be removed.
        /// </summary>
        public static bool IsRemovable { get; private set; } = false;

        /// <summary>
        /// The layer separator in the names of the values of the enum.
        /// </summary>
        public static char LayerNameSeparator { get; private set; } = Constants.ENUM_TREE_DEFAULT_LAYER_SEP_CHAR;
        #endregion

        #region Public functions
        /// <summary>
        /// Adds a new enum value to the list.
        /// </summary>
        /// <param name="parrent">The parrent enum value to add this value to.</param>
        /// <param name="name">The name of the value.</param>
        /// <returns>The new value.</returns>
        /// <exception cref="ArgumentException">Thrown if the value already exists.</exception>
        public static EnumTreeValue<TSelf> AddValue(EnumTreeValue<TSelf>? parrent, string name)
        {
            var trueParrentChildren = parrent is not null ? GetDTOValue(parrent.FullName).Children : EnumValues;
            var indexes = parrent is not null
                ? parrent.Indexes.Append(trueParrentChildren.Count).ToArray()
                : [trueParrentChildren.Count];
            var fullName = parrent is not null ? parrent.FullName + LayerNameSeparator + name : name;

            var value = new EnumTreeValueDTO<TSelf>(indexes, fullName, name);
            var success = trueParrentChildren.Add(value);

            return success
                ? value
                : throw new ArgumentException(
                    $"A value already exists with this name in the {typeof(TSelf)} enum{(parrent is not null ? $" subvalue \"{parrent.FullName}\"" : "")}.",
                    nameof(name)
                );
        }

        /// <summary>
        /// Tries to add a new enum value to the list.
        /// </summary>
        /// <param name="fullName">The full name of the value to add.</param>
        /// <param name="value">The new value, if it was added, or the existing value if not.</param>
        /// <param name="recursive">Whether to add the parrent values if those don't exist either.</param>
        /// <returns>If the new value was successfuly added.</returns>
        public static bool TryAddValue(string fullName, [NotNullWhen(true)] out EnumTreeValue<TSelf>? value, bool recursive = true)
        {
            if (
                TryGetDTOValue(fullName, out var valueDTO) ||
                string.IsNullOrWhiteSpace(fullName)
            )
            {
                value = valueDTO;
                return false;
            }

            value = null;
            var lastSepIndex = fullName.LastIndexOf(LayerNameSeparator);
            if (lastSepIndex != -1)
            {
                var parrentName = fullName[..lastSepIndex];
                var newValueName = (lastSepIndex + 1) < fullName.Length ? fullName[(lastSepIndex + 1)..] : "";
                if (
                    !string.IsNullOrWhiteSpace(parrentName) &&
                    !string.IsNullOrWhiteSpace(newValueName)
                )
                {
                    if (recursive)
                    {
                        TryAddValue(parrentName, out _, true);
                    }
                    if (TryGetDTOValue(parrentName, out var parrent))
                    {
                        value = AddValue(parrent, newValueName);
                        return true;
                    }
                }
                return false;
            }

            value = AddValue(null, fullName);
            return true;
        }

        /// <summary>
        /// Removes an existing enum value from the list.
        /// </summary>
        /// <param name="parrent">The parrent enum value to remove this value from.</param>
        /// <param name="name">The name of the value.</param>
        /// <exception cref="ArgumentException">Thrown if the value doesn't exists, or <see cref="IsRemovable"/> is false.</exception>
        public static void RemoveValue(EnumTreeValue<TSelf>? parrent, string name)
        {
            if (!IsClearable)
            {
                throw new ArgumentException("Removing enum values is disabled for this enum!", nameof(IsRemovable));
            }

            var trueParrentChildren = parrent is not null ? GetDTOValue(parrent.FullName).Children : EnumValues;
            var fullName = parrent is not null ? parrent.FullName + LayerNameSeparator + name : name;

            var value = new EnumTreeValueDTO<TSelf>([0], fullName, "test");
            var success = trueParrentChildren.Remove(value);

            if (!success)
            {
                throw new ArgumentException(
                    $"A value already exists with this name in the {typeof(TSelf)} enum{(parrent is not null ? $" subvalue \"{parrent.FullName}\"" : "")}.",
                    nameof(name)
                );
            }
        }

        /// <summary>
        /// Tries to remove an existing enum value from the list.
        /// </summary>
        /// <param name="name">The name of the value.</param>
        /// <returns>If the new value was successfuly removed.</returns>
        /// <exception cref="ArgumentException">Thrown if <see cref="IsRemovable"/> is false.</exception>
        public static bool TryRemoveValue(string fullName, bool recursive = true)
        {
            if (!IsClearable)
            {
                throw new ArgumentException("Removing enum values is disabled for this enum!", nameof(IsRemovable));
            }

            if (string.IsNullOrWhiteSpace(fullName))
            {
                return false;
            }

            var lastSepIndex = fullName.LastIndexOf(LayerNameSeparator);
            if (lastSepIndex == -1)
            {
                var value = new EnumTreeValueDTO<TSelf>([0], fullName, "test");
                return EnumValues.Remove(value);
            }
            else
            {
                var parrentName = fullName[..lastSepIndex];
                if (!TryGetDTOValue(parrentName, out var parrent))
                {
                    return false;
                }
                var value = new EnumTreeValueDTO<TSelf>([0], fullName, "test");
                return parrent.Children.Remove(value);
            }
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
        /// Returns the full names of all enum values and subvalues.
        /// </summary>
        public static List<string> GetAllFullNames()
        {
            var names = new List<string>();
            foreach (var value in EnumValues)
            {
                names.Add(value.Name);
                GetNamesPrivate(value, ref names);
            }
            return names;
        }

        /// <summary>
        /// Returns the names of the subvalues of an enum value.
        /// </summary>
        /// <param name="parrent">The parrent value to return the subvalue names from, or the root names if null.</param>
        public static List<string> GetNames(EnumTreeValue<TSelf>? parrent)
        {
            return (parrent is not null ? GetDTOValue(parrent.FullName).Children : EnumValues).Select(v => v.Name).ToList();
        }

        /// <summary>
        /// Returns all enum values and subvalues.
        /// </summary>
        public static List<EnumTreeValue<TSelf>> GetAllValues()
        {
            var values = new List<EnumTreeValue<TSelf>>();
            foreach (var value in EnumValues)
            {
                values.Add(value);
                GetValuesPrivate(value, ref values);
            }
            return values;
        }

        /// <summary>
        /// Returns all enum subvalues of a value.
        /// </summary>
        /// <param name="parrent">The parrent value to return the subvalues from, or the root values if null.</param>
        public static List<EnumTreeValue<TSelf>> GetValues(EnumTreeValue<TSelf>? parrent)
        {
            return (
                parrent is null
                    ? EnumValues
                    : (
                        parrent is EnumTreeValueDTO<TSelf> parrentDTO
                            ? parrentDTO
                            : GetDTOValue(parrent.FullName)
                        ).Children
                    )
                    .Cast<EnumTreeValue<TSelf>>()
                    .ToList();
        }

        /// <summary>
        /// Gets the number of enum values and subvalues of this type.
        /// </summary>
        public static int AllCount()
        {
            var count = 0;
            foreach (var value in EnumValues)
            {
                count += GetCountPrivate(value) + 1;
            }
            return count;
        }

        /// <summary>
        /// Returns the number of values in this subvalue.
        /// </summary>
        /// <param name="parrent">The parrent value to return the subvalue count from, or the root value count if null.</param>
        public static int Count(EnumTreeValue<TSelf>? parrent)
        {
            return (parrent is not null ? GetDTOValue(parrent.FullName).Children : EnumValues).Count;
        }

        /// <summary>
        /// Tries to return the enum value with the given name that is the child of the specified parrent.<br/>
        /// </summary>
        /// <param name="parrent">The parrent value.</param>
        /// <param name="childName">The name of the child value.</param>
        /// <param name="value">The found value, or null.</param>
        public static bool TryGetChildValue(EnumTreeValue<TSelf>? parrent, string childName, [NotNullWhen(true)] out EnumTreeValue<TSelf>? value)
        {
            HashSet<EnumTreeValueDTO<TSelf>> parrentChildren;
            if (parrent is null)
            {
                parrentChildren = EnumValues;
            }
            else if (parrent is EnumTreeValueDTO<TSelf> parrentDTO)
            {
                parrentChildren = parrentDTO.Children;
            }
            else if (TryGetDTOValue(parrent.FullName, out var parrentDTO2))
            {
                parrentChildren = parrentDTO2.Children;
            }
            else
            {
                value = null;
                return false;
            }

            var testValue = new EnumTreeValueDTO<TSelf>(
                [0],
                $"{(parrent is null ? "" : parrent.FullName + LayerNameSeparator)}{childName}",
                "test"
            );
            var success = parrentChildren.TryGetValue(testValue, out var valueDTO);
            value = valueDTO;
            return success;
        }

        /// <summary>
        /// Tries to return the enum value that has a specified name.<br/>
        /// </summary>
        /// <param name="fullName">The full name of the value.</param>
        /// <param name="value">The found value, or null.</param>
        public static bool TryGetValue(string? fullName, [NotNullWhen(true)] out EnumTreeValue<TSelf>? value)
        {
            var success = TryGetDTOValue(fullName, out var valueDTO);
            value = valueDTO;
            return success;
        }

        /// <summary>
        /// Tries to return the enum value at a specified index.<br/>
        /// WARNING: SLOW!
        /// </summary>
        /// <param name="indexes">The index array of the value.</param>
        /// <param name="value">The found value, or null.</param>
        public static bool TryGetValue(int[] indexes, [NotNullWhen(true)] out EnumTreeValue<TSelf>? value)
        {
            var success = TryGetDTOValue(indexes, out var valueDTO);
            value = valueDTO;
            return success;
        }

        /// <summary>
        /// Retuns the enum value that has a specified name.<br/>
        /// </summary>
        /// <param name="fullName">The full name of the value.</param>
        public static EnumTreeValue<TSelf> GetValue(string fullName)
        {
            return TryGetValue(fullName, out var value)
                ? value
                : throw new ArgumentException($"A value with the given name doesn't exist in the {typeof(TSelf)} enum", nameof(fullName));
        }

        /// <summary>
        /// Rretuns the enum value at a specified index.<br/>
        /// WARNING: SLOW!
        /// </summary>
        /// <param name="indexes">The index array of the value.</param>
        public static EnumTreeValue<TSelf> GetValue(int[] indexes)
        {
            return TryGetDTOValue(indexes, out var value)
                ? value
                : throw new ArgumentException($"A value with the given indexes doesn't exist in the {typeof(TSelf)} enum", nameof(indexes));
        }
        #endregion

        #region Protected functions
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

        protected static char UpdateLayerSeparator(char layerSep)
        {
            LayerNameSeparator = layerSep;
            return layerSep;
        }

        protected static bool TryGetDTOValue(string? fullName, [NotNullWhen(true)] out EnumTreeValueDTO<TSelf>? value)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                value = null;
                return false;
            }
            var sepIndex = fullName.LastIndexOf(LayerNameSeparator);
            if (sepIndex == -1)
            {
                return EnumValues.TryGetValue(new EnumTreeValueDTO<TSelf>([0], fullName, "test"), out value);
            }
            if (!TryGetDTOValue(fullName[..sepIndex], out var parrentValue))
            {
                value = null;
                return false;
            }
            return parrentValue.Children.TryGetValue(new EnumTreeValueDTO<TSelf>([0], fullName, "test"), out value);
        }

        protected static EnumTreeValueDTO<TSelf> GetDTOValue(string fullName)
        {
            return TryGetDTOValue(fullName, out var value)
                ? value
                : throw new ArgumentException($"A value with the given name doesn't exist in the {typeof(TSelf)} enum", nameof(fullName));
        }

        protected static bool TryGetDTOValue(int[] indexes, [NotNullWhen(true)] out EnumTreeValueDTO<TSelf>? value)
        {
            if (indexes.Length == 0 || indexes[^1] < 0)
            {
                value = null;
                return false;
            }
            var currIndex = indexes[^1];
            if (indexes.Length <= 1)
            {
                if (currIndex >= EnumValues.Count)
                {
                    value = null;
                    return false;
                }
                value = EnumValues.ElementAt(currIndex);
                return true;
            }
            return TryGetDTOValue(indexes[..^1], out value);
        }
        #endregion

        #region Private function
        private static void GetNamesPrivate(EnumTreeValueDTO<TSelf> value, ref List<string> names)
        {
            foreach (var child in value.Children)
            {
                names.Add(child.Name);
                if (child.Children.Count > 0)
                {
                    GetNamesPrivate(child, ref names);
                }
            }
        }

        private static void GetValuesPrivate(EnumTreeValueDTO<TSelf> value, ref List<EnumTreeValue<TSelf>> values)
        {
            foreach (var child in value.Children)
            {
                values.Add(child);
                if (child.Children.Count > 0)
                {
                    GetValuesPrivate(child, ref values);
                }
            }
        }

        private static int GetCountPrivate(EnumTreeValueDTO<TSelf> value)
        {
            var count = 0;
            foreach (var child in value.Children)
            {
                count += GetCountPrivate(child) + 1;
            }
            return count;
        }
        #endregion
    }
}

using System.Diagnostics.CodeAnalysis;

namespace PACommon.ConfigManagement
{
    /// <summary>
    /// Interface for reading config files, to loading config dictionaries.
    /// </summary>
    public interface IConfigManager
    {
        #region Public functions
        /// <summary>
        /// Gets the full path of a config file.
        /// </summary>
        /// <param name="configName">The name of the config file name.</param>
        public string GetConfigFilePath(string configName);

        /// <summary>
        /// Returns if a config file exists.
        /// </summary>
        /// <param name="configName">The name of the config file.</param>
        public bool ConfigFileExists(string configName);

        #region Get config
        /// <summary>
        /// Gets the value of an object from a config file.
        /// </summary>
        /// <typeparam name="T">The type of the config object.</typeparam>
        /// <param name="configName">The name of the config file.</param>
        /// <param name="expectedVersion">The expected version of the config json.<br/>
        /// If not null, and the version in the file is not the same, it will regenerate the config file.<br/>
        /// If it's an empty string, it doesn't care about the version.<br/>
        /// If null, it assumes that the config json only contains the data.</param>
        /// <exception cref="NullReferenceException">Trown if the deserialized config object is null.</exception>
        public T GetConfig<T>(string configName, string? expectedVersion);

        /// <summary>
        /// Gets the value of an object from a config file.<br/>
        /// For config objects, where the type of the object is a dictionary, where the keys are not deserializable.
        /// </summary>
        /// <typeparam name="TK">The type of the keys in the resulting dictionary.</typeparam>
        /// <typeparam name="TV">The type of the values in the resulting dictionary.</typeparam>
        /// <param name="deserializeDictionaryKeys">A function to convert the string representation of the original keys in the dictionary, to their original type.</param>
        /// <inheritdoc cref="GetConfig{T}(string, string?)"/>
        public Dictionary<TK, TV> GetConfigDict<TK, TV>(
            string configName,
            string? expectedVersion,
            Func<string, TK> deserializeDictionaryKeys
        ) where TK : notnull;

        /// <typeparam name="TVC">The type of the converted values in the dictionary.</typeparam>
        /// <param name="deserializeDictionaryValues">A function to convert the converted representation of the original valuess in the dictionary, to their original type.</param>
        /// <inheritdoc cref="GetConfigDict{TK, TV}(string, string?, Func{string, TK})"/>
        public Dictionary<TK, TV> GetConfigDict<TK, TV, TVC>(
            string configName,
            string? expectedVersion,
            Func<TVC, TV> deserializeDictionaryValues,
            Func<string, TK>? deserializeDictionaryKeys = null
        ) where TK : notnull;
        #endregion

        #region Try get config
        /// <summary>
        /// Tries to get the value of a config object from a config file.
        /// </summary>
        /// <typeparam name="T">The type of the config object.</typeparam>
        /// <param name="configName">The name of the config file.</param>
        /// <param name="expectedVersion">The expected version of the config json.<br/>
        /// If not null, and the version in the file is not the same, it will regenerate the config file.<br/>
        /// If it's an empty string, it doesn't care about the version.<br/>
        /// If null, it assumes that the config json only contains the data.</param>
        /// <param name="configValue">The returned config value.</param>
        public bool TryGetConfig<T>(
            string configName,
            string? expectedVersion,
            [NotNullWhen(true)] out T? configValue
        );

        /// <typeparam name="TK">The type of the keys in the resulting dictionary.</typeparam>
        /// <typeparam name="TV">The type of the values in the resulting dictionary.</typeparam>
        /// <param name="deserializeDictionaryKeys">A function to convert the string representation of the original keys in the dictionary, to their original type.</param>
        /// <inheritdoc cref="TryGetConfig{T}(string, string?, out T)"/>
        public bool TryGetConfigDict<TK, TV>(
            string configName,
            string? expectedVersion,
            [NotNullWhen(true)] out Dictionary<TK, TV>? configValue,
            Func<string, TK> deserializeDictionaryKeys
        ) where TK : notnull;

        /// <typeparam name="TVC">The type of the converted values in the dictionary.</typeparam>
        /// <param name="deserializeDictionaryValues">A function to convert the converted representation of the original valuess in the dictionary, to their original type.</param>
        /// <inheritdoc cref="TryGetConfigDict{TK, TV}(string, string?, out Dictionary{TK, TV}?, Func{string, TK})"/>
        public bool TryGetConfigDict<TK, TV, TVC>(
            string configName,
            string? expectedVersion,
            [NotNullWhen(true)] out Dictionary<TK, TV>? configValue,
            Func<TVC, TV> deserializeDictionaryValues,
            Func<string, TK>? deserializeDictionaryKeys = null
        ) where TK : notnull;
        #endregion

        #region Try get config or recreate
        /// <summary>
        /// Tries to get the value of a config object, and if it doesn't work, it recreates the config file from the default value, and tries again.
        /// </summary>
        /// <param name="defaultContent">The default config object value.</param>
        /// <param name="justRecreate">If true, it doesn't try to get the config before recreating it.</param>
        /// <inheritdoc cref="TryGetConfig{T}(string, string?, out T)"/>
        public T TryGetConfigOrRecreate<T>(
            string configName,
            string? expectedVersion,
            T defaultContent,
            bool justRecreate = false
        );

        /// <summary>
        /// Tries to get the value of a config object, and if it doesn't work, it recreates the config file from the default value, and tries again.<br/>
        /// For config objects, where the type of the object is a dictionary, where the keys are not deserializable.
        /// </summary>
        /// <typeparam name="TK">The type of the keys in the resulting dictionary.</typeparam>
        /// <typeparam name="TV">The type of the values in the resulting dictionary.</typeparam>
        /// <param name="serializeDictionaryKeys">A function to convert the keys of the dictionary to string values.</param>
        /// <param name="deserializeDictionaryKeys">A function to convert the string representation of the original keys in the dictionary, to their original type.</param>
        /// <inheritdoc cref="TryGetConfigOrRecreate{T}(string, string?, T, bool)"/>
        public Dictionary<TK, TV> TryGetConfigOrRecreateDict<TK, TV>(
            string configName,
            string? expectedVersion,
            IDictionary<TK, TV> defaultContent,
            Func<TK, string> serializeDictionaryKeys,
            Func<string, TK> deserializeDictionaryKeys,
            bool justRecreate = false
        ) where TK : notnull;

        /// <typeparam name="TVC">The type of the converted values in the dictionary.</typeparam>
        /// <param name="serializeDictionaryValues">A function to convert the values of the dictionary.</param>
        /// <param name="deserializeDictionaryValues">A function to convert the converted representation of the original valuess in the dictionary, to their original type.</param>
        /// <inheritdoc cref="TryGetConfigOrRecreateDict{TK, TV}(string, string?, IDictionary{TK, TV}, Func{TK, string}, Func{string, TK}, bool)"/>
        public Dictionary<TK, TV> TryGetConfigOrRecreateDict<TK, TV, TVC>(
            string configName,
            string? expectedVersion,
            IDictionary<TK, TV> defaultContent,
            Func<TV, TVC> serializeDictionaryValues,
            Func<TVC, TV> deserializeDictionaryValues,
            Func<TK, string>? serializeDictionaryKeys = null,
            Func<string, TK>? deserializeDictionaryKeys = null,
            bool justRecreate = false
        ) where TK : notnull;
        #endregion

        #region Set config
        /// <summary>
        /// Sets the value of an object in a config file.
        /// </summary>
        /// <typeparam name="T">The type of the config data.</typeparam>
        /// <param name="configName">The name of the config file.</param>
        /// <param name="configVersion">The version of the config json.<br/>
        /// <param name="configData">The object to put into the config file.</param>
        /// If null, the config json only contains the data.</param>
        public void SetConfig<T>(string configName, string? configVersion, T configData);

        /// <summary>
        /// Sets the value of an object in a config file.<br/>
        /// For config objects, where the type of the object is a dictionary, where the keys are not serializable.
        /// </summary>
        /// <typeparam name="TK">The type of the keys in the config data.</typeparam>
        /// <typeparam name="TV">The type of the values in the config data.</typeparam>
        /// <param name="configData">The object to put into the config file.</param>
        /// <param name="serializeDictionaryKeys">A function to convert the keys of the dictionary to string values.</param>
        /// <inheritdoc cref="SetConfig{T}(string, string?, T)"/>
        public void SetConfigDict<TK, TV>(
            string configName,
            string? configVersion,
            IDictionary<TK, TV> configData,
            Func<TK, string> serializeDictionaryKeys
        ) where TK : notnull;

        /// <typeparam name="TVC">The type of the converted values in the dictionary.</typeparam>
        /// <param name="serializeDictionaryValues">A function to convert the values of the dictionary.</param>
        /// <inheritdoc cref="SetConfigDict{TK, TV}(string, string?, IDictionary{TK, TV}, Func{TK, string})"/>
        public void SetConfigDict<TK, TV, TVC>(
            string configName,
            string? configVersion,
            IDictionary<TK, TV> configData,
            Func<TV, TVC> serializeDictionaryValues,
            Func<TK, string>? serializeDictionaryKeys = null
        ) where TK : notnull;
        #endregion
        #endregion
    }
}

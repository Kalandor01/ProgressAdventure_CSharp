using Newtonsoft.Json;

namespace PACommon
{
    /// <summary>
    /// Abstract class for reading config files, to loading config dictionaries.
    /// </summary>
    public abstract class AConfigManager
    {
        #region Protected fields
        /// <summary>
        /// The list of json converters to use, when converting to/from specific types.
        /// </summary>
        protected readonly JsonConverter[] _converters;

        /// <summary>
        /// The path to the folder, where the config folder should be.
        /// </summary>
        protected readonly string _configsFolderParrentPath;
        /// <summary>
        /// The name of the configs folder.
        /// </summary>
        protected readonly string _configsFolderName;
        /// <summary>
        /// The extension of config files.
        /// </summary>
        protected readonly string _configExtension;
        #endregion

        #region Protected constructors
        /// <summary>
        /// <inheritdoc cref="AConfigManager"/>
        /// </summary>
        /// <param name="converters"><inheritdoc cref="_converters" path="//summary"/></param>
        /// <param name="configsFolderParrentPath"><inheritdoc cref="_configsFolderParrentPath" path="//summary"/></param>
        /// <param name="configsFolderName"><inheritdoc cref="_configsFolderName" path="//summary"/></param>
        /// <param name="configExtension"><inheritdoc cref="_configExtension" path="//summary"/></param>
        /// <exception cref="DirectoryNotFoundException">Thrown if <paramref name="configsFolderParrentPath"/> doesn't exist.</exception>
        protected AConfigManager(
            JsonConverter[]? converters = null,
            string? configsFolderParrentPath = null,
            string configsFolderName = Constants.CONFIGS_FOLDER,
            string configExtension = Constants.CONFIG_EXT
        )
        {
            _converters = converters ?? Array.Empty<JsonConverter>();
            _configsFolderParrentPath = configsFolderParrentPath ?? Constants.ROOT_FOLDER;
            if (!Directory.Exists(_configsFolderParrentPath))
            {
                throw new DirectoryNotFoundException("Configs folder parrent directory not found.");
            }
            _configsFolderName = configsFolderName;
            _configExtension = configExtension;
        }
        #endregion

        #region Public functions
        /// <summary>
        /// Gets the value of an object from a config file.
        /// </summary>
        /// <typeparam name="T">The type of the config object.</typeparam>
        /// <param name="configName">The name of the config file.</param>
        /// <exception cref="NullReferenceException">Trown if the deserialized config object is null.</exception>
        public T GetConfig<T>(string configName)
        {
            Tools.RecreateFolder(_configsFolderName, _configsFolderParrentPath, "configs");
            var filePath = Path.Join(_configsFolderParrentPath, _configsFolderName, $"{configName}.{_configExtension}");
            var safeFilePath = Path.GetRelativePath(_configsFolderParrentPath, filePath);

            var fileData = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<T>(fileData, _converters)
                ?? throw new NullReferenceException($"The config json data is null in \"{safeFilePath}\".");
        }

        /// <summary>
        /// Gets the value of an object from a config file.<br/>
        /// For config objects, where the type of the object is a dictionary, where the keys are not deserializable.
        /// </summary>
        /// <typeparam name="TK">The type of the keys in the resulting dictionary.</typeparam>
        /// <typeparam name="TV">The type of the values in the resulting dictionary.</typeparam>
        /// <param name="configName">The name of the config file.</param>
        /// <param name="deserializeDictionaryKeys">A function to convert the string representation of the original keys in the dictionary, to their original type.</param>
        /// <exception cref="ArgumentNullException">Thrown if the deserialized config object, or it's converted version is null.</exception>
        public Dictionary<TK, TV> GetConfig<TK, TV>(string configName, Func<string, TK> deserializeDictionaryKeys)
            where TK : notnull
        {
            var tempResult = GetConfig<IDictionary<string, TV>>(configName);
            return tempResult?.ToDictionary(key => deserializeDictionaryKeys(key.Key), value => value.Value)
                ?? throw new ArgumentNullException(nameof(tempResult));
        }

        /// <summary>
        /// Tries to get the value of a config object, and if it doesn't work, it recreates the config file from the default value, and tries again.
        /// </summary>
        /// <typeparam name="T">The type of the config object.</typeparam>
        /// <param name="configName">The name of the config file.</param>
        /// <param name="defaultContent">The default config object value.</param>
        public T TryGetConfig<T>(string configName, T defaultContent)
        {
            try
            {
                return GetConfig<T>(configName);
            }
            catch
            {
                SetConfig(configName, defaultContent);
                return GetConfig<T>(configName);
            }
        }

        /// <summary>
        /// Tries to get the value of a config object, and if it doesn't work, it recreates the config file from the default value, and tries again.<br/>
        /// For config objects, where the type of the object is a dictionary, where the keys are not deserializable.
        /// </summary>
        /// <typeparam name="TK">The type of the keys in the resulting dictionary.</typeparam>
        /// <typeparam name="TV">The type of the values in the resulting dictionary.</typeparam>
        /// <param name="configName">The name of the config file.</param>
        /// <param name="defaultContent">The default config object value.</param>
        /// <param name="serializeDictionaryKeys">A function to convert the keys of the dictionary to string values.</param>
        /// <param name="deserializeDictionaryKeys">A function to convert the string representation of the original keys in the dictionary, to their original type.</param>
        public Dictionary<TK, TV> TryGetConfig<TK, TV>(
            string configName,
            IDictionary<TK, TV> defaultContent,
            Func<TK, string> serializeDictionaryKeys,
            Func<string, TK> deserializeDictionaryKeys
        )
            where TK : notnull
        {
            try
            {
                return GetConfig<TK, TV>(configName, deserializeDictionaryKeys);
            }
            catch
            {
                var tempDefaultContent = defaultContent.ToDictionary(key => serializeDictionaryKeys(key.Key), value => value.Value);
                SetConfig(configName, tempDefaultContent);
                return GetConfig<TK, TV>(configName, deserializeDictionaryKeys);
            }
        }

        /// <summary>
        /// Returns all of the custom json converters from this config manager, except the one, with a specific type.
        /// </summary>
        /// <typeparam name="T">The type of the json converter to not return.</typeparam>
        public JsonConverter[] GetConvertersNonInclusive<T>()
            where T : JsonConverter
        {
            return _converters.Where(converter => converter.GetType() != typeof(T)).ToArray();
        }
        #endregion

        #region protected functions
        /// <summary>
        /// Sets the value of an object in a config file.
        /// </summary>
        /// <typeparam name="T">The type of the config data.</typeparam>
        /// <param name="configName">The name of the config file.</param>
        /// <param name="configData">The object to put into the config file.</param>
        protected void SetConfig<T>(string configName, T configData)
        {
            Tools.RecreateFolder(_configsFolderName, _configsFolderParrentPath, "configs");
            var filePath = Path.Join(_configsFolderParrentPath, _configsFolderName, $"{configName}.{_configExtension}");
            var safeFilePath = Path.GetRelativePath(_configsFolderParrentPath, filePath);

            var jsonString = JsonConvert.SerializeObject(configData, _converters);
            File.WriteAllText(filePath, jsonString);
            PACSingletons.Instance.Logger.Log($"Config file recreated", $"file path: \"{safeFilePath}\"");
        }
        #endregion
    }
}

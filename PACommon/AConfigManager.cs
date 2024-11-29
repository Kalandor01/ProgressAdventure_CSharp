using PACommon.Enums;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PACommon
{
    /// <summary>
    /// Abstract class for reading config files, to loading config dictionaries.
    /// </summary>
    public abstract class AConfigManager
    {
        #region Protected fields
        private readonly JsonSerializerOptions _jsonReaderOptions;
        private readonly JsonSerializerOptions _jsonWriterOptions;

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
        /// <summary>
        /// The config file version, that every config file should have.<br/>
        /// If not null, and the version in the file is not the same, it will regenerate the config file.
        /// </summary>
        protected readonly string? _currentConfigFileVersion;
        #endregion

        #region Protected constructors
        /// <summary>
        /// <inheritdoc cref="AConfigManager"/>
        /// </summary>
        /// <param name="converters">The list of json serializers to use when converting to/from specific types.</param>
        /// <param name="configsFolderParrentPath"><inheritdoc cref="_configsFolderParrentPath" path="//summary"/></param>
        /// <param name="configsFolderName"><inheritdoc cref="_configsFolderName" path="//summary"/></param>
        /// <param name="configExtension"><inheritdoc cref="_configExtension" path="//summary"/></param>
        /// <param name="currentConfigFileVersion"><inheritdoc cref="_currentConfigFileVersion" path="//summary"/></param>
        /// <exception cref="DirectoryNotFoundException">Thrown if <paramref name="configsFolderParrentPath"/> doesn't exist.</exception>
        protected AConfigManager(
            JsonConverter[]? converters = null,
            string? configsFolderParrentPath = null,
            string configsFolderName = Constants.CONFIGS_FOLDER,
            string configExtension = Constants.CONFIG_EXT,
            string? currentConfigFileVersion = null
        )
        {
            _jsonReaderOptions = new JsonSerializerOptions
            {
                IncludeFields = true,
            };
            _jsonWriterOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                IncludeFields = true,
            };
            foreach (var converter in converters ?? [])
            {
                _jsonReaderOptions.Converters.Add(converter);
                _jsonWriterOptions.Converters.Add(converter);
            }
            _configsFolderParrentPath = configsFolderParrentPath ?? Constants.ROOT_FOLDER;
            if (!Directory.Exists(_configsFolderParrentPath))
            {
                throw new DirectoryNotFoundException("Configs folder parrent directory not found.");
            }
            _configsFolderName = configsFolderName;
            _configExtension = configExtension;
            _currentConfigFileVersion = currentConfigFileVersion;
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
            var rawData = JsonSerializer.Deserialize<Dictionary<string, object?>>(fileData, _jsonReaderOptions)
                ?? throw new NullReferenceException($"The config json is null in \"{safeFilePath}\".");
            if (!(
                rawData.TryGetValue("version", out var configVersion) &&
                rawData.TryGetValue("data", out var configDataObj) &&
                configDataObj is JsonElement configDataJson
            ))
            {
                throw new Exception($"Incorrect config file structure in \"{safeFilePath}\".");
            }
            var configData = configDataJson.Deserialize<T>(_jsonReaderOptions)
                ?? throw new NullReferenceException($"The config json data is null in \"{safeFilePath}\".");
            if (_currentConfigFileVersion is not null &&
                !(
                configVersion is JsonElement versionElement &&
                versionElement.GetString() is string configVersionStr &&
                _currentConfigFileVersion == configVersionStr
            ))
            {
                throw new Exception($"Incorrect config file version in \"{safeFilePath}\" ({_currentConfigFileVersion} => {configVersion?.ToString() ?? "[NULL]"}).");
            }
            return configData;
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
            catch (Exception e)
            {
                PACSingletons.Instance.Logger.Log("Config file error", e.ToString(), LogSeverity.WARN);
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
            catch (Exception e)
            {
                PACSingletons.Instance.Logger.Log("Config file error", e.ToString(), LogSeverity.WARN);
                var tempDefaultContent = defaultContent.ToDictionary(key => serializeDictionaryKeys(key.Key), value => value.Value);
                SetConfig(configName, tempDefaultContent);
                return GetConfig<TK, TV>(configName, deserializeDictionaryKeys);
            }
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

            var configFileData = new Dictionary<string, object?>
            {
                ["version"] = _currentConfigFileVersion,
                ["data"] = configData,
            };
            var jsonString = JsonSerializer.Serialize(configFileData, _jsonWriterOptions);
            File.WriteAllText(filePath, jsonString);
            PACSingletons.Instance.Logger.Log($"Config file recreated", $"file path: \"{safeFilePath}\"");
        }
        #endregion
    }
}

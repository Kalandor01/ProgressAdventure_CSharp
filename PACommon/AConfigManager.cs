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
        /// The path to the configs folder.
        /// </summary>
        protected readonly string _configsFolderPath;
        /// <summary>
        /// The extension of config files.
        /// </summary>
        protected readonly string _configExtension;
        #endregion

        #region Protected constructors
        /// <summary>
        /// <inheritdoc cref="AConfigManager"/>
        /// </summary>
        /// <param name="converters">The list of json serializers to use when converting to/from specific types.</param>
        /// <param name="configsFolderPath"><inheritdoc cref="_configsFolderPath" path="//summary"/></param>
        /// <param name="configExtension"><inheritdoc cref="_configExtension" path="//summary"/></param>
        /// <exception cref="DirectoryNotFoundException">Thrown if <paramref name="configsFolderParrentPath"/> doesn't exist.</exception>
        protected AConfigManager(
            JsonConverter[]? converters = null,
            string? configsFolderPath = null,
            string configExtension = Constants.CONFIG_EXT
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
            _configsFolderPath = configsFolderPath ?? Path.Join(Constants.ROOT_FOLDER, Constants.CONFIGS_FOLDER);
            _configExtension = configExtension;
        }
        #endregion

        #region Public functions
        /// <summary>
        /// Gets the value of an object from a config file.
        /// </summary>
        /// <typeparam name="T">The type of the config object.</typeparam>
        /// <param name="configName">The name of the config file.</param>
        /// <param name="expectedVersion">The expected version of the config json.<br/>
        /// If not null, and the version in the file is not the same, it will regenerate the config file.</param>
        /// <exception cref="NullReferenceException">Trown if the deserialized config object is null.</exception>
        public T GetConfig<T>(string configName, string? expectedVersion)
        {
            Tools.RecreateFolder(_configsFolderPath, "configs");
            var filePath = Path.Join(_configsFolderPath, $"{configName}.{_configExtension}");
            var parrentPath = Path.GetDirectoryName(_configsFolderPath);
            var safeFilePath = parrentPath is null ? filePath : Path.GetRelativePath(parrentPath, filePath);

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
            if (expectedVersion is not null &&
                !(
                configVersion is JsonElement versionElement &&
                versionElement.GetString() is string configVersionStr &&
                expectedVersion == configVersionStr
            ))
            {
                throw new FormatException($"Incorrect config file version in \"{safeFilePath}\" ({expectedVersion} => {configVersion?.ToString() ?? "[NULL]"}).");
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
        /// <param name="expectedVersion">The expected version of the config json.<br/>
        /// If not null, and the version in the file is not the same, it will regenerate the config file.</param>
        /// <param name="deserializeDictionaryKeys">A function to convert the string representation of the original keys in the dictionary, to their original type.</param>
        /// <exception cref="ArgumentNullException">Thrown if the deserialized config object, or it's converted version is null.</exception>
        public Dictionary<TK, TV> GetConfig<TK, TV>(
            string configName,
            string? expectedVersion,
            Func<string, TK> deserializeDictionaryKeys
        )
            where TK : notnull
        {
            var tempResult = GetConfig<IDictionary<string, TV>>(configName, expectedVersion);
            return tempResult?.ToDictionary(key => deserializeDictionaryKeys(key.Key), value => value.Value)
                ?? throw new ArgumentNullException("The deserialized config is null!", nameof(tempResult));
        }

        /// <summary>
        /// Gets the value of an object from a config file.<br/>
        /// For config objects, where the type of the object is a dictionary, where the keys are not deserializable.
        /// </summary>
        /// <typeparam name="TK">The type of the keys in the resulting dictionary.</typeparam>
        /// <typeparam name="TV">The type of the values in the resulting dictionary.</typeparam>
        /// <typeparam name="TVC">The type of the converted values in the dictionary.</typeparam>
        /// <param name="configName">The name of the config file.</param>
        /// <param name="expectedVersion">The expected version of the config json.<br/>
        /// If not null, and the version in the file is not the same, it will regenerate the config file.</param>
        /// <param name="deserializeDictionaryValues">A function to convert the converted representation of the original valuess in the dictionary, to their original type.</param>
        /// <param name="deserializeDictionaryKeys">A function to convert the string representation of the original keys in the dictionary, to their original type.</param>
        /// <exception cref="ArgumentNullException">Thrown if the deserialized config object, or it's converted version is null.</exception>
        public Dictionary<TK, TV> GetConfig<TK, TV, TVC>(
            string configName,
            string? expectedVersion,
            Func<TVC, TV> deserializeDictionaryValues,
            Func<string, TK>? deserializeDictionaryKeys = null
        )
            where TK : notnull
        {
            if (deserializeDictionaryKeys is null)
            {
                var tempResult = GetConfig<IDictionary<TK, TVC>>(configName, expectedVersion)
                    ?? throw new ArgumentNullException("The deserialized config is null!");
                return tempResult.ToDictionary(key => key.Key, value => deserializeDictionaryValues(value.Value));
            }
            else
            {
                var tempResult = GetConfig<IDictionary<string, TVC>>(configName, expectedVersion)
                    ?? throw new ArgumentNullException("The deserialized config is null!");
                return tempResult.ToDictionary(key => deserializeDictionaryKeys(key.Key), value => deserializeDictionaryValues(value.Value));
            }
        }

        /// <summary>
        /// Tries to get the value of a config object, and if it doesn't work, it recreates the config file from the default value, and tries again.
        /// </summary>
        /// <typeparam name="T">The type of the config object.</typeparam>
        /// <param name="configName">The name of the config file.</param>
        /// <param name="expectedVersion">The expected version of the config json.<br/>
        /// If not null, and the version in the file is not the same, it will regenerate the config file.</param>
        /// <param name="defaultContent">The default config object value.</param>
        public T TryGetConfig<T>(string configName, string? expectedVersion, T defaultContent)
        {
            try
            {
                return GetConfig<T>(configName, expectedVersion);
            }
            catch (FileNotFoundException)
            {
                var relativePath = Path.GetRelativePath(Constants.ROOT_FOLDER, Path.Join(_configsFolderPath, $"{configName}.{_configExtension}"));
                PACSingletons.Instance.Logger.Log("Config file error", $"File not found: \"{relativePath}\"", LogSeverity.WARN);
            }
            catch (Exception e)
            {
                PACSingletons.Instance.Logger.Log("Config file error", e.ToString(), LogSeverity.WARN);
            }
            SetConfig(configName, expectedVersion, defaultContent);
            return GetConfig<T>(configName, expectedVersion);
        }

        /// <summary>
        /// Tries to get the value of a config object, and if it doesn't work, it recreates the config file from the default value, and tries again.<br/>
        /// For config objects, where the type of the object is a dictionary, where the keys are not deserializable.
        /// </summary>
        /// <typeparam name="TK">The type of the keys in the resulting dictionary.</typeparam>
        /// <typeparam name="TV">The type of the values in the resulting dictionary.</typeparam>
        /// <param name="configName">The name of the config file.</param>
        /// <param name="expectedVersion">The expected version of the config json.<br/>
        /// If not null, and the version in the file is not the same, it will regenerate the config file.</param>
        /// <param name="defaultContent">The default config object value.</param>
        /// <param name="serializeDictionaryKeys">A function to convert the keys of the dictionary to string values.</param>
        /// <param name="deserializeDictionaryKeys">A function to convert the string representation of the original keys in the dictionary, to their original type.</param>
        public Dictionary<TK, TV> TryGetConfig<TK, TV>(
            string configName,
            string? expectedVersion,
            IDictionary<TK, TV> defaultContent,
            Func<TK, string> serializeDictionaryKeys,
            Func<string, TK> deserializeDictionaryKeys
        )
            where TK : notnull
        {
            try
            {
                return GetConfig<TK, TV>(configName, expectedVersion, deserializeDictionaryKeys);
            }
            catch (FileNotFoundException)
            {
                var relativePath = Path.GetRelativePath(Constants.ROOT_FOLDER, Path.Join(_configsFolderPath, $"{configName}.{_configExtension}"));
                PACSingletons.Instance.Logger.Log("Config file error", $"File not found: \"{relativePath}\"", LogSeverity.WARN);
            }
            catch (Exception e)
            {
                PACSingletons.Instance.Logger.Log("Config file error", e.ToString(), LogSeverity.WARN);
            }
            SetConfig(configName, expectedVersion, defaultContent, serializeDictionaryKeys);
            return GetConfig<TK, TV>(configName, expectedVersion, deserializeDictionaryKeys);
        }

        /// <summary>
        /// Tries to get the value of a config object, and if it doesn't work, it recreates the config file from the default value, and tries again.<br/>
        /// For config objects, where the type of the object is a dictionary, where the keys are not deserializable.
        /// </summary>
        /// <typeparam name="TK">The type of the keys in the resulting dictionary.</typeparam>
        /// <typeparam name="TV">The type of the values in the resulting dictionary.</typeparam>
        /// <typeparam name="TVC">The type of the converted values in the dictionary.</typeparam>
        /// <param name="configName">The name of the config file.</param>
        /// <param name="expectedVersion">The expected version of the config json.<br/>
        /// If not null, and the version in the file is not the same, it will regenerate the config file.</param>
        /// <param name="defaultContent">The default config object value.</param>
        /// <param name="serializeDictionaryValues">A function to convert the values of the dictionary.</param>
        /// <param name="deserializeDictionaryValues">A function to convert the converted representation of the original valuess in the dictionary, to their original type.</param>
        /// <param name="serializeDictionaryKeys">A function to convert the keys of the dictionary to string values.</param>
        /// <param name="deserializeDictionaryKeys">A function to convert the string representation of the original keys in the dictionary, to their original type.</param>
        public Dictionary<TK, TV> TryGetConfig<TK, TV, TVC>(
            string configName,
            string? expectedVersion,
            IDictionary<TK, TV> defaultContent,
            Func<TV, TVC> serializeDictionaryValues,
            Func<TVC, TV> deserializeDictionaryValues,
            Func<TK, string>? serializeDictionaryKeys = null,
            Func<string, TK>? deserializeDictionaryKeys = null
        )
            where TK : notnull
        {
            try
            {
                return GetConfig(configName, expectedVersion, deserializeDictionaryValues, deserializeDictionaryKeys);
            }
            catch (FileNotFoundException)
            {
                var relativePath = Path.GetRelativePath(Constants.ROOT_FOLDER, Path.Join(_configsFolderPath, $"{configName}.{_configExtension}"));
                PACSingletons.Instance.Logger.Log("Config file error", $"File not found: \"{relativePath}\"", LogSeverity.WARN);
            }
            catch (Exception e)
            {
                PACSingletons.Instance.Logger.Log("Config file error", e.ToString(), LogSeverity.WARN);
            }
            SetConfig(configName, expectedVersion, defaultContent, serializeDictionaryValues, serializeDictionaryKeys);
            return GetConfig(configName, expectedVersion, deserializeDictionaryValues, deserializeDictionaryKeys);
        }

        /// <summary>
        /// Sets the value of an object in a config file.
        /// </summary>
        /// <typeparam name="T">The type of the config data.</typeparam>
        /// <param name="configName">The name of the config file.</param>
        /// <param name="configData">The object to put into the config file.</param>
        /// <param name="configVersion">The version of the config json.</param>
        public void SetConfig<T>(string configName, string? configVersion, T configData)
        {
            Tools.RecreateFolder(_configsFolderPath, "configs");
            var filePath = Path.Join(_configsFolderPath, $"{configName}.{_configExtension}");
            var parrentPath = Path.GetDirectoryName(_configsFolderPath);
            var safeFilePath = parrentPath is null ? filePath : Path.GetRelativePath(parrentPath, filePath);

            var configFileData = new Dictionary<string, object?>
            {
                ["version"] = configVersion,
                ["data"] = configData,
            };
            var jsonString = JsonSerializer.Serialize(configFileData, _jsonWriterOptions);
            File.WriteAllText(filePath, jsonString);
            PACSingletons.Instance.Logger.Log($"Config file recreated", $"file path: \"{safeFilePath}\"");
        }

        /// <summary>
        /// Sets the value of an object in a config file.<br/>
        /// For config objects, where the type of the object is a dictionary, where the keys are not serializable.
        /// </summary>
        /// <typeparam name="TK">The type of the keys in the config data.</typeparam>
        /// <typeparam name="TV">The type of the values in the config data.</typeparam>
        /// <param name="configName">The name of the config file.</param>
        /// <param name="configVersion">The version of the config json.</param>
        /// <param name="configData">The object to put into the config file.</param>
        /// <param name="serializeDictionaryKeys">A function to convert the keys of the dictionary to string values.</param>
        public void SetConfig<TK, TV>(
            string configName,
            string? configVersion,
            IDictionary<TK, TV> configData,
            Func<TK, string> serializeDictionaryKeys
        )
            where TK : notnull
        {
            var tempConfigData = configData.ToDictionary(key => serializeDictionaryKeys(key.Key), value => value.Value);
            SetConfig(configName, configVersion, tempConfigData);
        }

        /// <summary>
        /// Sets the value of an object in a config file.<br/>
        /// For config objects, where the type of the object is a dictionary, where the keys are not serializable.
        /// </summary>
        /// <typeparam name="TK">The type of the keys in the config data.</typeparam>
        /// <typeparam name="TV">The type of the values in the config data.</typeparam>
        /// <typeparam name="TVC">The type of the converted values in the dictionary.</typeparam>
        /// <param name="configName">The name of the config file.</param>
        /// <param name="configVersion">The version of the config json.</param>
        /// <param name="configData">The object to put into the config file.</param>
        /// <param name="serializeDictionaryValues">A function to convert the values of the dictionary.</param>
        /// <param name="serializeDictionaryKeys">A function to convert the keys of the dictionary to string values.</param>
        public void SetConfig<TK, TV, TVC>(
            string configName,
            string? configVersion,
            IDictionary<TK, TV> configData,
            Func<TV, TVC> serializeDictionaryValues,
            Func<TK, string>? serializeDictionaryKeys = null
        )
            where TK : notnull
        {
            if (serializeDictionaryKeys is null)
            {
                var tempconfigData = configData.ToDictionary(
                    key => key.Key,
                    value => serializeDictionaryValues(value.Value)
                );
                SetConfig(configName, configVersion, tempconfigData);
            }
            else
            {
                var tempconfigData = configData.ToDictionary(
                    key => serializeDictionaryKeys(key.Key),
                    value => serializeDictionaryValues(value.Value)
                );
                SetConfig(configName, configVersion, tempconfigData);
            }
        }
        #endregion
    }
}

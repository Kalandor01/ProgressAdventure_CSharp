using PACommon.Enums;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PACommon.ConfigManagement
{
    /// <summary>
    /// Abstract class for reading config files, to loading config dictionaries.
    /// </summary>
    public abstract class AConfigManager : IConfigManager
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
        protected AConfigManager(
            JsonConverter[]? converters = null,
            string? configsFolderPath = null,
            string configExtension = Constants.CONFIG_EXT
        )
        {
            _jsonReaderOptions = new JsonSerializerOptions
            {
                IncludeFields = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
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
        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public string GetConfigFilePath(string configName)
        {
            Tools.RecreateFolder(_configsFolderPath, "configs");
            return Path.Join(_configsFolderPath, $"{configName}.{_configExtension}");
        }

        public bool ConfigFileExists(string configName)
        {
            return File.Exists(GetConfigFilePath(configName));
        }

        #region Get config
        public T GetConfig<T>(string configName, string? expectedVersion)
        {
            Tools.RecreateFolder(_configsFolderPath, "configs");
            var filePath = GetConfigFilePath(configName);
            var parrentPath = Path.GetDirectoryName(_configsFolderPath);
            var safeFilePath = parrentPath is null ? filePath : Path.GetRelativePath(parrentPath, filePath);

            var fileData = File.ReadAllText(filePath);

            T? configData;
            if (expectedVersion is not null)
            {
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

                if (expectedVersion != "" &&
                    !(
                    configVersion is JsonElement versionElement &&
                    versionElement.GetString() is string configVersionStr &&
                    expectedVersion == configVersionStr
                ))
                {
                    throw new FormatException($"Incorrect config file version in \"{safeFilePath}\" ({expectedVersion} => {configVersion?.ToString() ?? "[NULL]"}).");
                }

                configData = configDataJson.Deserialize<T>(_jsonReaderOptions);
            }
            else
            {
                configData = JsonSerializer.Deserialize<T>(fileData, _jsonReaderOptions);
            }

            return configData ?? throw new NullReferenceException($"The config json data is null in \"{safeFilePath}\".");
        }

        public Dictionary<TK, TV> GetConfigDict<TK, TV>(
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

        public Dictionary<TK, TV> GetConfigDict<TK, TV, TVC>(
            string configName,
            string? expectedVersion,
            Func<TVC, TV> deserializeDictionaryValues,
            Func<string, TK>? deserializeDictionaryKeys = null
        )
            where TK : notnull
        {
            if (deserializeDictionaryKeys is null && typeof(TK) != typeof(string))
            {
                throw new ArgumentException("The type of the key must be string if no key deserializer is porvided!", nameof(deserializeDictionaryKeys));
            }

            var tempResult = GetConfig<IDictionary<string, TVC>>(configName, expectedVersion)
                ?? throw new ArgumentNullException("The deserialized config is null!");
            return tempResult.ToDictionary(
                key => deserializeDictionaryKeys is null ? (TK)(object)key.Key : deserializeDictionaryKeys(key.Key),
                value => deserializeDictionaryValues(value.Value)
            );
        }
        #endregion

        #region Try get config
        public bool TryGetConfig<T>(
            string configName,
            string? expectedVersion,
            [NotNullWhen(true)] out T? configValue
        )
        {
            return TryGetConfigPrivate(
                configName,
                () => GetConfig<T>(configName, expectedVersion),
                out configValue
            );
        }

        public bool TryGetConfigDict<TK, TV>(
            string configName,
            string? expectedVersion,
            [NotNullWhen(true)] out Dictionary<TK, TV>? configValue,
            Func<string, TK> deserializeDictionaryKeys
        )
            where TK : notnull
        {
            return TryGetConfigPrivate(
                configName,
                () => GetConfigDict<TK, TV>(configName, expectedVersion, deserializeDictionaryKeys),
                out configValue
            );
        }

        public bool TryGetConfigDict<TK, TV, TVC>(
            string configName,
            string? expectedVersion,
            [NotNullWhen(true)] out Dictionary<TK, TV>? configValue,
            Func<TVC, TV> deserializeDictionaryValues,
            Func<string, TK>? deserializeDictionaryKeys = null
        )
            where TK : notnull
        {
            return TryGetConfigPrivate(
                configName,
                () => GetConfigDict(configName, expectedVersion, deserializeDictionaryValues, deserializeDictionaryKeys),
                out configValue
            );
        }
        #endregion

        #region Try get config or recreate
        public T TryGetConfigOrRecreate<T>(
            string configName,
            string? expectedVersion,
            T defaultContent,
            bool justRecreate = false
        )
        {
            if (
                !justRecreate &&
                TryGetConfig(
                    configName,
                    expectedVersion,
                    out T? configValue
                )
            )
            {
                return configValue;
            }
            SetConfig(configName, expectedVersion, defaultContent);
            return GetConfig<T>(configName, expectedVersion);
        }

        public Dictionary<TK, TV> TryGetConfigOrRecreateDict<TK, TV>(
            string configName,
            string? expectedVersion,
            IDictionary<TK, TV> defaultContent,
            Func<TK, string> serializeDictionaryKeys,
            Func<string, TK> deserializeDictionaryKeys,
            bool justRecreate = false
        )
            where TK : notnull
        {
            if (
                !justRecreate &&
                TryGetConfigDict(
                    configName,
                    expectedVersion,
                    out Dictionary<TK, TV>? configValue,
                    deserializeDictionaryKeys
                )
            )
            {
                return configValue;
            }
            SetConfigDict(configName, expectedVersion, defaultContent, serializeDictionaryKeys);
            return GetConfigDict<TK, TV>(configName, expectedVersion, deserializeDictionaryKeys);
        }

        public Dictionary<TK, TV> TryGetConfigOrRecreateDict<TK, TV, TVC>(
            string configName,
            string? expectedVersion,
            IDictionary<TK, TV> defaultContent,
            Func<TV, TVC> serializeDictionaryValues,
            Func<TVC, TV> deserializeDictionaryValues,
            Func<TK, string>? serializeDictionaryKeys = null,
            Func<string, TK>? deserializeDictionaryKeys = null,
            bool justRecreate = false
        )
            where TK : notnull
        {
            if (
                !justRecreate &&
                TryGetConfigDict(
                    configName,
                    expectedVersion,
                    out Dictionary<TK, TV>? configValue,
                    deserializeDictionaryValues,
                    deserializeDictionaryKeys
                )
            )
            {
                return configValue;
            }
            SetConfigDict(configName, expectedVersion, defaultContent, serializeDictionaryValues, serializeDictionaryKeys);
            return GetConfigDict(configName, expectedVersion, deserializeDictionaryValues, deserializeDictionaryKeys);
        }
        #endregion

        #region Set config
        public void SetConfig<T>(string configName, string? configVersion, T configData)
        {
            Tools.RecreateFolder(_configsFolderPath, "configs");
            var filePath = GetConfigFilePath(configName);
            var configFolder = Path.GetDirectoryName(filePath);
            if (configFolder is not null && configFolder != _configsFolderPath)
            {
                Tools.RecreateFolder(configFolder, $"{Path.GetFileNameWithoutExtension(configFolder)} config");
            }
            var parrentPath = Path.GetDirectoryName(_configsFolderPath);
            var safeFilePath = parrentPath is null ? filePath : Path.GetRelativePath(parrentPath, filePath);

            string jsonString;
            if (configVersion is null)
            {
                jsonString = JsonSerializer.Serialize(configData, _jsonWriterOptions);
            }
            else
            {
                var configFileData = new Dictionary<string, object?>
                {
                    ["version"] = configVersion,
                    ["data"] = configData,
                };
                jsonString = JsonSerializer.Serialize(configFileData, _jsonWriterOptions);
            }
            File.WriteAllText(filePath, jsonString);
            PACSingletons.Instance.Logger.Log($"Config file recreated", $"file path: \"{safeFilePath}\"");
        }

        public void SetConfigDict<TK, TV>(
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

        public void SetConfigDict<TK, TV, TVC>(
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
        #endregion

        #region Private functions
        /// <summary>
        /// Tries to get the value of a config object, and logs any errors if it doesn't work.
        /// </summary>
        /// <typeparam name="T">The type of the config object.</typeparam>
        /// <param name="configName">The name of the config file.</param>
        /// <param name="getConfigMethod">The method to get the config data.</param>
        /// <param name="configValue">The default config object value.</param>
        private bool TryGetConfigPrivate<T>(string configName, Func<T> getConfigMethod, [NotNullWhen(true)] out T? configValue)
        {
            configValue = default;
            try
            {
                configValue = getConfigMethod();
                if (configValue is null)
                {
                    var fileRelativePath = Path.GetRelativePath(Constants.ROOT_FOLDER, GetConfigFilePath(configName));
                    throw new ArgumentNullException($"The returned config value from \"{fileRelativePath}\" is somehow null!");
                }
                return true;
            }
            catch (FileNotFoundException)
            {
                var fileRelativePath = Path.GetRelativePath(Constants.ROOT_FOLDER, GetConfigFilePath(configName));
                PACSingletons.Instance.Logger.Log("Config file error", $"File not found: \"{fileRelativePath}\"", LogSeverity.WARN);
            }
            catch (DirectoryNotFoundException)
            {
                var folderRelativePath = Path.GetRelativePath(Constants.ROOT_FOLDER, Path.GetDirectoryName(GetConfigFilePath(configName)) ?? "");
                PACSingletons.Instance.Logger.Log("Config folder error", $"Folder not found: \"{folderRelativePath}\"", LogSeverity.WARN);
            }
            catch (FormatException fe)
            {
                PACSingletons.Instance.Logger.Log("Config file error", fe.Message, LogSeverity.WARN);
            }
            catch (Exception e)
            {
                PACSingletons.Instance.Logger.Log("Config file error", e.ToString(), LogSeverity.WARN);
            }
            return false;
        }
        #endregion
    }
}

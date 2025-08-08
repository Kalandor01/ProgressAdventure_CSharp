using PACommon;
using PACommon.JsonUtils;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace ProgressAdventure.ConfigManagement
{
    /// <summary>
    /// Class for storing the data in a config's data file.
    /// </summary>
    public class ConfigData : IJsonConvertableExtra<ConfigData, string>
    {
        /// <summary>
        /// The name of the folder that the config is located in the configs folder.
        /// </summary>
        public readonly string FolderName;
        /// <summary>
        /// The unique namespace of the config.
        /// </summary>
        public readonly string Namespace;
        /// <summary>
        /// The format version of the config.
        /// </summary>
        public readonly string Format;
        /// <summary>
        /// The version number of the config.
        /// </summary>
        public readonly string Version;
        /// <summary>
        /// The list of config namespaces that this config depends on.
        /// </summary>
        public readonly ReadOnlyCollection<string> Dependencies;

        /// <summary>
        /// <inheritdoc cref="ConfigData" path="//summary"/>
        /// </summary>
        /// <param name="configFolderName"><inheritdoc cref="FolderName" path="//summary"/></param>
        /// <param name="namespace"><inheritdoc cref="Namespace" path="//summary"/></param>
        /// <param name="format"><inheritdoc cref="Format" path="//summary"/></param>
        /// <param name="version"><inheritdoc cref="Version" path="//summary"/></param>
        /// <param name="dependencies"><inheritdoc cref="Dependencies" path="//summary"/></param>
        /// <exception cref="ArgumentException">If the namespace is invaid, or the config folder name is empty.</exception>
        public ConfigData(string configFolderName, string @namespace, string format, string version, IList<string> dependencies)
        {
            if (string.IsNullOrWhiteSpace(configFolderName))
            {
                throw new ArgumentException($"'{nameof(configFolderName)}' cannot be null or whitespace.", nameof(configFolderName));
            }

            FolderName = configFolderName;
            Namespace = ConfigUtils.NamespaceRegex().IsMatch(@namespace)
                ? @namespace
                : throw new ArgumentException("Invalid namespace name", nameof(@namespace));
            Format = format;
            Version = version;
            Dependencies = dependencies.AsReadOnly();
        }

        /// <summary>
        /// <inheritdoc cref="ConfigData" path="//summary"/>
        /// </summary>
        /// <param name="configFolderName"><inheritdoc cref="FolderName" path="//summary"/></param>
        /// <param name="namespace"><inheritdoc cref="Namespace" path="//summary"/></param>
        /// <param name="format"><inheritdoc cref="Format" path="//summary"/></param>
        /// <param name="version"><inheritdoc cref="Version" path="//summary"/></param>
        public ConfigData(string configFolderName, string @namespace, string format, string version)
            : this(configFolderName, @namespace, format, version, []) { }

        public override string? ToString()
        {
            return $"\"{FolderName}\"({Namespace}): {Format}-{Version}";
        }

        #region JsonConvert
        static List<(Action<JsonDictionary, string> objectJsonCorrecter, string newFileVersion)> IJsonConvertableExtra<ConfigData, string>.VersionCorrecters { get; } =
        [
            // v2 -> v3
            ((oldJson, folderName) =>
            {
                oldJson["dependencies"] = new JsonArray();
            }, "v3"),
            // v8 -> v9
            ((oldJson, folderName) =>
            {
                // format and version split
                JsonDataCorrecterUtils.RenameKeyIfExists(oldJson, "version", "format");
                oldJson["version"] = "1." +
                (
                    oldJson.TryGetValue("format", out var formatJson) &&
                    formatJson?.ToString() is string format &&
                    format.Length > 1
                        ? format[1..]
                        : "0"
                );
            }, "v9"),
        ];

        public JsonDictionary ToJson()
        {
            return new JsonDictionary
            {
                [Constants.JsonKeys.ConfigData.NAMESPACE] = Namespace,
                [Constants.JsonKeys.ConfigData.FORMAT] = Format,
                [Constants.JsonKeys.ConfigData.VERSION] = Version,
                [Constants.JsonKeys.ConfigData.DEPENDENCIES] = Dependencies.Select(dep => (JsonObject?)dep).ToList(),
            };
        }

        public static bool FromJsonWithoutCorrection(
            JsonDictionary configJson,
            string folderName,
            string fileVersion,
            [NotNullWhen(true)] ref ConfigData? convertedObject
        )
        {
            if (string.IsNullOrWhiteSpace(folderName))
            {
                PACommon.Tools.LogJsonTypeParseError("invalid extra data for this type", true);
                return false;
            }

            if (
                !PACommon.Tools.TryParseJsonValue<string>(configJson, Constants.JsonKeys.ConfigData.NAMESPACE, out var namespaceName, isCritical: true) ||
                !ConfigUtils.NamespaceRegex().IsMatch(namespaceName)
            )
            {
                PACommon.Tools.LogJsonParseError(nameof(namespaceName), $"invalid namespace name: \"{namespaceName}\"", true);
                return false;
            }

            if (
                !PACommon.Tools.TryParseJsonValue<string>(configJson, Constants.JsonKeys.ConfigData.FORMAT, out var format, isCritical: true) ||
                !PACommon.Tools.TryParseJsonValue<string>(configJson, Constants.JsonKeys.ConfigData.VERSION, out var version, isCritical: true) ||
                !PACommon.Tools.TryParseJsonListValue(configJson, Constants.JsonKeys.ConfigData.DEPENDENCIES,
                    dependency => {
                        if (
                            PACommon.Tools.TryParseValueForJsonParsing<string>(dependency, out var value) &&
                            ConfigUtils.NamespaceRegex().IsMatch(value)
                        )
                        {
                            return (true, value);
                        }

                        PACommon.Tools.LogJsonParseError(nameof(namespaceName), $"invalid dependency name: \"{value}\" for namespace \"{namespaceName}\"", true);
                        return (false, null);
                    },
                    out var dependencies, true
                )
            )
            {
                return false;
            }

            convertedObject = new ConfigData(folderName, namespaceName, format, version, dependencies);
            return true;
        }
        #endregion

        /// <summary>
        /// Writes the config data to a config data file.
        /// </summary>
        /// <param name="configFolderName">The name of the config folder to write the config data to.</param>
        public void SerializeToFile()
        {
            var namespaceFolder = Path.Join(Constants.CONFIGS_FOLDER_PATH, FolderName);
            PACommon.Tools.RecreateFolder(namespaceFolder, $"{Namespace} namespace");
            var dataFilePath = Path.Join(namespaceFolder, Constants.CONFIG_FILE_NAME_DATA);
            var jsonData = ToJson();
            PACommon.Tools.SaveJsonFile(jsonData, dataFilePath, Constants.CONFIG_EXT, true);
        }

        /// <summary>
        /// Gets the config data from a config data file.
        /// </summary>
        /// <param name="configFolderName">The name of the config folder to get the config data from.</param>
        public static ConfigData? DeserializeFromFile(string configFolderName)
        {
            var dataFilePath = Path.Join(Constants.CONFIGS_FOLDER_PATH, configFolderName, Constants.CONFIG_FILE_NAME_DATA);
            if (!File.Exists(dataFilePath + $".{Constants.CONFIG_EXT}"))
            {
                return null;
            }

            JsonDictionary? configJson;
            try
            {
                configJson = PACommon.Tools.LoadJsonFile(dataFilePath, null, Constants.CONFIG_EXT);
            }
            catch (Exception ex)
            {
                PACSingletons.Instance.Logger.Log("Config data file parse error", ex.ToString(), PACommon.Enums.LogSeverity.ERROR);
                return null;
            }

            var configFormat = Constants.OLDEST_CONFIG_FORMAT_VERSION;
            if (
                configJson is not null &&
                configJson.TryGetValue(Constants.JsonKeys.ConfigData.FORMAT, out var configFormatJs) &&
                configFormatJs?.ToString() is string configFormatStr
            )
            {
                configFormat = configFormatStr;
            }
            else if (
                configJson is not null &&
                !configJson.ContainsKey(Constants.JsonKeys.ConfigData.FORMAT) &&
                configJson.TryGetValue(Constants.JsonKeys.ConfigData.VERSION, out var configOldFormatJs) &&
                configOldFormatJs?.ToString() is string configOldFormatStr
            )
            {
                configFormat = configOldFormatStr;
            }
            else
            {
                PACommon.Tools.LogJsonParseError(nameof(configFormat), "assuming minimum config format");
            }

            return PACommon.Tools.TryFromJsonExtra(
                configJson,
                configFolderName,
                configFormat,
                out ConfigData? configData
            ) ? configData : null;
        }
    }
}

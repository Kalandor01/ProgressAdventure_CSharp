using PACommon;
using PACommon.JsonUtils;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace ProgressAdventure.ConfigManagement
{
    public class ConfigData : IJsonConvertableExtra<ConfigData, string>
    {
        public readonly string FolderName;
        public readonly string Namespace;
        public readonly string Version;
        public readonly ReadOnlyCollection<string> Dependencies;

        public ConfigData(string configFolderName, string @namespace, string version, IList<string> dependencies)
        {
            if (string.IsNullOrWhiteSpace(configFolderName))
            {
                throw new ArgumentException($"'{nameof(configFolderName)}' cannot be null or whitespace.", nameof(configFolderName));
            }

            FolderName = configFolderName;
            Namespace = ConfigUtils.NamespaceRegex().IsMatch(@namespace)
                ? @namespace
                : throw new ArgumentException("Invalid namespace name", nameof(@namespace));
            Version = version;
            Dependencies = dependencies.AsReadOnly();
        }

        public ConfigData(string configFolderName, string @namespace, string version)
            : this(configFolderName, @namespace, version, []) { }

        public override string? ToString()
        {
            return $"\"{FolderName}\"({Namespace}): {Version}";
        }

        #region JsonConvert
        static List<(Action<JsonDictionary, string> objectJsonCorrecter, string newFileVersion)> IJsonConvertableExtra<ConfigData, string>.VersionCorrecters { get; } =
        [
            // v2 -> v3
            ((oldJson, folderName) =>
            {
                oldJson["dependencies"] = new JsonArray();
            }, "v3"),
        ];

        public JsonDictionary ToJson()
        {
            return new JsonDictionary
            {
                [Constants.JsonKeys.ConfigData.NAMESPACE] = Namespace,
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
            if (
                !PACommon.Tools.TryParseJsonValue<ConfigData, string>(configJson, Constants.JsonKeys.ConfigData.NAMESPACE, out var namespaceName, isCritical: true) ||
                !ConfigUtils.NamespaceRegex().IsMatch(namespaceName)
            )
            {
                PACommon.Tools.LogJsonParseError<ConfigData>(nameof(namespaceName), $"invalid namespace name: \"{namespaceName}\"", true);
                return false;
            }

            if (
                !PACommon.Tools.TryParseJsonValue<ConfigData, string>(configJson, Constants.JsonKeys.ConfigData.VERSION, out var version, isCritical: true) ||
                !PACommon.Tools.TryParseJsonListValue<ConfigData, string>(configJson, Constants.JsonKeys.ConfigData.DEPENDENCIES,
                    dependency => {
                        if (
                            PACommon.Tools.TryParseValueForJsonParsing<ConfigData, string>(dependency, out var value) &&
                            ConfigUtils.NamespaceRegex().IsMatch(value)
                        )
                        {
                            return (true, value);
                        }

                        PACommon.Tools.LogJsonParseError<ConfigData>(nameof(namespaceName), $"invalid dependency name: \"{value}\" for namespace \"{namespaceName}\"", true);
                        return (false, null);
                    },
                    out var dependencies, true
                )
            )
            {
                return false;
            }

            convertedObject = new ConfigData(folderName, namespaceName, version, dependencies);
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

            var configVersion = Constants.OLDEST_CONFIG_VERSION;
            if (
                configJson?.TryGetValue(Constants.JsonKeys.ConfigData.VERSION, out var configVersionJs) == true &&
                configVersionJs?.ToString() is string configVersionStr
            )
            {
                configVersion = configVersionStr;
            }
            else
            {
                PACommon.Tools.LogJsonParseError<ConfigData>(nameof(configVersion), "assuming minimum config version");
            }

            return PACommon.Tools.TryFromJsonExtra(
                configJson,
                configFolderName,
                configVersion,
                out ConfigData? configData
            ) ? configData : null;
        }
    }
}

using PACommon;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProgressAdventure.ConfigManagement
{
    public class ConfigData
    {
        private static readonly JsonSerializerOptions _readerOptions = new JsonSerializerOptions { IncludeFields = true };
        private static readonly JsonSerializerOptions _writerOptions = new JsonSerializerOptions { IncludeFields = true, WriteIndented = true };

        [JsonPropertyName("namespace")]
        public readonly string Namespace;

        [JsonPropertyName("version")]
        public readonly string Version;

        [JsonConstructor]
        public ConfigData(string @namespace, string version)
        {
            Namespace = ConfigUtils.NamespaceRegex().IsMatch(@namespace)
                ? @namespace
                : throw new ArgumentException("Invalid namespace name", nameof(@namespace));
            Version = version;
        }

        public override string? ToString()
        {
            return $"{Namespace}: {Version}";
        }

        /// <summary>
        /// Writes the config data to a config data file.
        /// </summary>
        /// <param name="configFolderName">The name of the config folder to write the config data to.</param>
        public void SerializeToFile(string configFolderName)
        {
            var namespaceFolder = Path.Join(Constants.CONFIGS_FOLDER_PATH, configFolderName);
            PACommon.Tools.RecreateFolder(namespaceFolder, $"{Namespace} namespace");
            var dataFilePath = Path.Join(namespaceFolder, $"{Constants.CONFIG_FILE_NAME_DATA}.{Constants.CONFIG_EXT}");
            var jsonData = JsonSerializer.Serialize(this, _writerOptions);
            File.WriteAllText(dataFilePath, jsonData);
        }

        /// <summary>
        /// Gets the config data from a config data file.
        /// </summary>
        /// <param name="configFolderName">The name of the config folder to get the config data from.</param>
        public static ConfigData? DeserializeFromFile(string configFolderName)
        {
            var dataFilePath = Path.Join(Constants.CONFIGS_FOLDER_PATH, configFolderName, $"{Constants.CONFIG_FILE_NAME_DATA}.{Constants.CONFIG_EXT}");
            if (!File.Exists(dataFilePath))
            {
                return null;
            }

            try
            {
                var jsonData = File.ReadAllText(dataFilePath);
                return JsonSerializer.Deserialize<ConfigData>(jsonData, _readerOptions);
            }
            catch (Exception ex)
            {
                PACSingletons.Instance.Logger.Log("Config data file parse error", ex.ToString(), PACommon.Enums.LogSeverity.ERROR);
                return null;
            }
        }
    }
}

using PACommon;

namespace ProgressAdventure.ConfigManagement
{
    public class ConfigDataFull
    {
        public readonly string folderName;
        public readonly ConfigData configData;

        public ConfigDataFull(string folderName, ConfigData configData)
        {
            this.folderName = folderName;
            this.configData = configData;
        }

        /// <summary>
        /// Gets the config data from a config data file.
        /// </summary>
        /// <param name="configFolderName">The name of the config folder to get the config data from.</param>
        public static ConfigDataFull? DeserializeFromFile(string configFolderName)
        {
            var data = ConfigData.DeserializeFromFile(configFolderName);
            return data == null ? null : new ConfigDataFull(configFolderName, data);
        }

        public override string? ToString()
        {
            return $"\"{folderName}\" -> {configData}";
        }
    }
}

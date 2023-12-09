using Newtonsoft.Json;
using PACommon;
using PACConstants = PACommon.Constants;

namespace ProgressAdventure.ConfigManagement
{
    /// <summary>
    /// Class for reading config files, for loading config dictionaries.
    /// </summary>
    public static class ConfigManager
    {
        #region Public functions
        public static T TryGetConfig<T>(string configName, T defaultContent)
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

        public static T GetConfig<T>(string configName)
        {
            PACommon.Tools.RecreateFolder(Constants.CONFIGS_FOLDER);
            var filePath = Path.Join(Constants.CONFIGS_FOLDER_PATH, $"{configName}.{Constants.CONFIG_EXT}");
            var safeFilePath = Path.GetRelativePath(PACConstants.ROOT_FOLDER, filePath);

            var fileData = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<T>(fileData) ?? throw new NullReferenceException($"The config json data is null in \"{safeFilePath}\".");
        }
        #endregion

        #region Priate functions
        private static void SetConfig<T>(string configName, T configData)
        {
            PACommon.Tools.RecreateFolder(Constants.CONFIGS_FOLDER);
            var filePath = Path.Join(Constants.CONFIGS_FOLDER_PATH, $"{configName}.{Constants.CONFIG_EXT}");
            var safeFilePath = Path.GetRelativePath(PACConstants.ROOT_FOLDER, filePath);

            var jsonString = JsonConvert.SerializeObject(configData);
            File.WriteAllText(filePath, jsonString);
            PACSingletons.Instance.Logger.Log($"Config file recreated", $"file path: \"{safeFilePath}\"");
        }
        #endregion
    }
}

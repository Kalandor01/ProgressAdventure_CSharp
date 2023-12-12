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
        #region Private fields
        private static readonly JsonConverter[] _converters = new JsonConverter[]
        {
            new ItemTypeIDConverter(),
            new CompoundItemAttributesDTOConverter(),
            new MaterialItemAttributesDTOConverter(),
            new IngredientDTOConverter(),
            new EnumConverter(),
        };
        #endregion

        #region Public functions
        public static T GetConfig<T>(string configName)
        {
            PACommon.Tools.RecreateFolder(Constants.CONFIGS_FOLDER);
            var filePath = Path.Join(Constants.CONFIGS_FOLDER_PATH, $"{configName}.{Constants.CONFIG_EXT}");
            var safeFilePath = Path.GetRelativePath(PACConstants.ROOT_FOLDER, filePath);

            var fileData = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<T>(fileData, _converters) ?? throw new NullReferenceException($"The config json data is null in \"{safeFilePath}\".");
        }

        public static T GetConfig<T, TK, TV>(string configName, Func<string, TK> convertFrom)
            where T : Dictionary<TK, TV>
            where TK : notnull
        {
            var tempResult = GetConfig<Dictionary<string, TV>>(configName);
            return (T)(tempResult?.ToDictionary(key => convertFrom(key.Key), value => value.Value) ?? throw new ArgumentNullException(nameof(tempResult)));
        }

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
        
        public static T TryGetConfig<T, TK, TV>(string configName, T defaultContent, Func<TK, string> convertTo, Func<string, TK> convertFrom)
            where T : Dictionary<TK, TV>
            where TK : notnull
        {
            try
            {
                return GetConfig<T, TK, TV>(configName, convertFrom);
            }
            catch
            {
                var tempDefaultContent = defaultContent.ToDictionary(key => convertTo(key.Key), value => value.Value);
                SetConfig(configName, tempDefaultContent);
                return GetConfig<T, TK, TV>(configName, convertFrom);
            }
        }
        #endregion

        #region Internal functions
        internal static JsonConverter[] GetConvertersNonInclusive<T>()
            where T : JsonConverter
        {
            return _converters.Where(converter => converter.GetType() != typeof(T)).ToArray();
        }
        #endregion

        #region Priate functions
        private static void SetConfig<T>(string configName, T configData)
        {
            PACommon.Tools.RecreateFolder(Constants.CONFIGS_FOLDER);
            var filePath = Path.Join(Constants.CONFIGS_FOLDER_PATH, $"{configName}.{Constants.CONFIG_EXT}");
            var safeFilePath = Path.GetRelativePath(PACConstants.ROOT_FOLDER, filePath);

            var jsonString = JsonConvert.SerializeObject(configData, _converters);
            File.WriteAllText(filePath, jsonString);
            PACSingletons.Instance.Logger.Log($"Config file recreated", $"file path: \"{safeFilePath}\"");
        }
        #endregion
    }
}

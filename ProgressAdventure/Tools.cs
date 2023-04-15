using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProgressAdventure.Enums;
using SaveFileManager;
using System.Collections;

namespace ProgressAdventure
{
    /// <summary>
    /// Contains project specific useful functions.
    /// </summary>
    public static class Tools
    {
        #region Public functions
        /// <param name="data">The list of data to write to the file, where each element of the list is a line.</param>
        /// <inheritdoc cref="EncodeSaveShort(IEnumerable{IDictionary}, string, long?, string?)"/>
        public static void EncodeSaveShort(IDictionary data, string filePath, long? seed = null, string? extension = null)
        {
            EncodeSaveShort(new List<IDictionary> { data }, filePath, seed, extension);
        }

        /// <summary>
        /// Shorthand for <c>EncodeFile</c> + convert from json to string.
        /// </summary>
        /// <param name="dataList">The data to write to the file.</param>
        /// <param name="filePath">The path and the name of the file without the extension, that will be created.<br/>
        /// If the path contains a *, it will be replaced with the seed.</param>
        /// <param name="seed">The seed for encoding the file.</param>
        /// <param name="extension">The extension of the file that will be created.</param>
        public static void EncodeSaveShort(IEnumerable<IDictionary> dataList, string filePath, long? seed = null, string? extension = null)
        {
            seed ??= Constants.SAVE_SEED;
            extension ??= Constants.SAVE_EXT;
            // convert from json to string
            var JsonDataList = new List<string>();
            foreach (var data in dataList)
            {
                JsonDataList.Add(JsonConvert.SerializeObject(data));
            }
            FileConversion.EncodeFile(JsonDataList, (long)seed, filePath, extension, Constants.FILE_ENCODING_VERSION, Constants.ENCODING);
        }

        /// <summary>
        /// Shorthand for <c>DecodeFile</c> + convert from string to json.
        /// </summary>
        /// <param name="filePath">The path and the name of the file without the extension, that will be decoded.<br/>
        /// If the path contains a *, it will be replaced with the seed.</param>
        /// <param name="lineNum">The line, that you want go get back (starting from 0).</param>
        /// <param name="seed">The seed for decoding the file.</param>
        /// <param name="extension">The extension of the file that will be decoded.</param>
        /// <exception cref="FormatException">Exeption thrown, if the file couldn't be decode.</exception>
        /// <exception cref="FileNotFoundException">Exeption thrown, if the file couldn't be found.</exception>
        public static Dictionary<string, object?>? DecodeSaveShort(string filePath, int lineNum = 0, long? seed = null, string? extension = null)
        {
            seed ??= Constants.SAVE_SEED;
            extension ??= Constants.SAVE_EXT;
            var safeFilePath = Path.GetRelativePath(Constants.ROOT_FOLDER, filePath);

            string decodedLine;
            try
            {
                decodedLine = FileConversion.DecodeFile((long)seed, filePath, extension, lineNum + 1, Constants.ENCODING).Last();
            }
            catch (FormatException)
            {
                Logger.Log("Decode error", $"file name: {safeFilePath}.{Constants.SAVE_EXT}", LogSeverity.ERROR);
                throw;
            }
            catch (FileNotFoundException)
            {
                Logger.Log("File not found", $"file name: {safeFilePath}.{Constants.SAVE_EXT}", LogSeverity.ERROR);
                throw;
            }
            try
            {
                return DeserializeJson(decodedLine);
            }
            catch (Exception)
            {
                Logger.Log("Json decode error", $"file name: {safeFilePath}.{Constants.SAVE_EXT}", LogSeverity.ERROR);
                throw;
            }
        }

        /// <summary>
        /// Recreates the folder, if it doesn't exist.
        /// </summary>
        /// <param name="folderName">The name of the folder to check.</param>
        /// <param name="parentFolderPath">The path to the parrent folder, where the folder should be located.</param>
        /// <param name="displayName">The display name of the folder, for the logger, if it needs to be recreated.</param>
        /// <returns>If the folder needed to be recreated.</returns>
        public static bool RecreateFolder(string folderName, string? parentFolderPath = null, string? displayName = null)
        {
            parentFolderPath ??= Constants.ROOT_FOLDER;
            displayName ??= folderName.ToLower();

            var folderPath = Path.Join(parentFolderPath, folderName);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                Logger.Log($"Recreating {displayName} folder");
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// <c>RecreateFolder</c> for the saves folder.
        /// </summary>
        public static bool RecreateSavesFolder()
        {
            return RecreateFolder(Constants.SAVES_FOLDER);
        }

        /// <summary>
        /// <c>RecreateFolder</c> for the backups folder.
        /// </summary>
        public static bool RecreateBackupsFolder()
        {
            return RecreateFolder(Constants.BACKUPS_FOLDER);
        }

        /// <summary>
        /// <c>RecreateFolder</c> for the logs folder.
        /// </summary>
        public static bool RecreateLogsFolder()
        {
            return RecreateFolder(Constants.LOGS_FOLDER);
        }

        /// <summary>
        /// Turns the json string into a dictionary.
        /// </summary>
        /// <param name="jsonString">The json string.</param>
        public static Dictionary<string, object?>? DeserializeJson(string jsonString)
        {
            var partialDict = JsonConvert.DeserializeObject<Dictionary<string, object?>>(jsonString);
            return partialDict is not null ? DeserializePartialJTokenDict(partialDict) : null;
        }

        /// <summary>
        /// Returns the value of the JToken
        /// </summary>
        /// <param name="token">The JToken to deserialize.</param>
        public static object? DeserializeJToken(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.None:
                    return null;
                case JTokenType.Object:
                    var partialDict = token.ToObject<Dictionary<string, object?>>();
                    return partialDict is not null ? DeserializePartialJTokenDict(partialDict) : null;
                case JTokenType.Array:
                    var partialList = token.ToObject<List<object?>>();
                    return partialList is not null ? DeserializePartialJTokenList(partialList) : null;
                case JTokenType.Constructor:
                    return ((JConstructor)token).ToString();
                case JTokenType.Property:
                    var prop = (JProperty)token;
                    return DeserializeJToken(prop.Value);
                case JTokenType.Comment:
                    return token.ToString();
                case JTokenType.Integer:
                    return (int)token;
                case JTokenType.Float:
                    return (float)token;
                case JTokenType.String:
                    return token.ToString();
                case JTokenType.Boolean:
                    return (bool)token;
                case JTokenType.Null:
                    return null;
                case JTokenType.Undefined:
                    Logger.Log("Undefined JToken value", token.ToString(), LogSeverity.WARN);
                    return null;
                case JTokenType.Date:
                    return (DateTime)token;
                case JTokenType.Raw:
                    return token.ToString();
                case JTokenType.Bytes:
                    return (byte)token;
                case JTokenType.Guid:
                    return (Guid)token;
                case JTokenType.Uri:
                    return token.ToString();
                case JTokenType.TimeSpan:
                    return (TimeSpan)token;
                default:
                    return token;
            }
        }
        #endregion

        #region Private functions
        /// <summary>
        /// Turns the JTokens in a dictionary into the value of the JToken. 
        /// </summary>
        /// <param name="partialJsonDict">The dictionary containing values, including JTokens.</param>
        private static Dictionary<string, object?> DeserializePartialJTokenDict(IDictionary<string, object?> partialJsonDict)
        {
            var jsonDict = new Dictionary<string, object?>();
            foreach (var kvPair in partialJsonDict)
            {
                object? kvValue;
                if (
                    kvPair.Value is not null &&
                    typeof(JToken).IsAssignableFrom(kvPair.Value.GetType())
                )
                {
                    kvValue = DeserializeJToken((JToken)kvPair.Value);
                }
                else
                {
                    kvValue = kvPair.Value;
                }
                jsonDict.Add(kvPair.Key, kvValue);
            }
            return jsonDict;
        }

        /// <summary>
        /// Turns the JTokens in a list into the value of the JToken. 
        /// </summary>
        /// <param name="partialJsonList">The list containing values, including JTokens.</param>
        private static List<object?> DeserializePartialJTokenList(IEnumerable<object?> partialJsonList)
        {
            var jsonList = new List<object?>();
            foreach (var element in partialJsonList)
            {
                object? value;
                if (
                    element is not null &&
                    typeof(JToken).IsAssignableFrom(element.GetType())
                )
                {
                    value = DeserializeJToken((JToken)element);
                }
                else
                {
                    value = element;
                }
                jsonList.Add(value);
            }
            return jsonList;
        }
        #endregion
    }
}
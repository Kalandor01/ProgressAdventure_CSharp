using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProjectAdventure.Enums;
using SaveFileManager;

namespace ProjectAdventure
{
    /// <summary>
    /// Contains project specific useful functions.
    /// </summary>
    public class Tools
    {
        #region Public functions
        /// <param name="data">The list of data to write to the file, where each element of the list is a line.</param>
        /// <inheritdoc cref="EncodeSaveShort(IEnumerable{IDictionary{string, object?}}, string, long?, string?)"/>
        public static void EncodeSaveShort(IDictionary<string, object?> data, string filePath, long? seed = null, string? extension = null)
        {
            EncodeSaveShort(new List<IDictionary<string, object?>> { data }, filePath, seed, extension);
        }

        /// <summary>
        /// Shorthand for <c>EncodeFile</c> + convert from json to string.
        /// </summary>
        /// <param name="dataList">The data to write to the file.</param>
        /// <param name="filePath">The path and the name of the file without the extension, that will be created.<br/>
        /// If the path contains a *, it will be replaced with the seed.</param>
        /// <param name="seed">The seed for encoding the file.</param>
        /// <param name="extension">The extension of the file that will be created.</param>
        public static void EncodeSaveShort(IEnumerable<IDictionary<string, object?>> dataList, string filePath, long? seed = null, string? extension = null)
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
        /// <returns></returns>
        public static JObject DecodeSaveShort(string filePath, int lineNum = 0, long? seed = null, string? extension = null)
        {
            seed ??= Constants.SAVE_SEED;
            extension ??= Constants.SAVE_EXT;
            try
            {
                var decodedLines = FileConversion.DecodeFile((long)seed, filePath, extension, lineNum + 1, Constants.ENCODING);
                return (JObject)JsonConvert.DeserializeObject(decodedLines.Last());
            }
            catch (FormatException)
            {
                var safeFilePath = Path.GetRelativePath(Constants.ROOT_FOLDER, filePath);
                Logger.Log("Decode error", $"file name: {safeFilePath}.{Constants.SAVE_EXT}", LogSeverity.ERROR);
                throw;
            }
            catch (FileNotFoundException)
            {
                var safeFilePath = Path.GetRelativePath(Constants.ROOT_FOLDER, filePath);
                Logger.Log("File not found", $"file name: {safeFilePath}.{Constants.SAVE_EXT}", LogSeverity.ERROR);
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
        #endregion
    }
}
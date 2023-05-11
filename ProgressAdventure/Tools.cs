using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NPrng;
using NPrng.Generators;
using NPrng.Serializers;
using ProgressAdventure.Enums;
using SaveFileManager;
using System.Collections;
using System.IO.Compression;
using System.Reflection;

namespace ProgressAdventure
{
    /// <summary>
    /// Contains project specific useful functions.
    /// </summary>
    public static class Tools
    {
        #region Public functions
        #region Short
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
        /// <exception cref="DirectoryNotFoundException">Exeption thrown, if the directory containing the file couldn't be found.</exception>
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
            catch (DirectoryNotFoundException)
            {
                Logger.Log("Folder containing file not found", $"folder name: {safeFilePath}.{Constants.SAVE_EXT}", LogSeverity.ERROR);
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
        #endregion

        #region Recreate folder
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
        /// <returns><inheritdoc cref="RecreateFolder(string, string?, string?)"/></returns>
        public static bool RecreateSavesFolder()
        {
            return RecreateFolder(Constants.SAVES_FOLDER);
        }

        /// <summary>
        /// <c>RecreateFolder</c> for the backups folder.
        /// </summary>
        /// <returns><inheritdoc cref="RecreateFolder(string, string?, string?)"/></returns>
        public static bool RecreateBackupsFolder()
        {
            return RecreateFolder(Constants.BACKUPS_FOLDER);
        }

        /// <summary>
        /// <c>RecreateFolder</c> for the logs folder.
        /// </summary>
        /// <returns><inheritdoc cref="RecreateFolder(string, string?, string?)"/></returns>
        public static bool RecreateLogsFolder()
        {
            return RecreateFolder(Constants.LOGS_FOLDER);
        }

        /// <summary>
        /// <c>RecreateFolder</c> for a save folder, and all previous folders up to root.
        /// </summary>
        /// <param name="saveFolderName">The save name.</param>
        /// <returns><inheritdoc cref="RecreateFolder(string, string?, string?)"/></returns>
        public static bool RecreateSaveFileFolder(string saveFolderName)
        {
            RecreateSavesFolder();
            return RecreateFolder(saveFolderName, Constants.SAVES_FOLDER_PATH, $"save file: \"{saveFolderName}\"");
        }

        /// <summary>
        /// <c>RecreateFolder</c> for the currently loaded save folder, and all previous folders up to root.
        /// </summary>
        /// <returns><inheritdoc cref="RecreateFolder(string, string?, string?)"/></returns>
        public static bool RecreateSaveFileFolder()
        {
            return RecreateSaveFileFolder(SaveData.saveName);
        }

        /// <summary>
        /// <c>RecreateFolder</c> for a save's chunk folder, and all previous folders up to root.
        /// </summary>
        /// <param name="saveFolderName">The save name.</param>
        /// <returns><inheritdoc cref="RecreateFolder(string, string?, string?)"/></returns>
        public static bool RecreateChunksFolder(string saveFolderName)
        {
            RecreateSaveFileFolder();
            return RecreateFolder(Constants.SAVE_FOLDER_NAME_CHUNKS, GetSaveFolderPath(saveFolderName), $"chunks: \"{saveFolderName}\"");
        }

        /// <summary>
        /// <c>RecreateFolder</c> for the currently loaded save's chunk folder, and all previous folders up to root.
        /// </summary>
        /// <returns><inheritdoc cref="RecreateFolder(string, string?, string?)"/></returns>
        public static bool RecreateChunksFolder()
        {
            return RecreateChunksFolder(SaveData.saveName);
        }
        #endregion

        #region Deserialize data
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
                    return (long)token;
                case JTokenType.Float:
                    return (double)token;
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

        #region Random
        /// <summary>
        /// Turns a Splittable random into its string representation.
        /// </summary>
        /// <param name="randomGenerator">The random generator.</param>
        public static string SerializeRandom(SplittableRandom randomGenerator)
        {
            return new SplittableRandomSerializer().WriteToString(randomGenerator);
        }

        /// <summary>
        /// Turns the string representation of a Splittable random into an object.
        /// </summary>
        /// <param name="randomString">The random generator's string representation.</param>
        public static SplittableRandom DeserializeRandom(string randomString)
        {
            return (SplittableRandom)new SplittableRandomSerializer().ReadFromString(randomString);
        }

        /// <summary>
        /// Returns a new <c>SplittableRandom</c> from another <c>SplittableRandom</c>.
        /// </summary>
        /// <param name="parrentRandom">The random generator to use, to generate the other generator.</param>
        public static SplittableRandom MakeRandomGenerator(SplittableRandom parrentRandom)
        {
            return parrentRandom.Split();
        }
        #endregion

        #region Misc
        /// <summary>
        /// Returns what the save's folder path should be.
        /// </summary>
        /// <param name="saveFolderName">The save name.</param>
        public static string? GetSaveFolderPath(string saveFolderName)
        {
            return Path.Join(Constants.SAVES_FOLDER_PATH, saveFolderName);
        }

        /// <summary>
        /// Returns what the currently loaded save's folder path should be.
        /// </summary>
        public static string? GetSaveFolderPath()
        {
            return GetSaveFolderPath(SaveData.saveName);
        }

        /// <summary>
        /// Makes a backup of a save folder, into the backups folder (as a zip file).
        /// </summary>
        /// <param name="saveFolderName">The name of the save folder.</param>
        /// <param name="isTemporary">If the backup is supposed to be temporary.</param>
        /// <returns>The full backup path, and a version of it, that doesn't contain the root directory.</returns>
        public static (string backupPath, string relativeBackupPath)? CreateBackup(string saveFolderName, bool isTemporary = false)
        {
            // recreate folders
            RecreateSavesFolder();
            RecreateBackupsFolder();

            var now = DateTime.Now;
            var saveFolderPath = GetSaveFolderPath(saveFolderName);
            if (Directory.Exists(saveFolderPath))
            {
                // make more variables
                var backupNameEnd = $"{saveFolderName};{Utils.MakeDate(now)};{Utils.MakeTime(now, "-", isTemporary, "-")}.{Constants.BACKUP_EXT}";
                var backupPath = Path.Join(Constants.BACKUPS_FOLDER_PATH, backupNameEnd);
                var displayBackupPath = Path.Join(Constants.BACKUPS_FOLDER, backupNameEnd);
                // make zip
                ZipFile.CreateFromDirectory(saveFolderPath, backupPath);
                Logger.Log($"Made {(isTemporary ? "temporary " : "")}backup", displayBackupPath, isTemporary ? LogSeverity.DEBUG : LogSeverity.INFO);
                return (backupPath, displayBackupPath);
            }
            else
            {
                var displaySavePath = Path.Join(Constants.SAVES_FOLDER, saveFolderName);
                Logger.Log($"{(isTemporary ? "Temporary b" : "B")}ackup failed", $"save folder not found: {displaySavePath}", LogSeverity.WARN);
                return null;
            }
        }

        /// <summary>
        /// Checks if the file name exists in the saves directory.
        /// </summary>
        /// <param name="saveFolderName">The name of the save folder.</param>
        private static bool CheckSaveFolderExists(string saveFolderName)
        {
            return !RecreateSavesFolder() && Directory.Exists(Path.Join(Constants.SAVES_FOLDER_PATH, saveFolderName));
        }

        /// <summary>
        /// Returns a variant of thr save name, that can be a folder name, and doesn't exist yet.
        /// </summary>
        /// <param name="rawSaveName">The save folder name to correct.</param>
        public static string CorrectSaveName(string? rawSaveName)
        {
            if (string.IsNullOrWhiteSpace(rawSaveName))
            {
                rawSaveName = "new save";
            }
            var saveName = Utils.RemoveInvalidFileNameCharacters(rawSaveName);
            if (string.IsNullOrWhiteSpace(saveName))
            {
                saveName = "new save";
            }
            if (CheckSaveFolderExists(saveName))
            {
                var extraNum = 1;
                while (CheckSaveFolderExists(saveName + "_" + extraNum))
                {
                    extraNum++;
                }
                saveName += "_" + extraNum;
            }
            return saveName;
        }

        /// <summary>
        /// Deletes the save folder, if it exists.
        /// </summary>
        /// <param name="saveFolderName">The name of the save folder.</param>
        public static void DeleteSave(string saveFolderName)
        {
            var saveFolderPath = GetSaveFolderPath(saveFolderName);
            if (Directory.Exists(saveFolderPath))
            {
                Directory.Delete(saveFolderPath);
                Logger.Log("Deleted save", $"save name: {saveFolderName}");
            }
        }

        /// <summary>
        /// Searches for all public static fields in a (static) class, and all of its nested (public static) classes, and returns their values.
        /// </summary>
        /// <typeparam name="T">The type of values to search for.</typeparam>
        /// <param name="classType">The type of the static class to search in.</param>
        public static List<T> GetNestedStaticClassFields<T>(Type classType)
        {
            var subClassFieldValues = new List<T>();
            
            var subClasses = classType.GetNestedTypes();

            foreach (var subClass in subClasses)
            {
                subClassFieldValues.AddRange(GetNestedStaticClassFields<T>(subClass));
            }
            FieldInfo[] properties = classType.GetFields();

            var classFieldValues = new List<T>();
            foreach (FieldInfo property in properties)
            {
                if (property.IsStatic && property.FieldType == typeof(T))
                {
                    var value = property.GetValue(null);
                    if (value is not null)
                    {
                        classFieldValues.Add((T)value);
                    }
                }
            }
            classFieldValues.AddRange(subClassFieldValues);

            return classFieldValues;
        }

        /// <summary>
        /// Returns if the current version string is equal or higher than the minimum version.<br/>
        /// If the version number string has a letter in it, it only checks if the min version string also has one.
        /// </summary>
        /// <param name="minimumVersion">The minimum version number to qualify for being up to date.</param>
        /// <param name="currentVersion">The version number to check.</param>
        public static bool IsUpToDate(string minimumVersion, string currentVersion)
        {
            var version = currentVersion.Split(".");
            var minVersion = minimumVersion.Split(".");

            for (int x = 0; x < version.Length; x++)
            {
                // min v. shorter
                if (minVersion.Length < (x + 1))
                {
                    return true;
                }
                var vIsNum = int.TryParse(version[x], out int vInt);
                var minIsNum = int.TryParse(minVersion[x], out int minInt);
                if (vIsNum && minIsNum)
                {
                    // v. > min v. ?
                    if (vInt > minInt)
                    {
                        return true;
                    }
                    // v. < min v. ?
                    else if (vInt < minInt)
                    {
                        return false;
                    }
                }
                else
                {
                    if (vIsNum != minIsNum)
                    {
                        return !vIsNum;
                    }
                }
            }
            // v. shorter
            return true;
        }
        #endregion
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
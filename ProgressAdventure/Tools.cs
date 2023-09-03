using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NPrng;
using NPrng.Generators;
using NPrng.Serializers;
using ProgressAdventure.Enums;
using ProgressAdventure.Extensions;
using ProgressAdventure.SettingsManagement;
using SaveFileManager;
using System.Collections;
using System.IO.Compression;
using System.Reflection;
using System.Text;

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
        /// <param name="expected">If the file is expected to exist.<br/>
        /// ONLY ALTERS THE LOGS DISPLAYED, IF THE FILE/FOLDER DOESN'T EXIST.</param>
        /// <exception cref="FormatException">Exeption thrown, if the file couldn't be decode.</exception>
        /// <exception cref="FileNotFoundException">Exeption thrown, if the file couldn't be found.</exception>
        /// <exception cref="DirectoryNotFoundException">Exeption thrown, if the directory containing the file couldn't be found.</exception>
        public static Dictionary<string, object?>? DecodeSaveShort(string filePath, int lineNum = 0, long? seed = null, string? extension = null, bool expected = true)
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
                Logger.Log("File not found", $"{(expected ? "" : "(but it was expected) ")}file name: {safeFilePath}.{Constants.SAVE_EXT}", expected ? LogSeverity.ERROR : LogSeverity.INFO);
                throw;
            }
            catch (DirectoryNotFoundException)
            {
                Logger.Log("Folder containing file not found", $"{(expected ? "" : "(but it was expected) ")}file name: {safeFilePath}.{Constants.SAVE_EXT}", expected ? LogSeverity.ERROR : LogSeverity.INFO);
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
        /// <param name="saveFolderName">The name of the save folder.<br/>
        /// If null, it uses the save name of <c>SaveData</c> instead.</param>
        /// <returns><inheritdoc cref="RecreateFolder(string, string?, string?)"/></returns>
        public static bool RecreateSaveFileFolder(string? saveFolderName = null)
        {
            saveFolderName ??= SaveData.saveName;
            RecreateSavesFolder();
            return RecreateFolder(saveFolderName, Constants.SAVES_FOLDER_PATH, $"save file: \"{saveFolderName}\"");
        }

        /// <summary>
        /// <c>RecreateFolder</c> for a save's chunk folder, and all previous folders up to root.
        /// </summary>
        /// <param name="saveFolderName">The save name.<br/>
        /// If null, it uses the save name of <c>SaveData</c> instead.</param>
        /// <returns><inheritdoc cref="RecreateFolder(string, string?, string?)"/></returns>
        public static bool RecreateChunksFolder(string? saveFolderName = null)
        {
            saveFolderName ??= SaveData.saveName;
            RecreateSaveFileFolder(saveFolderName);
            return RecreateFolder(Constants.SAVE_FOLDER_NAME_CHUNKS, GetSaveFolderPath(saveFolderName), $"chunks: \"{saveFolderName}\"");
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
        public static SplittableRandom? DeserializeRandom(string? randomString)
        {
            if (randomString is null)
            {
                Logger.Log("Random parse error", "random seed is null", LogSeverity.WARN);
                return null;
            }
            try
            {
                return (SplittableRandom)new SplittableRandomSerializer().ReadFromString(randomString);
            }
            catch (Exception e)
            {
                if (e is ArgumentNullException || e is FormatException)
                {
                    Logger.Log("Random parse error", "cannot parse random generator from seed string", LogSeverity.WARN);
                    return null;
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Tries to turn the string representation of a Splittable random into an object, and returns the success.
        /// </summary>
        /// <param name="randomString">The random generator's string representation.</param>
        /// <param name="random">The random generator, that got deserialised.</param>
        public static bool TryDeserializeRandom(string? randomString, out SplittableRandom? random)
        {
            random = DeserializeRandom(randomString);
            return random is not null;
        }

        /// <summary>
        /// Tries to turn the int value of the log severity into a <c>LogSeverity</c> enum, and returns the success.
        /// </summary>
        /// <param name="severityValue">The log sevrity's int representation.</param>
        /// <param name="severity">The sevrity, that got parsed, or created.</param>
        public static bool TryParseLogSeverityFromValue(int severityValue, out LogSeverity severity)
        {
            foreach (var loggingValue in Logger.loggingValuesMap)
            {
                if (loggingValue.Value == severityValue)
                {
                    severity = loggingValue.Key;
                    return true;
                }
            }

            severity = LogSeverity.DEBUG;
            return false;
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

        #region String corrector
        /// <summary>
        /// A function to return the corrected verson of a string, that the user inputed.
        /// </summary>
        /// <param name="rawText">The raw user input to correct.</param>
        public delegate string StringCorrectorDelegate(string? rawText);

        /// <summary>
        /// Temporary field for the string corrector function.
        /// </summary>
        private static StringCorrectorDelegate stringCorrectorFunction;
        /// <summary>
        /// Temporary field for the string corrector <c>TextField</c>.
        /// </summary>
        private static TextField correctorTextField;

        /// <summary>
        /// Displays a <c>TextField</c>, where the user can input a string, that will be corrected as the user types it.<br/>
        /// NOT THREAD SAFE!!!
        /// </summary>
        /// <param name="preValue">The text to display before the part, where the user inputs the string.</param>
        /// <param name="stringCorrector"></param>
        /// <param name="postValue">The text displayed immediately after the user's inputed text.</param>
        /// <param name="startingValue">The starting value of the input field.</param>
        /// <param name="clearScreen">Whether to "clear" the screen, before displaying the text, or not.</param>
        /// <returns>The uncorrected version of the final string.</returns>
        public static string GetRealTimeCorrectedString(string preValue, StringCorrectorDelegate stringCorrector, string postValue = "", string startingValue = "", bool clearScreen = true)
        {
            stringCorrectorFunction = stringCorrector;
            correctorTextField = new TextField(
                "",
                preValue,
                postValue + " -> " + stringCorrectorFunction(startingValue),
                oldValueAsStartingValue: true,
                keyValidatorFunction: new TextField.KeyValidatorDelegate(StringCorrectorKeyValidator)
            );
            new BaseUIDisplay(correctorTextField, autoEnter: true, clearScreen: clearScreen).Display(Settings.Keybinds.KeybindList);
            Console.WriteLine();
            return correctorTextField.value;
        }

        private static bool StringCorrectorKeyValidator(StringBuilder text, ConsoleKeyInfo key)
        {
            if (key.Key == ConsoleKey.Enter ||
                key.KeyChar == '\0' ||
                key.Key == ConsoleKey.Escape
            )
            {
                return true;
            }

            string newText = "";
            if (key.Key == ConsoleKey.Backspace)
            {
                if (text.Length > 0)
                {
                    newText = text.ToString()[0..(text.Length - 1)];
                }
            }
            else
            {
                newText = text.ToString() + key.KeyChar;
            }

            correctorTextField.PostValue = " -> " + stringCorrectorFunction(newText);
            return true;
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

        public static string CorrectPlayerName(string? rawPlayerName)
        {
            return string.IsNullOrWhiteSpace(rawPlayerName) ? "You" : rawPlayerName;
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
                Directory.Delete(saveFolderPath, true);
                Logger.Log("Deleted save", $"save name: {saveFolderName}");
            }
        }

        /// <summary>
        /// Colors text fore/background (influenced by <c>Settings.EnableColoredText</c>).
        /// </summary>
        /// <param name="text">The text to display</param>
        /// <param name="foregroundColor">The RGB color of the foreground color.</param>
        /// <param name="backgroundColor">The RGB color of the background color.</param>
        /// <returns>A string that will color the text, when writen out in the console.</returns>
        public static string StylizedText(string text, (byte r, byte g, byte b)? foregroundColor = null, (byte r, byte g, byte b)? backgroundColor = null)
        {
            return Settings.EnableColoredText ? Utils.StylizedText(text, foregroundColor, backgroundColor) : text;
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
            if (minimumVersion == currentVersion)
            {
                return true;
            }

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
            // v. <=
            return false;
        }

        /// <summary>
        /// Tries to convert the json representation of the object to an object format.
        /// </summary>
        /// <param name="objectJson">The json representation of the object.</param>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        /// <param name="convertedObject">The object representation of the json.</param>
        /// <returns>If the conversion was succesfull without any warnings.</returns>
        public static bool FromJson<T>(IDictionary<string, object?>? objectJson, string fileVersion, out T? convertedObject)
            where T : IJsonConvertable<T>
        {
            return T.FromJson(objectJson, fileVersion, out convertedObject);
        }

        /// <summary>
        /// Corrects the json data to a specific version.
        /// </summary>
        /// <param name="objectJsonCorrecter">The correcter function.</param>
        /// <param name="objectJson">The json data to correct.</param>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        /// <param name="newFileVersion">The version number, this function will correct the json data to.</param>
        public static void CorrectJsonDataVersion<T>(Action<IDictionary<string, object?>> objectJsonCorrecter, ref IDictionary<string, object?> objectJson, ref string fileVersion, string newFileVersion)
        {
            if (!IsUpToDate(newFileVersion, fileVersion))
            {
                objectJsonCorrecter(objectJson);

                Logger.Log($"Corrected {typeof(T)} json data", $"{fileVersion} -> {newFileVersion}", LogSeverity.DEBUG);
                fileVersion = newFileVersion;
            }
        }

        /// <summary>
        /// Mimics the CorrectJsonData() part of the FromJson() function.
        /// </summary>
        /// <param name="objectJson">The json representation of the object.</param>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        public static void CorrectJsonData<T>(
            ref IDictionary<string, object?> objectJson,
            List<(Action<IDictionary<string, object?>> objectJsonCorrecter, string newFileVersion)> correcters,
            string fileVersion)
        {
            if (IsUpToDate(Constants.SAVE_VERSION, fileVersion))
            {
                return;
            }

            Logger.Log($"{typeof(T)} json data is old", "correcting data");
            foreach (var (objectJsonCorrecter, newFileVersion) in correcters)
            {
                CorrectJsonDataVersion<T>(objectJsonCorrecter, ref objectJson, ref fileVersion, newFileVersion);
            }
            Logger.Log($"{typeof(T)} json data corrected");
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
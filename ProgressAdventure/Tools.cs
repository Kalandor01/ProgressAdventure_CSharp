using PACommon;
using PACommon.Enums;
using PACommon.JsonUtils;
using ProgressAdventure.SettingsManagement;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Compression;
using static ProgressAdventure.Constants;
using PACTools = PACommon.Tools;
using Utils = PACommon.Utils;

namespace ProgressAdventure
{
    /// <summary>
    /// Contains project specific useful functions.
    /// </summary>
    public static class Tools
    {
        #region Public functions
        #region Encode/decode Short
        /// <inheritdoc cref="PACTools.EncodeSaveShort(IDictionary, string, long, string)"/>
        public static void EncodeSaveShort(
            IDictionary data,
            string filePath,
            long seed = SAVE_SEED,
            string extension = SAVE_EXT
        )
        {
            EncodeSaveShort(new List<IDictionary> { data }, filePath, seed, extension);
        }

        /// <inheritdoc cref="PACTools.EncodeSaveShort(IEnumerable{IDictionary}, string, long, string)"/>
        public static void EncodeSaveShort(
            IEnumerable<IDictionary> dataList,
            string filePath,
            long seed = SAVE_SEED,
            string extension = SAVE_EXT
        )
        {
            PACTools.EncodeSaveShort(dataList, filePath, seed, extension);
        }

        /// <inheritdoc cref="PACTools.DecodeSaveShort(string, long, string, int, bool)"/>
        public static Dictionary<string, object?>? DecodeSaveShort(
            string filePath,
            int lineNum = 0,
            long seed = SAVE_SEED,
            string extension = SAVE_EXT,
            bool expected = true
        )
        {
            return PACTools.DecodeSaveShort(filePath, seed, extension, lineNum, expected);
        }
        #endregion

        #region Recreate folder
        /// <summary>
        /// <c>RecreateFolder</c> for the saves folder.
        /// </summary>
        /// <returns><inheritdoc cref="PACTools.RecreateFolder(string, string?, string?)"/></returns>
        public static bool RecreateSavesFolder()
        {
            return PACTools.RecreateFolder(Constants.SAVES_FOLDER);
        }

        /// <summary>
        /// <c>RecreateFolder</c> for the backups folder.
        /// </summary>
        /// <returns><inheritdoc cref="PACTools.RecreateFolder(string, string?, string?)"/></returns>
        public static bool RecreateBackupsFolder()
        {
            return PACTools.RecreateFolder(Constants.BACKUPS_FOLDER);
        }

        /// <summary>
        /// <c>RecreateFolder</c> for a save folder, and all previous folders up to root.
        /// </summary>
        /// <param name="saveFolderName">The name of the save folder.<br/>
        /// If null, it uses the save name of <c>SaveData</c> instead.</param>
        /// <returns><inheritdoc cref="PACTools.RecreateFolder(string, string?, string?)"/></returns>
        public static bool RecreateSaveFileFolder(string? saveFolderName = null)
        {
            saveFolderName ??= SaveData.Instance.saveName;
            RecreateSavesFolder();
            return PACTools.RecreateFolder(saveFolderName, Constants.SAVES_FOLDER_PATH, $"save file: \"{saveFolderName}\"");
        }

        /// <summary>
        /// <c>RecreateFolder</c> for a save's chunk folder, and all previous folders up to root.
        /// </summary>
        /// <param name="saveFolderName">The save name.<br/>
        /// If null, it uses the save name of <c>SaveData</c> instead.</param>
        /// <returns><inheritdoc cref="PACTools.RecreateFolder(string, string?, string?)"/></returns>
        public static bool RecreateChunksFolder(string? saveFolderName = null)
        {
            saveFolderName ??= SaveData.Instance.saveName;
            RecreateSaveFileFolder(saveFolderName);
            return PACTools.RecreateFolder(Constants.SAVE_FOLDER_NAME_CHUNKS, GetSaveFolderPath(saveFolderName), $"chunks: \"{saveFolderName}\"");
        }
        #endregion

        #region Logger short
        /// <summary>
        /// Logs a json parsing error for a custom error.
        /// </summary>
        /// <typeparam name="T">The class that is being parsed.</typeparam>
        /// <param name="message">The error message.</param>
        /// <param name="isError">If the error will make it, so the parsing function is halted.</param>
        public static void LogJsonError<T>(string message, bool isError = false)
        {
            var stackTrace = GetFromJsonCallStackString();
            PACSingletons.Instance.Logger.Log(
                $"{typeof(T)} parse {(isError ? "error" : "warning")}",
                message + (stackTrace is null ? "" : $"\n{stackTrace}"),
                isError ? LogSeverity.ERROR : LogSeverity.WARN
            );
        }

        /// <summary>
        /// Logs a json parsing error for a parameter parsing error.
        /// </summary>
        /// <typeparam name="T">The class that is being parsed.</typeparam>
        /// <param name="parameterName">The name of the parameter (or class) that caused the error.</param>
        /// <param name="isError">If the error will make it, so the parsing function is halted.</param>
        public static void LogJsonParseError<T>(string parameterName, bool isError = false)
        {
            LogJsonError<T>($"couldn't parse {parameterName}", isError);
        }

        /// <summary>
        /// Logs a json parsing error for a null parameter error.
        /// </summary>
        /// <typeparam name="T">The class that is being parsed.</typeparam>
        /// <param name="parameterName">The name of the parameter (or class) that caused the error.</param>
        /// <param name="isError">If the error will make it, so the parsing function is halted.</param>
        public static void LogJsonNullError<T>(string parameterName, bool isError = false)
        {
            LogJsonError<T>($"{parameterName} json is null", isError);
        }

        /// <summary>
        /// Returns the call stack of "FromJson()" methods.
        /// </summary>
        public static List<StackFrame> GetFromJsonCallStack()
        {
            var frames = new StackTrace(true).GetFrames();
            var frame = frames.First();
            var fromJsonMethod = PACTools.FromJsonWithoutCorrection<SaveData>;
            var fromJsonMethodName = fromJsonMethod.Method.Name;
            return frames.Where(frame => frame.GetMethod()?.Name.Contains(fromJsonMethodName) ?? false).ToList();
        }

        /// <summary>
        /// Returns the string representation of the call stack of "FromJson()" methods.
        /// </summary>
        public static string? GetFromJsonCallStackString()
        {
            var stackFrames = GetFromJsonCallStack();
            return !stackFrames.Any() ? null : string.Join(
                "\n",
                stackFrames.Select(frame => 
                $"\tat {frame.GetMethod()?.DeclaringType?.FullName} in {frame.GetFileName()}:line {frame.GetFileLineNumber()}"
            ));
        }
        #endregion

        #region Json parse short
        /// <summary>
        /// Tries to parse a value from a json dictionary, and logs a warning, if it doesn't exist or null.
        /// </summary>
        /// <typeparam name="T">The class that is being parsed.</typeparam>
        /// <param name="objectJson">The json dictionary to parse the value from.</param>
        /// <param name="jsonKey">The key, to the value.</param>
        /// <param name="value">The returned value.</param>
        /// <param name="isCritical">If the value is critical for the parsing of the object.<br/>
        /// If true, it will return immidietly if it can't be parsed and logs errors.</param>
        /// <returns>If the value was sucessfuly parsed.</returns>
        public static bool TryGetJsonObjectValue<T>(IDictionary<string, object?> objectJson, string jsonKey, out object? value, bool isCritical = false)
        {
            if (
                objectJson.TryGetValue(jsonKey, out value) &&
                value is not null
            )
            {
                return true;
            }
            LogJsonNullError<T>(jsonKey, isCritical);
            return false;
        }

        /// <summary>
        /// Tries to parse a json value to a type. If the value can't be parsed, it logs a json parse warning.<br/>
        /// Usable types:<br/>
        /// - bool<br/>
        /// - string<br/>
        /// - numbers<br/>
        /// - enums<br/>
        /// - any type that has a converter? and can convert from string representation to object<br/>
        /// - nullables (will only make the default value null instead of the default value for the type)
        /// </summary>
        /// <typeparam name="T">The class that is being parsed.</typeparam>
        /// <typeparam name="TRes">The type to parse to</typeparam>
        /// <param name="value">The value to parse.</param>
        /// <param name="parsedValue">The parsed value, or default if it wasn't successful.</param>
        /// <param name="parameterName">The parameter name to use for logging unsuccesful parsing.</param>
        /// <param name="isCritical">If the value is critical for the parsing of the object.<br/>
        /// If true, it will return immidietly if it can't be parsed and logs errors.</param>
        /// <returns>If the value was successfuly parsed.</returns>
        public static bool TryParseValueForJsonParsing<T, TRes>(
            object? value,
            out TRes? parsedValue,
            bool logParseWarning = true,
            string? parameterName = null,
            bool isCritical = false
        )
        {
            parsedValue = default;
            var valueText = value?.ToString();

            if (valueText is null)
            {
                return false;
            }

            var parseType = typeof(TRes);
            var actualType = Nullable.GetUnderlyingType(parseType) ?? parseType;
            if (actualType == typeof(string))
            {
                parsedValue = (TRes)(object)valueText;
            }

            var parsedResult = default(TRes?);
            var parseFailed = false;
            try
            {
                var converter = TypeDescriptor.GetConverter(parseType);
                parsedResult = (TRes?)converter.ConvertFromString(valueText);
            }
            catch
            {
                parseFailed = true;
            }

            if (!parseFailed && parsedResult is not null)
            {
                if (!actualType.IsEnum || Enum.IsDefined(actualType, parsedResult))
                {
                    parsedValue = parsedResult;
                    return true;
                }
            }

            if (logParseWarning)
            {
                LogJsonParseError<T>(parameterName ?? $"{parseType} type parameter", isCritical);
            }
            return false;
        }

        /// <summary>
        /// Tries to parse a value from a json dictionary, and logs a warning, if it can't pe parsed.<br/>
        /// Usable types:<br/>
        /// - bool<br/>
        /// - string<br/>
        /// - numbers<br/>
        /// - enums<br/>
        /// - any type that has a converter? and can convert from string representation to object (has [type].TryParse()?)<br/>
        /// - nullables (will only make the default value null instead of the default value for the type)
        /// </summary>
        /// <typeparam name="TRes">The type to parse to.</typeparam>
        /// <inheritdoc cref="TryGetJsonObjectValue{T}(IDictionary{string, object?}, string, out object?, bool)"/>
        public static bool TryParseJsonValue<T, TRes>(
            IDictionary<string, object?> objectJson,
            string jsonKey,
            out TRes? value,
            bool isCritical = false
        )
        {
            value = default;
            if (!TryGetJsonObjectValue<T>(objectJson, jsonKey, out var result, isCritical))
            {
                return false;
            }

            return TryParseValueForJsonParsing<T, TRes>(result, out value, parameterName: jsonKey, isCritical: isCritical);
        }

        /// <summary>
        /// Tries to parse a json dictionary value from a json dictionary, and logs a warning, if it can't pe parsed.
        /// </summary>
        /// <inheritdoc cref="TryGetJsonObjectValue{T}(IDictionary{string, object?}, string, out object?, bool)"/>
        public static bool TryParseJsonDictValue<T>(
            IDictionary<string, object?> objectJson,
            string jsonKey,
            out IDictionary<string, object?>? value,
            bool isCritical = false
        )
        {
            value = null;
            if (!TryGetJsonObjectValue<T>(objectJson, jsonKey, out var result, isCritical))
            {
                return false;
            }

            if (result is IDictionary<string, object?> resultValue)
            {
                value = resultValue;
                return true;
            }
            LogJsonParseError<T>(jsonKey, isCritical);
            return false;
        }

        /// <summary>
        /// Tries to parse an IJsonConvertable value from a json dictionary, and logs a warning, if it can't pe parsed.
        /// </summary>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        /// <typeparam name="TJc">The IJsonConvertable class to convert to.</typeparam>
        /// <returns>If the object was parsed without warnings.</returns>
        /// <inheritdoc cref="TryGetJsonObjectValue{T}(IDictionary{string, object?}, string, out object?, bool)"/>
        public static bool TryParseJsonConvertableValue<T, TJc>(
            IDictionary<string, object?> objectJson,
            string fileVersion,
            string jsonKey,
            out TJc? value,
            bool isCritical = false
        )
            where TJc : IJsonConvertable<TJc>
        {
            value = default;
            if (!TryParseJsonDictValue<T>(objectJson, jsonKey, out var result, isCritical))
            {
                return false;
            }
            
            var success = PACTools.TryFromJson(result, fileVersion, out value);
            if (value is null)
            {
                LogJsonParseError<T>(jsonKey, isCritical);
            }
            return success;
        }

        /// <summary>
        /// Tries to parse a list value from a json dictionary, and logs a warning, if it can't pe parsed.
        /// </summary>
        /// <typeparam name="TRes">The type of the values in the list.</typeparam>
        /// <param name="parseFunction">The function to use, to parse the elemets of the list to the correct type.<br/>
        /// If the success is false or result is null, it will not be added to the list.</param>
        /// <inheritdoc cref="TryGetJsonObjectValue{T}(IDictionary{string, object?}, string, out object?, bool)"/>
        public static bool TryParseListValue<T, TRes>(
            IDictionary<string, object?> objectJson,
            string jsonKey,
            Func<object?, (bool success, TRes? result)> parseFunction,
            out List<TRes>? value,
            bool isCritical = false
        )
        {
            value = null;
            if (!TryGetJsonObjectValue<T>(objectJson, jsonKey, out var result, isCritical))
            {
                return false;
            }

            if (result is IEnumerable<object?> resultList)
            {
                value = new List<TRes>();
                foreach (var element in resultList)
                {
                    var parsedResult = parseFunction(element);
                    if (parsedResult.success && parsedResult.result is not null)
                    {
                        value.Add(parsedResult.result);
                    }
                    else
                    {
                        LogJsonParseError<T>($"an element of the {jsonKey} list");
                    }
                }
                return true;
            }
            LogJsonParseError<T>(jsonKey, isCritical);
            return false;
        }
        #endregion

        #region Misc
        /// <summary>
        /// Returns what the save's folder path should be.
        /// </summary>
        /// <param name="saveFolderName">The save name.</param>
        public static string? GetSaveFolderPath(string saveFolderName)
        {
            return Path.Join(SAVES_FOLDER_PATH, saveFolderName);
        }

        /// <summary>
        /// Returns what the currently loaded save's folder path should be.
        /// </summary>
        public static string? GetSaveFolderPath()
        {
            return GetSaveFolderPath(SaveData.Instance.saveName);
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
                var backupNameEnd = $"{saveFolderName};{Utils.MakeDate(now)};{Utils.MakeTime(now, "-", isTemporary, "-")}.{BACKUP_EXT}";
                var backupPath = Path.Join(BACKUPS_FOLDER_PATH, backupNameEnd);
                var displayBackupPath = Path.Join(BACKUPS_FOLDER, backupNameEnd);
                // make zip
                ZipFile.CreateFromDirectory(saveFolderPath, backupPath);
                PACSingletons.Instance.Logger.Log($"Made {(isTemporary ? "temporary " : "")}backup", $"\"{displayBackupPath}\"", isTemporary ? LogSeverity.DEBUG : LogSeverity.INFO);
                return (backupPath, displayBackupPath);
            }
            else
            {
                var displaySavePath = Path.Join(SAVES_FOLDER, saveFolderName);
                PACSingletons.Instance.Logger.Log($"{(isTemporary ? "Temporary b" : "B")}ackup failed", $"save folder not found: \"{displaySavePath}\"", LogSeverity.WARN);
                return null;
            }
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
                PACSingletons.Instance.Logger.Log("Deleted save", $"save name: {saveFolderName}");
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
        #endregion
        #endregion

        #region Private functions
        /// <summary>
        /// Checks if the file name exists in the saves directory.
        /// </summary>
        /// <param name="saveFolderName">The name of the save folder.</param>
        private static bool CheckSaveFolderExists(string saveFolderName)
        {
            return !RecreateSavesFolder() && Directory.Exists(Path.Join(SAVES_FOLDER_PATH, saveFolderName));
        }
        #endregion
    }
}
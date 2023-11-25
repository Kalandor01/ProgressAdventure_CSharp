using PACommon;
using PACommon.Enums;
using PACommon.JsonUtils;
using ProgressAdventure.Entity;
using ProgressAdventure.Enums;
using ProgressAdventure.SettingsManagement;
using System;
using System.Collections;
using System.IO.Compression;
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
        #region Short
        /// <inheritdoc cref="PACTools.EncodeSaveShort(IDictionary, string, long, string)"/>
        public static void EncodeSaveShort(
            IDictionary data,
            string filePath,
            long seed = Constants.SAVE_SEED,
            string extension = Constants.SAVE_EXT
        )
        {
            EncodeSaveShort(new List<IDictionary> { data }, filePath, seed, extension);
        }

        /// <inheritdoc cref="PACTools.EncodeSaveShort(IEnumerable{IDictionary}, string, long, string)"/>
        public static void EncodeSaveShort(
            IEnumerable<IDictionary> dataList,
            string filePath,
            long seed = Constants.SAVE_SEED,
            string extension = Constants.SAVE_EXT
        )
        {
            PACTools.EncodeSaveShort(dataList, filePath, seed, extension);
        }

        /// <inheritdoc cref="PACTools.DecodeSaveShort(string, long, string, int, bool)"/>
        public static Dictionary<string, object?>? DecodeSaveShort(
            string filePath,
            int lineNum = 0,
            long seed = Constants.SAVE_SEED,
            string extension = Constants.SAVE_EXT,
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
            PACSingletons.Instance.Logger.Log(
                $"{typeof(T)} parse {(isError ? "error" : "warning")}",
                message,
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
            PACSingletons.Instance.Logger.Log(
                $"{typeof(T)} parse {(isError ? "error" : "warning")}",
                $"couldn't parse {parameterName}",
                isError ? LogSeverity.ERROR : LogSeverity.WARN
            );
        }

        /// <summary>
        /// Logs a json parsing error for a null parameter error.
        /// </summary>
        /// <typeparam name="T">The class that is being parsed.</typeparam>
        /// <param name="parameterName">The name of the parameter (or class) that caused the error.</param>
        /// <param name="isError">If the error will make it, so the parsing function is halted.</param>
        public static void LogJsonNullError<T>(string parameterName, bool isError = false)
        {
            PACSingletons.Instance.Logger.Log(
                $"{typeof(T)} parse {(isError ? "error" : "warning")}",
                $"{parameterName} json is null",
                isError ? LogSeverity.ERROR : LogSeverity.WARN
            );
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
        /// <returns>If the value was sucessfuly parsed.</returns>
        public static bool TryParseObjectValue<T>(IDictionary<string, object?> objectJson, string jsonKey, out object? value)
        {
            if (
                objectJson.TryGetValue(jsonKey, out value) &&
                value is not null
            )
            {
                return true;
            }
            LogJsonNullError<T>(jsonKey);
            return false;
        }

        /// <summary>
        /// Tries to parse a string value from a json dictionary, and logs a warning, if it can't pe parsed.
        /// </summary>
        /// <inheritdoc cref="TryParseObjectValue{T}(IDictionary{string, object?}, string, out object?)"/>
        public static bool TryParseStringValue<T>(IDictionary<string, object?> objectJson, string jsonKey, out string? value)
        {
            value = null;
            if (!TryParseObjectValue<T>(objectJson, jsonKey, out var result))
            {
                return false;
            }

            if (result is not null)
            {
                value = result.ToString();
                return true;
            }
            LogJsonParseError<T>(jsonKey);
            return false;
        }

        /// <summary>
        /// Tries to parse an int value from a json dictionary, and logs a warning, if it can't pe parsed.
        /// </summary>
        /// <inheritdoc cref="TryParseObjectValue{T}(IDictionary{string, object?}, string, out object?)"/>
        public static bool TryParseIntValue<T>(IDictionary<string, object?> objectJson, string jsonKey, out int? value)
        {
            value = null;
            if (!TryParseObjectValue<T>(objectJson, jsonKey, out var result))
            {
                return false;
            }

            if (int.TryParse(result?.ToString(), out var resultValue))
            {
                value = resultValue;
                return true;
            }
            LogJsonParseError<T>(jsonKey);
            return false;
        }

        /// <summary>
        /// Tries to parse an enum value from a json dictionary, and logs a warning, if it can't pe parsed.
        /// </summary>
        /// <typeparam name="TEnum">The enum type.</typeparam>
        /// <inheritdoc cref="TryParseObjectValue{T}(IDictionary{string, object?}, string, out object?)"/>
        public static bool TryParseEnumValue<T, TEnum>(IDictionary<string, object?> objectJson, string jsonKey, out TEnum? value)
             where TEnum : struct
        {
            value = null;
            if (!TryParseObjectValue<T>(objectJson, jsonKey, out var result))
            {
                return false;
            }

            if (
                Enum.TryParse(typeof(TEnum), result?.ToString(), out var enumValue) &&
                Enum.IsDefined(typeof(TEnum), (TEnum)enumValue)
            )
            {
                value = (TEnum)enumValue;
                return true;
            }
            LogJsonParseError<T>(jsonKey);
            return false;
        }

        /// <summary>
        /// Tries to parse a DateTime value from a json dictionary, and logs a warning, if it can't pe parsed.
        /// </summary>
        /// <inheritdoc cref="TryParseObjectValue{T}(IDictionary{string, object?}, string, out object?)"/>
        public static bool TryParseDateTimeValue<T>(IDictionary<string, object?> objectJson, string jsonKey, out DateTime? value)
        {
            value = null;
            if (!TryParseObjectValue<T>(objectJson, jsonKey, out var result))
            {
                return false;
            }

            if (DateTime.TryParse(result?.ToString(), out var resultValue))
            {
                value = resultValue;
                return true;
            }
            LogJsonParseError<T>(jsonKey);
            return false;
        }

        /// <summary>
        /// Tries to parse a TimeSpan value from a json dictionary, and logs a warning, if it can't pe parsed.
        /// </summary>
        /// <inheritdoc cref="TryParseObjectValue{T}(IDictionary{string, object?}, string, out object?)"/>
        public static bool TryParseTimeSpanValue<T>(IDictionary<string, object?> objectJson, string jsonKey, out TimeSpan? value)
        {
            value = null;
            if (!TryParseObjectValue<T>(objectJson, jsonKey, out var result))
            {
                return false;
            }

            if (TimeSpan.TryParse(result?.ToString(), out var resultValue))
            {
                value = resultValue;
                return true;
            }
            LogJsonParseError<T>(jsonKey);
            return false;
        }

        /// <summary>
        /// Tries to parse a IJsonConvertable value from a json dictionary, and logs a warning, if it can't pe parsed.
        /// </summary>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        /// <typeparam name="TJc">The IJsonConvertable class to convert to.</typeparam>
        /// <returns>If the object was parsed without warnings.</returns>
        /// <inheritdoc cref="TryParseObjectValue{T}(IDictionary{string, object?}, string, out object?)"/>
        public static bool TryParseJsonConvertableValue<T, TJc>(IDictionary<string, object?> objectJson, string fileVersion, string jsonKey, out TJc? value)
            where TJc : IJsonConvertable<TJc>
        {
            value = default;
            if (!TryParseObjectValue<T>(objectJson, jsonKey, out var result))
            {
                return false;
            }

            return PACTools.TryFromJson((IDictionary<string, object?>?)result, fileVersion, out value);
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
                var backupNameEnd = $"{saveFolderName};{Utils.MakeDate(now)};{Utils.MakeTime(now, "-", isTemporary, "-")}.{Constants.BACKUP_EXT}";
                var backupPath = Path.Join(Constants.BACKUPS_FOLDER_PATH, backupNameEnd);
                var displayBackupPath = Path.Join(Constants.BACKUPS_FOLDER, backupNameEnd);
                // make zip
                ZipFile.CreateFromDirectory(saveFolderPath, backupPath);
                PACSingletons.Instance.Logger.Log($"Made {(isTemporary ? "temporary " : "")}backup", $"\"{displayBackupPath}\"", isTemporary ? LogSeverity.DEBUG : LogSeverity.INFO);
                return (backupPath, displayBackupPath);
            }
            else
            {
                var displaySavePath = Path.Join(Constants.SAVES_FOLDER, saveFolderName);
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
            return !RecreateSavesFolder() && Directory.Exists(Path.Join(Constants.SAVES_FOLDER_PATH, saveFolderName));
        }
        #endregion
    }
}
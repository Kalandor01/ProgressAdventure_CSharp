using PACommon;
using PACommon.Enums;
using PACommon.JsonUtils;
using ProgressAdventure.ConfigManagement;
using ProgressAdventure.Entity;
using ProgressAdventure.ItemManagement;
using ProgressAdventure.SettingsManagement;
using ProgressAdventure.WorldManagement;
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
        /// <inheritdoc cref="PACTools.SaveCompressedFile(IEnumerable{JsonDictionary}, string, string)"/>
        public static void SaveCompressedFile(
            IEnumerable<JsonDictionary> dataList,
            string filePath,
            string extension = SAVE_EXT
        )
        {
            PACTools.SaveCompressedFile(dataList, filePath, extension);
        }

        /// <inheritdoc cref="PACTools.SaveCompressedFile(JsonDictionary, string, string)"/>
        public static void SaveCompressedFile(
            JsonDictionary data,
            string filePath,
            string extension = SAVE_EXT
        )
        {
            SaveCompressedFile([data], filePath, extension);
        }

        /// <inheritdoc cref="PACTools.LoadCompressedFile(string, string, int, bool)"/>
        /// <param name="tryOldDecoding">Whether to try to use <see cref="PACTools.DecodeFileShort(string, long, string, int, bool)"/>, if the newer decoding doesn't work.</param>
        public static JsonDictionary? LoadCompressedFile(
            string filePath,
            int lineNum = 0,
            string extension = SAVE_EXT,
            bool expected = true,
            bool tryOldDecoding = true
        )
        {
            var safeFilePath = Path.GetRelativePath(PACommon.Constants.ROOT_FOLDER, filePath);
            try
            {
                return PACTools.LoadCompressedFile(filePath, extension, lineNum, expected && !tryOldDecoding);
            }
            catch (FileNotFoundException)
            {
                if (tryOldDecoding)
                {
                    PACSingletons.Instance.Logger.Log("File not found", $"(but it was expected) file name: {safeFilePath}.{extension}", LogSeverity.INFO);
                }
                else
                {
                    throw;
                }
            }
            catch (FormatException)
            {
                if (tryOldDecoding)
                {
                    PACSingletons.Instance.Logger.Log("File decoding error", $"(but it was expected) file name: {safeFilePath}.{extension}", LogSeverity.INFO);
                }
                else
                {
                    throw;
                }
            }
            catch (InvalidDataException)
            {
                if (tryOldDecoding)
                {
                    PACSingletons.Instance.Logger.Log("File decompression error", $"(but it was expected) file name: {safeFilePath}.{extension}", LogSeverity.INFO);
                }
                else
                {
                    throw;
                }
            }

            return PACTools.DecodeFileShort(filePath, OLD_SAVE_SEED, OLD_SAVE_EXT, lineNum, expected);
        }

        /// <summary>
        /// <inheritdoc cref="PACTools.DecodeFileShort(string, long, string, int, bool)"/>
        /// </summary>
        /// <inheritdoc cref="PACTools.DecodeFileShort(string, long, string, int, bool)"/>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileTypeName">The name of the file that is being loaded.</param>
        /// <param name="extraFileInformation">Extra information about the file to display in the log, if the file/folder can't be found.</param>
        /// <param name="tryOldDecoding">Whether to try to use <see cref="PACTools.DecodeFileShort(string, long, string, int, bool)"/>, if the newer decoding doesn't work.</param>
        public static JsonDictionary? LoadCompressedFileExpected<T>(
            string filePath,
            int lineNum = 0,
            string extension = SAVE_EXT,
            bool expected = true,
            string? fileTypeName = null,
            string? extraFileInformation = null,
            bool tryOldDecoding = true
        )
        {
            var objectTypeName = typeof(T).Name;
            try
            {
                var fileJson = LoadCompressedFile(filePath, lineNum, extension, expected, tryOldDecoding);
                if (fileJson is null)
                {
                    PACTools.LogJsonNullError<T>(objectTypeName, extraFileInformation, true);
                    return null;
                }
                return fileJson;
            }
            catch (Exception e)
            {
                fileTypeName ??= objectTypeName;
                if (e is FormatException)
                {
                    PACTools.LogJsonParseError<T>(objectTypeName, $"json couldn't be parsed from file{(extraFileInformation is null ? "" : $", {extraFileInformation}")}", true);
                    return null;
                }
                else if (e is FileNotFoundException)
                {
                    PACSingletons.Instance.Logger.Log($"{fileTypeName} file not found", $"{(expected ? "" : "(but it was expected)")} {extraFileInformation}", expected ? LogSeverity.ERROR : LogSeverity.INFO);
                    return null;
                }
                else if (e is DirectoryNotFoundException)
                {
                    PACSingletons.Instance.Logger.Log($"{fileTypeName} folder not found", $"{(expected ? "" : "(but it was expected)")} {extraFileInformation}", expected ? LogSeverity.ERROR : LogSeverity.INFO);
                    return null;
                }
                throw;
            }
        }
        #endregion

        #region Recreate folder
        /// <summary>
        /// <c>RecreateFolder</c> for the saves folder.
        /// </summary>
        /// <returns><inheritdoc cref="PACTools.RecreateFolder(string, string?, string?)"/></returns>
        public static bool RecreateSavesFolder()
        {
            return PACTools.RecreateFolder(SAVES_FOLDER);
        }

        /// <summary>
        /// <c>RecreateFolder</c> for the backups folder.
        /// </summary>
        /// <returns><inheritdoc cref="PACTools.RecreateFolder(string, string?, string?)"/></returns>
        public static bool RecreateBackupsFolder()
        {
            return PACTools.RecreateFolder(BACKUPS_FOLDER);
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
            return PACTools.RecreateFolder(Path.Join(SAVES_FOLDER_PATH, saveFolderName), $"save file: \"{saveFolderName}\"");
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
            return PACTools.RecreateFolder(Path.Join(GetSaveFolderPath(saveFolderName), SAVE_FOLDER_NAME_CHUNKS), $"chunks: \"{saveFolderName}\"");
        }
        #endregion

        #region Misc
        /// <summary>
        /// Returns what the save's folder path should be.
        /// </summary>
        /// <param name="saveFolderName">The save name.</param>
        public static string GetSaveFolderPath(string saveFolderName)
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
            return PASingletons.Instance.Settings.EnableColoredText ? Utils.StylizedText(text, foregroundColor, backgroundColor) : text;
        }

        /// <summary>
        /// Loads the default vlues for all variables that come from configs;
        /// </summary>
        public static void LoadDefaultConfigs()
        {
            SettingsUtils.LoadDefaultConfigs();
            EntityUtils.LoadDefaultConfigs();
            ItemUtils.LoadDefaultConfigs();
            WorldUtils.LoadDefaultConfigs();

            PACSingletons.Instance.Logger.Log("Loaded all default configs");
        }

        /// <summary>
        /// Resets all config files to their default states.
        /// </summary>
        public static void WriteDefaultConfigs()
        {
            SettingsUtils.WriteDefaultConfigs();
            EntityUtils.WriteDefaultConfigs();
            ItemUtils.WriteDefaultConfigs();
            WorldUtils.WriteDefaultConfigs();

            PACSingletons.Instance.Logger.Log("Reset all config files");
        }

        /// <summary>
        /// Displays the config progress for a config folder, if enabled.
        /// </summary>
        /// <param name="configPath">The path to the config file that is being reloaded, without extension</param>
        /// <param name="showProgressIndentation">If not null, shows the progress of loading the configs on the console.</param>
        public static void ReloadConfigsFolderDisplayProgress(string configPath, int? showProgressIndentation = null)
        {
            PACSingletons.Instance.Logger.Log(
                "Reloading from config folder",
                $"\"{configPath}\"",
                LogSeverity.DEBUG
            );

            if (showProgressIndentation is null)
            {
                return;
            }
            Console.WriteLine(new string(' ', (int)showProgressIndentation * 4) + $"Loading from folder \"{configPath}\":");
        }

        /// <summary>
        /// Reload all variables that come from configs.
        /// </summary>
        /// <param name="showProgressIndentation">If not null, shows the progress of loading the configs on the console.</param>
        public static void ReloadConfigs(int? showProgressIndentation = null)
        {
            var vanillaRecreated = ConfigUtils.TryGetLoadingOrderAndCorrect(out var loadingOrder);
            var configDatas = ConfigUtils.GetValidConfigDatas(null);
            var namespaces = loadingOrder
                .Where(lo => lo.Enabled)
                .Select(lo => configDatas.First(cd => cd.configData.Namespace == lo.Namespace).folderName)
                .ToList();

            SettingsUtils.ReloadConfigs(namespaces, vanillaRecreated, showProgressIndentation);
            EntityUtils.ReloadConfigs(namespaces, vanillaRecreated, showProgressIndentation);
            ItemUtils.ReloadConfigs(namespaces, vanillaRecreated, showProgressIndentation);
            WorldUtils.ReloadConfigs(namespaces, vanillaRecreated, showProgressIndentation);

            PACSingletons.Instance.Logger.Log("All configs reloaded");
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
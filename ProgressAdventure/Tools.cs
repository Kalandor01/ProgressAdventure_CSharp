using PACommon;
using PACommon.Enums;
using ProgressAdventure.SettingsManagement;
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
                PACSingletons.Instance.Logger.Log($"Made {(isTemporary ? "temporary " : "")}backup", displayBackupPath, isTemporary ? LogSeverity.DEBUG : LogSeverity.INFO);
                return (backupPath, displayBackupPath);
            }
            else
            {
                var displaySavePath = Path.Join(Constants.SAVES_FOLDER, saveFolderName);
                PACSingletons.Instance.Logger.Log($"{(isTemporary ? "Temporary b" : "B")}ackup failed", $"save folder not found: {displaySavePath}", LogSeverity.WARN);
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
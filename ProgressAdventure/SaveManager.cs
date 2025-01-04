using PACommon;
using PACommon.Enums;
using PACommon.JsonUtils;
using ProgressAdventure.Entity;
using ProgressAdventure.WorldManagement;
using System.Collections;
using System.Text;
using static PACommon.RealTimeCorrectedTextField;
using PACTools = PACommon.Tools;
using Utils = PACommon.Utils;

namespace ProgressAdventure
{
    public static class SaveManager
    {
        #region Public functions
        /// <summary>
        /// Creates a save file from the save data.<br/>
        /// Makes a temporary backup.
        /// </summary>
        /// <param name="clearChunks">Whether to clear all chunks from the world object, after saving.</param>
        /// <param name="showProgressText">If not null, it writes out a progress percentage with this string while saving.</param>
        public static void MakeSave(bool clearChunks = true, string? showProgressText = null)
        {
            // make backup
            var backupStatus = Tools.CreateBackup(SaveData.Instance.saveName, true);
            // DATA FILE
            SaveDataFile();
            // CHUNKS/WORLD
            Tools.RecreateChunksFolder();
            PACSingletons.Instance.Logger.Log("Saving chunks");
            World.SaveAllChunksToFiles(null, clearChunks, showProgressText);
            // remove backup
            if (backupStatus is not null)
            {
                File.Delete(backupStatus.Value.backupPath);
                PACSingletons.Instance.Logger.Log("Removed temporary backup", $"\"{backupStatus.Value.relativeBackupPath}\"", LogSeverity.DEBUG);
            }
        }

        /// <summary>
        /// Creates the data for a new save file.
        /// </summary>
        /// <param name="displaySaveName">The display name of the save file.</param>
        /// <param name="playerName">The name of the player.</param>
        public static void CreateSaveData(string? displaySaveName, string? playerName)
        {
            PACSingletons.Instance.Logger.Log("Preparing game data");
            // make save name
            var saveName = Tools.CorrectSaveName(displaySaveName);
            // random generators
            RandomStates.Initialize();
            // player
            var player = new Player(playerName);
            // load to class
            SaveData.Initialize(saveName, string.IsNullOrWhiteSpace(displaySaveName) ? saveName : displaySaveName, null, null, player, false);
            World.Initialize();
            World.GenerateTile((SaveData.Instance.player.position.x, SaveData.Instance.player.position.y));
        }

        /// <summary>
        /// Creates the data for a new save file, using user input.
        /// </summary>
        public static void CreateSaveData()
        {
            var displaySaveName = new RealTimeCorrectedTextField(
                "Name your save: ",
                new StringCorrectorDelegate(Tools.CorrectSaveName),
                clearScreen: false
            ).GetString(PASingletons.Instance.Settings.Keybinds.KeybindList);

            var playerName = new RealTimeCorrectedTextField(
                "What is your name?: ",
                new StringCorrectorDelegate(Tools.CorrectPlayerName),
                clearScreen: false
            ).GetString(PASingletons.Instance.Settings.Keybinds.KeybindList);

            CreateSaveData(displaySaveName, playerName);
        }

        /// <summary>
        /// Loads a save file into the <c>SaveData</c> object.
        /// </summary>
        /// <param name="saveName">The name of the save folder.</param>
        /// <param name="backupChoice">If the user can choose, whether to backup the save.</param>
        /// <param name="automaticBackup">If the save folder should be backed up. (only applies if <c>backupChoice</c> is false)</param>
        /// <param name="savesFolderPath">The path to the saves folder. By default, the current saves folder.</param>
        /// <exception cref="FileNotFoundException">Thrown, if the save file doesn't exist.</exception>
        /// <exception cref="FileLoadException">Thrown, if the save file doesn't have a save version.</exception>
        /// <returns>If the file was loaded without json load warnings.</returns>
        public static bool LoadSave(string saveName, bool backupChoice = true, bool automaticBackup = true, string? savesFolderPath = null)
        {
            var saveFolderPath = savesFolderPath is not null ? Path.Join(savesFolderPath, saveName) : Tools.GetSaveFolderPath(saveName);
            var dataFilePath = Path.Join(saveFolderPath, Constants.SAVE_FILE_NAME_DATA);

            if (!Directory.Exists(saveFolderPath))
            {
                PACSingletons.Instance.Logger.Log("Not a valid save folder", $"folder name: \"{saveName}\"", LogSeverity.ERROR);
                throw new FileNotFoundException("Not a valid save folder", saveName);
            }

            var data = Tools.LoadCompressedFile(dataFilePath, 1);

            if (data is null)
            {
                PACSingletons.Instance.Logger.Log("Save data is empty", $"save name: \"{saveName}\"", LogSeverity.ERROR);
                throw new FileLoadException("Save data is empty", saveName);
            }

            var success = true;
            // save version
            var fileVersion = GetSaveVersion<SaveData>(
                data,
                Constants.JsonKeys.SaveData.OLD_SAVE_VERSION,
                Constants.JsonKeys.SaveData.SAVE_VERSION,
                saveName
            );
            if (fileVersion is null)
            {
                PACSingletons.Instance.Logger.Log($"Unknown {typeof(SaveData).Name} version", $"{typeof(SaveData).Name} name: {saveName}", LogSeverity.ERROR);
                throw new FileLoadException("Unknown save version", saveName);
            }

            if (BackupSaveIfAppropriate(fileVersion, saveName, backupChoice, automaticBackup))
            {
                PACSingletons.Instance.Logger.Log("Save version is too old", $"save version is older than the oldest recognised version number, {Constants.OLDEST_SAVE_VERSION} -> {fileVersion}", LogSeverity.ERROR);
                fileVersion = Constants.OLDEST_SAVE_VERSION;
                success = false;
            }

            // LOADING
            PACSingletons.Instance.Logger.Log("Preparing game data");
            data[Constants.JsonKeys.SaveData.SAVE_NAME] = new JsonValue(saveName);
            success &= PACTools.TryFromJson<SaveData>(data, fileVersion, out _);
            PACSingletons.Instance.Logger.Log("Game data loaded", $"save name: \"{SaveData.Instance.saveName}\", player name: \"{SaveData.Instance.player.FullName}\", last saved: {Utils.MakeDate(SaveData.Instance.LastSave)} {Utils.MakeTime(SaveData.Instance.LastSave)}, playtime: {SaveData.Instance.Playtime}");
            World.Initialize();
            return success;
        }

        /// <summary>
        /// Gets all save files from the save folder, and proceses them for display.
        /// </summary>
        /// <param name="savesFolderPath">The path of the saves folder.</param>
        public static List<(string saveName, string displayText)> GetSavesData(string? savesFolderPath = null)
        {
            Tools.RecreateSavesFolder();
            // read saves
            var folders = GetSaveFolders(savesFolderPath);
            var datas = GetFoldersDisplayData(folders);
            // process file data
            var datasProcessed = new List<(string saveName, string displayText)>();
            foreach (var data in datas)
            {
                if (data.data is null)
                {
                    PACSingletons.Instance.Logger.Log("Decode error", $"save name: {data.folderName}", LogSeverity.ERROR);
                    Utils.PressKey($"\"{data.folderName}\" is corrupted!");
                }
                else
                {
                    var processedData = ProcessSaveDisplayData(data);
                    if (processedData is not null)
                    {
                        datasProcessed.Add(((string saveName, string displayText))processedData);
                    }
                }
            }
            return datasProcessed;
        }

        /// <summary>
        /// Gets the save version of the file from the json data.
        /// </summary>
        /// <typeparam name="T">The type of the object to get the version for.</typeparam>
        /// <param name="dataJson">The json representation of a file json data.</param>
        /// <param name="oldJsonKey">The pre-2.2 json key for the save version.</param>
        /// <param name="newJsonKey">The new json key for the save version.</param>
        /// <param name="fileName">The name of the currenly loaded save file.</param>
        public static string? GetSaveVersion<T>(JsonDictionary dataJson, string oldJsonKey, string newJsonKey, string fileName)
        {
            if (PACTools.TryParseJsonValue<T, string>(dataJson, newJsonKey, out var fileVersion))
            {
                return fileVersion;
            }

            if (PACTools.TryParseJsonValue<T, string>(dataJson, oldJsonKey, out var fileVersionBackup))
            {
                PACSingletons.Instance.Logger.Log($"Old style {typeof(T).Name} version (< 2.2)", $"{typeof(T).Name} name: {fileName}", LogSeverity.INFO);
                return fileVersionBackup;
            }
            return null;
        }
        #endregion

        #region Private functions
        /// <summary>
        /// Creates the data file part of a save file from the save data.
        /// </summary>
        private static void SaveDataFile()
        {
            // FOLDER
            Tools.RecreateSaveFileFolder();
            var saveFolderPath = Tools.GetSaveFolderPath();
            // DATA FILE
            var displayData = DisplaySaveData.ToJsonFromSaveData(SaveData.Instance);
            var mainData = SaveData.Instance.ToJson();
            // create new save
            Tools.SaveCompressedFile([displayData, mainData], Path.Join(saveFolderPath, Constants.SAVE_FILE_NAME_DATA));
        }

        /// <summary>
        /// Backs up the save if appropriate.
        /// </summary>
        /// <param name="fileVersion">The file version extracted from the json.</param>
        /// <param name="saveName">The name of the save folder.</param>
        /// <param name="backupChoice">If the user can choose, whether to backup the save.</param>
        /// <param name="automaticBackup">If the save folder should be backed up. (only applies if <paramref name="backupChoice"/> is false)</param>
        /// <returns>If the save version is older than the oldest recognized save version.</returns>
        private static bool BackupSaveIfAppropriate(string fileVersion, string saveName, bool backupChoice, bool automaticBackup)
        {
            if (!backupChoice && automaticBackup)
            {
                Tools.CreateBackup(saveName);
                return !Utils.IsUpToDate(Constants.OLDEST_SAVE_VERSION, fileVersion);
            }

            if (
                !backupChoice ||
                fileVersion == Constants.SAVE_VERSION
            )
            {
                return false;
            }

            var isOlder = !Utils.IsUpToDate(Constants.SAVE_VERSION, fileVersion);
            PACSingletons.Instance.Logger.Log("Trying to load save with an incorrect version", $"{fileVersion} -> {Constants.SAVE_VERSION}", LogSeverity.WARN);
            var createBackup = MenuManager.AskYesNoUIQuestion(
                $"\"{saveName}\" is {(isOlder ? "an older" : "a newer")} version than what it should be! Do you want to backup the save before loading it?",
                keybinds: PASingletons.Instance.Settings.Keybinds
            );

            if (createBackup)
            {
                Tools.CreateBackup(saveName);
            }

            return isOlder && !Utils.IsUpToDate(Constants.OLDEST_SAVE_VERSION, fileVersion);
        }

        /// <summary>
        /// Formats the json display data from a save file, into a displayable string.
        /// </summary>
        /// <param name="data">The save folder's name, and the data extracted from the data file's diplay data.</param>
        public static (string saveName, string displayText)? ProcessSaveDisplayData((string folderName, JsonDictionary? jsonData) data)
        {
            var folderName = data.folderName;
            var dataJson = data.jsonData;

            try
            {
                if (dataJson is null)
                {
                    PACSingletons.Instance.Logger.Log("Save display data parse error", $"no data in save file: {folderName}", LogSeverity.ERROR);
                    throw new ArgumentException("No data in save file.");
                }

                string? fileVersion = GetSaveVersion<DisplaySaveData>(
                    dataJson,
                    Constants.JsonKeys.SaveData.OLD_SAVE_VERSION,
                    Constants.JsonKeys.SaveData.SAVE_VERSION,
                    folderName
                );
                if (fileVersion is null)
                {
                    PACSingletons.Instance.Logger.Log($"Unknown {typeof(SaveData).Name} version", $"{typeof(SaveData).Name} name: {folderName}", LogSeverity.ERROR);
                    fileVersion = Constants.OLDEST_SAVE_VERSION;
                }

                PACTools.TryFromJson(dataJson, fileVersion, out DisplaySaveData? displaySaveData);
                if (displaySaveData is null)
                {
                    throw new ArgumentNullException(nameof(displaySaveData), "Somehow the DisplaySaveData is null after being converted from json.");
                }

                var displayText = new StringBuilder();

                // display name
                var displaySaveName = displaySaveData.displaySaveName ?? folderName;
                var playerName = displaySaveData.playerName ?? "[UNKNOWN PLAYER NAME]";
                var lastSave = displaySaveData.lastSave ?? DateTime.Now;
                var playtime = displaySaveData.playtime ?? TimeSpan.Zero;
                var displayFileVersion = displaySaveData.saveVersion ?? "[UNKNOWN VERSION]";

                var isNewestVersion = displayFileVersion == Constants.SAVE_VERSION;

                displayText.Append($"{displaySaveName}: {playerName}\n");
                displayText.Append($"Last saved: {Utils.MakeDate(lastSave, ".")} {Utils.MakeTime(lastSave)} ");
                displayText.Append($"Playtime: {playtime}");
                displayText.Append(Tools.StylizedText($" v.{displayFileVersion}", isNewestVersion ? Constants.Colors.GREEN : Constants.Colors.RED));

                return (folderName, displayText.ToString());
            }
            catch (Exception ex)
            {
                if (ex is InvalidCastException || ex is ArgumentException || ex is KeyNotFoundException)
                {
                    PACSingletons.Instance.Logger.Log("Save display data parse error", $"Save name: {folderName}, exception: " + ex.ToString(), LogSeverity.ERROR);
                    Utils.PressKey($"\"{folderName}\" could not be parsed!");
                    return null;
                }
                throw;
            }
        }

        /// <summary>
        /// Gets all folders from the saves folder.
        /// </summary>
        /// <param name="savesFolderPath">The path of the saves folder.</param>
        private static List<string> GetSaveFolders(string? savesFolderPath = null)
        {
            var folders = new List<string>();
            var folderPaths = Directory.GetDirectories(savesFolderPath ?? Constants.SAVES_FOLDER_PATH);
            var dataFileName = $"{Constants.SAVE_FILE_NAME_DATA}.{Constants.SAVE_EXT}";
            foreach (var folderPath in folderPaths)
            {
                if (File.Exists(Path.Join(folderPath, dataFileName)))
                {
                    folders.Add(Path.GetFileName(folderPath));
                }
            }
            folders.Sort();
            return folders;
        }

        /// <summary>
        /// Gets the display data from all save files in the saves folder.
        /// </summary>
        /// <param name="folders">A list of valid save folders.</param>
        /// <returns>A list of tuples, containing the folder name, and the data in it. The data will be null, if the folder wasn't readable.</returns>
        private static List<(string folderName, JsonDictionary? data)> GetFoldersDisplayData(IEnumerable<string> folders)
        {
            var datas = new List<(string folderName, JsonDictionary? data)>();
            foreach (var folder in folders)
            {
                JsonDictionary? data = null;
                try
                {
                    data = Tools.LoadCompressedFile(Path.Join(Tools.GetSaveFolderPath(folder), Constants.SAVE_FILE_NAME_DATA), 0);
                }
                catch (FormatException) { }
                datas.Add((folder, data));
            }
            return datas;
        }
        #endregion
    }
}

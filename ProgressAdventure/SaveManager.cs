using PACommon;
using PACommon.Enums;
using PACommon.Logging;
using ProgressAdventure.Entity;
using ProgressAdventure.SettingsManagement;
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
            Logger.Instance.Log("Saving chunks");
            World.SaveAllChunksToFiles(null, clearChunks, showProgressText);
            // remove backup
            if (backupStatus is not null)
            {
                File.Delete(backupStatus.Value.backupPath);
                Logger.Instance.Log("Removed temporary backup", backupStatus.Value.relativeBackupPath, LogSeverity.DEBUG);
            }
        }

        /// <summary>
        /// Creates the data for a new save file.
        /// </summary>
        /// <param name="displaySaveName">The display name of the save file.</param>
        /// <param name="playerName">The name of the player.</param>
        public static void CreateSaveData(string? displaySaveName, string? playerName)
        {
            Logger.Instance.Log("Preparing game data");
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
            ).GetString(Settings.Keybinds.KeybindList);

            var playerName = new RealTimeCorrectedTextField(
                "What is your name?: ",
                new StringCorrectorDelegate(Tools.CorrectPlayerName),
                clearScreen: false
            ).GetString(Settings.Keybinds.KeybindList);

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
        public static void LoadSave(string saveName, bool backupChoice = true, bool automaticBackup = true, string? savesFolderPath = null)
        {
            var saveFolderPath = savesFolderPath is not null ? Path.Join(savesFolderPath, saveName) : Tools.GetSaveFolderPath(saveName);
            var dataFilePath = Path.Join(saveFolderPath, Constants.SAVE_FILE_NAME_DATA);

            if (!Directory.Exists(saveFolderPath))
            {
                Logger.Instance.Log("Not a valid save folder", $"folder name: {saveName}", LogSeverity.ERROR);
                throw new FileNotFoundException("Not a valid save folder", saveName);
            }

            var data = Tools.DecodeSaveShort(dataFilePath, 1);

            // auto backup?
            if (!backupChoice && automaticBackup)
            {
                Tools.CreateBackup(saveName);
            }

            if (data is null)
            {
                Logger.Instance.Log("Save data is empty", $"save name: {saveName}", LogSeverity.ERROR);
                throw new FileLoadException("Save data is empty", saveName);
            }

            // save version
            string fileVersion = GetSaveVersion(data, saveName) ?? throw new FileLoadException("Unknown save version", saveName);

            if (fileVersion != Constants.SAVE_VERSION)
            {
                // backup
                if (backupChoice)
                {
                    var isOlder = !Utils.IsUpToDate(Constants.SAVE_VERSION, fileVersion);
                    Logger.Instance.Log("Trying to load save with an incorrect version", $"{fileVersion} -> {Constants.SAVE_VERSION}", LogSeverity.WARN);
                    var createBackup = MenuManager.AskYesNoUIQuestion(
                        $"\"{saveName}\" is {(isOlder ? "an older version" : "a newer version")} than what it should be! Do you want to backup the save before loading it?",
                        keybinds: Settings.Keybinds
                    );
                    if (createBackup)
                    {
                        Tools.CreateBackup(saveName);
                    }
                    // correct too old save version
                    if (isOlder && !Utils.IsUpToDate(Constants.OLDEST_SAVE_VERSION, fileVersion))
                    {
                        Logger.Instance.Log("Save version is too old", $"save version is older than the oldest recognised version number, {Constants.OLDEST_SAVE_VERSION} -> {fileVersion}", LogSeverity.ERROR);
                        fileVersion = Constants.OLDEST_SAVE_VERSION;
                    }
                }
            }

            // LOADING
            Logger.Instance.Log("Preparing game data");
            data[Constants.JsonKeys.SaveData.SAVE_NAME] = saveName;
            PACTools.TryFromJson<SaveData>(data, fileVersion, out _);
            Logger.Instance.Log("Game data loaded", $"save name: {SaveData.Instance.saveName}, player name: \"{SaveData.Instance.player.FullName}\", last saved: {Utils.MakeDate(SaveData.Instance.LastSave)} {Utils.MakeTime(SaveData.Instance.LastSave)}, playtime: {SaveData.Instance.Playtime}");
            World.Initialize();
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
                    Logger.Instance.Log("Decode error", $"save name: {data.folderName}", LogSeverity.ERROR);
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
            Tools.EncodeSaveShort(new List<IDictionary> { displayData, mainData }, Path.Join(saveFolderPath, Constants.SAVE_FILE_NAME_DATA));
        }

        /// <summary>
        /// Gets the save version of the file from the display/normal json data.
        /// </summary>
        /// <param name="dataJson">The json representation of the (display) save data.</param>
        /// <param name="saveName">The name of the currenly loaded save file.</param>
        private static string? GetSaveVersion(IDictionary<string, object?> dataJson, string saveName)
        {
            if (
                dataJson.TryGetValue("save_version", out object? versionValue) &&
                versionValue is not null &&
                versionValue is string fileVersion
            )
            {
                return fileVersion;
            }

            // old save version key
            if (
                dataJson.TryGetValue("saveVersion", out object? versionValueBackup) &&
                versionValueBackup is not null &&
                versionValueBackup is string fileVersionBackup
            )
            {
                Logger.Instance.Log("Old style save version (< 2.2)", $"save name: {saveName}", LogSeverity.INFO);
                return fileVersionBackup;
            }

            Logger.Instance.Log("Unknown save version", $"save name: {saveName}", LogSeverity.ERROR);
            return null;
        }

        /// <summary>
        /// Formats the json display data from a save file, into a displayable string.
        /// </summary>
        /// <param name="data">The save folder's name, and the data extracted from the data file's diplay data.</param>
        private static (string saveName, string displayText)? ProcessSaveDisplayData((string folderName, Dictionary<string, object?>? jsonData) data)
        {
            var folderName = data.folderName;
            var dataJson = (IDictionary<string, object?>?)data.jsonData;

            try
            {
                if (dataJson is null)
                {
                    Logger.Instance.Log("Save display data parse error", $"no data in save file: {folderName}", LogSeverity.ERROR);
                    throw new ArgumentException("No data in save file.");
                }

                string? fileVersion = GetSaveVersion(dataJson, folderName);

                PACTools.TryFromJson(dataJson, fileVersion ?? Constants.OLDEST_SAVE_VERSION, out DisplaySaveData? displaySaveData);
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
                    Logger.Instance.Log("Save display data parse error", $"Save name: {folderName}, exception: " + ex.ToString(), LogSeverity.ERROR);
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
        private static List<(string folderName, Dictionary<string, object?>? data)> GetFoldersDisplayData(IEnumerable<string> folders)
        {
            var datas = new List<(string folderName, Dictionary<string, object?>? data)>();
            foreach (var folder in folders)
            {
                Dictionary<string, object?>? data = null;
                try
                {
                    data = Tools.DecodeSaveShort(Path.Join(Tools.GetSaveFolderPath(folder), Constants.SAVE_FILE_NAME_DATA), 0);
                }
                catch (FormatException) { }
                datas.Add((folder, data));
            }
            return datas;
        }
        #endregion
    }
}

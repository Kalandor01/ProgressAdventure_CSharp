using ProgressAdventure.Entity;
using ProgressAdventure.Enums;
using ProgressAdventure.SettingsManagement;
using ProgressAdventure.WorldManagement;
using SaveFileManager;
using System.Collections;
using System.Text;

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
            var backupStatus = Tools.CreateBackup(SaveData.saveName, true);
            // DATA FILE
            SaveDataFile();
            // CHUNKS/WORLD
            Tools.RecreateChunksFolder();
            Logger.Log("Saving chunks");
            World.SaveAllChunksToFiles(null, clearChunks, showProgressText);
            // remove backup
            if (backupStatus is not null)
            {
                File.Delete(backupStatus.Value.backupPath);
                Logger.Log("Removed temporary backup", backupStatus.Value.relativeBackupPath, LogSeverity.DEBUG);
            }
        }

        /// <summary>
        /// Creates the data for a new save file.
        /// </summary>
        /// <param name="displaySaveName">The display name of the save file.</param>
        /// <param name="playerName">The name of the player.</param>
        public static void CreateSaveData(string? displaySaveName, string? playerName)
        {
            Logger.Log("Preparing game data");
            // make save name
            var saveName = Tools.CorrectSaveName(displaySaveName);
            // random generators
            RandomStates.Initialise();
            // player
            var player = new Player(playerName);
            // load to class
            SaveData.Initialise(saveName, string.IsNullOrWhiteSpace(displaySaveName) ? saveName : displaySaveName, null, null, player, false);
            World.Initialise();
            World.GenerateTile((SaveData.player.position.x, SaveData.player.position.y));
        }

        /// <summary>
        /// Creates the data for a new save file, using user input.
        /// </summary>
        public static void CreateSaveData()
        {
            var displaySaveName = Tools.GetRealTimeCorrectedString("Name your save: ", new Tools.StringCorrectorDelegate(Tools.CorrectSaveName), clearScreen: false);
            var playerName = Tools.GetRealTimeCorrectedString("What is your name?: ", new Tools.StringCorrectorDelegate(Tools.CorrectPlayerName), clearScreen: false);
            CreateSaveData(displaySaveName, playerName);
        }

        /// <summary>
        /// Loads a save file into the <c>SaveData</c> object.
        /// </summary>
        /// <param name="saveName">The name of the save folder.</param>
        /// <param name="backupChoice">If the user can choose, whether to backup the save, or not.</param>
        /// <param name="automaticBackup">If the save folder should be backed up or not. (only applies if <c>backupChoice</c> is false)</param>
        /// <param name="savesFolderPath">The path to the saves folder. By default, the current saves folder.</param>
        /// <exception cref="FileNotFoundException">Thrown, if the save file doesn't exist.</exception>
        /// <exception cref="FileLoadException">Thrown, if the save file doesn't have a save version.</exception>
        public static void LoadSave(string saveName, bool backupChoice = true, bool automaticBackup = true, string? savesFolderPath = null)
        {
            var saveFolderPath = savesFolderPath is not null ? Path.Join(savesFolderPath, saveName) : Tools.GetSaveFolderPath(saveName);
            Dictionary<string, object?>? data;

            if (Directory.Exists(saveFolderPath))
            {
                data = Tools.DecodeSaveShort(Path.Join(saveFolderPath, Constants.SAVE_FILE_NAME_DATA), 1);
            }
            else
            {
                Logger.Log("Not a valid save folder", $"folder name: {saveName}", LogSeverity.ERROR);
                throw new FileNotFoundException("Not a valid save folder", saveName);
            }
            // read data
    
            // auto backup
            if (!backupChoice && automaticBackup)
            {
                Tools.CreateBackup(saveName);
            }

            if (data is null)
            {
                Logger.Log("Save data is empty", $"save name: {saveName}", LogSeverity.ERROR);
                throw new FileLoadException("Save data is empty", saveName);
            }

            // save version
            string saveVersion;
            if (data.TryGetValue("save_version", out object? versionValue) && versionValue is not null)
            {
                saveVersion = (string)versionValue;
            }
            else
            {
                // old save version key
                if (data.TryGetValue("saveVersion", out object? versionValueBackup) && versionValueBackup is not null)
                {
                    Logger.Log("Old style save version (< 2.2)", $"save name: {saveName}", LogSeverity.INFO);

                    saveVersion = (string)versionValueBackup;
                }
                else
                {
                    Logger.Log("Unknown save version", $"save name: {saveName}", LogSeverity.ERROR);
                    throw new FileLoadException("Unknown save version", saveName);
                }
            }

            if (saveVersion != Constants.SAVE_VERSION)
            {
                // backup
                if (backupChoice)
                {
                    var isOlder = !Tools.IsUpToDate(Constants.SAVE_VERSION, saveVersion);
                    Logger.Log("Trying to load save with an incorrect version", $"{saveVersion} -> {Constants.SAVE_VERSION}", LogSeverity.WARN);
                    var ans = (int)new UIList(new string[] { "Yes", "No" }, $"\"{saveName}\" is {(isOlder ? "an older version" : "a newer version")} than what it should be! Do you want to backup the save before loading it?").Display(Settings.Keybinds.KeybindList);
                    if (ans == 0)
                    {
                        Tools.CreateBackup(saveName);
                    }
                    // correct too old save version
                    if (isOlder && !Tools.IsUpToDate(Constants.OLDEST_SAVE_VERSION, saveVersion))
                    {
                        Logger.Log("Save version is too old", $"save version is older than the oldest recognised version number, {Constants.OLDEST_SAVE_VERSION} -> {saveVersion}", LogSeverity.ERROR);
                        saveVersion = Constants.OLDEST_SAVE_VERSION;
                    }
                }
                // correct
                CorrectSaveData(data, saveVersion);
            }

            // load random states
            var randomStates = data["random_states"] as IDictionary<string, object?>;
            RandomStates.FromJson(randomStates, saveVersion);
            // display name
            var displayName = (string?)data["display_name"];
            // last save
            var lastSave = (DateTime?)data["last_save"];
            // playtime
            _ = TimeSpan.TryParse(data["playtime"]?.ToString(), out var playtime);
            // player
            var playerData = data["player"] as IDictionary<string, object?>;
            Tools.FromJson(playerData, saveVersion, out Player? player);
            Logger.Log("Loaded save data from json", $"save name: {saveName}");

            // PREPARING
            Logger.Log("Preparing game data");
            // load to class
            SaveData.Initialise(saveName, displayName, lastSave, playtime, player, false);
            Logger.Log("Game data loaded", $"save name: {saveName}, player name: \"{SaveData.player.FullName}\", last saved: {Utils.MakeDate(SaveData.LastSave)} {Utils.MakeTime(SaveData.LastSave)}, playtime: {SaveData.Playtime}");
            World.Initialise();
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
                    Logger.Log("Decode error", $"save name: {data.folderName}", LogSeverity.ERROR);
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
            var (displayData, mainData) = SaveData.MakeSaveData();
            // create new save
            Tools.EncodeSaveShort(new List<IDictionary> { displayData, mainData }, Path.Join(saveFolderPath, Constants.SAVE_FILE_NAME_DATA));
        }

        /// <summary>
        /// Modifies the save data, to make it up to date, with the newest save file data structure.
        /// </summary>
        /// <param name="jsonData">The json representation of the jave data.</param>
        /// <param name="fileVersion">The original version of the save file.</param>
        private static void CorrectSaveData(Dictionary<string, object?> jsonData, string fileVersion)
        {
            Logger.Log("Correcting save data");
            //correct data
            // 2.0.1 -> 2.0.2
            var newFileVersion = "2.0.2";
            if (!Tools.IsUpToDate(newFileVersion, fileVersion))
            {
                // saved entity types
                if (
                    jsonData.TryGetValue("player", out object? playerJson) &&
                    playerJson is IDictionary<string, object?> playerDict
                )
                {
                    playerDict["type"] = "player";
                }

                Logger.Log("Corrected save data", $"{fileVersion} -> {newFileVersion}", LogSeverity.DEBUG);
                fileVersion = newFileVersion;
            }
            // 2.1.1 -> 2.2
            newFileVersion = "2.2";
            if (!Tools.IsUpToDate(newFileVersion, fileVersion))
            {
                // snake case rename
                if (jsonData.TryGetValue("randomStates", out object? rsRename))
                {
                    jsonData["random_states"] = rsRename;
                }
                if (jsonData.TryGetValue("displayName", out object? dnRename))
                {
                    jsonData["display_name"] = dnRename;
                }
                if (jsonData.TryGetValue("lastSave", out object? lsRename))
                {
                    jsonData["last_save"] = lsRename;
                }

                Logger.Log("Corrected save data", $"{fileVersion} -> {newFileVersion}", LogSeverity.DEBUG);
                fileVersion = newFileVersion;
            }
        }

        /// <summary>
        /// Formats the json display data from a save file, into a displayable string.
        /// </summary>
        /// <param name="data">The save folder's name, and the data extracted from the data file's diplay data.</param>
        private static (string saveName, string displayText)? ProcessSaveDisplayData((string folderName, Dictionary<string, object?>? data) data)
        {
            try
            {
                if (data.data is null)
                {
                    Logger.Log("Save display data parse error", $"no data in save file: {data.folderName}", LogSeverity.ERROR);
                    throw new ArgumentException("No data in save file.");
                }

                // get save version
                string? saveVersion;
                if (data.data.TryGetValue("save_version", out object? versionValue) && versionValue is not null)
                {
                    saveVersion = (string)versionValue;
                }
                else
                {
                    // old save version key
                    if (data.data.TryGetValue("saveVersion", out object? versionValueBackup) && versionValueBackup is not null)
                    {
                        Logger.Log("Old style save version (< 2.2)", $"save name: {data.folderName}", LogSeverity.INFO);

                        saveVersion = (string)versionValueBackup;
                    }
                    else
                    {
                        Logger.Log("Unknown save version", $"save name: {data.folderName}", LogSeverity.ERROR);
                        saveVersion = null;
                    }
                }


                // correct data
                var currentCorrectedSaveVersion = saveVersion;
                if (currentCorrectedSaveVersion is not null && !Tools.IsUpToDate(Constants.SAVE_VERSION, currentCorrectedSaveVersion))
                {
                    Logger.Log($"Save display json data is old", "correcting data");
                    // 2.1.1 -> 2.2
                    var newSaveVersion = "2.2";
                    if (!Tools.IsUpToDate(newSaveVersion, currentCorrectedSaveVersion))
                    {
                        // item material
                        if (data.data.TryGetValue("displayName", out var rsRename))
                        {
                            data.data["display_name"] = rsRename;
                        }
                        if (data.data.TryGetValue("playerName", out var pnRename))
                        {
                            data.data["player_name"] = pnRename;
                        }
                        if (data.data.TryGetValue("lastSave", out var lsRename))
                        {
                            data.data["last_save"] = lsRename;
                        }

                        Logger.Log("Corrected save display json data", $"{currentCorrectedSaveVersion} -> {newSaveVersion}", LogSeverity.DEBUG);
                        currentCorrectedSaveVersion = newSaveVersion;
                    }
                    Logger.Log($"Save display json data corrected");
                }


                var displayText = new StringBuilder();

                // display name
                var displayName = data.data["display_name"] ?? data.folderName;
                displayText.Append($"{displayName}: {data.data["player_name"]}\n");

                // last save
                var lastSave = (DateTime)(data.data["last_save"] ?? DateTime.Now);
                displayText.Append($"Last saved: {Utils.MakeDate(lastSave, ".")} {Utils.MakeTime(lastSave)} ");

                // playtime
                var response = TimeSpan.TryParse(data.data["playtime"]?.ToString(), out var playtime);
                if (!response)
                {
                    playtime = TimeSpan.Zero;
                }
                displayText.Append($"Playtime: {playtime}");

                // check version
                saveVersion ??= "[UNKNOWN VERSION]";
                displayText.Append(Tools.StylizedText($" v.{saveVersion}", saveVersion == Constants.SAVE_VERSION ? Constants.Colors.GREEN : Constants.Colors.RED));
                    
                return (data.folderName, displayText.ToString());
            }
            catch (Exception ex)
            {
                if (ex is InvalidCastException || ex is ArgumentException || ex is KeyNotFoundException)
                {
                    Logger.Log("Save display data parse error", $"Save name: {data.folderName}, exception: " + ex.ToString(), LogSeverity.ERROR);
                    Utils.PressKey($"\"{data.folderName}\" could not be parsed!");
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

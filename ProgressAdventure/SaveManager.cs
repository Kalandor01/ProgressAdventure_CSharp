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
            SaveData.Initialise(saveName, displaySaveName, null, null, player, false);
            World.Initialise();
            World.GenerateTile((SaveData.player.position.x, SaveData.player.position.y));
        }

        /// <summary>
        /// Creates the data for a new save file, using user input.
        /// </summary>
        public static void CreateSaveData()
        {
            var displaySaveName = Utils.Input("Name your save: ");
            var playerName = Utils.Input("What is your name?: ");
            CreateSaveData(displaySaveName, playerName);
        }

        /// <summary>
        /// Loads a save file into the <c>SaveData</c> object.
        /// </summary>
        /// <param name="saveName">The name of the save folder.</param>
        /// <param name="backupChoice">If the user can choose, whether to backup the save, or not.</param>
        /// <param name="automaticBackup">If the save folder should be backed up or not. (only applies if <c>backupChoice</c> is false)</param>
        public static void LoadSave(string saveName, bool backupChoice = true, bool automaticBackup = true)
        {
            var saveFolderPath = Tools.GetSaveFolderPath(saveName);
            // get if save is a file
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
                Logger.Log("Unknown save version", $"save name: {saveName}", LogSeverity.ERROR);
                throw new FileLoadException("Unknown save version", saveName);
            }

            // save version
            string saveVersion;
            if (data.TryGetValue("saveVersion", out object? versionValue) && versionValue is not null)
            {
                saveVersion = (string)versionValue;
            }
            else
            {
                Logger.Log("Unknown save version", $"save name: {saveName}", LogSeverity.ERROR);
                throw new FileLoadException("Unknown save version", saveName);
            }

            if (saveVersion != Constants.SAVE_VERSION)
            {
                // backup
                if (backupChoice)
                {
                    var isOlder = Tools.IsUpToDate(saveVersion, Constants.SAVE_VERSION);
                    Logger.Log("Trying to load save with an incorrect version", $"{saveVersion} -> {Constants.SAVE_VERSION}", LogSeverity.WARN);
                    var ans = (int)new UIList(new string[] { "Yes", "No" }, $"\"{saveName}\" is {(isOlder ? "an older version" : "a newer version")} than what it should be! Do you want to backup the save before loading it?").Display(Settings.Keybinds.KeybindList);
                    if (ans == 0)
                    {
                        Tools.CreateBackup(saveName);
                    }
                }
                // correct
                CorrectSaveData(data, saveVersion);
            }
            // load random states
            var randomStates = (IDictionary<string, object?>?)data["randomStates"];
            RandomStates.FromJson(randomStates);
            // display name
            var displayName = (string?)data["displayName"];
            // last save
            var lastSave = (DateTime?)data["lastSave"];
            // playtime
            _ = TimeSpan.TryParse(data["playtime"]?.ToString(), out var playtime);
            // player
            var playerData = (IDictionary<string, object?>?)data["player"];
            var player = Player.FromJson(playerData);
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
        public static List<(string saveName, string displayText)> GetSavesData()
        {
            Tools.RecreateSavesFolder();
            // read saves
            var folders = GetSaveFolders();
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
        /// <param name="saveVersion">The original version of the save file.</param>
        private static void CorrectSaveData(Dictionary<string, object?> jsonData, string saveVersion)
        {
            Logger.Log("Correcting save data");
            // 2.0 -> 2.0.1
            if (saveVersion == "2.0")
            {
                var pSaveVersion = saveVersion;
                // inventory items in dictionary

                var inventoryItems = ((Dictionary<string, object?>)jsonData["player"])["inventory"];
                ((Dictionary<string, object?>)jsonData["player"])["inventory"] = new Dictionary<string, object> { ["items"] = inventoryItems };
                
                saveVersion = "2.0.1";
                Logger.Log("Corrected save data", $"{pSaveVersion} -> {saveVersion}", LogSeverity.DEBUG);
            }
        }

        /// <summary>
        /// Turns the json display data from a json into more uniform data.
        /// </summary>
        /// <param name="data"></param>
        private static (string saveName, string displayText)? ProcessSaveDisplayData((string folderName, Dictionary<string, object?>? data) data)
        {
            try
            {
                if (data.data is not null)
                {
                    var displayText = new StringBuilder();
                    var displayName = data.data["displayName"] ?? data.folderName;
                    displayText.Append($"{displayName}: {data.data["playerName"]}\n");
                    var lastSave = (DateTime)(data.data["lastSave"] ?? DateTime.Now);
                    var res = TimeSpan.TryParse(data.data["playtime"]?.ToString(), out var playtime);
                    if (!res)
                    {
                        playtime = TimeSpan.Zero;
                    }
                    displayText.Append($"Last saved: {Utils.MakeDate(lastSave, ".")} {Utils.MakeTime(lastSave)}");
                    displayText.Append($"Playtime: {playtime}");
                    // check version
                    var saveVersion = (string)(data.data["saveVersion"] ?? "[UNKNOWN VERSION]");
                    displayText.Append(Utils.StylizedText($" v.{saveVersion}", saveVersion == Constants.SAVE_VERSION ? Constants.Colors.GREEN : Constants.Colors.RED));
                    return (data.folderName, displayText.ToString());
                }
                else
                {
                    throw new ArgumentException("No data in save file.");
                }
            }
            catch (Exception ex)
            {
                if (ex is InvalidCastException || ex is ArgumentException || ex is KeyNotFoundException)
                {
                    Logger.Log("Parse error", $"Save name: {data.folderName}", LogSeverity.ERROR);
                    Utils.PressKey($"\"{data.folderName}\" could not be parsed!");
                    return null;
                }
                throw;
            }
        }

        /// <summary>
        /// Gets all folders from the saves folder.
        /// </summary>
        private static List<string> GetSaveFolders()
        {
            var folders = new List<string>();
            var folderPaths = Directory.GetDirectories(Constants.SAVES_FOLDER_PATH);
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

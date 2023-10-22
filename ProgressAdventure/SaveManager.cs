using PACommon;
using PACommon.Enums;
using ProgressAdventure.Entity;
using ProgressAdventure.SettingsManagement;
using ProgressAdventure.WorldManagement;
using SaveFileManager;
using System.Collections;
using System.Text;
using static PACommon.RealTimeCorrectedTextField;
using PACTools = PACommon.Tools;
using Utils = PACommon.Utils;

namespace ProgressAdventure
{
    public static class SaveManager
    {
        #region Private dicts
        private static readonly List<(Action<IDictionary<string, object?>> objectJsonCorrecter, string newFileVersion)> displayDataversionCorrecters = new()
        {
            // 2.1.1 -> 2.2
            (oldJson => {
                // snake case rename
                if (oldJson.TryGetValue("displayName", out var rsRename))
                {
                    oldJson["display_name"] = rsRename;
                }
                if (oldJson.TryGetValue("playerName", out var pnRename))
                {
                    oldJson["player_name"] = pnRename;
                }
                if (oldJson.TryGetValue("lastSave", out var lsRename))
                {
                    oldJson["last_save"] = lsRename;
                }
            }, "2.2"),
        };

        private static readonly List<(Action<IDictionary<string, object?>> objectJsonCorrecter, string newFileVersion)> dataFileversionCorrecters = new()
        {
            // 2.1.1 -> 2.2
            (oldJson => {
                // snake case rename
                if (oldJson.TryGetValue("randomStates", out object? rsRename))
                {
                    oldJson["random_states"] = rsRename;
                }
            }, "2.2"),
        };
        #endregion

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
            World.GenerateTile((SaveData.player.position.x, SaveData.player.position.y));
        }

        /// <summary>
        /// Creates the data for a new save file, using user input.
        /// </summary>
        public static void CreateSaveData()
        {
            var displaySaveName = new RealTimeCorrectedTextField("Name your save: ", new StringCorrectorDelegate(Tools.CorrectSaveName), clearScreen: false).GetString(Settings.Keybinds.KeybindList);
            var playerName = new RealTimeCorrectedTextField("What is your name?: ", new StringCorrectorDelegate(Tools.CorrectPlayerName), clearScreen: false).GetString(Settings.Keybinds.KeybindList);
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
            IDictionary<string, object?>? data;

            if (Directory.Exists(saveFolderPath))
            {
                data = Tools.DecodeSaveShort(Path.Join(saveFolderPath, Constants.SAVE_FILE_NAME_DATA), 1);
            }
            else
            {
                Logger.Instance.Log("Not a valid save folder", $"folder name: {saveName}", LogSeverity.ERROR);
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
                Logger.Instance.Log("Save data is empty", $"save name: {saveName}", LogSeverity.ERROR);
                throw new FileLoadException("Save data is empty", saveName);
            }

            // save version
            string fileVersion;
            if (data.TryGetValue("save_version", out object? versionValue) && versionValue is not null)
            {
                fileVersion = (string)versionValue;
            }
            else
            {
                // old save version key
                if (data.TryGetValue("saveVersion", out object? versionValueBackup) && versionValueBackup is not null)
                {
                    Logger.Instance.Log("Old style save version (< 2.2)", $"save name: {saveName}", LogSeverity.INFO);

                    fileVersion = (string)versionValueBackup;
                }
                else
                {
                    Logger.Instance.Log("Unknown save version", $"save name: {saveName}", LogSeverity.ERROR);
                    throw new FileLoadException("Unknown save version", saveName);
                }
            }

            if (fileVersion != Constants.SAVE_VERSION)
            {
                // backup
                if (backupChoice)
                {
                    var isOlder = !Utils.IsUpToDate(Constants.SAVE_VERSION, fileVersion);
                    Logger.Instance.Log("Trying to load save with an incorrect version", $"{fileVersion} -> {Constants.SAVE_VERSION}", LogSeverity.WARN);
                    var ans = (int)new UIList(new string[] { "Yes", "No" }, $"\"{saveName}\" is {(isOlder ? "an older version" : "a newer version")} than what it should be! Do you want to backup the save before loading it?").Display(Settings.Keybinds.KeybindList);
                    if (ans == 0)
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
                // correct
                JsonDataCorrecter.Instance.CorrectJsonData(typeof(SaveData).ToString(), ref data, dataFileversionCorrecters, fileVersion);
            }

            // load random states
            var randomStates = data["random_states"] as IDictionary<string, object?>;
            PACTools.FromJson<RandomStates>(randomStates, fileVersion);

            // PREPARING
            Logger.Instance.Log("Preparing game data");
            // load to class
            SaveData.FromJson(saveName, data, fileVersion);
            Logger.Instance.Log("Game data loaded", $"save name: {SaveData.saveName}, player name: \"{SaveData.player.FullName}\", last saved: {Utils.MakeDate(SaveData.LastSave)} {Utils.MakeTime(SaveData.LastSave)}, playtime: {SaveData.Playtime}");
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
            var (displayData, mainData) = SaveData.MakeSaveData();
            // create new save
            Tools.EncodeSaveShort(new List<IDictionary> { displayData, mainData }, Path.Join(saveFolderPath, Constants.SAVE_FILE_NAME_DATA));
        }

        /// <summary>
        /// Formats the json display data from a save file, into a displayable string.
        /// </summary>
        /// <param name="data">The save folder's name, and the data extracted from the data file's diplay data.</param>
        private static (string saveName, string displayText)? ProcessSaveDisplayData((string folderName, Dictionary<string, object?>? data) data)
        {
            var folderName = data.folderName;
            var dataJson = (IDictionary<string, object?>?)data.data;

            try
            {
                if (dataJson is null)
                {
                    Logger.Instance.Log("Save display data parse error", $"no data in save file: {folderName}", LogSeverity.ERROR);
                    throw new ArgumentException("No data in save file.");
                }

                // get save version
                string? fileVersion;
                if (dataJson.TryGetValue("save_version", out object? versionValue) && versionValue is not null)
                {
                    fileVersion = (string)versionValue;
                }
                else
                {
                    // old save version key
                    if (dataJson.TryGetValue("saveVersion", out object? versionValueBackup) && versionValueBackup is not null)
                    {
                        Logger.Instance.Log("Old style save version (< 2.2)", $"save name: {data.folderName}", LogSeverity.INFO);

                        fileVersion = (string)versionValueBackup;
                    }
                    else
                    {
                        Logger.Instance.Log("Unknown save version", $"save name: {data.folderName}", LogSeverity.ERROR);
                        fileVersion = null;
                    }
                }


                // correct data
                JsonDataCorrecter.Instance.CorrectJsonData("Save display", ref dataJson, displayDataversionCorrecters, fileVersion ?? Constants.OLDEST_SAVE_VERSION);


                var displayText = new StringBuilder();

                // display name
                var displayName = dataJson["display_name"] ?? folderName;
                displayText.Append($"{displayName}: {dataJson["player_name"]}\n");

                // last save
                var lastSave = (DateTime)(dataJson["last_save"] ?? DateTime.Now);
                displayText.Append($"Last saved: {Utils.MakeDate(lastSave, ".")} {Utils.MakeTime(lastSave)} ");

                // playtime
                var response = TimeSpan.TryParse(dataJson["playtime"]?.ToString(), out var playtime);
                if (!response)
                {
                    playtime = TimeSpan.Zero;
                }
                displayText.Append($"Playtime: {playtime}");

                // check version
                fileVersion ??= "[UNKNOWN VERSION]";
                displayText.Append(Tools.StylizedText($" v.{fileVersion}", fileVersion == Constants.SAVE_VERSION ? Constants.Colors.GREEN : Constants.Colors.RED));
                    
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

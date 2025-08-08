using PACommon;
using PACommon.Enums;
using PACommon.JsonUtils;
using ProgressAdventure.ConfigManagement;
using ProgressAdventure.EntityManagement;
using ProgressAdventure.Extensions;
using ProgressAdventure.WorldManagement;
using System.Diagnostics.CodeAnalysis;
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
        /// <param name="threadManager">The <see cref="ThreadManager"/> to use to cancel the task.</param>
        public static void MakeSave(bool clearChunks = true, string? showProgressText = null, ThreadManager? threadManager = null)
        {
            if (threadManager?.IsCanceled == true)
            {
                return;
            }

            if (showProgressText is not null)
            {
                PACSingletons.Instance.ConsoleProxy.Write("|" + showProgressText + "\r");
            }
            // make backup
            var backupStatus = Tools.CreateBackup(SaveData.Instance.SaveName, true);

            // DATA FILE
            if (threadManager?.IsCanceled == true)
            {
                return;
            }
            SaveDataFile();

            // CHUNKS/WORLD
            if (threadManager?.IsCanceled == true)
            {
                return;
            }
            Tools.RecreateChunksFolder();
            PACSingletons.Instance.Logger.Log("Saving chunks");
            World.SaveAllChunksToFiles(null, clearChunks, showProgressText, threadManager);

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
        /// <param name="seedString">The seed string.</param>
        public static void CreateSaveData(string? displaySaveName, string? playerName, string? seedString)
        {
            PACSingletons.Instance.Logger.Log("Preparing game data");
            // make save name
            var saveName = Tools.CorrectSaveName(displaySaveName);
            // random generators
            RandomStates.Initialize(seedString is not null ? NPrngExtensionsPA.GetRandomFromString(seedString, out _) : null);
            // player
            var player = new Entity(EntityUtils.PlayerEntityType, playerName);
            // load to class
            SaveData.Initialize(
                saveName,
                string.IsNullOrWhiteSpace(displaySaveName) ? saveName : displaySaveName,
                player: player,
                initialiseRandomGenerators: false
            );
            World.Initialize();
            //explicitly resolve player
            _ = SaveData.Instance.PlayerRef;
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

            var seedText = new RealTimeCorrectedTextField(
                "Custom seed: ",
                new StringCorrectorDelegate(Tools.CorrectSeed),
                clearScreen: false
            ).GetString(PASingletons.Instance.Settings.Keybinds.KeybindList);

            string? seedString = null;
            if (seedText != "")
            {
                NPrngExtensionsPA.GetRandomFromString(seedText, out seedString);
            }

            CreateSaveData(displaySaveName, playerName, seedString);
        }

        /// <summary>
        /// Loads a save file into the <see cref="SaveData"/> object.
        /// </summary>
        /// <param name="saveName">The name of the save folder.</param>
        /// <param name="automaticBackup">If the save folder should be backed up without user choice.<br/>
        /// null to ask the user, and true/false to always/never back up.</param>
        /// <param name="savesFolderPath">The path to the saves folder. By default, the current saves folder.</param>
        /// <param name="configDiff">The config diff of the save (probably from <see cref="DisplaySaveData"/>).<br/>
        /// (only matters if <paramref name="automaticBackup"/> is null)</param>
        /// <exception cref="FileNotFoundException">Thrown, if the save file doesn't exist.</exception>
        /// <exception cref="FileLoadException">Thrown, if the save file doesn't have a save version.</exception>
        /// <returns>If the file was loaded without json load warnings.</returns>
        public static bool LoadSave(
            string saveName,
            bool? automaticBackup = null,
            string? savesFolderPath = null,
            ConfigDiff? configDiff = null
        )
        {
            var saveFolderPath = savesFolderPath is not null ? Path.Join(savesFolderPath, saveName) : Tools.GetSaveFolderPath(saveName);
            var dataFilePath = Path.Join(saveFolderPath, Constants.SAVE_FILE_NAME_DATA);

            if (!Directory.Exists(saveFolderPath))
            {
                PACSingletons.Instance.Logger.Log("Not a valid save folder", $"folder name: \"{saveName}\"", LogSeverity.ERROR);
                throw new FileNotFoundException("Not a valid save folder", saveName);
            }

            var data = Tools.LoadCompressedOrFMFile(dataFilePath, 1);

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

            if (fileVersion != Constants.SAVE_VERSION)
            {
                PACSingletons.Instance.Logger.Log("Trying to load save with an incorrect version", $"{fileVersion} -> {Constants.SAVE_VERSION}", LogSeverity.WARN);
            }

            if (!Utils.IsUpToDate(Constants.OLDEST_SAVE_VERSION, fileVersion))
            {
                PACSingletons.Instance.Logger.Log("Save version is too old", $"save version is older than the oldest recognised version number, {Constants.OLDEST_SAVE_VERSION} -> {fileVersion}", LogSeverity.ERROR);
                fileVersion = Constants.OLDEST_SAVE_VERSION;
                success = false;
            }

            if (configDiff is ConfigDiff cd && !cd.IsEmpty)
            {
                PACSingletons.Instance.Logger.Log(
                    "Trying to load save with an different config layout",
                    $"added: {cd.added.Length}, removed: {cd.removed.Length}, version changed: {cd.versionChanged.Length}, order changed: {cd.orderChanged.Length}",
                    LogSeverity.WARN
                );
            }

            BackupSaveIfAppropriate(fileVersion, saveName, automaticBackup, configDiff);

            // LOADING
            PACSingletons.Instance.Logger.Log("Preparing game data");
            data[Constants.JsonKeys.SaveData.SAVE_NAME] = new JsonValue(saveName);
            success &= PACTools.TryFromJson<SaveData>(data, fileVersion, out _);
            World.Initialize();
            PACSingletons.Instance.Logger.Log("Game data loaded", $"save name: \"{SaveData.Instance.SaveName}\", player name: \"{SaveData.Instance.PlayerRef.FullName}\", last saved: {Utils.MakeDate(SaveData.Instance.LastSave)} {Utils.MakeTime(SaveData.Instance.LastSave)}, playtime: {SaveData.Instance.Playtime}");
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
                var folderName = data.folderName;
                if (data.data is not DisplaySaveData displayData)
                {
                    PACSingletons.Instance.ConsoleProxy.PressKey($"\"{folderName}\" is corrupted!");
                    continue;
                }

                var formatedData = ProcessSaveDisplayData(folderName, displayData);
                if (formatedData is not null)
                {
                    datasProcessed.Add((folderName, formatedData));
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
        /// <param name="automaticBackup">If the save folder should be backed up without user choice.<br/>
        /// null to ask the user, and true/false to always/never back up.</param>
        /// <param name="configDiff">The config diff of the save (probably from <see cref="DisplaySaveData"/>).<br/>
        /// (only matters if <paramref name="automaticBackup"/> is null)</param>
        private static void BackupSaveIfAppropriate(
            string fileVersion,
            string saveName,
            bool? automaticBackup,
            ConfigDiff? configDiff = null
        )
        {
            static void ConfigDiffDisplayHelper(
                string categoryName,
                StringBuilder txt,
                IEnumerable<string> changeList,
                (byte, byte, byte) color
            )
            {
                var diffText = $"\t{categoryName}:\n\t\t-{string.Join("\n\t\t-", changeList)}";
                txt.Append(Tools.StylizedText(diffText, color));
                txt.Append('\n');
            }



            if (automaticBackup is bool mustBackup)
            {
                if (mustBackup)
                {
                    Tools.CreateBackup(saveName);
                }
                return;
            }

            var versionDiff = fileVersion != Constants.SAVE_VERSION;
            var isConfigDiff = configDiff is ConfigDiff cd && !cd.IsEmpty;
            if (!versionDiff && !isConfigDiff)
            {
                return;
            }

            if (versionDiff)
            {
                var isOlder = !Utils.IsUpToDate(Constants.SAVE_VERSION, fileVersion);
                var createBackup = MenuManager.AskYesNoUIQuestion(
                    $"\"{saveName}\" is {(isOlder ? "an older" : "a newer")} version than what it should be! Do you want to backup the save before loading it?",
                    keybinds: PASingletons.Instance.Settings.Keybinds
                );

                if (createBackup)
                {
                    Tools.CreateBackup(saveName);
                    return;
                }
            }

            if (isConfigDiff)
            {
                var diff = (ConfigDiff)configDiff!;

                var txt = new StringBuilder($"\"{saveName}\" has a different config layout than the last time it was saved:\n");
                if (diff.added.Length > 0)
                {
                    ConfigDiffDisplayHelper("added configs", txt, diff.added.Select(c => c.FolderName), Constants.Colors.RED);
                }
                if (diff.removed.Length > 0)
                {
                    ConfigDiffDisplayHelper("removed namespaces", txt, diff.removed.Select(c => c.Namespace), Constants.Colors.RED);
                }
                if (diff.versionChanged.Length > 0)
                {
                    ConfigDiffDisplayHelper(
                        "version changed",
                        txt,
                        diff.versionChanged.Select(c => $"{c.config.FolderName}: {c.oldVersion} -> {c.config.Version}"),
                        Constants.Colors.WARNING
                    );
                }
                if (diff.orderChanged.Length > 0)
                {
                    ConfigDiffDisplayHelper(
                        "order changed",
                        txt,
                        diff.orderChanged.Select(c => $"{c.config.FolderName}: {c.oldIndex + 1} -> {c.newIndex + 1}"),
                        Constants.Colors.WARNING
                    );
                }
                txt.Append("Do you want to backup the save before loading it?");
                
                var createBackup = MenuManager.AskYesNoUIQuestion(
                    txt.ToString(),
                    keybinds: PASingletons.Instance.Settings.Keybinds
                );

                if (createBackup)
                {
                    Tools.CreateBackup(saveName);
                    return;
                }
            }
        }

        /// <summary>
        /// Gets the display data from a save folder.
        /// </summary>
        /// <param name="saveFolder">The name of a save folder.</param>
        /// <param name="displaySaveData">The parsed display data</param>
        /// <returns>If the display data was returned without parsing warnings.</returns>
        public static bool GetDisplayDataFromSaveFolder(string saveFolder, [NotNullWhen(true)] out DisplaySaveData? displaySaveData)
        {
            displaySaveData = null;
            var filePath = Path.Join(Tools.GetSaveFolderPath(saveFolder), Constants.SAVE_FILE_NAME_DATA);
            var dataJson = Tools.LoadFileExpected<DisplaySaveData>(filePath, out var isFileInvalid, 0);

            if (dataJson is null)
            {
                PACSingletons.Instance.Logger.Log("Decode error", $"save name: {saveFolder}", LogSeverity.ERROR);
                return false;
            }

            var fileVersion = GetSaveVersion<DisplaySaveData>(
                dataJson,
                Constants.JsonKeys.SaveData.OLD_SAVE_VERSION,
                Constants.JsonKeys.SaveData.SAVE_VERSION,
                saveFolder
            );
            if (fileVersion is null)
            {
                PACSingletons.Instance.Logger.Log($"Unknown {typeof(SaveData).Name} version", $"{typeof(SaveData).Name} name: {saveFolder}", LogSeverity.ERROR);
                fileVersion = Constants.OLDEST_SAVE_VERSION;
            }

            var success = PACTools.TryFromJson(dataJson, fileVersion, out displaySaveData);
            if (displaySaveData is null)
            {
                PACTools.LogJsonParseError<DisplaySaveData>(nameof(displaySaveData), $"somehow the display save data is null after being converted from json.", true);
                return false;
            }
            return success;
        }

        /// <summary>
        /// Turns the display data into a formated string.
        /// </summary>
        /// <param name="folderName">The save folder's name</param>
        /// <param name="displaySaveData">The display save data.</param>
        public static string? ProcessSaveDisplayData(string folderName, DisplaySaveData displaySaveData)
        {
            try
            {
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

                var diffs = ConfigUtils.GetConfigDiff(displaySaveData.lastLoadedConfigs);

                var configDiffMessages = new List<string>();
                if (diffs.added.Length > 0)
                {
                    configDiffMessages.Add(Tools.StylizedText($"added({diffs.added.Length})", Constants.Colors.RED));
                }
                if (diffs.removed.Length > 0)
                {
                    configDiffMessages.Add(Tools.StylizedText($"removed({diffs.removed.Length})", Constants.Colors.RED));
                }
                if (diffs.versionChanged.Length > 0)
                {
                    configDiffMessages.Add(Tools.StylizedText($"version changed({diffs.versionChanged.Length})", Constants.Colors.WARNING));
                }
                if (diffs.orderChanged.Length > 0)
                {
                    configDiffMessages.Add(Tools.StylizedText($"order changed({diffs.orderChanged.Length})", Constants.Colors.WARNING));
                }

                if (configDiffMessages.Count > 0)
                {
                    displayText.Append("\nconfigs: " + string.Join(", ", configDiffMessages));
                }

                return displayText.ToString();
            }
            catch (Exception ex)
            {
                if (ex is InvalidCastException || ex is ArgumentException || ex is KeyNotFoundException)
                {
                    PACSingletons.Instance.Logger.Log("Save display data parse error", $"Save name: {folderName}, exception: " + ex.ToString(), LogSeverity.ERROR);
                    PACSingletons.Instance.ConsoleProxy.PressKey($"\"{folderName}\" could not be parsed!");
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
            var oldDataFileName = $"{Constants.SAVE_FILE_NAME_DATA}.{Constants.OLD_SAVE_EXT}";
            var dataFileName = $"{Constants.SAVE_FILE_NAME_DATA}.{Constants.SAVE_EXT}";
            foreach (var folderPath in folderPaths)
            {
                if (
                    File.Exists(Path.Join(folderPath, dataFileName)) ||
                    File.Exists(Path.Join(folderPath, oldDataFileName))
                )
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
        /// <returns>A list of tuples, containing the folder name, and the display data in it. The data will be null, if the display data wasn't parsable.</returns>
        private static List<(string folderName, DisplaySaveData? data)> GetFoldersDisplayData(IEnumerable<string> folders)
        {
            var datas = new List<(string folderName, DisplaySaveData? data)>();
            foreach (var folder in folders)
            {
                var success = GetDisplayDataFromSaveFolder(folder, out var displaySaveData);
                datas.Add((folder, displaySaveData));
            }
            return datas;
        }
        #endregion
    }
}

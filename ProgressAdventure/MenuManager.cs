using ProgressAdventure.Enums;
using ProgressAdventure.SettingsManagement;
using ProgressAdventure.WorldManagement;
using SaveFileManager;
using SFMUtils = SaveFileManager.Utils;

namespace ProgressAdventure
{
    /// <summary>
    /// Class for managing menus.
    /// </summary>
    public static class MenuManager
    {
        #region Private Lists
        /// <summary>
        /// A list of logging severities to choose from in the menu.
        /// </summary>
        private static readonly List<(int value, string name)> loggingSeveritiesList = new()
        {
            (Logger.loggingValuesMap[LogSeverity.MINIMAL], LogSeverity.MINIMAL.ToString()),
            (Logger.loggingValuesMap[LogSeverity.FATAL], LogSeverity.FATAL.ToString()),
            (Logger.loggingValuesMap[LogSeverity.ERROR], LogSeverity.ERROR.ToString()),
            (Logger.loggingValuesMap[LogSeverity.WARN], LogSeverity.WARN.ToString()),
            (Logger.loggingValuesMap[LogSeverity.INFO], LogSeverity.INFO.ToString()),
            (Logger.loggingValuesMap[LogSeverity.DEBUG], "ALL"),
        };

        /// <summary>
        /// A list of default backup actions to choose from in the menu.
        /// </summary>
        private static readonly List<(int value, string name)> defBackupActionsList = new()
        {
            (-1, "ask"),
            (0, "don't backup"),
            (1, "backup"),
        };
        #endregion

        #region Delegate functions
        /// <summary>
        /// A function for a <c>UIAction</c> object, that exits the <c>OptionsUI</c> function.
        /// </summary>
        private static object UIExitFunction()
        {
            return -1;
        }
        #endregion

        #region Public functions
        /// <summary>
        /// Displays a simple yes or no prompt, and returns the user's answer.
        /// </summary>
        /// <param name="question">The question to print.</param>
        /// <param name="yesFirst">If yes or no shoul be the first answer in the answers list.</param>
        /// <param name="canEscape">If the player can press escape, to exit the prompt. Exiting will be treated as a no answer.</param>
        /// <param name="keybinds">The keybinds to use.</param>
        public static bool AskYesNoUIQuestion(string question, bool yesFirst = true, bool canEscape = true, Keybinds? keybinds = null)
        {
            var answersList = yesFirst ? new List<string?> { "Yes", "No" } : new List<string?> { "No", "Yes" };
            var keybindList = keybinds is null ? Settings.Keybinds.KeybindList : keybinds.KeybindList;
            return (int)new UIList(answersList, question, canEscape: canEscape).Display(keybindList) == (yesFirst ? 0 : 1);
        }

        /// <summary>
        /// Displays the other options menu.
        /// </summary>
        public static void OtherOptions()
        {
            // auto save
            var autoSaveElement = new Toggle(Settings.AutoSave ? 1 : 0, "Auto save: ");

            // logging
            var loggingValues = loggingSeveritiesList.Select(el => el.value);
            var loggingNames = loggingSeveritiesList.Select(el => el.name);
            var currentLoggingValue = Logger.loggingValuesMap[Settings.LoggingLevel];

            var loggingValue = loggingValues.Count() - 1;
            foreach (var value in loggingValues)
            {
                if (value == currentLoggingValue)
                {
                    loggingValue = value;
                    break;
                }
            }
            var loggingElement = new PAChoice(loggingNames, loggingValue, "Logging: ");

            // menu elements
            var menuElements = new List<BaseUI?> { autoSaveElement, loggingElement, null, GenerateSimpleButton() };
            
            // response
            var response = SFMUtils.OptionsUI(menuElements, " Other options", keybinds: Settings.Keybinds.KeybindList);
            if (response is not null)
            {
                var newAutoSaveValue = autoSaveElement.value == 1;
                _ = Tools.TryParseLogSeverityFromValue(loggingValues.ElementAt(loggingElement.value), out LogSeverity newLoggingLevel);

                Settings.AutoSave = newAutoSaveValue;
                Settings.LoggingLevel = newLoggingLevel;
            }
        }

        /// <summary>
        /// Displays the ask options menu.
        /// </summary>
        public static void AskOptions()
        {
            var askDeleteSaveElement = new Toggle(Settings.AskDeleteSave ? 1 : 0, "Confirm save folder delete: ", "yes", "no");
            var askRegenerateSaveElement = new Toggle(Settings.AskRegenerateSave ? 1 : 0, "Confirm save folders regeneration: ", "yes", "no");

            // default backup action
            var backupActionValues = defBackupActionsList.Select(ac => ac.value);
            var backupActionNames = defBackupActionsList.Select(ac => ac.name);
            var backupActionValue = backupActionValues.ElementAt(0);
            foreach (var action in backupActionValues)
            {
                if (action == Settings.DefBackupAction)
                {
                    backupActionValue = action;
                    break;
                }
            }

            var defBackupActionElement = new PAChoice(backupActionNames, backupActionValue, "On save folder backup prompt: ");
            
            // menu elements
            var askSettingsElements = new List<BaseUI?> { askDeleteSaveElement, askRegenerateSaveElement, defBackupActionElement, null, GenerateSimpleButton() };

            // response
            var response = SFMUtils.OptionsUI(askSettingsElements, " Question popups", keybinds: Settings.Keybinds.KeybindList);
            if (response is not null)
            {
                Settings.AskDeleteSave = askDeleteSaveElement.value == 1;
                Settings.AskRegenerateSave = askRegenerateSaveElement.value == 1;
                Settings.DefBackupAction = backupActionValues.ElementAt(defBackupActionElement.value);
            }
        }

        /// <summary>
        /// Asks the used for a key, and sets it as a key in the given keybind.
        /// </summary>
        /// <param name="keybind">The keybind to modify.</param>
        public static void SetKeybind(ActionKey keybind)
        {
            Console.Write("\n\nPress any key\n\n");
            var key = Console.ReadKey();
            keybind.Keys = new List<ConsoleKeyInfo> { key };
        }

        /// <summary>
        /// Returns the name of the current key for a given action, and colors it, depending on if it conflicts with other keys.
        /// </summary>
        /// <param name="keybinds">The keybinds to get the key from.</param>
        /// <param name="actionType">The action type to return the key for.</param>
        public static string GetKeybindName(Keybinds keybinds, ActionType actionType)
        {
            var key = keybinds.GetActionKey(actionType);
            return Utils.StylizedText(key.Name, (key.conflict ? Constants.Colors.RED : null));
        }

        /// <summary>
        /// Displays the keybinds menu.
        /// </summary>
        public static void KeybindSettings()
        {
            var tempKeybinds = Settings.Keybinds.DeepCopy();
            while (true)
            {
                var response = (int)new UIList(new List<string?>
                    {
                    $"Escape: {GetKeybindName(tempKeybinds, ActionType.ESCAPE)}",
                    $"Up: {GetKeybindName(tempKeybinds, ActionType.UP)}",
                    $"Down: {GetKeybindName(tempKeybinds, ActionType.DOWN)}",
                    $"Left: {GetKeybindName(tempKeybinds, ActionType.LEFT)}",
                    $"Right: {GetKeybindName(tempKeybinds, ActionType.RIGHT)}",
                    $"Enter: {GetKeybindName(tempKeybinds, ActionType.ENTER)}",
                    null, "Save"
                    },
                    " Keybinds", null, false, true
                ).Display(Settings.Keybinds.KeybindList);
                // exit
                if (response == -1)
                {
                    break;
                }
                // done
                else if (response > 5)
                {
                    Logger.Log("Keybinds changed", $"{Settings.Keybinds} -> {tempKeybinds}", LogSeverity.DEBUG);
                    Settings.Keybinds = tempKeybinds;
                    break;
                }
                else
                {
                    SetKeybind(tempKeybinds.KeybindList.ElementAt(response));
                    tempKeybinds.UpdateKeybindConflicts();
                }
            }
        }

        /// <summary>
        /// The main game loop of the game.
        /// </summary>
        public static void GameLoop()
        {
            Console.WriteLine("\n\nGAME LOOP NOT DONE YET!!!\n\n");
            Logger.Log("GAME LOOP PLACEHOLDER", "yes", LogSeverity.FAIL);
        }

        /// <summary>
        /// Creates a new save.
        /// </summary>
        public static void NewSave()
        {
            SaveManager.CreateSaveData();
            SaveManager.MakeSave();
            Logger.Log("Created save", $"save name: {SaveData.saveName}, player name: \"{SaveData.player.FullName}\"");
            GameLoop();
        }

        /// <summary>
        /// Loads an existing save.
        /// </summary>
        public static void LoadSave(string saveName)
        {
            var backupChoice = Settings.DefBackupAction == -1;
            var automaticBackup = Settings.DefBackupAction == 1;
            SaveManager.LoadSave(saveName, backupChoice, automaticBackup);
            GameLoop();
        }

        /// <summary>
        /// Regenerates the save file.
        /// </summary>
        /// <param name="saveName">The name of the save folder.</param>
        /// <param name="makeBackup">Whether to make a backup before regenerating.</param>
        public static void RegenerateSaveFile(string saveName, bool makeBackup = true)
        {
            Console.WriteLine($"Regenerating \"{saveName}\":");
            Logger.Log("Regenerating save file", $"save name: {saveName}");
            Console.Write("\tLoading...");
            SaveManager.LoadSave(saveName, false, makeBackup);
            Console.WriteLine("DONE!");
            Logger.Log("Loading all chunks from file", $"save name: {saveName}");
            World.LoadAllChunksFromFolder(showProgressText: "\tLoading world...");
            Console.Write("\tDeleting...");
            Tools.DeleteSave(saveName);
            Console.WriteLine("DONE!");
            Console.Write("\tSaving...\r");
            SaveManager.MakeSave(showProgressText: "\tSaving...");
            Logger.Log("Save file regenerated", $"save name: {saveName}");
        }

        /// <summary>
        /// Displays the main menu.
        /// </summary>
        public static void MainMenu()
        {
            Console.WriteLine("REFACTOR MAIN MENU... STILL!");
            Logger.Log("REFACTOR MAIN MENU... STILL!", "do it", LogSeverity.FAIL);
            var savesData = SaveManager.GetSavesData();
            var inMainMenu = true;
            while (true)
            {
                (int menuStatus, string? SelectedSaveName) status = (-1, null);
                int selectedElement;
                List<string> mainMenuElementList;

                if (inMainMenu)
                {
                    inMainMenu = false;
                    if (savesData.Any())
                    {
                        mainMenuElementList = new List<string> { "New save", "Load/Delete save", "Options" };
                    }
                    else
                    {
                        mainMenuElementList = new List<string> { "New save", "Options" };
                    }
                    selectedElement = (int)new UIList(mainMenuElementList, " Main menu", canEscape: true).Display(Settings.Keybinds.KeybindList);
                }
                else if (savesData.Any())
                {
                    selectedElement = 1;
                }
                else
                {
                    selectedElement = -2;
                    inMainMenu = true;
                }

                // new file
                if (selectedElement == 0)
                {
                    status = (1, "");
                }
                else if (selectedElement == -1)
                {
                    break;
                }
                // load/delete
                else if (selectedElement == 1 && savesData.Any())
                {
                    // get data from file_data
                    var savesMenuElements = new List<string?>();
                    foreach (var (saveName, displayText) in savesData)
                    {
                        savesMenuElements.Add(displayText);
                        savesMenuElements.Add(null);
                    }
                    savesMenuElements.Add("Regenerate all save files");
                    savesMenuElements.Add("Delete file");
                    savesMenuElements.Add("Back");
                    var filesMenuSelected = (int)new UIList(savesMenuElements, " Level select", null, true, true, excludeNulls: true).Display(Settings.Keybinds.KeybindList);
                    // load
                    if (filesMenuSelected != -1 && filesMenuSelected < savesData.Count)
                    {
                        status = (0, savesData[filesMenuSelected].saveName);
                    }
                    // regenerate
                    else if (filesMenuSelected == savesData.Count)
                    {
                        if (!Settings.AskRegenerateSave || AskYesNoUIQuestion(" Are you sure you want to regenerate ALL save files? This will load, delete then resave EVERY save file!", false))
                        {
                            bool backupSaves;
                            if (Settings.DefBackupAction == -1)
                            {
                                backupSaves = AskYesNoUIQuestion(" Do you want to backup your save files before regenerating them?");
                            }
                            else
                            {
                                backupSaves = Settings.DefBackupAction == 1;
                            }
                            Console.WriteLine("Regenerating save files...\n");
                            foreach (var (saveName, displayText) in savesData)
                            {
                                RegenerateSaveFile(saveName, backupSaves);
                            }
                            savesData = SaveManager.GetSavesData();
                            Console.WriteLine("\nDONE!");
                        }
                    }
                    // delete
                    else if (filesMenuSelected == savesData.Count + 1)
                    {
                        // remove "delete file" + "regenerate save files"
                        savesMenuElements.RemoveAt(savesMenuElements.Count - 2);
                        savesMenuElements.RemoveAt(savesMenuElements.Count - 2);
                        while (savesData.Any())
                        {
                            filesMenuSelected = (int)new UIList(savesMenuElements, " Delete mode!", Constants.DELETE_CURSOR_ICONS, true, true, excludeNulls: true).Display(Settings.Keybinds.KeybindList);
                            if (filesMenuSelected != -1 && filesMenuSelected < (savesMenuElements.Count - 1) / 2)
                            {
                                if (!Settings.AskDeleteSave || AskYesNoUIQuestion($" Are you sure you want to remove Save file {savesData[filesMenuSelected].saveName}?", false))
                                {
                                    Tools.DeleteSave(savesData[filesMenuSelected].saveName);
                                    savesMenuElements.RemoveAt(filesMenuSelected * 2);
                                    savesMenuElements.RemoveAt(filesMenuSelected * 2);
                                    savesData.RemoveAt(filesMenuSelected);
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                        if (savesData.Count == 0)
                        {
                            inMainMenu = true;
                        }
                    }
                    // back
                    else
                    {
                        inMainMenu = true;
                    }
                }
                else if ((selectedElement == 2 && savesData.Any()) || (selectedElement == 1 && !savesData.Any()))
                {
                    var optionsMenuAnswers = new List<string?>
                    {
                        "Keybinds",
                        "Question popups",
                        "Other",
                        null,
                        "Back"
                    };
                    var optionsMenuActions = new List<UIAction>
                    {
                        new UIAction(KeybindSettings),
                        new UIAction(AskOptions),
                        new UIAction(OtherOptions),
                    };
                    new UIList(optionsMenuAnswers, " Options", null, false, true, optionsMenuActions, true).Display(Settings.Keybinds.KeybindList);
                    inMainMenu = true;
                }

                // action
                // new save
                if (status.menuStatus == 1)
                {
                    Utils.PressKey("\nCreating new save!\n");
                    NewSave();
                    savesData = SaveManager.GetSavesData();
                }
                // load
                else if (status.menuStatus == 0 && status.SelectedSaveName is not null)
                {
                    Utils.PressKey($"\nLoading save: {status.SelectedSaveName}!");
                    LoadSave(status.SelectedSaveName);
                    savesData = SaveManager.GetSavesData();
                }
            }
        }
        #endregion

        #region Private function
        /// <summary>
        /// Returns a button for the <c>OptionsUI</c>, that exits the function.
        /// </summary>
        private static Button GenerateSimpleButton(string text = "Save")
        {
            return new Button(new UIAction(UIExitFunction), text: text);
        }
        #endregion
    }
}

using ProgressAdventure.Enums;
using ProgressAdventure.Extensions;
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
        #region Private fields
        /// <summary>
        /// The action list for <c>OptionsUI</c> and <c>UIList</c>.
        /// </summary>
        private static List<ActionKey> _actionList;

        /// <summary>
        /// The key results.
        /// </summary>
        private static List<object> _resultsList;

        /// <summary>
        /// The current list of saves data.<br/>
        /// SHOULD NOT BE MODIFIED MANUALY!
        /// </summary>
        private static List<(string saveName, string displayText)> _savesData;
        #endregion

        #region Private Properties
        /// <summary>
        /// <inheritdoc cref="_actionList"/>
        /// </summary>
        private static List<ActionKey> ActionList
        {
            get
            {
                if (_actionList is null)
                {
                    _actionList = Settings.Keybinds.KeybindList.ToList();
                    _resultsList = SFMUtils.GetResultsList(_actionList).ToList();
                }
                return _actionList;
            }
            set
            {
                _actionList = value;
                _resultsList = SFMUtils.GetResultsList(_actionList).ToList();
            }
        }

        /// <summary>
        /// The key results.
        /// </summary>
        private static List<object> ResultsList
        {
            get
            {
                _resultsList ??= SFMUtils.GetResultsList(ActionList).ToList();
                return _resultsList;
            }
        }

        /// <summary>
        /// The current list of saves data.
        /// </summary>
        private static List<(string saveName, string displayText)> SavesData
        {
            get
            {
                return _savesData ??= SaveManager.GetSavesData();
            }
        }
        #endregion

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
        #region Keybinds
        /// <summary>
        /// Asks the used for a key, and sets it as a key in the given keybind.
        /// </summary>
        /// <param name="keybind">The keybind to modify.</param>
        public static void SetKeybind(ActionKey keybind)
        {
            Console.Write("\n\nPress any key!\n\n");
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
            return Tools.StylizedText(key.Name, (key.conflict ? Constants.Colors.RED : null));
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
                ).Display(ActionList, ResultsList);
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

                    // in place keybinds switch
                    ActionList.Clear();
                    ActionList.AddRange(Settings.Keybinds.KeybindList);
                    ResultsList.Clear();
                    ResultsList.AddRange(SFMUtils.GetResultsList(ActionList));

                    break;
                }
                else
                {
                    SetKeybind(tempKeybinds.KeybindList.ElementAt(response));
                    tempKeybinds.UpdateKeybindConflicts();
                }
            }
        }
        #endregion

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
            IEnumerable<ActionKey> keybindList;
            IEnumerable<object>? resultsList = null;
            if (keybinds is null)
            {
                keybindList = ActionList;
                resultsList = ResultsList;
            }
            else
            {
                keybindList = keybinds.KeybindList;
            }
            return (int)new UIList(answersList, question, Constants.STANDARD_CURSOR_ICONS, canEscape: canEscape).Display(keybindList, resultsList) == (yesFirst ? 0 : 1);
        }

        /// <summary>
        /// Displays the other options menu.
        /// </summary>
        public static void OtherOptions()
        {
            // auto save
            var autoSaveElement = new Toggle(Settings.AutoSave, "Auto save: ");

            // logging
            var loggingValues = loggingSeveritiesList.Select(el => el.value);
            var loggingNames = loggingSeveritiesList.Select(el => el.name);
            var currentLoggingValue = Logger.loggingValuesMap[Settings.LoggingLevel];

            var loggingValue = loggingValues.Count() - 1;
            for (var x = 0; x < loggingValues.Count(); x++)
            {
                if (loggingValues.ElementAt(x) == currentLoggingValue)
                {
                    loggingValue = x;
                    break;
                }
            }
            var loggingElement = new PAChoice(loggingNames, loggingValue, "Logging: ");

            // enable colored text
            var coloredTextElement = new Toggle(Settings.EnableColoredText, "Colored text: ", "enabled", "disabled");

            // menu elements
            var menuElements = new List<BaseUI?> { autoSaveElement, loggingElement, coloredTextElement, null, GenerateSimpleButton() };
            
            // response
            var response = SFMUtils.OptionsUI(menuElements, " Other options", keybinds: Settings.Keybinds.KeybindList);
            if (response is not null)
            {
                var newAutoSaveValue = autoSaveElement.value == 1;
                _ = Tools.TryParseLogSeverityFromValue(loggingValues.ElementAt(loggingElement.value), out LogSeverity newLoggingLevel);
                var newColoredTextValue = coloredTextElement.value == 1;

                Settings.AutoSave = newAutoSaveValue;
                Settings.LoggingLevel = newLoggingLevel;
                Settings.EnableColoredText = newColoredTextValue;
            }
        }

        /// <summary>
        /// Displays the ask options menu.
        /// </summary>
        public static void AskOptions()
        {
            var askDeleteSaveElement = new Toggle(Settings.AskDeleteSave, "Confirm save folder delete: ", "yes", "no");
            var askRegenerateSaveElement = new Toggle(Settings.AskRegenerateSave, "Confirm save folders regeneration: ", "yes", "no");

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
            ActionList = Settings.Keybinds.KeybindList.ToList();

            var (answers, actions) = GetMainMenuLists();

            new UIList(
                answers,
                " Main menu",
                canEscape: true,
                actions: actions,
                modifiableUIList: true
            ).Display(ActionList, ResultsList);
        }
        #endregion

        #region Menu actions
        /// <summary>
        /// Action, called when thw new save button is pressed.
        /// </summary>
        /// <param name="mainMenuUI">The main menu <c>UIList</c>.</param>
        private static void NewSaveAction(UIList mainMenuUI)
        {
            Utils.PressKey("\nCreating new save!\n");
            GameManager.NewSave();

            var (answers, actions) = GetMainMenuLists();
            mainMenuUI.answers = answers;
            mainMenuUI.actions = actions;
        }

        /// <summary>
        /// Action, called when thw load saves button is pressed.
        /// </summary>
        /// <param name="mainMenuUI">The main menu <c>UIList</c>.</param>
        private static void LoadSavesAction(UIList mainMenuUI)
        {
            GetSavesMenu().Display(ActionList, ResultsList);

            var (answers, actions) = GetMainMenuLists();
            mainMenuUI.answers = answers;
            mainMenuUI.actions = actions;
        }

        /// <summary>
        /// Action, called when a load save button is pressed.
        /// </summary>
        /// <param name="loadSaveUI">The load save menu <c>UIList</c>.</param>
        /// <param name="selectedSaveName">The name of the save to load.</param>
        private static object? LoadSaveAction(UIList loadSaveUI, string selectedSaveName)
        {
            Utils.PressKey($"\nLoading save: {selectedSaveName}!");
            GameManager.LoadSave(selectedSaveName);

            var (answers, actions) = GetSavesMenuLists();
            loadSaveUI.answers = answers;
            loadSaveUI.actions = actions;

            return SavesData.Any() ? null : -1;
        }

        /// <summary>
        /// Action, called when the delete saves button is pressed.
        /// </summary>
        /// <param name="loadSaveUI">The load save menu <c>UIList</c>.</param>
        private static object? DeleteSavesAction(UIList loadSaveUI)
        {
            GetDeleteSavesMenu().Display(ActionList, ResultsList);

            var (answers, actions) = GetSavesMenuLists();
            loadSaveUI.answers = answers;
            loadSaveUI.actions = actions;

            return SavesData.Any() ? null : -1;
        }

        /// <summary>
        /// Action, called when a delete save button is pressed.
        /// </summary>
        /// <param name="deleteSavesUI">The delete saves menu <c>UIList</c>.</param>
        /// <param name="selectedSaveName">The name of the save to delete.</param>
        private static object? DeleteSaveAction(UIList deleteSavesUI, string selectedSaveName)
        {
            if (!Settings.AskDeleteSave || AskYesNoUIQuestion($" Are you sure you want to remove Save file {selectedSaveName}?", false))
            {
                Tools.DeleteSave(selectedSaveName);
            }

            var (answers, actions) = GetDeleteSavesMenuLists();
            deleteSavesUI.answers = answers;
            deleteSavesUI.actions = actions;

            return SavesData.Any() ? null : -1;
        }

        /// <summary>
        /// Action, called when the regenerate saves button is pressed.
        /// </summary>
        /// <param name="loadSaveUI">The load saves menu <c>UIList</c>.</param>
        private static void RegenerateSavesAction(UIList loadSaveUI)
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
                foreach (var (saveName, _) in SavesData)
                {
                    RegenerateSaveFile(saveName, backupSaves);
                }
                UpdateSavesData();
                Console.WriteLine("\nDONE!");

                var (answers, actions) = GetSavesMenuLists();
                loadSaveUI.answers = answers;
                loadSaveUI.actions = actions;
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

        /// <summary>
        /// Updates the saves data, used for answers and actions in <c>UIList</c>s.
        /// </summary>
        private static void UpdateSavesData()
        {
            _savesData = SaveManager.GetSavesData();
        }

        /// <summary>
        /// Returns the answers and actions lists, used in the delete saves menu.
        /// </summary>
        private static (List<string?> answers, List<UIAction?> actions) GetDeleteSavesMenuLists()
        {
            UpdateSavesData();

            var answers = new List<string?>();
            var actions = new List<UIAction?>();

            foreach (var (saveName, displayText) in SavesData)
            {
                answers.Add(displayText);
                answers.Add(null);

                actions.Add(new UIAction(DeleteSaveAction, new List<object?> { saveName }));
            }

            answers.Add("Back");

            return (answers, actions);
        }

        /// <summary>
        /// Returns the delete saves menu.
        /// </summary>
        private static UIList GetDeleteSavesMenu()
        {
            var (answers, actions) = GetDeleteSavesMenuLists();
            return new UIList(
                answers,
                " Delete mode!",
                Constants.DELETE_CURSOR_ICONS,
                true,
                true,
                actions,
                true,
                true
            );
        }

        /// <summary>
        /// Returns the answers and actions lists, used in the load saves menu.
        /// </summary>
        private static (List<string?> answers, List<UIAction?> actions) GetSavesMenuLists()
        {
            UpdateSavesData();
            
            var answers = new List<string?>();
            var actions = new List<UIAction?>();

            foreach (var (saveName, displayText) in SavesData)
            {
                answers.Add(displayText);
                answers.Add(null);

                actions.Add(new UIAction(LoadSaveAction, new List<object?> { saveName }));
            }

            answers.Add("Regenerate all save files");
            answers.Add("Delete file");
            answers.Add("Back");

            actions.Add(new UIAction(RegenerateSavesAction));
            actions.Add(new UIAction(DeleteSavesAction));

            return (answers, actions);
        }

        /// <summary>
        /// Returns the load saves menu.
        /// </summary>
        private static UIList GetSavesMenu()
        {
            var (answers, actions) = GetSavesMenuLists();
            return new UIList(
                answers,
                " Level select",
                Constants.STANDARD_CURSOR_ICONS,
                true,
                true,
                actions,
                true,
                true
            );
        }

        /// <summary>
        /// Returns the options menu.
        /// </summary>
        private static UIList GetOptionsMenu()
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

            return new UIList(
                optionsMenuAnswers,
                " Options",
                Constants.STANDARD_CURSOR_ICONS,
                canEscape: true,
                actions: optionsMenuActions
            );
        }

        /// <summary>
        /// Returns the answers and actions lists, used in the main menu.
        /// </summary>
        private static (List<string?> answers, List<UIAction?> actions) GetMainMenuLists()
        {
            UpdateSavesData();

            // actions
            var newSaveAction = new UIAction(NewSaveAction);
            var optionsAction = new UIAction(GetOptionsMenu());

            // lists
            List<string?> answers;
            List<UIAction?> actions;

            if (SavesData.Any())
            {
                var loadSaveAction = new UIAction(LoadSavesAction);

                answers = new List<string?> { "New save", "Load/Delete save", "Options" };
                actions = new List<UIAction?>
                {
                    newSaveAction,
                    loadSaveAction,
                    optionsAction,
                };
            }
            else
            {
                answers = new List<string?> { "New save", "Options" };
                actions = new List<UIAction?>
                {
                    newSaveAction,
                    optionsAction,
                };
            }

            return (answers, actions);
        }
        #endregion
    }
}

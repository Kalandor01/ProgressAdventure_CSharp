using ConsoleUI;
using ConsoleUI.UIElements;
using PACommon;
using PACommon.Enums;
using PACommon.Extensions;
using PACommon.SettingsManagement;
using ProgressAdventure.ConfigManagement;
using ProgressAdventure.Enums;
using ProgressAdventure.Exceptions;
using ProgressAdventure.ItemManagement;
using ProgressAdventure.SettingsManagement;
using ProgressAdventure.WorldManagement;
using Utils = PACommon.Utils;

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
        /// The current list of saves data.<br/>
        /// SHOULD NOT BE MODIFIED MANUALY!
        /// </summary>
        private static List<(string saveName, string displayText)> _savesData;

        /// <summary>
        /// The temp keybinds, used to update the conflicts when a keybind changes.
        /// </summary>
        private static Keybinds tempKeybinds;
        #endregion

        #region Private Properties
        /// <summary>
        /// <inheritdoc cref="_actionList"/>
        /// </summary>
        private static List<ActionKey> ActionList
        {
            get
            {
                _actionList ??= PASingletons.Instance.Settings.Keybinds.KeybindList.ToList();
                return _actionList;
            }
            set
            {
                _actionList = value;
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
        private static readonly List<(LogSeverity value, string name)> loggingSeveritiesList =
        [
            (LogSeverity.DISABLED, "MINIMAL"),
            (LogSeverity.FATAL, LogSeverity.FATAL.ToString()),
            (LogSeverity.ERROR, LogSeverity.ERROR.ToString()),
            (LogSeverity.WARN, LogSeverity.WARN.ToString()),
            (LogSeverity.INFO, LogSeverity.INFO.ToString()),
            (LogSeverity.DEBUG, "ALL"),
        ];

        /// <summary>
        /// A list of default backup actions to choose from in the menu.
        /// </summary>
        private static readonly List<(int value, string name)> defBackupActionsList =
        [
            (-1, "ask"),
            (0, "don't backup"),
            (1, "backup"),
        ];
        #endregion

        #region Delegate functions
        /// <summary>
        /// A function for a <c>UIAction</c> object, that exits the <c>OptionsUI</c> function.
        /// </summary>
        private static object UIExitFunction()
        {
            return -1;
        }

        /// <inheritdoc cref="KeyField.DisplayValueDelegate"/>
        private static string KeybindValueDisplay(KeyField<EnumValue<ActionType>> keyField, string icons, OptionsUI? optionsUI = null)
        {
            return string.Join(", ", KeybindUtils.GetColoredNames(keyField.Value));
        }
        #endregion

        #region Public functions
        #region Common
        /// <summary>
        /// Returns a back button for OptionsUI-s.
        /// </summary>
        /// <param name="text">The text to display in the button.</param>
        public static PAButton GetBackButton(string text = "Back")
        {
            return new PAButton(new UIAction(() => -1), text: text);
        }

        /// <summary>
        /// Displays a simple yes or no prompt, and returns the user's answer.
        /// </summary>
        /// <param name="question">The question to print.</param>
        /// <param name="yesFirst">If yes or no shoul be the first answer in the answers list.</param>
        /// <param name="canEscape">If the player can press escape, to exit the prompt. Exiting will be treated as a no answer.</param>
        /// <param name="keybinds">The keybinds to use.</param>
        public static bool AskYesNoUIQuestion(string question, bool yesFirst = true, bool canEscape = true, Keybinds? keybinds = null)
        {
            List<string?> answersList = yesFirst ? ["Yes", "No"] : ["No", "Yes"];
            IEnumerable<ActionKey> keybindList;
            if (keybinds is null)
            {
                keybindList = ActionList;
            }
            else
            {
                keybindList = keybinds.KeybindList;
            }
            return (int)new UIList(answersList, question, Constants.STANDARD_CURSOR_ICONS, canEscape: canEscape).Display(keybindList) == (yesFirst ? 0 : 1);
        }

        /// <summary>
        /// Hadles exceptions in a main context and displays a menu for reloading.
        /// </summary>
        /// <param name="exception">The exception thwt was thrown.</param>
        /// <param name="isPreloading">If the current context is preloading.</param>
        /// <returns>Whether to rethrow.</returns>
        public static bool HandleErrorMenu(Exception exception, bool isPreloading)
        {
            if (
                exception is RestartException ||
                exception.InnerException is RestartException
            )
            {
                return true;
            }

            var contextName = isPreloading ? "preloading" : "instance";
            PACSingletons.Instance.Logger.Log(
                $"{contextName.Capitalize()} crashed",
                exception.ToString(),
                LogSeverity.FATAL,
                forceLog: true
            );

            if (!Constants.ERROR_HANDLING)
            {
                return true;
            }

            try
            {
                var restartAnwers = new List<string?>
                {
                    "Restart",
                    "Restart in safe mode (only vanilla config enabled)",
                    "Exit"
                };
                var response = (int)new UIList(restartAnwers, "ERROR: " + exception.Message).Display();
                
                if (response == 1)
                {
                    ConfigUtils.SetLoadingOrderData([new ConfigLoadingData(Constants.VANILLA_CONFIGS_NAMESPACE, true)]);
                    throw new RestartException($"Restarting {contextName} in safe mode");
                }
                else if (response == 0)
                {
                    PACSingletons.Instance.Logger.Log($"Restarting {contextName}", forceLog: true);
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception inException)
            {
                if (
                    inException is RestartException ||
                    inException.InnerException is RestartException
                )
                {
                    throw;
                }

                Console.WriteLine("ERROR: " + inException.Message);
                Console.WriteLine("WHILE TRYING TO DISPLAY THE ORIGINAL ERROR: " + exception.Message);
                var ans = Utils.Input("Restart?(Y/N): ");
                var restart = ans is not null && ans.ToUpper() == "Y";
                if (restart)
                {
                    PACSingletons.Instance.Logger.Log($"Restarting {contextName}", forceLog: true);
                }
                return !restart;
            }
        }
        #endregion

        #region Inventory viewer
        /// <summary>
        /// Opens an item viewing menu.
        /// </summary>
        /// <param name="item">The item to view.</param>
        public static void ItemViever(AItem item)
        {
            var title = $"{item}\n\n" +
                $"Mass: {item.Mass} ({item.MassMultiplier}) kg\n" +
                $"Volume: {item.Volume} ({item.VolumeMultiplier}) m^3\n" +
                $"Density: {item.Density} kg/m^3";
            var backButton = GetBackButton();

            if (item is not CompoundItem compundItem)
            {
                new OptionsUI([backButton], title, Constants.STANDARD_CURSOR_ICONS).Display(PASingletons.Instance.Settings.Keybinds.KeybindList);
                return;
            }

            var elementsList = new List<BaseUI?>();
            foreach (var partItem in compundItem.Parts)
            {
                elementsList.Add(new PAButton(new UIAction(ItemViever, partItem), text: partItem.ToString() ?? ""));
            }
            elementsList.Add(null);
            elementsList.Add(backButton);

            new OptionsUI(elementsList, title, Constants.STANDARD_CURSOR_ICONS).Display(PASingletons.Instance.Settings.Keybinds.KeybindList);
        }

        /// <summary>
        /// Opens the inventory viewer menu.
        /// </summary>
        /// <param name="inventory">The inventory to view.</param>
        public static void InventoryViewer(Inventory inventory)
        {
            var elementsList = new List<BaseUI>();
            foreach (var item in inventory.items)
            {
                elementsList.Add(new PAButton(new UIAction(ItemViever, item), text: item.ToString() ?? ""));
            }
            if (elementsList.Count == 0)
            {
                elementsList.Add(GetBackButton("Empty"));
            }
            new OptionsUI(
                elementsList,
                "Inventory",
                Constants.STANDARD_CURSOR_ICONS,
                scrollSettings: new ScrollSettings(10)
            ).Display(PASingletons.Instance.Settings.Keybinds.KeybindList);
        }
        #endregion

        #region Pause
        /// <summary>
        /// Action for exiting without saving.
        /// </summary>
        private static object? ExitWithoutSavingAction()
        {
            if (!AskYesNoUIQuestion("Are you sure you want to exit WITHOUT saving?", false))
            {
                return null;
            }

            PACSingletons.Instance.Logger.Log("Beginning manual exit", $"save name: {SaveData.Instance.saveName}");
            GameManager.ExitGame(false);
            return -1;
        }

        /// <summary>
        /// Action for exiting and saving.
        /// </summary>
        private static object? SaveAndExitAction()
        {
            if (!AskYesNoUIQuestion("Are you sure you want to exit?", false))
            {
                return null;
            }

            PACSingletons.Instance.Logger.Log("Beginning manual save and exit", $"save name: {SaveData.Instance.saveName}");
            GameManager.ExitGame();
            return -1;
        }

        /// <summary>
        /// Opens the pause menu.
        /// </summary>
        /// <returns>If the game loop should exit.</returns>
        public static bool PauseMenu()
        {
            var elementsList = new List<BaseUI>
            {
                new PAButton(new UIAction(() => -1), text: "Resume"),
                new PAButton(new UIAction(ExitWithoutSavingAction), text: "Exit without saving"),
                new PAButton(new UIAction(SaveAndExitAction), text: "Save and exit"),
            };

            var returnValue = new OptionsUI(elementsList, "Paused", Constants.STANDARD_CURSOR_ICONS).Display(PASingletons.Instance.Settings.Keybinds.KeybindList);
            return returnValue is not null;
        }
        #endregion

        #region Keybinds
        /// <summary>
        /// Displays the keybinds menu.
        /// </summary>
        public static void KeybindSettings()
        {
            tempKeybinds = PASingletons.Instance.Settings.Keybinds.DeepCopy();

            var elementList = new List<BaseUI?>();
            foreach (var actionType in SettingsUtils.ActionTypeAttributes.Keys)
            {
                var actionKey = tempKeybinds.GetActionKey(actionType);
                if (actionKey is null)
                {
                    PACSingletons.Instance.Logger.Log("Action type doesn't exist in keybind, adding default", $"action type: {actionType}", LogSeverity.WARN);
                    actionKey = new ActionKey(actionType, SettingsUtils.ActionTypeAttributes[actionType].defaultKeys.DeepCopy());
                    tempKeybinds.KeybindList = tempKeybinds.KeybindList.Append(actionKey);
                }
                elementList.Add(new KeyField<EnumValue<ActionType>>(
                    actionKey,
                    actionType.Name.Capitalize() + ": ",
                    validatorFunction: KeybindChange,
                    displayValueFunction: new KeyField<EnumValue<ActionType>>.DisplayValueDelegate(KeybindValueDisplay),
                    keyNum: 2
                ));
            }
            elementList.Add(null);
            elementList.Add(new PAButton(new UIAction(SaveKeybinds), text: "Save"));

            new OptionsUI(elementList, " Keybinds", Constants.STANDARD_CURSOR_ICONS).Display(ActionList);
        }
        #endregion

        #region Configs
        public static void ConfigSettings()
        {
            static MultiButtonElement GetMultiButtonEnabledText(bool enabled)
            {
                return new MultiButtonElement(
                    null,
                    enabled
                        ? Tools.StylizedText(" Enabled  ", Constants.Colors.GREEN)
                        : Tools.StylizedText(" Disabled ", Constants.Colors.RED),
                    enabled
                        ? Tools.StylizedText("[Enabled] ", Constants.Colors.GREEN)
                        : Tools.StylizedText("[Disabled]", Constants.Colors.RED)
                );
            }

            static void RefreshConfigMultiChoice(MultiButton multiButton, ConfigLoadingData loadedConfigData)
            {
                loadedConfigData.Enabled = !loadedConfigData.Enabled;
                var newText = GetMultiButtonEnabledText(loadedConfigData.Enabled);
                var button = multiButton.buttons[0];
                button.inactiveText = newText.inactiveText;
                button.activeText = newText.activeText;
            }

            static bool MoveConfigMultiChoice(MultiButton button, List<ConfigLoadingData> loadingOrder, OptionsUI optionsUI, bool moveDown)
            {
                var buttonIndex = optionsUI.elements.IndexOf(button);
                if (
                    buttonIndex == -1 ||
                    (moveDown && buttonIndex >= optionsUI.elements.Count - 3) ||
                    (!moveDown && buttonIndex == 0)
                )
                {
                    return false;
                }
                var otherIndex = buttonIndex + (moveDown ? 1 : -1);
                optionsUI.selected = otherIndex;

                (optionsUI.elements[otherIndex], optionsUI.elements[buttonIndex]) = (optionsUI.elements[buttonIndex], optionsUI.elements[otherIndex]);
                (loadingOrder[otherIndex], loadingOrder[buttonIndex]) = (loadingOrder[buttonIndex], loadingOrder[otherIndex]);
                return true;
            }



            var vanillaInvalid = ConfigUtils.TryGetLoadingOrderAndCorrect(out var loadingOrder);
            var configs = ConfigUtils.GetValidConfigDatas(null);

            // menu elements
            var configsElements = new List<BaseUI?>();
            var configOptionsUI = new OptionsUI([new Toggle()], " EnabledConfigs", Constants.STANDARD_CURSOR_ICONS);
            foreach (var loadedConfig in loadingOrder)
            {
                var config = configs.FirstOrDefault(c => c.configData.Namespace == loadedConfig.Namespace);
                if (config is null)
                {
                    continue;
                }

                var multiButtonEnableText = GetMultiButtonEnabledText(loadedConfig.Enabled);
                var configManagerUIElement = new MultiButton(
                    [
                        new(
                            new(RefreshConfigMultiChoice, loadedConfig),
                            multiButtonEnableText.inactiveText,
                            multiButtonEnableText.activeText
                        ),
                        new(
                            new(MoveConfigMultiChoice, loadingOrder, configOptionsUI, false),
                            " Move Up ",
                            "[Move Up]"
                        ),
                        new(
                            new(MoveConfigMultiChoice, loadingOrder, configOptionsUI, true),
                            " Move Down ",
                            "[Move Down]"
                        )
                    ],
                    " ",
                    preValue: $"\"{config.folderName}\" {Tools.StylizedText(
                            config.configData.Version,
                            config.configData.Version == Constants.CONFIG_VERSION ? Constants.Colors.GREEN : Constants.Colors.RED
                        )} ({loadedConfig.Namespace}): ",
                    modifyList: true
                );
                configsElements.Add(configManagerUIElement);
            }
            configsElements.AddRange([null, GenerateSimpleButton()]);
            configOptionsUI.elements = configsElements;

            // response
            var response = configOptionsUI.Display(PASingletons.Instance.Settings.Keybinds.KeybindList);
            if (response is not null)
            {
                var vanillaEnabled = loadingOrder.FirstOrDefault(c => c.Namespace == Constants.VANILLA_CONFIGS_NAMESPACE)!.Enabled;
                if (!vanillaEnabled &&
                    !AskYesNoUIQuestion(
                        "The vanilla configs have been disabled! This will most likely crash the game, when the configs are next reloded.\n" +
                        $"You can manualy modify enabled configs at \"{Path.GetRelativePath(PACommon.Constants.ROOT_FOLDER, Path.Join(Constants.CONFIGS_FOLDER_PATH, $"{Constants.CONFIGS_LOADING_ORDER_FILE_NAME}.{Constants.CONFIG_EXT}"))}\"\n" +
                        "Are you sure you want to save these settings.",
                        false
                    )
                )
                {
                    return;
                }

                ConfigUtils.SetLoadingOrderData(loadingOrder);

                if (AskYesNoUIQuestion("Do you want to reload the configs now?"))
                {
                    throw new RestartException("Reloading configs");
                }
            }
        }
        #endregion

        #region Ask options
        /// <summary>
        /// Displays the ask options menu.
        /// </summary>
        public static void AskOptions()
        {
            var askDeleteSaveElement = new Toggle(PASingletons.Instance.Settings.AskDeleteSave, "Confirm save folder delete: ", "yes", "no");
            var askRegenerateSaveElement = new Toggle(PASingletons.Instance.Settings.AskRegenerateSave, "Confirm save folders regeneration: ", "yes", "no");

            // default backup action
            var backupActionValues = defBackupActionsList.Select(ac => ac.value);
            var backupActionNames = defBackupActionsList.Select(ac => ac.name).ToList();
            var backupActionValue = backupActionValues.ElementAt(0);
            foreach (var action in backupActionValues)
            {
                if (action == PASingletons.Instance.Settings.DefBackupAction)
                {
                    backupActionValue = action;
                    break;
                }
            }

            var defBackupActionElement = new PAChoice(backupActionNames, backupActionValue, "On save folder backup prompt: ");

            // menu elements
            var askSettingsElements = new List<BaseUI?> { askDeleteSaveElement, askRegenerateSaveElement, defBackupActionElement, null, GenerateSimpleButton() };

            // response
            var response = new OptionsUI(askSettingsElements, " Question popups", Constants.STANDARD_CURSOR_ICONS).Display(PASingletons.Instance.Settings.Keybinds.KeybindList);
            if (response is not null)
            {
                PASingletons.Instance.Settings.AskDeleteSave = askDeleteSaveElement.Value;
                PASingletons.Instance.Settings.AskRegenerateSave = askRegenerateSaveElement.Value;
                PASingletons.Instance.Settings.DefBackupAction = backupActionValues.ElementAt(defBackupActionElement.Value);
            }
        }
        #endregion

        #region other options
        /// <summary>
        /// Displays the other options menu.
        /// </summary>
        public static void OtherOptions()
        {
            // auto save
            var autoSaveElement = new Toggle(PASingletons.Instance.Settings.AutoSave, "Auto save: ");

            // logging
            var loggingSeverities = loggingSeveritiesList.Select(el => el.value);
            var loggingSeverityNames = loggingSeveritiesList.Select(el => el.name).ToList();
            var currentLoggingLevel = PASingletons.Instance.Settings.LoggingLevel;

            var loggingLevelIndex = loggingSeverities.Count() - 1;
            for (var x = 0; x < loggingSeverities.Count(); x++)
            {
                if (loggingSeverities.ElementAt(x) == currentLoggingLevel)
                {
                    loggingLevelIndex = x;
                    break;
                }
            }
            var loggingElement = new PAChoice(loggingSeverityNames, loggingLevelIndex, "Logging: ");

            // enable colored text
            var coloredTextElement = new Toggle(PASingletons.Instance.Settings.EnableColoredText, "Colored text: ", "enabled", "disabled");

            // menu elements
            var menuElements = new List<BaseUI?> { autoSaveElement, loggingElement, coloredTextElement, null, GenerateSimpleButton() };

            // response
            var response = new OptionsUI(menuElements, " Other options", Constants.STANDARD_CURSOR_ICONS).Display(PASingletons.Instance.Settings.Keybinds.KeybindList);
            if (response is not null)
            {
                var newAutoSaveValue = autoSaveElement.Value;
                var newLoggingLevel = loggingSeverities.ElementAt(loggingElement.Value);
                var newColoredTextValue = coloredTextElement.Value;

                PASingletons.Instance.Settings.AutoSave = newAutoSaveValue;
                PASingletons.Instance.Settings.LoggingLevel = newLoggingLevel;
                PASingletons.Instance.Settings.EnableColoredText = newColoredTextValue;
            }
        }
        #endregion

        /// <summary>
        /// Regenerates the save file.
        /// </summary>
        /// <param name="saveName">The name of the save folder.</param>
        /// <param name="makeBackup">Whether to make a backup before regenerating.</param>
        public static void RegenerateSaveFile(string saveName, bool makeBackup = true)
        {
            Console.WriteLine($"Regenerating \"{saveName}\":");
            PACSingletons.Instance.Logger.Log("Regenerating save file", $"save name: {saveName}");
            Console.Write("\tLoading...");
            SaveManager.LoadSave(saveName, false, makeBackup);
            Console.WriteLine("DONE!");
            PACSingletons.Instance.Logger.Log("Loading all chunks from file", $"save name: {saveName}");
            World.LoadAllChunksFromFolder(showProgressText: "\tLoading world...");
            Console.Write("\tDeleting...");
            Tools.DeleteSave(saveName);
            Console.WriteLine("DONE!");
            Console.Write("\tSaving...\r");
            SaveManager.MakeSave(showProgressText: "\tSaving...");
            PACSingletons.Instance.Logger.Log("Save file regenerated", $"save name: {saveName}");
        }

        /// <summary>
        /// Displays the main menu.
        /// </summary>
        public static void MainMenu()
        {
            ActionList = PASingletons.Instance.Settings.Keybinds.KeybindList.ToList();

            var (answers, actions) = GetMainMenuLists();

            new UIList(
                answers,
                " Main menu",
                Constants.STANDARD_CURSOR_ICONS,
                canEscape: true,
                actions: actions,
                modifiableUIList: true
            ).Display(ActionList);
        }
        #endregion

        #region Menu actions
        /// <summary>
        /// Action, called when the new save button is pressed.
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
        /// Action, called when the load saves button is pressed.
        /// </summary>
        /// <param name="mainMenuUI">The main menu <c>UIList</c>.</param>
        private static void LoadSavesAction(UIList mainMenuUI)
        {
            GetSavesMenu().Display(ActionList);

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

            return SavesData.Count != 0 ? null : -1;
        }

        /// <summary>
        /// Action, called when the delete saves button is pressed.
        /// </summary>
        /// <param name="loadSaveUI">The load save menu <c>UIList</c>.</param>
        private static object? DeleteSavesAction(UIList loadSaveUI)
        {
            GetDeleteSavesMenu().Display(ActionList);

            var (answers, actions) = GetSavesMenuLists();
            loadSaveUI.answers = answers;
            loadSaveUI.actions = actions;

            return SavesData.Count != 0 ? null : -1;
        }

        /// <summary>
        /// Action, called when a delete save button is pressed.
        /// </summary>
        /// <param name="deleteSavesUI">The delete saves menu <c>UIList</c>.</param>
        /// <param name="selectedSaveName">The name of the save to delete.</param>
        private static object? DeleteSaveAction(UIList deleteSavesUI, string selectedSaveName)
        {
            if (!PASingletons.Instance.Settings.AskDeleteSave || AskYesNoUIQuestion($" Are you sure you want to remove Save file {selectedSaveName}?", false))
            {
                Tools.DeleteSave(selectedSaveName);
            }

            var (answers, actions) = GetDeleteSavesMenuLists();
            deleteSavesUI.answers = answers;
            deleteSavesUI.actions = actions;

            return SavesData.Count != 0 ? null : -1;
        }

        /// <summary>
        /// Action, called when the regenerate saves button is pressed.
        /// </summary>
        /// <param name="loadSaveUI">The load saves menu <c>UIList</c>.</param>
        private static void RegenerateSavesAction(UIList loadSaveUI)
        {
            if (!PASingletons.Instance.Settings.AskRegenerateSave || AskYesNoUIQuestion(" Are you sure you want to regenerate ALL save files? This will load, delete then resave EVERY save file!", false))
            {
                bool backupSaves;
                if (PASingletons.Instance.Settings.DefBackupAction == -1)
                {
                    backupSaves = AskYesNoUIQuestion(" Do you want to backup your save files before regenerating them?");
                }
                else
                {
                    backupSaves = PASingletons.Instance.Settings.DefBackupAction == 1;
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

        /// <inheritdoc cref="KeyField.ValidatorDelegate"/>
        private static (TextFieldValidatorStatus status, string? message) KeybindChange(ConsoleKeyInfo key, KeyField<EnumValue<ActionType>> keyField)
        {
            tempKeybinds.UpdateKeybindConflicts();
            return (TextFieldValidatorStatus.VALID, null);
        }

        /// <summary>
        /// Runs, when the user saves the edited keybinds.
        /// </summary>
        private static object SaveKeybinds()
        {
            PACSingletons.Instance.Logger.Log("Keybinds changed", $"\n{PASingletons.Instance.Settings.Keybinds}\n -> \n{tempKeybinds}", LogSeverity.DEBUG);
            PASingletons.Instance.Settings.Keybinds = tempKeybinds;

            // in place keybinds switch
            ActionList.Clear();
            ActionList.AddRange(PASingletons.Instance.Settings.Keybinds.KeybindList);

            return -1;
        }
        #endregion

        #region Private function
        /// <summary>
        /// Returns a button for the <c>OptionsUI</c>, that exits the function.
        /// </summary>
        private static PAButton GenerateSimpleButton(string text = "Save")
        {
            return new PAButton(new UIAction(UIExitFunction), text: text);
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

                actions.Add(new UIAction(DeleteSaveAction, saveName));
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

                actions.Add(new UIAction(LoadSaveAction, saveName));
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
                "Configs",
                "Question popups",
                "Other",
                null,
                "Back"
            };

            var optionsMenuActions = new List<UIAction>
            {
                new(KeybindSettings),
                new(ConfigSettings),
                new(AskOptions),
                new(OtherOptions),
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

            if (SavesData.Count != 0)
            {
                var loadSaveAction = new UIAction(LoadSavesAction);

                answers = ["New save", "Load/Delete save", "Options"];
                actions =
                [
                    newSaveAction,
                    loadSaveAction,
                    optionsAction,
                ];
            }
            else
            {
                answers = ["New save", "Options"];
                actions =
                [
                    newSaveAction,
                    optionsAction,
                ];
            }

            return (answers, actions);
        }
        #endregion
    }
}

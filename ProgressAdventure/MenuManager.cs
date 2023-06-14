using ProgressAdventure.Enums;
using ProgressAdventure.SettingsManagement;
using SaveFileManager;
using System.Drawing;
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

        //public static void MainMenu()
        //{
        //    files_data = sm.get_saves_data()
        //    in_main_menu = True
        //    while True:
        //        status: tuple[int, str | Literal[-1], bool] = (-1, -1, False)
        //        if in_main_menu:
        //            in_main_menu = False
        //            if len(files_data) :
        //                mm_list = ["New save", "Load/Delete save", "Options"]
        //            else:
        //                mm_list = ["New save", "Options"]
        //        option = sfm.UI_list_s(mm_list, " Main menu", can_esc = True).display(Settings.keybinds)
        //        elif len(files_data):
        //            option = 1
        //        else:
        //            option = -2
        //            in_main_menu = True
        //        // new file
        //        if option == 0:
        //            status = (1, "", False)
        //        elif option == -1:
        //            break
        //        // load/delete
        //        elif option == 1 and len(files_data):
        //            // get data from file_data
        //            list_data = []
        //            for data in files_data:
        //                list_data.append(data[1])
        //                list_data.append(None)
        //            list_data.append("Regenerate all save files")
        //            list_data.append("Delete file")
        //            list_data.append("Back")
        //            option = sfm.UI_list_s(list_data, " Level select", True, True, exclude_nones = True).display(Settings.keybinds)
        //            // load
        //            if option != -1 and option<len(files_data):
        //                status = (0, files_data[int(option)][0], files_data[int(option)][2])
        //            // regenerate
        //            elif option == len(files_data) :
        //                if not Settings.ask_regenerate_save or sfm.UI_list_s(["No", "Yes"], f" Are you sure you want to regenerate ALL save files? This will load, delete then resave EVERY save file!", can_esc = True).display(Settings.keybinds) == 1:
        //                    if Settings.def_backup_action == -1:
        //                        backup_saves = bool(sfm.UI_list_s(["Yes", "No"], f" Do you want to backup your save files before regenerating them?", can_esc = True).display(Settings.keybinds) == 0)
        //                    else:
        //                        backup_saves = bool(Settings.def_backup_action)
        //                    print("Regenerating save files...\n")
        //                    for save in files_data:
        //                        regenerate_save_file(save[0], save[2], backup_saves)
        //                    files_data = sm.get_saves_data()
        //                    print("\nDONE!")
        //            // delete
        //            elif option == len(files_data) + 1:
        //                // remove "delete file" + "regenerate save files"
        //                list_data.pop(len(list_data) - 2)
        //                list_data.pop(len(list_data) - 2)
        //                while len(files_data) > 0:
        //                    option = sfm.UI_list(list_data, " Delete mode!", DELETE_CURSOR_ICONS, True, True, exclude_nones = True).display(Settings.keybinds)
        //                    if option != -1 and option< (len(list_data) -1) / 2:
        //                        if not Settings.ask_delete_save or sfm.UI_list_s(["No", "Yes"], f" Are you sure you want to remove Save file {files_data[option][0]}?", can_esc = True).display(Settings.keybinds) :
        //                            ts.remove_save(files_data[option][0], files_data[option][2])
        //                            list_data.pop(option * 2)
        //                            list_data.pop(option * 2)
        //                            files_data.pop(option)
        //                    else:
        //                        break
        //                if len(files_data) == 0:
        //                    in_main_menu = True
        //            // back
        //            else:
        //                in_main_menu = True
        //        elif(option == 2 and len(files_data)) or(option == 1 and not len(files_data)) :
        //            sfm.UI_list(["Keybinds", "Question popups", "Other", None, "Back"], " Options", None, False, True, [keybind_setting, ask_options, other_options], True).display(Settings.keybinds)
        //            in_main_menu = True

        //        // action
        //        // new save
        //        if status[0] == 1:
        //            press_key(f"\nCreating new save!\n")
        //            new_save()
        //            files_data = sm.get_saves_data()
        //        // load
        //        elif status[0] == 0:
        //            press_key(f"\nLoading save: {status[1]}!")
        //            load_save(status[1], status[2])
        //            files_data = sm.get_saves_data()
        //}
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

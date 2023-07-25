using ProgressAdventure.Enums;
using SaveFileManager;

namespace ProgressAdventure.SettingsManagement
{
    public static class SettingsUtils
    {
        #region Config dictionaries
        /// <summary>
        /// The dictionary pairing up action types, to their ignore modes.
        /// </summary>
        public static readonly Dictionary<ActionType, List<GetKeyMode>> actionTypeIgnoreMapping = new()
        {
            [ActionType.ESCAPE] = new List<GetKeyMode> { GetKeyMode.IGNORE_ESCAPE },
            [ActionType.UP] = new List<GetKeyMode> { GetKeyMode.IGNORE_VERTICAL },
            [ActionType.DOWN] = new List<GetKeyMode> { GetKeyMode.IGNORE_VERTICAL },
            [ActionType.LEFT] = new List<GetKeyMode> { GetKeyMode.IGNORE_HORIZONTAL },
            [ActionType.RIGHT] = new List<GetKeyMode> { GetKeyMode.IGNORE_HORIZONTAL },
            [ActionType.ENTER] = new List<GetKeyMode> { GetKeyMode.IGNORE_ENTER }
        };

        /// <summary>
        /// The dictionary pairing up action types, to responses.
        /// </summary>
        public static readonly Dictionary<ActionType, Key> actionTypeResponseMapping = new()
        {
            [ActionType.ESCAPE] = Key.ESCAPE,
            [ActionType.UP] = Key.UP,
            [ActionType.DOWN] = Key.DOWN,
            [ActionType.LEFT] = Key.LEFT,
            [ActionType.RIGHT] = Key.RIGHT,
            [ActionType.ENTER] = Key.ENTER
        };

        /// <summary>
        /// The dictionary for the name of some keys that normaly don't display anything.
        /// </summary>
        public static readonly Dictionary<ConsoleKey, string> specialKeyNameMap = new()
        {
            [ConsoleKey.Enter] = "enter",
            [ConsoleKey.Escape] = "escape",
            [ConsoleKey.Spacebar] = "space",
            [ConsoleKey.Delete] = "delete",

            [ConsoleKey.UpArrow] = "up arrow",
            [ConsoleKey.DownArrow] = "down arrow",
            [ConsoleKey.LeftArrow] = "left arrow",
            [ConsoleKey.RightArrow] = "right arrow",

            [ConsoleKey.NumPad0] = "Num0",
            [ConsoleKey.NumPad1] = "Num1",
            [ConsoleKey.NumPad2] = "Num2",
            [ConsoleKey.NumPad3] = "Num3",
            [ConsoleKey.NumPad4] = "Num4",
            [ConsoleKey.NumPad5] = "Num5",
            [ConsoleKey.NumPad6] = "Num6",
            [ConsoleKey.NumPad7] = "Num7",
            [ConsoleKey.NumPad8] = "Num8",
            [ConsoleKey.NumPad9] = "Num9",

            [ConsoleKey.Insert] = "insert",
            [ConsoleKey.End] = "end",
            [ConsoleKey.PageDown] = "page down",
            [ConsoleKey.Clear] = "clear",
            [ConsoleKey.Home] = "home",
            [ConsoleKey.PageUp] = "page up"
        };
        #endregion

        #region Public functions
        /// <summary>
        /// Returns the string representation of the key, that the <c>ConsoleKeyInfo</c> represents.
        /// </summary>
        /// <param name="key">The key.</param>
        public static string GetKeyName(ConsoleKeyInfo key)
        {
            var keyMods = new List<string>();
            //modifier
            if (Utils.GetBit((int)key.Modifiers, 2))
            {
                keyMods.Add("Ctrl");
            }
            if (Utils.GetBit((int)key.Modifiers, 0))
            {
                keyMods.Add("Alt");
            }
            if (Utils.GetBit((int)key.Modifiers, 1))
            {
                keyMods.Add("Shift");
            }
            //mods string
            var keyModsStr = "";
            if (keyMods.Any())
            {
                keyModsStr = $" ({string.Join(" + ", keyMods)})";
            }
            //key
            string keyName;
            if (specialKeyNameMap.ContainsKey(key.Key))
            {
                keyName = specialKeyNameMap[key.Key];
            }
            else if (key.KeyChar != 0 && !char.IsControl(key.KeyChar) && !string.IsNullOrWhiteSpace(key.KeyChar.ToString()))
            {
                keyName = key.KeyChar.ToString();
            }
            else if (!string.IsNullOrWhiteSpace(key.Key.ToString()))
            {
                keyName = (int)key.Key == 18 ? "[CONTROLL KEY]" : key.Key.ToString();
            }
            else
            {
                keyName = "[UNDISPLAYABLE KEY]";
            }
            return keyName + keyModsStr;
        }

        /// <summary>
        /// Returns the colored version of the key names, depending on if it conflicts.
        /// </summary>
        /// <param name="actionKey">The <c>ActionKey</c> to get the names from.</param>
        public static List<string> GetColoredNames(ActionKey actionKey)
        {
            var names = new List<string>();
            for (int x = 0; x < actionKey.Keys.Count(); x++)
            {
                names.Add(Utils.StylizedText(actionKey.Names[x], actionKey.conflicts[x] ? Constants.Colors.RED : null));
            }
            return names;
        }

        /// <summary>
        /// Returns the default keybind list, for a Keybinds object.
        /// </summary>
        public static List<ActionKey> GetDefaultKeybindList()
        {
            return new List<ActionKey>
            {
                new ActionKey(ActionType.ESCAPE, new List<ConsoleKeyInfo> { new ConsoleKeyInfo((char)ConsoleKey.Escape, ConsoleKey.Escape, false, false, false) }),
                new ActionKey(ActionType.UP, new List<ConsoleKeyInfo> { new ConsoleKeyInfo((char)0, ConsoleKey.UpArrow, false, false, false) }),
                new ActionKey(ActionType.DOWN, new List<ConsoleKeyInfo> { new ConsoleKeyInfo((char)0, ConsoleKey.DownArrow, false, false, false) }),
                new ActionKey(ActionType.LEFT, new List<ConsoleKeyInfo> { new ConsoleKeyInfo((char)0, ConsoleKey.LeftArrow, false, false, false) }),
                new ActionKey(ActionType.RIGHT, new List<ConsoleKeyInfo> { new ConsoleKeyInfo((char)0, ConsoleKey.RightArrow, false, false, false) }),
                new ActionKey(ActionType.ENTER, new List<ConsoleKeyInfo> { new ConsoleKeyInfo((char)ConsoleKey.Enter, ConsoleKey.Enter, false, false, false) })
            };
        }

        /// <summary>
        /// Returns the "json" representation of the default settings file.
        /// </summary>
        public static Dictionary<string, object> GetDefaultSettings()
        {
            return new Dictionary<string, object>
            {
                [SettingsKey.AUTO_SAVE.ToString()] = true,
                [SettingsKey.LOGGING_LEVEL.ToString()] = 0,
                [SettingsKey.KEYBINDS.ToString()] = new Keybinds(GetDefaultKeybindList()).ToJson(),
                [SettingsKey.ASK_DELETE_SAVE.ToString()] = true,
                [SettingsKey.ASK_REGENERATE_SAVE.ToString()] = true,
                [SettingsKey.DEF_BACKUP_ACTION.ToString()] = -1,
                [SettingsKey.ENABLE_COLORED_TEXT.ToString()] = true,
            };
        }
        #endregion
    }
}

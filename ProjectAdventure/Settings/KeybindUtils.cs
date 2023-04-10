using ProjectAdventure.Enums;
using SaveFileManager;
using System;

namespace ProjectAdventure.Settings
{
    public static class KeybindUtils
    {
        #region Config dictionaries
        /// <summary>
        /// The dictionary pairing up action types, to their ignore modes.
        /// </summary>
        public static readonly IDictionary<ActionType, IEnumerable<GetKeyMode>> actionTypeIgnoreMapping = new Dictionary<ActionType, IEnumerable<GetKeyMode>> {
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
        public static readonly IDictionary<ActionType, Key> actionTypeResponseMapping = new Dictionary<ActionType, Key> {
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
        public static readonly IDictionary<ConsoleKey, string> specialKeyNameMap = new Dictionary<ConsoleKey, string>
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

        public static IEnumerable<ActionKey> GetDefaultKeybindList()
        {
            return new List<ActionKey> {
                new ActionKey(ActionType.ESCAPE, new List<ConsoleKeyInfo> { new ConsoleKeyInfo((char)ConsoleKey.Escape, ConsoleKey.Escape, false, false, false) }),
                new ActionKey(ActionType.UP, new List<ConsoleKeyInfo> { new ConsoleKeyInfo((char)ConsoleKey.UpArrow, ConsoleKey.UpArrow, false, false, false) }),
                new ActionKey(ActionType.DOWN, new List<ConsoleKeyInfo> { new ConsoleKeyInfo((char)ConsoleKey.DownArrow, ConsoleKey.DownArrow, false, false, false) }),
                new ActionKey(ActionType.LEFT, new List<ConsoleKeyInfo> { new ConsoleKeyInfo((char)ConsoleKey.LeftArrow, ConsoleKey.LeftArrow, false, false, false) }),
                new ActionKey(ActionType.RIGHT, new List<ConsoleKeyInfo> { new ConsoleKeyInfo((char)ConsoleKey.RightArrow, ConsoleKey.RightArrow, false, false, false) }),
                new ActionKey(ActionType.ENTER, new List<ConsoleKeyInfo> { new ConsoleKeyInfo((char)ConsoleKey.Enter, ConsoleKey.Enter, false, false, false) })
            };
        }

        /// <summary>
        /// Turns the settings part of the settings file into an <c>Keybinds</c> object.
        /// </summary>
        /// <param name="keybindsJson">The json representation of the <c>Keybinds</c> object.</param>
        public static Keybinds KeybindsFromJson(IDictionary<string, IEnumerable<IDictionary<string, object>>> keybindsJson)
        {
            var actions = new List<ActionKey>();
            foreach (var actionTypeStr in keybindsJson.Keys)
            {
                if (Enum.TryParse(typeof(ActionType), actionTypeStr, out object? res))
                {
                    var actionType = (ActionType)res;
                    var keyList = keybindsJson[actionTypeStr];
                    var keys = new List<ConsoleKeyInfo>();
                    foreach (var key in keyList)
                    {
                        if (
                            key.ContainsKey("key") &&
                            key.ContainsKey("keyChar") &&
                            key.ContainsKey("modifiers") &&
                            Enum.TryParse(typeof(ConsoleKey), key["key"].ToString(), out object? keyEnum) &&
                            char.TryParse(key["keyChar"].ToString(), out char keyChar) &&
                            int.TryParse(key["modifiers"].ToString(), out int keyMods)
                            )
                        {
                            var alt = Utils.GetBit(keyMods, 0);
                            var shift = Utils.GetBit(keyMods, 1);
                            var ctrl = Utils.GetBit(keyMods, 2);
                            keys.Add(new ConsoleKeyInfo(keyChar, (ConsoleKey)keyEnum, shift, alt, ctrl));
                        }
                    }
                    actions.Add(new ActionKey(actionType, keys));
                }
            }
            return new Keybinds(actions);
        }
        #endregion
    }
}

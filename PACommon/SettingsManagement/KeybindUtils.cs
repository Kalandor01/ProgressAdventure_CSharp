namespace PACommon.SettingsManagement
{
    public static class KeybindUtils
    {
        #region Config dictionaries
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

        #region Public Fields
        public static bool colorEnabled = true;
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
            if (keyMods.Count != 0)
            {
                keyModsStr = $" ({string.Join(" + ", keyMods)})";
            }
            //key
            string keyName;
            if (specialKeyNameMap.TryGetValue(key.Key, out string? value))
            {
                keyName = value;
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
        public static List<string> GetColoredNames<T>(AActionKey<T> actionKey)
            where T : notnull
        {
            var names = new List<string>();
            for (int x = 0; x < actionKey.Keys.Count(); x++)
            {
                names.Add(colorEnabled ? Utils.StylizedText(actionKey.Names[x], actionKey.Conflicts[x] ? Constants.Colors.RED : null) : actionKey.Names[x]);
            }
            return names;
        }
        #endregion
    }
}

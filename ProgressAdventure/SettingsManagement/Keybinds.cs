using ProgressAdventure.Enums;
using System.Text;

namespace ProgressAdventure.SettingsManagement
{
    /// <summary>
    /// Class for storing the keybinds list.
    /// </summary>
    public class Keybinds : IJsonConvertable<Keybinds>
    {
        #region Private fields
        /// <summary>
        /// The list of actions(/keybinds).
        /// </summary>
        private IEnumerable<ActionKey> _keybinds;
        #endregion

        #region Public properties
        /// <summary>
        /// <inheritdoc cref="KeybindList" path="//summary"/>
        /// </summary>
        public IEnumerable<ActionKey> KeybindList
        {
            get => _keybinds;
            set
            {
                _keybinds = value;
                FillAllKeybinds();
                UpdateKeybindConflicts();
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// <inheritdoc cref="KeybindList"/>
        /// </summary>
        /// <param name="actions"><inheritdoc cref="KeybindList" path="//summary"/></param>
        public Keybinds(IEnumerable<ActionKey>? actions)
        {
            if (actions is null)
            {
                Logger.Log("No actions in actions list.", "Recreating key actions from defaults", LogSeverity.ERROR);
            }
            KeybindList = actions ?? SettingsUtils.GetDefaultKeybindList();
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Returns the <c>ActionKey</c> by type.
        /// </summary>
        /// <param name="actionType">The action type.</param>
        public ActionKey GetActionKey(ActionType actionType)
        {
            foreach (var key in KeybindList)
            {
                if (key.actionType == actionType)
                {
                    return key;
                }
            }
            Logger.Log("Unknown ActionType", "trying to create a placeholder key", severity:LogSeverity.ERROR);
            return new ActionKey(ActionType.ESCAPE, new List<ConsoleKeyInfo> { new ConsoleKeyInfo((char)ConsoleKey.Escape, ConsoleKey.Escape, false, false, false) });
        }

        public void FillAllKeybinds()
        {
            if (!KeybindList.Any())
            {
                Logger.Log("No actions in actions list.", "Recreating key actions from defaults", LogSeverity.ERROR);
                KeybindList = SettingsUtils.GetDefaultKeybindList();
            }
        }

        /// <summary>
        /// Marks all of the keys, that are the same, as another key.
        /// </summary>
        public void UpdateKeybindConflicts()
        {
            foreach (var keybind in KeybindList)
            {
                keybind.conflicts = keybind.Keys.Select(k => false).ToList();
            }
            foreach (var action1 in KeybindList)
            {
                foreach (var action2 in KeybindList)
                {
                    if (action1 != action2)
                    {
                        var key1 = action1.Keys;
                        var key2 = action2.Keys;
                        for (var x = 0; x < key1.Count(); x++)
                        {
                            for (var y = 0; y < key2.Count(); y++)
                            {
                                if (key1.ElementAt(x).Equals(key2.ElementAt(y)))
                                {
                                    action1.conflicts[x] = true;
                                    action2.conflicts[y] = true;
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Public overrides
        public override bool Equals(object? obj)
        {
            if (obj is null || GetType() != obj.GetType())
            {
                return false;
            }
            if (base.Equals(obj))
            {
                return true;
            }
            var kbObj = (Keybinds)obj;
            if (_keybinds.Equals(kbObj._keybinds))
            {
                return true;
            }
            if (_keybinds.Count() != kbObj._keybinds.Count())
            {
                return false;
            }
            for (var x = 0; x < _keybinds.Count(); x++)
            {
                if (!_keybinds.ElementAt(x).Equals(kbObj._keybinds.ElementAt(x)))
                {
                    return false;
                }
            }
            return true;
        }

        public override string ToString()
        {
            return string.Join("\n", KeybindList);
        }
        #endregion

        #region JsonConvert
        public Dictionary<string, object?> ToJson()
        {
            var keybindsJson = new Dictionary<string, object?>();
            foreach (var keybind in KeybindList)
            {
                var kbJson = keybind.ToJson().First();
                keybindsJson.Add(kbJson.Key.ToLower(), kbJson.Value);
            }
            return keybindsJson;
        }

        public static bool FromJson(IDictionary<string, object?>? keybindsJson, string fileVersion, out Keybinds keybinds)
        {
            if (keybindsJson is null)
            {
                Logger.Log("Keybinds parse error", "keybinds json is null", LogSeverity.WARN);
                keybinds = new Keybinds(null);
                return false;
            }

            var success = true;
            var actions = new List<ActionKey>();
            foreach (var actionJson in keybindsJson)
            {
                success &= ActionKey.FromJson(
                    new Dictionary<string, object?> { [actionJson.Key] = actionJson.Value },
                    fileVersion,
                    out ActionKey? actionKey
                );
                if (actionKey is not null)
                {
                    actions.Add(actionKey);
                }
            }
            keybinds = new Keybinds(actions);
            return success;
        }
        #endregion
    }
}

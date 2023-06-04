using ProgressAdventure.Enums;

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
                keybind.conflict = false;
            }
            foreach (var key1 in KeybindList)
            {
                foreach (var key2 in KeybindList)
                {
                    if (key1 != key2)
                    {
                        if
                        (
                            key1.Keys.Any() &&
                            key2.Keys.Any() &&
                            key1.Keys.First() == key2.Keys.First()
                        )
                        {
                            key1.conflict = true;
                            key2.conflict = true;
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
        #endregion

        #region JsonConvert
        public Dictionary<string, object?> ToJson()
        {
            var keybindsJson = new Dictionary<string, object?>();
            foreach (var keybind in KeybindList)
            {
                var kbJson = keybind.ToJson().First();
                keybindsJson.Add(kbJson.Key, kbJson.Value);
            }
            return keybindsJson;
        }

        public static Keybinds FromJson(IDictionary<string, object?>? keybindsJson, string fileVersion = Constants.SAVE_VERSION)
        {
            if (keybindsJson is null)
            {
                Logger.Log("Keybinds parse error", "keybinds json is null", LogSeverity.WARN);
                return new Keybinds(null);
            }

            var actions = new List<ActionKey>();
            foreach (var actionJson in keybindsJson)
            {
                var actionKey = ActionKey.FromJson(
                    new Dictionary<string, object?> { [actionJson.Key] = actionJson.Value },
                    fileVersion
                );
                if (actionKey is not null)
                {
                    actions.Add(actionKey);
                }
            }
            return new Keybinds(actions);
        }
        #endregion
    }
}

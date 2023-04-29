using ProgressAdventure.Enums;
using System.Collections;

namespace ProgressAdventure.SettingsManagement
{
    /// <summary>
    /// Class for storing the keybinds list.
    /// </summary>
    public class Keybinds
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
                UpdateKeybindConflicts();
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// <inheritdoc cref="KeybindList"/>
        /// </summary>
        /// <param name="actions"><inheritdoc cref="KeybindList" path="//summary"/></param>
        /// <exception cref="ArgumentException"></exception>
        public Keybinds(IEnumerable<ActionKey> actions)
        {
            if (!actions.Any())
            {
                Logger.Log("No actions in actions list.", severity: LogSeverity.FATAL);
                throw new ArgumentException("No actions in actions list.", nameof(actions));
            }
            KeybindList = actions;
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

        /// <summary>
        /// Turns the <c>Keybinds</c> objest into a json object for the settings file.
        /// </summary>
        public Dictionary<string, List<Dictionary<string, object>>> ToJson()
        {
            var keybindsJson = new Dictionary<string, List<Dictionary<string, object>>>();
            foreach (var keybind in KeybindList)
            {
                var kbJson = keybind.ToJson();
                keybindsJson.Add(kbJson.Key, kbJson.Value);
            }
            return keybindsJson;
        }
        #endregion

        #region Public functions
        /// <summary>
        /// Converts the <c>Keybinds</c> json to object format.
        /// </summary>
        /// <param name="keybindsJson">The json representation of the <c>Keybinds</c> object.<br/>
        /// Its actual type should be IDictionary{string, IEnumerable{IDictionary{string, object}}}</param>
        public static Keybinds FromJson(IDictionary<string, object> keybindsJson)
        {
            var actions = new List<ActionKey>();
            foreach (var actionJson in keybindsJson)
            {
                if (
                    Enum.TryParse(typeof(ActionType), actionJson.Key, out object? res) &&
                    Enum.IsDefined(typeof(ActionType), (ActionType)res)
                )
                {
                    var actionType = (ActionType)res;
                    var keys = new List<ConsoleKeyInfo>();
                    foreach (var actionKey in (IEnumerable)actionJson.Value)
                    {
                        var actionDict = (IDictionary<string, object>)actionKey;
                        if (
                            Enum.TryParse(typeof(ConsoleKey), actionDict.TryGetValue("key", out var keyValue) ? keyValue.ToString() : null, out object? keyEnum) &&
                            Enum.IsDefined(typeof(ConsoleKey), (ConsoleKey)keyEnum) &&
                            char.TryParse(actionDict.TryGetValue("keyChar", out var charValue) ? charValue.ToString() : null, out char keyChar) &&
                            int.TryParse(actionDict.TryGetValue("modifiers", out var modValue) ? modValue.ToString() : null, out int keyMods)
                            )
                        {
                            var alt = Utils.GetBit(keyMods, 0);
                            var shift = Utils.GetBit(keyMods, 1);
                            var ctrl = Utils.GetBit(keyMods, 2);
                            keys.Add(new ConsoleKeyInfo(keyChar, (ConsoleKey)keyEnum, shift, alt, ctrl));
                        }
                        else
                        {
                            Logger.Log("Couldn't parse key from action JSON", actionKey.ToString(), LogSeverity.WARN);
                        }
                    }
                    actions.Add(new ActionKey(actionType, keys));
                }
                else
                {
                    Logger.Log("Couldn't parse action from Keybinds JSON", actionJson.ToString(), LogSeverity.WARN);
                }
            }
            return new Keybinds(actions);
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
    }
}

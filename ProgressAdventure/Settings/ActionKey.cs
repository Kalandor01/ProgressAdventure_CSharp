using ProgressAdventure.Enums;
using SaveFileManager;

namespace ProgressAdventure.Settings
{
    /// <summary>
    /// Class for storing a key for keybinds.
    /// </summary>
    public class ActionKey : KeyAction
    {
        #region Public fields
        /// <summary>
        /// The type of the action.
        /// </summary>
        public ActionType actionType;
        /// <summary>
        /// If the key conflicts with another key in the keybinds or not.
        /// </summary>
        public bool conflict;
        #endregion

        #region Public properties
        /// <summary>
        /// The keys that can be pressed to trigger this action.
        /// </summary>
        public new IEnumerable<ConsoleKeyInfo> Keys
        {
             get => base.Keys;
            set {
                base.Keys = value;
                UpdateName();
            }
        }

        /// <summary>
        /// The display name of the key.
        /// </summary>
        public string Name { get; private set; }
        #endregion

        #region Constructors
        /// <summary>
        /// <inheritdoc cref="ActionKey"/>
        /// </summary>
        /// <param name="actionType"><inheritdoc cref="actionType" path="//summary"/></param>
        /// <param name="keys"><inheritdoc cref="Keys" path="//summary"/></param>
        /// <exception cref="ArgumentException"></exception>
        public ActionKey(ActionType actionType, IEnumerable<ConsoleKeyInfo> keys)
            : base(
                  KeybindUtils.actionTypeResponseMapping[actionType],
                  keys,
                  KeybindUtils.actionTypeIgnoreMapping[actionType]
            )
        {
            if (!keys.Any())
            {
                Logger.Log("No keys in keys list!", severity:LogSeverity.FATAL);
                throw new ArgumentException("No keys in keys list!", nameof(keys));
            }
            this.actionType = actionType;
            conflict = false;
            UpdateName();
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Updates the display name of the key.
        /// </summary>
        public void UpdateName()
        {
            Name = KeybindUtils.GetKeyName(Keys.ElementAt(0));
        }

        /// <summary>
        /// Turns the <c>ActionKey</c> objest into a json object for the settings file.
        /// </summary>
        public (string key, IEnumerable<IDictionary<string, object>> value) ToJson()
        {
            var keyList = new List<IDictionary<string, object>>();
            foreach (var key in Keys)
            {
                var keyJson = new Dictionary<string, object>()
                {
                    ["key"] = (int)key.Key,
                    ["keyChar"] = key.KeyChar,
                    ["modifiers"] = (int)key.Modifiers
                };
                keyList.Add(keyJson);
            }
            return (actionType.ToString(), keyList);
        }
        #endregion
    }
}

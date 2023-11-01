using PACommon.Enums;
using PACommon.Logging;
using SaveFileManager;

namespace PACommon.SettingsManagement
{
    /// <summary>
    /// Class for storing a key for keybinds.
    /// </summary>
    /// <typeparam name="T">The enum to use, for denoting the action type of this object.</typeparam>
    public abstract class AActionKey<T> : KeyAction
        where T : notnull, Enum
    {
        #region Public properties
        /// <summary>
        /// The type of the action.
        /// </summary>
        public T ActionType { get; protected set; }

        /// <summary>
        /// A list indicating if a key in a list conflicts with another key in the keybinds.
        /// </summary>
        public List<bool> Conflicts { get; set; }

        /// <summary>
        /// The keys that can be pressed to trigger this action.
        /// </summary>
        public new IEnumerable<ConsoleKeyInfo> Keys
        {
            get => base.Keys;
            set
            {
                base.Keys = value.Distinct();
                UpdateNames();
                Conflicts = Keys.Select(k => false).ToList();
            }
        }

        /// <summary>
        /// The display names of the keys.
        /// </summary>
        public List<string> Names { get; protected set; }
        #endregion

        #region Constructors
        /// <summary>
        /// <inheritdoc cref="ActionKey"/>
        /// </summary>
        /// <param name="actionType"><inheritdoc cref="actionType" path="//summary"/></param>
        /// <param name="keys"><inheritdoc cref="Keys" path="//summary"/></param>
        /// <exception cref="ArgumentException"></exception>
        public AActionKey(T actionType, Key response, IEnumerable<ConsoleKeyInfo> keys, List<GetKeyMode> ignoreModes)
            : base(response, keys, ignoreModes)
        {
            if (!keys.Any())
            {
                Logger.Instance.Log("No keys in keys list!", severity: LogSeverity.FATAL);
                throw new ArgumentException("No keys in keys list!", nameof(keys));
            }
            ActionType = actionType;
            Conflicts = Keys.Select(k => false).ToList();
            Keys = keys;
        }
        #endregion

        #region Public methods
        public void UpdateNames()
        {
            Names = new List<string>();
            foreach (var key in Keys)
            {
                Names.Add(KeybindUtils.GetKeyName(key));
            }
        }

        public bool IsKey()
        {
            return Keys.Contains(Console.ReadKey(true));
        }
        #endregion
    }
}

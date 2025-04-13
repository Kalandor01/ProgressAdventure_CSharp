using ConsoleUI.Keybinds;
using PACommon.Enums;

namespace PACommon.SettingsManagement
{
    /// <summary>
    /// Class for storing a key for keybinds.
    /// </summary>
    /// <typeparam name="T">The enum to use, for denoting the action type of this object.</typeparam>
    public abstract class AActionKey<T> : KeyAction
        where T : notnull
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
                Conflicts = [.. Keys.Select(k => false)];
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
        /// <param name="actionType"><inheritdoc cref="ActionType" path="//summary"/></param>
        /// <param name="response"><inheritdoc cref="KeyAction.response" path="//summary"/></param>
        /// <param name="keys"><inheritdoc cref="Keys" path="//summary"/></param>
        /// <param name="ignoreModes"><inheritdoc cref="KeyAction.ignoreModes" path="//summary"/></param>
        /// <exception cref="ArgumentException"></exception>
        public AActionKey(T actionType, object response, IEnumerable<ConsoleKeyInfo> keys, List<GetKeyMode> ignoreModes)
            : base(response, keys, ignoreModes)
        {
            if (!keys.Any())
            {
                PACSingletons.Instance.Logger.Log("No keys in keys list!", severity: LogSeverity.FATAL);
                throw new ArgumentException("No keys in keys list!", nameof(keys));
            }
            ActionType = actionType;
            Conflicts = [.. Keys.Select(k => false)];
            Keys = keys;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Updates the names of the keys.
        /// </summary>
        public void UpdateNames()
        {
            Names = [];
            foreach (var key in Keys)
            {
                Names.Add(KeybindUtils.GetKeyName(key));
            }
        }

        /// <summary>
        /// Gets a key from the user, and then checks, if that keys maches this action.
        /// </summary>
        public bool IsKey()
        {
            return IsKey(Console.ReadKey(true));
        }

        /// <summary>
        /// Checks, if a keys maches this action.
        /// </summary>
        /// <param name="key">The key to check.</param>
        public bool IsKey(ConsoleKeyInfo key)
        {
            return Keys.Contains(key);
        }
        #endregion
    }
}

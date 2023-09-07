using PACommon.Enums;
using SaveFileManager;

namespace PACommon.SettingsManagement
{
    /// <summary>
    /// Class for storing the keybinds list.
    /// </summary>
    public abstract class AKeybinds<T, TA>
        where T : notnull, Enum
        where TA : AActionKey<T>
    {
        #region Private fields
        /// <summary>
        /// The list of actions(/keybinds).
        /// </summary>
        protected IEnumerable<TA> _keybinds;
        #endregion

        #region Public properties
        /// <summary>
        /// <inheritdoc cref="_keybinds" path="//summary"/>
        /// </summary>
        public IEnumerable<TA> KeybindList
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
        /// <inheritdoc cref="AKeybinds{T, TA}"/>
        /// </summary>
        /// <param name="actions"><inheritdoc cref="KeybindList" path="//summary"/></param>
        public AKeybinds(IEnumerable<TA> actions)
        {
            KeybindList = actions;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Returns the <c>ActionKey</c> by type.
        /// </summary>
        /// <param name="actionType">The action type.</param>
        public TA? GetActionKey(T actionType)
        {
            foreach (var key in KeybindList)
            {
                if (key.ActionType.Equals(actionType))
                {
                    return key;
                }
            }
            return null;
        }

        /// <summary>
        /// Replaces missing keybinds in the keybinds list.
        /// </summary>
        public abstract void FillAllKeybinds();

        /// <summary>
        /// Marks all of the keys, that are the same, as another key.
        /// </summary>
        public void UpdateKeybindConflicts()
        {
            foreach (var keybind in KeybindList)
            {
                keybind.Conflicts = keybind.Keys.Select(k => false).ToList();
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
                                    action1.Conflicts[x] = true;
                                    action2.Conflicts[y] = true;
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
            var kbObj = (AKeybinds<T, TA>)obj;
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

        public override int GetHashCode()
        {
            return KeybindList.GetHashCode();
        }

        public override string ToString()
        {
            return string.Join("\n", KeybindList);
        }
        #endregion
    }
}

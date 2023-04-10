using ProjectAdventure.Enums;

namespace ProjectAdventure.Settings
{
    /// <summary>
    /// Class for storing the keybinds list.
    /// </summary>
    public class Keybinds
    {
        #region Public fields
        /// <summary>
        /// The list of actions.
        /// </summary>
        public IEnumerable<ActionKey> keybinds;
        #endregion

        #region Constructors
        /// <summary>
        /// <inheritdoc cref="Keybinds"/>
        /// </summary>
        /// <param name="actions"><inheritdoc cref="keybinds" path="//summary"/></param>
        /// <exception cref="ArgumentException"></exception>
        public Keybinds(IEnumerable<ActionKey> actions)
        {
            if (!actions.Any())
            {
                Logger.Log("No actions in actions list.", severity: LogSeverity.FATAL);
                throw new ArgumentException("No actions in actions list.", nameof(actions));
            }
            this.keybinds = actions;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Returns the <c>ActionKey</c> by type.
        /// </summary>
        /// <param name="actionType">The action type.</param>
        public ActionKey GetActionKey(ActionType actionType)
        {
            foreach (var key in keybinds)
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
        /// Turns the <c>Keybinds</c> objest into a json object for the settings file.
        /// </summary>
        public IDictionary<string, IEnumerable<IDictionary<string, object>>> ToJson()
        {
            var keybindsJson = new Dictionary<string, IEnumerable<IDictionary<string, object>>>();
            foreach (var keybind in keybinds)
            {
                (string key, IEnumerable<IDictionary<string, object>> value) = keybind.ToJson();
                keybindsJson.Add(key, value);
            }
            return keybindsJson;
        }
        #endregion
    }
}

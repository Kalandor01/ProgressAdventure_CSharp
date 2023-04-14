using Newtonsoft.Json.Linq;
using ProgressAdventure.Enums;

namespace ProgressAdventure.Settings
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

        #region Public functions
        /// <summary>
        /// Turns json representing a <c>Keybinds</c> object into a <c>Keybinds</c> object.
        /// </summary>
        /// <param name="keybindsJson">The json representation of the <c>Keybinds</c> object.</param>
        public static Keybinds KeybindsFromJson(JToken keybindsJson)
        {
            var actions = new List<ActionKey>();
            foreach (var actionJson in keybindsJson)
            {
                if (
                    actionJson.Type == JTokenType.Property &&
                    Enum.TryParse(typeof(ActionType), ((JProperty)actionJson).Name, out object? res)
                    )
                {
                    var actionType = (ActionType)res;
                    var keys = new List<ConsoleKeyInfo>();
                    foreach (var key in ((JProperty)actionJson).Value)
                    {
                        if (
                            Enum.TryParse(typeof(ConsoleKey), Utils.GetJTokenValue(key, "key"), out object? keyEnum) &&
                            char.TryParse(Utils.GetJTokenValue(key, "keyChar"), out char keyChar) &&
                            int.TryParse(Utils.GetJTokenValue(key, "modifiers"), out int keyMods)
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

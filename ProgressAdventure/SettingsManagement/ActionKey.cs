using ProgressAdventure.Enums;
using SaveFileManager;
using System.Collections;

namespace ProgressAdventure.SettingsManagement
{
    /// <summary>
    /// Class for storing a key for keybinds.
    /// </summary>
    public class ActionKey : KeyAction, IJsonConvertable<ActionKey>
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
#pragma warning disable CS0108 // hiding was intended
        public IEnumerable<ConsoleKeyInfo> Keys
#pragma warning restore CS0108 // hiding was intended
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
                  SettingsUtils.actionTypeResponseMapping[actionType],
                  keys,
                  SettingsUtils.actionTypeIgnoreMapping[actionType]
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
            Name = SettingsUtils.GetKeyName(Keys.ElementAt(0));
        }

        /// <summary>
        /// Waits for a keypress, and returns, if it matches the <c>ActionKey</c>.
        /// </summary>
        public bool IsKey()
        {
            return Keys.Contains(Console.ReadKey(true));
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
            var akObj = (ActionKey)obj;
            if (actionType != akObj.actionType || !response.Equals(akObj.response))
            {
                return false;
            }
            if (Keys.Count() != akObj.Keys.Count() || ignoreModes.Count() != akObj.ignoreModes.Count())
            {
                return false;
            }
            for (var x = 0; x < Keys.Count(); x++)
            {
                if (!Keys.ElementAt(x).Equals(akObj.Keys.ElementAt(x)))
                {
                    return false;
                }
            }
            for (var x = 0; x < ignoreModes.Count(); x++)
            {
                if (!ignoreModes.ElementAt(x).Equals(akObj.ignoreModes.ElementAt(x)))
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
            var keyListJson = new List<Dictionary<string, object>>();
            foreach (var key in Keys)
            {
                var keyJson = new Dictionary<string, object>()
                {
                    ["key"] = (int)key.Key,
                    ["keyChar"] = key.KeyChar,
                    ["modifiers"] = (int)key.Modifiers
                };
                keyListJson.Add(keyJson);
            }
            return new Dictionary<string, object?> { [actionType.ToString()] = keyListJson };
        }

        public static ActionKey? FromJson(IDictionary<string, object?>? actionKeyJson, string fileVersion)
        {
            if (
                actionKeyJson is null ||
                !actionKeyJson.Any()
            )
            {
                Logger.Log("Action key parse error", "action key json is null", LogSeverity.WARN);
                return null;
            }

            var actionJson = actionKeyJson.First();
            if (
                Enum.TryParse(actionJson.Key, out ActionType actionType) &&
                Enum.IsDefined(actionType) &&
                actionJson.Value is not null
            )
            {
                var keys = new List<ConsoleKeyInfo>();
                foreach (var actionKey in (IEnumerable)actionJson.Value)
                {
                    var actionDict = (IDictionary<string, object>)actionKey;
                    if (
                        Enum.TryParse(actionDict.TryGetValue("key", out var keyValue) ? keyValue.ToString() : null, out ConsoleKey keyEnum) &&
                        Enum.IsDefined(keyEnum) &&
                        char.TryParse(actionDict.TryGetValue("keyChar", out var charValue) ? charValue.ToString() : null, out char keyChar) &&
                        int.TryParse(actionDict.TryGetValue("modifiers", out var modValue) ? modValue.ToString() : null, out int keyMods)
                        )
                    {
                        var alt = Utils.GetBit(keyMods, 0);
                        var shift = Utils.GetBit(keyMods, 1);
                        var ctrl = Utils.GetBit(keyMods, 2);
                        keys.Add(new ConsoleKeyInfo(keyChar, keyEnum, shift, alt, ctrl));
                    }
                    else
                    {
                        Logger.Log("Action key parse error", $"couldn't parse key from action key json, action type: {actionJson.Key}", LogSeverity.WARN);
                    }
                }
                return new ActionKey(actionType, keys);
            }
            else
            {
                Logger.Log("Action key parse error", $"couldn't parse action from action key json, action type: {actionJson.Key}", LogSeverity.WARN);
                return null;
            }
        }
        #endregion
    }
}

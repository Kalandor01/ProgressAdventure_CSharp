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
        /// A list indicating if a key in a list conflicts with another key in the keybinds.
        /// </summary>
        public List<bool> conflicts;
        #endregion

        #region Public properties
        /// <summary>
        /// The keys that can be pressed to trigger this action.
        /// </summary>
#pragma warning disable CS0108 // Hiding was intended
        public IEnumerable<ConsoleKeyInfo> Keys
#pragma warning restore CS0108 // Hiding was intended
        {
            get => base.Keys;
            set {
                base.Keys = value.Distinct();
                UpdateNames();
                conflicts = Keys.Select(k => false).ToList();
            }
        }

        /// <summary>
        /// The display names of the keys.
        /// </summary>
        public List<string> Names { get; private set; }
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
            conflicts = Keys.Select(k => false).ToList();
            Keys = keys;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Updates the display name of the key.
        /// </summary>
        public void UpdateNames()
        {
            Names = new List<string>();
            foreach (var key in Keys)
            {
                Names.Add(SettingsUtils.GetKeyName(key));
            }
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

        public override int GetHashCode()
        {
            return (actionType, Keys, ignoreModes).GetHashCode();
        }

        public override string ToString()
        {
            return actionType.ToString() + ": " + string.Join(", ", Names);
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
                    ["key_char"] = key.KeyChar,
                    ["modifiers"] = (int)key.Modifiers
                };
                keyListJson.Add(keyJson);
            }
            return new Dictionary<string, object?> { [actionType.ToString().ToLower()] = keyListJson };
        }

        public static bool FromJson(IDictionary<string, object?>? actionKeyJson, string fileVersion, out ActionKey? actionKeyObject)
        {
            if (
                actionKeyJson is null ||
                !actionKeyJson.Any()
            )
            {
                Logger.Log("Action key parse error", "action key json is null", LogSeverity.WARN);
                actionKeyObject = null;
                return false;
            }

            //correct data
            if (!Tools.IsUpToDate(Constants.SAVE_VERSION, fileVersion))
            {
                Logger.Log($"Action key json data is old", "correcting data");
                // 2.1.1 -> 2.2
                var newFileVersion = "2.2";
                if (!Tools.IsUpToDate(newFileVersion, fileVersion))
                {
                    // item material
                    if (actionKeyJson.TryGetValue("keyChar", out var kcRename))
                    {
                        actionKeyJson["key_char"] = kcRename;
                    }

                    Logger.Log("Corrected action key json data", $"{fileVersion} -> {newFileVersion}", LogSeverity.DEBUG);
                    fileVersion = newFileVersion;
                }
                Logger.Log($"Action key json data corrected");
            }

            var actionJson = actionKeyJson.First();
            if (
                Enum.TryParse(actionJson.Key.ToUpper(), out ActionType actionType) &&
                Enum.IsDefined(actionType) &&
                actionJson.Value is IEnumerable actionKeyList
            )
            {
                var success = true;
                var keys = new List<ConsoleKeyInfo>();
                foreach (var actionKey in actionKeyList)
                {
                    var actionDict = actionKey as IDictionary<string, object>;
                    if (
                        actionDict is not null &&
                        Enum.TryParse(actionDict.TryGetValue("key", out var keyValue) ? keyValue.ToString() : null, out ConsoleKey keyEnum) &&
                        Enum.IsDefined(keyEnum) &&
                        char.TryParse(actionDict.TryGetValue("key_char", out var charValue) ? charValue.ToString() : null, out char keyChar) &&
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
                        success = false;
                    }
                }
                actionKeyObject = new ActionKey(actionType, keys);
                return success;
            }
            else
            {
                Logger.Log("Action key parse error", $"couldn't parse action from action key json, action type: {actionJson.Key}", LogSeverity.WARN);
                actionKeyObject = null;
                return false;
            }
        }
        #endregion
    }
}

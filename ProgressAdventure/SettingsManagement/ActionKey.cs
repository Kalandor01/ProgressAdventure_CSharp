using PACommon;
using PACommon.Enums;
using PACommon.JsonUtils;
using PACommon.SettingsManagement;
using ProgressAdventure.Enums;
using System.Collections;
using Utils = PACommon.Utils;

namespace ProgressAdventure.SettingsManagement
{
    /// <summary>
    /// Class for storing a key for keybinds.
    /// </summary>
    public class ActionKey : AActionKey<ActionType>, IJsonConvertable<ActionKey>
    {
        #region Constructors
        /// <summary>
        /// <inheritdoc cref="ActionKey"/>
        /// </summary>
        /// <param name="actionType"><inheritdoc cref="actionType" path="//summary"/></param>
        /// <param name="keys"><inheritdoc cref="Keys" path="//summary"/></param>
        /// <exception cref="ArgumentException"></exception>
        public ActionKey(ActionType actionType, IEnumerable<ConsoleKeyInfo> keys)
            : base(
                  actionType,
                  SettingsUtils.actionTypeResponseMapping[actionType],
                  keys,
                  SettingsUtils.actionTypeIgnoreMapping[actionType]
            )
        { }
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
            if (ActionType != akObj.ActionType || !response.Equals(akObj.response))
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
            return (ActionType, Keys, ignoreModes).GetHashCode();
        }

        public override string ToString()
        {
            return ActionType.ToString() + ": " + string.Join(", ", Names);
        }
        #endregion

        #region JsonConvert
        static List<(Action<IDictionary<string, object?>> objectJsonCorrecter, string newFileVersion)> IJsonConvertable<ActionKey>.VersionCorrecters { get; } = new()
        {
            // 2.1.1 -> 2.2
            (oldJson =>
            {
                // key rename
                if (oldJson.TryGetValue("keyChar", out var kcRename))
                {
                    oldJson["key_char"] = kcRename;
                }
            }, "2.2"),
        };

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
            return new Dictionary<string, object?> { [ActionType.ToString().ToLower()] = keyListJson };
        }

        static bool IJsonConvertable<ActionKey>.FromJsonWithoutCorrection(IDictionary<string, object?> actionKeyJson, string fileVersion, ref ActionKey? actionKeyObject)
        {
            if (!actionKeyJson.Any())
            {
                Logger.Instance.Log("Action key parse error", "action key json is null", LogSeverity.WARN);
                return false;
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
                        Logger.Instance.Log("Action key parse error", $"couldn't parse key from action key json, action type: {actionJson.Key}", LogSeverity.WARN);
                        success = false;
                    }
                }
                actionKeyObject = new ActionKey(actionType, keys);
                return success;
            }
            else
            {
                Logger.Instance.Log("Action key parse error", $"couldn't parse action from action key json, action type: {actionJson.Key}", LogSeverity.WARN);
                return false;
            }
        }
        #endregion
    }
}

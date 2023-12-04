using PACommon.JsonUtils;
using PACommon.SettingsManagement;
using ProgressAdventure.Enums;
using System.Diagnostics.CodeAnalysis;
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
                    [Constants.JsonKeys.ActionKey.KEY] = (int)key.Key,
                    [Constants.JsonKeys.ActionKey.KEY_CHAR] = key.KeyChar,
                    [Constants.JsonKeys.ActionKey.MODIFIERS] = (int)key.Modifiers
                };
                keyListJson.Add(keyJson);
            }
            return new Dictionary<string, object?> { [ActionType.ToString().ToLower()] = keyListJson };
        }

        static bool IJsonConvertable<ActionKey>.FromJsonWithoutCorrection(IDictionary<string, object?> actionKeyJson, string fileVersion, [NotNullWhen(true)] ref ActionKey? actionKeyObject)
        {
            if (!actionKeyJson.Any())
            {
                Tools.LogJsonError<ActionKey>($"{nameof(actionKeyJson)} is empty", true);
                return false;
            }

            var actionJson = actionKeyJson.First();

            if (!(
                Tools.TryParseValueForJsonParsing<ActionKey, ActionType>(
                    actionJson.Key.ToUpper(),
                    out var actionType,
                    parameterName: nameof(actionJson),
                    parameterExtraInfo: $"action type: {actionJson.Key}",
                    isCritical: true
                ) &&
                Tools.TryCastAnyValueForJsonParsing<ActionKey, IEnumerable<object?>>(actionJson.Value, out var actionKeyList, nameof(actionJson) + " value", true)
            ))
            {
                return false;
            }

            var success = true;
            success = Tools.TryParseListValueForJsonParsing<ActionKey, ConsoleKeyInfo>(actionKeyList, nameof(actionKeyList), actionKeyJsonValue => {
                if (
                    Tools.TryCastAnyValueForJsonParsing<ActionKey, IDictionary<string, object?>>(actionKeyJsonValue, out var actionKeyJson, nameof(actionKeyJsonValue)) &&
                    Tools.TryParseJsonValue<ActionKey, ConsoleKey>(actionKeyJson, Constants.JsonKeys.ActionKey.KEY, out var keyEnum) &&
                    Tools.TryParseJsonValue<ActionKey, char>(actionKeyJson, Constants.JsonKeys.ActionKey.KEY_CHAR, out var keyChar) &&
                    Tools.TryParseJsonValue<ActionKey, int>(actionKeyJson, Constants.JsonKeys.ActionKey.MODIFIERS, out var keyModifiers)
                    )
                {
                    var alt = Utils.GetBit(keyModifiers, 0);
                    var shift = Utils.GetBit(keyModifiers, 1);
                    var ctrl = Utils.GetBit(keyModifiers, 2);
                    return (true, new ConsoleKeyInfo(keyChar, keyEnum, shift, alt, ctrl));
                }
                success = false;
                return (false, default);
            }, out var keys);

            actionKeyObject = new ActionKey(actionType, keys);
            return success;
        }
        #endregion
    }
}

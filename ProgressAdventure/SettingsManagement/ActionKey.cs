﻿using PACommon.Enums;
using PACommon.JsonUtils;
using PACommon.SettingsManagement;
using ProgressAdventure.Enums;
using System.Diagnostics.CodeAnalysis;
using PACTools = PACommon.Tools;
using Utils = PACommon.Utils;

namespace ProgressAdventure.SettingsManagement
{
    /// <summary>
    /// Class for storing a key for keybinds.
    /// </summary>
    public class ActionKey : AActionKey<EnumValue<ActionType>>, IJsonConvertable<ActionKey>
    {
        #region Constructors
        /// <summary>
        /// <inheritdoc cref="ActionKey"/>
        /// </summary>
        /// <param name="actionType"><inheritdoc cref="AActionKey{T}.ActionType" path="//summary"/></param>
        /// <param name="keys"><inheritdoc cref="AActionKey{T}.Keys" path="//summary"/></param>
        /// <exception cref="ArgumentException"></exception>
        public ActionKey(EnumValue<ActionType> actionType, IEnumerable<ConsoleKeyInfo> keys)
            : base(
                  actionType,
                  SettingsUtils.ActionTypeAttributes[actionType].response,
                  keys,
                  SettingsUtils.ActionTypeAttributes[actionType].ignoreModes
            )
        { }
        #endregion

        #region Public overrides
        public override bool Equals(object? obj)
        {
            if (obj is not ActionKey akObj)
            {
                return false;
            }
            if (base.Equals(obj))
            {
                return true;
            }
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
        static List<(Action<JsonDictionary> objectJsonCorrecter, string newFileVersion)> IJsonConvertable<ActionKey>.VersionCorrecters { get; } =
        [
            // 2.1.1 -> 2.2
            (oldJson =>
            {
                // key rename
                JsonDataCorrecterUtils.RenameKeyIfExists(oldJson, "keyChar", "key_char");
            }, "2.2"),
        ];

        public JsonDictionary ToJson()
        {
            var keyListJson = new JsonArray();
            foreach (var key in Keys)
            {
                var keyJson = new JsonDictionary()
                {
                    [Constants.JsonKeys.ActionKey.KEY] = (int)key.Key,
                    [Constants.JsonKeys.ActionKey.KEY_CHAR] = key.KeyChar,
                    [Constants.JsonKeys.ActionKey.MODIFIERS] = (int)key.Modifiers,
                };
                keyListJson.Add(keyJson);
            }
            return new JsonDictionary { [ActionType.Name] = keyListJson };
        }

        static bool IJsonConvertable<ActionKey>.FromJsonWithoutCorrection(JsonDictionary actionKeyJson, string fileVersion, [NotNullWhen(true)] ref ActionKey? actionKeyObject)
        {
            if (actionKeyJson.Count == 0)
            {
                PACTools.LogJsonError($"{nameof(actionKeyJson)} is empty", true);
                return false;
            }

            var actionJson = actionKeyJson.First();

            if (!(
                PACTools.TryParseValueForJsonParsing<EnumValue<ActionType>>(
                    new JsonValue(actionJson.Key),
                    out var actionType,
                    parameterExtraInfo: $"action type: {actionJson.Key}",
                    isCritical: true
                ) &&
                PACTools.TryCastAnyValueForJsonParsing<JsonArray>(
                    actionJson.Value, out var actionKeyList, isCritical: true, isStraigthCast: true
                )
            ))
            {
                return false;
            }

            var success = true;
            var allSuccess = PACTools.TryParseListValueForJsonParsing(actionKeyList, nameof(actionKeyList), actionKeyJsonValue =>
            {
                if (
                    PACTools.TryCastAnyValueForJsonParsing<JsonDictionary>(
                        actionKeyJsonValue, out var actionKeyJson, isStraigthCast: true
                    ) &&
                    PACTools.TryParseJsonValue<ConsoleKey>(actionKeyJson, Constants.JsonKeys.ActionKey.KEY, out var keyEnum) &&
                    PACTools.TryParseJsonValue<char>(actionKeyJson, Constants.JsonKeys.ActionKey.KEY_CHAR, out var keyChar) &&
                    PACTools.TryParseJsonValue<int>(actionKeyJson, Constants.JsonKeys.ActionKey.MODIFIERS, out var keyModifiers)
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
            return success && allSuccess;
        }
        #endregion
    }
}

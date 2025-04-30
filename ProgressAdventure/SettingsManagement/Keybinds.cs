using PACommon;
using PACommon.Enums;
using PACommon.JsonUtils;
using PACommon.SettingsManagement;
using ProgressAdventure.Enums;
using ProgressAdventure.Exceptions;
using System.Diagnostics.CodeAnalysis;
using PACTools = PACommon.Tools;

namespace ProgressAdventure.SettingsManagement
{
    /// <summary>
    /// Class for storing the keybinds list.
    /// </summary>
    public class Keybinds : AKeybinds<EnumValue<ActionType>, ActionKey>, IJsonConvertable<Keybinds>
    {
        #region Constructors
        /// <summary>
        /// <inheritdoc cref="Keybinds"/>
        /// </summary>
        /// <param name="actions"><inheritdoc cref="AKeybinds{T, TA}.KeybindList" path="//summary"/></param>
        public Keybinds(IEnumerable<ActionKey>? actions)
            : base(actions ?? SettingsUtils.GetDefaultKeybindList())
        {
            if (actions is null)
            {
                PACSingletons.Instance.Logger.Log("No actions in actions list.", "Recreating key actions from defaults", LogSeverity.ERROR);
            }
        }

        public Keybinds()
            : this(SettingsUtils.GetDefaultKeybindList()) { }
        #endregion

        #region Public methods
        public override void FillAllKeybinds()
        {
            var defKebindList = SettingsUtils.GetDefaultKeybindList();
            if (!KeybindList.Any())
            {
                PACSingletons.Instance.Logger.Log("No actions in actions list.", "Recreating key actions from defaults", LogSeverity.ERROR);
                if (defKebindList.Count == 0)
                {
                    PACSingletons.Instance.Logger.Log("No actions in default actions list!", severity: LogSeverity.FATAL);
                    throw new NotEnoughKeybindsException("No actions in default actions list!");
                }
                KeybindList = defKebindList;
                return;
            }
            foreach (var defKeybind in defKebindList)
            {
                if(!KeybindList.Any(key => key.ActionType == defKeybind.ActionType))
                {
                    PACSingletons.Instance.Logger.Log("Missing action key in keybinds list", $"action type: {defKeybind.ActionType}", LogSeverity.WARN);
                    _keybinds = KeybindList.Concat([defKeybind]);
                }
            }
        }
        #endregion

        #region JsonConvert
        public JsonDictionary ToJson()
        {
            var keybindsJson = new JsonDictionary();
            foreach (var keybind in KeybindList)
            {
                var kbJson = keybind.ToJson().First();
                keybindsJson.Add(kbJson.Key, kbJson.Value);
            }
            return keybindsJson;
        }

        static bool IJsonConvertable<Keybinds>.FromJsonWithoutCorrection(JsonDictionary keybindsJson, string fileVersion, [NotNullWhen(true)] ref Keybinds? keybinds)
        {
            var success = true;
            var allSuccess = PACTools.TryParseListValueForJsonParsing(keybindsJson, nameof(keybindsJson), keybind =>
            {
                success &= PACTools.TryFromJson(
                    new JsonDictionary { [keybind.Key] = keybind.Value },
                    fileVersion,
                    out ActionKey? actionKey
                );
                return (actionKey is not null, actionKey);
            }, out var actions);

            var defKb = SettingsUtils.GetDefaultKeybindList();
            if (
                actions.Count < defKb.Count ||
                !defKb.All(kb => actions.Any(action => action.ActionType == kb.ActionType))
            )
            {
                return false;
            }
            keybinds = new Keybinds(actions);
            return success && allSuccess;
        }
        #endregion
    }
}

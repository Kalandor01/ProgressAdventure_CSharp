using PACommon;
using PACommon.Enums;
using PACommon.JsonUtils;
using PACommon.SettingsManagement;
using ProgressAdventure.Enums;
using PACTools = PACommon.Tools;

namespace ProgressAdventure.SettingsManagement
{
    /// <summary>
    /// Class for storing the keybinds list.
    /// </summary>
    public class Keybinds : AKeybinds<ActionType, ActionKey>, IJsonConvertable<Keybinds>
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
            if (!KeybindList.Any())
            {
                PACSingletons.Instance.Logger.Log("No actions in actions list.", "Recreating key actions from defaults", LogSeverity.ERROR);
                KeybindList = SettingsUtils.GetDefaultKeybindList();
            }
        }
        #endregion

        #region JsonConvert
        public Dictionary<string, object?> ToJson()
        {
            var keybindsJson = new Dictionary<string, object?>();
            foreach (var keybind in KeybindList)
            {
                var kbJson = keybind.ToJson().First();
                keybindsJson.Add(kbJson.Key.ToLower(), kbJson.Value);
            }
            return keybindsJson;
        }

        static bool IJsonConvertable<Keybinds>.FromJsonWithoutCorrection(IDictionary<string, object?> keybindsJson, string fileVersion, ref Keybinds? keybinds)
        {
            var success = true;
            var actions = new List<ActionKey>();
            foreach (var actionJson in keybindsJson)
            {
                success &= PACTools.TryFromJson(
                    new Dictionary<string, object?> { [actionJson.Key] = actionJson.Value },
                    fileVersion,
                    out ActionKey? actionKey
                );
                if (actionKey is not null)
                {
                    actions.Add(actionKey);
                }
            }
            keybinds = new Keybinds(actions);
            return success;
        }
        #endregion
    }
}

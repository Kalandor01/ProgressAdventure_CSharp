using ConsoleUI.Keybinds;
using System.Text.Json.Serialization;

namespace ProgressAdventure.SettingsManagement
{
    /// <summary>
    /// DTO used for storing the attributes of an action type.
    /// </summary>
    public class ActionTypeAttributesDTO
    {
        #region Fields
        /// <summary>
        /// The value that will be used to identify this action type.
        /// </summary>
        [JsonPropertyName("response")]
        public readonly string response;
        /// <summary>
        /// The display name of this action type.
        /// </summary>
        [JsonPropertyName("display_name")]
        public readonly string displayName;
        /// <summary>
        /// The ignore modes that will disable checking for this action type.
        /// </summary>
        [JsonPropertyName("ignore_modes")]
        public readonly List<GetKeyMode> ignoreModes;
        /// <summary>
        /// The default value for the keys that can be pressed to trigger this action type.
        /// </summary>
        [JsonPropertyName("default_keys")]
        public readonly List<ConsoleKeyInfo> defaultKeys;
        #endregion

        #region Public constructors
        /// <summary>
        /// <inheritdoc cref="ActionTypeAttributesDTO"/>
        /// </summary>
        /// <param name="response"><inheritdoc cref="response" path="//summary"/></param>
        /// <param name="displayName"><inheritdoc cref="response" path="//summary"/></param>
        /// <param name="ignoreModes"><inheritdoc cref="ignoreModes" path="//summary"/></param>
        /// <param name="defaultKeys"><inheritdoc cref="defaultKeys" path="//summary"/></param>
        [JsonConstructor]
        public ActionTypeAttributesDTO(string response, string displayName, List<GetKeyMode> ignoreModes, List<ConsoleKeyInfo> defaultKeys)
        {
            this.response = response;
            this.displayName = displayName;
            this.ignoreModes = ignoreModes;
            this.defaultKeys = defaultKeys;
        }
        #endregion

        #region Overrides
        public override string? ToString()
        {
            return $"\"{displayName}\" ({response}){(ignoreModes.Count == 0 ? "" : $" ({string.Join(", ", ignoreModes)})")}, {string.Join(", ", defaultKeys.Select(k => k.Key))}";
        }
        #endregion
    }
}

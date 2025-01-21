using PACommon.SettingsManagement;

namespace ProgressAdventure.Exceptions
{
    /// <summary>
    /// Represents an error that ocured because there aren't enough keybinds in the default <see cref="AKeybinds{T, TA}.KeybindList"/> list.
    /// </summary>
    public class NotEnoughKeybindsException : Exception
    {
        public NotEnoughKeybindsException(string message)
            : base(message) { }
    }
}

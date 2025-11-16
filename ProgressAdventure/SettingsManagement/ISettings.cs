using PACommon.Enums;
using ProgressAdventure.Enums;

namespace ProgressAdventure.SettingsManagement
{
    /// <summary>
    /// Interface for managing the data in the settings file.
    /// </summary>
    public interface ISettings : IDisposable
    {
        #region Public properties
        /// <summary>
        /// If the game should auto save.
        /// </summary>
        public bool AutoSave { get; set; }

        /// <summary>
        /// The minimum level of logs, that will be recorded.
        /// </summary>
        public LogSeverity LoggingLevel { get; set; }

        /// <summary>
        /// The keybinds object to use, for the app.
        /// </summary>
        public Keybinds Keybinds { get; set; }

        /// <summary>
        /// If the user should be asked for confirmation, when trying to delete a save file.
        /// </summary>
        public bool AskDeleteSave { get; set; }

        /// <summary>
        /// If the user should be asked for confirmation, when trying to regenerate a save file.
        /// </summary>
        public bool AskRegenerateSave { get; set; }

        /// <summary>
        /// The default action for backing up save files.<br/>
        /// -1: ask user<br/>
        /// 0: never backup<br/>
        /// 1: always backup
        /// </summary>
        public int DefBackupAction { get; set; }

        /// <summary>
        /// Whether to enable colored text on the terminal.
        /// </summary>
        public bool EnableColoredText { get; set; }

        /// <summary>
        /// The currently selected language.
        /// </summary>
        public EnumValue<Language> CurrentLanguage { get; set; }
        #endregion

        #region Public functions
        /// <summary>
        /// Returns the value of the <see cref="AutoSave"/> from the setting file.
        /// </summary>
        public bool GetAutoSave();

        /// <summary>
        /// Returns the value of the <see cref="LoggingLevel"/> from the setting file.
        /// </summary>
        public LogSeverity GetLoggingLevel();

        /// <summary>
        /// Returns the value of the <see cref="LoggingLevel"/>> from the setting file.
        /// </summary>
        public Keybinds GetKeybins();

        /// <summary>
        /// Returns the value of the <see cref="AskDeleteSave"/> from the setting file.
        /// </summary>
        public bool GetAskDeleteSave();

        /// <summary>
        /// Returns the value of the <see cref="AskRegenerateSave"/> from the setting file.
        /// </summary>
        public bool GetAskRegenerateSave();

        /// <summary>
        /// Returns the value of the <see cref="DefBackupAction"/> from the setting file.
        /// </summary>
        public int GetDefBackupAction();

        /// <summary>
        /// Returns the value of the <see cref="EnableColoredText"/> from the setting file.
        /// </summary>
        public bool GetEnableColoredText();

        /// <summary>
        /// Returns the value of the <see cref="CurrentLanguage"/> from the setting file.
        /// </summary>
        public EnumValue<Language> GetCurrentLanguage();
        #endregion
    }
}

using Lua;
using PACommon.Enums;

namespace PACommon.EmbededLua
{
    /// <summary>
    /// Contains functions for logging.
    /// </summary>
    [LuaObject]
    public partial class LuaLogger
    {
        #region Public properties
        [LuaMember("default_write_out")]
        public bool DefaultWriteOut
        {
            get => PACSingletons.Instance.Logger.DefaultWriteOut;
            set => PACSingletons.Instance.Logger.DefaultWriteOut = value;
        }

        [LuaMember("log_ms")]
        public bool LogMS
        {
            get => PACSingletons.Instance.Logger.LogMS;
            set => PACSingletons.Instance.Logger.LogMS = value;
        }

        [LuaMember("logging_enabled")]
        public bool LoggingEnabled => PACSingletons.Instance.Logger.LoggingEnabled;

        [LuaMember("logging_level")]
        public int LoggingLevel
        {
            get => (int)PACSingletons.Instance.Logger.LoggingLevel;
            set => PACSingletons.Instance.Logger.LoggingLevel = (LogSeverity)value;
        }

        // [LuaMember("force_log_lnterval")]
        // public TimeSpan ForceLogInterval
        // {
        //     get => PACSingletons.Instance.Logger.ForceLogInterval;
        //     set => PACSingletons.Instance.Logger.ForceLogInterval = value;
        // }
        #endregion

        #region Public methods
        [LuaMember("log")]
        public async void Log(
            string message,
            string? details = "",
            int severity = 2,
            bool? writeOut = null,
            bool newLine = false,
            bool forceLog = false
        )
        {
            PACSingletons.Instance.Logger.Log(message, details, (LogSeverity)severity, writeOut, newLine, forceLog);
        }

        [LuaMember("log_async")]
        public void LogAsync(
            string message,
            string? details = "",
            int severity = 2,
            bool? writeOut = null,
            bool newLine = false,
            bool forceLog = false
        )
        {
            PACSingletons.Instance.Logger.LogAsync(message, details, (LogSeverity)severity, writeOut, newLine, forceLog);
        }

        [LuaMember("log_new_line")]
        public async void LogNewLine()
        {
            PACSingletons.Instance.Logger.LogNewLine();
        }
        #endregion
    }
}

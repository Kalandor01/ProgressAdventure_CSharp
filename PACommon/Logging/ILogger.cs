using PACommon.Enums;

namespace PACommon.Logging
{
    /// <summary>
    /// Interface for logging.
    /// </summary>
    public interface ILogger : IDisposable
    {
        #region Public properties
        /// <summary>
        /// The default value for is the logs should be writen out to the console.
        /// </summary>
        public bool DefaultWriteOut { get; set; }

        /// <summary>
        /// If the logger should log the milisecond in time.
        /// </summary>
        public bool LogMS { get; set; }

        /// <summary>
        /// If logging is enabled.
        /// </summary>
        public bool LoggingEnabled { get; }

        /// <summary>
        /// The current logging level.
        /// </summary>
        public LogSeverity LoggingLevel { get; set; }

        /// <summary>
        /// The minimum time between, where normal logging just stores the logs to a buffer for later logging.
        /// </summary>
        public TimeSpan ForceLogInterval { get; set; }
        #endregion

        #region Public functions
        /// <summary>
        /// Tries to turn the int value of the log severity into a <c>LogSeverity</c> enum, and returns the success.
        /// </summary>
        /// <param name="severityValue">The log sevrity's int representation.</param>
        /// <param name="severity">The sevrity, that got parsed, or created.</param>
        public static bool TryParseSeverityValue(int severityValue, out LogSeverity severity)
        {
            foreach (var loggingValue in Enum.GetValues<LogSeverity>())
            {
                if ((int)loggingValue == severityValue)
                {
                    severity = loggingValue;
                    return true;
                }
            }

            severity = LogSeverity.DEBUG;
            return false;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Progress Adventure logger.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="details">The details of the message.</param>
        /// <param name="severity">The severity of the message.</param>
        /// <param name="writeOut">Whether to write out the log message to the console.</param>
        /// <param name="newLine">Whether to write a new line before the message. If null, it will use the DefaultWriteOut's value. Will always force log.</param>
        /// <param name="forceLog">Whether to log this, and the built-up batch of logs right now, or only when the logging interval has expired.</param>
        public void Log(
            string message,
            string? details = "",
            LogSeverity severity = LogSeverity.INFO,
            bool? writeOut = null,
            bool newLine = false,
            bool forceLog = false
        );

        /// <summary>
        /// Progress Adventure logger.
        /// </summary>
        /// <inheritdoc cref="Log(string, string?, LogSeverity, bool?, bool)"/>
        public void LogAsync(
            string message,
            string? details = "",
            LogSeverity severity = LogSeverity.INFO,
            bool? writeOut = null,
            bool newLine = false,
            bool forceLog = false
        );

        /// <summary>
        /// Puts a newline in the logs.<br/>
        /// Always force logs.
        /// </summary>
        public void LogNewLine();
        #endregion
    }
}

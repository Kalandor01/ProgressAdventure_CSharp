namespace PACommon.Logging
{
    /// <summary>
    /// An interface to manage how to do logging.
    /// </summary>
    public interface ILoggerStream
    {
        /// <summary>
        /// Logs a text.
        /// </summary>
        /// <param name="text">The text to log.</param>
        /// <param name="newLine">Whether to insert a newline before the logged text.</param>
        Task LogTextAsync(List<(string message, DateTime time)> logs, bool newLine = false);

        /// <summary>
        /// Puts an empty line into the logs.
        /// </summary>
        Task LogNewLineAsync();

        /// <summary>
        /// Writes out a log text (to the user).
        /// </summary>
        /// <param name="text">The text to log.</param>
        /// <param name="newLine">Whether to insert a newline before the logged text.</param>
        Task WriteOutLogAsync(string text, bool newLine = false);

        /// <summary>
        /// Logs a logging exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        Task LogLoggingExceptionAsync(Exception exception);

        /// <summary>
        /// Handles the scenario, where the logging of a logging exception threw an exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        void LogFinalLoggingException(Exception exception);
    }
}

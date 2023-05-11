using ProgressAdventure.Enums;

namespace ProgressAdventure
{
    /// <summary>
    /// Contains functions for logging.
    /// </summary>
    public static class Logger
    {
        #region Public functions
        /// <summary>
        /// Progress Adventure logger.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="detail">The details of the message.</param>
        /// <param name="severity">The type of the message.</param>
        /// <param name="writeOut">Whether to write out the log message to the console, or not.</param>
        /// <param name="newLine">Whether to write a new line before the message. or not.</param>
        public static void Log(string message, string? detail = "", LogSeverity severity = LogSeverity.INFO, bool writeOut = false, bool newLine = false)
        {
            try
            {
                if (Constants.LOGGING_ENABLED && Constants.LOGGING_LEVEL <= (int)severity)
                {
                    var lType = severity.ToString();
                    var now = DateTime.Now;
                    var currentDate = Utils.MakeDate(now);
                    var currentTime = Utils.MakeTime(now, writeMs: Constants.LOG_MS);
                    Tools.RecreateLogsFolder();
                    using (var f = File.AppendText(Path.Join(Constants.LOGS_FOLDER_PATH, $"{currentDate}.{Constants.LOG_EXT}")))
                    {
                        if (newLine)
                        {
                            f.Write("\n");
                        }
                        f.Write($"[{currentTime}] [{Thread.CurrentThread.Name}/{lType}]\t: |{message}| {detail}\n");
                    }
                    if (writeOut)
                    {
                        if (newLine)
                        {
                            Console.WriteLine();
                        }
                        Console.WriteLine($"{Path.Join(Constants.LOGS_FOLDER, $"{currentDate}.{Constants.LOG_EXT}")} -> [{currentTime}] [{Thread.CurrentThread.Name}/{lType}]\t: |{message}| {detail}");
                    }
                }
            }
            catch (Exception e)
            {
                if (Constants.LOGGING_ENABLED)
                {
                    using var f = File.AppendText(Path.Join(Constants.ROOT_FOLDER, "CRASH.log"));
                    f.Write($"\n[{Utils.MakeDate(DateTime.Now)}_{Utils.MakeTime(DateTime.Now, writeMs: true)}] [LOGGING CRASHED]\t: |{e}|\n");
                }
            }
        }

        /// <summary>
        /// Sets the <c>LOGGING_LEVEL</c>, and if logging is enabled or not.
        /// </summary>
        /// <param name="value">The level to set the <c>LOGGING_LEVEL</c>.</param>
        public static void ChangeLoggingLevel(int value)
        {
            if (Constants.LOGGING_LEVEL != value)
            {
                // logging level
                var oldLoggingLevel = Constants.LOGGING_LEVEL;
                Constants.LOGGING_LEVEL = value;
                Log("Logging level changed", $"{oldLoggingLevel} -> {Constants.LOGGING_LEVEL}");
                // logging
                var oldLogging = Constants.LOGGING_ENABLED;
                Constants.LOGGING_ENABLED = value != -1;
                if (Constants.LOGGING_ENABLED != oldLogging)
                {
                    Log($"Logging {(Constants.LOGGING_ENABLED ? "enabled" : "disabled")}");
                }
            }
        }

        /// <summary>
        /// Puts a newline in the logs.
        /// </summary>
        public static void LogNewLine()
        {
            try
            {
                if (Constants.LOGGING_ENABLED)
                {
                    Tools.RecreateLogsFolder();
                    using var f = File.AppendText(Path.Join(Constants.LOGS_FOLDER_PATH, $"{Utils.MakeDate(DateTime.Now)}.{Constants.LOG_EXT}"));
                    f.Write("\n");
                }
            }
            catch (Exception e)
            {
                if (Constants.LOGGING_ENABLED)
                {
                    using var f = File.AppendText(Path.Join(Constants.ROOT_FOLDER, "CRASH.log"));
                    f.Write($"\n[{Utils.MakeDate(DateTime.Now)}_{Utils.MakeTime(DateTime.Now, writeMs: true)}] [LOG NEW LINE CRASHED]\t: |{e}|\n");
                }
            }
        }
        #endregion
    }
}

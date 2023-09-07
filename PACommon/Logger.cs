using PACommon.Enums;

namespace PACommon
{
    /// <summary>
    /// Contains functions for logging.
    /// </summary>
    public static class Logger
    {
        #region Public dicts
        /// <summary>
        /// Dictionary mapping logging severities to their logging values.<br/>
        /// A log will actualy happen, if the current logging level is lower than the loggin level of the message.
        /// </summary>
        internal static readonly Dictionary<LogSeverity, int> loggingValuesMap = new()
        {
            [LogSeverity.DISABLED] = -1,
            [LogSeverity.DEBUG] = (int)LogSeverity.DEBUG,
            [LogSeverity.INFO] = (int)LogSeverity.INFO,
            [LogSeverity.WARN] = (int)LogSeverity.WARN,
            [LogSeverity.ERROR] = (int)LogSeverity.ERROR,
            [LogSeverity.FATAL] = (int)LogSeverity.FATAL,

            [LogSeverity.PASS] = (int)LogSeverity.PASS,
            [LogSeverity.FAIL] = (int)LogSeverity.FAIL,
            [LogSeverity.OTHER] = (int)LogSeverity.OTHER,
        };
        #endregion

        #region Private fields
        /// <summary>
        /// The default value for is the logs should be writen out to the console.
        /// </summary>
        private static bool _defaultWriteOut = false;
        /// <summary>
        /// If logging is enabled or not.
        /// </summary>
        private static bool _isLoggingEnabled = true;
        /// <summary>
        /// The current logging level.
        /// </summary>
        private static LogSeverity _loggingLevel = LogSeverity.DEBUG;
        #endregion

        #region Public properties
        /// <summary>
        /// <inheritdoc cref="_defaultWriteOut" path="//summary"/>
        /// </summary>
        public static bool DefaultWriteOut
        {
            get
            {
                return _defaultWriteOut;
            }
            set
            {
                if (_defaultWriteOut != value)
                {
                    _defaultWriteOut = value;
                    Log($"Logging write out {(value ? "enabled" : "disabled")}");
                }
            }
        }

        /// <summary>
        /// If the logger should log the milisecond in time.
        /// </summary>
        public static bool LogMS { get; set; }

        /// <summary>
        /// <inheritdoc cref="_isLoggingEnabled" path="//summary"/>
        /// </summary>
        public static bool LoggingEnabled { get => _isLoggingEnabled; private set => _isLoggingEnabled = value; }

        /// <summary>
        /// <inheritdoc cref="_loggingLevel" path="//summary"/>
        /// </summary>
        public static LogSeverity LoggingLevel {
            get => _loggingLevel;
            set
            {
                ChangeLoggingLevel(value);
            }
        }
        #endregion

        #region Public functions
        /// <summary>
        /// Progress Adventure logger.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="details">The details of the message.</param>
        /// <param name="severity">The severity of the message.</param>
        /// <param name="writeOut">Whether to write out the log message to the console, or not.</param>
        /// <param name="newLine">Whether to write a new line before the message. or not.</param>
        public static void Log(string message, string? details = "", LogSeverity severity = LogSeverity.INFO, bool? writeOut = null, bool newLine = false)
        {
            try
            {
                if (LoggingEnabled && loggingValuesMap[LoggingLevel] <= loggingValuesMap[severity])
                {
                    var now = DateTime.Now;
                    var currentDate = Utils.MakeDate(now);
                    var currentTime = Utils.MakeTime(now, writeMs: LogMS);
                    Tools.RecreateLogsFolder();
                    using (var f = File.AppendText(Path.Join(Constants.LOGS_FOLDER_PATH, $"{currentDate}.{Constants.LOG_EXT}")))
                    {
                        if (newLine)
                        {
                            f.Write("\n");
                        }
                        f.Write($"[{currentTime}] [{Thread.CurrentThread.Name}/{severity}]\t: |{message}| {details}\n");
                    }
                    if (writeOut is null ? _defaultWriteOut : (bool)writeOut)
                    {
                        if (newLine)
                        {
                            Console.WriteLine();
                        }
                        Console.WriteLine($"{Path.Join(Constants.LOGS_FOLDER, $"{currentDate}.{Constants.LOG_EXT}")} -> [{currentTime}] [{Thread.CurrentThread.Name}/{severity}]\t: |{message}| {details}");
                    }
                }
            }
            catch (Exception e)
            {
                if (LoggingEnabled)
                {
                    try
                    {
                        using var f = File.AppendText(Path.Join(Constants.ROOT_FOLDER, "CRASH.log"));
                        f.Write($"\n[{Utils.MakeDate(DateTime.Now)}_{Utils.MakeTime(DateTime.Now, writeMs: true)}] [LOGGING CRASHED]\t: |{e}|\n");
                    }
                    catch (Exception e2)
                    {
                        Console.WriteLine("\n\n\n\n\n\n\n\n\n\n" + "JUST...NO!!! (Logger exception level 2):\n" + e2.ToString() + "\n\n\n");
                        Console.ReadKey();
                    }
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
                if (LoggingEnabled)
                {
                    Tools.RecreateLogsFolder();
                    using var f = File.AppendText(Path.Join(Constants.LOGS_FOLDER_PATH, $"{Utils.MakeDate(DateTime.Now)}.{Constants.LOG_EXT}"));
                    f.Write("\n");
                }
            }
            catch (Exception e)
            {
                if (LoggingEnabled)
                {
                    using var f = File.AppendText(Path.Join(Constants.ROOT_FOLDER, "CRASH.log"));
                    f.Write($"\n[{Utils.MakeDate(DateTime.Now)}_{Utils.MakeTime(DateTime.Now, writeMs: true)}] [LOG NEW LINE CRASHED]\t: |{e}|\n");
                }
            }
        }
        #endregion

        #region Private functions
        /// <summary>
        /// Sets the <c>LOGGING_LEVEL</c>, and if logging is enabled or not.
        /// </summary>
        /// <param name="value">The level to set the <c>LOGGING_LEVEL</c>.</param>
        private static void ChangeLoggingLevel(LogSeverity value)
        {
            if (_loggingLevel != value)
            {
                // logging level
                var oldLoggingLevel = _loggingLevel;
                var logginValue = loggingValuesMap[value];
                _loggingLevel = value;
                Log("Logging level changed", $"{oldLoggingLevel} -> {_loggingLevel}");

                // logging enabled
                var oldLogging = LoggingEnabled;
                LoggingEnabled = logginValue != -1;
                if (LoggingEnabled != oldLogging)
                {
                    Log($"Logging {(LoggingEnabled ? "enabled" : "disabled")}");
                }
            }
        }

        /// <summary>
        /// Tries to turn the int value of the log severity into a <c>LogSeverity</c> enum, and returns the success.
        /// </summary>
        /// <param name="severityValue">The log sevrity's int representation.</param>
        /// <param name="severity">The sevrity, that got parsed, or created.</param>
        public static bool TryParseSeverityValue(int severityValue, out LogSeverity severity)
        {
            foreach (var loggingValue in loggingValuesMap)
            {
                if (loggingValue.Value == severityValue)
                {
                    severity = loggingValue.Key;
                    return true;
                }
            }

            severity = LogSeverity.DEBUG;
            return false;
        }

        public static int GetSeverityValue(LogSeverity severity)
        {
            return loggingValuesMap[severity];
        }
        #endregion
    }
}

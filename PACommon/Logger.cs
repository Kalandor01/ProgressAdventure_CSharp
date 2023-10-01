using PACommon.Enums;

namespace PACommon
{
    /// <summary>
    /// Contains functions for logging.
    /// </summary>
    public static class Logger
    {
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
        public static bool LoggingEnabled
        {
            get => _isLoggingEnabled;
            private set => _isLoggingEnabled = value;
        }

        /// <summary>
        /// <inheritdoc cref="_loggingLevel" path="//summary"/>
        /// </summary>
        public static LogSeverity LoggingLevel
        {
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
        public static async void Log(string message, string? details = "", LogSeverity severity = LogSeverity.INFO, bool? writeOut = null, bool newLine = false)
        {
            try
            {
                if (LoggingEnabled && (int)LoggingLevel <= (int)severity)
                {
                    var now = DateTime.Now;
                    var currentDate = Utils.MakeDate(now);
                    var currentTime = Utils.MakeTime(now, writeMs: LogMS);
                    Tools.RecreateLogsFolder();
                    string text =$"{(newLine ? "\n" : "")}[{currentTime}] [{Thread.CurrentThread.Name}/{severity}]\t: |{message}| {details}\n";
                    while (true)
                    {
                        try
                        {
                            using var f = File.AppendText(Path.Join(Constants.LOGS_FOLDER_PATH, $"{currentDate}.{Constants.LOG_EXT}"));
                            await f.WriteAsync(text);
                            break;
                        }
                        catch (IOException) { }
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
                        await f.WriteAsync($"\n[{Utils.MakeDate(DateTime.Now)}_{Utils.MakeTime(DateTime.Now, writeMs: true)}] [LOGGING CRASHED]\t: |{e}|\n");
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
        /// Progress Adventure logger.<br/>
        /// WILL log things out of order!
        /// </summary>
        /// <inheritdoc cref="Log(string, string?, LogSeverity, bool?, bool)"/>
        public static void LogAsync(string message, string? details = "", LogSeverity severity = LogSeverity.INFO, bool? writeOut = null, bool newLine = false)
        {
            LogAsyncTask(Thread.CurrentThread.Name + "(async)", message, details, severity, writeOut, newLine);
        }

        /// <summary>
        /// Puts a newline in the logs.
        /// </summary>
        public static async void LogNewLine()
        {
            try
            {
                if (LoggingEnabled)
                {
                    Tools.RecreateLogsFolder();
                    using var f = File.AppendText(Path.Join(Constants.LOGS_FOLDER_PATH, $"{Utils.MakeDate(DateTime.Now)}.{Constants.LOG_EXT}"));
                    await f.WriteAsync("\n");
                }
            }
            catch (Exception e)
            {
                if (LoggingEnabled)
                {
                    using var f = File.AppendText(Path.Join(Constants.ROOT_FOLDER, "CRASH.log"));
                    await f.WriteAsync($"\n[{Utils.MakeDate(DateTime.Now)}_{Utils.MakeTime(DateTime.Now, writeMs: true)}] [LOG NEW LINE CRASHED]\t: |{e}|\n");
                }
            }
        }
        #endregion

        #region Private functions
        /// <summary>
        /// Progress Adventure logger.<br/>
        /// WILL log things out of order!
        /// </summary>
        /// <param name="threadName">The name of the outer thread.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="details">The details of the message.</param>
        /// <param name="severity">The severity of the message.</param>
        /// <param name="writeOut">Whether to write out the log message to the console, or not.</param>
        /// <param name="newLine">Whether to write a new line before the message. or not.</param>
        private static async void LogAsyncTask(string? threadName, string message, string? details = "", LogSeverity severity = LogSeverity.INFO, bool? writeOut = null, bool newLine = false)
        {
            await Task.Run(() => {
                Thread.CurrentThread.Name = threadName;
                Log(message, details, severity, writeOut, newLine);
            });
        }

        /// <summary>
        /// Sets the <c>LOGGING_LEVEL</c>, and if logging is enabled or not.
        /// </summary>
        /// <param name="value">The level to set the <c>LOGGING_LEVEL</c>.</param>
        private static void ChangeLoggingLevel(LogSeverity value)
        {
            if (_loggingLevel != value)
            {
                // logging level
                Log("Logging level changed", $"{_loggingLevel} -> {value}");
                _loggingLevel = value;

                // logging enabled
                var newLoggingEnabled = (int)value != -1;
                if (LoggingEnabled != newLoggingEnabled)
                {
                    Log($"Logging {(newLoggingEnabled ? "enabled" : "disabled")}");
                    LoggingEnabled = newLoggingEnabled;
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
    }
}

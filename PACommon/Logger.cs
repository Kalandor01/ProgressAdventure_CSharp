using PACommon.Enums;

namespace PACommon
{
    /// <summary>
    /// Contains functions for logging.
    /// </summary>
    public class Logger
    {
        #region Private fields
        /// <summary>
        /// The singleton istance.
        /// </summary>
        private static Logger? _instance = null;
        /// <summary>
        /// The default value for is the logs should be writen out to the console.
        /// </summary>
        private bool _defaultWriteOut = false;
        /// <summary>
        /// If logging is enabled or not.
        /// </summary>
        private bool _isLoggingEnabled = true;
        /// <summary>
        /// The current logging level.
        /// </summary>
        private LogSeverity _loggingLevel = LogSeverity.DEBUG;

        /// <summary>
        /// The name of the folder, where the logs will be placed.
        /// </summary>
        private readonly string logsFolder;
        /// <summary>
        /// The path to the folder that contains the logs folder. Must be an existing folder.
        /// </summary>
        private readonly string logsFolderParrentPath;
        /// <summary>
        /// The extension used for log files.
        /// </summary>
        private readonly string logsExt;
        #endregion

        #region Public properties
        /// <summary>
        /// <inheritdoc cref="_instance" path="//summary"/>
        /// </summary>
        public static Logger Instance { get { return _instance ?? Initialize(Constants.ROOT_FOLDER); } }

        /// <summary>
        /// <inheritdoc cref="_defaultWriteOut" path="//summary"/>
        /// </summary>
        public bool DefaultWriteOut
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
        public bool LogMS { get; set; }

        /// <summary>
        /// <inheritdoc cref="_isLoggingEnabled" path="//summary"/>
        /// </summary>
        public bool LoggingEnabled
        {
            get => _isLoggingEnabled;
            private set => _isLoggingEnabled = value;
        }

        /// <summary>
        /// <inheritdoc cref="_loggingLevel" path="//summary"/>
        /// </summary>
        public LogSeverity LoggingLevel
        {
            get => _loggingLevel;
            set
            {
                ChangeLoggingLevel(value);
            }
        }
        #endregion

        #region Private Constructors
        /// <summary>
        /// <inheritdoc cref="Logger"/>
        /// </summary>
        /// <param name="logsFolderParrentPath"><inheritdoc cref="logsFolderParrentPath" path="//summary"/></param>
        /// <param name="logsFolder"><inheritdoc cref="logsFolder" path="//summary"/></param>
        /// <param name="logsExtension"><inheritdoc cref="logsExt" path="//summary"/></param>
        /// <param name="logMilliseconds"><inheritdoc cref="LogMS" path="//summary"/></param>
        /// <param name="defaultWriteOut"><inheritdoc cref="_defaultWriteOut" path="//summary"/></param>
        /// <param name="loggingLevel"><inheritdoc cref="_loggingLevel" path="//summary"/></param>
        private Logger(
            string logsFolderParrentPath,
            string logsFolder,
            string logsExtension,
            bool logMilliseconds = Constants.DEFAULT_LOG_MS,
            bool defaultWriteOut = false,
            LogSeverity loggingLevel = LogSeverity.DEBUG
        )
        {
            if (string.IsNullOrWhiteSpace(logsFolderParrentPath))
            {
                throw new ArgumentException($"'{nameof(logsFolderParrentPath)}' cannot be null or whitespace.", nameof(logsFolderParrentPath));
            }

            if (string.IsNullOrWhiteSpace(logsFolder))
            {
                throw new ArgumentException($"'{nameof(logsFolder)}' cannot be null or whitespace.", nameof(logsFolder));
            }

            if (string.IsNullOrWhiteSpace(logsExtension))
            {
                throw new ArgumentException($"'{nameof(logsExtension)}' cannot be null or whitespace.", nameof(logsExtension));
            }

            if (!Directory.Exists(logsFolderParrentPath))
            {
                throw new DirectoryNotFoundException($"'{nameof(logsFolderParrentPath)}' is not an existing directory.");
            }

            this.logsFolderParrentPath = logsFolderParrentPath;
            this.logsFolder = logsFolder;
            logsExt = logsExtension;
            LogMS = logMilliseconds;
            _defaultWriteOut = defaultWriteOut;
            _isLoggingEnabled = loggingLevel != LogSeverity.DISABLED;
            _loggingLevel = loggingLevel;
        }
        #endregion

        #region Public functions
        /// <summary>
        /// Initializes the object's values.
        /// </summary>
        /// <param name="logInitialization">Whether to log the fact that the <c>Logger</c> was initialized.</param>
        /// <param name="logsFolderParrentPath"><inheritdoc cref="logsFolderParrentPath" path="//summary"/></param>
        /// <param name="logsFolder"><inheritdoc cref="logsFolder" path="//summary"/></param>
        /// <param name="logsExtension"><inheritdoc cref="logsExt" path="//summary"/></param>
        /// <param name="logMilliseconds"><inheritdoc cref="LogMS" path="//summary"/></param>
        /// <param name="defaultWriteOut"><inheritdoc cref="_defaultWriteOut" path="//summary"/></param>
        /// <param name="loggingLevel"><inheritdoc cref="_loggingLevel" path="//summary"/></param>
        public static Logger Initialize(
            string logsFolderParrentPath,
            string logsFolder = Constants.DEFAULT_LOGS_FOLDER,
            string logsExtension = Constants.DEFAULT_LOG_EXT,
            bool logMilliseconds = Constants.DEFAULT_LOG_MS,
            bool defaultWriteOut = false,
            LogSeverity loggingLevel = LogSeverity.DEBUG,
            bool logInitialization = true
        )
        {
            _instance = new Logger(logsFolderParrentPath, logsFolder, logsExtension, logMilliseconds, defaultWriteOut, loggingLevel);
            if (logInitialization)
            {
                _instance.Log("Logger initialized", newLine: true);
            }
            return _instance;
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

        #region Public methods
        /// <summary>
        /// Progress Adventure logger.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="details">The details of the message.</param>
        /// <param name="severity">The severity of the message.</param>
        /// <param name="writeOut">Whether to write out the log message to the console, or not.</param>
        /// <param name="newLine">Whether to write a new line before the message, or not.</param>
        public async void Log(string message, string? details = "", LogSeverity severity = LogSeverity.INFO, bool? writeOut = null, bool newLine = false)
        {
            try
            {
                if (LoggingEnabled && (int)LoggingLevel <= (int)severity)
                {
                    var now = DateTime.Now;
                    var currentDate = Utils.MakeDate(now);
                    var currentTime = Utils.MakeTime(now, writeMs: LogMS);
                    RecreateLogsFolder();
                    string text =$"{(newLine ? "\n" : "")}[{currentTime}] [{Thread.CurrentThread.Name}/{severity}]\t: |{message}| {details}\n";
                    while (true)
                    {
                        try
                        {
                            using var f = File.AppendText(Path.Join(logsFolderParrentPath, logsFolder, $"{currentDate}.{logsExt}"));
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
                        Console.WriteLine($"{Path.Join(logsFolder, $"{currentDate}.{logsExt}")} -> [{currentTime}] [{Thread.CurrentThread.Name}/{severity}]\t: |{message}| {details}");
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
                        Console.WriteLine($"\n\n\n\n\n\n\n\n\n\nJUST...NO!!! (Logger exception level 2):\n{e2}\n\n\n");
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
        public void LogAsync(string message, string? details = "", LogSeverity severity = LogSeverity.INFO, bool? writeOut = null, bool newLine = false)
        {
            LogAsyncTask(Thread.CurrentThread.Name + "(async)", message, details, severity, writeOut, newLine);
        }

        /// <summary>
        /// Puts a newline in the logs.
        /// </summary>
        public async void LogNewLine()
        {
            try
            {
                if (LoggingEnabled)
                {
                    RecreateLogsFolder();
                    using var f = File.AppendText(Path.Join(logsFolderParrentPath, logsFolder, $"{Utils.MakeDate(DateTime.Now)}.{logsExt}"));
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

        /// <summary>
        /// <c>RecreateFolder</c> for the logs folder.
        /// </summary>
        /// <returns><inheritdoc cref="RecreateFolder(string, string?, string?)"/></returns>
        public bool RecreateLogsFolder()
        {
            return Tools.RecreateFolder(logsFolder, logsFolderParrentPath);
        }
        #endregion

        #region Private methods
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
        private async void LogAsyncTask(string? threadName, string message, string? details = "", LogSeverity severity = LogSeverity.INFO, bool? writeOut = null, bool newLine = false)
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
        private void ChangeLoggingLevel(LogSeverity value)
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
                    Log($"Logging disabled");
                    LoggingEnabled = newLoggingEnabled;
                    Log($"Logging enabled");
                }
            }
        }
        #endregion
    }
}

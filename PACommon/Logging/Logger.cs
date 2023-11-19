using PACommon.Enums;

namespace PACommon.Logging
{
    /// <summary>
    /// Contains functions for logging.
    /// </summary>
    public class Logger : ILogger
    {
        #region Private fields
        /// <summary>
        /// Object used for locking the thread while the singleton gets created.
        /// </summary>
        private static readonly object _threadLock = new();
        /// <summary>
        /// The singleton istance.
        /// </summary>
        private static Logger? _instance = null;

        /// <summary>
        /// The default value for is the logs should be writen out to the console.
        /// </summary>
        private bool _defaultWriteOut = false;
        /// <summary>
        /// If logging is enabled.
        /// </summary>
        private bool _isLoggingEnabled = true;
        /// <summary>
        /// The current logging level.
        /// </summary>
        private LogSeverity _loggingLevel = LogSeverity.DEBUG;

        /// <summary>
        /// The logger stream to use to write logs.
        /// </summary>
        private readonly ILoggerStream loggerStream;
        #endregion

        #region Public properties
        /// <summary>
        /// <inheritdoc cref="_instance" path="//summary"/>
        /// </summary>
        public static Logger Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_threadLock)
                    {
                        _instance ??= Initialize(new FileLoggerStream(Constants.ROOT_FOLDER));
                    }
                }
                return _instance;
            }
        }

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

        public bool LogMS { get; set; }

        public bool LoggingEnabled
        {
            get => _isLoggingEnabled;
            private set => _isLoggingEnabled = value;
        }

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
        /// <param name="loggerStream"><inheritdoc cref="loggerStream" path="//summary"/></param>
        /// <param name="logMilliseconds"><inheritdoc cref="LogMS" path="//summary"/></param>
        /// <param name="defaultWriteOut"><inheritdoc cref="_defaultWriteOut" path="//summary"/></param>
        /// <param name="loggingLevel"><inheritdoc cref="_loggingLevel" path="//summary"/></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        private Logger(
            ILoggerStream loggerStream,
            bool logMilliseconds = Constants.DEFAULT_LOG_MS,
            bool defaultWriteOut = false,
            LogSeverity loggingLevel = LogSeverity.DEBUG
        )
        {
            this.loggerStream = loggerStream;
            LogMS = logMilliseconds;
            _defaultWriteOut = defaultWriteOut;
            _isLoggingEnabled = loggingLevel != LogSeverity.DISABLED;
            _loggingLevel = loggingLevel;
        }
        #endregion

        #region "Initializer"
        /// <summary>
        /// Initializes the object's values.
        /// </summary>
        /// <param name="loggerStream"><inheritdoc cref="loggerStream" path="//summary"/></param>
        /// <param name="logMilliseconds"><inheritdoc cref="LogMS" path="//summary"/></param>
        /// <param name="defaultWriteOut"><inheritdoc cref="_defaultWriteOut" path="//summary"/></param>
        /// <param name="loggingLevel"><inheritdoc cref="_loggingLevel" path="//summary"/></param>
        /// <param name="logInitialization">Whether to log the fact that the <c>Logger</c> was initialized.</param>
        public static Logger Initialize(
            ILoggerStream loggerStream,
            bool logMilliseconds = Constants.DEFAULT_LOG_MS,
            bool defaultWriteOut = false,
            LogSeverity loggingLevel = LogSeverity.DEBUG,
            bool logInitialization = true
        )
        {
            _instance = new Logger(loggerStream, logMilliseconds, defaultWriteOut, loggingLevel);
            if (logInitialization)
            {
                _instance.Log($"{nameof(Logger)} initialized", newLine: true);
            }
            return _instance;
        }
        #endregion

        #region Public methods
        public async void Log(string message, string? details = "", LogSeverity severity = LogSeverity.INFO, bool? writeOut = null, bool newLine = false)
        {
            try
            {
                if (LoggingEnabled && (int)LoggingLevel <= (int)severity)
                {
                    var now = DateTime.Now;
                    var currentDate = Utils.MakeDate(now);
                    var currentTime = Utils.MakeTime(now, writeMs: LogMS);
                    string logLine = $"[{currentTime}] [{Thread.CurrentThread.Name}/{severity}]\t: |{message}| {details}\n";

                    await loggerStream.LogTextAsync(logLine, newLine);
                    if (writeOut is null ? _defaultWriteOut : (bool)writeOut)
                    {
                        await loggerStream.WriteOutLogAsync(logLine, newLine);
                    }
                }
            }
            catch (Exception e)
            {
                if (LoggingEnabled)
                {
                    try
                    {
                        await loggerStream.LogLoggingExceptionAsync(e);
                    }
                    catch (Exception e2)
                    {
                        loggerStream.LogFinalLoggingException(e2);
                    }
                }
            }
        }

        /// <summary>
        /// Progress Adventure logger.<br/>
        /// WILL log things out of order!
        /// </summary>
        /// <inheritdoc cref="ILogger.LogAsync(string, string?, LogSeverity, bool?, bool)"/>
        public void LogAsync(string message, string? details = "", LogSeverity severity = LogSeverity.INFO, bool? writeOut = null, bool newLine = false)
        {
            LogAsyncTask(Thread.CurrentThread.Name + "(async)", message, details, severity, writeOut, newLine);
        }

        public async void LogNewLine()
        {
            try
            {
                if (LoggingEnabled)
                {
                    await loggerStream.LogNewLineAsync();
                }
            }
            catch (Exception e)
            {
                if (LoggingEnabled)
                {
                    try
                    {
                        await loggerStream.LogLoggingExceptionAsync(e);
                    }
                    catch (Exception e2)
                    {
                        loggerStream.LogFinalLoggingException(e2);
                    }
                }
            }
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
        /// <param name="writeOut">Whether to write out the log message to the console.</param>
        /// <param name="newLine">Whether to write a new line before the message.</param>
        private async void LogAsyncTask(string? threadName, string message, string? details = "", LogSeverity severity = LogSeverity.INFO, bool? writeOut = null, bool newLine = false)
        {
            await Task.Run(() =>
            {
                Thread.CurrentThread.Name = threadName;
                Log(message, details, severity, writeOut, newLine);
            });
        }

        /// <summary>
        /// Sets the <c>LOGGING_LEVEL</c>, and if logging is enabled.
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

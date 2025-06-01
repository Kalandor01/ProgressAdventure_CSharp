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
        /// <inheritdoc cref="DefaultWriteOut" path="//summary"/>
        /// </summary>
        private bool _defaultWriteOut = false;
        /// <summary>
        /// <inheritdoc cref="LoggingLevel" path="//summary"/>
        /// </summary>
        private LogSeverity _loggingLevel = LogSeverity.DEBUG;

        /// <summary>
        /// The logger stream to use to write logs.
        /// </summary>
        private readonly ILoggerStream loggerStream;

        /// <summary>
        /// The buffer for logs, and when they were originaly put into the buffer.
        /// </summary>
        private List<(string message, DateTime time)> _logMessageBuffer;

        /// <summary>
        /// The time, when the messages in the buffer were last logged.
        /// </summary>
        private DateTime _lastLogTime;
        #endregion

        #region Public properties
        /// <summary>
        /// <inheritdoc cref="_instance" path="//summary"/>
        /// </summary>
        public static Logger Instance
        {
            get
            {
                if (_instance is null)
                {
                    _instance ??= Initialize(
                        new FileLoggerStream(Path.Join(Constants.ROOT_FOLDER, Constants.DEFAULT_LOGS_FOLDER)),
                        onlyIfUninitialized: true
                    );
                }
                return _instance;
            }
        }

        public bool DefaultWriteOut
        {
            get => _defaultWriteOut;
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

        public bool LoggingEnabled { get; private set; }

        public LogSeverity LoggingLevel
        {
            get => _loggingLevel;
            set => ChangeLoggingLevel(value);
        }

        public TimeSpan ForceLogInterval { get; set; }
        #endregion

        #region Private Constructors
        /// <summary>
        /// <inheritdoc cref="Logger"/>
        /// </summary>
        /// <param name="loggerStream"><inheritdoc cref="loggerStream" path="//summary"/></param>
        /// <param name="logMilliseconds"><inheritdoc cref="LogMS" path="//summary"/></param>
        /// <param name="defaultWriteOut"><inheritdoc cref="DefaultWriteOut" path="//summary"/></param>
        /// <param name="loggingLevel"><inheritdoc cref="LoggingLevel" path="//summary"/></param>
        /// <param name="forceLogInterval"><inheritdoc cref="ForceLogInterval" path="//summary"/></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        private Logger(
            ILoggerStream loggerStream,
            bool logMilliseconds = Constants.DEFAULT_LOG_MS,
            bool defaultWriteOut = false,
            LogSeverity loggingLevel = LogSeverity.DEBUG,
            TimeSpan? forceLogInterval = null
        )
        {
            this.loggerStream = loggerStream;
            LogMS = logMilliseconds;
            _defaultWriteOut = defaultWriteOut;
            LoggingEnabled = loggingLevel != LogSeverity.DISABLED;
            _loggingLevel = loggingLevel;
            ForceLogInterval = forceLogInterval ?? new TimeSpan(0, 0, 5);
            _logMessageBuffer = [];
            _lastLogTime = DateTime.Now;
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
        /// <param name="forceLogInterval"><inheritdoc cref="_forceLogInterval" path="//summary"/></param>
        /// <param name="logInitialization">Whether to log the fact that the <c>Logger</c> was initialized.</param>
        /// <param name="onlyIfUninitialized">If true, only initializes the singleton if it hasn't been initialized yet.</param>
        public static Logger Initialize(
            ILoggerStream loggerStream,
            bool logMilliseconds = Constants.DEFAULT_LOG_MS,
            bool defaultWriteOut = false,
            LogSeverity loggingLevel = LogSeverity.DEBUG,
            TimeSpan? forceLogInterval = null,
            bool logInitialization = true,
            bool onlyIfUninitialized = false
        )
        {
            lock (_threadLock)
            {
                if (onlyIfUninitialized && _instance is not null)
                {
                    return _instance;
                }

                _instance?.Dispose();
                _instance = new Logger(loggerStream, logMilliseconds, defaultWriteOut, loggingLevel, forceLogInterval);
                if (logInitialization)
                {
                    _instance.Log($"{nameof(Logger)} initialized", newLine: true);
                }
                return _instance;
            }
        }
        #endregion

        #region Public methods
        public async void Log(
            string message,
            string? details = "",
            LogSeverity severity = LogSeverity.INFO,
            bool? writeOut = null,
            bool newLine = false,
            bool forceLog = false
        )
        {
            try
            {
                if (LoggingEnabled && (int)LoggingLevel <= (int)severity)
                {
                    var now = DateTime.Now;
                    var currentDate = Utils.MakeDate(now);
                    var currentTime = Utils.MakeTime(now, writeMs: LogMS);
                    string logLine = $"[{currentTime}] [{Thread.CurrentThread.Name}/{severity}]\t: |{message}| {details}";
                    _logMessageBuffer.Add((logLine, now));
                    await LogIfNeeded(newLine, forceLog);
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
        /// MIGHT log things out of order!
        /// </summary>
        /// <inheritdoc cref="ILogger.LogAsync(string, string?, LogSeverity, bool?, bool)"/>
        public void LogAsync(
            string message,
            string? details = "",
            LogSeverity severity = LogSeverity.INFO,
            bool? writeOut = null,
            bool newLine = false,
            bool forceLog = false
        )
        {
            LogAsyncTask(Thread.CurrentThread.Name + "(async)", message, details, severity, writeOut, newLine, forceLog);
        }

        public async void LogNewLine()
        {
            try
            {
                if (LoggingEnabled)
                {
                    if (_logMessageBuffer.Count == 0)
                    {
                        await loggerStream.LogNewLineAsync();
                        return;
                    }
                    await loggerStream.LogTextAsync(_logMessageBuffer, true);
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
        private async void LogAsyncTask(string? threadName,
            string message,
            string? details = "",
            LogSeverity severity = LogSeverity.INFO,
            bool? writeOut = null,
            bool newLine = false,
            bool forceLog = false
        )
        {
            await Task.Run(() =>
            {
                Thread.CurrentThread.Name = threadName;
                Log(message, details, severity, writeOut, newLine, forceLog);
            });
        }

        private async Task LogIfNeeded(bool newLine, bool forceLog)
        {
            if (forceLog || newLine || _lastLogTime + ForceLogInterval < DateTime.Now)
            {
                _lastLogTime = DateTime.Now;
                var previousList = _logMessageBuffer.ToList();
                _logMessageBuffer = [];
                await loggerStream.LogTextAsync(previousList, newLine);
            }
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

        public void Dispose()
        {
            loggerStream.LogTextAsync(_logMessageBuffer).Wait();
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}

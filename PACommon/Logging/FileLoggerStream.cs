using System.Text;

namespace PACommon.Logging
{
    /// <summary>
    /// Class to do logging to files.
    /// </summary>
    public class FileLoggerStream : ILoggerStream
    {
        #region Private fields

        /// <summary>
        /// The name of the folder, where the logs will be placed.
        /// </summary>
        private readonly string logsFolderPath;
        /// <summary>
        /// The extension used for log files.
        /// </summary>
        private readonly string logsExt;
        #endregion

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logsFolderPath"><inheritdoc cref="logsFolderPath" path="//summary"/></param>
        /// <param name="logsExtension"><inheritdoc cref="logsExt" path="//summary"/></param>
        public FileLoggerStream(
            string? logsFolderPath = null,
            string logsExtension = Constants.DEFAULT_LOG_EXT
        )
        {
            if (string.IsNullOrWhiteSpace(logsFolderPath))
            {
                throw new ArgumentException($"'{nameof(logsFolderPath)}' cannot be null or whitespace.", nameof(logsFolderPath));
            }

            if (string.IsNullOrWhiteSpace(logsExtension))
            {
                throw new ArgumentException($"'{nameof(logsExtension)}' cannot be null or whitespace.", nameof(logsExtension));
            }

            this.logsFolderPath = logsFolderPath ?? Path.Join(Constants.ROOT_FOLDER, Constants.DEFAULT_LOGS_FOLDER);
            logsExt = logsExtension;
        }
        #endregion

        #region Public methods
        public async Task LogTextAsync(List<(string message, DateTime time)> logsBuffer, bool newLine = false)
        {
            if (logsBuffer.Count == 0)
            {
                return;
            }
            if (newLine)
            {
                var (message, time) = logsBuffer.Last();
                logsBuffer[^1] = ("\n" + message, time);
            }

            do
            {
                var firstTime = logsBuffer.First().time;
                var firstDate = firstTime.Date;
                var logsJoined = new StringBuilder();
                int x;
                for (x = 0; x < logsBuffer.Count; x++)
                {
                    if (logsBuffer[x].time.Date == firstDate)
                    {
                        logsJoined.Append(logsBuffer[x].message);
                        logsJoined.Append('\n');
                        continue;
                    }
                    break;
                }
                logsBuffer.RemoveRange(0, x);
                await LogSameDayBatchAsync(logsJoined.ToString(), firstTime);
            }
            while (logsBuffer.Count != 0);
        }

        public async Task LogSameDayBatchAsync(string joinedLogs, DateTime date)
        {
            RecreateLogsFolder();
            var currentDate = Utils.MakeDate(date);

            while (true)
            {
                try
                {
                    using var writer = File.AppendText(Path.Join(logsFolderPath, $"{currentDate}.{logsExt}"));
                    await writer.WriteAsync(joinedLogs);
                    break;
                }
                catch (IOException) { }
            }
        }

        public async Task LogNewLineAsync()
        {
            using var f = File.AppendText(Path.Join(logsFolderPath, $"{Utils.MakeDate(DateTime.Now)}.{logsExt}"));
            await f.WriteAsync("\n");
        }

        public async Task WriteOutLogAsync(string text, bool newLine = false)
        {
            var currentDate = Utils.MakeDate(DateTime.Now);
            Console.Write($"{(newLine ? "\n" : "")}{Path.Join(logsFolderPath, $"{currentDate}.{logsExt}")} -> {text}");
        }

        public async Task LogLoggingExceptionAsync(Exception exception)
        {
            RecreateLogsFolder();
            using var f = File.AppendText(Path.Join(Constants.ROOT_FOLDER, "CRASH.log"));
            await f.WriteAsync($"\n[{Utils.MakeDate(DateTime.Now)}_{Utils.MakeTime(DateTime.Now, writeMs: true)}] [LOGGING CRASHED]\t: |{exception}|\n");
        }

        public void LogFinalLoggingException(Exception exception)
        {
            Console.WriteLine($"\nLogger exception level 2: {exception.Message}");
        }

        /// <summary>
        /// <c>RecreateFolder</c> for the logs folder.
        /// </summary>
        /// <returns><inheritdoc cref="Tools.RecreateFolder(string, string?, string?)"/></returns>
        public bool RecreateLogsFolder()
        {
            return Tools.RecreateFolder(logsFolderPath);
        }
        #endregion
    }
}

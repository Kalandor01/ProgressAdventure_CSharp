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

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logsFolderParrentPath"><inheritdoc cref="logsFolderParrentPath" path="//summary"/></param>
        /// <param name="logsFolder"><inheritdoc cref="logsFolder" path="//summary"/></param>
        /// <param name="logsExtension"><inheritdoc cref="logsExt" path="//summary"/></param>
        public FileLoggerStream(
            string logsFolderParrentPath,
            string logsFolder = Constants.DEFAULT_LOGS_FOLDER,
            string logsExtension = Constants.DEFAULT_LOG_EXT
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
        }
        #endregion

        #region Public methods
        public async Task LogTextAsync(string text, bool newLine = false)
        {
            RecreateLogsFolder();
            var currentDate = Utils.MakeDate(DateTime.Now);
            while (true)
            {
                try
                {
                    using var f = File.AppendText(Path.Join(logsFolderParrentPath, logsFolder, $"{currentDate}.{logsExt}"));
                    await f.WriteAsync($"{(newLine ? "\n" : "")}{text}");
                    break;
                }
                catch (IOException) { }
            }
        }

        public async Task LogNewLineAsync()
        {
            using var f = File.AppendText(Path.Join(logsFolderParrentPath, logsFolder, $"{Utils.MakeDate(DateTime.Now)}.{logsExt}"));
            await f.WriteAsync("\n");
        }

        public async Task WriteOutLogAsync(string text, bool newLine = false)
        {
            var currentDate = Utils.MakeDate(DateTime.Now);
            Console.Write($"{(newLine ? "\n" : "")}{Path.Join(logsFolder, $"{currentDate}.{logsExt}")} -> {text}");
        }

        public async Task LogLoggingExceptionAsync(Exception exception)
        {
            RecreateLogsFolder();
            using var f = File.AppendText(Path.Join(Constants.ROOT_FOLDER, "CRASH.log"));
            await f.WriteAsync($"\n[{Utils.MakeDate(DateTime.Now)}_{Utils.MakeTime(DateTime.Now, writeMs: true)}] [LOGGING CRASHED]\t: |{exception}|\n");
        }

        public void LogFinalLoggingException(Exception exception)
        {
            Console.WriteLine($"\n\n\n\n\n\n\n\n\n\nLogger exception level 2:\n{exception}\n\n\n");
            Console.ReadKey();
        }

        /// <summary>
        /// <c>RecreateFolder</c> for the logs folder.
        /// </summary>
        /// <returns><inheritdoc cref="Tools.RecreateFolder(string, string?, string?)"/></returns>
        public bool RecreateLogsFolder()
        {
            return Tools.RecreateFolder(logsFolder, logsFolderParrentPath);
        }
        #endregion
    }
}

namespace ProgressAdventure.Enums
{
    public enum LogSeverity : int
    {
        DEBUG = 0,
        INFO = 1,
        WARN = 2,
        ERROR = 3,
        FATAL = 4,
        PASS = 5,
        FAIL = 6,
        OTHER = 7,
        /// <summary>
        /// Logging level for REPRESENTING MINIMAL/NO LOGGING ONLY!!!<br/>
        /// DON'T USE WHEN CALLING <c>Logger.Log()</c>!!!<br/>
        /// Represents (almost) no logging.
        /// </summary>
        MINIMAL = 100,
    }
}

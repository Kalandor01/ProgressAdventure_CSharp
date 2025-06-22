namespace PACommon.Enums
{
    public enum LogSeverity : int
    {
        TRACE = 0,
        DEBUG = 1,
        INFO = 2,
        WARN = 3,
        ERROR = 4,
        FATAL = 5,
        PASS = 6,
        FAIL = 7,
        OTHER = 8,
        /// <summary>
        /// Logging level REPRESENTING MINIMAL/NO LOGGING ONLY!!!<br/>
        /// DON'T USE WHEN CALLING <c>PACSingletons.Instance.Logger.Log()</c>!!!<br/>
        /// </summary>
        DISABLED = -1,
    }
}

﻿namespace PACommon.Enums
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
        /// Logging level REPRESENTING MINIMAL/NO LOGGING ONLY!!!<br/>
        /// DON'T USE WHEN CALLING <c>PACSingletons.Instance.Logger.Log()</c>!!!<br/>
        /// </summary>
        DISABLED = -1,
    }
}

using ProgressAdventure.Enums;

namespace ProgressAdventureTests
{
    /// <summary>
    /// Struct used for storing the results of a test function.
    /// </summary>
    public struct TestResult
    {
        #region Fields
        public LogSeverity resultType;
        public string? resultMessage;
        #endregion

        #region Constructors
        /// <summary>
        /// <inheritdoc cref="TestResult"/>
        /// </summary>
        public TestResult()
            : this(LogSeverity.PASS, null) { }

        /// <summary>
        /// <inheritdoc cref="TestResult"/>
        /// </summary>
        public TestResult(LogSeverity resultType, string? resultMessage = null)
        {
            this.resultType = resultType;
            this.resultMessage = resultMessage;
        }
        #endregion
    }
}

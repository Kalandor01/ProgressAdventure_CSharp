using PACommon.Enums;

namespace PACommon
{
    /// <summary>
    /// DTO used for storing the results of a test function.
    /// </summary>
    public struct TestResultDTO
    {
        #region Fields
        /// <summary>
        /// The type of the result.
        /// </summary>
        public LogSeverity resultType;
        /// <summary>
        /// The message of the result.
        /// </summary>
        public string? resultMessage;
        #endregion

        #region Constructors
        /// <summary>
        /// <inheritdoc cref="TestResultDTO"/>
        /// </summary>
        public TestResultDTO()
            : this(LogSeverity.PASS, null) { }

        /// <summary>
        /// <inheritdoc cref="TestResultDTO"/>
        /// </summary>
        /// <param name="resultType"><inheritdoc cref="resultType" path="//summary"/></param>
        /// <param name="resultMessage"><inheritdoc cref="resultMessage" path="//summary"/></param>
        public TestResultDTO(LogSeverity resultType, string? resultMessage = null)
        {
            this.resultType = resultType;
            this.resultMessage = resultMessage;
        }
        #endregion
    }
}

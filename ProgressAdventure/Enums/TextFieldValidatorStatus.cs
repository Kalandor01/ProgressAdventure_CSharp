namespace ProgressAdventure.Enums
{
    /// <summary>
    /// Values that can be returned by <c>TextField</c>'s validator function's status.
    /// </summary>
    public enum TextFieldValidatorStatus
    {
        /// <summary>
        /// Value is valid.
        /// </summary>
        VALID,
        /// <summary>
        /// Value is invalid, but the user can retry.
        /// </summary>
        RETRY,
        /// <summary>
        /// Value is invalid, and is discarded.
        /// </summary>
        INVALID,
    }
}

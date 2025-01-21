namespace ProgressAdventure.Exceptions
{
    /// <summary>
    /// Represents an error that signals that the game should reload now.
    /// </summary>
    public class RestartException : Exception
    {
        public RestartException(string message)
            : base(message) { }
    }
}

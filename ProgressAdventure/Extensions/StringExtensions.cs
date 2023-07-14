namespace ProgressAdventure.Extensions
{
    /// <summary>
    /// Object for storing extensions for <c>String</c>.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Capitalises the first letter of the string, and lowers all others.
        /// </summary>
        /// <param name="text">The string to capitalise.</param>
        public static string Capitalize(this string text)
        {
            return text.Length > 0 ? text[..1].ToUpper() + text[1..].ToLower() : "";
        }
    }
}

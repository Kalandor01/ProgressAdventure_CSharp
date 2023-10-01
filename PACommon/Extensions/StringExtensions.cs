using System.Text;

namespace PACommon.Extensions
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

        /// <summary>
        /// Multiplies the string a specified number of times.
        /// </summary>
        /// <param name="text">The string to multiply.</param>
        /// <param name="count">How many times to repeat the string.</param>
        /// <returns>The multiplied string.</returns>
        public static string Multiply(this string text, int count)
        {
            return new StringBuilder().Insert(0, text, count).ToString();
        }
    }
}

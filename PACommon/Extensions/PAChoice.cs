using ConsoleUI.UIElements;

namespace PACommon.Extensions
{
    /// <summary>
    /// <inheritdoc cref="Choice"/>
    /// </summary>
    public class PAChoice : Choice
    {
        /// <param name="choices">The list of options the user can choose from.</param>
        /// <inheritdoc cref="Choice(IList{string}, int, string, string, bool, string, bool)"/>
        public PAChoice(
            IList<string> choices,
            int value = 0,
            string preText = "",
            string preValue = "",
            bool displayValue = false,
            string postValue = "",
            bool multiline = false
        )
            : base(
                  choices,
                  value,
                  preText + "<",
                  ">" + preValue,
                  displayValue,
                  postValue,
                  multiline
                )
        { }
    }
}

using ConsoleUI.UIElements;

namespace PACommon.Extensions
{
    /// <summary>
    /// <inheritdoc cref="MultiButton"/>
    /// </summary>
    public class OpenMultiButton : MultiButton
    {
        public string PreText { get => preText; set => preText = value; }

        public string PreValue { get => preValue; set => preValue = value; }

        public bool DisplayValue { get => displayValue; set => displayValue = value; }

        public string PostValue { get => postValue; set => postValue = value; }

        public bool Multiline { get => multiline; set => multiline = value; }

        /// <param name="choices">The list of options the user can choose from.</param>
        /// <inheritdoc cref="MultiButton(IList{MultiButtonElement}, string, IList{string}?, int, string, string, bool, bool)"/>
        public OpenMultiButton(
            IList<MultiButtonElement> buttons,
            string defaultSplitter,
            IList<string>? splitters = null,
            int value = 0,
            string preValue = "",
            string postValue = "",
            bool modifyList = false,
            bool multiline = false
        )
            : base(
                  buttons,
                  defaultSplitter,
                  splitters,
                  value,
                  preValue,
                  postValue,
                  modifyList,
                  multiline
                )
        { }
    }
}

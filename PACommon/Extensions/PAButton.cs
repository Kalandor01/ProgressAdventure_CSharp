using SaveFileManager;

namespace PACommon.Extensions
{
    /// <summary>
    /// <inheritdoc cref="Button"/>
    /// </summary>
    public class PAButton : Button
    {
        /// <param name="action">The action to invoke when the button is pressed.<br/>
        /// - If the action invokes a function, and returns false the UI will not update.<br/>
        /// - If the function returns anything other than a bool, the OptionsUI will instantly return that value.</param>
        /// <param name="modifyList">If its true, and the action invokes a function, it will get a the Button object as its first argument (and can modify it) when the function is called.</param>
        /// <inheritdoc cref="Button(UIAction, bool, string, bool)"/>
        public PAButton(
            UIAction action,
            bool modifyList = false,
            string text = "",
            bool multiline = false
        )
            : base(action, modifyList, "[" + text + "]", multiline)
        { }
    }
}

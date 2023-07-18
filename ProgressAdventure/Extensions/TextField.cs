using SaveFileManager;

namespace ProgressAdventure.Extensions
{
    /// <summary>
    /// Object for the <c>OptionsUI</c> method.<br/>
    /// When used as input in the <c>OptionsUI</c> function, it draws a field that can be selected to edit it's value in place, with the enter action.<br/>
    /// Structure: [<c>preText</c>][<c>value</c>][<c>postValue</c>]
    /// </summary>
    public class TextField : BaseUI
    {
        #region Private fields
        /// <summary>
        /// The current value of the object.
        /// </summary>
        new string? value;
        #endregion

        #region Constructors
        /// <summary>
        /// <inheritdoc cref="Toggle"/>
        /// </summary>
        /// <param name="value"><inheritdoc cref="value" path="//summary"/></param>
        /// <inheritdoc cref="BaseUI(int, string, string, bool, string, bool)"/>
        public TextField(string? value, string preText = "", string postValue = "", bool multiline = false)
            : base(-1, preText, "", false, postValue, multiline)
        {
            this.value = value;
        }
        #endregion

        #region Override methods
        /// <inheritdoc cref="BaseUI.MakeSpecial"/>
        protected override string MakeSpecial(string icons, IEnumerable<BaseUI?>? elementList = null)
        {
            return value ?? "";
        }

        /// <inheritdoc cref="BaseUI.HandleAction"/>
        public override object HandleAction(object key, IEnumerable<object> keyResults, IEnumerable<KeyAction>? keybinds = null, IEnumerable<BaseUI?>? elementList = null)
        {
            if (key.Equals(keyResults.ElementAt((int)Key.ENTER)))
            {
                // get position on console with "elementList" and element.MakeText()
                // use ANSI esc chars to get the cursor there
                // Console.ReadLine(); there
                // go back
                value = Console.ReadLine();
            }
            return true;
        }

        /// <inheritdoc cref="BaseUI.IsOnlyClickable"/>
        public override bool IsOnlyClickable()
        {
            return true;
        }
        #endregion
    }
}

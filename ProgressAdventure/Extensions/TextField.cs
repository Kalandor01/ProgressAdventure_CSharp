using SaveFileManager;
using System.Globalization;
using System.Text;

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

        /// <summary>
        /// The last used left icon.
        /// </summary>
        string lastIcon;
        /// <summary>
        /// The last used right icon.
        /// </summary>
        string lastIconR;
        /// <summary>
        /// Wether it should have an emplty string, or the old value, when editing the value.
        /// </summary>
        bool oldValueAsStartingValue;
        /// <summary>
        /// The maximum length of the input. By default it's the width of the console window. Set to -1 to set it to unlimited.
        /// </summary>
        int? maxInputLength;
        /// <summary>
        /// The function to run on the input. If it returns false, the value will not be saved.
        /// </summary>
        ValidatorDelegate? validatorFunction;
        #endregion

        #region Public delegates
        /// <summary>
        /// Afunction to return if the value the user inputed is valid or not.
        /// </summary>
        /// <param name="inputValue">The value that the user inputed.</param>
        public delegate bool ValidatorDelegate(string inputValue);
        #endregion

        #region Constructors
        /// <summary>
        /// <inheritdoc cref="Toggle"/>
        /// </summary>
        /// <param name="value"><inheritdoc cref="value" path="//summary"/></param>
        /// <param name="oldValueAsStartingValue"><inheritdoc cref="oldValueAsStartingValue" path="//summary"/></param>
        /// <param name="maxInputLength"><inheritdoc cref="maxInputLength" path="//summary"/></param>
        /// <param name="validatorFunction"><inheritdoc cref="validatorFunction" path="//summary"/></param>
        /// <inheritdoc cref="BaseUI(int, string, string, bool, string, bool)"/>
        public TextField(string? value, string preText = "", string postValue = "", bool multiline = false, bool oldValueAsStartingValue = false, int? maxInputLength = null, ValidatorDelegate? validatorFunction = null)
            : base(-1, preText, "", false, postValue, multiline)
        {
            this.value = value;
            lastIcon = "";
            lastIconR = "";
            this.oldValueAsStartingValue = oldValueAsStartingValue;
            this.maxInputLength = maxInputLength;
            this.validatorFunction = validatorFunction;
        }
        #endregion

        #region Override methods
        /// <inheritdoc cref="BaseUI.MakeText(string, string, IEnumerable{BaseUI?}?)"/>
        public override string MakeText(string icon, string iconR, IEnumerable<BaseUI?>? elementList = null)
        {
            lastIcon = icon;
            lastIconR = iconR;
            return base.MakeText(icon, iconR, elementList);
        }

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
                if (elementList == null || !elementList.Any(element => element == this))
                {
                    Console.WriteLine(preText);
                    value = Console.ReadLine();
                }
                else
                {
                    var xOffset = GetCurrentLineCharCountBeforeValue();
                    var yOffset = GetLineNumberAfterTextField(elementList);
                    Utils.MoveCursor((xOffset, yOffset));

                    var newValue = ReadInput(xOffset);
                    if (validatorFunction is null || validatorFunction.Invoke(newValue))
                    {
                        value = newValue;
                    }
                }

                return true;
            }
            return true;
        }

        /// <inheritdoc cref="BaseUI.IsOnlyClickable"/>
        public override bool IsOnlyClickable()
        {
            return true;
        }
        #endregion

        #region Private functions
        /// <summary>
        /// Gets the number of lines after the line, that this object is on in the display.
        /// </summary>
        /// <param name="elements">The list of elements passed into the <c>OptionsUI</c>, that includes this object.</param>
        private int GetLineNumberAfterTextField(IEnumerable<BaseUI?> elements)
        {
            var foundTextField = false;
            var txt = new StringBuilder();
            for (var x = 0; x < elements.Count(); x++)
            {
                var element = elements.ElementAt(x);
                if (foundTextField)
                {
                    if (element is not null && typeof(BaseUI).IsAssignableFrom(element.GetType()))
                    {
                        txt.Append(element.MakeText(
                            //selected == x ? cursorIcon.sIcon : cursorIcon.icon,
                            //selected == x ? cursorIcon.sIconR : cursorIcon.iconR,
                            "", "",
                            elements
                        ));
                    }
                    else if (element is null)
                    {
                        txt.Append('\n');
                    }
                    else
                    {
                        txt.Append(element.ToString() + "\n");
                    }
                }
                else
                {
                    if (element == this)
                    {
                        foundTextField = true;
                    }
                }
            }
            txt.Append('\n');
            return txt.ToString().Count(c => c == '\n') + 1;
        }

        /// <summary>
        /// Gets the number of characters in this object's display line string, before the value.
        /// </summary>
        private int GetCurrentLineCharCountBeforeValue()
        {
            var lineText = new StringBuilder();
            string text = lastIconR + "\n" + lastIcon;
            lineText.Append(lastIcon);
            if (multiline)
            {
                lineText.Append(preText.Replace("\n", text));
            }
            else
            {
                lineText.Append(preText);
            }
            return lineText.ToString().Length;
        }

        /// <summary>
        /// Reads user input, like <c>Console.ReadLine()</c>, but puts the <c>postValue</c> after the text, while typing.
        /// </summary>
        /// <param name="xOffset">The x offset from the left side of the console window, where the input should be placed.</param>
        private string ReadInput(int xOffset)
        {
            var postLength = postValue.Length;
            var maxLength = maxInputLength ?? Console.BufferWidth - (xOffset + postLength);
            var newValue = new StringBuilder(oldValueAsStartingValue ? value : "");
            var (Left, Top) = Console.GetCursorPosition();

            while (true)
            {
                Console.SetCursorPosition(Left, Top);
                Console.Write("\u001b[0K" + newValue.ToString() + postValue);
                Utils.MoveCursor((-postLength, 0));

                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                {
                    break;
                }
                else if (key.Key == ConsoleKey.Backspace)
                {
                    if (newValue.Length > 0)
                    {
                        newValue.Remove(newValue.Length - 1, 1);
                    }
                }
                else if (maxLength < 0 || newValue.Length < maxLength)
                {
                    newValue.Append(key.KeyChar);
                }
            }

            return newValue.ToString();
        }
        #endregion
    }
}

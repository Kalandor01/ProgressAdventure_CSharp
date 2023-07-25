using ProgressAdventure.Enums;
using SaveFileManager;
using System.Text;

namespace ProgressAdventure.Extensions
{
    /// <summary>
    /// Object for the <c>OptionsUI</c> method.<br/>
    /// When used as input in the <c>OptionsUI</c> function, it draws a field for a string, that can be selected to edit it's value in place, with the enter action.<br/>
    /// Structure: [<c>preText</c>][<c>value</c>][<c>postValue</c>]
    /// </summary>
    public class TextField : BaseUI
    {
        #region Public fields
        /// <summary>
        /// The current value of the object.
        /// </summary>
#pragma warning disable CS0108 // Hiding was intended
        public string value;
#pragma warning restore CS0108 // Hiding was intended
        /// <summary>
        /// Wether it should have an emplty string, or the old value, when editing the value.
        /// </summary>
        public bool oldValueAsStartingValue;
        /// <summary>
        /// The maximum length of the input. By default it's the width of the console window. Set to -1 to set it to unlimited.
        /// </summary>
        public int? maxInputLength;
        /// <summary>
        /// Wether to interpret string lengths as the length of the string as it will be displayed in the terminal, or just the string.Length.
        /// </summary>
        public bool lengthAsDisplayLength;
        /// <summary>
        /// The function to run on the input.
        /// </summary>
        public TextValidatorDelegate? textValidatorFunction;
        /// <summary>
        /// The function to run every keypress.
        /// </summary>
        public KeyValidatorDelegate? keyValidatorFunction;
        #endregion

        #region Public delegates
        /// <summary>
        /// A function to return the status of the vaue, the user inputed.
        /// </summary>
        /// <param name="inputValue">The value that the user inputed.</param>
        public delegate (TextFieldValidatorStatus status, string? message) TextValidatorDelegate(string inputValue);

        /// <summary>
        /// A function to return if the key the user inputed is valid or not.
        /// </summary>
        /// <param name="inputKey">The key that the user inputed.</param>
        public delegate bool KeyValidatorDelegate(ConsoleKeyInfo inputKey);
        #endregion

        #region Constructors
        /// <summary>
        /// <inheritdoc cref="TextField"/>
        /// </summary>
        /// <param name="value"><inheritdoc cref="value" path="//summary"/></param>
        /// <param name="oldValueAsStartingValue"><inheritdoc cref="oldValueAsStartingValue" path="//summary"/></param>
        /// <param name="maxInputLength"><inheritdoc cref="maxInputLength" path="//summary"/></param>
        /// <param name="lengthAsDisplayLength"><inheritdoc cref="lengthAsDisplayLength" path="//summary"/></param>
        /// <param name="textValidatorFunction"><inheritdoc cref="textValidatorFunction" path="//summary"/></param>
        /// <param name="keyValidatorFunction"><inheritdoc cref="keyValidatorFunction" path="//summary"/></param>
        /// <inheritdoc cref="BaseUI(int, string, string, bool, string, bool)"/>
        public TextField(string value, string preText = "", string postValue = "", bool multiline = false, bool oldValueAsStartingValue = false, int? maxInputLength = null, bool lengthAsDisplayLength = true, TextValidatorDelegate? textValidatorFunction = null, KeyValidatorDelegate? keyValidatorFunction = null)
            : base(-1, preText, "", false, postValue, multiline)
        {
            this.value = value;

            this.oldValueAsStartingValue = oldValueAsStartingValue;
            this.maxInputLength = maxInputLength;
            this.lengthAsDisplayLength = lengthAsDisplayLength;
            this.textValidatorFunction = textValidatorFunction;
            this.keyValidatorFunction = keyValidatorFunction;
        }
        #endregion

        #region Override methods
        /// <inheritdoc cref="BaseUI.MakeSpecial"/>
        protected override string MakeSpecial(string icons, OptionsUI? optionsUI = null)
        {
            return value;
        }

        /// <inheritdoc cref="BaseUI.HandleAction"/>
        public override object HandleAction(object key, IEnumerable<object> keyResults, IEnumerable<KeyAction>? keybinds = null, OptionsUI? optionsUI = null)
        {
            if (key.Equals(keyResults.ElementAt((int)Key.ENTER)))
            {
                if (optionsUI == null || !optionsUI.elements.Any(element => element == this))
                {
                    Console.WriteLine(preText);
                    value = Console.ReadLine() ?? "";
                }
                else
                {
                    var xOffset = GetCurrentLineCharCountBeforeValue(optionsUI.cursorIcon);
                    var yOffset = GetLineNumberAfterTextFieldValue(optionsUI);
                    Utils.MoveCursor((xOffset, yOffset));

                    bool retry;
                    do
                    {
                        retry = false;
                        var newValue = ReadInput(xOffset, optionsUI.cursorIcon);
                        if (textValidatorFunction is null)
                        {
                            value = newValue;
                        }
                        else
                        {
                            var (status, message) = textValidatorFunction(newValue);
                            if (message != null)
                            {
                                Utils.MoveCursor((-newValue.Length, 0));
                                Console.Write("\u001b[0K" + message);
                                Console.ReadKey(true);
                                Utils.MoveCursor((-message.Length, 0));
                                Console.Write("\u001b[0K" + newValue);
                                var (Left, Top) = Console.GetCursorPosition();
                                if (multiline)
                                {
                                    Console.Write(postValue.Replace("\n", optionsUI.cursorIcon.sIconR + "\n" + optionsUI.cursorIcon.sIcon));
                                }
                                else
                                {
                                    Console.Write(postValue);
                                }
                                Console.SetCursorPosition(Left, Top);
                            }
                            if (status == TextFieldValidatorStatus.VALID)
                            {
                                value = newValue;
                            }
                            else if (status == TextFieldValidatorStatus.RETRY)
                            {
                                retry = true;
                                Utils.MoveCursor((-newValue.Length, 0));
                            }
                        }
                    }
                    while (retry);
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
        /// Gets the number of lines after the value that is in this object, in the display.
        /// </summary>
        /// <param name="optionsUI">The <c>OptionsUI</c>, that includes this object.</param>
        private int GetLineNumberAfterTextFieldValue(OptionsUI optionsUI)
        {
            var foundTextField = false;
            var txt = new StringBuilder();

            // current object's line
            if (multiline)
            {
                txt.Append(postValue.Replace("\n", optionsUI.cursorIcon.sIconR + "\n" + optionsUI.cursorIcon.sIcon));
            }
            else
            {
                txt.Append(postValue);
            }
            txt.Append(optionsUI.cursorIcon.sIconR);

            // lines after current object
            for (var x = 0; x < optionsUI.elements.Count(); x++)
            {
                var element = optionsUI.elements.ElementAt(x);
                if (foundTextField)
                {
                    if (element is not null && typeof(BaseUI).IsAssignableFrom(element.GetType()))
                    {
                        txt.Append(element.MakeText(
                            optionsUI.cursorIcon.icon,
                            optionsUI.cursorIcon.iconR,
                            optionsUI
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
        /// <param name="cursorIcon">The <c>CursorIcon</c> passed into the <c>OptionsUI</c>, that includes this object.</param>
        private int GetCurrentLineCharCountBeforeValue(CursorIcon cursorIcon)
        {
            var lineText = new StringBuilder();
            lineText.Append(cursorIcon.sIcon);
            if (multiline)
            {
                lineText.Append(preText.Replace("\n", cursorIcon.sIconR + "\n" + cursorIcon.sIcon));
            }
            else
            {
                lineText.Append(preText);
            }
            var lastLine = lineText.ToString().Split("\n").Last();
            return lengthAsDisplayLength ? Utils.GetDisplayLen(lastLine) : lastLine.Length;
        }

        /// <summary>
        /// Reads user input, like <c>Console.ReadLine()</c>, but puts the <c>postValue</c> after the text, while typing.
        /// </summary>
        /// <param name="xOffset">The x offset from the left side of the console window, where the input should be placed.</param>
        /// <param name="cursorIcon">The <c>CursorIcon</c> passed into the <c>OptionsUI</c>, that includes this object.</param>
        private string ReadInput(int xOffset, CursorIcon cursorIcon)
        {
            var fullPostValue = "";
            if (multiline)
            {
                fullPostValue += postValue.Replace("\n", cursorIcon.sIconR + "\n" + cursorIcon.sIcon);
            }
            else
            {
                fullPostValue += postValue;
            }
            fullPostValue += cursorIcon.sIconR;
            fullPostValue = fullPostValue.Split("\n").First();
            var newValue = new StringBuilder(oldValueAsStartingValue ? value : "");
            var preValuePos = Console.GetCursorPosition();

            while (true)
            {
                var postLength = lengthAsDisplayLength ? Utils.GetDisplayLen(fullPostValue, xOffset + newValue.Length) : fullPostValue.Length;
                var maxLength = maxInputLength ?? Console.BufferWidth - (xOffset + postLength);
                Console.SetCursorPosition(preValuePos.Left, preValuePos.Top);
                Console.Write("\u001b[0K" + newValue.ToString());
                var (Left, Top) = Console.GetCursorPosition();
                if (multiline)
                {
                    Console.Write(postValue.Replace("\n", cursorIcon.sIconR + "\n" + cursorIcon.sIcon));
                }
                else
                {
                    Console.Write(postValue);
                }
                Console.SetCursorPosition(Left, Top);

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
                else if (
                    key.KeyChar != '\0' &&
                    key.Key != ConsoleKey.Escape &&
                    (
                        maxLength < 0 ||
                        (lengthAsDisplayLength ? Utils.GetDisplayLen(newValue.ToString() + key.KeyChar, xOffset) : newValue.Length + 1) <= maxLength
                    ) &&
                    (keyValidatorFunction is null || keyValidatorFunction(key)))
                {
                    newValue.Append(key.KeyChar);
                }
            }

            return newValue.ToString();
        }
        #endregion
    }
}

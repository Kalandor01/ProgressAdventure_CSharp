using ConsoleUI;
using ConsoleUI.Keybinds;
using ConsoleUI.UIElements;
using ConsoleUI.UIElements.EventArgs;
using PACommon.Extensions;
using System.Text;

namespace PACommon.SettingsManagement
{
    /// <summary>
    /// Object for the <c>OptionsUI</c> method.<br/>
    /// When used as input in the <c>OptionsUI</c> function, it draws a field for one keypress, that can be selected to edit it's value in place, with the enter action.<br/>
    /// Structure: [<c>preText</c>][<c>value</c>][<c>postValue</c>]
    /// </summary>
    public class KeyField<T> : BaseUI
        where T : notnull
    {
        #region Public fields
        /// <summary>
        /// The current value of the object.
        /// </summary>
        public new AActionKey<T> Value;
        /// <summary>
        /// A function to return the status of the key, the user inputed.
        /// </summary>
        public ValidatorDelegate? validatorFunction;
        /// <summary>
        /// A function to return the display value of the value of the <c>ActionKey</c>.
        /// </summary>
        public DisplayValueDelegate? displayValueFunction;
        /// <summary>
        /// The number of keys to request for the <c>ActionKey</c>.
        /// </summary>
        public int keyNum;
        /// <summary>
        /// Wether to interpret string lengths as the length of the string as it will be displayed in the terminal, or just the string.Length.
        /// </summary>
        public bool lengthAsDisplayLength;
        #endregion

        #region Override properties
        /// <inheritdoc cref="BaseUI.IsClickable"/>
        public override bool IsClickable { get => true; }

        /// <inheritdoc cref="BaseUI.IsOnlyClickable"/>
        public override bool IsOnlyClickable { get => true; }
        #endregion

        #region Public delegates
        /// <summary>
        /// <inheritdoc cref="validatorFunction" path="//summary"/>
        /// </summary>
        /// <param name="key">The key that the user inputed.</param>
        /// <param name="keyField">The <c>KeyField</c> that the called this function.</param>
        public delegate (TextFieldValidatorStatus status, string? message) ValidatorDelegate(ConsoleKeyInfo key, KeyField<T> keyField);
        /// <summary>
        /// <inheritdoc cref="displayValueFunction" path="//summary"/>
        /// </summary>
        /// <inheritdoc cref="BaseUI.MakeSpecial"/>
        /// <param name="keyField">The <c>KeyField</c>, that called the function.</param>
        public delegate string DisplayValueDelegate(KeyField<T> keyField, string icons, OptionsUI? optionsUI = null);
        #endregion

        #region Constructors
        /// <summary>
        /// <inheritdoc cref="KeyField"/>
        /// </summary>
        /// <param name="value"><inheritdoc cref="Value" path="//summary"/></param>
        /// <param name="validatorFunction"><inheritdoc cref="validatorFunction" path="//summary"/></param>
        /// <param name="displayValueFunction"><inheritdoc cref="displayValueFunction" path="//summary"/></param>
        /// <param name="keyNum"><inheritdoc cref="keyNum" path="//summary"/></param>
        /// <param name="lengthAsDisplayLength"><inheritdoc cref="lengthAsDisplayLength" path="//summary"/></param>
        /// <inheritdoc cref="BaseUI(int, string, string, bool, string, bool)"/>
        public KeyField(
            AActionKey<T> value,
            string preText = "",
            string postValue = "",
            bool multiline = false,
            ValidatorDelegate? validatorFunction = null,
            DisplayValueDelegate? displayValueFunction = null,
            int keyNum = 1,
            bool lengthAsDisplayLength = true
        )
            : base(-1, preText, "", false, postValue, multiline)
        {
            Value = value;
            this.validatorFunction = validatorFunction;
            this.displayValueFunction = displayValueFunction;
            this.keyNum = keyNum;
            this.lengthAsDisplayLength = lengthAsDisplayLength;
        }
        #endregion

        #region Override methods
        /// <inheritdoc cref="BaseUI.MakeSpecial"/>
        protected override string MakeSpecial(string icons, OptionsUI? optionsUI = null)
        {
            return displayValueFunction is not null ? displayValueFunction(this, icons, optionsUI) : string.Join(", ", Value.Names);
        }

        /// <inheritdoc cref="BaseUI.HandleAction"/>
        protected override object HandleActionProtected(UIKeyPressedEventArgs args)
        {
            if (args.pressedKey.Equals(args.keybinds.ElementAt((int)Key.ENTER)))
            {
                if (args.optionsUI == null || !args.optionsUI.elements.Any(element => element == this))
                {
                    Console.WriteLine(preText);
                    var keys = new List<ConsoleKeyInfo>();
                    for (int x = 0; x < keyNum; x++)
                    {
                        var pressedKey = Console.ReadKey();
                        keys.Add(pressedKey);
                        Console.Write(KeybindUtils.GetKeyName(pressedKey));
                        if (x < keyNum - 1)
                        {
                            Console.Write(", ");
                        }
                    }
                    Value.Keys = keys;
                }
                else
                {
                    var xOffset = GetCurrentLineCharCountBeforeValue(args.optionsUI.cursorIcon);
                    var yOffset = GetLineNumberAfterTextFieldValue(args.optionsUI);
                    Utils.MoveCursor((xOffset, yOffset));

                    var keys = new List<ConsoleKeyInfo>();

                    for (int x = 0; x < keyNum; x++)
                    {
                        bool retry;
                        do
                        {
                            retry = false;
                            var newValue = ReadInput(args.optionsUI.cursorIcon, keys);
                            if (validatorFunction is null)
                            {
                                keys.Add(newValue);
                                Value.Keys = keys;
                            }
                            else
                            {
                                var keysBak = Value.Keys.DeepCopy();
                                keys.Add(newValue);
                                Value.Keys = keys;
                                var (status, message) = validatorFunction(newValue, this);
                                if (message != null)
                                {
                                    (int preMessageLeft, int preMessageTop) = Console.GetCursorPosition();
                                    Console.Write("\u001b[0K" + message);
                                    Console.ReadKey(true);
                                    Console.SetCursorPosition(preMessageLeft, preMessageTop);
                                    Console.Write("\u001b[0K");
                                    var (Left, Top) = Console.GetCursorPosition();
                                    if (multiline)
                                    {
                                        Console.Write(postValue.Replace("\n", args.optionsUI.cursorIcon.sIconR + "\n" + args.optionsUI.cursorIcon.sIcon));
                                    }
                                    else
                                    {
                                        Console.Write(postValue);
                                    }
                                    Console.SetCursorPosition(Left, Top);
                                }
                                if (status != TextFieldValidatorStatus.VALID)
                                {
                                    Value.Keys = keysBak;
                                    keys = keysBak.ToList();
                                    if (status == TextFieldValidatorStatus.RETRY)
                                    {
                                        retry = true;
                                    }
                                }
                            }
                        }
                        while (retry);
                    }
                }

                return true;
            }
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

            // get displayed range
            int endIndex;
            if (optionsUI.scrollSettings.maxElements == -1 || optionsUI.scrollSettings.maxElements >= optionsUI.elements.Count())
            {
                endIndex = optionsUI.elements.Count();
            }
            else
            {
                endIndex = Math.Clamp(optionsUI.startIndex + optionsUI.scrollSettings.maxElements, 0, optionsUI.elements.Count());
            }

            // lines after current object
            for (var x = optionsUI.selected + 1; x < endIndex; x++)
            {
                var element = optionsUI.elements.ElementAt(x);
                if (element is not null)
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
            txt.Append(endIndex == optionsUI.elements.Count() ? optionsUI.scrollSettings.scrollIcon.bottomEndIndicator : optionsUI.scrollSettings.scrollIcon.bottomContinueIndicator);
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
        /// Reads user input, like <c>Console.ReadKey()</c>, but puts the <c>postValue</c> after the text, while typing.
        /// </summary>
        /// <param name="cursorIcon">The <c>CursorIcon</c> passed into the <c>OptionsUI</c>, that includes this object.</param>
        /// <param name="keys">The keys, that already exist.</param>
        private ConsoleKeyInfo ReadInput(CursorIcon cursorIcon, List<ConsoleKeyInfo> keys)
        {
            Console.Write("\u001b[0K");
            var preValuePos = Console.GetCursorPosition();

            foreach (var key in keys)
            {
                Console.Write(KeybindUtils.GetKeyName(key) + ", ");
            }

            var (Left, Top) = Console.GetCursorPosition();
            if (multiline)
            {
                Console.Write(postValue.Replace("\n", cursorIcon.sIconR + "\n" + cursorIcon.sIcon));
            }
            else
            {
                Console.Write(postValue);
            }
            Console.Write(cursorIcon.sIconR);

            Console.SetCursorPosition(Left, Top);
            var pressedKey = Console.ReadKey(true);
            Console.SetCursorPosition(preValuePos.Left, preValuePos.Top);
            return pressedKey;
        }
        #endregion
    }
}

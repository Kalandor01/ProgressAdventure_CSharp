using System.Text;
using ConsoleUI;
using ConsoleUI.Keybinds;
using ConsoleUI.UIElements;
using ConsoleUI.UIElements.EventArgs;
using PACommon.Extensions;

namespace PACommon.SettingsManagement
{
    /// <summary>
    /// Object for the <see cref="OptionsUI"/> method.<br/>
    /// When used as input in the <see cref="OptionsUI"/> function, it draws a field for one keypress, that can be selected to edit its value in place, with the enter action.<br/>
    /// Structure: [<see cref="BaseUI.preText"/>][<see cref="BaseUI.Value"/>][<see cref="BaseUI.postValue"/>]
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
        /// A function to return the display value of the value of the <see cref="AActionKey{T}"/>.
        /// </summary>
        public DisplayValueDelegate? displayValueFunction;
        /// <summary>
        /// The number of keys to request for the <see cref="AActionKey{T}"/>.
        /// </summary>
        public int keyNum;
        /// <summary>
        /// Wether to interpret string lengths as the length of the string as it will be displayed in the terminal, or just the string.Length.
        /// </summary>
        public bool lengthAsDisplayLength;
        #endregion

        #region Override properties
        /// <inheritdoc cref="BaseUI.IsClickable"/>
        public override bool IsClickable => true;
        
        /// <inheritdoc cref="BaseUI.IsOnlyClickable"/>
        public override bool IsOnlyClickable => true;
        #endregion

        #region Public delegates
        /// <summary>
        /// <inheritdoc cref="validatorFunction" path="//summary"/>
        /// </summary>
        /// <param name="key">The key that the user inputed.</param>
        /// <param name="keyField">The <see cref="KeyField{T}"/> that called this function.</param>
        public delegate (TextFieldValidatorStatus status, string? message) ValidatorDelegate(ConsoleKeyInfo key, KeyField<T> keyField);
        /// <summary>
        /// <inheritdoc cref="displayValueFunction" path="//summary"/>
        /// </summary>
        /// <inheritdoc cref="BaseUI.MakeSpecial"/>
        /// <param name="keyField">The <see cref="KeyField{T}"/> that called this function.</param>
        public delegate string DisplayValueDelegate(KeyField<T> keyField, string icons, OptionsUI? optionsUI = null);
        #endregion

        #region Constructors
        /// <summary>
        /// <inheritdoc cref="KeyField{T}"/>
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
            if (!args.pressedKey.Equals(args.keybinds.ElementAt((int)Key.ENTER)))
            {
                return true;
            }

            var consoleProxy = args.optionsUI?.consoleProxy ?? new ConsoleProxy();
            var keys = new List<ConsoleKeyInfo>();
            if (args.optionsUI is null || !args.optionsUI.elements.Contains(this))
            {
                consoleProxy.WriteLine(preText);
                for (var x = 0; x < keyNum; x++)
                {
                    var pressedKey = consoleProxy.ReadKey();
                    keys.Add(pressedKey);
                    consoleProxy.Write(KeybindUtils.GetKeyName(pressedKey));
                    if (x < keyNum - 1)
                    {
                        consoleProxy.Write(", ");
                    }
                }
                Value.Keys = keys;
                return true;
            }

            var xOffset = GetCurrentLineCharCountBeforeValue(args.optionsUI.cursorIcon);
            var yOffset = GetLineNumberAfterTextFieldValue(args.optionsUI);
            consoleProxy.MoveCursor(xOffset, yOffset);

            for (var x = 0; x < keyNum; x++)
            {
                bool retry;
                do
                {
                    retry = false;
                    var newValue = ReadInput(consoleProxy, args.optionsUI.cursorIcon, keys);
                    if (validatorFunction is null)
                    {
                        keys.Add(newValue);
                        Value.Keys = keys;
                        continue;
                    }

                    var keysBak = Value.Keys.DeepCopy();
                    keys.Add(newValue);
                    Value.Keys = keys;
                    var (status, message) = validatorFunction(newValue, this);
                    if (message is not null)
                    {
                        var (preMessageCol, preMessageRow) = consoleProxy.GetCursorPosition();
                        consoleProxy.Write("\e[0K" + message);
                        consoleProxy.ReadKey(false);
                        consoleProxy.WriteAtPosition("\e[0K", preMessageCol, preMessageRow);
                        var (column, row) = consoleProxy.GetCursorPosition();
                        consoleProxy.Write(
                            multiline
                                ? postValue.Replace("\n", args.optionsUI.cursorIcon.sIconR + "\n" + args.optionsUI.cursorIcon.sIcon)
                                : postValue
                        );
                        consoleProxy.SetCursorPosition(column, row);
                    }

                    if (status == TextFieldValidatorStatus.VALID)
                    {
                        continue;
                    }

                    Value.Keys = keysBak;
                    keys = [.. keysBak];
                    if (status == TextFieldValidatorStatus.RETRY)
                    {
                        retry = true;
                    }
                }
                while (retry);
            }
            return true;
        }
        #endregion
        
        #region Private functions
        /// <summary>
        /// Gets the number of lines after the value that is in this object, in the display.
        /// </summary>
        /// <param name="optionsUI">The <see cref="OptionsUI"/>, that includes this object.</param>
        private int GetLineNumberAfterTextFieldValue(OptionsUI optionsUI)
        {
            var txt = new StringBuilder();
            
            // current object's line
            txt.Append(
                multiline
                    ? postValue.Replace("\n", optionsUI.cursorIcon.sIconR + "\n" + optionsUI.cursorIcon.sIcon)
                    : postValue
            );
            txt.Append(optionsUI.cursorIcon.sIconR);
            
            // get displayed range
            var endIndex = optionsUI.scrollSettings.maxElements != -1 && optionsUI.scrollSettings.maxElements < optionsUI.elements.Count
                ? Math.Clamp(optionsUI.startIndex + optionsUI.scrollSettings.maxElements, 0, optionsUI.elements.Count)
                : optionsUI.elements.Count;
            
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
                    txt.Append(element + "\n");
                }
            }
            txt.Append(
                endIndex == optionsUI.elements.Count
                    ? optionsUI.scrollSettings.scrollIcon.bottomEndIndicator
                    : optionsUI.scrollSettings.scrollIcon.bottomContinueIndicator
            );
            txt.Append('\n');
            
            return txt.ToString().Count(c => c == '\n') + 1;
        }
        
        /// <summary>
        /// Gets the number of characters in this object's display line string, before the value.
        /// </summary>
        /// <param name="cursorIcon">The <see cref="CursorIcon"/> passed into the <see cref="OptionsUI"/>, that includes this object.</param>
        private int GetCurrentLineCharCountBeforeValue(CursorIcon cursorIcon)
        {
            var lineText = new StringBuilder();
            lineText.Append(cursorIcon.sIcon);
            lineText.Append(
                multiline ? preText.Replace("\n", cursorIcon.sIconR + "\n" + cursorIcon.sIcon) : preText
            );
            var lastLine = lineText.ToString().Split("\n").Last();
            return lengthAsDisplayLength ? Utils.GetDisplayLen(lastLine) : lastLine.Length;
        }
        
        /// <summary>
        /// Reads user input, like <see cref="IConsoleProxy.ReadKey(bool)"/>, but puts the <see cref="BaseUI.postValue"/> after the text, while typing.
        /// </summary>
        /// <param name="consoleProxy">The <see cref="IConsoleProxy"/> to use.</param>
        /// <param name="cursorIcon">The <see cref="CursorIcon"/> passed into the <see cref="OptionsUI"/>, that includes this object.</param>
        /// <param name="keys">The keys, that already exist.</param>
        private ConsoleKeyInfo ReadInput(IConsoleProxy consoleProxy, CursorIcon cursorIcon, List<ConsoleKeyInfo> keys)
        {
            consoleProxy.Write("\e[0K");
            var (prewCol, prewRow) = consoleProxy.GetCursorPosition();

            foreach (var key in keys)
            {
                consoleProxy.Write(KeybindUtils.GetKeyName(key) + ", ");
            }

            var (column, row) = consoleProxy.GetCursorPosition();
            consoleProxy.Write(
                multiline ? postValue.Replace("\n", cursorIcon.sIconR + "\n" + cursorIcon.sIcon) : postValue
            );
            consoleProxy.Write(cursorIcon.sIconR);

            consoleProxy.SetCursorPosition(column, row);
            var pressedKey = consoleProxy.ReadKey();
            consoleProxy.SetCursorPosition(prewCol, prewRow);
            return pressedKey;
        }
        #endregion
    }
}

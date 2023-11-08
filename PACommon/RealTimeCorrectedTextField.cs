using PACommon.Extensions;
using SaveFileManager;
using System.Text;

namespace PACommon
{
    /// <summary>
    /// Displays a <c>TextField</c>, where the user can input a string, that will be corrected as the user types it.<br/>
    /// DOESN'T INHERIT FROM <c>BaseUI</c>!
    /// </summary>
    public class RealTimeCorrectedTextField
    {
        #region Public delegates
        /// <summary>
        /// A function to return the corrected verson of a string, that the user inputed.
        /// </summary>
        /// <param name="rawText">The raw user input to correct.</param>
        public delegate string StringCorrectorDelegate(string? rawText);
        #endregion

        #region Private fields
        /// <summary>
        /// The text to display before the part, where the user inputs the string.
        /// </summary>
        private string _preValue;
        /// <summary>
        /// The string corrector function, used in displaying the corrected value of the input string.
        /// </summary>
        private StringCorrectorDelegate _stringCorrector;
        /// <summary>
        /// The text displayed immediately after the user's inputed text.
        /// </summary>
        private string _postValue;
        /// <summary>
        /// The starting value of the input field.
        /// </summary>
        private string _startingValue;
        /// <summary>
        /// Whether to "clear" the screen, before displaying the text.
        /// </summary>
        private bool _clearScreen;
        /// <summary>
        /// Temporary field for the string corrector <c>TextField</c>.
        /// </summary>
        private TextField correctorTextField;
        /// <summary>
        /// The baseUIDisplay to use.
        /// </summary>
        private BaseUIDisplay baseUIDisplay;
        #endregion

        #region Public Properties
        /// <summary>
        /// <inheritdoc cref="_preValue" path="//summary"/>
        /// </summary>
        public string PreValue
        {
            get
            {
                return _preValue;
            }
            set
            {
                _preValue = value;
                UpdateTextField();
            }
        }
        /// <summary>
        /// <inheritdoc cref="_stringCorrector" path="//summary"/>
        /// </summary>
        public StringCorrectorDelegate StringCorrector
        {
            get
            {
                return _stringCorrector;
            }
            set
            {
                _stringCorrector = value;
                UpdateTextField();
            }
        }
        /// <summary>
        /// <inheritdoc cref="_postValue" path="//summary"/>
        /// </summary>
        public string PostValue
        {
            get
            {
                return _postValue;
            }
            set
            {
                _postValue = value;
                UpdateTextField();
            }
        }
        /// <summary>
        /// <inheritdoc cref="_startingValue" path="//summary"/>
        /// </summary>
        public string StartingValue
        {
            get
            {
                return _startingValue;
            }
            set
            {
                _startingValue = value;
                UpdateTextField();
            }
        }
        /// <summary>
        /// <inheritdoc cref="_clearScreen" path="//summary"/>
        /// </summary>
        public bool ClearScreen
        {
            get
            {
                return _clearScreen;
            }
            set
            {
                _clearScreen = value;
                UpdateBaseUI();
            }
        }
        #endregion

        /// <summary>
        /// <inheritdoc cref="RealTimeCorrectedTextField" path="//summary"/>
        /// </summary>
        /// <param name="preValue"><inheritdoc cref="_preValue" path="//summary"/></param>
        /// <param name="stringCorrector"><inheritdoc cref="_stringCorrector" path="//summary"/></param>
        /// <param name="postValue"><inheritdoc cref="_postValue" path="//summary"/></param>
        /// <param name="startingValue"><inheritdoc cref="_startingValue" path="//summary"/></param>
        /// <param name="clearScreen"><inheritdoc cref="_clearScreen" path="//summary"/></param>
        public RealTimeCorrectedTextField(
            string preValue,
            StringCorrectorDelegate stringCorrector,
            string postValue = "",
            string startingValue = "",
            bool clearScreen = true
        )
        {
            _preValue = preValue;
            _postValue = postValue;
            _startingValue = startingValue;
            _clearScreen = clearScreen;

            _stringCorrector = stringCorrector;

            UpdateTextField();
            UpdateBaseUI();
        }

        #region Public methods
        /// <summary>
        /// Displays the <c>TextField</c>.
        /// </summary>
        /// <param name="keybinds">The list of KeyAction objects to use, if the selected action is a UIList.</param>
        /// <param name="keyResults">The list of posible results returned by pressing a key.<br/>
        /// The order of the elements in the list should be:<br/>
        /// - escape, up, down, left, right, enter<br/>
        /// If it is null, the default value is either returned from the keybinds or:<br/>
        /// - { Key.ESCAPE, Key.UP, Key.DOWN, Key.LEFT, Key.RIGHT, Key.ENTER }</param>
        /// <returns>The uncorrected version of the final string.</returns>
        public string GetString(IEnumerable<KeyAction>? keybinds = null, IEnumerable<object>? keyResults = null)
        {
            baseUIDisplay.Display(keybinds, keyResults);
            Console.WriteLine();
            return correctorTextField.Value;
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Updates the TextField object.
        /// </summary>
        private void UpdateTextField()
        {
            var correctorPostValue = PostValue + " -> " + StringCorrector(_startingValue);
            if (correctorTextField is null)
            {
                correctorTextField = new TextField(
                    "",
                    PreValue,
                    correctorPostValue,
                    oldValueAsStartingValue: true,
                    keyValidatorFunction: new TextField.KeyValidatorDelegate(StringCorrectorKeyValidator),
                    overrideDefaultKeyValidatorFunction: false
                );
            }
            else
            {
                correctorTextField.PreText = PreValue;
                correctorTextField.PostValue = correctorPostValue;
            }
        }

        /// <summary>
        /// Updates the BaseUIDisplay object.
        /// </summary>
        private void UpdateBaseUI()
        {
            if (baseUIDisplay is null)
            {
                baseUIDisplay = new BaseUIDisplay(correctorTextField, autoEnter: true, clearScreen: ClearScreen);
            }
            else
            {
                baseUIDisplay.Element = correctorTextField;
                baseUIDisplay.clearScreen = ClearScreen;
            }
        }

        private bool StringCorrectorKeyValidator(StringBuilder text, ConsoleKeyInfo? key, int cursorPosition)
        {
            string newText = "";
            if (key is null)
            {
                if (text.Length > 0)
                {
                    var sbText = text.ToString();
                    newText = sbText[0..cursorPosition] + sbText[(cursorPosition + 1)..];
                }
            }
            else
            {
                newText = text.ToString().Insert(cursorPosition, key?.KeyChar.ToString() ?? "");
            }

            correctorTextField.PostValue = " -> " + StringCorrector(newText);
            return true;
        }
        #endregion
    }
}

using ConsoleUI;
using ConsoleUI.Keybinds;
using ConsoleUI.UIElements;
using System.Text;
using CUIUtils = ConsoleUI.Utils;

namespace PACommon.Extensions
{
    /// <summary>
    /// An object for displaying 1 <see cref="BaseUI"/> object.
    /// </summary>
    public class BaseUIDisplay
    {
        #region Private fields
        private readonly OptionsUI? optionsUI;
        private readonly string? title;
        private readonly CursorIcon cursorIcon;
        private readonly bool canEscape;
        public bool autoEnter;
        public bool clearScreen;
        public IConsoleProxy consoleProxy;
        #endregion

        #region Public properties
        public BaseUI Element
        {
            get;
            set
            {
                field = value;
                optionsUI?.elements = [field];
            }
        }
        #endregion

        #region Public constructors
        /// <summary>
        /// Creates an object for displaying a <see cref="BaseUI"/> object, where a fake <see cref="OptionsUI"/> object is also created.
        /// </summary>
        /// <param name="element">The <see cref="BaseUI"/> to use.</param>
        /// <param name="createOptionsUIObject">Whether to create a fake <see cref="OptionsUI"/>.</param>
        /// <param name="title">The string to print before the element.</param>
        /// <param name="cursorIcon">The cursor icon to use. By default, it uses <see cref="Constants.NO_CURSOR_ICONS"/>.</param>
        /// <param name="canEscape">Allows the user to press the key associated with escape, to exit the menu.</param>
        /// <param name="autoEnter">If true, an enter action will be simulated immediately, and after exiting the <see cref="BaseUI.HandleAction(KeyAction, IEnumerable{KeyAction}, CUIUtils.GetKeyFunctionDelegate, OptionsUI?)"/>, the function will immediately return.</param>
        /// <param name="clearScreen">Whether to "clear" the screen, before displaying the element.</param>
        /// <param name="consoleProxy">The <see cref="IConsoleProxy"/> to use.</param>
        public BaseUIDisplay(
            BaseUI element,
            bool createOptionsUIObject = true,
            string? title = null,
            CursorIcon? cursorIcon = null,
            bool canEscape = true,
            bool autoEnter = false,
            bool clearScreen = true,
            IConsoleProxy? consoleProxy = null
        )
        {
            cursorIcon ??= Constants.NO_CURSOR_ICONS;
            consoleProxy ??= new ConsoleProxy();
            if (createOptionsUIObject)
            {
                optionsUI = new OptionsUI([element], title, cursorIcon, canEscape, true, consoleProxy: consoleProxy);
            }
            Element = element;

            this.title = title;
            this.cursorIcon = cursorIcon;
            this.canEscape = canEscape;
            this.autoEnter = autoEnter;
            this.clearScreen = clearScreen;
            this.consoleProxy = consoleProxy;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Displays the UI element.
        /// </summary>
        /// <param name="keybinds">The list of KeyAction objects to use. The order of the actions should be:<br/>
        /// - escape, up, down, left, right, enter.</param>
        /// <param name="getKeyFunction">The function to get the next valid key the user pressed.<br/>
        /// Should function similarly to <see cref="CUIUtils.GetKey(GetKeyMode, IEnumerable{KeyAction}?, IConsoleProxy?)"/>.</param>
        /// <exception cref="UINoSelectablesExeption">Trown, if the UI element is not selectable.</exception>
        public object? Display(IEnumerable<KeyAction>? keybinds = null, CUIUtils.GetKeyFunctionDelegate? getKeyFunction = null)
        {
            if (!Element.IsSelectable)
            {
                throw new UINoSelectablesExeption();
            }

            if (keybinds is null || keybinds.Count() < 6)
            {
                keybinds = CUIUtils.GetDefaultKeybinds();
            }

            getKeyFunction ??= (mode, keybinds) => CUIUtils.GetKey(mode, keybinds, consoleProxy);

            // is enter needed?
            var enterKeyNeeded = Element.IsClickable;

            // render/getkey loop
            var firstLoop = true;
            KeyAction pressedKey;
            do
            {
                // prevent infinite loop
                if (!Element.IsSelectable)
                {
                    throw new UINoSelectablesExeption();
                }

                // clear screen + render
                var txt = new StringBuilder(clearScreen ? "\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n" : "");

                // title
                if (title is not null)
                {
                    txt.Append(title + "\n\n");
                }

                // element
                txt.Append(Element.MakeText(
                    cursorIcon.sIcon,
                    cursorIcon.sIconR,
                    optionsUI
                ));

                consoleProxy.WriteLine(txt);

                // move selection/change value
                var actualMove = false;
                do
                {
                    // get pressedKey
                    pressedKey = keybinds.ElementAt((int)Key.ENTER);

                    // auto enter
                    if (!(firstLoop && autoEnter))
                    {
                        if (
                            Element.IsClickable &&
                            Element.IsOnlyClickable
                        )
                        {
                            pressedKey = getKeyFunction(GetKeyMode.IGNORE_HORIZONTAL, keybinds);
                        }
                        else
                        {
                            while (pressedKey.Equals(keybinds.ElementAt((int)Key.ENTER)))
                            {
                                pressedKey = getKeyFunction(GetKeyMode.IGNORE_VERTICAL, keybinds);
                                if (pressedKey.Equals(keybinds.ElementAt((int)Key.ENTER)) && !enterKeyNeeded)
                                {
                                    pressedKey = keybinds.ElementAt((int)Key.ESCAPE);
                                }
                            }
                        }
                    }
                    firstLoop = false;

                    // change value
                    if (
                        Element.IsSelectable &&
                        (
                            pressedKey.Equals(keybinds.ElementAt((int)Key.LEFT)) ||
                            pressedKey.Equals(keybinds.ElementAt((int)Key.RIGHT)) ||
                            pressedKey.Equals(keybinds.ElementAt((int)Key.ENTER)))
                        )
                    {
                        var returned = Element.HandleAction(pressedKey, keybinds, getKeyFunction, optionsUI);
                        if (returned is not null)
                        {
                            if (returned.GetType() == typeof(bool))
                            {
                                actualMove = (bool)returned;
                            }
                            else
                            {
                                return returned;
                            }
                        }
                    }
                    else if (canEscape && pressedKey.Equals(keybinds.ElementAt((int)Key.ESCAPE)))
                    {
                        actualMove = true;
                    }
                }
                while (!(actualMove || autoEnter));
            }
            while ((!canEscape || !pressedKey.Equals(keybinds.ElementAt((int)Key.ESCAPE))) && !autoEnter);
            return null;
        }
        #endregion
    }
}

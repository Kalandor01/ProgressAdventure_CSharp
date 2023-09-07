using SaveFileManager;
using System.Text;
using SFMUtils = SaveFileManager.Utils;

namespace PACommon.Extensions
{
    /// <summary>
    /// An object for displaying 1 <c>BaseUI</c> object.
    /// </summary>
    public class BaseUIDisplay
    {
        #region Private fields
        private BaseUI _element;
        private readonly OptionsUI? optionsUI;
        private readonly string? title;
        private readonly CursorIcon cursorIcon;
        private readonly bool canEscape;
        private readonly bool autoEnter;
        private readonly bool clearScreen;
        #endregion

        #region Public properties
        public BaseUI Element
        {
            get => _element;
            set
            {
                _element = value;
                if (optionsUI is not null)
                {
                    optionsUI.elements = new List<BaseUI> { _element };
                }
            }
        }
        #endregion

        #region Public constructors
        /// <summary>
        /// Creates an object for displaying a <c>BaseUI</c> object, where a fake <c>OptionsUI</c> object is also created.
        /// </summary>
        /// <param name="element">The <c>BaseUI</c> object to use.</param>
        /// <param name="createOptionsUIObject">Whether to create a fake <c>OptionsUI</c> object.</param>
        /// <param name="title">The string to print before the element.</param>
        /// <param name="cursorIcon">The cursor icon to use. By default, it uses the <c>Constants.NO_CURSOR_ICONS</c>.</param>
        /// <param name="canEscape">Allows the user to press the key associated with escape, to exit the menu.</param>
        /// <param name="autoEnter">If true, an enter action will be simulated immediately, and after exiting the <c>HandleAction</c>, the function will immediately return.</param>
        /// <param name="clearScreen">Whether to "clear" the screen, before displaying the element, or not.</param>
        public BaseUIDisplay(BaseUI element, bool createOptionsUIObject = true, string? title = null, CursorIcon? cursorIcon = null, bool canEscape = true, bool autoEnter = false, bool clearScreen = true)
        {
            cursorIcon ??= Constants.NO_CURSOR_ICONS;
            if (createOptionsUIObject)
            {
                optionsUI = new OptionsUI(new List<BaseUI?> { element }, title, cursorIcon, canEscape, true);
            }
            Element = element;

            this.title = title;
            this.cursorIcon = cursorIcon;
            this.canEscape = canEscape;
            this.autoEnter = autoEnter;
            this.clearScreen = clearScreen;
        }
        #endregion

        #region Public methods
        public object? Display(IEnumerable<KeyAction>? keybinds = null, IEnumerable<object>? keyResults = null)
        {
            if (!Element.IsSelectable())
            {
                throw new UINoSelectablesExeption();
            }

            var firstLoop = true;

            if (keyResults is null || keyResults.Count() < 6)
            {
                if (keybinds is null)
                {
                    keyResults = new List<object> { Key.ESCAPE, Key.UP, Key.DOWN, Key.LEFT, Key.RIGHT, Key.ENTER };
                }
                else
                {
                    keyResults = SFMUtils.GetResultsList(keybinds);
                }
            }

            // is enter needed?
            var enterKeyNeeded = Element.IsClickable();

            // render/getkey loop
            object pressedKey;
            do
            {
                // prevent infinite loop
                if (!Element.IsSelectable())
                {
                    throw new UINoSelectablesExeption();
                }

                // clear screen + render
                var txt = new StringBuilder(clearScreen ? "\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n" : "");

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

                Console.WriteLine(txt);

                // move selection/change value
                var actualMove = false;
                do
                {
                        // get pressedKey
                        pressedKey = keyResults.ElementAt((int)Key.ENTER);

                    // auto enter
                    if (!(firstLoop && autoEnter))
                    {
                        if (
                            Element.IsClickable() &&
                            Element.IsOnlyClickable()
                        )
                        {
                            pressedKey = SFMUtils.GetKey(GetKeyMode.IGNORE_HORIZONTAL, keybinds);
                        }
                        else
                        {
                            while (pressedKey.Equals(keyResults.ElementAt((int)Key.ENTER)))
                            {
                                pressedKey = SFMUtils.GetKey(GetKeyMode.IGNORE_VERTICAL, keybinds);
                                if (pressedKey.Equals(keyResults.ElementAt((int)Key.ENTER)) && !enterKeyNeeded)
                                {
                                    pressedKey = keyResults.ElementAt((int)Key.ESCAPE);
                                }
                            }
                        }
                    }
                    firstLoop = false;

                    // change value
                    if (
                        Element.IsSelectable() &&
                        (
                            pressedKey.Equals(keyResults.ElementAt((int)Key.LEFT)) ||
                            pressedKey.Equals(keyResults.ElementAt((int)Key.RIGHT)) ||
                            pressedKey.Equals(keyResults.ElementAt((int)Key.ENTER)))
                        )
                    {
                        var returned = Element.HandleAction(pressedKey, keyResults, keybinds, optionsUI);
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
                    else if (canEscape && pressedKey.Equals(keyResults.ElementAt((int)Key.ESCAPE)))
                    {
                        actualMove = true;
                    }
                }
                while (!(actualMove || autoEnter));
            }
            while ((!canEscape || !pressedKey.Equals(keyResults.ElementAt((int)Key.ESCAPE))) && !autoEnter);
            return null;
        }
        #endregion
    }
}

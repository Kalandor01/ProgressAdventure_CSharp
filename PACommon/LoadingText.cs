namespace PACommon
{
    /// <summary>
    /// Displays a loading progress with an optional spinner.<br/>
    /// The loading text will be displayed as: [PRE SPINNER][SPINNER][POST SPINNER][FORMATED VALUE / OVERWRITE][POST VALUE]
    /// </summary>
    public class LoadingText
    {
        #region Private fields
        private static readonly char[] spinnerChars = ['|', '/', '-', '\\'];

        private int startPosX = 0;
        private int startPosY = 0;
        private int spinnerPosX = -1;
        private int spinnerPosY = -1;
        private int postSpinnerPosX = -1;
        private int postSpinnerPosY = -1;
        private int valuePosX = -1;
        private int valuePosY = -1;
        private int postValuePosX = -1;
        private int postValuePosY = -1;

        private bool preSpinnerChanged = true;
        private bool postSpinnerChanged = true;
        private bool valueChanged = true;
        private bool postValueChanged = true;

        private int spinnerCharIndex = 0;
        private bool isLooping = false;
        private string? valueOverwrite = null;
        #endregion

        #region Public properties

        /// <summary>
        /// If the loading text is currently displayed.
        /// </summary>
        public bool IsLoading { get; private set; } = false;

        /// <summary>
        /// If the screen is currently updating.
        /// </summary>
        public bool IsUpdating { get; private set; } = false;

        /// <summary>
        /// The delay between spinner updates. If -1, the spinner doesn't show up.
        /// </summary>
        public int SpinnerRefreshDelay { get; set; }

        /// <summary>
        /// The text before the spinner.
        /// </summary>
        public string PreSpinner {
            get;
            set
            {
                if (field == value)
                {
                    return;
                }
                field = value;
                preSpinnerChanged = true;
                postSpinnerChanged = true;
                valueChanged = true;
                postValueChanged = true;
            }
        }

        /// <summary>
        /// The text between the spinner and the value.
        /// </summary>
        public string PostSpinner
        {
            get;
            set
            {
                if (field == value)
                {
                    return;
                }
                field = value;
                postSpinnerChanged = true;
                valueChanged = true;
                postValueChanged = true;
            }
        }

        /// <summary>
        /// The format string to format the value with.
        /// </summary>
        public string ValueFormat
        {
            get;
            set
            {
                if (field == value)
                {
                    return;
                }
                field = value;
                valueChanged = true;
                postValueChanged = true;
            }
        }

        /// <summary>
        /// The loading value. If -1, the value doesn't show up.
        /// </summary>
        public double? Value
        {
            get;
            set
            {
                if (field == value)
                {
                    return;
                }
                field = value;
                valueChanged = true;
                postValueChanged = true;
            }
        }

        /// <summary>
        /// The text after the value.
        /// </summary>
        public string PostValue
        {
            get;
            set
            {
                if (field == value)
                {
                    return;
                }
                field = value;
                postValueChanged = true;
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// <inheritdoc cref="LoadingText" path="//summary"/>
        /// </summary>
        /// <param name="spinnerRefreshDelay"><inheritdoc cref="SpinnerRefreshDelay" path="//summary"/></param>
        /// <param name="preSpinner"><inheritdoc cref="PreSpinner" path="//summary"/></param>
        /// <param name="postSpinner"><inheritdoc cref="PostSpinner" path="//summary"/></param>
        /// <param name="valueFormat"><inheritdoc cref="ValueFormat" path="//summary"/></param>
        /// <param name="value"><inheritdoc cref="Value" path="//summary"/></param>
        /// <param name="postValue"><inheritdoc cref="PostValue" path="//summary"/></param>
        public LoadingText(
            string preSpinner,
            string postSpinner,
            string postValue = "",
            double? value = 0,
            int spinnerRefreshDelay = 100,
            string valueFormat = ""
        )
        {
            SpinnerRefreshDelay = spinnerRefreshDelay;
            PreSpinner = preSpinner;
            PostSpinner = postSpinner;
            ValueFormat = valueFormat;
            Value = value;
            PostValue = postValue;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Displays the loading text and starts updating it.
        /// </summary>
        public void Display()
        {
            IsLoading = true;
            spinnerCharIndex = 0;
            preSpinnerChanged = true;
            valueOverwrite = null;
            (startPosX, startPosY) = Console.GetCursorPosition();
            spinnerPosX = -1;
            spinnerPosY = -1;
            postSpinnerPosX = -1;
            postSpinnerPosY = -1;
            valuePosX = -1;
            valuePosY = -1;
            postValuePosX = -1;
            postValuePosY = -1;

            new Thread(UpdateLoop).Start();
        }

        /// <summary>
        /// Manualy updates the loading text.
        /// </summary>
        public void Update()
        {
            UpdatePrivate(false);
        }

        /// <summary>
        /// Stops the updating of the loading text.
        /// </summary>
        /// <param name="finalOverwriteValue">The text to replace the value text with.</param>
        /// <param name="wait">Whether to wait for the auto update loop to finish before returning.</param>
        public void StopLoading(string? finalOverwriteValue = null, bool wait = true)
        {
            if (!IsLoading)
            {
                return;
            }

            IsLoading = false;
            var originalRefreshDelay = SpinnerRefreshDelay;
            SpinnerRefreshDelay = 1;
            if (finalOverwriteValue is not null)
            {
                valueOverwrite = finalOverwriteValue;
                valueChanged = true;
                postValueChanged = true;
                UpdatePrivate(false, true);
            }
            while (isLooping && wait)
            {

            }
            SpinnerRefreshDelay = originalRefreshDelay;
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Updates the loading text.
        /// </summary>
        /// <param name="updateSpinner">Whether to progress the spinner.</param>
        /// <param name="forceUpdate">Whether to wait for the current update to complete, or just stop this update.</param>
        private void UpdatePrivate(bool updateSpinner, bool forceUpdate = false)
        {
            if (IsUpdating)
            {
                if (!forceUpdate)
                {
                    return;
                }
                while (IsUpdating)
                {

                }
            }

            IsUpdating = true;
            var (currentPosX, currentPosY) = Console.GetCursorPosition();
            if (preSpinnerChanged)
            {
                Console.SetCursorPosition(startPosX, startPosY);
                Console.Write(PreSpinner);
                (spinnerPosX, spinnerPosY) = Console.GetCursorPosition();
            }

            if (updateSpinner || preSpinnerChanged)
            {
                if (!preSpinnerChanged)
                {
                    Console.SetCursorPosition(spinnerPosX, spinnerPosY);
                }

                Console.Write(spinnerChars[spinnerCharIndex].ToString());
                if (preSpinnerChanged)
                {
                    (postSpinnerPosX, postSpinnerPosY) = Console.GetCursorPosition();
                }
                if (updateSpinner)
                {
                    spinnerCharIndex = (spinnerCharIndex + 1) % spinnerChars.Length;
                }
            }

            if (postSpinnerChanged)
            {
                if (!preSpinnerChanged)
                {
                    Console.SetCursorPosition(postSpinnerPosX, postSpinnerPosY);
                }
                Console.Write(PostSpinner);
                (valuePosX, valuePosY) = Console.GetCursorPosition();
            }

            if (valueChanged)
            {
                if (!postSpinnerChanged)
                {
                    Console.SetCursorPosition(valuePosX, valuePosY);
                }
                var valueStr = valueOverwrite
                    ?? (Value is null
                        ? ""
                        : ((double)Value).ToString(ValueFormat));
                Console.Write(valueStr);
                (postValuePosX, postValuePosY) = Console.GetCursorPosition();
            }

            if (postValueChanged)
            {
                if (!valueChanged)
                {
                    Console.SetCursorPosition(postValuePosX, postValuePosY);
                }
                Console.Write(PostValue);
            }

            Console.SetCursorPosition(currentPosX, currentPosY);
            preSpinnerChanged = false;
            postSpinnerChanged = false;
            valueChanged = false;
            postValueChanged = false;
            IsUpdating = false;
        }

        /// <summary>
        /// The mainly spinner updating loop.
        /// </summary>
        private void UpdateLoop()
        {
            isLooping = true;
            while (IsLoading)
            {
                if (SpinnerRefreshDelay > 0)
                {
                    UpdatePrivate(true);
                    Thread.Sleep(SpinnerRefreshDelay);
                }
                else
                {
                    Thread.Sleep(10);
                }
            }
            isLooping = false;
        }
        #endregion
    }
}

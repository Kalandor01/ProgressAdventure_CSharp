using SaveFileManager;

namespace ProgressAdventure.Extensions
{
    /// <summary>
    /// A version of the <c>SaveFileManager</c> <c>Slider</c>, that allways displays a customisable display name, for each value.
    /// </summary>
    public class AdvancedSlider : Slider
    {
        #region Private Fields
        /// <summary>
        /// The array of strings to display, for the actual values of the Slider.
        /// </summary>
        private readonly IEnumerable<string> _displayValues;
        #endregion

        #region Constructors
        /// <summary>
        /// <inheritdoc cref="AdvancedSlider"/>
        /// </summary>
        /// <param name="displayValues"><inheritdoc cref="_displayValues" path="//summary"/></param>
        /// <exception cref="ArgumentException">Thrown, if <c>displayValues</c> doesn't contain any values!</exception>
        /// <inheritdoc cref="Slider(int, int, int, int, string, string, string, string, bool, string, bool)"/>
        public AdvancedSlider(IEnumerable<string> displayValues, int value = 0, string preText = "", string symbol = "#", string symbolEmpty = "-", string preValue = "", string postValue = "", bool multiline = false)
            : base(0, displayValues.Count() - 1, 1, value, preText, symbol, symbolEmpty, preValue, true, postValue, multiline)
        {
            if (!displayValues.Any())
            {
                throw new ArgumentException($"{nameof(displayValues)} doesn't contain any values!");
            }
            _displayValues = displayValues;

            this.value = Math.Clamp(value, 0, displayValues.Count() - 1);
        }
        #endregion

        #region Overrides
        protected override string MakeValue()
        {
            return _displayValues.ElementAt(value);
        }
        #endregion
    }
}

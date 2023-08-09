using ProgressAdventure.Enums;

namespace ProgressAdventure.ItemManagement
{
    /// <summary>
    /// Abstract class for an item.
    /// </summary>
    public abstract class AItem : IJsonReadable
    {
        #region Protected fields
        /// <summary>
        /// The number of items.
        /// </summary>
        protected double _amount;
        #endregion

        #region Public properties
        /// <summary>
        /// The type of the item.
        /// </summary>
        public ItemTypeID Type { get; protected set; }

        /// <summary>
        /// The material, the item is (mainly) made out of.
        /// </summary>
        public Material Material { get; protected set; }

        /// <summary>
        /// <inheritdoc cref="_amount"/>
        /// </summary>
        public double Amount {
            get
            {
                return _amount;
            }
            set
            {
                _amount = value;

                if (_amount < 0)
                {
                    _amount = 0;
                }
                else
                {
                    if (Unit == ItemAmountUnit.AMOUNT)
                    {
                        _amount = Math.Floor(_amount);
                    }
                    else
                    {
                        _amount = Math.Round(_amount, Constants.ITEM_AMOUNT_ROUNDING_DIGITS);
                    }
                }
            }
        }

        /// <summary>
        /// The display name of the item.
        /// </summary>
        public string DisplayName { get; protected set; }

        /// <summary>
        /// <inheritdoc cref="ItemAmountUnit"/>
        /// </summary>
        public ItemAmountUnit Unit { get; protected set; }
        #endregion

        #region JsonConvert
        public abstract Dictionary<string, object?> ToJson();
        #endregion
    }
}

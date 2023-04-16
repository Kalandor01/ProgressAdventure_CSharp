using ProgressAdventure.Enums;

namespace ProgressAdventure.Entity
{
    public class Item
    {
        #region Public fields
        /// <summary>
        /// The number of items
        /// </summary>
        public long amount;
        #endregion

        #region Public properties
        /// <summary>
        /// The type of the item.
        /// </summary>
        public ItemType Type { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// <inheritdoc cref="Item"/>
        /// </summary>
        /// <param name="type"><inheritdoc cref="Type" path="//summary"/></param>
        /// <param name="amount"><inheritdoc cref="amount" path="//summary"/></param>
        public Item(ItemType type, long amount = 1)
        {
            Type = type;
            this.amount = amount;
        }
        #endregion

        #region Public overrides
        public override bool Equals(object? obj)
        {
            return obj is Item item && Type == item.Type;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type);
        }
        #endregion
    }
}

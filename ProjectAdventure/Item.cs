using ProjectAdventure.Enums;

namespace ProjectAdventure
{
    public class Item
    {
        #region Public fields and properties
        /// <summary>
        /// The type of the item.
        /// </summary>
        public ItemType type { get; }
        /// <summary>
        /// The number of items
        /// </summary>
        public long amount;
        #endregion

        #region Constructors
        /// <summary>
        /// <inheritdoc cref="Item"/>
        /// </summary>
        /// <param name="type"><inheritdoc cref="type" path="//summary"/></param>
        /// <param name="amount"><inheritdoc cref="amount" path="//summary"/></param>
        public Item(ItemType type, long amount = 1)
        {
            this.type = type;
            this.amount = amount;
        }
        #endregion

        #region Public overrides
        public override bool Equals(object? obj)
        {
            return obj is Item item && type == item.type;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(type);
        }
        #endregion
    }
}

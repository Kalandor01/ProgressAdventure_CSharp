namespace ProgressAdventure.ItemManagement
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
        public ItemTypeID Type { get; }

        /// <summary>
        /// The display name of the item.
        /// </summary>
        public string DisplayName { get; private set; }

        /// <summary>
        /// If the amount of the item gets decreased, if it gets used.
        /// </summary>
        public bool Consumable { get; private set; }
        #endregion

        #region Constructors
        /// <summary>
        /// <inheritdoc cref="Item"/>
        /// </summary>
        /// <param name="type"><inheritdoc cref="Type" path="//summary"/></param>
        /// <param name="amount"><inheritdoc cref="amount" path="//summary"/></param>
        public Item(ItemTypeID type, long amount = 1)
        {
            Type = type;
            this.amount = amount;
            SetAttributes();
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Sets the item's attributes, based on its type.
        /// </summary>
        public void SetAttributes()
        {
            if (ItemUtils.itemAttributes.ContainsKey(Type))
            {
                var (displayName, consumable) = ItemUtils.itemAttributes[Type];
                DisplayName = displayName;
                Consumable = consumable;
            }
            else
            {
                var name = Type.ToString();
                if (name is null)
                {
                    Logger.Log("Unknown item", $"type: {Type}, amount: {amount}");
                }
                else
                {
                    name = name.Split('.').Last();
                    name = name[0].ToString().ToUpper() + name[1..].ToLower();
                    name = name.Replace("_", " ");
                }
                DisplayName = name ?? "[UNKNOWN ITEM]";
                Consumable = false;
            }
        }

        /// <summary>
        /// Tries to use the item, and returns the success.
        /// </summary>
        public bool Use()
        {
            if (amount > 0)
            {
                if (Consumable)
                {
                    amount--;
                }
                return true;
            }
            return false;
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

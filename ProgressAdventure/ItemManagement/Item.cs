using ProgressAdventure.Enums;

namespace ProgressAdventure.ItemManagement
{
    public class Item : IJsonConvertable<Item>
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
            string? displayNameValue = null;
            bool? consumableValue = null;
            if (ItemUtils.itemAttributes.TryGetValue(Type, out ItemAttributes attributes))
            {
                displayNameValue = attributes.displayName;
                consumableValue = attributes.consumable;
            }

            DisplayName = displayNameValue ?? ItemUtils.ItemIDToDisplayName(Type);
            Consumable = consumableValue ?? false;
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

        public override string? ToString()
        {
            return $"{DisplayName}{(amount > 1 ? " x" + amount.ToString() : "")}";
        }
        #endregion

        #region JsonConvert
        public Dictionary<string, object?> ToJson()
        {
            return new Dictionary<string, object?>
            {
                ["type"] = ItemUtils.itemAttributes[Type].typeName,
                ["amount"] = amount,
            };
        }

        public static Item? FromJson(IDictionary<string, object?>? itemJson)
        {
            if (itemJson is null)
            {
                Logger.Log("Item parse error", "item json is null", LogSeverity.ERROR);
                return null;
            }

            if (
                itemJson.TryGetValue("type", out var typeNameValue) &&
                ItemUtils.TryParseItemType(typeNameValue?.ToString(), out ItemTypeID itemType)
            )
            {
                int itemAmount = 1;
                if (
                    itemJson.TryGetValue("amount", out var amountValue) &&
                    int.TryParse(amountValue?.ToString(), out itemAmount)
                )
                {
                    if (itemAmount < 1)
                    {
                        Logger.Log("Item parse error", "invalid item amount in item json (amount < 1)", LogSeverity.WARN);
                        return null;
                    }
                }
                else
                {
                    Logger.Log("Item parse error", "couldn't parse item amount from json, defaulting to 1", LogSeverity.WARN);
                }
                return new Item(itemType, itemAmount);
            }
            else
            {
                Logger.Log("Item parse error", "couldn't parse item type from json", LogSeverity.ERROR);
                return null;
            }
        }
        #endregion
    }
}

using ProgressAdventure.Enums;

namespace ProgressAdventure.ItemManagement
{
    public class Item : IJsonConvertable<Item>
    {
        #region Private dicts
        /// <summary>
        /// The dictionary pairing up item type IDs, to their name.
        /// </summary>
        private static readonly Dictionary<int, string> _itemTypeNameMap = new()
        {
            //weapons
            [65536] = "weapon/wooden_sword",
            [65537] = "weapon/stone_sword",
            [65538] = "weapon/steel_sword",
            [65539] = "weapon/wooden_bow",
            [65540] = "weapon/steel_arrow",
            [65541] = "weapon/wooden_club",
            [65542] = "weapon/club_with_teeth",
            //defence
            [65792] = "defence/wooden_shield",
            [65793] = "defence/leather_cap",
            [65794] = "defence/leather_tunic",
            [65795] = "defence/leather_pants",
            [65796] = "defence/leather_boots",
            //materials
            [66048] = "material/bootle",
            [66049] = "material/wool",
            [66050] = "material/cloth",
            [66051] = "material/wood",
            [66052] = "material/stone",
            [66053] = "material/steel",
            [66054] = "material/gold",
            [66055] = "material/teeth",
            //misc
            [66304] = "misc/health_potion",
            [66305] = "misc/gold_coin",
            [66306] = "misc/silver_coin",
            [66307] = "misc/copper_coin",
            [66308] = "misc/rotten_flesh",
        };
        #endregion

        #region Public fields
        /// <summary>
        /// The number of items.
        /// </summary>
        private long _amount;
        #endregion

        #region Public properties
        /// <summary>
        /// The type of the item.
        /// </summary>
        public ItemTypeID Type { get; }

        /// <summary>
        /// <inheritdoc cref="_amount"/>
        /// </summary>
        public long Amount {
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
            }
        }

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
        /// <param name="amount"><inheritdoc cref="Amount" path="//summary"/></param>
        /// <exception cref="ArgumentException">Thrown if the item type is an unknown item type id.</exception>
        public Item(ItemTypeID type, long amount = 1)
        {
            var typeValue = ItemUtils.ToItemType(type.GetHashCode());
            if (typeValue is null)
            {
                Logger.Log("Unknown item type", $"id: {type.GetHashCode()}", LogSeverity.ERROR);
                throw new ArgumentException("Unknown item type", nameof(type));
            }
            Type = (ItemTypeID)typeValue;
            Amount = amount;
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
            if (Amount > 0)
            {
                if (Consumable)
                {
                    Amount--;
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
            return $"{DisplayName}{(Amount > 1 ? " x" + Amount.ToString() : "")}";
        }
        #endregion

        #region JsonConvert
        public Dictionary<string, object?> ToJson()
        {
            string typeName;
            if (ItemUtils.itemAttributes.TryGetValue(Type, out ItemAttributes attributes))
            {
                typeName = attributes.typeName;
            }
            else
            {
                typeName = "[UNKNOWN TYPE NAME]";
                Logger.Log("Item to json", $"item type doesn't have a type name, type:{Type}", LogSeverity.ERROR);
            }

            return new Dictionary<string, object?>
            {
                ["type"] = typeName,
                ["amount"] = Amount,
            };
        }

        public static Item? FromJson(IDictionary<string, object?>? itemJson, string fileVersion)
        {
            if (itemJson is null)
            {
                Logger.Log("Item parse error", "item json is null", LogSeverity.ERROR);
                return null;
            }

            //correct data
            if (!Tools.IsUpToDate(Constants.SAVE_VERSION, fileVersion))
            {
                Logger.Log($"Item json data is old", "correcting data");
                // 2.0.2 -> 2.1
                var newFileVersion = "2.1";
                if (!Tools.IsUpToDate(newFileVersion, fileVersion))
                {
                    // inventory items in dictionary
                    if (
                        itemJson.TryGetValue("type", out var typeIDValue) &&
                        int.TryParse(typeIDValue?.ToString(), out int itemID) &&
                        _itemTypeNameMap.TryGetValue(itemID, out string? itemName))
                    {
                        itemJson["type"] = itemName;
                    }

                    Logger.Log("Corrected item json data", $"{fileVersion} -> {newFileVersion}", LogSeverity.DEBUG);
                    fileVersion = newFileVersion;
                }
                Logger.Log($"Item json data corrected");
            }

            //convert
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

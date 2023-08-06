using ProgressAdventure.Enums;
using ProgressAdventure.Extensions;

namespace ProgressAdventure.ItemManagement
{
    public class Item : IJsonConvertable<Item>
    {
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
        /// The material, the item is made out of.
        /// </summary>
        public Material? Material { get; }

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
        /// <param name="material">The material of the item.</param>
        /// <param name="amount"><inheritdoc cref="Amount" path="//summary"/></param>
        /// <exception cref="ArgumentException">Thrown if the item type is an unknown item type id, or no material was provided, when it was required.</exception>
        public Item(ItemTypeID type, Material? material, long amount = 1)
        {
            var typeValue = ItemUtils.ToItemType(type.GetHashCode());
            if (typeValue is null)
            {
                Logger.Log("Unknown item type", $"id: {type.GetHashCode()}", LogSeverity.ERROR);
                throw new ArgumentException("Unknown item type", nameof(type));
            }

            if (
                material is null &&
                ItemUtils.itemAttributes.TryGetValue((ItemTypeID)typeValue, out ItemAttributesDTO? itemAttributes) &&
                itemAttributes.canHaveMaterial
            )
            {
                Logger.Log("Item material was not provided, but is required", $"item type: {typeValue}", LogSeverity.ERROR);
                throw new ArgumentException("Required material not provided", nameof(material));
            }

            Type = (ItemTypeID)typeValue;
            Material = material;
            Amount = amount;
            SetAttributes();
        }
        #endregion

        #region Public methods
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

        #region Private methods
        /// <summary>
        /// Sets the item's attributes, based on its type.
        /// </summary>
        private void SetAttributes()
        {
            var attributes = ItemUtils.itemAttributes[Type];
            DisplayName = attributes.displayName;
            Consumable = attributes.consumable;
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
            var type = Material is null ? "" : Material.ToString()?.Capitalize().Replace("_", " ");
            var name = Type == ItemType.Misc.MATERIAL ? "" : ((Material is null ? "" : " ") + DisplayName);
            return $"{type}{name}{(Amount > 1 ? " x" + Amount.ToString() : "")}";
        }
        #endregion

        #region JsonConvert
        public Dictionary<string, object?> ToJson()
        {
            string typeName;
            if (ItemUtils.itemAttributes.TryGetValue(Type, out ItemAttributesDTO? attributes))
            {
                typeName = attributes.typeName;
            }
            else
            {
                typeName = ItemUtils.ItemIDToTypeName(Type);
                Logger.Log("Item to json", $"item type doesn't have a type name, type:{Type}", LogSeverity.ERROR);
            }

            return new Dictionary<string, object?>
            {
                ["type"] = typeName,
                ["material"] = Material?.ToString(),
                ["amount"] = Amount,
            };
        }

        public static bool FromJson(IDictionary<string, object?>? itemJson, string fileVersion, out Item? itemObject)
        {
            itemObject = null;
            if (itemJson is null)
            {
                Logger.Log("Item parse error", "item json is null", LogSeverity.ERROR);
                return false;
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
                        ItemUtils._legacyItemTypeNameMap.TryGetValue(itemID, out string? itemName))
                    {
                        itemJson["type"] = itemName;
                    }

                    Logger.Log("Corrected item json data", $"{fileVersion} -> {newFileVersion}", LogSeverity.DEBUG);
                    fileVersion = newFileVersion;
                }
                // 2.1.1 -> 2.2
                newFileVersion = "2.2";
                if (!Tools.IsUpToDate(newFileVersion, fileVersion))
                {
                    // item material
                    if (
                        itemJson.TryGetValue("type", out var typeValue) &&
                        ItemUtils._legacyItemNameMaterialMap.TryGetValue(typeValue?.ToString() ?? "", out (string itemType, string? material) newItemattributes))
                    {
                        itemJson["type"] = newItemattributes.itemType;
                        itemJson["material"] = newItemattributes.material;
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
                Material? material = null;
                if (
                    itemJson.TryGetValue("material", out var materialValue)
                )
                {
                    if (
                        materialValue is not null &&
                        Enum.TryParse(materialValue?.ToString()?.ToUpper(), out Material materialParsed)
                    )
                    {
                        material = materialParsed;
                    }
                    else if (materialValue is not null)
                    {
                        Logger.Log("Item parse error", "invalid material type in item json, defaulting to no material", LogSeverity.WARN);
                    }
                }
                else
                {
                    Logger.Log("Item parse error", "couldn't parse material type from json", LogSeverity.WARN);
                }

                int itemAmount = 1;
                if (
                    itemJson.TryGetValue("amount", out var amountValue) &&
                    int.TryParse(amountValue?.ToString(), out itemAmount)
                )
                {
                    if (itemAmount < 1)
                    {
                        Logger.Log("Item parse error", "invalid item amount in item json (amount < 1)", LogSeverity.WARN);
                        return false;
                    }
                }
                else
                {
                    Logger.Log("Item parse error", "couldn't parse item amount from json, defaulting to 1", LogSeverity.WARN);
                }

                try
                {
                    itemObject = new Item(itemType, material, itemAmount);
                }
                catch (Exception ex)
                {
                    Logger.Log("Failed to create an item, from json", ex.ToString(), LogSeverity.ERROR);
                    return false;
                }
                return true;
            }
            else
            {
                Logger.Log("Item parse error", "couldn't parse item type from json", LogSeverity.ERROR);
                return false;
            }
        }
        #endregion
    }
}

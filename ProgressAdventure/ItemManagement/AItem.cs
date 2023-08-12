using ProgressAdventure.Enums;

namespace ProgressAdventure.ItemManagement
{
    /// <summary>
    /// Abstract class for an item.
    /// </summary>
    public abstract class AItem : IJsonConvertable<AItem>
    {
        #region Protected fields
        /// <summary>
        /// The number of items.
        /// </summary>
        protected double _amount;

        /// <summary>
        /// The mass of the item, if the amount is 1.
        /// </summary>
        protected double? _massMultiplier;

        /// <summary>
        /// The volume of the item, if the amount is 1.
        /// </summary>
        protected double? _volumeMultiplier;
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

        /// <summary>
        /// <inheritdoc cref="_massMultiplier" path="//summary"/>
        /// </summary>
        public double MassMultiplier
        {
            get
            {
                return _massMultiplier ?? GetMassMultiplier();
            }
        }

        /// <summary>
        /// <inheritdoc cref="_volumeMultiplier" path="//summary"/>
        /// </summary>
        public double VolumeMultiplier
        {
            get
            {
                return _volumeMultiplier ?? GetVolumeMultiplier();
            }
        }

        /// <summary>
        /// The total mass of the item in KGs.
        /// </summary>
        public double Mass
        {
            get
            {
                return MassMultiplier * Amount;
            }
        }

        /// <summary>
        /// The total volume of the item in M3s.
        /// </summary>
        public double Volume
        {
            get
            {
                return VolumeMultiplier * Amount;
            }
        }
        #endregion

        #region Protected methods
        /// <summary>
        /// Gets the mass of the item, if the amount is 1.
        /// </summary>
        protected abstract double GetMassMultiplier();

        /// <summary>
        /// Gets the volume of the item, if the amount is 1.
        /// </summary>
        protected abstract double GetVolumeMultiplier();
        #endregion

        #region Public overrides
        public override bool Equals(object? obj)
        {
            if (base.Equals(obj))
            {
                return true;
            }

            if (obj is not null && obj.GetType() == obj.GetType())
            {
                var item = (AItem)obj;
                return Type == item.Type && Material == item.Material;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Material, Amount);
        }

        public override string? ToString()
        {
            var amount = (Amount != 1 || Unit != ItemAmountUnit.AMOUNT) && Amount > 0 ?
                " x" + Amount.ToString() + (Unit == ItemAmountUnit.AMOUNT ? "" : " " + Unit.ToString().ToLower()) :
                "";
            return $"{DisplayName}{amount}";
        }
        #endregion

        #region JsonConvert
        public virtual Dictionary<string, object?> ToJson()
        {
            return new Dictionary<string, object?>
            {
                ["type"] = Type == ItemUtils.MATERIAL_ITEM_TYPE ? ItemUtils.MATERIAL_TYPE_NAME : ItemUtils.ItemIDToTypeName(Type),
                ["material"] = Material.ToString(),
                ["amount"] = Amount,
            };
        }

        public static bool FromJson(IDictionary<string, object?>? itemJson, string fileVersion, out AItem? itemObject)
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
                bool success;
                if (itemType == ItemUtils.MATERIAL_ITEM_TYPE)
                {
                    success = MaterialItem.FromJson(itemJson, fileVersion, out MaterialItem? materialObj);
                    itemObject = materialObj;
                    return success;
                }

                success = CompoundItem.FromJson(itemJson, fileVersion, out CompoundItem? compoundObj);
                itemObject = compoundObj;
                return success;
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

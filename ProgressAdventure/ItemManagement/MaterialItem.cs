using ProgressAdventure.Enums;
using ProgressAdventure.Extensions;

namespace ProgressAdventure.ItemManagement
{
    /// <summary>
    /// Class storing a material as an item.
    /// </summary>
    public class MaterialItem : AItem, IJsonConvertable<MaterialItem>
    {
        #region Public constructors
        /// <summary>
        /// <inheritdoc cref="MaterialItem"/>
        /// </summary>
        /// <param name="material">The material.</param>
        /// <param name="amount"><inheritdoc cref="Amount" path="//summary"/></param>
        public MaterialItem(Material material, double amount = 1)
        {
            Type = ItemUtils.MATERIAL_ITEM_TYPE;
            Material = material;

            SetAttributes();

            Amount = amount;
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
                // DO THIS WITH TAGS!

                //if (Consumable)
                //{
                    Amount--;
                //}
                return true;
            }
            return false;
        }
        #endregion

        #region Protected methods
        protected override double GetMassMultiplier()
        {
            if (Unit == ItemAmountUnit.KG)
            {
                return 1;
            }

            // L / M3 (/ amount???)
            return ItemUtils.materialProperties[Material].density * (Unit == ItemAmountUnit.L ? 0.001 : 1);
        }

        protected override double GetVolumeMultiplier()
        {
            if (Unit == ItemAmountUnit.M3)
            {
                return 1;
            }

            if (Unit == ItemAmountUnit.L)
            {
                return 0.001;
            }

            // KG (/ amount???)
            return 1 / ItemUtils.materialProperties[Material].density;
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Sets the item's attributes, based on its type.
        /// </summary>
        private void SetAttributes()
        {
            var attributes = ItemUtils.materialItemAttributes[Material];
            Unit = attributes.unit;
            DisplayName = attributes.displayName;
        }
        #endregion

        #region JsonConvert
        public static bool FromJson(IDictionary<string, object?>? itemJson, string fileVersion, out MaterialItem? itemObject)
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
                    if (itemJson.TryGetValue("type", out var typeValue) &&
                        ItemUtils._legacyMaterialItemMap.TryGetValue(typeValue?.ToString() ?? "", out string? materialItemFixed)
                    )
                    {
                        itemJson["type"] = "misc/material";
                        itemJson["material"] = materialItemFixed;
                    }

                    Logger.Log("Corrected item json data", $"{fileVersion} -> {newFileVersion}", LogSeverity.DEBUG);
                    fileVersion = newFileVersion;
                }
                Logger.Log($"Item json data corrected");
            }

            //convert
            Material material;
            if (
                itemJson.TryGetValue("material", out var materialValue)
            )
            {
                if (Enum.TryParse(materialValue?.ToString()?.ToUpper(), out Material materialParsed))
                {
                    material = materialParsed;
                }
                else
                {
                    Logger.Log("Item parse error", "invalid material type in item json", LogSeverity.ERROR);
                    return false;
                }
            }
            else
            {
                Logger.Log("Item parse error", "couldn't parse material type from json", LogSeverity.ERROR);
                return false;
            }

            double itemAmount = 1;
            if (
                itemJson.TryGetValue("amount", out var amountValue) &&
                double.TryParse(amountValue?.ToString(), out itemAmount)
            )
            {
                if (itemAmount <= 0)
                {
                    Logger.Log("Item parse error", "invalid item amount in item json (amount <= 0)", LogSeverity.ERROR);
                    return false;
                }
            }
            else
            {
                Logger.Log("Item parse error", "couldn't parse item amount from json, defaulting to 1", LogSeverity.WARN);
            }

            try
            {
                itemObject = new MaterialItem(material, itemAmount);
            }
            catch (Exception ex)
            {
                Logger.Log("Failed to create an item, from json", ex.ToString(), LogSeverity.ERROR);
                return false;
            }
            return true;
        }
        #endregion
    }
}

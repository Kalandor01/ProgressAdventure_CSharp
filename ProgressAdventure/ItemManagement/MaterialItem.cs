using PACommon;
using PACommon.Enums;
using PACommon.JsonUtils;
using ProgressAdventure.Enums;

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
                //TODO: DO THIS WITH TAGS!

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
            var densityMultiplier = Unit == ItemAmountUnit.L ? 0.001 : 1;
            return ItemUtils.materialProperties[Material].density * densityMultiplier;
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
        static List<(Action<IDictionary<string, object?>> objectJsonCorrecter, string newFileVersion)> IJsonConvertable<MaterialItem>.VersionCorrecters { get; } = new()
        {
            // 2.0.2 -> 2.1
            (oldJson =>
            {
                // inventory items in dictionary
                if (
                    oldJson.TryGetValue("type", out var typeIDValue) &&
                    int.TryParse(typeIDValue?.ToString(), out int itemID) &&
                    ItemUtils._legacyItemTypeNameMap.TryGetValue(itemID, out string? itemName))
                {
                    oldJson["type"] = itemName;
                }
            }, "2.1"),
            // 2.1.1 -> 2.2
            (oldJson =>
            {
                // item material
                if (oldJson.TryGetValue("type", out var typeValue) &&
                    ItemUtils._legacyMaterialItemMap.TryGetValue(typeValue?.ToString() ?? "", out string? materialItemFixed)
                )
                {
                    oldJson["type"] = "misc/material";
                    oldJson["material"] = materialItemFixed;
                }
            }, "2.2"),
        };

        static bool IJsonConvertable<MaterialItem>.FromJsonWithoutCorrection(IDictionary<string, object?> itemJson, string fileVersion, ref MaterialItem? itemObject)
        {
            Material material;
            if (!itemJson.TryGetValue("material", out var materialValue))
            {
                Logger.Instance.Log("Item parse error", "couldn't parse material type from json", LogSeverity.ERROR);
                return false;
            }

            if (!Enum.TryParse(materialValue?.ToString()?.ToUpper(), out Material materialParsed))
            {
                Logger.Instance.Log("Item parse error", "invalid material type in item json", LogSeverity.ERROR);
                return false;
            }
            material = materialParsed;

            double itemAmount = 1;
            if (
                !itemJson.TryGetValue("amount", out var amountValue) ||
                !double.TryParse(amountValue?.ToString(), out itemAmount)
            )
            {
                Logger.Instance.Log("Item parse error", "couldn't parse item amount from json, defaulting to 1", LogSeverity.WARN);
            }

            if (itemAmount <= 0)
            {
                Logger.Instance.Log("Item parse error", "invalid item amount in item json (amount <= 0)", LogSeverity.ERROR);
                return false;
            }

            try
            {
                itemObject = new MaterialItem(material, itemAmount);
            }
            catch (Exception ex)
            {
                Logger.Instance.Log("Failed to create an item, from json", ex.ToString(), LogSeverity.ERROR);
                return false;
            }

            return true;
        }
        #endregion
    }
}

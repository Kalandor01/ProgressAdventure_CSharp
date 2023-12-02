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
            var success = true;

            success &= Tools.TryParseJsonValue<MaterialItem, Material?>(itemJson, Constants.JsonKeys.AItem.MATERIAL, out var material, true);
            if (material is null)
            {
                return false;
            }

            success &= Tools.TryParseJsonValue<MaterialItem, double?>(itemJson, Constants.JsonKeys.AItem.AMOUNT, out var itemAmount);
            if (itemAmount is null)
            {
                Tools.LogJsonError<MaterialItem>("couldn't parse item amount from json, defaulting to 1");
            }

            if (itemAmount <= 0)
            {
                Tools.LogJsonError<MaterialItem>("invalid item amount in item json (amount <= 0)", true);
                return false;
            }

            try
            {
                itemObject = new MaterialItem((Material)material, itemAmount ?? 1);
            }
            catch (Exception ex)
            {
                Tools.LogJsonError<MaterialItem>($"failed to create an object, from json: {ex}", true);
                return false;
            }

            return success;
        }
        #endregion
    }
}

using PACommon.JsonUtils;
using ProgressAdventure.Enums;
using System.Diagnostics.CodeAnalysis;
using PACTools = PACommon.Tools;

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
        static List<(Action<JsonDictionary> objectJsonCorrecter, string newFileVersion)> IJsonConvertable<MaterialItem>.VersionCorrecters { get; } =
        [
            // 2.0.2 -> 2.1
            (oldJson =>
            {
                // inventory items in dictionary
                if (
                    oldJson.TryGetValue("type", out var typeIDValue) &&
                    int.TryParse(typeIDValue?.Value.ToString(), out var itemID) &&
                    ItemUtils._legacyItemTypeNameMap.TryGetValue(itemID, out var itemName))
                {
                    oldJson["type"] = itemName;
                }
            }, "2.1"),
            // 2.1.1 -> 2.2
            (oldJson =>
            {
                // item material
                if (oldJson.TryGetValue("type", out var typeValue) &&
                    ItemUtils._legacyMaterialItemMap.TryGetValue(typeValue?.Value.ToString() ?? "", out var materialItemFixed)
                )
                {
                    oldJson["type"] = "misc/material";
                    oldJson["material"] = materialItemFixed;
                }
            }, "2.2"),
        ];

        static bool IJsonConvertable<MaterialItem>.FromJsonWithoutCorrection(JsonDictionary itemJson, string fileVersion, [NotNullWhen(true)] ref MaterialItem? itemObject)
        {
            var success = true;

            success &= PACTools.TryParseJsonValue<MaterialItem, Material?>(
                itemJson,
                Constants.JsonKeys.AItem.MATERIAL,
                out var material,
                isCritical: true
            );
            if (material is null)
            {
                return false;
            }

            success &= PACTools.TryParseJsonValue<MaterialItem, double?>(itemJson, Constants.JsonKeys.AItem.AMOUNT, out var itemAmount);
            if (itemAmount is null)
            {
                PACTools.LogJsonError<MaterialItem>("defaulting to 1");
            }

            if (itemAmount <= 0)
            {
                PACTools.LogJsonError<MaterialItem>("invalid item amount in item json (amount <= 0)", true);
                return false;
            }

            try
            {
                itemObject = new MaterialItem((Material)material, itemAmount ?? 1);
            }
            catch (Exception ex)
            {
                PACTools.LogJsonError<MaterialItem>($"failed to create an object, from json: {ex}", true);
                return false;
            }

            return success;
        }
        #endregion
    }
}

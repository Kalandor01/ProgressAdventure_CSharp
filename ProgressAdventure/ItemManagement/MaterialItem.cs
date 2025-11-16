using System.Diagnostics.CodeAnalysis;
using PACommon.Enums;
using PACommon.JsonUtils;
using ProgressAdventure.ConfigManagement;
using ProgressAdventure.Enums;
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
        /// <param name="amount"><inheritdoc cref="AItem.Amount" path="//summary"/></param>
        public MaterialItem(EnumValue<Material> material, double amount = 1)
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
            if (!(Amount > 0))
            {
                return false;
            }
            
            //TODO: DO THIS WITH TAGS!

            //if (Consumable)
            //{
            Amount--;
            //}
            return true;
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
            return ItemUtils.MaterialItemAttributes[Material].properties.density * densityMultiplier;
        }

        protected override double GetVolumeMultiplier()
        {
            return Unit switch
            {
                ItemAmountUnit.M3 => 1,
                ItemAmountUnit.L => 0.001,
                // KG (/ amount???)
                _ => 1 / ItemUtils.MaterialItemAttributes[Material].properties.density
            };
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Sets the item's attributes, based on its type.
        /// </summary>
        private void SetAttributes()
        {
            var attributes = ItemUtils.MaterialItemAttributes[Material];
            Unit = attributes.unit;
            DisplayName = PASingletons.Instance.Localizer.GetLocalizedString(attributes.displayName);
        }
        #endregion

        #region JsonConvert
        static List<(Action<JsonDictionary> objectJsonCorrecter, string newFileVersion)> IJsonConvertable<MaterialItem>.VersionCorrecters { get; } =
        [
            // 2.0.2 -> 2.1
            (oldJson =>
            {
                // inventory items in dictionary
                JsonDataCorrecterUtils.TransformValue<int>(oldJson, "type", (itemID)
                    => (ItemUtils._legacyItemTypeNameMap.TryGetValue(itemID, out var itemName), itemName));
            }, "2.1"),
            // 2.1.1 -> 2.2
            (oldJson =>
            {
                // item material
                JsonDataCorrecterUtils.TransformMultipleValues<string, string>(
                    oldJson,
                    "type",
                    (typeValue) => (ItemUtils._legacyMaterialItemMap.TryGetValue(typeValue ?? "", out var fixedType), fixedType),
                    (fixedType) => new Dictionary<string, JsonObject?>
                    {
                        ["type"] = "misc/material",
                        ["material"] = fixedType,
                    });
            }, "2.2"),
            // 2.3 -> 2.4
            (oldJson =>
            {
                // namespaced type/material
                JsonDataCorrecterUtils.TransformMultipleValues<string, string>(
                    oldJson,
                    "material",
                    (materialValue) => (!string.IsNullOrWhiteSpace(materialValue), materialValue),
                    (materialValue) => new Dictionary<string, JsonObject?>
                    {
                        ["type"] = "pa:misc/material",
                        ["material"] = ConfigUtils.GetSpecificNamespacedString(materialValue.ToLower()),
                    });
            }, "2.4"),
        ];

        static bool IJsonConvertable<MaterialItem>.FromJsonWithoutCorrection(JsonDictionary itemJson, string fileVersion, [NotNullWhen(true)] ref MaterialItem? itemObject)
        {
            if (
                !PACTools.TryParseJsonValue<EnumValue<Material>>(
                    itemJson,
                    Constants.JsonKeys.AItem.MATERIAL,
                    out var material,
                    isCritical: true
                )
            )
            {
                return false;
            }

            var success = true;
            if (!PACTools.TryParseJsonValue<double>(itemJson, Constants.JsonKeys.AItem.AMOUNT, out var itemAmount))
            {
                PACTools.LogJsonError("defaulting to 1");
                itemAmount = 1;
                success = false;
            }

            if (itemAmount <= 0)
            {
                PACTools.LogJsonError("invalid item amount in item json (amount <= 0)", true);
                return false;
            }

            try
            {
                itemObject = new MaterialItem(material, itemAmount);
            }
            catch (Exception ex)
            {
                PACTools.LogJsonError($"failed to create an object, from json: {ex}", true);
                return false;
            }

            return success;
        }
        #endregion
    }
}

﻿using PACommon.Enums;
using PACommon.JsonUtils;
using ProgressAdventure.ConfigManagement;
using ProgressAdventure.Enums;
using System.Diagnostics.CodeAnalysis;
using PACTools = PACommon.Tools;

namespace ProgressAdventure.ItemManagement
{
    /// <summary>
    /// Abstract class for an item.
    /// </summary>
    public abstract class AItem : IJsonConvertable<AItem>
    {
        #region Constants
        private static readonly string ITEM_AMOUNT_FORMATING = $"0.{new string('0', Constants.ITEM_AMOUNT_ROUNDING_DIGITS)}E0";
        #endregion

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

        /// <summary>
        /// The average density of the item in KG/M^3.
        /// </summary>
        protected double? _density;
        #endregion

        #region Public properties
        /// <summary>
        /// The type of the item.
        /// </summary>
        public EnumTreeValue<ItemType> Type { get; protected set; }

        /// <summary>
        /// The material, the item is (mainly) made out of.
        /// </summary>
        public EnumValue<Material> Material { get; protected set; }

        /// <summary>
        /// <inheritdoc cref="_amount"/>
        /// </summary>
        public double Amount
        {
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
                    return;
                }

                if (Unit == ItemAmountUnit.AMOUNT)
                {
                    _amount = Math.Floor(_amount);
                    return;
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
        /// The total mass of the item in KG.
        /// </summary>
        public double Mass
        {
            get
            {
                return MassMultiplier * Amount;
            }
        }

        /// <summary>
        /// The total volume of the item in M^3.
        /// </summary>
        public double Volume
        {
            get
            {
                return VolumeMultiplier * Amount;
            }
        }

        /// <summary>
        /// <inheritdoc cref="_density" path="//summary"/>
        /// </summary>
        public double Density
        {
            get
            {
                return _density ?? MassMultiplier / VolumeMultiplier;
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
            var amount = (Amount != 1 || Unit != ItemAmountUnit.AMOUNT) && Amount > 0
                ? $" x{FormatAmount(Amount)}{(Unit == ItemAmountUnit.AMOUNT
                    ? ""
                    : $" {Unit.ToString().ToLower()}")}"
                : "";
            return $"{DisplayName}{amount}";
        }
        #endregion

        #region Private functions
        public static string FormatAmount(double amount)
        {
            var absAmount = Math.Abs(amount);
            if (
                absAmount > Math.Pow(10, Constants.ITEM_AMOUNT_SCIENTIFIC_FORMAT_DIGITS) ||
                absAmount < Math.Pow(10, -Constants.ITEM_AMOUNT_SCIENTIFIC_FORMAT_DIGITS)
            )
            {
                return amount.ToString(ITEM_AMOUNT_FORMATING);
            }

            return Math.Round(amount, Constants.ITEM_AMOUNT_ROUNDING_DIGITS).ToString();
        }
        #endregion

        #region JsonConvert
        static List<(Action<JsonDictionary> objectJsonCorrecter, string newFileVersion)> IJsonConvertable<AItem>.VersionCorrecters { get; } =
        [
            // 2.0.2 -> 2.1
            (oldJson =>
            {
                // inventory items in dictionary
                JsonDataCorrecterUtils.TransformValue<AItem, int>(oldJson, "type", (itemID) =>
                {
                    return (ItemUtils._legacyItemTypeNameMap.TryGetValue(itemID, out var itemName), itemName);
                });
            }, "2.1"),
            // 2.1.1 -> 2.2
            (oldJson =>
            {
                // item material
                if (!PACTools.TryParseJsonValue<string>(oldJson, "type", out var typeValue, false))
                {
                    return;
                }

                if (ItemUtils._legacyCompoundtemMap.TryGetValue(typeValue, out var compoundItemFixedJson))
                {
                    JsonDataCorrecterUtils.SetMultipleValues(oldJson, new Dictionary<string, JsonObject?>
                    {
                        ["type"] = compoundItemFixedJson.typeName,
                        ["material"] = "WOOD",
                        ["parts"] = compoundItemFixedJson.partsJson,
                    });
                }
                else if (ItemUtils._legacyMaterialItemMap.TryGetValue(typeValue, out string? materialItemFixed))
                {
                    JsonDataCorrecterUtils.SetMultipleValues(oldJson, new Dictionary<string, JsonObject?>
                    {
                        ["type"] = "misc/material",
                        ["material"] = materialItemFixed,
                    });
                }
            }, "2.2"),
            // 2.3 -> 2.4
            (oldJson =>
            {
                // namespaced type/material
                if (
                    PACTools.TryParseJsonValue<string>(oldJson, "type", out var typeValue, false) &&
                    !string.IsNullOrWhiteSpace(typeValue) &&
                    PACTools.TryParseJsonValue<string>(oldJson, "material", out var materialValue, false) &&
                    !string.IsNullOrWhiteSpace(materialValue)
                )
                {
                    JsonDataCorrecterUtils.SetMultipleValues(oldJson, new Dictionary<string, JsonObject?>
                    {
                        ["type"] = ConfigUtils.GetSpecificNamespacedString(typeValue),
                        ["material"] = ConfigUtils.GetSpecificNamespacedString(materialValue.ToLower()),
                    });
                }
            }, "2.4"),
        ];

        public virtual JsonDictionary ToJson()
        {
            return new JsonDictionary
            {
                [Constants.JsonKeys.AItem.TYPE] = Type == ItemUtils.MATERIAL_ITEM_TYPE
                    ? ItemUtils.MATERIAL_TYPE_NAME
                    : Type.FullName,
                [Constants.JsonKeys.AItem.MATERIAL] = Material.ToString(),
                [Constants.JsonKeys.AItem.AMOUNT] = Amount,
            };
        }

        static bool IJsonConvertable<AItem>.FromJsonWithoutCorrection(JsonDictionary itemJson, string fileVersion, [NotNullWhen(true)] ref AItem? itemObject)
        {
            if (!(
                PACTools.TryParseJsonValue<string?>(itemJson, Constants.JsonKeys.Entity.TYPE, out var typeName, isCritical: true) &&
                ItemUtils.TryParseItemType(typeName, out var itemType)
            ))
            {
                PACTools.LogJsonError($"unknown item type: \"{typeName}\"", true);
                return false;
            }

            bool success;
            if (itemType == ItemUtils.MATERIAL_ITEM_TYPE)
            {
                success = PACTools.TryFromJsonWithoutCorrection(itemJson, fileVersion, out MaterialItem? materialObj);
                itemObject = materialObj;
                return success;
            }

            success = PACTools.TryFromJsonWithoutCorrection(itemJson, fileVersion, out CompoundItem? compoundObj);
            itemObject = compoundObj;
            return success;
        }
        #endregion
    }
}

﻿using ProgressAdventure.Enums;
using ProgressAdventure.Extensions;
using System.Collections;

namespace ProgressAdventure.ItemManagement
{
    /// <summary>
    /// Class storing an item that is made up of mutiple items.
    /// </summary>
    public class CompoundItem : AItem, IJsonConvertable<CompoundItem>
    {

        #region Public properties
        /// <summary>
        /// The parts making up this item.
        /// </summary>
        public List<AItem> Parts { get; private set; }
        #endregion

        #region Constructors
        /// <summary>
        /// <inheritdoc cref="CompoundItem"/>
        /// </summary>
        /// <param name="type"><inheritdoc cref="AItem.Type" path="//summary"/></param>
        /// <param name="parts"><inheritdoc cref="Parts" path="//summary"/></param>
        /// <param name="amount"><inheritdoc cref="Amount" path="//summary"/></param>
        /// <exception cref="ArgumentException">Thrown if the item type is not a compound item type id, or the parts list doesn't have an element.</exception>
        public CompoundItem(ItemTypeID type, List<AItem> parts, double amount = 1)
        {
            var typeValue = ItemUtils.ToItemType(type.GetHashCode());
            if (typeValue is null)
            {
                Logger.Log("Unknown item type", $"id: {type.GetHashCode()}", LogSeverity.ERROR);
                throw new ArgumentException("Unknown item type", nameof(type));
            }

            Type = (ItemTypeID)typeValue;

            if (Type == ItemUtils.MATERIAL_ITEM_TYPE)
            {
                Logger.Log("Item type cannot be \"material\" for a compound item", null, LogSeverity.ERROR);
                throw new ArgumentException("Item type is not a compound item type", nameof(type));
            }

            if (!parts.Any())
            {
                Logger.Log("Compound item has no parts", $"id: {type}", LogSeverity.ERROR);
                throw new ArgumentException("Parts list doesn't have an element.", nameof(parts));
            }

            
            Parts = parts;
            Material = Parts.First().Material;
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

            var totalMass = 0d;
            foreach (var part in Parts)
            {
                totalMass += part.Mass;
            }
            return totalMass * (Unit == ItemAmountUnit.L ? 0.001 : 1);
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

            var totalVolume = 0d;
            foreach (var part in Parts)
            {
                totalVolume += part.Volume;
            }
            return totalVolume;
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Sets the item's attributes, based on its type.
        /// </summary>
        private void SetAttributes()
        {
            var attributes = ItemUtils.compoundItemAttributes[Type];
            DisplayName = ItemUtils.ParseCompoundItemDisplayName(attributes.displayName, Parts);
        }
        #endregion

        #region Public overrides
        public override bool Equals(object? obj)
        {
            if (base.Equals(obj))
            {
                var item = (CompoundItem)obj;
                return Parts.Count == item.Parts.Count &&
                    Parts.First().Equals(item.Parts.First()) &&
                    Parts.UnorderedSequenceEqual(item.Parts, (part1, part2) => part1.Amount == part2.Amount);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Parts, Amount);
        }
        #endregion

        #region JsonConvert
        public override Dictionary<string, object?> ToJson()
        {
            var itemJson = base.ToJson();

            string typeName;
            if (ItemUtils.compoundItemAttributes.TryGetValue(Type, out CompoundItemAttributesDTO? attributes))
            {
                typeName = attributes.typeName;
            }
            else
            {
                typeName = ItemUtils.ItemIDToTypeName(Type);
                Logger.Log("Item to json", $"item type doesn't have a type name, type:{Type}", LogSeverity.ERROR);
            }

            itemJson["type"] = typeName;
            itemJson["parts"] = Parts.Select(part => part.ToJson());

            return itemJson;
        }

        public static bool FromJson(IDictionary<string, object?>? itemJson, string fileVersion, out CompoundItem? itemObject)
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
                        ItemUtils._legacyCompoundtemMap.TryGetValue(typeValue?.ToString() ?? "", out (string typeName, List<Dictionary<string, object?>> partsJson) compoundItemFixedJson)
                    )
                    {
                        itemJson["type"] = compoundItemFixedJson.typeName;
                        itemJson["material"] = "WOOD";
                        itemJson["parts"] = compoundItemFixedJson.partsJson;
                    }

                    Logger.Log("Corrected item json data", $"{fileVersion} -> {newFileVersion}", LogSeverity.DEBUG);
                    fileVersion = newFileVersion;
                }
                Logger.Log($"Item json data corrected");
            }

            //convert
            if (!itemJson.TryGetValue("type", out var typeNameValue))
            {
                Logger.Log("Item parse error", "couldn't find item type in item json", LogSeverity.ERROR);
                return false;
            }

            if (ItemUtils.TryParseItemType(typeNameValue?.ToString(), out ItemTypeID itemType))
            {
                var parts = new List<AItem>();
                if (itemJson.TryGetValue("parts", out var partsListJson))
                {
                    if (partsListJson is IEnumerable partsList)
                    {
                        foreach (var partJson in partsList)
                        {
                            FromJson(partJson as Dictionary<string, object?>, fileVersion, out AItem? part);
                            if (part is not null)
                            {
                                parts.Add(part);
                            }
                        }
                    }
                    else
                    {
                        Logger.Log("Item parse error", "parts list is not a list", LogSeverity.ERROR);
                        return false;
                    }
                }
                else
                {
                    Logger.Log("Item parse error", "couldn't find parts in json", LogSeverity.ERROR);
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
                        Logger.Log("Item parse error", "invalid item amount in item json (amount < 1)", LogSeverity.ERROR);
                        return false;
                    }
                }
                else
                {
                    Logger.Log("Item parse error", "couldn't parse item amount from json, defaulting to 1", LogSeverity.WARN);
                }

                try
                {
                    itemObject = new CompoundItem(itemType, parts, itemAmount);
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
                Logger.Log("Item parse error", $"item type value is an unknown item type: \"{typeNameValue}\"", LogSeverity.ERROR);
                return false;
            }
        }
        #endregion
    }
}
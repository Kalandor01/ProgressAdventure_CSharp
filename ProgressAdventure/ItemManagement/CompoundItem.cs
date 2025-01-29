using PACommon;
using PACommon.Enums;
using PACommon.Extensions;
using PACommon.JsonUtils;
using ProgressAdventure.Enums;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using PACTools = PACommon.Tools;

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
        public ImmutableList<AItem> Parts { get; private set; }
        #endregion

        #region Constructors
        /// <summary>
        /// <inheritdoc cref="CompoundItem"/>
        /// </summary>
        /// <param name="type"><inheritdoc cref="AItem.Type" path="//summary"/></param>
        /// <param name="parts"><inheritdoc cref="Parts" path="//summary"/></param>
        /// <param name="amount"><inheritdoc cref="Amount" path="//summary"/></param>
        /// <exception cref="ArgumentException">Thrown if the item type is not a compound item type id, or the parts list doesn't have an element.</exception>
        public CompoundItem(EnumTreeValue<ItemType> type, List<AItem> parts, double amount = 1)
        {
            var typeValue = ItemUtils.ParseItemType(type.FullName);
            if (typeValue is null)
            {
                PACSingletons.Instance.Logger.Log("Unknown item type", type.FullName, LogSeverity.ERROR);
                throw new ArgumentException("Unknown item type", nameof(type));
            }

            if (typeValue == ItemUtils.MATERIAL_ITEM_TYPE)
            {
                PACSingletons.Instance.Logger.Log("Item type cannot be \"material\" for a compound item", null, LogSeverity.ERROR);
                throw new ArgumentException("Item type is not a compound item type", nameof(type));
            }

            if (parts.Count == 0)
            {
                PACSingletons.Instance.Logger.Log("Compound item has no parts", $"id: {type}", LogSeverity.ERROR);
                throw new ArgumentException("Parts list doesn't have an element.", nameof(parts));
            }

            Type = typeValue;
            Parts = [.. parts];
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
                // TODO: DO THIS WITH TAGS!

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

            var massMultiplier = Unit == ItemAmountUnit.L ? 0.001 : 1;
            return totalMass * massMultiplier;
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
            var attributes = ItemUtils.CompoundItemAttributes[Type];
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
        static List<(Action<JsonDictionary> objectJsonCorrecter, string newFileVersion)> IJsonConvertable<CompoundItem>.VersionCorrecters { get; } =
        [
            // 2.0.2 -> 2.1
            (oldJson =>
            {
                // inventory items in dictionary
                JsonDataCorrecterUtils.TransformValue<CompoundItem, int>(oldJson, "type", (itemID) =>
                {
                    return (ItemUtils._legacyItemTypeNameMap.TryGetValue(itemID, out var itemName), itemName);
                });
            }, "2.1"),
            // 2.1.1 -> 2.2
            (oldJson =>
            {
                // item material
                JsonDataCorrecterUtils.TransformMultipleValues<CompoundItem, string, (string typeName, JsonArray partsJson)>(
                    oldJson,
                    "type",
                    (typeValue) => (ItemUtils._legacyCompoundtemMap.TryGetValue(typeValue ?? "", out var fixedJson), fixedJson),
                    (fixedJson) =>
                    {
                        return new Dictionary<string, JsonObject?>
                        {
                            ["type"] = fixedJson.typeName,
                            ["material"] = "WOOD",
                            ["parts"] = fixedJson.partsJson,
                        };
                    }
                );
            }, "2.2"),
        ];

        public override JsonDictionary ToJson()
        {
            var itemJson = base.ToJson();

            itemJson[Constants.JsonKeys.AItem.TYPE] = Type.FullName;
            itemJson[Constants.JsonKeys.CompoundItem.PARTS] = Parts.Select(part => (JsonObject?)part.ToJson()).ToList();

            return itemJson;
        }

        static bool IJsonConvertable<CompoundItem>.FromJsonWithoutCorrection(JsonDictionary itemJson, string fileVersion, [NotNullWhen(true)] ref CompoundItem? itemObject)
        {
            if (!(
                PACTools.TryCastJsonAnyValue<CompoundItem, string>(itemJson, Constants.JsonKeys.AItem.TYPE, out var typeName, true) &&
                ItemUtils.TryParseItemType(typeName, out var itemType)
            ))
            {
                PACTools.LogJsonError<CompoundItem>($"unknown item type: \"{typeName}\"", true);
                return false;
            }

            var success = true;
            // "ItemUtils._legacyCompoundtemMap" has the parts in the 2.2 json format, so I bump the version, so it doesn't try to correct the json twice.
            var partsFileVersion = Utils.IsUpToDate("2.2", fileVersion) ? fileVersion : "2.2";
            success &= PACTools.TryParseJsonListValue<CompoundItem, AItem>(itemJson, Constants.JsonKeys.CompoundItem.PARTS,
                partJson => {
                    success &= PACTools.TryFromJson(partJson as JsonDictionary, partsFileVersion, out AItem? part);
                    return (part is not null, part);
                },
                out var parts, true);
            if (parts is null)
            {
                return false;
            }

            if (!PACTools.TryParseJsonValue<CompoundItem, double>(itemJson, Constants.JsonKeys.AItem.AMOUNT, out var itemAmount))
            {
                PACTools.LogJsonError<CompoundItem>("item amount is null, defaulting to 1");
                itemAmount = 1;
                success = false;
            }

            if (itemAmount <= 0)
            {
                PACTools.LogJsonError<CompoundItem>("invalid item amount in item json (amount <= 0)", true);
                return false;
            }

            try
            {
                itemObject = new CompoundItem(itemType, parts, itemAmount);
            }
            catch (Exception ex)
            {
                PACTools.LogJsonError<CompoundItem>($"failed to create an object, from json: {ex}", true);
                return false;
            }

            return success;
        }
        #endregion
    }
}

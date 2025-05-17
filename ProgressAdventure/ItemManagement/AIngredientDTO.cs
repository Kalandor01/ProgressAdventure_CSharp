using PACommon.Enums;
using ProgressAdventure.Enums;
using System.Text.Json.Serialization;

namespace ProgressAdventure.ItemManagement
{
    /// <summary>
    /// DTO used for storing the attributes of an ingredient.
    /// </summary>
    public abstract class AIngredientDTO
    {
        #region Fields
        /// <summary>
        /// The type of the item.
        /// </summary>
        [JsonPropertyName("item_type")]
        public readonly EnumTreeValue<ItemType> itemType;
        /// <summary>
        /// The material of the item, that is required for the recipe.
        /// </summary>
        [JsonPropertyName("material")]
        public readonly EnumValue<Material>? material;
        /// <summary>
        /// The amount of the item, that the recipe requires.
        /// </summary>
        [JsonPropertyName("amount")]
        public readonly double amount;
        /// <summary>
        /// The unit to interpret the amount in.
        /// </summary>
        [JsonPropertyName("unit")]
        public readonly ItemAmountUnit? unit;
        #endregion

        #region Public constructors
        /// <summary>
        /// <inheritdoc cref="AIngredientDTO"/>
        /// </summary>
        /// <param name="itemType"><inheritdoc cref="itemType" path="//summary"/></param>
        /// <param name="material"><inheritdoc cref="material" path="//summary"/></param>
        /// <param name="amount"><inheritdoc cref="amount" path="//summary"/></param>
        /// <param name="unit"><inheritdoc cref="unit" path="//summary"/><br/>
        /// Amount does nothing (the same as null).</param>
        /// <exception cref="ArgumentException">Thrown, if the item type is material, and the unit is amount.</exception>
        public AIngredientDTO(EnumTreeValue<ItemType> itemType, EnumValue<Material>? material = null, double amount = 1, ItemAmountUnit? unit = null)
        {
            this.itemType = itemType;
            this.material = material;
            this.amount = Math.Max(amount, 0);

            var itemUnit = ItemUtils.CompoundItemAttributes[this.itemType].unit;

            if (itemUnit == ItemAmountUnit.AMOUNT)
            {
                if (unit is not null && unit != ItemAmountUnit.AMOUNT)
                {
                    throw new ArgumentException("Unit type must be amount if the item type's unit type is amount. Set unit to null.", nameof(unit));
                }

                if (this.amount % 1 != 0)
                {
                    throw new ArgumentException("The ingredient item type's unit type is amount, but requires a non-whole amount.", nameof(amount));
                }
            }

            this.unit = unit;
        }
        #endregion

        public override string? ToString()
        {
            return $"{(
                    material is null && itemType == ItemType.Misc.MATERIAL
                        ? "ANY MATERIAL"
                        : $"{(material is null ? "" : $"{material} ")}{(itemType == ItemType.Misc.MATERIAL ? "" : itemType.FullName)}"
                )} x{amount}{(unit is null || unit == ItemAmountUnit.AMOUNT ? "" : (ItemAmountUnit)unit)}";
        }
    }
}

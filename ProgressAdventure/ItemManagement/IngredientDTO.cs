using PACommon;
using PACommon.Enums;
using ProgressAdventure.Enums;

namespace ProgressAdventure.ItemManagement
{
    /// <summary>
    /// DTO used for storing the attributes of an ingredient.
    /// </summary>
    public class IngredientDTO
    {
        #region Fields
        /// <summary>
        /// The type of the item.
        /// </summary>
        public readonly ItemTypeID itemType;
        /// <summary>
        /// The material of the item, the is required for the recipe.
        /// </summary>
        public readonly Material? material;
        /// <summary>
        /// The amount of the item, that the recipe requires.
        /// </summary>
        public readonly double amount;
        /// <summary>
        /// The unit to interpret the amount in.
        /// </summary>
        public readonly ItemAmountUnit? unit;
        #endregion

        #region Public constructors
        /// <summary>
        /// <inheritdoc cref="IngredientDTO"/><br/>
        /// Used for a compound item ingrediend.
        /// </summary>
        /// <param name="itemType"><inheritdoc cref="itemType" path="//summary"/></param>
        /// <param name="amount"><inheritdoc cref="amount" path="//summary"/></param>
        /// <param name="unit"><inheritdoc cref="unit" path="//summary"/></param>
        public IngredientDTO(ItemTypeID itemType, double amount = 1, ItemAmountUnit? unit = null)
            : this(
                  itemType,
                  null,
                  amount
                )
        { }

        /// <summary>
        /// <inheritdoc cref="IngredientDTO"/><br/>
        /// Used for a material ingredient.
        /// </summary>
        /// <param name="material"><inheritdoc cref="material" path="//summary"/></param>
        /// <param name="amount"><inheritdoc cref="amount" path="//summary"/></param>
        /// <param name="unit"><inheritdoc cref="unit" path="//summary"/></param>
        /// <exception cref="ArgumentException">Thrown, if the unit is amount.</exception>
        public IngredientDTO(Material? material = null, double amount = 1, ItemAmountUnit? unit = null)
            : this(ItemUtils.MATERIAL_ITEM_TYPE, material, amount, unit)
        { }

        /// <summary>
        /// <inheritdoc cref="IngredientDTO"/>
        /// </summary>
        /// <param name="itemType"><inheritdoc cref="itemType" path="//summary"/></param>
        /// <param name="material"><inheritdoc cref="material" path="//summary"/></param>
        /// <param name="amount"><inheritdoc cref="amount" path="//summary"/></param>
        /// <param name="unit"><inheritdoc cref="unit" path="//summary"/><br/>
        /// Amount does nothing (the same as null).</param>
        /// <exception cref="ArgumentException">Thrown, if the item type is material, and the unit is amount.</exception>
        public IngredientDTO(ItemTypeID itemType, Material? material = null, double amount = 1, ItemAmountUnit? unit = null)
        {
            this.itemType = itemType;
            this.material = material;
            this.amount = Math.Max(amount, 0);

            var itemUnit = ItemUtils.compoundItemAttributes[this.itemType].unit;

            if (itemUnit == ItemAmountUnit.AMOUNT)
            {
                if (unit is not null && unit != ItemAmountUnit.AMOUNT)
                {
                    PACSingletons.Instance.Logger.Log("Ingredient error", "required unit type cannot be converted from the item type's amount unit type, set unit to null", LogSeverity.ERROR);
                    throw new ArgumentException("Required unit type cannot be converted from the item type's amount unit type. Set unit to null.", nameof(unit));
                }

                if (this.amount % 1 != 0)
                {
                    this.amount = Math.Floor(this.amount);
                    PACSingletons.Instance.Logger.Log("Ingredient amount missmatch", "ingredient unit type is amount, but requires a non-whole amount, corrected", LogSeverity.WARN);
                }
            }

            this.unit = unit;
        }
        #endregion
    }
}

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
        #endregion

        #region Public constructors
        /// <summary>
        /// <inheritdoc cref="IngredientDTO"/>
        /// </summary>
        /// <param name="itemType"><inheritdoc cref="itemType" path="//summary"/></param>
        /// <param name="amount"><inheritdoc cref="amount" path="//summary"/></param>
        public IngredientDTO(ItemTypeID itemType, double amount = 1)
            : this(
                  itemType,
                  null,
                  amount
                )
        { }

        /// <summary>
        /// <inheritdoc cref="IngredientDTO"/>
        /// </summary>
        /// <param name="itemType"><inheritdoc cref="itemType" path="//summary"/></param>
        /// <param name="material"><inheritdoc cref="material" path="//summary"/></param>
        /// <param name="amount"><inheritdoc cref="amount" path="//summary"/></param>
        public IngredientDTO(ItemTypeID itemType, Material? material = null, double amount = 1)
        {
            this.itemType = itemType;
            this.material = material;
            this.amount = Math.Max(amount, 0);
            
            if (ItemUtils.itemAttributes[this.itemType].unit == ItemAmountUnit.AMOUNT)
            {
                this.amount = Math.Floor(this.amount);
            }
        }
        #endregion
    }
}

using PACommon.Enums;
using ProgressAdventure.Enums;
using System.Text.Json.Serialization;

namespace ProgressAdventure.ItemManagement
{
    /// <summary>
    /// DTO used for storing the attributes of an ingredient.
    /// </summary>
    public class IngredientDTO : AIngredientDTO
    {
        #region Public constructors
        /// <summary>
        /// <inheritdoc cref="IngredientDTO"/>
        /// </summary>
        /// <param name="itemType"><inheritdoc cref="itemType" path="//summary"/></param>
        /// <param name="amount"><inheritdoc cref="amount" path="//summary"/></param>
        /// <param name="unit"><inheritdoc cref="unit" path="//summary"/></param>
        public IngredientDTO(EnumTreeValue<ItemType> itemType, double amount = 1, ItemAmountUnit? unit = null)
            : this(
                  itemType,
                  null,
                  amount,
                  unit
                )
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
        [JsonConstructor]
        public IngredientDTO(EnumTreeValue<ItemType> itemType, EnumValue<Material>? material = null, double amount = 1, ItemAmountUnit? unit = null)
            : base(itemType, material, amount, unit)
        {
            if (itemType == ItemUtils.MATERIAL_ITEM_TYPE)
            {
                throw new ArgumentException("A solid recipe ingredient cannot be a material", nameof(itemType));
            }
        }
        #endregion
    }
}

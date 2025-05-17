using PACommon.Enums;
using ProgressAdventure.Enums;
using System.Text.Json.Serialization;

namespace ProgressAdventure.ItemManagement
{
    /// <summary>
    /// DTO used for storing the attributes of a non-solid material ingredient.
    /// </summary>
    public class NonSolidIngredientDTO : AIngredientDTO
    {
        #region Public constructors
        /// <summary>
        /// <inheritdoc cref="NonSolidIngredientDTO"/>
        /// </summary>
        /// <param name="amount"><inheritdoc cref="amount" path="//summary"/></param>
        /// <param name="unit"><inheritdoc cref="unit" path="//summary"/><br/>
        /// Amount does nothing (the same as null).</param>
        [JsonConstructor]
        public NonSolidIngredientDTO(double amount = 1, ItemAmountUnit? unit = null)
            : this(null, amount, unit)
        {

        }

        /// <summary>
        /// <inheritdoc cref="NonSolidIngredientDTO"/>
        /// </summary>
        /// <param name="material"><inheritdoc cref="material" path="//summary"/></param>
        /// <param name="amount"><inheritdoc cref="amount" path="//summary"/></param>
        /// <param name="unit"><inheritdoc cref="unit" path="//summary"/><br/>
        /// Amount does nothing (the same as null).</param>
        [JsonConstructor]
        public NonSolidIngredientDTO(EnumValue<Material>? material = null, double amount = 1, ItemAmountUnit? unit = null)
            : base(ItemUtils.MATERIAL_ITEM_TYPE, material, amount, unit)
        {

        }
        #endregion
    }
}

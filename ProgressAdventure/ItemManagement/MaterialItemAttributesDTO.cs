using PACommon.Extensions;
using ProgressAdventure.Enums;

namespace ProgressAdventure.ItemManagement
{
    /// <summary>
    /// DTO used for storing the attributes of a material.
    /// </summary>
    public class MaterialItemAttributesDTO : AItemAttributesDTO
    {
        #region Public constructors
        /// <summary>
        /// <inheritdoc cref="MaterialItemAttributesDTO"/>
        /// </summary>
        /// <param name="material">The material.</param>
        /// <param name="unit"><inheritdoc cref="unit" path="//summary"/></param>
        public MaterialItemAttributesDTO(Material material, ItemAmountUnit unit = ItemAmountUnit.KG)
            : this(
                  material.ToString().Replace("_", " ").Capitalize(),
                  unit
                )
        { }

        /// <summary>
        /// <inheritdoc cref="MaterialItemAttributesDTO"/>
        /// </summary>
        /// <param name="displayName"><inheritdoc cref="displayName" path="//summary"/></param>
        /// <param name="unit"><inheritdoc cref="unit" path="//summary"/></param>
        public MaterialItemAttributesDTO(string displayName, ItemAmountUnit unit = ItemAmountUnit.KG)
            : base(
                  ItemUtils.MATERIAL_TYPE_NAME,
                  displayName,
                  unit != ItemAmountUnit.AMOUNT ? unit : throw new ArgumentException("Material atributes cannot have amount as unit", nameof(unit))
                )
        { }
        #endregion
    }
}

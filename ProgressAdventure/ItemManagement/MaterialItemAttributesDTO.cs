using PACommon.Extensions;
using ProgressAdventure.Enums;
using System.Text.Json.Serialization;

namespace ProgressAdventure.ItemManagement
{
    /// <summary>
    /// DTO used for storing the attributes of a material.
    /// </summary>
    public class MaterialItemAttributesDTO : AItemAttributesDTO
    {
        #region Fields
        /// <summary>
        /// The material properties of the material.
        /// </summary>
        [JsonPropertyName("properties")]
        public readonly MaterialPropertiesDTO properties;
        #endregion

        #region Public constructors
        /// <summary>
        /// <inheritdoc cref="MaterialItemAttributesDTO"/>
        /// </summary>
        /// <param name="material">The material.</param>
        /// <param name="unit"><inheritdoc cref="AItemAttributesDTO.unit" path="//summary"/></param>
        public MaterialItemAttributesDTO(Material material, MaterialPropertiesDTO properties, ItemAmountUnit unit = ItemAmountUnit.KG)
            : this(
                  material.ToString().Replace("_", " ").Capitalize(),
                  properties,
                  unit
                )
        { }

        /// <summary>
        /// <inheritdoc cref="MaterialItemAttributesDTO"/>
        /// </summary>
        /// <param name="displayName"><inheritdoc cref="AItemAttributesDTO.displayName" path="//summary"/></param>
        /// <param name="properties"><inheritdoc cref="properties" path="//summary"/></param>
        /// <param name="unit"><inheritdoc cref="AItemAttributesDTO.unit" path="//summary"/></param>
        [JsonConstructor]
        public MaterialItemAttributesDTO(string displayName, MaterialPropertiesDTO properties, ItemAmountUnit unit = ItemAmountUnit.KG)
            : base(
                  ItemUtils.MATERIAL_TYPE_NAME,
                  displayName,
                  unit != ItemAmountUnit.AMOUNT ? unit : throw new ArgumentException($"Material atributes cannot have {ItemAmountUnit.AMOUNT} as unit", nameof(unit))
                )
        {
            this.properties = properties;
        }
        #endregion

        #region Overrides
        public override string? ToString()
        {
            return $"{displayName} material, {unit}";
        }
        #endregion
    }
}

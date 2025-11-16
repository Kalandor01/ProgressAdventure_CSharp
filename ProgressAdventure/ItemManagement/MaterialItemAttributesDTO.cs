using System.Text.Json.Serialization;
using PACommon.Enums;
using PACommon.Extensions;
using ProgressAdventure.ConfigManagement;
using ProgressAdventure.Enums;

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
        public MaterialItemAttributesDTO(EnumValue<Material> material, MaterialPropertiesDTO properties, ItemAmountUnit unit = ItemAmountUnit.KG)
            : this(
                  AddLocalizationFromMaterial(material),
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
        public MaterialItemAttributesDTO(EnumValue<LocalizationKey> displayName, MaterialPropertiesDTO properties, ItemAmountUnit unit = ItemAmountUnit.KG)
            : base(
                  displayName,
                  unit != ItemAmountUnit.AMOUNT ? unit : throw new ArgumentException($"Material atributes cannot have {ItemAmountUnit.AMOUNT} as unit", nameof(unit))
                )
        {
            this.properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }
        #endregion

        #region Overrides
        public override string? ToString()
        {
            return $"{PASingletons.Instance.Localizer.GetLocalizedString(displayName)} material, {unit}";
        }
        #endregion

        #region Private functions
        private static EnumValue<LocalizationKey> AddLocalizationFromMaterial(EnumValue<Material> material)
        {
            var namespaceName = ConfigUtils.GetNamespace(material.Name);
            return Tools.TryAddEnglishLocalizationFromEnumLikeValue(
                namespaceName,
                "material_display_name",
                ConfigUtils.RemoveNamespace(material.Name),
                ConfigUtils.RemoveNamespace(material.Name).Replace("_", " ").Capitalize(),
                namespaceName == Constants.VANILLA_CONFIGS_NAMESPACE
            );
        }
        #endregion
    }
}

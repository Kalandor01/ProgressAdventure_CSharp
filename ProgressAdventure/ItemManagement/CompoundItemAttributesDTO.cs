using System.Text.Json.Serialization;
using PACommon.Enums;
using ProgressAdventure.ConfigManagement;
using ProgressAdventure.Enums;

namespace ProgressAdventure.ItemManagement
{
    /// <summary>
    /// DTO used for storing the attributes of a compound item.
    /// </summary>
    public class CompoundItemAttributesDTO : AItemAttributesDTO
    {
        #region Fields
        /// <summary>
        /// The properties of the <see cref="CompoundItem"/>.
        /// </summary>
        [JsonPropertyName("properties")]
        public readonly CompoundItemPropertiesDTO properties;
        #endregion

        #region Public constructors
        /// <summary>
        /// <inheritdoc cref="CompoundItemAttributesDTO"/>
        /// </summary>
        /// <param name="itemType">The type of the item.</param>
        /// <param name="properties"><inheritdoc cref="properties" path="//summary"/></param>
        /// <param name="unit"><inheritdoc cref="unit" path="//summary"/></param>
        public CompoundItemAttributesDTO(
            EnumTreeValue<ItemType> itemType,
            CompoundItemPropertiesDTO properties,
            ItemAmountUnit unit = ItemAmountUnit.AMOUNT
        )
            : this(
                  AddLocalizationFromCompoundItemType(itemType),
                  properties,
                  unit
                )
        { }

        /// <summary>
        /// <inheritdoc cref="CompoundItemAttributesDTO"/>
        /// </summary>
        /// <param name="displayName"><inheritdoc cref="displayName" path="//summary"/></param>
        /// <param name="properties"><inheritdoc cref="properties" path="//summary"/></param>
        /// <param name="unit"><inheritdoc cref="unit" path="//summary"/></param>
        [JsonConstructor]
        public CompoundItemAttributesDTO(
            EnumValue<LocalizationKey> displayName,
            CompoundItemPropertiesDTO properties,
            ItemAmountUnit unit = ItemAmountUnit.AMOUNT
        )
            : base(
                  displayName,
                  unit
                )
        {
            this.properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }
        #endregion

        #region Private functions
        private static EnumValue<LocalizationKey> AddLocalizationFromCompoundItemType(EnumTreeValue<ItemType> itemType)
        {
            var namespaceName = ConfigUtils.GetNamespace(itemType.Name);
            return Tools.TryAddEnglishLocalizationFromEnumLikeValue(
                namespaceName,
                "compound_item_display_name",
                ConfigUtils.RemoveNamespace(itemType.Name),
                $"*/0MC/* {ItemUtils.ItemTypeToDisplayName(itemType)}",
                namespaceName == Constants.VANILLA_CONFIGS_NAMESPACE
            );
        }
        #endregion
    }
}

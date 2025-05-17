using PACommon.Enums;
using ProgressAdventure.Enums;
using System.Text.Json.Serialization;

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
                  $"*/0MC/* {ItemUtils.ItemTypeToDisplayName(itemType)}",
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
            string displayName,
            CompoundItemPropertiesDTO properties,
            ItemAmountUnit unit = ItemAmountUnit.AMOUNT
        )
            : base(
                  displayName,
                  unit
                )
        {
            this.properties = properties;
        }
        #endregion
    }
}

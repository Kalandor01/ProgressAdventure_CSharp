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
        #region Public constructors
        /// <summary>
        /// <inheritdoc cref="CompoundItemAttributesDTO"/>
        /// </summary>
        /// <param name="itemType">The type of the item.</param>
        /// <param name="unit"><inheritdoc cref="unit" path="//summary"/></param>
        public CompoundItemAttributesDTO(EnumTreeValue<ItemType> itemType, ItemAmountUnit unit = ItemAmountUnit.AMOUNT)
            : this(
                  $"*/0MC/* {ItemUtils.ItemIDToDisplayName(itemType)}",
                  unit
                )
        { }

        /// <summary>
        /// <inheritdoc cref="CompoundItemAttributesDTO"/>
        /// </summary>
        /// <param name="displayName"><inheritdoc cref="displayName" path="//summary"/></param>
        /// <param name="unit"><inheritdoc cref="unit" path="//summary"/></param>
        [JsonConstructor]
        public CompoundItemAttributesDTO(string displayName, ItemAmountUnit unit = ItemAmountUnit.AMOUNT)
            : base(
                  displayName,
                  unit
                )
        { }
        #endregion
    }
}

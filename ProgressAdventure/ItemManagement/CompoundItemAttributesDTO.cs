using ProgressAdventure.Enums;

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
        public CompoundItemAttributesDTO(ItemTypeID itemType, ItemAmountUnit unit = ItemAmountUnit.AMOUNT)
            : this(
                  itemType,
                  $"*/0MC/* {ItemUtils.ItemIDToDisplayName(itemType)}",
                  unit
                )
        { }

        /// <summary>
        /// <inheritdoc cref="CompoundItemAttributesDTO"/>
        /// </summary>
        /// <param name="itemTypeID">The type of the item.</param>
        /// <param name="displayName"><inheritdoc cref="displayName" path="//summary"/></param>
        /// <param name="unit"><inheritdoc cref="unit" path="//summary"/></param>
        public CompoundItemAttributesDTO(ItemTypeID itemTypeID, string displayName, ItemAmountUnit unit = ItemAmountUnit.AMOUNT)
            : base(
                  ItemUtils.ItemIDToTypeName(itemTypeID),
                  displayName,
                  unit
                )
        { }
        #endregion
    }
}

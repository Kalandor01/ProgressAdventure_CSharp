using ProgressAdventure.Enums;

namespace ProgressAdventure.ItemManagement
{
    /// <summary>
    /// DTO used for storing the attributes of an item.
    /// </summary>
    public class ItemAttributesDTO
    {
        #region Fields
        /// <summary>
        /// The unique name of the item, used in the json representation of the item.<br/>
        /// Usualy "item_category/item_type".
        /// </summary>
        public readonly string typeName;
        /// <summary>
        /// The display name of the item.
        /// </summary>
        public readonly string displayName;
        /// <summary>
        /// If the item type is a compound item, or not.
        /// </summary>
        public readonly bool isCompoundItem;
        /// <summary>
        /// <inheritdoc cref="ItemAmountUnit"/>
        /// </summary>
        public readonly ItemAmountUnit unit;
        /// <summary>
        /// If the item is consumed after using it.
        /// </summary>
        public readonly bool consumable;
        #endregion

        #region Public constructors
        /// <summary>
        /// <inheritdoc cref="ItemAttributesDTO"/>
        /// </summary>
        /// <param name="itemType">The type of the item.</param>
        /// <param name="isCompoundItem"><inheritdoc cref="isCompoundItem" path="//summary"/></param>
        /// <param name="unit"><inheritdoc cref="unit" path="//summary"/></param>
        /// <param name="consumable"><inheritdoc cref="consumable" path="//summary"/></param>
        public ItemAttributesDTO(ItemTypeID itemType, bool isCompoundItem = false, ItemAmountUnit unit = ItemAmountUnit.AMOUNT, bool consumable = false)
            : this(
                  itemType,
                  ItemUtils.ItemIDToDisplayName(itemType),
                  isCompoundItem,
                  unit,
                  consumable
                )
        { }

        /// <summary>
        /// <inheritdoc cref="ItemAttributesDTO"/>
        /// </summary>
        /// <param name="itemTypeID">The type of the item.</param>
        /// <param name="displayName"><inheritdoc cref="displayName" path="//summary"/></param>
        /// <param name="isCompoundItem"><inheritdoc cref="isCompoundItem" path="//summary"/></param>
        /// <param name="unit"><inheritdoc cref="unit" path="//summary"/></param>
        /// <param name="consumable"><inheritdoc cref="consumable" path="//summary"/></param>
        public ItemAttributesDTO(ItemTypeID itemTypeID, string displayName, bool isCompoundItem = false, ItemAmountUnit unit = ItemAmountUnit.AMOUNT, bool consumable = false)
            : this(
                  ItemUtils.ItemIDToTypeName(itemTypeID),
                  displayName,
                  isCompoundItem,
                  unit,
                  consumable
                )
        { }

        /// <summary>
        /// <inheritdoc cref="ItemAttributesDTO"/>
        /// </summary>
        /// <param name="typeName"><inheritdoc cref="typeName" path="//summary"/></param>
        /// <param name="displayName"><inheritdoc cref="displayName" path="//summary"/></param>
        /// <param name="isCompoundItem"><inheritdoc cref="isCompoundItem" path="//summary"/></param>
        /// <param name="unit"><inheritdoc cref="unit" path="//summary"/></param>
        /// <param name="consumable"><inheritdoc cref="consumable" path="//summary"/></param>
        public ItemAttributesDTO(string typeName, string displayName, bool isCompoundItem = false, ItemAmountUnit unit = ItemAmountUnit.AMOUNT, bool consumable = false)
        {
            this.typeName = typeName;
            this.displayName = displayName;
            this.isCompoundItem = isCompoundItem;
            this.unit = unit;
            this.consumable = consumable;
        }
        #endregion
    }
}

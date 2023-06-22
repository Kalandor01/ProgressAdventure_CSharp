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
        /// If the item is consumed after using it.
        /// </summary>
        public readonly bool consumable;
        #endregion

        #region Constructor
        /// <summary>
        /// <inheritdoc cref="ItemAttributesDTO"/>
        /// </summary>
        /// <param name="itemType">The type of the item.</param>
        /// <param name="consumable"><inheritdoc cref="consumable" path="//summary"/></param>
        public ItemAttributesDTO(ItemTypeID itemType, bool consumable = false)
            : this(
                  itemType,
                  ItemUtils.ItemIDToDisplayName(itemType),
                  consumable
                ) { }

        /// <summary>
        /// <inheritdoc cref="ItemAttributesDTO"/>
        /// </summary>
        /// <param name="itemType">The type of the item.</param>
        /// <param name="displayName"><inheritdoc cref="displayName" path="//summary"/></param>
        /// <param name="consumable"><inheritdoc cref="consumable" path="//summary"/></param>
        public ItemAttributesDTO(ItemTypeID itemType, string displayName, bool consumable = false)
            : this(
                  ItemUtils.ItemIDToTypeName(itemType),
                  displayName,
                  consumable
                )
        { }

        /// <summary>
        /// <inheritdoc cref="ItemAttributesDTO"/>
        /// </summary>
        /// <param name="typeName"><inheritdoc cref="typeName" path="//summary"/></param>
        /// <param name="displayName"><inheritdoc cref="displayName" path="//summary"/></param>
        /// <param name="consumable"><inheritdoc cref="consumable" path="//summary"/></param>
        public ItemAttributesDTO(string typeName, string displayName, bool consumable = false)
        {
            this.typeName = typeName;
            this.displayName = displayName;
            this.consumable = consumable;
        }
        #endregion
    }
}

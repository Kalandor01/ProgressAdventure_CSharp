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
        /// If the item can have a material, or not.
        /// </summary>
        public readonly bool canHaveMaterial;
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
        /// <param name="canHaveMaterial"><inheritdoc cref="canHaveMaterial" path="//summary"/></param>
        /// <param name="consumable"><inheritdoc cref="consumable" path="//summary"/></param>
        public ItemAttributesDTO(ItemTypeID itemType, bool canHaveMaterial = true, bool consumable = false)
            : this(
                  itemType,
                  ItemUtils.ItemIDToDisplayName(itemType),
                  canHaveMaterial,
                  consumable
                )
        { }

        /// <summary>
        /// <inheritdoc cref="ItemAttributesDTO"/>
        /// </summary>
        /// <param name="itemType">The type of the item.</param>
        /// <param name="displayName"><inheritdoc cref="displayName" path="//summary"/></param>
        /// <param name="canHaveMaterial"><inheritdoc cref="canHaveMaterial" path="//summary"/></param>
        /// <param name="consumable"><inheritdoc cref="consumable" path="//summary"/></param>
        public ItemAttributesDTO(ItemTypeID itemType, string displayName, bool canHaveMaterial = true, bool consumable = false)
            : this(
                  ItemUtils.ItemIDToTypeName(itemType),
                  displayName,
                  canHaveMaterial,
                  consumable
                )
        { }

        /// <summary>
        /// <inheritdoc cref="ItemAttributesDTO"/>
        /// </summary>
        /// <param name="typeName"><inheritdoc cref="typeName" path="//summary"/></param>
        /// <param name="displayName"><inheritdoc cref="displayName" path="//summary"/></param>
        /// <param name="canHaveMaterial"><inheritdoc cref="canHaveMaterial" path="//summary"/></param>
        /// <param name="consumable"><inheritdoc cref="consumable" path="//summary"/></param>
        public ItemAttributesDTO(string typeName, string displayName, bool canHaveMaterial = true, bool consumable = false)
        {
            this.typeName = typeName;
            this.displayName = displayName;
            this.canHaveMaterial = canHaveMaterial;
            this.consumable = consumable;
        }
        #endregion
    }
}

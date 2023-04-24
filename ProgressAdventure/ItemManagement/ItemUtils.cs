namespace ProgressAdventure.ItemManagement
{
    /// <summary>
    /// Utils for items.
    /// </summary>
    public static class ItemUtils
    {
        #region Public dicts
        /// <summary>
        /// The dictionary pairing up item types, to their attributes.
        /// </summary>
        public static readonly Dictionary<
            ItemTypeID,
            (
                string displayName,
                bool consumable
            )> itemAttributes = new()
            {
                // weapons
                [ItemType.Weapon.CLUB_WITH_TEETH] = ("Teeth club", false),
                // misc
                [ItemType.Misc.HEALTH_POTION] = ("Health potion", true)
        };
        #endregion

        #region Public fuctions
        /// <summary>
        /// Returs the item type, if the item type ID is an id for an item type.
        /// </summary>
        /// <param name="itemTypeID"></param>
        public static ItemTypeID? ToItemType(int itemTypeID)
        {
            var newItemType = (ItemTypeID)itemTypeID;
            var itemTypes = Tools.GetNestedStaticClassFields<ItemTypeID>(typeof(ItemType));
            foreach (var itemType in itemTypes)
            {
                if (newItemType == itemType)
                {
                    return itemType;
                }
            }
            return null;
        }
        #endregion
    }
}

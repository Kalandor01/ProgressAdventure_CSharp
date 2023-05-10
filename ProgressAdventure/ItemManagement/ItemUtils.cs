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
        /// <param name="itemTypeID">The int representation of the item's ID.</param>
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

        /// <summary>
        /// Tries to converts the int representation of the item ID to an item ID, and returns the success.
        /// </summary>
        /// <param name="itemTypeID">The int representation of the item's ID.</param>
        /// <param name="itemType">The resulting item, or a default item.</param>
        public static bool TryParseItemType(int itemTypeID, out ItemTypeID itemType)
        {
            var resultItem = ToItemType(itemTypeID);
            itemType = resultItem ?? ItemType.Misc.COPPER_COIN;
            return resultItem is not null;
        }
        #endregion
    }
}

namespace ProgressAdventure.ItemManagement
{
    /// <summary>
    /// Utils for items.
    /// </summary>
    public static class ItemUtils
    {
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
    }
}
